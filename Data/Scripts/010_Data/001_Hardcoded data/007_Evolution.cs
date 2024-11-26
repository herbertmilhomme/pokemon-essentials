//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Evolution {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int parameter		{ get { return _parameter; } }			protected int _parameter;
		/// <summary>false means parameter is the minimum level</summary>
		public int any_level_up		{ get { return _any_level_up; } }			protected int _any_level_up;
		public int level_up_proc		{ get { return _level_up_proc; } }			protected int _level_up_proc;
		public int battle_level_up_proc		{ get { return _battle_level_up_proc; } }			protected int _battle_level_up_proc;
		public int use_item_proc		{ get { return _use_item_proc; } }			protected int _use_item_proc;
		public int on_trade_proc		{ get { return _on_trade_proc; } }			protected int _on_trade_proc;
		public int after_battle_proc		{ get { return _after_battle_proc; } }			protected int _after_battle_proc;
		public int event_proc		{ get { return _event_proc; } }			protected int _event_proc;
		public int after_evolution_proc		{ get { return _after_evolution_proc; } }			protected int _after_evolution_proc;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                   = hash.id;
			@real_name            = hash.id.ToString()      || "Unnamed";
			@parameter            = hash.parameter;
			@any_level_up         = hash.any_level_up || false;
			@level_up_proc        = hash.level_up_proc;
			@battle_level_up_proc = hash.battle_level_up_proc;
			@use_item_proc        = hash.use_item_proc;
			@on_trade_proc        = hash.on_trade_proc;
			@after_battle_proc    = hash.after_battle_proc;
			@event_proc           = hash.event_proc;
			@after_evolution_proc = hash.after_evolution_proc;
		}

		alias name real_name;

		public void call_level_up(*args) {
			return (@level_up_proc) ? @level_up_proc.call(*args) : null;
		}

		public void call_battle_level_up(*args) {
			if (@battle_level_up_proc) {
				return @battle_level_up_proc.call(*args);
			} else if (@level_up_proc) {
				return @level_up_proc.call(*args);
			}
			return null;
		}

		public void call_use_item(*args) {
			return (@use_item_proc) ? @use_item_proc.call(*args) : null;
		}

		public void call_on_trade(*args) {
			return (@on_trade_proc) ? @on_trade_proc.call(*args) : null;
		}

		public void call_after_battle(*args) {
			return (@after_battle_proc) ? @after_battle_proc.call(*args) : null;
		}

		public void call_event(*args) {
			return (@event_proc) ? @event_proc.call(*args) : null;
		}

		public void call_after_evolution(*args) {
			@after_evolution_proc&.call(*args);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.Evolution.register({
	id = :None;
});

//===============================================================================
//
//===============================================================================

GameData.Evolution.register({
	id            = :Level,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter;
	}
});

GameData.Evolution.register({
	id            = :LevelMale,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && pkmn.male();
	}
});

GameData.Evolution.register({
	id            = :LevelFemale,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && pkmn.female();
	}
});

GameData.Evolution.register({
	id            = :LevelDay,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && DayNight.isDay();
	}
});

GameData.Evolution.register({
	id            = :LevelNight,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && DayNight.isNight();
	}
});

GameData.Evolution.register({
	id            = :LevelMorning,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && DayNight.isMorning();
	}
});

GameData.Evolution.register({
	id            = :LevelAfternoon,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && DayNight.isAfternoon();
	}
});

GameData.Evolution.register({
	id            = :LevelEvening,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && DayNight.isEvening();
	}
});

GameData.Evolution.register({
	id            = :LevelNoWeather,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.game_screen && Game.GameData.game_screen.weather_type == types.None;
	}
});

GameData.Evolution.register({
	id            = :LevelSun,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.game_screen &&
				GameData.Weather.get(Game.GameData.game_screen.weather_type).category == :Sun;
	}
});

GameData.Evolution.register({
	id            = :LevelRain,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.game_screen &&
				new []{:Rain, :Fog}.Contains(GameData.Weather.get(Game.GameData.game_screen.weather_type).category);
	}
});

