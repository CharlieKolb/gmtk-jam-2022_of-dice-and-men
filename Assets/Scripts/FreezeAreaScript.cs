using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeAreaScript : MonoBehaviour
{
    float timeAlive = 7.5f; 
    float startTransparency = 0.2f;

    SpriteRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        startTransparency = timeAlive * 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive -= Time.deltaTime;
        if (timeAlive < 0) {
            Destroy(this.transform.parent.gameObject);
        }

        rend.color = new Color(1, 1, 1, Mathf.Clamp(timeAlive / startTransparency, 0, 1));
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            var pc = other.gameObject.GetComponent<PlayerController>();
            pc.lockDirection = true;
            pc.nextMovementMultiplier *= 1.2f;
        }
    }
}
