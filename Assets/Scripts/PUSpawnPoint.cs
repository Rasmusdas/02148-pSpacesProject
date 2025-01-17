using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PUSpawnPoint : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Spawner());
    }

    bool live = false;

    IEnumerator Spawner()
    {
        int r;
        while (!live)
        {
            yield return new WaitForSeconds(2);
            if (NetworkServer.masterClient && NetworkServer.running)
            {
                r = Random.Range(1, 101);
                if (r <= 7)
                {
                    NetworkServer.Instantiate("Shield", transform.position, Quaternion.identity);
                    live = true;
                } 
                else if (r <= 25)
                {
                    NetworkServer.Instantiate("MedPak", transform.position, Quaternion.identity);
                    live = true;
                }
                else if (r <= 32)
                {
                    NetworkServer.Instantiate("Boom", transform.position, Quaternion.identity);
                    live = true;
                }
                else if (r <= 40)
                {
                    NetworkServer.Instantiate("SpeedBooster", transform.position+new Vector3(0, 0.5f, 0), Quaternion.Euler(90, 0, 0));
                    live = true;
                }

                if(r <=40)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
