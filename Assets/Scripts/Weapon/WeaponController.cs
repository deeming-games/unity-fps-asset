using Decal;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapon
{
	public class WeaponController : MonoBehaviour
	{
		[SerializeField] private FirstPersonController firstPersonController;
		[SerializeField] private DecalController decalController;
		
		[SerializeField] private Camera foregroundCamera;
		[SerializeField] private LayerMask ignoreLayer;
		
		[SerializeField] private ParticleSystem muzzleFlash;
		[SerializeField] private GameObject shellEjector;
		[SerializeField] private GameObject shellPrefab;

		[SerializeField] private AudioClip shootAudio;
		[SerializeField] private AudioClip shootEmptyAudio;
		[SerializeField] private AudioClip reloadAudio;
		[SerializeField] private AudioClip drawAudio;
		[SerializeField] private AudioClip drawEmptyAudio;
		[SerializeField] private AudioClip holsterAudio;

		[SerializeField] private GameObject debugBlob;

		[SerializeField] private int damage;
		[SerializeField] private float spread;
		[SerializeField] private float range;
		[SerializeField] private float timeBetweenShots;
		[SerializeField] private int magazineSize;
		[SerializeField] private int bulletsPerBurst = 1;
		[SerializeField] private bool allowButtonHold;
		[SerializeField] private bool automaticReload = true;

		[SerializeField] private bool showCrosshair = true;

		private WeaponSwitcher _weaponSwitcher;
		private Animator _animator;
		private AudioSource _audioSource;

		private int _bulletsLeft;
		private int _burstBulletsLeft;

		private bool _isShooting;
		private bool _isReadyToShoot;
		private bool _isReloading;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			_audioSource = GetComponent<AudioSource>();
			_weaponSwitcher = transform.parent.GetComponent<WeaponSwitcher>();

			SetBulletsLeft(magazineSize);
		}

		private void OnDisable()
		{
			_animator.Rebind();
		}

		private void OnEnable()
		{
			_animator.Update(0f);
		}

		private void Update()
		{
			_isShooting = allowButtonHold ? InputController.IsShootHeld() : InputController.IsShootPressed();

			if (_isReadyToShoot && _isShooting && !_isReloading)
			{
				if (_bulletsLeft > 0)
				{
					_burstBulletsLeft = bulletsPerBurst;
					HandleShoot();	
				}
				else
				{
					if (shootEmptyAudio)
					{
						_audioSource.PlayOneShot(shootEmptyAudio);
					}
				}
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

			_animator.Play(_bulletsLeft > 1 ? "shoot_not_empty" : "shoot_empty");

			if (Physics.Raycast(foregroundCamera.transform.position, direction, out var rayHit, range, ~ignoreLayer))
			{
				decalController.SpawnDecal(rayHit);
			}

			SetBulletsLeft(_bulletsLeft - 1);
			_burstBulletsLeft--;

			if (_burstBulletsLeft > 0 && _bulletsLeft > 0)
			{
				Invoke(nameof(HandleShoot), timeBetweenShots);
			}
		}

		public void ShootEventStart()
		{
			if (shootAudio)
			{
				_audioSource.PlayOneShot(shootAudio);
			}

			if (muzzleFlash)
			{
				muzzleFlash.Play();
			}

			if (shellPrefab)
			{
				var shell = Instantiate(shellPrefab, shellEjector.transform.position, Quaternion.FromToRotation(Vector3.right, transform.right));
                Destroy(shell, 5f);
			}

		}

		public void ShootEventEnd()
		{
			_isReadyToShoot = true;
			
			if (_bulletsLeft == 0 && automaticReload)
			{
				HandleReload();
			}
		}

		private void HandleReload()
		{
			_animator.Play(_bulletsLeft > 1 ? "reload_not_empty" : "reload_empty");
		}

		public void ReloadEventStart()
		{
			_isReloading = true;
			_isReadyToShoot = false;

			if (reloadAudio)
			{
				_audioSource.PlayOneShot(reloadAudio);
			}
		}

		public void ReloadEventEnd()
		{
			SetBulletsLeft(magazineSize);
			_isReloading = false;
			_isReadyToShoot = true;
		}
		
		private void SetBulletsLeft(int bulletsLeft)
		{
			_bulletsLeft = bulletsLeft;
		}
		
		public void Draw()
		{
			_animator.Play(_bulletsLeft > 0 ? "draw_not_empty" : "draw_empty");
		}

		public void DrawEventStart()
		{
			_isReadyToShoot = false;

			if (drawAudio)
			{
				_audioSource.PlayOneShot(_bulletsLeft > 0 ? drawAudio : drawEmptyAudio);
			}
		}

		public void DrawEventEnd()
		{
			_isReadyToShoot = true;
			UIController.Instance.SetCrosshairActive(showCrosshair);
		}

		public void Holster()
		{
			_animator.Play(_bulletsLeft > 0 ? "holster_not_empty" : "holster_empty");
		}

		public void HolsterEventStart()
		{
			_isReadyToShoot = false;

			if (holsterAudio)
			{
				_audioSource.PlayOneShot(holsterAudio);
			}
		}

		public void HolsterEventEnd()
		{
			_weaponSwitcher.CompleteSwitchingWeapon();
		}
	}
}