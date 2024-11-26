//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class TownMap {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int filename		{ get { return _filename; } }			protected int _filename;
		public int point		{ get { return _point; } }			protected int _point;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "town_map.dat";
		public const string PBS_BASE_FILENAME = "town_map";
		SCHEMA = {
			"SectionName" => new {:id,        "u"},
			"Name"        => new {:real_name, "s"},
			"Filename"    => new {:filename,  "s"},
			"Point"       => new {:point,     "^uusSUUUU"},
			"Flags"       => new {:flags,     "*s"}
		}

		extend ClassMethodsIDNumbers;
		include InstanceMethods;

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@real_name       = hash.real_name       || "???";
			@filename        = hash.filename;
			@point           = hash.point           || [];
			@flags           = hash.flags           || [];
			@s_file_suffix = hash.s_file_suffix || "";
		}

		// @return [String] the translated name of this region
		public void name() {
			return GetMessageFromHash(MessageTypes.REGION_NAMES, @real_name);
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}
	}
}
