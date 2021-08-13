using Player;
using UnityEngine;

namespace Weapon
{
	public class WeaponMovement : MonoBehaviour
	{
		[SerializeField] private float swayMoveAmount = 0.1f;
		[SerializeField] private float swayMoveMaxAmount = 0.25f;
		[SerializeField] private float swayMoveSmoothAmount = 5f;
		
		[SerializeField] private float swayRotationAmount = 4f;
		[SerializeField] private float swayRotationMaxAmount = 5f;
		[SerializeField] private float swayRotationSmoothAmount = 12f;
		
		[SerializeField] private float forwardBobSpeed = 10.0f;
		[SerializeField] private float forwardBobMaxAmount = 0.05f;
		[SerializeField] private float sideBobSpeed = 0.15f;
		[SerializeField] private float sideBobAmount = 0.04f;

		private FirstPersonController _firstPersonInstance; 

		private float _forwardBobAmount;
		private float _timer;

		private Vector3 _initialPosition;
		private Quaternion _initialRotation;
		
		private void Awake()
		{
			_initialPosition = transform.localPosition;
			_initialRotation = transform.localRotation;
		}

		private void Start()
		{
			_firstPersonInstance = FirstPersonController.Instance;
		}

		private void Update()
		{
			HandleCameraSway();
			HandleMovementBob();
		}

		private void HandleCameraSway()
		{
			var mouseMovement = InputController.GetMouseDelta();
			var movementX = Mathf.Clamp(-mouseMovement.x * swayMoveAmount, -swayMoveMaxAmount, swayMoveMaxAmount);
			var movementY = Mathf.Clamp(-mouseMovement.y * swayMoveAmount, -swayMoveMaxAmount, swayMoveMaxAmount);

			var finalPosition = new Vector3(movementX, movementY, 0f);
			transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + _initialPosition, Time.deltaTime * swayMoveSmoothAmount);

			var rotationY = Mathf.Clamp(-mouseMovement.x * swayRotationAmount, -swayRotationMaxAmount, swayRotationMaxAmount);
			var rotationX = Mathf.Clamp(-mouseMovement.y * swayRotationAmount, -swayRotationMaxAmount, swayRotationMaxAmount);
			
			var finalRotation = Quaternion.Euler(new Vector3(rotationX, -rotationY, 0f));
			transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * _initialRotation, Time.deltaTime * swayRotationSmoothAmount);
		}

		private void HandleMovementBob()
		{
			if (_firstPersonInstance.IsJumping 
			    || !_firstPersonInstance.IsGrounded()
			)
			{
				return;
			}

			var movement = InputController.GetPlayerMovement();
			var horizontal = Mathf.Abs(movement.x);
			var vertical = Mathf.Abs(movement.y);

			if (horizontal > 0 || vertical > 0)
			{
				var moveSpeed = Mathf.Clamp(horizontal + vertical, 0, 1) * forwardBobSpeed;

				_forwardBobAmount += moveSpeed;
			}
			else
			{
				_forwardBobAmount -= forwardBobSpeed;

				if (_forwardBobAmount < 0)
				{
					_forwardBobAmount = 0;
				}
			}

			_forwardBobAmount *= Time.deltaTime;
			
			var speedFactor = 1f;
			var amountFactor = 2f;
			if (_firstPersonInstance.IsCrouching)
			{
				speedFactor = 0.5f;
				amountFactor = 0.5f;
			}
			else if (_firstPersonInstance.IsSprinting)
			{
				speedFactor = 1.5f;
			}

			var xMovement = 0f;
			var yMovement = 0f;

			var calcPosition = _initialPosition;
			
			if (horizontal == 0 && vertical == 0)
			{
				_timer = 0.0f;
			}
			else
			{
				xMovement = Mathf.Sin(_timer);
				yMovement = -Mathf.Abs(Mathf.Abs(xMovement) - 1);

				_timer += sideBobSpeed * speedFactor;
				
				if (_timer > Mathf.PI * 2)
				{
					_timer -= Mathf.PI * 2;
				}
			}

			var totalMovement = Mathf.Clamp(vertical + horizontal, 0, 1);
			
			if (xMovement != 0)
			{
				xMovement *= totalMovement;
				calcPosition.x = _initialPosition.x + xMovement * sideBobAmount * amountFactor;
			}
			else
			{
				calcPosition.x = _initialPosition.x;
			}
			
			if (yMovement != 0)
			{
				yMovement *= totalMovement;
				calcPosition.y = _initialPosition.y + yMovement * sideBobAmount * amountFactor;
			}
			else
			{
				calcPosition.y = _initialPosition.y;
			}
			
			var totalFrontX = Mathf.Clamp(_forwardBobAmount, -forwardBobMaxAmount, forwardBobMaxAmount);
			var totalFrontY = Mathf.Clamp(_forwardBobAmount, -forwardBobMaxAmount, forwardBobMaxAmount);
			var totalFrontZ = Mathf.Clamp(_forwardBobAmount, -forwardBobMaxAmount, forwardBobMaxAmount);

			calcPosition.x += totalFrontX;
			calcPosition.y -= totalFrontY;
			calcPosition.z -= totalFrontZ;

			transform.localPosition = Vector3.Lerp(transform.localPosition, calcPosition, Time.deltaTime);
		}
	}
}