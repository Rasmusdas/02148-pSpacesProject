using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MedPakSpawnTimer());
        StartCoroutine(ShieldSpawnTimer());
    }

    IEnumerator MedPakSpawnTimer()
    {
        float time, x, z;
        while (true)
        {
            time = Random.Range(5f, 10f);
            x = Random.Range(-50f,50f);
            z = Random.Range(-50f,50f);
            yield return new WaitForSeconds(time);
            if (NetworkServer.running) NetworkServer.Instantiate("MedPak", new Vector3(x, 0f, z), Quaternion.identity);
        }
    }

    IEnumerator ShieldSpawnTimer()
    {
        float time, x, z;
        while (true)
        {
            time = Random.Range(10f, 20f);
            x = Random.Range(-50f, 50f);
            z = Random.Range(-50f, 50f);
            yield return new WaitForSeconds(time);
            if (NetworkServer.running) NetworkServer.Instantiate("Shield", new Vector3(x, 0f, z), Quaternion.identity);
        }
    }
}
