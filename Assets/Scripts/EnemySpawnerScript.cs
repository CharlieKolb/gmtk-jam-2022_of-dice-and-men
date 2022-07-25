using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float countdown;

    public List<int> options;
    public float? rerollDuration;
    public float? turnSpeed;
    public float? delayFactor;


    //==================================
    private float cycleCooldown = 0.3f;
    private float currentCycleCooldown;

    public List<Sprite> enemySprites;

    SpriteRenderer spriteRenderer;
    SpriteRenderer armsRenderer;

    public GameObject audioPlayerPrefab;
    public AudioClip success;
    public AudioClip failure;


    float cancelCountdown = 2f;
    bool playerInRange = false;
    TMPro.TextMeshPro stopText;
    TMPro.TextMeshPro spawnText;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        armsRenderer = transform.Find("Arms").GetComponent<SpriteRenderer>();
        stopText = transform.Find("stopButton/stopText").GetComponent<TMPro.TextMeshPro>();
        stopText.text = string.Format("{0:0.0}", cancelCountdown);
        spawnText = transform.Find("stopButton/spawnText").GetComponent<TMPro.TextMeshPro>();
    }


    private int prevRoll = -1;

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        spawnText.text = string.Format("{0:0.0}", countdown);

        if (playerInRange) {
            cancelCountdown -= Time.deltaTime;
            stopText.text = string.Format("{0:0.0}", cancelCountdown);
        }

        if (countdown <= 0) {
            var audio = Instantiate(audioPlayerPrefab).GetComponent<AudioSource>();
            audio.clip = failure;
            audio.loop = false;
            audio.Play();
            var enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            var edc = enemy.GetComponent<EnemyDiceController>();
            edc.rerollOptions = options;
            if (rerollDuration.HasValue) edc.rerollDuration = rerollDuration.Value;
            if (turnSpeed.HasValue) edc.turnSpeed = turnSpeed.Value;
            if (delayFactor.HasValue) edc.delayFactor = delayFactor.Value;
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, Vector3.zero, Random.Range(0.25f, 2.5f));
            Destroy(this.gameObject);
        }
        else if (cancelCountdown <= 0) {
            var audio = Instantiate(audioPlayerPrefab).GetComponent<AudioSource>();
            audio.clip = success;
            audio.loop = false;
            audio.Play();
            Destroy(this.gameObject);
        }


        currentCycleCooldown -= Time.deltaTime;
        if (currentCycleCooldown < 0) {
            currentCycleCooldown = cycleCooldown;
            int opt;
            if (options.Count > 1) {
                int retry = 5;
                do {
                    opt = getValidRandomOption();
                } while (retry-- > 0 && opt == prevRoll);
                prevRoll = opt;
            }
            else {
                opt = options[0];
            }

            var edc = enemyPrefab.GetComponent<EnemyDiceController>();
            spriteRenderer.sprite = edc.baseSprites[opt];
            armsRenderer.sprite = edc.idleAndAttackSprites[opt * 2];
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            playerInRange = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            playerInRange = false;
        }
    }

    int getValidRandomOption() {
        return options[Random.Range(0, options.Count)];
    }
}
