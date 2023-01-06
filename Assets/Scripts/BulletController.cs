using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Stats")]
    public float bulletSpeed = 50f;
    public float damge = 2f;

    [Header("VFX")]
    public GameObject hitVFX;
    
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
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
            collision.gameObject.GetComponent<PlayerController>().TakeDamge(damge);
        }

        Instantiate(hitVFX, collision.contacts[0].point, Quaternion.Euler(collision.contacts[0].normal));
       
        Destroy(gameObject);
    }
}