using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class flockLeader : MonoBehaviour
{
    [SerializeField] private float speed, maxSpeed, deleteRadius = 1.5f, rotationSpeed = 150, squadMemberOffset, framesUntilSlowDown, maxFramesUntilSlowDown = 60f, framesUntilStop, maxframesUntilStop = 30f;

    //[SerializeField] private GameObject [] dropPoints;

    [SerializeField] private Material defaultMesh;
    [SerializeField] private Material targetedMesh;

    [SerializeField] private GameObject flockSpotPrefab;

    [Header("The maximum number of rays to use for raycast detection is O")]
    [SerializeField] public int numRays;

    [Header("The angle between each raycast is O")]
    [SerializeField] public float angle = 90;

    [SerializeField] public float lineRayRange = 2f;
    [SerializeField] public float TriRayRange = 4f;


    private Vector3 target;
    private Rigidbody rb;

    [SerializeField] private List<GameObject> squadSpots;
    [SerializeField] public List<enemy> squadMembers;
    [SerializeField] private List<GameObject> dropPoints;

    public static flockLeader Instance;

    bool freezeInPlace;

    public bool maintainForm = false;

    public Text squadText;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;


        freezeInPlace = false;

        framesUntilSlowDown = maxFramesUntilSlowDown;
        framesUntilStop = maxframesUntilStop;

        Physics.IgnoreLayerCollision(0, 3);
        Physics.IgnoreLayerCollision(2, 3);
        rb = GetComponent<Rigidbody>();

        squadFormStart();
    }

    // Update is called once per frame
    void Update()
    {

        restructureSquadList();

        bool changeIntoTri = false;
        bool changeIntoLine = false;
        bool changeIntoArrow = true;

        //clamp velocity to max speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, (speed - 1));

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        squadFormTri(out changeIntoTri);
        squadFormLine(out changeIntoLine);

        if (changeIntoTri || changeIntoLine)
        {
            changeIntoArrow = false;
        }

        checkIfShouldStayInFormation();
        checkIfSquadMembersArrived(changeIntoArrow);

        if (changeIntoArrow)
        {

            if (!maintainForm)
            {
                squadFormArrow();
            }
        }
        

    }

    private void FixedUpdate()
    {

        //checkIfSquadMembersArrived();
        pathFollow();
       

    }

    void squadFormStart()
    {
        squadText.text = "Two-Level Formation";
        // clearSquadSpots();

        setSquadMemberTargetRadius(2f);
        setSquadLeaderOffset(1f);
        turnOnConeCollisionDetection();

        int count = 1;

        int numSquadMembers;
        try
        {
            numSquadMembers = flockManager.Instance.numFlockMembers;
        }
        catch
        {
            numSquadMembers = 12;
        }

        for (int i = 0; i < numSquadMembers; i++)
        {
            GameObject squadSpot = Instantiate(flockSpotPrefab, Vector3.zero, Quaternion.identity);
            squadSpots.Add(squadSpot);
            squadSpot.transform.SetParent(transform);
 
            if (i % 2 == 0)
            {
                squadSpot.transform.position = transform.TransformPoint(new Vector3(squadMemberOffset * count, 0f, -(squadMemberOffset * count)));
            }
            else
            {
                squadSpot.transform.position = transform.TransformPoint(new Vector3(-(squadMemberOffset * count), 0f, -(squadMemberOffset * count)));
                count++;
            }
        }


       


    }
    void squadFormArrow()
    {

        // clearSquadSpots();

        setSquadMemberTargetRadius(2f);
        setSquadLeaderOffset(1f);
        turnOnConeCollisionDetection();

        int count = 1;

        int numSquadMembers;
        try
        {
            numSquadMembers = squadMembers.Count;
        }
        catch
        {
            numSquadMembers = 12;
        }

        for (int i = 0; i < numSquadMembers; i++)
        {
            GameObject squadSpot = squadSpots[i];
            /*GameObject squadSpot = Instantiate(flockSpotPrefab, Vector3.zero, Quaternion.identity);
            squadSpots.Add(squadSpot);
            squadSpot.transform.SetParent(transform);*/
            if (i % 2 == 0)
            {
                squadSpot.transform.position = transform.TransformPoint(new Vector3(squadMemberOffset * count, 0f, -(squadMemberOffset * count)));
            }
            else
            {
                squadSpot.transform.position = transform.TransformPoint(new Vector3(-(squadMemberOffset * count), 0f, -(squadMemberOffset * count)));
                count++;
            }
        }

        //resetSquadTargets();


    }


    void squadFormLine(out bool shouldChangeForm)
    {
        
        shouldChangeForm = false;
        int rayHitCount = 0;

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numRays - 1)) * angle * 2 - angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward;

            var ray = new Ray(transform.position, direction);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, lineRayRange))
            {
                if (hitInfo.transform.tag != "Player")
                {
                    rayHitCount++;
                    //Debug.Log("One ray hit");
                }
            }

        }

        

        if (rayHitCount >= 2)
        {
     
            setSquadMemberTargetRadius(1f);
            setSquadLeaderOffset(2f);
            shouldChangeForm = true;
            turnOffConeCollisionDetection();
            //Debug.Log("FORM 1x1");

           // clearSquadSpots();

            int numSquadMembers;

            try
            {
                numSquadMembers = squadMembers.Count;
            }
            catch
            {
                numSquadMembers = 12;
            }

            for (int i = 0; i <numSquadMembers; i++)
            {
                GameObject squadSpot = squadSpots[i];
/*                GameObject squadSpot = Instantiate(flockSpotPrefab, Vector3.zero, Quaternion.identity);
                squadSpots.Add(squadSpot);
                squadSpot.transform.SetParent(transform);*/
                squadSpot.transform.position = transform.TransformPoint(new Vector3(0f, 0f, -(squadMemberOffset * (i+1))));
            }
            //resetSquadTargets();
        }


    }

    void squadFormTri(out bool shouldChangeForm)
    {

        shouldChangeForm = false;
        int rayHitCount = 0;

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numRays - 1)) * angle * 2 - angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward;
            var ray = new Ray(transform.position, direction);


            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, TriRayRange))
            {
                if (hitInfo.transform.tag != "Player")
                {
                    rayHitCount++;
                    //Debug.Log("One ray hit");
                }
            }

        }

        if (rayHitCount >= 2)
        {
          
            shouldChangeForm = true;
            setSquadMemberTargetRadius(0.5f);
            setSquadLeaderOffset(2.5f);
            turnOffConeCollisionDetection();
            //Debug.Log("FORM 3x3");

            //clearSquadSpots();

            int numSquadMembers;

            try
            {
                numSquadMembers = squadMembers.Count;
            }
            catch
            {
                numSquadMembers = 12;
            }

            int row = 1;
            int col = 1;

            for (int i = 0; i < numSquadMembers; i++)
            {
                GameObject squadSpot = squadSpots[i];
/*                GameObject squadSpot = Instantiate(flockSpotPrefab, Vector3.zero, Quaternion.identity);
                squadSpots.Add(squadSpot);
                squadSpot.transform.SetParent(transform);*/

                if (col == 1)
                {

                    //left
                    squadSpot.transform.position = transform.TransformPoint(new Vector3(-squadMemberOffset , 0f, - (squadMemberOffset * row)));
                    col++;
                    continue;
                }

                if (col == 2)
                {
                    //mid
                    squadSpot.transform.position = transform.TransformPoint(new Vector3(0f, 0f, - (squadMemberOffset * row)));
                    col++;
                    continue;
                }

                if (col == 3)
                {
     
                    //right
                    squadSpot.transform.position = transform.TransformPoint(new Vector3(squadMemberOffset, 0f, - (squadMemberOffset * row)));
                    row++;
                    col = 1;
                }
                
            }

           // resetSquadTargets();
        }


    }

    void pathFollow()
    {
        if (freezeInPlace)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //make sure there are points to follow
        if (dropPoints.Count > 0)
        {
            Vector3 dropPointPos= new Vector3(dropPoints[0].transform.position.x, 0, dropPoints[0].transform.position.z);
            float closestMagnitude;
            GameObject closestDropPoint;


            //Set the closest magnitude and closes point to first item in list, otherwise return null
            try
            {
                closestMagnitude = (dropPointPos - pos).magnitude;
                closestDropPoint = dropPoints[0];
            }
            catch
            {

                closestMagnitude = 0f;
                closestDropPoint = null;
            }

            //For each point, set their color to white and see if their distance is smaller than the current min distance. Set min distance to new distanc eif it is smaller
            foreach (GameObject dp in dropPoints)
            {
                Vector3 dpPos = new Vector3(dp.transform.position.x, 0, dp.transform.position.z);
                try
                {
                    dp.GetComponent<MeshRenderer>().material = defaultMesh;

                    if ((dpPos - pos).magnitude < closestMagnitude)
                    {
                        closestMagnitude = (dpPos - pos).magnitude;
                        closestDropPoint = dp;
                    }
                }
                //If failed, return
                catch
                {
                    return;
                }

            }



            //Set target to closest point and change its color to red
            GameObject targetPoint = closestDropPoint;

            targetPoint.GetComponent<MeshRenderer>().material = targetedMesh;

            target = new Vector3 (targetPoint.transform.position.x,0, targetPoint.transform.position.z);


            //animator.transform.position = Vector2.MoveTowards(animator.transform.position, playerPos.position, speed * Time.deltaTime);

            //velocity.Normalize();
            Vector3 desiredVelocity = target - pos;

            //Debug.Log(targetPoint.name+" "+desiredVelocity+" : "+desiredVelocity.magnitude );
            //If desired veloicty is less than delete radius, delete it and look for new point by returning
            if (desiredVelocity.magnitude < deleteRadius)
            {
                List<GameObject> tempList = new List<GameObject>();

                for(int i=0; i < dropPoints.Count; i++)
                {
                    if (i > 0)
                    {
                        tempList.Add(dropPoints[i]);
                    }
                }
                Destroy(dropPoints[0]);
                dropPoints.Clear();
                dropPoints = tempList;

                return;
            }


            //Seek to target point
            desiredVelocity.Normalize();
            desiredVelocity *= speed * 1.2f;

            Vector3 steering = (desiredVelocity - velocity);


            // move
            rb.AddForce(steering);


            if (rb.velocity != Vector3.zero)
            {
                //rotate
                Quaternion rotateTo = Quaternion.LookRotation(rb.velocity, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed * Time.fixedDeltaTime * 2);

            }
            else
            {
                //rotate
                Quaternion rotateTo = Quaternion.LookRotation(transform.forward, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed * Time.fixedDeltaTime * 2);
            }

        }

    }

    void checkIfShouldStayInFormation()
    {
        foreach (enemy squadMember in squadMembers)
        {
            if (squadMember.stayInFormation == true)
            {
                maintainForm = true;
                return;
            }
        }

        //Debug.Log("ALL TRUE");
        maintainForm = false;
        return;
    }

    void checkIfSquadMembersArrived(bool changeIntoArrow)
    {
        speed = maxSpeed;

        int amountNotArrived = 0;

        foreach(enemy squadMember in squadMembers)
        {
            if (!(squadMember.arrivedAtTarget))
            {
                amountNotArrived++;
            }
        }

       // Debug.Log(amountNotArrived);

        /*if (amountNotArrived == 0)
        {
            speed = maxSpeed;
        }

        if (amountNotArrived > flockManager.Instance.numFlockMembers / 4)
        {
            speed = maxSpeed / 2;
        }*/

        //Debug.Log(amountNotArrived);

        if (amountNotArrived > squadMembers.Count / 2)
        {
            framesUntilSlowDown--;
        }
        else
        {
            framesUntilSlowDown = maxFramesUntilSlowDown;
            framesUntilStop = maxframesUntilStop;
            freezeInPlace = false;
        }

        if (framesUntilSlowDown <= 0)
        {
            framesUntilStop--;
            //Debug.Log("SLOW");
            //freezeInPlace = true;
            speed = maxSpeed/2;
        }

        if (framesUntilStop <= 0)
        {
            

            if (!changeIntoArrow)
            {
                freezeInPlace = true;
                speed = 0;
                //Debug.Log("NOT ARROW");
            }
            else
            {
                speed = maxSpeed / 2;
                //Debug.Log(" ARROW");
            }
           
        }

        
    }

    public void clearSquadSpots()
    {
        foreach (GameObject spot in squadSpots)
        {
            Destroy(spot.gameObject);

        }

        squadSpots.Clear();
    }

    public void resetSquadTargets()
    {
        for (int i = 0; i < squadMembers.Count; i++)
        {
            squadMembers[i].seekTarget = squadSpots[i];
        }
    }

    public void restructureSquadList()
    {
      foreach(enemy squadMember in squadMembers)
        {
            if (!squadMember)
            {
                squadMembers.Remove(squadMember);
            }
        }


    }

    void turnOffConeCollisionDetection()
    {
        foreach(enemy squadMember in squadMembers)
        {
            squadMember.shouldDetectConeCollision = false;
        }
    }

    void turnOnConeCollisionDetection()
    {
        foreach (enemy squadMember in squadMembers)
        {
            squadMember.shouldDetectConeCollision = true;
        }
    }

    void setSquadMemberTargetRadius(float newTargetRadius)
    {
        foreach (enemy squadMember in squadMembers)
        {
            squadMember.targetRadius = newTargetRadius;
        }
    }

    void setSquadLeaderOffset(float newOffset)
    {
        squadMemberOffset = newOffset;
    }

    private void OnDrawGizmos()
    {

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numRays - 1)) * angle * 2 - angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward * lineRayRange;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, direction);
        }

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numRays - 1)) * angle * 2 - angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward * TriRayRange;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, direction);
        }
    }

   

    }

