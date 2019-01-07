using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inspection : MonoBehaviour
{
    CharacterControllerSub charContr;

    public bool grab = false;

    public Transform inspectionPos;
    public CharacterMotor motor;

    public GameObject item;
    public Camera cam;

    float startTime = 0;
    float waitTime = 0.5f;
    public float force = 10f;
    public float playerImpactingForce = 4f;

    public Vector3 originalPos = new Vector3(0, 0, 0);


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        charContr = gameObject.GetComponent<CharacterControllerSub>();
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawLine(ray.origin, cam.transform.forward * 10000, Color.red);
        RaycastHit hit;

        if (!grab && Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag == "Item" || hit.transform.tag == "Item 2" && hit.distance < 3f)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    grab = true;
                    item = hit.transform.gameObject;
                    startTime = Time.time;
                    originalPos = item.transform.position;
                }
            }
        }

        //"Item 2" = item you can't move around with (e.g. paper, pens)
        //"Item" = item you can move around with (e.g. rocks)
        if (grab)
        {
            item.GetComponent<Rigidbody>().useGravity = false;
            item.GetComponent<Rigidbody>().isKinematic = true;

            item.transform.SetParent(inspectionPos);
            item.transform.localPosition = new Vector3(0, 0, 0);
            item.GetComponent<Rigidbody>().detectCollisions = false;

            if (item.transform.tag == "Item 2")
            {
                charContr.movementSpeed = 0;
            }

            if (Time.time - startTime > waitTime)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    item.GetComponent<Rigidbody>().detectCollisions = true;
                    item.transform.parent = null;
                    grab = false;

                    item.GetComponent<Rigidbody>().isKinematic = false;
                    item.GetComponent<Rigidbody>().useGravity = true;

                    if (item.transform.tag == "Item 2")
                    {
                        item.transform.position = originalPos; //snap item back to original position
                        charContr.movementSpeed = 6;
                        
                    }
                    if (item.transform.tag == "Item")
                    {
                        item.GetComponent<Rigidbody>().AddForce(cam.transform.forward * force + motor.rBody.velocity*playerImpactingForce);

                    }
                }
            }
        }
    }
}
