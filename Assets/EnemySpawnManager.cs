using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public static class Spopts {
    public static List<int> melee = new List<int>{ 1, 4 };
    public static List<int> ranged = new List<int>{ 5, 6 };
    public static List<int> support = new List<int>{ 2, 4 };
    public static List<int> growth = new List<int>{ 3 };


    public static List<int> make((float, float, float, float) chances) {
        return make(chances.Item1, chances.Item2, chances.Item3, chances.Item4);
    }
    public static List<int> make(float meleeChance, float rangedChance, float supportChance, float growthChance) {
        var res = new HashSet<int>();

        foreach (var e in new List<(float, List<int>)>{ (meleeChance, melee), (rangedChance, ranged), (supportChance, support), (growthChance, growth) }) {
            float chance = e.Item1;
            var values = e.Item2;
            while (chance > 1 || Random.value < chance) {
                chance -= 1;
                int opt;
                int retry = 10;
                do {
                    opt = values[Random.Range(0, values.Count)];
                } while (retry-- > 0 && res.Contains(opt));
                res.Add(opt);
            }
        }
        if (res.Count == 0) return make(meleeChance, rangedChance, supportChance, growthChance);
        return res.ToList();
    }

    public static (float, float, float, float) earlyMelee = (1.1f, 0f, 0f, 0.2f); 
    public static (float, float, float, float) midMelee = (1.5f, 0.25f, 0.25f, 0.8f); 
    public static (float, float, float, float) lateMelee = (2f, 0.1f, 0.1f, 1f); 

    public static (float, float, float, float) earlyRanged = (0f, 1.5f, 0f, 0.2f); 
    public static (float, float, float, float) midRanged = (0f, 2f, 0.75f, 0.8f); 
    public static (float, float, float, float) lateRanged = (0f, 2f, 0.1f, 1f); 

    public static (float, float, float, float) earlyAllrounder = (0.5f, 0.75f, 0.75f, 0.2f); 
    public static (float, float, float, float) midAllrounder = (0.6f, 0.9f, 0.9f, 0.8f); 
    public static (float, float, float, float) lateAllrounder = (2f, 2f, 2f, 1f); 


    public static List<(float, float, float, float)> lategameUnits = new List<(float, float, float, float)>{
        lateMelee,
        lateRanged,
        lateAllrounder
    };


}

public enum SpawnStrategy {
    CloseToPlayer,
    FarFromPlayer,
    TopRow, // exactly 3
    MidRow, // exactly 3
    BottomRow, // exactly 3
    Random,
    Center,
    TopCenter,
}

public class SpawnPlan {
    public float spawnTime;
    public List<(float, float, float, float)> spawns = new List<(float, float, float, float)>();
    public SpawnStrategy strategy = SpawnStrategy.Center;
    public float countDown = 10f;
    public float turnSpeed = 4.0f;
    public float delayFactor = 1.0f;
    public float rerollDuration = 2.0f;
}




public class EnemySpawnManager : MonoBehaviour
{
    // Manually sorted!
    List<SpawnPlan> spawnPlan;

    public float distanceToCornerPoint;

    //==============================

    public GameObject enemyPrefab;
    public GameObject enemySpawnerPrefab;

    GameObject player;

    //==============================

    // Start is called before the first frame update
    void Start()
    {
        var name = SceneManager.GetActiveScene().name;
        spawnPlan = name == "mainScene" ? mainSceneSortedList() : (name == "endlessScene" ? endlessSceneList() : new List<SpawnPlan>());
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        handleSpawns();
    }

    void handleSpawns() {
        while (spawnPlan.Count > 0 && spawnPlan[0].spawnTime < Util.gameTime) {
            var e = spawnPlan[0];
            spawnPlan.RemoveAt(0);

            fulfillSpawnPlan(e);

        }
    }

    void fulfillSpawnPlan(SpawnPlan plan) {
        var spawners = plan.spawns
            .Select(x => Spopts.make(x))
            .Select(x => {
                var obj = Instantiate(enemySpawnerPrefab);
                var scr = obj.GetComponent<EnemySpawnerScript>();
                scr.countdown = plan.countDown;
                scr.options = x;
                scr.turnSpeed = plan.turnSpeed;
                scr.delayFactor = plan.delayFactor;
                scr.rerollDuration = plan.rerollDuration;
                // scr.turnSpeed
                return scr;
            }).ToList();
        switch (plan.strategy) {
            case SpawnStrategy.CloseToPlayer:
                placeCloseTo(spawners, player.transform.position); break;
            case SpawnStrategy.FarFromPlayer:
                placeFarFromPlayer(spawners); break;
            case SpawnStrategy.TopRow:
                placeRow(spawners, distanceToCornerPoint); break;
            case SpawnStrategy.MidRow:
                placeRow(spawners, 0); break;
            case SpawnStrategy.BottomRow:
                placeRow(spawners, -distanceToCornerPoint); break;
            case SpawnStrategy.Random:
                foreach (var ess in spawners) {
                    ess.transform.position = RandomPointInBox(transform.position, new Vector2(distanceToCornerPoint, distanceToCornerPoint), new Vector2(4, 4));
                }
                break;
            case SpawnStrategy.Center:
                foreach (var ess in spawners) {
                    ess.transform.position = RandomPointInBox(transform.position, new Vector2(3, 3));
                }
                break;
            case SpawnStrategy.TopCenter:
                spawners[0].transform.position = new Vector2(0, distanceToCornerPoint);
                break;

        }

        // spawners aren't centered -> adjust position manually
        spawners.ForEach(x => x.transform.position -= new Vector3(1f, 0, 0));
    }

