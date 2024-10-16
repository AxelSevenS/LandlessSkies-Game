namespace LandlessSkies.Core;

using System.Collections.Generic;

public interface IWeapon : IUIObject {
	WeaponType Type { get; }
	WeaponUsage Usage { get; }
	WeaponSize Size { get; }

	abstract uint Style { get; set; }
	virtual uint MaxStyle => 0;

	IEnumerable<Action.Wrapper> GetAttacks(Entity target);
}