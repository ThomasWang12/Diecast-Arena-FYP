using System.Collections;
using System.Drawing;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

public class RaceCheckpointCol : MonoBehaviour
{
    GameMaster master;
    Common common;
    RaceActivity race;

    GameObject checkpoint;
    Material checkpointVisualMat;
    GameObject arrow;
    Material arrowMat;
    Color arrowBaseColor, arrowEmissionColor;
    GameObject flag;
    Material flagMat;
    Color flagBaseColor, flagEmissionColor;

    [HideInInspector] public int index = -1;
    float colliderRadius;
    [Tooltip("Manually adjust the spawn height of arrow/flag for this checkpoint.")]
    [Range(-10f, 10f)][SerializeField] float heightAdjust = 0;
    [SerializeField] bool isCollided = false;
    float collideTime = 0;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        common = master.ManagerObject(Manager.type.common).GetComponent<Common>();
        race = transform.parent.parent.parent.GetComponent<RaceActivity>();
        checkpoint = transform.parent.gameObject;
        checkpointVisualMat = Methods.GetChildContainsName(checkpoint, "[Visual]").GetComponent<MeshRenderer>().material;
    }

    void Start()
    {
        colliderRadius = transform.parent.localScale.x / 2;
    }

    void Update()
    {
        if (race.started)
        {
            if (isCollided == false)
            {
                float alpha = CheckpointCol.FadeCloserValue(transform.position, colliderRadius);
                checkpointVisualMat.SetFloat("_AlphaA", alpha);
            }
            else
            {
                checkpointVisualMat.SetColor("_Color", Color.white);
                float fade = CheckpointCol.FadeCollidedValue(collideTime);
                checkpointVisualMat.SetFloat("_AlphaA", fade);
                CheckpointCol.FadeCollided(arrow, arrowMat, fade);
                CheckpointCol.FadeCollided(flag, flagMat, fade);
            }
        }
    }

    public void InitializeCheckpoint()
    {
        checkpointVisualMat.SetColor("_Color", common.raceCheckpoint);
    }

    public void InitializeFinalCheckpoint()
    {
        checkpointVisualMat.SetColor("_Color", common.raceCheckpointFinish);
        race.flagScript.UpdateActive();
    }

    public void InitializeArrow()
    {
        arrow = race.arrowScript.SpawnArrow(index, heightAdjust);
        arrowMat = arrow.GetComponent<MeshRenderer>().material;
        arrowBaseColor = arrowMat.GetColor("_BaseColor");
        arrowEmissionColor = arrowMat.GetColor("_EmissionColor");
    }

    public void InitializeFlag()
    {
        flag = race.flagScript.SpawnFlag(index, heightAdjust);
        flagMat = flag.GetComponent<MeshRenderer>().material;
        flagBaseColor = flagMat.GetColor("_BaseColor");
        flagEmissionColor = flagMat.GetColor("_EmissionColor");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Methods.IsOwnedPlayer(other)) return;

        // Avoid repeated detection
        if (isCollided) return;
        isCollided = true;
        collideTime = Time.time;
        StartCoroutine("WaitForInactive");

        // Only the final checkpoint has a flag assigned to it, others only have arrow
        if (flag == null)
            race.arrowScript.StartCoroutine("WaitForInactive", index);
        else
        {
            // The final checkpoint
            if (race.currentLap < race.totalLap)
                race.arrowScript.StartCoroutine("WaitForInactive", index);
            else race.flagScript.StartCoroutine("WaitForInactive");
        }

        // Send this checkpoint index to the race script
        race.CheckpointReached(index);
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

        checkpointVisualMat.SetColor("_Color", common.raceCheckpoint);

        if (arrow != null)
        {
            arrowMat.SetColor("_BaseColor", arrowBaseColor);
            arrowMat.SetColor("_EmissionColor", arrowEmissionColor);
        }
        if (flag != null)
        {
            flagMat.SetColor("_BaseColor", flagBaseColor);
            flagMat.SetColor("_EmissionColor", flagEmissionColor);
        }

        checkpoint.SetActive(false);
    }
}
