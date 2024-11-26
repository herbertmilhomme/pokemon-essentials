//==============================================================================#
//                              Pokémon Essentials                              #
//                                 Version 21.1                                 #
//                https://github.com/Maruno17/pokemon-essentials                #
//==============================================================================#

public static partial class Settings {
	// The version of your game. It has to adhere to the MAJOR.MINOR.PATCH format.
	public const string GAME_VERSION = "1.0.0";

	// The generation that the battle system follows. Used throughout the battle
	// scripts, and also by some other settings which are used in and out of battle
	// (you can of course change those settings to suit your game).
	// Note that this isn't perfect. Essentials doesn't accurately replicate every
	// single generation's mechanics. It's considered to be good enough. Only
	// generations 5 and later are reasonably supported.
	public const int MECHANICS_GENERATION = 9;

	//-----------------------------------------------------------------------------
	// Credits
	//-----------------------------------------------------------------------------

	// Your game's credits, in an array. You can allow certain lines to be
	// translated by wrapping them in _INTL() as shown. Blank lines are just "".
	// To split a line into two columns, put "<s>" in it. Plugin credits and
	// Essentials engine credits are added to the end of these credits
	// automatically.
	public static void game_credits() {
		return new {
			_INTL("My Game by:"),
			"Maruno",
			"",
			_INTL("Also involved were:"),
			"A. Lee Uss<s>Anne O'Nymus",
			"Ecksam Pell<s>Jane Doe",
			"Joe Dan<s>Nick Nayme",
			"Sue Donnim<s>",
			"",
			_INTL("Special thanks to:"),
			"Pizza";
		}
	}

	//-----------------------------------------------------------------------------
	// The player and NPCs
	//-----------------------------------------------------------------------------

	// The maximum amount of money the player can have.
	public const int MAX_MONEY            = 999_999;
	// The maximum number of Game Corner coins the player can have.
	public const int MAX_COINS            = 99_999;
	// The maximum number of Battle Points the player can have.
	public const int MAX_BATTLE_POINTS    = 9_999;
	// The maximum amount of soot the player can have.
	public const int MAX_SOOT             = 9_999;
	// The maximum length, in characters, that the player's name can be.
	public const int MAX_PLAYER_NAME_SIZE = 10;
	// A set of arrays each containing a trainer type followed by a Game Variable
	// number. If the Variable isn't set to 0, then all trainers with the
	// associated trainer type will be named as whatever is in that Variable.
	RIVAL_NAMES = new {
		new {:RIVAL1,   12},
		new {:RIVAL2,   12},
		new {:CHAMPION, 12}
	}

	//-----------------------------------------------------------------------------
	// Overworld
	//-----------------------------------------------------------------------------

	// Whether outdoor maps should be shaded according to the time of day.
	public const bool TIME_SHADING               = true;
	// Whether the reflections of the player/events will ripple horizontally.
	public const bool ANIMATE_REFLECTIONS        = true;
	// Whether planted berries grow according to Gen 4 mechanics (true) or Gen 3
	// mechanics (false).
	public const bool NEW_BERRY_PLANTS           = (MECHANICS_GENERATION >= 4)
	// Whether fishing automatically hooks the Pokémon (true), or whether there is
	// a reaction test first (false).
	public const bool FISHING_AUTO_HOOK          = false;
	// The ID of the common event that runs when the player starts fishing (runs
	// instead of showing the casting animation).
	public const int FISHING_BEGIN_COMMON_EVENT = -1;
	// The ID of the common event that runs when the player stops fishing (runs
	// instead of showing the reeling in animation).
	public const int FISHING_END_COMMON_EVENT   = -1;
	// The number of steps allowed before a Safari Zone game is over (0=infinite).
	public const int SAFARI_STEPS               = 600;
	// The number of seconds a Bug-Catching Contest lasts for (0=infinite).
	public const int BUG_CONTEST_TIME           = 20 * 60;   // 20 minutes
	// Pairs of map IDs, where the location signpost isn't shown when moving from
	// one of the maps in a pair to the other (and vice versa). Useful for single
	// long routes/towns that are spread over multiple maps.
	//   e.g. [4,5,16,17,42,43] will be map pairs 4,5 and 16,17 and 42,43.
	// Moving between two maps that have the exact same name won't show the
	// location signpost anyway, so you don't need to list those maps here.
	NO_SIGNPOSTS               = new List<string>();
	// Whether poisoned Pokémon will lose HP while walking around in the field.
	public const bool POISON_IN_FIELD            = (MECHANICS_GENERATION <= 4)
	// Whether poisoned Pokémon will faint while walking around in the field
	// (true), or survive the poisoning with 1 HP (false).
	public const bool POISON_FAINT_IN_FIELD      = (MECHANICS_GENERATION <= 3)

