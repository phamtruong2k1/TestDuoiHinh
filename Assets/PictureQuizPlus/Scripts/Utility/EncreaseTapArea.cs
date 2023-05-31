using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//The utility component class that encreases a tap area of a button when its sprite is small (Lamp button in our case)
public class EncreaseTapArea : MonoBehaviour
{
    public float width;
    public float height;
    public class EmptyGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
    void OnValidate()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            width = Mathf.Max(width, rectTransform.sizeDelta.x);
            height = Mathf.Max(height, rectTransform.sizeDelta.y);
        }
    }
    void Awake()
    {
        CreateHitZone();
    }

    void CreateHitZone()
    {
        GameObject gobj = new GameObject("Button Hit Zone");
        RectTransform hitzoneRectTransform = gobj.AddComponent<RectTransform>();
        hitzoneRectTransform.SetParent(transform);
        hitzoneRectTransform.localPosition = Vector3.zero;
        hitzoneRectTransform.localScale = Vector3.one;
        hitzoneRectTransform.sizeDelta = new Vector2(width, height);
        
        gobj.AddComponent<EmptyGraphic>();
        if(gobj.GetComponent<CanvasRenderer>() == null) {
            gobj.AddComponent<CanvasRenderer>();
        }
        EventTrigger eventTrigger = gobj.AddComponent<EventTrigger>();
        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerDown,
            (BaseEventData data) =>
            {
                ExecuteEvents.Execute(gameObject, data,
                   ExecuteEvents.pointerDownHandler);
            });
        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerUp,
            (BaseEventData data) =>
            {
                ExecuteEvents.Execute(gameObject, data,
                   ExecuteEvents.pointerUpHandler);
            });
        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerClick,
            (BaseEventData data) =>
            {
                ExecuteEvents.Execute(gameObject, data,
                   ExecuteEvents.pointerClickHandler);
            });
    }
    static void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType,
                                         System.Action<BaseEventData> method)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType,
            callback = new EventTrigger.TriggerEvent()
        };
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(method));
        trigger.triggers.Add(entry);
    }
}
