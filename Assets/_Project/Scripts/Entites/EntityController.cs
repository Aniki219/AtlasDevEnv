using UnityEngine;

[RequireComponent(typeof(EntityContext))]
public class EntityController : MonoBehaviour
{
    public int facing = 1;

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
}