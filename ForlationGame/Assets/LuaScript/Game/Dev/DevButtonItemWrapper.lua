---@class DevButtonItemWrapper : BaseViewWrapper
---@field _button CS.UnityEngine.UI.Button
---@field _buttonText CS.UnityEngine.UI.Text
---@field _indexText CS.UnityEngine.UI.Text
---@field _callback function
---@field _name string
DevButtonItemWrapper = DefineInheritedClass(DevButtonItemWrapper, BaseViewWrapper)

function DevButtonItemWrapper:_OnInit()
    self._button = self:GetUIButton("Button");
    self._buttonText = self:GetUIText("Text");
    self._indexText = self:GetUIText("Text_Index");

    self:RegisterButtonClick(self._button, self._OnClick);
end

function DevButtonItemWrapper:_OnDestroy()
    self._callback = nil;
end

---@param name string
---@param callback function
---@param index number
function DevButtonItemWrapper:Reset(name, callback, index)
    self._name = name;
    self._buttonText.text = name;
    self._callback = callback;

    self._indexText.text = tostring(index);
end

function DevButtonItemWrapper:_OnClick()
    if (NotNull(self._callback)) then
        FuncCall(nil, self._callback);
    end
end