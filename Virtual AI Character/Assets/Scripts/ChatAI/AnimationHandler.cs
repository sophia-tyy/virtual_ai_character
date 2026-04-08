using System.Collections.Generic;
using Live2D.Cubism.Framework.MotionFade;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Animator))]
public class EmotionAnimatorLink : MonoBehaviour
{
    [SerializeField] private AIChatbot emotionSource;

    [Header("=== Settings ===")]
    [Range(0f, 1f)] public float threshold = 0.3f;
    private Animator anim;
    private AffectionData data;
    private Dictionary<string, float> prev = new();
    private Dictionary<string, string> map = new();
    private string lastStrongestEmotion = null;
    private float lastStrongestValue = float.NaN;
    public float stabilityEpsilon = 0.1f;
    private CubismFadeController fadeCtrl;
    private int lastLineCount = -1;
    private readonly string chatHistoryFileName = "chat_history.txt";
    private string savePath;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        savePath = Path.Combine(Application.persistentDataPath, "affection.json");
        string json = File.ReadAllText(savePath);
        data = JsonUtility.FromJson<AffectionData>(json);

        if (data.xp >= 3)
        {
            Debug.Log("Enable additional emotions.");
            map = new Dictionary<string, string>
            {
                { "happy", "happy" },
                { "sad", "sad" },
                { "angry", "angry" },
                { "surprised", "surprise" },
                { "neutral", "neutral" },
                { "curious", "curious" },
                { "disgusted", "nonono" }
            };
        }
        else
        {
            Debug.Log("Enable default emotions.");
            map = new Dictionary<string, string>
            {
                { "happy", "happy" },
                { "sad", "sad" },
                { "angry", "angry" },
                { "surprised", "surprise" },
            };
        }

        if (emotionSource == null)
        {
            Debug.LogError("Assign script in AnimationHandler Inspector!!");
            enabled = false;
            return;
        }

        // for null reference error
        fadeCtrl = GetComponentInParent<CubismFadeController>();
        if (fadeCtrl != null)
        {
            fadeCtrl.enabled = false;
        }

    }

    

    private void Update()
    {
        if (emotionSource == null) return;

        int currentLineCount = GetChatHistoryLineCount();
        if (currentLineCount <= lastLineCount) return;
        lastLineCount = currentLineCount;

        Debug.Log("Emotions changed, previous emotions: ......");
        foreach (var kvp in prev)
        {
            if (kvp.Key == null) continue;
            Debug.Log($"------{kvp.Key}: {kvp.Value}------");
        }
        CheckEmotionTriggers();
        
    }

    private int GetChatHistoryLineCount()
    {
        string path = Path.Combine(Application.persistentDataPath, chatHistoryFileName);
        if (!File.Exists(path)) return 0;

        try
        {
            return File.ReadAllLines(path).Length;
        }
        catch
        {
            return lastLineCount;
        }
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

        // if (!hasPrev)
        // {
        //     CacheCurrentEmotions();
        //     return;
        // }

        bool stable = Mathf.Abs(bestVal - prevVal) <= stabilityEpsilon;
        bool isSameEmotion = lastStrongestEmotion == bestLower;

        if (stable)
        {
            if (!isSameEmotion)
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