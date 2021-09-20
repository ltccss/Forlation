using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingView : MonoBehaviour
{
    public Text loadingText;
    public Slider progressSlider;
    public Text versionText;

    void Start()
    {
        this.loadingText.text = string.Empty;
        this.progressSlider.value = 0f;

        this.versionText.text = "";
    }

    public void RefreshVersion(string version)
    {
        this.versionText.text = version;
    }

    public void RefreshLoadingText(string text)
    {
        this.loadingText.text = text;
    }

    public void RefreshProgress(float progress)
    {
        this.progressSlider.value = progress;
    }

    public void ShowProgress(bool on)
    {
        this.loadingText.gameObject.SetActive(on);
        this.progressSlider.gameObject.SetActive(on);
    }
}
