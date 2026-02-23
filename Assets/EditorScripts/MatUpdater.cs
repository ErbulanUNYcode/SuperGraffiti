using UnityEngine;

[ExecuteAlways]
public class MatUpdater : MonoBehaviour
{
	[SerializeField]
	private Material material;
	[SerializeField]
	private Transform spray;
	[SerializeField]
	private MeshFilter meshFilter;//quad
	[SerializeField]
	private Vector3[] positions;
	private Vector4[] poss = new Vector4[4];


	[ExecuteAlways]
	void Update()
	{
		positions = meshFilter.mesh.vertices;
		for (int i = 0; i < 4; i++)
		{
			positions[i].x *= meshFilter.transform.localScale.x;
			positions[i].y *= meshFilter.transform.localScale.y;
		}

		for (int i = 0; i < 4; i++)
		{
			//offset
			var pos = positions[i] - spray.position + meshFilter.transform.position;
			var eu = spray.eulerAngles / 180f * 3.14159265358979f;

			//rotate y
			var a = Mathf.Atan2(pos.x, pos.z) - eu.y;
			var l = new Vector2(pos.x, pos.z).magnitude;
			pos.x = Mathf.Sin(a) * l;
			pos.z = Mathf.Cos(a) * l;

			//rotate x
			a = Mathf.Atan2(pos.y, pos.z) + eu.x;
			l = new Vector2(pos.y, pos.z).magnitude;
			pos.y = Mathf.Sin(a) * l;
			pos.z = Mathf.Cos(a) * l;

			//rotate z to random
			a = Mathf.Atan2(pos.y, pos.x);// + Random.value * 6.28318530718f;
			l = new Vector2(pos.y, pos.x).magnitude;
			pos.y = Mathf.Sin(a) * l;
			pos.x = Mathf.Cos(a) * l;
			pos /= 2;
			poss[i] = pos;
		}

		material.SetVectorArray("_Corners", poss);

		material.SetInt("_Count", 1);
	}
}
