//===============================================================================
// Stored in Game.GameData.stats
//===============================================================================
public partial class GameStats {
	// Travel
	public int distance_walked		{ get { return _distance_walked; } set { _distance_walked = value; } }			protected int _distance_walked;
	public int distance_cycled		{ get { return _distance_cycled; } set { _distance_cycled = value; } }			protected int _distance_cycled;
	/// <summary>surfed includes diving</summary>
	public int distance_surfed		{ get { return _distance_surfed; } set { _distance_surfed = value; } }			protected int _distance_surfed;
	/// <summary>Also counted in distance_walked</summary>
	public int distance_slid_on_ice		{ get { return _distance_slid_on_ice; } set { _distance_slid_on_ice = value; } }			protected int _distance_slid_on_ice;
	/// <summary>Times the player walked into something</summary>
	public int bump_count		{ get { return _bump_count; } set { _bump_count = value; } }			protected int _bump_count;
	public int cycle_count		{ get { return _cycle_count; } set { _cycle_count = value; } }			protected int _cycle_count;
	public int surf_count		{ get { return _surf_count; } set { _surf_count = value; } }			protected int _surf_count;
	public int dive_count		{ get { return _dive_count; } set { _dive_count = value; } }			protected int _dive_count;
	// Field actions
	public int fly_count		{ get { return _fly_count; } set { _fly_count = value; } }			protected int _fly_count;
	public int cut_count		{ get { return _cut_count; } set { _cut_count = value; } }			protected int _cut_count;
	public int flash_count		{ get { return _flash_count; } set { _flash_count = value; } }			protected int _flash_count;
	public int rock_smash_count		{ get { return _rock_smash_count; } set { _rock_smash_count = value; } }			protected int _rock_smash_count;
	public int rock_smash_battles		{ get { return _rock_smash_battles; } set { _rock_smash_battles = value; } }			protected int _rock_smash_battles;
	public int headbutt_count		{ get { return _headbutt_count; } set { _headbutt_count = value; } }			protected int _headbutt_count;
	public int headbutt_battles		{ get { return _headbutt_battles; } set { _headbutt_battles = value; } }			protected int _headbutt_battles;
	/// <summary>Number of shoves, not the times Strength was used</summary>
	public int strength_push_count		{ get { return _strength_push_count; } set { _strength_push_count = value; } }			protected int _strength_push_count;
	public int waterfall_count		{ get { return _waterfall_count; } set { _waterfall_count = value; } }			protected int _waterfall_count;
	public int waterfalls_descended		{ get { return _waterfalls_descended; } set { _waterfalls_descended = value; } }			protected int _waterfalls_descended;
	// Items
	public int repel_count		{ get { return _repel_count; } set { _repel_count = value; } }			protected int _repel_count;
	public int itemfinder_count		{ get { return _itemfinder_count; } set { _itemfinder_count = value; } }			protected int _itemfinder_count;
	public int fishing_count		{ get { return _fishing_count; } set { _fishing_count = value; } }			protected int _fishing_count;
	public int fishing_battles		{ get { return _fishing_battles; } set { _fishing_battles = value; } }			protected int _fishing_battles;
	public int poke_radar_count		{ get { return _poke_radar_count; } set { _poke_radar_count = value; } }			protected int _poke_radar_count;
	public int poke_radar_longest_chain		{ get { return _poke_radar_longest_chain; } set { _poke_radar_longest_chain = value; } }			protected int _poke_radar_longest_chain;
	public int berry_plants_picked		{ get { return _berry_plants_picked; } set { _berry_plants_picked = value; } }			protected int _berry_plants_picked;
	public int max_yield_berry_plants		{ get { return _max_yield_berry_plants; } set { _max_yield_berry_plants = value; } }			protected int _max_yield_berry_plants;
	public int berries_planted		{ get { return _berries_planted; } set { _berries_planted = value; } }			protected int _berries_planted;
	// NPCs
	public int poke_center_count		{ get { return _poke_center_count; } set { _poke_center_count = value; } }			protected int _poke_center_count;
	public int revived_fossil_count		{ get { return _revived_fossil_count; } set { _revived_fossil_count = value; } }			protected int _revived_fossil_count;
	/// <summary>Times won any prize at all</summary>
	public int lottery_prize_count		{ get { return _lottery_prize_count; } set { _lottery_prize_count = value; } }			protected int _lottery_prize_count;
	// Pokémon
	public int eggs_hatched		{ get { return _eggs_hatched; } set { _eggs_hatched = value; } }			protected int _eggs_hatched;
	public int evolution_count		{ get { return _evolution_count; } set { _evolution_count = value; } }			protected int _evolution_count;
	public int evolutions_cancelled		{ get { return _evolutions_cancelled; } set { _evolutions_cancelled = value; } }			protected int _evolutions_cancelled;
	public int trade_count		{ get { return _trade_count; } set { _trade_count = value; } }			protected int _trade_count;
	public int moves_taught_by_item		{ get { return _moves_taught_by_item; } set { _moves_taught_by_item = value; } }			protected int _moves_taught_by_item;
	public int moves_taught_by_tutor		{ get { return _moves_taught_by_tutor; } set { _moves_taught_by_tutor = value; } }			protected int _moves_taught_by_tutor;
	public int moves_taught_by_reminder		{ get { return _moves_taught_by_reminder; } set { _moves_taught_by_reminder = value; } }			protected int _moves_taught_by_reminder;
	public int day_care_deposits		{ get { return _day_care_deposits; } set { _day_care_deposits = value; } }			protected int _day_care_deposits;
	public int day_care_levels_gained		{ get { return _day_care_levels_gained; } set { _day_care_levels_gained = value; } }			protected int _day_care_levels_gained;
	public int pokerus_infections		{ get { return _pokerus_infections; } set { _pokerus_infections = value; } }			protected int _pokerus_infections;
	public int shadow_pokemon_purified		{ get { return _shadow_pokemon_purified; } set { _shadow_pokemon_purified = value; } }			protected int _shadow_pokemon_purified;
	// Battles
	public int wild_battles_won		{ get { return _wild_battles_won; } set { _wild_battles_won = value; } }			protected int _wild_battles_won;
	/// <summary>Lost includes fled from</summary>
	public int wild_battles_lost		{ get { return _wild_battles_lost; } set { _wild_battles_lost = value; } }			protected int _wild_battles_lost;
	public int trainer_battles_won		{ get { return _trainer_battles_won; } set { _trainer_battles_won = value; } }			protected int _trainer_battles_won;
	public int trainer_battles_lost		{ get { return _trainer_battles_lost; } set { _trainer_battles_lost = value; } }			protected int _trainer_battles_lost;
	public int total_exp_gained		{ get { return _total_exp_gained; } set { _total_exp_gained = value; } }			protected int _total_exp_gained;
	public int battle_money_gained		{ get { return _battle_money_gained; } set { _battle_money_gained = value; } }			protected int _battle_money_gained;
	public int battle_money_lost		{ get { return _battle_money_lost; } set { _battle_money_lost = value; } }			protected int _battle_money_lost;
	public int blacked_out_count		{ get { return _blacked_out_count; } set { _blacked_out_count = value; } }			protected int _blacked_out_count;
	public int mega_evolution_count		{ get { return _mega_evolution_count; } set { _mega_evolution_count = value; } }			protected int _mega_evolution_count;
	public int primal_reversion_count		{ get { return _primal_reversion_count; } set { _primal_reversion_count = value; } }			protected int _primal_reversion_count;
	public int failed_poke_ball_count		{ get { return _failed_poke_ball_count; } set { _failed_poke_ball_count = value; } }			protected int _failed_poke_ball_count;
	// Currency
	public int money_spent_at_marts		{ get { return _money_spent_at_marts; } set { _money_spent_at_marts = value; } }			protected int _money_spent_at_marts;
	public int money_earned_at_marts		{ get { return _money_earned_at_marts; } set { _money_earned_at_marts = value; } }			protected int _money_earned_at_marts;
	public int mart_items_bought		{ get { return _mart_items_bought; } set { _mart_items_bought = value; } }			protected int _mart_items_bought;
	public int premier_balls_earned		{ get { return _premier_balls_earned; } set { _premier_balls_earned = value; } }			protected int _premier_balls_earned;
	public int drinks_bought		{ get { return _drinks_bought; } set { _drinks_bought = value; } }			protected int _drinks_bought;
	/// <summary>From vending machines</summary>
	public int drinks_won		{ get { return _drinks_won; } set { _drinks_won = value; } }			protected int _drinks_won;
	public int coins_won		{ get { return _coins_won; } set { _coins_won = value; } }			protected int _coins_won;
	/// <summary>Not bought, not spent</summary>
	public int coins_lost		{ get { return _coins_lost; } set { _coins_lost = value; } }			protected int _coins_lost;
	public int battle_points_won		{ get { return _battle_points_won; } set { _battle_points_won = value; } }			protected int _battle_points_won;
	public int battle_points_spent		{ get { return _battle_points_spent; } set { _battle_points_spent = value; } }			protected int _battle_points_spent;
	public int soot_collected		{ get { return _soot_collected; } set { _soot_collected = value; } }			protected int _soot_collected;
	// Special stats
	/// <summary>An array of integers</summary>
	public int gym_leader_attempts		{ get { return _gym_leader_attempts; } set { _gym_leader_attempts = value; } }			protected int _gym_leader_attempts;
	/// <summary>An array of times in seconds</summary>
	public int times_to_get_badges		{ get { return _times_to_get_badges; } set { _times_to_get_badges = value; } }			protected int _times_to_get_badges;
	public int elite_four_attempts		{ get { return _elite_four_attempts; } set { _elite_four_attempts = value; } }			protected int _elite_four_attempts;
	/// <summary>See also Game Variable 13</summary>
	public int hall_of_fame_entry_count		{ get { return _hall_of_fame_entry_count; } set { _hall_of_fame_entry_count = value; } }			protected int _hall_of_fame_entry_count;
	/// <summary>In seconds</summary>
	public int time_to_enter_hall_of_fame		{ get { return _time_to_enter_hall_of_fame; } set { _time_to_enter_hall_of_fame = value; } }			protected int _time_to_enter_hall_of_fame;
	public int safari_pokemon_caught		{ get { return _safari_pokemon_caught; } set { _safari_pokemon_caught = value; } }			protected int _safari_pokemon_caught;
	public int most_captures_per_safari_game		{ get { return _most_captures_per_safari_game; } set { _most_captures_per_safari_game = value; } }			protected int _most_captures_per_safari_game;
	public int bug_contest_count		{ get { return _bug_contest_count; } set { _bug_contest_count = value; } }			protected int _bug_contest_count;
	public int bug_contest_wins		{ get { return _bug_contest_wins; } set { _bug_contest_wins = value; } }			protected int _bug_contest_wins;
	// Play
	/// <summary>In seconds; the reader also updates the value</summary>
	public int play_time		{ get { return _play_time; } }			protected int _play_time;
	public int play_sessions		{ get { return _play_sessions; } set { _play_sessions = value; } }			protected int _play_sessions;
	/// <summary>In seconds</summary>
	public int time_last_saved		{ get { return _time_last_saved; } set { _time_last_saved = value; } }			protected int _time_last_saved;

