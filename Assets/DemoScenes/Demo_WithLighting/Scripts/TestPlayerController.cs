using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    
    private InteractiveWater.InteractiveWater _interactiveWater;

    private Rigidbody2D _rb;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _interactiveWater = FindAnyObjectByType<InteractiveWater.InteractiveWater>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;
        if ((keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
            (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame))
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        }
    }

    private void FixedUpdate()
    {
        var move = 0f;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move += 1f;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            var stickMove = gamepad.leftStick.ReadValue().x;
            if (Mathf.Abs(stickMove) > Mathf.Abs(move)) move = stickMove;
        }

        _rb.linearVelocity = new Vector2(move * _moveSpeed, _rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
    }
}
