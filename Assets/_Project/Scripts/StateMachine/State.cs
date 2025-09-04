using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class State : MonoBehaviour
{
    public AnimationClip stateAnimation;
    public StateType stateType;

    private float startTime;
    public bool isComplete = false;

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

    public void MarkComplete()
    {
        isComplete = true;
    }

    private void OnDisable()
    {
        startTime = 0;
    }

    public void OnExit()
    {
        isComplete = false;
    }
}