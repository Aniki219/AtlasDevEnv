using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deformer : MonoBehaviour
{
    [SerializeField] const Material defaultMaterial = null;
    [SerializeField] const Material flashMaterial = null;
    [SerializeField] const SpriteController sprite = null;

    Collider2D col;

    Vector3 startPosition;
    Vector3 startRotation;
    Vector3 startScale;

    private int facing = 1;

    Vector3 totalScale = Vector3.one;
    Vector3 totalOffset = Vector3.zero;

    float frameCount;

    [SerializeField]
    public List<Oscillator> oscillators;
    [SerializeField]
    public List<Deformation> deforms;

    public FlashColor flashColorRef;

    public void Start()
    {

        //deforms = new List<Deformation>();

        //frameCount is to remain sync'd for all oscillators
        frameCount = (int)UnityEngine.Random.Range(0, 100) * Time.deltaTime;

        startPosition = transform.localPosition;
        startRotation = transform.localEulerAngles;
        startScale = transform.localScale;

        //Setup Deformers
        foreach (Deformation d in deforms)
        {
            d.startTime += Time.time;
        }

        //Setup Oscillators
        foreach (Oscillator o in oscillators)
        {
            switch ((int)o.axis)
            {
                case 0:
                    o.oscillationDirection = transform.right;
                    break;
                case 1:
                    o.oscillationDirection = transform.up;
                    break;
                case 2:
                    o.oscillationDirection = transform.forward;
                    break;
            }
            if (o.rotational)
            {
                transform.localEulerAngles -= o.oscillationDirection * o.oscillationSize * 0.5f;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        frameCount += Time.deltaTime;

        totalScale = Vector3.one;
        totalOffset = Vector3.zero;

        RemoveDisabledDeforms();
        UpdateDeformers();
        UpdateOscillators();

        startScale = Vector3.Scale(
            new Vector3(
                Mathf.Abs(startScale.x),
                Mathf.Abs(startScale.y),
                Mathf.Abs(startScale.z)
            ),
            new Vector3(
                Mathf.Sign(transform.localScale.x),
                Mathf.Sign(transform.localScale.y),
                Mathf.Sign(transform.localScale.z)
            ));

        transform.localScale = Vector3.Scale(startScale, totalScale);
        transform.localPosition = totalOffset;
    }

    public void setFacing(float vel)
    {
        // //During Movement we can keep track of the direction the player is facing each frame
        // if (Mathf.Approximately(vel, 0)) return;
        // transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facing, transform.localScale.y, transform.localScale.z);
    }

    public int getFacing()
    {
        return facing;
    }

    void UpdateOscillators()
    {
        //totalOffset = startPosition;
        foreach (Oscillator o in oscillators)
        {
            if (!o.enabled) continue;
            if (o.rotational)
            {
                float angle = Mathf.Sin(frameCount * 2.0f * Mathf.PI * o.cyclesPerSecond);
                transform.localRotation = Quaternion.Euler(
                    startRotation +
                    o.oscillationDirection *
                    angle *
                    o.oscillationSize *
                    transform.localScale.y); //Upside down switches CW to CCW
            }
            else
            {
                totalOffset += o.oscillationDirection * Mathf.Sin(frameCount * 2.0f * Mathf.PI * o.cyclesPerSecond) * o.oscillationSize;
            }
        }
    }

    public void SetOscillatorActive(string tag = "default", bool on = true)
    {
        foreach (Oscillator o in oscillators)
        {
            if (o.tag == tag)
            {
                o.enabled = on;
            }
        }
    }

    void UpdateDeformers()
    {
        foreach (Deformation d in deforms)
        {
            if (!d.enabled) continue;

            Vector3 newScale = Vector3.one;
            Vector3 newOffset = Vector3.zero;

            float elapsedTime = Time.time - d.startTime;

            if (elapsedTime <= d.timeTo)
            {
                newScale = Vector3.Lerp(Vector3.one, d.to, elapsedTime / d.timeTo);
            }
            else
            {
                if (d.timeReturn < 0)
                {
                    newScale = d.to;
                }
                else
                {
                    newScale = Vector3.Lerp(d.to, Vector3.one, (elapsedTime - d.timeTo) / d.timeReturn);
                }
            }

            if (col != null)
            {
                newOffset = startPosition +
                    Vector3.up * (col.bounds.extents.y - col.offset.y) * (1.0f - newScale.y) * d.offsetDir.y +
                    Vector3.right * (col.bounds.extents.x - col.offset.x) * (1.0f - newScale.x) * d.offsetDir.x;
            }

            if (d.timeReturn > 0 && elapsedTime >= d.timeTo + d.timeReturn)
            {
                d.enabled = false;
            }

            totalScale.Scale(newScale);
            totalOffset += newOffset;
        }
    }

    void RemoveDisabledDeforms()
    {
        deforms.RemoveAll(d => !d.enabled);
    }

    public void RemoveDeform(string tag)
    {
        deforms.RemoveAll(d => d.tag.Equals(tag));
    }

    public void StartDeform(Vector3 to, float timeTo, float timeReturn = 0.5f, Vector2 offsetDir = default, string tag = "default", bool unique = false)
    {
        Deformation newDeform = new Deformation(to, timeTo, timeReturn, offsetDir);
        newDeform.tag = tag;
        if (unique && tag != "default" && tag != null && tag != "")
        {
            if (deforms.Find(d => d.tag.Equals(tag)) != null)
            {
                return;
            }
        }
        deforms.Add(newDeform);
    }

    public void flashColor(FlashColor _flashColor = null)
    {
        // if (flashColorRef != null && flashColorRef.Equals(_flashColor)) return;
        // endFlashColor();
        // if (_flashColor == null) {
        //     flashColorRef = FlashColor.builder
        //         .withColor(Color.white)
        //         .withTimeUnits(TimeUnits.COUNTS)
        //         .withCycleCount(1)
        //         .build();
        // } else {
        //     flashColorRef = _flashColor;
        // }
        // flashColorRef.task = StartCoroutine(nameof(performFlashColor));
    }

    public void endFlashColor()
    {
        if (flashColorRef == null || flashColorRef.task == null) return;
        StopCoroutine(flashColorRef.task);
        sprite.ResetMaterial();
        sprite.ResetColor();
        flashColorRef = null;
    }

    private IEnumerator performFlashColor()
    {
        int counts = 0;
        float startTime = Time.time;
        if (!flashColorRef.tint) sprite.SetMaterial(flashMaterial);
        while (condition())
        {
            counts++;
            sprite.SetColor(flashColorRef.color);
            if (flashColorRef.timeUnits == TimeUnits.SECONDS)
            {
                yield return new WaitForSeconds(flashColorRef.effectDuration);
                break;
            }
            yield return new WaitForSeconds(0.2f);
        }

        endFlashColor();

        bool condition()
        {
            if (flashColorRef.timeUnits.Equals(TimeUnits.COUNTS))
            {
                return counts < flashColorRef.effectDuration;
            }
            if (flashColorRef.timeUnits.Equals(TimeUnits.SECONDS))
            {
                return startTime + flashColorRef.effectDuration < Time.time;
            }
            //Continous
            return true;
        }
    }


}

public enum TimeUnits
{
    SECONDS,
    COUNTS,
    CONTINUOUS
}

public class FlashColor
{
    public TimeUnits timeUnits { get; private set; }
    public float effectDuration { get; private set; }
    public float cycleDuration { get; private set; }
    public int cycleCount { get; private set; }
    public Color color { get; private set; }
    public bool tint { get; private set; }

    public Coroutine task;

    private FlashColor(FlashColorRequest fcrb)
    {
        timeUnits = fcrb.timeUnits;
        effectDuration = fcrb.effectDuration;
        cycleDuration = fcrb.cycleDuration;
        cycleCount = fcrb.cycleCount;
        color = fcrb.color;
        tint = fcrb.tint;
    }

    public bool Equals(FlashColor other)
    {
        if (other == null) return false;
        return color.Equals(other.color);
    }

    public static FlashColorRequest builder;

    public struct FlashColorRequest
    {
        public TimeUnits timeUnits;
        public float effectDuration;
        public float cycleDuration;
        public int cycleCount;
        public Color color;
        public bool tint;

        public FlashColorRequest withTimeUnits(TimeUnits timeUnitsToUse)
        {
            timeUnits = timeUnitsToUse;
            return this;
        }

        public FlashColorRequest withEffectDuration(float effectDurationToUse)
        {
            effectDuration = effectDurationToUse;
            return this;
        }

        public FlashColorRequest withCycleDuration(float cycleDurationToUse)
        {
            cycleDuration = cycleDurationToUse;
            return this;
        }

        public FlashColorRequest withCycleCount(int cycleCountToUse)
        {
            cycleCount = cycleCountToUse;
            return this;
        }

        public FlashColorRequest withColor(Color colorToUse)
        {
            color = colorToUse;
            return this;
        }

        public FlashColorRequest withTint(bool isTint)
        {
            tint = isTint;
            return this;
        }

        public FlashColor build()
        {
            return new FlashColor(this);
        }
    }
}

[System.Serializable]
public class Oscillator
{
    public enum Axes
    {
        x = 0,
        y = 1,
        z = 2
    }

    public string tag = "default";
    public bool enabled = true;
    public Axes axis = Axes.x;
    public bool rotational = false;
    public float cyclesPerSecond = 1.0f;
    public float oscillationSize = 5.0f;
    [HideInInspector] public Vector3 oscillationDirection;

    public Oscillator()
    {
        cyclesPerSecond = 1.0f;
        axis = Axes.x;
        oscillationSize = 5.0f;
        rotational = false;
        tag = "default";
        enabled = true;
    }
}

[System.Serializable]
public class Deformation
{
    public string tag = "default";
    public bool enabled = true;
    public Vector3 to;
    public float timeTo;
    public float timeReturn = 0.5f;
    public Vector2 offsetDir = Vector2.zero;
    public float startTime;

    public Deformation(Vector3 to, float timeTo, float timeReturn = 0.5f, Vector2 offsetDir = default)
    {
        this.to = to;
        this.timeTo = timeTo;
        this.timeReturn = timeReturn;
        this.offsetDir = offsetDir;
        startTime = Time.time;
    }
}
