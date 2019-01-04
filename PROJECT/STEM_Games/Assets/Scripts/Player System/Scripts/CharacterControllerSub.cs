using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerSub : MonoBehaviour
{
    public CharacterMotor motor;

    [Header("Speeds")]
    public float movementSpeed;
    public float gravityPower;

    public bool isUnderControl = false;

    private void Update() {

        if (isUnderControl) {
            Vector3 gravity = motor.Gravity(gravityPower);
            Vector3 input = motor.GainAxisInput(movementSpeed, false);

            motor.Jump(true);
            motor.Jump(false);
            motor.AddVelocity(input+gravity,1);
            motor.FinalMovementCalculation(1);
            
            Debug.DrawRay(transform.position,transform.forward,Color.blue,0.05f);
        }

    }


}
