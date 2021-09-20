---@class TestCardItemWrapper : BaseViewWrapper
---@field _titleText CS.UnityEngine.UI.Text
---@field _starText CS.UnityEngine.UI.Text
---@field _scaleRoot CS.UnityEngine.RectTransform
---@field _canvasGroup CS.UnityEngine.CanvasGroup
---@field _scrollRect CS.UnityEngine.UI.ScrollRect
---@field _button CS.UnityEngine.UI.Button
---@field _func fun(vec2 : CS.UnityEngine.Vector2)
---@field _index number
---@field _cardData TestCardData
TestCardItemWrapper = DefineInheritedClass(TestCardItemWrapper, BaseViewWrapper)

function TestCardItemWrapper:_OnInit()
    self._titleText = self:GetUIText("Text_Title");
    self._starText = self:GetUIText("Text_Star");

    self._scaleRoot = self:GetRectTransform("Root_Scale");
    self._canvasGroup = self:GetCanvasGroup("Root_Scale");

    self._scrollRect = self._xRoot.gameObject:GetComponentInParent(typeof(CS.UnityEngine.UI.ScrollRect));

    self._button = self:GetUIButton("Image");
    self:RegisterButtonClick(self._button, self._OnClick);

    self._func = function(vec2)
        if (self._xRoot.gameObject.activeInHierarchy) then
            self:_OnScroll(vec2);
        end
    end

    self._scrollRect.onValueChanged:AddListener(self._func);

    self:_AddTestFunc("测试方法1", function() 
        MessageTip.Pop("名字: " .. self._cardData.name)
    end)
end

function TestCardItemWrapper:_OnDestroy()
    self._scrollRect.onValueChanged:RemoveListener(self._func);
end

---@param vec2 CS.UnityEngine.Vector2
function TestCardItemWrapper:_OnScroll(vec2)
    if (not self._cardData) then
        return
    end

    local pos = self._scrollRect.transform:InverseTransformPoint(self._xRoot.transform.position)

    -- if (self._index == 0) then
    --     LogUtil.Log("pos " .. pos.x);
    -- end
    
    if (pos.x >= 0) then
        local scale = EaseUtil.QuadIn(pos.x, 1, -0.4, 950)
        scale = math.min(scale, 1);
        local vec3 = CS.UnityEngine.Vector3(scale, scale, scale);
        self._scaleRoot.localScale = vec3;
    else
        local scale = EaseUtil.QuadIn(-pos.x, 1, -0.4, 950)
        scale = math.min(scale, 1);
        local vec3 = CS.UnityEngine.Vector3(scale, scale, scale);
        self._scaleRoot.localScale = vec3;
    end
    
    if (pos.x > 300 ) then
        local alpha = EaseUtil.SineIn(pos.x - 300, 1, -1, 550)
        self._canvasGroup.alpha = alpha;
    elseif (pos.x < -300) then
        local alpha = EaseUtil.SineIn(pos.x + 300, 1, -1, -550)
        self._canvasGroup.alpha = alpha;
    else
        self._canvasGroup.alpha = 1;
    end

    if (pos.x >= 0) then
        local x = EaseUtil.QuadIn(pos.x, 0, -230, 950)
        local y = EaseUtil.SineIn(pos.x, -40, 40, 950)
        local vec3 = CS.UnityEngine.Vector3(x, y, 0);
        self._scaleRoot.localPosition = vec3;
    else
        local x = EaseUtil.QuadIn(-pos.x, 0, 230, 950)
        local y = EaseUtil.SineIn(-pos.x, -40, 40, 950)
        local vec3 = CS.UnityEngine.Vector3(x, y, 0);
        self._scaleRoot.localPosition = vec3;
    end
end

---@param cardData TestCardData
---@param index number
function TestCardItemWrapper:ResetData(cardData, index)

    self._cardData = cardData;
    self._index = index;

    if (not cardData) then
        self._canvasGroup.alpha = 0;
        return
    end

    self._titleText.text = tostring(index + 1) .. ". " .. cardData.name;
    
    local starText = "";
    for i = 1, cardData.star do
        starText = starText .. "★"
    end

    self._starText.text = starText;

    -- self:_OnScroll()

    -- 防止特定情况下UI刷新延迟导致的某些帧的错误显示
    self._canvasGroup.alpha = 0;
end

function TestCardItemWrapper:_OnClick()
    MessageBox.PopOk("阿巴阿巴阿巴", "你点击了 - " .. self._cardData.name);
end