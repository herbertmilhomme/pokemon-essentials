//===============================================================================
//
//===============================================================================
public static partial class SaveData {
	// Contains Conversion objects for each defined conversion:
	// {
	//   essentials = {
	//     '19'    => [<Conversion>, ...],
	//     '19.1'  => [<Conversion>, ...],
	//     ...
	//   },
	//   game = {
	//     '1.1.0' => [<Conversion>, ...],
	//     '1.2.0' => [<Conversion>, ...],
	//     ...
	//   }
	// }
	// Populated during runtime by SaveData.register_conversion calls.
	@conversions = {
		essentials: {},
		game:       {}
	}

	//=============================================================================
	// Represents a conversion made to save data.
	// New conversions are added using {SaveData.register_conversion}.
	//=============================================================================
	public partial class Conversion {
		/// <summary>conversion ID</summary>
		public Symbol id		{ get { return _id; } }			private Symbol _id;
		/// <summary>conversion title</summary>
		public String title		{ get { return _title; } }			private String _title;
		/// <summary>trigger type of the conversion (+:essentials+ or +:game+)</summary>
		public Symbol trigger_type		{ get { return _trigger_type; } }			private Symbol _trigger_type;
		/// <summary>trigger version of the conversion</summary>
		public String version		{ get { return _version; } }			private String _version;

		/// <param name="id">conversion ID</param>
		public void initialize(String id, Action block = null) {
			@id = id;
			@value_procs = new List<string>();
			@all_proc = null;
			@title = $"Running conversion {@id}";
			@trigger_type = null;
			@version = null;
			instance_eval(&block);
			if (@trigger_type.null() || @version.null()) {
				Debug.LogError($"Conversion {@id} is missing a condition");
				//throw new ArgumentException($"Conversion {@id} is missing a condition");
			}
		}

		// Returns whether the conversion should be run with the given version.
		/// <param name="version">version to check</param>
		// @return [Boolean] whether the conversion should be run
		public bool should_run(String version) {
			return PluginManager.compare_versions(version, @version) < 0;
		}

		// Runs the conversion on the given save data.
		/// <param name="save_data"></param>
		public void run(Hash save_data) {
			@value_procs.each do |value_id, proc|
				unless (save_data.has_key(value_id)) {
					Debug.LogError($"Save data does not have value {value_id.inspect}");
					//throw new ArgumentException($"Save data does not have value {value_id.inspect}");
				}
				proc.call(save_data[value_id]);
			}
			if (@all_proc.is_a(Proc)) @all_proc.call(save_data);
		}

		// Runs the conversion on the given object.
		// @param object
		/// <param name="key"></param>
		public void run_single(object, Symbol key) {
			if (@value_procs[key].is_a(Proc)) @value_procs[key].call(object);
		}

		//---------------------------------------------------------------------------

		private;

		// @!group Configuration

		// Sets the conversion's title.
		/// <param name="new_title">conversion title</param>
		// @note Since conversions are run before loading the player's chosen language,
		//   conversion titles can not be localized.
		/// <see cref="SaveData.register_conversion"/>
		public void display_title(String new_title) {
			validate new_title => String;
			@title = new_title;
		}

		// Sets the conversion to trigger for save files created below
		// the given Essentials version.
		/// <param name="version"> | Numeric, String</param>
		/// <see cref="SaveData.register_conversion"/>
		public void essentials_version(Numeric version) {
			validate version => [Numeric, String];
			Debug.LogError($"Multiple conditions in conversion {@id}" unless @version.null());
			//throw new Exception($"Multiple conditions in conversion {@id}" unless @version.null());
			@trigger_type = types.essentials;
			@version = version.ToString();
		}

		// Sets the conversion to trigger for save files created below
		// the given game version.
		/// <param name="version"> | Numeric, String</param>
		/// <see cref="SaveData.register_conversion"/>
		public void game_version(Numeric version) {
			validate version => [Numeric, String];
			Debug.LogError($"Multiple conditions in conversion {@id}" unless @version.null());
			//throw new Exception($"Multiple conditions in conversion {@id}" unless @version.null());
			@trigger_type = types.game;
			@version = version.ToString();
		}

		// Defines a conversion to the given save value.
		/// <param name="value_id">save value ID</param>
		/// <see cref="SaveData.register_conversion"/>
		public void to_value(Symbol value_id, Action block = null) {
			validate value_id => Symbol;
			unless (block_given()) Debug.LogError("No block given to to_value");
			//throw new Exception("No block given to to_value" unless block_given());
			if (@value_procs[value_id].is_a(Proc)) {
				Debug.LogError($"Multiple to_value definitions in conversion {@id} for {value_id}");
				//throw new ArgumentException($"Multiple to_value definitions in conversion {@id} for {value_id}");
			}
			@value_procs[value_id] = block;
		}

