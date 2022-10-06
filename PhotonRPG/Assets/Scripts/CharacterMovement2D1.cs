using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine.Examples
{

    [AddComponentMenu("")] // Don't display in add component menu
    public class CharacterMovement2D1 : MonoBehaviour
    {
        public KeyCode sprintJoystick = KeyCode.JoystickButton2;
        public KeyCode jumpJoystick = KeyCode.JoystickButton0;
        public KeyCode sprintKeyboard = KeyCode.LeftShift;
        public KeyCode jumpKeyboard = KeyCode.Space;
        public float jumpVelocity = 7f;
        public float groundTolerance = 0.2f;
        public bool checkGroundForJump = true;

        float speed = 0f;
        bool isSprinting = false;
        Animator anim;
        Vector2 input;
        float velocity;
        bool headingleft = false;
        Quaternion targetrot;
        Rigidbody rigbody;

        // Use this for initialization
        void Start()
        {
            anim = GetComponentInChildren<Animator>();
            rigbody = GetComponent<Rigidbody>();
            targetrot = transform.rotation;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                input.x = Input.GetAxis("Horizontal");
                speed = (input.x);
                Debug.Log(speed);
                // speed = Mathf.Abs(input.x);
                // speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
                if (Mathf.Abs(rigbody.velocity.x) < 10)
                    rigbody.AddForce(new Vector3(speed * Time.fixedDeltaTime * 50, 0, 0), ForceMode.Impulse);
                GetComponentInChildren<SpriteRenderer>().flipX = input.x > 0;
                // anim.SetFloat("Speed", speed);
                anim.SetBool("isMove", true);
            }
            else
                anim.SetBool("isMove", false);

            // Check if direction changes
            // if ((input.x < 0f && !headingleft) || (input.x > 0f && headingleft))
            // {
            // if (input.x < 0f) targetrot = Quaternion.Euler(0, 270, 0);
            // if (input.x > 0f) targetrot = Quaternion.Euler(0, 90, 0);
            //     headingleft = !headingleft;
            // }
            // Rotate player if direction changes
            // transform.rotation = Quaternion.Lerp(transform.rotation, targetrot, Time.deltaTime * 20f);

            // set speed to horizontal inputs

            // set sprinting
            // if ((Input.GetKeyDown(sprintJoystick) || Input.GetKeyDown(sprintKeyboard)) && input != Vector2.zero) isSprinting = true;
            // if ((Input.GetKeyUp(sprintJoystick) || Input.GetKeyUp(sprintKeyboard)) || input == Vector2.zero) isSprinting = false;
            // anim.SetBool("isSprinting", isSprinting);
        }

        private void Update()
        {
            // Jump
            if ((Input.GetKeyDown(jumpJoystick) || Input.GetKeyDown(jumpKeyboard)))
            {
                rigbody.AddForce(new Vector3(Mathf.Abs(rigbody.velocity.x) > 10 ? 0 : rigbody.velocity.x, jumpVelocity, 0), ForceMode.Impulse);
            }
        }

        public bool isGrounded()
        {
            if (checkGroundForJump)
                return Physics.Raycast(transform.position, Vector3.down, groundTolerance);
            else
                return true;
        }
    }

}
