using System.Collections.Generic;
using Configurations.Script;
using Leopotam.Ecs;
using Leopotam.Ecs.Ui.Systems;
using Pixelgrid.DataModels;
using Pixelgrid.ScriptableObjects;
using Pixelgrid.Systems.Audio;
using Pixelgrid.Systems.Execution;
using Pixelgrid.Systems.GameModes.ColorPicker;
using UnityEngine;
using UnityEngine.UI;

namespace Pixelgrid.Startups
{
    sealed class ColorPickerStartup : MonoBehaviour
    {
        [SerializeField] EcsUiEmitter _ecsUiEmitter;
        EcsWorld _world;
        EcsSystems _systems;

        private ColorPickerModels _colorPickerModels;

        public ColorPickerConfigs ColorPickerConfigs;
        public GameModeConfiguration GameModeConfiguration;
        public DifficultyConfiguration difficultyConfiguration;
        public ImageHolderContainer ImageHolderContainer;
        public AudioPlayer AudioPlayer;
        public EndgameScreenPresenter EndgamePresenter;
        public TutorialScreenPresenter TutorialPresenter;
        public ProgressBar ProgressBar;
        public UiScreenContainer ScreenContainer;
        public Slider Slider;
        public GameContent GameContent;

        void Start()
        {
            var i18n = I18n.Instance;
            I18n.SetLocale("ru-RU");
            // void can be switched to IEnumerator for support coroutines.
            _world = new EcsWorld();
            _systems = new EcsSystems(_world);
#if UNITY_EDITOR
            Leopotam.Ecs.UnityIntegration.EcsWorldObserver.Create(_world);
            Leopotam.Ecs.UnityIntegration.EcsSystemsObserver.Create(_systems);
#endif

            var systemNamesContainer = new SystemNamesContainer();
            var systemNames = systemNamesContainer.Systems;
            systemNames.Add("Pausable", new List<string> 
            {
                "UpdateStopwatches",
                "OnSliderValueChange",
                "CheckColorPickerClick"
            });

            _colorPickerModels = new ColorPickerModels();

            _systems
                 // register your systems here:

                 //InitSystems go here:
                 .Add(new LoadPlayerSystem())
                 .Add(new SetDifficultySystem())
                 .Add(new CreateStatDataTrackerSystem())
                 .Add(new LoadTutorialMessageSystem())
                 .Add(new CreateColorContainerSystem())
                 .Add(new CreateColorDataSystem())
                 .Add(new LaunchGameplayLoopSystem())
                 //The rest of the systems go here:
                 .Add(new CheckPauseClickSystem())
                 .Add(new CheckRestartClickSystem())
                 .Add(new UpdateStopwatchesSystem(), "UpdateStopwatches")
                 .Add(new ResetColorsDataSystem())
                 .Add(new ResetProgressBarSystem())
                 .Add(new ResetStopwatchTimeSystem())
                 .Add(new ResetStatTrackerSystem())
                 .Add(new StartGameSystem())
                 .Add(new OnSliderValueChangeSystem(), "OnSliderValueChange")
                 .Add(new CheckColorPickerClickSystem(), "CheckColorPickerClick")
                 .Add(new LaunchStatTrackerStopwatchSystem())
                 .Add(new CheckColorPickerAnswerSystem())
                 .Add(new UpdateStatDataSystem())
                 .Add(new UpdateProgressBarSystem())
                 .Add(new GameOverOnTimerEndSystem())
                 .Add(new DisableStopwatchOnGameOverSystem())
                 .Add(new ShowEndgameScreenSystem())
                 .Add(new PauseSystem())
                 .Add(new UnpauseSystem())
                 .Add(new DisableSystemsByTypeSystem(_systems, systemNamesContainer))
                 .Add(new EnableSystemsByTypeSystem(_systems, systemNamesContainer))
                 .Add(new UpdateImageSpritesSystem())
                 .Add(new EnqueueCorrectAnswerAudioClipSystem())
                 .Add(new EnqueueWrongAnswerAudioClipSystem())
                 .Add(new EnqueueGameOverAudioClipSystem())
                 .Add(new PlayAudioClipSystem())
                 .InjectUi(_ecsUiEmitter)

                 // register one-frame components (order is important), for example:
                 .OneFrame<ColorChosenEvent>()
                 .OneFrame<StartGameEvent>()
                 .OneFrame<GameModeDataGeneratedEvent>()
                 .OneFrame<CorrectAnswerEvent>()
                 .OneFrame<WrongAnswerEvent>()
                 .OneFrame<TimeChangeEvent>()
                 .OneFrame<GameOverEvent>()
                 .OneFrame<RestartGameEvent>()
                 .OneFrame<PauseEvent>()
                 .OneFrame<UnpauseEvent>()
                 .OneFrame<DisableSystemTypeEvent>()
                 .OneFrame<EnableSystemTypeEvent>()
                 .OneFrame<UpdateSpriteImageEvent>()

                 // inject service instances here (order doesn't important), for example:
                 .Inject(_colorPickerModels.ColorPickerDataModel)
                 .Inject(_colorPickerModels.ColorContainerModel)
                 .Inject(ColorPickerConfigs)
                 .Inject(GameModeConfiguration)
                 .Inject(difficultyConfiguration)
                 .Inject(ImageHolderContainer)
                 .Inject(GameContent.AudioContent)
                 .Inject(AudioPlayer)
                 .Inject(EndgamePresenter)
                 .Inject(TutorialPresenter)
                 .Inject(ProgressBar)
                 .Inject(ScreenContainer)
                 .Inject(Slider)
                 .Inject(i18n)
                 .Init();
        }

        void Update()
        {
            _systems?.Run();
        }

        void OnDestroy()
        {
            if (_systems != null)
            {
                _systems.Destroy();
                _systems = null;
                _world.Destroy();
                _world = null;
            }
        }
    }
}
