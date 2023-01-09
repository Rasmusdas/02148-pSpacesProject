using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Player : MonoBehaviour
{

    public bool inst;
    public GameObject gb;
    public bool register;
    public bool inst2;

    private void Start()
    {
        string json = JsonUtility.ToJson(("test",1,2,3));

        Debug.Log(json);

        var fromJson = ((string, int, int, int))JsonUtility.FromJson(json,typeof((string,int,int,int)));

        Debug.Log(fromJson);
    }

    public void Update()
    {
        if(inst)
        {
            inst = false;

            if (register)
            {
                Debug.Log("Starting Server");
                NetworkServer.StartServer(new ServerInfo("tcp", "0.0.0.0", 5555, "test", "KEEP"));
            }
            else
            {
                Debug.Log("Joining Server");
                NetworkServer.JoinServer(new ServerInfo("tcp", "82.211.223.108", 5555, "test", "KEEP"));
            }
        }

        if(inst2)
        {
            inst2 = false;
            NetworkServer.Instantiate("Player",new Vector3(UnityEngine.Random.Range(-10, 10),0, UnityEngine.Random.Range(-10, 10)),Quaternion.identity);
        }
    }
}
