using System.Collections.Generic;
using Live2D.Cubism.Framework.MotionFade;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EmotionAnimatorLink : MonoBehaviour
{
    [SerializeField] private AIChatbot emotionSource;

    [Header("=== Settings ===")]
    [Range(0f, 1f)] public float threshold = 0.1f;
    public bool triggerOnRiseOnly = true;
    private Animator anim;
    private readonly Dictionary<string, float> prev = new();
    // map emotion keys to Animator trigger names
    private readonly Dictionary<string, string> map = new()
    {
        { "happy",     "happy" },
        { "sad",       "sad" },
        { "angry",     "angry" },
        { "surprised", "surprise" },
        { "neutral",   "neutral" },
        { "curiuos",   "curious" },
        { "disgusted", "nonono" }
    };

    // last triggered/handled strongest emotion to avoid duplicates
    private string lastStrongestEmotion = null;
    private float lastStrongestValue = float.NaN;

    [Tooltip("When comparing values for stability, treat changes smaller than this as 'no change'.")]
    public float stabilityEpsilon = 0.01f;

    private CubismFadeController fadeCtrl;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (emotionSource == null)
        {
            Debug.LogError("[EmotionAnimatorLink] Assign script in AnimationHandler Inspector!");
            enabled = false;
            return;
        }

        // for null reference error
        fadeCtrl = GetComponentInParent<CubismFadeController>();
        if (fadeCtrl != null)
        {
            fadeCtrl.enabled = false;
            Debug.Log("[EmotionAnimatorLink] Disabled CubismFadeController");
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

        if (EmotionsChanged())
        {
            CheckEmotionTriggers();
        }
    }

    private bool EmotionsChanged()
    {
        if (emotionSource.currentEmotions.Count != prev.Count)
            return true;

        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key == null) continue;
            string keyLower = kv.Key.ToLowerInvariant();

            if (!prev.TryGetValue(keyLower, out float prevVal))
                return true;

            if (!Mathf.Approximately(kv.Value, prevVal))
                return true;
        }

        return false;
    }
    
    private void CheckEmotionTriggers()
    {
        // Find the strongest allowed emotion and its value
        string best = null;
        string bestLower = null;
        float bestVal = float.MinValue;

        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key == null) continue;
            string keyLower = kv.Key.ToLowerInvariant();

            // check if keys in map
            if (!map.ContainsKey(keyLower)) continue;

            // check value range
            float v = kv.Value;
            if (float.IsNaN(v) || float.IsInfinity(v) || v < 0f || v > 1f) continue;

            if (v > bestVal)
            {
                bestVal = v;
                best = kv.Key;
                bestLower = keyLower;
            }
        }

        // No emotion or nothing above threshold -> skip trigger
        if (best == null || bestVal < threshold)
        {
            // update prev snapshot for next comparison
            foreach (var kv in emotionSource.currentEmotions)
            {
                if (kv.Key == null) continue;
                prev[kv.Key.ToLowerInvariant()] = kv.Value;
            }
            return;
        }

        // Determine the trigger name using the map
        string trig = null;
        if (bestLower != null && map.TryGetValue(bestLower, out string mapped))
            trig = mapped;

        // If there's no mapped trigger for this emotion, just update prev and return
        if (trig == null)
        {
            foreach (var kv in emotionSource.currentEmotions)
            {
                if (kv.Key == null) continue;
                prev[kv.Key.ToLowerInvariant()] = kv.Value;
            }
            return;
        }

        // try check stability --------------------------------------

        bool hasPrev = prev.TryGetValue(bestLower, out float prevVal);

        // if (!hasPrev)
        // {
        //     prev[bestLower] = bestVal;
        //     return;
        // }

        bool stable = Mathf.Abs(bestVal - prevVal) <= stabilityEpsilon;

        // If the emotion is stable and passes threshold, trigger it once
        if (stable)
        {
            // Avoid re-triggering for the same emotion/value (handles streaming/refinements)
            if (lastStrongestEmotion != null && lastStrongestEmotion == bestLower && Mathf.Abs(bestVal - lastStrongestValue) <= stabilityEpsilon)
            {
                // already triggered for this stable emotion/value; skip
            }
            else
            {
                anim.SetTrigger(trig);
                Debug.Log($"[EmotionAnimatorLink] Triggered animation '{trig}' for emotion '{best}' with value {bestVal:F2}");
                lastStrongestEmotion = bestLower;
                lastStrongestValue = bestVal;
            }
        }

        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key == null) continue;
            prev[kv.Key.ToLowerInvariant()] = kv.Value;
        }
    }
}