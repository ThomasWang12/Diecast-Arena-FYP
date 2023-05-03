using System.Collections;
using System.Diagnostics;
using System.Drawing;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
using static CollectActivity;
using DG.Tweening;
using System.Collections.Generic;

public class CollectCheckpointCol : MonoBehaviour
{
    GameMaster master;
    PlayerNetwork network;
    Common common;
    CollectActivity collect;

    GameObject checkpoint;
    Material checkpointVisualMat;
    GameObject star;
    Material starMat;
    Color starBaseColor, starEmissionColor;
    GameObject diamond;
    Material diamondMat;
    Color diamondBaseColor, diamondEmissionColor;
    GameObject plus;
    Material plusMat;
    Color plusBaseColor, plusEmissionColor;

    [HideInInspector] public int index = -1;
    float colliderRadius;

    [Tooltip("Manually adjust the spawn height of arrow/flag for this checkpoint.")]
    [Range(-10f, 10f)][SerializeField] float heightAdjust = 0;
    [Tooltip("If both forceInclude and forceExclude are checked, it is regarded as excluded.")]
    public bool forceExclude = false;
    [Tooltip("Mark this to be included in the selected checkpoints.")]
    public bool forceInclude = false;
    public checkpointType type = checkpointType.Initial;

    [Space(10)]
    [SerializeField] bool isCollided = false;

    float collideTime = 0;
    [HideInInspector] public bool revertStar = false;
    // When reset after activity, set its type as 'Star' because it was manually set as 'Star' before activity start
    [HideInInspector] public bool revertDiamond = false;
    // When reset after activity, set its type as 'Diamond' because it was manually set as 'Diamond' before activity start

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
        network = master.network;
        common = master.common;
        collect = Methods.FindParentWithTagRecursive(gameObject, "Activity").GetComponent<CollectActivity>();
        checkpoint = transform.parent.gameObject;
        checkpointVisualMat = Methods.GetChildContainsName(checkpoint, "[Visual]").GetComponent<MeshRenderer>().material;
    }

    void Start()
    {
        colliderRadius = transform.parent.localScale.x / 2;
    }

    void Update()
    {
        if (collect.started)
        {
            // Objects keep facing towards the camera (rotate y-axis)
            if (type == checkpointType.Star)
            {
                Vector3 starRotateY = new Vector3(master.camPos.x, star.transform.position.y, master.camPos.z);
                star.transform.LookAt(starRotateY);
                star.transform.Rotate(0, 90, 0);
            }
            if (type == checkpointType.Diamond)
            {
                Vector3 diamondRotateY = new Vector3(master.camPos.x, diamond.transform.position.y, master.camPos.z);
                diamond.transform.LookAt(diamondRotateY);
                diamond.transform.Rotate(0, 90, 0);
                Vector3 plusRotateY = new Vector3(master.camPos.x, plus.transform.position.y, master.camPos.z);
                plus.transform.LookAt(plusRotateY);
                plus.transform.Rotate(0, 90, 0);
            }

            if (isCollided == false)
            {
                // When player is getting closer to the checkpoint, alpha fades to less (for visibility)
                float alpha = CheckpointCol.FadeCloserValue(transform.position, colliderRadius);
                checkpointVisualMat.SetFloat("_AlphaA", alpha);
            }
            else
            {
                // Checkpoint reached, mesh turns white and fades out
                checkpointVisualMat.SetColor("_Color", Color.white);
                float fade = CheckpointCol.FadeCollidedValue(collideTime);
                checkpointVisualMat.SetFloat("_AlphaA", fade);
                CheckpointCol.FadeCollided(star, starMat, fade);
                CheckpointCol.FadeCollided(diamond, diamondMat, fade);
                CheckpointCol.FadeCollided(plus, plusMat, fade);
            }
        }
    }

    public void InitializeByType(checkpointType checkpointType)
    {
        type = checkpointType;

        if (type == checkpointType.Star)
        {
            checkpointVisualMat.SetColor("_Color", common.collectCheckpoint);

            star = collect.starScript.SpawnStar(index, heightAdjust);
            starMat = star.GetComponent<MeshRenderer>().material;
            starBaseColor = starMat.GetColor("_BaseColor");
            starEmissionColor = starMat.GetColor("_EmissionColor");
        }

        if (type == checkpointType.Diamond)
        {
            checkpointVisualMat.SetColor("_Color", common.collectCheckpointDiamond);
            GameObject diamondParent = collect.diamondScript.SpawnDiamond(index, heightAdjust);

            diamond = Methods.GetChildContainsName(diamondParent, "[Diamond]");
            diamondMat = diamond.GetComponent<MeshRenderer>().material;
            diamondBaseColor = diamondMat.GetColor("_BaseColor");
            diamondEmissionColor = diamondMat.GetColor("_EmissionColor");

            plus = Methods.GetChildContainsName(diamondParent, "[Plus]");
            plusMat = plus.GetComponent<MeshRenderer>().material;
            plusBaseColor = plusMat.GetColor("_BaseColor");
            plusEmissionColor = plusMat.GetColor("_EmissionColor");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Methods.IsOwnedPlayer(other)) return;

        // Avoid repeated detection
        if (isCollided) return;
        isCollided = true;
        collideTime = Time.time;
        StartCoroutine("WaitForInactive");

        // Send this checkpoint index and type to the collect script
        collect.CheckpointReached(index, type);
    }

    IEnumerator WaitForInactive()
    {
        yield return new WaitForSeconds(CheckpointCol.inactiveAfter);
        Reset();
    }

    public void Reset()
    {
        index = -1;
        type = ((!collect.started) && revertDiamond) ? checkpointType.Diamond : checkpointType.Initial;
        isCollided = false;
        collideTime = 0;

        checkpointVisualMat.SetColor("_Color", common.collectCheckpoint);

        if (type == checkpointType.Star)
        {
            starMat.SetColor("_BaseColor", starBaseColor);
            starMat.SetColor("_EmissionColor", starEmissionColor);
        }
        if (type == checkpointType.Diamond)
        {
            diamondMat.SetColor("_BaseColor", diamondBaseColor);
            diamondMat.SetColor("_EmissionColor", diamondEmissionColor);
            plusMat.SetColor("_BaseColor", plusBaseColor);
            plusMat.SetColor("_EmissionColor", plusEmissionColor);
        }

        checkpoint.SetActive(false);
    }
}