using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GBHelper : MonoBehaviour
{
    static GBHelper starter;
    static GBHelper Starter { get
        {
            if(starter == null)
            {
                starter = (GBHelper)new GameObject().AddComponent(typeof(GBHelper));
                DontDestroyOnLoad(starter.gameObject);
            }

            return starter;
        }
    }

    public static void Start(IEnumerator routine)
    {
        Starter.StartCoroutine(routine);
    }

    public static GameObject Instantiate(GameObject gb)
    {
        return Instantiate(gb,Vector3.zero,Quaternion.identity,null);
    }
}
