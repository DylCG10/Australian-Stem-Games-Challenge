using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HackingOutputs : MonoBehaviour
{
    HackingSystem hackingSys;
    [HideInInspector] public string[] voids = { "open", "close", "help" };

    public Animator doorAnim;
    bool doorOpen = false;

    void Start()
    {
        hackingSys = GetComponent<HackingSystem>();

        //To avoid repetition when entering new commands
        for (int i = 0; i < voids.Length; i++)
        {
            voids[i] = "void" + " " + voids[i].ToString() + "()";

            Debug.Log(voids[i]);
        }
    }

    public void CodeEntries(string code, GameObject computer)
    {
        if (code == "void open()")
        {
            DoorOpen(computer);
        }

        else if (code == "void close()")
        {
            DoorClose(computer);
        }


        if (code == "void help()")
        {
            hackingSys.compTerminal.text += "\n" + "This is supposed to show some text to help you";
        }
    }

    public void DoorOpen(GameObject computer)
    {
        computer.GetComponent<Computer_Objects>().door.GetComponent<Animator>().SetBool("Open", true);
    }

    public void DoorClose(GameObject computer)
    {
        computer.GetComponent<Computer_Objects>().door.GetComponent<Animator>().SetBool("Open", false);

    }
}
