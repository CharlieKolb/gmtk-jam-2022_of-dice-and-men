using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UrlButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene("menuScene");
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("menuScene");
        }

    }

    public void OpenCharlieURL()
    {
        Application.OpenURL("http://www.linktr.ee/CharlieKolb");
    }
}
