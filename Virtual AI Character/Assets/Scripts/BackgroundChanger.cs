using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundChanger : MonoBehaviour
{
    [Header("Private Variable")]
    [SerializeField] private static int backgroundCount = 4;
    [SerializeField] private GameObject[] backgroundObjects = new GameObject[backgroundCount];
    [SerializeField] private int currentBackgroundIndex = 0;

    public void ChangeBackgroundImage()
    {
        int nextBackgroundIndex = (currentBackgroundIndex + 1) % backgroundCount;
        backgroundObjects[currentBackgroundIndex].SetActive(false);
        backgroundObjects[nextBackgroundIndex].SetActive(true);
        currentBackgroundIndex = nextBackgroundIndex;
    }

    public void ChangeBackgroundMusic()
    {
        
    }
}
