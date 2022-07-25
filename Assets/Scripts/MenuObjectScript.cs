using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuObjectScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) {
            SceneManager.LoadScene("mainScene");
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            SceneManager.LoadScene("endlessScene");
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            SceneManager.LoadScene("sevenScene");
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            SceneManager.LoadScene("credits");
        }
    }
}
