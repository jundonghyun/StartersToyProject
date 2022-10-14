using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed;

    Rigidbody playerRigidbody;
    Animator playerAnimator;
    float hAxis, vAxis;
    Vector3 moveDirection;
    Vector3 dodgeDirection;

    public Gun equipGun;
    //public GameObject[] gunList;

    float shotDelay;

    bool doDodge, doShot, doSwap;
    bool isDodge, isShotReady;

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
        GetInput();
        Dodge();
        Shot();
        Swap();
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        doDodge = Input.GetKeyDown(KeyCode.Space);
        doShot = Input.GetButtonDown("Fire1");
        doSwap = Input.GetKeyDown(KeyCode.Alpha1);
    }

    void Move()
    {
        moveDirection = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge) moveDirection = dodgeDirection;

        playerRigidbody.velocity = moveDirection * moveSpeed;

        playerAnimator.SetBool("isRun", moveDirection != Vector3.zero);
    }

    void Rotate()
    {
        if (isDodge)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        else
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
        if (doDodge && !isDodge && moveDirection != Vector3.zero)
        {
            dodgeDirection = moveDirection;
            moveSpeed *= 2f;

            isDodge = true;
            Invoke("DodgeOff", 0.5f);

            playerAnimator.SetTrigger("doDodge");
        }
    }

    void DodgeOff()
    {
        moveSpeed *= 0.5f;

        isDodge = false;
    }

    void Shot()
    {
        if (equipGun == null) return;

        shotDelay += Time.deltaTime;
        isShotReady = equipGun.rate < shotDelay;

        if (doShot && isShotReady && !isDodge)//  && !isSwap)
        {
            equipGun.Shot();

            playerAnimator.SetTrigger("doShot");

            shotDelay = 0;
        }
        
    }

    void Swap()
    {
        if (doSwap)
        {
            playerAnimator.SetTrigger("doSwap");
        }
    }
}
