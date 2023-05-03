using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameMaster;
using static InputManager;

public class UIManager : MonoBehaviour
{
    GameMaster master;
    PlayerNetwork network;
    InputManager input;
    SoundManager sound;
    VehicleManager vehicle;
    ActivityOption activityOption;

    [SerializeField] GameObject canvas;
    [SerializeField] CanvasGroup screen;
    [SerializeField] CanvasGroup gameUI;
    [SerializeField] CanvasGroup options;
    [SerializeField] CanvasGroup controls;

    [Header("Screen")]
    public bool toogleHUD = true;
    [SerializeField] GameObject blackScreen;
    [SerializeField] GameObject whiteScreen;
    [SerializeField] GameObject blackFadeOverlay;
    [SerializeField] GameObject blackOverlay;

    [Header("HUD")]
    [SerializeField] public Text speed;

    [Header("Game")]
    [SerializeField] TMP_Text gameSateTMP;
    [SerializeField] TMP_Text toggleOptionsTMP;
    [SerializeField] TMP_Text exitHintTMP;

    [Header("Options")]
    [SerializeField] RawImage protoTaxi;
    [SerializeField] Texture[] protoTaxiColors;
    [SerializeField] GameObject changePlayerName;
    [SerializeField] GameObject changeTaxiColor;
    [SerializeField] GameObject colorOptionsPanel;
    [SerializeField] GameObject[] colors;
    TMP_Text[] colorSelectTMP;

    [Header("Activity Trigger")]
    [SerializeField] CanvasGroup activityTrigger;
    [SerializeField] GameObject activityTriggerTypeIcon;
    [SerializeField] GameObject activityTriggerTypeText;
    [SerializeField] GameObject activityName;
    [SerializeField] GameObject activityPressStart;
    [SerializeField] GameObject activityOptions;
    [SerializeField] GameObject activityPressAdjust;

    [SerializeField] GameObject[] activityTriggerCanvas;

    [Header("Activity")]
    [SerializeField] GameObject activityCountdown10;
    [SerializeField] GameObject activityCountdown5;
    [SerializeField] GameObject activityCountdown321;
    [SerializeField] GameObject activityCountdownZero;
    [SerializeField] GameObject activityProgress;
    [SerializeField] TMP_Text countTMP;
    [SerializeField] TMP_Text checkpointTMP;
    [SerializeField] TMP_Text timeTMP;
    [SerializeField] public TMP_Text huntSpeedLimitTMP;

    [Header("Activity Finish")]
    [SerializeField] GameObject activityResult;
    [SerializeField] RawImage activityFinishTypeIcon;
    [SerializeField] TMP_Text activityNameTMP;
    [SerializeField] TMP_Text finishRankTMP;
    [SerializeField] TMP_Text playerResultTMP;
    [SerializeField] public TMP_Text returnSessionTMP;

    [Header("Icons")]
    [SerializeField] Texture activityIconRace;
    [SerializeField] Texture activityIconCollect;
    [SerializeField] Texture activityIconHunt;

    [Header("Animations")]
    public AnimationClip[] clips;

    /* Button Prompts */
    bool promptsFirstInit = false;
    [HideInInspector] public bool promptsInitialized = false;
    string prompt_startActivity;
    string prompt_adjustActivity;
    string prompt_toggleOptions;
    string prompt_toggleControls;
    string prompt_exitHint;
    TMP_Text activityPressStartTMP;
    TMP_Text activityPressAdjustTMP;

    /* Race UI */
    int race_totalLap;
    int race_totalCheckpoint;

    /* Collect UI */
    int collect_totalCheckpoint;

    /* Hunt UI  */
    int hunt_initialPoint;

    bool toggleOptions = true;
    bool toggleControls = false;
    float returnSessionTime;

