using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Inspection : MonoBehaviour
{
    CharacterControllerSub charContr;
    public CharacterMotor motor;
    OrbitController orbitContr;
    DepthOfField DOF;

    public bool grab = false;

    public Transform inspectionPos;
    public Camera cam;
    public Canvas canvas;
    GameObject item;


    float startTime = 0;
    float waitTime = 0.5f;
    public float force = 10f;

    public float playerImpactingForce = 4f;
    public float rotSpeed = 20f;


    Vector3 originalPos;
    Quaternion originalRot;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        charContr = gameObject.GetComponent<CharacterControllerSub>();
        orbitContr = gameObject.GetComponentInChildren<OrbitController>();
        DOF = gameObject.GetComponentInChildren<DepthOfField>();
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawLine(ray.origin, cam.transform.forward * 10000, Color.red);
        RaycastHit hit;

        if (!grab && Physics.Raycast(ray, out hit) && hit.distance < 3f)
        {
            if (hit.transform.tag == "Item" || hit.transform.tag == "Item 2" && hit.distance < 3f)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    grab = true;
                    item = hit.transform.gameObject;
                    startTime = Time.time;
                    originalPos = item.transform.position;
                    originalRot = item.transform.rotation;
                    canvas.enabled = false;
                }
            }
        }

        //"Item 2" = item you can't move around with (e.g. paper, pens)
        //"Item" = item you can move around with (e.g. rocks)
        if (grab)
        {
            item.GetComponent<Rigidbody>().useGravity = false;
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Rigidbody>().detectCollisions = false;

            item.transform.SetParent(inspectionPos);
            item.transform.localPosition = new Vector3(0, 0, 0);

            if (item.transform.tag == "Item 2")
            {
                charContr.movementSpeed = 0;
                orbitContr.enabled = false;

                cam.fieldOfView = 40;
                DOF.focalTransform = item.transform;
                DOF.aperture = 0.6f;
            }

            if (Time.time - startTime > waitTime && Input.GetKeyDown(KeyCode.F))
            {
               
                item.GetComponent<Rigidbody>().detectCollisions = true;
                item.GetComponent<Rigidbody>().isKinematic = false;
                item.GetComponent<Rigidbody>().useGravity = true;

                item.transform.parent = null;
                grab = false;

                if (item.transform.tag == "Item 2")
                {
                    item.transform.rotation = originalRot;
                    item.transform.position = originalPos; //snap item back to original position
                    charContr.movementSpeed = 6;
                    orbitContr.enabled = true;
                    cam.fieldOfView = 70;
                    DOF.focalTransform = null;
                    DOF.aperture = 0f;

                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                        
                }

                if (item.transform.tag == "Item")
                {
                    item.GetComponent<Rigidbody>().AddForce(cam.transform.forward * force + motor.rBody.velocity*playerImpactingForce);

                }

                canvas.enabled = true;
            }

            if (Input.GetMouseButton(0) && item.transform.tag == "Item 2")
            {
                float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
                float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

                item.transform.Rotate(cam.transform.up, rotX, Space.World);
                item.transform.Rotate(cam.transform.right, rotY, Space.World);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

}
