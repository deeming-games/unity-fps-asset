using UnityEngine;

namespace Player
{
	public class InputController : MonoBehaviour
	{
		private PlayerControls _controls;

		protected void Awake()
		{
			_controls = new PlayerControls();
		}
		
		private void OnEnable()
		{
			_controls.Enable();
		}

		private void OnDisable()
		{
			_controls.Disable();
		}

		public Vector2 GetPlayerMovement()
		{
			return _controls.Player.Movement.ReadValue<Vector2>();
		}

		public Vector2 GetMouseDelta()
		{
			return _controls.Player.Look.ReadValue<Vector2>();
		}

		public bool IsCrouching()
		{
			return _controls.Player.Crouch.ReadValue<float>() > 0;
		}

		public bool IsSprinting()
		{
			return _controls.Player.Sprint.ReadValue<float>() > 0;
		}

		public bool JumpTriggered()
		{
			return _controls.Player.Jump.triggered;
		}

		public bool LockCursor()
		{
			return _controls.Player.LockCursor.triggered;
		}

		public bool UnlockCursor()
		{
			return _controls.Player.UnlockCursor.triggered;
		}
	}
}