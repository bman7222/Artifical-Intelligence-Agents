using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flock : MonoBehaviour
{
    float speed;
    bool turning = false; 

    // Start is called before the first frame update
    void Start()
    {
        //speed = Random.Range(flockManager.Instance.minSpeed, flockManager.Instance.maxSpeed);
       
    }

    // Update is called once per frame
    void Update()
    {
        Bounds b = new Bounds(flockManager.Instance.targetPos, flockManager.Instance.moveLimits * 2);

        if (!b.Contains(transform.position))
        {
            turning = true;
        }
        else
        {
            turning = false;
        }

        if (turning)
        {
            Vector3 direction = flockManager.Instance.targetPos - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), flockManager.Instance.rotationSpeed * Time.deltaTime);

        }
        else
        {
            //random chance go in random direction
            if (Random.Range(0, 100) < 10)
            {
                speed = Random.Range(flockManager.Instance.minSpeed, flockManager.Instance.maxSpeed);
            }

            //random chance rules of flocking apply
            if (Random.Range(0, 100) < 10)
            {
                ApplyRules();
            }

        }

        //move flock member
        transform.transform.Translate(0, 0, speed * Time.deltaTime);


    }

    void ApplyRules()
    {
        GameObject[] flockMemberArray;

        flockMemberArray = flockManager.Instance.flockMemberArray;

        Vector3 vCenter = Vector3.zero;

        Vector3 vAvoid = Vector3.zero;

        float groupSpeed = 0.01f;

        float neighborDistance;

        int groupSize = 0;

        foreach(GameObject flockMember in flockMemberArray)
        {
            if (flockMember != this.gameObject)
            {
                neighborDistance = Vector3.Distance(flockMember.transform.position, transform.position);

                if(neighborDistance <= flockManager.Instance.neighbourDistance)
                {
                    vCenter += flockMember.transform.position;

                    groupSize++; 

                    if(neighborDistance < 1.0f)
                    {
                        vAvoid = vAvoid + (transform.position - flockMember.transform.position);
                    }

                    flock anotherFlock = GetComponent<flock>();

                    groupSpeed = groupSpeed + anotherFlock.speed;
                }
            }
        }

        if(groupSize> 0)
        {
            vCenter = vCenter / groupSize + (flockManager.Instance.targetPos- transform.position);
            speed = groupSpeed / groupSize;

            if (speed > flockManager.Instance.maxSpeed)
            {
                speed = flockManager.Instance.maxSpeed;
            }
            Vector3 direction = (vCenter + vAvoid) - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), flockManager.Instance.rotationSpeed*Time.deltaTime);

            }
        }
    }
}
