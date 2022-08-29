using UnityEngine;

/// <summary>
/// ΩÃ±€≈Ê ∫Œ∏ ≈¨∑°Ω∫ ΩÃ≈¨≈Ê¿Ã « ø‰«“∞ÊøÏ ªÛº”
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	[Header("DD - Check")]
	public bool isDontDestory;
	private static T m_instance;

	public static T Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = FindObjectOfType(typeof(T)) as T;

				if (m_instance == null)
				{
					GameObject singletonObj = new GameObject();
					singletonObj.name = typeof(T).ToString();

					m_instance = singletonObj.AddComponent<T>();
				}
			}
			return m_instance;
		}

		private set { }
	}
	public static bool isDontDestroyOnLoad = false;

	public void Awake()
	{
		if (isDontDestroyOnLoad == true)
		{
			DestroyImmediate(gameObject);
			return;
		}
		m_instance = FindObjectOfType(typeof(T)) as T;

		if (isDontDestory == true)
		{
			DontDestroyOnLoad(m_instance);
			isDontDestroyOnLoad = true;
		}

		Initialized();
	}

	public virtual void Initialized()
	{
	}

	public static bool IsCreated()
	{
		return m_instance != null;
	}

	public static void DestroySelf()
	{
		if (m_instance == null)
			return;

		DestroyImmediate(m_instance.gameObject);
		m_instance = null;
		isDontDestroyOnLoad = false;
	}
}