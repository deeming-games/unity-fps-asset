using System;
using System.Collections;
using Decal;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapon
{
	public enum WeaponFireMode {
		SemiAuto,
		FullAuto,
		Burst
	};

	public class WeaponControllerBase : MonoBehaviour
	{
		[SerializeField] private protected Camera foregroundCamera;
		[SerializeField] private protected LayerMask ignoreLayer;
		
		[SerializeField] private protected ParticleSystem muzzleFlash;
		[SerializeField] private protected GameObject impactEffect;
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
		
		[SerializeField] private protected WeaponFireMode fireMode = WeaponFireMode.SemiAuto;
		
		[Tooltip("Rate of fire (RPM)")]
		[SerializeField] private protected float rateOfFire = 600f;
		[SerializeField] private protected int burstSize = 1;

		[SerializeField] private protected float shellEjectVelocity = 10f;
		[SerializeField] private protected float shellEjectSpin = 2f;
		
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
			if (IsReadyToShoot && !IsReloading)
			{
				if (BulletsLeft > 0)
				{
					switch (fireMode)
					{
						case WeaponFireMode.SemiAuto:
						case WeaponFireMode.Burst:
							IsShooting = InputController.IsShootPressed();
							break;
				
						case WeaponFireMode.FullAuto:
							IsShooting = InputController.IsShootHeld();
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					if (IsShooting)
					{
						StartCoroutine(nameof(HandleShoot));
					}	
				}
			}

			if (InputController.IsReloadPressed() && BulletsLeft < magazineSize && !IsReloading)
			{
				HandleReload();
			}

			if (BulletsLeft == 0)
			{
				if (InputController.IsShootPressed())
				{
					AudioSource.PlayOneShot(shootEmptyAudio);
				}

				if (automaticReload && !IsReloading)
				{
					HandleReload();
				}
			}
		}

		private IEnumerator HandleShoot()
		{
			var thisBurstSize = burstSize; 

			if (fireMode != WeaponFireMode.Burst)
			{
				thisBurstSize = 1;
			}
			
			IsReadyToShoot = false;

			for (var i = 0; i < thisBurstSize; i++)
			{
				PlayShootAnimation();
				HandleShootEffects();
				SetBulletsLeft(BulletsLeft - 1);

				if (BulletsLeft == 0)
				{
					yield break;
				}
				
				yield return new WaitForSeconds(60 / rateOfFire);
			}
			
			if (fireMode == WeaponFireMode.Burst)
			{
				yield return new WaitForSeconds(60 / rateOfFire * thisBurstSize - 1);
			}

			IsReadyToShoot = true;
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

			HandleShellEjection();
			
			if (Physics.Raycast(foregroundCamera.transform.position, direction, out var rayHit, range, ~ignoreLayer))
			{
				DecalInstance.SpawnDecal(rayHit);

				if (impactEffect)
				{
					var impact = Instantiate(impactEffect, rayHit.point, Quaternion.LookRotation(rayHit.normal));
					Destroy(impact, 2f);
				}
			}
		}

		private void HandleShellEjection()
		{
			if (!shellPrefab)
			{
				return;
			}
			
			var shell = Instantiate(
				shellPrefab, 
				shellEjector.transform.position, 
				shellEjector.transform.rotation
			);
			
			var shellCollider = shell.GetComponent<Collider>();
			var playerCollider = FirstPersonInstance.GetComponent<Collider>();
		
			Physics.IgnoreCollision(shellCollider, playerCollider);

			var shellRigidbody = shell.GetComponent<Rigidbody>();
			shellRigidbody.useGravity = true;

			// orients the shell casing correctly relative to the vertical rotation of the camera
			var euler = shell.transform.eulerAngles;
			euler.x += 90f;

			shell.transform.eulerAngles = euler;

			var force = shellEjector.transform.right.normalized * shellEjectVelocity;
			force.y += shellEjectVelocity * Random.Range(0.75f, 1f);

			shellRigidbody.AddForce(force, ForceMode.Impulse);

			var velocityForce = FirstPersonInstance.GetCharacterVelocity();
			shellRigidbody.AddForce(velocityForce, ForceMode.VelocityChange);

			if (Random.value > 0.5f)
			{
				shellRigidbody.AddRelativeTorque(-Random.rotation.eulerAngles * shellEjectSpin);
			}
			else
			{
				shellRigidbody.AddRelativeTorque(Random.rotation.eulerAngles * shellEjectSpin);
			}

			Destroy(shell, 3f);
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