	public void initialize() {
		// Travel
		@distance_walked               = 0;
		@distance_cycled               = 0;
		@distance_surfed               = 0;
		@distance_slid_on_ice          = 0;
		@bump_count                    = 0;
		@cycle_count                   = 0;
		@surf_count                    = 0;
		@dive_count                    = 0;
		// Field actions
		@fly_count                     = 0;
		@cut_count                     = 0;
		@flash_count                   = 0;
		@rock_smash_count              = 0;
		@rock_smash_battles            = 0;
		@headbutt_count                = 0;
		@headbutt_battles              = 0;
		@strength_push_count           = 0;
		@waterfall_count               = 0;
		@waterfalls_descended          = 0;
		// Items
		@repel_count                   = 0;
		@itemfinder_count              = 0;
		@fishing_count                 = 0;
		@fishing_battles               = 0;
		@poke_radar_count              = 0;
		@poke_radar_longest_chain      = 0;
		@berry_plants_picked           = 0;
		@max_yield_berry_plants        = 0;
		@berries_planted               = 0;
		// NPCs
		@poke_center_count             = 0;   // Incremented in Poké Center nurse events
		@revived_fossil_count          = 0;   // Incremented in fossil reviver events
		@lottery_prize_count           = 0;   // Incremented in lottery NPC events
		// Pokémon
		@eggs_hatched                  = 0;
		@evolution_count               = 0;
		@evolutions_cancelled          = 0;
		@trade_count                   = 0;
		@moves_taught_by_item          = 0;
		@moves_taught_by_tutor         = 0;
		@moves_taught_by_reminder      = 0;
		@day_care_deposits             = 0;
		@day_care_levels_gained        = 0;
		@pokerus_infections            = 0;
		@shadow_pokemon_purified       = 0;
		// Battles
		@wild_battles_won              = 0;
		@wild_battles_lost             = 0;
		@trainer_battles_won           = 0;
		@trainer_battles_lost          = 0;
		@total_exp_gained              = 0;
		@battle_money_gained           = 0;
		@battle_money_lost             = 0;
		@blacked_out_count             = 0;
		@mega_evolution_count          = 0;
		@primal_reversion_count        = 0;
		@failed_poke_ball_count        = 0;
		// Currency
		@money_spent_at_marts          = 0;
		@money_earned_at_marts         = 0;
		@mart_items_bought             = 0;
		@premier_balls_earned          = 0;
		@drinks_bought                 = 0;   // Incremented in vending machine events
		@drinks_won                    = 0;   // Incremented in vending machine events
		@coins_won                     = 0;
		@coins_lost                    = 0;
		@battle_points_won             = 0;
		@battle_points_spent           = 0;
		@soot_collected                = 0;
		// Special stats
		@gym_leader_attempts           = [0] * 50;   // Incremented in Gym Leader events (50 is arbitrary but suitably large)
		@times_to_get_badges           = new List<string>();   // Set with set_time_to_badge(number) in Gym Leader events
		@elite_four_attempts           = 0;   // Incremented in door event leading to the first E4 member
		@hall_of_fame_entry_count      = 0;   // Incremented in Hall of Fame event
		@time_to_enter_hall_of_fame    = 0;   // Set with set_time_to_hall_of_fame in Hall of Fame event
		@safari_pokemon_caught         = 0;
		@most_captures_per_safari_game = 0;
		@bug_contest_count             = 0;
		@bug_contest_wins              = 0;
		// Play
		@play_time                     = 0;
		@play_sessions                 = 0;
		@time_last_saved               = 0;
	}

