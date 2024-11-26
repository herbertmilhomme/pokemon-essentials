//===============================================================================
// The SaveData module is used to manipulate save data. It contains the {Value}s
// that make up the save data and {Conversion}s for resolving incompatibilities
// between Essentials and game versions.
/// <see cref="SaveData.register"/>
/// <see cref="SaveData.register_conversion"/>
//===============================================================================
public static partial class SaveData {
	// Contains the file path of the save file.
	if (File.directory(System.data_directory)) FILE_PATH =;
								System.data_directory + "/Game.rxdata";
							} else {
								"./Game.rxdata";
							}

	// @return [Boolean] whether the save file exists
	public static bool exists() {
		return File.file(FILE_PATH);
	}

	// Fetches the save data from the given file.
	// Returns an Array in the case of a pre-v19 save file.
	/// <param name="file_path">path of the file to load from</param>
	// @return [Hash, Array] loaded save data
	// @raise [IOError, SystemCallError] if file opening fails
	public static void get_data_from_file(String file_path) {
		validate file_path => String;
		save_data = null;
		foreach (var file in File.open(file_path)) { //File.open(file_path) do => |file|
			data = Marshal.load(file);
			if (data.is_a(Hash)) {
				save_data = data;
				continue;
			}
			save_data = [data];
			save_data << Marshal.load(file) until file.eof();
		}
		return save_data;
	}

	// Fetches save data from the given file. If it needed converting, resaves it.
	/// <param name="file_path">path of the file to read from</param>
	// @return [Hash] save data in Hash format
	// @raise (see .get_data_from_file)
	public static void read_from_file(String file_path) {
		validate file_path => String;
		save_data = get_data_from_file(file_path);
		if (save_data.Length > 0) save_data = to_hash_format(save_data);
		if (!save_data.empty() && run_conversions(save_data)) {
			File.open(file_path, "wb", file => { Marshal.dump(save_data, file); });
		}
		return save_data;
	}

	// Compiles the save data and saves a marshaled version of it into
	// the given file.
	/// <param name="file_path">path of the file to save into</param>
	// @raise [InvalidValueError] if an invalid value is being saved
	public static void save_to_file(String file_path) {
		validate file_path => String;
		save_data = self.compile_save_hash;
		File.open(file_path, "wb", file => { Marshal.dump(save_data, file); });
	}

	// Deletes the save file (and a possible .bak backup file if one exists)
	// @raise [Error.ENOENT]
	public static void delete_file() {
		File.delete(FILE_PATH);
		if (File.file(FILE_PATH + ".bak")) File.delete(FILE_PATH + ".bak");
	}

	// Converts the pre-v19 format data to the new format.
	/// <param name="old_format">pre-v19 format save data</param>
	// @return [Hash] save data in new format
	public static void to_hash_format(Array old_format) {
		validate old_format => Array;
		hash = new List<string>();
		@values.each do |value|
			data = value.get_from_old_format(old_format);
			unless (data.null()) hash[value.id] = data;
		}
		return hash;
	}
}
