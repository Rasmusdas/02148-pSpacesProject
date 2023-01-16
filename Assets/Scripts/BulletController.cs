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

        foreach(var v in NetworkServer.networkObjects)
        {
            if(v.Value.owner == nt.owner)
            {
                if (v.Value == null) continue;

                if(v.Value.TryGetComponent<PlayerController>(out PlayerController con))
                {
                    Debug.Log(con.name + " | " + name);
                    Physics.IgnoreCollision(GetComponent<Collider>(), con.GetComponent<Collider>());
                }
            }
        }
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
                        PlayerController pc = item.collider.gameObject.GetComponent<PlayerController>();
                        if (pc.shielded == 0)
                        {
                            pc.TakeDamge(damage);
                            //hitSound.Play();

                            Vector3 pos = item.collider.transform.position + new Vector3(0, 1, 0);
                            GameObject temp = Instantiate(bloodVFX, pos, Quaternion.identity);
                            ParticleSystem ps = temp.GetComponent<ParticleSystem>();
                            var sh = ps.shape;
                            sh.shapeType = ParticleSystemShapeType.Sphere;
                            var em = ps.emission;
                            em.burstCount *= 2;
                        }
                    }
                }
               
            }
            Destroy(gameObject);
        }
        else
        {
            if (collision.gameObject.tag == "Player")
            {
                PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                Instantiate(hitVFX, collision.contacts[0].point, Quaternion.identity);
                if (pc.shielded == 0)
                {
                    Instantiate(bloodVFX, collision.contacts[0].point, transform.rotation);
                }
                if (collision.gameObject.GetComponent<NetworkTransform>().owner != nt.owner)
                {

                    if (nt.isOwner)
                    {
                        if (pc.shielded == 0)
                        {
                            pc.TakeDamge(damage);
                            //hitSound.Play();
                        }
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
                Debug.Log(collision.gameObject);
                Destroy(gameObject);
            }
        }
    }
}
