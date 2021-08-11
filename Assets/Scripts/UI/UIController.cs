using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UIController : SingletonMonoBehaviour<UIController>
	{
		[SerializeField] private Image crosshair;
		
		protected override void _Awake() {}

		public void SetCrosshairActive(bool showCrosshair)
		{
			crosshair.gameObject.SetActive(showCrosshair);
		}
	}
}
