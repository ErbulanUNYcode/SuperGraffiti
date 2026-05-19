using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Spray : UdonSharpBehaviour
{
	[UdonSynced] private Color color = Color.white;
	[SerializeField] private GameObject colorPicker;
	[SerializeField] private Material colorPickerMaterial;
	[SerializeField] private Material colorNeonMaterial;
	[SerializeField] private GameObject colorPick;
	[SerializeField] private Material colorPickMaterial;
	[SerializeField] private Material sprayMaterial;
	[SerializeField] private Transform colorPalette;
	[SerializeField] private GameObject paletteExem;
	[SerializeField] private GameObject paletteExemBlack;
	[UdonSynced] private float grainMode = 0;
	[SerializeField] private GameObject grainChanger;
	[SerializeField] private GameObject grain;
	[SerializeField] private SpriteRenderer grainView;
	[SerializeField] private Sprite gradient;
	[SerializeField] private Sprite stars;
	[SerializeField] private Sprite circles;
	[SerializeField] private VRCPickup pickup;
	[SerializeField] private Transform trafaretCam;
	[SerializeField] private Transform sprayHead;

	public Vector3 pos => trafaretCam.transform.position + Vector3.down * 20 - trafaretCam.transform.forward * 0.07f;
	public Vector3 rot => trafaretCam.eulerAngles / 180 * 3.14159265358979f;
	public Color col => color;
	public float gr => grainMode;
	public bool isCurrentRight => pickup.IsHeld && pickup.currentPlayer == localPlayer && pickup.currentHand == VRC_Pickup.PickupHand.Right;
	public bool isCurrentLeft => pickup.IsHeld && pickup.currentPlayer == localPlayer && pickup.currentHand == VRC_Pickup.PickupHand.Left;

	private VRCPlayerApi localPlayer;
	[SerializeField]
	private Ruler currentRuler;

	private void Start()
	{
		color = sprayMaterial.GetColor("_Color");
		localPlayer = Networking.LocalPlayer;
		if (!localPlayer.IsUserInVR()) pickup.enabled = false;
	}

	private void Update()
	{
		if (color.a == 0 && currentRuler != null) currentRuler.SetPosition(pickup.transform.position + pickup.transform.forward * 0.07f);

		trafaretCam.position = currentRuler != null ? currentRuler.GetPosition(pickup.transform.position + pickup.transform.forward * 0.07f) : pickup.transform.position + pickup.transform.forward * 0.07f;
		sprayHead.position = trafaretCam.position;
		var rot = pickup.transform.eulerAngles;
		rot.z = Random.Range(0, 360);
		trafaretCam.eulerAngles = rot;
		pickup.AutoHold = Networking.LocalPlayer.IsUserInVR() ? VRC_Pickup.AutoHoldMode.No : VRC_Pickup.AutoHoldMode.Yes;

		if (!pickup.IsHeld)
		{
			if (color.a != 0 && Networking.IsOwner(gameObject))
			{
				color.a = 0;
				RequestSerialization();
			}
			//pickup.enabled = true;
			return;
		}

		if (localPlayer != pickup.currentPlayer)
		{
			if (Networking.IsOwner(gameObject))
				Networking.SetOwner(pickup.currentPlayer, gameObject);
			return;
		}


		if (pickup.transform.position.z > 0)
		{
			return;
		}

		if (pickup.currentHand == VRC_Pickup.PickupHand.Right)
		{
			if (colorPicker.activeSelf)
			{
				var pos = pickup.transform.position + pickup.transform.forward * 0.15f;

				if (pos != Vector3.zero)

					colorPick.transform.position = pos;

				colorPick.transform.localPosition = Snap(colorPick.transform.localPosition);

				for (int i = 0; i < colorPalette.childCount; i++)
				{
					if ((colorPalette.GetChild(i).localPosition - colorPick.transform.localPosition).magnitude < 0.05f) colorPick.transform.position = colorPalette.GetChild(i).position;
				}
				pos = colorPick.transform.localPosition;
				if (pos.x > 1.1f || pos.y > 1.1f || pos.z > 1.1f) pos = pos / 1.2f + Vector3.one;
				color = new Color(pos.x, pos.y, pos.z, 0).gamma;
				colorPickMaterial.SetColor("_MyCol", color);
				sprayMaterial.SetColor("_Color", color);
				return;
			}

			if (grainChanger.activeSelf)
			{
				grain.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
				grain.transform.localPosition = new Vector3(Mathf.Abs(grain.transform.localPosition.x) > 0.15 ? Mathf.Clamp(grain.transform.localPosition.x, -0.75f, 0.75f) : 0, 0, 0);
				grainMode = grain.transform.localPosition.x;
				grainMode = Mathf.Max(0, Mathf.Abs(grainMode) - 0.15f) * (grainMode > 0 ? 4 : -4);
				grainView.sprite = grainMode < 0 ? stars : grainMode > 0 ? circles : gradient;
				grain.transform.localScale = Vector3.one * (grainMode == 0 ? 0.1f : Mathf.Abs(grainMode / 25));
				return;
			}
		}

		var power = localPlayer.IsUserInVR() ? Input.GetAxis(pickup.currentHand == VRC_Pickup.PickupHand.Right ? "Oculus_CrossPlatform_SecondaryIndexTrigger" : "Oculus_CrossPlatform_PrimaryIndexTrigger") : Input.GetMouseButton(0) ? 0.5f : 0;

		color.a = power * power;
		RequestSerialization();
	}

	public override void OnDeserialization()
	{
		colorPickMaterial.SetColor("_MyCol", color);
		sprayMaterial.SetColor("_Color", color);
	}

	public override void InputUse(bool value, UdonInputEventArgs args)
	{
		if (colorPicker.activeSelf && pickup.IsHeld && pickup.currentHand == VRC_Pickup.PickupHand.Right && pickup.currentPlayer == localPlayer && value && args.handType == HandType.RIGHT)
		{
			for (int i = 0; i < colorPalette.childCount; i++)
			{
				var ch = colorPalette.GetChild(i);
				if (ch.position == colorPick.transform.position)
				{
					Destroy(ch.gameObject);
					return;
				}
			}

			var isNeon = colorPick.transform.localPosition.x > 1.1f || colorPick.transform.localPosition.y > 1.1f || colorPick.transform.localPosition.z > 1.1f;
			var t = Instantiate(isNeon ? paletteExemBlack : paletteExem, colorPalette).transform;
			t.localScale *= 0.4f;
			t.localPosition = colorPick.transform.localPosition;
		}
	}

	public override void InputLookVertical(float value, UdonInputEventArgs args)
	{
		if (!localPlayer.IsUserInVR()) return;

		if (args.handType == HandType.LEFT) return;

		if (!pickup.IsHeld) return;

		if (localPlayer != pickup.currentPlayer) return;

		if (pickup.currentHand == VRC_Pickup.PickupHand.Left) return;


		if (value > 0.8)
		{
			if (colorPicker.activeSelf) return;
			grainChanger.SetActive(false);
			colorPicker.SetActive(true);

			colorPicker.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
			colorPicker.transform.eulerAngles = new Vector3(0, pickup.transform.eulerAngles.y, 0);
			var pos = new Vector3(color.linear.r, color.linear.g, color.linear.b);
			if (pos.x > 1.1f || pos.y > 1.1f || pos.z > 1.1) pos = (pos - Vector3.one) * 1.2f;
			colorPick.transform.localPosition = pos;
			colorPicker.transform.position += colorPicker.transform.position - colorPick.transform.position;
			pos = colorPicker.transform.position;
			colorPickerMaterial.SetVector("_MyPos", new Vector4(pos.x, pos.y, pos.z, colorPicker.transform.rotation.eulerAngles.y / 180f * 3.14159265358979f));
			colorNeonMaterial.SetVector("_MyPos", new Vector4(pos.x, pos.y, pos.z, colorPicker.transform.rotation.eulerAngles.y / 180f * 3.14159265358979f));
		}
		else if (value < -0.8)
		{
			if (grainChanger.activeSelf) return;
			colorPicker.SetActive(false);
			grainChanger.SetActive(true);
			var c = color;
			c.a = 1;
			grainView.color = c;
			grainChanger.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
			grainChanger.transform.eulerAngles = new Vector3(0, pickup.transform.eulerAngles.y, 0);
			grain.transform.localPosition = new Vector3(grainMode == 0f ? 0f : (Mathf.Abs(grainMode) / 4f + 0.15f) * Mathf.Sign(grainMode), 0f, 0f);
			grainChanger.transform.position += grainChanger.transform.position - grain.transform.position;
		}
		else if (value > -0.2 && value < 0.2)
		{
			if (colorPicker.activeSelf)
				colorPicker.SetActive(false);
			if (grainChanger.activeSelf)
				grainChanger.SetActive(false);
		}
	}

	Vector3 Snap(Vector3 v)
	{
		float best = float.MaxValue;
		Vector3 b = v;

		float d;
		Vector3 c;

		c = new Vector3(0f, 1.2f, Mathf.Clamp(v.z, 0f, 1.2f));
		d = (v - c).sqrMagnitude;
		if (d < best) { best = d; b = c; }

		c = new Vector3(0f, Mathf.Clamp(v.y, 0f, 1.2f), 1.2f);
		d = (v - c).sqrMagnitude;
		if (d < best) { best = d; b = c; }

		c = new Vector3(1.2f, 0f, Mathf.Clamp(v.z, 0f, 1.2f));
		d = (v - c).sqrMagnitude;
		if (d < best) { best = d; b = c; }

		c = new Vector3(Mathf.Clamp(v.x, 0f, 1.2f), 0f, 1.2f);
		d = (v - c).sqrMagnitude;
		if (d < best) { best = d; b = c; }

		c = new Vector3(1.2f, Mathf.Clamp(v.y, 0f, 1.2f), 0f);
		d = (v - c).sqrMagnitude;
		if (d < best) { best = d; b = c; }

		c = new Vector3(Mathf.Clamp(v.x, 0f, 1.2f), 1.2f, 0f);
		d = (v - c).sqrMagnitude;
		if (d < best) { best = d; b = c; }

		// cube
		c = new Vector3(
			Mathf.Clamp(v.x, 0f, 1f),
			Mathf.Clamp(v.y, 0f, 1f),
			Mathf.Clamp(v.z, 0f, 1f)
		);
		d = (v - c).sqrMagnitude;
		if (d < best) { best = d; b = c; }

		return b;
	}
}