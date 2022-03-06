using _7DRL.Scenes.Combat;
using UnityEngine;

public class CombatScene : MonoBehaviour {
	[SerializeField] protected SpriteRenderer  _background;
	[SerializeField] protected CombatCharacter _player;
	[SerializeField] protected Transform       _playerPosition;
	[SerializeField] protected Transform[]     _foesPositions;

	public CombatCharacter player => _player;

	private void Update() {
		var worldScreenHeight = (float)(CameraUtils.main.orthographicSize * 2.0);
		var worldScreenWidth = worldScreenHeight / Display.main.renderingHeight * Display.main.renderingWidth;
		RelocatePlayerAndFoes(worldScreenWidth);
	}

	private void RelocatePlayerAndFoes(float worldScreenWidth) {
		_playerPosition.position = new Vector3(-worldScreenWidth * .25f, 0, 0);
		for (var index = 0; index < _foesPositions.Length; index++) {
			_foesPositions[index].position = new Vector3(worldScreenWidth * (.05f + index * .4f / _foesPositions.Length), 0, 0);
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(_playerPosition.position, .2f);
		Gizmos.color = Color.red;
		foreach (var foePosition in _foesPositions) Gizmos.DrawWireSphere(foePosition.position, .2f);
	}

	public Transform GetFoePosition(int index) => _foesPositions[index];
}