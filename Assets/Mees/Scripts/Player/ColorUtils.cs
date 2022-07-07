using UnityEngine;

public class ColorUtils
{
    public static Color combineColors(Color first, Color second, float ratio) {
        float remainder = 1.0F - ratio;

        float r = (first.r * remainder + second.r * ratio);
        float g = (first.g * remainder + second.g * ratio);
        float b = (first.b * remainder + second.b * ratio);
        float a = (first.a * remainder + second.a * ratio);
        return new Color(r, g, b, a);
    }

}
