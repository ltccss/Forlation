using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Graphic))]
public class UIShaderReplacer : UIBehaviour, IMaterialModifier
{
    [SerializeField]
    private Shader m_newShader;

    public Shader NewShader
    {
        get
        {
            return this.m_newShader;
        }
        set
        {
            this.m_newShader = value;
            this.SetDirty();
        }
    }

    private Material _mat;

    protected override void Start()
    {
        base.Start();
        this.SetDirty();
    }

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (this.m_newShader == null)
        {
            return baseMaterial;
        }

        this._mat = new Material(baseMaterial);
        this._mat.shader = this.m_newShader;

        return this._mat;
    }

    public void SetDirty()
    {
        Graphic g = this.GetComponent<Graphic>();
        if (g != null)
        {
            g.SetMaterialDirty();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        this.SetDirty();
    }
#endif
}
