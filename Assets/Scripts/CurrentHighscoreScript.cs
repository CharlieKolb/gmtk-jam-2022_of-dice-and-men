using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentHighscoreScript : MonoBehaviour
{
    public float currCounter;

    TMPro.TextMeshPro text;

    public GameObject bestHighscorePrefab;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMPro.TextMeshPro>();
        var found = GameObject.FindWithTag("BestHighscore");
        if (found == null) Instantiate(bestHighscorePrefab).name = "BestHighscore";
        else found.GetComponent<BestHighscoreScript>().currCounter = 0;        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Util.gameStarted) return;

        currCounter += Time.deltaTime;
        text.text = TimeSpan.FromSeconds(currCounter).ToString(@"mm\:ss\.ff");
    }
}

