using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OneWayPlatformController : MonoBehaviour
{
    GameManager gameManager;
    EntityBody playerBody;
    new BoxCollider2D collider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (!gm)
        {
            throw new UnityException("No GameManager");
        }
        gameManager = gm.GetComponent<GameManager>();
        playerBody = gameManager.GetPlayer().GetComponentInChildren<EntityBody>();
        collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        collider.enabled = playerBody.GetBottomYValue() >= collider.bounds.max.y && playerBody.velocity.y <= 0;
    }
}
