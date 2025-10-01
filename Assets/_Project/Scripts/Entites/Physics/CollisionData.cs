using UnityEngine;

public struct CollisionData
{
    public bool hit;
    public Vector2 normal;
    public float separation;
    public Collider2D collider;
    public Collider2D otherCollider;
}