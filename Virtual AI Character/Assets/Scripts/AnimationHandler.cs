using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EmotionAnimatorLink : MonoBehaviour
{
    [SerializeField] private AIChatbot emotionSource;

    [Header("=== Settings ===")]
    [Range(0f, 1f)] public float threshold = 0.1f;
    public bool triggerOnRiseOnly = true;       // avoid spam
    private Animator anim;
    private readonly Dictionary<string, float> prev = new();
    // map emotion keys (lowercase) to Animator trigger names
    private readonly Dictionary<string, string> map = new()
    {
        { "happy",     "curious" },
        { "sad",       "curious" },
        { "angry",     "curious" },
        { "surprised", "curious" },
        { "neutral",   "curious" },
        { "fearful",   "curious" },
        { "disgusted", "curious" }
    };
    // generalized trigger tracking
    private string lastTriggeredEmotion = null;
    private bool triggered = false;
    private float lastTriggeredValue = float.NaN;
    [Tooltip("When comparing values for stability, treat changes smaller than this as 'no change'.")]
    public float stabilityEpsilon = 0.01f;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (emotionSource == null)
        {
            Debug.LogError("[EmotionAnimatorLink] Assign script in AnimationHandler Inspector!");
            enabled = false;
            return;
        }

        // copy start values (normalize keys to lowercase)
        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key == null) continue;
            prev[kv.Key.ToLowerInvariant()] = kv.Value;
        }
    }

    private void Update()
    {
        if (emotionSource == null) return;

    // Continuously check emotions and trigger animations when conditions met
    CheckEmotionTriggers();

    }
    
    private void CheckEmotionTriggers()
    {
        // Find the strongest allowed emotion and its value (normalize keys)
        string best = null;
        string bestLower = null;
        float bestVal = float.MinValue;

        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key == null) continue;
            string keyLower = kv.Key.ToLowerInvariant();

            // only consider emotions from the allowed set (keys in map)
            if (!map.ContainsKey(keyLower)) continue;

            // value must be a finite float in [0,1]
            float v = kv.Value;
            if (float.IsNaN(v) || float.IsInfinity(v) || v < 0f || v > 1f) continue;

            if (v > bestVal)
            {
                bestVal = v;
                best = kv.Key;
                bestLower = keyLower;
            }
        }

        // No emotion or nothing above threshold -> reset trigger state if emotion changed
        if (best == null || bestVal < threshold)
        {
            // if previously triggered, reset so we can trigger later
            if (triggered)
            {
                triggered = false;
                lastTriggeredEmotion = null;
                lastTriggeredValue = float.NaN;
            }

            // update prev snapshot
            foreach (var kv in emotionSource.currentEmotions)
                prev[kv.Key] = kv.Value;
            return;
        }

        // Determine the trigger name using the map
        string trig = null;
        if (bestLower != null && map.TryGetValue(bestLower, out string mapped))
            trig = mapped;

        // If there's no mapped trigger for this emotion, just store and return
        if (trig == null)
        {
            foreach (var kv in emotionSource.currentEmotions)
                prev[kv.Key] = kv.Value;
            return;
        }

        // Check stability: we only fire when the value is stable (not changing)
        bool hasPrev = prev.TryGetValue(bestLower, out float prevVal);

        if (!hasPrev)
        {
            // First observation: store and wait for a stable value next frame
            prev[bestLower] = bestVal;
            return;
        }

        bool stable = Mathf.Abs(bestVal - prevVal) <= stabilityEpsilon;

        // If the emotion changed (not stable) and it's the currently triggered one, allow retrigger later
        if (lastTriggeredEmotion != null && lastTriggeredEmotion != bestLower)
        {
            triggered = false;
            lastTriggeredValue = float.NaN;
        }

        if (stable && !triggered)
        {
            // trigger once
            anim.SetTrigger(trig);
            triggered = true;
            lastTriggeredEmotion = bestLower;
            lastTriggeredValue = bestVal;
        }

        // If value changed sufficiently from the last triggered value, allow retrigger
        if (triggered && !float.IsNaN(lastTriggeredValue) && Mathf.Abs(bestVal - lastTriggeredValue) > stabilityEpsilon)
        {
            triggered = false;
            lastTriggeredEmotion = null;
            lastTriggeredValue = float.NaN;
        }

        // update prev snapshot
        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key == null) continue;
            prev[kv.Key.ToLowerInvariant()] = kv.Value;
        }
    }
}