GameData.Evolution.register({
	id            = :LevelSnow,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.game_screen &&
				GameData.Weather.get(Game.GameData.game_screen.weather_type).category == :Hail;
	}
});

GameData.Evolution.register({
	id            = :LevelSandstorm,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.game_screen &&
				GameData.Weather.get(Game.GameData.game_screen.weather_type).category == :Sandstorm;
	}
});

GameData.Evolution.register({
	id            = :LevelCycling,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.PokemonGlobal && Game.GameData.PokemonGlobal.bicycle;
	}
});

GameData.Evolution.register({
	id            = :LevelSurfing,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.PokemonGlobal && Game.GameData.PokemonGlobal.surfing;
	}
});

GameData.Evolution.register({
	id            = :LevelDiving,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.PokemonGlobal && Game.GameData.PokemonGlobal.diving;
	}
});

GameData.Evolution.register({
	id            = :LevelCoins,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next Game.GameData.player.coins >= parameter;
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		Game.GameData.player.coins -= parameter;
		next true;
	}
});

GameData.Evolution.register({
	id            = :LevelDarkness,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.game_map.metadata&.dark_map;
	}
});

GameData.Evolution.register({
	id            = :LevelDarkInParty,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && Game.GameData.player.has_pokemon_of_type(types.DARK);
	}
});

GameData.Evolution.register({
	id            = :AttackGreater,   // Hitmonlee
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && pkmn.attack > pkmn.defense;
	}
});

GameData.Evolution.register({
	id            = :AtkDefEqual,   // Hitmontop
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && pkmn.attack == pkmn.defense;
	}
});

GameData.Evolution.register({
	id            = :DefenseGreater,   // Hitmonchan
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && pkmn.attack < pkmn.defense;
	}
});

GameData.Evolution.register({
	id            = :Silcoon,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && (((pkmn.personalID >> 16) & 0xFFFF) % 10) < 5;
	}
});

GameData.Evolution.register({
	id            = :Cascoon,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter && (((pkmn.personalID >> 16) & 0xFFFF) % 10) >= 5;
	}
});

GameData.Evolution.register({
	id            = :Ninjask,
	parameter     = Integer,
	level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter;
	}
});

GameData.Evolution.register({
	id                   = :Shedinja,
	parameter            = Integer,
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (Game.GameData.player.party_full()) next false;
		if (!Game.GameData.bag.has(:POKEBALL)) next false;
		PokemonEvolutionScene.DuplicatePokemon(pkmn, new_species);
		Game.GameData.bag.remove(Moves.POKEBALL);
		next true;
	}
});

GameData.Evolution.register({
	id            = :Happiness,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220);
	}
});

GameData.Evolution.register({
	id            = :HappinessMale,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220) && pkmn.male();
	}
});

GameData.Evolution.register({
	id            = :HappinessFemale,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220) && pkmn.female();
	}
});

GameData.Evolution.register({
	id            = :HappinessDay,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220) && DayNight.isDay();
	}
});

GameData.Evolution.register({
	id            = :HappinessNight,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220) && DayNight.isNight();
	}
});

GameData.Evolution.register({
	id            = :HappinessMove,
	parameter     = :Move,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		if (pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220)) {
			next pkmn.moves.any(m => m && m.id == parameter);
		}
	}
});

GameData.Evolution.register({
	id            = :HappinessMoveType,
	parameter     = :Type,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		if (pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220)) {
			next pkmn.moves.any(m => m && m.type == parameter);
		}
	}
});

GameData.Evolution.register({
	id                   = :HappinessHoldItem,
	parameter            = :Item,
	any_level_up         = true,   // Needs any level up
	level_up_proc        = (pkmn, parameter) => {
		next pkmn.item == parameter && pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220);
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id            = :MaxHappiness,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.happiness == 255;
	}
});

GameData.Evolution.register({
	id            = :Beauty,   // Feebas
	parameter     = Integer,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.beauty >= parameter;
	}
});

