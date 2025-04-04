namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;
using static Godot.CharacterBody3D;

[Tool]
[GlobalClass]
public partial class SwimmingBehaviour : MovementBehaviour, IPlayerHandler, IWaterCollisionListener, IWaterDisplacementSubscriber {
	private const float SurfaceThreshold = 2f;
	private const float CenterOfMassOffset = 2f;



	[Export] private Water? Water;
	private EntityBehaviour? previousBehaviour;

	private float _moveSpeed;

	private float _floatingDisplacement = 0f;

	private float _waterSurface;
	private float _waterDisplacement;

	private bool _jumpInput;



	public float EntityCenterOfMass => Entity.CenterOfMass is null ? (Entity.GlobalPosition.Y + CenterOfMassOffset) : Entity.CenterOfMass.GlobalPosition.Y;
	public float OffsetToWaterSurface => _waterSurface + _waterDisplacement - EntityCenterOfMass;
	public bool IsOnWaterSurface => Mathf.Abs(OffsetToWaterSurface) <= SurfaceThreshold;

	protected SwimmingBehaviour() : this(null!, null!) { }
	public SwimmingBehaviour(Entity entity, Water waterArea) : base(entity) {
		Water = waterArea;
	}


	public (Vector3 location, WaterMesh mesh)? GetInfo() => Water?.Mesh is null ? null : (Entity.GlobalPosition, Water.Mesh);
	public void UpdateWaterDisplacement(Vector3 waterDisplacement) => _waterDisplacement = waterDisplacement.Y;

	private void UpdateWaterSurfaceHeight() {
		_waterSurface = Water?.GetSurfaceInDirection(Entity.GlobalPosition, Vector3.Up, out Collisions.IntersectRay3DResult result) ?? false
			? result.Point.Y
			: Mathf.Inf;
	}



	protected override Vector3 ProcessInertia(double delta) {
		float mult = Mathf.Max(1 - 6f * (float)delta, 0f);
		return (Entity.Gravity *= mult) + (Entity.Inertia *= mult);
	}

	protected override Vector3 ProcessMovement(double delta) {
		float floatDelta = (float)delta;
		UpdateWaterSurfaceHeight();

		float offsetToWaterSurface = OffsetToWaterSurface;
		float distanceToWaterSurface = Mathf.Abs(offsetToWaterSurface);
		bool isOnWaterSurface = distanceToWaterSurface <= SurfaceThreshold;


		float newSpeed = Entity.GetTraitValue(Traits.GenericMoveSpeed);

		float speedDelta = Entity.GetTraitValue(_moveSpeed < newSpeed ? Traits.GenericAcceleration : Traits.GenericDeceleration);
		_moveSpeed = _moveSpeed.MoveToward(newSpeed, speedDelta * floatDelta);


		// ----- Rotation & Movement -----
		float rotationSpeed = Entity.GetTraitValue(Traits.GenericTurnSpeed);

		Entity.GlobalBasis = Entity.GlobalBasis.SafeSlerp(Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection), (float)delta * rotationSpeed);


		// ----- Floating at the Surface -----
		if (isOnWaterSurface) {
			float floatingSpeed = Mathf.IsZeroApprox(Entity.Movement.LengthSquared())
				? (offsetToWaterSurface <= 0f ? 5f : 2.5f)
				: 1f;

			_floatingDisplacement = _floatingDisplacement.MoveToward(offsetToWaterSurface, 2f * floatDelta);
			Vector3 floatingDisplacementVector = Vector3.Up * 2f * _floatingDisplacement * floatingSpeed / (Mathf.Abs(distanceToWaterSurface) + 1f);

			Entity.Gravity = Entity.Gravity.MoveToward(floatingDisplacementVector, 12f * floatDelta);
		}

		return Entity.Movement = _movement.Normalized() * _moveSpeed;
	}


	private void ExitWater() {
		if (previousBehaviour is null) {
			Entity.SetBehaviourOrReset<GroundedBehaviour>();
		}
		else {
			Entity.SetBehaviour(previousBehaviour);
		}
	}

	public override void _Process(double delta) {
		if (!Engine.IsEditorHint() && Water is not null) {
			bool isSubmerged = EntityCenterOfMass < _waterSurface;
			bool hasFoothold = Entity.IsOnWall() && Entity.GetWallNormal().Dot(Entity.UpDirection) >= 0f;
			if (IsActive && !isSubmerged && hasFoothold) {
				ExitWater();
			}
			else if (!IsActive && isSubmerged) {
				Entity.SetBehaviour(this);
			}
		}
		base._Process(delta);
	}

	protected override void _Start(EntityBehaviour? previousBehaviour) {
		this.previousBehaviour = previousBehaviour;

		Entity.GlobalForward = Entity.GlobalForward.SlideOnFace(Entity.UpDirection).Normalized();
		Entity.GlobalBasis = Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection);

		Entity.MotionMode = MotionModeEnum.Floating;

		WaterDisplacementEffect.Subscribers.Add(this);
	}
	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		if (_jumpInput && OffsetToWaterSurface <= 0f && nextBehaviour is GroundedBehaviour groundedBehaviour) {
			groundedBehaviour.Jump(true);
		}

		DisavowPlayer();

		WaterDisplacementEffect.Subscribers.Remove(this);
	}


	public virtual void HandlePlayer(Player player) {
		if (!IsActive) return;

		Vector2 input = player.InputDevice.GetVector(
			Inputs.MoveLeft, Inputs.MoveRight,
			Inputs.MoveForward, Inputs.MoveBackward
		).ClampMagnitude(1f);
		player.CameraController.GetCameraRelativeMovement(input, out _, out Vector3 movement);

		if (player.CameraController.SetOrAddBehaviour<GravitatedCameraBehaviour>(() => new(player.CameraController), out var cameraBehaviour)) {
			cameraBehaviour.Subject = Entity;
			cameraBehaviour.MoveCamera(
				player.InputDevice.GetVector(Inputs.LookLeft, Inputs.LookRight, Inputs.LookDown, Inputs.LookUp) * player.InputDevice.Sensitivity
			);
		}

		if (IsOnWaterSurface) {
			movement = movement.SlideOnFace(-Entity.UpDirection);
		}

		_jumpInput = player.InputDevice.IsActionPressed(Inputs.Jump);
		if (_jumpInput) {
			movement = (movement + Entity.UpDirection).ClampMagnitude(1f);
		}

		Move(movement);
	}

	public virtual void DisavowPlayer() {
		_jumpInput = false;
	}


	public void OnEnterWater(Water water) {
		Water ??= water;
		UpdateWaterSurfaceHeight();
	}

	public void OnExitWater(Water water) {
		if (water != Water) return;

		Water = null;
		ExitWater();
	}
}