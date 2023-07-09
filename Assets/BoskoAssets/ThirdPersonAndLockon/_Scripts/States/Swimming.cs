using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Swimming : State<PlayerBehaviour>, IState
{
    public Swimming(PlayerBehaviour actor) : base(actor)
    {

    }

    Vector3 waterHeigth;
    float swimmingSpeed = 4;
    bool mounting = false;

    public override void OnStateEnter()
    {
        mounting = false;
        Owner.anim.applyRootMotion = false;
        Owner.cc.enabled = true;

        Owner.Underwater(0.75f);

        Owner.transform.position = new Vector3(Owner.transform.position.x, Owner.lastCachedhit.point.y - 1.2f, Owner.transform.position.z);

        Owner.anim.SetTrigger("EnterWater");
    }

    public override void OnStateExit()
    {
        Owner.anim.applyRootMotion = true;
        Owner.cc.enabled = true;

        Owner.anim.ResetTrigger("EnterWater");
    }

    public override void OnStateUpdate()
    {
        if (mounting)
        {
            return;
        }

        Owner.RotateToCam(false, 0.3f);

        float xy = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));

        xy = Mathf.Clamp(xy, 0, 1);

        Owner.anim.SetFloat("y+x", xy);

        Owner.cc.Move(Owner.transform.forward * swimmingSpeed * xy * Time.deltaTime);

        Debug.Log(Owner.transform.position.y);

        if (!Owner.RayHit(Owner.transform.position + Vector3.up, Vector3.down, 1, Owner.everything))
        {
            if (Owner.Underwater(2f))
            {
                Owner.transform.position = new Vector3(Owner.transform.position.x, Owner.lastCachedhit.point.y - 1.2f, Owner.transform.position.z);
            }
        }

        if (Owner.Underwater(1f) == false)
        {
            Owner.anim.SetTrigger("LetGo");
            Owner.stateMachine.SwitchState(typeof(Locomotion));
        }

        if (Owner.Underwater(1f) && Input.GetKeyDown(Owner.pc.crouch))
        {
            if (Owner.RayHit(Owner.transform.position, Vector3.down, 0.5f, Owner.everything) == false) // Check if water is deep enough
            {
                Owner.cc.Move(new Vector3(0, -0.25f, 0));
                Owner.stateMachine.SwitchState(typeof(Underwater));
            }
        }

        ClimbUp();
    }

    public void ClimbUp()
    {
        Vector3 climbingOffset = Owner.heightOffset + Owner.transform.forward * Owner.forwardOffset;
        if (Owner.RayHit(Owner.transform.position + Vector3.up * 1f + climbingOffset + Vector3.down * 0.25f, Owner.transform.forward, 0.25f, Owner.everything))
        {
            if (!Owner.RayHit(Owner.transform.position + Owner.transform.forward * 1f + Vector3.up * 1.7f, Owner.transform.up, 1.75f, Owner.everything) &&
                    !Owner.RayHit(Owner.transform.position + Vector3.up * 1f + climbingOffset, Owner.transform.forward, 1f, Owner.everything))
            {
                mounting = true;

                Owner.anim.Play("MountFromWater");
                Owner.LerpToPosition(true, true, new LerpInfo(0.3f, Owner.transform.position + Vector3.up * 1.5f), new LerpInfo(0.5f, Owner.transform.position + Owner.transform.forward + Vector3.up * 1.5f));
            }
        }
    }
}