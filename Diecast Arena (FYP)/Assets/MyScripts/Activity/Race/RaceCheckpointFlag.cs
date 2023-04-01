using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RaceCheckpointFlag : MonoBehaviour
{
    GameMaster master;
    RaceActivity race;
    [SerializeField] GameObject flagPrefab;
    [HideInInspector] public GameObject flag;

    /* Tunables */
    [Range(0.0f, 10.0f)]
    [SerializeField] float flagHeight = 3.0f;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        race = transform.parent.gameObject.GetComponent<RaceActivity>();
    }

    void Update()
    {
        if (race.started)
        {
            if (!race.finished)
            {
                // The flag keeps facing towards the camera (rotate y-axis)
                Vector3 rotateY = new Vector3(master.camPos.x, flag.transform.position.y, master.camPos.z);
                flag.transform.LookAt(rotateY);
            }
        }
    }

    public GameObject SpawnFlag(int index, float heightAdjust)
    {
        Vector3 checkpointPos = race.checkpoints[index].transform.position;
        Vector3 flagPos = new Vector3(checkpointPos.x, checkpointPos.y + flagHeight + heightAdjust, checkpointPos.z);
        flag = Instantiate(flagPrefab, flagPos, Quaternion.identity);
        flag.name = "Finish Flag";
        flag.transform.SetParent(transform);
        flag.SetActive(false);
        return flag;
    }

    public void UpdateActive()
    {
        flag.SetActive(true);
    }

    public IEnumerator WaitForInactive()
    {
        yield return new WaitForSeconds(CheckpointCol.inactiveAfter);
        flag.SetActive(false);
    }
}