    /* Tunables */
    float activityTriggerFade = 0.1f;

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
        network = master.network;
        input = master.input;
        sound = master.sound;
        vehicle = master.vehicle;
        activityOption = GameObject.Find("[Activity Triggers]").GetComponent<ActivityOption>();

        // Get the select text for each color option
        colorSelectTMP = new TMP_Text[colors.Length];
        for (int i = 0; i < colors.Length; i++)
            colorSelectTMP[i] = colors[i].transform.Find("[Select]").GetComponent<TextMeshProUGUI>();

        // Get the trigger canvas for each activity
        activityTriggerCanvas = new GameObject[master.activityList.Length];
        for (int i = 0; i < master.activityList.Length; i++)
        {
            string activityName = master.activityList[i].mainObject.name;
            GameObject triggerCanvas = Methods.GetChildContainsName(master.activityList[i].triggerObject, activityName);
            if (triggerCanvas != null && triggerCanvas.name.Contains("[Canvas]"))
                activityTriggerCanvas[i] = triggerCanvas;
        }

        activityPressStartTMP = activityPressStart.gameObject.GetComponent<TextMeshProUGUI>();
        activityPressAdjustTMP = activityPressAdjust.gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        AnimationsInitial();
        ChangeButtonType(inputType.MouseKeyboard);
        CanvasGroupToggle(options, toggleOptions);
        CanvasGroupToggle(controls, toggleControls);
        huntSpeedLimitTMP.enabled = false;
    }

    void AnimationsInitial()
    {
        // Screen
        Initial(blackScreen, "Fade Black Initial", 0, 1.0f);
        Initial(whiteScreen, "White Flash Initial", 0, 1.0f);

        // Game UI
        HideActivityInfo();
        foreach (var canvas in activityTriggerCanvas)
        {
            if (canvas.transform.parent.gameObject.activeSelf)
                Initial(canvas, "Activity Trigger Canvas Initial", 0, 1.0f);
        }
        Initial(activityCountdown10, "Activity Countdown 10 Initial", 0, 0.0f);
        Initial(activityCountdown5, "Activity Countdown 5 Initial", 0, 0.0f);
        Initial(activityCountdown321, "Activity Countdown 321 Initial", 0, 0.0f);
        Initial(activityCountdownZero, "Activity Countdown Start Initial", 0, 1.0f);
        Initial(activityProgress, "Activity UI Initial", 0, 0.0f);
        Initial(activityResult, "Activity UI Initial", 0, 0.0f);

        // Options
        foreach (var color in colors)
            Initial(color, "Color Option Initial", 0, 0.0f);
    }

    void Update()
    {
        if (input.ToggleHUD()) ToggleHUD();
        InitializePromptText();
        UpdatePromptText();

        if (master.ready)
        {
            if (input.ToggleOptions())
            {
                toggleOptions = !toggleOptions;
                ToggleOptions(toggleOptions);
            }

            if (input.ToggleControls())
            {
                toggleControls = !toggleControls;
                ToggleControls(toggleControls);
            }
        }
        else
        {
            blackOverlay.SetActive(true);
        }

        activityTrigger.alpha = (toggleOptions || toggleControls) ? activityTriggerFade : 1;
        exitHintTMP.enabled = input.allowExitActivity;

        if (returnSessionTMP.isActiveAndEnabled)
        {
            int remaining = Mathf.FloorToInt(returnSessionTime + master.activityFinishWaitDuration - Time.time);
            returnSessionTMP.text = "Return to session (0:0" + remaining + ")";
        }
    }

    void ToggleHUD()
    {
        toogleHUD = !toogleHUD;
        CanvasGroupToggle(canvas.GetComponent<CanvasGroup>(), toogleHUD);
    }

    public void Toggles(bool state)
    {
        ToggleOptions(state);
        ToggleControls(state);
    }

    void ToggleOptions(bool state)
    {
        toggleOptions = state;
        CanvasGroupToggle(options, state);
        blackOverlay.SetActive(state);
        master.postFX.ToggleDOV(state);
    }

    void ToggleControls(bool state)
    {
        toggleControls = state;
        CanvasGroupToggle(controls, state);
        blackOverlay.SetActive(state);
        master.postFX.ToggleDOV(state);
    }

    public void DisplayGameSate(string state)
    {
        gameSateTMP.text = state;
    }

    public void EnterSession()
    {
        ToggleOptions(false);
        protoTaxi.enabled = false;
        changePlayerName.GetComponent<RectTransform>().localPosition = new Vector2(-650, 150);
        changeTaxiColor.GetComponent<RectTransform>().localPosition = new Vector2(-650, 0);
        colorOptionsPanel.GetComponent<RectTransform>().localPosition = new Vector2(-650, -60);
    }

    #region Button Prompts

    public void ChangeButtonType(inputType type)
    {
        if (type == inputType.MouseKeyboard)
        {
            prompt_startActivity = "E";
            prompt_adjustActivity = "+/-";
            prompt_toggleOptions = "Tab";
            prompt_toggleControls = "Ctrl";
            prompt_exitHint = "Esc";
        }

        if (type == inputType.Gamepad)
        {
            prompt_startActivity = ControllerFont('x');
            prompt_adjustActivity = ControllerFont('V');
            prompt_toggleOptions = ControllerFont('W');
            prompt_toggleControls = ControllerFont('m');
            prompt_exitHint = ControllerFont('v');
        }
    }

    string ControllerFont(char key)
    {
        string opening = "<font=\"XboxOne-Controller SDF\">";
        string closing = "</font>";
        return opening + "<b>" + key + "</b>" + closing;
    }

    void InitializePromptText()
    {
        if (!promptsFirstInit)
        {
            ChangeButtonType(inputType.Gamepad);
            PromptsInitial("Show");
            promptsFirstInit = true;
        }
        else if (!promptsInitialized)
        {
            PromptsInitial("Initial");
            ChangeButtonType(inputType.MouseKeyboard);
            promptsInitialized = true;
        }

        // Button Prompt Texts that use controller font should be listed here to be initialized
        void PromptsInitial(string type)
        {
            if (type == "Show")
            {
                Show(activityPressStart, "Activity Press Start In", 0, 0.0f);
                Show(activityPressAdjust, "Activity Option In", 0, 0.0f);
            }
            if (type == "Initial")
            {
                Initial(activityPressStart, "Activity Press Start Initial", 0, 0.0f);
                Initial(activityPressAdjust, "Activity Option Initial", 0, 0.0f);
            }
        }
    }

    void UpdatePromptText()
    {
        activityPressStartTMP.text = "Press " + prompt_startActivity + " to start";
        activityPressAdjustTMP.text = "Press " + prompt_adjustActivity + " to adjust";
        toggleOptionsTMP.text = prompt_toggleOptions + " - Options" + "     " + prompt_toggleControls + " - Controls";
        exitHintTMP.text = prompt_exitHint + " - Exit Activity";
    }

    #endregion

    #region Initial / Show / Hide

    void Initial(GameObject user, string stateName, int layer, float normalizedTime)
    {
        user.GetComponent<Animator>().Play(stateName, layer, normalizedTime);
        ToggleVisible(user, false);
    }

    void Show(GameObject user, string stateName, int layer, float normalizedTime)
    {
        ToggleVisible(user, true);
        user.GetComponent<Animator>().Play(stateName, layer, normalizedTime);
    }

    void Hide(GameObject user, string stateName, int layer, float normalizedTime)
    {
        user.GetComponent<Animator>().Play(stateName, layer, normalizedTime);
        StartCoroutine(InactiveAfter(user.GetComponent<Animator>()));
        IEnumerator InactiveAfter(Animator animator)
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
            ToggleVisible(user, false);
        }
    }

    void ToggleVisible(GameObject user, bool visibility)
    {
        if (user.GetComponent<RawImage>() != null) user.GetComponent<RawImage>().enabled = visibility;
        if (user.GetComponent<TextMeshProUGUI>() != null) user.GetComponent<TextMeshProUGUI>().enabled = visibility;
    }

    #endregion

    #region Options

    public void OnColorOptionHover(GameObject colorOption)
    {
        int i = Array.IndexOf(colors, colorOption);
        if (i == vehicle.playerColorIndex) return;
        Show(colors[i], "Color Option In", 0, 0.0f);
        sound.Play(Sound.name.Select);
    }

    public void OnColorOptionExit(GameObject colorOption)
    {
        int i = Array.IndexOf(colors, colorOption);
        if (i == vehicle.playerColorIndex) return;
        Hide(colors[i], "Color Option Out", 0, 0.0f);
    }

    public void SelectColorOption(int prev, int current)
    {
        if (prev != -1)
        {
            Hide(colors[prev], "Color Option Out", 0, 0.0f);
            colorSelectTMP[prev].text = "Select";
        }
        Show(colors[current], "Color Option Initial", 0, 1.0f);
        colorSelectTMP[current].text = "Selected";

        protoTaxi.texture = protoTaxiColors[current];
    }

    #endregion

    #region Activity

    public void UpdateActivityOptions(int index)
    {
        activityType type = master.activityList[index].type;
        string optionText = "";

        if (type == activityType.RaceCircuit)
            optionText = "Laps: " + activityOption.raceLaps[activityOption.currentRaceLap];

        if (type == activityType.CollectionBattle)
            optionText = "Duration: " + activityOption.collectDurations[activityOption.currentCollectDuration] + " mins";

        if (type == activityType.CarHunt)
            optionText = "Duration: " + activityOption.huntDurations[activityOption.currentHuntDuration] + " mins";

        activityOptions.GetComponent<TextMeshProUGUI>().text = optionText;
    }

    public void ShowActivityInfoAnim(int index)
    {
        activityType type = master.activityList[index].type;
        string name = master.activityList[index].name;

        activityTriggerTypeIcon.gameObject.GetComponent<RawImage>().texture = ActivityTypeIcon(type);
        Show(activityTriggerTypeIcon, "Activity Type Icon In", 0, 0.0f);
        activityTriggerTypeText.gameObject.GetComponent<TextMeshProUGUI>().text = ActivityTypeName(type);
        Show(activityTriggerTypeText, "Activity Type Text In", 0, 0.0f);
        activityName.gameObject.GetComponent<TextMeshProUGUI>().text = name;
        Show(activityName, "Activity Name In", 0, 0.0f);
        Show(activityPressStart, "Activity Press Start In", 0, 0.0f);
        Show(activityOptions, "Activity Option In", 0, 0.0f);
        Show(activityPressAdjust, "Activity Option In", 0, 0.0f);
        Show(blackFadeOverlay, "Black Fade Left Overlay In", 0, 0.0f);
    }

    public void HideActivityInfoAnim(int index)
    {
        Hide(activityTriggerTypeIcon, "Activity Type Icon Out", 0, 0.0f);
        Hide(activityTriggerTypeText, "Activity Type Text Out", 0, 0.0f);
        Hide(activityName, "Activity Name Out", 0, 0.0f);
        Hide(activityPressStart, "Activity Press Start Out", 0, 0.0f);
        Hide(activityOptions, "Activity Option Out", 0, 0.0f);
        Hide(activityPressAdjust, "Activity Option Out", 0, 0.0f);
        Hide(blackFadeOverlay, "Black Fade Left Overlay Out", 0, 0.0f);
    }

    public void HideActivityInfo()
    {
        Initial(activityTriggerTypeIcon, "Activity Type Icon Initial", 0, 0.0f);
        Initial(activityTriggerTypeText, "Activity Type Text Initial", 0, 0.0f);
        Initial(activityName, "Activity Name Initial", 0, 0.0f);
        Initial(activityPressStart, "Activity Press Start Initial", 0, 0.0f);
        Initial(activityOptions, "Activity Option Initial", 0, 0.0f);
        Initial(activityPressAdjust, "Activity Option Initial", 0, 0.0f);
        Initial(blackFadeOverlay, "Black Fade Left Overlay Initial", 0, 0.0f);
    }

    public void ActivityTriggerCanvas(int index, string type)
    {
        if (type == "Show") Show(activityTriggerCanvas[index], "Activity Trigger Canvas In", 0, 0.0f);
        if (type == "Hide") Hide(activityTriggerCanvas[index], "Activity Trigger Canvas Out", 0, 0.0f);
    }

    public void ActivityCountdown10(string type)
    {
        if (type == "Play") Show(activityCountdown10, "Activity Countdown 10", 0, 0.0f);
        if (type == "Initial") Initial(activityCountdown10, "Activity Countdown 10 Initial", 0, 0.0f);
    }

    public void ActivityCountdown5(string type)
    {
        if (type == "Play") Show(activityCountdown5, "Activity Countdown 5", 0, 0.0f);
        if (type == "Initial") Initial(activityCountdown5, "Activity Countdown 5 Initial", 0, 0.0f);
    }

    public void ActivityCountdown321(string type)
    {
        if (type == "Play") Show(activityCountdown321, "Activity Countdown 321", 0, 0.0f);
        if (type == "Initial") Initial(activityCountdown321, "Activity Countdown 321 Initial", 0, 0.0f);
    }

    public void ActivityCountdown(string text)
    {
        activityCountdownZero.GetComponent<TextMeshProUGUI>().text = text;
        Show(activityCountdownZero, "Activity Countdown Start", 0, 0.0f);
    }

    public void ActivityUI(activityType activityType, string type)
    {
        if (type == "Show") Show(activityProgress, "Activity UI Show", 0, 0.0f);
        if (type == "Hide") Hide(activityProgress, "Activity UI Hide", 0, 0.0f);
        if (type == "Initial") Initial(activityProgress, "Activity UI Initial", 0, 0.0f);
    }

    #endregion

    #region Info (Activity) UI
    // Info of this activity that do not need to be continuously updated during the activity.
    public void InfoRaceUI(int totalLap, int totalCheckpoint)
    {
        race_totalLap = totalLap;
        race_totalCheckpoint = totalCheckpoint;
    }

    public void InfoCollectUI(int totalCheckpoint)
    {
        collect_totalCheckpoint = totalCheckpoint;
    }

    public void InfoHuntUI(int initialPoint)
    {
        hunt_initialPoint = initialPoint;
    }

    #endregion

    #region Update (Activity) UI
    // Variables that are continuously updated during the activity.

    public void UpdateRaceDestinationUI(int currentCheckpoint, float raceTime)
    {
        countTMP.text = currentCheckpoint + " / " + race_totalCheckpoint;
        checkpointTMP.text = "Checkpoint";
        timeTMP.text = "Time: " + Methods.TimeFormat(raceTime, true);
    }

    public void UpdateRaceCircuitUI(int currentLap, int currentCheckpoint, float raceTime)
    {
        countTMP.text = currentLap + " / " + race_totalLap + " <size=\"40\">Lap</size>";
        checkpointTMP.text = "Checkpoint: " + currentCheckpoint + " / " + race_totalCheckpoint;
        timeTMP.text = "Time: " + Methods.TimeFormat(raceTime, true);
    }

    public void UpdateCollectUI(int score, int remainingCount, float remainingTime)
    {
        countTMP.text = score + " <size=\"40\">Score</size>";
        checkpointTMP.text = "Remaining: " + remainingCount + " / " + collect_totalCheckpoint;
        timeTMP.text = "Time left: " + Methods.TimeFormat(remainingTime, false);
    }

    public void UpdateHuntUI(int point, float remainingTime)
    {
        countTMP.text = Methods.TimeFormat(remainingTime, false); point.ToString();
        checkpointTMP.text = "Points remaining: " + point.ToString();
        timeTMP.text = "";
    }

    #endregion

    #region Result (Activity) UI
    // Variables that are recorded upon completion of the activity.

    public void ResultRaceUI(int index, float raceTime, float currentTime)
    {
        activityFinishTypeIcon.texture = ActivityTypeIcon(master.activityList[index].type);
        activityNameTMP.text = master.activityList[index].name;
        finishRankTMP.text = "FINISH";
        playerResultTMP.text = Methods.TimeFormat(raceTime, true) + " | Player";
        returnSessionTime = currentTime;
        returnSessionTMP.enabled = true;
    }

    public void ResultCollectUI(int index, int score, bool cleared, float currentTime)
    {
        activityFinishTypeIcon.texture = ActivityTypeIcon(master.activityList[index].type);
        activityNameTMP.text = master.activityList[index].name;
        finishRankTMP.text = (cleared) ? "CLEAR!" : "FINISH";
        playerResultTMP.text = score + " | Player";
        returnSessionTime = currentTime;
        returnSessionTMP.enabled = true;
    }

    public void ResultHuntUI(int index, int point, bool win, float remainingTime, float currentTime)
    {
        activityFinishTypeIcon.texture = ActivityTypeIcon(master.activityList[index].type);
        activityNameTMP.text = master.activityList[index].name;
        finishRankTMP.text = (win) ? "WIN" : "BUSTED!"; // for hunter: WIN / LOSE
        playerResultTMP.text = Methods.TimeFormat(remainingTime, true) + " | " + point + " Points | Player";
        returnSessionTime = currentTime;
        returnSessionTMP.enabled = true;
    }

    public void ActivityResultUI(activityType activityType, string type)
    {
        if (type == "Show")
        {
            Show(activityResult, "Activity UI Show", 0, 0.0f);
            Show(blackFadeOverlay, "Black Fade Left Overlay In", 0, 0.0f);
        }
        if (type == "Hide")
        {
            Hide(activityResult, "Activity UI Hide", 0, 0.0f);
            Hide(blackFadeOverlay, "Black Fade Left Overlay Out", 0, 0.0f);
        }
        if (type == "Initial")
        {
            Initial(activityResult, "Activity UI Initial", 0, 0.0f);
            Initial(blackFadeOverlay, "Black Fade Left Overlay Initial", 0, 0.0f);
        }
    }

    #endregion

    public void FadeBlack(string type)
    {
        if (type == "In") Show(blackScreen, "Fade In Black", 0, 0.0f);
        if (type == "Out") Show(blackScreen, "Fade Out Black", 0, 0.0f);
    }

    public void WhiteFlash()
    {
        Show(whiteScreen, "White Flash", 0, 0.0f);
    }

    Texture ActivityTypeIcon(activityType type)
    {
        // To convert activity type to its display icon
        return type switch
        {
            activityType.RaceDestination => activityIconRace,
            activityType.RaceCircuit => activityIconRace,
            activityType.CollectionBattle => activityIconCollect,
            activityType.CarHunt => activityIconHunt,
            _ => new Texture2D(32, 32)
        };
    }

    string ActivityTypeName(activityType type)
    {
        // To convert activity type to its display name
        return type switch
        {
            activityType.RaceDestination => "Destination Race",
            activityType.RaceCircuit => "Circuit Race",
            activityType.CollectionBattle => "Collection Battle",
            activityType.CarHunt => "Car Hunt",
            _ => ""
        };
    }

    public int AnimNameToIndex(string name)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == name)
                return i; // The index
        }
        return -1; // It is not in the list
    }

    void CanvasGroupToggle(CanvasGroup canvasGroup, bool state)
    {
        int a = (state) ? 1 : 0;
        canvasGroup.alpha = a;
        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }
}
