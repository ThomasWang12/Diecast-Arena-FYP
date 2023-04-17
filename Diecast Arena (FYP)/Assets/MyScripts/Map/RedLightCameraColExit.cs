using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedLightCameraColExit : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (!Methods.IsOwnedPlayer(other)) return;

        transform.parent.GetComponent<RedLightCameraCol>().isCollided = false;
    }
}
