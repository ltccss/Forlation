using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 页面出入场动画便捷控制(搭配BaseDlg使用)
/// 使用方法：
/// 1、在页面根节点或任意子节点加上该组件和Animator组件，
/// 2、使用Animator制作入场动画并命名为UIAnim_Open
/// 3、使用Animator制作出场动画并命名为UIAnim_Close，并在动画结束的时间点添加事件帧，调用OnClose方法
/// </summary>
[RequireComponent(typeof(Animator))]
public class DialogAnim : MonoBehaviour
{
    private Animator _animator;

    private bool _inited = false;

    private Action _closeCallback;

    private Vector3 _rootScale;

    private Vector3 _specZero = new Vector3(0f, 0f, 11f);

    private void _Init()
    {
        if (this._inited)
        {
            return;
        }
        this._animator = this.GetComponent<Animator>();

        // 先把root的scale调成0，不然第一帧有点问题
        this._rootScale = this.transform.localScale;
        this.transform.localScale = _specZero;

        StartCoroutine(_NextFrame());

        this._inited = true;
    }

    IEnumerator _NextFrame()
    {
        yield return 0;
        // 如果下一帧scale变化了，说明有动画在控制，就不管了
        // 如果下一帧scale还是0，那就把scale还原一下
        if (this.transform.localScale == _specZero)
        {
            this.transform.localScale = this._rootScale;
        }
    }

    public void PlayOpenAnim()
    {
        this._Init();
        this._animator.Play("UIAnim_Open");
    }

    public void PlayCloseAnim(Action finishCallback)
    {
        this._Init();
        this._closeCallback = finishCallback;
        int stateId = Animator.StringToHash("UIAnim_Close");
        if (this._animator.HasState(0, stateId))
        {
            this._animator.Play("UIAnim_Close");
        }
        else
        {
            this.OnClose();
        }
    }

    public void OnClose()
    {
        if (this._closeCallback != null)
        {
            this._closeCallback();
            this._closeCallback = null;
        }
    }

    void OnDestroy()
    {
        this._closeCallback = null;
    }
}
