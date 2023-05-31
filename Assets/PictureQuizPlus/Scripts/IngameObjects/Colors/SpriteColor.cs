using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;

[RequireComponent(typeof(Image))]
public class SpriteColor : BaseThemeColor //Component class that mast be attached to every game element that should be localized
{
    public override Color Color
    {
        get { return base.Color;    }
        set { base.Color = value; if (Image != null) { Image.color = value; } }
    }
    public Image Image => GetComponent<Image>();

}