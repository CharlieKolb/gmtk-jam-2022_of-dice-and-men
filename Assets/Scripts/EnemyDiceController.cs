using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Mode {
    One, // Stab charge
    Two,
    Three,
    Four, // Slow Area
    Five, // Laser beam
    Six,
    Rolling,
}

public enum AttackState {
    Idle,
    Rotating,
    Attacking,
}

static class Util {
    public static List<Mode> outcomes = new List<Mode>{
        Mode.Rolling,
        Mode.One,
        Mode.Two,
        Mode.Three,
        Mode.Four,
        Mode.Five, 
        Mode.Six,
    };

    public static List<float> attackDelay = new List<float>{
        0.5f,
        0.25f,
        0.25f,
        3.0f,
        0.25f,
        0.25f,
        0.5f,
    };

    public static List<float> postAttackDelay = new List<float>{
        0.0f,
        0.25f,
        1.0f,
        3.0f,
        2.5f,
        2.0f,
        2.0f,
    };


    public static List<float> preAttackDelay = new List<float>{
        0.5f,
        1.5f,
        0.5f,
        3.0f,
        0.5f,
        0.0f,
        1.5f,
    };

    public static List<bool> isPlaying = new List<bool> {
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };

    public static float defaultYaw = 90; 


    public static bool gameStarted = false;
    public static float gameTime;
}

public class EnemyDiceController : MonoBehaviour
{
    public List<int> rerollOptions = new List<int>{
        1, 2, 3, 4, 5, 6,
    };
    int mode;
   //============================================================
    public float rerollDuration = 2.5f;
    private float currentRerollDuration;

    private float timeBetweenSwitchesDuringReroll = 0.2f;
    private float currentTimeBetweenSwitchesDuringReroll;

    public List<Sprite> baseSprites;
    public List<Sprite> idleAndAttackSprites;
    public List<AudioClip> attackClips;

    //============================================================
    public GameObject freezeAreaPrefab;
    public GameObject slowAreaPrefab;
    public GameObject laserPrefab;
    public GameObject explosionPrefab;


    SpriteRenderer spriteRenderer;
    SpriteRenderer armsRenderer;
    AudioSource armsAudioSource;

    AttackState attackState = AttackState.Idle;
    Vector2 attackGoalPosition;
    public float turnSpeed = 3.0f;

    public float delayFactor = 1.2f;

    private PlayerController player;

    Collider2D attackColl;
    Rigidbody2D rb;


    // Start is called before the first frame update
    void Start()
    {
        currentRerollDuration = rerollDuration;
        currentTimeBetweenSwitchesDuringReroll = timeBetweenSwitchesDuringReroll;
        spriteRenderer = GetComponent<SpriteRenderer>();
        attackColl = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        armsRenderer = transform.Find("Arms").GetComponent<SpriteRenderer>();
        armsAudioSource = transform.Find("Arms").GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        var opt = getValidRandomOption();
        spriteRenderer.sprite = baseSprites[opt];
        armsRenderer.sprite = idleAndAttackSprites[opt * 2];
        StartCoroutine(changeMode(0));
    }

