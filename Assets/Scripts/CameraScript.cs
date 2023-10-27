using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    public float mouseSens= 0.69f;
    //public float xRotation=0f;
    private float _rotationVelocity;
	public GameObject CinemachineCameraTarget;
    private float _cinemachineTargetPitch;
    private StarterAssetsInputs _input;
    private GameObject _mainCamera;

    [Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 90.0f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -90.0f;

    private const float _threshold = 0.001f;
    private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

    // Start is called before the first frame update
    void Start()
    {
       // Cursor.lockState = CursorLockMode.Locked;
         _input = GetComponent<StarterAssetsInputs>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
 
      if (_input.look.sqrMagnitude >= _threshold)
      {
        //Don't multiply mouse input by Time.deltaTime
        float deltaTimeMultiplier = Time.deltaTime;
        float inputX = _input.look.x;
        float inputY = _input.look.y;
        _cinemachineTargetPitch += inputY * mouseSens * deltaTimeMultiplier;
        _rotationVelocity = inputX * mouseSens * deltaTimeMultiplier;

        // clamp our pitch rotation
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Update Cinemachine camera target pitch
        CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

        // rotate the player left and right
        transform.Rotate(Vector3.up * _rotationVelocity);

      }
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
            Debug.Log("iN CLAMP ");
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}
}
