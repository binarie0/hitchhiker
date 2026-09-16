using System;
using UnityEngine;

public static class RectExtensions
{
    /// <summary>
    /// Converts a rectangle to its four points.
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Span<Vector3> ToPoints(this Rect rect)
    {

        Span<Vector3> ogVectors = new Span<Vector3>(new Vector3[]{
            new(rect.x, rect.y),
            new(rect.x + rect.width, rect.y),
            new(rect.x + rect.width, rect.y + rect.height),
            new(rect.x, rect.y + rect.height)
        });
        return ogVectors;

    }

    public static Rect ToRect(this Resolution res)
    {
        return new Rect(0, 0, res.width, res.height);
    }

}
