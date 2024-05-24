namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Utility;

public abstract partial class Attack(SingleWeapon Weapon) : EntityAction() {
	protected SingleWeapon Weapon { get; private set; } = Weapon;


	private Attack() : this(null!) { }


	[Flags]
	public enum AttackType : byte {
		Melee = 1 << 0,
		Projectile = 1 << 1,
		Hitscan = 1 << 2,
		Parry = 1 << 3,
		Explosive = 1 << 4,
	}
}



public static class AttackExtensions {
	public static AttackBuilder SelectAttack(this AttackBuilder[] attacks, IComparer<AttackBuilder> priority, float skillLevel = 0.5f) {
		skillLevel = Mathf.Clamp(skillLevel, 0, 1);

		Array.Sort(attacks, priority);

		float skillMargin = skillLevel * attacks.Length;
		int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

		return attacks[weightedIndex];
	}



	public static class Priorities {
		public static readonly IComparer<AttackBuilder> PureDamage = new ComparisonComparer<AttackBuilder>(
			(a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0
		);
	}
}