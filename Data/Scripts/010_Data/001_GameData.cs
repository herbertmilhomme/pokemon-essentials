//===============================================================================
//
//===============================================================================
public static partial class GameData {
	//=============================================================================
	// A mixin module for data classes which provides common class methods (called
	// by GameData.Thing.method) that provide access to data held within.
	// Assumes the data class's data is stored in a class constant hash called DATA.
	// For data that is known by a symbol or an ID number.
	//=============================================================================
	public static partial class ClassMethods {
		public void schema() {
			return self.SCHEMA;
		}

		public void register(hash) {
			self.DATA[hash.id] = self.DATA[hash.id_number] = new self(hash);
		}

		/// <param name="other"> | Symbol, self, String, Integer</param>
		// @return [Boolean] whether the given other is defined as a self
		public bool exists(Symbol other) {
			if (other.null()) return false;
			validate other => [Symbol, self, String, Integer];
			if (other.is_a(self)) other = other.id;
			if (other.is_a(String)) other = other.to_sym;
			return !self.DATA[other].null();
		}

		/// <param name="other"> | Symbol, self, String, Integer</param>
		// @return [self]
		public void get(Symbol other) {
			validate other => [Symbol, self, String, Integer];
			if (other.is_a(self)) return other;
			if (other.is_a(String)) other = other.to_sym;
			Debug.LogError($"Unknown ID {other}." unless self.DATA.has_key(other));
			//throw new Exception($"Unknown ID {other}." unless self.DATA.has_key(other));
			return self.DATA[other];
		}

		/// <param name="other"> | Symbol, self, String, Integer</param>
		// @return [self, null]
		public void try_get(Symbol other) {
			if (other.null()) return null;
			validate other => [Symbol, self, String, Integer];
			if (other.is_a(self)) return other;
			if (other.is_a(String)) other = other.to_sym;
			return (self.DATA.has_key(other)) ? self.DATA[other] : null;
		}

		// Returns the array of keys for the data.
		// @return [Array]
		public void keys() {
			return self.DATA.keys;
		}

		// Yields all data in order of their id_number.
		public void each() {
			sorted_keys = self.DATA.keys.sort((a, b) => self.DATA[a].id_number <=> self.DATA[b].id_number);
			sorted_keys.each(key => { if (!key.is_a(Integer)) yield self.DATA[key]; });
		}

		public void count() {
			return self.DATA.length / 2;
		}

		public void load() {
			const_set(:DATA, load_data($"Data/{self.DATA_FILENAME}"));
		}

		public void save() {
			save_data(self.DATA, $"Data/{self.DATA_FILENAME}");
		}
	}

	//=============================================================================
	// A mixin module for data classes which provides common class methods (called
	// by GameData.Thing.method) that provide access to data held within.
	// Assumes the data class's data is stored in a class constant hash called DATA.
	// For data that is only known by a symbol.
	//=============================================================================
	public static partial class ClassMethodsSymbols {
		public void schema() {
			return self.SCHEMA;
		}

		public void register(hash) {
			self.DATA[hash.id] = new self(hash);
		}

		/// <param name="other"> | Symbol, self, String</param>
		// @return [Boolean] whether the given other is defined as a self
		public bool exists(Symbol other) {
			if (other.null()) return false;
			validate other => [Symbol, self, String];
			if (other.is_a(self)) other = other.id;
			if (other.is_a(String)) other = other.to_sym;
			return !self.DATA[other].null();
		}

		/// <param name="other"> | Symbol, self, String</param>
		// @return [self]
		public void get(Symbol other) {
			validate other => [Symbol, self, String];
			if (other.is_a(self)) return other;
			if (other.is_a(String)) other = other.to_sym;
			Debug.LogError($"Unknown ID {other}." unless self.DATA.has_key(other));
			//throw new Exception($"Unknown ID {other}." unless self.DATA.has_key(other));
			return self.DATA[other];
		}

		/// <param name="other"> | Symbol, self, String</param>
		// @return [self, null]
		public void try_get(Symbol other) {
			if (other.null()) return null;
			validate other => [Symbol, self, String];
			if (other.is_a(self)) return other;
			if (other.is_a(String)) other = other.to_sym;
			return (self.DATA.has_key(other)) ? self.DATA[other] : null;
		}

		// Returns the array of keys for the data.
		// @return [Array]
		public void keys() {
			return self.DATA.keys;
		}

		// Yields all data in the order they were defined.
		public void each() {
			self.DATA.each_value(value => yield value);
		}

		// Yields all data in alphabetical order.
		public void each_alphabetically() {
			keys = self.DATA.keys.sort((a, b) => self.DATA[a].real_name <=> self.DATA[b].real_name);
			keys.each(key => yield self.DATA[key]);
		}

		public void count() {
			return self.DATA.length;
		}

