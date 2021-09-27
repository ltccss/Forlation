---@class DevCategoryItemWrapper : BaseViewWrapper
---@field _button CS.UnityEngine.UI.Button
---@field _highlightObj CS.UnityEngine.GameObject
---@field _text CS.UnityEngine.UI.Text
---@field _categoryName string
DevCategoryItemWrapper = DefineInheritedClass(DevCategoryItemWrapper, BaseViewWrapper)

function DevCategoryItemWrapper:_OnInit()
    self._button = self:GetUIButton('Button');
    self._text = self:GetUIText('Text');
    self._highlightObj = self:GetGameObject('Image_Highlight');

    self:RegisterButtonClick(self._button, self._ItemOnClick);
end

---@param categoryName string
function DevCategoryItemWrapper:ResetCategoryName(categoryName)
    self._categoryName = categoryName
    self._text.text = categoryName;
end

function DevCategoryItemWrapper:_OnDestroy()
end

function DevCategoryItemWrapper:_ItemOnClick()
    local dlg = DlgMgr.GetDlg(DevDlg);
    if (dlg) then
        dlg:SwitchToCategory(self._categoryName);
    end
end

function DevCategoryItemWrapper:GetCategoryName()
    return self._categoryName;
end

---@param on bool
function DevCategoryItemWrapper:SetHighlight(on)
    self._highlightObj:SetActive(on)
end