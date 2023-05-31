using System;
using UnityEngine;
using UnityEngine.UI;

public class Letter : MonoBehaviour //Component class that attached to the each letter prefab
{
    public Text textField; //Reference to the text field of the object

    public event Action<Letter> Clicked; //Event is fired when the gameobject clicked

    public void OnClick()
    {
        Clicked(this);
    }
}
