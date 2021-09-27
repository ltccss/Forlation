---@class DevButtonExItemWrapper : BaseViewWrapper
---@field _button CS.UnityEngine.UI.Button
---@field _buttonText CS.UnityEngine.UI.Text
---@field _indexText CS.UnityEngine.UI.Text
---@field _inputField CS.UnityEngine.UI.InputField
---@field _callback fun(param : string)
---@field _name string
DevButtonExItemWrapper = DefineInheritedClass(DevButtonExItemWrapper, BaseViewWrapper)

function DevButtonExItemWrapper:_OnInit()
    self._button = self:GetUIButton("Button");
    self._buttonText = self:GetUIText("Text");
    self._inputField = self:GetUIInputField("InputField");
    self._indexText = self:GetUIText("Text_Index");

    self:RegisterButtonClick(self._button, self._OnClick);
end

function DevButtonExItemWrapper:_OnDestroy()
    self._callback = nil;
end

---@param name string
---@param callback fun(param : string)
---@param index number
function DevButtonExItemWrapper:Reset(name, callback, index)
    self._name = name;
    self._buttonText.text = name;
    self._callback = callback;

    self._indexText.text = tostring(index);
end

function DevButtonExItemWrapper:_OnClick()
    if (NotNull(self._callback)) then
        FuncCall(nil, self._callback, self._inputField.text);
    end
end