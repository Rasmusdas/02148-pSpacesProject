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
    private float rotDelta;
    private Quaternion prevRot;

    private void Start()
    {
        if (!isOwner) return;
        prevPos = transform.position;
    }

    private void Update()
    {
        if (!isOwner) return;
        t += Time.deltaTime;
        if (t >= 1f / updateRate)
        {
            if (Vector3.Distance(transform.position, prevPos) > moveDelta)
            {
                prevPos = transform.position;
                NetworkServer.MovementUpdate(new Packet(PacketType.Movement, NetworkServer.playerId, "Server", JsonUtility.ToJson((id, transform.position))));
            }

            //if (Quaternion.Angle(transform.rotation, prevRot) > rotDelta)
            //{
            //    prevRot = transform.rotation;
            //    NetworkServer.RotationUpdate(new Packet(PacketType.Rotation, NetworkServer.playerId, "Server", (id, transform.rotation)));
            //}
            t = 0;
        }

    }

    public void UpdatePosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void UpdateRotation(Quaternion rot)
    {
        transform.rotation = rot;
    }

}
