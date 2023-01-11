using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServerUI : MonoBehaviour
{
    public bool inServer;

    string joinIP = "82.211.223.108";
    string createIP = "0.0.0.0";

    string joinPort = "5555";
    string createPort = "5555";

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnGUI()
    {
        createIP = GUI.TextField(new Rect(160, 5, 100, 25), createIP);
        joinIP = GUI.TextField(new Rect(160, 35, 100, 25), joinIP);

        createPort = GUI.TextField(new Rect(265, 5, 50, 25), createPort);
        joinPort = GUI.TextField(new Rect(265, 35, 50, 25), joinPort);


        if (!inServer)
        {
            if (GUI.Button(new Rect(5, 5, 150, 25), "Start Server"))
            {
                Debug.Log("Starting Server");
                NetworkServer.running = true;
                NetworkServer.StartServer(new ServerInfo("tcp", createIP, int.Parse(createPort), "lobby", "KEEP"));
                inServer = true;
            }
            else if (GUI.Button(new Rect(5, 35, 150, 25), "Join Server"))
            {
                Debug.Log("Joining Server");
                NetworkServer.running = NetworkServer.JoinServer(new ServerInfo("tcp", joinIP, int.Parse(joinPort), "lobby", "KEEP"));
                inServer = NetworkServer.running;
            }
        }
        else
        {
            if (GUI.Button(new Rect(5, 5, 150, 25), "Spawn Player"))
            {
                NetworkServer.Instantiate("Player", new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10)), Quaternion.identity);
            }

            if (GUI.Button(new Rect(5, 35, 150, 25), "Leave"))
            {
                inServer = false;
                NetworkServer.running = false;
                NetworkServer.CloseServer(new ServerInfo("tcp", joinIP, int.Parse(joinPort), "lobby", "KEEP"));
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (GUI.Button(new Rect(5, 65, 150, 25), "Spawn Penguin"))
            {
                NetworkServer.Instantiate("NewPlayer", new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10)), Quaternion.identity);
            }
        }
    }
}
