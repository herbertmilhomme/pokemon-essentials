//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class BerryPlant {
		public int id		{ get { return _id; } }			protected int _id;
		public int hours_per_stage		{ get { return _hours_per_stage; } }			protected int _hours_per_stage;
		public int drying_per_hour		{ get { return _drying_per_hour; } }			protected int _drying_per_hour;
		public int yield		{ get { return _yield; } }			protected int _yield;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "berry_plants.dat";
		public const string PBS_BASE_FILENAME = "berry_plants";
		SCHEMA = {
			"SectionName"   => new {:id,              "m"},
			"HoursPerStage" => new {:hours_per_stage, "v"},
			"DryingPerHour" => new {:drying_per_hour, "u"},
			"Yield"         => new {:yield,           "uv"}
		}
		public const int NUMBER_OF_REPLANTS           = 9;
		public const int NUMBER_OF_GROWTH_STAGES      = 4;
		public const int NUMBER_OF_FULLY_GROWN_STAGES = 4;
		WATERING_CANS                = new {:SPRAYDUCK, :SQUIRTBOTTLE, :WAILMERPAIL, :SPRINKLOTAD};

		extend ClassMethodsSymbols;
		include InstanceMethods;

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@hours_per_stage = hash.hours_per_stage || 3;
			@drying_per_hour = hash.drying_per_hour || 15;
			@yield           = hash.yield           || new {2, 5};
			if (@yield[1] < @yield[0]) @yield.reverse!;
			@s_file_suffix = hash.s_file_suffix || "";
		}

		public void minimum_yield() {
			return @yield[0];
		}

		public void maximum_yield() {
			return @yield[1];
		}
	}
}
