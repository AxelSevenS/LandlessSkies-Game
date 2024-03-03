using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class NemesisWeaponData : WeaponData {
#if TOOLS
    protected override bool EditableType => false;
    protected override bool EditableUsage => false;
    protected override bool EditableSize => false;
#endif



	private NemesisWeaponData() : base() {
		Type = IWeapon.Type.Sword;
		Usage = IWeapon.Usage.Slashing | IWeapon.Usage.Bludgeoning;
		Size = IWeapon.Size.TwoHanded;
	}



	public override NemesisWeapon Instantiate(WeaponCostume? costume = null) =>
		new(this, costume);
}