//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class EncounterType {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		/// <summary>:land, :cave, :water, :fishing, :contest, :none</summary>
		public int type		{ get { return _type; } }			protected int _type;
		public int trigger_chance		{ get { return _trigger_chance; } }			protected int _trigger_chance;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id             = hash.id;
			@real_name      = hash.id.ToString()        || "Unnamed";
			@type           = hash.type           || :none;
			@trigger_chance = hash.trigger_chance || 0;
		}

		alias name real_name;
	}
}

//===============================================================================
//
//===============================================================================

GameData.EncounterType.register({
	id             = :Land,
	type           = :land,
	trigger_chance = 21;
});

GameData.EncounterType.register({
	id             = :LandDay,
	type           = :land,
	trigger_chance = 21;
});

GameData.EncounterType.register({
	id             = :LandNight,
	type           = :land,
	trigger_chance = 21;
});

GameData.EncounterType.register({
	id             = :LandMorning,
	type           = :land,
	trigger_chance = 21;
});

GameData.EncounterType.register({
	id             = :LandAfternoon,
	type           = :land,
	trigger_chance = 21;
});

GameData.EncounterType.register({
	id             = :LandEvening,
	type           = :land,
	trigger_chance = 21;
});

GameData.EncounterType.register({
	id             = :PokeRadar,
	type           = :land,
	trigger_chance = 20;
});

GameData.EncounterType.register({
	id             = :Cave,
	type           = :cave,
	trigger_chance = 5;
});

GameData.EncounterType.register({
	id             = :CaveDay,
	type           = :cave,
	trigger_chance = 5;
});

GameData.EncounterType.register({
	id             = :CaveNight,
	type           = :cave,
	trigger_chance = 5;
});

GameData.EncounterType.register({
	id             = :CaveMorning,
	type           = :cave,
	trigger_chance = 5;
});

GameData.EncounterType.register({
	id             = :CaveAfternoon,
	type           = :cave,
	trigger_chance = 5;
});

GameData.EncounterType.register({
	id             = :CaveEvening,
	type           = :cave,
	trigger_chance = 5;
});

GameData.EncounterType.register({
	id             = :Water,
	type           = :water,
	trigger_chance = 2;
});

GameData.EncounterType.register({
	id             = :WaterDay,
	type           = :water,
	trigger_chance = 2;
});

GameData.EncounterType.register({
	id             = :WaterNight,
	type           = :water,
	trigger_chance = 2;
});

GameData.EncounterType.register({
	id             = :WaterMorning,
	type           = :water,
	trigger_chance = 2;
});

GameData.EncounterType.register({
	id             = :WaterAfternoon,
	type           = :water,
	trigger_chance = 2;
});

GameData.EncounterType.register({
	id             = :WaterEvening,
	type           = :water,
	trigger_chance = 2;
});

GameData.EncounterType.register({
	id             = :OldRod,
	type           = :fishing;
});

GameData.EncounterType.register({
	id             = :GoodRod,
	type           = :fishing;
});

GameData.EncounterType.register({
	id             = :SuperRod,
	type           = :fishing;
});

GameData.EncounterType.register({
	id             = :RockSmash,
	type           = :none,
	trigger_chance = 50;
});

GameData.EncounterType.register({
	id             = :HeadbuttLow,
	type           = :none;
});

GameData.EncounterType.register({
	id             = :HeadbuttHigh,
	type           = :none;
});

GameData.EncounterType.register({
	id             = :BugContest,
	type           = :contest,
	trigger_chance = 21;
});
