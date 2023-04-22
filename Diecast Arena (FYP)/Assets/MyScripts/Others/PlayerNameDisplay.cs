using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class PlayerNameDisplay : MonoBehaviour
{
    Transform playername;
    [HideInInspector] public TMP_Text playernameTMP;
    int id;

    void Start()
    {
        playername = transform.Find("playername");
        playernameTMP = playername.GetComponent<TextMeshProUGUI>();
        id = (int) transform.parent.GetComponent<NetworkObject>().OwnerClientId;

        if (transform.parent.GetComponent<NetworkObject>().IsOwner)
            gameObject.SetActive(false);

        playernameTMP.text = "Player " + id;
    }

    void Update()
    {
        if (Methods.FindOwnedPlayer() != null)
        {
            // playername keeps facing towards the player (rotate y-axis)
            float playerX = Methods.FindOwnedPlayer().transform.position.x;
            float playerZ = Methods.FindOwnedPlayer().transform.position.z;
            Vector3 rotateY = new Vector3(playerX, playername.transform.position.y, playerZ);
            transform.LookAt(rotateY);
        }
    }

    public void UpdateName(string name)
    {
        playernameTMP.text = name;
    }
}
