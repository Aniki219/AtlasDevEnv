using UnityEngine;
using UnityEngine.Diagnostics;

public interface IStateBehavior
{
    public void StartState();
    public void UpdateState();
    public void FixedUpdateState();
    public void ExitState();
}

public abstract class StateBehavior : MonoBehaviour
{
    protected EntityContext ctx;
    protected EntityController entity;
    protected PlayerController pc;
    protected EntityBody body;
    protected SpriteController sprite;
    protected InputManager input;
    protected State state;

    private void Awake()
    {
        ctx = GetComponentInParent<EntityContext>();
        entity = ctx.controller;
        pc = entity as PlayerController;
        body = ctx.body;
        sprite = ctx.sprite;
        input = ctx.input;
        state = GetComponentInParent<State>();
    }
}