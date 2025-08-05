using UnityEngine;
using UnityEngine.Timeline;

public class FaceSpriteDirection : MonoBehaviour
{
    [SerializeField] Transform sprite;

    void Update()
    {
        transform.localScale = new(AtlasHelpers.Sign(sprite.transform.localScale.x), 1, 1);
    }
}
