using UnityEngine;
using UnityEngine.EventSystems;

//Animations handler for almost all UI buttons

class ButtonAnim : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private void OnEnable()
    {
        GetComponent<Animator>().SetBool("isPressed", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GetComponent<Animator>().SetBool("isPressed", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GetComponent<Animator>().SetBool("isPressed", false);
    }
    
}
