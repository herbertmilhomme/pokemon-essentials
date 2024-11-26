//===============================================================================
// Global metadata not specific to a map.  This class holds field state data that
// span multiple maps.
//===============================================================================
public partial class PokemonGlobalMetadata {
	// Movement
	public int bicycle		{ get { return _bicycle; } set { _bicycle = value; } }			protected int _bicycle;
	public int surfing		{ get { return _surfing; } set { _surfing = value; } }			protected int _surfing;
	public int diving		{ get { return _diving; } set { _diving = value; } }			protected int _diving;
	public int ice_sliding		{ get { return _ice_sliding; } set { _ice_sliding = value; } }			protected int _ice_sliding;
	public int descending_waterfall		{ get { return _descending_waterfall; } set { _descending_waterfall = value; } }			protected int _descending_waterfall;
	public int ascending_waterfall		{ get { return _ascending_waterfall; } set { _ascending_waterfall = value; } }			protected int _ascending_waterfall;
	public int fishing		{ get { return _fishing; } set { _fishing = value; } }			protected int _fishing;
	// Player data
	public int startTime		{ get { return _startTime; } set { _startTime = value; } }			protected int _startTime;
	public int stepcount		{ get { return _stepcount; } set { _stepcount = value; } }			protected int _stepcount;
	public int pcItemStorage		{ get { return _pcItemStorage; } set { _pcItemStorage = value; } }			protected int _pcItemStorage;
	public int mailbox		{ get { return _mailbox; } set { _mailbox = value; } }			protected int _mailbox;
	public int phone		{ get { return _phone; } set { _phone = value; } }			protected int _phone;
	public int partner		{ get { return _partner; } set { _partner = value; } }			protected int _partner;
	public int creditsPlayed		{ get { return _creditsPlayed; } set { _creditsPlayed = value; } }			protected int _creditsPlayed;
	// Pokédex
	/// <summary>Dex currently looking at (-1 is National Dex)</summary>
	public int pokedexDex		{ get { return _pokedexDex; } set { _pokedexDex = value; } }			protected int _pokedexDex;
	/// <summary>Last species viewed per Dex</summary>
	public int pokedexIndex		{ get { return _pokedexIndex; } set { _pokedexIndex = value; } }			protected int _pokedexIndex;
	/// <summary>Search mode</summary>
	public int pokedexMode		{ get { return _pokedexMode; } set { _pokedexMode = value; } }			protected int _pokedexMode;
	// Day Care
	public int day_care		{ get { return _day_care; } set { _day_care = value; } }			protected int _day_care;
	// Special battle modes
	public int safariState		{ get { return _safariState; } set { _safariState = value; } }			protected int _safariState;
	public int bugContestState		{ get { return _bugContestState; } set { _bugContestState = value; } }			protected int _bugContestState;
	public int challenge		{ get { return _challenge; } set { _challenge = value; } }			protected int _challenge;
	/// <summary>Saved recording of a battle</summary>
	public int lastbattle		{ get { return _lastbattle; } set { _lastbattle = value; } }			protected int _lastbattle;
	// Events
	public int eventvars		{ get { return _eventvars; } set { _eventvars = value; } }			protected int _eventvars;
	// Affecting the map
	public int bridge		{ get { return _bridge; } set { _bridge = value; } }			protected int _bridge;
	public int repel		{ get { return _repel; } set { _repel = value; } }			protected int _repel;
	public int flashUsed		{ get { return _flashUsed; } set { _flashUsed = value; } }			protected int _flashUsed;
	public int encounter_version		{ get { return _encounter_version; } }			protected int _encounter_version;
	// Map transfers
	public int healingSpot		{ get { return _healingSpot; } set { _healingSpot = value; } }			protected int _healingSpot;
	public int escapePoint		{ get { return _escapePoint; } set { _escapePoint = value; } }			protected int _escapePoint;
	public int pokecenterMapId		{ get { return _pokecenterMapId; } set { _pokecenterMapId = value; } }			protected int _pokecenterMapId;
	public int pokecenterX		{ get { return _pokecenterX; } set { _pokecenterX = value; } }			protected int _pokecenterX;
	public int pokecenterY		{ get { return _pokecenterY; } set { _pokecenterY = value; } }			protected int _pokecenterY;
	public int pokecenterDirection		{ get { return _pokecenterDirection; } set { _pokecenterDirection = value; } }			protected int _pokecenterDirection;
	// Movement history
	public int visitedMaps		{ get { return _visitedMaps; } set { _visitedMaps = value; } }			protected int _visitedMaps;
	public int mapTrail		{ get { return _mapTrail; } set { _mapTrail = value; } }			protected int _mapTrail;
	// Counters
	public int happinessSteps		{ get { return _happinessSteps; } set { _happinessSteps = value; } }			protected int _happinessSteps;
	public int pokerusTime		{ get { return _pokerusTime; } set { _pokerusTime = value; } }			protected int _pokerusTime;
	// Save file
	public int safesave		{ get { return _safesave; } set { _safesave = value; } }			protected int _safesave;

