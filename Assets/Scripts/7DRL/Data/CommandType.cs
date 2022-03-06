using System;
using UnityEngine;
using Utils.Id;

namespace _7DRL.Data {
	[CreateAssetMenu(menuName = "Command Type")]
	public class CommandType : DataScriptableObject {
		[SerializeField] protected string   _descriptionPattern;
		[SerializeField] protected float    _powerMultiplier = 1;
		[SerializeField] protected Location _usedInLocations;

		[Flags]
		public enum Location {
			Combat = 1 << 0,
			Map    = 1 << 1
		}

		public int FixPower(int power) => Mathf.RoundToInt(_powerMultiplier * power);

		public string GetDescription(int power) => _descriptionPattern.Replace("[POW]", $"{FixPower(power)}");

		public bool IsUsable(Location location) => (_usedInLocations & location) != 0;
	}
}