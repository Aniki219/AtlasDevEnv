using UnityEngine;

public class BroomTurnAroundBehavior : StateBehavior, IStateBehavior {

    public float transitionTime;
    [SerializeField] BroomBehavior broomBehavior;
    [SerializeField] State BroomStraight;

    public void StartState()
    {
        if (transitionTime <= 0)
        {
            Destroy(gameObject);
            throw new System.Exception(
                $"BroomTurnAround transition time is non positive: {transitionTime}"
            );
        }
    }

    public void UpdateState()
    {
        float t = state.GetElapsedTime() / transitionTime;
        broomBehavior.yAngle = Mathf.Lerp(0, 180, t);

        if (t >= 1)
        {
            ctx.stateMachine.ChangeState(BroomStraight);
        }
    }

    public void FixedUpdateState() {}

    public void ExitState()
    {
        broomBehavior.yAngle = 0;
        broomBehavior.pitchLerper = new RateLerper();
        entity.SetFacing(-entity.facing);
    }
}