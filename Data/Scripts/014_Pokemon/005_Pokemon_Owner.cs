//===============================================================================
// Stores information about a Pokémon's owner.
//===============================================================================
public partial class Pokemon {
	public partial class Owner {
		/// <summary>the ID of the owner</summary>
		public Integer id		{ get { return _id; } }			private Integer _id;
		/// <summary>the name of the owner</summary>
		public String name		{ get { return _name; } }			private String _name;
		/// <summary>the gender of the owner (0 = male, 1 = female, 2 = unknown)</summary>
		public Integer gender		{ get { return _gender; } }			private Integer _gender;
		/// <summary>the language of the owner (see GetLanguage for language IDs)</summary>
		public Integer language		{ get { return _language; } }			private Integer _language;

		/// <param name="id">the ID of the owner</param>
		/// <param name="name">the name of the owner</param>
		/// <param name="gender">the gender of the owner (0 = male, 1 = female, 2 = unknown)</param>
		/// <param name="language">the language of the owner (see GetLanguage for language IDs)</param>
		public void initialize(Integer id, String name, Integer gender, Integer language) {
			validate id => Integer, name => String, gender => Integer, language => Integer;
			@id = id;
			@name = name;
			@gender = gender;
			@language = language;
		}

		// Returns a new Owner object populated with values taken from +trainer+.
		/// <param name="trainer">trainer object to read data from | Player, NPCTrainer</param>
		// @return [Owner] new Owner object
		public static void new_from_trainer(Player trainer) {
			validate trainer => new {Player, NPCTrainer};
			return new(trainer.id, trainer.name, trainer.gender, trainer.language);
		}

		// Returns an Owner object with a foreign ID.
		/// <param name="name">owner name</param>
		/// <param name="gender">owner gender</param>
		/// <param name="language">owner language</param>
		// @return [Owner] foreign Owner object
		public static void new_foreign(String name = "", Integer gender = 2, Integer language = 2) {
			return new(Game.GameData.player.make_foreign_ID, name, gender, language);
		}

		// @param new_id [Integer] new owner ID
		public int id { set {
			validate new_id => Integer;
			@id = new_id;
			}
		}

		// @param new_name [String] new owner name
		public int name { set {
			validate new_name => String;
			@name = new_name;
			}
		}

		// @param new_gender [Integer] new owner gender
		public int gender { set {
			validate new_gender => Integer;
			@gender = new_gender;
			}
		}

		// @param new_language [Integer] new owner language
		public int language { set {
			validate new_language => Integer;
			@language = new_language;
			}
		}

		// @return [Integer] the public portion of the owner's ID
		public void public_id() {
			return @id & 0xFFFF;
		}
	}
}
