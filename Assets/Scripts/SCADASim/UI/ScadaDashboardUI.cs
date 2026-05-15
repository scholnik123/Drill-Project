using System.Collections.Generic;
using SCADASim.Audio;
using SCADASim.Bootstrap;
using SCADASim.Core;
using SCADASim.Crew;
using SCADASim.Environment;
using SCADASim.Physics;
using SCADASim.Trajectory;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace SCADASim.UI
{
    public sealed class ScadaDashboardUI : MonoBehaviour
    {
        private const float WellboreVisualScale = 0.035f;
        private const float WellboreVisualRadiusMeters = 8.0f;
        private const int TutorialStepCount = 12;
        private const float DesignWidth = 1600f;
        private const float DesignHeight = 900f;

        private static readonly Color GraphRed = new Color(0.88f, 0.02f, 0.08f);
        private static readonly Color GraphGreen = new Color(0.13f, 0.55f, 0.32f);
        private static readonly Color GraphBlue = new Color(0.26f, 0.52f, 1f);
        private static readonly Color GraphBlack = new Color(0.05f, 0.05f, 0.05f);
        private static readonly GraphAxisScale TorqueScale = new GraphAxisScale("Момент, кНм", 0f, 65f, 8f, 42f);
        private static readonly GraphAxisScale VibrationScale = new GraphAxisScale("Вибрация, %", 0f, 100f, 0f, 55f);
        private static readonly GraphAxisScale StandpipePressureScale = new GraphAxisScale("Давление насоса, бар", 0f, 360f, 70f, 260f);
        private static readonly GraphAxisScale FlowBalanceScale = new GraphAxisScale("Баланс расхода, л/мин", -600f, 600f, -150f, 150f);
        private static readonly GraphAxisScale RopScale = new GraphAxisScale("Скорость проходки, м/ч", 0f, 45f, 6f, 32f);
        private static readonly GraphAxisScale RiskPercentScale = new GraphAxisScale("Риск или износ, %", 0f, 100f, 0f, 45f);
        private static readonly GraphAxisScale BottomHolePressureScale = new GraphAxisScale("Забойное давление, МПа", 0f, 80f);
        private static readonly GraphAxisScale PorePressureScale = new GraphAxisScale("Пластовое давление, МПа", 0f, 80f);

        private readonly List<string> locationChoices = new List<string>
        {
            "Суша",
            "Море"
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
            "0 м",
            "500 м",
            "1500 м",
            "2500 м",
            "3500 м",
            "4500 м"
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
            "ЛЕГКИЙ",
            "ПРОМЫСЛОВЫЙ",
            "ЭКСПЕРТНЫЙ"
        };

        private readonly List<string> physicsChoices = new List<string>
        {
            "СТАБИЛЬНАЯ",
            "ПОЛЕВАЯ",
            "ОСЛОЖНЕНИЯ"
        };

        private readonly List<string> crewChoices = new List<string>
        {
            "7 человек",
            "4 человека",
            "3 человека",
            "стажерская"
        };

        private readonly List<string> tutorialChoices = new List<string>
        {
            "включить",
            "пропустить"
        };

        private readonly List<string> musicVolumeChoices = new List<string>
        {
            "35%",
            "15%",
            "60%",
            "выкл."
        };

        private readonly List<string> startSpeedChoices = new List<string>
        {
            "x1",
            "x2",
            "x5"
        };

        private readonly List<string> shiftDurationChoices = new List<string>
        {
            "8 мин",
            "6 мин",
            "12 мин"
        };

        private readonly List<string> supervisorPaceChoices = new List<string>
        {
            "нормально",
            "чаще",
            "реже"
        };

        private readonly List<string> resolutionChoices = new List<string>
        {
            "Авто",
            "1920 x 1080",
            "1600 x 900",
            "1366 x 768",
            "1280 x 720",
            "2560 x 1440",
            "Полный экран"
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
        private VisualElement scaledViewport;
        private VisualElement startScreen;
        private VisualElement mainScreen;
        private VisualElement introScreen;
        private VisualElement telemetryPage;
        private VisualElement profilePage;
        private VisualElement crewPage;
        private VisualElement tutorialOverlay;
        private Button telemetryTab;
        private Button profileTab;
        private Button crewTab;
        private Button tutorialNextButton;
        private Label tutorialTitle;
        private Label tutorialBody;
        private float nextRefreshTime;
        private int tutorialStepIndex;
        private bool introCompleted;

        private ChoiceBinding locationChoice;
        private ChoiceBinding profileChoice;
        private ChoiceBinding depthChoice;
        private ChoiceBinding regionChoice;
        private ChoiceBinding difficultyChoice;
        private ChoiceBinding physicsChoice;
        private ChoiceBinding crewChoice;
        private ChoiceBinding tutorialChoice;
        private ChoiceBinding musicVolumeChoice;
        private ChoiceBinding startSpeedChoice;
        private ChoiceBinding shiftDurationChoice;
        private ChoiceBinding supervisorPaceChoice;
        private ChoiceBinding resolutionChoice;

        private Label configLine;
        private Label rigModelStatus;
        private Label environmentStatus;
        private Label formationStatusValue;
        private Label pressurePoreValue;
        private Label pressureBottomValue;
        private Label pressureGradientValue;
        private Label radioLogValue;
        private Label crewStatusValue;
        private Label crewShiftValue;
        private Label crewRosterValue;
        private Label crewProcedureValue;
        private Label profileSummaryValue;
        private Label profilePressureWindowValue;
        private Label profileRiskValue;
        private Label profileLegendValue;
        private Image aiAssistantImage;
        private Label aiAssistantCaption;
        private Label aiRecommendationValue;
        private Label aiCrewAdvisorValue;
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

        public void DebugStartProfileSessionForCapture()
        {
            ApplyConfigAndStart();
            HideTutorialOverlay();
            ShowProfilePage();
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
            ApplyDisplayResolution(0);

            document = gameObject.GetComponent<UIDocument>();
            if (document == null)
            {
                document = gameObject.AddComponent<UIDocument>();
            }
            document.enabled = false;

            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;

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
            root.RegisterCallback<GeometryChangedEvent>(_ => UpdateViewportScale());

            StyleSheet styleSheet = Resources.Load<StyleSheet>("SCADASim/ScadaDashboard");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            scaledViewport = new VisualElement();
            scaledViewport.AddToClassList("scada-viewport");
            root.Add(scaledViewport);

            BuildStartScreen(scaledViewport);
            BuildMainScreen(scaledViewport);
            BuildIntroScreen(scaledViewport);
            UpdateViewportScale();

            ShowIntroOrStartScreen();
        }

        private void UpdateViewportScale()
        {
            if (document == null || scaledViewport == null)
            {
                return;
            }

            VisualElement root = document.rootVisualElement;
            float width = root.resolvedStyle.width;
            float height = root.resolvedStyle.height;
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            float scale = Mathf.Min(width / DesignWidth, height / DesignHeight);
            if (scale <= 0f)
            {
                return;
            }

            scaledViewport.style.width = Mathf.Max(DesignWidth, width / scale);
            scaledViewport.style.height = Mathf.Max(DesignHeight, height / scale);
            scaledViewport.style.left = 0f;
            scaledViewport.style.top = 0f;
            scaledViewport.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }

        private void ApplyDisplayResolution(int choiceIndex)
        {
            if (Application.isEditor)
            {
                return;
            }

            ResolveResolution(choiceIndex, out int width, out int height, out FullScreenMode mode);
            Screen.SetResolution(width, height, mode);
            StartCoroutine(RefreshViewportScaleAfterResolutionChange());
        }

        private IEnumerator RefreshViewportScaleAfterResolutionChange()
        {
            yield return null;
            UpdateViewportScale();
        }

        private static void ResolveResolution(int choiceIndex, out int width, out int height, out FullScreenMode mode)
        {
            Resolution current = Screen.currentResolution;
            width = Mathf.Max(1280, current.width);
            height = Mathf.Max(720, current.height);
            mode = FullScreenMode.Windowed;

            switch (choiceIndex)
            {
                case 1:
                    width = 1920;
                    height = 1080;
                    break;
                case 2:
                    width = 1600;
                    height = 900;
                    break;
                case 3:
                    width = 1366;
                    height = 768;
                    break;
                case 4:
                    width = 1280;
                    height = 720;
                    break;
                case 5:
                    width = 2560;
                    height = 1440;
                    break;
                case 6:
                    mode = FullScreenMode.FullScreenWindow;
                    break;
            }
        }

        private void BuildIntroScreen(VisualElement root)
        {
            introScreen = new VisualElement();
            introScreen.AddToClassList("intro-screen");
            root.Add(introScreen);

            VisualElement logoBlock = new VisualElement();
            logoBlock.AddToClassList("intro-logo-block");
            introScreen.Add(logoBlock);

            Label logo = new Label("ZVZ");
            logo.AddToClassList("intro-logo");
            logoBlock.Add(logo);

            VisualElement rule = new VisualElement();
            rule.AddToClassList("intro-logo-rule");
            logoBlock.Add(rule);

            Label caption = new Label("SCADA INTELLIGENT SIMULATION v4");
            caption.AddToClassList("intro-caption");
            logoBlock.Add(caption);

            Button skipButton = new Button(FinishIntro);
            skipButton.text = "ПРОПУСТИТЬ";
            skipButton.AddToClassList("intro-skip-button");
            introScreen.Add(skipButton);

            StartCoroutine(PlayLogoIntro());
        }

        private IEnumerator PlayLogoIntro()
        {
            yield return new WaitForSecondsRealtime(2.8f);
            FinishIntro();
        }

        private void BuildStartScreen(VisualElement root)
        {
            startScreen = new VisualElement();
            startScreen.AddToClassList("start-screen");
            root.Add(startScreen);

            VisualElement titleBar = new VisualElement();
            titleBar.AddToClassList("window-titlebar");
            startScreen.Add(titleBar);
            titleBar.Add(new Label("LUKOIL X ZVZ :: SCADA INTELLIGENT SIMULATION v4"));

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

            Label subtitle = new Label("SCADA INTELLIGENT SIMULATION v4");
            subtitle.AddToClassList("start-subtitle");
            brand.Add(subtitle);

            Label scenario = new Label("Минимальная панель запуска. Выберите параметры сценария, реализма, смены и интерфейса; все настройки применяются к физике бурения, задачам мастера и поведению бригады.");
            scenario.AddToClassList("scenario-box");
            brand.Add(scenario);

            VisualElement configPanel = new VisualElement();
            configPanel.AddToClassList("start-config-panel");
            body.Add(configPanel);

            Label configTitle = new Label("ПАРАМЕТРЫ СИМУЛЯЦИИ");
            configTitle.AddToClassList("start-config-title");
            configPanel.Add(configTitle);

            locationChoice = AddConfigChoice(configPanel, "Локация", locationChoices, 0);
            profileChoice = AddConfigChoice(configPanel, "Профиль", profileChoices, 0);
            depthChoice = AddConfigChoice(configPanel, "Стартовая глубина", depthChoices, 0);
            regionChoice = AddConfigChoice(configPanel, "Регион", regionChoices, 0);
            difficultyChoice = AddConfigChoice(configPanel, "Реализм", difficultyChoices, 0);
            physicsChoice = AddConfigChoice(configPanel, "Физика", physicsChoices, 0);
            crewChoice = AddConfigChoice(configPanel, "Бригада", crewChoices, 0);
            shiftDurationChoice = AddConfigChoice(configPanel, "Смена", shiftDurationChoices, 0);
            supervisorPaceChoice = AddConfigChoice(configPanel, "Задачи", supervisorPaceChoices, 0);
            startSpeedChoice = AddConfigChoice(configPanel, "Скорость", startSpeedChoices, 0);
            musicVolumeChoice = AddConfigChoice(configPanel, "Музыка", musicVolumeChoices, 0);
            resolutionChoice = AddConfigChoice(configPanel, "Разрешение", resolutionChoices, 0);
            tutorialChoice = AddConfigChoice(configPanel, "Обучение", tutorialChoices, 0);

            Button startButton = new Button(ApplyConfigAndStart);
            startButton.text = "НАЧАТЬ СМЕНУ";
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
            crewTab = CreateTabButton("БРИГАДА", ShowCrewPage);
            tabs.Add(telemetryTab);
            tabs.Add(profileTab);
            tabs.Add(crewTab);

            VisualElement profileControls = new VisualElement();
            profileControls.AddToClassList("profile-view-buttons");
            tabs.Add(profileControls);
            profileControls.Add(CreateFlatButton("СЛЕДИТЬ", () => SetProfileCameraView(140f, 28f, 0.52f)));
            profileControls.Add(CreateFlatButton("ВСЯ СКВАЖИНА", () => SetProfileCameraView(145f, 38f, 1.02f)));
            profileControls.Add(CreateFlatButton("3/4", () => SetProfileCameraView(140f, 28f, 0.74f)));
            profileControls.Add(CreateFlatButton("СБОКУ", () => SetProfileCameraView(90f, 18f, 0.86f)));
            profileControls.Add(CreateFlatButton("СВЕРХУ", () => SetProfileCameraView(180f, 72f, 1.0f)));
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

            crewPage = new VisualElement();
            crewPage.AddToClassList("crew-page");
            mainScreen.Add(crewPage);
            BuildCrewPage(crewPage);

            BuildTutorialOverlay(mainScreen);
        }

        private void BuildTelemetryPage(VisualElement page)
        {
            VisualElement left = new VisualElement();
            left.AddToClassList("telemetry-left");
            page.Add(left);

            rotationGraph = AddGraphPanel(left, "ВРАЩЕНИЕ И МОМЕНТ", GraphRed, GraphBlack, TorqueScale, VibrationScale);
            hydraulicGraph = AddGraphPanel(left, "ГИДРАВЛИКА И РАСХОД", GraphGreen, GraphBlack, StandpipePressureScale, FlowBalanceScale);
            bottomGraph = AddGraphPanel(left, "СКОРОСТЬ ПРОХОДКИ И ИЗНОС", GraphBlue, GraphRed, RopScale, RiskPercentScale);

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
            formationStatusValue = AddStatusBox(environmentPanel, "ФОРМАЦИЯ: глина // вертикальная глубина 0 м // эквив. плотность 1.10 SG // вынос шлама 86%");

            VisualElement pressurePanel = CreateScadaPanel("БАЛАНС ДАВЛЕНИЙ");
            pressurePanel.AddToClassList("pressure-panel");
            center.Add(pressurePanel);
            VisualElement pressureNumbers = new VisualElement();
            pressureNumbers.AddToClassList("pressure-numbers");
            pressurePanel.Add(pressureNumbers);
            pressurePoreValue = CreatePressureNumber(pressureNumbers, "ПЛАСТОВОЕ (МПа)");
            pressureBottomValue = CreatePressureNumber(pressureNumbers, "ЗАБОЙНОЕ (МПа)");
            pressureGradientValue = CreatePressureNumber(pressureNumbers, "ГИДРОРАЗРЫВ (МПа)");
            pressureGraph = new TrendGraphElement(GraphRed, GraphBlack, BottomHolePressureScale, PorePressureScale);
            pressureGraph.AddToClassList("pressure-graph");
            pressurePanel.Add(pressureGraph);
            depthValue = AddStatusBox(pressurePanel, "ГЛУБИНА: 0 МЕТРОВ");

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
            ropValue = AddMetric(metricGrid, "СКОР. ПРОХОДКИ");
            wobValue = AddMetric(metricGrid, "НАГРУЗКА");
            sppValue = AddMetric(metricGrid, "ДАВЛЕНИЕ НАСОСА");
            flowInValue = AddMetric(metricGrid, "РАСХОД ВХОД");
            flowOutValue = AddMetric(metricGrid, "РАСХОД ВЫХОД");
            mudWeightValue = AddMetric(metricGrid, "ПЛОТНОСТЬ РАСТВОРА");
            bottomTempValue = AddMetric(metricGrid, "ТЕМП. ЗАБОЙ");
            ecdValue = AddMetric(metricGrid, "ЭКВИВ. ПЛОТНОСТЬ");
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
            aiCrewAdvisorValue = AddStatusBox(aiPanel, "ИИ-БРИГАДИР: смена на связи. Жду изменения параметров и внешних условий.");
            aiCrewAdvisorValue.AddToClassList("ai-crew-advisor-box");
            supervisorTaskValue = AddStatusBox(aiPanel, "ЗАДАЧА: ожидание распоряжения бурового мастера.");
            supervisorTaskValue.AddToClassList("supervisor-task-box");
            taskEconomyValue = AddStatusBox(aiPanel, "БЮДЖЕТ СМЕНЫ: 0 кредитов // выполнено 0 // провалено 0.");
            incidentConsequenceValue = AddStatusBox(aiPanel, "ПОСЛЕДСТВИЯ: простои 0 мин // приток 0% // поглощение 0%.");
        }

        private void BuildProfilePage(VisualElement page)
        {
            VisualElement profileStatus = new VisualElement();
            profileStatus.AddToClassList("profile-status");
            page.Add(profileStatus);

            Label title = new Label("3D-ПРОФИЛЬ СКВАЖИНЫ");
            title.AddToClassList("profile-status-title");
            profileStatus.Add(title);

            Label details = new Label("Камера: ПКМ - свободный обзор, WASD/Q/E - движение, колесо - приближение, F - фокус на стволе. Траектория, станции глубины и зона риска обновляются по текущему режиму бурения.");
            details.AddToClassList("profile-status-body");
            profileStatus.Add(details);

            VisualElement dataPanel = new VisualElement();
            dataPanel.AddToClassList("profile-data-panel");
            page.Add(dataPanel);

            Label dataTitle = new Label("ПАРАМЕТРЫ ПРОФИЛЯ");
            dataTitle.AddToClassList("profile-status-title");
            dataPanel.Add(dataTitle);

            profileSummaryValue = AddStatusBox(dataPanel, "ТРАЕКТОРИЯ: ожидание данных.");
            profilePressureWindowValue = AddStatusBox(dataPanel, "ОКНО ДАВЛЕНИЙ: ожидание данных.");
            profileRiskValue = AddStatusBox(dataPanel, "РИСКИ: ожидание данных.");
            profileLegendValue = AddStatusBox(dataPanel, "ЛЕГЕНДА: красная сфера - текущее долото; полупрозрачная зона - суммарный операционный риск; метки - станции глубины по стволу.");
        }

        private void BuildCrewPage(VisualElement page)
        {
            VisualElement left = new VisualElement();
            left.AddToClassList("crew-column-main");
            page.Add(left);

            VisualElement shiftPanel = CreateScadaPanel("СМЕНА И СОСТАВ БРИГАДЫ");
            shiftPanel.AddToClassList("crew-shift-panel");
            left.Add(shiftPanel);
            crewShiftValue = AddStatusBox(shiftPanel, "СМЕНА: ожидание запуска.");
            crewRosterValue = AddStatusBox(shiftPanel, "РОЛИ: состав будет назначен после запуска сценария.");
            crewProcedureValue = AddStatusBox(shiftPanel, "ГОТОВНОСТЬ: опыт, усталость, дисциплина и координация будут рассчитаны моделью.");

            VisualElement imageStrip = new VisualElement();
            imageStrip.AddToClassList("crew-image-strip");
            shiftPanel.Add(imageStrip);
            AddGeneratedImageCard(imageStrip, "SCADASim/Generated/crew_driller");
            AddGeneratedImageCard(imageStrip, "SCADASim/Generated/crew_mud");
            AddGeneratedImageCard(imageStrip, "SCADASim/Generated/crew_mwd");

            VisualElement radioPanel = CreateScadaPanel("РАЦИЯ И ЖУРНАЛ ДЕЙСТВИЙ");
            radioPanel.AddToClassList("crew-radio-panel");
            left.Add(radioPanel);
            radioLogValue = AddStatusBox(radioPanel, "[00:00:00] Ожидание запуска смены.");
            crewStatusValue = AddStatusBox(radioPanel, "БРИГАДА: данные появятся после инициализации системы.");

            VisualElement actionsPanel = CreateScadaPanel("ПОЛЕВЫЕ ПРОЦЕДУРЫ");
            actionsPanel.AddToClassList("crew-actions-panel");
            left.Add(actionsPanel);
            Label actionIntro = new Label("Процедуры выполняются с учетом опыта, усталости и дисциплины текущей смены.");
            actionIntro.AddToClassList("crew-note");
            actionsPanel.Add(actionIntro);

            VisualElement crewActions = new VisualElement();
            crewActions.AddToClassList("crew-action-grid");
            actionsPanel.Add(crewActions);
            crewActions.Add(CreateFlatButton("РАСТВОР: ЗАМЕР", () => RunCrewAction(CrewActionType.MudCheck)));
            crewActions.Add(CreateFlatButton("ОСМОТР ВЫШКИ", () => RunCrewAction(CrewActionType.RigInspection)));
            crewActions.Add(CreateFlatButton("ПЛАН РЕЙСА", () => RunCrewAction(CrewActionType.BitRunPlanning)));
            crewActions.Add(CreateFlatButton("ИНКЛИНОМЕТРИЯ", () => RunCrewAction(CrewActionType.DirectionalSurvey)));
            crewActions.Add(CreateFlatButton("ПРОМЫВКА СТВОЛА", () => RunCrewAction(CrewActionType.HoleCleaning)));
            crewActions.Add(CreateFlatButton("ИНСТРУКТАЖ", () => RunCrewAction(CrewActionType.ShiftBriefing)));
            crewActions.Add(CreateFlatButton("ПРИТОК: ГЛУШЕНИЕ", () => RunCrewAction(CrewActionType.KickControl)));
            crewActions.Add(CreateFlatButton("ПОГЛОЩЕНИЕ: МАТЕРИАЛ", () => RunCrewAction(CrewActionType.LossControl)));
            crewActions.Add(CreateFlatButton("ПРИХВАТ: РАСХАЖИВАНИЕ", () => RunCrewAction(CrewActionType.FreeStuckPipe)));
            crewActions.Add(CreateFlatButton("АВТОКОЛЕБАНИЯ: СНИЗИТЬ", () => RunCrewAction(CrewActionType.StickSlipMitigation)));
            crewActions.Add(CreateFlatButton("ПРОРАБОТКА СТВОЛА", () => RunCrewAction(CrewActionType.BackreamAndReam)));

            VisualElement right = new VisualElement();
            right.AddToClassList("crew-column-side");
            page.Add(right);

            VisualElement doctrinePanel = CreateScadaPanel("ОПЕРАЦИОННАЯ ОЦЕНКА");
            doctrinePanel.AddToClassList("crew-doctrine-panel");
            right.Add(doctrinePanel);
            AddStatusBox(doctrinePanel, "РЕАЛИЗМ: команды не исполняются мгновенно. Задержка, ошибка уставки и качество процедур зависят от усталости, опыта и координации смены.");
            AddStatusBox(doctrinePanel, "ПЕРЕСМЕНКА: при окончании смены меняются работники, их профиль опыта и текущая усталость. Инструктаж снижает риск плохой передачи вахты.");
            AddStatusBox(doctrinePanel, "РАБОТА С РИСКАМИ: при притоке контролируйте выход, газ и окно давлений; при поглощении снижайте динамическую нагрузку; при прихвате не дергайте колонну резкими командами.");
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
            VisualElement row = new VisualElement();
            row.AddToClassList("config-row");
            parent.Add(row);

            Label label = new Label(title);
            label.AddToClassList("config-label");
            row.Add(label);

            ChoiceBinding binding = new ChoiceBinding(choices, Mathf.Clamp(index, 0, choices.Count - 1));
            Button button = new Button(binding.Next);
            button.AddToClassList("config-choice");
            binding.Bind(button);
            row.Add(button);
            return binding;
        }

        private void ApplyConfigAndStart()
        {
            SimulationRuntimeConfig config = BuildRuntimeConfigFromUI();
            Debug.Log($"SCADA simulation start: {config.EnvironmentType}, {config.ProfileType}, measured depth {config.StartMeasuredDepth:0}-{config.MaxMeasuredDepth:0} m.");

            ApplyDisplayResolution(resolutionChoice != null ? resolutionChoice.Index : 0);

            environmentManager?.LoadEnvironment(config.EnvironmentType);
            rigModelManager?.LoadRigModel(config.EnvironmentType);
            crewManager?.ApplyPreset(config.CrewPreset);
            crewManager?.SetShiftDurationMinutes(ResolveShiftDurationMinutes());

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
            drillingModel?.SetSimulationSpeedMultiplier(ResolveStartSpeedMultiplier());
            drillingModel?.SetSupervisorCadenceMultiplier(ResolveSupervisorCadenceMultiplier());
            musicPlayer?.SetVolume(ResolveMusicVolume());
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
            if (aiCrewAdvisorValue != null)
            {
                aiCrewAdvisorValue.text = "ИИ-БРИГАДИР: смена на связи. Жду изменения параметров и внешних условий.";
            }

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
            depthValue.text = $"ГЛУБИНА: по стволу {state.MeasuredDepth:0} м // по вертикали {state.TrueVerticalDepth:0} м";
            bottomTempValue.text = $"{12f + state.TrueVerticalDepth * 0.031f:0} C";
            ecdValue.text = $"{state.EquivalentCirculatingDensitySG:0.00}";
            vibrationValue.text = $"{state.Vibration.LowFrequencyEnergy * 3.5f:0.0}";
            incValue.text = $"{state.InclinationDegrees:0.0}";
            azimuthValue.text = $"{state.AzimuthDegrees:0}";
            bitWearValue.text = $"{state.BitWear01 * 100f:0}%";

            pressurePoreValue.text = $"{state.PorePressureMPa:0.0}";
            pressureBottomValue.text = $"{state.BottomHolePressureMPa:0.0}";
            pressureGradientValue.text = $"{state.FracturePressureMPa:0.0}";
            formationStatusValue.text = $"ФОРМАЦИЯ: {ToRussianLithology(state.Lithology)} // вертикаль {state.TrueVerticalDepth:0} м // эквив. плотность {state.EquivalentCirculatingDensitySG:0.00} SG // вынос шлама {state.CuttingsTransportEfficiency01 * 100f:0}% // газ {state.GasUnitsPercent:0.0}%";

            rpmControlValue.text = $"ОБОРОТЫ: {state.Rpm:0} об/мин";
            wobControlValue.text = $"НАГРУЗКА: {state.WeightOnBitTonnes:0.0} т";
            flowControlValue.text = $"РАСХОД: {state.FlowRateLps * 60f:0} л/мин";
            mudControlValue.text = $"ПЛОТНОСТЬ: {state.MudWeightSG:0.00} SG";
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
                string shiftLine = crewManager != null
                    ? $"{crewManager.ShiftStatusLine}. {crewManager.ActiveCrewLine}."
                    : "смена не назначена.";
                crewStatusValue.text = CompactStatus(
                    $"БРИГАДА: {shiftLine} Усталость {crew.Fatigue * 100f:0}%, мораль {crew.Morale * 100f:0}%, реакция {crew.ReactionDelaySeconds:0.0} с, дисциплина {crew.ProcedureDiscipline01 * 100f:0}%.",
                    230);
            }

            if (crewShiftValue != null)
            {
                crewShiftValue.text = crewManager != null
                    ? $"СМЕНА: {crewManager.ShiftStatusLine}."
                    : "СМЕНА: не назначена.";
            }

            if (crewRosterValue != null)
            {
                crewRosterValue.text = crewManager != null
                    ? $"РОЛИ: {crewManager.ActiveCrewLine}."
                    : "РОЛИ: ожидание состава.";
            }

            if (crewProcedureValue != null)
            {
                crewProcedureValue.text =
                    $"ГОТОВНОСТЬ: опыт {crew.ExperienceLevel * 100f:0}% // усталость {crew.Fatigue * 100f:0}% // дисциплина {crew.ProcedureDiscipline01 * 100f:0}% // координация {crew.ShiftCoordination01 * 100f:0}% // реакция {crew.ReactionDelaySeconds:0.0} с.";
            }

            if (profileSummaryValue != null)
            {
                profileSummaryValue.text =
                    $"ТРАЕКТОРИЯ: по стволу {state.MeasuredDepth:0} м // вертикаль {state.TrueVerticalDepth:0} м // зенит {state.InclinationDegrees:0.0}° // азимут {state.AzimuthDegrees:0}° // искривление {state.DoglegSeverityDegPer30m:0.0}°/30 м.";
            }

            if (profilePressureWindowValue != null)
            {
                float lowMargin = state.BottomHolePressureMPa - state.PorePressureMPa;
                float highMargin = state.FracturePressureMPa - state.BottomHolePressureMPa;
                profilePressureWindowValue.text =
                    $"ОКНО ДАВЛЕНИЙ: запас к пластовому {lowMargin:0.0} МПа // запас до гидроразрыва {highMargin:0.0} МПа // эквив. плотность {state.EquivalentCirculatingDensitySG:0.00} SG.";
            }

            if (profileRiskValue != null)
            {
                profileRiskValue.text =
                    $"РИСКИ: прихват {state.StuckPipeRisk01 * 100f:0}% // осыпь {state.BoreholeInstabilityRisk01 * 100f:0}% // приток {state.KickRisk01 * 100f:0}% // поглощение {state.LostCirculationRisk01 * 100f:0}% // очистка {state.CuttingsTransportEfficiency01 * 100f:0}%.";
            }

            if (incidentConsequenceValue != null)
            {
                incidentConsequenceValue.text =
                    $"ПОСЛЕДСТВИЯ: простои {state.NonProductiveTimeMinutes:0} мин // приток {state.KickRisk01 * 100f:0}% // поглощение {state.LostCirculationRisk01 * 100f:0}%";
            }

            if (taskEconomyValue != null && drillingModel != null)
            {
                taskEconomyValue.text =
                    $"БЮДЖЕТ СМЕНЫ: {drillingModel.CompanyCredits:+0;-0;0} кредитов // выполнено {drillingModel.SuccessfulSupervisorTasks} // провалено {drillingModel.FailedSupervisorTasks}.";
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
                supervisorTaskValue.text = CompactStatus(FormatSupervisorTask(activeSupervisorTask), 520);
            }

            rotationGraph?.Push(state.SurfaceTorqueKnM, state.Vibration.LowFrequencyEnergy * 100f);
            hydraulicGraph?.Push(state.StandpipePressureBar, (state.FlowOutLps - state.FlowRateLps) * 60f);
            bottomGraph?.Push(state.RopMPerHour, Mathf.Max(state.BitWear01, state.StuckPipeRisk01) * 100f);
            pressureGraph?.Push(state.BottomHolePressureMPa, state.PorePressureMPa);

            if (state.Vibration.LowFrequencyEnergy > 0.58f && state.InclinationDegrees > 70f && crew.Fatigue > 0.5f)
            {
                aiRecommendationValue.text = "РЕКОМЕНДАЦИЯ ИИ: риск автоколебаний колонны. Действие: плавно снизить обороты на 10-15% и стабилизировать нагрузку.";
            }
        }

        private void HandleSimulationEvent(SimulationEvent simulationEvent)
        {
            if (simulationEvent.Type == SimulationEventType.EdgeAIAlert && simulationEvent.Payload is AIRecommendation recommendation)
            {
                aiRecommendationValue.text = FormatAIRecommendation(recommendation);
                return;
            }

            if (simulationEvent.Type == SimulationEventType.CrewAIAlert && simulationEvent.Payload is AIRecommendation crewRecommendation)
            {
                if (aiCrewAdvisorValue != null)
                {
                    aiCrewAdvisorValue.text = FormatAIRecommendation(crewRecommendation);
                }

                return;
            }

            if (simulationEvent.Type == SimulationEventType.OperationalIncident && simulationEvent.Payload is OperationalIncident incident)
            {
                aiRecommendationValue.text = CompactStatus($"{incident.Title.ToUpperInvariant()}: {incident.Message}", 170);
                if (incidentConsequenceValue != null)
                {
                    incidentConsequenceValue.text = CompactStatus($"ПОСЛЕДСТВИЯ: {incident.Consequence}", 180);
                }

                radioLogValue.text = $"[{FormatClock()}] Инцидент на глубине по стволу {incident.MeasuredDepth:0} м: {incident.Title}.";
                return;
            }

            if (simulationEvent.Type == SimulationEventType.SupervisorTask && simulationEvent.Payload is SupervisorTask task)
            {
                activeSupervisorTask = task;
                hasActiveSupervisorTask = true;
                if (supervisorTaskValue != null)
                {
                    supervisorTaskValue.text = CompactStatus(FormatSupervisorTask(task), 520);
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
                    supervisorTaskValue.text = $"{status}: {taskResult.Task.Title}. {(taskResult.CreditsDelta >= 0 ? "+" : string.Empty)}{taskResult.CreditsDelta} кредитов.";
                }

                if (taskEconomyValue != null)
                {
                    taskEconomyValue.text = $"БЮДЖЕТ СМЕНЫ: {taskResult.TotalCredits:+0;-0;0} кредитов // последний результат: {(taskResult.CreditsDelta >= 0 ? "+" : string.Empty)}{taskResult.CreditsDelta}.";
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
            ShowStartScreen();
        }

        private void ShowMainScreen()
        {
            introCompleted = true;
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
            crewPage.style.display = DisplayStyle.None;
            telemetryTab.AddToClassList("tab-active");
            profileTab.RemoveFromClassList("tab-active");
            crewTab.RemoveFromClassList("tab-active");
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
                    tutorialTitle.text = "1/12  ЦЕЛЬ СМЕНЫ";
                    tutorialBody.text = "Главная цель - держать скважину в безопасном окне давлений. Забойное давление должно быть выше пластового, но ниже давления гидроразрыва. Если забойное ниже пластового - растет риск притока. Если забойное близко к гидроразрыву - растет риск поглощения.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 1:
                    tutorialTitle.text = "2/12  ПАРАМЕТРЫ УПРАВЛЕНИЯ";
                    tutorialBody.text = "Обороты управляют моментом и автоколебаниями. Нагрузка на долото управляет скоростью проходки, но ускоряет износ и вибрацию. Расход улучшает вынос шлама, но поднимает давление насоса. Плотность раствора и штуцер меняют эквивалентную плотность и окно давлений.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 2:
                    tutorialTitle.text = "3/12  ГРАФИКИ И ДАТЧИКИ";
                    tutorialBody.text = "На графиках важен не шум, а тренд. Рост момента вместе с вибрацией говорит о механической нестабильности. Рост давления насоса при плохой очистке указывает на сальникообразование. Разница расхода вход/выход показывает приток или поглощение.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 3:
                    tutorialTitle.text = "4/12  МЕНЮ ЗАПУСКА";
                    tutorialBody.text = "Главное меню работает как набор параметров. Локация, профиль, глубина и регион задают геометрию и геологию. Реализм, физика, состав бригады, длительность смены и частота задач меняют поведение модели. Разрешение в режиме Авто берется с текущего устройства; ручной режим принудительно меняет окно.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 4:
                    tutorialTitle.text = "5/12  ИИ-РЕКОМЕНДАЦИИ";
                    tutorialBody.text = "ИИ-рекомендация всегда состоит из причины и действия. Причина объясняет, какой тренд опасен. Действие говорит, какой параметр менять: обороты, нагрузку, расход, плотность раствора или штуцер. Выполняйте изменения ступенями, затем проверяйте датчики.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 5:
                    tutorialTitle.text = "6/12  ЗАДАЧИ ОТ МАСТЕРА";
                    tutorialBody.text = "В задаче смотрите четыре блока: критерий успеха, план инженера, контрольные датчики и срок. План инженера прямо указывает, что менять. Например: при притоке - прикрыть штуцер и поднять плотность; при поглощении - снизить расход и открыть штуцер.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 6:
                    tutorialTitle.text = "7/12  ПРИМЕРЫ РЕШЕНИЙ";
                    tutorialBody.text = "Приток: цель - выход не выше входа, газ ниже 6%, забойное давление выше пластового. Действия: штуцер -5..10%, плотность +0.02..0.05 SG. Поглощение: цель - восстановить выход и не разогнать давление. Действия: расход -100..250 л/мин, штуцер +5..10%.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 7:
                    tutorialTitle.text = "8/12  БРИГАДА И СМЕНЫ";
                    tutorialBody.text = "Бригада работает ролями: буровой мастер, бурильщик, растворщик, механик и инженер направленного бурения. Смена дня меняет людей и профиль опыта. Усталость увеличивает задержку команд и вероятность ошибки уставки. Инструктаж, осмотр и план рейса снижают операционный риск.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 8:
                    tutorialTitle.text = "9/12  ПОЛЕВЫЕ ПРОЦЕДУРЫ";
                    tutorialBody.text = "Процедуры бригады не заменяют крутилки, а дополняют их. Промывка помогает очистке ствола, замер раствора улучшает контроль плотности, осмотр вышки снижает риск отказа оборудования, расхаживание помогает при прихвате, снижение автоколебаний корректирует обороты и нагрузку.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 9:
                    tutorialTitle.text = "10/12  СКОРОСТЬ ВРЕМЕНИ";
                    tutorialBody.text = "Кнопки x1/x2/x5/x10/x20 ускоряют физику, усталость бригады и таймер задач одновременно. Для диагностики и сложных задач используйте x1 или x2. x10 и x20 удобны для ожидания, но опасны при притоке, поглощении и пересменке.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                case 10:
                    tutorialTitle.text = "11/12  3D-ПРОФИЛЬ";
                    tutorialBody.text = "3D-вкладка показывает положение долота, направление движения, станции глубины по стволу и суммарную зону риска. В наклонных и горизонтальных участках растут сопротивление движению, шламовая постель и автоколебания, поэтому решения по расходу, нагрузке и оборотам становятся важнее.";
                    tutorialNextButton.text = "ДАЛЕЕ";
                    break;

                default:
                    tutorialTitle.text = "12/12  ОКНО И ИТОГ";
                    tutorialBody.text = "Интерфейс подстраивается под выбранное разрешение и сохраняет читаемость при изменении окна. На устройстве 1920x1080 можно оставить Авто: приложение откроет это разрешение и масштабирует рабочий экран. Во время смены ориентируйтесь на критерий задачи, план инженера, умные графики и журнал бригады.";
                    tutorialNextButton.text = "НАЧАТЬ СМЕНУ";
                    break;
            }
        }

        private void ShowProfilePage()
        {
            telemetryPage.style.display = DisplayStyle.None;
            profilePage.style.display = DisplayStyle.Flex;
            crewPage.style.display = DisplayStyle.None;
            profileTab.AddToClassList("tab-active");
            telemetryTab.RemoveFromClassList("tab-active");
            crewTab.RemoveFromClassList("tab-active");
            if (cameraOrbit != null)
            {
                SetProfileCameraView(140f, 30f, 0.78f);
            }
        }

        private void SetProfileCameraView(float yaw, float pitch, float distanceFactor)
        {
            if (cameraOrbit == null)
            {
                return;
            }

            cameraOrbit.SetTarget(ResolveWellboreViewTarget());
            cameraOrbit.SetView(yaw, pitch, ResolveWellboreViewDistance(distanceFactor));
        }

        private void ShowCrewPage()
        {
            telemetryPage.style.display = DisplayStyle.None;
            profilePage.style.display = DisplayStyle.None;
            crewPage.style.display = DisplayStyle.Flex;
            crewTab.AddToClassList("tab-active");
            telemetryTab.RemoveFromClassList("tab-active");
            profileTab.RemoveFromClassList("tab-active");
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

        private float ResolveWellboreViewDistance(float distanceFactor)
        {
            if (wellbore == null || wellbore.Samples.Count < 2)
            {
                return 58f;
            }

            Vector3 min = wellbore.transform.TransformPoint(wellbore.Samples[0].Position);
            Vector3 max = min;
            for (int i = 1; i < wellbore.Samples.Count; i++)
            {
                Vector3 point = wellbore.transform.TransformPoint(wellbore.Samples[i].Position);
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }

            Vector3 size = max - min;
            float dominantExtent = Mathf.Max(size.x, size.y, size.z) * 0.5f;
            float diagonalExtent = size.magnitude * 0.5f;
            float fieldOfView = Camera.main != null ? Camera.main.fieldOfView : 48f;
            float fitDistance = Mathf.Max(dominantExtent * 1.35f, diagonalExtent) /
                                Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            return Mathf.Clamp(fitDistance * Mathf.Max(0.35f, distanceFactor), 38f, 220f);
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

            Label help = new Label(GetGraphExplanation(title));
            help.AddToClassList("graph-help-label");
            panel.Add(help);

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

        private static string GetGraphExplanation(string title)
        {
            switch (title)
            {
                case "ВРАЩЕНИЕ И МОМЕНТ":
                    return "Момент = сопротивление вращению долота и колонны, кНм. Вибрация = низкочастотные колебания КНБК; быстрый рост означает риск stick-slip, прихвата или перегруза привода.";

                case "ГИДРАВЛИКА И РАСХОД":
                    return "Давление насоса показывает нагрузку на циркуляцию. Баланс расхода = выход минус вход: плюс намекает на приток, минус на поглощение или потери.";

                case "СКОРОСТЬ ПРОХОДКИ И ИЗНОС":
                    return "Скорость проходки = метры в час. Красная линия объединяет износ долота и риск прихвата: если она растет при падении скорости, режим лучше смягчить.";

                default:
                    return "Линии показывают текущий тренд за последние секунды. Цвета совпадают с подписями осей, безопасный коридор подсвечен фоном.";
            }
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

        private float ResolveMusicVolume()
        {
            if (musicVolumeChoice == null)
            {
                return 0.35f;
            }

            switch (musicVolumeChoice.Index)
            {
                case 1:
                    return 0.15f;
                case 2:
                    return 0.6f;
                case 3:
                    return 0f;
                default:
                    return 0.35f;
            }
        }

        private float ResolveStartSpeedMultiplier()
        {
            if (startSpeedChoice == null)
            {
                return 1f;
            }

            switch (startSpeedChoice.Index)
            {
                case 1:
                    return 2f;
                case 2:
                    return 5f;
                default:
                    return 1f;
            }
        }

        private float ResolveShiftDurationMinutes()
        {
            if (shiftDurationChoice == null)
            {
                return 8f;
            }

            switch (shiftDurationChoice.Index)
            {
                case 1:
                    return 6f;
                case 2:
                    return 12f;
                default:
                    return 8f;
            }
        }

        private float ResolveSupervisorCadenceMultiplier()
        {
            if (supervisorPaceChoice == null)
            {
                return 1f;
            }

            switch (supervisorPaceChoice.Index)
            {
                case 1:
                    return 0.65f;
                case 2:
                    return 1.45f;
                default:
                    return 1f;
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

            return CompactStatus($"ИИ: {severity}. {recommendation.Title}. Причина: {recommendation.Message} Действие: {action}", 240);
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

            return $"ЗАДАЧА: {task.Title}. До срока: {timerText}. Критерий: {task.SuccessCriteria} План инженера: {BuildEngineeringTaskGuide(task)} Контроль: {task.SensorFocus}. Бонус +{task.RewardCredits} / штраф -{task.FailurePenaltyCredits}.";
        }

        private string BuildEngineeringTaskGuide(SupervisorTask task)
        {
            if (drillingModel == null)
            {
                return task.ControlHints;
            }

            DrillingState state = drillingModel.CurrentState;
            float flowIn = Mathf.Max(0.1f, state.FlowRateLps);
            float flowBalancePercent = (state.FlowOutLps - state.FlowRateLps) / flowIn * 100f;
            float vibrationG = state.Vibration.LowFrequencyEnergy * 3.5f;
            float lowMargin = state.BottomHolePressureMPa - state.PorePressureMPa;
            float highMargin = state.FracturePressureMPa - state.BottomHolePressureMPa;
            float flowLpm = state.FlowRateLps * 60f;

            switch (task.Type)
            {
                case SupervisorTaskType.KickControl:
                    return $"сейчас выход {flowBalancePercent:+0.0;-0.0;0.0}%, газ {state.GasUnitsPercent:0.0}%, запас к пластовому {lowMargin:0.0} МПа. Прикрывайте штуцер на 5-10% и поднимайте плотность на 0.02-0.05 SG до запаса >0.25 МПа; обороты и нагрузку не повышать.";

                case SupervisorTaskType.LossControl:
                    return $"сейчас выход {flowBalancePercent:+0.0;-0.0;0.0}%, расход {flowLpm:0} л/мин. Снизьте расход на 100-250 л/мин и откройте штуцер на 5-10%; плотность не повышать, пока выход не лучше -5%.";

                case SupervisorTaskType.DirectionalDrag:
                    return $"сейчас сопротивление {state.DragTonnes:0.0} т, очистка {state.CuttingsTransportEfficiency01 * 100f:0}%, вибрация {vibrationG:0.0} g. Снизьте нагрузку на 0.5-2 т, при плохой очистке добавьте расход 100-200 л/мин, при вибрации снизьте обороты на 10-20.";

                case SupervisorTaskType.HoleCleaning:
                    return $"сейчас очистка {state.CuttingsTransportEfficiency01 * 100f:0}%, давление насоса {state.StandpipePressureBar:0} бар. Поднимайте расход ступенями по 100 л/мин, снизьте нагрузку на 1-2 т и выполните промывку ствола; остановиться, если давление насоса растет >12 бар.";

                case SupervisorTaskType.ShaleStability:
                    return $"сейчас запас к пластовому {lowMargin:0.0} МПа, до гидроразрыва {highMargin:0.0} МПа. Если нижний запас <0.25 МПа - плотность +0.01-0.03 SG или штуцер -5%; если верхний запас <0.45 МПа - плотность -0.01-0.03 SG или штуцер +5%.";

                case SupervisorTaskType.BitAssessment:
                    return $"сейчас вибрация {vibrationG:0.0} g, момент {state.SurfaceTorqueKnM:0.0} кНм, скорость {state.RopMPerHour:0.0} м/ч. Снизьте нагрузку на 1-3 т и обороты на 10-25; затем возвращайте нагрузку только если вибрация <2.2 g.";

                case SupervisorTaskType.PressureWindow:
                    return $"сейчас запас к пластовому {lowMargin:0.0} МПа, до гидроразрыва {highMargin:0.0} МПа. Работайте плотностью по 0.01-0.03 SG и штуцером по 5%; держите оба запаса положительными и не меняйте расход резко.";

                case SupervisorTaskType.CrewHandover:
                    return $"сейчас усталость смены высокая. Выполните инструктаж, осмотр вышки и план рейса; держите скорость времени x1 и не отправляйте лишние команды, пока усталость не снизится ниже 58%.";

                case SupervisorTaskType.PumpEfficiency:
                    return $"сейчас расход {flowLpm:0} л/мин, очистка {state.CuttingsTransportEfficiency01 * 100f:0}%, давление насоса {state.StandpipePressureBar:0} бар. Если очистка <64% - расход +100 л/мин; если давление выросло >18 бар - расход -100 л/мин и проверьте вынос.";

                case SupervisorTaskType.GasMonitoring:
                    return $"сейчас газ {state.GasUnitsPercent:0.0}%, выход {flowBalancePercent:+0.0;-0.0;0.0}%, запас к пластовому {lowMargin:0.0} МПа. Не снижайте давление, поручите бригаде контроль дегазатора и емкостей.";

                case SupervisorTaskType.ToolfaceControl:
                    return $"сейчас dogleg {state.DoglegSeverityDegPer30m:0.0}°/30 м, зенит {state.InclinationDegrees:0.0}°, drag {state.DragTonnes:0.0} т. Снизьте грубую нагрузку и запросите контрольный MWD-замер.";

                case SupervisorTaskType.PumpIntegrity:
                    return $"сейчас давление насоса {state.StandpipePressureBar:0} бар, расход {flowLpm:0} л/мин, баланс {flowBalancePercent:+0.0;-0.0;0.0}%. Меняйте расход ступенями и отправьте механику на проверку насосов.";

                case SupervisorTaskType.EquipmentInspection:
                    return $"сейчас момент {state.SurfaceTorqueKnM:0.0} кНм, вибрация {vibrationG:0.0} g. Выполните осмотр вышки/верхнего привода и избегайте одновременного изменения оборотов, нагрузки и расхода.";

                case SupervisorTaskType.WeatherResponse:
                    return $"внешние условия мешают работе смены. Проведите инструктаж, подтвердите связь и контроль емкостей; параметры менять медленнее обычного.";

                case SupervisorTaskType.MwdSurvey:
                    return $"сейчас зенит {state.InclinationDegrees:0.0}°, азимут {state.AzimuthDegrees:0}°, dogleg {state.DoglegSeverityDegPer30m:0.0}°/30 м. До замера ННБ держите мягкий режим без форсирования угла.";

                case SupervisorTaskType.TorqueSmoothing:
                    return $"сейчас момент {state.SurfaceTorqueKnM:0.0} кНм, вибрация {vibrationG:0.0} g. Снизьте обороты и нагрузку малыми шагами; возвращайте режим только после стабилизации момента.";

                case SupervisorTaskType.ConnectionProcedure:
                    return $"сейчас баланс {flowBalancePercent:+0.0;-0.0;0.0}%, давление насоса {state.StandpipePressureBar:0} бар. Перед наращиванием стабилизируйте циркуляцию и проведите чек-лист бригады.";

                default:
                    return $"держите скорость проходки >10 м/ч при вибрации <2.2 g: обороты и нагрузку повышать малыми шагами, расход держать под очистку, плотность и штуцер не выводить из окна давлений.";
            }
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
            private const float DragPixelsForFullRange = 260f;
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
            public readonly float SafeMin;
            public readonly float SafeMax;
            public readonly bool HasSafeRange;

            public GraphAxisScale(string title, float defaultMin, float defaultMax, float safeMin = float.NaN, float safeMax = float.NaN)
            {
                Title = title;
                DefaultMin = Mathf.Min(defaultMin, defaultMax);
                DefaultMax = Mathf.Max(defaultMin, defaultMax);
                SafeMin = Mathf.Min(safeMin, safeMax);
                SafeMax = Mathf.Max(safeMin, safeMax);
                HasSafeRange = !float.IsNaN(safeMin) && !float.IsNaN(safeMax);
            }

            public void GetRange(List<float> values, out float min, out float max)
            {
                if (values == null || values.Count == 0)
                {
                    min = DefaultMin;
                    max = DefaultMax;
                    return;
                }

                min = values[0];
                max = values[0];
                for (int i = 1; i < values.Count; i++)
                {
                    min = Mathf.Min(min, values[i]);
                    max = Mathf.Max(max, values[i]);
                }

                float defaultRange = Mathf.Max(DefaultMax - DefaultMin, 1f);
                float minVisibleRange = defaultRange * 0.18f;
                float center = (min + max) * 0.5f;
                float range = max - min;
                if (range < minVisibleRange)
                {
                    min = center - minVisibleRange * 0.5f;
                    max = center + minVisibleRange * 0.5f;
                    range = max - min;
                }

                float edgePadding = Mathf.Max(range * 0.08f, 0.001f);
                min -= edgePadding;
                max += edgePadding;

                if (DefaultMin >= 0f)
                {
                    min = Mathf.Max(DefaultMin, min);
                }

                if (max <= min)
                {
                    max = min + minVisibleRange;
                }
            }
        }

        private sealed class TrendGraphElement : VisualElement
        {
            private const int Capacity = 180;
            private const float PlotLeftPadding = 48f;
            private const float PlotRightPadding = 48f;
            private const float PlotTopPadding = 30f;
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
            private readonly Label primaryCurrentLabel;
            private readonly Label secondaryCurrentLabel;
            private readonly Label statusLabel;

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
                primaryCurrentLabel = CreateGraphLabel("graph-label-primary-current", primaryColor);
                secondaryCurrentLabel = CreateGraphLabel("graph-label-secondary-current", secondaryColor);
                statusLabel = CreateGraphLabel("graph-label-status", GraphBlack);
                CreateAxisTitle(primaryScale.Title, "graph-axis-title-left", primaryColor);
                CreateAxisTitle(secondaryScale.Title, "graph-axis-title-right", secondaryColor);
                CreateAxisTitle("Последние 27 с", "graph-axis-title-bottom", GraphBlack);

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
                primaryCurrentLabel.text = FormatCurrent(primary, primaryScale);
                secondaryCurrentLabel.text = FormatCurrent(secondary, secondaryScale);
                statusLabel.text = BuildStatus(primary, primaryScale);
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

            private static string FormatCurrent(List<float> values, GraphAxisScale scale)
            {
                if (values.Count == 0)
                {
                    return string.Empty;
                }

                float current = values[values.Count - 1];
                return $"{FormatValue(current)} {TrendLabel(values)}";
            }

            private static string TrendLabel(List<float> values)
            {
                if (values.Count < 10)
                {
                    return "стаб.";
                }

                float recent = AverageTail(values, 8);
                float earlier = AverageRange(values, Mathf.Max(0, values.Count - 24), Mathf.Max(0, values.Count - 12));
                float delta = recent - earlier;
                float reference = Mathf.Max(1f, Mathf.Abs(earlier));
                if (Mathf.Abs(delta) / reference < 0.025f)
                {
                    return "стаб.";
                }

                return delta > 0f ? "рост" : "сниж.";
            }

            private static string BuildStatus(List<float> values, GraphAxisScale scale)
            {
                if (!scale.HasSafeRange || values.Count == 0)
                {
                    return "ТРЕНД";
                }

                float current = values[values.Count - 1];
                if (current < scale.SafeMin)
                {
                    return $"НИЖЕ ОКНА {FormatValue(scale.SafeMin)}";
                }

                if (current > scale.SafeMax)
                {
                    return $"ВЫШЕ ОКНА {FormatValue(scale.SafeMax)}";
                }

                return $"ОКНО {FormatValue(scale.SafeMin)}-{FormatValue(scale.SafeMax)}";
            }

            private static float AverageTail(List<float> values, int count)
            {
                return AverageRange(values, Mathf.Max(0, values.Count - count), values.Count);
            }

            private static float AverageRange(List<float> values, int startInclusive, int endExclusive)
            {
                int start = Mathf.Clamp(startInclusive, 0, values.Count);
                int end = Mathf.Clamp(endExclusive, start, values.Count);
                if (end <= start)
                {
                    return values.Count > 0 ? values[values.Count - 1] : 0f;
                }

                float sum = 0f;
                for (int i = start; i < end; i++)
                {
                    sum += values[i];
                }

                return sum / (end - start);
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
                DrawSafeBand(painter, plotRect, primary, primaryScale);
                DrawGrid(painter, plotRect);
                DrawZeroLine(painter, plotRect, primary, primaryScale);
                DrawZeroLine(painter, plotRect, secondary, secondaryScale);
                DrawSeries(painter, plotRect, secondary, secondaryColor, 2.0f, secondaryScale);
                DrawSeries(painter, plotRect, primary, primaryColor, 3.4f, primaryScale);
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

            private static void DrawSafeBand(Painter2D painter, Rect rect, List<float> values, GraphAxisScale scale)
            {
                if (!scale.HasSafeRange)
                {
                    return;
                }

                scale.GetRange(values, out float min, out float max);
                float yTop = Mathf.Lerp(rect.yMax, rect.yMin, Mathf.Clamp01(Mathf.InverseLerp(min, max, scale.SafeMax)));
                float yBottom = Mathf.Lerp(rect.yMax, rect.yMin, Mathf.Clamp01(Mathf.InverseLerp(min, max, scale.SafeMin)));
                painter.fillColor = new Color(0.13f, 0.55f, 0.32f, 0.07f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(rect.xMin, yTop));
                painter.LineTo(new Vector2(rect.xMax, yTop));
                painter.LineTo(new Vector2(rect.xMax, yBottom));
                painter.LineTo(new Vector2(rect.xMin, yBottom));
                painter.ClosePath();
                painter.Fill();
            }

            private static void DrawZeroLine(Painter2D painter, Rect rect, List<float> values, GraphAxisScale scale)
            {
                scale.GetRange(values, out float min, out float max);
                if (min > 0f || max < 0f)
                {
                    return;
                }

                float y = Mathf.Lerp(rect.yMax, rect.yMin, Mathf.InverseLerp(min, max, 0f));
                painter.lineWidth = 1.5f;
                painter.strokeColor = new Color(0.05f, 0.05f, 0.05f, 0.45f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(rect.xMin, y));
                painter.LineTo(new Vector2(rect.xMax, y));
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
                float smoothed = values[0];
                Vector2 lastPoint = Vector2.zero;

                for (int i = 0; i < values.Count; i++)
                {
                    smoothed = i == 0 ? values[i] : Mathf.Lerp(smoothed, values[i], 0.32f);
                    float x = Mathf.Lerp(rect.xMin, rect.xMax, i / (float)Mathf.Max(1, values.Count - 1));
                    float normalized = Mathf.Clamp01(Mathf.InverseLerp(min, max, smoothed));
                    float y = Mathf.Lerp(rect.yMax, rect.yMin, normalized);
                    Vector2 point = new Vector2(x, y);
                    lastPoint = point;

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
                DrawDot(painter, lastPoint, width + 2.5f, color);
            }

            private static void DrawDot(Painter2D painter, Vector2 center, float radius, Color color)
            {
                painter.fillColor = color;
                painter.BeginPath();
                for (int i = 0; i < 18; i++)
                {
                    float angle = i / 18f * Mathf.PI * 2f;
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
