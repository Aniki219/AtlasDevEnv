using UnityEngine;
using UnityEngine.Timeline;

public class FaceSpriteDirection : MonoBehaviour
{
    [SerializeField] Transform sprite;

    void Update()
    {
        transform.localScale = new(
            transform.localScale.x * AtlasHelpers.Sign(sprite.transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }
}
