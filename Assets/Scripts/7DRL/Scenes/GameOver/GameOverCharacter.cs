using UnityEngine;

public class GameOverCharacter : MonoBehaviour {
	[SerializeField] protected Animator _animator;

	private static readonly int victoryAnimParam = Animator.StringToHash("Victory");
	private static readonly int deadAnimParam    = Animator.StringToHash("Dead");
	private static readonly int lostAnimParam    = Animator.StringToHash("Lost");

	public void PlayVictory() => _animator.SetTrigger(victoryAnimParam);
	public void PlayDead() => _animator.SetTrigger(deadAnimParam);
	public void PlayLost() => _animator.SetTrigger(lostAnimParam);
}