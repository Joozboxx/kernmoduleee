using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GamePicker : MonoBehaviour
{
    public GameObject wheel;
    public GameObject ground;
    public Material groundMaterial;
    public Material bridgeMaterial;
    public Material slotMaterial;
    public SkinnedMeshRenderer slotRenderer;
    public TMP_Text timer;
    public GameObject skullsImage;
    public Material[] gates;
    public GameObject[] flames;
    public GameObject[] cleared;

    public float rotationTime = 5f;
    int currentGame = 0;

    void Start()
    {
        slotMaterial.SetFloat("_y_naar_0", 0.35f);
        slotMaterial.SetFloat("_draai_dat_ding", 2);
        slotMaterial.SetFloat("_y_naar_0v2", 1.68f);

        groundMaterial.SetFloat("_bridge_activate", 0);
        bridgeMaterial.SetFloat("_secret_bridgfe_open", 100);

        StartCoroutine(GameTimer());
        StartCoroutine(GameRotation());
    }

    public void StartRotation()
    {
        skullsImage.SetActive(true);
        Invoke("HideSkullImage", 3);
        StartCoroutine(GameRotation());
    }

    public void ResetGround()
    {
        skullsImage.SetActive(true);
        Invoke("HideSkullImage", 3);
        groundMaterial.SetFloat("_bridge_activate", 0);
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].SetFloat("_ditherfade", 0);
        }
        for (int i = 0; i < flames.Length; i++)
        {
            flames[i].SetActive(false);
        }
        flames[flames.Length - 1].SetActive(true);
        StartCoroutine(OpenBridge());
    }

    void HideSkullImage()
    {
        skullsImage.SetActive(false);
    }

    IEnumerator GameTimer()
    {
        int currentTime = 500000;
        while (true)
        {
            timer.text = currentTime.ToString();
            yield return new WaitForSeconds(1f);
            currentTime -= 1000;
        }
    }

    IEnumerator GroundShader()
    {
        float elapsedTime = 0;
        float waitTime = 2f;
        float currentBridgeFloat = 0;

        for (int i = 0; i < cleared.Length; i++)
        {
            cleared[i].SetActive(false);
            flames[i].SetActive(false);
        }
        while (elapsedTime < waitTime)
        {
            groundMaterial.SetFloat("_bridge_activate", Mathf.Lerp(currentBridgeFloat, 1, (elapsedTime / waitTime)));
            for (int i = 0; i < gates.Length; i++)
            {
                if (i != currentGame - 1)
                {
                    gates[i].SetFloat("_ditherfade", Mathf.Lerp(0, 5, (elapsedTime / waitTime)));
                }
            }
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        flames[currentGame - 1].SetActive(true);
    }

    IEnumerator GameRotation()
    {
        float elapsedTime = 0;
        float currentGround = groundMaterial.GetFloat("_bridge_activate");
        while (elapsedTime < 1)
        {
            groundMaterial.SetFloat("_bridge_activate", Mathf.Lerp(currentGround, 0, (elapsedTime / 1)));
            for (int i = 0; i < gates.Length; i++)
            {
                gates[i].SetFloat("_ditherfade", Mathf.Lerp(5, 0, (elapsedTime / 1)));
            }
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        groundMaterial.SetFloat("_bridge_activate", 0);

        for (int i = 0; i < cleared.Length; i++)
        {
            cleared[i].SetActive(true);
        }

        switch (currentGame)
        {
            case 0:
                groundMaterial.SetFloat("_Bridge1", 1);
                groundMaterial.SetFloat("_Bridge3", 0);
                break;
            case 1:
                groundMaterial.SetFloat("_Bridge1", 0);
                groundMaterial.SetFloat("_Bridge3", 0);
                break;
            case 2:
                groundMaterial.SetFloat("_Bridge1", 1);
                groundMaterial.SetFloat("_Bridge3", 1);
                break;
            default:
                break;
        }

        currentGame++;

        elapsedTime = 0;
        Quaternion endPos = ground.transform.localRotation;

        while (elapsedTime < rotationTime)
        {
            ground.transform.Rotate(0, -5, 0);
            wheel.transform.Rotate(15, 0, 0);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Quaternion currentPos = ground.transform.localRotation;

        elapsedTime = 0;
        while (elapsedTime < 1)
        {
            ground.transform.localRotation = Quaternion.Lerp(currentPos, endPos, (elapsedTime / 1));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(GroundShader());
    }

    IEnumerator OpenBridge()
    {
        float elapsedTime = 0;
        while (elapsedTime < 1.5f)
        {
            groundMaterial.SetFloat("_bridge_activate", Mathf.Lerp(0.9f, 0, (elapsedTime / 1.5f)));
            elapsedTime += Time.deltaTime;
            for (int i = 0; i < gates.Length - 1; i++)
            {
                gates[i].SetFloat("_ditherfade", Mathf.Lerp(0, 5, (elapsedTime / 1.5f)));
            }
            yield return new WaitForEndOfFrame();
        }

        elapsedTime = 0;
        while (elapsedTime < 3)
        {
            bridgeMaterial.SetFloat("_secret_bridgfe_open", Mathf.Lerp(100, 0, (elapsedTime / 3)));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(5f);
        elapsedTime = 0;
        while (elapsedTime < 1)
        {
            slotRenderer.SetBlendShapeWeight(0, Mathf.Lerp(0, 100, (elapsedTime / 1)));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        elapsedTime = 0;
        while (elapsedTime < 1)
        {
            slotRenderer.SetBlendShapeWeight(0, Mathf.Lerp(100, 0, (elapsedTime / 1)));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        elapsedTime = 0;
        while (elapsedTime < 2)
        {
            slotMaterial.SetFloat("_y_naar_0", Mathf.Lerp(0.35f, 0, (elapsedTime / 2)));
            slotMaterial.SetFloat("_draai_dat_ding", Mathf.Lerp(2, 0, (elapsedTime / 2)));
            slotMaterial.SetFloat("_y_naar_0v2", Mathf.Lerp(1.68f, 0, (elapsedTime / 2)));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
