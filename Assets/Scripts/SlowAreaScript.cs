using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowAreaScript : MonoBehaviour
{
    private static float timeAlive = 25.0f; 
    float currentTimeAlive = SlowAreaScript.timeAlive;
    float startTransparency = SlowAreaScript.timeAlive * 0.2f;

    SpriteRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTimeAlive -= Time.deltaTime;
        if (currentTimeAlive < 0) Destroy(this.gameObject);

        rend.color = new Color(1, 1, 1, Mathf.Clamp(currentTimeAlive / startTransparency, 0, 1));
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            other.gameObject.GetComponent<PlayerController>().nextMovementMultiplier *= 0.4f + Mathf.Clamp(0.6f - (currentTimeAlive / startTransparency), 0, 0.5f);
        }
    }
}
