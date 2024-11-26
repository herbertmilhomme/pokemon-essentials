//===============================================================================
// NOTE: The order these shapes are registered are the order they are listed in
//       the Pokédex search screen.
//       "Graphics/UI/Pokedex/icon_shapes.png" contains icons for these
//       shapes.
//===============================================================================
public static partial class GameData {
	public partial class BodyShape {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		/// <summary>Where this shape's icon is within icon_shapes.png</summary>
		public int icon_position		{ get { return _icon_position; } }			protected int _icon_position;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id            = hash.id;
			@real_name     = hash.name          || "Unnamed";
			@icon_position = hash.icon_position || 0;
		}

		// @return [String] the translated name of this body shape
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.BodyShape.register({
	id            = :Head,
	name          = _INTL("Head"),
	icon_position = 0;
});

GameData.BodyShape.register({
	id            = :Serpentine,
	name          = _INTL("Serpentine"),
	icon_position = 1;
});

GameData.BodyShape.register({
	id            = :Finned,
	name          = _INTL("Finned"),
	icon_position = 2;
});

GameData.BodyShape.register({
	id            = :HeadArms,
	name          = _INTL("Head and arms"),
	icon_position = 3;
});

GameData.BodyShape.register({
	id            = :HeadBase,
	name          = _INTL("Head and base"),
	icon_position = 4;
});

GameData.BodyShape.register({
	id            = :BipedalTail,
	name          = _INTL("Bipedal with tail"),
	icon_position = 5;
});

GameData.BodyShape.register({
	id            = :HeadLegs,
	name          = _INTL("Head and legs"),
	icon_position = 6;
});

GameData.BodyShape.register({
	id            = :Quadruped,
	name          = _INTL("Quadruped"),
	icon_position = 7;
});

GameData.BodyShape.register({
	id            = :Winged,
	name          = _INTL("Winged"),
	icon_position = 8;
});

GameData.BodyShape.register({
	id            = :Multiped,
	name          = _INTL("Multiped"),
	icon_position = 9;
});

GameData.BodyShape.register({
	id            = :MultiBody,
	name          = _INTL("Multi Body"),
	icon_position = 10;
});

GameData.BodyShape.register({
	id            = :Bipedal,
	name          = _INTL("Bipedal"),
	icon_position = 11;
});

GameData.BodyShape.register({
	id            = :MultiWinged,
	name          = _INTL("Multi Winged"),
	icon_position = 12;
});

GameData.BodyShape.register({
	id            = :Insectoid,
	name          = _INTL("Insectoid"),
	icon_position = 13;
});
