---@class DevMenuDlg : BaseDlg
---@field _devButton CS.UnityEngine.UI.Button
---@field _dragEventSystem CS.UnityEngine.EventSystems.EventTrigger
---@field _isMoved bool
DevMenuDlg = DefineInheritedClass(DevMenuDlg, BaseDlg)

---@return string
function DevMenuDlg:GetPrefabPath()
    -- todo (prefab路径可以通过右键菜单 - Copy Path获取)
    return "Assets/Assets_UI/Dev/Prefabs/DevMenuDlg.prefab"
end

---@return string
function DevMenuDlg:GetLayerName()
    return DlgLayer.Top
end

function DevMenuDlg:_OnInit()
    self._isMoved = false

    self._devButton = self:GetUIButton("Image_DevButtonDrag");
    self:RegisterButtonClick(self._devButton, self._OnClick);

    self._dragEventSystem = self:GetComponentWithClass("Image_DevButtonDrag", CS.UnityEngine.EventSystems.EventTrigger);

    ---@type CS.UnityEngine.UI.CanvasScaler
    local canvarScaler = self._xRoot:GetComponentInParent(typeof(CS.UnityEngine.UI.CanvasScaler))
    local rate = canvarScaler.referenceResolution.x / CS.UnityEngine.Screen.width;

    local dragStartEntry = CS.UnityEngine.EventSystems.EventTrigger.Entry();
    dragStartEntry.eventID = CS.UnityEngine.EventSystems.EventTriggerType.BeginDrag;
    dragStartEntry.callback:AddListener(function(eventData)
        self._isMoved = true
    end);

    self._dragEventSystem.triggers:Add(dragStartEntry)

    local dragEndEntry = CS.UnityEngine.EventSystems.EventTrigger.Entry();
    dragEndEntry.eventID = CS.UnityEngine.EventSystems.EventTriggerType.EndDrag;
    dragEndEntry.callback:AddListener(function(eventData)
        self._isMoved = false
    end);

    self._dragEventSystem.triggers:Add(dragEndEntry)

    local dragProcessEntry = CS.UnityEngine.EventSystems.EventTrigger.Entry();
    dragProcessEntry.eventID = CS.UnityEngine.EventSystems.EventTriggerType.Drag;
    dragProcessEntry.callback:AddListener(function(eventData)
        local oldPos = self._xRoot.transform.localPosition;
        oldPos.x = oldPos.x + eventData.delta.x * rate;
        oldPos.y = oldPos.y + eventData.delta.y * rate;
        self._xRoot.transform.localPosition = oldPos;
        -- if (eventData.delta.x ~= 0 or eventData.delta.y ~= 0) then
        --     self._isMoved = true
        -- end
    end);

    self._dragEventSystem.triggers:Add(dragProcessEntry)

    self._xRoot.gameObject:SetActive(CS.AppSetting.runningMode == "debug")

    ---@type CS.UnityEngine.Canvas
    local canvas = self._xRoot:GetComponentInParent(typeof(CS.UnityEngine.Canvas))
    canvas = canvas.rootCanvas
    ---@type CS.UnityEngine.RectTransform
    local canvasRectTrans = canvas.transform;
    self._xRoot.transform.localPosition = CS.UnityEngine.Vector3(canvasRectTrans.rect.width * 0.5 - 150, canvasRectTrans.rect.height * 0.5 - 120, 0);
    
end

function DevMenuDlg:_OnVisible(visible)
    -- todo
end

function DevMenuDlg:_OnDestroy()
    local triggerCount = self._dragEventSystem.triggers.Count;
    for i = 0, triggerCount - 1 do
        self._dragEventSystem.triggers[i].callback:RemoveAllListeners();
        self._dragEventSystem.triggers[i].callback:ReleaseUnusedListeners();
    end
    self._dragEventSystem.triggers:Clear();
end

function DevMenuDlg:_OnClick()
    if (self._isMoved) then
        return
    end
    LogUtil.Log("Dev menu button click")
    local devDlg = DlgMgr.GetDlg(DevDlg)
    if (IsNull(devDlg)) then
        DlgMgr.FetchDlg(DevDlg)
    else
        DlgMgr.CloseDlg(devDlg)
    end
    
end