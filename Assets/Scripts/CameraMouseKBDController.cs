using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class CameraMouseKBDController : MonoBehaviour
{
    // Start is called before the first frame update

    public bool mouseKBDenabled = true;
    public float fwdSpeed = 1.0f;
        
    float verticalLookRotation;
    Transform cameraT;
        
    public float mouseSensitivityX = 1.0f;
    public float mouseSensitivityY = 1.0f;

    public float strafeSpeed = 1.0f;
    public float forwardSpeed = 1.0f;
    public float vertSpeed = 1.0f;

    void Start()
    {
    }

        
    
    // Update is called once per frame
    void Update()
    {
        if (mouseKBDenabled) {
            
            float yRotationLimit = 88f;
            if (Mouse.current.rightButton.isPressed) {

                float inputRotationx = mouseSensitivityX * Mouse.current.delta.x.ReadValue();
                float inputRotationy = mouseSensitivityX * Mouse.current.delta.y.ReadValue();                
                inputRotationy = Mathf.Clamp(inputRotationy, -yRotationLimit, yRotationLimit);
                Vector3 eV = new Vector3(- inputRotationy, inputRotationx, 0.0f);
                transform.eulerAngles += eV;
                
            }
            

            
            if (Keyboard.current.wKey.isPressed) {
                transform.position += Vector3.Normalize(transform.forward) * forwardSpeed * Time.deltaTime;
            }
            if (Keyboard.current.sKey.isPressed) {
                transform.position -= Vector3.Normalize(transform.forward) * forwardSpeed * Time.deltaTime;;
            }

            if (Keyboard.current.aKey.isPressed) {            
                transform.position -= Vector3.Normalize(transform.right) * strafeSpeed * Time.deltaTime;;
            }

            if (Keyboard.current.dKey.isPressed) {            
                transform.position += Vector3.Normalize(transform.right) * strafeSpeed * Time.deltaTime;;
            }

            if (Keyboard.current.qKey.isPressed) {
                transform.position += Vector3.Normalize(transform.up) * vertSpeed * Time.deltaTime;;

            }
            if (Keyboard.current.zKey.isPressed) {
                transform.position -= Vector3.Normalize(transform.up) * vertSpeed * Time.deltaTime;;
            }        
        }
    }
}

