using System;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.mediation.api;
using strange.framework.api;
using strange.extensions.injector.api;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;
using strange.framework.impl;
using Sirenix.OdinInspector;
using Faymar.Components;
using Faymar.Models;
using Faymar.Utils;
using Faymar.Views;
using Faymar.Views.Roles;
using Faymar.Views.Teleporters;
//============================================================
//@author	JiphuTzu
//@create	07/14/2017
//@company	FaymAR
//
//@description:
//============================================================
namespace Faymar.Contexts
{
    ///=======================================================
    ///场景入口基类。进行场景配置，并整体启动场景。
    ///=======================================================
    public class BasicBootstrap<T> : BasicBootstrap where T : CrossContext
    {
        protected virtual void Awake()
        {
            //base.Awake();
            context = (T)Activator.CreateInstance(typeof(T), new MonoBehaviour[] { this });
        }
    }
    public class BasicBootstrap : ContextView
    {
        // [LabelText("隐藏主角皮肤")]
        // public bool hidePlayer = false;
        [LabelText("关闭背景音乐")]
        public bool closeBGM;
        [LabelText("背景音乐"), HideIf("closeBGM"), InlineEditor(InlineEditorModes.SmallPreview)]
        public AudioClip bgm;
        [BoxGroup("camera", false), LabelText("摄像机剔除方式"), ValueDropdown("GetClearFlags")]
        public CameraClearFlags clearFlags = CameraClearFlags.Skybox;
        [BoxGroup("camera", false), LabelText("摄像机背景颜色"), ShowIf("clearFlags", CameraClearFlags.SolidColor)]
        public Color backgroundColor = Color.white;
        [HorizontalGroup("hands")]
        [ShowIf("ShowHands"), BoxGroup("hands/默认左手", true, true), HideLabel, PreviewField(ObjectFieldAlignment.Center)]
        public Pickupable leftHand;
        [ShowIf("ShowHands"), BoxGroup("hands/默认右手", true, true), HideLabel, PreviewField(ObjectFieldAlignment.Center)]
        public Pickupable rightHand;
        // [LabelText("场景相机")]
        // public Camera contentCamera;
        [LabelText("记录点"), ShowIf("ShowHands")]
        public RecordPoint[] recordPoints;

        protected void Start()
        {
            Camera camera = Camera.main;
            camera.clearFlags = clearFlags;
            if (clearFlags == CameraClearFlags.SolidColor)
                camera.backgroundColor = backgroundColor;
            context.Launch();
        }
#if UNITY_EDITOR
        private static ValueDropdownList<CameraClearFlags> _availiableClearFlags;
        private ValueDropdownList<CameraClearFlags> GetClearFlags()
        {
            if (_availiableClearFlags == null)
            {
                _availiableClearFlags = new ValueDropdownList<CameraClearFlags>();
                _availiableClearFlags.Add("Skybox", CameraClearFlags.Skybox);
                _availiableClearFlags.Add("Solid Color", CameraClearFlags.SolidColor);
                //_availiableClearFlags.Add("Depth Only",CameraClearFlags.Depth);
                //_availiableClearFlags.Add("Don't Clear",CameraClearFlags.Nothing);
            }
            return _availiableClearFlags;
        }
        protected virtual bool ShowHands()
        {
            return true;
        }
#endif
    }
    internal class StartupSignal : Signal { }

    public class BasicContext<SuC> : BasicContext where SuC : StartupCommand
    {
        private bool _isStartupSignalBind;
        public BasicContext(MonoBehaviour view) : base(view) { }
        protected override void mapBindings()
        {
            _isStartupSignalBind = typeof(SuC).IsSubclassOf(typeof(StartupCommand));
            //Debug.Log(this + " has subclass of StartupCommand : " + _isStartupSignalBind);
            if (_isStartupSignalBind) BindCommand<StartupSignal, SuC>(true);
            MapBinds();
        }
        protected virtual void MapBinds() { }
        public override void Launch()
        {
            if (_isStartupSignalBind) injectionBinder.GetInstance<StartupSignal>().Dispatch();
        }
    }
    public class BasicContext : CrossContext
    {
        private static ISemiBinding _viewCache = new SemiBinding();
        private ICommandBinder _commandBinder;
        private IMediationBinder _mediationBinder;
        public BasicContext(MonoBehaviour view) : base(view, ContextStartupFlags.MANUAL_LAUNCH) { }
        override public IContext SetContextView(object view)
        {
            contextView = (view as MonoBehaviour).gameObject;
            if (contextView == null)
                throw new ContextException("BasicContext requires a ContextView of type MonoBehaviour", ContextExceptionType.NO_CONTEXT_VIEW);
            return this;
        }
        protected override void addCoreComponents()
        {
            base.addCoreComponents();
            injectionBinder.Bind<IInstanceProvider>().Bind<IInjectionBinder>().ToValue(injectionBinder);
            injectionBinder.Bind<IContext>().ToValue(this).ToName(ContextKeys.CONTEXT);
            injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
            injectionBinder.Bind<IMediationBinder>().To<SignalViewBinder>().ToSingleton();
        }
        protected override void instantiateCoreComponents()
        {
            base.instantiateCoreComponents();
            injectionBinder.Bind<GameObject>().ToValue(contextView).ToName(ContextKeys.CONTEXT_VIEW);
            _commandBinder = injectionBinder.GetInstance<ICommandBinder>();
            _mediationBinder = injectionBinder.GetInstance<IMediationBinder>();
        }

