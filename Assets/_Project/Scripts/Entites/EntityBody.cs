using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

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

    [Header("Momentum")]
    public Vector3 momentum;
    public Vector3 initialMomentum;
    public float momentumStartTime;
    public float momentumHangTime;
    public float momentumDecayTime = 1.0f;

    [Header("Velocity")]
    public Vector3 velocity;
    public Vector3 velocityOut { get; private set; }
    [SerializeField] Vector3 additionalVelocity;

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
        //Keep track of "justLanded" and "justHeadBonked"
        UpdateBoundaryPoints();
        if (canMove)
        {
            Move(velocity);

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
        BoxCollider2D collider = colliderManager.getCollider();
        if (debug)
        {
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
                Debug.DrawLine(collider.bounds.center, collider.bounds.center + velocityOut.normalized, Color.white);
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
            currentGravity = gravity * Time.fixedDeltaTime * gravityMod * (velocity.y > 0 ? 1 : 1.25f);
            velocity.y = Mathf.Max(velocity.y, termVel);
        }

        velocity.y += currentGravity;
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

    public void Move(Vector3 vel)
    {

        float my = initialMomentum.y * Mathf.Clamp01(
                    Mathf.Lerp(1, 0, Mathf.Max(Time.time - momentumStartTime - momentumHangTime, 0) / momentumDecayTime)
                );
        float mx = initialMomentum.x * Mathf.Clamp01(
                    Mathf.Lerp(1, 0, Mathf.Max(Time.time - momentumStartTime - momentumHangTime, 0) / momentumDecayTime / 2)
                );
        if (IsGrounded())
        {
            mx = my = 0;
        }
        momentum = new Vector2(mx, my);

        vel += momentum;

        vel = (vel + additionalVelocity) * Time.deltaTime;

        if (!collisions.isTangible())
        {
            transform.position += vel;
            return;
        }

        if (collisions.hasNormWhere(norm => norm.y > 0) &&
            collisions.hasNormWhere(norm => norm.y < 0.75f) &&
            vel.y <= 0)
        {
            vel *= 0.8f;
        }

        if (lockPosition) { return; }

        collisions.Reset();

        Vector3 oldPosition = transform.position * 1.0f;

        Vector3 d = (Vector3)resolveCollision(vel);

        transform.position += d;
        CheckGrounded();

        if (canDescendRamps)
        {
            DescendRamp(vel);
        }

        velocityOut = transform.position - oldPosition;
    }
    [Range(5, 100)]
    public float slopeDetectRange = 5;

    private Vector2 resolveCollision(Vector2 vel, bool canSlide = true)
    {
        Vector2 dp = vel;

        int tries = 0;
        while (tries <= 8)
        {
            CollisionData collisionData = DetectCollision(dp);

            //TODO: seems wrong af??
            if (!canSlide)
            {
                collisionData.normal = Vector2.up;
            }

            if (collisionData.hit)
            {
                dp += collisionData.normal * (Mathf.Abs(collisionData.distance) + 0.002f);

                //TODO: what is this? step up??
                // if (Vector2.Dot(vel, collisionData.normal) > 0) {
                //     dp = collisionData.normal * vel.magnitude;
                // }
            }
            else
            {
                return dp;
            }
            tries++;
        }
        return Vector2.zero;
    }

    private void DescendRamp(Vector2 vel)
    {
        if (collisions.wasGrounded && !collisions.isGrounded() && vel.y <= 0)
        {
            CollisionData checkDown = DetectCollision(slopeDetectRange * Time.deltaTime * Vector2.down);
            if (checkDown.hit)
            {
                float dist = Mathf.Abs(checkDown.distance);
                transform.position += (Vector3)resolveCollision(dist * Vector3.down, false);
                CheckGrounded();
            }
        }
    }

    public struct CollisionData
    {
        public bool hit;
        public Vector2 normal;
        public float distance;
        public Collider2D collider;
        public Collider2D otherCollider;
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
            ColliderDistance2D colliderDistance = boxhitCollider.Distance(collider);

            returnData.hit = true;
            returnData.distance = colliderDistance.distance;
            returnData.normal = new Vector2(Mathf.Round(colliderDistance.normal.x * 100f) / 100f, Mathf.Round(colliderDistance.normal.y * 100f) / 100f);

            returnData.collider = collider;
            returnData.otherCollider = boxhitCollider;


            collisions.setCollisionInfo(returnData);

            if (Mathf.Approximately(returnData.normal.y, -1) && velocity.y > 0)
            {
                OnBonkCeiling.Invoke();
            }

            //Land on slopes as if they were horizontal
            if (returnData.normal.y >= maxSlope)
            {
                returnData.normal = Vector2.up;
            }

            //Walk into steep walls as if they were vertical ->/ => ->|
            if (collisions.wasGrounded && returnData.normal.y < maxSlope && Mathf.Sign(returnData.normal.x) != Mathf.Sign(velocity.x))
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

    public void AddVelocity(Vector3 amount)
    {
        if (!collisions.isTangible()) { return; }
        additionalVelocity += amount;
    }

    public void SetForwardVelocity(float vel)
    {
        velocity.x = Mathf.Abs(vel) * entity.facing;
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

    struct BoundaryPoints
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

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

        public void Reset(bool resetBelow = true)
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

    public void debugBoundaryCollisions()
    {
        Debug.DrawLine(boundaryPoints.bottomLeft, boundaryPoints.bottomRight, collisions.getBelow() ? Color.green : Color.white);
        Debug.DrawLine(boundaryPoints.bottomLeft, boundaryPoints.topLeft, collisions.getLeft() ? Color.green : Color.white);
        Debug.DrawLine(boundaryPoints.bottomRight, boundaryPoints.topRight, collisions.getRight() ? Color.green : Color.white);
        Debug.DrawLine(boundaryPoints.topLeft, boundaryPoints.topRight, collisions.getAbove() ? Color.green : Color.white);
    }
}