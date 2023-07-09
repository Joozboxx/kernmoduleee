using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Sliding : State<PlayerBehaviour>, IState
{
    public Sliding(PlayerBehaviour actor) : base(actor)
    {

    }

    float slideDuration = 1f;
    float timeSliding = 0;

    float slideSpeed = 0;

    Vector3 ccOffset;
    float ccHeight;

    public override void OnStateEnter()
    {
        timeSliding = 0;
        slideDuration = 1f;

        ccOffset = Owner.cc.center;
        ccHeight = Owner.cc.height;

        Owner.cc.center = new Vector3(0, 0.5f, 0);
        Owner.cc.height = 0.75f;

        Owner.anim.applyRootMotion = false;
        Owner.cc.enabled = true;

        Owner.anim.ResetTrigger("LetGo");
        Owner.anim.SetTrigger("Slide");

        if (Owner.RayHit(Owner.transform.position + (Owner.transform.up * Owner.startHeight), (Vector3.down), Owner.range, Owner.everything, true, Color.magenta))
        {
            float angle = Vector3.Angle(Owner.lastCachedhit.normal, Vector3.up);
            if (angle == 0)
            {
                slideSpeed = 5;
            }
            else
            {
                slideSpeed = angle / 4;
            }
        }
        else
        {
            Owner.stateMachine.SwitchState(typeof(Locomotion));
        }
        yDifference = Owner.transform.position.y;
    }

    public override void OnStateExit()
    {
        slideDuration = 1f;

        Owner.cc.center = ccOffset;
        Owner.cc.height = ccHeight;

        Owner.anim.ResetTrigger("Slide");
        Owner.anim.applyRootMotion = true;
    }

    float yDifference;
    public override void OnStateUpdate()
    {
        if (Owner.Grounded() == false)
        {
            Owner.anim.SetBool("Land", false);
            Owner.stateMachine.SwitchState(typeof(InAir));
        }

        Owner.RotateToCam(false, 0.1f);

        float xy = (Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical")));

        timeSliding += Time.deltaTime;
        Owner.cc.Move(((Owner.transform.forward * slideSpeed) + (Vector3.down * 15)) * Time.deltaTime);

        if ((yDifference - Owner.transform.position.y) < 0)
        {
            slideDuration -= (Time.deltaTime * 4f);
        }
        if (timeSliding >= slideDuration)
        {
            if ((yDifference - Owner.transform.position.y) <= 0 || xy == 0)
            {
                if (StandingIsBlocked() == false)
                {
                    Owner.anim.SetTrigger("LetGo");
                    Owner.GoToLocomotion();
                }
            }
        }
        yDifference = Owner.transform.position.y;

        if (Input.GetKeyDown(Owner.pc.jump))
        {
            if (StandingIsBlocked() == false)
            {
                Owner.anim.SetTrigger("Jump");
                Owner.stateMachine.SwitchState(typeof(InAir));
            }
        }

        if (Owner.Underwater(1f))
        {
            Owner.stateMachine.SwitchState(typeof(Swimming));
        }
    }

    bool StandingIsBlocked()
    {
        return Owner.RayHit(Owner.transform.position + Vector3.up * 0.5f, Vector3.up, 1.4f, Owner.everything, true, Color.white);
    }
}
