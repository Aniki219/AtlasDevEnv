using UnityEngine;

public interface IStateTransition
{
    public bool CheckCondition();
    public State ToState();
}

public abstract class StateTransition : MonoBehaviour, IStateTransition
{
    [SerializeField] protected State to;

    protected EntityContext ctx;
    protected EntityController entity;
    protected PlayerController pc;
    protected EntityBody body;
    protected SpriteController sprite;
    protected InputManager input;
    protected State state;
    protected StateMachine stateMachine;

    private void Awake()
    {
        ctx = GetComponentInParent<EntityContext>();
        entity = ctx.controller;
        pc = entity as PlayerController;
        body = ctx.body;
        sprite = ctx.sprite;
        input = ctx.input;
        stateMachine = ctx.stateMachine;
        state = GetComponentInParent<State>();
    }

    public virtual bool CheckCondition()
    {
        throw new System.NotImplementedException();
    }

    public virtual State ToState()
    {
        return to;
    }
}

public abstract class EventTransition : StateTransition
{
    protected bool activeThisFrame = false;

    public override bool CheckCondition()
    {
        return activeThisFrame;
    }
}