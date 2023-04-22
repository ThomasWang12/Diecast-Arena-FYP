using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class HuntActivity : MonoBehaviour
{
    GameMaster master;
    PlayerNetwork network;
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
    float underSpeedTime;
    bool endCountdown = false;

    /* Tunables */
    int minSpeed = 20;
    int pointLimit = 15;
    int pointRedLight = 3;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        network = master.ManagerObject(Manager.type.network).GetComponent<PlayerNetwork>();
        input = master.ManagerObject(Manager.type.input).GetComponent<InputManager>();
        sound = master.ManagerObject(Manager.type.sound).GetComponent<SoundManager>();
        UI = master.ManagerObject(Manager.type.UI).GetComponent<UIManager>();
    }

    void Start()
    {
        activityIndex = master.ActivityObjectToIndex(gameObject);
        startPos = Methods.GetStartPosition(transform.Find("[Start Position]").gameObject, network.ownerPlayerId).transform.position;
        startRot = Methods.GetStartPosition(transform.Find("[Start Position]").gameObject, network.ownerPlayerId).transform.rotation;
    }

    void Update()
    {
        if (started && !finished)
        {
            float remainingTime = duration - (Time.time - startTime);
            UI.UpdateHuntUI(pointLimit - point, remainingTime);

            if (master.playerSpeed < minSpeed)
            {
                if (!endCountdown)
                {
                    underSpeedTime = Time.time + 5.0f;
                    UI.huntSpeedLimitTMP.enabled = true;
                    UI.ActivityCountdown5("Play");
                    sound.Play(Sound.name.Countdown5);
                    endCountdown = true;
                }
                else
                {
                    if (Time.time >= underSpeedTime)
                    {
                        // Car Hunt finished (Busted)
                        finished = true;
                        master.FinishActivity(activityIndex);
                        UI.ActivityCountdown5("Initial");
                        UI.ActivityCountdown("BUSTED");
                        UI.ResultHuntUI(activityIndex, point, false, 0, Time.time);
                        sound.Play(Sound.name.GameLose);

                        // In case it is during countdown when finishing
                        UI.ActivityCountdown5("Initial");
                        sound.Stop(Sound.name.Countdown5);
                    }
                }
            }
            else
            {
                UI.huntSpeedLimitTMP.enabled = false;
                UI.ActivityCountdown5("Initial");
                sound.Stop(Sound.name.Countdown5);
                endCountdown = false;
            }

            if (remainingTime <= 0)
            {
                // Car Hunt finished (Time's up)
                finished = true;
                master.FinishActivity(activityIndex);
                UI.ActivityCountdown5("Initial");
                UI.ActivityCountdown("FINISH");
                UI.ResultHuntUI(activityIndex, point, true, 0, Time.time);
                sound.Play(Sound.name.CheckpointBold);

                // In case it is during countdown when finishing
                UI.ActivityCountdown5("Initial");
                sound.Stop(Sound.name.Countdown5);
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
        UI.huntSpeedLimitTMP.enabled = true;
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
        UI.huntSpeedLimitTMP.enabled = false;
    }
}
