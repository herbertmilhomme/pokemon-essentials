//===============================================================================
// NOTE: If adding a new target, you will need to add code in several places to
//       make them work properly:
//         - def FindTargets
//         - public bool MoveCanTarget() {
//         - def CreateTargetTexts
//         - def FirstTarget
//         - public bool TargetsMultiple() {
//===============================================================================
public static partial class GameData {
	public partial class Target {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		/// <summary>0, 1 or 2 (meaning 2+)</summary>
		public int num_targets		{ get { return _num_targets; } }			protected int _num_targets;
		/// <summary>Is able to target one or more foes</summary>
		public int targets_foe		{ get { return _targets_foe; } }			protected int _targets_foe;
		/// <summary>Crafty Shield can't protect from these moves</summary>
		public int targets_all		{ get { return _targets_all; } }			protected int _targets_all;
		/// <summary>Pressure also affects these moves</summary>
		public int affects_foe_side		{ get { return _affects_foe_side; } }			protected int _affects_foe_side;
		/// <summary>Hits non-adjacent targets</summary>
		public int long_range		{ get { return _long_range; } }			protected int _long_range;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id               = hash.id;
			@real_name        = hash.name             || "Unnamed";
			@num_targets      = hash.num_targets      || 0;
			@targets_foe      = hash.targets_foe      || false;
			@targets_all      = hash.targets_all      || false;
			@affects_foe_side = hash.affects_foe_side || false;
			@long_range       = hash.long_range       || false;
		}

		// @return [String] the translated name of this target
		public void name() {
			return _INTL(@real_name);
		}

		public bool can_choose_distant_target() {
			return @num_targets == 1 && @long_range;
		}

		public bool can_target_one_foe() {
			return @num_targets == 1 && @targets_foe;
		}
	}
}

//===============================================================================
//
//===============================================================================

// Bide, Counter, Metal Burst, Mirror Coat (calculate a target)
GameData.Target.register({
	id               = :None,
	name             = _INTL("None");
});

GameData.Target.register({
	id               = :User,
	name             = _INTL("User");
});

// Aromatic Mist, Helping Hand, Hold Hands
GameData.Target.register({
	id               = :NearAlly,
	name             = _INTL("Near Ally"),
	num_targets      = 1;
});

// Acupressure
GameData.Target.register({
	id               = :UserOrNearAlly,
	name             = _INTL("User or Near Ally"),
	num_targets      = 1;
});

// Coaching
GameData.Target.register({
	id               = :AllAllies,
	name             = _INTL("All Allies"),
	num_targets      = 2,
	targets_all      = true,
	long_range       = true;
});

// Aromatherapy, Gear Up, Heal Bell, Life Dew, Magnetic Flux, Howl (in Gen 8+)
GameData.Target.register({
	id               = :UserAndAllies,
	name             = _INTL("User and Allies"),
	num_targets      = 2,
	long_range       = true;
});

// Me First
GameData.Target.register({
	id               = :NearFoe,
	name             = _INTL("Near Foe"),
	num_targets      = 1,
	targets_foe      = true;
});

// Petal Dance, Outrage, Struggle, Thrash, Uproar
GameData.Target.register({
	id               = :RandomNearFoe,
	name             = _INTL("Random Near Foe"),
	num_targets      = 1,
	targets_foe      = true;
});

GameData.Target.register({
	id               = :AllNearFoes,
	name             = _INTL("All Near Foes"),
	num_targets      = 2,
	targets_foe      = true;
});

// For throwing a Poké Ball
GameData.Target.register({
	id               = :Foe,
	name             = _INTL("Foe"),
	num_targets      = 1,
	targets_foe      = true,
	long_range       = true;
});

// Unused
GameData.Target.register({
	id               = :AllFoes,
	name             = _INTL("All Foes"),
	num_targets      = 2,
	targets_foe      = true,
	long_range       = true;
});

GameData.Target.register({
	id               = :NearOther,
	name             = _INTL("Near Other"),
	num_targets      = 1,
	targets_foe      = true;
});

GameData.Target.register({
	id               = :AllNearOthers,
	name             = _INTL("All Near Others"),
	num_targets      = 2,
	targets_foe      = true;
});

// Most Flying-type moves, pulse moves (hits non-near targets)
GameData.Target.register({
	id               = :Other,
	name             = _INTL("Other"),
	num_targets      = 1,
	targets_foe      = true,
	long_range       = true;
});

// Flower Shield, Perish Song, Rototiller, Teatime
GameData.Target.register({
	id               = :AllBattlers,
	name             = _INTL("All Battlers"),
	num_targets      = 2,
	targets_foe      = true,
	targets_all      = true,
	long_range       = true;
});

GameData.Target.register({
	id               = :UserSide,
	name             = _INTL("User Side");
});

// Entry hazards
GameData.Target.register({
	id               = :FoeSide,
	name             = _INTL("Foe Side"),
	affects_foe_side = true;
});

GameData.Target.register({
	id               = :BothSides,
	name             = _INTL("Both Sides"),
	affects_foe_side = true;
});
