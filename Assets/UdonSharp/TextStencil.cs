
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class TextStencil : UdonSharpBehaviour
{
	[SerializeField] private Slider sizeSlider;
	[SerializeField] private GameObject UI;
	[SerializeField] private VRC_Pickup pickup;
	[SerializeField] private Material material;
	[SerializeField] private Transform stencil;
	[SerializeField] private TextMeshProUGUI[] fonts;
	[SerializeField] private TMP_InputField inputField;

	[UdonSynced] private string text = "Text";
	[UdonSynced] private bool reverse = false;
	[UdonSynced] private float size = 1f;
	[UdonSynced] private int font = 0;

	private VRCPlayerApi localPlayer;

	private void Start()
	{
		localPlayer = Networking.LocalPlayer;
		UI.SetActive(false);
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

		//text = textMeshProUGUI.text;
		size = sizeSlider.value;
		stencil.localScale = new Vector3(0.4f, 0.1f, 1f) * size;

		text = inputField.text;
		foreach (var f in fonts) f.text = text;
	}

	public override void OnDeserialization()
	{
		//textMeshProUGUI.text = text;
		stencil.localScale = new Vector3(0.4f, 0.1f, 1f) * size;
		sizeSlider.value = size;
		material.SetInt("_Reverse", reverse ? 1 : 0);
		foreach (var f in fonts)
		{
			f.gameObject.SetActive(false);
			f.text = text;
		}
		fonts[font].gameObject.SetActive(true);
		foreach (var f in fonts) f.text = text;
		inputField.text = text;
	}

	public void NextFont()
	{
		font++;
		font %= fonts.Length;

		foreach (var f in fonts) f.gameObject.SetActive(false);
		fonts[font].gameObject.SetActive(true);
	}

	public void PrevFont()
	{
		font--;
		font = (font + fonts.Length) % fonts.Length;

		foreach (var f in fonts) f.gameObject.SetActive(false);
		fonts[font].gameObject.SetActive(true);
	}

	public void Reverse()
	{
		reverse = !reverse;
		material.SetInt("_Reverse", reverse ? 1 : 0);
	}
}
