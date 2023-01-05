using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkTransform : MonoBehaviour
{

    [Range(1, 100)]
    public int updateRate;
    public bool syncTrans, syncRot, syncScale;
    public int id;
    public float t;
    public bool register;
    public float moveDelta = 0.3f;
    public bool isOwner;

    Vector3 prevPos;

    private void Start()
    {
        if (!isOwner) return;
        prevPos = transform.position;
    }

    private void Update()
    {
        if (!isOwner) return;
        t += Time.deltaTime;
        if (t >= 1f/updateRate && Vector3.Distance(transform.position,prevPos) < moveDelta)
        {
            t = 0;
            prevPos = transform.position;
            NetworkServer.MovementUpdate(new Packet(PacketType.Movement, NetworkServer.playerId, "Server", (id, transform.position)));
        }
    }

    public void UpdatePosition(Vector3 pos)
    {
        transform.position = pos;
    }

}
