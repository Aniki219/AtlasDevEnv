using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

using static ButtonType;

public class InputManager : MonoBehaviour, IGameManager
{
    // Singleton instance
    public static InputManager Instance { get; private set; }
    private PlayerController player => PlayerController.Instance;
    
    private InputMaster inputMaster;
    private InputMaster.PlayerActions playerActions;
    
    // Dictionary to store button information for each input type
    public Dictionary<ButtonType, ButtonInfo> buttonStates = new Dictionary<ButtonType, ButtonInfo>();
    private Dictionary<ButtonType, InputAction> buttonToActionMap = new Dictionary<ButtonType, InputAction>();
    private Vector2 currentAxis;
    private Vector2 previousAxis;

    [SerializeField] private float doubleTapWindow = 0.3f;
    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple InputManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        foreach (ButtonType buttonType in Enum.GetValues(typeof(ButtonType)))
        {
            buttonStates[buttonType] = new ButtonInfo();
        }

        //DontDestroyOnLoad(gameObject); // Ensure persistence across scenes
    }

    public Task Init()
    {
        if (Instance != this) {
            return Instance.Init();
        }

        if (initialized)
        {
            return Task.CompletedTask;
        }

        inputMaster = new InputMaster();
        playerActions = inputMaster.Player;
        
        // Initialize all button states
        foreach (ButtonType buttonType in Enum.GetValues(typeof(ButtonType)))
        {
            buttonStates[buttonType] = new ButtonInfo();
        }

        initialized = true;
        OnEnable();
        return Task.CompletedTask;
    }

    private void OnEnable()
    {
        if (!initialized) return;
        inputMaster.Enable();
        
        foreach (var buttonName in Enum.GetNames(typeof(ButtonType)))
        {
            var action = InputSystem.actions.FindAction($"Player/{buttonName}");
            Enum.TryParse<ButtonType>(buttonName, false, out var buttonType);
            action.performed += ctx => OnButtonPressed(buttonType);
            action.canceled += ctx => OnButtonReleased(buttonType);
        }
        
        // Handle movement separately since it's not a discrete button
        playerActions.Movement.performed += ctx => currentAxis = ctx.ReadValue<Vector2>();
        playerActions.Movement.canceled += ctx => currentAxis = Vector2.zero;
    }

    private void OnDisable()
    {
        if (!initialized) return;
        // Allegedly handles unsubscribing from all events
        inputMaster.Disable();

        foreach (var buttonName in Enum.GetNames(typeof(ButtonType)))
        {
            var action = InputSystem.actions.FindAction($"Player/{buttonName}");
            Enum.TryParse<ButtonType>(buttonName, false, out var buttonType);
            action.performed -= ctx => OnButtonPressed(buttonType);
            action.canceled -= ctx => OnButtonReleased(buttonType);
        }

        playerActions.Movement.performed -= ctx => currentAxis = ctx.ReadValue<Vector2>();
        playerActions.Movement.canceled -= ctx => currentAxis = Vector2.zero;
    }

    private void LateUpdate()
    {
        UpdateButtonHoldTimes();
        previousAxis = currentAxis;
    }

    private void OnButtonPressed(ButtonType buttonType)
    {
        var info = buttonStates[buttonType];
        var currentTime = Time.time;
        
        // Check for double-tap
        info.wasDoubleTapped = currentTime - info.lastPressTime <= doubleTapWindow;
        
        info.wasPressedThisFrame = true;
        info.isCurrentlyDown = true;
        info.wasReleasedThisFrame = false;
        info.lastPressTime = currentTime;
        info.pressStartTime = currentTime;
        info.tiltOnPress = GetTilt();
    }

    private void OnButtonReleased(ButtonType buttonType)
    {
        var info = buttonStates[buttonType];
        
        info.wasReleasedThisFrame = true;
        info.isCurrentlyDown = false;
        info.wasPressedThisFrame = false;
    }

    private void UpdateButtonHoldTimes()
    {
        foreach (var (btn, info) in buttonStates)
        {
            info.wasPressedThisFrame = false;
            info.wasReleasedThisFrame = false;
            info.wasDoubleTapped = false;
        }
    }

    public InputQuery Query(ButtonType buttonType) => new InputQuery(buttonStates[buttonType]);
    public static InputQuery GetInput(ButtonType buttonType) => Instance.Query(buttonType);
    
    // Axis and movement queries
    public Vector2 GetAxis() => currentAxis;
    public float X => currentAxis.x;
    public float Y => currentAxis.y;
    
    public bool Forward() => 
        Mathf.Abs(currentAxis.x) > 0 && AtlasHelpers.SameSign(currentAxis.x, player.GetFacing());
        
    public bool Back() => 
        Mathf.Abs(currentAxis.x) > 0 && !AtlasHelpers.SameSign(currentAxis.x, player.GetFacing());

    public Tilt GetTilt()
    {
        int playerFacing = player.GetFacing();

        if (currentAxis.y != 0)
            return currentAxis.y > 0 ? Tilt.Up : Tilt.Down;
            
        if (currentAxis.x != 0)
            return AtlasHelpers.SameSign(currentAxis.x, playerFacing) ? Tilt.Forward : Tilt.Backward;
            
        return Tilt.Neutral;
    }
}

