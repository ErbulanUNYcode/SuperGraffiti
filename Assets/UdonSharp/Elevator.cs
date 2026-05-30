using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Elevator : UdonSharpBehaviour
{
	private VRCPlayerApi localPlayer;
	private float offset;
	[SerializeField] private Transform saveSphere;
	[SerializeField] private MeshRenderer meshRenderer;
	private void Start()
	{
		localPlayer = Networking.LocalPlayer;
	}

	public override void OnPlayerRespawn(VRCPlayerApi player)
	{
		transform.position = Vector3.up * 20;
	}

	public override void PostLateUpdate()
	{
		saveSphere.gameObject.SetActive(VRCCameraSettings.PhotoCamera != null && VRCCameraSettings.PhotoCamera.Active);
		if (saveSphere.gameObject.activeSelf && (VRCCameraSettings.PhotoCamera.Position - saveSphere.position).magnitude > saveSphere.localScale.x / 2)
		{
			var sp = saveSphere.position;
			sp.y = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position.y;
			saveSphere.position = sp;
		}
		if (offset == 0) return;
		var p2 = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
		if (localPlayer.GetPosition().y <= 20 && offset <= p2.y)
		{
			return;
		}
		var p = transform.position;
		p.y += offset - p2.y;
		p.y = Mathf.Max(p.y, 20);
		transform.position = p;
		var td = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
		p = td.position;
		p.y += offset - p2.y;
		localPlayer.TeleportTo(
			p,
			td.rotation,
			VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint
		);
	}

	public override void InputJump(bool value, UdonInputEventArgs args)
	{
		if (!localPlayer.IsPlayerGrounded()) return;

		if (value)
		{
			offset = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position.y;
			return;
		}

		offset = 0;
	}
}
