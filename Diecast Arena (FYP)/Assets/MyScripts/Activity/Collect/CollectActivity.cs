using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static GameMaster;
using Random = UnityEngine.Random;

public class CollectActivity : MonoBehaviour
{
    GameMaster master;
    PlayerNetwork network;
    InputManager input;
    SoundManager sound;
    UIManager UI;
    [HideInInspector] public CollectCheckpointStar starScript;
    [HideInInspector] public CollectCheckpointDiamond diamondScript;
    public int duration = 60; // seconds
    [Tooltip("All checkpoints found inside the [Checkpoints] object.")]
    [SerializeField][HideInInspector] List<GameObject> checkpointsAll;
    [Tooltip("0 is regarded as include all checkpoints.")]
    public int totalCheckpoint = 0;
    public int totalDiamond = 0;
    [Tooltip("Checkpoints that are used in the current activity. (Randomly selected from 'checkpointsAll' list)")]
    public List<GameObject> checkpoints;
    [Tooltip("Checkpoints that are diamonds. (Randomly selected from 'checkpoints' list)")]
    public List<GameObject> diamonds;
    CollectCheckpointCol[] checkpointCols;
    int activityIndex;
    Vector3 startPos;
    Quaternion startRot;
    bool initialized = false;

    public enum checkpointType { Initial, Star, Diamond };

    [Space(10)]

    public bool started = false;
    public bool finished = false;
    float startTime = 0;
    public int collectedCheckpoint = 0;
    public int score = 0;
    bool endCountdown = false;

    /* Tunables */
    int pointStar = 1;
    int pointDiamond = 3;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        network = master.ManagerObject(Manager.type.network).GetComponent<PlayerNetwork>();
        input = master.ManagerObject(Manager.type.input).GetComponent<InputManager>();
        sound = master.ManagerObject(Manager.type.sound).GetComponent<SoundManager>();
        UI = master.ManagerObject(Manager.type.UI).GetComponent<UIManager>();

