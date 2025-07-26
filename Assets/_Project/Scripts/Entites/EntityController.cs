using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EntityContext))]
public class EntityController : MonoBehaviour
{
    [field: SerializeField] public int facing { get; private set; } = 1;

    protected EntityContext ctx;
    protected EntityBody body;
    protected SpriteController sprite;
    protected PlayerCanvasController canvas;

    private bool wasAnimEnd = false;
    public UnityEvent OnAnimationEnd;

    public virtual void Awake()
    {
        ctx = GetComponent<EntityContext>();
        body = ctx.body;
        sprite = ctx.sprite;
        canvas = ctx.canvas;
    }

    public void Update()
    {
        CheckAnimEnd();
    }
    
    /*
        This should be the only way to change the facing direction of the player
        We may need to rethink this- allowing facing and sprite x-scale to be different
    */
    public void SetFacing(int to)
    {
        facing = to;
        sprite.SetFacing(to);
    }

    public void TurnAround()
    {
        SetFacing(facing * -1);
    }

    /*
        Checks one for animation end using normalized time on the current clip.
        Might be worth checking or returning information on whether this was an
        override clip or not
    */
    private void CheckAnimEnd()
    {
        bool isAnimEnd = sprite.GetNormalizedTime() >= 1;

        if (!wasAnimEnd && isAnimEnd)
        {
            OnAnimationEnd?.Invoke();
        }

        wasAnimEnd = isAnimEnd;
    }
}