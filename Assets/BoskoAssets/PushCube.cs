using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushCube : MonoBehaviour
{
    public Transform player;
    public float pushDistance = 1.35f;
    public float pushForce = 1;
    [Space]
    public Vector3 goalDestination;
    public float goalMarge = 1;
    public bool atGoal;
    [Space]
    public bool setGoalToBase;

    private void OnValidate()
    {
        if (setGoalToBase)
        {
            goalDestination = transform.position;
        }
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= pushDistance)
        {
            transform.position -= ((player.transform.position - transform.position) * pushForce * Time.deltaTime);
        }
        if (Vector3.Distance(goalDestination, transform.position) <= goalMarge)
        {
            atGoal = true;
        }
        else
        {
            atGoal = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, goalDestination);
    }
}
