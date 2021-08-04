using UnityEngine;

namespace Player
{
	[RequireComponent(typeof (CharacterController), typeof (InputController), typeof (CameraController))]
	public class FirstPersonController : MonoBehaviour
	{
		[SerializeField] private float crouchingSpeed = 1f;
		[SerializeField] private float walkingSpeed = 3f;
		[SerializeField] private float sprintingSpeed = 5f;
		[SerializeField] private float jumpHeight = 1f;
		[SerializeField] private float gravityValue = -9.81f;
		
		private CharacterController _characterController;
		private InputController _inputController;
		private CameraController _cameraController;

		private Vector3 _playerVelocity;
		private bool _previouslyGrounded;

		public bool IsCrouching { get; set; }
		public bool IsJumping { get; private set; }
		public bool IsSprinting { get; private set; }

		private void Awake()
		{
			_characterController = GetComponent<CharacterController>();
			_inputController = GetComponent<InputController>();
			_cameraController = GetComponent<CameraController>();

			_previouslyGrounded = true;
		}

		private void Update()
		{
			IsSprinting = _inputController.IsSprinting();

			HandleMovement();
		}

		private void HandleMovement()
		{
			if (!_cameraController.IsCursorLocked())
			{
				return;
			}

			var isGrounded = _characterController.isGrounded;
			
			if (isGrounded && _playerVelocity.y < 0)
			{
				_playerVelocity.y = 0f;
			}
			if (!_previouslyGrounded && isGrounded)
			{
				IsJumping = false;
			}
			_previouslyGrounded = isGrounded;

			var movementInput = _inputController.GetPlayerMovement();
			var movement = transform.forward * movementInput.y + transform.right * movementInput.x;

			var speed = walkingSpeed;

			if (IsCrouching)
			{
				speed = crouchingSpeed;
			}
			else if (IsSprinting)
			{
				speed = sprintingSpeed;
			}
			
			movement.x *= speed;
			movement.z *= speed;
			
			_characterController.Move(movement * Time.deltaTime);
			_cameraController.UpdateCameraOnMovement(movement);

			if (_inputController.JumpTriggered() && isGrounded)
			{
				_playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
				IsJumping = true;
			}

			_playerVelocity.y += gravityValue * Time.deltaTime;
			_characterController.Move(_playerVelocity * Time.deltaTime);
		}

		public bool IsGrounded()
		{
			return _characterController.isGrounded;
		}

		public bool IsCursorLocked()
		{
			return _cameraController.IsCursorLocked();
		}
	}
}
