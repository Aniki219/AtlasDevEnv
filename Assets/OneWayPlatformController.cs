using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OneWayPlatformController : MonoBehaviour
{
    EntityBody playerBody;
    new BoxCollider2D collider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerBody = PlayerController.Instance.GetComponentInChildren<EntityBody>();
        collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        collider.enabled = playerBody.GetBottomYValue() >= collider.bounds.max.y && playerBody.velocity.y <= 0;
    }
}
