using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterMotor : MonoBehaviour
{
    #region Variables
    public CharacterController charController;
    public Transform cameraTransform;

    [Header("Grounding")]
    public float groundingDistance;
    public float groundingRadius;

    [Header("Data Collection")]
    public bool visualise;

    [Header("Bounds Settings")]
    [Range(0f,1f)][HideInInspector]
    public float boundRange;
    [Range(0f,10f)][HideInInspector]
    public float collisionSpread,bendIn;
    [Range(0,2f)][HideInInspector]
    public float collisionalDistance = 1.2f;
    [HideInInspector]
    public float minimumCollisionDistance = 1f;

    [Header("Step Heights")]
    public float stepHeight;
    public float forwardStep;
    public float requiredMax;

    [Header("Future Settings & Side")]
    [Range(0f,2)]
    public float futureSpread;
    public float sideSpread;
    public LayerMask discludePlayer;

    [Header("Physical Data")]
    public CapsuleCollider collider;
    public Rigidbody rBody;

    [Header("Movement Data")]
    public float maxAngleSlope;
    public Camera playerCamera;

    [Header("Custom Variables")]
    //If you need any extra control
    public List<float> floats = new List<float>();

    #endregion
   
    #region  Visualisation 

    private void OnDrawGizmos() {
        if (visualise) {
           Gizmos.DrawWireSphere(groundHit.point,groundingRadius);
            
        }
    }

    #endregion

    #region  Updates

    #endregion

    #region Physics Simulations

    RaycastHit groundHit;
    public bool grounded;
    public Vector3 Gravity(float Gravity) {
        
        Ray ray = new Ray(rBody.position,-transform.up);
        RaycastHit hit;

        if (Physics.SphereCast(ray, groundingRadius, out hit)) {
            groundHit = hit;
            if (hit.distance < groundingDistance) {
                
                //This is the auto clamping down feature (Should only be activated on state)
                if (Vector3.Angle(Vector3.up, hit.normal) < maxAngleSlope) 
                {
                    grounded = true;

                    //FIXME: Need to be managed by state machine
                    if (!climbingStairs && !isjumping) {
                        //FUTURE: This is a Hard Coded Lerp value, it manages how quickly slope directions can change
                         transform.position = Vector3.Lerp(rBody.position,new Vector3(rBody.position.x, hit.point.y+1.05f, rBody.position.z),.2f);
                    }
                }
                else {
                    //TODO: This is all a bit redundant
                    print("Max Slope Detection on Grounding");
                    grounded = false;
                }
                   
                 return Vector3.zero;
            }
            else {
                grounded = false;
            }
        }
        else {
            grounded = false;
        }

        grounded = false;

        if (!isjumping)
            return -transform.up*Gravity;

        return Vector3.zero;
       

    }

    #endregion

    #region  Movement Calculations

    public Vector3 currentVelocity = Vector3.zero;
    private Vector3 lastVelocity = Vector3.zero;

    public Vector3 GainAxisInput(float multiplier, bool turn) {
       Vector3 movementVelocity = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
       
       movementVelocity *= multiplier;
       movementVelocity = cameraTransform.TransformDirection(movementVelocity);
       movementVelocity.y = 0;

        if (turn)
        transform.LookAt(transform.position+movementVelocity);

       movementVelocity = SlopeCheck(maxAngleSlope,movementVelocity);
       StepOverTest(movementVelocity);
       
       Debug.DrawLine(transform.position,transform.position+movementVelocity,Color.magenta,0.1f);

       

       return movementVelocity;
       
    }
    

    private bool climbingStairs = false;


    public void AddVelocity (Vector3 direction, float multiplier) {
         currentVelocity += direction*multiplier;
    }

    public void FinalMovementCalculation(float globalSpeed) {

        rBody.velocity = currentVelocity*globalSpeed;

        lastVelocity = currentVelocity;
        currentVelocity = Vector3.zero;

         

    }

    public void AvoidPerpetualVelocity() {
        rBody.velocity = Vector3.zero;
    }

    #endregion

    #region  Custom Cases (Stairs, Slipping etc..)

    //Step Management
    public void StepOverTest (Vector3 dir) {
        Ray ray = new Ray(transform.position+transform.up*stepHeight+dir.normalized*forwardStep,-transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, discludePlayer)) {
            Debug.DrawLine(ray.origin, hit.point,Color.black);
            
            float ang = Vector3.Angle(Vector3.up, hit.normal);

            //FIXME: Need to be managed by state machine
            if (hit.distance < requiredMax && !isjumping && ang < maxAngleSlope) {
                //FUTURE: This is a Hard Coded Lerp value, it manages how quickly slope directions can change
                transform.position = Vector3.Lerp(transform.position,new Vector3(transform.position.x,hit.point.y+1.05f,transform.position.z),.3f);
                climbingStairs = true;
            }
            climbingStairs = false;   
        }
        else {
            climbingStairs = false;
        }
    }

    #endregion

    #region  Slope Management

    private Vector3 previousDir;Vector3 dif;
    private Vector3 movementDir;
    private Vector3 previousSlopeVector3;
    private Vector3 SlopeCheck (float maxSlope, Vector3 forwVelocity) {
       
        Ray ray = new Ray(transform.position+transform.up+forwVelocity.normalized*futureSpread,-transform.up);
        Ray ray2 = new Ray(transform.position+transform.up+forwVelocity.normalized*(futureSpread+0.005f),-transform.up);
        RaycastHit hit; RaycastHit hit2;

        float slope = 0f;
        float slope2 = 0f;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, discludePlayer)) {
            slope2 = Vector3.Angle(Vector3.up,hit.normal);
        }if (Physics.Raycast(ray2, out hit2, Mathf.Infinity, discludePlayer)) {
            slope = Vector3.Angle(Vector3.up,hit2.normal);
        }
        
        Debug.DrawLine(ray.origin,ray.origin+ray.direction*10,Color.red);

        Vector3 h2 = hit2.point;

        //FIXME: Need to be managed by state machine
        if (slope > maxSlope || slope2 > maxSlope) {
            return forwVelocity;
        }
    
       dif = h2-hit.point;
       Vector3 l = dif.normalized*forwVelocity.magnitude;

       movementDir = l;
        
       //FIXME: Need to be managed by state machine
       if (!isjumping) {
           //FUTURE: This is a Hard Coded Lerp value, it manages how quickly slope directions can change
           l = Vector3.Lerp(previousSlopeVector3,l,0.3f);
           previousSlopeVector3 = l;
            return l;
       }

       return forwVelocity;
    }

    #endregion


    #region  Jumping
    [Header("Jumping")]
    public float jumpForce;
    public float jumpDecrease;
    public float groundDeactivate;

    public float incrementJumpFallSpeed;


    private string jumpPhase = "None";
    private float jumpHeight;
    private float fallMultiplier = -1f;
    private float startY;
    private bool activado = false;
    private bool isjumping;

    public void Jump(bool isUpdate)
    {
        //TODO: You need to fix this so that it rays up and checks for a roof that is too low
        if (isUpdate)
        {
            //Make sure not slipping
            if (Input.GetKeyDown(KeyCode.Space) && (grounded))
            {

                
                isjumping = true;
                jumpHeight += jumpForce;
                startY = transform.position.y;
                return;
            }
        }
        else
        {
            if (isjumping)
            {
                jumpHeight -= (jumpHeight * jumpDecrease * Time.deltaTime) + fallMultiplier * Time.deltaTime;
                fallMultiplier += incrementJumpFallSpeed;

                currentVelocity.y = jumpHeight;


                if (Mathf.Abs(transform.position.y - startY) > 0.6f)
                activado = true;
                //if (activado == false && !grounded)
                    

                if (activado && (grounded))
                {
                    jumpHeight = 0;
                    fallMultiplier = -1;
                    startY = transform.position.y;
                    isjumping = false;
                    activado = false;
                }



            }

        }



    }

     public void JumpR(bool isUpdate)
    {
        //TODO: You need to fix this so that it rays up and checks for a roof that is too low
        if (isUpdate)
        {
            //Make sure not slipping
            if ((grounded))
            {
                isjumping = true;
                jumpHeight += jumpForce;
                startY = transform.position.y;
                return;
            }
        }
        else
        {
            if (isjumping)
            {
                jumpHeight -= (jumpHeight * jumpDecrease * Time.deltaTime) + fallMultiplier * Time.deltaTime;
                fallMultiplier += incrementJumpFallSpeed;

                currentVelocity.y = jumpHeight;


                if (Mathf.Abs(transform.position.y - startY) > 0.6f)
                activado = true;
                //if (activado == false && !grounded)
                    

                if (activado && (grounded))
                {
                    jumpHeight = 0;
                    fallMultiplier = -1;
                    startY = transform.position.y;
                    isjumping = false;
                    activado = false;
                }



            }

        }



    }

    #endregion

    #region  Input Modified Events

    public Vector3 GainAxisInput(float horizontal, float vertical, float multiplier, Quaternion placeholder, bool turn) {
       Vector3 movementVelocity = new Vector3(horizontal,0,vertical);
       
        Vector3 e = placeholder.eulerAngles;
        e.x*= 0;
        placeholder.eulerAngles = e;;

       movementVelocity *= multiplier;
       movementVelocity = placeholder*movementVelocity;
       movementVelocity.y = 0;

        if (turn)
       transform.LookAt(transform.position+movementVelocity);

       movementVelocity = SlopeCheck(maxAngleSlope,movementVelocity);
       StepOverTest(movementVelocity);
       
       Debug.DrawLine(transform.position,transform.position+movementVelocity,Color.magenta,0.1f);

       

       return movementVelocity;
       
    }

    

    public Vector3 GainAxisInput(float horizontal, float vertical, float multiplier) {
       Vector3 movementVelocity = new Vector3(horizontal,0,vertical);
       
       movementVelocity *= multiplier;
       movementVelocity = transform.TransformDirection(movementVelocity);
       movementVelocity.y = 0;

       //transform.LookAt(transform.position+movementVelocity);

       movementVelocity = SlopeCheck(maxAngleSlope,movementVelocity);
       StepOverTest(movementVelocity);
       
       Debug.DrawLine(transform.position,transform.position+movementVelocity,Color.magenta,0.1f);

       

       return movementVelocity;
       
    }

    #endregion

}


