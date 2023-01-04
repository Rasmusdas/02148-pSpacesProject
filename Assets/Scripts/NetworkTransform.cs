using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTransform : MonoBehaviour
{

    [Range(1, 100)]
    public int updateRate;
    public bool syncTrans, syncRot, syncScale;
    public int id;
    public float t;
    public bool register;

    private void Start()
    {
        if (register)
        {
            Debug.Log("Starting Server");
            NetworkServer.StartServer(new ServerInfo("tcp","127.0.0.1",1234,"test","CONN"));
            NetworkServer.Instantiate(this);
        }
        else
        {
            Debug.Log("Joining Server");
            NetworkServer.JoinServer(new ServerInfo("tcp", "127.0.0.1", 1234, "test", "CONN"));
        }
    }

    private void Update()
    {
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
