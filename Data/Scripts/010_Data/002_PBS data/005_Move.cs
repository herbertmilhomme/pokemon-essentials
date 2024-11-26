//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Move {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int type		{ get { return _type; } }			protected int _type;
		public int category		{ get { return _category; } }			protected int _category;
		public int power		{ get { return _power; } }			protected int _power;
		public int accuracy		{ get { return _accuracy; } }			protected int _accuracy;
		public int total_pp		{ get { return _total_pp; } }			protected int _total_pp;
		public int target		{ get { return _target; } }			protected int _target;
		public int priority		{ get { return _priority; } }			protected int _priority;
		public int function_code		{ get { return _function_code; } }			protected int _function_code;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int effect_chance		{ get { return _effect_chance; } }			protected int _effect_chance;
		public int real_description		{ get { return _real_description; } }			protected int _real_description;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "moves.dat";
		public const string PBS_BASE_FILENAME = "moves";
		SCHEMA = {
			"SectionName"  => new {:id,               "m"},
			"Name"         => new {:real_name,        "s"},
			"Type"         => new {:type,             "e", :Type},
			"Category"     => new {:category,         "e", new {"Physical", "Special", "Status"}},
			"Power"        => new {:power,            "u"},
			"Accuracy"     => new {:accuracy,         "u"},
			"TotalPP"      => new {:total_pp,         "u"},
			"Target"       => new {:target,           "e", :Target},
			"Priority"     => new {:priority,         "i"},
			"FunctionCode" => new {:function_code,    "s"},
			"Flags"        => new {:flags,            "*s"},
			"EffectChance" => new {:effect_chance,    "u"},
			"Description"  => new {:real_description, "q"}
		}
		CATEGORY_ICON_SIZE = new {64, 28};

		extend ClassMethodsSymbols;
		include InstanceMethods;

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id               = hash.id;
			@real_name        = hash.real_name        || "Unnamed";
			@type             = hash.type             || :NONE;
			@category         = hash.category         || 2;
			@power            = hash.power            || 0;
			@accuracy         = hash.accuracy         || 100;
			@total_pp         = hash.total_pp         || 5;
			@target           = hash.target           || :None;
			@priority         = hash.priority         || 0;
			@function_code    = hash.function_code    || "None";
			@flags            = hash.flags            || [];
			if (!@flags.Length > 0) @flags            = [@flags];
			@effect_chance    = hash.effect_chance    || 0;
			@real_description = hash.real_description || "???";
			@s_file_suffix  = hash.s_file_suffix  || "";
		}

		// @return [String] the translated name of this move
		public void name() {
			return GetMessageFromHash(MessageTypes.MOVE_NAMES, @real_name);
		}

		// @return [String] the translated description of this move
		public void description() {
			return GetMessageFromHash(MessageTypes.MOVE_DESCRIPTIONS, @real_description);
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		public bool physical() {
			if (@power == 0) return false;
			if (Settings.MOVE_CATEGORY_PER_MOVE) return @category == 0;
			return GameData.Type.get(@type).physical();
		}

		public bool special() {
			if (@power == 0) return false;
			if (Settings.MOVE_CATEGORY_PER_MOVE) return @category == 1;
			return GameData.Type.get(@type).special();
		}

		public bool damaging() {
			return @category != 2;
		}

		public bool status() {
			return @category == 2;
		}

		public bool hidden_move() {
			foreach (var i in GameData.Item) { //'GameData.Item.each' do => |i|
				if (i.is_HM() && i.move == @id) return true;
			}
			return false;
		}

		public void display_type(pkmn, move = null) {
=begin;
			switch (@function_code) {
				case "TypeDependsOnUserIVs":
					return HiddenPower(pkmn)[0];
				case "TypeAndPowerDependOnUserBerry":
					item_data = pkmn.item;
					if (item_data) {
						foreach (var flag in item_data.flags) { //'item_data.flags.each' do => |flag|
							if (!System.Text.RegularExpressions.Regex.IsMatch(flag,@"^NaturalGift_(\w+)_(?:\d+)$",RegexOptions.IgnoreCase)) continue;
							typ = $~[1].to_sym;
							if (GameData.Type.exists(typ)) ret = typ;
							break;
						}
					}
					return :NORMAL;
				case "TypeDependsOnUserPlate":
					item_types = {
						FISTPLATE   = :FIGHTING,
						SKYPLATE    = :FLYING,
						TOXICPLATE  = :POISON,
						EARTHPLATE  = :GROUND,
						STONEPLATE  = :ROCK,
						INSECTPLATE = :BUG,
						SPOOKYPLATE = :GHOST,
						IRONPLATE   = :STEEL,
						FLAMEPLATE  = :FIRE,
						SPLASHPLATE = :WATER,
						MEADOWPLATE = :GRASS,
						ZAPPLATE    = :ELECTRIC,
						MINDPLATE   = :PSYCHIC,
						ICICLEPLATE = :ICE,
						DRACOPLATE  = :DRAGON,
						DREADPLATE  = :DARK,
						PIXIEPLATE  = :FAIRY;
					}
					if (pkmn.hasItem()) {
						item_types.each do |item, item_type|
							if (pkmn.item_id == item && GameData.Type.exists(item_type)) return item_type;
						}
					}
					break;
				case "TypeDependsOnUserMemory":
					item_types = {
						FIGHTINGMEMORY = :FIGHTING,
						FLYINGMEMORY   = :FLYING,
						POISONMEMORY   = :POISON,
						GROUNDMEMORY   = :GROUND,
						ROCKMEMORY     = :ROCK,
						BUGMEMORY      = :BUG,
						GHOSTMEMORY    = :GHOST,
						STEELMEMORY    = :STEEL,
						FIREMEMORY     = :FIRE,
						WATERMEMORY    = :WATER,
						GRASSMEMORY    = :GRASS,
						ELECTRICMEMORY = :ELECTRIC,
						PSYCHICMEMORY  = :PSYCHIC,
						ICEMEMORY      = :ICE,
						DRAGONMEMORY   = :DRAGON,
						DARKMEMORY     = :DARK,
						FAIRYMEMORY    = :FAIRY;
					}
					if (pkmn.hasItem()) {
						item_types.each do |item, item_type|
							if (pkmn.item_id == item && GameData.Type.exists(item_type)) return item_type;
						}
					}
					break;
				case "TypeDependsOnUserDrive":
					item_types = {
						SHOCKDRIVE = :ELECTRIC,
						BURNDRIVE  = :FIRE,
						CHILLDRIVE = :ICE,
						DOUSEDRIVE = :WATER;
					}
					if (pkmn.hasItem()) {
						item_types.each do |item, item_type|
							if (pkmn.item_id == item && GameData.Type.exists(item_type)) return item_type;
						}
					}
					break;
				case "TypeIsUserFirstType":
					return pkmn.types[0];
			}
