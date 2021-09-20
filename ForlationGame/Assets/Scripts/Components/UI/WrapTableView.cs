using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// 循环列表
/// 目前支持自上到下和自左到右的滚动
/// </summary>
[RequireComponent(typeof(ScrollRect))]
[ExecuteInEditMode]
public class WrapTableView : MonoBehaviour
{
    [Header("*可以通过右键菜单-UpdateGridLayout来更新GridLayout设置")]
    public GameObject cellPrefab;
    [Header("是否回收再利用初始存在的cells?")]
    public bool recycleExist;


    /// <summary>
    /// UpdateCell(int dataIndex, GameObject cell)
    /// </summary>
    public Action<int, GameObject> OnUpdateCell;

    private bool _needRebuild = true;

    private GridLayoutGroup _gridLayoutGroup;

    private ScrollRect _scrollRect;

    private float[] _rowHeights;

    private int _cleanCumulativeIndex;

    private Dictionary<int, GameObject> _visibleCells;
    private ViewRange _visibleLineRange;

    private RectTransform _recycleRoot;
    private LinkedList<GameObject> _recycledCells;

    private float _scrollPos;

    private bool _needRefresh;

    private int _childCount = 0;

    private int _lineCount = 0;

    private int _topPadding = 0;
    private int _bottomPadding = 0;
    private int _leftPadding = 0;
    private int _rightPadding = 0;

    private float _contentWidth = 0;
    private float _contentHeight = 0;

    public int ChildCount
    {
        get
        {
            return this._childCount;
        }
        set
        {
            this._childCount = value;
            _needRebuild = true;
        }
    }

    /// <summary>
    /// 编辑器用
    /// </summary>
    [ContextMenu("Update Grid Layout")]
    public void UpdateGridLayout()
    {
        GridLayoutGroup gridLayoutGroup = this.GetComponentInChildren<GridLayoutGroup>();
        if (gridLayoutGroup == null)
        {
            var scrollRect = this.GetComponent<ScrollRect>();
            gridLayoutGroup = scrollRect.content.gameObject.AddComponent<GridLayoutGroup>();
        }

        this.MakeGridLayoutAdapted();
    }

