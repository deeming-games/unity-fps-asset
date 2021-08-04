using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapon
{
	public class WeaponController : MonoBehaviour
	{
		[SerializeField] private FirstPersonController firstPersonController;
		[SerializeField] private Camera foregroundCamera;
		[SerializeField] private ParticleSystem muzzleFlash;
		[SerializeField] private GameObject debugBlob;

		[SerializeField] private int damage;
		[SerializeField] private float timeBetweenShooting;
		[SerializeField] private float spread;
		[SerializeField] private float range;
		[SerializeField] private float reloadTime;
		[SerializeField] private float timeBetweenShots;
		[SerializeField] private int magazineSize;
		[SerializeField] private int bulletsPerBurst = 1;
		[SerializeField] private bool allowButtonHold;

		private Animator _animator;

		private int _bulletsLeft;
		private int _burstBulletsLeft;

		private bool _isShooting;
		private bool _isReadyToShoot;
		private bool _isReloading;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			
			_bulletsLeft = magazineSize;
			_isReadyToShoot = true;
		}

		private void Update()
		{
			_isShooting = allowButtonHold ? InputController.IsShootHeld() : InputController.IsShootPressed();

			if (_isReadyToShoot && _isShooting && !_isReloading && _bulletsLeft > 0)
			{
				_burstBulletsLeft = bulletsPerBurst;
				HandleShoot();
			}

			if (InputController.IsReloadPressed() && _bulletsLeft < magazineSize && !_isReloading)
			{
				HandleReload();
			}
		}

		private void HandleShoot()
		{
			_isReadyToShoot = false;

			var spreadX = Random.Range(-spread, spread);
			var spreadY = Random.Range(-spread, spread);
			var spreadZ = Random.Range(-spread, spread);

			var spreadFactor = 1f;

			if (firstPersonController.IsCrouching)
			{
				spreadFactor = 0.5f;
			}
			else if (firstPersonController.IsSprinting)
			{
				spreadFactor = 2f;
			}

			var direction = foregroundCamera.transform.forward + new Vector3(-spreadX, -spreadY, -spreadZ) * spreadFactor;
			
			muzzleFlash.Play();
			_animator.SetTrigger("animatorShooting");

			if (Physics.Raycast(foregroundCamera.transform.position, direction, out var rayHit, range))
			{
				Instantiate(debugBlob, rayHit.point, Quaternion.identity);
			}

			_bulletsLeft--;
			_burstBulletsLeft--;
			
			Invoke(nameof(ResetShot), timeBetweenShooting);

			if (_burstBulletsLeft > 0 && _bulletsLeft > 0)
			{
				Invoke(nameof(HandleShoot), timeBetweenShots);
			}
		}

		private void ResetShot()
		{
			_isReadyToShoot = true;
		}

		private void HandleReload()
		{
			_isReloading = true;
			_animator.SetBool("animatorReloading", true);
			Invoke(nameof(ResetReload), reloadTime);
		}

		private void ResetReload()
		{
			_bulletsLeft = magazineSize;
			_isReloading = false;
			_animator.SetBool("animatorReloading", false);
		}
	}
}