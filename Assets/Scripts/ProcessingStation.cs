using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProcessingStation : NetworkBehaviour
{
    [SerializeField] float processDuration = 2f;
    [SerializeField] float interactDistance = 1.5f;

    [SerializeField]NetworkVariable<float> progress = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    NetworkVariable<bool> isProcessing = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkList<ulong> interactingPlayerIds = new NetworkList<ulong>(
        null,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }


    public void TryStartProcessing(ulong playerId)
    {
        if (!IsServer) return;
        var player = GetPlayer(playerId);
        if (player == null) return;

        if (!interactingPlayerIds.Contains(playerId))
        {
            interactingPlayerIds.Add(playerId);
        }

        if (!isProcessing.Value)
        {
            isProcessing.Value = true;
            progress.Value = 0f;
        }
    }

    void Update()
    {
        if (!IsServer || !isProcessing.Value) return;

        List<ulong> playersToRemove = new List<ulong>();
        foreach(ulong playerId in interactingPlayerIds)
        {
            var player = GetPlayer(playerId);
            if (player == null || !IsPlayerInRange(player))
            {
                playersToRemove.Add(playerId);
            }
        }

        foreach(ulong player in playersToRemove)
        {
            interactingPlayerIds.Remove(player);
        }

        if(interactingPlayerIds.Count == 0)
        {
            CancelProcessing();
            return;
        }

        int workers = interactingPlayerIds.Count;
        progress.Value += (workers / processDuration) * Time.deltaTime;

        Debug.Log(progress.Value);

        if (progress.Value >= 1f)
        {
            CompleteProcessing();
        }
    }

    void CompleteProcessing()
    {
        isProcessing.Value = false;
        progress.Value = 1f;

        // Replace item, spawn chopped food, etc.
    }

    void CancelProcessing()
    {
        isProcessing.Value = false;
        progress.Value = 0f;
    }

    bool IsPlayerInRange(NetworkObject player)
    {
        float sqrDist =
            (player.transform.position - transform.position).sqrMagnitude;

        return sqrDist <= interactDistance * interactDistance;
    }

    bool IsPlayerFacing(NetworkObject player)
    {
        Vector3 dir =
            (transform.position - player.transform.position).normalized;

        return Vector3.Dot(player.transform.forward, dir) > 0.7f;
    }

    NetworkObject GetPlayer(ulong clientId)
    {
        NetworkManager.Singleton.ConnectedClients.TryGetValue(
            clientId,
            out var client);

        return client?.PlayerObject;
    }

    public static void CancelByPlayer(ulong playerId)
    {
        foreach (var station in FindObjectsByType<ProcessingStation>(FindObjectsSortMode.None))
        {
            if (station.interactingPlayerIds.Contains(playerId))
            {
                station.interactingPlayerIds.Remove(playerId);
                if(station.interactingPlayerIds.Count == 0) station.CancelProcessing();
                return;
            }
        }
    }
}
