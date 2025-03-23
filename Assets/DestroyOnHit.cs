using UnityEngine;

public class DestroyOnHit : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Hitbox"))
        {
            if (other.GetComponent<SpriteRenderer>().enabled)
            {
                Destroy(gameObject);
            }
        }
    }
}
