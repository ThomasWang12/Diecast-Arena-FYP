using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerColorById : NetworkBehaviour
{
    [SerializeField] Texture yellowCar;
    [SerializeField] Texture redCar;
    [SerializeField] Texture blueCar;

    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == 0) GetComponent<Renderer>().material.SetTexture("_BaseMap", yellowCar);
        if (OwnerClientId == 1) GetComponent<Renderer>().material.SetTexture("_BaseMap", redCar);
        if (OwnerClientId == 2) GetComponent<Renderer>().material.SetTexture("_BaseMap", blueCar);
    }
}
