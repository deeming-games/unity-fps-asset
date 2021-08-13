using System;
using System.Collections;
using Decal;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapon
{
	public class WeaponControllerBase : MonoBehaviour
	{
		[SerializeField] private protected Camera foregroundCamera;
		[SerializeField] private protected LayerMask ignoreLayer;
		
		[SerializeField] private protected ParticleSystem muzzleFlash;
		[SerializeField] private protected GameObject shellEjector;
		[SerializeField] private protected GameObject shellPrefab;

		[SerializeField] private protected AudioClip shootAudio;
		[SerializeField] private protected AudioClip shootEmptyAudio;
		[SerializeField] private protected AudioClip drawEmptyAudio;
		[SerializeField] private protected AudioClip reloadAudio;
		[SerializeField] private protected AudioClip drawAudio;
		[SerializeField] private protected AudioClip holsterAudio;

		[SerializeField] private protected int damage;
		[SerializeField] private protected float spread;
		[SerializeField] private protected float range;
		[SerializeField] private protected int magazineSize;
		[SerializeField] private protected bool allowButtonHold;
		[SerializeField] private protected float fireRate = 1f;
		[SerializeField] private protected bool automaticReload = true;

		[SerializeField] private protected bool showCrosshair = true;
		
		private protected Animator Animator;
		private protected AudioSource AudioSource;
		private protected DecalController DecalInstance;
		private protected FirstPersonController FirstPersonInstance;
		private protected WeaponSwitcher WeaponSwitcher;

		private protected int BulletsLeft;

		private protected bool IsShooting;
		private protected bool IsReloading;
		private protected bool IsReadyToShoot;
		private protected float NextTimeToShoot;

		private void Awake()
		{
			Animator = GetComponent<Animator>();
			AudioSource = GetComponent<AudioSource>();
			WeaponSwitcher = transform.parent.GetComponent<WeaponSwitcher>();

			SetBulletsLeft(magazineSize);
		}

		private void Start()
		{
			DecalInstance = DecalController.Instance;
			FirstPersonInstance = FirstPersonController.Instance;
		}

		private void OnDisable()
		{
			Animator.Rebind();
		}

		private void OnEnable()
		{
			Animator.Update(0f);
		}

		private void Update()
		{
			var isShootHeld = InputController.IsShootHeld();
			var isShootPressed = InputController.IsShootPressed();
			
			IsShooting = allowButtonHold ? isShootHeld : isShootPressed;

			if (IsReadyToShoot && IsShooting && !IsReloading && Time.time >= NextTimeToShoot)
			{
				NextTimeToShoot = Time.time + 1f / fireRate;
				
				if (BulletsLeft > 0)
				{
					HandleShoot();
				}
				else if (isShootPressed)
				{
					if (shootEmptyAudio)
					{
						AudioSource.PlayOneShot(shootEmptyAudio);
					}
				}
			}

			if (InputController.IsReloadPressed() && BulletsLeft < magazineSize && !IsReloading)
			{
				HandleReload();
			}

			if (BulletsLeft == 0 && automaticReload && !IsReloading)
			{
				HandleReload();
			}
		}

		private void HandleShoot()
		{
			PlayShootAnimation();
			HandleShootEffects();
			
			SetBulletsLeft(BulletsLeft - 1);
		}

		private protected virtual void PlayShootAnimation()
		{
			Animator.Play("shoot");
		}

		private void HandleShootEffects()
		{
			var spreadX = Random.Range(-spread, spread);
			var spreadY = Random.Range(-spread, spread);
			var spreadZ = Random.Range(-spread, spread);

			var spreadFactor = 1f;

			if (FirstPersonInstance.IsCrouching)
			{
				spreadFactor = 0.5f;
			}
			else if (FirstPersonInstance.IsSprinting)
			{
				spreadFactor = 2f;
			}

			var direction = foregroundCamera.transform.forward + new Vector3(-spreadX, -spreadY, -spreadZ) * spreadFactor;

			if (shootAudio)
			{
				AudioSource.PlayOneShot(shootAudio);
			}

			if (muzzleFlash)
			{
				muzzleFlash.Play();
			}

			if (shellPrefab)
			{
				var shell = Instantiate(shellPrefab, shellEjector.transform.position, Quaternion.FromToRotation(Vector3.right, transform.right));

				var euler = shell.transform.eulerAngles;
				euler.x += 90f;

				shell.transform.eulerAngles = euler;

				Destroy(shell, 5f);
			}
			
			if (Physics.Raycast(foregroundCamera.transform.position, direction, out var rayHit, range, ~ignoreLayer))
			{
				DecalInstance.SpawnDecal(rayHit);
			}
		}

		private void HandleReload()
		{
			PlayReloadAnimation();
		}
		
		private protected virtual void PlayReloadAnimation()
		{
			Animator.Play("reload");
		}

		public void ReloadEventStart()
		{
			IsReloading = true;
			IsReadyToShoot = false;

			if (reloadAudio)
			{
				AudioSource.PlayOneShot(reloadAudio);
			}
		}

		public void ReloadEventEnd()
		{
			SetBulletsLeft(magazineSize);
			IsReloading = false;
			IsReadyToShoot = true;
		}
		
		private void SetBulletsLeft(int bulletsLeft)
		{
			BulletsLeft = bulletsLeft;
		}
		
		public void Draw()
		{
			PlayDrawAnimation();
		}

		private protected virtual void PlayDrawAnimation()
		{
			Animator.Play("draw");
		}

		public void DrawEventStart()
		{
			IsReadyToShoot = false;

			if (!drawAudio)
			{
				return;
			}

			if (drawEmptyAudio)
			{
				AudioSource.PlayOneShot(BulletsLeft > 0 ? drawAudio : drawEmptyAudio);	
			}
			else
			{
				AudioSource.PlayOneShot(drawAudio);
			}
		}

		public void DrawEventEnd()
		{
			IsReadyToShoot = true;
			UIController.Instance.SetCrosshairActive(showCrosshair);
		}

		public void Holster()
		{
			PlayHolsterAnimation();
		}

		private protected virtual void PlayHolsterAnimation()
		{
			Animator.Play("holster");
		}

		public void HolsterEventStart()
		{
			IsReadyToShoot = false;

			if (holsterAudio)
			{
				AudioSource.PlayOneShot(holsterAudio);
			}
		}

		public void HolsterEventEnd()
		{
			WeaponSwitcher.CompleteSwitchingWeapon();
		}
	}
}