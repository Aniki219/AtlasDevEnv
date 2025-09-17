using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : EntityController
{
    #region State
    //Could be in the Reset Behavior
    [HideInInspector] public Vector3 lastSafePosition;
    public bool invulnerable = false;
    #endregion

    public static PlayerController Instance;

    #region Unity functions
    public void Start()
    {
        setLastSafePosition();
        //warpToCurrentDoor();
    }

    public override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PlayerController Instance already exists");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        base.Awake();
    }

    private void LateUpdate()
    {
        checkRoomBoundaries();
        handleControllerDebug();

        if (isGrounded())
        {
            checkSafePosition();
        }

        if (body.showSafetyCheck)
        {
            Debug.DrawLine(lastSafePosition, lastSafePosition + Vector3.up, Color.blue);
        }
    }

    private void checkSafePosition()
    {
        if (body.velocity.y <= 0 && body.isSafePosition())
        {
            lastSafePosition = transform.position;
        }
    }

    public int GetFacing()
    {
        return facing;
    }

    private void handleControllerDebug()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            body.debug = !body.debug;
            PlayerManager.Instance.ShowStateDisplay(body.debug);
            canvas.Shoutout("Debug " + (body.debug ? "On" : "Off"));
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            body.showNormal = !body.showNormal;
            canvas.Shoutout("Collision Normals " + (body.showNormal ? "On" : "Off"));
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            body.showVelocityNormal = !body.showVelocityNormal;
            canvas.Shoutout("Velocity Normal " + (body.showVelocityNormal ? "On" : "Off"));
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            body.showCollisionResolution = !body.showCollisionResolution;
            canvas.Shoutout("Collision Highlights " + (body.showCollisionResolution ? "On" : "Off"));
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            body.highlightGrounded = !body.highlightGrounded;
            canvas.Shoutout("Highlight Grounded " + (body.highlightGrounded ? "On" : "Off"));
        }

        //Rework to contact SpriteController
        if (body.debug)
        {
            //     if (body.highlightGrounded)
            //     {
            //         if (isGrounded())
            //         {
            //             FlashColor flashColor = FlashColor.builder
            //                                                 .withColor(Color.green)
            //                                                 .withTimeUnits(TimeUnits.CONTINUOUS)
            //                                                 .build();
            //             deformer.flashColor(flashColor);
            //         }
            //         else
            //         {
            //             FlashColor flashColor = FlashColor.builder
            //                                                 .withColor(Color.red)
            //                                                 .withTimeUnits(TimeUnits.CONTINUOUS)
            //                                                 .build();
            //             deformer.flashColor(flashColor);
            //         }
            //     }
            //     else
            //     {
            //         //TODO: NO
            //         deformer.endFlashColor();
            //     }
            // }
            // else
            // {
            //     //TODO: NO
            //     deformer.endFlashColor();
            // }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        List<Collider2D> hitters = new List<Collider2D>();
        other.GetContacts(hitters);

        //Because the hitboxes appear as children, we have to filter AllyHitboxes out
        //Otherwise you could get hurt by whacking brambles or something
        //We can probably use this to implement hitlag when hurting things..
        foreach (Collider2D h in hitters)
        {
            if (h.tag == "AllyHitbox") continue;
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("BroomTrigger"))
        {
            other.SendMessage("OnBroomCollide");
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Danger") && (!invulnerable || other.CompareTag("ResetDamaging")))
        {

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Tornado")
        {

        }

        if (other.CompareTag("Door"))
        {

        }

        if (other.CompareTag("ResetDamaging"))
        {

        }

        if (other.CompareTag("Water"))
        {

        }
    }
    #endregion

    #region Helpers
    public void setLastSafePosition()
    {
        lastSafePosition = transform.position;
    }

    //TODO: this should be on particlecreator
    public void createStars(Vector3? position = null)
    {
        if (position == null) position = transform.position;
        Instantiate(Resources.Load<GameObject>("Prefabs/Effects/StarParticles"), (Vector3)position + Vector3.up * 0.5f, Quaternion.Euler((facing == 1) ? 180 : 0, 90, 0));
    }

    //Flip horizontal without turn animation
    public void flipHorizontal()
    {
        Debug.LogWarning("flipHorizontal is deprecated, use EntityController.SetFacing");
        SetFacing(-(int)Mathf.Sign(sprite.transform.localScale.x));
        sprite.transform.localScale = new Vector3(Mathf.Abs(sprite.transform.localScale.x) * facing, 1, 1);
    }

    void checkRoomBoundaries()
    {

    }
    #endregion

    #region isBools
    public bool isGrounded()
    {
        return body.collisions.isGrounded();
    }

    public Vector2 getVelocity()
    {
        return new Vector2(body.velocity.x, body.velocity.y);
    }
    #endregion
}