		// Defines a conversion to the entire save data.
		/// <see cref="SaveData.register_conversion"/>
		public void to_all(Action block = null) {
			unless (block_given()) Debug.LogError("No block given to to_all");
			//throw new Exception("No block given to to_all" unless block_given());
			if (@all_proc.is_a(Proc)) {
				Debug.LogError($"Multiple to_all definitions in conversion {@id}");
				//throw new ArgumentException($"Multiple to_all definitions in conversion {@id}");
			}
			@all_proc = block;
		}

		// @!endgroup
	}

	//---------------------------------------------------------------------------

	// Registers a {Conversion} to occur for save data that meets the given criteria.
	// Two types of criteria can be defined: {Conversion#essentials_version} and
	// {Conversion#game_version}. The conversion is automatically run on save data
	// that contains an older version number.
	//
	// A single value can be modified with {Conversion#to_value}. The entire save data
	// is accessed with {Conversion#to_all}, and a conversion title can be specified
	// with {Conversion#display_title}.
	// @example Registering a new conversion
	//   SaveData.register_conversion(:my_conversion) do
	//     game_version '1.1.0'
	//     display_title 'Converting some stuff'
	//     to_value :player do |player|
	//       // code that modifies the :player value
	//     }
	//     foreach (var save_data in to_all) { //to_all do => |save_data|
	//       save_data.new_value = new Foo()
	//     }
	//   }
	// @yield the block of code to be saved as a Conversion
	public static void register_conversion(id, Action block = null) {
		validate id => Symbol;
		unless (block_given()) {
			Debug.LogError("No block given to SaveData.register_conversion");
			//throw new Exception("No block given to SaveData.register_conversion");
		}
		conversion = new Conversion(id, &block);
		@conversions[conversion.trigger_type][conversion.version] ||= new List<string>();
		@conversions[conversion.trigger_type][conversion.version] << conversion;
	}

	/// <param name="save_data">save data to get conversions for</param>
	// @return [Array<Conversion>] all conversions that should be run on the data
	public static void get_conversions(Hash save_data) {
		conversions_to_run = new List<string>();
		versions = {
			essentials: save_data.essentials_version || "18.1",
			game:       save_data.game_version || "0.0.0";
		}
		[:essentials, :game].each do |trigger_type|
			// Ensure the versions are sorted from lowest to highest
			sorted_versions = @conversions[trigger_type].keys.sort do |v1, v2|
				PluginManager.compare_versions(v1, v2);
			}
			foreach (var version in sorted_versions) { //'sorted_versions.each' do => |version|
				@conversions[trigger_type][version].each do |conversion|
					unless (conversion.should_run(versions[trigger_type])) continue;
					conversions_to_run << conversion;
				}
			}
		}
		return conversions_to_run;
	}

	// Runs all possible conversions on the given save data.
	// Saves a backup before running conversions.
	/// <param name="save_data">save data to run conversions on</param>
	// @return [Boolean] whether conversions were run
	public static void run_conversions(Hash save_data) {
		validate save_data => Hash;
		conversions_to_run = self.get_conversions(save_data);
		if (conversions_to_run.none()) return false;
		File.open(SaveData.FILE_PATH + ".bak", "wb", f => { Marshal.dump(save_data, f); });
		Console.echo_h1(_INTL("Converting save file"));
		foreach (var conversion in conversions_to_run) { //'conversions_to_run.each' do => |conversion|
			Console.echo_li($"{conversion.title}...");
			conversion.run(save_data);
			Console.echo_done(true);
		}
		Console.echoln_li_done(_INTL("Successfully applied {1} save file conversion(s)", conversions_to_run.length));
		save_data.essentials_version = Essentials.VERSION;
		save_data.game_version = Settings.GAME_VERSION;
		return true;
	}

	// Runs all possible conversions on the given object.
	/// <param name="object">object to run conversions on</param>
	/// <param name="key">object's key in save data</param>
	/// <param name="save_data">save data to run conversions on</param>
	public static void run_single_conversions(Hash object, Hash key, Hash save_data) {
		validate key => Symbol;
		conversions_to_run = self.get_conversions(save_data);
		foreach (var conversion in conversions_to_run) { //'conversions_to_run.each' do => |conversion|
			conversion.run_single(object, key);
		}
	}
}
