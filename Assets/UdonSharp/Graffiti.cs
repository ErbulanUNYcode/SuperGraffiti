
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class Graffiti : UdonSharpBehaviour
{
	[SerializeField] private GameObject colorPicker;
	[SerializeField] private Material material;
	[SerializeField] private Material alphaClear;
	[SerializeField] private Spray[] sprays;
	private Vector4[] positions;
	private Vector4[] rotations;
	private Color[] colors;
	[SerializeField] private MeshRenderer renderer;
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
	}

	private void LateUpdate()
	{

		if (contOneFrame)
		{
			contOneFrame = false;
			return;
		}
		renderer.material = alphaClear;

		int currentRight = -1;
		int currentLeft = -1;
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
		if (currentRight == -1) colorPicker.SetActive(false);
		material.SetVectorArray("_Pos", positions);
		material.SetVectorArray("_Rot", rotations);
		material.SetColorArray("_Col", colors);
		material.SetInt("_CurrentR", currentRight);
		material.SetInt("_CurrentL", currentLeft);
	}
}
