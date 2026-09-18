using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Extensions
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
     

    /// <summary>
    /// Searches for a visual element inside the hierarchy with a set ID.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="id">The ID to look for. This should be entered without a hash.</param>
    /// <returns></returns>
    public static VisualElement SearchByID(this VisualElement root, string id)
    {
        if (root.name.Contains(id))
        {
            return root;
        }
        //Debug.Log(root.name);
        List<VisualElement> children = new List<VisualElement>(root.Children());
        for (int i = 0; i < root.childCount; i++)
        {
            VisualElement s = SearchByID(children[i], id);
            if (s != null)
            {
                return s;   
            }
        }
        return null;
    }

    /// <summary>
    /// Gets an element's absolute position
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static Vector2 GetAbsoluteLocation(this VisualElement element)
    {
        //Debug.Log(element.localBound.position)
        return element.localBound.position;
        //return new Vector2(element.style.left.value.value, element.style.top.value.value);
    }
    public static Vector2 Size(this VisualElement element)
    {
        return new Vector2(element.style.width.value.value, element.style.height.value.value);
    }
}