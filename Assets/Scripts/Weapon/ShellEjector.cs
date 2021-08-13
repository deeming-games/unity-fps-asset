using Player;
using UnityEngine;

namespace Weapon
{
	public class ShellEjector : MonoBehaviour
	{
		[SerializeField] private AudioClip[] shellDropSounds;

		private AudioSource _audioSource;
		private bool _shellDropPlayed;

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
		}
		
		private void Start()
		{
			var shellCollider = GetComponent<Collider>();
			var playerCollider = FirstPersonController.Instance.GetComponent<Collider>();
		
			Physics.IgnoreCollision(shellCollider, playerCollider);
	    
			var rigidBody = GetComponent<Rigidbody>();
			rigidBody.useGravity = true;

			var force = transform.right * Random.Range(2f, 4f);
			force.y += Random.Range(3f, 6f);

			rigidBody.AddForce(force);
		}
		
		private void OnCollisionEnter(Collision collision)
		{
			if (_shellDropPlayed)
			{
				return;
			}

			var shellDrop = shellDropSounds[Random.Range(0, shellDropSounds.Length - 1)];

			_audioSource.PlayOneShot(shellDrop);
			_shellDropPlayed = true;
		}
	}
}