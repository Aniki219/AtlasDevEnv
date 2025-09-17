using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{

    [SerializeField] Collider2D roomBounds;
    [SerializeField] EntityBody target;

    Camera cam;
    Vector3 shakeValue;
    Vector3 shakeVelocity;

    float w;
    float h;

    public float percentX = 0;
    public float percentY = 0;

    public float trackingFactor = 0.5f;

    Vector3 velocity;
    Vector3 focusVelocity;
    public float smoothingTime = 0.5f;
    public bool cameraTracking = true;
    Vector3 focusPoint;
    Bounds bounds;

    void Start()
    {
        cam = GetComponent<Camera>();
        bounds = GameObject.Find("RoomBounds").GetComponent<BoxCollider2D>().bounds;
        h = cam.orthographicSize;
        w = h * Screen.width / Screen.height;

        target = PlayerController.Instance.GetComponentInChildren<EntityBody>();

        Vector3 to = target.transform.position;
        to.x = Mathf.Clamp(to.x, bounds.min.x + w, bounds.max.x - w);
        to.y = Mathf.Clamp(to.y, bounds.min.y + h, bounds.max.y - h);
        transform.position = new Vector3(to.x, to.y, transform.position.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPoint = target.transform.position;
        Vector3 to;

        if (!cameraTracking)
        {
            to = targetPoint;
            to.x = Mathf.Clamp(to.x, bounds.min.x + w, bounds.max.x - w);
            to.y = Mathf.Clamp(to.y, bounds.min.y + h, bounds.max.y - h);
            transform.position = new Vector3(to.x, to.y, transform.position.z);
            return;
        }
        Vector3 focusTarget = targetPoint + Vector3.up + target.velocity.x * Vector3.right * trackingFactor;
        focusPoint = Vector3.SmoothDamp(focusPoint, focusTarget, ref focusVelocity, smoothingTime / 200.0f);
        to = new Vector3(focusPoint.x, focusPoint.y, transform.position.z);

        float minX = bounds.min.x + w;
        float maxX = bounds.max.x - w;

        float minY = bounds.min.y + h;
        float maxY = bounds.max.y - h;

        to.x = Mathf.Clamp(to.x, minX, maxX);
        to.y = Mathf.Clamp(to.y, minY, maxY);


        float smoothX = Mathf.Lerp(transform.position.x, to.x, smoothingTime / 10.0f);
        float smoothY = Mathf.Lerp(transform.position.y, to.y, smoothingTime / 4.0f);
        transform.position = new Vector3(smoothX, smoothY, transform.position.z) + shakeValue;
        transform.position = Vector3.SmoothDamp(transform.position, to, ref velocity, smoothingTime);

        percentX = 0;
        percentY = 0;

        if (maxX - minX > 0) percentX = (transform.position.x - minX) / (maxX - minX);
        if (maxY - minY > 0) percentY = (transform.position.y - minY) / (maxY - minY);
    }

    public void StartShake(float shakeMagnitude = 0.1f, float shakeDuration = 0.5f)
    {
        StartCoroutine(shakeCoroutine(shakeMagnitude, shakeDuration));
    }

    IEnumerator shakeCoroutine(float shakeMagnitude, float shakeDuration)
    {
        float startTime = Time.time;
        while (Time.time - startTime < shakeDuration)
        {
            shakeValue = (Vector3)Random.insideUnitCircle * shakeMagnitude;
            yield return new WaitForEndOfFrame();
        }
        shakeValue = Vector3.zero;
    }
}
