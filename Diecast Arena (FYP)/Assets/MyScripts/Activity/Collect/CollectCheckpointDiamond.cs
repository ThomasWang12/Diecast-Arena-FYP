using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class CollectCheckpointDiamond : MonoBehaviour
{
    CollectActivity collect;
    [SerializeField] GameObject diamondPrefab;
    [HideInInspector] public List<GameObject> diamonds;

    /* Tunables */
    [Range(0.0f, 10.0f)]
    [SerializeField] float diamondHeight = 1.25f;

    void Awake()
    {
        collect = transform.parent.gameObject.GetComponent<CollectActivity>();
    }

    public GameObject SpawnDiamond(int index, float heightAdjust)
    {
        Vector3 checkpointPos = collect.checkpoints[index].transform.position;
        Vector3 diamondPos = new Vector3(checkpointPos.x, checkpointPos.y + diamondHeight + heightAdjust, checkpointPos.z);
        GameObject diamond = Instantiate(diamondPrefab, diamondPos, Quaternion.identity);
        diamond.name = "Diamond " + index;
        diamond.transform.SetParent(transform);
        diamond.SetActive(false);
        diamonds.Add(diamond);
        return diamond;
    }

    public IEnumerator WaitForInactive(int index)
    {
        yield return new WaitForSeconds(CheckpointCol.inactiveAfter);
        diamonds[index].SetActive(false);
    }
}
