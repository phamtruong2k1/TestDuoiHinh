using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGenericPopup
{
    LocalizationItemType title { get; }
    void OnClose();
    void OnOpen();

}