=end;
			return @type;
		}

		public void display_damage(pkmn, move = null) {
=begin;
			switch (@function_code) {
				case "TypeDependsOnUserIVs":
					return HiddenPower(pkmn)[1];
				case "TypeAndPowerDependOnUserBerry":
					item_data = pkmn.item;
					if (item_data) {
						foreach (var flag in item_data.flags) { //'item_data.flags.each' do => |flag|
							if (System.Text.RegularExpressions.Regex.IsMatch(flag,@"^NaturalGift_(?:\w+)_(\d+)$",RegexOptions.IgnoreCase)) return (int)Math.Max($~[1].ToInt(), 10);
						}
					}
					return 1;
				case "ThrowUserItemAtTarget":
					item_data = pkmn.item;
					if (item_data) {
						foreach (var flag in item_data.flags) { //'item_data.flags.each' do => |flag|
							if (System.Text.RegularExpressions.Regex.IsMatch(flag,@"^Fling_(\d+)$",RegexOptions.IgnoreCase)) return (int)Math.Max($~[1].ToInt(), 10);
						}
						return 10;
					}
					return 0;
				case "PowerHigherWithUserHP":
					return (int)Math.Max(150 * pkmn.hp / pkmn.totalhp, 1);
				case "PowerLowerWithUserHP":
					n = 48 * pkmn.hp / pkmn.totalhp;
					if (n < 2) return 200;
					if (n < 5) return 150;
					if (n < 10) return 100;
					if (n < 17) return 80;
					if (n < 33) return 40;
					return 20;
				case "PowerHigherWithUserHappiness":
					return (int)Math.Max((int)Math.Floor(pkmn.happiness * 2 / 5), 1);
				case "PowerLowerWithUserHappiness":
					return (int)Math.Max((int)Math.Floor((255 - pkmn.happiness) * 2 / 5), 1);
				case "PowerHigherWithLessPP":
					dmgs = new {200, 80, 60, 50, 40};
					ppLeft = (int)Math.Min((int)Math.Max((move&.pp || @total_pp) - 1, 0), dmgs.length - 1);
					return dmgs[ppLeft];
			}
=end;
			return @power;
		}

		public void display_category(pkmn, move = null) {return @category; }
		public void display_accuracy(pkmn, move = null) {return @accuracy; }

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			ret = __orig__get_property_for_PBS(key);
			if (new []{"Power", "Priority", "EffectChance"}.Contains(key) && ret == 0) ret = null;
			return ret;
		}
	}
}
