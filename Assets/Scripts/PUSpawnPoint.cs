using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PUSpawnPoint : MonoBehaviour
{
    // Start is called before the first frame update
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
                Debug.Log("I think i am master");
                r = Random.Range(1, 101);
                if (r <= 10)
                {
                    NetworkServer.Instantiate("Shield", transform.position, Quaternion.identity);
                    live = true;
                } 
                else if (r <= 30)
                {
                    NetworkServer.Instantiate("MedPak", transform.position, Quaternion.identity);
                    live = true;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && live && NetworkServer.masterClient)
        {
            live = false;
            StartCoroutine(Spawner());
        }
    }
}
