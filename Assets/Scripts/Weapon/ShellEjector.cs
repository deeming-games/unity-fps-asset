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