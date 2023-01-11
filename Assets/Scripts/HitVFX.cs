using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using static UnityEditor.PlayerSettings;

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

 
