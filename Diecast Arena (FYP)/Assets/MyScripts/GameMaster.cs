using RVP;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    InputManager input;
    SoundManager sound;
    UIManager UI;

    [Header("Game State")]
    public gameState currentState;
    gameState prevState = gameState.Initial;
    string gameStateInfo;

    public enum gameState
    {
        Initial,
        Menu, Session, Session_StartingActivity,
        Activity_Loading, Activity_BeforeStart, Activity, Activity_Finished,
        Activity0_Loading, Activity0_BeforeStart, Activity0, Activity0_Finished,
        Activity1_Loading, Activity1_BeforeStart, Activity1, Activity1_Finished,
        Activity2_Loading, Activity2_BeforeStart, Activity2, Activity2_Finished,
        Activity3_Loading, Activity3_BeforeStart, Activity3, Activity3_Finished,
        Activity4_Loading, Activity4_BeforeStart, Activity4, Activity4_Finished,
    }

    [Header("Objects")]
    public GameObject player;
    public Camera cam;

    [Header("Variables")]
    public Vector3 playerPos;
    public Vector3 camPos;

    [Header("Activities")]
    public activity[] activityList;

    #region Activity

    public enum activityType { RaceDestination, RaceCircuit, CollectionBattle, CarHunt }
    [System.Serializable]
    public struct activity
    {
        public GameObject mainObject;
        [HideInInspector] public GameObject triggerObject;
        public activityType type;
        public string name;
        public bool available;
    }

    #endregion

    [HideInInspector] public bool ready = false;
    GearboxTransmission vehicleGearbox;

    /* Tunables */
    [HideInInspector] public float activityFinishWaitDuration = 3.0f;

    void Awake()
    {
        input = ManagerObject(Manager.type.input).GetComponent<InputManager>();
        sound = ManagerObject(Manager.type.sound).GetComponent<SoundManager>();
        UI = ManagerObject(Manager.type.UI).GetComponent<UIManager>();
    }

    // Centralize the method of getting managers in scripts
    public GameObject ManagerObject(Manager.type type)
    {
        if (type == Manager.type.input) return GameObject.Find("Input Manager");
        if (type == Manager.type.sound) return GameObject.Find("Sound Manager");
        if (type == Manager.type.UI) return GameObject.Find("UI Manager");
        return null;
    }

    void Start()
    {
        UpdateGameState(gameState.Menu);

        // Get the trigger object for each activity
        for (int i = 0; i < activityList.Length; i++)
        {
            GameObject triggerObject = Methods.GetChildContainsName(GameObject.Find("[Activity Triggers]"), activityList[i].mainObject.name);
            if (triggerObject != null && triggerObject.name.Contains("[Trigger]"))
            {
                activityList[i].triggerObject = triggerObject;
                triggerObject.GetComponent<ActivityTrigger>().activity = activityList[i].mainObject;
            }
        }

        InitializeSession();
    }

    public void GetReady()
    {
        player = GameObject.FindWithTag("Player");
        vehicleGearbox = player.transform.Find("chassis").transform.Find("transmission").GetComponent<GearboxTransmission>();
        cam = Camera.main;

        if (player != null)
        {
            ready = true;
            UpdateGameState(gameState.Session);
        }
    }

    public void InitializeSession()
    {
        // Enable the trigger objects of available activites
        for (int i = 0; i < activityList.Length; i++)
        {
            if (activityList[i].available)
            {
                activityList[i].triggerObject.SetActive(true);
                UI.ActivityTriggerCanvas(i, "Show");
            }
            else
            {
                activityList[i].triggerObject.SetActive(false);
            }
        }
    }

    public void CleanUpSession()
    {
        // Disable all trigger objects of activites
        foreach (var activity in activityList)
            activity.triggerObject.SetActive(false);
    }

    void Update()
    {
        if (!ready) return;
        OnGameStateChange(currentState);
        playerPos = player.transform.position;
        camPos = cam.transform.position;
    }

    public void EnterActivity(int i)
    {
        if (!activityList[i].available) return;

        StartCoroutine(ActivityStartSequence(i, activityList[i].type));
        activityList[i].triggerObject.SetActive(false);

        IEnumerator ActivityStartSequence(int index, activityType type)
        {
            #region Starting activity

            UpdateGameState(gameState.Session_StartingActivity);
            input.ForceBrake();
            UI.FadeBlack("Out");
            yield return new WaitForSeconds(UI.clips[UI.AnimNameToIndex("Black Fade")].length);

            #endregion

            #region Load Activity

            if (Enum.TryParse("Activity" + index + "_Loading", out gameState loading))
                UpdateGameState(loading);
            else UpdateGameState(gameState.Activity_Loading);

            CleanUpSession();
            vehicleGearbox.ShiftToGear(2);
            cam.GetComponent<CameraControl>().ToggleSmooth(false);
            UI.HideActivityInfo();

            if (type == activityType.RaceDestination || type == activityType.RaceCircuit)
                activityList[index].mainObject.GetComponent<RaceActivity>().InitializeActivity();

            if (type == activityType.CollectionBattle)
                activityList[index].mainObject.GetComponent<CollectActivity>().InitializeActivity();

            #endregion

            #region Activity (Before Start)

            if (Enum.TryParse("Activity" + index + "_BeforeStart", out gameState beforeStart))
                UpdateGameState(beforeStart);
            else UpdateGameState(gameState.Activity_BeforeStart);

            input.ForceBrake();
            UI.FadeBlack("In");
            yield return new WaitForSeconds(UI.clips[UI.AnimNameToIndex("Black Fade")].length);

            UI.ActivityCountdown321("Play");
            sound.Play("Start Countdown");
            yield return new WaitForSeconds(UI.clips[UI.AnimNameToIndex("Activity Countdown 321")].length);
            UI.ActivityCountdown321("Initial");

            #endregion

            #region Activity Start

            if (Enum.TryParse("Activity" + index, out gameState activity))
                UpdateGameState(activity);
            else UpdateGameState(gameState.Activity);

            cam.GetComponent<CameraControl>().ToggleSmooth(true);
            UI.ActivityCountdown("START");
            UI.ActivityUI(type, "Show");

            if (type == activityType.RaceDestination || type == activityType.RaceCircuit)
                activityList[index].mainObject.GetComponent<RaceActivity>().StartActivity();

            if (type == activityType.CollectionBattle)
                activityList[index].mainObject.GetComponent<CollectActivity>().StartActivity();

            #endregion
        }
    }

    public void FinishActivity(int i)
    {
        StartCoroutine(ActivityFinishSequence(i, activityList[i].type));

        IEnumerator ActivityFinishSequence(int index, activityType type)
        {
            #region Activity Finish

            if (Enum.TryParse("Activity" + index + "_Finished", out gameState state))
                UpdateGameState(state);
            else UpdateGameState(gameState.Activity_Finished);

            UI.ActivityUI(type, "Initial");
            UI.ActivityResultUI(type, "Show");
            yield return new WaitForSeconds(activityFinishWaitDuration);

            #endregion

            #region Return To Session

            UI.returnSessionTMP.enabled = false;
            UI.FadeBlack("Out");
            yield return new WaitForSeconds(UI.clips[UI.AnimNameToIndex("Black Fade")].length);

            if (type == activityType.RaceDestination || type == activityType.RaceCircuit)
                activityList[index].mainObject.GetComponent<RaceActivity>().Reset();

            if (type == activityType.CollectionBattle)
                activityList[index].mainObject.GetComponent<CollectActivity>().Reset();

            UI.ActivityResultUI(type, "Initial");
            UpdateGameState(gameState.Session);
            UI.FadeBlack("In");

            #endregion
        }
    }

    public void TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        StartCoroutine(player.GetComponent<VehicleDebug>().SetPosition(position, rotation));
    }

    public void UpdateGameState(gameState state)
    {
        currentState = state;
        if (currentState == gameState.Session) InitializeSession();
    }

    void OnGameStateChange(gameState state)
    {
        if (prevState != state)
        {
            if (Dev.log_gameState)
                Debug.Log("Game State: " + state + " | Display: " + GameStateInfo(state));

            UI.DisplayGameSate(GameStateInfo(state));
            prevState = state;
        }
    }

    string GameStateInfo(gameState state)
    {
        // To convert game states to display text for readability
        return gameStateInfo = state switch
        {
            gameState.Menu => "Menu",
            gameState.Session => "Session",
            gameState.Session_StartingActivity => "Session (Starting Activity)",

            gameState.Activity0_Loading => "Loading " + activityList[0].name,
            gameState.Activity0_BeforeStart => activityList[0].name + " (Before Start)",
            gameState.Activity0 => activityList[0].name,
            gameState.Activity0_Finished => activityList[0].name + " (Finished)",

            gameState.Activity1_Loading => "Loading " + activityList[1].name,
            gameState.Activity1_BeforeStart => activityList[1].name + " (Before Start)",
            gameState.Activity1 => activityList[1].name,
            gameState.Activity1_Finished => activityList[1].name + " (Finished)",

            gameState.Activity2_Loading => "Loading " + activityList[2].name,
            gameState.Activity2_BeforeStart => activityList[2].name + " (Before Start)",
            gameState.Activity2 => activityList[2].name,
            gameState.Activity2_Finished => activityList[2].name + " (Finished)",

            gameState.Activity3_Loading => "Loading " + activityList[3].name,
            gameState.Activity3_BeforeStart => activityList[3].name + " (Before Start)",
            gameState.Activity3 => activityList[3].name,
            gameState.Activity3_Finished => activityList[3].name + " (Finished)",

            gameState.Activity4_Loading => "Loading " + activityList[4].name,
            gameState.Activity4_BeforeStart => activityList[4].name + " (Before Start)",
            gameState.Activity4 => activityList[4].name,
            gameState.Activity4_Finished => activityList[4].name + " (Finished)",

            _ => state.ToString()
        };
    }

    public int ActivityObjectToIndex(GameObject activityObject)
    {
        for (int i = 0; i < activityList.Length; i++)
        {
            if (activityList[i].mainObject == activityObject)
                return i; // The index
        }
        return -1; // It is not in the list
    }
}
