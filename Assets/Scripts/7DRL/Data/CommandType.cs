using System;
using UnityEngine;
using Utils.Id;

namespace _7DRL.Data {
	[CreateAssetMenu(menuName = "Command Type")]
	public class CommandType : DataScriptableObject {
		[SerializeField] protected string _descriptionPattern;
		[SerializeField] protected float  _powerMultiplier = 1;
		[SerializeField] protected bool   _usedInCombat;
		[SerializeField] protected bool   _usedOnMap;

		public bool usedInCombat => _usedInCombat;
		public bool usedOnMap    => _usedOnMap;

		public int FixPower(int power) => Mathf.RoundToInt(_powerMultiplier * power);

		public string GetDescription(int power) => _descriptionPattern.Replace("[POW]", $"{FixPower(power)}");
	}
}