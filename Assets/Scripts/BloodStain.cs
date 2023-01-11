using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodStain : MonoBehaviour
{
    public float minSize, maxSize;
    public float despawnTime;

    // Start is called before the first frame update
    void Start()
    {
        float temp = Random.Range(minSize, maxSize);
        transform.localScale= new Vector3(temp,transform.localScale.y, temp);

        float rot = Random.Range(-360, 360);
        transform.RotateAroundLocal(transform.up, rot);

        float time = Random.Range(despawnTime/2, despawnTime+ despawnTime / 2);
        Destroy(gameObject, time);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
