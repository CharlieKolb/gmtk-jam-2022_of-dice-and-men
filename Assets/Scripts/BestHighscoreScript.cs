using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BestHighscoreScript : MonoBehaviour
{
    public float bestCounter;
    public float currCounter;

    TMPro.TextMeshPro text;

    string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMPro.TextMeshPro>();

        sceneName = SceneManager.GetActiveScene().name;
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name != sceneName) Destroy(this.gameObject);
        if (!Util.gameStarted) return;
    
        currCounter += Time.deltaTime;
        if (currCounter > bestCounter) bestCounter = currCounter;
        text.text = TimeSpan.FromSeconds(bestCounter).ToString(@"mm\:ss\.ff");
    }
}
