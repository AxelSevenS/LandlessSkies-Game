namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;

/// <summary>
/// An Action is analogous to an RPG's turn action, for example, throwing a projectile, using a weapon or evading.<para/>
/// An Entity can only execute one Action at a time.
/// </summary>
public abstract partial class Action : Node {
	/// <summary>
	/// The Lifetime of the Action, when it was started onwards.
	/// </summary>
	public readonly SevenDev.Boundless.Utility.Timer Lifetime = new(false);

	/// <summary>
	/// The Entity which is executing the Action
	/// </summary>
	[Export] public Entity Entity;
	public IEnumerable<TraitModifier> Modifiers;

	public event System.Action? OnStart;
	public event System.Action? OnStop;

	/// <param name="entity">The Entity which will execute the Action.</param>
	/// <param name="modifiers">
	/// The Modifiers intrisic to the Action,<para/>
	/// they will be applied to the Entity when the Action is started and removed from the Entity when the Action is stopped.
	/// </param>
	public Action(Entity entity, IEnumerable<TraitModifier>? modifiers = null) {
		Entity = entity;
		Modifiers = modifiers ?? [];
	}

	/// <summary>
	/// Whether the Action can be cancelled by attempting to start another Action.<para/>
	/// This can be trumped by the '<paramref name="forceExecute"/>' argument in <see cref="Entity.ExecuteAction(IEntityActionBuilder, bool)"/>
	/// </summary>
	public abstract bool IsCancellable { get; }

	/// <summary>
	/// Whether the Action can be interrupted by being 'knocked' out of it, typically from being attacked.<para/>
	/// This can be viewed as determining whether the Action currently has 'super-armor' or 'poise'.
	/// </summary>
	public abstract bool IsInterruptable { get; }

	/// <summary>
	/// Start the Action. Call this after having setup the Action
	/// </summary>
	public void Start() {
		if (Entity is null) {
			GD.PushError($"Entity was null when trying to execute {GetType().Name} Action");
			return;
		}
		Lifetime.Start();
		OnStart?.Invoke();

		Entity.TraitModifiers.AddRange(Modifiers);
		_Start();
	}

	/// <summary>
	/// Stop the Action and destroy it.
	/// </summary>
	public void Stop() {
		OnStop?.Invoke();

		Entity.TraitModifiers.RemoveRange(Modifiers);
		_Stop();

		this.UnparentAndQueueFree();
	}

	/// <summary>
	/// Callback method that is invoked when the Action is started.
	/// </summary>
	protected virtual void _Start() { }

	/// <summary>
	/// Callback method that is invoked when the Action is stopped, i.e. when it is destroyed.
	/// </summary>
	protected virtual void _Stop() { }



	public abstract class Builder {
		public abstract Action Build(Entity entity);
	}

	/// <summary>
	/// An Action Wrapper is a wrapper for use in pre-execution Action configuration, sets up and starts an Action.<para/>
	/// It is passed to <see cref="Entity.ExecuteAction(Wrapper, bool)"/> to execute the given Action.
	/// </summary>
	public sealed class Wrapper(Builder builder) {
		public readonly Builder Builder = builder;
		public System.Action? BeforeExecute { get; set; }
		public System.Action? AfterExecute { get; set; }


		internal Action Create(Entity entity) {
			Action action = Builder.Build(entity);
			action.OnStart += BeforeExecute;
			action.OnStop += AfterExecute;

			return action;
		}

		public static implicit operator Wrapper(Builder from) => new(from);
	}
}