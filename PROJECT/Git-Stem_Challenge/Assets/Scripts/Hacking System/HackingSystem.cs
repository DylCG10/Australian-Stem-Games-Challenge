using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HackingSystem : MonoBehaviour
{
    HackingOutputs hackingOutputs;

    public Camera cam;
    public Canvas canvas;

    public GameObject hackingPos;
    GameObject computer;

    public TMP_Text compTerminal;

    CharacterControllerSub charContr;
    OrbitController orbitContr;

    public bool isHacking = false;

    float startTime = 0f;
    float waitTime = 0.5f;
    public int line = 0;


    void Start()
    {
        charContr = gameObject.GetComponent<CharacterControllerSub>();
        hackingOutputs = gameObject.GetComponent<HackingOutputs>();
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

            if (Time.time - startTime > waitTime)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    isHacking = false;
                    charContr.movementSpeed = 6;
                }


                //Coding Realtime
                foreach (char c in Input.inputString)
                {
                    string enteredText = compTerminal.text.Replace("Console: ", "").Split('\n')[line].ToLower();
                    string[] voids = hackingOutputs.voids;

                    if (c == '\b') // has backspace/delete been pressed?
                    {
                        if (enteredText.Length != 0)
                        {
                            compTerminal.text = compTerminal.text.Substring(0, compTerminal.text.Length - 1);
                        }
                    }
                    else if ((c == '\n') || (c == '\r')) // enter/return
                    {
                        print("Entered: " + enteredText);

                        for (int i = 0; i < voids.Length; i++)
                        {
                            if (enteredText == voids[i])
                            {
                                print("Valid Response!");
                                hackingOutputs.CodeEntries(enteredText, computer);
                            }

                        }

                        compTerminal.text += "\n" + "Console: ";
                        line += 1;
                    }
                    else
                    {
                        compTerminal.text += c;
                    }
                }
            }
        }
    }
}