	public void distance_moved() {
		return @distance_walked + @distance_cycled + @distance_surfed;
	}

	public void caught_pokemon_count() {
		if (!Game.GameData.player) return 0;
		ret = 0;
		GameData.Species.each_species(sp => ret += Game.GameData.player.pokedex.caught_count(sp));
		return ret;
	}

	public void save_count() {
		return Game.GameData.game_system&.save_count || 0;
	}

	public void set_time_to_badge(number) {
		@times_to_get_badges[number] = play_time;
	}

	public void set_time_to_hall_of_fame() {
		if (@time_to_enter_hall_of_fame == 0) @time_to_enter_hall_of_fame = play_time;
	}

	public void play_time() {
		if (Game.GameData.game_temp&.last_uptime_refreshed_play_time) {
			now = System.uptime;
			@play_time += now - Game.GameData.game_temp.last_uptime_refreshed_play_time;
			Game.GameData.game_temp.last_uptime_refreshed_play_time = now;
		}
		return @play_time;
	}

	public void play_time_per_session() {
		return play_time / @play_sessions;
	}

	public void set_time_last_saved() {
		@time_last_saved = play_time;
	}

	public void time_since_last_save() {
		return play_time - @time_last_saved;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Game_Temp {
	public int last_uptime_refreshed_play_time		{ get { return _last_uptime_refreshed_play_time; } set { _last_uptime_refreshed_play_time = value; } }			protected int _last_uptime_refreshed_play_time;
}
