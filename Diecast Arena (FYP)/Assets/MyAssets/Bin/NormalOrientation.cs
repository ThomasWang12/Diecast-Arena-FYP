using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NormalOrientation : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        gameObject.name = "Player " + OwnerClientId + " Normal Orientation";
    }
}
