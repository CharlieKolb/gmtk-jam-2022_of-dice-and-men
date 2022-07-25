using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour
{
    public Sprite partTwo;
    public float warningTime = 1f; // change in prefab
    public float laserTime = 1.5f;

    private bool done;

    SpriteRenderer rend;
    Collider2D coll;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        coll.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (coll.enabled) {
            laserTime -= Time.deltaTime;
            if (laserTime < 0) {
                Destroy(this.transform.parent.gameObject);
            }
        }
        else {
            warningTime -= Time.deltaTime;
            if (warningTime < 0) {
                coll.enabled = true;
                rend.sprite = partTwo;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            other.GetComponent<PlayerController>().beHit();
        }
    }
}
