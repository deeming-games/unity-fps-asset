using System;
using UnityEngine;

namespace Player
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private Camera viewCamera;

		[SerializeField] private float sensitivityX = 1f;
		[SerializeField] private float sensitivityY = 1f;

		[SerializeField] private bool clampRotation = true;
		[SerializeField] private float clampMinX = -90f;
		[SerializeField] private float clampMaxX = 90f;

		[SerializeField] private bool enableCameraBob = true;
		[SerializeField] private float crouchBobSpeed = 10f;
		[SerializeField] private float crouchBobAmount = 0.02f;
		[SerializeField] private float walkBobSpeed = 15f;
		[SerializeField] private float walkBobAmount = 0.05f;
		[SerializeField] private float sprintBobSpeed = 20f;
		[SerializeField] private float sprintBobAmount = 0.1f;

		[SerializeField] private bool lockCursor = true;
		
		private InputController _inputController;
		private FirstPersonController _firstPersonController;

		private float _originalPositionY;
		private float _bobTimer;
		
		private bool _isCursorLocked;

		private void Awake()
		{
			_inputController = GetComponent<InputController>();
			_firstPersonController = GetComponent<FirstPersonController>();

			_originalPositionY = viewCamera.transform.localPosition.y;
		}

		private void Start()
		{
			_isCursorLocked = lockCursor;
		}

		private void Update()
		{
			UpdateCursorLock();

			if (!IsCursorLocked())
			{
				return;
			}
			
			var mouseDelta = _inputController.GetMouseDelta();
			var rotationY = mouseDelta.x * sensitivityX;
			var rotationX = mouseDelta.y * sensitivityY;

			var characterRotation = transform.localRotation;
			var cameraRotation = viewCamera.transform.localRotation;

			characterRotation *= Quaternion.Euler(0f, rotationY, 0f);
			cameraRotation *= Quaternion.Euler(-rotationX, 0f, 0f);

			if (clampRotation)
			{
				cameraRotation = ClampRotationAroundXAxis(cameraRotation);
			}

			transform.localRotation = characterRotation;
			viewCamera.transform.localRotation = cameraRotation;
		}
		
		private Quaternion ClampRotationAroundXAxis(Quaternion q)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1.0f;

			var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

			angleX = Mathf.Clamp (angleX, clampMinX, clampMaxX);

			q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

			return q;
		}

		public void UpdateCameraOnMovement(Vector3 movement)
		{
			if (!enableCameraBob)
			{
				return;
			}

			var speed = walkBobSpeed;
			var amount = walkBobAmount;

			if (_firstPersonController.IsCrouching)
			{
				speed = crouchBobSpeed;
				amount = crouchBobAmount;
			}
			else if (_firstPersonController.IsSprinting)
			{
				speed = sprintBobSpeed;
				amount = sprintBobAmount;
			}
			else if (_firstPersonController.IsJumping)
			{
				speed *= 0f;
				amount *= 0f;
			}
			
			if (Mathf.Abs(movement.x) > 0.1f || Mathf.Abs(movement.z) > 0.1)
			{
				_bobTimer += Time.deltaTime * speed;
				viewCamera.transform.localPosition = new Vector3(viewCamera.transform.localPosition.x, _originalPositionY + Mathf.Sin(_bobTimer) * amount, viewCamera.transform.localPosition.z);
			}
			else
			{
				_bobTimer = 0;
				viewCamera.transform.localPosition = new Vector3(viewCamera.transform.localPosition.x, Mathf.Lerp(viewCamera.transform.localPosition.y, _originalPositionY, Time.deltaTime * speed), viewCamera.transform.localPosition.z);
			}
		}

		private void UpdateCursorLock()
		{
			if (lockCursor)
			{
				InternalLockUpdate();
			}
		}

		public bool IsCursorLocked()
		{
			return _isCursorLocked;
		}
		
		private void InternalLockUpdate()
		{
			if (_inputController.UnlockCursor())
			{
				_isCursorLocked = false;
			}
			else if (_inputController.LockCursor())
			{
				_isCursorLocked = true;
			}
			
			switch (_isCursorLocked)
			{
				case true:
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
					break;

				case false:
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					break;
			}
		}
	}
}