using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Stats")]
    public float bulletSpeed = 50f;
    public int damage = 2;

    [Header("VFX")]
    public GameObject hitVFX;
    
    Rigidbody rb;

    NetworkTransform nt;

    // Start is called before the first frame update
    void Start()
    {
        nt = GetComponent<NetworkTransform>();
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * bulletSpeed;
        Destroy(gameObject, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if(collision.gameObject.GetComponent<NetworkTransform>().owner != nt.owner)
            {
                Instantiate(hitVFX, collision.contacts[0].point, Quaternion.Euler(collision.contacts[0].normal));
                if(NetworkServer.playerId == nt.owner)
                {
                    collision.gameObject.GetComponent<PlayerController>().TakeDamge(damage);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            Instantiate(hitVFX, collision.contacts[0].point, Quaternion.Euler(collision.contacts[0].normal));
            Destroy(gameObject);
        }

        
    }
}
