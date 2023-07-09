using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underwater : State<PlayerBehaviour>, IState
{
    public Underwater(PlayerBehaviour actor) : base(actor)
    {

    }

    float swimmingSpeed = 4;
    public override void OnStateEnter()
    {
        Owner.anim.SetBool("Underwater", true);
        Owner.anim.applyRootMotion = false;
        Owner.cc.enabled = true;
    }

    public override void OnStateExit()
    {
        Owner.anim.SetBool("Underwater", false);
        Owner.anim.applyRootMotion = true;
        Owner.cc.enabled = true;
    }

    public override void OnStateUpdate()
    {
        Owner.RotateToCam(false, 0.3f);

        float xy = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
        xy = Mathf.Clamp(xy, 0, 1);
        Owner.anim.SetFloat("y+x", xy);

        float heightMovement = 0;

        if (Input.GetKey(Owner.pc.jump))
        {
            heightMovement = 1;
        }
        else if (Input.GetKey(Owner.pc.crouch))
        {
            heightMovement = -1;
        }

        Owner.cc.Move(((Owner.transform.up * heightMovement)) * Time.deltaTime + Owner.transform.forward * swimmingSpeed * xy * Time.deltaTime);

        if (Owner.RayHit(Owner.transform.position, Vector3.up, 1.2f, Owner.water))
        {
            Owner.stateMachine.SwitchState(typeof(Swimming));
        }
    }
}