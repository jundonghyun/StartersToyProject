using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed;

    Rigidbody playerRigidbody;
    Animator playerAnimator;
    Vector3 direction;
    Vector3 dodgeDirection;

    bool isDodge;

    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        Move();
        Dodge();
    }

    void FixedUpdate()
    {
        
        Rotate();
    }

    void Move()
    {
        if (!isDodge)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            direction = new Vector3(x, 0, z).normalized;

            playerRigidbody.velocity = direction * moveSpeed;

            playerAnimator.SetBool("isRun", direction != Vector3.zero);
        }
    }

    void Rotate()
    {
        if (!isDodge)
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (plane.Raycast(cameraRay, out rayDistance))
            {
                Vector3 lookPoint = cameraRay.GetPoint(rayDistance);

                transform.LookAt(new Vector3(lookPoint.x, transform.position.y, lookPoint.z));
            }
        }
    }

    void Dodge()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isDodge)
            {
                dodgeDirection = direction;
                transform.LookAt(dodgeDirection);

                moveSpeed *= 2f;

                isDodge = true;
                Invoke("DodgeOff", 0.3f);

                playerAnimator.SetTrigger("doDodge");
            }



        }
    }

    void DodgeOff()
    {
        moveSpeed *= 0.5f;

        isDodge = false;
    }
}
