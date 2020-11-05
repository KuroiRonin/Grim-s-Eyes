using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator;
    int isWalkingHash;
    int isRunningHash;
    //testing line of code
    int isPickingUpItemHash;
    int isWalkingLeftHash;
    int isRunningLeftHash;

    int isWalkingRightHash;
    int isRunningRightHash;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        //testing line of code
        isPickingUpItemHash = Animator.StringToHash("isPickingUpItem");
        isWalkingLeftHash = Animator.StringToHash("isWalkingLeft");
        isRunningLeftHash = Animator.StringToHash("isRunningLeft");

        isWalkingRightHash = Animator.StringToHash("isWalkingRight");
        isRunningRightHash = Animator.StringToHash("isRunningRight");
       // Debug.Log(animator);
    }

    // Update is called once per frame
    void Update()
    {
        bool isrunning = animator.GetBool(isRunningHash);
        bool isWalking = animator.GetBool(isWalkingHash);
        //testing line of code
        bool isPickingUpItem = animator.GetBool(isPickingUpItemHash);
        bool isWalkingLeft = animator.GetBool(isWalkingLeftHash);
        bool isRunningLeft = animator.GetBool(isRunningLeftHash);
        bool isWalkingRight = animator.GetBool(isWalkingRightHash);
        bool isRunningRight = animator.GetBool(isRunningRightHash);
        
        bool forwardPressed = Input.GetKey("w");
        bool runPressed = Input.GetKey("left shift");
        //test line of code
        bool liftPressed = Input.GetKey("g");
        bool leftPressed = Input.GetKey("a");
        bool leftrunPressed = Input.GetKey("left shift");//big tester

        bool rightPressed = Input.GetKey("d");
        bool rightrunPressed = Input.GetKey("left shift");


        //Walking
        if (!isWalking && forwardPressed)
        {
            animator.SetBool(isWalkingHash, true);
        }

        if (isWalking && !forwardPressed)
        {
            animator.SetBool(isWalkingHash, false);
        }



        //Running
        if (!isrunning && (forwardPressed && runPressed))
        {
            animator.SetBool(isRunningHash, true);
        }
        if (isrunning && (!forwardPressed || !runPressed))
        {
            animator.SetBool(isRunningHash, false);
        }


        //test line of code
        //Walking Left
        if (!isWalkingLeft && leftPressed)
        {
            animator.SetBool(isWalkingLeftHash, true);
        }

        if (isWalkingLeft && !leftPressed)
        {
            animator.SetBool(isWalkingLeftHash, false);
        }


        //Running left
        //Running
        if (!isRunningLeft && (leftPressed && leftrunPressed))
        {
            animator.SetBool(isRunningLeftHash, true);
        }
        if (isRunningLeft && (!leftPressed || !leftrunPressed))
        {
            animator.SetBool(isRunningLeftHash, false);
        }




        //Walking Right
        if (!isWalkingRight && rightPressed)
        {
            animator.SetBool(isWalkingRightHash, true);
        }

        if (isWalkingRight && !rightPressed)
        {
            animator.SetBool(isWalkingRightHash, false);
        }


        //Running Right
        //Running
        if (!isRunningRight && (rightPressed && rightrunPressed))
        {
            animator.SetBool(isRunningRightHash, true);
        }
        if (isRunningRight && (!rightPressed || !rightrunPressed))
        {
            animator.SetBool(isRunningRightHash, false);
        }




        //test line of code
        //Picking Up Items
        if (!isPickingUpItem && liftPressed)
        {
            animator.SetBool(isPickingUpItemHash, true);
        }

        if (isPickingUpItem && !liftPressed)
        {
            animator.SetBool(isPickingUpItemHash, false);
        }

    }
}
