using UnityEngine;

[RequireComponent(typeof(EntityContext))]
public class EntityController : MonoBehaviour
{
    [field: SerializeField] public int facing { get; private set; } = 1;

    protected EntityContext ctx;
    protected EntityBody body;
    protected SpriteController sprite;
    protected PlayerCanvasController canvas;

    public virtual void Awake()
    {
        ctx = GetComponent<EntityContext>();
        body = ctx.body;
        sprite = ctx.sprite;
        canvas = ctx.canvas;
    }

    // This should be the only way to change the facing direction of the player
    // We may need to rethink this- allowing facing and sprite x-scale to be different
    public void SetFacing(int to)
    {
        facing = to;
        sprite.SetFacing(to);
    }
}