	//-----------------------------------------------------------------------------
	// Using moves in the overworld
	//-----------------------------------------------------------------------------
	// Whether you need at least a certain number of badges to use some hidden
	// moves in the field (true), or whether you need one specific badge to use
	// them (false). The amounts/specific badges are defined below.
	public const bool FIELD_MOVES_COUNT_BADGES = true;
	// Depending on FIELD_MOVES_COUNT_BADGES, either the number of badges required
	// to use each hidden move in the field, or the specific badge number required
	// to use each move. Remember that badge 0 is the first badge, badge 1 is the
	// second badge, etc.
	//   e.g. To require the second badge, put false and 1.
	//        To require at least 2 badges, put true and 2.
	public const int BADGE_FOR_CUT       = 1;
	public const int BADGE_FOR_FLASH     = 2;
	public const int BADGE_FOR_ROCKSMASH = 3;
	public const int BADGE_FOR_SURF      = 4;
	public const int BADGE_FOR_FLY       = 5;
	public const int BADGE_FOR_STRENGTH  = 6;
	public const int BADGE_FOR_DIVE      = 7;
	public const int BADGE_FOR_WATERFALL = 8;

	//-----------------------------------------------------------------------------
	// Pokémon
	//-----------------------------------------------------------------------------

	// The maximum level Pokémon can reach.
	public const int MAXIMUM_LEVEL                       = 100;
	// The level of newly hatched Pokémon.
	public const int EGG_LEVEL                           = 1;
	// The odds of a newly generated Pokémon being shiny (out of 65536).
	public const int SHINY_POKEMON_CHANCE                = (MECHANICS_GENERATION >= 6) ? 16 : 8
	// Whether super shininess is enabled (uses a different shiny animation).
	public const bool SUPER_SHINY                         = (MECHANICS_GENERATION == 8)
	// Whether Pokémon with the "Legendary", "Mythical" or "Ultra Beast" flags will
	// have at least 3 perfect IVs.
	public const bool LEGENDARIES_HAVE_SOME_PERFECT_IVS   = (MECHANICS_GENERATION >= 6)
	// The odds of a wild Pokémon/bred egg having Pokérus (out of 65536).
	public const int POKERUS_CHANCE                      = 3;
	// Whether IVs and EVs are treated as 0 when calculating a Pokémon's stats.
	// IVs and EVs still exist, and are used by Hidden Power and some cosmetic
	// things as normal.
	public const bool DISABLE_IVS_AND_EVS                 = false;
	// Whether the Move Relearner can also teach egg moves that the Pokémon knew
	// when it hatched and moves that the Pokémon was once taught by a TR. Moves
	// from the Pokémon's level-up moveset of the same or a lower level than the
	// Pokémon can always be relearned.
	public const bool MOVE_RELEARNER_CAN_TEACH_MORE_MOVES = (MECHANICS_GENERATION >= 6)

	//-----------------------------------------------------------------------------
	// Breeding Pokémon and Day Care
	//-----------------------------------------------------------------------------

	// Whether Pokémon in the Day Care gain Exp for each step the player takes.
	// This should be true for the Day Care and false for the Pokémon Nursery, both
	// of which use the same code in Essentials.
	public const bool DAY_CARE_POKEMON_GAIN_EXP_FROM_WALKING     = (MECHANICS_GENERATION <= 6)
	// Whether two Pokémon in the Day Care can learn egg moves from each other if
	// they are the same species.
	public const bool DAY_CARE_POKEMON_CAN_SHARE_EGG_MOVES       = (MECHANICS_GENERATION >= 8)
	// Whether a bred baby Pokémon can inherit any TM/TR/HM moves from its father.
	// It can never inherit TM/TR/HM moves from its mother.
	public const bool BREEDING_CAN_INHERIT_MACHINE_MOVES         = (MECHANICS_GENERATION <= 5)
	// Whether a bred baby Pokémon can inherit egg moves from its mother. It can
	// always inherit egg moves from its father.
	public const bool BREEDING_CAN_INHERIT_EGG_MOVES_FROM_MOTHER = (MECHANICS_GENERATION >= 6)