GameData.Evolution.register({
	id                   = :HoldItem,
	parameter            = :Item,
	any_level_up         = true,   // Needs any level up
	level_up_proc        = (pkmn, parameter) => {
		next pkmn.item == parameter;
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id                   = :HoldItemMale,
	parameter            = :Item,
	any_level_up         = true,   // Needs any level up
	level_up_proc        = (pkmn, parameter) => {
		next pkmn.item == parameter && pkmn.male();
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id                   = :HoldItemFemale,
	parameter            = :Item,
	any_level_up         = true,   // Needs any level up
	level_up_proc        = (pkmn, parameter) => {
		next pkmn.item == parameter && pkmn.female();
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id                   = :DayHoldItem,
	parameter            = :Item,
	any_level_up         = true,   // Needs any level up
	level_up_proc        = (pkmn, parameter) => {
		next pkmn.item == parameter && DayNight.isDay();
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id                   = :NightHoldItem,
	parameter            = :Item,
	any_level_up         = true,   // Needs any level up
	level_up_proc        = (pkmn, parameter) => {
		next pkmn.item == parameter && DayNight.isNight();
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id                   = :HoldItemHappiness,
	parameter            = :Item,
	any_level_up         = true,   // Needs any level up
	level_up_proc        = (pkmn, parameter) => {
		next pkmn.item == parameter && pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220);
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id            = :HasMove,
	parameter     = :Move,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.moves.any(m => m && m.id == parameter);
	}
});

GameData.Evolution.register({
	id            = :HasMoveType,
	parameter     = :Type,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.moves.any(m => m && m.type == parameter);
	}
});

GameData.Evolution.register({
	id            = :HasInParty,
	parameter     = :Species,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next Game.GameData.player.has_species(parameter);
	}
});

GameData.Evolution.register({
	id            = :Location,
	parameter     = Integer,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next Game.GameData.game_map.map_id == parameter;
	}
});

GameData.Evolution.register({
	id            = :LocationFlag,
	parameter     = String,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next Game.GameData.game_map.metadata&.has_flag(parameter);
	}
});

GameData.Evolution.register({
	id            = :Region,
	parameter     = Integer,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		map_metadata = Game.GameData.game_map.metadata;
		next map_metadata&.town_map_position && map_metadata.town_map_position[0] == parameter;
	}
});

GameData.Evolution.register({
	id            = :Counter,
	parameter     = Integer,
	any_level_up  = true,   // Needs any level up
	level_up_proc = (pkmn, parameter) => {
		next pkmn.evolution_counter >= parameter;
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		pkmn.evolution_counter = 0;
		next true;
	}
});

//===============================================================================
// Evolution methods that trigger when levelling up in battle.
//===============================================================================

GameData.Evolution.register({
	id                   = :LevelBattle,
	parameter            = Integer,
	battle_level_up_proc = (pkmn, parameter) => {
		next pkmn.level >= parameter;
	}
});

//===============================================================================
// Evolution methods that trigger when using an item on the Pokémon.
//===============================================================================

GameData.Evolution.register({
	id            = :Item,
	parameter     = :Item,
	use_item_proc = (pkmn, parameter, item) => {
		next item == parameter;
	}
});

GameData.Evolution.register({
	id            = :ItemMale,
	parameter     = :Item,
	use_item_proc = (pkmn, parameter, item) => {
		next item == parameter && pkmn.male();
	}
});

GameData.Evolution.register({
	id            = :ItemFemale,
	parameter     = :Item,
	use_item_proc = (pkmn, parameter, item) => {
		next item == parameter && pkmn.female();
	}
});

GameData.Evolution.register({
	id            = :ItemDay,
	parameter     = :Item,
	use_item_proc = (pkmn, parameter, item) => {
		next item == parameter && DayNight.isDay();
	}
});

GameData.Evolution.register({
	id            = :ItemNight,
	parameter     = :Item,
	use_item_proc = (pkmn, parameter, item) => {
		next item == parameter && DayNight.isNight();
	}
});

GameData.Evolution.register({
	id            = :ItemHappiness,
	parameter     = :Item,
	use_item_proc = (pkmn, parameter, item) => {
		next item == parameter && pkmn.happiness >= (Settings.APPLY_HAPPINESS_SOFT_CAP ? 160 : 220);
	}
});

