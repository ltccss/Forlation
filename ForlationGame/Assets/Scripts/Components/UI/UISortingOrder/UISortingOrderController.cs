using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISortingOrderController : MonoBehaviour
{
    private int _startOrder = 0;
    private HashSet<UISortingOrderItem> _itemSet = new HashSet<UISortingOrderItem>();

    public int StartOrder
    {
        get
        {
            return this._startOrder;
        }
        set
        {
            this._startOrder = value;
            _UpdateItems();
        }
    }

    private void Start()
    {
        this.CollectItems();
    }

    public void CollectItems()
    {
        foreach (var item in _itemSet)
        {
            item.SetOrderController(null);
        }
        _itemSet.Clear();
        var comps = this.GetComponentsInChildren<UISortingOrderItem>(true);
        for (int i = 0; i < comps.Length; i++)
        {
            this.RegisterItem(comps[i]);
        }
    }

    public void RegisterItem(UISortingOrderItem item)
    {
        if (!_itemSet.Contains(item))
        {
            _itemSet.Add(item);
        }
        item.SetOrderController(this);
    }

    public void Unregister(UISortingOrderItem item)
    {
        item.SetOrderController(null);
        _itemSet.Remove(item);
    }

    private void _UpdateItems()
    {
        foreach (var item in _itemSet)
        {
            item.Refresh();
        }
    }
}
