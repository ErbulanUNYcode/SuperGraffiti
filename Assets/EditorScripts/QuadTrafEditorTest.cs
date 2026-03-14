using UnityEngine;

[ExecuteAlways]
public class QuadTrafEditorTest : MonoBehaviour
{
	[SerializeField] Material targetMaterial;

	[Range(3, 10)]
	[SerializeField] int vertCount = 5;

	int vertCountPrev = -1;

	// 5 float4 = 10 вершин
	static readonly Vector4[] vertices = new Vector4[5];

	void OnValidate()
	{
		UpdateData();
	}

	void UpdateData()
	{
		if (targetMaterial == null) return;
		if (vertCountPrev == vertCount) return;

		vertCountPrev = vertCount;

		float tau = Mathf.PI * 2f;
		float step = tau / vertCount;

		for (int i = 0; i < vertCount; i++)
		{
			float a = step * (0.5f + i);

			float x = Mathf.Sin(a);
			float y = Mathf.Cos(a);

			int v = i >> 1;

			if ((i & 1) == 0)
			{
				vertices[v].x = x;
				vertices[v].y = y;
			}
			else
			{
				vertices[v].z = x;
				vertices[v].w = y;
			}
		}

		targetMaterial.SetInt("_VertCount", vertCount);
		targetMaterial.SetVectorArray("_Vertices", vertices);
	}
}