    void placeCloseTo(List<EnemySpawnerScript> spawners, Vector2 position) {
        var toAvoid = new List<Vector2>{ position };
        foreach (var ess in spawners) {
            Vector2 vec;
            int retries = 10;
            do {
                vec = RandomPointInBox(position, new Vector2(8, 8), new Vector2(4, 4));

                if (toAvoid.Any(x => {
                    var mag = (vec - x).magnitude;
                    return mag < 4;
                })) {
                    if(--retries < 0) break;
                    continue;
                }
                else {
                    break;
                }
            } while (true);

            if (retries <= 0) {
                placeCloseTo(new List<EnemySpawnerScript>{ ess }, transform.position);
            }
            toAvoid.Add(vec);
            ess.transform.position = vec;
        }
    }

    void placeFarFromPlayer(List<EnemySpawnerScript> spawners) {
        foreach (var ess in spawners) {
            Vector2 vec;
            int retries = 10;
            do {
                vec = RandomPointInBox(transform.position, new Vector3(distanceToCornerPoint, distanceToCornerPoint, 0));
            } while ((vec - (Vector2) player.transform.position).magnitude < 7.5 && --retries > 0);

            if (retries <= 0) {
                placeCloseTo(new List<EnemySpawnerScript>{ ess }, transform.position);
            }

            ess.transform.position = vec;
        }
    }

    void placeRow(List<EnemySpawnerScript> spawners, float y) {
        spawners[0].transform.position = new Vector2(-distanceToCornerPoint, y);
        spawners[1].transform.position = new Vector2(0, y);
        spawners[2].transform.position = new Vector2(distanceToCornerPoint, y);
    }

    private static Vector2 RandomPointInBox(Vector2 center, Vector2 size, Vector2 avoidSize) {
        return center + new Vector2(
            (Random.value >= 0.5 ? 1 : -1 ) * Random.Range(avoidSize.x / size.x, 1) * size.x,
            (Random.value >= 0.5 ? 1 : -1 ) * Random.Range(avoidSize.y / size.y, 1) * size.y
        );
    }

    private static Vector2 RandomPointInBox(Vector2 center, Vector2 size) {
        return center + new Vector2(
            (Random.value - 0.5f) * size.x,
            (Random.value - 0.5f) * size.y
        );
    }


    List<SpawnPlan> endlessSceneList() {
        var l = new List<SpawnPlan>();
        StartCoroutine(endlessAdder(l));
        return l;
    }

    IEnumerator endlessAdder(List<SpawnPlan> list) {
        list.Add(new SpawnPlan {
            spawnTime = 0f,
            spawns = Spopts.lategameUnits,
            strategy = SpawnStrategy.MidRow,
            countDown = 1f,
        });

        list.Add(new SpawnPlan {
            spawnTime = 2f,
            spawns = new List<(float, float, float, float)>{
                Spopts.earlyRanged,
                Spopts.earlyRanged,
                Spopts.earlyRanged,
            },
            strategy = SpawnStrategy.TopRow,
            countDown = 15f,
        });


        list.Add(new SpawnPlan {
            spawnTime = 2f,
            spawns = new List<(float, float, float, float)>{
                Spopts.earlyMelee,
                Spopts.earlyMelee,
                Spopts.earlyMelee,
            },
            strategy = SpawnStrategy.BottomRow,
            countDown = 15f,
        });

        list.Add(new SpawnPlan {
            spawnTime = 25f,
            spawns = new List<(float, float, float, float)>{
                Spopts.midAllrounder,
                Spopts.midAllrounder,
                Spopts.midAllrounder,
            },
            strategy = SpawnStrategy.MidRow,
            countDown = 20,
        });

        list.Add(new SpawnPlan {
            spawnTime = 25f,
            spawns = new List<(float, float, float, float)>{
                Spopts.midMelee,
                Spopts.midMelee,
                Spopts.midMelee,
            },
            strategy = SpawnStrategy.BottomRow,
            countDown = 20f,
        });

        var nextDespawnTime = 45f;
        var turnSpeed = 4f;
        var delayFactor = 1.0f;
        var rerollDuration = 2.0f;
        var countdownMin = 20f;
        var countdownMax = 40f;
        while (true) {
            while (list.Count < 20) {
                var spawnTime = nextDespawnTime + Random.Range(0, 10);
                var countdown = Random.Range(countdownMin, countdownMax);
                nextDespawnTime = spawnTime + countdown;
                var spawns = new List<(float, float, float, float)>();
                float n = countdown/Random.Range(4f, 10f);
                for(int i = 0; i < n; ++i) {
                    spawns.Add(Spopts.lategameUnits[Random.Range(0, 3)]);
                }

                var statChange = Random.value;
                if (statChange < 0.33) turnSpeed = Mathf.Clamp(turnSpeed + 0.5f, 4, 10);
                else if (statChange < 0.66) delayFactor = Mathf.Clamp(delayFactor - 0.05f, 0.5f, 1);
                else rerollDuration = Mathf.Clamp(rerollDuration - 0.1f, 2, 4);

                var countdownChange = Random.value;
                if (countdownChange < 0.5) countdownMin = Mathf.Clamp(countdownMin - 1f, 10, 20);
                else countdownMax = Mathf.Clamp(countdownMax - 1, 30, 40);

                list.Add(new SpawnPlan {
                    spawnTime = spawnTime,
                    spawns = spawns,
                    strategy = SpawnStrategy.Random,
                    countDown = Random.value < 0.05 ? 5f : countdown,
                    turnSpeed = turnSpeed,
                    delayFactor = delayFactor,
                    rerollDuration = rerollDuration,
                });
            }
            yield return new WaitForSeconds(1);
        }
        
    }


