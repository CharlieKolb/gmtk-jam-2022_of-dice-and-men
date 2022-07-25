using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirstrikeProjectileScript : MonoBehaviour
{
    public Sprite partTwo;
    public float warningTime = 1.5f;
    public float explosionTime = 0.5f;

    SpriteRenderer rend;
    Collider2D coll;

    float currentTimeAlive;
    float startTransparency;

    static bool isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        coll.enabled = false;

        currentTimeAlive = explosionTime;
        startTransparency = explosionTime * 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (coll.enabled) {
            this.transform.rotation = Quaternion.identity;
            explosionTime -= Time.deltaTime;
            if (explosionTime < 0) {
                isPlaying = false;
                Destroy(this.gameObject);
            }
            else {
                rend.color = new Color(1, 1, 1, Mathf.Clamp(explosionTime / startTransparency, 0.3f, 1));
            }
        }
        else {
            warningTime -= Time.deltaTime;
            rend.color = new Color(1, 1, 1, Mathf.Clamp(warningTime, 0.2f, 1));

            if (warningTime < 0) {
                coll.enabled = true;
                rend.sprite = partTwo;
                rend.color = new Color(1, 1, 1,1);
                if (!isPlaying) {
                    GetComponent<AudioSource>().Play();
                    isPlaying = true;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            other.GetComponent<PlayerController>().beHit();
        }
    }
}
