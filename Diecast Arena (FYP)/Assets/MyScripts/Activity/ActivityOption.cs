using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameMaster;

public class ActivityOption : MonoBehaviour
{
    GameMaster master;
    InputManager input;
    UIManager UI;

    ActivityTrigger[] activityTriggers;

    [HideInInspector] public int[] raceLaps = { 1, 2, 3, 4, 5 };
    [HideInInspector] public float[] collectDurations = { 0.5f, 1, 1.5f, 2, 3, 4, 5 };
    [HideInInspector] public float[] huntDurations = { 0.5f, 1, 1.5f, 2, 3, 4, 5 };

    [HideInInspector] public int currentRaceLap = 1;
    [HideInInspector] public int currentCollectDuration = 3;
    [HideInInspector] public int currentHuntDuration = 3;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        input = master.ManagerObject(Manager.type.input).GetComponent<InputManager>();
        UI = master.ManagerObject(Manager.type.UI).GetComponent<UIManager>();
    }

    void Start()
    {
        activityTriggers = new ActivityTrigger[master.activityList.Length];
        for (int i = 0; i < activityTriggers.Length; i++)
            activityTriggers[i] = master.activityList[i].triggerObject.GetComponent<ActivityTrigger>();
    }

    void Update()
    {
        int inTrigger = -1;

        for (int i = 0; i < activityTriggers.Length; i++)
        {
            if (master.activityList[i].available)
            {
                if (activityTriggers[i].inTrigger)
                    inTrigger = i;
            }
        }

        if (inTrigger > -1)
        {
            // Race Circuit
            if (master.activityList[inTrigger].type == activityType.RaceCircuit)
            {
                if (Input.GetKeyDown(KeyCode.Minus) || input.GamepadLeftButton())
                {
                    currentRaceLap--;
                }
                if (Input.GetKeyDown(KeyCode.Equals) || input.GamepadRightButton())
                {
                    currentRaceLap++;
                }
                currentRaceLap = Mathf.Clamp(currentRaceLap, 0, raceLaps.Length - 1);
            }

            // Collection Battle
            if (master.activityList[inTrigger].type == activityType.CollectionBattle)
            {
                if (Input.GetKeyDown(KeyCode.Minus) || input.GamepadLeftButton())
                {
                    currentCollectDuration--;
                }
                if (Input.GetKeyDown(KeyCode.Equals) || input.GamepadRightButton())
                {
                    currentCollectDuration++;
                }
                currentCollectDuration = Mathf.Clamp(currentCollectDuration, 0, collectDurations.Length - 1);
            }

            // Car Hunt
            if (master.activityList[inTrigger].type == activityType.CarHunt)
            {
                if (Input.GetKeyDown(KeyCode.Minus) || input.GamepadLeftButton())
                {
                    currentHuntDuration--;
                }
                if (Input.GetKeyDown(KeyCode.Equals) || input.GamepadRightButton())
                {
                    currentHuntDuration++;
                }
                currentHuntDuration = Mathf.Clamp(currentHuntDuration, 0, huntDurations.Length - 1);
            }

            UI.UpdateActivityOptions(inTrigger);
        }
    }

    public void ApplyOptionsLocal(int activityIndex)
    {
        GameObject activityObject = master.activityList[activityIndex].mainObject;

        // Race Circuit
        if (master.activityList[activityIndex].type == activityType.RaceCircuit)
        {
            activityObject.GetComponent<RaceActivity>().totalLap = raceLaps[currentRaceLap];
        }

        // Collection Battle
        if (master.activityList[activityIndex].type == activityType.CollectionBattle)
        {
            activityObject.GetComponent<CollectActivity>().duration = Mathf.RoundToInt(collectDurations[currentCollectDuration] * 60);
        }

        // Car Hunt
        if (master.activityList[activityIndex].type == activityType.CarHunt)
        {
            activityObject.GetComponent<HuntActivity>().duration = Mathf.RoundToInt(huntDurations[currentHuntDuration] * 60);
        }
    }

    public void ApplyOptions(int activityIndex, int launcherOption)
    {
        GameObject activityObject = master.activityList[activityIndex].mainObject;

        // Race Circuit
        if (master.activityList[activityIndex].type == activityType.RaceCircuit)
        {
            activityObject.GetComponent<RaceActivity>().totalLap = raceLaps[launcherOption];
        }

        // Collection Battle
        if (master.activityList[activityIndex].type == activityType.CollectionBattle)
        {
            activityObject.GetComponent<CollectActivity>().duration = Mathf.RoundToInt(collectDurations[launcherOption] * 60);
        }

        // Car Hunt
        if (master.activityList[activityIndex].type == activityType.CarHunt)
        {
            activityObject.GetComponent<HuntActivity>().duration = Mathf.RoundToInt(huntDurations[launcherOption] * 60);
        }
    }
}
