using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Spray : UdonSharpBehaviour
{
	[UdonSynced] private Color color;
	[UdonSynced] public bool works = false;
	[SerializeField] private GameObject colorPicker;
	[SerializeField] private Material colorPickerMaterial;
	[SerializeField] private GameObject colorPick;
	[SerializeField] private Material colorPickMaterial;
	[SerializeField] private VRCPickup pickup;

	public Vector3 pos => pickup.transform.position;
	public Vector2 rot => pickup.transform.eulerAngles / 180 * 3.14159265358979f;

	private ColorPickerMode mode;
	private VRCPlayerApi localPlayer;

	private void Start()
	{
		localPlayer = Networking.LocalPlayer;
		pickup.AutoHold = localPlayer.IsUserInVR() ? VRC_Pickup.AutoHoldMode.No : VRC_Pickup.AutoHoldMode.Yes;
	}

	private void Update()
	{
		if (!localPlayer.IsUserInVR())
			return;

		if (pickup.transform.position.z > 0)
		{
			works = false;
			return;
		}

		if (colorPicker.activeSelf)
		{
			colorPick.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
			//clamp local position of 0 - 1
			colorPick.transform.localPosition =
				new Vector3(
					Mathf.Clamp(colorPick.transform.localPosition.x, 0, 1),
					Mathf.Clamp(colorPick.transform.localPosition.y, 0, 1),
					Mathf.Clamp(colorPick.transform.localPosition.z, 0, 1)
				);
			var color = new Color(colorPick.transform.localPosition.x, colorPick.transform.localPosition.y, colorPick.transform.localPosition.z, 1).gamma;
			colorPickMaterial.SetColor("_MyCol", color);
			if (mode == ColorPickerMode.Select)
			{
				this.color = color;
			}
			works = false;
			return;
		}

		if (!pickup.IsHeld)
		{
			works = false;
			return;
		}

		if (localPlayer != pickup.currentPlayer)
		{
			if (Networking.IsOwner(gameObject))
				Networking.SetOwner(pickup.currentPlayer, gameObject);
			return;
		}

		var power = Input.GetAxis(pickup.currentHand == VRC_Pickup.PickupHand.Right ? "Oculus_CrossPlatform_SecondaryIndexTrigger" : "Oculus_CrossPlatform_PrimaryIndexTrigger") / 2;

		if (power == 0)
		{
			works = false;
			return;
		}

		works = true;
		color.a = power;
		RequestSerialization();
	}

	public override void OnDeserialization()
	{
		if (color.a == 0)
		{
			color.a = 1;
			works = false;
		}
		works = true;
	}

	public override void InputLookVertical(float value, UdonInputEventArgs args)
	{
		if (!localPlayer.IsUserInVR())
		{
			base.InputLookVertical(value, args);
			return;
		}

		if (!pickup.IsHeld)
		{
			base.InputLookVertical(value, args);
			return;
		}

		if (localPlayer != pickup.currentPlayer)
		{
			base.InputLookVertical(value, args);
			return;
		}

		if (value > 0.8)
		{
			if (colorPicker.activeSelf) return;
			colorPicker.SetActive(true);

			colorPicker.transform.position = pickup.transform.position + pickup.transform.forward * 0.15f;
			colorPicker.transform.eulerAngles = new Vector3(0, pickup.transform.eulerAngles.y, 0);
			colorPick.transform.localPosition = new Vector3(color.r, color.g, color.b);
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
