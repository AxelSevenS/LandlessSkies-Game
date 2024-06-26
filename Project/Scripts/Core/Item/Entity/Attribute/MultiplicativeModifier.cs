namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class MultiplicativeModifier : AttributeModifier {
	[Export] public float Multiplier {
		get => _multiplier;
		private set {
			_multiplier = value;
			UpdateName();
		}
	}
	private float _multiplier;

	[Export] public bool _isStacking = false;
	public override bool IsStacking => _isStacking;



	public MultiplicativeModifier(StringName name, float multiplier, bool isStacking = false) : base(name) {
		_multiplier = multiplier;
		_isStacking = isStacking;
	}
	private MultiplicativeModifier() : base() {}



	public override float ApplyTo(float baseValue) {
		return baseValue * _multiplier;
	}

	protected override string GetResourceName() {
		return $"{_multiplier:0.##%} {Name}";
	}
}