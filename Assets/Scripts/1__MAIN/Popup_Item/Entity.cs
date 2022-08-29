using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity<T> : MonoBehaviour
{
	protected T entityData;

	public abstract void SetEntity(T _data);

	public T GetEntityData()
	{
		return entityData;
	}

	public virtual void Hide()
	{
		gameObject.SetActive(false);
	}
}
