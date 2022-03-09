using TMPro;
using UnityEngine;

public class DialogTextLineUi : MonoBehaviour {
	[SerializeField] protected TMP_Text _text;

	public void Set(string text) => _text.text = text;
}