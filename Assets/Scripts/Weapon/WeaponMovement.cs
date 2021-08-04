using Player;
using UnityEngine;

namespace Weapon
{
	public class WeaponMovement : MonoBehaviour
	{
		[SerializeField] private InputController inputController;
		[SerializeField] private FirstPersonController firstPersonController;

		[SerializeField] private float swayAmount = 0.01f;
		[SerializeField] private float swayMaxAmount = 0.02f;
		[SerializeField] private float swaySmoothAmount = 2f;
		
		[SerializeField] private float forwardBobSpeed = 10.0f;
		[SerializeField] private float forwardBobMaxAmount = 0.05f;
		[SerializeField] private float sideBobSpeed = 0.15f;
		[SerializeField] private float sideBobAmount = 0.04f;

		private float _forwardBobAmount;
		private float _timer;

		private Vector3 _initialPosition;
		
		private void Awake()
		{
			_initialPosition = transform.localPosition;
		}

		private void Update()
		{
			HandleCameraSway();
			HandleMovementBob();
		}

		private void HandleCameraSway()
		{
			if (!firstPersonController.IsCursorLocked())
			{
				return;
			}

			var movement = inputController.GetMouseDelta();
			var movementX = Mathf.Clamp(-movement.x * swayAmount, -swayMaxAmount, swayMaxAmount);
			var movementY = Mathf.Clamp(-movement.y * swayAmount, -swayMaxAmount, swayMaxAmount);

			var finalPosition = new Vector3(movementX, movementY, 0f);
			transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + _initialPosition, Time.deltaTime * swaySmoothAmount);
		}

		private void HandleMovementBob()
		{
			if (firstPersonController.IsJumping 
			    || !firstPersonController.IsGrounded()
			    || !firstPersonController.IsCursorLocked()
			)
			{
				return;
			}

			var movement = inputController.GetPlayerMovement();
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
			if (firstPersonController.IsCrouching)
			{
				speedFactor = 0.5f;
				amountFactor = 0.5f;
			}
			else if (firstPersonController.IsSprinting)
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