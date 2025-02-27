namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Boundless.Utility;
using static SevenDev.Boundless.Utility.Collisions;


[Tool]
[GlobalClass]
public partial class VehicleBehaviour : GroundedBehaviour, IWaterCollisionNotifier {
	[Export] public DrivingBehaviour? Driver;

	private bool _drifting;

	private Vector3 _inertiaMovement;
	private float _moveSpeed;

	private Vector3 _modelForward = Vector3.Forward;
	private Vector3 _modelUp = Vector3.Up;


	protected VehicleBehaviour() : this(null!) { }
	public VehicleBehaviour(Entity entity) : base(entity, new BipedJumpAction.Builder()) { }



	protected override void _Start(EntityBehaviour? previousBehaviour) {
		base._Start(previousBehaviour);
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);

		Driver?.Dismount();
	}

	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);
		_drifting = player.InputDevice.IsActionPressed(Inputs.AttackHeavy);
	}

	protected override Vector3 ProcessGroundedMovement(double delta) {
		float floatDelta = (float)delta;
		float entityMoveSpeed = Entity.GetTraitValue(Traits.GenericMoveSpeed);
		bool onFloor = Entity.IsOnFloor();
		Vector3 entityUp = Entity.UpDirection;

		Vector3 groundUp = entityUp;
		if (onFloor) {
			Vector3 entityPos = Entity.GlobalPosition;
			if (Entity.GetWorld3D().IntersectRay3D(entityPos + entityUp, entityPos - entityUp, out IntersectRay3DResult res, Entity.CollisionMask)) {
				groundUp = res.Normal.Normalized();
			}
		}

		float deceleration = Mathf.InverseLerp(entityMoveSpeed * 0.5f, 0f, _moveSpeed).Clamp01();

		if (deceleration > 0f) {
			Entity.UpDirection = entityUp.Slerp(Vector3.Up, 3f * deceleration * floatDelta);
		}
		else if (onFloor && groundUp.Dot(entityUp) >= 0.75f) {
			Entity.UpDirection = groundUp;
		}


		Basis rotationToNormal = Entity.Transform.Up().FromToBasis(Entity.UpDirection);

		Entity.GlobalForward = Entity.GlobalForward.SafeSlerp(rotationToNormal * Entity.GlobalForward, 18f * floatDelta);
		_movement = rotationToNormal * _movement;



		float newSpeed = 0f;
		Vector3 direction = Vector3.Zero;
		if (!_movement.IsEqualApprox(Vector3.Zero)) {
			direction = _movement.Normalized();

			newSpeed = Mathf.Clamp(Entity.GlobalForward.Dot(direction) + 0.25f, 0f, 1f) * entityMoveSpeed;
			Entity.GlobalForward = Entity.GlobalForward.Slerp(direction, Entity.GetTraitValue(Traits.GenericTurnSpeed) * floatDelta);
		}

		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		if (_drifting) {
			newSpeed = 0f;
		}
		else {
			_inertiaMovement = _inertiaMovement.Lerp(Entity.GlobalForward, 12f * floatDelta);
		}

		float speedDelta = newSpeed > _moveSpeed ? 1f : 0.25f;
		_moveSpeed = _moveSpeed.MoveToward(newSpeed, speedDelta * Entity.GetTraitValue(Traits.GenericAcceleration) * floatDelta);

		ComputeRotation(direction, groundUp, floatDelta);

		_movement = Vector3.Zero;
		return _inertiaMovement * _moveSpeed;

		void ComputeRotation(Vector3 direction, Vector3 groundUp, float floatDelta) {
			if (Entity.CostumeHolder?.Costume is not Costume model) return;

			Basis toGroundUp = Entity.GlobalBasis.Up().FromToBasis(groundUp);
			Vector3 entityRight = toGroundUp * Entity.GlobalBasis.Right();

			_modelForward = _modelForward.Lerp(toGroundUp * Entity.GlobalForward, 14f * floatDelta).Normalized();
			_modelUp = _modelUp.Lerp((groundUp + direction.Dot(entityRight) * entityRight).Normalized(), 7f * floatDelta).Normalized();

			model.GlobalBasis = Basis.LookingAt(_modelForward, _modelUp);
		}
	}

	// protected override Vector3 ProcessInertia(double delta, Vector3 horizontalInertia) {
	// 	return _inertiaMovement * _moveSpeed;
	// }

	public void OnEnterWater(Water water) {
		Driver?.Dismount();
		Entity.VoidOut();
	}

	public void OnExitWater(Water water) { }
}