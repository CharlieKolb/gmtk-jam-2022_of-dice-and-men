using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 12;
    private float currentMovementSpeed;


    public Rigidbody2D body;

    [HideInInspector]
    public float nextMovementMultiplier = 1.0f;
    public bool lockDirection = false;
    bool canBeLocked = true;
    float hor;
    float ver;

    bool won = false;
    public GameObject endcardRendererObject;
    public List<GameObject> endcardTextObjects;
    bool waitingForSceneKey = false;

    float timeTarget = 180;

    string sceneName;

    public Sprite deadSprite;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        currentMovementSpeed = movementSpeed;
        
        Time.timeScale = 1;
        Util.gameTime = 0;
        Util.gameStarted = false;

        sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "mainScene") {
            timeTarget = float.MaxValue;
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("menuScene");
        }

        if (waitingForSceneKey && Input.anyKeyDown) {
            SceneManager.LoadScene("credits");
        }

        if (Time.timeSinceLevelLoad < 1) return;

        hor = Input.GetAxisRaw("Horizontal");
        ver = Input.GetAxisRaw("Vertical");

        if (hor != 0.0f || ver  != 0.0f) {
            Util.gameStarted = true;
        }

        if (Util.gameStarted) {
            Util.gameTime += Time.deltaTime;
            if (Util.gameTime > timeTarget) {
                winGame();
            }
        }

    }

    void FixedUpdate()
    {
        body.velocity = (((canBeLocked && lockDirection && body.velocity != Vector2.zero) ? body.velocity.normalized : new Vector2(hor, ver).normalized) * currentMovementSpeed * nextMovementMultiplier);
        nextMovementMultiplier = 1.0f;
        lockDirection = false;
        canBeLocked = true;
    }


    public IEnumerator startDeath() {
        GetComponent<AudioSource>().Play();
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f);
        GetComponent<SpriteRenderer>().sprite = deadSprite;
        yield return new WaitForSecondsRealtime(1.5f);
        Time.timeScale = 1f;
 
        SceneManager.LoadScene(sceneName);
    }

    public void beHit() {
        if (!won) {
            StartCoroutine(startDeath());
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "HurtsPlayer") {
            beHit();
        }
        if (other.tag == "Wall") {
            canBeLocked = false;
        }
    }

    IEnumerator endGame() {
        var spr = endcardRendererObject.GetComponent<SpriteRenderer>();
        while (spr.color.a < 0.9) {
            yield return new WaitForEndOfFrame();
            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, spr.color.a + 0.001f * Time.unscaledDeltaTime);
        }
        spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, 1);

        yield return new WaitForSecondsRealtime(1f);

        for (var i = 0; i < endcardTextObjects.Count - 1; ++i) {
            yield return new WaitForSecondsRealtime(3f);
            endcardTextObjects[i].SetActive(true);
        }
        yield return new WaitForSecondsRealtime(1.2f);
        endcardTextObjects[endcardTextObjects.Count - 1].SetActive(true);

        waitingForSceneKey = true;
    }


    void winGame() {
        won = true;
        foreach (var x in GameObject.FindObjectsOfType<EnemyDiceController>()) {
            x.surrender();
        }
        Time.timeScale = 0;
        StartCoroutine(endGame());
    }   


}
