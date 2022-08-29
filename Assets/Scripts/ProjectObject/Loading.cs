using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
	public void OnEnable()
	{
		StopCoroutine(nameof(R_Rotate));
		StartCoroutine(nameof(R_Rotate));
	}

	public void OnDisable()
	{
		StopCoroutine(nameof(R_Rotate));
	}

	private IEnumerator R_Rotate()
	{
		transform.rotation = Quaternion.identity;
		while (gameObject.activeSelf)
		{
			transform.Rotate(Vector3.forward,Time.deltaTime * 10);
			yield return null;
		}
	}
}
