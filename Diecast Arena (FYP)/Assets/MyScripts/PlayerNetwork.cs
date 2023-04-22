using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using static CollectActivity;

public class PlayerNetwork : NetworkBehaviour
{
    GameMaster master;
    ActivityOption activityOption;

    [SerializeField] TMP_InputField playerNameInput;

    [Space(10)]

    public int ownerPlayerId;
    public int activeActivityIndex = -1;
    NetworkList<FixedString64Bytes> playerName;

    void Awake()
    {
        master = GameObject.FindWithTag("GameManager").GetComponent<GameMaster>();
        activityOption = GameObject.Find("[Activity Triggers]").GetComponent<ActivityOption>();

        playerName = new NetworkList<FixedString64Bytes>();
    }

    public override void OnNetworkSpawn()
    {
        for (int i = 0; i < Constants.MaxPlayers; i++) playerName.Add("Player " + i);
    }

    public void Initialize()
    {
        ownerPlayerId = (int) Methods.FindOwnedPlayer().GetComponent<NetworkObject>().OwnerClientId;
    }

    public void ChangePlayerName()
    {
        if (Methods.IsEmptyOrWhiteSpace(playerNameInput.text)) return;
        ChangePlayerNameServerRpc(ownerPlayerId, playerNameInput.text);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangePlayerNameServerRpc(int id, string customName)
    {
        playerName[id] = customName;
        UpdatePlayerNameClientRpc(id, customName);
    }

    [ClientRpc]
    void UpdatePlayerNameClientRpc(int id, string customName)
    {
        playerName[id] = customName;
        Methods.FindPlayerById(id).transform.Find("network").GetComponent<PlayerNameDisplay>().UpdateName(customName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EnterActivityServerRpc(int activityIndex, int launcherOption)
    {
        activeActivityIndex = activityIndex;
        EnterActivityClientRpc(activityIndex, launcherOption);
    }

    [ClientRpc]
    void EnterActivityClientRpc(int activityIndex, int launcherOption)
    {
        activityOption.ApplyOptions(activityIndex, launcherOption);
        master.EnterActivity(activityIndex);
    }
}
