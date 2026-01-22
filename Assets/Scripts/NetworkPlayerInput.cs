using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class NetworkPlayerInput : MonoBehaviour
{
    public Vector2 move;
    public bool interact;

    // Code-subscription events
    public event Action InteractPressed;
    public event Action InteractReleased;

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }
    public void OnInteract(InputValue value)
    {
        InteractInput(value.Get<float>()>0.5f);
    }

    // Called when the Interact action enters the Started phase (button initially pressed)
    public void OnInteractStarted(InputAction.CallbackContext context)
    {
        InteractInput(true);
        InteractPressed?.Invoke();
    }

    // Called when the Interact action enters the Canceled phase (button released)
    public void OnInteractCanceled(InputAction.CallbackContext context)
    {
        InteractInput(false);
        InteractReleased?.Invoke();
    }
#endif

    public void MoveInput(Vector2 newMove)
    {
        move = newMove;
    }
    public void InteractInput(bool newInteract)
    {
        interact = newInteract;
        if(interact)
        {
            Debug.Log("Press Started");
            InteractPressed?.Invoke();        
        }
        else 
        {
            Debug.Log("Press Canceled");
            InteractReleased?.Invoke();
        }
    }
}
