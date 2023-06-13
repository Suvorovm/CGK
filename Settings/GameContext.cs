using System;
using System.Collections.Generic;
using Client.CameraPlayer.Service;
using Client.Core.Analytics;
using Client.Core.CameraMove.Service;
using Client.Core.Console;
using Client.Core.DataSo;
using Client.Core.Descriptor.Service;
using Client.Core.Event.Service;
using Client.Core.ResourceService;
using Client.Core.Reward.Service;
using Client.Core.Snapshot.Factory;
using Client.Core.Snapshot.Service;
using Client.Core.Trigger.Factory;
using Client.Core.Trigger.Service;
using Client.Core.World;
using Client.Farm.Descriptor;
using Client.Farm.Service;
using Client.OneStepAnalytics.Service;
using Client.Player.Descriptor;
using Client.Player.Service;
using Client.Quest.Descriptor;
using Client.Quest.Service;
using Client.Tutorial.Service;
using Client.UI.Service;
using Cysharp.Threading.Tasks;

namespace Client.Core.Settings
{
    public class GameContext
    {
        private static GameContext _gameContext;
        private const string FILE_SAVED = "Invasion/snapchotData.data";

        private bool _inited;
        private List<IDisposable> _disposablesService = new List<IDisposable>();

        public static GameContext Instance
        {
            get
            {
                if (_gameContext == null)
                {
                    _gameContext = new GameContext();
                }

                return _gameContext;
            }
        }

        public DescriptorService DescriptorService { get; private set; }

        public TriggerService TriggerService { get; private set; }

        public QuestService QuestService { get; private set; }

        public RewardService RewardService { get; private set; }

        public DataSoHub DataSoHub { get; private set; }

        public SnapshotService SnapshotService { get; private set; }

        public PlayerService PlayerService { get; private set; }

        public FarmService FarmService { get; set; }

        public EventDispatcher EventDispatcher { get; set; }

        public AutoSnapshotService AutoSnapshotService { get; set; }

        public CacheResourceService ResourceService { get; set; }

        public IconService IconService { get; set; }

        public TutorialService TutorialService { get; set; }
        public GameWorld GameWorld { get; set; }
        public CameraMoveService CameraMoveService { get; set; }
        public CameraPlayerLevelMove CameraPlayerLevelMove { get; set; }

        public AnalyticsFacade AnalyticsFacade { get; set; }

        public OneTimeAnalyticsService OneTimeAnalyticsService { get; set; }

        private GameContext()
        {
        }

        private async UniTask ConfigureAnalytics()
        {
            AnalyticsFacade = new AnalyticsFacade(new List<IAnalyticsAdapter>()
            {
                new GameAnalyticsAdapter(),
                new FireBaseAnalyticsAdapter(),
                new FaceBookAnalyticsAdapter()
            });
             await AnalyticsFacade.Init();
        }

        public async UniTask Init()
        {
            if (_inited)
            {
                return;
            }

            ResourceService = new CacheResourceService();
            LoadDescriptors();
            await ConfigureAnalytics();
            EventDispatcher = new EventDispatcher();
            DataSoHub = new DataSoHub();
            GameWorld = new GameWorld(EventDispatcher);
            CameraMoveService = new CameraMoveService();
            PlayerService = new PlayerService(DataSoHub, AnalyticsFacade);
            TriggerService = new TriggerService(new TriggerFactory(), this);
            RewardService = new RewardService(PlayerService);
            QuestService = new QuestService(TriggerService, DescriptorService, RewardService, AnalyticsFacade);
            AutoSnapshotService = new AutoSnapshotService(EventDispatcher, DescriptorService);
            IconService = new IconService(DescriptorService, ResourceService);
            TutorialService = new TutorialService(EventDispatcher);
            FarmService = new FarmService(DescriptorService, RewardService, PlayerService, EventDispatcher,
                AnalyticsFacade);
            CameraPlayerLevelMove =
                new CameraPlayerLevelMove(CameraMoveService, PlayerService, EventDispatcher, GameWorld);
            OneTimeAnalyticsService =
                new OneTimeAnalyticsService(AnalyticsFacade, EventDispatcher, PlayerService, QuestService);
            LoadSnapshot();
            ConfigureConsole();
            RegisterDisposable();
            _inited = true;
        }

        private void RegisterDisposable()
        {
            _disposablesService = new List<IDisposable>()
            {
                SnapshotService,
                TutorialService,
                GameWorld,
                CameraMoveService,
                CameraPlayerLevelMove,
                OneTimeAnalyticsService
            };
        }

        private void LoadSnapshot()
        {
            SnapshotFactory snapshotFactory =
                new SnapshotFactory(PlayerService, FarmService, QuestService, TutorialService, OneTimeAnalyticsService);
            SnapshotService = new SnapshotService(snapshotFactory, EventDispatcher, FILE_SAVED);
            if (!SnapshotService.HasValidSnapshot())
            {
                SnapshotService.CreateDefaultSnapshot();
                return;
            }

            SnapshotService.LoadSnapshot();
        }

        private void ConfigureConsole()
        {
            GameConfig gameConfig = DescriptorService.GetDescriptor<GameConfig>();
            if (gameConfig.BuildSettings.CheatBuild)
            {
                ConsoleCommand.CreateConsole();
            }
        }

        private void Clear()
        {
            foreach (IDisposable disposable in _disposablesService)
            {
                disposable.Dispose();
            }

            _disposablesService.Clear();
        }

        private void LoadDescriptors()
        {
            DescriptorService = new DescriptorService();
            DescriptorService.LoadDescriptor<GameConfig>(Descriptor.Descriptor.BuildSettings);
            DescriptorService.LoadDescriptor<GameSettings>(Descriptor.Descriptor.GameSettings);
            DescriptorService.LoadDescriptor<QuestConfig>(Descriptor.Descriptor.Quest);
            DescriptorService.LoadDescriptor<ResourceDescriptor>(Descriptor.Descriptor.ResourceConfiguration);
            DescriptorService.LoadDescriptor<FarmConfigDescriptor>(Descriptor.Descriptor.Farm);
        }

        public static void ClearContext()
        {
            _gameContext.Clear();
            _gameContext = null;
        }
    }
}