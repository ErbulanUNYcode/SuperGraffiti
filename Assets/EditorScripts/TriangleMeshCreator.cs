#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TriangleMeshCreator : MonoBehaviour
{
	[SerializeField] private int id = 0;
	public void CreateTriangleMesh()
	{
		Mesh mesh = new Mesh();
		mesh.name = "TriangleMesh";

		Color32[] colors = new Color32[32];
		for (int i = 0; i < colors.Length; i++) colors[i] = new Color32((byte)id, 0, 0, 0);

		Vector3[] vertices = new Vector3[]
		{
			new Vector3(0,1,4),
			new Vector3(0.19509032f,0.98078525f,4),
			new Vector3(0.38268346f,0.9238795f,4),
			new Vector3(0.55557024f,0.8314696f,4),
			new Vector3(0.70710677f,0.70710677f,4),
			new Vector3(0.83146966f,0.5555702f,4),
			new Vector3(0.9238795f,0.38268343f,4),
			new Vector3(0.9807853f,0.19509023f,4),
			new Vector3(1,0,4),
			new Vector3(0.98078525f,-0.19509032f,4),
			new Vector3(0.9238795f,-0.38268352f,4),
			new Vector3(0.83146954f,-0.55557036f,4),
			new Vector3(0.70710677f,-0.70710677f,4),
			new Vector3(0.5555702f,-0.83146966f,4),
			new Vector3(0.38268328f,-0.9238796f,4),
			new Vector3(0.19509031f,-0.9807853f,4),
			new Vector3(0,-1,4),
			new Vector3(-0.19509049f,-0.98078525f,4),
			new Vector3(-0.38268343f,-0.9238795f,4),
			new Vector3(-0.5555703f,-0.83146954f,4),
			new Vector3(-0.7071069f,-0.70710665f,4),
			new Vector3(-0.8314698f,-0.55557f,4),
			new Vector3(-0.9238797f,-0.38268313f,4),
			new Vector3(-0.98078525f,-0.19509038f,4),
			new Vector3(-1,0,4),
			new Vector3(-0.98078525f,0.19509041f,4),
			new Vector3(-0.92387944f,0.3826836f,4),
			new Vector3(-0.8314695f,0.5555704f,4),
			new Vector3(-0.70710653f,0.707107f,4),
			new Vector3(-0.5555703f,0.8314696f,4),
			new Vector3(-0.38268343f,0.92387956f,4),
			new Vector3(-0.19509023f,0.9807853f,4)
		};

		int[] triangles = new int[]
		{
			31,0,1,
			31,1,2,
			30,2,3,
			29,3,4,
			28,4,5,
			27,5,6,
			26,6,7,
			25,7,8,
			24,8,9,
			23,9,10,
			22,10,11,
			21,11,12,
			20,12,13,
			19,13,14,
			18,14,15,
			17,15,16,
			30,31,2,
			29,30,3,
			28,29,4,
			27,28,5,
			26,27,6,
			25,26,7,
			24,25,8,
			23,24,9,
			22,23,10,
			21,22,11,
			20,21,12,
			19,20,13,
			18,19,14,
			17,18,15
		};

		mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();

		string path = "Assets/CircleMesh (" + id + ").asset";
		AssetDatabase.CreateAsset(mesh, path);
		AssetDatabase.SaveAssets();

		Debug.Log("Triangle mesh saved at " + path);
	}
}

//cutom inspector with button "CreateTriangleMesh"

[CustomEditor(typeof(TriangleMeshCreator))]
public class TriangleMeshCreatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		TriangleMeshCreator myScript = (TriangleMeshCreator)target;
		if (GUILayout.Button("Create Triangle Mesh"))
		{
			myScript.CreateTriangleMesh();
		}
	}
}
#endif