using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Locomotion : State<PlayerBehaviour>, IState
{

    bool vaulting = false;

    public Locomotion(PlayerBehaviour actor) : base(actor)
    {

    }

    public override void OnStateEnter()
    {
        Owner.anim.ResetTrigger("Jump");
        Owner.anim.ResetTrigger("fall");
        Owner.anim.ResetTrigger("Slide");

        Owner.anim.applyRootMotion = true;
        Owner.cc.enabled = true;
        Owner.airtime = 0;
        Owner.lockRotation = false;

        vaulting = false;
    }

    public override void OnStateExit()
    {
        Owner.anim.ResetTrigger("LetGo");
        Owner.anim.ResetTrigger("Slide");
    }

    Vector3 oldPos = Vector3.zero;
    public override void OnStateUpdate()
    {
        if (!Owner.lockedOn && !Owner.lockRotation)
        {
            Owner.RotateToCam();
        }

        Armed();
        AgainstWall();
        GrabLedge();
        Movement();
        JumpOrVault();

        if (vaulting == false)
        {
            Slide();
        }

        if (Owner.Underwater(0.75f))
        {
            Owner.stateMachine.SwitchState(typeof(Swimming));
        }

        Owner.CanTarget();
        Owner.Grounded();

        if (Owner.airtime > 0.75f)
        {
            Owner.anim.SetTrigger("fall");
            Owner.stateMachine.SwitchState(typeof(InAir));
        }

        oldPos = Owner.transform.position;
    }

    float timeToLeanAgainstWall = 0.4f;
    float timeWalkingAgainstWall;

    float leanTransition = 0;

    void AgainstWall()
    {
        if (Owner.armed)
        {
            return;
        }
        if (Mathf.Abs(Vector3.Distance(oldPos, Owner.transform.position)) < 0.005f && Owner.anim.GetFloat("y+x") > 0 && Owner.lockedOn == false)
        {
            timeWalkingAgainstWall += Time.deltaTime;
            if (timeWalkingAgainstWall > timeToLeanAgainstWall)
            {
                if (Owner.RayHit(Owner.transform.position + Vector3.up * 2, Owner.transform.forward, 0.3f, Owner.everything, true, Color.white))
                {
                    if (!Owner.RayHit(Owner.transform.position + Owner.transform.right * 0.75f + Vector3.up + (Owner.transform.forward * -0.5f), Owner.transform.forward, 1.5f, Owner.everything, true, Color.white)
                        && !Owner.RayHit(Owner.transform.position + Vector3.up, Owner.transform.right, 0.75f, Owner.everything, true, Color.white))
                    {
                        Lean(false);
                    }
                    else if (!Owner.RayHit(Owner.transform.position - Owner.transform.right * 0.75f + Vector3.up + (Owner.transform.forward * -0.5f), Owner.transform.forward, 1.5f, Owner.everything, true, Color.white)
                        && !Owner.RayHit(Owner.transform.position + Vector3.up, Owner.transform.right, -0.75f, Owner.everything, true, Color.white))
                    {
                        Lean(true);
                    }
                }
            }
        }
        else
        {
            if (leanTransition != 0)
            {
                if (leanTransition > 0)
                {
                    leanTransition -= Time.deltaTime * 3;
                    if (leanTransition < 0)
                    {
                        leanTransition = 0;
                    }
                }
                if (leanTransition < 0)
                {
                    leanTransition += Time.deltaTime * 3;
                    if (leanTransition > 0)
                    {
                        leanTransition = 0;
                    }
                }

                Owner.anim.applyRootMotion = true;
                Owner.lockRotation = false;

                leanTransition = Mathf.Clamp(leanTransition, -1, 1);

                Owner.anim.SetLayerWeight(1, Mathf.Abs(leanTransition));
            }

            timeWalkingAgainstWall = 0;
        }
        Owner.oc.cameraOffset = (Owner.oc.transform.right * leanTransition);
    }

    void Lean(bool left)
    {
        Owner.anim.applyRootMotion = false;
        Owner.lockRotation = true;
        if (left)
        {
            if (leanTransition > -1)
            {
                leanTransition -= Time.deltaTime * 1.5f;
            }
            else
            {
                leanTransition = -1;
            }
        }
        else
        {
            if (leanTransition < 1)
            {
                leanTransition += Time.deltaTime * 1.5f;
            }
            else
            {
                leanTransition = 1;
            }
        }
        Owner.anim.SetLayerWeight(1, Mathf.Abs(leanTransition));
        Owner.anim.SetFloat("Leaning", leanTransition);
    }

    void Armed()
    {
        if (Input.GetKeyDown(Owner.pc.equip))
        {
            Owner.armed = !Owner.armed;

            Owner.weaponHand.SetActive(Owner.armed);
            Owner.weaponBack.SetActive(!Owner.armed);
        }
    }

    void JumpOrVault()
    {
        if (Owner.lockedOn)
        {
            if (Input.GetKeyDown(Owner.pc.jump) && Owner.anim.GetCurrentAnimatorStateInfo(0).IsName("Dodge") == false)
            {
                Owner.anim.SetTrigger("Jump");
            }
            return;
        }
        if (Owner.armed)
        {
            if (Input.GetKey(Owner.pc.jump))
            {
                Owner.anim.SetBool("Dodging", true);
            }
            else
            {
                Owner.anim.SetBool("Dodging", false);
            }
            return;
        }
        if (Input.GetKeyDown(Owner.pc.jump) && Owner.lockedOn == false)
        {
            if (Owner.RayHit(Owner.transform.position + Vector3.up * 0.5f, Owner.transform.forward, 3, Owner.everything))
            {
                Vector3 nextCheck = Owner.lastCachedhit.point;
                Vector3 normalHit = Owner.lastCachedhit.normal;

                if (Owner.RayHit(Owner.lastCachedhit.point + Vector3.up * 3f + Owner.transform.forward * 0.035f, Vector3.down, 3f, Owner.everything, true, Color.white)) // HeightCheck
                {
                    Vector3 height = Owner.lastCachedhit.point;
                    if (CatJump(nextCheck, height) || FenceJump(nextCheck, height, normalHit))
                    {
                        return;
                    }
                }
            }
            Owner.anim.SetTrigger("Jump");
            Owner.stateMachine.SwitchState(typeof(InAir));
        }
    }

    bool CatJump(Vector3 nextCheck, Vector3 height)
    {
        if (Owner.anim.GetFloat("y+x") > 0.5f && vaulting == false)
        {
            if (Owner.RayHit(nextCheck + Vector3.up + Owner.transform.forward, Vector3.down, 1f, Owner.everything, true, Color.green))
            {
                Vector3 heightShot = nextCheck;
                heightShot.y = height.y + 0.1f;
                if (!Owner.RayHit(heightShot, Owner.transform.forward, 4, Owner.everything, true, Color.yellow))
                {
                    if (Owner.RayHit(nextCheck + Owner.transform.forward * 2, -Owner.transform.forward, 1.5f, Owner.everything, true, Color.magenta))
                    {
                        if (!Owner.RayHit(nextCheck + Owner.transform.forward * 2 + Vector3.up * 0.5f, Vector3.down, 0.95f, Owner.everything, true, Color.black))
                        {
                            Vector3 temp = new Vector3(Owner.lastCachedhit.point.x, height.y, Owner.lastCachedhit.point.z);

                            temp -= Owner.transform.forward * 0.3f;

                            Debug.DrawLine(Owner.transform.position, temp, Color.white, 10);
                            Debug.DrawLine(temp, (Owner.lastCachedhit.point + Owner.transform.forward * 0.5f), Color.white, 10);

                            Vector3 newCachedPoint = Owner.lastCachedhit.point;
                            newCachedPoint.y = Owner.transform.position.y;

                            LerpInfo[] lerpTrajectory;

                            if (Owner.anim.GetBool("Sprinting"))
                            {
                                lerpTrajectory = new LerpInfo[] { new LerpInfo(0.4f, temp), new LerpInfo(0.4f, newCachedPoint + Owner.transform.forward * 2.5f) };
                                Owner.anim.SetFloat("VaultType", 1);
                            }
                            else
                            {
                                lerpTrajectory = new LerpInfo[] { new LerpInfo(0.35f, height), new LerpInfo(0.25f, height + Owner.transform.forward * 2), new LerpInfo(0.25f, newCachedPoint + Owner.transform.forward * 1.75f) };
                                Owner.anim.SetFloat("VaultType", 0);
                            }

                            if (Owner.PlayerCanStand(lerpTrajectory[lerpTrajectory.Length-1].target))
                            {
                                Owner.anim.SetTrigger("Vault");

                                vaulting = true;

                                Owner.StartCoroutine(EnableVault(true, true, lerpTrajectory));
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    bool FenceJump(Vector3 nextCheck, Vector3 height, Vector3 normal)
    {
        if (Vector3.Angle(normal, Owner.transform.forward) < 160 && vaulting == false)
        {
            return false;
        }
        float heightClimb = height.y - Owner.transform.position.y;
        if (!Owner.RayHit(nextCheck + (Vector3.up * 3) + Owner.transform.forward * 0.3f, Vector3.down, 3.4f, Owner.everything, true, Color.blue))
        {
            if (Owner.RayHit(nextCheck + Vector3.up + Owner.transform.forward * 1f, Vector3.down, 1.6f, Owner.everything, true, Color.black))
            {
                if (!Owner.RayHit(nextCheck + Owner.transform.forward * 0.25f, Owner.transform.forward, 1.75f, Owner.everything, true, Color.white))
                {
                    LerpInfo[] lerpTrajectory;
                    if (heightClimb > 2)
                    {
                        lerpTrajectory = new LerpInfo[]
                        {
                            new LerpInfo(0.3f, nextCheck - Owner.transform.forward * 0.5f),
                            new LerpInfo(1.2f, height - Vector3.up * 0.5f - Owner.transform.forward * 0.3f),
                            new LerpInfo(0.25f, height - Vector3.up * 0.75f + Owner.transform.forward * 0.25f),
                            new LerpInfo(0.3f, Owner.lastCachedhit.point + Owner.transform.forward * 1f)
                        };
                        Owner.anim.SetFloat("VaultType", 3);
                    }
                    else
                    {
                        lerpTrajectory = new LerpInfo[]
                        {
                            new LerpInfo(0.5f, height - Vector3.up * 0.3f, PlayerBehaviour.UseIK.left, height),
                            new LerpInfo(0.2f, Owner.lastCachedhit.point + Owner.transform.forward * 1f)
                        };
                        Owner.anim.SetFloat("VaultType", 2);
                    }

                    if (Owner.PlayerCanStand(lerpTrajectory[lerpTrajectory.Length - 1].target))
                    {
                        Owner.anim.SetTrigger("Vault");

                        vaulting = true;

                        Owner.StartCoroutine(EnableVault(true, true, lerpTrajectory));
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IEnumerator EnableVault(bool enableRoot, bool ignoreAir, params LerpInfo[] pos)
    {
        yield return Owner.StartCoroutine(Owner.LerpToPos(pos, enableRoot, ignoreAir));
        vaulting = false;
        Debug.Log("can vault again");
    }
    
    void GrabLedge()
    {
        if (Owner.grounded == false)
        {
            if (Input.GetKey(Owner.pc.grab))
            {
                Owner.LedgeInfo();
            }
        }
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Owner.anim.SetFloat("x", x);
        Owner.anim.SetFloat("y", y);
        Owner.anim.SetFloat("y+x", (Mathf.Abs(x) + Mathf.Abs(y)));

        //Walking
        if (x != 0 || y != 0)
        {
            Owner.anim.SetBool("Walking", true);
        }
        else
        {
            Owner.anim.SetBool("Walking", false);
        }
        //Sprinting
        if (Input.GetKey(Owner.pc.sprint))
        {
            Owner.anim.SetBool("Sprinting", true);
        }
        else
        {
            Owner.anim.SetBool("Sprinting", false);
        }
    }

    void Slide()
    {
        // Slide
        if (Input.GetKeyDown(Owner.pc.crouch) && Owner.anim.GetFloat("y+x") > 0.2f)
        {
            Owner.stateMachine.SwitchState(typeof(Sliding));
        }
    }
}
