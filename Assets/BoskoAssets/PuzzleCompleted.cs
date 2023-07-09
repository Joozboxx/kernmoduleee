using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleCompleted : MonoBehaviour
{
    public GameObject skull;
    public GamePicker gameManager;
    public Transform player;
    public Vector3 spawnPos;
    public GameObject puzzleGate;
    public GameObject mainGate;
    public bool setSpawn;
    [Space]
    public PuzzleCompleted[] allPuzzles;
    public List<PushCube> cubes = new List<PushCube>();
    [Space]
    public bool completed;

    private void OnValidate()
    {
        if (setSpawn)
        {
            spawnPos = player.position;
        }
    }

    private void Update()
    {
        if (completed)
            return;

        foreach (var c in cubes)
        {
            if (c.atGoal == false)
            {
                return;
            }
        }
        if (completed == false)
        {
            completed = true;
            puzzleGate.SetActive(true);
            skull.SetActive(true);
            foreach (var p in allPuzzles)
            {
                if (p.completed == false)
                {
                    StartCoroutine(ResetPlayer(true));
                    return;
                }
            }
            StartCoroutine(ResetPlayer(false));
            mainGate.SetActive(false);
        }
    }

    IEnumerator ResetPlayer(bool rotateFloor)
    {
        Animator anim = player.GetComponent<Animator>();
        anim.applyRootMotion = false;
        yield return new WaitForSeconds(0.25f);
        player.transform.position = spawnPos;
        yield return new WaitForSeconds(0.25f);
        anim.applyRootMotion = true;

        if (rotateFloor)
        {
            gameManager.StartRotation();
        }
        else
        {
            gameManager.ResetGround();
        }
    }
}
