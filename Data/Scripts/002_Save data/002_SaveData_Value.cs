//===============================================================================
//
//===============================================================================
public static partial class SaveData {
	// Contains Value objects for each save element.
	// Populated during runtime by SaveData.register calls.
	// @type [Array<Value>]
	@values = new List<string>();

	//=============================================================================
	// An error raised if an invalid save value is being saved or loaded.
	//=============================================================================
	public partial class InvalidValueError : RuntimeError { }

	//=============================================================================
	// Represents a single value in save data.
	// New values are added using {SaveData.register}.
	//=============================================================================
	public partial class Value {
		/// <summary>the value id</summary>
		public Symbol id		{ get { return _id; } }			private Symbol _id;

		/// <param name="id">value id</param>
		public void initialize(Symbol id, Action block = null) {
			validate id => Symbol, block => Proc;
			@id = id;
			@loaded = false;
			@load_in_bootup = false;
			@reset_on_new_game = false;
			instance_eval(&block);
			if (@save_proc.null()) raise $"No save_value defined for save value {id.inspect}";
			if (@load_proc.null()) raise $"No load_value defined for save value {id.inspect}";
		}

		/// <param name="value">value to check</param>
		// @return [Boolean] whether the given value is valid
		public bool valid(Object value) {
			if (@ensured_class.null()) return true;
			return value.is_a(Object.const_get(@ensured_class));
		}

		// Calls the value's load proc with the given argument passed into it.
		/// <param name="value">load proc argument</param>
		// @raise [InvalidValueError] if an invalid value is being loaded
		public void load(Object value) {
			validate_value(value);
			@load_proc.call(value);
			@loaded = true;
		}

		// Calls the value's save proc and returns its value.
		// @return [Object] save proc value
		// @raise [InvalidValueError] if an invalid value is being saved
		public void save() {
			value = @save_proc.call;
			validate_value(value);
			return value;
		}

		// @return [Boolean] whether the value has a new game value proc defined
		public bool has_new_game_proc() {
			return @new_game_value_proc.is_a(Proc);
		}

		// Calls the save value's load proc with the value fetched
		// from the defined new game value proc.
		// @raise (see #load)
		public void load_new_game_value() {
			unless (self.has_new_game_proc()) {
				Debug.LogError($"Save value {@id.inspect} has no new_game_value defined");
				//throw new ArgumentException($"Save value {@id.inspect} has no new_game_value defined");
			}
			self.load(@new_game_value_proc.call);
		}

		// @return [Boolean] whether the value should be loaded during bootup
		public bool load_in_bootup() {
			return @load_in_bootup;
		}

		public void reset_on_new_game() {
			@reset_on_new_game = true;
		}

		public bool reset_on_new_game() {
			return @reset_on_new_game;
		}

		// @return [Boolean] whether the value has been loaded
		public bool loaded() {
			return @loaded;
		}

		// Marks value as unloaded.
		public void mark_as_unloaded() {
			@loaded = false;
		}

		// Uses the {#from_old_format} proc to select the correct data from
		// +old_format+ and return it.
		// Returns null if the proc is undefined.
		/// <param name="old_format">old format to load value from</param>
		// @return [Object] data from the old format
		public void get_from_old_format(Array old_format) {
			if (@old_format_get_proc.null()) return null;
			return @old_format_get_proc.call(old_format);
		}

		//---------------------------------------------------------------------------

		private;

		// Raises an {InvalidValueError} if the given value is invalid.
		/// <param name="value">value to check</param>
		// @raise [InvalidValueError] if the value is invalid
		public void validate_value(Object value) {
			if (self.valid(value)) return;
			Debug.LogError($"Save value {@id.inspect} is not a {@ensured_class} ({value.class.name} given)");
			//throw new InvalidValueException($"Save value {@id.inspect} is not a {@ensured_class} ({value.class.name} given)");
		}

		// @!group Configuration

		// If present, ensures that the value is of the given class.
		/// <param name="class_name">class to enforce</param>
		/// <see cref="SaveData.register"/>
		public void ensure_class(Symbol class_name) {
			validate class_name => Symbol;
			@ensured_class = class_name;
		}

		// Defines how the loaded value is placed into a global variable.
		// Requires a block with the loaded value as its parameter.
		/// <see cref="SaveData.register"/>
		public void load_value(Action block = null) {
			unless (block_given()) Debug.LogError("No block given to load_value");
			//throw new Exception("No block given to load_value" unless block_given());
			@load_proc = block;
		}

		// Defines what is saved into save data. Requires a block.
		/// <see cref="SaveData.register"/>
		public void save_value(Action block = null) {
			unless (block_given()) Debug.LogError("No block given to save_value");
			//throw new Exception("No block given to save_value" unless block_given());
			@save_proc = block;
		}

		// If present, defines what the value is set to at the start of a new game.
		/// <see cref="SaveData.register"/>
		public void new_game_value(Action block = null) {
			unless (block_given()) Debug.LogError("No block given to new_game_value");
			//throw new Exception("No block given to new_game_value" unless block_given());
			@new_game_value_proc = block;
		}

