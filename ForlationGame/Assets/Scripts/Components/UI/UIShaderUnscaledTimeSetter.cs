using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShaderUnscaledTimeSetter : MonoBehaviour, IMaterialModifier
{
    private Material _mat;
    private bool _needSet = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (this._needSet)
        {
            // Time since level load (t/20, t, t*2, t*3), use to animate things inside the shaders.
            float t = Time.unscaledTime;
            Vector4 t4 = new Vector4(t / 20, t, t * 2, t * 3);
            this._mat.SetVector("_UnscaledTime", t4);
        }
    }

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        this._mat = baseMaterial;
        this.CheckIfNeedSetTime();
        return baseMaterial;
    }

    void OnDisable()
    {
        this._mat = null;
    }

    void OnEnable()
    {
        this.CheckIfNeedSetTime();
    }

    void CheckIfNeedSetTime()
    {
        if (this._mat != null && this._mat.HasProperty("_UnscaledTime"))
        {
            this._needSet = true;
        }
        else
        {
            this._needSet = false;
        }
    }
}
