using UnityEngine;

public class OnComplete : StateTransition
{
    bool active;

    private void OnEnable()
    {
        active = false;
    }

    public void Activate()
    {
        active = true;
    }

    public override bool CheckCondition()
    {
        return active;
    }
}