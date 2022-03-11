using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;

namespace _7DRL.Scenes.Combat {
	public class CombatCharacter : MonoBehaviour {
		[SerializeField] protected Animator       _animator;
		[SerializeField] protected SpriteRenderer _spriteRenderer;
		[SerializeField] protected SpriteResolver _spriteResolver;
		[SerializeField] protected Transform      _headTransform;

		private static readonly int restartAnimParam       = Animator.StringToHash("Restart");
		private static readonly int attackAnimParam        = Animator.StringToHash("Attack");
		private static readonly int healAnimParam          = Animator.StringToHash("Heal");
		private static readonly int dodgeAnimParam         = Animator.StringToHash("Dodge");
		private static readonly int damagedAnimParam       = Animator.StringToHash("Damaged");
		private static readonly int defenseAnimParam       = Animator.StringToHash("Defense");
		private static readonly int deadAnimParam          = Animator.StringToHash("Dead");
		private static readonly int escapeSuccessAnimParam = Animator.StringToHash("Escape_Success");
		private static readonly int escapeFailAnimParam    = Animator.StringToHash("Escape_Fail");

		public Transform headTransform => _headTransform;

		public void Init(byte spriteSeed, bool foe) {
			if (_animator && _animator.isActiveAndEnabled) {
				_animator.SetTrigger(restartAnimParam);
				_animator.ResetTrigger(attackAnimParam);
				_animator.ResetTrigger(damagedAnimParam);
				_animator.ResetTrigger(healAnimParam);
				_animator.ResetTrigger(dodgeAnimParam);
				_animator.ResetTrigger(defenseAnimParam);
				_animator.ResetTrigger(escapeSuccessAnimParam);
				_animator.ResetTrigger(escapeFailAnimParam);
				_animator.SetBool(deadAnimParam, false);
			}
			_spriteRenderer.color = Color.white;
			_spriteRenderer.flipX = foe;
			var spriteCategory = foe ? "Foes" : "Player";
			var spriteOptions = _spriteResolver.spriteLibrary.spriteLibraryAsset.GetCategoryLabelNames(spriteCategory).ToArray();
			_spriteResolver.SetCategoryAndLabel(spriteCategory, spriteOptions[spriteSeed % spriteOptions.Length]);
		}

		public void PlayAttack() => _animator.SetTrigger(attackAnimParam);
		public void PlayHeal() => _animator.SetTrigger(healAnimParam);
		public void PlayDodge() => _animator.SetTrigger(dodgeAnimParam);
		public void PlayDamaged() => _animator.SetTrigger(damagedAnimParam);
		public void PlayDefense() => _animator.SetTrigger(defenseAnimParam);
		public void PlayEscape(bool success) => _animator.SetTrigger(success ? escapeSuccessAnimParam : escapeFailAnimParam);
		public void SetDead(bool dead) => _animator.SetBool(deadAnimParam, dead);
	}
}