using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;

[RequireComponent(typeof(Outline))]
public class OutlineColor : BaseThemeColor //Component class that mast be attached to every game element that should be localized
{
    public override Color Color
    {
        get { return base.Color; }
        set { base.Color = value; if (Outline != null) { Outline.effectColor = value; } }
    }
    public Outline Outline => GetComponent<Outline>();

}