using System.Collections.Generic;
using UnityEngine;

namespace Decal
{
	public class DecalController : MonoBehaviour
	{
		[SerializeField] private GameObject bulletHoleDecalPrefab;
		[SerializeField] private int maxConcurrentDecals = 10;
		
		private Queue<GameObject> _decalsInPool;
		private Queue<GameObject> _decalsActiveInWorld;
		
		private void Awake()
		{
			InitializeDecals();
		}

		private void InitializeDecals()
		{
			_decalsInPool = new Queue<GameObject>();
			_decalsActiveInWorld = new Queue<GameObject>();

			for (var i = 0; i < maxConcurrentDecals; i++)
			{
				InstantiateDecal();
			}
		}
		
		private void InstantiateDecal()
		{
			var spawned = Instantiate(bulletHoleDecalPrefab, transform, true);

			_decalsInPool.Enqueue(spawned);
			spawned.SetActive(false);
		}
		
		public void SpawnDecal(RaycastHit hit)
		{
			var decal = GetNextAvailableDecal();
			
			if (decal == null)
			{
				return;
			}
			
			

			decal.transform.position = hit.point;
			decal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
			decal.transform.position -= decal.transform.forward * 0.001f;
			decal.transform.SetParent(hit.collider.transform);
			decal.SetActive(true);

			_decalsActiveInWorld.Enqueue(decal);
		}
		
		private GameObject GetNextAvailableDecal()
		{
			if (_decalsInPool.Count > 0)
			{
				return _decalsInPool.Dequeue();
			}

			var oldestActiveDecal = _decalsActiveInWorld.Dequeue();
			return oldestActiveDecal;
		}
	}
}
