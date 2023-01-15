using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedPak : MonoBehaviour
{
    public int health;

    public void Update()
    {
        transform.Rotate(0, 100*Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (other.gameObject.GetComponent<NetworkTransform>().isOwner)
            {
                other.gameObject.GetComponent<PlayerController>().AddHealth(health);
            }
            gameObject.GetComponent<NetworkTransform>().Destroy();
        }
    }
}