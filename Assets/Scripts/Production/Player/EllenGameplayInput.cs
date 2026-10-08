using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optional desktop/gamepad bridge for the existing mobile-first controls.
/// It forwards to the same APIs as touch buttons and therefore doesn't alter
/// movement, abilities or attack behavior on mobile.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public sealed class EllenGameplayInput : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAbilityController abilities;
    [SerializeField] private CharacterAttack attack;
    [SerializeField] private PlayerHealth health;

    private bool leftHeld;
    private bool rightHeld;
    private bool jumpHeld;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (abilities == null) abilities = GetComponent<PlayerAbilityController>();
        if (attack == null) attack = GetComponent<CharacterAttack>();
        if (health == null) health = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || (health != null && !health.isAlive))
        {
            ReleaseInputs();
            return;
        }

        bool left = false;
        bool right = false;
        bool jump = false;
        bool dashPressed = false;
        bool wallPressed = false;
        bool spiritPressed = false;
        bool firePressed = false;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            left |= keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            right |= keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            jump |= keyboard.spaceKey.isPressed || keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
            dashPressed |= keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
            wallPressed |= keyboard.eKey.wasPressedThisFrame;
            spiritPressed |= keyboard.qKey.wasPressedThisFrame;
            firePressed |= keyboard.fKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 stick = gamepad.leftStick.ReadValue();
            left |= stick.x < -0.35f || gamepad.dpad.left.isPressed;
            right |= stick.x > 0.35f || gamepad.dpad.right.isPressed;
            jump |= gamepad.buttonSouth.isPressed;
            dashPressed |= gamepad.buttonEast.wasPressedThisFrame;
            wallPressed |= gamepad.buttonNorth.wasPressedThisFrame;
            spiritPressed |= gamepad.buttonWest.wasPressedThisFrame;
            firePressed |= gamepad.rightShoulder.wasPressedThisFrame;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null) firePressed |= mouse.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        jump = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        wallPressed = Input.GetKeyDown(KeyCode.E);
        spiritPressed = Input.GetKeyDown(KeyCode.Q);
        firePressed = Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J) ||
                      Input.GetMouseButtonDown(0);
#endif

        if (movement != null)
        {
            // Only forward transitions. Existing UI buttons remain independent
            // when there is no attached keyboard/gamepad.
            if (left != leftHeld)
            {
                if (left) movement.OnButtonDown_A();
                else movement.OnButtonUp_A();
                leftHeld = left;
            }
            if (right != rightHeld)
            {
                if (right) movement.OnButtonDown_D();
                else movement.OnButtonUp_D();
                rightHeld = right;
            }
            if (jump != jumpHeld)
            {
                if (jump) movement.Jump();
                else movement.OnPressUp_W();
                jumpHeld = jump;
            }
        }

        if (dashPressed) abilities?.TryDash();
        if (wallPressed) abilities?.TryWallJump();
        if (spiritPressed) abilities?.TryToggleSpirit();
        if (firePressed) attack?.AttackStart();
    }

    private void OnDisable() => ReleaseInputs();

    private void ReleaseInputs()
    {
        if (movement != null)
        {
            if (leftHeld) movement.OnButtonUp_A();
            if (rightHeld) movement.OnButtonUp_D();
            if (jumpHeld) movement.OnPressUp_W();
        }
        leftHeld = false;
        rightHeld = false;
        jumpHeld = false;
    }
}
