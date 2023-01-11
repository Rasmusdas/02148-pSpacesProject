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
    public GameObject bloodVFX;

    AudioSource hitSound;

    Rigidbody rb;

    NetworkTransform nt;

    // Start is called before the first frame update
    void Start()
    {
        nt = GetComponent<NetworkTransform>();
        hitSound = GetComponent<AudioSource>();
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
                Instantiate(hitVFX, collision.contacts[0].point, Quaternion.identity);
                Instantiate(bloodVFX, collision.contacts[0].point, transform.rotation);
                if (nt.isOwner)
                {
                    collision.gameObject.GetComponent<PlayerController>().TakeDamge(damage);
                    //hitSound.Play
                }
                Destroy(gameObject);
            }
            
        }
        else
        {
            GameObject obj = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.Euler(collision.contacts[0].normal));
            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            ps.startColor = Color.blue;
            ps.startLifetime /= 2;
            Destroy(gameObject);
        }

        
    }
}
