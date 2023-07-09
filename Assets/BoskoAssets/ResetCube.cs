using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCube : MonoBehaviour
{
    public Transform player;
    public float resetDistance;
    public List<GameObject> cubes;
    public Dictionary<GameObject, Vector3> cubepositions = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        foreach (var c in cubes)
        {
            cubepositions.Add(c, c.transform.position);
        }
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= resetDistance)
        {
            foreach (var c in cubepositions)
            {
                c.Key.transform.position = c.Value;
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var c in cubes)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, c.transform.position);
        }
    }
}
