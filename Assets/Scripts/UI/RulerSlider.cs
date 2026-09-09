using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RulerSlider : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Range")]
    public float minValue = 0f;
    public float maxValue = 300f;
    public float startValue = 175f;

    [Header("References")]
    public RulerTicksGraphic ticksGraphic;
    public RectTransform indicator;
    public TMP_Text valueText;
    public string unitSuffix = " inch";

    [Header("Interaction")]
    public float pxPerUnit = 30f;
    [Range(0f, 0.999f)] public float momentumDamping = 0.9f;
    public float momentumThreshold = 0.5f;
    [Range(0.01f, 0.5f)] public float snapDuration = 0.18f;
    public bool magneticSnap = true;
    public bool hapticTicks = true;

    [Header("Indicator Stretch")]
    [Range(1f, 1.5f)] public float dragStretch = 1.15f;
    [Range(0.05f, 0.4f)] public float stretchSmooth = 0.18f;

    public event Action<float> onValueChanged;

    float value;
    float velocity;
    int lastWhole;
    float indicatorScaleY;
    float lastDragX;
    float lastDragTime;
    float dragVelocity;

    bool dragging;
    bool snapping;
    bool reduceMotion;
    float snapFrom;
    float snapTarget;
    float snapTime;

    public float Value
    {
        get { return value; }
        set
        {
            float clamped = Mathf.Clamp(value, minValue, maxValue);
            snapping = false;
            velocity = 0f;
            SetValue(clamped);
        }
    }

    void Awake()
    {
        if (ticksGraphic == null) ticksGraphic = GetComponentInChildren<RulerTicksGraphic>();
        reduceMotion = RulerHaptics.IsReduceMotionEnabled();
        RulerHaptics.Prepare();
        value = Mathf.Clamp(startValue, minValue, maxValue);
        lastWhole = Mathf.RoundToInt(value);
        indicatorScaleY = 1f;
        Apply();    }

    void Update()
    {
        UpdateIndicator();

        if (dragging) return;

        if (!reduceMotion && Mathf.Abs(velocity) > momentumThreshold)
        {
            value += velocity * Time.deltaTime;
            velocity *= Mathf.Pow(momentumDamping, Time.deltaTime * 60f);
            if (Mathf.Abs(velocity) < momentumThreshold)
            {
                velocity = 0f;
                Clamp();
                Apply();
                if (magneticSnap) StartSnap();
            }
            else
            {
                Clamp();
                Apply();
            }
        }
        else if (magneticSnap && snapping)
        {
            velocity = 0f;
            StepSnap();
        }
        else if (!snapping)
        {
            velocity = 0f;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        snapping = false;
        velocity = 0f;
        dragVelocity = 0f;
        lastDragX = eventData.position.x;
        lastDragTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dx = eventData.position.x - lastDragX;
        float dt = Mathf.Max(Time.unscaledTime - lastDragTime, 0.0001f);
        lastDragX = eventData.position.x;
        lastDragTime = Time.unscaledTime;
        dragVelocity = dx / pxPerUnit / dt;

        value -= dx / pxPerUnit;
        Clamp();
        Apply();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        velocity = -dragVelocity;

        if (reduceMotion || Mathf.Abs(velocity) < momentumThreshold)
        {
            velocity = 0f;
            StartSnap();
        }
    }

    void StartSnap()
    {
        if (!magneticSnap) return;
        snapping = true;
        snapFrom = value;
        int tick = Mathf.RoundToInt(snapFrom * 1f);
        snapTarget = Mathf.Clamp((float)tick, minValue, maxValue);
        snapTime = 0f;
    }

    void StepSnap()
    {
        if (reduceMotion)
        {
            value = snapTarget;
            snapping = false;
            Apply();
            return;
        }

        snapTime += Time.deltaTime / snapDuration;
        float t = Mathf.SmoothStep(0f, 1f, snapTime);
        value = Mathf.Lerp(snapFrom, snapTarget, t);
        if (t >= 1f)
        {
            value = snapTarget;
            snapping = false;
            Apply();
        }
    }

    void UpdateIndicator()
    {
        float targetY = dragging ? dragStretch : 1f;
        float k = 1f - Mathf.Exp(-Time.deltaTime / stretchSmooth);
        indicatorScaleY = Mathf.Lerp(indicatorScaleY, targetY, k);
        if (indicator != null) indicator.localScale = new Vector3(1f, indicatorScaleY, 1f);
    }

    void Clamp()
    {
        if (value < minValue) { value = minValue; velocity = 0f; }
        else if (value > maxValue) { value = maxValue; velocity = 0f; }
    }

    void SetValue(float v)
    {
        value = v;
        Apply();
    }

    void Apply()
    {
        if (ticksGraphic != null)
        {
            ticksGraphic.pxPerUnit = pxPerUnit;
            ticksGraphic.SetRange(minValue, maxValue);
            ticksGraphic.SetValue(value);
        }

        int whole = Mathf.RoundToInt(value);
        if (whole == lastWhole) return;
        lastWhole = whole;
        if (hapticTicks && !reduceMotion) RulerHaptics.Tick();
        if (valueText != null) valueText.text = whole.ToString() + unitSuffix;
        if (onValueChanged != null) onValueChanged(whole);
    }
}