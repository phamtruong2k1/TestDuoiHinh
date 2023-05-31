using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageToZoom : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!LevelStateController.isPaused)
        {
            var zoomPrefab = Utils.CreateFromPrefab("ZoomImage");
            zoomPrefab.GetComponent<ZoomImage>().Initialize(gameObject.GetComponent<Image>().sprite);
            Instantiate(zoomPrefab, Utils.getRootTransform());
        }
    }
}
