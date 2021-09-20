---@class MessageTip : BaseDlg
---@field _startTrans CS.UnityEngine.Transform
---@field _recycle CS.UnityEngine.Transform
---@field _usedTipWrapperArray MessageTipItemWrapper[]
---@field _recycledTipWrapperArray MessageTipItemWrapper[]
MessageTip = DefineInheritedClass(MessageTip, BaseDlg)

---@type MessageTip[]
local _dlgArray = {};

---@param tip string
function MessageTip.Pop(tip)
    local dlg = DlgMgr.FetchDlg(MessageTip);
    
    dlg:_AddTip(tip)
end

---@return string
function MessageTip:GetPrefabPath()
    return "Prefabs/UISystem/MessageTip"
end

function MessageTip:IsInPackage()
    return true;
end

---@return string
function MessageTip:GetLayerName()
    -- todo
    return DlgLayer.Toppest
end

function MessageTip:_OnInit()
    self._startTrans = self:GetTransform("Start")
    self._recycle = self:GetTransform("_Recycle")

    self._usedTipWrapperArray = {}
    self._recycledTipWrapperArray = {}
end

function MessageTip:_OnVisible(visible)
    -- todo
end

function MessageTip:_OnWillClose()
    -- todo
end

function MessageTip:_OnDestroy()

end

---@param tip string
function MessageTip:_AddTip(tip)
    ---@type MessageTipItemWrapper
    local wrapper
    if (#self._recycledTipWrapperArray > 0) then
        wrapper = self._recycledTipWrapperArray[#self._recycledTipWrapperArray]
        table.remove(self._recycledTipWrapperArray, #self._recycledTipWrapperArray)
        wrapper:GetXRoot().transform:SetParent(self._startTrans)
    else
        local prefab = CS.UnityEngine.Resources.Load("Prefabs/UISystem/MesssageTipItem")
        local obj = CS.UnityEngine.GameObject.Instantiate(prefab, self._startTrans)
        wrapper = self:_CreateChildWrapper(obj, MessageTipItemWrapper)
        wrapper:SetDlg(self)
    end

    wrapper:GetXRoot().transform.localPosition = CS.UnityEngine.Vector3.zero

    wrapper:SetTip(tip)
    
    table.insert(self._usedTipWrapperArray, wrapper)

    for i = 1, #self._usedTipWrapperArray do
        self._usedTipWrapperArray[i]:MoveTo((#self._usedTipWrapperArray - i) * 90)
    end
end

---@param tipWrapper MessageTipItemWrapper
function MessageTip:RecycleTipWrapper(tipWrapper)
    for i = #self._usedTipWrapperArray, 1, -1 do
        if (tipWrapper == self._usedTipWrapperArray[i]) then
            table.remove(self._usedTipWrapperArray, i)
            break
        end
    end

    table.insert(self._recycledTipWrapperArray, tipWrapper)
    tipWrapper:GetXRoot().transform:SetParent(self._recycle)
    LogUtil.WarnObject("22222", tipWrapper:GetXRoot())
end