using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEditor.Experimental.GraphView;
using static AtlasHelpers;

public class EntityBody : MonoBehaviour
{
    #region Defs
    [Header("Debug")]
    public bool debug = false;
    public bool showNormal = true;
    public bool showVelocityNormal = false;
    public bool showCollisionResolution = false;
    public bool showSafetyCheck = false;
    public bool highlightGrounded = false;

    [Header("Movement")]
    public Vector3 acceleration;
    public Vector3 velocity;
    public Vector3 targetVelocity;
    public float groundedFriction;
    public float airFriction;

    public bool isFlying = false;

    private float xVelocitySmoothing;

    [Header("Control")]
    public bool lockPosition = false;
    public bool canMove = true;
    public bool canGravity = true;
    public bool canCrouch = false;
    public bool canDescendRamps = true;

    [Header("Collisions")]
    public CollisionState collisions;

    public LayerMask collisionMask;
    public LayerMask dangerMask;


    //TODO: wtf is this?
    private float safetyMargin = 1;

    [Header("Gravity")]
    public float termVel = -10;
    public float gravityMod = 1.0f;
    [SerializeField] private float gravity;
    [SerializeField] private float currentGravity = 0;
    public float maxGravity = 8.0f;

    [Header("Misc")]
    public float maxSlope = 0.5f;

    [Header("Components")]
    [SerializeField] ColliderManager colliderManager;
    [SerializeField] new Transform transform;
    [SerializeField] EntityController entity;
    BoundaryPoints boundaryPoints;
    #endregion

    #region Events
    public bool bonkCeiling = false;
    public UnityEvent OnBonkCeiling;

    public bool hasLandingEvent = false;
    public UnityEvent OnLanding;

    #endregion

    void Start()
    {
        gravity = -17.6f; //gameManager.Instance.gravity;
        collisions.Init();
    }

    private void FixedUpdate()
    {
        UpdateBoundaryPoints();
        ApplyFriction();

        //Keep track of "justLanded" and "justHeadBonked"
        if (canMove)
        {
            Move();

            //TODO: Make CanGravity a method. Remove this if statement
            if (canGravity)
            {
                doGravity();
            }
            else
            {
                ResetGravity();
            }
        }
    }

    private void LateUpdate()
    {
        DebugBody();
    }

    private void ApplyFriction()
    {
        if (Mathf.Approximately(velocity.x, 0)) return; 
    
        float friction = IsGrounded() ? groundedFriction : airFriction;
        
        // Friction opposes velocity direction and scales with speed
        Vector2 frictionForce = -friction * velocity.x * Vector2.right;

        AddForce(frictionForce);
    }

    public void AddForce(Vector2 force)
    {
        acceleration += (Vector3)force;
    }

    private void ResetAcceleration()
    {
        acceleration = Vector3.zero;
    }

    private void DebugBody()
    {
        if (debug)
        {
            BoxCollider2D collider = colliderManager.getCollider();
            UpdateBoundaryPoints();
            //GetComponentInChildren<SpriteRenderer>().material.color = Color.white.WithAlphaSetTo(0.75f);
            if (showCollisionResolution) debugBoundaryCollisions();
            if (showNormal)
            {
                foreach (Vector2 normal in collisions.getNorms())
                {
                    Debug.DrawLine(collider.bounds.center, collider.bounds.center + (Vector3)normal, Color.magenta);
                }
                Debug.DrawLine(collider.bounds.center, collider.bounds.center + (Vector3)collisions.getAverageNorm(), Color.red);
            }
            if (showVelocityNormal)
            {
                Debug.DrawLine(collider.bounds.center, collider.bounds.center + velocity.normalized, Color.grey);
            }
        }
    }

    private void doGravity()
    {
        if (collisions.isGrounded() && collisions.getGroundSlope().y >= maxSlope)
        {
            currentGravity = 0;
            velocity.y = Mathf.Max(velocity.y, 0);
        }
        else
        {
            currentGravity = gravity * gravityMod * (velocity.y > 0 ? 1 : 1.25f);
            velocity.y = Mathf.Max(velocity.y, termVel);
        }

        AddForce(currentGravity * Vector2.up);
    }

    public void ResetGravity()
    {
        currentGravity = 0;
    }

    public void SetCollider(string colliderName)
    {
        colliderManager.enableCollider(colliderName);
    }

    public float GetBottomYValue()
    {
        return boundaryPoints.bottomLeft.y;
    }

