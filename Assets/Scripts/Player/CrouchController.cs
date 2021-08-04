using UnityEngine;

namespace Player
{
	public class CrouchController : MonoBehaviour
	{
		[SerializeField] private float crouchHeightFactor = 0.3f;
		[SerializeField] private float transitionSpeed = 3f;

		private FirstPersonController _firstPersonController;
		private CharacterController _characterController;

		private bool _isCrouching;
		private bool _isUncrouching;
		private bool _canUncrouch;

		private float _standingHeight;
		private float _targetHeight;
		private float _crouchingHeight;

		private void Awake()
		{
			_firstPersonController = GetComponent<FirstPersonController>();
			_characterController = GetComponent<CharacterController>();
			
			_standingHeight = _characterController.height;
			_crouchingHeight = _standingHeight * crouchHeightFactor;
			_targetHeight = _standingHeight;
		}

		private void Update()
		{
			if (InputController.IsCrouchHeld())
			{
				_isCrouching = true;
				_targetHeight = _crouchingHeight;

				_firstPersonController.IsCrouching = true;
			}
			else
			{
				if (_isCrouching && !_isUncrouching)
				{
					_isUncrouching = true;
				}

				if (_canUncrouch)
				{
					_isUncrouching = false;
					_isCrouching = false;
					_canUncrouch = false;
					_targetHeight = _standingHeight;
					
					_firstPersonController.IsCrouching = false;
				}
			}

			_characterController.height = Mathf.Lerp(
				_characterController.height, 
				_targetHeight, transitionSpeed * Time.deltaTime
			);
		}

		private void FixedUpdate()
		{
			if (!_isUncrouching)
			{
				return;
			}

			var upRay = new Ray(_characterController.transform.position, Vector3.up);
			if (Physics.SphereCast(upRay, _characterController.radius - _characterController.skinWidth, out var hit, _crouchingHeight))
			{
				_canUncrouch = hit.distance > _standingHeight;
			}
			else
			{
				// has not hit anything so can uncrouch
				_canUncrouch = true;
			}
		}
	}
}