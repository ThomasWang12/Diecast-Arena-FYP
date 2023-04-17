using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameMaster;
using static TrafficLight;

public class RedLightCameraCol : MonoBehaviour
{
    GameMaster master;
    UIManager UI;
    SoundManager sound;

    TrafficLightControl control;

    public bool detecting = true;
    public bool isCollided = false;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        UI = master.ManagerObject(Manager.type.UI).GetComponent<UIManager>();
        sound = master.ManagerObject(Manager.type.sound).GetComponent<SoundManager>();

        control = transform.parent.GetComponent<TrafficLightControl>();
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) isCollided = false;

        detecting = (control.current == currentLight.Red || control.current == currentLight.RedYellow) ? true : false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!Methods.IsOwnedPlayer(other)) return;

        // Avoid repeated detection
        if (isCollided) return;

        if (detecting)
        {
            isCollided = true;
            UI.WhiteFlash();
            sound.Play(Sound.name.CameraShutter);

            if (master.activeActivityIndex > -1)
            {
                if (master.activityList[master.activeActivityIndex].type == activityType.CarHunt)
                    master.activityList[master.activeActivityIndex].mainObject.GetComponent<HuntActivity>().RecordPoint();
            }
        }
    }
}
