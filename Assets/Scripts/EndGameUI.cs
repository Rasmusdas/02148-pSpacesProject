using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    public GameObject win;
    public GameObject lose;
    void Update()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        int count = 0;
        bool alive = true;

        foreach (var v in players)
        {
            if (v.TryGetComponent(out PlayerController con))
            {
                if (con.health <= 0)
                {
                    count++;
                    if (con.GetComponent<NetworkTransform>().isOwner)
                    {
                        alive = false;
                    }
                }
            }
        }

        if(count >= players.Length-1 && count != 0)
        {
            if(alive)
            {
                win.SetActive(true);
            }
            else
            {
                lose.SetActive(false);
            }
        }
    }
}
