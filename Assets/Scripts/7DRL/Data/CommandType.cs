using System;
using UnityEngine;
using Utils.Id;

namespace _7DRL.Data {
	[CreateAssetMenu(menuName = "Command Type")]
	public class CommandType : DataScriptableObject {
		[SerializeField] protected string   _descriptionPattern;
		[SerializeField] protected float    _powerMultiplier = 1;
		[SerializeField] protected int      _minPower        = 1;
		[SerializeField] protected int      _maxPower        = int.MaxValue;
		[SerializeField] protected Location _usedInLocations;
		[SerializeField] protected int      _initialPlayerAmount            = 1;
		[SerializeField] protected float    _initialLetterAmountCoefficient = 1;

		public int      initialPlayerAmount            => _initialPlayerAmount;
		public float    initialLetterAmountCoefficient => _initialLetterAmountCoefficient;
		public Location usedInLocations                => _usedInLocations;

		[Flags]
		public enum Location {
			Combat = 1 << 0,
			Map    = 1 << 1
		}

		public int FixPower(int power) => Mathf.RoundToInt(Mathf.Clamp(_powerMultiplier * power, _minPower, _maxPower));
		public string GetDescription(int power) => _descriptionPattern.Replace("[POW]", $"{power}");
		public bool IsUsable(Location location) => (_usedInLocations & location) != 0;
	}
}