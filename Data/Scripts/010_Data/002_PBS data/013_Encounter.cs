//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Encounter {
		public int id		{ get { return _id; } set { _id = value; } }			protected int _id;
		public int map		{ get { return _map; } set { _map = value; } }			protected int _map;
		public int version		{ get { return _version; } set { _version = value; } }			protected int _version;
		public int step_chances		{ get { return _step_chances; } }			protected int _step_chances;
		public int types		{ get { return _types; } }			protected int _types;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "encounters.dat";
		public const string PBS_BASE_FILENAME = "encounters";

		extend ClassMethodsSymbols;
		include InstanceMethods;

		/// <param name="map_id"></param>
		/// <param name="map_version"> | Integer, null</param>
		// @return [Boolean] whether there is encounter data for the given map ID/version
		public static bool exists(Integer map_id, Integer map_version = 0) {
			validate map_id => [Integer];
			validate map_version => [Integer];
			key = string.Format("{0}_{0}", map_id, map_version).to_sym;
			return !self.DATA[key].null();
		}

		/// <param name="map_id"></param>
		/// <param name="map_version"> | Integer, null</param>
		// @return [self, null]
		public static void get(Integer map_id, Integer map_version = 0) {
			validate map_id => Integer;
			validate map_version => Integer;
			trial_key = string.Format("{0}_{0}", map_id, map_version).to_sym;
			key = (self.DATA.has_key(trial_key)) ? trial_key : string.Format("{0}_0", map_id).to_sym;
			return self.DATA[key];
		}

		// Yields all encounter data in order of their map and version numbers.
		public static void each() {
			keys = self.DATA.keys.sort do |a, b|
				if (self.DATA[a].map == self.DATA[b].map) {
					self.DATA[a].version <=> self.DATA[b].version;
				} else {
					self.DATA[a].map <=> self.DATA[b].map;
				}
			}
			keys.each(key => yield self.DATA[key]);
		}

		// Yields all encounter data for the given version. Also yields encounter
		// data for version 0 of a map if that map doesn't have encounter data for
		// the given version.
		public static void each_of_version(version = 0) {
			foreach (var data in self) { //'self.each' do => |data|
				if (data.version == version) yield data;
				if (version > 0 && data.version == 0 && !self.DATA.has_key(new {data.map, version})) {
					yield data;
				}
			}
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@map             = hash.map;
			@version         = hash.version         || 0;
			@step_chances    = hash.step_chances;
			@types           = hash.types           || {};
			@s_file_suffix = hash.s_file_suffix || "";
		}
	}
}
