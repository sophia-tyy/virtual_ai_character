using System.Collections.Generic;
using Live2D.Cubism.Framework.MotionFade;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EmotionAnimatorLink : MonoBehaviour
{
    [SerializeField] private AIChatbot emotionSource;

    [Header("=== Settings ===")]
    [Range(0f, 1f)] public float threshold = 0.3f;
    private Animator anim;
    private readonly Dictionary<string, float> prev = new();
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
    private string lastStrongestEmotion = null;
    private float lastStrongestValue = float.NaN;
    public float stabilityEpsilon = 0.1f;
    private CubismFadeController fadeCtrl;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (emotionSource == null)
        {
            Debug.LogError("[EmotionAnimatorLink] Assign script in AnimationHandler Inspector");
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
    
    private void CacheCurrentEmotions()
    {
        prev.Clear();
        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key != null)
                prev[kv.Key.ToLowerInvariant()] = kv.Value;
        }
    }
    private void CheckEmotionTriggers()
    {
        // find strongest emotion ------------------------------------
        string best = null;
        string bestLower = null;
        float bestVal = float.MinValue;

        foreach (var kv in emotionSource.currentEmotions)
        {
            if (kv.Key == null) continue;
            string keyLower = kv.Key.ToLowerInvariant();

            if (!map.ContainsKey(keyLower)) continue;

            float v = kv.Value;
            if (float.IsNaN(v) || float.IsInfinity(v) || v < 0f || v > 1f) continue;

            if (v > bestVal)
            {
                bestVal = v;
                best = kv.Key;
                bestLower = keyLower;
            }
        }

        if (best == null || bestVal < threshold)
        {
            CacheCurrentEmotions();
            return;
        }

        string trig = null;
        if (bestLower != null && map.TryGetValue(bestLower, out string mapped))
            trig = mapped;

        if (trig == null)
        {
            CacheCurrentEmotions();
            return;
        }

        // check stability ---------------------------------------------

        bool hasPrev = prev.TryGetValue(bestLower, out float prevVal);

        if (!hasPrev)
        {
            CacheCurrentEmotions();
            return;
        }

        bool stable = Mathf.Abs(bestVal - prevVal) <= stabilityEpsilon;
        bool isSameEmotion = lastStrongestEmotion == bestLower;

        if (stable)
        {
            if (isSameEmotion)
            {
                //skip
            }
            else
            {
                anim.SetTrigger(trig);
                Debug.Log($"[EmotionAnimatorLink] Triggered animation '{trig}' for emotion '{best}' with value {bestVal:F2}");
                lastStrongestEmotion = bestLower;
                lastStrongestValue = bestVal;
            }
        }

        CacheCurrentEmotions();
    }
}