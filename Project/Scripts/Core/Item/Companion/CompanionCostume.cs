namespace LandlessSkies.Core;

using Godot;
using static LandlessSkies.Core.IPortraitProvider;

[Tool]
[GlobalClass]
public partial class CompanionCostume : Costume, IPortraitProvider {
	[Export] private string _displayName = string.Empty;
	public override string DisplayName => _displayName;

	[Export] private Texture2D? _displayPortrait = null;
	public override Texture2D? DisplayPortrait => _displayPortrait;

	[Export] public Texture2D? PortraitDetermined { get; private set; }
	[Export] public Texture2D? PortraitHesitant { get; private set; }
	[Export] public Texture2D? PortraitShocked { get; private set; }
	[Export] public Texture2D? PortraitDisgusted { get; private set; }
	[Export] public Texture2D? PortraitMelancholic { get; private set; }
	[Export] public Texture2D? PortraitJoyous { get; private set; }


	public Texture2D? GetPortrait(CharacterEmotion emotion) => Select(emotion,
		neutral: DisplayPortrait,
		determined: PortraitDetermined,
		hesitant: PortraitHesitant,
		shocked: PortraitShocked,
		disgusted: PortraitDisgusted,
		melancholic: PortraitMelancholic,
		joyous: PortraitJoyous
	);
}