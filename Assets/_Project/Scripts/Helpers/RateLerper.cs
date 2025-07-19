using UnityEngine;

[System.Serializable]
public class RateLerper
{
    [SerializeField] private readonly float startValue;
    [SerializeField] private readonly float targetValue;
    [SerializeField] private readonly float rate;
    private readonly float startTime;
    private readonly float totalTime;

    private readonly bool forwards;

    public RateLerper(
        float _startValue,
        float _targetValue,
        float _rate,
        float _startTime,
        bool _forwards = true
    )
    {
        startValue = _startValue;
        targetValue = _targetValue;
        rate = _rate;
        startTime = _startTime;
        forwards = _forwards;
        if (rate == 0)
        {
            totalTime = Mathf.Infinity;
        }
        else
        {
            totalTime = Mathf.Abs(targetValue - startValue) / rate;
        }
    }

    public RateLerper()
    {
        startValue = 0;
        targetValue = 0;
        rate = 0;
        startTime = 0;
        forwards = true;
        totalTime = 0;
    }

    public float Value()
    {
        return ValueAt(Time.time);
    }

    public float ValueAt(float time)
    {
        if (totalTime <= 0)
        {
            return startValue;
        }
        float t = (time - startTime) / totalTime;
        return forwards ?
            Mathf.Lerp(startValue, targetValue, t) :
            Mathf.Lerp(targetValue, startValue, t);
    }

    public float DeltaValue(float deltaTime)
    {
        return ValueAt(Time.time) - ValueAt(Time.time - deltaTime);
    }

    public float NormalizedValue()
    {
        return Mathf.Clamp01(
            (ValueAt(Time.time) - startValue) /
            (targetValue - startValue)
        );
    }
}