	//-----------------------------------------------------------------------------
	// Roaming Pokémon
	//-----------------------------------------------------------------------------

	// A list of maps used by roaming Pokémon. Each map has an array of other maps
	// it can lead to.
	ROAMING_AREAS = {
		5  => new {   21, 28, 31, 39, 41, 44, 47, 66, 69},
		21 => new {5,     28, 31, 39, 41, 44, 47, 66, 69},
		28 => new {5, 21,     31, 39, 41, 44, 47, 66, 69},
		31 => new {5, 21, 28,     39, 41, 44, 47, 66, 69},
		39 => new {5, 21, 28, 31,     41, 44, 47, 66, 69},
		41 => new {5, 21, 28, 31, 39,     44, 47, 66, 69},
		44 => new {5, 21, 28, 31, 39, 41,     47, 66, 69},
		47 => new {5, 21, 28, 31, 39, 41, 44,     66, 69},
		66 => new {5, 21, 28, 31, 39, 41, 44, 47,     69},
		69 => new {5, 21, 28, 31, 39, 41, 44, 47, 66    }
	}
	// A set of arrays, each containing the details of a roaming Pokémon. The
	// information within each array is as follows:
	//   * Species.
	//   * Level.
	//   * Game Switch; the Pokémon roams while this is ON.
	//   * Encounter type (see def RoamingMethodAllowed for their use):
	//       0 = grass, walking in cave, surfing
	//       1 = grass, walking in cave
	//       2 = surfing
	//       3 = fishing
	//       4 = surfing, fishing
	//   * Name of BGM to play for that encounter (optional).
	//   * Roaming areas specifically for this Pokémon (optional; used instead of
	//     ROAMING_AREAS).
	ROAMING_SPECIES = new {
		new {:LATIAS, 30, 53, 0, "Battle roaming"},
		new {:LATIOS, 30, 53, 0, "Battle roaming"},
		new {:KYOGRE, 40, 54, 2, null, {
			2  => new {   21, 31    },
			21 => new {2,     31, 69},
			31 => new {2, 21,     69},
			69 => new {   21, 31    }
		}},
		new {:ENTEI, 40, 55, 1}
	}

	//-----------------------------------------------------------------------------
	// Party and Pokémon storage
	//-----------------------------------------------------------------------------

	// The maximum number of Pokémon that can be in the party.
	public const int MAX_PARTY_SIZE      = 6;
	// The number of boxes in Pokémon storage.
	public const int NUM_STORAGE_BOXES   = 40;
	// Whether putting a Pokémon into Pokémon storage will heal it. If false, they
	// are healed by the Recover All: Entire Party event command (at Poké Centers).
	public const bool HEAL_STORED_POKEMON = (MECHANICS_GENERATION <= 7)

	//-----------------------------------------------------------------------------
	// Items
	//-----------------------------------------------------------------------------

