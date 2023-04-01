using System.Collections;
using System.Diagnostics;
using System.Drawing;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

public class CollectCheckpointCol : MonoBehaviour
{
    GameMaster master;
    CollectActivity collect;

    GameObject checkpoint;
    Material checkpointVisualMat;
    GameObject star;
    Material starMat, starMatInitial;
    GameObject diamond;
    Material diamondMat, diamondMatInitial;

    int index;
    float colliderRadius;
    [Tooltip("Manually adjust the spawn height of arrow/flag for this checkpoint.")]
    [Range(-10f, 10f)] [SerializeField] float heightAdjust = 0;
    [SerializeField] bool isCollided = false;
    float collideTime = 0;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        collect = transform.parent.parent.parent.GetComponent<CollectActivity>();
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
            }
        }
    }

    public void GetCheckpointIndex()
    {
        index = collect.checkpoints.IndexOf(checkpoint);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Methods.IsPlayer(other)) return;

        // Avoid repeated detection
        if (isCollided) return;
        isCollided = true;
        collideTime = Time.time;
        StartCoroutine("WaitForInactive");

        // Send this checkpoint index to the race script
        collect.CheckpointReached(index);
    }

    IEnumerator WaitForInactive()
    {
        yield return new WaitForSeconds(CheckpointCol.inactiveAfter);
        Reset();
        checkpoint.SetActive(false);
    }

    public void Reset()
    {
        isCollided = false;
        collideTime = 0;
    }
}