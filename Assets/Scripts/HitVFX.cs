using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitVFX : MonoBehaviour
{
    ParticleSystem ps;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Play();
    }
    // Update is called once per frame
    void Update()
    {
        if (!ps.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
