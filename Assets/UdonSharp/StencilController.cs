using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class StencilController : UdonSharpBehaviour
{
	[UdonSynced] private int countVal;
	[UdonSynced] private float foldVal;
	[UdonSynced] private float scaleVal;
	[SerializeField]
	[UdonSynced] private bool reverce = false;
	[SerializeField]
	[UdonSynced] private bool mirror = true;
	[SerializeField]
	[UdonSynced] private bool collide = false;

	[SerializeField]
	[UdonSynced] private Vector2 point1Val;
	[SerializeField]
	[UdonSynced] private Vector2 point2Val;

	[SerializeField] private VRC_Pickup pickup;
	[SerializeField] private Canvas UI;
	[SerializeField] private Transform traf;
	[SerializeField] private Material material;
	[SerializeField] private Slider vert;
	[SerializeField] private Slider fold;
	[SerializeField] private Slider scale;
	private Slider[] sliders;
	[SerializeField] private Coursor coursor;
	[SerializeField] private RectTransform point1;
	[SerializeField] private RectTransform point2;
	[SerializeField] private LineRenderer lowCurveView;
	[SerializeField] private LineRenderer highCurveView;
	private Vector3[] highCurve = new Vector3[17];

	[SerializeField] private MeshFilter meshFilter;

	private VRCPlayerApi localPlayer;

	private void Start()
	{
		sliders = new Slider[] { vert, fold, scale };
		localPlayer = Networking.LocalPlayer;
		UI.gameObject.SetActive(false);
		countVal = (int)vert.value;
		foldVal = fold.value;
		scaleVal = scale.value;

		ChangeVertices((int)vert.value);
		ChangFold(fold.value);
		ChangeScale(scale.value);
		material.SetInt("_Reverse", reverce ? 1 : 0);
		material.SetInt("_Mirror", mirror ? 1 : 0);
		material.SetInt("_Collide", collide ? 1 : 0);
	}

	private void Update()
	{

		if (!pickup.IsHeld)
		{
			UI.gameObject.SetActive(false);
			return;
		}

		if (pickup.currentPlayer != localPlayer)
		{
			Networking.SetOwner(pickup.currentPlayer, gameObject);
			UI.gameObject.SetActive(false);
			return;
		}
		bool hand = pickup.currentHand == VRC_Pickup.PickupHand.Right;
		var handPos = hand ? localPlayer.GetBonePosition(HumanBodyBones.LeftHand) : localPlayer.GetBonePosition(HumanBodyBones.RightHand);
		var handRot = hand ? localPlayer.GetBoneRotation(HumanBodyBones.LeftHand) : localPlayer.GetBoneRotation(HumanBodyBones.RightHand);

		var press = Input.GetAxis(hand ? "Oculus_CrossPlatform_PrimaryIndexTrigger" : "Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.7;

		var pos = coursor.GetPos(UI.GetComponent<Canvas>(), handPos, handRot);

		UI.gameObject.SetActive(true);

		SliderControl(pos, press);
		ButtonControl(pos, press);
		CurveControl(pos, press);

		ChangeVertices((int)vert.value);
		ChangFold(fold.value);
		ChangeScale(scale.value);
		material.SetInt("_Reverse", reverce ? 1 : 0);
		material.SetInt("_Mirror", mirror ? 1 : 0);
		material.SetInt("_Collide", collide ? 1 : 0);
	}

	public void ChangeVertices(int count)
	{
		countVal = count;
		material.SetInt("_Count", count);
	}

	public void ChangFold(float value)
	{
		foldVal = value;
		material.SetFloat("_Fold", value);
	}

	public void ChangeScale(float value)
	{
		scaleVal = value;
		traf.localScale = Vector3.one * value;
	}

	public override void OnDeserialization()
	{
		vert.value = countVal;
		fold.value = foldVal;
		scale.value = scaleVal;

		ChangeVertices(countVal);
		ChangFold(foldVal);
		ChangeScale(scaleVal);
		material.SetInt("_Reverse", reverce ? 1 : 0);
		material.SetInt("_Mirror", mirror ? 1 : 0);
		material.SetInt("_Collide", collide ? 1 : 0);
		UpdateCurve();
	}

	public void Reverse()
	{
		reverce = !reverce;
	}

	public void Mirror()
	{
		mirror = !mirror;
	}

	public void Collide()
	{
		collide = !collide;
	}

	Slider activeSlider = null;
	float offset;

	/// <param name="pointerPos">The position of the pointer in canvas space</param>
	/// <param name="isPressed"></param>
	private void SliderControl(Vector2 pointerPos, bool isPressed)
	{
		if (!isPressed)
		{
			activeSlider = null;
			return;
		}


		if (activeSlider == null && !activeButton && !activePoint)
		{
			for (int i = 0; i < sliders.Length; i++)
			{
				var slider = sliders[i];
				Vector2 handlePos = slider.handleRect.localPosition;
				handlePos.x += 90;
				handlePos.y = -20 * i;
				var handleSize = 20;

				if (pointerPos.x >= handlePos.x && pointerPos.x <= handlePos.x + handleSize &&
					pointerPos.y <= handlePos.y && pointerPos.y >= handlePos.y - handleSize)
				{
					activeSlider = slider;
					offset = pointerPos.x - handlePos.x;
					break;
				}
			}
		}

		if (activeSlider != null)
		{
			var pos = pointerPos.x - offset;
			activeSlider.value = activeSlider.minValue + Mathf.Clamp01(pos / 180) * (activeSlider.maxValue - activeSlider.minValue);
		}
	}

	private bool activeButton = false;

	private void ButtonControl(Vector2 pointerPos, bool isPressed)
	{
		if (!isPressed)
		{
			activeButton = false;
			return;
		}

		if (!activeButton && activeSlider == null && !activePoint)
		{
			if (pointerPos.x > 15 && pointerPos.x < 65 && pointerPos.y < -60 && pointerPos.y > -110)
			{
				activeButton = true;
				Reverse();
			}
			else if (pointerPos.x > 75 && pointerPos.x < 125 && pointerPos.y < -60 && pointerPos.y > -110)
			{
				activeButton = true;
				Mirror();
			}
			else if (pointerPos.x > 135 && pointerPos.x < 185 && pointerPos.y < -60 && pointerPos.y > -110)
			{
				activeButton = true;
				Collide();
			}
		}
	}

	private RectTransform activePoint = null;
	private Vector2 pointOffset;

	private void CurveControl(Vector2 pointerPos, bool isPressed)
	{
		if (!isPressed)
		{
			activePoint = null;
			return;
		}

		pointerPos.x -= 25;

		if (activePoint == null && !activeButton && !activeSlider)
		{
			var pos = (Vector2)point1.localPosition + Vector2.one * 75;

			if ((pointerPos - pos).sqrMagnitude < 100)
			{
				activePoint = point1;
				pointOffset = pointerPos - pos;
			}
			else
			{
				pos = (Vector2)point2.localPosition + Vector2.one * 75;
				if ((pointerPos - pos).sqrMagnitude < 100)
				{
					activePoint = point2;
					pointOffset = pointerPos - pos;
				}
			}
		}

		if (activePoint != null)
		{
			if (activePoint == point1)
			{
				point1Val = (pointerPos + -pointOffset) / 150;

				//clamp 0-1
				point1Val.x = Mathf.Clamp01(point1Val.x);
				point1Val.y = Mathf.Clamp01(point1Val.y);
			}
			else
			{
				point2Val = (pointerPos + -pointOffset) / 150;

				//clamp 0-1
				point2Val.x = Mathf.Clamp01(point2Val.x);
				point2Val.y = Mathf.Clamp01(point2Val.y);
			}
			UpdateCurve();
		}
	}

	private void UpdateCurve()
	{
		material.SetVector("_Point", new Vector4(point1Val.x, point1Val.y, point2Val.x, point2Val.y));

		point1.localPosition = point1Val * 150 - Vector2.one * 75;
		point2.localPosition = point2Val * 150 - Vector2.one * 75;

		lowCurveView.SetPosition(1, point1Val);
		lowCurveView.SetPosition(2, point2Val);

		float t;
		Vector2 center12, point0Val, point3Val;

		for (int i = 0; i < highCurve.Length; i++)
		{
			t = i / (float)(highCurve.Length - 1);
			center12 = Vector2.Lerp(point1Val, point2Val, t);
			point0Val = Vector2.Lerp(Vector2.up, point1Val, t);
			point0Val = Vector2.Lerp(point0Val, center12, t);
			point3Val = Vector2.Lerp(point2Val, Vector2.right, t);
			point3Val = Vector2.Lerp(center12, point3Val, t);
			highCurve[i] = Vector2.Lerp(point0Val, point3Val, t);
		}

		highCurveView.SetPositions(highCurve);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		material.SetInt("_Reverse", reverce ? 1 : 0);
		material.SetInt("_Mirror", mirror ? 1 : 0);
		material.SetInt("_Collide", collide ? 1 : 0);
		material.SetInt("_Count", (int)vert.value);
		material.SetFloat("_Fold", fold.value);
		ChangeScale(scale.value);

		UpdateCurve();
	}
#endif
}
