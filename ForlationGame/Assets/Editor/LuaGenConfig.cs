using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using XLua;
using System.Reflection;
using UnityEngine.Events;

public static class ExampleGenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
                // Unity API
                typeof(System.Object),
                typeof(System.IO.Path),
                typeof(UnityEngine.Object),
                typeof(String),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Color),
                typeof(Color32),
                typeof(Rect),
                typeof(Ray),
                typeof(Bounds),
                typeof(Ray2D),
                typeof(Time),
                typeof(GameObject),
                typeof(Component),
                typeof(Behaviour),
                typeof(Transform),
                typeof(Resources),
                typeof(TextAsset),
                typeof(Keyframe),
                typeof(AnimationCurve),
                typeof(AnimationClip),
                typeof(Animation),
                typeof(MonoBehaviour),
                typeof(ParticleSystem),
                typeof(MeshRenderer),
                typeof(SkinnedMeshRenderer),
                typeof(Renderer),
                typeof(UnityWebRequest),
                typeof(DownloadHandler),
                typeof(UploadHandler),
                typeof(YieldInstruction),
                typeof(AsyncOperation),
                typeof(UnityWebRequestAsyncOperation),
                typeof(WaitForSeconds),
                typeof(AssetBundleRequest),
                typeof(SystemInfo),
                typeof(UnityEngine.Rendering.CopyTextureSupport),
                typeof(NPOTSupport),
                typeof(BatteryStatus),
                typeof(OperatingSystemFamily),
                typeof(UnityEngine.Rendering.GraphicsDeviceType),
                typeof(DeviceType),
                typeof(Input),
                typeof(KeyCode),
                typeof(Event),

                typeof(Action<string>),

                typeof(UnityEngine.Debug),
                typeof(Mathf),
                typeof(Collider2D),
                typeof(CapsuleCollider2D),
                typeof(BoxCollider2D),
                typeof(Animator),
                typeof(AnimatorStateInfo),
                typeof(Application),
                typeof(Coroutine),
                typeof(Sprite),

                typeof(Camera),
                typeof(CameraClearFlags),
                typeof(CameraType),

                // Generic Collection
                // 如何在xlua里直接创建泛型类
                // https://github.com/Tencent/xLua/blob/master/Assets/XLua/Doc/faq.md#%E6%B3%9B%E5%9E%8B%E5%AE%9E%E4%BE%8B%E6%80%8E%E4%B9%88%E6%9E%84%E9%80%A0
                typeof(List<int>),
                typeof(List<string>),
                typeof(List<float>),
                typeof(List<double>),
                typeof(List<long>),
                // dictionary和hashset有坑，要手动排除，目测了下步骤挺繁琐的，先不搞了，参见FAQ
                //typeof(Dictionary<string, string>),
                //typeof(Dictionary<int, string>),
                //typeof(Dictionary<string, int>),
                //typeof(Dictionary<int, int>),
                //typeof(Dictionary<System.Object, int>),
                //typeof(Dictionary<System.Object, string>),
                //typeof(Dictionary<int, System.Object>),
                //typeof(Dictionary<string, System.Object>),
                //typeof(HashSet<int>),
                //typeof(HashSet<string>),
                //typeof(HashSet<long>),
                //typeof(HashSet<System.Object>),

                // Scene
                typeof(UnityEngine.SceneManagement.SceneManager),
                typeof(UnityEngine.SceneManagement.CreateSceneParameters),
                typeof(UnityEngine.SceneManagement.LoadSceneMode),
                typeof(UnityEngine.SceneManagement.LoadSceneParameters),
                typeof(UnityEngine.SceneManagement.LocalPhysicsMode),
                typeof(UnityEngine.SceneManagement.Scene),
                typeof(UnityEngine.SceneManagement.SceneUtility),
                typeof(UnityEngine.SceneManagement.UnloadSceneOptions),
                
                // UGUI
                typeof(Canvas),
                typeof(CanvasGroup),
                typeof(CanvasScaler),
                typeof(CanvasRenderer),
                typeof(GraphicRaycaster),
                typeof(Image),
                typeof(RawImage),
                typeof(Text),
                typeof(InputField),
                typeof(Outline),
                typeof(Shadow),
                typeof(Button),
                typeof(Button.ButtonClickedEvent),
                typeof(Selectable),
                typeof(RectTransform),
                typeof(RectTransform.Axis),
                typeof(RectTransformUtility),
                typeof(ScrollRect),
                typeof(MaskableGraphic),
                typeof(Mask),
                typeof(Graphic),
                typeof(Slider),
                typeof(Toggle),
                typeof(Toggle.ToggleEvent),
                typeof(ToggleGroup),
                typeof(UnityEngine.EventSystems.UIBehaviour),
                typeof(UnityEngine.EventSystems.EventTrigger),
                typeof(UnityEngine.EventSystems.EventTriggerType),
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.BaseEventData),
                typeof(UnityEngine.EventSystems.AbstractEventData),
                typeof(UnityEngine.EventSystems.AxisEventData),
                typeof(UnityEngine.EventSystems.PointerEventData),
                typeof(UnityEngine.EventSystems.PointerEventData.InputButton),
                typeof(UnityEngine.EventSystems.EventTrigger.Entry),
                typeof(UnityEngine.EventSystems.EventTrigger.TriggerEvent),
                typeof(UnityEngine.EventSystems.RaycastResult),
                typeof(UnityEngine.UI.LayoutElement),
                typeof(UnityEngine.UI.LayoutGroup),
                typeof(UnityEngine.UI.VerticalLayoutGroup),
                typeof(UnityEngine.UI.HorizontalLayoutGroup),
                typeof(UnityEngine.UI.HorizontalOrVerticalLayoutGroup),
                typeof(ColorSpace),
                typeof(LayoutRebuilder),
                typeof(ContentSizeFitter),


                // xLua view
                typeof(XLuaView),
                typeof(XLVInt),
                typeof(XLVLong),
                typeof(XLVBool),
                typeof(XLVDouble),
                typeof(XLVString),
                typeof(XLVColor),
                typeof(XLVSprite),

                typeof(XLVApplicationPauseLifeCycle),
                typeof(XLVUpdateLifeCycle),
                typeof(XLVLateUpdateLifeCycle),
                typeof(XLVFixedUpdateLifeCycle),

                // app setting
                typeof(AppSetting),

                // package info
                typeof(PackageInfo),

                // Util
                typeof(CoroutineUtil),
                typeof(DeviceUtil),
                typeof(LogUtil),
                typeof(MathUtil),
                typeof(RegexUtil),
                typeof(LzmaUtil),
                typeof(SecurityUtil),
                typeof(FileUtil),
                typeof(CommonUtil),

                // DOTween
                typeof(DG.Tweening.DOTween),
                typeof(DG.Tweening.ShortcutExtensions),
                typeof(DG.Tweening.DOTweenModuleUI),
                typeof(DG.Tweening.TweenSettingsExtensions),

                // Custom Components
                typeof(DialogAnim),
                typeof(WrapTableView),
                typeof(WebImageSetter),
                typeof(WebSpriteSetter),
                typeof(WebImageDownloader),
                typeof(UISortingOrderController),
                typeof(UISortingOrderItem),

                // Game Logic
                typeof(FileDownloadHandler),
                typeof(ForlationGameManager),
                typeof(AssetsManager),

                typeof(Screen),

            };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
                typeof(Action),
                typeof(Func<double, double, double>),
                typeof(Action<string>),
                typeof(Action<double>),
                typeof(Action<UnityEngine.Object>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(UnityEngine.Events.UnityAction<int>),
                typeof(UnityEngine.Events.UnityAction<bool>),
                typeof(UnityEngine.Events.UnityAction<string>),
                typeof(UnityEngine.Events.UnityAction<GameObject>),
                typeof(UnityEngine.Events.UnityAction<Vector2>),
                typeof(UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.Scene>),
                typeof(UnityEngine.Events.UnityAction<int, int>),
                typeof(System.Collections.IEnumerator),
                typeof(System.Collections.IDictionary),
                typeof(Button.ButtonClickedEvent),
                typeof(Action<int, GameObject>),

                typeof( UnityEvent<Vector2>),
                typeof(ScrollRect.ScrollRectEvent),

                typeof(InputField.OnChangeEvent),
                typeof(InputField.OnValidateInput),
                typeof(InputField.SubmitEvent),
            };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},

                new List<string>(){"UnityEngine.UI.Graphic", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},

            };

}