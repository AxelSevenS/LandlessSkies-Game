namespace LandlessSkies.Vanilla;

using System.IO;
using System.Threading.Tasks;
using Godot;
using KGySoft.Serialization.Binary;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Persistence;
using LandlessSkies.Core;

[GlobalClass]
public partial class TestInteractable : Interactable {
	public override string InteractLabel => "Interact";
	public override float? MinLookIncidence => 0f;

	public override void Interact(Entity entity, Player? player = null, int shapeIndex = 0) {
		GD.Print($"Entity {entity.Name} interacted with {Name}, shape {GetShape3D(shapeIndex)?.Name} (index {shapeIndex})");

		if (player is null) return;

		CloneEntity(entity);
	}

	private async void CloneEntity(Entity entity) {
		IPersistenceData<Entity>? savedEntity = entity.Save();

		await Task.Run(() => {
			string path = @$"{OS.GetUserDataDir()}/SaveData1.dat";
			BinarySerializationFormatter formatter = new(BinarySerializationOptions.CompactSerializationOfStructures);

			using (FileStream stream = new(path, FileMode.Create)) {
				formatter.SerializeToStream(stream, savedEntity);
			}
			using (FileStream stream = new(path, FileMode.Open)) {
				savedEntity = formatter.DeserializeFromStream<IPersistenceData<Entity>?>(stream);
			}
		});

		Entity? clonedEntity = savedEntity?.Load()?.SetOwnerAndParent(this);
		if (clonedEntity is not null) {
			clonedEntity.GlobalTransform = GlobalTransform;
		}
	}


	public override bool IsInteractable(Entity entity) {
		return true;
	}
}