using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static GameMaster;
using static InputManager;

public class UIManager : MonoBehaviour
{
    GameMaster master;
    InputManager input;

    [SerializeField] GameObject canvas;
    [SerializeField] GameObject screen;
    [SerializeField] GameObject gameUI;

    [Header("Screen")]
    public bool toogleHUD = true;
    [SerializeField] RawImage blackScreen;

    [Header("Game")]
    [SerializeField] TMP_Text gameSateTMP;

    [Header("Activity Trigger")]
    [SerializeField] GameObject activityTriggerTypeIcon;
    [SerializeField] GameObject activityTriggerTypeText;
    [SerializeField] GameObject activityName;
    [SerializeField] GameObject activityPressStart;
    [SerializeField] GameObject blackFadeOverlay;

    [SerializeField] List<GameObject> activityTriggerCanvas;

    [Header("Activity")]
    [SerializeField] GameObject activityCountdown10;
    [SerializeField] GameObject activityCountdown321;
    [SerializeField] GameObject activityCountdownZero;
    [SerializeField] GameObject activityProgress;
    [SerializeField] TMP_Text countTMP;
    [SerializeField] TMP_Text checkpointTMP;
    [SerializeField] TMP_Text timeTMP;

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
    string prompt_startActivity;
    TextMeshProUGUI activityPressStartTMP;

    /* Race UI */
    int race_totalLap;
    int race_totalCheckpoint;

    /* Collect UI */
    int collect_totalCheckpoint;

    float returnSessionTime;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        input = master.ManagerObject(Manager.type.input).GetComponent<InputManager>();

        activityPressStartTMP = activityPressStart.gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        ChangeButtonType(inputType.MouseKeyboard);
        AnimationsInitial();
    }

    void AnimationsInitial()
    {
        // Screen
        Initial(screen, "Fade Black Initial", 0, 1.0f);
        // Game UI
        HideActivityInfo();
        foreach (var canvas in activityTriggerCanvas)
        {
            if (canvas.transform.parent.gameObject.activeSelf)
                Initial(canvas, "Activity Trigger Canvas Initial", 0, 1.0f);
        }
        Initial(activityCountdown10, "Activity Countdown 10 Initial", 0, 0.0f);
        Initial(activityCountdown321, "Activity Countdown 321 Initial", 0, 0.0f);
        Initial(activityCountdownZero, "Activity Countdown Start Initial", 0, 1.0f);
        Initial(activityProgress, "Activity UI Initial", 0, 0.0f);
        Initial(activityResult, "Activity UI Initial", 0, 0.0f);
    }

    void Update()
    {
        if (input.ToggleHUD()) ToggleHUD();

        UpdatePromptText();

        if (returnSessionTMP.isActiveAndEnabled)
        {
            int remaining = Mathf.FloorToInt(returnSessionTime + master.activityFinishWaitDuration - Time.time);
            returnSessionTMP.text = "Return to session (0:0" + remaining + ")";
        }
    }

    void ToggleHUD()
    {
        toogleHUD = !toogleHUD;
        int alpha = toogleHUD ? 1 : 0;
        canvas.GetComponent<CanvasGroup>().alpha = alpha;
    }

    public void DisplayGameSate(string state)
    {
        gameSateTMP.text = state;
    }

    public void ChangeButtonType(inputType type)
    {
        if (type == inputType.MouseKeyboard)
        {
            prompt_startActivity = "E";
        }

        if (type == inputType.Gamepad)
        {
            prompt_startActivity = ControllerFont('D');
        }
    }

    string ControllerFont(char key)
    {
        string opening = "<font=\"XboxOne-Controller SDF\">";
        string closing = "</font>";
        return opening + key + closing;
    }

    void UpdatePromptText()
    {
        activityPressStartTMP.text = "Press " + prompt_startActivity + " to start";
    }

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
        Show(blackFadeOverlay, "Black Fade Left Overlay In", 0, 0.0f);
    }

    public void HideActivityInfoAnim(int index)
    {
        Hide(activityTriggerTypeIcon, "Activity Type Icon Out", 0, 0.0f);
        Hide(activityTriggerTypeText, "Activity Type Text Out", 0, 0.0f);
        Hide(activityName, "Activity Name Out", 0, 0.0f);
        Hide(activityPressStart, "Activity Press Start Out", 0, 0.0f);
        Hide(blackFadeOverlay, "Black Fade Left Overlay Out", 0, 0.0f);
    }

    public void HideActivityInfo()
    {
        Initial(activityTriggerTypeIcon, "Activity Type Icon Initial", 0, 0.0f);
        Initial(activityTriggerTypeText, "Activity Type Text Initial", 0, 0.0f);
        Initial(activityName, "Activity Name Initial", 0, 0.0f);
        Initial(activityPressStart, "Activity Press Start Initial", 0, 0.0f);
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
        if (activityType == activityType.RaceDestination || activityType == activityType.RaceCircuit)
        {
            if (type == "Show") Show(activityProgress, "Activity UI Show", 0, 0.0f);
            if (type == "Hide") Hide(activityProgress, "Activity UI Hide", 0, 0.0f);
            if (type == "Initial") Initial(activityProgress, "Activity UI Initial", 0, 0.0f);
        }
    }

    // Info (Activity) UI:
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

    // Update (Activity) UI:
    // Variables that are continuously updated during the activity.

    public void UpdateRaceDestinationUI(int currentCheckpoint, float raceTime)
    {
        countTMP.text = currentCheckpoint + " / " + race_totalCheckpoint;
        checkpointTMP.text = "Checkpoint";
        timeTMP.text = Methods.TimeFormat(raceTime);
    }

    public void UpdateRaceCircuitUI(int currentLap, int currentCheckpoint, float raceTime)
    {
        countTMP.text = currentLap + " / " + race_totalLap + " <size=\"40\">Lap</size>";
        checkpointTMP.text = "Checkpoint " + currentCheckpoint + " / " + race_totalCheckpoint;
        timeTMP.text = Methods.TimeFormat(raceTime);
    }
    
    public void UpdateCollectUI(int collectedCount, float remainingTime)
    {
        countTMP.text = collectedCount + " / " + collect_totalCheckpoint;
        checkpointTMP.text = "Collected";
        timeTMP.text = "Time left: " + Methods.TimeFormat(remainingTime);
    }

    // Result (Activity) UI:
    // Variables that are recorded upon completion of the activity.

    public void ResultRaceUI(int index, float raceTime, float currentTime)
    {
        activityNameTMP.text = master.activityList[index].name;
        playerResultTMP.text = Methods.TimeFormat(raceTime) + " | Player";
        returnSessionTime = currentTime;
        returnSessionTMP.enabled = true;
    }

    public void ResultCollectUI()
    {
        // ...
        returnSessionTMP.enabled = true;
    }

    public void ActivityResultUI(activityType activityType, string type)
    {
        if (activityType == activityType.RaceDestination || activityType == activityType.RaceCircuit)
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
    }

    public void FadeBlack(string type)
    {
        if (type == "In") Show(screen, "Fade In Black", 0, 0.0f);
        if (type == "Out") Show(screen, "Fade Out Black", 0, 0.0f);
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
}