		// If present, sets the value to be loaded during bootup.
		/// <see cref="SaveData.register"/>
		public void load_in_bootup() {
			@load_in_bootup = true;
		}

		// If present, defines how the value should be fetched from the pre-v19
		// save format. Requires a block with the old format array as its parameter.
		/// <see cref="SaveData.register"/>
		public void from_old_format(Action block = null) {
			unless (block_given()) Debug.LogError("No block given to from_old_format");
			//throw new Exception("No block given to from_old_format" unless block_given());
			@old_format_get_proc = block;
		}

		// @!endgroup
	}

	//---------------------------------------------------------------------------

	// Registers a {Value} to be saved into save data.
	// Takes a block which defines the value's saving ({Value#save_value})
	// and loading ({Value#load_value}) procedures.
	//
	// It is also possible to provide a proc for fetching the value
	// from the pre-v19 format ({Value#from_old_format}), define
	// a value to be set upon starting a new game with {Value#new_game_value}
	// and ensure that the saved and loaded value is of the correct
	// class with {Value#ensure_class}.
	//
	// Values can be registered to be loaded on bootup with
	// {Value#load_in_bootup}. If a new_game_value proc is defined, it
	// will be called when the game is launched for the first time,
	// or if the save data does not contain the value in question.
	//
	// @example Registering a new value
	//   SaveData.register(:foo) do
	//     ensure_class :Foo
	//     save_value { Game.GameData.foo }
	//     load_value { |value| Game.GameData.foo = value }
	//     new_game_value { new Foo() }
	//   }
	// @example Registering a value to be loaded on bootup
	//   SaveData.register(:bar) do
	//     load_in_bootup
	//     save_value { Game.GameData.bar }
	//     load_value { |value| Game.GameData.bar = value }
	//     new_game_value { new Bar() }
	//   }
	/// <param name="id">value id</param>
	// @yield the block of code to be saved as a Value
	public static void register(Symbol id, Action block = null) {
		validate id => Symbol;
		unless (block_given()) {
			Debug.LogError("No block given to SaveData.register");
			//throw new Exception("No block given to SaveData.register");
		}
		@values << new Value(id, &block);
	}

	public static void unregister(id) {
		validate id => Symbol;
		@values.delete_if(value => value.id == id);
	}

	/// <param name="save_data">save data to validate</param>
	// @return [Boolean] whether the given save data is valid
	public static bool valid(Hash save_data) {
		validate save_data => Hash;
		return @values.all(value => value.valid(save_data[value.id]));
	}

	// Loads values from the given save data.
	// An optional condition can be passed.
	/// <param name="save_data">save data to load from</param>
	/// <param name="condition_block">optional condition</param>
	// @api private
	public static void load_values(Hash save_data, &Proc condition_block) {
		@values.each do |value|
			if (block_given() && !condition_block.call(value)) continue;
			if (save_data.has_key(value.id)) {
				value.load(save_data[value.id]);
			} else if (value.has_new_game_proc()) {
				value.load_new_game_value;
			}
		}
	}

	// Loads the values from the given save data by
	// calling each {Value} object's {Value#load_value} proc.
	// Values that are already loaded are skipped.
	// If a value does not exist in the save data and has
	// a {Value#new_game_value} proc defined, that value
	// is loaded instead.
	/// <param name="save_data">save data to load</param>
	// @raise [InvalidValueError] if an invalid value is being loaded
	public static void load_all_values(Hash save_data) {
		validate save_data => Hash;
		load_values(save_data, value => { !value.loaded(); });
	}

	// Marks all values that aren't loaded on bootup as unloaded.
	public static void mark_values_as_unloaded() {
		@values.each do |value|
			if (!value.load_in_bootup() || value.reset_on_new_game()) value.mark_as_unloaded;
		}
	}

	// Loads each value from the given save data that has
	// been set to be loaded during bootup. Done when a save file exists.
	/// <param name="save_data">save data to load</param>
	// @raise [InvalidValueError] if an invalid value is being loaded
	public static void load_bootup_values(Hash save_data) {
		validate save_data => Hash;
		load_values(save_data, value => { !value.loaded() && value.load_in_bootup(); });
	}

	// Goes through each value with {Value#load_in_bootup} enabled and loads their
	// new game value, if one is defined. Done when no save file exists.
	public static void initialize_bootup_values() {
		@values.each do |value|
			unless (value.load_in_bootup()) continue;
			if (value.has_new_game_proc() && !value.loaded()) value.load_new_game_value;
		}
	}

	// Loads each {Value}'s new game value, if one is defined. Done when starting a
	// new game.
	public static void load_new_game_values() {
		@values.each do |value|
			if (value.has_new_game_proc() && (!value.loaded() || value.reset_on_new_game())) value.load_new_game_value;
		}
	}

	// @return [Hash{Symbol => Object}] a hash representation of the save data
	// @raise [InvalidValueError] if an invalid value is being saved
	public static void compile_save_hash() {
		save_data = new List<string>();
		@values.each(value => save_data[value.id] = value.save);
		return save_data;
	}
}
