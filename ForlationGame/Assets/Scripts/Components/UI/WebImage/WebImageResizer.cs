using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebImageResizer
{
    public static void Resize(RectTransform rectTrans, int fitWidth, int fitHeight, bool overflow, int originWidth, int originHeight)
    {
        if (fitWidth == 0 && fitHeight == 0)
        {
            rectTrans.sizeDelta = new Vector2(originWidth, originHeight);
        }
        else if (fitWidth != 0 && fitHeight != 0)
        {
            float fitRatio = (float)fitWidth / fitHeight;
            float originRatio = (float)originWidth / originHeight;
            if (fitRatio > originRatio)
            {
                if (overflow)
                {
                    _FitToWidth(rectTrans, fitWidth, originWidth, originHeight);
                }
                else
                {
                    _FitToHeight(rectTrans, fitHeight, originWidth, originHeight);
                }
            }
            else
            {
                if (overflow)
                {
                    _FitToHeight(rectTrans, fitHeight, originWidth, originHeight);
                }
                else
                {
                    _FitToWidth(rectTrans, fitWidth, originWidth, originHeight);
                }
            }
        }
        else if (fitWidth != 0)
        {
            _FitToWidth(rectTrans, fitWidth, originWidth, originHeight);
        }
        else if (fitHeight != 0)
        {
            _FitToHeight(rectTrans, fitHeight, originWidth, originHeight);
        }
    }

    private static void _FitToWidth(RectTransform rectTrans, int fitWidth, int originWidth, int originHeight)
    {
        rectTrans.sizeDelta = new Vector2(fitWidth, originHeight * fitWidth / originWidth );
    }

    private static void _FitToHeight(RectTransform rectTrans, int fitHeight, int originWidth, int originHeight)
    {
        rectTrans.sizeDelta = new Vector2(originWidth * fitHeight / originHeight, fitHeight);
    }
}
