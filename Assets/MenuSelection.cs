using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSelection : MonoBehaviour
{

    int selected = 0;

    List<string> lines = new List<string>{
        "Roll of the Dice",
        "Endless",
        "The Arena"
    };

    string makeString() {
        string res = "";
        for(int i = 0; i < lines.Count; ++i) {
            if (selected == i) res += "> ";
            res += lines[i] + '\n';
        }
        return res;
    }

    TMPro.TextMeshPro text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMPro.TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) selected = (selected + 1) % lines.Count;
        if (Input.GetKeyDown(KeyCode.W)) selected = (selected - 1) % lines.Count;

        text.text = makeString();
    }
}
