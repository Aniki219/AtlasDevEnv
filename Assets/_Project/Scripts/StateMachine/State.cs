using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class State : MonoBehaviour
{
    public AnimationClip stateAnimation;

    private float startTime;

    private void OnEnable()
    {
        startTime = Time.time;
    }

    public float GetElapsedTime()
    {
        return Time.time - startTime;
    }

    public float GetNomalizedTime(float duration)
    {
        return Mathf.Clamp01(GetElapsedTime() / duration);
    }

    private void OnDisable()
    {
        startTime = 0;
    }
}