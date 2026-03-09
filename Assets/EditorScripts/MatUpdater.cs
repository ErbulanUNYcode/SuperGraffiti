#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class MatUpdater : MonoBehaviour
{
	[SerializeField]
	private Material material;
	[SerializeField]
	private Transform spray;
	[SerializeField]
	private Color color;


	[ExecuteInEditMode]
	void Update()
	{
		material.SetVectorArray("_Positions", new Vector4[] { spray.position });
		material.SetVectorArray("_Rotations", new Vector4[] { spray.eulerAngles / 180f * 3.14159265358979f });
		material.SetColorArray("_Colors", new Color[] { color });
	}
}
#endif