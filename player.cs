using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class player : MonoBehaviour
{
    public bool willKillOnTouch;

    private Rigidbody rb;

    public Vector3 moveInput;

    public float moveSpeed, rotationSpeed;

    public InputActionReference move;
    //public InputActionReference fire;

   //// [SerializeField] private Camera mainCam;

  //  [SerializeField] private NavMeshAgent theAgent;

    public static player Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
/*        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = mainCam.nearClipPlane;
            //Vector3 Worldpos = mainCam.ScreenToWorldPoint(mousePos);

            Ray theRay = mainCam.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if(Physics.Raycast(theRay, out hit))
            {
                theAgent.SetDestination(hit.point);
            }

        }
*/
        moveInput.x = move.action.ReadValue<Vector2>().x;
        moveInput.z = move.action.ReadValue<Vector2>().y;
        moveInput.Normalize();

        if (moveInput != Vector3.zero)
        {
            //rotate
            Quaternion rotateTo = Quaternion.LookRotation(rb.velocity, Vector3.up); 
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTo, rotationSpeed * Time.fixedDeltaTime * 2);

        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(moveInput.x * moveSpeed, 0f,moveInput.z * moveSpeed);
    }
}
