using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HackingSystem : MonoBehaviour
{
    public Camera cam;
    public Canvas canvas;
    public GameObject hackingPos;
    public TMP_Text compTerminal;

    GameObject computer;

    CharacterControllerSub charContr;
    OrbitController orbitContr;

    public bool isHacking = false;

    float startTime = 0f;
    float waitTime = 0.5f;

    void Start()
    {
        charContr = gameObject.GetComponent<CharacterControllerSub>();
    }
    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawLine(ray.origin, cam.transform.forward * 10000, Color.red);
        RaycastHit hit;

        if (!isHacking && Physics.Raycast(ray, out hit) && hit.distance < 3f)
        {
            if (hit.transform.tag == "Computer")
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    startTime = Time.time;

                    isHacking = true;
                    canvas.enabled = false;
                    computer = hit.transform.gameObject;
                }
            }
        }

        if (isHacking)
        {
            gameObject.transform.SetPositionAndRotation(hackingPos.transform.position, hackingPos.transform.rotation);
            charContr.movementSpeed = 0;

            if (Time.time - startTime > waitTime && Input.GetKeyDown(KeyCode.F))
            {
                isHacking = false;
                charContr.movementSpeed = 6;
            }

            foreach (char c in Input.inputString)
            {
                print(c);

                if (c == '\b') // has backspace/delete been pressed?
                {
                    if (compTerminal.text.Length != 0)
                    {
                        compTerminal.text = compTerminal.text.Substring(0, compTerminal.text.Length - 1);
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    print("User entered their name: " + compTerminal.text);
                }
                else
                {
                    compTerminal.text += c;
                }
            }
        }
    }
}
