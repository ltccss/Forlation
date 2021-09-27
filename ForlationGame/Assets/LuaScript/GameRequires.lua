
-- requires start
require("Utils/LogUtil")
require("Debug/Debug")

-- extension
require("Extensions/CommonExtension")
require("Extensions/ClassExtension")
require("Extensions/FunctionCall")
require("Extensions/StringExtension")
require("Extensions/TableExtension")

-- util data collection
require("Utils/DataCollection/List")
require("Utils/DataCollection/Queue")

-- util
require("Utils/TimeUtil")
require("Utils/LuaDelegate")
require("Utils/TimeService")
require("Utils/LuaCoroutineUtil")
require("Utils/LuaAssetsManager")
require("Utils/JsonUtil")
require("Utils/Color")
require("Utils/UI/UITool")
require("Utils/UI/UIPool")
require("Utils/EaseUtil")

-- FF Mgr System
-- require("FF/FF") -- FF文件在LGInitMgrsState里require
require("FF/FF_Init")
require("FF/BaseMgr")
require("FF/FF_Destroy")


-- UI System
require("System/UI/BaseViewWrapper")
require("System/UI/BaseDlg")
require("System/UI/DlgMgr")

require("System/UI/MessageBox")
require("System/UI/MessageTip")
require("System/UI/MessageTipItemWrapper")


-- Local Storage System
require("System/DataSave/LocalStorage")
require("System/DataSave/DataSaveMgr")
require("System/DataSave/DataSaveNode")


-- Game Manager
require("LuaGameManager")

-- Game Load
require("System/GameLoad/LuaGameLoadMgr")
require("System/GameLoad/GameLoadState/BaseLuaGameLoadState")
require("System/GameLoad/GameLoadState/LGInitMgrsState")
require("System/GameLoad/GameLoadState/LGInitUISystemState")
require("System/GameLoad/GameLoadState/LGLoadConfigState")
require("System/GameLoad/GameLoadState/LGSyncPlayerDataState")


-- Game Reminder
require("Game/Remind/RemindType")
require("Game/Remind/RemindNode")
require("Game/Remind/RemindMgr")

-- Dev
require("Game/Dev/DevButtonExItemWrapper")
require("Game/Dev/DevButtonItemWrapper")
require("Game/Dev/DevCategoryItemWrapper")
require("Game/Dev/DevDlg")
require("Game/Dev/DevMenuDlg")


-- Game Custom
require("Game/Test/TestCardItemWrapper")
require("Game/Test/TestDlg")
require("Game/Test/TestMgr")
require("Game/Test/TestWrapper")
require("Game/UIJumpStack/UIJumpStackMgr")
require("Game/UIPopQueue/UIPopQueueMgr")



-- requires end