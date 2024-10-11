using UnityEngine;

public static class UnityUtil
{
    public static string ConsoleColorText(this string message, Color color) => $"<color={color.ToHex()}>{message}</color>";

    public static string ToHex(this Color c) => $"#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}";

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);

        return (byte)(f * 255);
    }
}

