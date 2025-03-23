using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ButtonTransition : StateTransition
{
    [SerializeField] InputActionReference actionReference;
    [SerializeField] bool not;
    [SerializeField] ButtonState buttonState;

    public override bool CheckCondition()
    {
        bool active;
        switch (buttonState)
        {
            case ButtonState.PRESSED:
                active = input.Pressed(actionReference);
                break;
            case ButtonState.DOWN:
                active = input.IsDown(actionReference);
                break;
            case ButtonState.RELEASED:
                active = input.Released(actionReference);
                break;
            default:
                throw new System.Exception("Invalid ButtonState for: " + gameObject.name);
        }
        return not ? !active : active;
    }
}