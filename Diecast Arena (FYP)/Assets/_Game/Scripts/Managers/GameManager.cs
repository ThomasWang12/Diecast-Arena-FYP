using RVP;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _normalOrientation;

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong playerId)
    {
        // Spawn vehicle for player
        GameObject spawnPoint = GameObject.Find("Spawn Points").transform.Find("Player " + playerId).gameObject; // #%
        var spawnedPlayer = Instantiate(_playerPrefab, spawnPoint.transform.position + Common.spawnHeightOffset, spawnPoint.transform.rotation); // #%
        spawnedPlayer.name = "Player " + playerId + " Vehicle";
        spawnedPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);

        // Spawn normal orientation object for the vehicle
        var spawnedNormalOrientation = Instantiate(_normalOrientation);
        spawnedNormalOrientation.name = "Player " + playerId + " Normal Orientation";
        spawnedNormalOrientation.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
    }

    public override async void OnDestroy()
    {
        base.OnDestroy();
        await MatchmakingService.LeaveLobby();
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
    }
}