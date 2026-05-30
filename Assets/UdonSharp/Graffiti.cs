using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class Graffiti : UdonSharpBehaviour
{
	[SerializeField] private GameObject colorPicker;
	[SerializeField] private GameObject grainChanger;
	[SerializeField] private Material material;
	[SerializeField] private Material alphaClearMaterial;
	[SerializeField] private Spray[] sprays;
	[SerializeField] private ReadRenderTexture RTSync;
	private Vector4[] positions;
	private Vector4[] rotations;

	private Vector4[] skip;

	private Color[] colors;
	[SerializeField] private MeshRenderer meshRenderer;
	private bool contOneFrame = true;
	private void Start()
	{
		var cam = VRCCameraSettings.ScreenCamera;
		var layers = cam.CullingMask;
		layers.value &= ~(1 << LayerMask.NameToLayer("Default"));
		cam.CullingMask = layers;

		positions = new Vector4[sprays.Length];
		rotations = new Vector4[sprays.Length];
		colors = new Color[sprays.Length];

		skip = new Vector4[sprays.Length];

		for (int i = 0; i < sprays.Length; i++)
		{
			skip[i] = new Vector4(0, 0, 1, 0);
		}
	}

	private void LateUpdate()
	{
		/*if (RTSync.Syncing)
		{
			material.SetVectorArray("_Pos", skip);
		}*/

		if (contOneFrame)
		{
			contOneFrame = false;
			return;
		}
		meshRenderer.enabled = false;

		int currentRight = -1;
		int currentLeft = -1;

		alphaClearMaterial.SetVectorArray("_Pos", positions);
		alphaClearMaterial.SetVectorArray("_Rot", rotations);

		for (int i = 0; i < sprays.Length; i++)
		{
			var s = sprays[i];
			positions[i] = s.pos;
			positions[i].w = Random.value;
			rotations[i] = s.rot;
			rotations[i].w = Random.value;
			colors[i] = s.col;
			if (s.isCurrentRight) currentRight = i;
			if (s.isCurrentLeft) currentLeft = i;
		}
		if (currentRight == -1)
		{
			colorPicker.SetActive(false);
			grainChanger.SetActive(false);
		}

		material.SetVectorArray("_Pos", positions);
		material.SetVectorArray("_Rot", rotations);
		material.SetColorArray("_Col", colors);
		material.SetVector("_Grain1", new Vector4(sprays[0].gr, sprays[1].gr, sprays[2].gr, sprays[3].gr) * 2);
		material.SetVector("_Grain2", new Vector4(sprays[4].gr, sprays[5].gr, sprays[6].gr, sprays[7].gr) * 2);
		material.SetInt("_CurrentR", currentRight);
		material.SetInt("_CurrentL", currentLeft);
	}
}
