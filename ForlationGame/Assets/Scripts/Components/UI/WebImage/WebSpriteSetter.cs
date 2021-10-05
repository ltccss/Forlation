using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class WebSpriteSetter : MonoBehaviour
{
    [Header("右键组件-Show 在Editor中显示图片")]
    [SerializeField]
    private string _url;
    [Header("是否需要调整尺寸")]
    public bool needResize = true;
    [Header("等比例缩放至指定宽")]
    public int fitToWidth = 0;
    [Header("等比例缩放至指定高")]
    public int fitToHeight = 0;
    [Header("当同时指定fitWidth和fitHeight时，是否是缩放到宽/高有一位至目标值，而另一位溢出；还是缩放至宽/高有一位至目标值，另一位不满目标值")]
    public bool overflow = true;
    public bool autoLoad = true;

    [Header("加载中隐藏，加载后显示的物件")]
    public GameObject[] showAfterLoadObjects;
    [Header("加载中显示，加载后隐藏的物件")]
    public GameObject[] hideAfterLoadObjects;

    public Action<bool> Event_ImageLoaded;

    private WebImageDownloader _downloader;

    private void OnEnable()
    {
        if ((this._downloader == null || !this._downloader.Done || this._downloader.Url != this._url) && this.autoLoad)
        {
            this.RefreshWebImage();
        }
    }

    public string Url
    {
        get
        {
            return this._url;
        }
    }

    public void ShowWebImage(string url, bool resize)
    {
        this._url = url;
        this.needResize = resize;
        if (this.gameObject.activeInHierarchy)
        {
            this.RefreshWebImage();
        }
    }

    private void SetSprite(Sprite sprite)
    {
        var image = this.GetComponent<Image>();
        image.sprite = sprite;
        if (needResize && sprite != null)
        {
            WebImageResizer.Resize(this.transform as RectTransform, this.fitToWidth, this.fitToHeight, this.overflow, (int)sprite.rect.width, (int)sprite.rect.height);
        }
    }

    [ContextMenu("Show")]
    public void RefreshWebImage()
    {
        if (string.IsNullOrEmpty(this._url))
        {
            return;
        }
        if (this._downloader != null)
        {
            this._downloader.Stop();
        }
        if (!this.gameObject.activeInHierarchy)
        {
            return;
        }
        this.ToggleObjects(false);
        this._downloader = WebImageDownloader.Create(this._url, this, () =>
        {
            if (string.IsNullOrEmpty(this._downloader.Error))
            {
                Sprite sprite = this._downloader.ToSprite();
                this.SetSprite(sprite);
                this.ToggleObjects(true);
                if (this.Event_ImageLoaded != null)
                {
                    this.Event_ImageLoaded(true);
                }
            }
            else
            {
                if (this.Event_ImageLoaded != null)
                {
                    this.Event_ImageLoaded(false);
                }
            }
        });
        this._downloader.StartDownloadImage();
    }

    private void ToggleObjects(bool isLoaded)
    {
        if (this.hideAfterLoadObjects != null)
        {
            for (int i = 0; i < this.hideAfterLoadObjects.Length; i++)
            {
                this.hideAfterLoadObjects[i].SetActive(!isLoaded);
            }
        }

        if (this.showAfterLoadObjects != null)
        {
            for (int i = 0; i < this.showAfterLoadObjects.Length; i++)
            {
                this.showAfterLoadObjects[i].SetActive(isLoaded);
            }
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        this._url = null;
        this.SetSprite(null);
    }
}
