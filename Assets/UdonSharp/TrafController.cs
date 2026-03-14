
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class TrafController : UdonSharpBehaviour
{
	[UdonSynced] private int countVal = 3;
	[UdonSynced] private float roundVal = 0.5f;
	[UdonSynced] private float scaleVal = 1f;
	[UdonSynced] private bool reverse = false;

	[SerializeField] private VRC_Pickup pickup;
	[SerializeField] private GameObject UI;
	[SerializeField] private Transform traf;
	[SerializeField] private Material material;
	[SerializeField] private Slider vert;
	[SerializeField] private Slider round;
	[SerializeField] private Slider scale;

	private VRCPlayerApi localPlayer;
	private Vector4[] vertices = new Vector4[5];

	private void Start()
	{
		localPlayer = Networking.LocalPlayer;
		UI.SetActive(false);
		vert.value = countVal;
		round.value = roundVal;
		scale.value = scaleVal;

		ChangeVertices((int)vert.value);
		ChangeRound(round.value);
		ChangeScale(scale.value);
		material.SetInt("_Reverse", reverse ? 1 : 0);
	}

	private void Update()
	{
		if (!pickup.IsHeld)
		{
			UI.SetActive(false);
			return;
		}

		if (pickup.currentPlayer != localPlayer)
		{
			Networking.SetOwner(pickup.currentPlayer, gameObject);
			UI.SetActive(false);
			return;
		}

		UI.SetActive(true);

		ChangeVertices((int)vert.value);
		ChangeRound(round.value);
		ChangeScale(scale.value);
		material.SetInt("_Reverse", reverse ? 1 : 0);
	}

	public void ChangeVertices(int count)
	{
		countVal = count;
		float tau = Mathf.PI * 2f;
		float step = tau / count;

		for (int i = 0; i < count; i++)
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

		material.SetInt("_VertCount", count);
		material.SetVectorArray("_Vertices", vertices);
	}

	public void ChangeRound(float value)
	{
		roundVal = value;
		material.SetFloat("_Round", value);
	}

	public void ChangeScale(float value)
	{
		scaleVal = value;
		traf.localScale = Vector3.one * value;
	}

	public override void OnDeserialization()
	{
		vert.value = countVal;
		round.value = roundVal;
		scale.value = scaleVal;

		ChangeVertices(countVal);
		ChangeRound(roundVal);
		ChangeScale(scaleVal);
		material.SetInt("_Reverse", reverse ? 1 : 0);
	}

	public void Reverse()
	{
		reverse = !reverse;
	}
}
