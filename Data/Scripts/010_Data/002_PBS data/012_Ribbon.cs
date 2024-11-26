//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Ribbon {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		/// <summary>Where this ribbon's graphic is within ribbons.png</summary>
		public int icon_position		{ get { return _icon_position; } }			protected int _icon_position;
		public int real_description		{ get { return _real_description; } }			protected int _real_description;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "ribbons.dat";
		public const string PBS_BASE_FILENAME = "ribbons";
		SCHEMA = {
			"SectionName"  => new {:id,               "m"},
			"Name"         => new {:real_name,        "s"},
			"IconPosition" => new {:icon_position,    "u"},
			"Description"  => new {:real_description, "q"},
			"Flags"        => new {:flags,            "*s"}
		}

		extend ClassMethodsSymbols;
		include InstanceMethods;

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id               = hash.id;
			@real_name        = hash.real_name        || "Unnamed";
			@icon_position    = hash.icon_position    || 0;
			@real_description = hash.real_description || "???";
			@flags            = hash.flags            || [];
			@s_file_suffix  = hash.s_file_suffix  || "";
		}

		// @return [String] the translated name of this ribbon
		public void name() {
			return GetMessageFromHash(MessageTypes.RIBBON_NAMES, @real_name);
		}

		// @return [String] the translated description of this ribbon
		public void description() {
			return GetMessageFromHash(MessageTypes.RIBBON_DESCRIPTIONS, @real_description);
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}
	}
}
