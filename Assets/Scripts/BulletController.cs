using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Stats")]
    public float bulletSpeed = 50f;
    public int damage = 2;
    public bool isExplosive = false;
    public float explosiveRange = 2f;

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
        if (isExplosive)
        {
            rb.velocity = Vector3.zero;
            GetComponentInChildren<Collider>().enabled = false;
            GetComponentInChildren<MeshRenderer>().enabled = false;
            GameObject gb = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.identity);
            gb.transform.localScale = new Vector3(explosiveRange / 2, explosiveRange / 2, explosiveRange / 2);
            RaycastHit[] hits = Physics.SphereCastAll(collision.contacts[0].point, explosiveRange,transform.forward, LayerMask.GetMask("Player"));
            
            //VFX
            Instantiate(hitVFX, collision.contacts[0].point, Quaternion.identity);
            if (hits != null)
            {
                foreach (var item in hits)
                {
                    if (item.collider.tag == "Player")
                    {
                        Vector3 pos = item.collider.transform.position + new Vector3(0,1,0);
                        GameObject temp = Instantiate(bloodVFX, pos, Quaternion.identity);
                        ParticleSystem ps = temp.GetComponent<ParticleSystem>();
                        var sh = ps.shape;
                        sh.shapeType = ParticleSystemShapeType.Sphere;
                        var em = ps.emission;
                        em.burstCount *= 2;

                        item.collider.gameObject.GetComponent<PlayerController>().TakeDamge(damage);
                        //hitSound.Play();
                    }
                }
               
            }
            Destroy(gameObject);
        }
        else
        {
            if (collision.gameObject.tag == "Player")
            {
                Instantiate(hitVFX, collision.contacts[0].point, Quaternion.identity);
                Instantiate(bloodVFX, collision.contacts[0].point, transform.rotation);
                if (collision.gameObject.GetComponent<NetworkTransform>().owner != nt.owner)
                {
                    if (nt.isOwner)
                    {
                        collision.gameObject.GetComponent<PlayerController>().TakeDamge(damage);
                        //hitSound.Play();
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
}
