//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class TerrainTag {
		public int id		{ get { return _id; } }			protected int _id;
		public int id_number		{ get { return _id_number; } }			protected int _id_number;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int can_surf		{ get { return _can_surf; } }			protected int _can_surf;
		/// <summary>The main part only, not the crest</summary>
		public int waterfall		{ get { return _waterfall; } }			protected int _waterfall;
		public int waterfall_crest		{ get { return _waterfall_crest; } }			protected int _waterfall_crest;
		public int can_fish		{ get { return _can_fish; } }			protected int _can_fish;
		public int can_dive		{ get { return _can_dive; } }			protected int _can_dive;
		public int deep_bush		{ get { return _deep_bush; } }			protected int _deep_bush;
		public int shows_grass_rustle		{ get { return _shows_grass_rustle; } }			protected int _shows_grass_rustle;
		public int shows_water_ripple		{ get { return _shows_water_ripple; } }			protected int _shows_water_ripple;
		public int land_wild_encounters		{ get { return _land_wild_encounters; } }			protected int _land_wild_encounters;
		public int double_wild_encounters		{ get { return _double_wild_encounters; } }			protected int _double_wild_encounters;
		public int battle_environment		{ get { return _battle_environment; } }			protected int _battle_environment;
		public int ledge		{ get { return _ledge; } }			protected int _ledge;
		public int ice		{ get { return _ice; } }			protected int _ice;
		public int bridge		{ get { return _bridge; } }			protected int _bridge;
		public int shows_reflections		{ get { return _shows_reflections; } }			protected int _shows_reflections;
		public int must_walk		{ get { return _must_walk; } }			protected int _must_walk;
		public int must_walk_or_run		{ get { return _must_walk_or_run; } }			protected int _must_walk_or_run;
		public int ignore_passability		{ get { return _ignore_passability; } }			protected int _ignore_passability;

		DATA = new List<string>();

		extend ClassMethods;
		include InstanceMethods;

		// @param other new {Symbol, self, String, Integer}
		// @return [self]
		public static void try_get(other) {
			if (other.null()) return self.get(:None);
			validate other => new {Symbol, self, String, Integer};
			if (other.is_a(self)) return other;
			if (other.is_a(String)) other = other.to_sym;
			return (self.DATA.has_key(other)) ? self.DATA[other] : self.get(:None);
		}

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                     = hash.id;
			@id_number              = hash.id_number;
			@real_name              = hash.id.ToString()                || "Unnamed";
			@can_surf               = hash.can_surf               || false;
			@waterfall              = hash.waterfall              || false;
			@waterfall_crest        = hash.waterfall_crest        || false;
			@can_fish               = hash.can_fish               || false;
			@can_dive               = hash.can_dive               || false;
			@deep_bush              = hash.deep_bush              || false;
			@shows_grass_rustle     = hash.shows_grass_rustle     || false;
			@shows_water_ripple     = hash.shows_water_ripple     || false;
			@land_wild_encounters   = hash.land_wild_encounters   || false;
			@double_wild_encounters = hash.double_wild_encounters || false;
			@battle_environment     = hash.battle_environment;
			@ledge                  = hash.ledge                  || false;
			@ice                    = hash.ice                    || false;
			@bridge                 = hash.bridge                 || false;
			@shows_reflections      = hash.shows_reflections      || false;
			@must_walk              = hash.must_walk              || false;
			@must_walk_or_run       = hash.must_walk_or_run       || false;
			@ignore_passability     = hash.ignore_passability     || false;
		}

		alias name real_name;

		public void can_surf_freely() {
			return @can_surf && !@waterfall && !@waterfall_crest;
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.TerrainTag.register({
	id                     = :None,
	id_number              = 0;
});

GameData.TerrainTag.register({
	id                     = :Ledge,
	id_number              = 1,
	ledge                  = true;
});

GameData.TerrainTag.register({
	id                     = :Grass,
	id_number              = 2,
	shows_grass_rustle     = true,
	land_wild_encounters   = true,
	battle_environment     = :Grass;
});

GameData.TerrainTag.register({
	id                     = :Sand,
	id_number              = 3,
	battle_environment     = :Sand;
});

GameData.TerrainTag.register({
	id                     = :Rock,
	id_number              = 4,
	battle_environment     = :Rock;
});

GameData.TerrainTag.register({
	id                     = :DeepWater,
	id_number              = 5,
	can_surf               = true,
	can_fish               = true,
	can_dive               = true,
	battle_environment     = :MovingWater;
});

GameData.TerrainTag.register({
	id                     = :StillWater,
	id_number              = 6,
	can_surf               = true,
	can_fish               = true,
	battle_environment     = :StillWater,
	shows_reflections      = true,
	shows_water_ripple     = true;
});

GameData.TerrainTag.register({
	id                     = :Water,
	id_number              = 7,
	can_surf               = true,
	can_fish               = true,
	battle_environment     = :MovingWater;
});

GameData.TerrainTag.register({
	id                     = :Waterfall,
	id_number              = 8,
	can_surf               = true,
	waterfall              = true;
});

GameData.TerrainTag.register({
	id                     = :WaterfallCrest,
	id_number              = 9,
	can_surf               = true,
	can_fish               = true,
	waterfall_crest        = true;
});

GameData.TerrainTag.register({
	id                     = :TallGrass,
	id_number              = 10,
	deep_bush              = true,
	land_wild_encounters   = true,
	double_wild_encounters = true,
	battle_environment     = :TallGrass,
	must_walk              = true;
});

GameData.TerrainTag.register({
	id                     = :UnderwaterGrass,
	id_number              = 11,
	land_wild_encounters   = true;
});

GameData.TerrainTag.register({
	id                     = :Ice,
	id_number              = 12,
	battle_environment     = :Ice,
	ice                    = true,
	must_walk_or_run       = true;
});

GameData.TerrainTag.register({
	id                     = :Neutral,
	id_number              = 13,
	ignore_passability     = true;
});

// NOTE: This is referenced by ID in the :pick_up_soot proc added to
//       EventHandlers. It adds soot to the Soot Sack if the player walks over
//       one of these tiles.
GameData.TerrainTag.register({
	id                     = :SootGrass,
	id_number              = 14,
	shows_grass_rustle     = true,
	land_wild_encounters   = true,
	battle_environment     = :Grass;
});

GameData.TerrainTag.register({
	id                     = :Bridge,
	id_number              = 15,
	bridge                 = true;
});

GameData.TerrainTag.register({
	id                     = :Puddle,
	id_number              = 16,
	battle_environment     = :Puddle,
	shows_reflections      = true,
	shows_water_ripple     = true;
});

GameData.TerrainTag.register({
	id                     = :NoEffect,
	id_number              = 17;
});
