using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component
{
	public static T Instance;
		
	protected void Awake()
	{
		// loose singleton pattern - only add one instance per scene!
		Instance = this as T;
			
		_Awake();
	}

	protected abstract void _Awake();
}