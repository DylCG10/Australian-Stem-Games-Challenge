using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeClimbingMotor : MonoBehaviour
{

    public CharacterMotor motor;
    public CharacterControllerSub sub;
    public float pushAway;
    private bool latched = false;

    private RaycastHit surface;

    private void FixedUpdate() {
        Latching();

        if (latched) {
            Ray ray = new Ray(transform.position+(transform.up*Input.GetAxis("Vertical")+transform.right*Input.GetAxis("Horizontal")).normalized, transform.forward);
            Debug.DrawRay(ray.origin,ray.direction,Color.red,2);

            if (Physics.Raycast(ray ,out surface, 5f, motor.discludePlayer)) {
                
                Vector3 norm = surface.normal;


                Vector3 c = Quaternion.AngleAxis(90, Vector3.right) * norm;
                Vector3 r = Quaternion.AngleAxis(-90, Vector3.up) * norm;
                Debug.DrawRay(surface.point,c,Color.blue);

                
                
                transform.up = Vector3.Lerp(transform.up,c,.5f);
                motor.AddVelocity(transform.up*Input.GetAxis("Vertical")+transform.right*Input.GetAxis("Horizontal"),1.7f);
                motor.FinalMovementCalculation(1f);
                

              

            }   

        }


    }

    private void Latching() {
         //Latch Checking ++ Unlatch Checking
        if (Input.GetAxis("Vertical") > 0.3f && latched == false) {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray ,out hit, 1.05f, motor.discludePlayer)) {

                if (Vector3.Angle(transform.up, hit.normal) > motor.maxAngleSlope) {
                    transform.position = hit.point+hit.normal*pushAway;
                    print("CLIMB");
                    latched = true;
                    motor.rBody.velocity = Vector3.zero;
                    sub.isUnderControl = false;
                }

            }
        }
    }


}
