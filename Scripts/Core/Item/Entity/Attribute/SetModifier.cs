using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class SetModifier : AttributeModifier {
	[Export]
	public float Value {
		get => _value;
		set {
			_value = value;
			EmitValueModified();
		}
	}
	private float _value;

	[Export] public bool _isStacking = false;
	public override bool IsStacking => _isStacking;


	public SetModifier(EntityAttribute target, float value, bool isStacking = false) : base(target) {
		_value = value;
		_isStacking = isStacking;
	}
	private SetModifier() : base() { }


	public override float ApplyTo(float baseValue) {
		return Mathf.Lerp(baseValue, _value, Efficiency);
	}

	protected override string GetResourceName() {
		return $"{AttributeName} = {_value}";
	}

	protected override bool EqualsInternal(AttributeModifier other) {
		return other is SetModifier setModifier && setModifier._value == _value;
	}
	public override int GetHashCode() {
		unchecked {
			return (base.GetHashCode() * 397) ^ (_value.GetHashCode() * 397);
		}
	}
}