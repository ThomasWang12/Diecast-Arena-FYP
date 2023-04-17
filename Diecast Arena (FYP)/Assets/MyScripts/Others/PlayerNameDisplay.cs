using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerNameDisplay : MonoBehaviour
{
    Transform playername;

    void Start()
    {
        playername = transform.Find("playername");

        if (transform.parent.GetComponent<NetworkObject>().IsOwner)
            gameObject.SetActive(false);

        string display = "Player " + transform.parent.GetComponent<NetworkObject>().OwnerClientId;
        playername.GetComponent<TextMeshProUGUI>().text = display;
    }

    void Update()
    {
        // playername keeps facing towards the player (rotate y-axis)
        float playerX = Methods.FindOwnedPlayer().transform.position.x;
        float playerZ = Methods.FindOwnedPlayer().transform.position.z;
        Vector3 rotateY = new Vector3(playerX, playername.transform.position.y, playerZ);
        transform.LookAt(rotateY);
    }
}
