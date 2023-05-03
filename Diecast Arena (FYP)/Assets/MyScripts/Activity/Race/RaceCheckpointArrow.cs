using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class RaceCheckpointArrow : MonoBehaviour
{
    RaceActivity race;
    [SerializeField] GameObject arrowPrefab;
    [HideInInspector] public List<GameObject> arrows;

    /* Tunables */
    [Range(0.0f, 10.0f)]
    [SerializeField] float arrowHeight = 3.0f;

    void Awake()
    {
        race = transform.parent.gameObject.GetComponent<RaceActivity>();
    }

    public GameObject SpawnArrow(int index, float heightAdjust)
    {
        Vector3 checkpointPos = race.checkpoints[index].transform.position;
        Vector3 arrowPos = new Vector3(checkpointPos.x, checkpointPos.y + arrowHeight + heightAdjust, checkpointPos.z);
        GameObject arrow = Instantiate(arrowPrefab, arrowPos, Quaternion.identity);
        arrow.name = "Arrow " + index;
        arrow.transform.SetParent(transform);
        PointArrow(arrow, index);
        arrow.SetActive(false);
        arrows.Add(arrow);
        return arrows[index];
    }

    void PointArrow(GameObject arrow, int index)
    {
        int nextIndex = index < race.checkpoints.Count - 1 ? index + 1 : 0;
        Vector3 nextArrowPos = race.checkpoints[nextIndex].transform.position;
        Vector3 look = new Vector3(nextArrowPos.x, nextArrowPos.y + arrowHeight, nextArrowPos.z);
        arrow.transform.LookAt(look);
        arrow.transform.Rotate(0, 0, 90);
    }

    public void UpdateActive(int index)
    {
        arrows[index].SetActive(true);
    }

    public IEnumerator WaitForInactive(int index)
    {
        yield return new WaitForSeconds(CheckpointCol.inactiveAfter);
        arrows[index].SetActive(false);
    }
}