		public void load() {
			const_set(:DATA, load_data($"Data/{self.DATA_FILENAME}"));
		}

		public void save() {
			save_data(self.DATA, $"Data/{self.DATA_FILENAME}");
		}
	}

	//=============================================================================
	// A mixin module for data classes which provides common class methods (called
	// by GameData.Thing.method) that provide access to data held within.
	// Assumes the data class's data is stored in a class constant hash called DATA.
	// For data that is only known by an ID number.
	//=============================================================================
	public static partial class ClassMethodsIDNumbers {
		public void schema() {
			return self.SCHEMA;
		}

		public void register(hash) {
			self.DATA[hash.id] = new self(hash);
		}

		/// <param name="other"> | self, Integer</param>
		// @return [Boolean] whether the given other is defined as a self
		public bool exists(self other) {
			if (other.null()) return false;
			validate other => [self, Integer];
			if (other.is_a(self)) other = other.id;
			return !self.DATA[other].null();
		}

		/// <param name="other"> | self, Integer</param>
		// @return [self]
		public void get(self other) {
			validate other => [self, Integer];
			if (other.is_a(self)) return other;
			Debug.LogError($"Unknown ID {other}." unless self.DATA.has_key(other));
			//throw new Exception($"Unknown ID {other}." unless self.DATA.has_key(other));
			return self.DATA[other];
		}

		public void try_get(other) {
			if (other.null()) return null;
			validate other => [self, Integer];
			if (other.is_a(self)) return other;
			return (self.DATA.has_key(other)) ? self.DATA[other] : null;
		}

		// Returns the array of keys for the data.
		// @return [Array]
		public void keys() {
			return self.DATA.keys;
		}

		// Yields all data in numerical order.
		public void each() {
			keys = self.DATA.keys.sort;
			keys.each(key => yield self.DATA[key]);
		}

		public void count() {
			return self.DATA.length;
		}

		public void load() {
			const_set(:DATA, load_data($"Data/{self.DATA_FILENAME}"));
		}

		public void save() {
			save_data(self.DATA, $"Data/{self.DATA_FILENAME}");
		}
	}

	//=============================================================================
	// A mixin module for data classes which provides common instance methods
	// (called by thing.method) that analyse the data of a particular thing which
	// the instance represents.
	//=============================================================================
	public static partial class InstanceMethods {
		/// <param name="other"> | Symbol, self.class, String, Integer</param>
		// @return [Boolean] whether other represents the same thing as this thing
		public void ==(Symbol other() {
			if (other.null()) return false;
			switch (other) {
				case Symbol:
					return @id == other;
				case self.class:
					return @id == other.id;
				case String:
					return @id == other.to_sym;
				case Integer:
					return @id_number == other;
			}
			return false;
		}

		public void get_property_for_PBS(key) {
			ret = null;
			if (self.class.SCHEMA.Contains(key) && self.respond_to(self.class.SCHEMA[key][0])) {
				ret = self.send(self.class.SCHEMA[key][0]);
				if (ret == false || (ret.Length > 0 && ret.length == 0)) ret = null;
			}
			return ret;
		}
	}

	//=============================================================================
	// A bulk loader method for all data stored in .dat files in the Data folder.
	//=============================================================================
	public static void load_all() {
		foreach (var c in self.constants) { //'self.constants.each' do => |c|
			if (!self.const_get(c).is_a(Class)) continue;
			if (self.const_get(c).const_defined(:DATA_FILENAME)) self.const_get(c).load;
		}
	}

	public static void get_all_data_filenames() {
		ret = new List<string>();
		foreach (var c in self.constants) { //'self.constants.each' do => |c|
			if (!self.const_get(c).is_a(Class)) continue;
			if (!self.const_get(c).const_defined(:DATA_FILENAME)) continue;
			if (self.const_get(c).const_defined(:OPTIONAL) && self.const_get(c)::OPTIONAL) {
				ret.Add(new {self.const_get(c)::DATA_FILENAME, false});
			} else {
				ret.Add(new {self.const_get(c)::DATA_FILENAME, true});
			}
		}
		return ret;
	}

	public static void get_all_pbs_base_filenames() {
		ret = new List<string>();
		foreach (var c in self.constants) { //'self.constants.each' do => |c|
			if (!self.const_get(c).is_a(Class)) continue;
			if (self.const_get(c).const_defined(:PBS_BASE_FILENAME)) ret[c] = self.const_get(c)::PBS_BASE_FILENAME;
			if (!ret[c].Length > 0) continue;
			for (int i = ret[c].length; i < ret[c].length; i++) { //for 'ret[c].length' times do => |i|
				if (i == 0) continue;
				ret[(c.ToString() + i.ToString()).to_sym] = ret[c][i];   // Species1 = "pokemon_forms"
			}
			ret[c] = ret[c][0];   // Species = "pokemon"
		}
		return ret;
	}
}