    List<SpawnPlan> mainSceneSortedList() {
        var sl = new List<SpawnPlan>();

        // Hardcoded into the level
        // sl.Add(new SpawnPlan {
        //     spawnTime = 0f,
        //     spawns = new List<(float, float, float, float)>{
        //         Spopts.earlyMelee,
        //     },
        //     strategy = SpawnStrategy.Center,
        //     countDown = 0f,
        // });

        sl.Add(new SpawnPlan {
            spawnTime = 5f,
            spawns = new List<(float, float, float, float)>{
                Spopts.earlyAllrounder,
                Spopts.earlyRanged,
                Spopts.earlyAllrounder,
            },
            strategy = SpawnStrategy.TopRow,
            countDown = 10f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 10f,
            spawns = new List<(float, float, float, float)>{
                Spopts.midRanged,
            },
            strategy = SpawnStrategy.Center,
            countDown = 2f,
        });
        
        sl.Add(new SpawnPlan {
            spawnTime = 18f,
            spawns = new List<(float, float, float, float)>{
                Spopts.earlyAllrounder,
            },
            strategy = SpawnStrategy.TopCenter,
            countDown = 1.4f,
        });

        
        sl.Add(new SpawnPlan {
            spawnTime = 20f,
            spawns = new List<(float, float, float, float)>{
                Spopts.earlyAllrounder,
                Spopts.midRanged,
                Spopts.earlyAllrounder,
            },
            strategy = SpawnStrategy.BottomRow,
            countDown = 10f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 40f,
            spawns = new List<(float, float, float, float)>{
                Spopts.earlyAllrounder,
                Spopts.midMelee,
                Spopts.earlyAllrounder,
            },
            strategy = SpawnStrategy.BottomRow,
            countDown = 35f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 40f,
            spawns = new List<(float, float, float, float)>{
                Spopts.midRanged,
                Spopts.midMelee,
                Spopts.midRanged,
            },
            strategy = SpawnStrategy.TopRow,
            countDown = 45f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 40f,
            spawns = new List<(float, float, float, float)>{
                Spopts.midMelee,
                Spopts.midAllrounder,
                Spopts.midMelee,
            },
            strategy = SpawnStrategy.MidRow,
            countDown = 55f,
        });


        sl.Add(new SpawnPlan {
            spawnTime = 85f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateMelee,
                Spopts.lateMelee,
                Spopts.lateMelee,
            },
            strategy = SpawnStrategy.TopRow,
            countDown = 30f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 85f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateRanged,
                Spopts.lateRanged,
                Spopts.lateRanged,
            },
            strategy = SpawnStrategy.BottomRow,
            countDown = 30f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 120f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
            },
            strategy = SpawnStrategy.MidRow,
            countDown = 25,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 120f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
            },
            strategy = SpawnStrategy.TopRow,
            countDown = 25,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 148f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
            },
            strategy = SpawnStrategy.TopRow,
            countDown = 30f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 148f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
            },
            strategy = SpawnStrategy.MidRow,
            countDown = 30f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 148f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
            },
            strategy = SpawnStrategy.BottomRow,
            countDown = 30f,
        });

        sl.Add(new SpawnPlan {
            spawnTime = 148f,
            spawns = new List<(float, float, float, float)>{
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
                Spopts.lateAllrounder,
            },
            strategy = SpawnStrategy.Random,
            countDown = 30f,
        });

        return sl;
    }
}
