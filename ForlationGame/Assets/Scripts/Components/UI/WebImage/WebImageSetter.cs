using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WebImageSetter : MonoBehaviour
{
    [Header("右键组件-Show 在Editor中显示图片")]
    public string url;
    [Header("是否需要调整尺寸")]
    public bool needResize = true;
    [Header("等比例缩放至指定宽")]
    public int fitToWidth = 0;
    [Header("等比例缩放至指定高")]
    public int fitToHeight = 0;
    [Header("当同时指定fitWidth和fitHeight时，是否是缩放到宽/高有一位至目标值，而另一位溢出；还是缩放至宽/高有一位至目标值，另一位不满目标值")]
    public bool overflow = true;
    public bool autoLoad = true;

    [Header("在第一次图片下载并加载后显示")]
    public GameObject[] showAfterLoadObjects;
    [Header("在第一次图片下载并加载后隐藏")]
    public GameObject[] hideAfterLoadObjects;

    private WebImageDownloader _downloader;

    private void OnEnable()
    {
        if ((this._downloader == null || !this._downloader.Done || this._downloader.Url != this.url) && this.autoLoad)
        {
            this.RefreshWebImage();
        }
    }

    public void ShowWebImage(string url, bool resize)
    {
        this.url = url;
        this.needResize = resize;
        if (this.gameObject.activeInHierarchy)
        {
            this.RefreshWebImage();
        }
    }

    public void SetTexture(Texture texture)
    {
        var rawImage = this.GetComponent<RawImage>();
        rawImage.texture = this._downloader.Texture;
        if (needResize)
        {
            WebImageResizer.Resize(this.transform as RectTransform, this.fitToWidth, this.fitToHeight, this.overflow, texture.width, texture.height);
        }
        this.ToggleObjects(true);
    }

    [ContextMenu("Show")]
    public void RefreshWebImage()
    {
        if (string.IsNullOrEmpty(this.url))
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
        this._downloader = WebImageDownloader.Create(this.url, this, () =>
        {
            if (string.IsNullOrEmpty(this._downloader.Error))
            {
                this.SetTexture(this._downloader.Texture);
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
}