//===============================================================================
// Evolution methods that trigger when the Pokémon is obtained in a trade.
//===============================================================================

GameData.Evolution.register({
	id            = :Trade,
	on_trade_proc = (pkmn, parameter, other_pkmn) => {
		next true;
	}
});

GameData.Evolution.register({
	id            = :TradeMale,
	on_trade_proc = (pkmn, parameter, other_pkmn) => {
		next pkmn.male();
	}
});

GameData.Evolution.register({
	id            = :TradeFemale,
	on_trade_proc = (pkmn, parameter, other_pkmn) => {
		next pkmn.female();
	}
});

GameData.Evolution.register({
	id            = :TradeDay,
	on_trade_proc = (pkmn, parameter, other_pkmn) => {
		next DayNight.isDay();
	}
});

GameData.Evolution.register({
	id            = :TradeNight,
	on_trade_proc = (pkmn, parameter, other_pkmn) => {
		next DayNight.isNight();
	}
});

GameData.Evolution.register({
	id                   = :TradeItem,
	parameter            = :Item,
	on_trade_proc        = (pkmn, parameter, other_pkmn) => {
		next pkmn.item == parameter;
	},
	after_evolution_proc = (pkmn, new_species, parameter, evo_species) => {
		if (evo_species != new_species || !pkmn.hasItem(parameter)) next false;
		pkmn.item = null;   // Item is now consumed
		next true;
	}
});

GameData.Evolution.register({
	id            = :TradeSpecies,
	parameter     = :Species,
	on_trade_proc = (pkmn, parameter, other_pkmn) => {
		next other_pkmn.species == parameter && !other_pkmn.hasItem(Items.EVERSTONE);
	}
});

//===============================================================================
// Evolution methods that are triggered after any battle.
//===============================================================================

GameData.Evolution.register({
	id                = :AfterBattleCounter,
	parameter         = Integer,
	any_level_up      = true,   // Needs any level up
	after_battle_proc = (pkmn, party_index, parameter) => {
		ret = pkmn.evolution_counter >= parameter;
		pkmn.evolution_counter = 0;   // Always resets after battle
		next ret;
	}
});

// Doesn't cause an evolution itself. Just makes the Pokémon ready to evolve by
// another means (e.g. via an event). Note that pkmn.evolution_counter is not
// reset after the battle.
GameData.Evolution.register({
	id                = :AfterBattleCounterMakeReady,
	parameter         = Integer,
	any_level_up      = true,   // Needs any level up
	after_battle_proc = (pkmn, party_index, parameter) => {
		if (pkmn.evolution_counter >= parameter) pkmn.ready_to_evolve = true;
		next false;
	}
});

//===============================================================================
// Evolution methods that are triggered by an event.
// Each event has its own number, which is the value of the parameter as defined
// in pokemon.txt/pokemon_forms.txt. It is also 'number' in def EvolutionEvent,
// which triggers evolution checks for a particular event number. 'value' in an
// event_proc is the number of the evolution event currently being triggered.
// Evolutions caused by different events should have different numbers. Used
// event numbers are:
//   1: Kubfu -> Urshifu
//   2: Galarian Yamask -> Runerigus
// These used event numbers are only used in pokemon.txt/pokemon_forms.txt and in
// map events that call EvolutionEvent, so they are relatively easy to change
// if (you need to (no script changes are required). However, you could just) {
// ignore them instead if you don't want to use them.
//===============================================================================
public void EvolutionEvent(number) {
	if (!Game.GameData.player) return;
	foreach (var pkmn in Game.GameData.player.able_party) { //'Game.GameData.player.able_party.each' do => |pkmn|
		pkmn.trigger_event_evolution(number);
	}
}

GameData.Evolution.register({
	id         = :Event,
	parameter  = Integer,
	event_proc = (pkmn, parameter, value) => {
		next value == parameter;
	}
});

GameData.Evolution.register({
	id         = :EventReady,
	parameter  = Integer,
	event_proc = (pkmn, parameter, value) => {
		next value == parameter && pkmn.ready_to_evolve;
	}
});