    public void Move()
    {
        // If the player is not inputting a direction maintain momentum...
        if (IsGrounded() || !Mathf.Approximately(targetVelocity.x, 0))
        {
            LerpToTargetVelocity();
        }

        velocity += acceleration * Time.fixedDeltaTime;
        ResetAcceleration();

        velocity.y = Mathf.Max(velocity.y, termVel);

        Vector3 dp = velocity * Time.fixedDeltaTime;

        // Skip collision checks if intangible
        if (!collisions.isTangible())
        {
            transform.position += dp;
            return;
        }

        // Is this the right place? Is this needed?
        if (lockPosition) { return; }

        // Modify speed on slopes to maintain overall horizontal speed
        if (collisions.hasNormWhere(norm => norm.y > 0) &&
            collisions.hasNormWhere(norm => norm.y < 0.75f) &&
            velocity.y <= 0)
        {
            velocity *= 0.8f;
        }

        collisions.Reset();
        dp = ResolveCollision(dp);

        transform.position += dp;

        CheckGrounded();
        if (canDescendRamps)
        {
            DescendRamp();
        }
    }
    public float groundAcceleration = 50;
    public float groundDeceleration = 60;
    public float airAcceleration = 30;
    public float airDeceleration = 30;
    public float groundMaxSpeed = 5f;
    public float stopThreshold = 0.1f;

    private void LerpToTargetVelocity()
    {
        float velocityDiff = targetVelocity.x - velocity.x;

        // Choose acceleration or deceleration rate
        float rate;
        if (SameSign(velocityDiff, targetVelocity.x))
        {
            // Accelerating toward target
            rate = IsGrounded() ? groundAcceleration : airAcceleration;
        }
        else
        {
            // Turning around
            rate = IsGrounded() ? groundDeceleration : airDeceleration;
        }

        // Apply the control force
        float maxChange = rate * Time.fixedDeltaTime;
        float change = Mathf.Clamp(velocityDiff, -maxChange, maxChange);

        velocity.x += change;

        // Snap to zero when stopping
        if (Mathf.Abs(velocity.x) < stopThreshold && Mathf.Approximately(targetVelocity.x, 0))
        {
            velocity.x = 0;
        }
    }

    [Range(5, 100)]
    public float slopeDetectRange = 5;

    private Vector2 ResolveCollision(Vector2 dp, bool canSlide = true)
    {
        int tries = 0;
        while (tries <= 8)
        {
            CollisionData collisionData = DetectCollision(dp);

            if (!canSlide)
            {
                collisionData.normal = Vector2.up;
            }

            if (collisionData.hit)
            {
                dp += collisionData.normal * (Mathf.Abs(collisionData.separation) + 0.002f);
            }
            else
            {
                return dp;
            }
            tries++;
        }
        return Vector2.zero;
    }

    private void DescendRamp()
    {
        if (collisions.wasGrounded && !collisions.isGrounded() && velocity.y <= 0)
        {
            CollisionData checkDown = DetectCollision(slopeDetectRange * Time.fixedDeltaTime * Vector2.down);
            if (checkDown.hit)
            {
                float dist = Mathf.Abs(checkDown.separation);
                Vector2 dp = dist * Vector3.down * Time.fixedDeltaTime;
                transform.position += (Vector3)ResolveCollision(dp, false);
                CheckGrounded();
            }
        }
    }

    public CollisionData DetectCollision(Vector2 dp)
    {
        BoxCollider2D collider = colliderManager.getCollider();

        Vector2 originalOffset = collider.offset;
        Collider2D boxhitCollider;

        // float maxStep = collider.size.x/2.0f;
        // int stepsNeeded = dp / maxStep;
        // for (int i = ) {
        collider.offset = originalOffset + dp;

        boxhitCollider = Physics2D.OverlapBox(
            (Vector2)transform.position + collider.offset,
            Vector2.Scale(collider.size, (Vector2)transform.localScale),
            0,
            collisionMask
        );

        CollisionData returnData = new CollisionData();

        if (boxhitCollider)
        {
            ColliderDistance2D separationDistance = boxhitCollider.Distance(collider);

            returnData.hit = true;
            returnData.separation = separationDistance.distance;
            returnData.normal = new Vector2(
                Mathf.Round(separationDistance.normal.x * 100f) / 100f,
                Mathf.Round(separationDistance.normal.y * 100f) / 100f
            );

            returnData.collider = collider;
            returnData.otherCollider = boxhitCollider;


            collisions.setCollisionInfo(returnData);

            // Bonk Ceiling
            if (Mathf.Approximately(returnData.normal.y, -1) && velocity.y > 0)
            {
                OnBonkCeiling.Invoke();
            }

            // Land on slopes as if they were horizontal
            if (returnData.normal.y >= maxSlope)
            {
                returnData.normal = Vector2.up;
            }

            // Walk into steep walls as if they were vertical ->/ => ->|
            if (
                collisions.wasGrounded &&
                returnData.normal.y < maxSlope &&
                !SameSign(returnData.normal.x, velocity.x)
            )
            {
                returnData.normal.y = 0;
                returnData.normal.Normalize();
            }

        }

        collider.offset = originalOffset;
        return returnData;
    }

    public CollisionData CheckGrounded()
    {
        CollisionData data = DetectCollision(Vector2.up * (-groundedCheckRange));
        if (!collisions.isGrounded() && data.hit) OnLanding.Invoke();
        if (velocity.y <= 0) collisions.setGroundSlope(data.normal);
        collisions.setGrounded(velocity.y <= 0 && data.hit);

        return data;
    }
    [Range(0.002f, 0.020f)]
    public float groundedCheckRange = 0.008f;

