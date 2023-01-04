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

    public bool isOwner;

    private void Start()
    {
        if (!isOwner) return;

    }

    private void Update()
    {
        if (!isOwner) return;
        t += Time.deltaTime;
        if (NetworkServer.masterClient && t >= 1f/updateRate)
        {
            t = 0;
            NetworkServer.MovementUpdate(new Packet(PacketType.Movement, NetworkServer.playerId, "Server", (id, transform.position)));
        }
    }

    public void UpdatePosition(Vector3 pos)
    {
        transform.position = pos;
    }

}