//FIXME: Redundant Code : 

/* 

 #region  Ray-Data Collection

    private void FutureUpdate(ref FutureCast future) {
        Ray ray = new Ray(transform.position+transform.forward*future.expansionFactor,-transform.up);
        
        
        Physics.Raycast(ray, out future.forward, Mathf.Infinity,discludePlayer);

        if (visualise) {
            Debug.DrawLine(transform.position,ray.origin);
            Debug.DrawLine(ray.origin,future.forward.point);
        }
        

        ray = new Ray(transform.position-transform.forward*future.expansionFactor,-transform.up);
       
        Physics.Raycast(ray, out future.backward, Mathf.Infinity,discludePlayer);

        if (visualise) {
            Debug.DrawLine(transform.position,ray.origin);
            Debug.DrawLine(ray.origin,future.backward.point);
        }

        ray = new Ray(transform.position-transform.right*future.expansionFactor,-transform.up);
       
        Physics.Raycast(ray, out future.left, Mathf.Infinity,discludePlayer);

        if (visualise) {
            Debug.DrawLine(transform.position,ray.origin);
            Debug.DrawLine(ray.origin,future.left.point);
        }

        ray = new Ray(transform.position+transform.right*future.expansionFactor,-transform.up);
        
        Physics.Raycast(ray, out future.right, Mathf.Infinity,discludePlayer);

        if (visualise) {
            Debug.DrawLine(transform.position,ray.origin);
            Debug.DrawLine(ray.origin,future.right.point);
        }
    }

    private void BoundsUpdate(ref BoundsCast boundsCast, bool openUpAndDown,float bendFactor) {
        
        Ray ray = new Ray(transform.position+transform.up*boundsCast.heightPercentage+transform.forward*collider.radius*bendFactor,transform.forward);
        Physics.Raycast(ray, out boundsCast.forward,collisionSpread,discludePlayer);
        
        if (visualise) {
            if (boundsCast.forward.point != Vector3.zero)
            Debug.DrawLine(ray.origin,boundsCast.forward.point,Color.cyan);
        }

        ray = new Ray(transform.position+transform.up*boundsCast.heightPercentage-transform.forward*collider.radius*bendFactor,-transform.forward);
        Physics.Raycast(ray, out boundsCast.backward,collisionSpread,discludePlayer);

        if (visualise) {
            if (boundsCast.backward.point != Vector3.zero)
            Debug.DrawLine(ray.origin,boundsCast.backward.point,Color.cyan);
        }

        ray = new Ray(transform.position+transform.up*boundsCast.heightPercentage+transform.right*collider.radius*bendFactor,transform.right);
        Physics.Raycast(ray, out boundsCast.right,collisionSpread,discludePlayer);

        if (visualise) {
            if (boundsCast.right.point != Vector3.zero)
            Debug.DrawLine(ray.origin,boundsCast.right.point,Color.cyan);
        }

        ray = new Ray(transform.position+transform.up*boundsCast.heightPercentage-transform.right*collider.radius*bendFactor,-transform.right);
        Physics.Raycast(ray, out boundsCast.left,collisionSpread,discludePlayer);

        if (visualise) {
            if (boundsCast.left.point != Vector3.zero)
            Debug.DrawLine(ray.origin,boundsCast.left.point,Color.cyan);
        }
        ray = new Ray(transform.position,transform.up);
        Physics.Raycast(ray, out boundsCast.up,Mathf.Infinity,discludePlayer);

        if (visualise) {
            if (boundsCast.up.point != Vector3.zero)
            Debug.DrawLine(ray.origin,boundsCast.up.point,Color.cyan);
        }

        ray = new Ray(transform.position,-transform.up);
        Physics.Raycast(ray, out boundsCast.down,Mathf.Infinity,discludePlayer);

        if (visualise) {
            if (boundsCast.down.point != Vector3.zero)
            Debug.DrawLine(ray.origin,boundsCast.down.point,Color.cyan);
        }

    }

    #endregion


 */
public struct FutureCast {

    public float expansionFactor;

    public RaycastHit forward;
    public RaycastHit backward;
    public RaycastHit left;
    public RaycastHit right;

}

public struct BoundsCast {

    public float heightPercentage;

    public RaycastHit up;
    public RaycastHit down;
    public RaycastHit right;
    public RaycastHit forward;
    public RaycastHit backward;
    public RaycastHit left;
}

public struct FutureThree {
    public RaycastHit top;
    public RaycastHit center;
    public RaycastHit bottom;
}