	// Whether various HP-healing items heal the amounts they do in Gen 7+ (true)
	// or in earlier Generations (false).
	public const bool REBALANCED_HEALING_ITEM_AMOUNTS      = (MECHANICS_GENERATION >= 7)
	// Whether vitamins can add EVs no matter how many that stat already has in it
	// (true), or whether they can't make that stat's EVs greater than 100 (false).
	public const bool NO_VITAMIN_EV_CAP                    = (MECHANICS_GENERATION >= 8)
	// Whether Rage Candy Bar acts as a Full Heal (true) or a Potion (false).
	public const bool RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS = (MECHANICS_GENERATION >= 7)
	// Whether the Black/White Flutes will raise/lower the levels of wild Pokémon
	// respectively (true), or will lower/raise the wild encounter rate
	// respectively (false).
	public const bool FLUTES_CHANGE_WILD_ENCOUNTER_LEVELS  = (MECHANICS_GENERATION >= 6)
	// Whether Rare Candy can be used on a Pokémon that is already at its maximum
	// level if it is able to evolve by level-up (if so, triggers that evolution).
	public const bool RARE_CANDY_USABLE_AT_MAX_LEVEL       = (MECHANICS_GENERATION >= 8)
	// Whether the player can choose how many of an item to use at once on a
	// Pokémon. This applies to Exp-changing items (Rare Candy, Exp Candies) and
	// EV-changing items (vitamins, feathers, EV-lowering berries).
	public const bool USE_MULTIPLE_STAT_ITEMS_AT_ONCE      = (MECHANICS_GENERATION >= 8)
	// If a move taught by a TM/HM/TR replaces another move, this setting is
	// whether the machine's move retains the replaced move's PP (true), or whether
	// the machine's move has full PP (false).
	public const bool TAUGHT_MACHINES_KEEP_OLD_PP          = (MECHANICS_GENERATION == 5)
	// Whether you get 1 Premier Ball for every 10 of any kind of Poké Ball bought
	// from a Mart at once (true), or 1 Premier Ball for buying 10+ regular Poké
	// Balls (false).
	public const bool MORE_BONUS_PREMIER_BALLS             = (MECHANICS_GENERATION >= 8)
	// The default sell price of an item to a Poké Mart is its buy price divided by
	// this number.
	public const bool ITEM_SELL_PRICE_DIVISOR              = (MECHANICS_GENERATION >= 9) ? 4 : 2

	//-----------------------------------------------------------------------------
	// Bag
	//-----------------------------------------------------------------------------

	// The names of each pocket of the Bag.
	public static void bag_pocket_names() {
		return new {
			_INTL("Items"),
			_INTL("Medicine"),
			_INTL("Poké Balls"),
			_INTL("TMs & HMs"),
			_INTL("Berries"),
			_INTL("Mail"),
			_INTL("Battle Items"),
			_INTL("Key Items");
		}
	}
	// The maximum number of slots per pocket (-1 means infinite number).
	public const int[] BAG_MAX_POCKET_SIZE  = new int[] {-1, -1, -1, -1, -1, -1, -1, -1}
	// Whether each pocket in turn auto-sorts itself by the order items are defined
	// in the PBS file items.txt.
	public const bool[] BAG_POCKET_AUTO_SORT = new bool[] {false, false, false, true, true, false, false, false}
	// The maximum number of items each slot in the Bag can hold.
	public const int BAG_MAX_PER_SLOT     = 999;

	//-----------------------------------------------------------------------------
	// Pokédex
	//-----------------------------------------------------------------------------

	// The names of the Pokédex lists, in the order they are defined in the PBS
	// file "regional_dexes.txt". The last name is for the National Dex and is
	// added onto the end of this array.
	// Each entry is either just a name, or is an array containing a name and a
	// number. If there is a number, it is a region number as defined in
	// town_map.txt. If there is no number, the number of the region the player is
	// currently in will be used. The region number determines which Town Map is
	// shown in the Area page when viewing that Pokédex list.
	public static void pokedex_names() {
		return new {
			new {_INTL("Kanto Pokédex"), 0},
			new {_INTL("Johto Pokédex"), 1},
			_INTL("National Pokédex");
		}
	}
	// Whether the Pokédex list shown is the one for the player's current region
	// (true), or whether a menu pops up for the player to manually choose which
	// Dex list to view if more than one is available (false).
	public const bool USE_CURRENT_REGION_DEX                    = false;
	// Whether all forms of a given species will be immediately available to view
	// in the Pokédex so long as that species has been seen at all (true), or
	// whether each form needs to be seen specifically before that form appears in
	// the Pokédex (false).
	public const bool DEX_SHOWS_ALL_FORMS                       = false;
	// An array of numbers, where each number is that of a Dex list (in the same
	// order as above, except the National Dex is -1). All Dex lists included here
	// will begin their numbering at 0 rather than 1 (e.g. Victini in Unova's Dex).
	DEXES_WITH_OFFSETS                        = new List<string>();
	// Whether the Pokédex entry of a newly owned species will be shown after it
	// hatches from an egg, after it evolves and after obtaining it from a trade,
	// in addition to after catching it in battle.
	public const bool SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN = (MECHANICS_GENERATION >= 7)

	//-----------------------------------------------------------------------------
	// Town Map
	//-----------------------------------------------------------------------------