public static class ButtonTypeExtensions {
    public static InputQuery Pressed(this ButtonType b) => Q(b).Pressed();
    public static InputQuery Held(this ButtonType b) => Q(b).Held();
    public static InputQuery Released(this ButtonType b) => Q(b).Released();
    public static InputQuery DoubleTap(this ButtonType b) => Q(b).DoubleTap();
    public static InputQuery WithTilt(this ButtonType b, Tilt tilt) => Q(b).WithTilt(tilt);
    public static InputQuery FTilt(this ButtonType b) => Q(b).FTilt();
    public static InputQuery DownTilt(this ButtonType b) => Q(b).DownTilt();
    public static InputQuery BackTilt(this ButtonType b) => Q(b).BackTilt();
    public static InputQuery UpTilt(this ButtonType b) => Q(b).UpTilt();
    public static InputQuery Neutral(this ButtonType b) => Q(b).Neutral();
    public static InputQuery HeldFor(this ButtonType b, float duration) => Q(b).HeldFor(duration);
    
    private static InputQuery Q(this ButtonType buttonType) {
        var inputManager = InputManager.Instance;
        return inputManager.Query(buttonType);
    }
}

public struct InputQuery
{
    private readonly ButtonInfo buttonInfo;
    private readonly bool currentResult; // Tracks the result of chained conditions
    
    public InputQuery(ButtonInfo buttonInfo, bool initialResult = true)
    {
        this.buttonInfo = buttonInfo;
        this.currentResult = initialResult;
    }

    public static implicit operator bool(InputQuery query) => query.currentResult;

    public InputQuery Pressed() => 
        new InputQuery(buttonInfo, currentResult && buttonInfo.wasPressedThisFrame);
        
    public InputQuery Held() => 
        new InputQuery(buttonInfo, currentResult && buttonInfo.isCurrentlyDown);
        
    public InputQuery Released() => 
        new InputQuery(buttonInfo, currentResult && buttonInfo.wasReleasedThisFrame);
        
    public InputQuery DoubleTap() => 
        new InputQuery(buttonInfo, currentResult && buttonInfo.wasDoubleTapped);

    // Directional queries that can be chained
    public InputQuery WithTilt(Tilt tilt)
    {
        bool tiltMatches = tilt == Tilt.Any || 
                          (buttonInfo.wasPressedThisFrame && buttonInfo.tiltOnPress == tilt) ||
                          (buttonInfo.isCurrentlyDown && InputManager.Instance.GetTilt() == tilt);
        return new InputQuery(buttonInfo, currentResult && tiltMatches);
    }

    // Convenience methods for common tilt directions
    public InputQuery FTilt() => WithTilt(Tilt.Forward);
    public InputQuery BackTilt() => WithTilt(Tilt.Backward);
    public InputQuery UpTilt() => WithTilt(Tilt.Up);
    public InputQuery DownTilt() => WithTilt(Tilt.Down);
    public InputQuery Neutral() => WithTilt(Tilt.Neutral);
            
    public InputQuery HeldFor(float duration) => 
        new InputQuery(buttonInfo, 
            currentResult && buttonInfo.isCurrentlyDown && (Time.time - buttonInfo.pressStartTime) >= duration);

    // Logical operations for combining conditions
    public InputQuery And(InputQuery other) =>
        new InputQuery(buttonInfo, currentResult && other.currentResult);
        
    public InputQuery Or(InputQuery other) =>
        new InputQuery(buttonInfo, currentResult || other.currentResult);
        
    public InputQuery Not() =>
        new InputQuery(buttonInfo, !currentResult);
}

[Serializable]
public class ButtonInfo
{
    // Current frame state
    public bool wasPressedThisFrame;
    public bool isCurrentlyDown;
    public bool wasReleasedThisFrame;
    public bool wasDoubleTapped;
    
    // Timing information
    public float lastPressTime;
    public float pressStartTime;
    
    // Context information
    public Tilt tiltOnPress;
}

public enum ButtonType
{
    Jump,
    Attack,
    Special,
    Broom,
    Interact,
    Inventory,
    Pause,
    Cheat,
    Up,
    Down
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