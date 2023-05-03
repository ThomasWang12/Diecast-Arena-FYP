using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetwork : NetworkBehaviour
{
    GameMaster master;
    VehicleManager vehicle;
    SoundManager sound;
    UIManager UI;
    ActivityOption activityOption;

    [SerializeField] TMP_InputField playerNameInput;

    [Tooltip("Toggle spawning player vehicle locally or over the network.")]
    public bool localPlay = false;

    [Space(10)]

    bool initialized = false;
    public int ownerPlayerId = 0;

    NetworkList<FixedString64Bytes> playerName;
    NetworkList<int> playerColorIndex;

    void Awake()
    {
        master = GameObject.FindWithTag("GameMaster").GetComponent<GameMaster>();
        vehicle = master.vehicle;
        sound = master.sound;
        UI = master.UI;
        activityOption = GameObject.Find("[Activity Triggers]").GetComponent<ActivityOption>();

        playerName = new NetworkList<FixedString64Bytes>();
        playerColorIndex = new NetworkList<int>();
    }

    void NetworkVarsInit()
    {
        for (int i = 0; i < Constants.MaxPlayers; i++)
        {
            playerName.Add("Player " + i);
            playerColorIndex.Add(-1);
        }
    }

    public override void OnNetworkSpawn()
    {
        localPlay = false;
        NetworkVarsInit();
    }

    void Start()
    {
        if (localPlay) NetworkVarsInit();
    }

    void Initialize()
    {
        int defaultColorIndex;

        if (localPlay)
        {
            ownerPlayerId = 0;
            defaultColorIndex = 0;
            playerColorIndex[ownerPlayerId] = defaultColorIndex;
            ChangePlayerColorCodes(0, defaultColorIndex);
        }
        else
        {
            ownerPlayerId = (int)Methods.FindOwnedPlayer().GetComponent<NetworkObject>().OwnerClientId;
            defaultColorIndex = ownerPlayerId % vehicle.playerColorTotal;
            UI.SelectColorOption(-1, defaultColorIndex);
            ChangePlayerColorServerRpc(ownerPlayerId, defaultColorIndex);
        }

        initialized = true;
    }

    void Update()
    {
        if (!initialized) Initialize();
    }

    public void EnterSession()
    {
        for (int i = 0; i < Constants.MaxPlayers; i++)
            vehicle.ApplyPlayerColor(i, playerColorIndex[i]);
    }

    #region Player Names

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
        Methods.FindPlayerById(id).transform.Find("network").GetComponent<PlayerNameDisplay>().UpdateName(customName);
    }

    #endregion

    #region Player Colors

    public void ChangePlayerColor(int colorIndex)
    {
        if (localPlay)
        {
            playerColorIndex[0] = colorIndex;
            ChangePlayerColorCodes(0, colorIndex);
        }
        else ChangePlayerColorServerRpc(ownerPlayerId, colorIndex);

        sound.Play(Sound.name.PaintSpray);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangePlayerColorServerRpc(int id, int colorIndex)
    {
        playerColorIndex[id] = colorIndex;
        ChangePlayerColorClientRpc(id, colorIndex);
    }

    [ClientRpc]
    void ChangePlayerColorClientRpc(int id, int colorIndex)
    {
        ChangePlayerColorCodes(id, colorIndex);
    }

    void ChangePlayerColorCodes(int id, int colorIndex)
    {
        if (ownerPlayerId == id)
        {
            UI.SelectColorOption(vehicle.playerColorIndex, colorIndex);
            vehicle.playerColorIndex = colorIndex;
        }
        if (master.ready) vehicle.ApplyPlayerColor(id, colorIndex);
    }

    #endregion

    #region Enter Activity

    [ServerRpc(RequireOwnership = false)]
    public void EnterActivityServerRpc(int activityIndex, int launcherOption)
    {
        EnterActivityClientRpc(activityIndex, launcherOption);
    }

    [ClientRpc]
    void EnterActivityClientRpc(int activityIndex, int launcherOption)
    {
        activityOption.ApplyOptions(activityIndex, launcherOption);
        master.EnterActivity(activityIndex);
    }

    #endregion
}
