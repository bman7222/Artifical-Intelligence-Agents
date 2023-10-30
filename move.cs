using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    public float speed = 1.19f,endPoint=5;
    Vector3 start;
    Vector3 end;


    void Start()
    {
        start = transform.position;
        end = transform.position + new Vector3(0, 0, endPoint);
    }

    void Update()
    {
        //PingPong between 0 and 1
        float time = Mathf.PingPong(Time.time * speed, 1);
        transform.position = Vector3.Lerp(start, end, time);
    }
}
