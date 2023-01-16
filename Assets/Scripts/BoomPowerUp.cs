using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomPowerUp : MonoBehaviour
{
    public GameObject bullet;

    NetworkTransform nt;

    private void Start()
    {
        nt= GetComponent<NetworkTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 100 * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.gameObject.GetComponent<NetworkTransform>().isOwner)
            {
                Gun gun = other.GetComponentInChildren<Gun>();
                gun.bullet = bullet;
                gun.SetFullAmmo();
            }
            nt.Destroy();
        }
    }
}
