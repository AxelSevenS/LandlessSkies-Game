namespace LandlessSkies.Vanilla;

using System.Collections.Generic;
using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class NemesisWeapon : Weapon, IPlayerHandler {
	private readonly SlashAttack.Builder slashAttack;
	public override IWeapon.WeaponKind Kind => IWeapon.WeaponKind.Sword;
	public override IWeapon.WeaponUsage Usage => IWeapon.WeaponUsage.Slash | IWeapon.WeaponUsage.Strike;
	public override IWeapon.WeaponSize Size => IWeapon.WeaponSize.TwoHanded;


	public NemesisWeapon() : base() {
		slashAttack = new(this);
	}


	public override Vector3 GetTipPosition() => new(0f, 3.2f, 0f);

	public override IEnumerable<Action.Wrapper> GetAttacks(Entity target) {
		return [
			slashAttack
		];
	}


	public void HandlePlayer(Player player) {
		if (player.Entity is null) return;
		if (IsHolstered) return;

		switch (player.Entity.CurrentBehaviour) {
			case GroundedBehaviour grounded:
				if (player.InputDevice.IsActionJustPressed(Inputs.AttackLight)) {
					player.Entity.ExecuteAction(slashAttack);
				}
				break;
		}
	}

	public void DisavowPlayer() { }
}