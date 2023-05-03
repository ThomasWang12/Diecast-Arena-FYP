using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    GameMaster master;
    Vector3 initialPos;

    /* Tunables */
    [SerializeField] float frequency = 1;
    [SerializeField] float magnitude = 1;

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
    }

    void Start()
    {
        initialPos = transform.position;
    }

    void Update()
    {
        if (master.ready) return;

        float sinZ = Mathf.Sin(Time.time * frequency) * magnitude;
        transform.position = new Vector3(initialPos.x, initialPos.y, initialPos.z - sinZ);
    }
}
