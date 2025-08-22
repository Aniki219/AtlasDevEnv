using UnityEngine;

public interface IStateTransition
{
    public bool TryGetFirstActiveTransition(out StateType outStateType);
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
    protected StateRegistry stateRegistry;

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
        stateRegistry = stateMachine.stateRegistry;
    }

    public virtual bool TryGetFirstActiveTransition(out StateType outStateType)
    {
        throw new System.NotImplementedException();
    }
}