using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkTransform : MonoBehaviour
{

    [Range(0, 100)]
    public int updateRate;
    public bool sync;
    public int id;
    public bool isOwner;
    public string owner;

    [Range(1, 10)]
    public int dampening;

    private int ticket;
    private Vector3 prevPos;
    private float rotDelta;
    private Quaternion prevRot;
    private Animator anim;
    private float t;

    const float MOVE_DELTA = 0.3f;

    private void Start()
    {
        anim = GetComponent<Animator>();
        if (!isOwner) return;
        prevPos = transform.position;
    }

    private void Update()
    {
        if (!isOwner || updateRate == 0 || !sync) return;
        t += Time.deltaTime;
        if (t >= 1f / updateRate)
        {
            if (Vector3.Distance(transform.position, prevPos) > MOVE_DELTA || Quaternion.Angle(transform.rotation,prevRot) > rotDelta)
            {
                prevPos = transform.position;
                prevRot = transform.rotation;
                NetworkServer.MovementUpdate(id, transform.position, transform.rotation);
            }
            t = 0;
        }
    }

    public void UpdatePosition(Vector3 pos)
    {
        ticket++;
        if(Vector3.Distance(pos,transform.position) > 0.1f)
        {
            StartCoroutine(UpdatePositionInterpolation(pos));
        }
    }

    private IEnumerator UpdatePositionInterpolation(Vector3 newPos)
    {
        int localTicket = ticket;
        float tt = 0;
        Vector3 startPos = transform.position;
        while (tt < 1 && ticket == localTicket)
        {
            anim.SetBool("Move", true);
            transform.position = Vector3.Lerp(startPos, newPos, tt);
            tt += Time.deltaTime*25/dampening;
            yield return new WaitForEndOfFrame();
        }
        anim.SetBool("Move", false);
    }

    public void UpdateRotation(Quaternion pos)
    {
        StartCoroutine(UpdateRotationInterpolation(pos));
    }

    private IEnumerator UpdateRotationInterpolation(Quaternion newPos)
    {
        int localTicket = ticket;
        float tt = 0;
        Quaternion startPos = transform.rotation;
        while (tt < 1 && ticket == localTicket)
        {
            transform.rotation = Quaternion.Lerp(startPos, newPos, tt);
            tt += Time.deltaTime * 25 / dampening;
            yield return new WaitForEndOfFrame();
        }
    }

    public void Destroy()
    {
        NetworkServer.Destroy(id);
    }
}
