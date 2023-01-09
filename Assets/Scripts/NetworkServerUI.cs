using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkServerUI : MonoBehaviour
{
    public bool inServer;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnGUI()
    {
        if(!inServer)
        {
            if (GUI.Button(new Rect(5, 5, 150, 25), "Start Server"))
            {
                Debug.Log("Starting Server");
                NetworkServer.StartServer(new ServerInfo("tcp", "0.0.0.0", 5555, "test", "KEEP"));
                inServer = true;
            }
            else if (GUI.Button(new Rect(5, 35, 150, 25), "Join Server"))
            {
                Debug.Log("Joining Server");
                NetworkServer.JoinServer(new ServerInfo("tcp", "82.211.223.108", 5555, "test", "KEEP"));
                inServer = true;
            }
        }
        else
        {
            if (GUI.Button(new Rect(5, 5, 150, 25), "Spawn Player"))
            {
                NetworkServer.Instantiate("Player", new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10)), Quaternion.identity);
            }
        }
    }
}
