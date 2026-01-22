using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteract : NetworkBehaviour
{
    [SerializeField] private NetworkPlayerInput networkInput;
    [SerializeField] private float interactRange = 1.5f;


    void OnEnable()
    {
        networkInput.InteractPressed += TryInteract;
        networkInput.InteractReleased += OnInteractCanceled;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(!IsOwner) return;
        if(networkInput == null) return;
    }

    void TryInteract()
    {
        if(TryGetInteractable(out NetworkObject interactable))
        {
            Debug.Log("hit interactable");
            InteractServerRpc(interactable.NetworkObjectId);
        }
    }

    void OnInteractCanceled()
    {
        StopInteractServerRpc();
    }

    bool TryGetInteractable(out NetworkObject target)
    {
        target = null;

        Ray ray = new Ray(transform.position, transform.forward);
        Debug.Log(ray);
        if(Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Debug.Log(hit);
            return hit.collider.TryGetComponent(out target);
        }

        return false;
    }

    [ServerRpc]
    void InteractServerRpc(ulong targetObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(targetObjectId, out NetworkObject target))
            return;

        ProcessingStation station = target.GetComponent<ProcessingStation>();
        if (station == null) return;

        station.TryStartProcessing(OwnerClientId);
    }

    [ServerRpc]
    void StopInteractServerRpc()
    {
        ProcessingStation.CancelByPlayer(OwnerClientId);
    }
}