    public void MakeGridLayoutAdapted()
    {
        GridLayoutGroup gridLayoutGroup = this.GetComponentInChildren<GridLayoutGroup>();
        ScrollRect scrollRect = this.GetComponent<ScrollRect>();

        if (gridLayoutGroup != null && scrollRect != null)
        {
            if (scrollRect.horizontal && !scrollRect.vertical)
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                scrollRect.content.anchorMin = new Vector2(0.5f, scrollRect.content.anchorMin.y);
                scrollRect.content.anchorMax = new Vector2(0.5f, scrollRect.content.anchorMax.y);
            }
            else if (!scrollRect.horizontal && scrollRect.vertical)
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                scrollRect.content.anchorMin = new Vector2(scrollRect.content.anchorMin.x, 0.5f);
                scrollRect.content.anchorMax = new Vector2(scrollRect.content.anchorMax.x, 0.5f);
            }
            else
            {
                return;
            }
        }
    }

    /// <summary>
    /// 如果是垂直滚动的列表，指每行有几个item
    /// 如果是水平滚动的列表，指每列有几个item
    /// </summary>
    /// <returns></returns>
    private int CellCounPerLine()
    {
        return Math.Max(1, this._gridLayoutGroup.constraintCount);
    }

    private int GetLineCount()
    {
        int lineCount = this._childCount / this.CellCounPerLine() + ((this._childCount % this.CellCounPerLine() == 0) ? 0 : 1);
        return lineCount;
    }

    private void Rebuild()
    {
        // Profiler.BeginSample("TableView.ReloadData for dataSource:" + m_dataSource.GetType().Name, this);

        if (this._gridLayoutGroup == null)
        {
            Debug.LogError("WrapTableView需要滚动区域设置有GridLayoutGroup才能工作");
            return;
        }

        this.MakeGridLayoutAdapted();

        RecycleAllVisibleCells();
        if (this._childCount == 0)
        {
            return;
        }

        this._lineCount = this.GetLineCount();

        if (this._scrollRect.horizontal)
        {
            this._scrollRect.content.sizeDelta = new Vector2(
                this._lineCount * (this._gridLayoutGroup.cellSize.x) + Math.Max(0, this._lineCount - 1) * this._gridLayoutGroup.spacing.x + this._leftPadding + this._rightPadding,
                this._contentHeight
            );
        }
        else if (this._scrollRect.vertical)
        {
            this._scrollRect.content.sizeDelta = new Vector2(
                this._contentWidth,
                this._lineCount * (this._gridLayoutGroup.cellSize.y) + Math.Max(0, this._lineCount - 1) * this._gridLayoutGroup.spacing.y + this._topPadding + this._bottomPadding
            );
        }
        

        RecalculateVisibleRowsFromScratch();

        _needRebuild = false;
        _needRefresh = false;

        // Profiler.EndSample();

    }

    /// <summary>
    /// 强制清空列表项并全部刷新一遍
    /// </summary>
    public void ForeceRefresh()
    {
        this._needRebuild = true;
    }

    /// <summary>
    /// 刷新一下当前显示的Cell
    /// </summary>
    public void RefreshVisibleCells()
    {
        foreach (var kvp in _visibleCells)
        {
            if (this.OnUpdateCell != null)
            {
                try
                {
                    //Debug.Log(">>> " + kvp.Value.ToString());
                    this.OnUpdateCell(kvp.Key, kvp.Value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }

    /// <summary>
    /// 当前可见的行范围
    /// </summary>
    public ViewRange visibleRowRange
    {
        get { return _visibleLineRange; }
    }

    /// <summary>
    /// Get the maximum scrollable distance of the table. scrollPos property will never be more than this.
    /// </summary>
    public float scrollableDistance
    {
        get
        {
            if (this._scrollRect.horizontal)
            {
                return _scrollRect.content.rect.width - this._scrollRect.viewport.rect.width;
            }
            else
            {
                return _scrollRect.content.rect.height - this._scrollRect.viewport.rect.height;
            }
            
        }
    }

    /// <summary>
    /// Get or set the current scrolling position of the table
    /// </summary>
    public float scrollPos
    {
        get
        {
            return _scrollPos;
        }
        set
        {
            if (this._childCount == 0)
            {
                return;
            }
            value = Mathf.Clamp(value, 0, GetScrollPosForLine(this._lineCount - 1, true));

            _scrollPos = value;
            _needRefresh = true;
            float relativeScroll = value / this.scrollableDistance;
            _scrollRect.verticalNormalizedPosition = 1 - relativeScroll;

        }
    }

    /// <summary>
    /// Get the pos that the table would need to scroll to to have a certain line at the top
    /// </summary>
    /// <param name="row">The desired row</param>
    /// <param name="above">Should the top of the table be above the row or below the row?</param>
    /// <returns>The y position to scroll to, can be used with scrollY property</returns>
    public float GetScrollPosForLine(int row, bool above)
    {
        float retVal = GetCumulativeLineHeight(row);
        retVal += this._topPadding;
        if (above)
        {
            retVal -= this._gridLayoutGroup.spacing.y;
        }
        return retVal;
    }




    private void ScrollViewValueChanged(Vector2 newScrollValue)
    {
        float relativeScroll = this._scrollRect.horizontal? newScrollValue.x : (1 - newScrollValue.y);
        _scrollPos = relativeScroll * scrollableDistance;
        _needRefresh = true;
        //Debug.Log(m_scrollY.ToString(("0.00")));
    }

    private void RecalculateVisibleRowsFromScratch()
    {
        RecycleAllVisibleCells();
        SetInitialVisibleRows();
    }

    private void RecycleAllVisibleCells()
    {

        foreach (var kvp in _visibleCells)
        {
            RecycleCell(kvp.Value);
        }

        _visibleCells.Clear();

        _visibleLineRange = new ViewRange(0, 0);
    }

    void Awake()
    {

        if (!Application.isPlaying)
        {
            this.UpdateGridLayout();
            return;
        }

        this._visibleLineRange = new ViewRange(0, 0);

        this._scrollRect = GetComponent<ScrollRect>();

        this._gridLayoutGroup = this.GetComponentInChildren<GridLayoutGroup>();

        this._topPadding = this._gridLayoutGroup.padding.top;
        this._bottomPadding = this._gridLayoutGroup.padding.bottom;
        this._leftPadding = this._gridLayoutGroup.padding.left;
        this._rightPadding = this._gridLayoutGroup.padding.right;

        this._contentWidth = this._scrollRect.content.sizeDelta[0];
        this._contentHeight = this._scrollRect.content.sizeDelta[1];

        //Debug.Log(">>>> top padding : " + this._topPadding);

        _visibleCells = new Dictionary<int, GameObject>();

        _recycleRoot = new GameObject("_recycle", typeof(RectTransform)).GetComponent<RectTransform>();
        _recycleRoot.SetParent(this.transform, false);
        _recycleRoot.gameObject.SetActive(false);
        _recycledCells = new LinkedList<GameObject>();

        if (this.recycleExist && this._gridLayoutGroup != null && this._gridLayoutGroup.transform.childCount > 0)
        {
            var objs = new GameObject[this._gridLayoutGroup.transform.childCount];
            for (int i = 0; i < this._gridLayoutGroup.transform.childCount; i++)
            {
                objs[i] = this._gridLayoutGroup.transform.GetChild(i).gameObject;
            }
            for (int i = 0; i < objs.Length; i++)
            {
                RecycleCell(objs[i]);
            }

        }
    }

    void Update()
    {
        if (_needRebuild && Application.isPlaying)
        {
            Rebuild();
        }
    }

    void LateUpdate()
    {
        if (_needRefresh && Application.isPlaying)
        {
            RefreshVisibleRows();
        }
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            this._scrollRect.onValueChanged.AddListener(ScrollViewValueChanged);
        }
        
    }

    void OnDisable()
    {
        if (Application.isPlaying)
        {
            this._scrollRect.onValueChanged.RemoveListener(ScrollViewValueChanged);
        }
        
    }

    private ViewRange CalculateCurrentVisibleLineRange()
    {
        //Debug.Log(">>> m_scrollY : " + m_scrollY);
        float startPos = _scrollPos;

        float endPos = startPos + (this._scrollRect.horizontal? this._scrollRect.viewport.rect.width : this._scrollRect.viewport.rect.height);

        // 开始的索引要floor
        int startIndex = FindIndexOfLineAtPos(startPos, false, true);

        // 结束的索引要ceil
        int endIndex = FindIndexOfLineAtPos(endPos, true, false);

        //Debug.Log(">>> cal range : " + startIndex + "   " + endIndex);

        return new ViewRange(startIndex, endIndex - startIndex + 1);
    }

    private void SetInitialVisibleRows()
    {
        ViewRange visibleLines = CalculateCurrentVisibleLineRange();
        for (int i = 0; i < visibleLines.count; i++)
        {
            AddLine(visibleLines.from + i, true);
        }
        _visibleLineRange = visibleLines;
        UpdatePaddingElements();
    }

    private void AddLine(int line, bool atEnd)
    {
        int cellCountPerLine = this.CellCounPerLine();
        for (int i = 0; i < cellCountPerLine; i++)
        {
            int dataIndex = line * cellCountPerLine + i;
            if (dataIndex < this._childCount && dataIndex >= 0)
            {
                GameObject newCell = this.GetReusableCell();
                newCell.transform.SetParent(_scrollRect.content, false);

                if (this.OnUpdateCell != null)
                {
                    try
                    {
                        this.OnUpdateCell(dataIndex, newCell);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                
                _visibleCells[dataIndex] = newCell;

                if (atEnd)
                {
                    // 尾部追加的时候排到最后面去
                    newCell.transform.SetAsLastSibling();
                }
                else
                {
                    // 头部插入的时候，根据第几列插
                    newCell.transform.SetSiblingIndex(i);
                }
            }

        }
    }

    private void RefreshVisibleRows()
    {
        _needRefresh = false;

        if (this._childCount == 0)
        {
            return;
        }

        ViewRange newVisibleLines = CalculateCurrentVisibleLineRange();
        int oldTo;
        if (_visibleLineRange.count > 0)
        {
            oldTo = _visibleLineRange.Last();
        }
        else
        {
            oldTo = _visibleLineRange.from;
        }

        int newTo = newVisibleLines.Last();

        if (newVisibleLines.from > oldTo || newTo < _visibleLineRange.from || _visibleLineRange.count == 0)
        {
            //We jumped to a completely different segment this frame, destroy all and recreate
            RecalculateVisibleRowsFromScratch();
            return;
        }

        //Remove rows that disappeared to the top
        for (int i = _visibleLineRange.from; i < newVisibleLines.from; i++)
        {
            HideRow(false);
        }
        //Remove rows that disappeared to the bottom
        for (int i = newTo; i < oldTo; i++)
        {
            HideRow(true);
        }
        //Add rows that appeared on top
        for (int i = _visibleLineRange.from - 1; i >= newVisibleLines.from; i--)
        {
            AddLine(i, false);
        }
        //Add rows that appeared on bottom
        for (int i = oldTo + 1; i <= newTo; i++)
        {
            AddLine(i, true);
        }
        _visibleLineRange = newVisibleLines;
        UpdatePaddingElements();
    }

    private void UpdatePaddingElements()
    {
        if (this._scrollRect.horizontal)
        {
            var leftContentPlaceHolderWidth = (this._gridLayoutGroup.cellSize.x + this._gridLayoutGroup.spacing.x) * _visibleLineRange.from;

            this._gridLayoutGroup.padding.left = Mathf.Max(this._leftPadding + Convert.ToInt32(leftContentPlaceHolderWidth), this._leftPadding);
        }
        else
        {
            var topContentPlaceHolderHeight = (this._gridLayoutGroup.cellSize.y + this._gridLayoutGroup.spacing.y) * _visibleLineRange.from;

            this._gridLayoutGroup.padding.top = Mathf.Max(this._topPadding + Convert.ToInt32(topContentPlaceHolderHeight), this._topPadding);
        }
    }

    private void HideRow(bool last)
    {
        //Debug.Log("Hiding row at scroll y " + m_scrollY.ToString("0.00"));

        int line = last ? _visibleLineRange.Last() : _visibleLineRange.from;
        int columnCount = this.CellCounPerLine();

        for (int i = 0; i < columnCount; i++)
        {
            int dataIndex = line * columnCount + i;

            if (dataIndex < this._childCount && dataIndex >= 0)
            {
                GameObject removedCell = _visibleCells[dataIndex];
                RecycleCell(removedCell);
                _visibleCells.Remove(dataIndex);
            }
        }

        _visibleLineRange.count -= 1;
        if (!last)
        {
            _visibleLineRange.from += 1;
        }
    }

    private int FindIndexOfLineAtPos(float pos, bool needCeil, bool needFloor)
    {
        float indexF;
        if (this._scrollRect.horizontal)
        {
            indexF = (pos - this._leftPadding) / (this._gridLayoutGroup.cellSize.x + this._gridLayoutGroup.spacing.x);
        }
        else
        {
            indexF = (pos - this._topPadding) / (this._gridLayoutGroup.cellSize.y + this._gridLayoutGroup.spacing.y);
        }
        
        if (needCeil)
        {
            return Mathf.CeilToInt(indexF);
        }
        if (needFloor)
        {
            return Mathf.FloorToInt(indexF);
        }
        return Convert.ToInt32(indexF);
    }

    private float GetCumulativeLineHeight(int line)
    {
        if (this._scrollRect.horizontal)
        {
            return line * this._gridLayoutGroup.cellSize.x + Math.Max(0, line - 1) * this._gridLayoutGroup.spacing.x;
        }
        else
        {
            return line * this._gridLayoutGroup.cellSize.y + Math.Max(0, line - 1) * this._gridLayoutGroup.spacing.y;
        }
        
    }

    private void RecycleCell(GameObject cell)
    {
        if (cell != null)
        {
            cell.transform.SetParent(_recycleRoot, false);
            _recycledCells.AddLast(cell);
        }

    }

    /// <summary>
    /// Get a cell that is no longer in use for reusing
    /// </summary>
    /// <param name="reuseIdentifier">The identifier for the cell type</param>
    /// <returns>A prepared cell if available, null if none</returns>
    private GameObject GetReusableCell()
    {
        GameObject cell;
        if (_recycledCells.Count > 0)
        {
            cell = _recycledCells.First.Value;
            _recycledCells.RemoveFirst();
        }
        else
        {
            cell = GameObject.Instantiate(this.cellPrefab);
        }

        cell.SetActive(true);

        return cell;
    }


}

public class ViewRange
{
    public int from;
    public int count;

    public ViewRange(int fromValue, int valueCount)
    {
        this.from = fromValue;
        this.count = valueCount;
    }

    public int Last()
    {
        if (this.count == 0)
        {
            throw new System.InvalidOperationException("Empty range has no to()");
        }
        return (this.from + this.count - 1);
    }

    public int End()
    {
        return this.from + this.count;
    }

    public bool Contains(int num)
    {
        return num >= this.from && num < (this.from + this.count);
    }
}
