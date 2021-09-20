---@class TestWrapper : BaseViewWrapper
---@field _testButton CS.UnityEngine.UI.Button
---@field _testText CS.UnityEngine.UI.Text
---@field _testImage CS.UnityEngine.UI.Image
---@field _destroyButton CS.UnityEngine.UI.Button
TestWrapper = DefineInheritedClass(TestWrapper, BaseViewWrapper)

function TestWrapper:_OnInit()
    self._testButton = self:GetUIButton("Button_Test")
    self._testText = self:GetUIText("Text_Test")
    self._testImage = self:GetUIImage("Image_Test")

    self._destroyButton = self:GetUIButton("Button_Destroy")

    self:RegisterButtonClick(self._testButton, self._TestOnClick)

    self:RegisterButtonClick(self._destroyButton, self._DstroyOnClick)

    self:Refresh()
end

function TestWrapper:_TestOnClick()
    LogUtil.Log("click1")
end

function TestWrapper:_DstroyOnClick()
    self:Destroy()
end

function TestWrapper:Refresh()

    self._testText.text = "dasdada"
    self._testImage.color = CS.UnityEngine.Color(1,0,1,1)
end

-- test
-- require("TestWrapper")
-- local go = CS.UnityEngine.GameObject("test")
-- CS.UnityEngine.GameObject.DontDestroyOnLoad(go)

-- local testPrefab = CS.AssetsManager.LoadAsset("Prefab/Canvas.prefab", typeof(CS.UnityEngine.GameObject))

-- ---@type CS.UnityEngine.GameObject
-- local testGo = CS.UnityEngine.GameObject.Instantiate(testPrefab)

-- testGo.transform:SetParent(go.transform)

-- local testWrapper = CreateClassInstance(TestWrapper)
-- testWrapper:Init(testGo)