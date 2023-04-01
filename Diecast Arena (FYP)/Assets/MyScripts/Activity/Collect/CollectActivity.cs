using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectActivity : MonoBehaviour
{
    GameMaster master;
    InputManager input;
    SoundManager sound;
    UIManager UI;
    [HideInInspector] public List<GameObject> checkpoints;
    Vector3 startPos;
    Quaternion startRot;

    public bool started = false;
    public bool finished = false;
    float startTime = 0;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        input = master.ManagerObject(Manager.type.input).GetComponent<InputManager>();
        sound = master.ManagerObject(Manager.type.sound).GetComponent<SoundManager>();
        UI = master.ManagerObject(Manager.type.UI).GetComponent<UIManager>();
    }

    void Start()
    {
        startPos = transform.Find("[Start Position]").position;
        startRot = transform.Find("[Start Position]").rotation;
    }

    void Update()
    {
        
    }

    public void InitializeActivity()
    {
        master.TeleportPlayer(startPos + Common.spawnHeightOffset, startRot);
    }

    public void StartActivity()
    {
        if (started) return;

        started = true;
        startTime = Time.time;
        input.EnableDrive();
    }

    public void CheckpointReached(int index)
    {
        sound.Play("Checkpoint");
        Debug.Log("Checkpoint " + index + " reached.");
    }

        public void Reset()
    {
        started = false;
        finished = false;
        startTime = 0;
    }
}
