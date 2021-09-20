---@class MessageTipItemWrapper : BaseViewWrapper
---@field _text CS.UnityEngine.UI.Text
---@field _canvasGroup CS.UnityEngine.CanvasGroup
---@field _dlg MessageTip
MessageTipItemWrapper = DefineInheritedClass(MessageTipItemWrapper, BaseViewWrapper)

function MessageTipItemWrapper:_OnInit()
    self._text = self:GetUIText("Text")
    self._canvasGroup = self:GetCanvasGroup(self._xRoot.gameObject)
end

function MessageTipItemWrapper:_OnDestroy()
end

---@param dlg MessageTip
function MessageTipItemWrapper:SetDlg(dlg)
    self._dlg = dlg
end

---@param tip string
function MessageTipItemWrapper:SetTip(tip)
    self._text.text = tip
    self._canvasGroup.alpha = 0
    self:Delay(0.15, self._Show)
    self:Delay(2, self._Hide);
    
end

function MessageTipItemWrapper:_Show()
    self._canvasGroup:DOFade(1, 0.25)
end

function MessageTipItemWrapper:_Hide()
    self._canvasGroup:DOFade(0, 1)
    self:Delay(1, self._Recycle);
end

function MessageTipItemWrapper:_Recycle()
    LogUtil.Warn("1111")
    self._dlg:RecycleTipWrapper(self)
end

function MessageTipItemWrapper:MoveTo(y)
    self._xRoot.transform:DOLocalMoveY(y, 0.36)
end