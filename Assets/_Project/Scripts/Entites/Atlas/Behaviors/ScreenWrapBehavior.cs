using UnityEngine;

public class ScreenWrapBehavior : MonoBehaviour, IStateBehavior
{
    [SerializeField] BoxCollider2D roomBounds;
    [SerializeField] float offset = 0.25f;

    public void StartState() { }

    public void UpdateState()
    {
        if (transform.root.position.y > roomBounds.bounds.max.y)
        {
            transform.root.position = new Vector3(transform.root.position.x, roomBounds.bounds.min.y + offset, 0);
        }
        if (transform.root.position.y < roomBounds.bounds.min.y)
        {
            transform.root.position = new Vector3(transform.root.position.x, roomBounds.bounds.max.y - offset, 0);
        }
        if (transform.root.position.x > roomBounds.bounds.max.x)
        {
            transform.root.position = new Vector3(roomBounds.bounds.min.x + offset, transform.root.position.y, 0);
        }
        if (transform.root.position.x < roomBounds.bounds.min.x)
        {
            transform.root.position = new Vector3(roomBounds.bounds.max.x - offset, transform.root.position.y, 0);
        }
    }

    public void FixedUpdateState() { }

    public void ExitState() { }
}