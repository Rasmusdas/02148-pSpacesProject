using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    public float speed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, -100 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.gameObject.GetComponent<NetworkTransform>().isOwner)
        {
            other.gameObject.GetComponent<PlayerController>().moveSpeed += speed;
            Destroy(gameObject);
        }
    }
}
