using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flockManager : MonoBehaviour
{
    public GameObject flockMemberPrefab,squadMemberPrefab;
    public int numFlockMembers;
    public GameObject[] flockMemberArray;
    public Vector3 moveLimits;

    public static flockManager Instance;

    [Header("Flock Member Settings")]
    [Range(0.0f, 100.0f)]
    public float minSpeed;
    [Range(0.0f, 100.0f)]
    public float maxSpeed;
    public float neighbourDistance;
    [Range(0.0f, 100.0f)]
    public float rotationSpeed;

    public GameObject target;

    public Vector3 targetPos = Vector3.zero;

    public float randomMoveChancePerFrame;

    public bool summonFlockers, summonSquadMembers;


// Start is called before the first frame update
void Start()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        flockMemberArray = new GameObject[numFlockMembers];
        for (int i=0;  i<numFlockMembers; i++)
        {

            Vector3 flockMemberPos = transform.position + new Vector3(Random.Range(-moveLimits.x, moveLimits.x), 0f, Random.Range(-moveLimits.z, moveLimits.z));

            if (summonFlockers) flockMemberArray[i] = Instantiate(flockMemberPrefab, flockMemberPos, Quaternion.identity);

            else if (summonSquadMembers)
            {
                flockMemberArray[i] = Instantiate(squadMemberPrefab, flockMemberPos, Quaternion.identity);
                flockLeader.Instance.squadMembers.Add(flockMemberArray[i].GetComponent<enemy>());
            }

            else Debug.Log("SPECIFY WHAT TO SUMMON USING BOOLS");
        }

        if (summonSquadMembers)
        {
            flockLeader.Instance.resetSquadTargets();
        }

    }


    // Update is called once per frame
    void Update()
    {
        /*if (Random.Range(0, 100) < randomMoveChancePerFrame)
        {
            targetPos += new Vector3(Random.Range(-moveLimits.x, moveLimits.x), Random.Range(-moveLimits.y, moveLimits.y), Random.Range(-moveLimits.z, moveLimits.z));

            target.transform.position = targetPos;
        }*/
    }
}
