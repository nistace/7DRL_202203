using UnityEngine;

namespace _7DRL.Scenes.Combat {
	public class CombatCharacter : MonoBehaviour {
		[SerializeField] protected Animator       _animator;
		[SerializeField] protected SpriteRenderer _spriteRenderer;

		private static readonly int attackAnimParam  = Animator.StringToHash("Attack");
		private static readonly int damagedAnimParam = Animator.StringToHash("Damaged");
		private static readonly int defenseAnimParam = Animator.StringToHash("Defense");
		private static readonly int deadAnimParam    = Animator.StringToHash("Dead");

		public void Init(bool faceRight) {
			_animator.ResetTrigger(attackAnimParam);
			_animator.ResetTrigger(damagedAnimParam);
			_animator.SetBool(deadAnimParam, false);
			_spriteRenderer.color = Color.white;
			_spriteRenderer.flipX = !faceRight;
		}

		public void PlayAttack() => _animator.SetTrigger(attackAnimParam);
		public void PlayDamaged() => _animator.SetTrigger(damagedAnimParam);
		public void PlayDefense() => _animator.SetTrigger(defenseAnimParam);
		public void SetDead(bool dead) => _animator.SetBool(deadAnimParam, dead);
	}
}