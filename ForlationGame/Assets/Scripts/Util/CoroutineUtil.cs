using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineUtil
{
    class CoroutineComponent : MonoBehaviour
    {
    }

    private static GameObject _root;
    private static CoroutineComponent _comp;

    

    /// <summary>
    /// 传进来的root应该是个永久稳定的节点
    /// </summary>
    /// <param name="root"></param>
    public static void Init(GameObject root)
    {
        _root = root;
        _comp = _root.AddComponent<CoroutineComponent>();
    }

    public static Coroutine StartCoroutine(IEnumerator co)
    {
        return _comp.StartCoroutine(co);
    }

    public static void StopCoroutine(IEnumerator co)
    {
        _comp.StopCoroutine(co);
    }

    public static void StopCoroutine(Coroutine co)
    {
        _comp.StopCoroutine(co);
    }
}
