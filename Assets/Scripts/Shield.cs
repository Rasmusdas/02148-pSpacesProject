using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        transform.Rotate(0, 100 * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerController>().Shield(3);
            if(other.GetComponent<NetworkTransform>().isOwner)
            {
                NetworkServer.Instantiate("PUSpawner", transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
