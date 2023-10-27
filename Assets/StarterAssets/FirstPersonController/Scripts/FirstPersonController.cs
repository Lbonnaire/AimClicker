using System.Security.Cryptography.X509Certificates;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 0.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;
		AimGame AimTask	;
		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;
    	private bool clicked;
		private bool taskStarted;
	
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		public StarterAssetsInputs _input;
		private GameObject _mainCamera;

		//private const float _threshold = 0.01f;
		public float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			originalRotation = transform.localRotation;
			mouseXStartPos = Mouse.current.position.x.value;
			AimTask = GameObject.Find("Game").gameObject.transform.GetComponent<AimGame>();
			clicked = false;
        	taskStarted = false;
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			if (Mouse.current.rightButton.value == 1) {
				mouseXStartPos= Mouse.current.position.x.value;
				this.distanceInPixelsTraveled = 0f;
				this.rotationX = 0f;
				this.stop = false;
			}

			/*if (!this.stop) { //cm/360 calc
			//Debug.Log("not stopped");
				MouseTestUpdate();
				MouseLookUpdate();
			}else{
				//Debug.Log("stopped");
			}*/

			JumpAndGravity();
			GroundedCheck();
			Move();
		}
 		// Update is called once per frame
		void FixedUpdate()
		{

			if (!clicked){
				if(_input.click){
				    clicked =true;
				    OnMouseClick();  
				}
			}

			if (clicked){
			
				if(!_input.click){
				    clicked = false;
				}
			}
		}
		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);




			}
		}


		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}
		void OnMouseClick(){
			if(!taskStarted){
				AimTask.StartTask();
				taskStarted=true;
			}else{
				//Debug.Log("clicked");
				int layerMask = 1<<7;
				//layerMask =~layerMask;
				GameObject AimOrigin =CinemachineCameraTarget;
				RaycastHit hit;

				if (Physics.Raycast(AimOrigin.transform.position,AimOrigin.transform.TransformDirection(Vector3.forward),out hit, Mathf.Infinity, layerMask))
				{
						
					Debug.DrawRay(AimOrigin.transform.position, AimOrigin.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
					AimTask.OnHit(hit);
				}
				else
				{
					Debug.DrawRay(AimOrigin.transform.position, AimOrigin.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
					Debug.Log("Did not Hit");
					AimTask.OnMiss(hit);
				}
			}
		}



		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			lfAngle = lfAngle % 360; // cm/360 calc
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}


		private void OnDrawGizmosSelected()

		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}








		private float mouseXPos;
		public float mouseDPI = 800f;
		private Vector2 mousePos;
		private Vector3 lastMousePos;
		private float distanceInPixelsTraveled;
		private float distanceIncmTraveled;
		private float physicalDistanceIncmTraveled;
		private bool stop = false;
		public float sensitivityX = 1F;
		public float sensitivityY = 1F;
		public float minimumX = -360F;
		public float maximumX = 360F;
		public float minimumY = -60F;
		public float maximumY = 60F;
		float rotationX = 0F;
		float rotationY = 0F;
		Quaternion originalRotation;
		private float previousRotationY;
		private float previousRotationX;
		private float mouseXEndPos;
		private float mouseXStartPos;
		
		private void MouseTestUpdate() {
			// Mouse position on the screen (used for crosshair thing).
			mousePos = Mouse.current.position.value;
			
			// Get the mouse delta -- 0.05 = 1 pixel worth of movement, so multiply by 20.
			mouseXPos = Mouse.current.position.x.value;
			if (mouseXPos < 0f) {
				mouseXPos = 0f;
			}
			this.distanceInPixelsTraveled = Mathf.Abs(mouseXStartPos)+mouseXEndPos;

			// Figure out how many physical inches the mouse cursor has traveled.
			this.distanceIncmTraveled = this.distanceInPixelsTraveled*2.54f / Screen.dpi;
			// Figure out how many physical inches the mouse has traveled.
			//this.physicalDistanceInInchesTraveled = this.distanceInInchesTraveled / (800 / Screen.dpi);
			this.physicalDistanceIncmTraveled = this.distanceInPixelsTraveled*2.54f/ mouseDPI;

			// Reset...
			if (Mouse.current.rightButton.value == 1) {
				this.distanceInPixelsTraveled = 0f;
				this.rotationX = 0f;
				this.stop = false;
			}
		}
	
		private void MouseLookUpdate() {
			//Gets rotational input from the mouse
			//rotationY += Input.GetAxisRaw("Mouse Y") * sensitivityY;
			float mousemove = _input.look.x * sensitivityX;
			//rotationX += Input.GetAxisRaw("Mouse X") * sensitivityX;
			rotationX += mousemove < 0 ? 0 : mousemove;
			

			//Clamp the rotation average to be within a specific value range
			//rotationY = ClampAngle(rotationY, minimumY, maximumY);
			if (rotationX >= 360f) {
				rotationX = ClampAngle(rotationX, minimumX, maximumX);
				mouseXEndPos = Mouse.current.position.x.value;
				if(mouseXStartPos>0){
					this.distanceInPixelsTraveled = -mouseXStartPos+mouseXEndPos;
				}else{
					this.distanceInPixelsTraveled = Mathf.Abs(mouseXStartPos)+mouseXEndPos;
				}
				this.distanceIncmTraveled = (this.distanceInPixelsTraveled*2.54f) / Screen.dpi;
					// Figure out how many physical inches the mouse has traveled.
					//this.physicalDistanceInInchesTraveled = this.distanceInInchesTraveled / (800 / Screen.dpi);
				this.physicalDistanceIncmTraveled = (this.distanceInPixelsTraveled*2.54f)/ mouseDPI;
				this.stop = true;
			}
			
			//Get the rotation you will be at next as a Quaternion
			//Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
			Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
			
			//Rotate
			Camera.main.transform.localRotation = originalRotation * xQuaternion;
			// * yQuaternion;
			//Debug.Log("mouse starting position: "+ mouseXStartPos);
			//Debug.Log("mouse ending position: "+ mouseXEndPos);
			//Debug.Log("distance in pixels traveled: "+ this.distanceInPixelsTraveled);				
			//Debug.Log("distance in cm traveled: "+ this.distanceIncmTraveled);
			//Debug.Log("physical distance in cm traveled: "+ this.physicalDistanceIncmTraveled);
			//Debug.Log("rotation: "+ this.rotationX);
			//Debug.Log("Screen dpi: "+ Screen.dpi);
		}
		public float GetLength(string which) {
			switch(which) {
				case "pixels": return this.distanceInPixelsTraveled;
				
				case "screeninches": return this.distanceIncmTraveled;
				case "padinches": return this.physicalDistanceIncmTraveled;
				default: return 0f;
			}
		}
		public float GetAngleX() {
				return rotationX;
		}
		public void SetSensitivity(string sensitivity) {
		// Source games conversion...
		//this.sensitivityX = float.Parse(sensitivity) / 2.2727f;
		//this.sensitivityY = float.Parse(sensitivity) / 2.2727f;
		
		this.sensitivityX = float.Parse(sensitivity);
		this.sensitivityY = float.Parse(sensitivity);
		}
	
	}
	
}