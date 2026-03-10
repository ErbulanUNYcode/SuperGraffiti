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
	[SerializeField] private GameObject colorPick;
	[SerializeField] private Material colorPickMaterial;
	[SerializeField] private VRCPickup pickup;
	[SerializeField] private Transform trafaretCam;
	[SerializeField] private int grain;

	public Vector3 pos => pickup.transform.position;
	public Vector3 rot => trafaretCam.eulerAngles / 180 * 3.14159265358979f;
	public Color col => color;
	public float gr => grain;
	public bool isCurrentRight => pickup.IsHeld && pickup.currentPlayer == localPlayer && pickup.currentHand == VRC_Pickup.PickupHand.Right;
	public bool isCurrentLeft => pickup.IsHeld && pickup.currentPlayer == localPlayer && pickup.currentHand == VRC_Pickup.PickupHand.Left;

	private ColorPickerMode mode;
	private VRCPlayerApi localPlayer;

	private void Start()
	{
		localPlayer = Networking.LocalPlayer;
		if (!localPlayer.IsUserInVR()) pickup.enabled = false;
	}

	private void LateUpdate()
	{
		trafaretCam.position = pickup.transform.position + pickup.transform.forward * 0.07f;
		var rot = pickup.transform.eulerAngles;
		rot.z = Random.Range(0, 360);
		trafaretCam.eulerAngles = rot;
		pickup.AutoHold = Networking.LocalPlayer.IsUserInVR() ? VRC_Pickup.AutoHoldMode.No : VRC_Pickup.AutoHoldMode.Yes;
	}

	public void Update()
	{
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

		if (colorPicker.activeSelf && pickup.currentHand == VRC_Pickup.PickupHand.Right)
		{
			colorPick.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
			colorPick.transform.localPosition =
				new Vector3(
					Mathf.Clamp(colorPick.transform.localPosition.x, 0, 1),
					Mathf.Clamp(colorPick.transform.localPosition.y, 0, 1),
					Mathf.Clamp(colorPick.transform.localPosition.z, 0, 1)
				);
			if (mode == ColorPickerMode.Select)
			{
				color = new Color(colorPick.transform.localPosition.x, colorPick.transform.localPosition.y, colorPick.transform.localPosition.z, 0).gamma;
				colorPickMaterial.SetColor("_MyCol", color);
			}
			return;
		}

		var power = localPlayer.IsUserInVR() ? Input.GetAxis(pickup.currentHand == VRC_Pickup.PickupHand.Right ? "Oculus_CrossPlatform_SecondaryIndexTrigger" : "Oculus_CrossPlatform_PrimaryIndexTrigger") : Input.GetMouseButton(0) ? 0.5f : 0;

		color.a = power * power;
		RequestSerialization();
	}

	public override void OnDeserialization()
	{
		colorPickMaterial.SetColor("_MyCol", color);
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
			colorPicker.SetActive(true);

			colorPicker.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
			colorPicker.transform.eulerAngles = new Vector3(0, pickup.transform.eulerAngles.y, 0);
			colorPick.transform.localPosition = new Vector3(color.linear.r, color.linear.g, color.linear.b);
			colorPicker.transform.position += colorPicker.transform.position - colorPick.transform.position;
			var pos = colorPicker.transform.position;
			colorPickerMaterial.SetVector("_MyPos", new Vector4(pos.x, pos.y, pos.z, colorPicker.transform.rotation.eulerAngles.y / 180f * 3.14159265358979f));
			mode = ColorPickerMode.Select;
		}
		else if (value < -0.8)
		{
			if (colorPicker.activeSelf) return;
			colorPicker.SetActive(true);

			colorPicker.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
			colorPicker.transform.eulerAngles = new Vector3(0, pickup.transform.eulerAngles.y, 0);
			colorPicker.transform.position += colorPicker.transform.position - colorPick.transform.position;
			var pos = colorPicker.transform.position;
			colorPickerMaterial.SetVector("_MyPos", new Vector4(pos.x, pos.y, pos.z, colorPicker.transform.rotation.eulerAngles.y / 180f * 3.14159265358979f));
			mode = ColorPickerMode.SaveList;
		}
		else if (value > -0.2 && value < 0.2)
		{
			if (!colorPicker.activeSelf) return;
			colorPicker.SetActive(false);
		}
	}
}

public enum ColorPickerMode
{
	Select,
	SaveList
}
