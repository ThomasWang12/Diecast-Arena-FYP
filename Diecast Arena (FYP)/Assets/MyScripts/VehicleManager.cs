using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    GameMaster master;
    PlayerNetwork network;

    [HideInInspector] public PlayerColor[] playerColor;
    [HideInInspector] public int playerColorTotal = 4;
    public int playerColorIndex = -1;

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
        network = master.network;
    }

    public void Initialize()
    {
        playerColor = new PlayerColor[Constants.MaxPlayers];

        if (network.localPlay)
        {
            playerColor[0] = master.player.transform.Find("network").GetComponent<PlayerColor>();
        }
        else
        {
            for (int i = 0; i < Constants.MaxPlayers; i++)
            {
                GameObject player = Methods.FindPlayerById(i);
                if (player != null)
                    playerColor[i] = player.transform.Find("network").GetComponent<PlayerColor>();
            }
        }
    }

    public void ApplyPlayerColor(int id, int colorIndex)
    {
        if (playerColor[id] == null) return;
        playerColor[id].ApplyColor(colorIndex);
    }
}
