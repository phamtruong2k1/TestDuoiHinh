using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ErasureManager : MonoBehaviour //Class to manage task with Erasure Type
{
    //References from tne Inspector
    public Material maskMaterial;
    public Transform erasureCanvas;
    public Vector3 penOffest = Vector3.up;

    private GameObject huskPrefab;
    private Texture2D penTexture, maskTexture, eraser;
    private static Image taskImage;
    private BoxCollider2D colliderBox; //Images bounds
    private Collider2D colliderMatch;
    private Color[] pencil; //Array of pen pixels
    private Vector3 currentPosition, tempPoint, moveToPoint;
    public Vector2 origin;
    public int pixelPerWorldUnitX, pixelPerWorldUnitY;
    private int imageWidth, imageHeight;
    private int pixelsCounter, eraserWidth, eraserHeight;
    private string path;
    private Color[] tempArea; //Array of target area pixels
    private GameObject spatula;
    private static Queue<GameObject> huskPool; //Pooling system
    LevelFrontendController frontendController;
    int huskSteps, stepsPaid = 0;
    int pixelsToPay, pixelsForHusk;
    float huskFrequency;
    float brushFrequency;

    private bool erasureStarted = false;

    public void OnStart(LevelInfo data) //Initializing method called from the UIMnager at the level start
    {
        huskPrefab = Utils.CreateFromPrefab("Husk");
        frontendController = GameObject.FindObjectOfType<LevelFrontendController>();
        Initialize(data);
        InitializePenSize();
        CreateAlphaMask(data.maskPath);
    }

    private void Initialize(LevelInfo data) //Erasure type level initializer
    {
        penTexture = GameController.Instance.Pen;
        erasureCanvas.gameObject.SetActive(true);
        path = Path.Combine(Application.persistentDataPath, data.directoryName + "mask.png"); //Path for saving alphamask
        LevelStateController.OnDataSave += SavePNG; //Subscribe for datasaving event
        spatula = Instantiate(Utils.CreateFromPrefab("Spatula"), gameObject.transform.parent); //Instantiate a spatula prefab
        huskPool = new Queue<GameObject>(); //Object pooling system
        taskImage = GetComponent<Image>(); //Reference to the main image of the current task
        taskImage.material = maskMaterial;
        imageWidth = taskImage.mainTexture.width;
        imageHeight = taskImage.mainTexture.height;
        huskFrequency = GameController.Instance.HuskFrequnecy;
        brushFrequency = GameController.Instance.PenFrequnecy;
        StartCoroutine(Init());

    }

    private IEnumerator Init()
    {
        yield return new WaitUntil(() => frontendController.IsImageReady);
        colliderBox = GetComponent<BoxCollider2D>();
        origin = colliderBox.transform.TransformPoint(new Vector3(-colliderBox.size.x / 2, -colliderBox.size.y / 2)); //(0,0) coordinates in the world space of the images pixel grid

        //some magic required for no reason
        float rate = 5f;
        float screenRatio = Utils.GetScreenRatio();
        if (screenRatio > 1.7 && screenRatio < 1.9)
        {
            rate = 4.85f;
        }
        else if (screenRatio >= 1.9)
        {
            rate = 4.6f;
        }
        // end of magic
        pixelPerWorldUnitX = Mathf.RoundToInt(imageWidth / rate); //One world unit in images pixels
        pixelPerWorldUnitY = Mathf.RoundToInt(imageHeight / rate);

        LevelStateController.TryToEducate(LocalizationItemType.education_erasure, taskImage.transform);
    }

    private static void SavePNG(LevelInfo data) //Save current alphamask to the file
    {
        Texture2D tex = (Texture2D)taskImage.material.GetTexture("_Alpha");
        byte[] rawdata = tex.EncodeToPNG();
        if (data.maskPath != null && data.maskPath != "")
        {
            File.WriteAllBytes(data.maskPath, rawdata);
        }
    }

    public static void UnSub()
    {
        LevelStateController.OnDataSave -= SavePNG;
    }

    private void CreateAlphaMask(string path) //Create new alphamask or load it from the file
    {

        maskTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.ARGB32, false);

        if (path == null || path == "")
        {
            var pixels = maskTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            maskTexture.SetPixels(pixels);
        }
        else
        {
            maskTexture.LoadImage(File.ReadAllBytes(path));
        }
        maskTexture.Apply();
        taskImage.material.SetTexture("_Alpha", maskTexture);
        pixelsCounter = maskTexture.GetPixels().Count(c => c.a == 1) % pixelsToPay;

    }



    private void InitializePenSize() //Pen texture rescaling depending on the images resolution
    {
        pencil = penTexture.GetPixels();
        eraser = new Texture2D(penTexture.width, penTexture.height, TextureFormat.ARGB32, false);
        eraser.SetPixels(pencil);
        int newPenWidth = (int)(eraser.width / (960f / imageWidth));
        int newPenHeight = (int)(eraser.height / (960f / imageHeight));
        TextureScaler.Bilinear(eraser, newPenWidth, newPenHeight);
        pencil = null;
        pencil = eraser.GetPixels();
        eraserWidth = eraser.width;
        eraserHeight = eraser.height;
        pixelsToPay = pencil.Count(c => c.a == 1f) * 100;
        pixelsForHusk = Mathf.RoundToInt(pixelsToPay / huskFrequency);
        // print($"pixelsToPay {pixelsToPay} of {pencil.Count()}");
    }

    void Update()
    {

        if (Input.GetMouseButton(0) && !LevelStateController.isPaused)
        {
            currentPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (!erasureStarted)
            {
                if (colliderBox == Physics2D.OverlapPoint(currentPosition))
                {
                    erasureStarted = true;
                }
            }
            if (erasureStarted)
            {

                moveToPoint = currentPosition + penOffest;
                colliderMatch = Physics2D.OverlapPoint(moveToPoint);

                if (colliderBox == colliderMatch) //If the mouse or touch point hits our image collider
                {
                    TryToDraw();
                }
                else
                {
                    tempPoint = Vector3.zero;
                }
            }

        }

        if (Input.GetMouseButtonUp(0)) //Touch stopped
        {
            spatula.SetActive(false);
            tempPoint = Vector3.zero;
            erasureStarted = false;
        }

        if (spatula.activeSelf)
        {
            spatula.transform.position = currentPosition; //Make the spatula to follow cursor or touches
        }
    }

    private void TryToDraw()
    {
        if (tempPoint == Vector3.zero)
        {
            tempPoint = moveToPoint;
        }
        if (tempPoint != moveToPoint) //If there is new point to move
        {
            if (Utils.EnoughCoinsForHint(Hint.erasure, false))
            {
                DrawOnMask();

                if (!spatula.activeSelf)
                {
                    spatula.SetActive(true);
                }
            }
        }
    }

    private void DrawOnMask()
    {

        // float distance = Vector2.Distance(tempPoint, moveToPoint);

        // Calculate how many times we should interpolate based on the amount of time that has passed since the last update

        for (float lerp = 0; lerp <= 1; lerp += brushFrequency)
        {
            Vector2 newPosition = Vector2.Lerp(tempPoint, moveToPoint, lerp);
            int targetPixelX = Mathf.RoundToInt((newPosition.x + Mathf.Abs(origin.x)) * (pixelPerWorldUnitX));
            int targetPixelY = Mathf.RoundToInt((newPosition.y + Mathf.Abs(origin.y)) * (pixelPerWorldUnitY));
            if (targetPixelX + eraserWidth > imageWidth)
            {
                targetPixelX -= (targetPixelX + eraserWidth) - imageWidth;
            }
            if (targetPixelY + eraserHeight > imageHeight)
            {
                targetPixelY -= (targetPixelY + eraserHeight) - imageHeight;
            }

            tempArea = maskTexture.GetPixels(targetPixelX, targetPixelY, eraserWidth, eraserHeight); //Area on the mask to be painted out
            int drawnPixelCounter = 0;

            //Compare area pixels with pen pixels
            for (int i = 0; i < tempArea.Length; i++)
            {
                if (tempArea[i].a != 1)
                {
                    if (pencil[i].a == 1)
                    {
                        drawnPixelCounter++;
                    }
                    tempArea[i] = tempArea[i].a < pencil[i].a ? pencil[i] : tempArea[i];
                }

            }

            if (drawnPixelCounter > 0)
            {
                maskTexture.SetPixels(targetPixelX, targetPixelY, eraserWidth, eraserHeight, tempArea);
            }


            pixelsCounter += drawnPixelCounter;

            int stepsToPay = pixelsCounter / pixelsToPay;
            int newHuskSteps = pixelsCounter / pixelsForHusk;
            int stepsUnpaid = stepsToPay - stepsPaid;
            int newHuskCount = newHuskSteps - huskSteps;
            bool shouldBreak = false;
            if (stepsUnpaid > 0)
            {
                for (int i = 0; i < stepsUnpaid; i++)
                {
                    if (Utils.EnoughCoinsForHint(Hint.erasure))
                    {
                        LevelFrontendController.HintEvent(Hint.erasure, path);
                        stepsPaid++;
                    }
                    else
                    {
                        shouldBreak = true;
                        break;
                    }
                }
            }

            if (newHuskCount > 0)
            {
                for (int i = 0; i < newHuskCount; i++)
                {
                    huskSteps++;
                    GetFromPool().transform.position = tempPoint; //Spawn husks

                }
            }
            if (shouldBreak) break;

        }
        tempPoint = moveToPoint;

        maskTexture.Apply();
        taskImage.material.SetTexture("_Alpha", maskTexture);
    }

    private GameObject GetFromPool() //Take a husk from the pool or instantiate new one if needed
    {
        if (huskPool.Count == 0 || huskPool == null)
        {
            GameObject newGo = Instantiate(huskPrefab, gameObject.transform);
            huskPool.Enqueue(newGo);
        }
        GameObject temp = huskPool.Dequeue();
        temp.SetActive(true);
        return temp;
    }

    public static void ReturnToPool(GameObject husk)
    {
        husk.SetActive(false);
        huskPool.Enqueue(husk);
    }

}
