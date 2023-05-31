using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZoomImage : MonoBehaviour, IPointerDownHandler
{
  public Image image;
  void Start()
  {
    image = GetComponentInChildren<Image>();
  }
  public void OnPointerDown(PointerEventData eventData)
  {
    Destroy(gameObject);
  }

  void Update()
  {
    if (Input.GetKeyDown("escape"))
    {
      Destroy(gameObject);
    }
  }

  public void Initialize(Sprite sprite)
  {
    image.sprite = sprite;
  }

}
