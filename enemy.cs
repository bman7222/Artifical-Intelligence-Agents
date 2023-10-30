using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class enemy : MonoBehaviour
{
    //The rigid body of the enemy
    private Rigidbody rb;

    [Header("The prefab for the function Displayer, holds the wanderTarget, avoidtarget, evadeTarget, and Collision Target")]
    [SerializeField] private GameObject functionDisplayerPrefab;

    //[Header("The game Object which holds the wanderTarget, avoidtarget, evadeTarget, and Collision Target")]
    private GameObject functionDisplayer;

    //[Header("The game objects to display: The wander radius and where the enemy will wander to, where the enemy will seek when raycasting, where the enemy will avoid when it detects with its cone, what will appear when the enemy collides with something")]
    private GameObject wanderTarget, avoidTarget, evadeTarget, collisionTarget;

    [Header("The speed of the enemy is O")]
    [SerializeField] private float speed;
    [Header("The speed of the enemy Y rotation is O")]
    [SerializeField] private float rotationSpeed;
    [Header("The offset for the circle for wandering is O")]
    [SerializeField] private float wanderOffset;
    [Header("The radius of the circle the point to wander to is O")]
    [SerializeField] private float wanderRadius;
    [Header("The amount to multiply the randomness in the X and Z directions when enemy is selecting where to seek to when wandering is O")]
    [SerializeField] private float wanderJit;
    [Header("The amount of randomness in the X and Z directions when enemy is selecting where to seek to when wandering is O")]
    [SerializeField] private float wanderRange;
    [Header("The distance away from the wall the enemy will seek when avoiding using raycast is O")]
    [SerializeField] private float avoidDistance;
    [Header("If using collision prediction and calculating time, if the result is a time less than O then the enemies will execute collision prediction avoidance")]
    [SerializeField] private float ifTimeIsLessThanThisAvoidCollision;
    [Header("If anything with the tag 'Player' is within O of the enemy they will consider it a possible obstacle to avoid using collision prediction avoidance")]
    [SerializeField] private float predictRadius;
    [Header("The amount of time the enemy will predict using collision avoidance is O")]
    [SerializeField] private float predictTime;

    [Header("The maximum number of rays to use for raycast detection is O")]
    [SerializeField] private int numRays;

    [Header("The angle between each raycast is O")]
    [SerializeField] private float angle = 10;

    [Header("The maximum possible angle between each raycast is O")]
    [SerializeField] private float maxAngle;

    [Header("The range of the raycast is O")]
    [SerializeField] private float rayRange = 2f;

    private bool collisionDetected;

    [Header("AAAAAAAAAAAAAAAAAAAAAAAA")]
    public  int increaseAngleRangeCount;

    [Header("The amount of frames X it takes to increase the distance between raycast by angle O")]
    [SerializeField] private int framesToIncrease, angleToIncreaseBy;

    [Header("The radius X that an object must be within for it to be detected by the cone detection and the angle that determines where the cone is displayed O ")]
    [SerializeField] private float detectionRadius = 10f, detectionAngle = 90f;

    [Header("The array of objects that are detectable by cone and collision prediction")]
    [SerializeField] private GameObject[] collidableConeObjects, collidablePredictionObjects;

    [Header("The game object to chase if the enemy is a chaser")]
    [SerializeField] public GameObject seekTarget;

    [Header("The default material of the enemy is O")]
    [SerializeField] private Material myMaterial;

    [Header("The material to use when doing collision avoidance is O")]
    [SerializeField] private Material predictMaterial;

    [Header("The bool that determines whether the enemy should chase something or wander")]
    [SerializeField] private bool chaser;

    [Header("The bool that determines whether the enemy should flock or not")]
    [SerializeField] private bool flocker;

    [Header("The bool that determines whether the enemy should be apart of a squad or not")]
    [SerializeField] private bool squadMember;

    [Header("The radius in which the squad member begins to slow down is O")]
    [SerializeField] private float slowRadius = 5f;

    [Header("The radius in which the squad member is considered at there spot is O")]
    [SerializeField] public float targetRadius = 0.5f;

    [Header("The bool which says if the squad member has arrived at target")]
    [SerializeField] public bool arrivedAtTarget;

    [Header("The bool which says if the squad member should detect cone collision")]
    [SerializeField] public bool shouldDetectConeCollision;

    [Header("The bool which says tells the squad leader/flock leader if the squad should stay in formation")]
    [SerializeField] public bool stayInFormation = false;

    bool turning = false;

    float flockSpeed;

    public int myGroupSize;

    private float originalRayRange;
    // Start is called before the first frame update
    void Start()
    {
        originalRayRange = rayRange;

        increaseAngleRangeCount = 0;
        rb = GetComponent<Rigidbody>();
        functionDisplayer = Instantiate(functionDisplayerPrefab, Vector3.zero, Quaternion.identity);

        foreach (Transform displayItem in functionDisplayer.transform)
        {

            switch (displayItem.name)
            {
                case "avoidTarget":
                    avoidTarget = displayItem.gameObject;
                    break;
                case "collisionTarget":
                    collisionTarget = displayItem.gameObject;
                    break;
                case "evadeTarget":
                    evadeTarget = displayItem.gameObject;
                    break;
                case "wanderTarget":
                    wanderTarget = displayItem.gameObject;
                    break;
            }

        }

        collisionTarget.SetActive(false);

        if (chaser && !seekTarget)
        {
            seekTarget = FindObjectOfType<player>().gameObject;
        }
        if (flocker)
        {
            predictRadius = 0.2f;
        }

    }

    // Update is called once per frame
    void Update()
    {
        //clamp velocity to max speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, (speed - 1));

        angle = Mathf.Clamp(angle, 0f, maxAngle);

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);


    }

    void OnCollisionEnter(Collision other)
    {

        collisionTarget.gameObject.SetActive(true);

        collisionTarget.gameObject.transform.position = transform.position;

        if (other.gameObject.tag=="PlayerController")
        {
            if (player.Instance.willKillOnTouch)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        float time = 0;
        Vector3 targetPos;
        Vector3 targetVelocity;
        targetPos = seekTarget.transform.position;

        if (squadMember)
        {
            /* if (!cone(out targetPos) || !shouldDetectConeCollision)
             {
                 squadMemberShouldFormTri();
                 squadMemberShouldFormLine();
                 targetPos = seekTarget.transform.position;
                 squadSeek(targetPos);
             }
             else
             {
                 stayInFormation = false;
                 //arrivedAtTarget = true;
                 evade( Vector3.zero, targetPos, predictTime);
                 //squadSeek(targetPos);
                 // evade(new Vector3(1,1,1), targetPos, predictTime);
             }*/

            if (shouldDetectConeCollision)
            {
                targetPos = avoid(targetPos);
            }
            

            //
            if (targetPos == seekTarget.transform.position )
            {
                //squadMemberShouldFormTri();
                squadMemberShouldFormLine();
                squadSeek(targetPos);
            }
            //avoid 
            else
            {
                stayInFormation = false;
                squadSeek(targetPos);
            }

            //angle stuff 
            if (collisionDetected)
            {
                increaseAngleRangeCount++;

            }
            else
            {
                increaseAngleRangeCount = 0;
                angle = 10;
                rayRange = originalRayRange;
            }

            if (increaseAngleRangeCount > framesToIncrease)
            {
                increaseAngleRangeCount = 0;
                angle += angleToIncreaseBy;
                if (rayRange < 5)
                {
                    rayRange++;
                }
            }
        }

        else
        {
            if (!AvoidCollisionPrediction(out targetVelocity, out targetPos, out time))
            {
                GetComponent<MeshRenderer>().material = myMaterial;

                if (!cone(out targetPos))
                {
                    if (chaser)
                    {
                        targetPos = seekTarget.transform.position;

                        targetPos = avoid(targetPos);
                        seek(targetPos);

                    }
                    else if (flocker)
                    {
                        targetPos = flockManager.Instance.target.transform.position;

                        targetPos = avoid(targetPos);
                        flockSeek(targetPos);

                    }
                    else if (squadMember)
                    {
                        targetPos = flockManager.Instance.target.transform.position;

                        targetPos = avoid(targetPos);
                        flockSeek(targetPos);

                    }
                    else
                    {
                        targetPos = wander();

                        targetPos = avoid(targetPos);
                        seek(targetPos);
                    }

                    if (flocker)
                    {
                        flockSetUp();
                    }

                    if (squadMember)
                    {
                        flockSetUp();
                    }

                    if (collisionDetected)
                    {
                        increaseAngleRangeCount++;

                    }
                    else
                    {
                        increaseAngleRangeCount = 0;
                        angle = 10;
                    }

                    if (increaseAngleRangeCount > framesToIncrease)
                    {
                        increaseAngleRangeCount = 0;
                        angle += angleToIncreaseBy;
                    }



                }
                else
                {
                    evade(targetVelocity, targetPos, predictTime);
                }
            }

            else
            {

                GetComponent<MeshRenderer>().material = predictMaterial;
                evade(new Vector3(1f, 0f, 1f), targetPos, time);

            }
        }

        //cone();

    }

 

    void squadMemberShouldFormLine()
    {


        int rayHitCount = 0;

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)flockLeader.Instance.numRays - 1)) * flockLeader.Instance.angle * 2 - flockLeader.Instance.angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward;

            var ray = new Ray(transform.position, direction);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, flockLeader.Instance.lineRayRange))
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
            stayInFormation = true;
           
        }
        else
        {
            stayInFormation = false;
        }
    }

   /* void squadMemberShouldFormTri()
    {
        int rayHitCount = 0;

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)flockLeader.Instance.numRays - 1)) * flockLeader.Instance.angle * 2 - flockLeader.Instance.angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward;
            var ray = new Ray(transform.position, direction);


            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, flockLeader.Instance.TriRayRange))
            {
                if (hitInfo.transform.tag != "Player")
                {
                    rayHitCount++;
                   // Debug.Log("One ray hit");
                }
            }

        }

        if (rayHitCount >= 2)
        {
            stayInFormation = true;
        }
        else
        {
            stayInFormation = false;
        }

    }*/

    public bool AvoidCollisionPrediction(out Vector3 targetToAvoidVelocity, out Vector3 targetToAvoid, out float t)
    {
        bool detectedObject = false;
        t = 0;
        float minDist = 1000f;
        Vector3 minDistanceBetween = Vector3.zero;
        GameObject minDistanceObject = null;

        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 target = Vector3.zero;
        targetToAvoid = target;
        targetToAvoidVelocity = target;

        collidablePredictionObjects = GameObject.FindGameObjectsWithTag("Player");



        foreach (GameObject collidableObject in collidablePredictionObjects)
        {



            if (!(GameObject.ReferenceEquals(this.gameObject, collidableObject)))
            {

                target = new Vector3(collidableObject.transform.position.x, 0, collidableObject.transform.position.z);

                Vector3 distanceBetween = target - pos;

                if (distanceBetween.magnitude < predictRadius)
                {

                    if (distanceBetween.magnitude < minDist)
                    {
                        minDist = distanceBetween.magnitude;
                        minDistanceBetween = distanceBetween;
                        minDistanceObject = collidableObject;
                        detectedObject = true;
                    }
                }
            }
        }

        if (detectedObject)
        {
            Vector3 dp = minDistanceBetween;
            Vector3 dv = minDistanceObject.GetComponent<Rigidbody>().velocity - rb.velocity;
            float dpdv = (dp.x * dv.x) + (dp.z * dv.z);
            float otherDV = (dv.x * dv.x) + (dv.z * dv.z);
            float time = -1 * dpdv / (otherDV * otherDV);

            if (Mathf.Abs(time) < ifTimeIsLessThanThisAvoidCollision)
            {

                //Debug.Log("TIME:+ " + ifTimeIsLessThanThisAvoidCollision + " " + time);
                targetToAvoid = minDistanceObject.transform.position;
                targetToAvoidVelocity = minDistanceObject.GetComponent<Rigidbody>().velocity;
                t = time;
                return true;
            }
        }



        return false;
    }

    bool cone(out Vector3 targetToAvoid)
    {
        bool detectedAnObject = false;
        int detectedCount = 0;

        targetToAvoid = Vector3.zero;

        collidableConeObjects = GameObject.FindGameObjectsWithTag("coneTarget");

        foreach (GameObject collidableObject in collidableConeObjects)
        {

            Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 target = new Vector3(collidableObject.transform.position.x, 0, collidableObject.transform.position.z);

            Vector3 targetPos = target - pos;

            if (targetPos.magnitude <= detectionRadius)
            {
                float dot1 = Vector3.Dot(targetPos, transform.forward);
                float dot2 = Mathf.Cos(detectionRadius * 0.5f * Mathf.Deg2Rad);

                if (dot1 > dot2)
                {

                    detectedCount++;
                    targetToAvoid += collidableObject.transform.position;
                    detectedAnObject = true;

                }
            }
        }

        if (detectedAnObject)
        {
            targetToAvoid.x /= detectedCount;
            targetToAvoid.z /= detectedCount;

            return true;
        }

        return false;
    }

    Vector3 wander()
    {
        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //Offset target position by current position
        Vector3 targetPos = velocity;
        targetPos -= (pos * wanderOffset);

        Vector3 wanderZone = new Vector3(Random.Range(-wanderRange, wanderRange) * wanderJit, 0, (Random.Range(-wanderRange, wanderRange) * wanderJit));

        wanderZone.Normalize();

        wanderZone *= wanderRadius;

        Vector3 dir = wanderZone + pos + (transform.forward * wanderOffset);

        Vector3 worldDir = transform.InverseTransformVector(dir);

        /*Vector3 localTarget = wanderZone +  pos + new Vector3(0, 0, wanderOffset);

        Vector3 worldTarget = transform.InverseTransformVector(localTarget);*/

        targetPos = dir;

        wanderTarget.transform.position = targetPos;

        return targetPos;
    }

    Vector3 avoid(Vector3 targetPos)
    {
        bool detectedNone = true;

      

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numRays - 1)) * angle * 2 - angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward;

            var ray = new Ray(transform.position, direction);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, rayRange))
            {
                if (hitInfo.transform.tag != "Player")
                {

                    detectedNone = false;
                    collisionDetected = true;
                    targetPos = hitInfo.point + hitInfo.normal * avoidDistance;
                    avoidTarget.transform.position = targetPos;
                }
            }

        }

        if (detectedNone == true)
        {
            collisionDetected = false;
            //Debug.Log(gameObject.name + " " + transform.position);
        }

        return targetPos;

    }

    void flee(Vector3 targetPos)
    {
        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //find desired direction
        Vector3 desiredVelocity = pos - targetPos;
        //normalize desired direction then multiply acceleration
        desiredVelocity.Normalize();
        desiredVelocity *= speed;

        //Pull down desired direction by current velocity
        Vector3 steering = (desiredVelocity - velocity);

        rb.AddForce(steering);

        //rotate
        Quaternion rotateTo = Quaternion.LookRotation(rb.velocity, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed);
    }

    void evade(Vector3 targetVelocity, Vector3 targetPos, float offset)
    {
        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        Vector3 targetV = targetVelocity * offset;

        targetV += targetPos;

        evadeTarget.transform.position = targetV;

        Vector3 desiredVelocity = pos - targetV;
        desiredVelocity.Normalize();
        desiredVelocity *= speed;

        //Pull down desired direction by current velocity
        Vector3 steering = (desiredVelocity - velocity);

        rb.AddForce(steering);

        //rotate
        Quaternion rotateTo = Quaternion.LookRotation(rb.velocity, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed);
    }

    void seek(Vector3 targetPos)
    {

        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //find desired direction
        Vector3 desiredVelocity = targetPos - pos;
        //normalize desired direction then multiply acceleration
        desiredVelocity.Normalize();
        desiredVelocity *= speed;

        //Pull down desired direction by current velocity
        Vector3 steering = (desiredVelocity - velocity);

        rb.AddForce(steering);

        //rotate
        Quaternion rotateTo = Quaternion.LookRotation(rb.velocity, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed);

        //Debug.Log(rb.velocity + " "+ (rb.velocity.x + rb.velocity.z));
    }

    void squadSeek  (Vector3 targetPos)
    {
        targetPos = new Vector3(targetPos.x, 0f, targetPos.z);

        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //find desired direction
        Vector3 desiredVelocity = targetPos - pos;
        //normalize desired direction then multiply acceleration
        desiredVelocity.Normalize();
        desiredVelocity *= speed;

        //Pull down desired direction by current velocity
        Vector3 steering = (desiredVelocity - velocity);

        //get offset, get ramped speed choose min between max speed and clipped speed
        Vector3 targetOffset = targetPos - pos;
        float dis = targetOffset.magnitude;
        float rampedSpeed = speed * (dis / slowRadius);
        float clippedSpeed = Mathf.Min(rampedSpeed, speed);

        //If slowing down, activate circle, slow down speed by distance, change steering
        if (clippedSpeed == rampedSpeed)
        {
            //Debug.Log("RAMP: " + rampedSpeed + " DIS: " + dis);
            desiredVelocity = (clippedSpeed / dis) * targetOffset;
            steering = (desiredVelocity - velocity);
        }

        rb.AddForce(steering);

        if (Vector3.Distance(targetPos, pos) <= targetRadius)
        {
            arrivedAtTarget = true;
            //rotate

            if (rb.velocity != Vector3.zero)
            {
                Quaternion rotateTo = Quaternion.LookRotation(flockLeader.Instance.gameObject.transform.forward, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed);
            }
        }
        else
        {
            arrivedAtTarget = false;
            if (rb.velocity != Vector3.zero)
            {
                //rotate
                Quaternion rotateTo = Quaternion.LookRotation(rb.velocity, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed);
            }

        }

        //Debug.Log(rb.velocity + " "+ (rb.velocity.x + rb.velocity.z));
    }

    void flockSetUp()
    {
        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 direction;

        //make bound
        Bounds b = new Bounds(flockManager.Instance.target.transform.position, flockManager.Instance.moveLimits * 2);

        //check if in bounds
        if (!b.Contains(transform.position))
        {
            turning = true;
        }
        else
        {
            turning = false;
        }

        //if not in bounds, turn around
        if (turning)
        {
            direction = flockManager.Instance.target.transform.position - pos;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), flockManager.Instance.rotationSpeed * Time.fixedDeltaTime);

        }
        //if in bounds, set some random speed and random chance to flock
        /* else
         {
             //random chance go in random direction
             if (Random.Range(0, 100) < 10)
             {
                 flockSpeed = Random.Range(flockManager.Instance.minSpeed, flockManager.Instance.maxSpeed);
             }

             *//*//random chance rules of flocking apply
             if (Random.Range(0, 100) < 100)
             {
                 flock();
             }*//*

         }*/

        flockSpeed = flockManager.Instance.maxSpeed;

        flock();

        speed = flockSpeed;

        //Vector3 pushForce = transform.forward * flockSpeed;

        //rb.AddForce(pushForce);

        //Debug.Log("PUSH ME: " + pushForce+" MY V: "+rb.velocity);
        //flockSeek(targetPos);

        //transform.transform.Translate(0, 0, flockSpeed * Time.fixedDeltaTime);

    }

    void flockSeek(Vector3 targetPos)
    {

        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //find desired direction
        Vector3 desiredVelocity = targetPos - pos;
        //normalize desired direction then multiply acceleration
        desiredVelocity.Normalize();
        desiredVelocity *= speed;

        //Pull down desired direction by current velocity
        Vector3 steering = (desiredVelocity - velocity);


        rb.AddForce(steering);

        //Debug.Log(rb.velocity + " "+ (rb.velocity.x + rb.velocity.z));
    }

    void flock()
    {

        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);

        GameObject[] flockMemberArray;

        flockMemberArray = flockManager.Instance.flockMemberArray;

        Vector3 vCenter = Vector3.zero;

        Vector3 vAvoid = Vector3.zero;

        float groupSpeed = 0.01f;

        float neighborDistance;

        int groupSize = 0;

        foreach (GameObject flockMember in flockMemberArray)
        {
            Vector3 neighborPos = new Vector3(flockMember.transform.position.x, 0, flockMember.transform.position.z);

            if (flockMember != this.gameObject)
            {
                neighborDistance = Vector3.Distance(neighborPos, pos);

                if (neighborDistance <= flockManager.Instance.neighbourDistance)
                {
                    vCenter += flockMember.transform.position;

                    groupSize++;

                    if (neighborDistance < 1.0f)
                    {
                        vAvoid = vAvoid + (pos - neighborPos);
                    }

                    enemy myEnemy = GetComponent<enemy>();

                    groupSpeed = groupSpeed + myEnemy.flockSpeed;
                }
            }
        }

        myGroupSize = groupSize;

        if (groupSize > 0)
        {
            vCenter = vCenter / groupSize + (flockManager.Instance.target.transform.position - pos);
            flockSpeed = groupSpeed / groupSize;

            if (flockSpeed > flockManager.Instance.maxSpeed)
            {
                flockSpeed = flockManager.Instance.maxSpeed;
            }
            Vector3 direction = (vCenter + vAvoid) - pos;
            if (direction != Vector3.zero)
            {
               // Debug.Log(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), flockManager.Instance.rotationSpeed);

            }
        }


    }

    void OnDestroy()
    {
      
        flockLeader.Instance.squadMembers.Remove(this);
        flockLeader.Instance.resetSquadTargets();
    }

    private void OnDrawGizmos()
    {

        if (squadMember)
        {
            
            for (int i = 0; i < numRays; i++)
            {
                var rotation = transform.rotation;
                var rotationMod = Quaternion.AngleAxis((i / ((float)flockLeader.Instance.numRays - 1)) * flockLeader.Instance.angle * 2 - flockLeader.Instance.angle, Vector3.up);
                var direction = rotation * rotationMod * Vector3.forward * flockLeader.Instance.lineRayRange;
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, direction);
            }
/*
            for (int i = 0; i < numRays; i++)
            {
                var rotation = transform.rotation;
                var rotationMod = Quaternion.AngleAxis((i / ((float)flockLeader.Instance.numRays - 1)) * flockLeader.Instance.angle * 2 - flockLeader.Instance.angle, Vector3.up);
                var direction = rotation * rotationMod * Vector3.forward * flockLeader.Instance.TriRayRange;
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, direction);
            }*/
        }

        for (int i = 0; i < numRays; i++)
        {
            var rotation = transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numRays - 1)) * angle * 2 - angle, Vector3.up);
            var direction = rotation * rotationMod * Vector3.forward * rayRange;
            Gizmos.DrawRay(transform.position, direction);



        }
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;

        Vector3 rotatedForward = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward;

        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, detectionAngle, detectionRadius);

#endif

    }


}