    int getValidRandomOption() {
        return rerollOptions[Random.Range(0, rerollOptions.Count)];
    }
    
    
    float maxDistancePerSecondAttackOne = 14f;
    IEnumerator updateOne(int run) {
        var maxTimeCharging = 5f;
        armsAudioSource.clip = attackClips[1];
        armsAudioSource.loop = false;
        armsAudioSource.Play();
        yield return new WaitForSeconds(Util.attackDelay[1]* delayFactor);
        if (currRun != run) {
            attackColl.enabled = false;
        };

        var chargeStart = Time.time;
        while (maxTimeCharging > 0 && Vector2.Distance(transform.position, attackGoalPosition) > 0.1) {
            transform.position = Vector2.MoveTowards(transform.position, attackGoalPosition, maxDistancePerSecondAttackOne * Time.deltaTime);
            if (currRun != run || Time.time - chargeStart > 5) {
                attackColl.enabled = false;
                armsAudioSource.Stop();
                yield break;
            }
            maxTimeCharging -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        armsAudioSource.Stop();

        if (currRun != run) {
            attackColl.enabled = false;
            yield break;
        }
        yield return new WaitForSeconds(Util.postAttackDelay[1]* delayFactor);
        if (currRun != run) {
            attackColl.enabled = false;
            yield break;
        }
        StartCoroutine(changeMode());

    }
    IEnumerator updateTwo(int run) {
        yield return new WaitForSeconds(Util.attackDelay[2]* delayFactor);
        if (currRun != run) yield break;
        // start attack
        armsAudioSource.clip = attackClips[2];
        armsAudioSource.loop = false;
        armsAudioSource.Play();

        
        var left = Instantiate(freezeAreaPrefab, transform);
        var right = Instantiate(freezeAreaPrefab, transform);

        left.transform.localPosition -= new Vector3(1.5f, 0f, 0f);
        left.name += "1";
        left.transform.parent = null;

        right.transform.localPosition += new Vector3(1.5f, 0f, 0f);
        right.transform.parent = null;

        if (currRun != run) yield break;
        yield return new WaitForSeconds(Util.postAttackDelay[2]* delayFactor);
        if (currRun != run) yield break;
        StartCoroutine(changeMode());

    }
    IEnumerator updateThree(int run) { 
        yield return new WaitForSeconds(Util.attackDelay[3]* delayFactor);
        armsAudioSource.clip = attackClips[3];
        armsAudioSource.loop = true;
        armsAudioSource.Play();

        if (currRun != run) { 
            armsAudioSource.Stop();
            yield break;
        }
        yield return new WaitForSeconds(Util.postAttackDelay[3]* delayFactor);
        if (currRun != run) {
            armsAudioSource.Stop();
            yield break;
        }        
        
        transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);
        if (transform.localScale.x > 2f) {
            rerollOptions.Remove(3);
            if (rerollOptions.Count == 0) {
                rerollOptions = new List<int>{ 1, 2, 4, 5, 6 };
            }
        }
        armsAudioSource.Stop();

        StartCoroutine(changeMode());
    }   

    float maxDistancePerSecondAttackFour = 25f;
    IEnumerator updateFour(int run) {
        yield return new WaitForSeconds(Util.attackDelay[4]* delayFactor);

        if (currRun != run) yield break;
        var chargeStart = Time.time;

        while (Vector2.Distance(transform.position, attackGoalPosition) > 0.1) {
            transform.position = Vector2.MoveTowards(transform.position, attackGoalPosition, maxDistancePerSecondAttackFour * Time.deltaTime);
            if (currRun != run || Time.time - chargeStart > 5) yield break;
            yield return new WaitForEndOfFrame();
        }
        armsAudioSource.clip = attackClips[5];
        armsAudioSource.loop = false;
        armsAudioSource.Play();

        Instantiate(slowAreaPrefab, transform).transform.parent = null;
        if (currRun != run) yield break;
        yield return new WaitForSeconds(Util.postAttackDelay[4]* delayFactor);
        if (currRun != run) yield break;
        StartCoroutine(changeMode());
    }
    IEnumerator updateFive(int run) {
        yield return new WaitForSeconds(Util.attackDelay[5]* delayFactor);
        if (currRun != run) yield break;
        var lazer = Instantiate(laserPrefab, transform);
        lazer.transform.parent = null;

        if (!Util.isPlaying[5]) {
            armsAudioSource.clip = attackClips[6];
            armsAudioSource.loop = false;
            armsAudioSource.Play();
            Util.isPlaying[5] = true;
        }
        

        var delay = Util.postAttackDelay[5]* delayFactor;
        while (delay > 0) {
            delay -= Time.deltaTime;

            if (currRun != run){
                Destroy(lazer);
                Util.isPlaying[5] = false;
                armsAudioSource.Stop();
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
        Util.isPlaying[5] = false;
        armsAudioSource.Stop();
        StartCoroutine(changeMode());        
    }
    IEnumerator updateSix(int run) {
        yield return new WaitForSeconds(Util.attackDelay[6]* delayFactor);
        // start attack
        if (currRun != run) yield break;
        
        for (int i = 0; i < 16; ++i) {
            var a = Instantiate(explosionPrefab, transform);
            a.transform.localPosition -= new Vector3(i % 2 == 0 ? -1.5f : 1.5f, 3 + 1.75f * i, 0);
            a.GetComponent<AirstrikeProjectileScript>().warningTime = (1.0f + (i/2*2) / 4f);
            a.transform.parent = null;
        } 

        if (currRun != run) yield break;
        yield return new WaitForSeconds(Util.postAttackDelay[6]* delayFactor);
        if (currRun != run) yield break;


        StartCoroutine(changeMode());        
    }


    private int prevRoll;
    IEnumerator updateReroll(int run) {
        var currentRerollDuration = rerollDuration * Random.Range(0.5f, 2f);
        
        while (currentRerollDuration > 0) {
            yield return new WaitForSeconds(timeBetweenSwitchesDuringReroll* delayFactor);
            if (currRun != run) yield break;
            currentRerollDuration -= timeBetweenSwitchesDuringReroll;
            int opt;
            if (rerollOptions.Count > 1) {
                int retry = 5;
                do {
                    opt = getValidRandomOption();
                } while (retry-- > 0 && opt == prevRoll);
                prevRoll = opt;
            }
            else {
                opt = rerollOptions[0];
            }



            if (currRun != run) yield break;
            spriteRenderer.sprite = baseSprites[opt];
            armsRenderer.sprite = idleAndAttackSprites[opt * 2];
        }

        if (currRun != run) yield break;
        armsAudioSource.Stop();

        StartCoroutine(changeMode(getValidRandomOption()));
    }


    // Update is called once per frame
    void Update()
    {
        if (!Util.gameStarted) return;

        if (attackState == AttackState.Rotating) {
            Vector2 Dir = player.transform.position - transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, Util.defaultYaw + Mathf.Atan2(Dir.y, Dir.x) * Mathf.Rad2Deg), 2 * Time.deltaTime * turnSpeed * Mathf.Rad2Deg);;    
            if (Vector2.Dot((player.transform.position - transform.position).normalized, -transform.up) > 0.995) {
                attackState = AttackState.Attacking;
                if (mode == 1) {
                    attackColl.enabled = true;
                }
                else {
                    attackColl.enabled = false;
                }

                attackGoalPosition = (Vector2) player.transform.position + player.body.velocity * (Random.Range(0, 1));
            }
        }
    }