    public bool IsGrounded()
    {
        return collisions.isGrounded();
    }

    public bool CollisionInMoveDirection()
    {
        return
            velocity.x < 0 && collisions.getLeft() ||
            velocity.x > 0 && collisions.getRight() ||
            velocity.y < 0 && collisions.getBelow() ||
            velocity.y > 0 && collisions.getAbove();
    }

    public bool HasSteepCollision()
    {
        return
            collisions.hasNormWhere(norm => Mathf.Abs(Vector2.Dot(norm, velocity.normalized)) > 0.8f, true);
    }

    // public void SetForwardVelocity(float vel)
    // {
    //     velocity.x = Mathf.Abs(vel) * entity.facing;
    // }

    // public void SetVelocity(Vector2 vel)
    // {
    //     velocity = vel;
    // }

    public void SetTargetVelocity(Vector2 vel)
    {
        targetVelocity = vel;
    }

    public void SetTargetForwardVelocity(float vel)
    {
        targetVelocity.x = Mathf.Abs(vel) * entity.facing;
    }

    public bool CheckVertDist(float dist)
    {
        return !DetectCollision(Vector2.up * dist).hit;
    }

    public float GetGravity()
    {
        return gravity * gravityMod;
    }

    #region Rework needed
    bool crushTest(bool vertical)
    {

        return false;
    }

    public bool isSafePosition()
    {
        BoxCollider2D collider = colliderManager.getCollider();

        if (!collisions.isTangible()) { return false; }
        Vector3 colliderWorldPos = transform.position + (Vector3)collider.offset;
        Vector3 boxCastOrigin = colliderWorldPos - Vector3.right * safetyMargin / 2;


        LayerMask safeGroundMask = collisionMask & ~LayerMask.GetMask("DesctructibleBlock");

        float sideDistance = 0.1f;
        float downDistance = 0.2f;

        if (showSafetyCheck)
        {
            Debug.DrawLine((Vector2)boxCastOrigin + collider.size.y / 2 * Vector2.up, (Vector2)boxCastOrigin + (safetyMargin + collider.size.x / 2) * Vector2.right - collider.size.y / 2 * Vector2.up, Color.red);
            Debug.DrawLine((Vector2)boxCastOrigin - collider.size.y / 2 * Vector2.up, (Vector2)boxCastOrigin + (safetyMargin + collider.size.x / 2) * Vector2.right + collider.size.y / 2 * Vector2.up, Color.red);

            Debug.DrawLine(transform.position, transform.position + Vector3.right * sideDistance, Color.cyan);
            Debug.DrawLine(transform.position, transform.position + Vector3.left * sideDistance, Color.cyan);
            Debug.DrawLine(colliderWorldPos + collider.size.x / 2 * Vector3.right,
                            colliderWorldPos + collider.size.x / 2 * Vector3.right + (collider.size.y + downDistance) * Vector3.down,
                            Color.cyan);
            Debug.DrawLine(colliderWorldPos + collider.size.x / 2 * Vector3.left,
                            colliderWorldPos + collider.size.x / 2 * Vector3.left + (collider.size.y + downDistance) * Vector3.down,
                            Color.cyan);
        }

        if (Physics2D.Raycast(transform.position, Vector2.right, sideDistance, safeGroundMask)) return false;
        if (Physics2D.Raycast(transform.position, -Vector2.right, sideDistance, safeGroundMask)) return false;
        if (!Physics2D.Raycast(colliderWorldPos + collider.size.x / 2 * Vector3.right, Vector2.down, collider.size.y + downDistance, safeGroundMask)) return false;
        if (!Physics2D.Raycast(colliderWorldPos + collider.size.x / 2 * Vector3.left, Vector2.down, collider.size.y + downDistance, safeGroundMask)) return false;

        if (Physics2D.BoxCast(boxCastOrigin, collider.size, 0, Vector3.right, safetyMargin, dangerMask)) return false;

        return true;
    }
    #endregion

    public void UpdateBoundaryPoints()
    {
        Bounds bounds = colliderManager.getCollider().bounds;

        boundaryPoints.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        boundaryPoints.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        boundaryPoints.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        boundaryPoints.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void debugBoundaryCollisions()
    {
        Debug.DrawLine(boundaryPoints.bottomLeft, boundaryPoints.bottomRight, collisions.getBelow() ? Color.green : Color.white);
        Debug.DrawLine(boundaryPoints.bottomLeft, boundaryPoints.topLeft, collisions.getLeft() ? Color.green : Color.white);
        Debug.DrawLine(boundaryPoints.bottomRight, boundaryPoints.topRight, collisions.getRight() ? Color.green : Color.white);
        Debug.DrawLine(boundaryPoints.topLeft, boundaryPoints.topRight, collisions.getAbove() ? Color.green : Color.white);
    }
}