using System.Collections.Generic;
using UnityEngine;

public class HitBoxController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var sprite = GetComponent<SpriteRenderer>().sprite;
        var polygonCollider2D = GetComponent<PolygonCollider2D>();
        var pointsList = new List<Vector2>();
        // 0 is the shape index, which size should be 1 if you are using Sprite Mode = Single.
        // Also the texture must have Generate Physics Shape set to true in its settings.
        sprite.GetPhysicsShape(0, pointsList);

        polygonCollider2D.points = pointsList.ToArray();
    }
}
