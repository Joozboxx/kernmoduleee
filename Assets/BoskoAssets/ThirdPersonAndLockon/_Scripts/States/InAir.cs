using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAir : State<PlayerBehaviour>, IState
{
    public InAir(PlayerBehaviour actor) : base(actor)
    {

    }

    public override void OnStateEnter()
    {
        Owner.anim.applyRootMotion = true;
        Owner.cc.enabled = true;
    }

    public override void OnStateExit()
    {

    }

    public override void OnStateUpdate()
    {
        Owner.Grounded();
        if (Input.GetKey(Owner.pc.grab))
        {
            Owner.LedgeInfo();
        }
        if (Owner.anim.GetCurrentAnimatorStateInfo(0).IsName("Fall"))
        {
            if (!Owner.RayHit(Owner.transform.position + Owner.transform.up, Owner.transform.forward, 1, Owner.everything))
            {
                Owner.cc.Move(Owner.transform.forward * Time.deltaTime * 3);
            }
        }
        if (Owner.grounded && Owner.ccGrounded)
        {
            Owner.stateMachine.SwitchState(typeof(Locomotion));
        }
        else if (Input.GetKeyDown(Owner.pc.jump))
        {
            if (Owner.doubleJump)
            {
                Owner.anim.SetTrigger("Jump");
            }
        }
        if (Owner.Underwater(1f))
        {
            Owner.stateMachine.SwitchState(typeof(Swimming));
        }
    }
}
