using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _7DRL.GameComponents.TextAndLetters.Ui {
	public class TextEffectUi : MonoBehaviour {
		private static TextEffectUi instance { get; set; }

		[Header("Lines"), SerializeField] protected TextEffectWordUi _lineAppearPrefab;
		[SerializeField]                  protected Vector2          _lineAppearOffset     = new Vector2(0, 20);
		[SerializeField]                  protected float            _lineAppearTime       = 2;
		[SerializeField]                  protected float            _lineWaitTime         = .5f;
		[SerializeField]                  protected float            _lineLettersFrequency = 10f;

		[Header("Letters"), SerializeField] protected TMP_Text _letterTextPrefab;
		[SerializeField]                    protected float    _accelerationToDestination = 2;
		[SerializeField]                    protected float    _destinationSqrDistance    = 2;

		private static Queue<TMP_Text> pool { get; } = new Queue<TMP_Text>();

		private void Awake() => instance = this;

		[ContextMenu("Test")]
		private void Test() {
			StartCoroutine(ShowLineAndMoveLettersToReserve("THIS A TEST LINE", new Vector2(600, 200), FindObjectOfType<LetterReserveUi>()));
		}

		public static IEnumerator ShowLineAndMoveLettersToReserve(string line, Vector2 originScreenPosition, LetterReserveUi reserveUi, Action<char> letterArrivedCallback = null) {
			var lineUi = Instantiate(instance._lineAppearPrefab, instance.transform);
			lineUi.text = line;
			lineUi.alpha = 0;
			for (var lerp = 0f; lerp <= 1; lerp += Time.deltaTime / instance._lineAppearTime) {
				lineUi.transform.position = Vector2.Lerp(originScreenPosition, originScreenPosition + instance._lineAppearOffset, lerp);
				lineUi.alpha = lerp;
				yield return null;
			}
			lineUi.transform.position = originScreenPosition + instance._lineAppearOffset;
			lineUi.alpha = 1;
			yield return new WaitForSeconds(instance._lineWaitTime);

			var nextLetterProgress = 0f;
			Coroutine lastRoutine = null;
			while (lineUi.textLength > 0) {
				while (nextLetterProgress < 1) {
					nextLetterProgress += Time.deltaTime * instance._lineLettersFrequency;
					yield return null;
				}
				var origin = lineUi.letterAnchor;
				var letter = lineUi.PopFirstLetter();
				if (reserveUi.TryGetPosition(letter, out var reservePosition)) {
					lastRoutine = instance.StartCoroutine(CreateLetterEffect(letter, origin, reservePosition, letterArrivedCallback));
				}
				nextLetterProgress--;
			}
			if (lastRoutine != null) yield return lastRoutine;
			Destroy(lineUi.gameObject);
		}

		public static IEnumerator CreateLetterEffect(char letter, Vector2 origin, LetterReserveUi reserveUi, Action<char> callback = null) =>
			CreateLetterEffect(letter, origin, reserveUi.TryGetPosition(letter, out var reservePosition) ? reservePosition : null, callback);

		public static IEnumerator CreateLetterEffect(char letter, Vector2 origin, Transform destination, Action<char> callback = null) {
			var letterText = pool.Count > 0 ? pool.Dequeue() : Instantiate(instance._letterTextPrefab, instance.transform);
			letterText.gameObject.SetActive(true);
			letterText.transform.SetParent(destination);
			letterText.text = $"{letter}";
			letterText.rectTransform.position = origin;

			var letterEffect = new LetterEffect(letterText, 0);

			while (letterEffect.localPosition.sqrMagnitude > instance._destinationSqrDistance) {
				letterEffect.localPosition = Vector2.MoveTowards(letterEffect.localPosition, Vector2.zero, letterEffect.velocity * Time.deltaTime);
				letterEffect.velocity += instance._accelerationToDestination * Time.deltaTime;
				yield return null;
			}
			pool.Enqueue(letterEffect.letterText);
			letterEffect.letterText.gameObject.SetActive(false);
			letterEffect.letterText.transform.SetParent(instance.transform);
			callback?.Invoke(letter);
		}

		private class LetterEffect {
			public TMP_Text letterText { get; }
			public float    velocity   { get; set; }

			public Vector2 localPosition {
				get => letterText.transform.localPosition;
				set => letterText.transform.localPosition = value;
			}

			public LetterEffect(TMP_Text letterText, float velocity) {
				this.letterText = letterText;
				this.velocity = velocity;
			}
		}
	}
}