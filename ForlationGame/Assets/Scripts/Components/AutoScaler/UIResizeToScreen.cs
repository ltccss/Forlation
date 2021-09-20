using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIResizeToScreen : MonoBehaviour
{
    [Header("填0则使用Rect Transform的长宽")]
    public Vector2 preferSize;
    public bool executeOnStart = true;
    public bool executeOnUpdate = false;

    public bool overflow = true;

    [ContextMenu("Resize")]
    public void Resize()
    {
        var canvas = this.GetComponentInParent<Canvas>();
        canvas = canvas.rootCanvas;

        var parentWidth = (canvas.transform as RectTransform).rect.width;
        var parentHeight = (canvas.transform as RectTransform).rect.height;

        if (preferSize == Vector2.zero)
        {
            var childRecttrans = this.transform as RectTransform;
            preferSize = new Vector2(childRecttrans.rect.width, childRecttrans.rect.height);
        }

        var childWidth = preferSize.x;
        var childHeight = preferSize.y;

        float parentRatio = (float)parentWidth / parentHeight;
        float childRatio = (float)childWidth / childHeight;

        bool scaleByWidth = true;

        if (overflow)
        {
            if (parentRatio > childRatio)
            {
                scaleByWidth = true;
            }
            else
            {
                scaleByWidth = false;
            }
        }
        else
        {
            if (parentRatio > childRatio)
            {
                scaleByWidth = false;
            }
            else
            {
                scaleByWidth = true;
            }
        }

        float scale = 1f;
        if (scaleByWidth)
        {
            scale = (float)parentWidth / childWidth;
        }
        else
        {
            scale = (float)parentHeight / childHeight;
        }
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    void Start()
    {
        if (executeOnStart)
        {
            this.Resize();
        }
    }

    void Update()
    {
        if (executeOnUpdate)
        {
            this.Resize();
        }
    }
}