	// A set of arrays, each containing details of a graphic to be shown on the
	// region map if appropriate. The values for each array are as follows:
	//   * Region number.
	//   * Game Switch; the graphic is shown if this is ON (non-wall maps only).
	//   * X coordinate of the graphic on the map, in squares.
	//   * Y coordinate of the graphic on the map, in squares.
	//   * Name of the graphic, found in the Graphics/UI/Town Map folder.
	//   * The graphic will always (true) or never (false) be shown on a wall map.
	REGION_MAP_EXTRAS = new {
		new {0, 51, 16, 15, "hidden_Berth", false},
		new {0, 52, 20, 14, "hidden_Faraday", false}
	}
	// Whether the player can use Fly while looking at the Town Map. This is only
	// allowed if the player can use Fly normally.
	public const bool CAN_FLY_FROM_TOWN_MAP = true;

	//-----------------------------------------------------------------------------
	// Phone
	//-----------------------------------------------------------------------------

	// The default setting for Phone.rematches_enabled, which determines whether
	// trainers registered in the Phone can become ready for a rematch. If false,
	// Phone.rematches_enabled = true will enable rematches at any point you want.
	public const bool PHONE_REMATCHES_POSSIBLE_FROM_BEGINNING     = false;
	// Whether the messages in a phone call with a trainer are colored blue or red
	// depending on that trainer's gender. Note that this doesn't apply to contacts
	// whose phone calls are in a Common Event; they will need to be colored
	// manually in their Common Events.
	public const bool COLOR_PHONE_CALL_MESSAGES_BY_CONTACT_GENDER = true;

	//-----------------------------------------------------------------------------
	// Battle starting
	//-----------------------------------------------------------------------------

	// Whether Repel uses the level of the first Pokémon in the party regardless of
	// its HP (true), or it uses the level of the first unfainted Pokémon (false).
	public const bool REPEL_COUNTS_FAINTED_POKEMON             = (MECHANICS_GENERATION >= 6)
	// Whether more abilities affect whether wild Pokémon appear, which Pokémon
	// they are, etc.
	public const bool MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS    = (MECHANICS_GENERATION >= 8)
	// Whether shiny wild Pokémon are more likely to appear if the player has
	// previously defeated/caught lots of other Pokémon of the same species.
	public const bool HIGHER_SHINY_CHANCES_WITH_NUMBER_BATTLED = (MECHANICS_GENERATION >= 8)
	// Whether overworld weather can set the default terrain effect in battle.
	// Storm weather sets Electric Terrain, and fog weather sets Misty Terrain.
	public const bool OVERWORLD_WEATHER_SETS_BATTLE_TERRAIN    = (MECHANICS_GENERATION >= 8)

	//-----------------------------------------------------------------------------
	// Game Switches
	//-----------------------------------------------------------------------------

	// The Game Switch that is set to ON when the player blacks out.
	public const int STARTING_OVER_SWITCH      = 1;
	// The Game Switch that is set to ON when the player has seen Pokérus in the
	// Poké Center (and doesn't need to be told about it again).
	public const int SEEN_POKERUS_SWITCH       = 2;
	// The Game Switch which, while ON, makes all wild Pokémon created be shiny.
	public const int SHINY_WILD_POKEMON_SWITCH = 31;
	// The Game Switch which, while ON, makes all Pokémon created considered to be
	// met via a fateful encounter.
	public const int FATEFUL_ENCOUNTER_SWITCH  = 32;
	// The Game Switch which, while ON, disables the effect of the Pokémon Box Link
	// and prevents the player from accessing Pokémon storage via the party screen
	// with it.
	public const int DISABLE_BOX_LINK_SWITCH   = 35;

	//-----------------------------------------------------------------------------
	// Overworld animation IDs
	//-----------------------------------------------------------------------------

	// ID of the animation played when the player steps on grass (grass rustling).
	public const int GRASS_ANIMATION_ID           = 1;
	// ID of the animation played when the player lands on the ground after hopping
	// over a ledge (shows a dust impact).
	public const int DUST_ANIMATION_ID            = 2;
	// ID of the animation played when the player finishes taking a step onto still
	// water (shows a water ripple).
	public const int WATER_RIPPLE_ANIMATION_ID    = 8;
	// ID of the animation played when a trainer notices the player (an exclamation
	// bubble).
	public const int EXCLAMATION_ANIMATION_ID     = 3;
	// ID of the animation played when a patch of grass rustles due to using the
	// Poké Radar.
	public const int RUSTLE_NORMAL_ANIMATION_ID   = 1;
	// ID of the animation played when a patch of grass rustles vigorously due to
	// using the Poké Radar. (Rarer species)
	public const int RUSTLE_VIGOROUS_ANIMATION_ID = 5;
	// ID of the animation played when a patch of grass rustles and shines due to
	// using the Poké Radar. (Shiny encounter)
	public const int RUSTLE_SHINY_ANIMATION_ID    = 6;
	// ID of the animation played when a berry tree grows a stage while the player
	// is on the map (for new plant growth mechanics only).
	public const int PLANT_SPARKLE_ANIMATION_ID   = 7;

