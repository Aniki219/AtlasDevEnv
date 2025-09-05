using UnityEngine;

public class ScreenWrapBehavior : StateBehavior, IStateBehavior
{
    [SerializeField] BoxCollider2D roomBounds;
    [SerializeField] float offset = 0.25f;
    Transform player;

    public void StartState()
    {
        roomBounds = GameObject.Find("RoomBounds")?.GetComponent<BoxCollider2D>();
        player = PlayerController.Instance.transform;
    }

    public void UpdateState()
    {
        if (roomBounds == null) return;

        if (player.position.y > roomBounds.bounds.max.y)
        {
            player.position = new Vector3(player.position.x, roomBounds.bounds.min.y + offset, 0);
        }
        if (player.position.y < roomBounds.bounds.min.y)
        {
            player.position = new Vector3(player.position.x, roomBounds.bounds.max.y - offset, 0);
        }
        if (player.position.x > roomBounds.bounds.max.x)
        {
            player.position = new Vector3(roomBounds.bounds.min.x + offset, player.position.y, 0);
        }
        if (player.position.x < roomBounds.bounds.min.x)
        {
            player.position = new Vector3(roomBounds.bounds.max.x - offset, player.position.y, 0);
        }
    }

    public void FixedUpdateState() { }

    public void ExitState() { }
}