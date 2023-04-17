using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class CollectCheckpointStar : MonoBehaviour
{
    CollectActivity collect;
    [SerializeField] GameObject starPrefab;
    [HideInInspector] public List<GameObject> stars;

    /* Tunables */
    [Range(0.0f, 10.0f)]
    [SerializeField] float starHeight = 1.25f;

    void Awake()
    {
        collect = transform.parent.gameObject.GetComponent<CollectActivity>();
    }

    public GameObject SpawnStar(int index, float heightAdjust)
    {
        Vector3 checkpointPos = collect.checkpoints[index].transform.position;
        Vector3 starPos = new Vector3(checkpointPos.x, checkpointPos.y + starHeight + heightAdjust, checkpointPos.z);
        GameObject star = Instantiate(starPrefab, starPos, Quaternion.identity);
        star.name = "Star " + index;
        star.transform.SetParent(transform);
        star.SetActive(false);
        stars.Add(star);
        return star;
    }

    public IEnumerator WaitForInactive(int index)
    {
        yield return new WaitForSeconds(CheckpointCol.inactiveAfter);
        stars[index].SetActive(false);
    }
}
