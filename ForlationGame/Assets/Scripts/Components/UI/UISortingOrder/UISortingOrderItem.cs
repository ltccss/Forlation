using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UISortingOrderItem : MonoBehaviour
{
    public int subOrder = 0;
    private UISortingOrderController _orderController;

    public void SetOrderController(UISortingOrderController ctrl)
    {
        _orderController = ctrl;
        this.Refresh();
    }

    private void Start()
    {
        var canvas = this.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
        }
        if (_orderController == null)
        {
            UISortingOrderController ctrl = this.GetComponentInParent<UISortingOrderController>();
            if (ctrl != null)
            {
                ctrl.RegisterItem(this);
            }
        }
    }

    private void OnDestroy()
    {
        if (_orderController != null)
        {
            var ctrl = _orderController;
            ctrl.Unregister(this);
        }
    }

    public void Refresh()
    {
        var canvas = this.GetComponent<Canvas>();
        if (canvas != null && _orderController != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = _orderController.StartOrder + subOrder;
        }
    }
    
}
