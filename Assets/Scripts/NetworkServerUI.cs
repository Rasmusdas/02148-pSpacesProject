using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServerUI : MonoBehaviour
{
    public static bool inServer;

    static string joinIP = "127.0.0.1";
    static string createIP = "0.0.0.0";

    static string joinPort = "5555";
    static string createPort = "5555";

    bool ready;

    private void OnGUI()
    {
        if (!inServer)
        {
            createIP = GUI.TextField(new Rect(160, 5, 100, 25), createIP);
            joinIP = GUI.TextField(new Rect(160, 35, 100, 25), joinIP);

            createPort = GUI.TextField(new Rect(265, 5, 50, 25), createPort);
            joinPort = GUI.TextField(new Rect(265, 35, 50, 25), joinPort);
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
            int height = 5;

            if(!ready)
            {
                if (GUI.Button(new Rect(5, height, 150, 25), "Ready"))
                {
                    NetworkServer.MarkReady();
                    ready = true;
                }
                height += 30;
            }
            else
            {
                var players = GameObject.FindGameObjectsWithTag("Player");
                int count = 0;
                foreach(var v in players)
                {
                    if(v.TryGetComponent(out PlayerController con))
                    {
                        if(con.health <= 0) count++;
                    }
                }

                if(count >= players.Length-1)
                {
                    if (GUI.Button(new Rect(5, height, 150, 25), "Restart"))
                    {
                        NetworkServer.RestartGame();
                    }
                    height += 30;
                }
            }

            if (GUI.Button(new Rect(5, height, 150, 25), "Leave"))
            {
                inServer = false;
                NetworkServer.running = false;
                NetworkServer.CloseServer(new ServerInfo("tcp", joinIP, int.Parse(joinPort), "lobby", "KEEP"));
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
