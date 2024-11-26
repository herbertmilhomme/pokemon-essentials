//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Type {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		/// <summary>Where this type's icon is within types.png</summary>
		public int icon_position		{ get { return _icon_position; } }			protected int _icon_position;
		public int special_type		{ get { return _special_type; } }			protected int _special_type;
		public int pseudo_type		{ get { return _pseudo_type; } }			protected int _pseudo_type;
		public int weaknesses		{ get { return _weaknesses; } }			protected int _weaknesses;
		public int resistances		{ get { return _resistances; } }			protected int _resistances;
		public int immunities		{ get { return _immunities; } }			protected int _immunities;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "types.dat";
		public const string PBS_BASE_FILENAME = "types";
		SCHEMA = {
			"SectionName"   => new {:id,            "m"},
			"Name"          => new {:real_name,     "s"},
			"IconPosition"  => new {:icon_position, "u"},
			"IsSpecialType" => new {:special_type,  "b"},
			"IsPseudoType"  => new {:pseudo_type,   "b"},
			"Weaknesses"    => new {:weaknesses,    "*m"},
			"Resistances"   => new {:resistances,   "*m"},
			"Immunities"    => new {:immunities,    "*m"},
			"Flags"         => new {:flags,         "*s"}
		}
		ICON_SIZE = new {64, 28};

		extend ClassMethodsSymbols;
		include InstanceMethods;

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@real_name       = hash.real_name       || "Unnamed";
			@icon_position   = hash.icon_position   || 0;
			@special_type    = hash.special_type    || false;
			@pseudo_type     = hash.pseudo_type     || false;
			@weaknesses      = hash.weaknesses      || [];
			if (!@weaknesses.Length > 0) @weaknesses      = [@weaknesses];
			@resistances     = hash.resistances     || [];
			if (!@resistances.Length > 0) @resistances     = [@resistances];
			@immunities      = hash.immunities      || [];
			if (!@immunities.Length > 0) @immunities      = [@immunities];
			@flags           = hash.flags           || [];
			@s_file_suffix = hash.s_file_suffix || "";
		}

		// @return [String] the translated name of this item
		public void name() {
			return GetMessageFromHash(MessageTypes.TYPE_NAMES, @real_name);
		}

		public bool physical() { return !@special_type; }
		public bool special() {  return @special_type; }

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		public void effectiveness(other_type) {
			if (!other_type) return Effectiveness.NORMAL_EFFECTIVE;
			if (@weaknesses.Contains(other_type)) return Effectiveness.SUPER_EFFECTIVE;
			if (@resistances.Contains(other_type)) return Effectiveness.NOT_VERY_EFFECTIVE;
			if (@immunities.Contains(other_type)) return Effectiveness.INEFFECTIVE;
			return Effectiveness.NORMAL_EFFECTIVE;
		}
	}
}

//===============================================================================
//
//===============================================================================
public static partial class Effectiveness {
	public const int INEFFECTIVE                   = 0;
	public const int NOT_VERY_EFFECTIVE            = 1;
	public const int NORMAL_EFFECTIVE              = 2;
	public const int SUPER_EFFECTIVE               = 4;
	INEFFECTIVE_MULTIPLIER        = INEFFECTIVE.to_f / NORMAL_EFFECTIVE;
	NOT_VERY_EFFECTIVE_MULTIPLIER = NOT_VERY_EFFECTIVE.to_f / NORMAL_EFFECTIVE;
	public const int NORMAL_EFFECTIVE_MULTIPLIER   = 1.0;
	SUPER_EFFECTIVE_MULTIPLIER    = SUPER_EFFECTIVE.to_f / NORMAL_EFFECTIVE;

	#region Class Functions
	#endregion

	public bool ineffective(value) {
		return value == INEFFECTIVE_MULTIPLIER;
	}

	public bool not_very_effective(value) {
		return value > INEFFECTIVE_MULTIPLIER && value < NORMAL_EFFECTIVE_MULTIPLIER;
	}

	public bool resistant(value) {
		return value < NORMAL_EFFECTIVE_MULTIPLIER;
	}

	public bool normal(value) {
		return value == NORMAL_EFFECTIVE_MULTIPLIER;
	}

	public bool super_effective(value) {
		return value > NORMAL_EFFECTIVE_MULTIPLIER;
	}

	public bool ineffective_type(attack_type, *defend_types) {
		value = calculate(attack_type, *defend_types);
		return ineffective(value);
	}

	public bool not_very_effective_type(attack_type, *defend_types) {
		value = calculate(attack_type, *defend_types);
		return not_very_effective(value);
	}

	public bool resistant_type(attack_type, *defend_types) {
		value = calculate(attack_type, *defend_types);
		return resistant(value);
	}

	public bool normal_type(attack_type, *defend_types) {
		value = calculate(attack_type, *defend_types);
		return normal(value);
	}

	public bool super_effective_type(attack_type, *defend_types) {
		value = calculate(attack_type, *defend_types);
		return super_effective(value);
	}

	public void get_type_effectiveness(attack_type, defend_type) {
		return GameData.Type.get(defend_type).effectiveness(attack_type);
	}

	public void calculate(attack_type, *defend_types) {
		ret = NORMAL_EFFECTIVE_MULTIPLIER;
		foreach (var type in defend_types) { //'defend_types.each' do => |type|
			ret *= get_type_effectiveness(attack_type, type) / NORMAL_EFFECTIVE.to_f;
		}
		return ret;
	}
}
