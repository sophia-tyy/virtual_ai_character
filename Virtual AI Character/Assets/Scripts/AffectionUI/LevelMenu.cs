using UnityEngine;
using TMPro;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;

public class LevelMenu : MonoBehaviour
{
    public TMP_Text levelNameText;
    public TMP_Text levelNoText;
    public TMP_Text levelXpText;
    public RectTransform progressBarBackground;
    public RectTransform progressBarFill;

    void OnEnable()
    {
        int level = AffectionDataManager.Instance.GetLevel();
        levelNameText.text = AffectionSystemConstants.AffectionLevelNames[level];
        levelNoText.text = "Level " + level.ToString();

        int xp = AffectionDataManager.Instance.GetXP();
        float backgroundWidth = progressBarBackground.rect.width;
        if (level + 1 >= AffectionSystemConstants.AffectionLevelThresholds.Length){
            levelXpText.text = "MAX LEVEL";
            progressBarFill.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Left,
                0f,
                backgroundWidth
            );
        }
        else {
            levelXpText.text = xp.ToString() + "/" + AffectionSystemConstants.AffectionLevelThresholds[level + 1].ToString();
            float progress = (float)xp / AffectionSystemConstants.AffectionLevelThresholds[level + 1];
            progressBarFill.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Left,
                0f,
                backgroundWidth * progress
            );
        }
    }
}