using Unity.Collections;
using UnityEngine;

public class BroomBehavior : MonoBehaviour
{
    public Transform SpriteTransform;
    SpriteRenderer Sprite;
    Vector2 currentDirection;

    public float gravity = -.25f;
    float thrust = 0;

    public float speed = 1;
    [Range(0, 5.0f)] public float Kp = 0.1f;
    [Range(0, 5.0f)] public float Kd = 0.1f;

    Vector2 velocity;

    void Start()
    {
        Sprite = SpriteTransform.GetComponent<SpriteRenderer>();
        velocity = Vector2.zero;
    }
float errorPrev = 0;
    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 targetVelocity = new Vector2(0, input.y * speed);

        float error = targetVelocity.y - velocity.y;
        float deltaError = (error - errorPrev)/Time.deltaTime;
        errorPrev = error;

        thrust = Kp * error + Kd * deltaError;

        Debug.DrawLine(transform.position, transform.position + (Vector3)(Vector2.up * thrust), Color.red);
    }

    void FixedUpdate() {
        Vector2 acc = Vector2.up * (thrust + gravity);
        
        velocity += acc;
        velocity = new Vector2(Mathf.Clamp(velocity.x, -10, 10), Mathf.Clamp(velocity.y, -10, 10));

        Move(velocity);
    }

    void Move(Vector2 displacement) {
        transform.position += (Vector3)displacement * Time.fixedDeltaTime;
        
        if (transform.position.y > 5.5f) {
            transform.position = new Vector3(transform.position.x, -5, 0);
        }
        if (transform.position.y < -5.5f) {
            transform.position = new Vector3(transform.position.x, 5, 0);
        }
        if (transform.position.x > 10.5f) {
            transform.position = new Vector3(-10, transform.position.y, 0);
        }
        if (transform.position.x < -10.5f) {
            transform.position = new Vector3(10, transform.position.y, 0);
        }
    }
}