    // void FixedUpdate() {
    //     if (targetRotation != null) {
    //         rb.MoveRotation((float) targetRotation);
    //     }
    //     if (targetPosition != null) {
    //         rb.velocity = ((Vector3) targetPosition - transform.position).normalized * maxDistancePerSecondAttackOne * Time.fixedDeltaTime;
    //     }
    // }
    int currRun = 0;
    IEnumerator changeMode(int newMode = -1) {
        while (!Util.gameStarted) {
            yield return new WaitForEndOfFrame();
        }
        canStop = false;
        var run = ++currRun;

        if (newMode == -1) newMode = rerollOptions.Count == 1 ? rerollOptions[0] : 0;

        attackState = AttackState.Idle;
        attackColl.enabled = false;
        

        mode = newMode;

        if (newMode != 0) {
            spriteRenderer.sprite = baseSprites[newMode];
            armsRenderer.sprite = idleAndAttackSprites[newMode * 2];
        }
        else {
            armsRenderer.sprite = idleAndAttackSprites[14];
        }

        
        StartCoroutine(startAttack(run));
    }

    bool canStop = false;
    IEnumerator startAttack(int run) {
        if (mode != 0) {
            armsRenderer.sprite = (idleAndAttackSprites[mode * 2 + 1]);
        }
        canStop = true;

        yield return new WaitForSeconds(Util.preAttackDelay[mode]* delayFactor);
        if (run != currRun) { yield break; }
        attackState = AttackState.Rotating;
        if (run != currRun) { yield break; }
        attackGoalPosition = player.transform.position;


        while (mode != 0 && attackState == AttackState.Rotating) {
            if (run != currRun) { yield break; }
            yield return new WaitForEndOfFrame();
        }

        switch (mode) {
            case 0:
                StartCoroutine(updateReroll(run));
                break;
            case 1:
                StartCoroutine(updateOne(run));
                break;
            case 2:
                StartCoroutine(updateTwo(run));
                break;
            case 3:
                StartCoroutine(updateThree(run));
                break;
            case 4:
                StartCoroutine(updateFour(run));
                break;
            case 5:
                StartCoroutine(updateFive(run));
                break;
            case 6:
                StartCoroutine(updateSix(run));
                break;
        }
    }


    private float lastCollTime = -5;
    private void OnCollisionEnter2D(Collision2D coll) {
        if (coll.otherCollider.tag == "Wall") return;
        if (!canStop) return;
        if (this.transform.localScale.x > 2) return;
        if (Time.time - lastCollTime < 4) return;
        lastCollTime = Time.time;

        spriteRenderer.sprite = baseSprites[0];
        StartCoroutine(changeMode(0));
    }


    public void surrender() {
        currRun += 1;
        spriteRenderer.sprite = baseSprites[7];
        armsRenderer.sprite = idleAndAttackSprites[15];
        attackState = AttackState.Idle;
        attackColl.enabled = false;
        Vector2 Dir = player.transform.position - transform.position;
        transform.rotation = Quaternion.Euler(0, 0, Util.defaultYaw + Mathf.Atan2(Dir.y, Dir.x) * Mathf.Rad2Deg);    
    }
}
