using RVP;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VehicleNormOrient : NetworkBehaviour
{
    [SerializeField] private Transform normOrientPrefab;
    public Transform spawnedNormOrient;

    public void SpawnNormOrient()
    {
        spawnedNormOrient = Instantiate(normOrientPrefab);
        spawnedNormOrient.GetComponent<NetworkObject>().Spawn(true);
    }
}
