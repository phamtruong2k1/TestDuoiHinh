using UnityEngine;

public enum GameMode
{
    Default,
    Pixel,
    Erasure,
    Planks,
    FourImages
}

public enum AnswerType
{
    Letters,
    Variants
}

public enum Hint
{
    one_letter,
    excess,
    complete,
    pixelate,
    plank,
    erasure,
    one_option,
    two_options,
    chance,
    bet,
}

public enum ThemeColorEnum
{
    Primary,
    Secondary,
    DarkText,
    Neutral,
    LightText,
    Normal,
    Positive,
    Light,
    Negative,
    Decorative,
}

[System.Serializable]
public struct ThemeColor
{
    public ThemeColor(ThemeColorEnum key, Color value)
    {
        this.key = key;
        this.color = value;
    }
    public ThemeColorEnum key;
    public Color color;
}

