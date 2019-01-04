using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InputManagement : MonoBehaviour
{

    public Text distance;
    public Text stage;

    [Header("-- Main Vars --")]
    public bool inputRecording;
    public int frameCount;
    public Vector3 startPos,startRot;
    public CharacterMotor motor;
    public CharacterControllerSub controller;

    [Header("Placeholder ")]
    public Transform cameraHolder;

    public bool isRecording = false;
    public bool isPlaying = false;

    //Private Vars
    public List<MovementFrame> movementFrameList = new List<MovementFrame>();
    private List<MovementFrame> tempFrames = new List<MovementFrame>();

    public List<Quaternion> cameraRotations = new List<Quaternion>();
    public List<Vector3> cameraPositions = new List<Vector3>();

    private void Awake() {
        startPos = transform.position;
        startRot = transform.eulerAngles;
        isRecording = true;
    }

    private void FixedUpdate() {
        
        if (isRecording || isPlaying)
        frameCount += 1;
        
        if (isRecording) {
            
            //cameraRotations.Add(cameraHolder.rotation);
            //cameraPositions.Add(cameraHolder.position);
            PlayerInputPush();
            FinalPlayerMovement();
        
        }
        
    
    }
    
    int cameraSelectionIndex = 0;
    private void GetCameraRotation() {
        if (cameraSelectionIndex < cameraRotations.Count) {
            cameraHolder.rotation = cameraRotations[cameraSelectionIndex];
            //cameraHolder.position = cameraPositions[cameraSelectionIndex];
            cameraSelectionIndex++;
        }
    }
    
    int lastIndex = 0;
    private void GetRecordedPlayerInput() {
        List<MovementFrame> frames = new List<MovementFrame>();
        for (int i = lastIndex; i < movementFrameList.Count; i++) {
            if (movementFrameList[i].frameCount <=  frameCount) {
                frames.Add(movementFrameList[i]);
                movementFrameList.RemoveAt(i);
            }

            else {
                break;
            }
        }

        for (int i = 0; i < frames.Count; i++) {
            
            transform.position = frames[i].safetyPosition;
            transform.eulerAngles = frames[i].safetyRotation;
            motor.rBody.velocity = frames[i].safetyVelocity;
            //TODO: Add new letters here (This is to register the keys on playback)
            if (frames[i].down) {
                if (frames[i].inputKey == KeyCode.W) {W=true;}
                if (frames[i].inputKey == KeyCode.A) {A=true;}
                if (frames[i].inputKey == KeyCode.S) {S=true;}
                if (frames[i].inputKey == KeyCode.D) {D=true;}
                if (frames[i].inputKey == KeyCode.Space) {J=true;}
            }
            if (!frames[i].down) {
                if (frames[i].inputKey == KeyCode.W) {W=false;}
                if (frames[i].inputKey == KeyCode.A) {A=false;}
                if (frames[i].inputKey == KeyCode.S) {S=false;}
                if (frames[i].inputKey == KeyCode.D) {D=false;}
                //TODO: Add new letters here (This is to register the keys on playback)
                if (frames[i].inputKey == KeyCode.Space) {J=false;}
            }
            
            

        }
        
        SimulateMovementInput();
        
        for (int i = 0; i < frames.Count; i++) {
             transform.position = frames[i].safetyPosition;
            transform.eulerAngles = frames[i].safetyRotation;
             motor.rBody.velocity = frames[i].safetyVelocity;
        }

    }

    Vector3 actPos;
    private void Update() {
        if (isRecording) {
        //TODO: Add new letters here
        //Input Queueing
        int w = CheckInput(KeyCode.W,KeyCode.UpArrow,W);
        if (w == 1) {W = true;}else if (w == -1) {W = false;}

        int a = CheckInput(KeyCode.A,KeyCode.LeftArrow,A);
        if (a == 1) {A = true;}else if (a == -1) {A = false;}

        int s = CheckInput(KeyCode.S,KeyCode.DownArrow,S);
        if (s == 1) {S = true;}else if (s == -1) {S = false;}

        int d = CheckInput(KeyCode.D, KeyCode.RightArrow,D);
        if (d == 1) {D = true;}else if (d == -1) {D = false;}

        int j = CheckInput(KeyCode.Space, KeyCode.Space,J);
        if (j == 1) {J = true;}else if (j == -1) {J = false;}
        

        }
    }

    //TODO: Add new letters here
    public bool W,A,S,D,J;
    public bool RW,RA,RS,RD;
    private int CheckInput(KeyCode a, KeyCode b, bool currentState) {

        /*if (Input.GetKeyDown(a) || Input.GetKeyDown(b)) 
        {
            if (currentState == false) {
                MovementFrame movementFrame = new MovementFrame();
                movementFrame.inputKey = a;
                movementFrame.down = true;
                
                tempFrames.Add(movementFrame);
                return 1;
            }
        }*/

        if (Input.GetKey(a)) 
        {
            if (currentState == false) {
                MovementFrame movementFrame = new MovementFrame();
                movementFrame.inputKey = a;
                movementFrame.down = true;
                
                tempFrames.Add(movementFrame);
                return 1;
            }
        }
        else {

            if (currentState == true) {
                MovementFrame movementFrame = new MovementFrame();
                movementFrame.inputKey = a;
                movementFrame.down = false;
                tempFrames.Add(movementFrame);
                return -1;
            }

        }

        if (Input.GetKeyUp(a) || Input.GetKeyUp(b)) {

            MovementFrame movementFrame = new MovementFrame();
            movementFrame.inputKey = a;
            movementFrame.down = false;
            
            tempFrames.Add(movementFrame);
            return -1;
        }

        

        return 0;
    }

    private void PlayerInputPush() {
        for (int i = 0; i < tempFrames.Count; i++) {    
            tempFrames[i].frameCount = frameCount;
            tempFrames[i].safetyPosition = transform.position;
            tempFrames[i].safetyRotation = transform.eulerAngles;
            tempFrames[i].safetyVelocity = motor.rBody.velocity;
        }

        SimulateMovementInput();
        tempFrames.Clear();
    }

    private Vector3 movementVect;
    private Vector3 gravity;

    private void SimulateMovementInput () {
        //TODO: Add new letters here
        float horizontal = 0f;
        float vertical = 0f;

        if (W) {vertical += 1;} 
        if (A) {horizontal -= 1;}
        if (S) {vertical -= 1;}
        if (D) {horizontal += 1;}

        gravity = motor.Gravity(controller.gravityPower);

        if(isRecording)    
        movementVect = motor.GainAxisInput(horizontal,vertical,controller.movementSpeed,cameraHolder.rotation, false);
        else if (isPlaying) {
            movementVect = motor.GainAxisInput(horizontal,vertical,controller.movementSpeed,cameraHolder.rotation, false);
        } 

    }

    private void FinalPlayerMovement() {

        if (J) {
            motor.JumpR(true);
        }

        motor.JumpR(false);

        motor.AddVelocity(movementVect + gravity,1);
        motor.FinalMovementCalculation(1);
        
        Debug.DrawRay(transform.position,transform.forward,Color.blue,0.05f);
    }

    //Add reliability updates on position later


}


[System.Serializable]
public class Frame {

    public int frameCount;
    public Vector3 safetyPosition;
    public Vector3 safetyRotation;
    public Vector3 safetyVelocity;


}
[System.Serializable]
public class MovementFrame : Frame {
    
    public KeyCode inputKey;
    public bool down;

}
[System.Serializable]
public class TransformFrame : Frame {

    public Vector3 position;
    public Vector3 rotation;

}
[System.Serializable]
public class MouseFrame : Frame {

    public Vector3 mousePosition;
    public bool down;

}