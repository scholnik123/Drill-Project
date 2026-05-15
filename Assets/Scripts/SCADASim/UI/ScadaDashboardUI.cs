using System.Collections.Generic;
using SCADASim.Audio;
using SCADASim.Bootstrap;
using SCADASim.Core;
using SCADASim.Crew;
using SCADASim.Environment;
using SCADASim.Physics;
using SCADASim.Trajectory;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace SCADASim.UI
{
    public sealed class ScadaDashboardUI : MonoBehaviour
    {
        private const float WellboreVisualScale = 0.035f;
        private const float WellboreVisualRadiusMeters = 4.0f;
        private const int TutorialStepCount = 8;

        private static readonly Color GraphRed = new Color(0.88f, 0.02f, 0.08f);
        private static readonly Color GraphGreen = new Color(0.13f, 0.55f, 0.32f);
        private static readonly Color GraphBlue = new Color(0.26f, 0.52f, 1f);
        private static readonly Color GraphBlack = new Color(0.05f, 0.05f, 0.05f);
        private static readonly GraphAxisScale TorqueScale = new GraphAxisScale("МОМЕНТ, кНм", 0f, 65f);
        private static readonly GraphAxisScale VibrationScale = new GraphAxisScale("ВИБР., %", 0f, 100f);
        private static readonly GraphAxisScale StandpipePressureScale = new GraphAxisScale("ДАВЛ., бар", 0f, 360f);
        private static readonly GraphAxisScale FlowBalanceScale = new GraphAxisScale("БАЛАНС, л/мин", -600f, 600f);
        private static readonly GraphAxisScale RopScale = new GraphAxisScale("ROP, м/ч", 0f, 45f);
        private static readonly GraphAxisScale RiskPercentScale = new GraphAxisScale("РИСК, %", 0f, 100f);
        private static readonly GraphAxisScale BottomHolePressureScale = new GraphAxisScale("ЗАБОЙ, МПа", 0f, 80f);
        private static readonly GraphAxisScale PorePressureScale = new GraphAxisScale("ПЛАСТ, МПа", 0f, 80f);

        private readonly List<string> locationChoices = new List<string>
        {
            "СУША - НАЗЕМНАЯ БУРОВАЯ",
            "МОРЕ - МОРСКАЯ ПЛАТФОРМА"
        };

        private readonly List<string> profileChoices = new List<string>
        {
            "ВЕРТИКАЛЬНАЯ",
            "J-ПРОФИЛЬ",
            "S-ПРОФИЛЬ",
            "ГОРИЗОНТАЛЬНАЯ"
        };

        private readonly List<string> depthChoices = new List<string>
        {
            "0 м - забуривание, кондуктор 324 мм",
            "500 м - под башмаком кондуктора",
            "1500 м - промежуточная колонна 245 мм",
            "2500 м - набор угла и продуктивный интервал",
            "3500 м - глубокий наклонный ствол",
            "4500 м - длинный горизонтальный участок"
        };

        private readonly List<string> regionChoices = new List<string>
        {
            "ЗАПАДНАЯ СИБИРЬ",
            "ВОЛГО-УРАЛЬСКИЙ РЕГИОН",
            "АРКТИЧЕСКИЙ ШЕЛЬФ",
            "ПРИКАСПИЙСКИЙ БАССЕЙН"
        };

        private readonly List<string> difficultyChoices = new List<string>
        {
            "ТРЕНИРОВКА",
            "ЛЕГКИЙ РЕЖИМ",
            "ПРОМЫСЛОВЫЙ РЕЖИМ",
            "ЭКСПЕРТНЫЙ РЕЖИМ"
        };

        private readonly List<string> physicsChoices = new List<string>
        {
            "СТАБИЛЬНАЯ ФИЗИКА",
            "ПОЛЕВАЯ ФИЗИКА",
            "ЖЕСТКИЕ ОСЛОЖНЕНИЯ"
        };

        private readonly List<string> crewChoices = new List<string>
        {
            "7 ЧЕЛОВЕК (ПОЛНАЯ)",
            "4 ЧЕЛОВЕКА (СТАНДАРТ)",
            "3 ЧЕЛОВЕКА (СОКРАЩЕННАЯ)",
            "СТАЖЕРСКАЯ СМЕНА"
        };

        private readonly List<string> tutorialChoices = new List<string>
        {
            "ВКЛЮЧИТЬ ОБУЧЕНИЕ",
            "ПРОПУСТИТЬ ОБУЧЕНИЕ"
        };

        private DrillingModel drillingModel;
        private CrewManager crewManager;
        private EnvironmentManager environmentManager;
        private RigModelManager rigModelManager;
        private WellboreProceduralMesh wellbore;
        private SCADASimCameraOrbit cameraOrbit;
        private AIAssistantPortrait aiAssistantPortrait;
        private ScadaMusicPlayer musicPlayer;
        private SimulationEventChannel eventChannel;

        private UIDocument document;
        private PanelSettings panelSettings;
        private VisualElement startScreen;
        private VisualElement mainScreen;
        private VisualElement introScreen;
        private VisualElement telemetryPage;
        private VisualElement profilePage;
        private VisualElement tutorialOverlay;
        private Button telemetryTab;
        private Button profileTab;
        private Button tutorialNextButton;
        private Label tutorialTitle;
        private Label tutorialBody;
        private float nextRefreshTime;
        private int tutorialStepIndex;
        private VideoPlayer introVideoPlayer;
        private AudioSource introAudioSource;
        private RenderTexture introRenderTexture;
        private Image introVideoImage;
        private Texture2D[] introFrames;
        private bool introCompleted;
        private bool introPrepared;
        private double introLastVideoTime;
        private float introLastProgressAt;

        private ChoiceBinding locationChoice;
        private ChoiceBinding profileChoice;
        private ChoiceBinding depthChoice;
        private ChoiceBinding regionChoice;
        private ChoiceBinding difficultyChoice;
        private ChoiceBinding physicsChoice;
        private ChoiceBinding crewChoice;
        private ChoiceBinding tutorialChoice;

        private Label configLine;
        private Label rigModelStatus;
        private Label environmentStatus;
        private Label formationStatusValue;
        private Label pressurePoreValue;
        private Label pressureBottomValue;
        private Label pressureGradientValue;
        private Label radioLogValue;
        private Label crewStatusValue;
        private Image aiAssistantImage;
        private Label aiAssistantCaption;
        private Label aiRecommendationValue;
        private Label supervisorTaskValue;
        private Label taskEconomyValue;
        private Label incidentConsequenceValue;
        private SupervisorTask activeSupervisorTask;
        private bool hasActiveSupervisorTask;

        private Label rpmValue;
        private Label torqueValue;
        private Label ropValue;
        private Label wobValue;
        private Label sppValue;
        private Label flowInValue;
        private Label flowOutValue;
        private Label mudWeightValue;
        private Label depthValue;
        private Label bottomTempValue;
        private Label ecdValue;
        private Label vibrationValue;
        private Label incValue;
        private Label azimuthValue;
        private Label bitWearValue;

        private Label rpmControlValue;
        private Label wobControlValue;
        private Label flowControlValue;
        private Label mudControlValue;
        private Label chokeControlValue;
        private Label timeSpeedValue;
        private Label musicStatusValue;
        private KnobControlElement rpmKnob;
        private KnobControlElement wobKnob;
        private KnobControlElement flowKnob;
        private KnobControlElement mudKnob;
        private KnobControlElement chokeKnob;

        private TrendGraphElement rotationGraph;
        private TrendGraphElement hydraulicGraph;
        private TrendGraphElement bottomGraph;
        private TrendGraphElement pressureGraph;

        public void Configure(
            DrillingModel model,
            CrewManager crew,
            EnvironmentManager environment,
            RigModelManager rigModel,
            WellboreProceduralMesh wellboreMesh,
            SCADASimCameraOrbit orbit,
            AIAssistantPortrait assistantPortrait,
            ScadaMusicPlayer music,
            SimulationEventChannel channel)
        {
            drillingModel = model;
            crewManager = crew;
            environmentManager = environment;
            rigModelManager = rigModel;
            wellbore = wellboreMesh;
            cameraOrbit = orbit;
            aiAssistantPortrait = assistantPortrait;
            musicPlayer = music;
            eventChannel = channel;

            if (eventChannel != null)
            {
                eventChannel.Raised += HandleSimulationEvent;
            }

            drillingModel?.SetSimulating(false);
            Build();
        }

        public void SetAssistantPortrait(AIAssistantPortrait assistantPortrait)
        {
            aiAssistantPortrait = assistantPortrait;
            if (aiAssistantImage != null && aiAssistantPortrait != null)
            {
                aiAssistantImage.image = aiAssistantPortrait.PortraitTexture;
            }
        }

        public void DebugStartDefaultSessionForCapture()
        {
            ApplyConfigAndStart();
            HideTutorialOverlay();
            ShowTelemetryPage();
        }

        private void OnDestroy()
        {
            if (eventChannel != null)
            {
                eventChannel.Raised -= HandleSimulationEvent;
            }

            if (panelSettings != null)
            {
                Destroy(panelSettings);
            }

            if (introRenderTexture != null)
            {
                introRenderTexture.Release();
                Destroy(introRenderTexture);
            }
        }

        private void Update()
        {
            if (drillingModel == null || Time.time < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.time + 0.15f;
            Refresh(drillingModel.CurrentState);
        }

        private void Build()
        {
            document = gameObject.GetComponent<UIDocument>();
            if (document == null)
            {
                document = gameObject.AddComponent<UIDocument>();
            }
            document.enabled = false;

            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1600, 900);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;

            ThemeStyleSheet themeStyleSheet = Resources.Load<ThemeStyleSheet>("SCADASim/UnityDefaultRuntimeTheme");
            if (themeStyleSheet != null)
            {
                panelSettings.themeStyleSheet = themeStyleSheet;
            }

            document.panelSettings = panelSettings;
            document.sortingOrder = 100;
            document.enabled = true;

            VisualElement root = document.rootVisualElement;
            root.Clear();
            root.AddToClassList("scada-root");

            StyleSheet styleSheet = Resources.Load<StyleSheet>("SCADASim/ScadaDashboard");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            BuildStartScreen(root);
            BuildMainScreen(root);
            BuildIntroScreen(root);
            
            // VERY PROMINENT DEBUG BOX
            VisualElement debugBox = new VisualElement();
            debugBox.style.width = 300;
            debugBox.style.height = 300;
            debugBox.style.backgroundColor = Color.magenta;
            debugBox.style.position = Position.Absolute;
            debugBox.style.top = 100;
            debugBox.style.left = 100;
            debugBox.style.borderBottomColor = Color.black;
            debugBox.style.borderBottomWidth = 5;
            debugBox.pickingMode = PickingMode.Ignore;
            Label debugText = new Label("DEBUG: UI REBUILT");
            debugText.style.fontSize = 30;
            debugText.style.color = Color.white;
            debugBox.Add(debugText);
            root.Add(debugBox);

            ShowIntroOrStartScreen();
        }

        private void BuildIntroScreen(VisualElement root)
        {
            introScreen = new VisualElement();
            introScreen.AddToClassList("intro-screen");
            root.Add(introScreen);

            Image videoImage = new Image();
            videoImage.AddToClassList("intro-video");
            introScreen.Add(videoImage);
            introVideoImage = videoImage;

            Label caption = new Label("SCADA INTELLIGENT SIMULATION v2.0");
            caption.AddToClassList("intro-caption");
            introScreen.Add(caption);

            Button skipButton = new Button(FinishIntro);
            skipButton.text = "ПРОПУСТИТЬ";
            skipButton.AddToClassList("intro-skip-button");
            introScreen.Add(skipButton);

            if (TryStartFrameSequenceIntro())
            {
                return;
            }

            string videoPath = Path.Combine(Application.streamingAssetsPath, "CompanyIntro_unity.mp4");
            if (!File.Exists(videoPath))
            {
                videoPath = Path.Combine(Application.streamingAssetsPath, "CompanyIntro.mp4");
            }

            if (!File.Exists(videoPath))
            {
                introCompleted = true;
                introScreen.style.display = DisplayStyle.None;
                return;
            }

            introRenderTexture = new RenderTexture(1600, 900, 0, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 2,
                name = "Company Intro Video"
            };
            videoImage.image = introRenderTexture;

            introAudioSource = gameObject.AddComponent<AudioSource>();
            introAudioSource.playOnAwake = false;

            introVideoPlayer = gameObject.AddComponent<VideoPlayer>();
            introVideoPlayer.playOnAwake = false;
            introVideoPlayer.isLooping = false;
            introVideoPlayer.waitForFirstFrame = false;
            introVideoPlayer.skipOnDrop = true;
            introVideoPlayer.source = VideoSource.Url;
            introVideoPlayer.url = videoPath;
            introVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            introVideoPlayer.targetTexture = introRenderTexture;
            introVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            introVideoPlayer.EnableAudioTrack(0, true);
            introVideoPlayer.SetTargetAudioSource(0, introAudioSource);
            introVideoPlayer.prepareCompleted += _ =>
            {
                introPrepared = true;
                introLastVideoTime = introVideoPlayer.time;
                introLastProgressAt = Time.unscaledTime;
                introVideoPlayer.Play();
            };
            introVideoPlayer.loopPointReached += _ => FinishIntro();
            introVideoPlayer.errorReceived += (_, message) =>
            {
                Debug.LogWarning($"Company intro video error: {message}");
                FinishIntro();
            };
            introVideoPlayer.Prepare();
            StartCoroutine(WatchIntroPlayback());
        }

        private bool TryStartFrameSequenceIntro()
        {
            introFrames = Resources.LoadAll<Texture2D>("SCADASim/IntroFrames");
            if (introFrames == null || introFrames.Length == 0 || introVideoImage == null)
            {
                return false;
            }

            System.Array.Sort(introFrames, (left, right) => string.CompareOrdinal(left.name, right.name));
            StartCoroutine(PlayIntroFrames());
            return true;
        }

        private IEnumerator PlayIntroFrames()
        {
            const float frameDelaySeconds = 1f / 12f;

            for (int i = 0; i < introFrames.Length && !introCompleted; i++)
            {
                introVideoImage.image = introFrames[i];
                yield return new WaitForSecondsRealtime(frameDelaySeconds);
            }

            FinishIntro();
        }

        private IEnumerator WatchIntroPlayback()
        {
            float startedAt = Time.unscaledTime;
            introLastProgressAt = startedAt;

            while (!introCompleted)
            {
                yield return null;

                float elapsed = Time.unscaledTime - startedAt;
                if (elapsed > 14f)
                {
                    FinishIntro();
                    yield break;
                }

                if (!introPrepared)
                {
                    if (elapsed > 4f)
                    {
                        Debug.LogWarning("Company intro video prepare timeout. Skipping intro.");
                        FinishIntro();
                        yield break;
                    }

                    continue;
                }

                if (introVideoPlayer == null)
                {
                    FinishIntro();
                    yield break;
                }

                double currentTime = introVideoPlayer.time;
                if (currentTime > introLastVideoTime + 0.03)
                {
                    introLastVideoTime = currentTime;
                    introLastProgressAt = Time.unscaledTime;
                    continue;
                }

                if (Time.unscaledTime - introLastProgressAt > 2.8f)
                {
                    Debug.LogWarning("Company intro video stalled. Skipping intro.");
                    FinishIntro();
                    yield break;
                }
            }
        }

        private void BuildStartScreen(VisualElement root)
        {
            startScreen = new VisualElement();
            startScreen.AddToClassList("start-screen");
            root.Add(startScreen);

            VisualElement titleBar = new VisualElement();
            titleBar.AddToClassList("window-titlebar");
            startScreen.Add(titleBar);
            titleBar.Add(new Label("LUKOIL X ZVZ :: ИНТЕЛЛЕКТУАЛЬНОЕ БУРЕНИЕ v2.0"));

            VisualElement body = new VisualElement();
            body.AddToClassList("start-body");
            startScreen.Add(body);

            VisualElement brand = new VisualElement();
            brand.AddToClassList("brand-zone");
            body.Add(brand);

            Label lukoil = new Label("LUKOIL");
            lukoil.AddToClassList("lukoil-logo");
            brand.Add(lukoil);

            Label cross = new Label("x");
            cross.AddToClassList("brand-cross");
            brand.Add(cross);

            Label zvz = new Label("ZVZ");
            zvz.AddToClassList("zvz-logo");
            brand.Add(zvz);

            Label subtitle = new Label("S C A D A   I N T E L L I G E N T   S I M U L A T I O N   v 2 . 0");
            subtitle.AddToClassList("start-subtitle");
            brand.Add(subtitle);

            Label scenario = new Label("1500M: промежуточная колонна 245MM. Классическая глубина перехода осложненного участка - граница мягких глин и плотных песчаников. Здесь возрастают пластовое давление и начинаются реальные геологические вызовы.");
            scenario.AddToClassList("scenario-box");
            brand.Add(scenario);

            VisualElement configPanel = new VisualElement();
            configPanel.AddToClassList("start-config-panel");
            body.Add(configPanel);

            Label configTitle = new Label("КОНФИГУРАЦИЯ СКВАЖИНЫ");
            configTitle.AddToClassList("start-config-title");
            configPanel.Add(configTitle);

            locationChoice = AddConfigChoice(configPanel, "ЛОКАЦИЯ:", locationChoices, 0);
            profileChoice = AddConfigChoice(configPanel, "ПРОФИЛЬ СКВАЖИНЫ:", profileChoices, 0);
            depthChoice = AddConfigChoice(configPanel, "НАЧАЛЬНАЯ ГЛУБИНА:", depthChoices, 0);
            regionChoice = AddConfigChoice(configPanel, "ГЕОЛОГИЧЕСКИЙ РЕГИОН:", regionChoices, 0);
            difficultyChoice = AddConfigChoice(configPanel, "РЕАЛИЗМ (СЛОЖНОСТЬ):", difficultyChoices, 0);
            physicsChoice = AddConfigChoice(configPanel, "НАСТРОЙКИ ФИЗИКИ:", physicsChoices, 0);
            crewChoice = AddConfigChoice(configPanel, "СОСТАВ БРИГАДЫ:", crewChoices, 0);
            tutorialChoice = AddConfigChoice(configPanel, "НАЧАЛЬНОЕ ОБУЧЕНИЕ:", tutorialChoices, 0);

            Button startButton = new Button(ApplyConfigAndStart);
            startButton.text = "ИНИЦИАЛИЗАЦИЯ СИСТЕМЫ";
            startButton.AddToClassList("start-button");
            configPanel.Add(startButton);
        }

        private void BuildMainScreen(VisualElement root)
        {
            mainScreen = new VisualElement();
            mainScreen.AddToClassList("main-screen");
            root.Add(mainScreen);

            VisualElement tabs = new VisualElement();
            tabs.AddToClassList("main-tabs");
            mainScreen.Add(tabs);

            telemetryTab = CreateTabButton("ТЕЛЕМЕТРИЯ", ShowTelemetryPage);
            profileTab = CreateTabButton("3D-ПРОФИЛЬ СКВАЖИНЫ", ShowProfilePage);
            tabs.Add(telemetryTab);
            tabs.Add(profileTab);

            VisualElement profileControls = new VisualElement();
            profileControls.AddToClassList("profile-view-buttons");
            tabs.Add(profileControls);
            profileControls.Add(CreateFlatButton("СЛЕДИТЬ", () => cameraOrbit?.SetView(140f, 28f, 32f)));
            profileControls.Add(CreateFlatButton("ВСЯ СКВАЖИНА", () => cameraOrbit?.SetView(145f, 38f, 62f)));
            profileControls.Add(CreateFlatButton("3/4", () => cameraOrbit?.SetView(140f, 28f, 36f)));
            profileControls.Add(CreateFlatButton("СБОКУ", () => cameraOrbit?.SetView(90f, 18f, 42f)));
            profileControls.Add(CreateFlatButton("СВЕРХУ", () => cameraOrbit?.SetView(180f, 72f, 54f)));
            profileControls.Add(CreateFlatButton("x10 ВРЕМЯ", () => SetSimulationSpeed(10f)));
            profileControls.Add(CreateFlatButton("МУЗЫКА", ToggleMusic));
            profileControls.Add(CreateFlatButton("ГЛАВНОЕ МЕНЮ", ReturnToMainMenu));

            telemetryPage = new VisualElement();
            telemetryPage.AddToClassList("telemetry-page");
            mainScreen.Add(telemetryPage);
            BuildTelemetryPage(telemetryPage);

            profilePage = new VisualElement();
            profilePage.AddToClassList("profile-page");
            mainScreen.Add(profilePage);
            BuildProfilePage(profilePage);

            BuildTutorialOverlay(mainScreen);
        }

        private void BuildTelemetryPage(VisualElement page)
        {
            VisualElement left = new VisualElement();
            left.AddToClassList("telemetry-left");
            page.Add(left);

            rotationGraph = AddGraphPanel(left, "ВРАЩЕНИЕ / МОМЕНТ", GraphRed, GraphBlack, TorqueScale, VibrationScale);
            hydraulicGraph = AddGraphPanel(left, "ГИДРАВЛИКА / ЕМКОСТИ", GraphGreen, GraphBlack, StandpipePressureScale, FlowBalanceScale);
            bottomGraph = AddGraphPanel(left, "ЗАБОЙНЫЕ ПАРАМЕТРЫ", GraphBlue, GraphRed, RopScale, RiskPercentScale);

            VisualElement center = new VisualElement();
            center.AddToClassList("telemetry-center");
            page.Add(center);

            VisualElement environmentPanel = CreateScadaPanel("УСЛОВИЯ СРЕДЫ");
            center.Add(environmentPanel);
            VisualElement envTitleRow = new VisualElement();
            envTitleRow.AddToClassList("env-title-row");
            environmentPanel.Add(envTitleRow);
            configLine = new Label("[СУША] | [ВЕРТИКАЛЬНАЯ] | ЗАПАДНАЯ СИБИРЬ | РЕЖИМ: ТРЕНИРОВКА");
            configLine.AddToClassList("config-line");
            envTitleRow.Add(configLine);
            Label lockup = new Label("LUKOIL x ZVZ");
            lockup.AddToClassList("mini-brand");
            envTitleRow.Add(lockup);
            environmentStatus = AddStatusBox(environmentPanel, "ПОГОДА: -15.3 C // ВЕТЕР: 13.2 М/С");
            rigModelStatus = AddStatusBox(environmentPanel, "3D МОДЕЛЬ: ОЖИДАНИЕ");
            formationStatusValue = AddStatusBox(environmentPanel, "ФОРМАЦИЯ: ГЛИНА // TVD: 0M // ECD: 1.10 SG // ВЫНОС ШЛАМА: 86%");

            VisualElement pressurePanel = CreateScadaPanel("БАЛАНС ДАВЛЕНИЙ");
            pressurePanel.AddToClassList("pressure-panel");
            center.Add(pressurePanel);
            VisualElement pressureNumbers = new VisualElement();
            pressureNumbers.AddToClassList("pressure-numbers");
            pressurePanel.Add(pressureNumbers);
            pressurePoreValue = CreatePressureNumber(pressureNumbers, "ПЛАСТОВОЕ (МПа)");
            pressureBottomValue = CreatePressureNumber(pressureNumbers, "ЗАБОЙНОЕ (МПа)");
            pressureGradientValue = CreatePressureNumber(pressureNumbers, "ГРП (МПа)");
            pressureGraph = new TrendGraphElement(GraphRed, GraphBlack, BottomHolePressureScale, PorePressureScale);
            pressureGraph.AddToClassList("pressure-graph");
            pressurePanel.Add(pressureGraph);
            depthValue = AddStatusBox(pressurePanel, "ГЛУБИНА: 0 МЕТРОВ");

            VisualElement crewPanel = CreateScadaPanel("СВЯЗЬ С БРИГАДОЙ (РАЦИЯ)");
            center.Add(crewPanel);
            radioLogValue = AddStatusBox(crewPanel, "[00:00:00] Регион: ЗАПАДНАЯ СИБИРЬ. Бригада: 7 чел. Стартовая готовность.");
            crewStatusValue = AddStatusBox(crewPanel, "БРИГАДА: опыт 78% // усталость 12% // мораль 82% // реакция 1.0 с.");
            VisualElement crewImageStrip = new VisualElement();
            crewImageStrip.AddToClassList("crew-image-strip");
            crewPanel.Add(crewImageStrip);
            AddGeneratedImageCard(crewImageStrip, "SCADASim/Generated/crew_driller");
            AddGeneratedImageCard(crewImageStrip, "SCADASim/Generated/crew_mud");
            AddGeneratedImageCard(crewImageStrip, "SCADASim/Generated/crew_mwd");

            VisualElement crewActions = new VisualElement();
            crewActions.AddToClassList("crew-action-grid");
            crewPanel.Add(crewActions);
            crewActions.Add(CreateFlatButton("РАСТВОРЩИК: ЗАМЕР", () => RunCrewAction(CrewActionType.MudCheck)));
            crewActions.Add(CreateFlatButton("МЕХАНИК: ОСМОТР", () => RunCrewAction(CrewActionType.RigInspection)));
            crewActions.Add(CreateFlatButton("ПЛАН РЕЙСА", () => RunCrewAction(CrewActionType.BitRunPlanning)));
            crewActions.Add(CreateFlatButton("ИНКЛИНОМЕТРИЯ", () => RunCrewAction(CrewActionType.DirectionalSurvey)));
            crewActions.Add(CreateFlatButton("ПРОМЫВКА СТВОЛА", () => RunCrewAction(CrewActionType.HoleCleaning)));
            crewActions.Add(CreateFlatButton("ИНСТРУКТАЖ", () => RunCrewAction(CrewActionType.ShiftBriefing)));
            crewActions.Add(CreateFlatButton("ПРИТОК: ГЛУШЕНИЕ", () => RunCrewAction(CrewActionType.KickControl)));
            crewActions.Add(CreateFlatButton("ПОГЛОЩЕНИЕ: LCM", () => RunCrewAction(CrewActionType.LossControl)));
            crewActions.Add(CreateFlatButton("ПРИХВАТ: РАСХАЖ.", () => RunCrewAction(CrewActionType.FreeStuckPipe)));
            crewActions.Add(CreateFlatButton("STICK-SLIP: СНИЗИТЬ", () => RunCrewAction(CrewActionType.StickSlipMitigation)));
            crewActions.Add(CreateFlatButton("ПРОРАБОТКА", () => RunCrewAction(CrewActionType.BackreamAndReam)));

            VisualElement right = new VisualElement();
            right.AddToClassList("telemetry-right");
            page.Add(right);

            VisualElement telemetryPanel = CreateScadaPanel("ТЕЛЕМЕТРИЯ");
            telemetryPanel.AddToClassList("telemetry-panel");
            right.Add(telemetryPanel);
            VisualElement metricGrid = new VisualElement();
            metricGrid.AddToClassList("metric-grid-light");
            telemetryPanel.Add(metricGrid);
            rpmValue = AddMetric(metricGrid, "ОБОРОТЫ");
            torqueValue = AddMetric(metricGrid, "МОМЕНТ (кНм)");
            ropValue = AddMetric(metricGrid, "ROP");
            wobValue = AddMetric(metricGrid, "НАГРУЗКА");
            sppValue = AddMetric(metricGrid, "ДАВЛ. НАСОСА");
            flowInValue = AddMetric(metricGrid, "РАСХОД ВХОД");
            flowOutValue = AddMetric(metricGrid, "РАСХОД ВЫХОД");
            mudWeightValue = AddMetric(metricGrid, "ПЛОТН. Р-РА");
            bottomTempValue = AddMetric(metricGrid, "ТЕМП. ЗАБОЙ");
            ecdValue = AddMetric(metricGrid, "ЭКВ. ПЛОТН.");
            vibrationValue = AddMetric(metricGrid, "ВИБРАЦИЯ");
            incValue = AddMetric(metricGrid, "ЗЕНИТ (°)");
            azimuthValue = AddMetric(metricGrid, "АЗИМУТ (°)");
            bitWearValue = AddMetric(metricGrid, "ИЗНОС ДОЛОТА");

            VisualElement controlPanel = CreateScadaPanel("АКТИВНОЕ УПРАВЛЕНИЕ");
            controlPanel.AddToClassList("control-panel");
            right.Add(controlPanel);
            rpmControlValue = AddKnobControlRow(controlPanel, "ОБОРОТЫ", "об/мин", 35f, 190f, 125f, 5f, value => drillingModel?.RequestSetRpm(value), out rpmKnob);
            wobControlValue = AddKnobControlRow(controlPanel, "НАГРУЗКА", "т", 2f, 26f, 11.5f, 0.5f, value => drillingModel?.RequestSetWeightOnBit(value), out wobKnob);
            flowControlValue = AddKnobControlRow(controlPanel, "РАСХОД", "л/мин", 900f, 3600f, 2280f, 50f, value => drillingModel?.RequestSetFlowRate(value / 60f), out flowKnob);
            mudControlValue = AddKnobControlRow(controlPanel, "ПЛОТНОСТЬ", "SG", 0.95f, 1.55f, 1.1f, 0.01f, value => drillingModel?.RequestSetMudWeight(value), out mudKnob);
            chokeControlValue = AddKnobControlRow(controlPanel, "ШТУЦЕР", "%", 0f, 100f, 100f, 2f, value => drillingModel?.RequestSetChokeOpening(value / 100f), out chokeKnob);
            timeSpeedValue = AddTimeSpeedRow(controlPanel);
            musicStatusValue = AddMusicRow(controlPanel);

            VisualElement aiPanel = CreateScadaPanel("ИИ-ПОМОЩНИК / РЕКОМЕНДАЦИИ");
            aiPanel.AddToClassList("ai-panel");
            right.Add(aiPanel);

            VisualElement assistantRow = new VisualElement();
            assistantRow.AddToClassList("ai-assistant-row");
            aiPanel.Add(assistantRow);

            AddGeneratedImageCard(assistantRow, "SCADASim/Generated/incident_card");

            aiAssistantImage = new Image();
            aiAssistantImage.AddToClassList("ai-assistant-portrait");
            aiAssistantImage.image = aiAssistantPortrait != null ? aiAssistantPortrait.PortraitTexture : null;
            assistantRow.Add(aiAssistantImage);

            aiAssistantCaption = new Label("ИИ-МОДУЛЬ: онлайн. Оценивает давление, вибрацию, расход и действия бригады.");
            aiAssistantCaption.AddToClassList("ai-assistant-caption");
            assistantRow.Add(aiAssistantCaption);

            aiRecommendationValue = AddStatusBox(aiPanel, "МОНИТОРИНГ: отклонений нет. Действие: продолжать текущий режим.");
            supervisorTaskValue = AddStatusBox(aiPanel, "ЗАДАЧА: ожидание распоряжения бурового мастера.");
            taskEconomyValue = AddStatusBox(aiPanel, "БЮДЖЕТ СМЕНЫ: 0 CR // УСПЕХ 0 // ПРОВАЛ 0.");
            incidentConsequenceValue = AddStatusBox(aiPanel, "ПОСЛЕДСТВИЯ: НПВ 0 мин // ПРИТОК 0% // ПОГЛОЩЕНИЕ 0%.");
        }

        private void BuildProfilePage(VisualElement page)
        {
            VisualElement profileStatus = new VisualElement();
            profileStatus.AddToClassList("profile-status");
            page.Add(profileStatus);

            Label title = new Label("3D-ПРОФИЛЬ СКВАЖИНЫ");
            title.AddToClassList("profile-status-title");
            profileStatus.Add(title);

            Label details = new Label("Камера: ПКМ - свободный обзор, WASD/Q/E - движение, Shift - ускорение, колесо - приближение, Alt+ЛКМ - орбита, СКМ - панорама, F - фокус. Траектория и аварийные зоны строятся по выбранному профилю.");
            details.AddToClassList("profile-status-body");
            profileStatus.Add(details);
        }

        private void BuildTutorialOverlay(VisualElement parent)
        {
            tutorialOverlay = new VisualElement();
            tutorialOverlay.AddToClassList("tutorial-overlay");
            parent.Add(tutorialOverlay);

            VisualElement panel = new VisualElement();
            panel.AddToClassList("tutorial-panel");
            tutorialOverlay.Add(panel);

            tutorialTitle = new Label("ОБУЧЕНИЕ");
            tutorialTitle.AddToClassList("tutorial-title");
            panel.Add(tutorialTitle);

            tutorialBody = new Label();
            tutorialBody.AddToClassList("tutorial-body");
            panel.Add(tutorialBody);

            VisualElement buttons = new VisualElement();
            buttons.AddToClassList("tutorial-buttons");
            panel.Add(buttons);

            Button skip = CreateFlatButton("ПРОПУСТИТЬ", HideTutorialOverlay);
            tutorialNextButton = CreateFlatButton("ДАЛЕЕ", NextTutorialStep);
            buttons.Add(skip);
            buttons.Add(tutorialNextButton);

            tutorialOverlay.style.display = DisplayStyle.None;
        }

        private ChoiceBinding AddConfigChoice(VisualElement parent, string title, List<string> choices, int index)
        {
            Label label = new Label(title);
            label.AddToClassList("config-label");
            parent.Add(label);

            ChoiceBinding binding = new ChoiceBinding(choices, Mathf.Clamp(index, 0, choices.Count - 1));
            Button button = new Button(binding.Next);
            button.AddToClassList("config-choice");
            binding.Bind(button);
            parent.Add(button);
            return binding;
        }

        private void ApplyConfigAndStart()
        {
            SimulationRuntimeConfig config = BuildRuntimeConfigFromUI();
            Debug.Log($"SCADA simulation start: {config.EnvironmentType}, {config.ProfileType}, MD {config.StartMeasuredDepth:0}-{config.MaxMeasuredDepth:0} m.");

            environmentManager?.LoadEnvironment(config.EnvironmentType);
            rigModelManager?.LoadRigModel(config.EnvironmentType);
            crewManager?.ApplyPreset(config.CrewPreset);

            wellbore?.ConfigureRuntime(
                config.ProfileType,
                config.MaxMeasuredDepth,
                35f,
                ResolveAzimuth(config.GeologyRegion),
                WellboreVisualRadiusMeters,
                32,
                10,
                eventChannel);
            wellbore?.Regenerate();
            wellbore?.SetVisibleMeasuredDepth(config.StartMeasuredDepth);
            UpdateWellboreVisualization(config.EnvironmentType);

            GeologyModel geology = GeologyModel.CreateRuntimeDemo(config.GeologyRegion);
            drillingModel?.ApplyRuntimeConfig(config, geology);
            bool runTutorial = tutorialChoice == null || tutorialChoice.Index == 0;
            drillingModel?.SetSimulating(!runTutorial);

            configLine.text = $"[{ToShortText(config.EnvironmentType)}] | [{ToShortText(config.ProfileType)}] | {ToShortText(config.GeologyRegion)} | РЕЖИМ: {ToShortText(config.Difficulty)}";
            rigModelStatus.text = rigModelManager != null ? rigModelManager.CurrentModelStatus : "3D МОДЕЛЬ: НЕ ЗАГРУЖЕНА";
            environmentStatus.text = config.EnvironmentType == EnvironmentType.Offshore
                ? "ПОГОДА: -4.1 C // ВЕТЕР: 21.8 М/С // ВОЛНА: 2.6 м"
                : "ПОГОДА: -15.3 C // ВЕТЕР: 13.2 М/С";
            radioLogValue.text = $"[00:00:01] Регион: {ToShortText(config.GeologyRegion)}. Бригада: {ToCrewSize(config.CrewPreset)} чел. Система запущена.";
            hasActiveSupervisorTask = false;
            supervisorTaskValue.text = "ЗАДАЧА: ожидание распоряжения бурового мастера.";

            ShowMainScreen();
            ShowTelemetryPage();
            if (runTutorial)
            {
                ShowTutorialOverlay();
            }
        }

        private void UpdateWellboreVisualization(EnvironmentType environmentType)
        {
            if (wellbore == null)
            {
                return;
            }

            wellbore.transform.localScale = Vector3.one * WellboreVisualScale;
            wellbore.transform.position = rigModelManager != null
                ? rigModelManager.GetWellheadWorldPosition(environmentType)
                : new Vector3(0f, environmentType == EnvironmentType.Offshore ? 2.55f : 0.05f, 0f);
        }

        private SimulationRuntimeConfig BuildRuntimeConfigFromUI()
        {
            float startDepth;
            switch (depthChoice.Index)
            {
                case 5:
                    startDepth = 4500f;
                    break;
                case 4:
                    startDepth = 3500f;
                    break;
                case 3:
                    startDepth = 2500f;
                    break;
                case 2:
                    startDepth = 1500f;
                    break;
                case 1:
                    startDepth = 500f;
                    break;
                default:
                    startDepth = 0f;
                    break;
            }
            WellboreProfileType profile = ResolveProfile(profileChoice.Index);

            return new SimulationRuntimeConfig
            {
                EnvironmentType = locationChoice.Index == 1 ? EnvironmentType.Offshore : EnvironmentType.Onshore,
                ProfileType = profile,
                GeologyRegion = ResolveRegion(regionChoice.Index),
                Difficulty = ResolveDifficulty(difficultyChoice.Index),
                PhysicsPreset = ResolvePhysics(physicsChoice.Index),
                CrewPreset = ResolveCrew(crewChoice.Index),
                StartMeasuredDepth = startDepth,
                MaxMeasuredDepth = CalculateMaxMeasuredDepth(profile, startDepth)
            };
        }

        private void Refresh(DrillingState state)
        {
            CrewInfluence crew = crewManager != null ? crewManager.GetInfluence() : default;

            rpmValue.text = $"{state.Rpm:0}";
            torqueValue.text = $"{state.SurfaceTorqueKnM:0.0}";
            ropValue.text = $"{state.RopMPerHour:0.0}";
            wobValue.text = $"{state.WeightOnBitTonnes:0.0}";
            sppValue.text = $"{state.StandpipePressureBar:0.0}";
            flowInValue.text = $"{state.FlowRateLps * 60f:0}";
            flowOutValue.text = $"{state.FlowOutLps * 60f:0}";
            mudWeightValue.text = $"{state.MudWeightSG:0.00}";
            depthValue.text = $"ГЛУБИНА: MD {state.MeasuredDepth:0} м // TVD {state.TrueVerticalDepth:0} м";
            bottomTempValue.text = $"{12f + state.TrueVerticalDepth * 0.031f:0} C";
            ecdValue.text = $"{state.EquivalentCirculatingDensitySG:0.00}";
            vibrationValue.text = $"{state.Vibration.LowFrequencyEnergy * 3.5f:0.0}";
            incValue.text = $"{state.InclinationDegrees:0.0}";
            azimuthValue.text = $"{state.AzimuthDegrees:0}";
            bitWearValue.text = $"{state.BitWear01 * 100f:0}%";

            pressurePoreValue.text = $"{state.PorePressureMPa:0.0}";
            pressureBottomValue.text = $"{state.BottomHolePressureMPa:0.0}";
            pressureGradientValue.text = $"{state.FracturePressureMPa:0.0}";
            formationStatusValue.text = $"ФОРМАЦИЯ: {ToRussianLithology(state.Lithology)} // TVD: {state.TrueVerticalDepth:0} м // ECD: {state.EquivalentCirculatingDensitySG:0.00} SG // ВЫНОС ШЛАМА: {state.CuttingsTransportEfficiency01 * 100f:0}% // ГАЗ: {state.GasUnitsPercent:0.0}%";

            rpmControlValue.text = $"ОБ/МИН: {state.Rpm:0}";
            wobControlValue.text = $"НАГР.: {state.WeightOnBitTonnes:0.0} т";
            flowControlValue.text = $"РАСХОД: {state.FlowRateLps * 60f:0} л/мин";
            mudControlValue.text = $"ПЛОТН.: {state.MudWeightSG:0.00} SG";
            chokeControlValue.text = $"ШТУЦЕР: {state.ChokeOpening01 * 100f:0}%";
            SyncKnobValue(rpmKnob, state.Rpm);
            SyncKnobValue(wobKnob, state.WeightOnBitTonnes);
            SyncKnobValue(flowKnob, state.FlowRateLps * 60f);
            SyncKnobValue(mudKnob, state.MudWeightSG);
            SyncKnobValue(chokeKnob, state.ChokeOpening01 * 100f);
            if (timeSpeedValue != null && drillingModel != null)
            {
                timeSpeedValue.text = $"СКОРОСТЬ: x{drillingModel.SimulationSpeedMultiplier:0.#}";
            }
            if (musicStatusValue != null && musicPlayer != null)
            {
                string playState = musicPlayer.IsPlaying ? "играет" : "пауза";
                musicStatusValue.text = $"МУЗЫКА: {playState} // {musicPlayer.CurrentTrackName}";
            }
            if (crewStatusValue != null)
            {
                crewStatusValue.text =
                    $"БРИГАДА: опыт {crew.ExperienceLevel * 100f:0}% // усталость {crew.Fatigue * 100f:0}% // мораль {crew.Morale * 100f:0}% // реакция {crew.ReactionDelaySeconds:0.0} с // дисциплина {crew.ProcedureDiscipline01 * 100f:0}%";
            }

            if (incidentConsequenceValue != null)
            {
                incidentConsequenceValue.text =
                    $"ПОСЛЕДСТВИЯ: НПВ {state.NonProductiveTimeMinutes:0} мин // ПРИТОК {state.KickRisk01 * 100f:0}% // ПОГЛОЩЕНИЕ {state.LostCirculationRisk01 * 100f:0}%";
            }

            if (taskEconomyValue != null && drillingModel != null)
            {
                taskEconomyValue.text =
                    $"БЮДЖЕТ СМЕНЫ: {drillingModel.CompanyCredits:+0;-0;0} CR // УСПЕХ {drillingModel.SuccessfulSupervisorTasks} // ПРОВАЛ {drillingModel.FailedSupervisorTasks}.";
            }

            if (aiAssistantImage != null && aiAssistantPortrait != null)
            {
                aiAssistantImage.image = aiAssistantPortrait.PortraitTexture;
                aiAssistantImage.style.opacity = 0.84f + Mathf.Sin(Time.unscaledTime * 2.4f) * 0.08f;
                if (aiAssistantCaption != null)
                {
                    aiAssistantCaption.text = aiAssistantPortrait.HasAssistantModel
                        ? "ИИ-МОДУЛЬ: онлайн. Анализ трендов в реальном времени."
                        : "ИИ-МОДУЛЬ: онлайн. Работает резервная визуализация.";
                }
            }

            if (hasActiveSupervisorTask && supervisorTaskValue != null)
            {
                supervisorTaskValue.text = CompactStatus(FormatSupervisorTask(activeSupervisorTask), 360);
            }

            rotationGraph?.Push(state.SurfaceTorqueKnM, state.Vibration.LowFrequencyEnergy * 100f);
            hydraulicGraph?.Push(state.StandpipePressureBar, (state.FlowOutLps - state.FlowRateLps) * 60f);
            bottomGraph?.Push(state.RopMPerHour, Mathf.Max(state.BitWear01, state.StuckPipeRisk01) * 100f);
            pressureGraph?.Push(state.BottomHolePressureMPa, state.PorePressureMPa);

            if (state.Vibration.LowFrequencyEnergy > 0.58f && state.InclinationDegrees > 70f && crew.Fatigue > 0.5f)
            {
                aiRecommendationValue.text = "РЕКОМЕНДАЦИЯ ИИ: риск stick-slip. Действие: плавно снизить RPM на 10-15% и стабилизировать WOB.";
            }
        }

        private void HandleSimulationEvent(SimulationEvent simulationEvent)
        {
            if (simulationEvent.Type == SimulationEventType.EdgeAIAlert && simulationEvent.Payload is AIRecommendation recommendation)
            {
                aiRecommendationValue.text = FormatAIRecommendation(recommendation);
                return;
            }

            if (simulationEvent.Type == SimulationEventType.OperationalIncident && simulationEvent.Payload is OperationalIncident incident)
            {
                aiRecommendationValue.text = CompactStatus($"{incident.Title.ToUpperInvariant()}: {incident.Message}", 170);
                if (incidentConsequenceValue != null)
                {
                    incidentConsequenceValue.text = CompactStatus($"ПОСЛЕДСТВИЯ: {incident.Consequence}", 180);
                }

                radioLogValue.text = $"[{FormatClock()}] Инцидент на MD {incident.MeasuredDepth:0} м: {incident.Title}.";
                return;
            }

            if (simulationEvent.Type == SimulationEventType.SupervisorTask && simulationEvent.Payload is SupervisorTask task)
            {
                activeSupervisorTask = task;
                hasActiveSupervisorTask = true;
                if (supervisorTaskValue != null)
                {
                    supervisorTaskValue.text = CompactStatus(FormatSupervisorTask(task), 320);
                }

                radioLogValue.text = $"[{FormatClock()}] {task.Title}: {task.Objective}";
                return;
            }

            if (simulationEvent.Type == SimulationEventType.SupervisorTaskResult && simulationEvent.Payload is SupervisorTaskResult taskResult)
            {
                hasActiveSupervisorTask = false;
                string status = taskResult.Succeeded ? "ЗАДАЧА ВЫПОЛНЕНА" : "ЗАДАЧА ПРОВАЛЕНА";
                aiRecommendationValue.text = CompactStatus($"{status}: {taskResult.Summary}", 190);
                if (supervisorTaskValue != null)
                {
                    supervisorTaskValue.text = $"{status}: {taskResult.Task.Title}. {(taskResult.CreditsDelta >= 0 ? "+" : string.Empty)}{taskResult.CreditsDelta} CR.";
                }

                if (taskEconomyValue != null)
                {
                    taskEconomyValue.text = $"БЮДЖЕТ СМЕНЫ: {taskResult.TotalCredits:+0;-0;0} CR // ПОСЛЕДНИЙ РЕЗУЛЬТАТ: {(taskResult.CreditsDelta >= 0 ? "+" : string.Empty)}{taskResult.CreditsDelta} CR.";
                }

                radioLogValue.text = $"[{FormatClock()}] {status}: {taskResult.Summary}";
                return;
            }

            if (simulationEvent.Type == SimulationEventType.CrewIncident)
            {
                radioLogValue.text = $"[{FormatClock()}] {simulationEvent.Message}";
            }

            if (simulationEvent.Type == SimulationEventType.CrewActionCompleted && simulationEvent.Payload is CrewActionReport report)
            {
                radioLogValue.text = $"[{FormatClock()}] {report.Role}: {report.Title}. {report.Effect}";
            }
        }

        private void ShowStartScreen()
        {
            if (introScreen != null)
            {
                introScreen.style.display = DisplayStyle.None;
            }

            startScreen.style.display = DisplayStyle.Flex;
            mainScreen.style.display = DisplayStyle.None;
        }

        private void ShowIntroOrStartScreen()
        {
            startScreen.style.display = DisplayStyle.None;
            mainScreen.style.display = DisplayStyle.None;

            if (introCompleted || introScreen == null)
            {
                ShowStartScreen();
                return;
            }

            introScreen.style.display = DisplayStyle.Flex;
        }

        private void FinishIntro()
        {
            if (introCompleted)
            {
                return;
            }

            introCompleted = true;
            if (introVideoPlayer != null)
            {
                introVideoPlayer.Stop();
            }

            ShowStartScreen();
        }

        private void ShowMainScreen()
        {
            if (introScreen != null)
            {
                introScreen.style.display = DisplayStyle.None;
            }

            startScreen.style.display = DisplayStyle.None;
            mainScreen.style.display = DisplayStyle.Flex;
        }

        private void ShowTelemetryPage()
        {
            telemetryPage.style.display = DisplayStyle.Flex;
            profilePage.style.display = DisplayStyle.None;
            telemetryTab.AddToClassList("tab-active");
            profileTab.RemoveFromClassList("tab-active");
        }

        private void ShowTutorialOverlay()
        {
            tutorialStepIndex = 0;
            if (tutorialOverlay != null)
            {
                tutorialOverlay.style.display = DisplayStyle.Flex;
            }

            drillingModel?.SetSimulating(false);
            RefreshTutorialText();
        }

        private void HideTutorialOverlay()
        {
            if (tutorialOverlay != null)
            {
                tutorialOverlay.style.display = DisplayStyle.None;
            }

            drillingModel?.SetSimulating(true);
            PushRadioMessage("Обучение завершено. Смена приступила к бурению.");
        }

        private void NextTutorialStep()
        {
            tutorialStepIndex++;
            if (tutorialStepIndex >= TutorialStepCount)
            {
                HideTutorialOverlay();
                return;
            }

            RefreshTutorialText();
        }

        private void RefreshTutorialText()
        {
            if (tutorialTitle == null || tutorialBody == null || tutorialNextButton == null)
            {
                return;
            }

            switch (tutorialStepIndex)
            {
                case 0:
                    tutorialTitle.text = "1/8  ЦЕЛЬ СМЕНЫ";
                    tutorialBody.text = "Держите скважину в безопасном окне давлений: забойное давление должно быть выше пластового, но ниже давления ГРП. Следите за расходом вход/выход: рост выхода и газа говорит о притоке, падение выхода - о поглощении.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 1:
                    tutorialTitle.text = "2/8  УПРАВЛЕНИЕ";
                    tutorialBody.text = "Обороты влияют на момент и вибрацию, нагрузка - на ROP и износ долота, расход - на вынос шлама и давление насоса. Плотность раствора и штуцер меняют ECD, поэтому ими нельзя работать резко.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 2:
                    tutorialTitle.text = "3/8  БРИГАДА";
                    tutorialBody.text = "Бригада теперь работает ролями. Растворщик, механик, инженер ННБ и буровой мастер выполняют процедуры с разным качеством. Усталость повышает задержку и шанс ошибки, инструктаж и хорошая организация снижают риск.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 3:
                    tutorialTitle.text = "4/8  ЗАДАЧИ ОТ МАСТЕРА";
                    tutorialBody.text = "Задачи появляются последовательно и имеют реальный таймер. Если время истекло, система выдает новую вводную и фиксирует последствия: НПВ, рост риска притока, поглощения или прихвата.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 4:
                    tutorialTitle.text = "5/8  АВАРИЙНЫЕ ПРОЦЕДУРЫ";
                    tutorialBody.text = "Кнопки бригады - это не магия, а полевые процедуры. При притоке используйте глушение, при поглощении - LCM и снижение расхода, при прихвате - расхаживание, при Stick-Slip - снижение оборотов и нагрузки.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 5:
                    tutorialTitle.text = "6/8  СКОРОСТЬ ВРЕМЕНИ";
                    tutorialBody.text = "Кнопки x1/x2/x5/x10/x20 ускоряют физику бурения и таймер задач одновременно. Если включили x10, дедлайн мастера тоже пройдет в десять раз быстрее.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 6:
                    tutorialTitle.text = "7/8  3D И КАМЕРА";
                    tutorialBody.text = "В 3D-профиле видно текущую позицию долота, станции MD и цветовую зону риска. При наклонном и горизонтальном бурении следите за шламовой постелью, drag, Stick-Slip, притоком и поглощением.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                default:
                    tutorialTitle.text = "8/8  МУЗЫКА И ОКНО";
                    tutorialBody.text = "Окно теперь можно менять по размеру: интерфейс масштабируется от базовой сетки 1600x900 и сохраняет взаимное расположение блоков. Музыка из папки Aphex Twin доступна в панели управления.";
                    tutorialNextButton.text = "НАЧАТЬ СМЕНУ";
                    break;
            }
        }

        private void ShowProfilePage()
        {
            telemetryPage.style.display = DisplayStyle.None;
            profilePage.style.display = DisplayStyle.Flex;
            profileTab.AddToClassList("tab-active");
            telemetryTab.RemoveFromClassList("tab-active");
            if (cameraOrbit != null)
            {
                cameraOrbit.SetTarget(ResolveWellboreViewTarget());
                cameraOrbit.SetView(140f, 30f, 58f);
            }
        }

        private void ReturnToMainMenu()
        {
            drillingModel?.SetSimulating(false);
            if (tutorialOverlay != null)
            {
                tutorialOverlay.style.display = DisplayStyle.None;
            }

            hasActiveSupervisorTask = false;
            if (supervisorTaskValue != null)
            {
                supervisorTaskValue.text = "ЗАДАЧА: ожидание распоряжения бурового мастера.";
            }

            if (wellbore != null)
            {
                wellbore.SetVisibleMeasuredDepth(0f);
            }

            ShowStartScreen();
        }

        private Vector3 ResolveWellboreViewTarget()
        {
            if (wellbore == null || wellbore.Samples.Count < 2)
            {
                return new Vector3(0f, 5f, 0f);
            }

            Vector3 start = wellbore.transform.TransformPoint(wellbore.Samples[0].Position);
            Vector3 end = wellbore.transform.TransformPoint(wellbore.Samples[wellbore.Samples.Count - 1].Position);
            return (start + end) * 0.5f + Vector3.up * 4f;
        }

        private void AdjustRpm(float delta)
        {
            if (drillingModel != null)
            {
                drillingModel.RequestSetRpm(drillingModel.CurrentState.Rpm + delta);
            }
        }

        private void AdjustWob(float delta)
        {
            if (drillingModel != null)
            {
                drillingModel.RequestSetWeightOnBit(drillingModel.CurrentState.WeightOnBitTonnes + delta);
            }
        }

        private void AdjustFlowLitersPerMinute(float deltaLitersPerMinute)
        {
            if (drillingModel != null)
            {
                drillingModel.RequestSetFlowRate(drillingModel.CurrentState.FlowRateLps + deltaLitersPerMinute / 60f);
            }
        }

        private void AdjustMudWeight(float delta)
        {
            if (drillingModel != null)
            {
                drillingModel.RequestSetMudWeight(drillingModel.CurrentState.MudWeightSG + delta);
            }
        }

        private void AdjustChoke(float deltaOpening01)
        {
            if (drillingModel != null)
            {
                drillingModel.RequestSetChokeOpening(drillingModel.CurrentState.ChokeOpening01 + deltaOpening01);
                PushRadioMessage(deltaOpening01 < 0f
                    ? "Штуцер прикрыт на 10%. Давление на забое будет расти."
                    : "Штуцер открыт на 10%. Давление на забое будет снижаться.");
            }
        }

        private void SetSimulationSpeed(float multiplier)
        {
            drillingModel?.SetSimulationSpeedMultiplier(multiplier);
            PushRadioMessage($"Скорость симуляции установлена x{multiplier:0.#}. Таймер заданий идет в реальном времени.");
        }

        private void ToggleMusic()
        {
            musicPlayer?.TogglePlayback();
        }

        private void NextMusicTrack()
        {
            musicPlayer?.NextTrack();
            PushRadioMessage("Аудиосопровождение переключено на следующий трек.");
        }

        private void RunCrewAction(CrewActionType actionType)
        {
            if (crewManager == null || drillingModel == null)
            {
                return;
            }

            CrewActionReport report = crewManager.ExecuteCrewAction(actionType, drillingModel.CurrentState);
            drillingModel.ApplyCrewMitigation(actionType, report.Quality01);
            PushRadioMessage($"{report.Role}: {report.Message} {report.Effect} Качество: {report.Quality01 * 100f:0}%.");

            if (actionType == CrewActionType.HoleCleaning && report.Quality01 > 0.35f)
            {
                float flowBoostLpm = Mathf.Lerp(80f, 220f, report.Quality01);
                AdjustFlowLitersPerMinute(flowBoostLpm);
            }

            if (actionType == CrewActionType.ShiftBriefing && report.Quality01 > 0.45f)
            {
                crewManager.ApplyRest(0.04f);
            }

            if (actionType == CrewActionType.KickControl && report.Quality01 > 0.4f)
            {
                AdjustMudWeight(0.02f);
                AdjustChoke(-0.08f);
            }

            if (actionType == CrewActionType.LossControl && report.Quality01 > 0.4f)
            {
                AdjustFlowLitersPerMinute(-120f);
                AdjustChoke(0.05f);
            }

            if (actionType == CrewActionType.StickSlipMitigation && report.Quality01 > 0.4f)
            {
                AdjustRpm(-15f);
                AdjustWob(-1f);
            }
        }

        private void PushRadioMessage(string message)
        {
            if (radioLogValue != null)
            {
                radioLogValue.text = $"[{FormatClock()}] {message}";
            }
        }

        private static Button CreateTabButton(string text, System.Action action)
        {
            Button button = new Button(action);
            button.text = text;
            button.AddToClassList("tab-button");
            return button;
        }

        private static Button CreateFlatButton(string text, System.Action action)
        {
            Button button = new Button(action);
            button.text = text;
            button.AddToClassList("flat-button");
            return button;
        }

        private static VisualElement CreateScadaPanel(string title)
        {
            VisualElement panel = new VisualElement();
            panel.AddToClassList("scada-panel");
            Label label = new Label(title);
            label.AddToClassList("scada-panel-title");
            panel.Add(label);
            return panel;
        }

        private static Label AddStatusBox(VisualElement parent, string text)
        {
            Label label = new Label(text);
            label.AddToClassList("status-box");
            parent.Add(label);
            return label;
        }

        private static void AddGeneratedImageCard(VisualElement parent, string resourcePath)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                return;
            }

            Image image = new Image
            {
                image = texture,
                scaleMode = ScaleMode.ScaleToFit
            };
            image.AddToClassList("generated-widget-image");
            parent.Add(image);
        }

        private static Label CreatePressureNumber(VisualElement parent, string label)
        {
            VisualElement item = new VisualElement();
            item.AddToClassList("pressure-number");
            parent.Add(item);

            Label title = new Label(label);
            title.AddToClassList("pressure-label");
            item.Add(title);

            Label value = new Label("0.1");
            value.AddToClassList("pressure-value");
            item.Add(value);
            return value;
        }

        private static Label AddMetric(VisualElement parent, string title)
        {
            VisualElement item = new VisualElement();
            item.AddToClassList("metric-item-light");
            parent.Add(item);

            Label label = new Label(title);
            label.AddToClassList("metric-title-light");
            item.Add(label);

            Label value = new Label("0");
            value.AddToClassList("metric-value-light");
            item.Add(value);
            return value;
        }

        private static Label AddKnobControlRow(
            VisualElement parent,
            string label,
            string unit,
            float min,
            float max,
            float initialValue,
            float step,
            System.Action<float> valueChanged,
            out KnobControlElement knob)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("knob-row");
            parent.Add(row);

            Label value = new Label($"{label}: {initialValue:0.#} {unit}");
            value.AddToClassList("knob-label");
            row.Add(value);

            knob = new KnobControlElement(min, max, initialValue, step);
            knob.AddToClassList("knob-control");
            knob.ValueChanged += newValue =>
            {
                value.text = $"{label}: {FormatControlValue(newValue, unit)}";
                valueChanged?.Invoke(newValue);
            };
            row.Add(knob);

            Label hint = new Label("тяни / колесо");
            hint.AddToClassList("knob-hint");
            row.Add(hint);
            return value;
        }

        private static void SyncKnobValue(KnobControlElement knob, float value)
        {
            if (knob != null && !knob.IsDragging)
            {
                knob.SetValueWithoutNotify(value);
            }
        }

        private static string FormatControlValue(float value, string unit)
        {
            if (unit == "SG")
            {
                return $"{value:0.00} {unit}";
            }

            if (unit == "т")
            {
                return $"{value:0.0} {unit}";
            }

            return $"{value:0} {unit}";
        }

        private static Label AddControlRow(VisualElement parent, string label, string leftText, string rightText, System.Action leftAction, System.Action rightAction)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("control-row");
            parent.Add(row);

            Label value = new Label($"{label}: 0");
            value.AddToClassList("control-label");
            row.Add(value);

            Button left = CreateFlatButton(leftText, leftAction);
            Button right = CreateFlatButton(rightText, rightAction);
            left.AddToClassList("control-button");
            right.AddToClassList("control-button");
            row.Add(left);
            row.Add(right);

            return value;
        }

        private Label AddTimeSpeedRow(VisualElement parent)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("control-row");
            row.AddToClassList("time-speed-row");
            parent.Add(row);

            Label value = new Label("СКОРОСТЬ: x1");
            value.AddToClassList("control-label");
            row.Add(value);

            row.Add(CreateTimeButton("x1", () => SetSimulationSpeed(1f)));
            row.Add(CreateTimeButton("x2", () => SetSimulationSpeed(2f)));
            row.Add(CreateTimeButton("x5", () => SetSimulationSpeed(5f)));
            row.Add(CreateTimeButton("x10", () => SetSimulationSpeed(10f)));
            row.Add(CreateTimeButton("x20", () => SetSimulationSpeed(20f)));
            return value;
        }

        private Label AddMusicRow(VisualElement parent)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("control-row");
            row.AddToClassList("music-row");
            parent.Add(row);

            Label value = new Label("МУЗЫКА: ожидание");
            value.AddToClassList("music-status-label");
            row.Add(value);

            Button toggle = CreateTimeButton("PLAY", ToggleMusic);
            Button next = CreateTimeButton("NEXT", NextMusicTrack);
            row.Add(toggle);
            row.Add(next);
            return value;
        }

        private static Button CreateTimeButton(string text, System.Action action)
        {
            Button button = CreateFlatButton(text, action);
            button.AddToClassList("time-speed-button");
            return button;
        }

        private static TrendGraphElement AddGraphPanel(
            VisualElement parent,
            string title,
            Color primary,
            Color secondary,
            GraphAxisScale primaryScale,
            GraphAxisScale secondaryScale)
        {
            VisualElement panel = CreateScadaPanel(title);
            panel.AddToClassList("graph-panel");
            parent.Add(panel);

            VisualElement graphContainer = new VisualElement();
            graphContainer.AddToClassList("graph-container");
            graphContainer.style.flexGrow = 1;
            panel.Add(graphContainer);
 
            TrendGraphElement graph = new TrendGraphElement(primary, secondary, primaryScale, secondaryScale);
            graph.AddToClassList("trend-graph-light");
            graph.style.position = Position.Absolute;
            graph.style.width = new Length(100, LengthUnit.Percent);
            graph.style.height = new Length(100, LengthUnit.Percent);
            graphContainer.Add(graph);

            return graph;
        }

        private static WellboreProfileType ResolveProfile(int index)
        {
            switch (index)
            {
                case 1:
                    return WellboreProfileType.JShape;
                case 2:
                    return WellboreProfileType.SShape;
                case 3:
                    return WellboreProfileType.Horizontal;
                default:
                    return WellboreProfileType.Vertical;
            }
        }

        private static GeologyRegion ResolveRegion(int index)
        {
            switch (index)
            {
                case 1:
                    return GeologyRegion.VolgaUral;
                case 2:
                    return GeologyRegion.ArcticShelf;
                case 3:
                    return GeologyRegion.Caspian;
                default:
                    return GeologyRegion.WestSiberia;
            }
        }

        private static DifficultyLevel ResolveDifficulty(int index)
        {
            switch (index)
            {
                case 1:
                    return DifficultyLevel.Easy;
                case 2:
                    return DifficultyLevel.Realistic;
                case 3:
                    return DifficultyLevel.Expert;
                default:
                    return DifficultyLevel.Training;
            }
        }

        private static PhysicsPreset ResolvePhysics(int index)
        {
            switch (index)
            {
                case 1:
                    return PhysicsPreset.Field;
                case 2:
                    return PhysicsPreset.Harsh;
                default:
                    return PhysicsPreset.Stable;
            }
        }

        private static CrewPreset ResolveCrew(int index)
        {
            switch (index)
            {
                case 1:
                    return CrewPreset.StandardFour;
                case 2:
                    return CrewPreset.ReducedThree;
                case 3:
                    return CrewPreset.TraineeShift;
                default:
                    return CrewPreset.FullSeven;
            }
        }

        private static float CalculateMaxMeasuredDepth(WellboreProfileType profile, float startDepth)
        {
            float profileDepth;
            switch (profile)
            {
                case WellboreProfileType.Horizontal:
                    profileDepth = 6200f;
                    break;
                case WellboreProfileType.SShape:
                    profileDepth = 5600f;
                    break;
                case WellboreProfileType.JShape:
                    profileDepth = 5200f;
                    break;
                default:
                    profileDepth = 4500f;
                    break;
            }

            return Mathf.Max(profileDepth, startDepth + 1200f);
        }

        private static float ResolveAzimuth(GeologyRegion region)
        {
            switch (region)
            {
                case GeologyRegion.VolgaUral:
                    return 55f;
                case GeologyRegion.ArcticShelf:
                    return 35f;
                case GeologyRegion.Caspian:
                    return 82f;
                default:
                    return 70f;
            }
        }

        private static string ToShortText(EnvironmentType type)
        {
            return type == EnvironmentType.Offshore ? "МОРЕ" : "СУША";
        }

        private static string ToShortText(WellboreProfileType type)
        {
            switch (type)
            {
                case WellboreProfileType.JShape:
                    return "J-ПРОФИЛЬ";
                case WellboreProfileType.SShape:
                    return "S-ПРОФИЛЬ";
                case WellboreProfileType.Horizontal:
                    return "ГОРИЗОНТАЛЬНАЯ";
                default:
                    return "ВЕРТИКАЛЬНАЯ";
            }
        }

        private static string ToShortText(GeologyRegion region)
        {
            switch (region)
            {
                case GeologyRegion.VolgaUral:
                    return "ВОЛГО-УРАЛ";
                case GeologyRegion.ArcticShelf:
                    return "АРКТИЧЕСКИЙ ШЕЛЬФ";
                case GeologyRegion.Caspian:
                    return "КАСПИЙ";
                default:
                    return "ЗАПАДНАЯ СИБИРЬ";
            }
        }

        private static string ToShortText(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    return "ЛЕГКИЙ";
                case DifficultyLevel.Realistic:
                    return "ПРОМЫСЛОВЫЙ";
                case DifficultyLevel.Expert:
                    return "ЭКСПЕРТНЫЙ";
                default:
                    return "ТРЕНИРОВКА";
            }
        }

        private static string ToRussianLithology(LithologyType lithology)
        {
            switch (lithology)
            {
                case LithologyType.Sandstone:
                    return "ПЕСЧАНИК";
                case LithologyType.Limestone:
                    return "ИЗВЕСТНЯК";
                case LithologyType.Dolomite:
                    return "ДОЛОМИТ";
                case LithologyType.Salt:
                    return "СОЛЬ";
                case LithologyType.Basement:
                    return "ФУНДАМЕНТ";
                default:
                    return "ГЛИНА";
            }
        }

        private static int ToCrewSize(CrewPreset preset)
        {
            switch (preset)
            {
                case CrewPreset.StandardFour:
                    return 4;
                case CrewPreset.ReducedThree:
                    return 3;
                case CrewPreset.TraineeShift:
                    return 5;
                default:
                    return 7;
            }
        }

        private static string FormatAIRecommendation(AIRecommendation recommendation)
        {
            string severity = recommendation.Severity == AlertSeverity.Critical ? "критический риск" : "предупреждение";
            string action = recommendation.RecommendedAction.Trim();
            if (!action.EndsWith("."))
            {
                action += ".";
            }

            return CompactStatus($"ИИ: {severity}. {recommendation.Title}. Действие: {action}", 155);
        }

        private static string CompactStatus(string text, int maxCharacters)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxCharacters)
            {
                return text;
            }

            return text.Substring(0, Mathf.Max(0, maxCharacters - 1)) + ".";
        }

        private string FormatSupervisorTask(SupervisorTask task)
        {
            float remainingMinutes = drillingModel != null
                ? drillingModel.ActiveSupervisorTaskRemainingMinutes
                : task.DeadlineMinutes;
            string timerText = remainingMinutes > 0f
                ? FormatCountdown(remainingMinutes)
                : "СРОК ИСТЕК";

            return $"ЗАДАЧА: {task.Title}. ТАЙМЕР: {timerText}. НАГРАДА +{task.RewardCredits} CR / ШТРАФ -{task.FailurePenaltyCredits} CR. {task.Objective} ДАТЧИКИ: {task.SensorFocus} РУЧКИ: {task.ControlHints} КРИТЕРИЙ: {task.SuccessCriteria}";
        }

        private static string FormatCountdown(float minutes)
        {
            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, minutes) * 60f);
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;
            return $"{m:00}:{s:00}";
        }

        private static string FormatClock()
        {
            int seconds = Mathf.FloorToInt(Time.time);
            int h = seconds / 3600;
            int m = seconds / 60 % 60;
            int s = seconds % 60;
            return $"{h:00}:{m:00}:{s:00}";
        }

        private sealed class KnobControlElement : VisualElement
        {
            private const float DragPixelsForFullRange = 340f;
            private readonly float min;
            private readonly float max;
            private readonly float step;
            private float value;
            private Vector2 dragStartPosition;
            private float dragStartValue;

            public event System.Action<float> ValueChanged;
            public bool IsDragging { get; private set; }

            public KnobControlElement(float min, float max, float initialValue, float step)
            {
                this.min = min;
                this.max = max;
                this.step = Mathf.Max(0.0001f, step);
                focusable = true;
                SetValueWithoutNotify(initialValue);

                RegisterCallback<PointerDownEvent>(OnPointerDown);
                RegisterCallback<PointerMoveEvent>(OnPointerMove);
                RegisterCallback<PointerUpEvent>(OnPointerUp);
                RegisterCallback<PointerCancelEvent>(OnPointerCancel);
                RegisterCallback<WheelEvent>(OnWheel);
                RegisterCallback<KeyDownEvent>(OnKeyDown);
                generateVisualContent += Draw;
            }

            public void SetValueWithoutNotify(float newValue)
            {
                value = Quantize(newValue);
                MarkDirtyRepaint();
            }

            private void SetValue(float newValue)
            {
                float quantized = Quantize(newValue);
                if (Mathf.Approximately(value, quantized))
                {
                    return;
                }

                value = quantized;
                MarkDirtyRepaint();
                ValueChanged?.Invoke(value);
            }

            private float Quantize(float rawValue)
            {
                float clamped = Mathf.Clamp(rawValue, min, max);
                return Mathf.Round(clamped / step) * step;
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.button != 0)
                {
                    return;
                }

                IsDragging = true;
                dragStartPosition = (Vector2)evt.position;
                dragStartValue = value;
                Focus();
                MarkDirtyRepaint();
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (!IsDragging)
                {
                    return;
                }

                Vector2 delta = (Vector2)evt.position - dragStartPosition;
                float range = max - min;
                float normalizedDelta = (delta.x * 0.6f - delta.y) / DragPixelsForFullRange;
                SetValue(dragStartValue + normalizedDelta * range);
                evt.StopPropagation();
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                EndDrag(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnPointerCancel(PointerCancelEvent evt)
            {
                EndDrag(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnWheel(WheelEvent evt)
            {
                float wheelSteps = Mathf.Clamp(evt.delta.y, -3f, 3f);
                SetValue(value - wheelSteps * step);
                evt.StopPropagation();
            }

            private void OnKeyDown(KeyDownEvent evt)
            {
                float delta = 0f;
                if (evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.DownArrow)
                {
                    delta = -step;
                }
                else if (evt.keyCode == KeyCode.RightArrow || evt.keyCode == KeyCode.UpArrow)
                {
                    delta = step;
                }
                else if (evt.keyCode == KeyCode.PageDown)
                {
                    delta = -step * 5f;
                }
                else if (evt.keyCode == KeyCode.PageUp)
                {
                    delta = step * 5f;
                }
                else if (evt.keyCode == KeyCode.Home)
                {
                    SetValue(min);
                    evt.StopPropagation();
                    return;
                }
                else if (evt.keyCode == KeyCode.End)
                {
                    SetValue(max);
                    evt.StopPropagation();
                    return;
                }

                if (!Mathf.Approximately(delta, 0f))
                {
                    SetValue(value + delta);
                    evt.StopPropagation();
                }
            }

            private void EndDrag(int pointerId)
            {
                IsDragging = false;
                MarkDirtyRepaint();
            }

            private void Draw(MeshGenerationContext context)
            {
                Rect rect = contentRect;
                Vector2 center = rect.center;
                float radius = Mathf.Max(5f, Mathf.Min(rect.width, rect.height) * 0.44f);
                float innerRadius = radius * 0.76f;
                float normalized = Mathf.InverseLerp(min, max, value);
                float angle = Mathf.Lerp(-135f, 135f, normalized) * Mathf.Deg2Rad;

                Painter2D painter = context.painter2D;
                DrawPolygon(painter, center, radius, 44, IsDragging ? new Color(0.1f, 0.1f, 0.09f) : new Color(0.06f, 0.06f, 0.06f));
                DrawPolygon(painter, center, innerRadius, 44, new Color(0.86f, 0.84f, 0.78f));

                painter.lineWidth = IsDragging ? 4f : 3f;
                painter.strokeColor = new Color(0.86f, 0.02f, 0.08f);
                painter.BeginPath();
                bool first = true;
                int arcSegments = 28;
                for (int i = 0; i <= arcSegments; i++)
                {
                    float t = Mathf.Lerp(0f, normalized, i / (float)arcSegments);
                    float arcAngle = Mathf.Lerp(-135f, 135f, t) * Mathf.Deg2Rad;
                    Vector2 point = center + new Vector2(Mathf.Sin(arcAngle), -Mathf.Cos(arcAngle)) * (radius + 2f);
                    if (first)
                    {
                        painter.MoveTo(point);
                        first = false;
                    }
                    else
                    {
                        painter.LineTo(point);
                    }
                }
                painter.Stroke();

                painter.lineWidth = IsDragging ? 4f : 3f;
                painter.strokeColor = Color.black;
                painter.BeginPath();
                painter.MoveTo(center);
                painter.LineTo(center + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * (innerRadius * 0.82f));
                painter.Stroke();
            }

            private static void DrawPolygon(Painter2D painter, Vector2 center, float radius, int segments, Color color)
            {
                painter.fillColor = color;
                painter.BeginPath();
                for (int i = 0; i < segments; i++)
                {
                    float angle = i / (float)segments * Mathf.PI * 2f;
                    Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    if (i == 0)
                    {
                        painter.MoveTo(point);
                    }
                    else
                    {
                        painter.LineTo(point);
                    }
                }

                painter.ClosePath();
                painter.Fill();
            }
        }

        private struct GraphAxisScale
        {
            public readonly string Title;
            public readonly float DefaultMin;
            public readonly float DefaultMax;

            public GraphAxisScale(string title, float defaultMin, float defaultMax)
            {
                Title = title;
                DefaultMin = Mathf.Min(defaultMin, defaultMax);
                DefaultMax = Mathf.Max(defaultMin, defaultMax);
            }

            public void GetRange(List<float> values, out float min, out float max)
            {
                min = DefaultMin;
                max = DefaultMax;

                if (values != null)
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        min = Mathf.Min(min, values[i]);
                        max = Mathf.Max(max, values[i]);
                    }
                }

                if (Mathf.Approximately(min, max))
                {
                    float padding = Mathf.Max(Mathf.Abs(min) * 0.05f, 1f);
                    min -= padding;
                    max += padding;
                    return;
                }

                float range = max - min;
                float edgePadding = Mathf.Max(range * 0.04f, 0.001f);
                if (min < DefaultMin)
                {
                    min -= edgePadding;
                }

                if (max > DefaultMax)
                {
                    max += edgePadding;
                }
            }
        }

        private sealed class TrendGraphElement : VisualElement
        {
            private const int Capacity = 180;
            private const float PlotLeftPadding = 48f;
            private const float PlotRightPadding = 48f;
            private const float PlotTopPadding = 16f;
            private const float PlotBottomPadding = 24f;

            private readonly List<float> primary = new List<float>(Capacity);
            private readonly List<float> secondary = new List<float>(Capacity);
            private readonly Color primaryColor;
            private readonly Color secondaryColor;
            private readonly GraphAxisScale primaryScale;
            private readonly GraphAxisScale secondaryScale;
            private readonly Label primaryMaxLabel;
            private readonly Label primaryMinLabel;
            private readonly Label secondaryMaxLabel;
            private readonly Label secondaryMinLabel;

            public TrendGraphElement(
                Color primaryColor,
                Color secondaryColor,
                GraphAxisScale primaryScale,
                GraphAxisScale secondaryScale)
            {
                this.primaryColor = primaryColor;
                this.secondaryColor = secondaryColor;
                this.primaryScale = primaryScale;
                this.secondaryScale = secondaryScale;
                usageHints = UsageHints.DynamicTransform;

                primaryMaxLabel = CreateGraphLabel("graph-label-primary-max", primaryColor);
                primaryMinLabel = CreateGraphLabel("graph-label-primary-min", primaryColor);
                secondaryMaxLabel = CreateGraphLabel("graph-label-secondary-max", secondaryColor);
                secondaryMinLabel = CreateGraphLabel("graph-label-secondary-min", secondaryColor);
                CreateAxisTitle(primaryScale.Title, "graph-axis-title-left", primaryColor);
                CreateAxisTitle(secondaryScale.Title, "graph-axis-title-right", secondaryColor);
                CreateAxisTitle("ВРЕМЯ, 27 С", "graph-axis-title-bottom", GraphBlack);

                UpdateLabels();
                generateVisualContent += Draw;
            }

            public void Push(float primaryValue, float secondaryValue)
            {
                PushValue(primary, primaryValue);
                PushValue(secondary, secondaryValue);
                UpdateLabels();
                MarkDirtyRepaint();
            }

            private Label CreateGraphLabel(string className, Color color)
            {
                Label label = new Label();
                label.AddToClassList("graph-label");
                label.AddToClassList(className);
                label.style.color = color;
                label.pickingMode = PickingMode.Ignore;
                Add(label);
                label.BringToFront();
                return label;
            }

            private void CreateAxisTitle(string text, string className, Color color)
            {
                Label label = new Label(text);
                label.AddToClassList("graph-axis-title");
                label.AddToClassList(className);
                label.style.color = color;
                label.pickingMode = PickingMode.Ignore;
                Add(label);
                label.BringToFront();
            }

            private void UpdateLabels()
            {
                UpdateLabelRange(primary, primaryScale, primaryMaxLabel, primaryMinLabel);
                UpdateLabelRange(secondary, secondaryScale, secondaryMaxLabel, secondaryMinLabel);
            }

            private static void UpdateLabelRange(List<float> values, GraphAxisScale scale, Label maxLabel, Label minLabel)
            {
                scale.GetRange(values, out float min, out float max);
                maxLabel.text = FormatValue(max);
                minLabel.text = FormatValue(min);
            }

            private static string FormatValue(float val)
            {
                if (Mathf.Abs(val) >= 100f) return val.ToString("0");
                if (Mathf.Abs(val) >= 10f) return val.ToString("0.#");
                return val.ToString("0.##");
            }

            private static void PushValue(List<float> values, float value)
            {
                values.Add(value);
                if (values.Count > Capacity)
                {
                    values.RemoveAt(0);
                }
            }

            private void Draw(MeshGenerationContext context)
            {
                Rect rect = contentRect;
                if (rect.width < 8f || rect.height < 8f)
                {
                    return;
                }

                Rect plotRect = GetPlotRect(rect);
                Painter2D painter = context.painter2D;
                DrawGrid(painter, plotRect);
                DrawSeries(painter, plotRect, secondary, secondaryColor, 1.5f, secondaryScale);
                DrawSeries(painter, plotRect, primary, primaryColor, 2.5f, primaryScale);
            }

            private static Rect GetPlotRect(Rect rect)
            {
                float leftPadding = Mathf.Min(PlotLeftPadding, rect.width * 0.2f);
                float rightPadding = Mathf.Min(PlotRightPadding, rect.width * 0.2f);
                float topPadding = Mathf.Min(PlotTopPadding, rect.height * 0.22f);
                float bottomPadding = Mathf.Min(PlotBottomPadding, rect.height * 0.28f);
                return new Rect(
                    rect.xMin + leftPadding,
                    rect.yMin + topPadding,
                    Mathf.Max(4f, rect.width - leftPadding - rightPadding),
                    Mathf.Max(4f, rect.height - topPadding - bottomPadding));
            }

            private static void DrawGrid(Painter2D painter, Rect rect)
            {
                painter.lineWidth = 1f;
                painter.strokeColor = new Color(0.18f, 0.18f, 0.16f, 0.2f);

                for (int i = 1; i < 5; i++)
                {
                    float y = Mathf.Lerp(rect.yMin, rect.yMax, i / 5f);
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(rect.xMin, y));
                    painter.LineTo(new Vector2(rect.xMax, y));
                    painter.Stroke();
                }

                for (int i = 1; i < 9; i++)
                {
                    float x = Mathf.Lerp(rect.xMin, rect.xMax, i / 9f);
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(x, rect.yMin));
                    painter.LineTo(new Vector2(x, rect.yMax));
                    painter.Stroke();
                }

                painter.lineWidth = 1.5f;
                painter.strokeColor = new Color(0.05f, 0.05f, 0.05f, 0.82f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
                painter.LineTo(new Vector2(rect.xMin, rect.yMax));
                painter.LineTo(new Vector2(rect.xMax, rect.yMax));
                painter.LineTo(new Vector2(rect.xMax, rect.yMin));
                painter.Stroke();
            }

            private static void DrawSeries(
                Painter2D painter,
                Rect rect,
                List<float> values,
                Color color,
                float width,
                GraphAxisScale scale)
            {
                if (values.Count < 2)
                {
                    return;
                }

                scale.GetRange(values, out float min, out float max);
                painter.lineWidth = width;
                painter.strokeColor = color;
                painter.BeginPath();

                for (int i = 0; i < values.Count; i++)
                {
                    float x = Mathf.Lerp(rect.xMin, rect.xMax, i / (float)Mathf.Max(1, values.Count - 1));
                    float normalized = Mathf.Clamp01(Mathf.InverseLerp(min, max, values[i]));
                    float y = Mathf.Lerp(rect.yMax, rect.yMin, normalized);
                    Vector2 point = new Vector2(x, y);

                    if (i == 0)
                    {
                        painter.MoveTo(point);
                    }
                    else
                    {
                        painter.LineTo(point);
                    }
                }

                painter.Stroke();
            }
        }

        private sealed class ChoiceBinding
        {
            private readonly List<string> choices;
            private Button button;

            public int Index { get; private set; }

            public ChoiceBinding(List<string> choices, int index)
            {
                this.choices = choices;
                Index = Mathf.Clamp(index, 0, choices.Count - 1);
            }

            public void Bind(Button target)
            {
                button = target;
                Refresh();
            }

            public void Next()
            {
                if (choices.Count == 0)
                {
                    return;
                }

                Index = (Index + 1) % choices.Count;
                Refresh();
            }

            private void Refresh()
            {
                if (button != null && choices.Count > 0)
                {
                    button.text = $"{choices[Index]}  ▾";
                }
            }
        }
    }
}
