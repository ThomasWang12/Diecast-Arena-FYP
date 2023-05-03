using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameMaster;

public class ActivityTrigger : MonoBehaviour
{
    GameMaster master;
    PlayerNetwork network;
    InputManager input;
    UIManager UI;
    ActivityOption activityOption;
    [HideInInspector] public GameObject activity;
    Material triggerMat;
    Canvas canvas;

    int activityIndex;
    float colliderRadius;
    public bool inTrigger = false;
    float alphaMax;

    /* Tunables */
    float fadeRange = 4f;
    float fadeDistPadding = 4f;
    float alphaMin = 0.25f;

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
        network = master.network;
        input = master.input;
        UI = master.UI;
        activityOption = transform.parent.GetComponent<ActivityOption>();
        triggerMat = GetComponent<MeshRenderer>().material;
        canvas = Methods.GetChildContainsName(gameObject, "[Canvas]").GetComponent<Canvas>();
    }

    void Start()
    {
        if (activity != null) activityIndex = master.ActivityObjectToIndex(activity);
        colliderRadius = transform.localScale.x / 2;
        alphaMax = triggerMat.GetFloat("_AlphaA");
    }

    void Update()
    {
        // Canvas keeps facing towards the camera (rotate y-axis)
        Vector3 rotateY = new Vector3(master.camPos.x, canvas.transform.position.y, master.camPos.z);
        canvas.transform.LookAt(rotateY);

        // When player is getting closer to the checkpoint, alpha fades to less (for visibility)
        float dist = Vector3.Distance(master.playerPos, transform.position) - colliderRadius - fadeDistPadding;
        float distClamped = Mathf.Clamp(dist, 0, fadeRange);
        float fadeMat = Methods.Map(distClamped, 0, fadeRange, alphaMin, alphaMax);
        triggerMat.SetFloat("_AlphaA", fadeMat);

        // Player input to start this activity
        if (inTrigger && input.EnterActivity())
        {
            inTrigger = false;

            int currentOption = 0;
            if (master.activityList[activityIndex].type == activityType.RaceCircuit)
                currentOption = activityOption.currentRaceLap;
            if (master.activityList[activityIndex].type == activityType.CollectionBattle)
                currentOption = activityOption.currentCollectDuration;
            if (master.activityList[activityIndex].type == activityType.CarHunt)
                currentOption = activityOption.currentHuntDuration;

            if (network.localPlay)
            {
                activityOption.ApplyOptions(activityIndex, currentOption);
                master.EnterActivity(activityIndex);
            }
            else network.EnterActivityServerRpc(activityIndex, currentOption);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Methods.IsOwnedPlayer(other)) return;
        if (!master.activityList[activityIndex].available) return;
        inTrigger = true;

        // UI
        UI.ShowActivityInfoAnim(activityIndex);
        UI.ActivityTriggerCanvas(activityIndex, "Hide");
    }

    void OnTriggerStay(Collider other)
    {
        if (!Methods.IsOwnedPlayer(other)) return;
        if (!master.activityList[activityIndex].available) return;
        inTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!Methods.IsOwnedPlayer(other)) return;
        if (!master.activityList[activityIndex].available) return;
        inTrigger = false;

        if (master.currentState == gameState.Session)
        {
            // UI
            UI.ActivityTriggerCanvas(activityIndex, "Show");
            // Check if player is in any activity trigger
            bool inAnyTrigger = false;
            foreach (var activity in master.activityList) {
                if (activity.triggerObject.GetComponent<ActivityTrigger>().inTrigger)
                    inAnyTrigger = true;
            }
            if (!inAnyTrigger) UI.HideActivityInfoAnim(activityIndex);
        }
    }
}
