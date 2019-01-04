using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitController : MonoBehaviour {

    [Header("General Options")]
    public Transform playerTarget;
    public Camera cam;
    public float distanceFromTarget;
    public Vector3 offsetOrbitPoint;

    [Header("Speeds")]
    public float followLag;
    public float rotationSpeed;
    public float cameraSpeed;

    [Header("Aiming Vars")]
    public Vector3 playerOffset;
    public float distanceFromOffset;
    public Transform rotator;

    [Header("Senss")]
    public float clampMax;

    public Vector2 sensitivity;
    public Vector2 pitchMinMax;

    [Header("While Moving")]
    public float fovAdd = 5;
    public float distanceAdd = 1;

    #region Privates
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;
    #endregion


    private float originalFOV;
    private float originalDist;
    private void Awake()
    {
        originalFOV = cam.fieldOfView;
        originalDist = distanceFromTarget;
    }

    private void OnDrawGizmos()
    {
       // Vector3 r = playerTarget.TransformPoint(playerOffset);
        Gizmos.color = Color.red;
       // Gizmos.DrawWireSphere(r, 0.1f);
    }

    private void ThirdPersonAiming()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, originalFOV + fovAdd, 0.1f);
        transform.position = Vector3.Lerp(transform.position, (playerTarget.TransformPoint(playerOffset) - transform.forward * distanceFromOffset),1);
        yaw += Input.GetAxis("Mouse X") * sensitivity.x;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity.y;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        currentRotation = Vector3.Lerp(currentRotation, new Vector3(pitch, yaw), rotationSpeed * Time.deltaTime);

        transform.eulerAngles = currentRotation;
        Vector3 e = transform.eulerAngles;
        e.x = 0;
       // transform.position = Vector3.Lerp(transform.position, (playerTarget.TransformPoint(playerOffset) - transform.forward * distanceFromOffset), cameraSpeed/4);

        //       rotator.eulerAngles = new Vector3(g.x, rotator.eulerAngles.y, rotator.eulerAngles.z);

        playerTarget.eulerAngles = e;
    }

    public bool thirdPersonAimingEnabled = false;
    public bool cameraIsFPS;
    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1) && thirdPersonAimingEnabled)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        if (Input.GetMouseButton(1)&& thirdPersonAimingEnabled)
        {
            ThirdPersonAiming();
        }
        else
        {
            if (!cameraIsFPS)
            transform.position = Vector3.Lerp(transform.position, (playerTarget.position + offsetOrbitPoint) - transform.forward * distanceFromTarget, cameraSpeed);
           
            yaw += Input.GetAxis("Mouse X") * sensitivity.x;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity.y;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

            currentRotation = Vector3.Lerp(currentRotation, new Vector3(pitch, yaw), rotationSpeed * Time.deltaTime);

            //TODOD : TOOO MUCH VELOCITY

            transform.eulerAngles = currentRotation;
            
             if (!cameraIsFPS)
            transform.position = Vector3.Lerp(transform.position, (playerTarget.position + offsetOrbitPoint) - transform.forward * distanceFromTarget, cameraSpeed);
            
        }
    }

  

    public void ApplyingVelocity(Vector3 velocity, float time)
    {

        if (velocity.magnitude > 0.1f || velocity.magnitude < -0.1f)
        {
            if (time > 0.2f)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, originalFOV + fovAdd, 0.1f);
                distanceFromTarget = Mathf.Lerp(distanceFromTarget, originalDist + distanceAdd, 0.1f);
            }
           
        }
        else
        {
            if (!Input.GetMouseButton(1))
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, originalFOV, 0.1f);
            distanceFromTarget = Mathf.Lerp(distanceFromTarget, originalDist, 0.1f);
        }
    }
}
