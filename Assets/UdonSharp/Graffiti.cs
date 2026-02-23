
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class Graffiti : UdonSharpBehaviour
{
	[SerializeField] private Material material;
	[SerializeField] private Spray[] sprays;
	[SerializeField] private MeshFilter meshFilter;//quad
	private Vector4[] matData;

	private void Start()
	{
		matData = new Vector4[sprays.Length * 4];
	}
	private void Update()
	{
		var count = 0;

		foreach (var spray in sprays)
		{
			if (!spray.works) continue;

			var poss = meshFilter.mesh.vertices;//quad has 4 vertices
			var random = Random.value * 6.28318530718f;
			for (int i = 0; i < 4; i++)
			{
				poss[i].x *= meshFilter.transform.localScale.x;
				poss[i].y *= meshFilter.transform.localScale.y;
				//offset
				var pos = poss[i] - spray.pos + meshFilter.transform.position;
				var eu = spray.rot;
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
				a = Mathf.Atan2(pos.y, pos.x) + random;
				l = new Vector2(pos.y, pos.x).magnitude;
				pos.y = Mathf.Sin(a) * l;
				pos.x = Mathf.Cos(a) * l;
				pos /= 2;
				matData[count * 4 + i] = new Vector4(pos.x, pos.y, pos.z, 0);
			}

			count++;
		}

		material.SetInt("_Count", count);
		material.SetVectorArray("_Corners", matData);
	}
}
