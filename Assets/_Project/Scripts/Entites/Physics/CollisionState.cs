using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct CollisionState
{
    private bool above, below;
    private bool left, right;

    [SerializeField] private List<Vector2> normals;
    [SerializeField] private Vector2 groundSlope;
    private Vector2 closestMidPoint;
    private Vector2 closestFootPoint;
    private Vector2 closestHeadPoint;
    [SerializeField] private bool grounded;
    public bool wasGrounded { get; private set; }

    [SerializeField] private bool tangible;
    [SerializeField] private bool collidable;

    public void Reset()
    {
        above = false;
        below = false;
        left = false;
        right = false;
        normals = new List<Vector2>();
        closestMidPoint = Vector2.zero;
        closestFootPoint = Vector2.zero;
        closestHeadPoint = Vector2.zero;
        wasGrounded = grounded;
        grounded = false;
    }

    public void Init()
    {
        Reset();
        tangible = true;
        collidable = true;
    }

    public void setCollisionInfo(CollisionData collisionData)
    {
        if (!normals.Contains(collisionData.normal))
        {
            normals.Add(collisionData.normal);
        }

        foreach (Vector2 normal in normals)
        {
            if (normal.x > 0) left = true;
            if (normal.x < 0) right = true;
            if (normal.y > 0) below = true;
            if (normal.y < 0) above = true;
        }

        //setGroundSlope(collisionData.normal);

        Collider2D collider = collisionData.collider;
        closestMidPoint = collisionData.otherCollider.ClosestPoint(collider.bounds.center);
        closestHeadPoint = collisionData.otherCollider.ClosestPoint((Vector2)collider.bounds.max + collider.bounds.extents.x * Vector2.left);
        closestFootPoint = collisionData.otherCollider.ClosestPoint((Vector2)collider.bounds.min + collider.bounds.extents.x * Vector2.right);
    }

    public bool isGrounded()
    {
        return grounded;
    }

    public void setGrounded(bool isGrounded)
    {
        grounded = isGrounded;
    }

    public void setGroundSlope(Vector2 slopeNormal)
    {
        groundSlope = slopeNormal;
    }

    public Vector2 getGroundSlope()
    {
        return groundSlope;
    }

    public Vector2 getPoint()
    {
        return closestMidPoint;
    }

    public bool getLeft()
    {
        return left;
    }

    public bool getRight()
    {
        return right;
    }

    public bool getAbove()
    {
        return above;
    }

    public bool getBelow()
    {
        return below;
    }

    public List<Vector2> getNorms()
    {
        return normals;
    }

    public Vector2 getAverageNorm()
    {
        return (normals.Aggregate(Vector2.zero, (v1, v2) => v1 + v2) / normals.Count).normalized;
    }

    public bool hasNormWhere(Predicate<Vector2> lambda, bool includeAverage = false)
    {
        List<Vector2> n = includeAverage ? normals.Concat(new List<Vector2> { getAverageNorm() }).ToList() : normals;
        return n.Find(lambda) != default;
    }

    public void setCollidable(bool on = true)
    {
        collidable = on;
    }

    public void setTangible(bool on = true)
    {
        tangible = on;
    }

    public bool isCollidable()
    {
        return collidable;
    }

    public bool isTangible()
    {
        return tangible;
    }

    public Vector2 getHeadPoint()
    {
        return closestHeadPoint;
    }

    public Vector2 getMidPoint()
    {
        return closestMidPoint;
    }

    public Vector2 getFootPoint()
    {
        return closestFootPoint;
    }
}