using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class HuntActivity : MonoBehaviour
{
    GameMaster master;
    InputManager input;
    SoundManager sound;
    UIManager UI;
    public int duration = 60; // seconds
    int activityIndex;
    Vector3 startPos;
    Quaternion startRot;
    //bool initialized = false;

    public enum playerRole { Initial, Target, Hunter };

    [Space(10)]

    public bool started = false;
    public bool finished = false;
    float startTime = 0;
    public int point = 0;
    bool endCountdown = false;

    /* Tunables */
    int pointLimit = 15;
    int pointRedLight = 3;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        input = master.ManagerObject(Manager.type.input).GetComponent<InputManager>();
        sound = master.ManagerObject(Manager.type.sound).GetComponent<SoundManager>();
        UI = master.ManagerObject(Manager.type.UI).GetComponent<UIManager>();
    }

    void Start()
    {
        activityIndex = master.ActivityObjectToIndex(gameObject);
        startPos = transform.Find("[Start Position]").position;
        startRot = transform.Find("[Start Position]").rotation;
    }

    void Update()
    {
        if (started && !finished)
        {
            float remainingTime = duration - (Time.time - startTime);
            UI.UpdateHuntUI(pointLimit - point, remainingTime);

            if (remainingTime <= 5 && !endCountdown)
            {
                UI.ActivityCountdown5("Play");
                sound.Play(Sound.name.Countdown5);
                endCountdown = true;
            }

            if (remainingTime <= 0)
            {
                // Car Hunt finished (Time's up)
                finished = true;
                master.FinishActivity(activityIndex);
                UI.ActivityCountdown5("Initial");
                UI.ActivityCountdown("TIME'S UP");
                UI.ResultHuntUI(activityIndex, point, false, 0, Time.time);
            }
        }
    }

    public void InitializeActivity()
    {
        master.TeleportPlayer(startPos + Common.spawnHeightOffset, startRot);
        UI.InfoCollectUI(point);
    }

    public void StartActivity()
    {
        if (started) return;

        started = true;
        startTime = Time.time;
        input.EnableDrive();
    }

    public void RecordPoint()
    {
        point += pointRedLight;

        if (point >= pointLimit)
        {
            // Collection Battle finished (No points remaining)
            finished = true;
            master.FinishActivity(activityIndex);
            float remainingTime = duration - (Time.time - startTime);
            UI.ResultHuntUI(activityIndex, point, false, remainingTime, Time.time);
            sound.Play(Sound.name.GameLose);

            // In case it is during countdown when finishing
            UI.ActivityCountdown5("Initial");
            sound.Stop(Sound.name.Countdown5);
        }
    }

    public void Reset()
    {
        //initialized = false;
        started = false;
        finished = false;
        startTime = 0;
        point = 0;
        endCountdown = false;
    }
}
