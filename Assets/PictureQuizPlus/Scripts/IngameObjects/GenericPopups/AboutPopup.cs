using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.about_button;

    public void OnClose()
    {

    }

    public void OnOpen()
    {
    }
}
