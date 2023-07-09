using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : State<PlayerBehaviour>, IState
{
    float defaultHeight;
    Vector3 defaultCenter;
    Vector3 climbingOffset = Vector3.zero;

    public Climbing(PlayerBehaviour actor) : base(actor)
    {

    }

    public override void OnStateEnter()
    {
        defaultHeight = Owner.cc.height;
        defaultCenter = Owner.cc.center;

        mountingWall = false;
        climbCooldown = true;
        Owner.StartCoroutine(ClimbCooldown(Owner.anim, 0.3f, false));

        Owner.anim.applyRootMotion = false;
        Owner.cc.enabled = false;

        Owner.RayHit(Owner.transform.position + (Owner.transform.forward * 0.6f) + (Owner.transform.up * 1.7f), Vector3.down, 0.35f, Owner.everything);

        Vector3 newPlayerPos = Owner.transform.position;
        newPlayerPos.y = Owner.lastCachedhit.point.y - Owner.grabHeight;

        Owner.transform.position = newPlayerPos;

        if (!Owner.PlayerFaceWall(Owner, new Vector3(0, 1, 0), Owner.transform.forward, 2))
        {
            Owner.stateMachine.SwitchState(typeof(Locomotion));
            return;
        }
        if (!Owner.PlayerToWall(Owner, Owner.transform.forward, false, 1.75f))
        {
            Owner.stateMachine.SwitchState(typeof(Locomotion));
            return;
        }

        Owner.cc.height = 1.2f;

        Owner.anim.SetTrigger("Climb");
    }

    public override void OnStateExit()
    {
        Owner.cc.height = defaultHeight;
        Owner.cc.center = defaultCenter;

        Owner.cc.enabled = true;
        Owner.airtime = 0;
    }

    private bool mountingWall;
    private bool climbCooldown;
    public override void OnStateUpdate()
    {
        climbingOffset = Owner.heightOffset + Owner.transform.forward * Owner.forwardOffset;

        Owner.anim.SetFloat("LookDirection", Owner.oc.transform.eulerAngles.y);
        
        if (mountingWall == true || climbCooldown == true)
        {
            return;
        }

        ShimmyClimbing();
        CheckClimbOptions();

        if (Input.GetKeyDown(Owner.pc.jump))
        {
            if (!Owner.RayHit(Owner.transform.position + Owner.transform.forward * 0.75f + Owner.transform.up * 1.25f + climbingOffset, Owner.transform.up, 1.75f, Owner.everything) &&
                    !Owner.RayHit(Owner.transform.position + Owner.transform.up * 1.5f + climbingOffset, Owner.transform.forward, 1f, Owner.everything))
            {
                mountingWall = true;
                Owner.anim.SetTrigger("Jump");
                Owner.LerpToPosition(true, true, new LerpInfo(0.3f, Owner.transform.position + Vector3.up * 2.25f), new LerpInfo(0.5f, Owner.transform.position + Owner.transform.forward + Vector3.up * 2));
            }
        }

        if (Input.GetKeyDown(Owner.pc.letgo))
        {
            Owner.anim.SetTrigger("LetGo");
            Owner.stateMachine.SwitchState(typeof(InAir));
        }
    }

    void CheckClimbOptions()
    {
        Vector3 dir = Vector3.up * Input.GetAxis("Vertical");
        dir += Owner.transform.right * Input.GetAxis("Horizontal");

        dir = dir.normalized;

        Debug.DrawRay(Owner.transform.position + Vector3.up, dir * 2, Color.white);
    }

    IEnumerator ClimbCooldown(Animator anim, float cld, bool recalculateHeight)
    {
        climbCooldown = true;
        yield return new WaitForSeconds(cld);
        if (Owner != null)
        {
            Owner.PlayerToWall(Owner, Owner.transform.forward, false, 1.75f);
        }
        yield return new WaitForEndOfFrame();
        if (recalculateHeight)
        {
            Owner.RayHit(Owner.transform.position + (Owner.transform.forward * 0.6f) + (Owner.transform.up * 1.7f) + climbingOffset, Vector3.down, 0.35f, Owner.everything);

            Vector3 newPlayerPos = Owner.transform.position;

            Owner.transform.position = newPlayerPos;
        }
        climbCooldown = false;
    }

    void ShimmyClimbing()
    {
        int shimmyDirection = (int)Input.GetAxisRaw(Owner.pc.inputHorizontal);
        Owner.anim.SetInteger("ClimbDirection", 0);
        if (shimmyDirection != 0)
        {
            Owner.PlayerToWall(Owner, Owner.transform.forward, false, 1.75f);
            if (shimmyDirection == 1)
            {
                if (!Owner.RayHit(Owner.transform.position + Owner.transform.up * 0.5f + climbingOffset, Owner.transform.right, 0.65f, Owner.everything))
                {
                    if (Owner.RayHit(Owner.transform.position + Owner.transform.right * 0.6f + Owner.transform.up * 1.2f + climbingOffset, Owner.transform.forward, 0.35f, Owner.everything))
                    {
                        if (!Owner.RayHit(Owner.transform.position + Owner.transform.forward * 0.4f + Owner.transform.up * 1.4f + climbingOffset, Owner.transform.right, 0.5f, Owner.everything))
                        {
                            Owner.transform.position += Owner.transform.right * Time.deltaTime * 2;
                            Owner.anim.SetInteger("ClimbDirection", shimmyDirection);
                        }
                    }
                }
            }
            else if (shimmyDirection == -1)
            {
                if (!Owner.RayHit(Owner.transform.position + Owner.transform.up * 0.5f + climbingOffset, -Owner.transform.right, 0.65f, Owner.everything))
                {
                    if (Owner.RayHit(Owner.transform.position + -Owner.transform.right * 0.6f + Owner.transform.up * 1.2f + climbingOffset, Owner.transform.forward, 0.35f, Owner.everything))
                    {
                        if (!Owner.RayHit(Owner.transform.position + Owner.transform.forward * 0.4f + Owner.transform.up * 1.4f + climbingOffset, -Owner.transform.right, 0.5f, Owner.everything))
                        {
                            Owner.transform.position -= Owner.transform.right * Time.deltaTime * 2;
                            Owner.anim.SetInteger("ClimbDirection", shimmyDirection);
                        }
                    }
                }
            }
        }
        if (Owner.anim.GetInteger("ClimbDirection") == 0)
        {
            CornerClimb(shimmyDirection);
        }
    }

    float cornerCooldown = 1.1f;
    float cornerSpeed = 0.5f;
    void CornerClimb(int shimmyDir)
    {
        if (shimmyDir == -1) // Left
        {
            // OUTSIDE RIGHT
            Vector3 startOffset = Owner.transform.position + Vector3.up * 0.1f + climbingOffset;
            if (!Owner.RayHit(startOffset, -Owner.transform.right, 1.25f, Owner.everything) && !Owner.RayHit(startOffset + Vector3.up, -Owner.transform.right, 1.25f, Owner.everything))
            {
                startOffset += (-Owner.transform.right * 1.25f);
                if (!Owner.RayHit(startOffset, Owner.transform.forward, 1, Owner.everything) && !Owner.RayHit(startOffset + Vector3.up, Owner.transform.forward, 1, Owner.everything))
                {
                    startOffset += (Owner.transform.forward);
                    if (Owner.RayHit(startOffset, Owner.transform.right, 1, Owner.everything))
                    {
                        startOffset += Vector3.up;
                        if (Owner.RayHit(startOffset, Owner.transform.right, 1f, Owner.everything))
                        {
                            Vector3 lerpPos = Owner.lastCachedhit.point;
                            startOffset += (Owner.transform.right * 0.825f) + (Vector3.up * 0.5f);
                            if (Owner.RayHit(startOffset, -Owner.transform.up, 0.5f, Owner.everything))
                            {
                                Owner.cc.enabled = false;

                                Owner.anim.SetFloat("CornerType", 0);
                                Owner.anim.SetTrigger("Corner");

                                Owner.transform.Rotate(0, 90, 0);
                                Vector3 offsetClimb = lerpPos - (Owner.transform.forward * 0.4f) - Vector3.up * 1.7f;

                                Owner.LerpToPosition(false, true, new LerpInfo(cornerSpeed / 2, Owner.transform.position - (Owner.transform.forward * 1f)), new LerpInfo(cornerSpeed / 2, offsetClimb));
                                Owner.StartCoroutine(ClimbCooldown(Owner.anim, cornerCooldown, true));
                            }
                        }
                    }
                }
            }
            else
            {
                // INSIDE LEFT
                Transform tOwner = Owner.transform;
                if (Owner.RayHit(tOwner.position + Vector3.up * 0.5f + climbingOffset, -tOwner.right, 0.7f, Owner.everything))
                {
                    if (Owner.RayHit(tOwner.position + Vector3.up * 0.5f + (-tOwner.forward * 0.5f) + climbingOffset, -tOwner.right, 0.7f, Owner.everything))
                    {
                        Vector3 targetPos = Owner.lastCachedhit.point;
                        if (Owner.RayHit(tOwner.position + (Vector3.up * 1.6f) + (-tOwner.forward * 0.2f) + (-tOwner.right * 0.7f) + climbingOffset, -tOwner.up, 0.7f, Owner.everything))
                        {
                            if (Owner.RayHit(tOwner.position + (Vector3.up * 1.6f) + (-tOwner.forward * 0.8f) + (-tOwner.right * 0.7f) + climbingOffset, -tOwner.up, 0.7f, Owner.everything))
                            {
                                Owner.cc.enabled = false;

                                Owner.anim.SetFloat("CornerType", 2);
                                Owner.anim.SetTrigger("Corner");

                                Owner.transform.Rotate(0, -90, 0);

                                Vector3 firstTarget = Owner.transform.position - Owner.transform.forward * 0.1f;
                                Vector3 offsetClimb = targetPos - (Owner.transform.forward * 0.4f) - Vector3.up * 1.1f;

                                Owner.LerpToPosition(false, true, new LerpInfo(cornerSpeed / 2, firstTarget), new LerpInfo(cornerSpeed / 2, offsetClimb));
                                Owner.StartCoroutine(ClimbCooldown(Owner.anim, cornerCooldown, true));
                            }
                        }
                    }
                }
            }
        }
        else if (shimmyDir == 1) // Right
        {
            // OUTSIDE RIGHT
            Vector3 startOffset = Owner.transform.position + Vector3.up * 0.1f + climbingOffset;
            if (!Owner.RayHit(startOffset, Owner.transform.right, 1.25f, Owner.everything) && !Owner.RayHit(startOffset + Vector3.up, Owner.transform.right, 1.25f, Owner.everything))
            {
                startOffset += (Owner.transform.right * 1.25f);
                if (!Owner.RayHit(startOffset, Owner.transform.forward, 1, Owner.everything) && !Owner.RayHit(startOffset + Vector3.up, Owner.transform.forward, 1, Owner.everything))
                {
                    startOffset += (Owner.transform.forward);
                    if (Owner.RayHit(startOffset, -Owner.transform.right, 1, Owner.everything))
                    {
                        startOffset += Vector3.up;
                        if (Owner.RayHit(startOffset, -Owner.transform.right, 1f, Owner.everything))
                        {
                            Vector3 lerpPos = Owner.lastCachedhit.point;
                            startOffset += (-Owner.transform.right * 0.825f) + (Vector3.up * 0.5f);
                            if (Owner.RayHit(startOffset, -Owner.transform.up, 0.5f, Owner.everything))
                            {
                                Owner.cc.enabled = false;

                                Owner.anim.SetFloat("CornerType", 1);
                                Owner.anim.SetTrigger("Corner");

                                Owner.transform.Rotate(0, -90, 0);

                                Vector3 offsetClimb = lerpPos - (Owner.transform.forward * 0.4f) - Vector3.up * 1.7f;

                                Owner.LerpToPosition(false, true, new LerpInfo(cornerSpeed / 2, Owner.transform.position - (Owner.transform.forward * 1f)), new LerpInfo(cornerSpeed / 2, offsetClimb));
                                Owner.StartCoroutine(ClimbCooldown(Owner.anim, cornerCooldown, true));
                            }
                        }
                    }
                }
            }
            else
            {
                // INSIDE RIGHT
                Transform tOwner = Owner.transform;
                if (Owner.RayHit(tOwner.position + Vector3.up * 0.5f + climbingOffset, tOwner.right, 0.7f, Owner.everything))
                {
                    if (Owner.RayHit(tOwner.position + Vector3.up * 0.5f + (-tOwner.forward * 0.5f) + climbingOffset, tOwner.right, 0.7f, Owner.everything))
                    {
                        Vector3 targetPos = Owner.lastCachedhit.point;
                        if (Owner.RayHit(tOwner.position + (Vector3.up * 1.6f) + (-tOwner.forward * 0.2f) + (tOwner.right * 0.7f) + climbingOffset, -tOwner.up, 0.7f, Owner.everything))
                        {
                            if (Owner.RayHit(tOwner.position + (Vector3.up * 1.6f) + (-tOwner.forward * 0.8f) + (tOwner.right * 0.7f) + climbingOffset, -tOwner.up, 0.7f, Owner.everything))
                            {
                                Owner.cc.enabled = false;

                                Owner.anim.SetFloat("CornerType", 3);
                                Owner.anim.SetTrigger("Corner");

                                Owner.transform.Rotate(0, 90, 0);

                                Vector3 firstTarget = Owner.transform.position - Owner.transform.forward * 0.1f;
                                Vector3 offsetClimb = targetPos - (Owner.transform.forward * 0.4f) - Vector3.up * 1.1f;

                                Owner.LerpToPosition(false, true, new LerpInfo(cornerSpeed / 2, firstTarget), new LerpInfo(cornerSpeed / 2, offsetClimb));
                                Owner.StartCoroutine(ClimbCooldown(Owner.anim, cornerCooldown, true));
                            }
                        }
                    }
                }
            }
        }
    }
}