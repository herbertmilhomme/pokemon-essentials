//===============================================================================
//
//===============================================================================
using System;

public static partial class GameData {
	public partial class PhoneMessage {
		public int id		{ get { return _id; } }			protected int _id;
		public int trainer_type		{ get { return _trainer_type; } set { _trainer_type = value; } }			protected int _trainer_type;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int version      { get { return _version; } }			protected int _version;
		public int intro		{ get { return _intro; } set { _intro = value; } }			protected int _intro;
		public int intro_morning		{ get { return _intro_morning; } }			protected int _intro_morning;
		public int intro_afternoon		{ get { return _intro_afternoon; } }			protected int _intro_afternoon;
		public int intro_evening		{ get { return _intro_evening; } }			protected int _intro_evening;
		public int body		{ get { return _body; } set { _body = value; } }			protected int _body;
		public int body1		{ get { return _body1; } }			protected int _body1;
		public int body2		{ get { return _body2; } }			protected int _body2;
		public int battle_request		{ get { return _battle_request; } set { _battle_request = value; } }			protected int _battle_request;
		public int battle_remind		{ get { return _battle_remind; } }			protected int _battle_remind;
		public int end		{ get { return _end; } }			protected int _end;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "phone.dat";
		public const string PBS_BASE_FILENAME = "phone";
		SCHEMA = {
			"SectionName"    => new {:id,              "q"},
			"Intro"          => new {:intro,           "^q"},
			"IntroMorning"   => new {:intro_morning,   "^q"},
			"IntroAfternoon" => new {:intro_afternoon, "^q"},
			"IntroEvening"   => new {:intro_evening,   "^q"},
			"Body"           => new {:body,            "^q"},
			"Body1"          => new {:body1,           "^q"},
			"Body2"          => new {:body2,           "^q"},
			"BattleRequest"  => new {:battle_request,  "^q"},
			"BattleRemind"   => new {:battle_remind,   "^q"},
			"End"            => new {:end,             "^q"}
		}

		extend ClassMethodsSymbols;
		include InstanceMethods;

		/// <param name="tr_type"> | Symbol, String</param>
		/// <param name="tr_name">only null for the default message set | String, null</param>
		// @param tr_version [Integer, null]
		// @return [Boolean] whether the given other is defined as a self
		public static bool exists(Symbol tr_type, String tr_name = null, tr_version = 0) {
			if (tr_type.Length > 0) {
				tr_name = tr_type[1];
				tr_version = tr_type[2];
				tr_type = tr_type[0];
			}
			validate tr_type => [Symbol, String];
			validate tr_name => [String, NilClass];
			key = [tr_type.to_sym, tr_name, tr_version];
			if (key[1].null()) key = key[0];
			return !self.DATA[key].null();
		}

		/// <param name="tr_type"> | Symbol, String</param>
		/// <param name="tr_name"></param>
		/// <param name="tr_version"> | Integer, null</param>
		// @return [self]
		public static void get(Symbol tr_type, String tr_name, Integer tr_version = 0) {
			validate tr_type => [Symbol, String];
			validate tr_name => [String];
			key = [tr_type.to_sym, tr_name, tr_version];
			Debug.LogError($"Phone messages not found for {tr_type} {tr_name} {tr_version}." unless self.DATA.has_key(key));
			//throw new Exception($"Phone messages not found for {tr_type} {tr_name} {tr_version}." unless self.DATA.has_key(key));
			return self.DATA[key];
		}

		/// <param name="tr_type"> | Symbol, String</param>
		/// <param name="tr_name"></param>
		/// <param name="tr_version"> | Integer, null</param>
		// @return [self, null]
		public static void try_get(Symbol tr_type, String tr_name, Integer tr_version = 0) {
			validate tr_type => [Symbol, String];
			validate tr_name => [String];
			key = [tr_type.to_sym, tr_name, tr_version];
			return (self.DATA.has_key(key)) ? self.DATA[key] : null;
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@trainer_type    = hash.trainer_type;
			@real_name       = hash.real_name;
			@version         = hash.version         || 0;
			@intro           = hash.intro;
			@intro_morning   = hash.intro_morning;
			@intro_afternoon = hash.intro_afternoon;
			@intro_evening   = hash.intro_evening;
			@body            = hash.body;
			@body1           = hash.body1;
			@body2           = hash.body2;
			@battle_request  = hash.battle_request;
			@battle_remind   = hash.battle_remind;
			@end             = hash.end;
			@s_file_suffix = hash.s_file_suffix || "";
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			if (key == "SectionName") {
				if (@id == "default") return "Default";
				ret = new {@trainer_type, @real_name, (@version > 0) ? @version : null};
				return ret.compact.join(",");
			}
			return __orig__get_property_for_PBS(key);
		}
	}
}
