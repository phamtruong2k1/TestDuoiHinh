using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MoreGamesButton : MonoBehaviour
{
    public GameObject popupPrefab;
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("blup");
            Instantiate(popupPrefab, Utils.getRootTransform());
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
