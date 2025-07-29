using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    InputMaster inputMaster;
    InputMaster.PlayerActions player;

    [SerializeField] PlayerManger playerManger;

    public bool Jump(ButtonState state = ButtonState.PRESSED) => GetButton(player.Jump, state);
    public bool Broom(ButtonState state = ButtonState.PRESSED) => GetButton(player.Broom, state);
    public bool Pause(ButtonState state = ButtonState.PRESSED) => GetButton(player.Pause, state);
    public bool Inventory(ButtonState state = ButtonState.PRESSED) => GetButton(player.Inventory, state);
    public bool Cheat(ButtonState state = ButtonState.PRESSED) => GetButton(player.Cheat, state);
    public bool Interact(ButtonState state = ButtonState.PRESSED) => GetButton(player.Interact, state);
    public bool Up(ButtonState state = ButtonState.PRESSED) => GetButton(player.Up, state);
    public bool Down(ButtonState state = ButtonState.PRESSED) => GetButton(player.Down, state);
    public bool Forward() => Mathf.Abs(X) > 0 && AtlasHelpers.SameSign(X, playerManger.GetPlayerFacing());
    public bool Back() => Mathf.Abs(X) > 0 && !AtlasHelpers.SameSign(X, playerManger.GetPlayerFacing());
    public bool Escape(ButtonState state = ButtonState.PRESSED) => GetButton(player.Down, state);

    public bool Attack(ButtonState state = ButtonState.PRESSED) => GetButton(player.Attack, state, Tilt.Neutral);
    public bool AttackUp(ButtonState state = ButtonState.PRESSED) => GetButton(player.Attack, state, Tilt.Up);
    public bool AttackDown(ButtonState state = ButtonState.PRESSED) => GetButton(player.Attack, state, Tilt.Down);
    public bool AttackForward(ButtonState state = ButtonState.PRESSED) => GetButton(player.Attack, state, Tilt.Forward);
    public bool AttackBack(ButtonState state = ButtonState.PRESSED) => GetButton(player.Attack, state, Tilt.Backward);
    public bool AttackSide(ButtonState state = ButtonState.PRESSED) => AttackForward(state) || AttackBack(state);

    public bool Special(ButtonState state = ButtonState.PRESSED) => GetButton(player.Special, state, Tilt.Neutral);
    public bool UpSpecial(ButtonState state = ButtonState.PRESSED) => GetButton(player.Special, state, Tilt.Up);
    public bool DownSpecial(ButtonState state = ButtonState.PRESSED) => GetButton(player.Special, state, Tilt.Down);
    public bool ForwardSpecial(ButtonState state = ButtonState.PRESSED) => GetButton(player.Special, state, Tilt.Forward);
    public bool BackSpecial(ButtonState state = ButtonState.PRESSED) => GetButton(player.Special, state, Tilt.Backward);
    public bool SideSpecial(ButtonState state = ButtonState.PRESSED) => ForwardSpecial(state) || BackSpecial(state);

    public Vector2 Axis() => player.Movement.ReadValue<Vector2>();
    public float X => Axis().x;
    public float Y => Axis().y;

    public bool Pressed(InputActionReference actionRef) => actionRef.action.WasPressedThisFrame();
    public bool IsDown(InputActionReference actionRef) => actionRef.action.IsPressed();
    public bool Released(InputActionReference actionRef) => actionRef.action.WasReleasedThisFrame();

    private void Awake()
    {
        inputMaster = new InputMaster();
        inputMaster.Enable();
        player = inputMaster.Player;
    }

    private void OnEnable()
    {
        inputMaster.Enable();
    }

    private void OnDisable()
    {
        inputMaster.Disable();
    }

    private bool GetButton(
            InputAction action,
            ButtonState state = ButtonState.PRESSED,
            Tilt withTilt = Tilt.Any
        )
    {
        if (withTilt != Tilt.Any && !GetTiltDirection().Equals(withTilt))
        {
            return false;
        }

        switch (state)
        {
            case ButtonState.PRESSED:
                return action.WasPressedThisFrame();
            case ButtonState.DOWN:
                return action.IsPressed();
            case ButtonState.RELEASED:
                return action.WasReleasedThisFrame();
            default:
                throw new Exception("No implementation for ButtonState: " + state);
        }
    }

    public Tilt GetTiltDirection()
    {
        int playerFacing = playerManger.GetPlayerFacing();

        if (Y != 0)
        {
            return Y > 0 ? Tilt.Up : Tilt.Down;
        }
        if (X != 0)
        {
            return AtlasHelpers.SameSign(X, playerFacing) ? Tilt.Forward : Tilt.Backward;
        }
        return Tilt.Neutral;
    }
}

public enum Tilt
{
    Neutral,
    Forward,
    Backward,
    Up,
    Down,
    Any
}

public enum ButtonState
{
    PRESSED,
    DOWN,
    RELEASED
}