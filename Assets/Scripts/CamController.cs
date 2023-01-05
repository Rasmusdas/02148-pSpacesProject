using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public GameObject player;

    Vector3 ofset;

    // Start is called before the first frame update
    void Start()
    {
        ofset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position + ofset, 1f);
    }
}