	public void initialize() {
		// Movement
		@bicycle              = false;
		@surfing              = false;
		@diving               = false;
		@ice_sliding          = false;
		@descending_waterfall = false;
		@ascending_waterfall  = false;
		@fishing              = false;
		// Player data
		@startTime            = Time.now;
		@stepcount            = 0;
		@pcItemStorage        = null;
		@mailbox              = null;
		@phone                = new Phone();
		@partner              = null;
		@creditsPlayed        = false;
		// Pokédex
		numRegions            = LoadRegionalDexes.length;
		@pokedexDex           = (numRegions == 0) ? -1 : 0;
		@pokedexIndex         = new List<string>();
		@pokedexMode          = 0;
		for (int i = (numRegions + 1); i < (numRegions + 1); i++) { //for '(numRegions + 1)' times do => |i|     // National Dex isn't a region, but is included
			@pokedexIndex[i] = 0;
		}
		// Day Care
		@day_care             = new DayCare();
		// Special battle modes
		@safariState          = null;
		@bugContestState      = null;
		@challenge            = null;
		@lastbattle           = null;
		// Events
		@eventvars            = new List<string>();
		// Affecting the map
		@bridge               = 0;
		@repel                = 0;
		@flashused            = false;
		@encounter_version    = 0;
		// Map transfers
		@healingSpot          = null;
		@escapePoint          = new List<string>();
		@pokecenterMapId      = -1;
		@pokecenterX          = -1;
		@pokecenterY          = -1;
		@pokecenterDirection  = -1;
		// Movement history
		@visitedMaps          = new List<string>();
		@mapTrail             = new List<string>();
		// Counters
		@happinessSteps       = 0;
		@pokerusTime          = null;
		// Save file
		@safesave             = false;
	}

	public int encounter_version { set {
		validate value => Integer;
		if (@encounter_version == value) return;
		@encounter_version = value;
		if (Game.GameData.PokemonEncounters && Game.GameData.game_map) Game.GameData.PokemonEncounters.setup(Game.GameData.game_map.map_id);
		}
	}

	public bool forced_movement() {
		return @ice_sliding || @descending_waterfall || @ascending_waterfall;
	}
}

//===============================================================================
// This class keeps track of erased and moved events so their position
// can remain after a game is saved and loaded.  This class also includes
// variables that should remain valid only for the current map.
//===============================================================================
public partial class PokemonMapMetadata {
	public int erasedEvents		{ get { return _erasedEvents; } }			protected int _erasedEvents;
	public int movedEvents		{ get { return _movedEvents; } }			protected int _movedEvents;
	public int strengthUsed		{ get { return _strengthUsed; } set { _strengthUsed = value; } }			protected int _strengthUsed;
	/// <summary>Black Flute's old effect</summary>
	public int lower_encounter_rate		{ get { return _lower_encounter_rate; } set { _lower_encounter_rate = value; } }			protected int _lower_encounter_rate;
	/// <summary>White Flute's old effect</summary>
	public int higher_encounter_rate		{ get { return _higher_encounter_rate; } set { _higher_encounter_rate = value; } }			protected int _higher_encounter_rate;
	/// <summary>White Flute's new effect</summary>
	public int lower_level_wild_pokemon		{ get { return _lower_level_wild_pokemon; } set { _lower_level_wild_pokemon = value; } }			protected int _lower_level_wild_pokemon;
	/// <summary>Black Flute's new effect</summary>
	public int higher_level_wild_pokemon		{ get { return _higher_level_wild_pokemon; } set { _higher_level_wild_pokemon = value; } }			protected int _higher_level_wild_pokemon;

	public void initialize() {
		clear;
	}

	public void clear() {
		@erasedEvents              = new List<string>();
		@movedEvents               = new List<string>();
		@strengthUsed              = false;
		@lower_encounter_rate      = false;   // Takes priority over @higher_encounter_rate
		@higher_encounter_rate     = false;
		@lower_level_wild_pokemon  = false;   // Takes priority over @higher_level_wild_pokemon
		@higher_level_wild_pokemon = false;
	}

	public void addErasedEvent(eventID) {
		key = new {Game.GameData.game_map.map_id, eventID};
		@erasedEvents[key] = true;
	}

	public void addMovedEvent(eventID) {
		key = new {Game.GameData.game_map.map_id, eventID};
		if (eventID.is_a(Integer)) event = Game.GameData.game_map.events[eventID];
		if (event) @movedEvents[key] = new {event.x, event.y, event.direction, event.through};
	}

	public void updateMap() {
		@erasedEvents.each do |i|
			if (i[0][0] == Game.GameData.game_map.map_id && i[1]) Game.GameData.game_map.events[i[0][1]]&.erase;
		}
		@movedEvents.each do |i|
			if (i[0][0] == Game.GameData.game_map.map_id && i[1]) {
				if (!Game.GameData.game_map.events[i[0][1]]) continue;
				Game.GameData.game_map.events[i[0][1]].moveto(i[1][0], i[1][1]);
				switch (i[1][2]) {
					case 2:  Game.GameData.game_map.events[i[0][1]].turn_down; break;
					case 4:  Game.GameData.game_map.events[i[0][1]].turn_left; break;
					case 6:  Game.GameData.game_map.events[i[0][1]].turn_right; break;
					case 8:  Game.GameData.game_map.events[i[0][1]].turn_up; break;
				}
			}
			if (i[1][3]) Game.GameData.game_map.events[i[0][1]].through = i[1][3];
		}
	}
}
