using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class Classss : UdonSharpBehaviour
{
	//incline>0 наклон вперёд в радианах
	private float Rul(float incline)
	{
		var player = Networking.LocalPlayer;
		var head = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
		var leftHand = player.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
		var rightHand = player.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
		var vec = (rightHand + leftHand) / 2 - head;
		var a = Mathf.Atan2(vec.x, vec.z);
		var plane = rightHand - leftHand;
		a = Mathf.Atan2(plane.x, plane.z) - a;
		var d = new Vector2(plane.x, plane.z).magnitude;
		plane.x = Mathf.Sin(a) * d;
		plane.z = Mathf.Cos(a) * d;
		if (plane.y > 0)
			a = Mathf.Atan2(plane.z, plane.y);
		else
			a = Mathf.Atan2(-plane.z, -plane.y);
		d = new Vector2(plane.z, plane.y).magnitude;
		plane.y = Mathf.Cos(incline - a) * (plane.y > 0 ? d : -d);
		return Mathf.Atan2(-plane.y, plane.x);
	}
}
