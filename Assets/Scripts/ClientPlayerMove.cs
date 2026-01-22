using Unity.Netcode;
using UnityEngine;
using InputPlayer = UnityEngine.InputSystem.PlayerInput;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private NetworkPlayerInput networkInput;
    [SerializeField] private InputPlayer unityPlayerInput;

    void Awake()
    {
        playerMovement.enabled = false;
        if (networkInput != null) networkInput.enabled = false;
        if (unityPlayerInput != null) unityPlayerInput.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            if (networkInput != null) networkInput.enabled = true;
            if (unityPlayerInput != null) unityPlayerInput.enabled = true;
        }
        if(IsServer)
        {
            playerMovement.enabled = true;
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateInputServerRPC(Vector2 move)
    {
        if (networkInput != null) networkInput.MoveInput(move);
    }

    void LateUpdate()
    {
        if(!IsOwner) return;
        if (networkInput == null) return;
        UpdateInputServerRPC(networkInput.move);
    }
}
