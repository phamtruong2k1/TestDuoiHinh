using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericPopupController : MonoBehaviour
{

    private List<GameObject> pages;
    private GenericPopupContainer container;

    public bool isOpened;
    private int currentPage = 0;

    // Start is called before the first frame update
    private bool canBeClosed = true;
    public bool CanBeClosed
    {
        get { return canBeClosed; }
        set { canBeClosed = value; container.closeButton.gameObject.SetActive(value); }
    }
    private Queue<PopupQueueItem> queue = new Queue<PopupQueueItem>();


    public T Open<T>() where T : IGenericPopup
    {
        isOpened = true;
        GameObject go = Utils.CreateFromPrefab(new string[] { "GenericPopups", "GenericPopupContainer" });
        go = GameObject.Instantiate(go, Utils.getRootTransform());
        container = go.GetComponent<GenericPopupContainer>();
        GameObject content = Utils.CreateFromPrefab(new string[] { "GenericPopups", typeof(T).Name });
        GameObject instantiated = container.SetContent(content);
        IGenericPopup popupContent = instantiated.GetComponent<T>();
        container.SetTitle(Utils.Capitalize(GameController.Instance.GetLocalizedValue(popupContent.title)));
        pages = new List<GameObject>() { instantiated };
        currentPage = pages.Count - 1;
        popupContent.OnOpen();
        container.backButton.gameObject.SetActive(false);
        container.closeButton.onClick.AddListener(() =>
        {
            Close();
        });
        return (T)popupContent;
    }

    public T OpenNextPage<T>() where T : IGenericPopup
    {
        container.Hide();
        GameObject current = pages[currentPage];
        current.SetActive(false);
        GameObject content = Utils.CreateFromPrefab(new string[] { "GenericPopups", typeof(T).Name });
        // content.SetActive(false);
        GameObject instantiated = container.SetContent(content);
        IGenericPopup popupContent = instantiated.GetComponent<T>();
        container.SetTitle(Utils.Capitalize(GameController.Instance.GetLocalizedValue(popupContent.title)));
        pages.Add(instantiated);
        currentPage = pages.Count - 1;
        popupContent.OnOpen();
        container.backButton.gameObject.SetActive(pages.Count > 1);
        if (container.backButton.gameObject.activeSelf)
        {
            container.backButton.onClick.RemoveAllListeners();
            container.backButton.onClick.AddListener(() =>
            {
                OpenPreviousPage();
            });
        }
        container.Show();
        return (T)popupContent;
    }
    public void OpenPreviousPage()
    {
        container.Hide();
        GameObject current = pages[currentPage];
        pages.Remove(current);
        Destroy(current);

        currentPage = pages.Count - 1;
        if(currentPage < 0) {
            Close();
            return;
        }
        SoundsController.instance.PlaySound("menus");
        GameObject instantiated = pages[currentPage];
        instantiated.SetActive(true);
        IGenericPopup popupContent = instantiated.GetComponent<IGenericPopup>();
        container.SetTitle(Utils.Capitalize(GameController.Instance.GetLocalizedValue(popupContent.title)));
        container.backButton.gameObject.SetActive(pages.Count > 1);
        if (container.backButton.gameObject.activeSelf)
        {
            container.backButton.onClick.RemoveAllListeners();
            container.backButton.onClick.AddListener(() =>
            {
                OpenPreviousPage();
            });
        }
        container.Show();
    }

    public void EnqueuePopup<T>(ThemeColorEnum color) where T : IGenericPopup
    {
        GameObject popupPrefab = Utils.CreateFromPrefab(new string[] { "GenericPopups", typeof(T).Name });
        queue.Enqueue(new PopupQueueItem() { popupPrefab = popupPrefab, color = color });
    }

    public T Open<T>(ThemeColorEnum color) where T : IGenericPopup
    {
        T result = Open<T>();
        container.SetColor(color);
        return result;
    }
    public T Open<T>(PopupSettings settings) where T : IGenericPopup
    {
        T result = Open<T>();
        if (settings.color != null)
        {
            container.SetColor(settings.color.Value);
        }
        if (!string.IsNullOrEmpty(settings.title))
        {
            container.SetTitle(settings.title);
        }
        if(settings.unableToClose) {
            
        }
        return result;
    }

    public void Close()
    {
        foreach (var page in pages)
        {
            page.GetComponent<IGenericPopup>().OnClose();
            Destroy(page);
        }
        if (queue.Count > 0)
        {
            container.Hide();

            PopupQueueItem item = queue.Dequeue();
            GameObject instantiated = container.SetContent(item.popupPrefab);
            // content.SetActive(false);
            IGenericPopup popupContent = instantiated.GetComponent<IGenericPopup>();
            container.SetTitle(Utils.Capitalize(GameController.Instance.GetLocalizedValue(popupContent.title)));
            container.SetColor(item.color);
            pages = new List<GameObject>() { instantiated };
            currentPage = pages.Count - 1;
            popupContent.OnOpen();
            container.backButton.gameObject.SetActive(pages.Count > 1);
            if (container.backButton.gameObject.activeSelf)
            {
                container.backButton.onClick.RemoveAllListeners();
                container.backButton.onClick.AddListener(() =>
                {
                    OpenPreviousPage();
                });
            }
            container.Show();
        }
        else
        {
            isOpened = false;
            Destroy(container.gameObject);
        }
    }

    void Update()
    {
        if (isOpened && CanBeClosed && Input.GetKeyDown("escape"))
        {
            Close();
        }
    }
}

public struct PopupQueueItem
{
    public GameObject popupPrefab;
    public ThemeColorEnum color;
}

public struct PopupSettings
{
    public ThemeColorEnum? color;
    public string title;
    public bool unableToClose;
}