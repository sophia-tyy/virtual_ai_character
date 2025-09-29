using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class change_bg : MonoBehaviour
{
    public static int bg_count = 4;
    public GameObject[] bg = new GameObject[bg_count];
    public int current_bg = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void bg_changer()
    {
        int next_bg = (current_bg + 1) % bg_count;
        bg[current_bg].SetActive(false);
        bg[next_bg].SetActive(true);
        current_bg = next_bg;
    }
}
