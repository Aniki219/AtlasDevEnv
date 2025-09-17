using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

public class SpriteController : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Deformer deformer;
    [SerializeField] Colorizer colorizer;
    [SerializeField] ParticleMaker particleMaker;

    [SerializeField][NotNull] AnimationClip animClip;
    [SerializeField] AnimationClip animOverrideClip;
    private bool holdOverrideClip;
    private bool wasAnimEnd;

    public readonly UnityEvent OnAnimationEnd = new UnityEvent();

    private void Update()
    {
        checkClearAnimOverride();
        checkAnimEnd();
    }

    #region Animator
    public void SetAnimationClip(AnimationClip clip)
    {
        animClip = clip;
        anim.Play(clip.name);
    }

    public void SetOverrideClip(AnimationClip clip, bool hold = false)
    {
        if (!clip || animOverrideClip == clip)
        {
            return;
        }

        animOverrideClip = clip;
        holdOverrideClip = hold;

        int clipHash = Animator.StringToHash(clip.name);
        if (!anim.HasState(0, clipHash))
        {
            throw new UnityException("Clip with name: " + clip.name +
                " does not exist for GameObject: " + gameObject.name);
        }
        anim.Play(clip.name);
    }

    public void ClearOverrideClip()
    {
        animOverrideClip = null;
        // if (!animClip) {
        //     throw new UnityException("No animClip for state: ");
        // }
        anim.Play(animClip.name);
    }

    public bool IsOverrideClip(AnimationClip clip)
    {
        return animOverrideClip && animOverrideClip.Equals(clip);
    }

    private void checkClearAnimOverride()
    {
        if (animOverrideClip && !holdOverrideClip && !anim.GetCurrentAnimatorStateInfo(0).loop)
        {
            if (GetNormalizedTime() >= 1)
            {
                ClearOverrideClip();
            }
        }
    }

    /*
        Checks one for animation end using normalized time on the current clip.
        Might be worth checking or returning information on whether this was an
        override clip or not.

        Also worth considering if this should ever invoke for looping animations.
    */
    private void checkAnimEnd()
    {
        bool isAnimEnd = GetNormalizedTime() >= 1;

        if (!wasAnimEnd && isAnimEnd)
        {
            OnAnimationEnd?.Invoke();
        }

        wasAnimEnd = isAnimEnd;
    }

    public float GetNormalizedTime()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    #endregion

    public void SetFacing(int facing)
    {
        transform.localScale = new Vector3(
            facing * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    public void StartDeform(
            Vector3 to,
            float timeTo,
            float timeReturn = 0.5f,
            Vector2 offsetDir = default,
            string tag = "default",
            bool unique = false
        )
    {
        deformer.StartDeform(to, timeTo, timeReturn, offsetDir, tag, unique);
    }

    public void SetColor(Color color)
    {

    }

    public void ResetColor()
    {

    }

    public void SetMaterial(Material material)
    {

    }
    public void ResetMaterial()
    {

    }

    public void CreateStars(Vector3 position)
    {
        particleMaker.CreateParticle();
    }
}