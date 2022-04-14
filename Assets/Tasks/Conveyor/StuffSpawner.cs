using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuffSpawner : MonoBehaviour
{
    public List<GameObject> Stuff = new List<GameObject>();
    private int spawn_index = 0;
    public bool RandomStuff = true;
    public bool RandomMotion = true;
    public float Velocity = 0.2f;
    public float Rate = 1;
    private float t = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 randomDirection() {
        float theta, phi;
        theta = Random.Range(0, Mathf.PI);
        phi = Random.Range(0, 2 * Mathf.PI);

        return new Vector3(
            Mathf.Sin(theta) * Mathf.Cos(phi),
            Mathf.Cos(theta),
            Mathf.Sin(theta) * Mathf.Sin(phi)
        );
    }

    void spawn() {
        if (Stuff.Count == 0) {
            return;
        }

        GameObject toSpawn;
        if (!RandomStuff) {
            toSpawn = Stuff[spawn_index];
            spawn_index = (spawn_index + 1) % Stuff.Count;
        } else {
            int index = (int) Mathf.Floor(Random.Range(0, Stuff.Count - 1e-5f));
            toSpawn = Stuff[index];
        }

        var spawned = Instantiate(toSpawn, transform);
        spawned.transform.localPosition = Vector3.zero;
        if (RandomMotion) {
            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.velocity = randomDirection() * Velocity;
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        float inc = 1 / Rate;
        while (t >= inc) {
            spawn();
            t -= inc;
        }
    }
}