	//-----------------------------------------------------------------------------
	// Languages
	//-----------------------------------------------------------------------------

	// An array of available languages in the game. Each one is an array containing
	// the display name of the language in-game, and that language's filename
	// fragment. A language will use the language data files from the Data folder
	// called messages_FRAGMENT_core.dat and messages_FRAGMENT_game.dat (if they
	// exist).
	LANGUAGES = new {
//    new {"English", "english"},
//    new {"Deutsch", "deutsch"}
	}

	//-----------------------------------------------------------------------------
	// Screen size and zoom
	//-----------------------------------------------------------------------------

	// The default screen width (at a scale of 1.0). You should also edit the
	// property "defScreenW" in mkxp.json to match.
	public const int SCREEN_WIDTH  = 512;
	// The default screen height (at a scale of 1.0). You should also edit the
	// property "defScreenH" in mkxp.json to match.
	public const int SCREEN_HEIGHT = 384;
	// The default screen scale factor. Possible values are 0.5, 1.0, 1.5 and 2.0.
	public const int SCREEN_SCALE  = 1.0;

	//-----------------------------------------------------------------------------
	// Messages
	//-----------------------------------------------------------------------------

	// Available speech frames. These are graphic files in "Graphics/Windowskins/".
	SPEECH_WINDOWSKINS = new {
		"speech hgss 1",
		"speech hgss 2",
		"speech hgss 3",
		"speech hgss 4",
		"speech hgss 5",
		"speech hgss 6",
		"speech hgss 7",
		"speech hgss 8",
		"speech hgss 9",
		"speech hgss 10",
		"speech hgss 11",
		"speech hgss 12",
		"speech hgss 13",
		"speech hgss 14",
		"speech hgss 15",
		"speech hgss 16",
		"speech hgss 17",
		"speech hgss 18",
		"speech hgss 19",
		"speech hgss 20",
		"speech pl 18";
	}
	// Available menu frames. These are graphic files in "Graphics/Windowskins/".
	MENU_WINDOWSKINS = new {
		"choice 1",
		"choice 2",
		"choice 3",
		"choice 4",
		"choice 5",
		"choice 6",
		"choice 7",
		"choice 8",
		"choice 9",
		"choice 10",
		"choice 11",
		"choice 12",
		"choice 13",
		"choice 14",
		"choice 15",
		"choice 16",
		"choice 17",
		"choice 18",
		"choice 19",
		"choice 20",
		"choice 21",
		"choice 22",
		"choice 23",
		"choice 24",
		"choice 25",
		"choice 26",
		"choice 27",
		"choice 28";
	}

	//-----------------------------------------------------------------------------
	// Debug helpers
	//-----------------------------------------------------------------------------

	// Whether the game will ask you if you want to fully compile every time you
	// start the game (in Debug mode). You will not need to hold Ctrl/Shift to
	// compile anything.
	public const bool PROMPT_TO_COMPILE    = false;
	// Whether the game will skip the intro splash screens and title screen, and go
	// straight to the Continue/New Game screen. Only applies to playing in Debug
	// mode.
	public const bool SKIP_TITLE_SCREEN    = true;
	// Whether the game will skip the Continue/New Game screen and go straight into
	// a saved game (if there is one) or start a new game (if there isn't). Only
	// applies to playing in Debug mode.
	public const bool SKIP_CONTINUE_SCREEN = false;
}

//===============================================================================
// DO NOT EDIT THESE!
//===============================================================================
public static partial class Essentials {
	public const string VERSION = "21.1";
	public const string ERROR_TEXT = "";
	public const string MKXPZ_VERSION = "2.4.2/c9378cf";
}
