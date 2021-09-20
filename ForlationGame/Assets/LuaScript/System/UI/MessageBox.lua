---@class MessageBox : BaseDlg
---@field _yesButton CS.UnityEngine.UI.Button
---@field _yesButtonText CS.UnityEngine.UI.Text
---@field _noButton CS.UnityEngine.UI.Button
---@field _noButtonText CS.UnityEngine.UI.Text
---@field _titleText CS.UnityEngine.UI.Text
---@field _contentText CS.UnityEngine.UI.Text
---@field _closeButton CS.UnityEngine.UI.Button
---@field _closeCallback function
---@field _yesCallback function
---@field _noCallback function
MessageBox = DefineInheritedClass(MessageBox, BaseDlg)

function MessageBox:GetPrefabPath()
    return "Prefabs/UISystem/MessageBox"
end

function MessageBox:GetLayerName()
    return DlgLayer.Top
end

function MessageBox:IsInPackage()
    return true
end

---@param title string
---@param content string
---@param okText string @不填默认"Ok"
---@param okCallback function
---@param closeCallback function
function MessageBox.PopOk(title, content, okText, okCallback, closeCallback)
    local dlg = DlgMgr.CreateDlg(MessageBox)
    dlg:_SetCommonContent(title, content, closeCallback)
    dlg:_SetOkContent(okText, okCallback)
end

function MessageBox.PopYesNo(title, content, yesText, yesCallback, noText, noCallback, closeCallback)
    local dlg = DlgMgr.CreateDlg(MessageBox)
    dlg:_SetCommonContent(title, content, closeCallback)
    dlg:_SetYesNoContent(yesText, yesCallback, noText, noCallback);
end

function MessageBox:_OnInit()
    self._yesButton = self:GetUIButton("Button_Yes")
    self._yesButtonText = self:GetUIText("Text_YesButton")

    self._noButton = self:GetUIButton("Button_No")
    self._noButtonText = self:GetUIText("Text_NoButton")

    self._titleText = self:GetUIText("Text_Title")
    self._contentText = self:GetUIText("Text_Content")

    self._closeButton = self:GetUIButton("Button_Close")

    self:RegisterButtonClick(self._yesButton, self._YesOnClick)
    self:RegisterButtonClick(self._noButton, self._NoOnClick);
    self:RegisterButtonClick(self._closeButton, self._CloseOnClick)
end

--- 设置公用部分内容
function MessageBox:_SetCommonContent(title, content, closeCallback)
    self._titleText.text = title
    
    self._contentText.text = content
    self._closeCallback = closeCallback
end

--- 设置单按钮布局特定内容
function MessageBox:_SetOkContent(oktext, okCallback)
    self._noButton.gameObject:SetActive(false);
    local okStr = oktext or "OK";
    okStr = string.upper(okStr)
    self._yesButtonText.text = okStr
    self._yesCallback = okCallback
end

function MessageBox:_SetYesNoContent(yesText, yesCallback, noText, noCallback)
    self._noButton.gameObject:SetActive(true);

    local yesStr = yesText or "YES";
    local noStr = noText or "NO";

    yesStr = string.upper(yesStr);
    noStr = string.upper(noStr);
    
    self._yesButtonText.text = yesStr;
    self._yesCallback = yesCallback;
    self._noButtonText.text = noStr;
    self._noCallback = noCallback;
end

function MessageBox:_YesOnClick()
    if (self._yesCallback) then
        self._yesCallback()
        self._yesCallback = nil
    end
    self:Close()
end

function MessageBox:_NoOnClick()
    if (self._noCallback) then
        self._noCallback();
        self._noCallback = nil;
    end
    self:Close();
end

function MessageBox:_CloseOnClick()
    if (self._closeCallback) then
        self._closeCallback()
        self._closeCallback = nil
    end
    self:Close()
end