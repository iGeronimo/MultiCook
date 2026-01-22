using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private NetworkPlayerInput playerInput;
    [SerializeField] private float _speed = 1f;

    void FixedUpdate()
    {
        Walking();
    }

    void Walking()
    {
        transform.localPosition += new Vector3(playerInput.move.x, 0, playerInput.move.y) * _speed;
    }
}
