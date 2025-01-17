using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class ConfettiVFX : MonoBehaviour
{
    public GameObject gb;
    ParticleSystem ps;
    ParticleCollisionEvent[] collisionEvents;
    List<Vector3> poses;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Callback;

        ps.Play();

        collisionEvents = new ParticleCollisionEvent[0];
    }
    // Update is called once per frame
    void Update()
    {
        if (!ps.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        int collCount = ps.GetSafeCollisionEventSize();

        if (collCount > collisionEvents.Length)
            collisionEvents = new ParticleCollisionEvent[collCount];

        int eventCount = ps.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < eventCount; i++)
        {
            //TODO: Do your collision stuff here. 
            // You can access the CollisionEvent[i] to obtaion point of intersection, normals that kind of t$$anonymous$$ng
            // You can simply use "other" GameObject to access it's rigidbody to apply force, or check if it implements a class that takes damage or whatever
            Vector3 pos = collisionEvents[i].intersection;
            //Quaternion rot = Quaternion.Euler(collisionEvents[i].normal);
            //GameObject temp = Instantiate(gb, pos, Quaternion.identity);
            Quaternion rot = Quaternion.identity;

            rot.SetLookRotation(-collisionEvents[i].normal);
            float size = Random.Range(0.05f, 0.2f);
            ConfettiVFXRenderer.Instance.Add(pos,rot, new Vector3(size, size, size));
        }

    }

    //private void OnParticleSystemStopped()
    //{
    //    Instantiate(gb);
    //}
}
