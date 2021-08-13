using Player;
using UI;
using UnityEngine;

namespace Weapon
{
	public class WeaponSwitcher : MonoBehaviour
	{
		[SerializeField] private int defaultWeapon;

		private int _selectedWeapon;
		private int? _nextWeapon;
		
		private readonly KeyCode[] _numericKeyCodes = {
			KeyCode.Alpha0,
			KeyCode.Alpha1,
			KeyCode.Alpha2,
			KeyCode.Alpha3,
			KeyCode.Alpha4,
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha8,
			KeyCode.Alpha9
		};

		private void Awake()
		{
			foreach (Transform childWeapon in transform)
			{
				childWeapon.gameObject.SetActive(false);
			}

			StartSwitchingWeapon(defaultWeapon);
		}

		private void Update()
		{
			var selectedWeapon = GetSelectedWeaponFromInput();
			if (selectedWeapon != _selectedWeapon)
			{
				StartSwitchingWeapon(selectedWeapon);
			}
		}

		private int GetSelectedWeaponFromInput()
		{
			var selectedWeapon = _selectedWeapon;
			var mouseScrollValue = InputController.GetMouseScrollValue();

			if (mouseScrollValue > 0)
			{
				selectedWeapon++;
			}
			else if (mouseScrollValue < 0)
			{
				selectedWeapon--;
			}
			else
			{
				for (var i = 0; i < _numericKeyCodes.Length; i++)
				{
					if (Input.GetKeyDown(_numericKeyCodes[i]))
					{
						selectedWeapon = i;
					}
				}
			}

			if (selectedWeapon > GetMaxWeaponIndex())
			{
				selectedWeapon = 0;
			}

			if (selectedWeapon < 0)
			{
				selectedWeapon = GetMaxWeaponIndex();
			}

			return selectedWeapon;
		}

		private void StartSwitchingWeapon(int weaponIndex)
		{
			_nextWeapon = weaponIndex;

			var currentlySelected = transform.GetChild(_selectedWeapon);
			var currentWeaponController = currentlySelected.GetComponent<WeaponControllerBase>();

			if (currentWeaponController)
			{
				currentWeaponController.Holster();
			}
			else
			{
				CompleteSwitchingWeapon();
			}
		}
		
		public void CompleteSwitchingWeapon()
		{
			if (_nextWeapon == null)
			{
				return;
			}

			var currentlySelected = transform.GetChild(_selectedWeapon);
			var nextSelected = transform.GetChild((int)_nextWeapon);
			var nextWeaponController = nextSelected.GetComponent<WeaponControllerBase>();
			
			currentlySelected.gameObject.SetActive(false);
			
			nextSelected.gameObject.SetActive(true);
			nextSelected.GetComponent<WeaponControllerBase>()?.Draw();

			if (!nextWeaponController)
			{
				UIController.Instance.SetCrosshairActive(false);
			}
			
			_selectedWeapon = (int)_nextWeapon;

			_nextWeapon = null;
		}
		
		private int GetMaxWeaponIndex()
		{
			return transform.childCount - 1;
		}
	}
}