        starScript = transform.Find("[Stars]").GetComponent<CollectCheckpointStar>();
        diamondScript = transform.Find("[Diamonds]").GetComponent<CollectCheckpointDiamond>();
    }

    void Start()
    {
        activityIndex = master.ActivityObjectToIndex(gameObject);
        startPos = Methods.GetStartPosition(transform.Find("[Start Position]").gameObject, network.ownerPlayerId).transform.position;
        startRot = Methods.GetStartPosition(transform.Find("[Start Position]").gameObject, network.ownerPlayerId).transform.rotation;
        InitializeCheckpoints();
        totalCheckpoint = Mathf.Clamp(totalCheckpoint, 0, checkpointsAll.Count);
        totalDiamond = Mathf.Clamp(totalDiamond, 0, totalCheckpoint);
    }

    void Update()
    {
        if (started && !finished)
        {
            float remainingTime = duration - (Time.time - startTime);
            UI.UpdateCollectUI(score, totalCheckpoint - collectedCheckpoint, remainingTime);

            if (remainingTime <= 5 && !endCountdown)
            {
                UI.ActivityCountdown5("Play");
                sound.Play(Sound.name.Countdown5);
                endCountdown = true;
            }

            if (remainingTime <= 0)
            {
                // Collection Battle finished (Time's up)
                finished = true;
                SetAllCheckpoints(false);
                master.FinishActivity(activityIndex);
                UI.ActivityCountdown5("Initial");
                UI.ActivityCountdown("TIME'S UP");
                UI.ResultCollectUI(activityIndex, score, false, Time.time);
            }
        }
    }

    void InitializeCheckpoints()
    {
        // Store the checkpoints inside 'Checkpoints' object into 'checkpoints' list
        Methods.GetChildRecursive(transform.Find("[Checkpoints]").gameObject, checkpointsAll, "Checkpoint");
        checkpointCols = new CollectCheckpointCol[checkpointsAll.Count];

        // For each checkpoint
        for (int i = 0; i < checkpointsAll.Count; i++)
        {
            GameObject col = Methods.GetChildContainsName(checkpointsAll[i].gameObject, "[Col]");
            checkpointCols[i] = col.GetComponent<CollectCheckpointCol>();
            checkpointsAll[i].SetActive(false);
        }

        totalCheckpoint = Mathf.Clamp(totalCheckpoint, 0, checkpointsAll.Count);
        totalDiamond = Mathf.Clamp(totalDiamond, 0, totalCheckpoint);
        initialized = true;
    }

    void SelectCheckpoints()
    {
        if (totalCheckpoint == 0) totalCheckpoint = checkpointsAll.Count;

        List<int> selectedList = new List<int>();
        int forceExcludes = 0;
        List<int> randomQueue = new List<int>(); // Indexes that is available to join the random selection
        List<int> randomList = new List<int>(); ; // Indexes that are randomly drawn from 'randomQueue' list

        int manualStars = 0;
        int manualDiamonds = 0;
        List<int> randomDiamondQueue = new List<int>(); // Indexes that is available to join the random diamond selection

        // For each checkpoint
        for (int i = 0; i < checkpointsAll.Count; i++)
        {
            if (checkpointCols[i].forceExclude) forceExcludes++;
            else
            {
                if (checkpointCols[i].forceInclude) selectedList.Add(i);
                else randomQueue.Add(i);
            }
        }

        totalCheckpoint = Mathf.Clamp(totalCheckpoint, selectedList.Count, checkpointsAll.Count - forceExcludes);

        // Select random checkpoints
        int randomCount = totalCheckpoint - selectedList.Count;
        if (randomCount > 0)
        {
            int[] randomIndexes = Methods.RandomIntArray(randomCount, 0, randomQueue.Count);
            foreach (var i in randomIndexes) randomList.Add(randomQueue[i]);
            selectedList.AddRange(randomList);
        }

        selectedList.Sort();
        foreach (var i in selectedList)
        {
            checkpoints.Add(checkpointsAll[i].gameObject);
            checkpointCols[i].index = checkpoints.Count - 1;
        }

        foreach (var i in selectedList)
        {
            switch (checkpointCols[i].type)
            {
                // Checkpoint type is manually set to 'Star' in inspector
                case checkpointType.Star:
                    manualStars++;
                    checkpointCols[i].InitializeByType(checkpointType.Star);
                    checkpointCols[i].revertStar = true;
                    checkpointCols[i].revertDiamond = false;
                    break;

                // Checkpoint type is manually set to 'Diamond' in inspector
                case checkpointType.Diamond:
                    manualDiamonds++;
                    diamonds.Add(checkpoints[i]);
                    checkpointCols[i].InitializeByType(checkpointType.Diamond);
                    checkpointCols[i].revertStar = false;
                    checkpointCols[i].revertDiamond = true;
                    break;

                // Checkpoint is not manually set to any type (i.e. Initial)
                default:
                    randomDiamondQueue.Add(i);
                    checkpointCols[i].revertStar = false;
                    checkpointCols[i].revertDiamond = false;
                    break;
            }
        }

        totalDiamond = Mathf.Clamp(totalDiamond, manualDiamonds, totalCheckpoint - manualStars);

        // Select random diamonds
        int randomDiamondCount = totalDiamond - manualDiamonds;
        if (randomDiamondCount > 0)
        {
            int[] randomIndexes = Methods.RandomIntArray(randomDiamondCount, 0, randomDiamondQueue.Count);

            // For each index in the queue, check if randomIndexes contains this index
            for (int i = 0; i < randomDiamondQueue.Count; i++)
            {
                if (randomIndexes.Contains(i))
                {
                    diamonds.Add(checkpointsAll[randomDiamondQueue[i]].gameObject);
                    checkpointCols[randomDiamondQueue[i]].InitializeByType(checkpointType.Diamond);
                }
                else checkpointCols[randomDiamondQueue[i]].InitializeByType(checkpointType.Star);
            }
        }
        else
        {
            // Set type to 'Star' for the remaining selected checkpoints
            foreach (var i in randomDiamondQueue)
                checkpointCols[i].InitializeByType(checkpointType.Star);
        }
    }

    void SetAllCheckpoints(bool state)
    {
        foreach (var checkpoint in checkpoints) checkpoint.SetActive(state);
        foreach (var star in starScript.stars) star.SetActive(state);
        foreach (var diamond in diamondScript.diamonds) diamond.SetActive(state);
    }

    public void InitializeActivity()
    {
        if (!initialized) InitializeCheckpoints();
        SelectCheckpoints();
        master.TeleportPlayer(startPos + Common.spawnHeightOffset, startRot);
        UI.InfoCollectUI(totalCheckpoint);
    }

    public void StartActivity()
    {
        if (started) return;

        started = true;
        startTime = Time.time;
        input.EnableDrive();
        SetAllCheckpoints(true);
    }

    public void CheckpointReached(int index, checkpointType type)
    {
        collectedCheckpoint++;

        if (type == checkpointType.Star) score += pointStar;
        if (type == checkpointType.Diamond) score += pointDiamond;

        if (collectedCheckpoint < totalCheckpoint)
        {
            if (type == checkpointType.Star) sound.Play(Sound.name.Checkpoint);
            if (type == checkpointType.Diamond) sound.Play(Sound.name.CheckpointBold);
        }
        else
        {
            // Collection Battle finished (Cleared)
            finished = true;
            SetAllCheckpoints(false);
            master.FinishActivity(activityIndex);
            UI.ResultCollectUI(activityIndex, score, true, Time.time);
            sound.Play(Sound.name.CheckpointBold);

            // In case it is during countdown when finishing
            UI.ActivityCountdown5("Initial");
            sound.Stop(Sound.name.Countdown5);
        }
    }

    public void Reset()
    {
        initialized = false;
        started = false;
        finished = false;
        startTime = 0;
        collectedCheckpoint = 0;
        score = 0;
        endCountdown = false;

        foreach (var col in checkpointCols) col.Reset();
        checkpointsAll.Clear();
        checkpointCols = null;
        checkpoints.Clear();
        diamonds.Clear();
        starScript.stars.Clear();
        diamondScript.diamonds.Clear();
        foreach (Transform child in starScript.transform) Destroy(child.gameObject);
        foreach (Transform child in diamondScript.transform) Destroy(child.gameObject);
    }
}