        protected override void postBindings()
        {
            MediateViewCache();
            _mediationBinder.Trigger(MediationEvent.AWAKE, (contextView as GameObject).GetComponent<ContextView>());
        }

        override public void AddView(object view)
        {
            if (_mediationBinder == null) CacheView(view as MonoBehaviour);
            else _mediationBinder.Trigger(MediationEvent.AWAKE, view as IView);
        }

        override public void RemoveView(object view)
        {
            _mediationBinder.Trigger(MediationEvent.DESTROYED, view as IView);
        }
        protected void CacheView(MonoBehaviour view)
        {
            if (_viewCache.constraint.Equals(BindingConstraintType.ONE))
                _viewCache.constraint = BindingConstraintType.MANY;

            _viewCache.Add(view);
        }
        protected void MediateViewCache()
        {
            if (_mediationBinder == null)
                throw new ContextException("BasicContext cannot mediate views without a mediationBinder", ContextExceptionType.NO_MEDIATION_BINDER);

            object[] values = _viewCache.value as object[];
            if (values == null) return;

            for (int i = 0; i < values.Length; i++)
                _mediationBinder.Trigger(MediationEvent.AWAKE, values[i] as IView);

            _viewCache = new SemiBinding();
        }
        protected void BindCommand<S, C>(bool once = false)
        {
            ICommandBinding binding = _commandBinder.Bind<S>().To<C>();
            if (once) binding.Once();
        }
        protected void BindSingleton<T>(bool crossContext = false)
        {
            IInjectionBinding binding = injectionBinder.Bind<T>().ToSingleton();
            if (crossContext) binding.CrossContext();
        }
        protected void BindValue<T>(object name, T value, bool crossContext = false)
        {
            IInjectionBinding binding = injectionBinder.Bind<T>().ToName(name).ToValue(value);
            if (crossContext) binding.CrossContext();
        }
        protected void BindValue<T>(T value, bool crossContext = false)
        {
            IInjectionBinding binding = injectionBinder.Bind<T>().ToValue(value);
            if (crossContext) binding.CrossContext();
        }
    }
    ///==========================================
    ///启动命令
    ///做一系列的初始化动作，包括：
    ///背景音乐、内容像机、手部姿势、场景记录、寻路、玩家位置等
    ///==========================================
    public class StartupCommand : Command
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }
        private BasicBootstrap _bootstrap;
        protected BasicBootstrap bootstrap { get { return _bootstrap ?? (_bootstrap = GetComponent<BasicBootstrap>()); } }
        protected GameManager _gm;
        protected GameManager gm { get { return _gm ?? (_gm = GetComponent<GameManager>("GameManager")); } }
        public override void Execute()
        {
            //Debug.Log("start up execute "+contextView);
            InitBGM();
            InitContentCamera();
            InitHandPicker();
            ReadSceneRecord();
            BuildNavMesh();
            InitPlayerSpace();
        }
        protected void InitHandPicker()
        {
            this.InitHand(bootstrap.leftHand, bootstrap.rightHand);
        }
        protected void InitBGM()
        {
            //if (bootstrap.hidePlayer) bootstrap.GetPlayer().HideSkin();
            //Debug.Log(this + ".InitBGM..." + bbs.closeBGM + " >> " + bbs.bgm);
            if (bootstrap.closeBGM) bootstrap.PlayBGM(null, 0);
            else if (bootstrap.bgm != null) bootstrap.PlayBGM(bootstrap.bgm, 0);
        }
        protected void InitContentCamera()
        {
            this.SetContentCamera(GetComponent<Camera>("ContentCamera"));
        }
        protected void BuildNavMesh()
        {
            FaymarNavMeshBuilder fnmb = GetComponent<FaymarNavMeshBuilder>("LocalNavMeshBuilder");
            fnmb.BuildAsync();
        }
        protected void ReadSceneRecord()
        {
            if (gm == null) return;
            //必须初始化需要记录的对象 //如果上一个场景类型为level，读取数据；
            gm.InitRecords(this.GetLastSceneName() != SceneName.Level);
            gm.ReadScene();
        }
        protected void InitPlayerSpace()
        {
            //Debug.Log("scene :: " + generalData.lastScene + " -> " + generalData.currentScene);
            RecordPoint rp;
            Vector3 pos;
            Vector3 rot;
            if (gm != null && gm.ReadPlayer(out pos, out rot))
            {
                rp = GetPlayerPosition(0);
                rp.position = pos;
                rp.rotation = rot;
            }
            else if (this.GetLastSceneName() == SceneName.Level && this.GetLoadType() != LoadType.Reload)
            {
                rp = GetPlayerPosition();
            }
            else
            {
                rp = GetPlayerPosition(0);
            }
            rp.MovePlayer(0, 0);
            rp.ReadyToShow();
        }
        protected RecordPoint GetPlayerPosition(int index = -1)
        {
            int i = bootstrap.recordPoints.Length;
            if (index >= 0 && index < i) return bootstrap.recordPoints[index];
            string name = "PlayerPositionFrom" + this.GetLastSceneName();
            while (--i >= 0)
            {
                if (bootstrap.recordPoints[i].name == name) return bootstrap.recordPoints[i];
            }
            return bootstrap.recordPoints[0];
        }
        protected T GetComponent<T>(string name) where T : Component
        {
            Transform t = contextView.transform.Find(name);
            //Debug.Log("get component "+t+"//"+contextView);
            if (t == null) return null;
            return t.GetComponent<T>();
        }
        protected T GetComponent<T>() where T : Component
        {
            return contextView.GetComponent<T>();
        }
    }
}