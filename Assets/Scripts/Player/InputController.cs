using UnityEngine;

namespace Player
{
	public static class InputController
	{
		public static Vector2 GetPlayerMovement()
		{
			var horizontal = Input.GetAxis("Horizontal");
			var vertical = Input.GetAxis("Vertical");
			
			var movement = new Vector2(horizontal, vertical);

			if (movement.sqrMagnitude > 1)
			{
				movement.Normalize();
			}
			
			return movement;
		}

		public static Vector2 GetMouseDelta()
		{
			return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		}

		public static float GetMouseScrollValue()
		{
			return Input.mouseScrollDelta.y;
		}

		public static bool IsCrouchHeld()
		{
			return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		}

		public static bool IsSprintHeld()
		{
			return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		}

		public static bool IsJumpPressed()
		{
			return Input.GetKeyDown(KeyCode.Space);
		}

		public static bool IsShootHeld()
		{
			return Input.GetKey(KeyCode.Mouse0);
		}

		public static bool IsShootPressed()
		{
			return Input.GetKeyDown(KeyCode.Mouse0);
		}

		public static bool IsReloadPressed()
		{
			return Input.GetKeyDown(KeyCode.R);
		}
	}
}