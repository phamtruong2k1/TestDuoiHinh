using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FourImagesManager : MonoBehaviour
{
    public Image[] images;

    internal void OnStart(Sprite[] sprites)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if(sprites.Length - 1 >= i) {
                images[i].sprite = sprites[i];
                images[i].gameObject.AddComponent<ImageToZoom>();
                images[i].gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
