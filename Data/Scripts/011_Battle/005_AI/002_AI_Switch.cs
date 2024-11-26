//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	// Called by the AI's def DefaultChooseEnemyCommand, and by def ChooseMove
	// if (the only moves known are bad ones (the latter forces a switch if) {
	// possible). Also aliased by the Battle Palace and Battle Arena.
	public void ChooseToSwitchOut(terrible_moves = false) {
		if (!@battle.canSwitch) return false;   // Battle rule
		if (@user.wild()) return false;
		if (!@battle.CanSwitchOut(@user.index)) return false;
		// Don't switch if all foes are unable to do anything, e.g. resting after
		// Hyper Beam, will Truant (i.e. free turn)
		if (@trainer.high_skill()) {
			foe_can_act = false;
			each_foe_battler(@user.side) do |b, i|
				if (!b.can_attack()) continue;
				foe_can_act = true;
				break;
			}
			if (!foe_can_act) return false;
		}
		// Various calculations to decide whether to switch
		if (terrible_moves) {
			Debug.log_ai($"{@user.name} is being forced to switch out");
		} else {
			if (!@trainer.has_skill_flag("ConsiderSwitching")) return false;
			reserves = get_non_active_party_pokemon(@user.index);
			if (reserves.empty()) return false;
			should_switch = Battle.AI.Handlers.should_switch(@user, reserves, self, @battle);
			if (should_switch && @trainer.medium_skill()) {
				if (Battle.AI.Handlers.should_not_switch(@user, reserves, self, @battle)) should_switch = false;
			}
			if (!should_switch) return false;
		}
		// Want to switch; find the best replacement Pokémon
		idxParty = choose_best_replacement_pokemon(@user.index, terrible_moves);
		if (idxParty < 0) {   // No good replacement Pokémon found
			Debug.Log($"   => no good replacement Pokémon, will not switch after all");
			return false;
		}
		// Prefer using Baton Pass instead of switching
		baton_pass = -1;
		@user.battler.eachMoveWithIndex do |m, i|
			if (m.function_code != "SwitchOutUserPassOnEffects") continue;   // Baton Pass
			if (!@battle.CanChooseMove(@user.index, i, false)) continue;
			baton_pass = i;
			break;
		}
		if (baton_pass >= 0 && @battle.RegisterMove(@user.index, baton_pass, false)) {
			Debug.Log($"   => will use Baton Pass to switch out");
			return true;
		} else if (@battle.RegisterSwitch(@user.index, idxParty)) {
			Debug.Log($"   => will switch with {@battle.Party(@user.index)[idxParty].name}");
			return true;
		}
		return false;
	}

	public void get_non_active_party_pokemon(idxBattler) {
		ret = new List<string>();
		@battle.Party(idxBattler).each_with_index do |pkmn, i|
			if (@battle.CanSwitchIn(idxBattler, i)) ret.Add(pkmn);
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	public void choose_best_replacement_pokemon(idxBattler, terrible_moves = false) {
		// Get all possible replacement Pokémon
		party = @battle.Party(idxBattler);
		idxPartyStart, idxPartyEnd = @battle.TeamIndexRangeFromBattlerIndex(idxBattler);
		reserves = new List<string>();
		party.each_with_index do |_pkmn, i|
			if (!@battle.CanSwitchIn(idxBattler, i)) continue;
			if (!terrible_moves) {   // Not terrible_moves means choosing an action for the round
				ally_will_switch_with_i = false;
				@battle.allSameSideBattlers(idxBattler).each do |b|
					if (@battle.choices[b.index].Action != :SwitchOut || @battle.choices[b.index].Index != i) continue;
					ally_will_switch_with_i = true;
					break;
				}
				if (ally_will_switch_with_i) continue;
			}
			// Ignore ace if possible
			if (@trainer.has_skill_flag("ReserveLastPokemon") && i == idxPartyEnd - 1) {
				if (!terrible_moves || reserves.length > 0) continue;
			}
			reserves.Add(new {i, 100});
			if (@trainer.has_skill_flag("UsePokemonInOrder") && reserves.length > 0) break;
		}
		if (reserves.length == 0) return -1;
		// Rate each possible replacement Pokémon
		reserves.each_with_index do |reserve, i|
			reserves[i][1] = rate_replacement_pokemon(idxBattler, party[reserve[0]], reserve[1]);
		}
		reserves.sort! { |a, b| b[1] <=> a[1] };   // Sort from highest to lowest rated
		// Don't bother choosing to switch if all replacements are poorly rated
		if (@trainer.high_skill() && !terrible_moves) {
			if (reserves[0][1] < 100) return -1;   // If best replacement rated at <100, don't switch
		}
		// Return the party index of the best rated replacement Pokémon
		return reserves[0][0];
	}

	public void rate_replacement_pokemon(idxBattler, pkmn, score) {
		pkmn_types = pkmn.types;
		entry_hazard_damage = calculate_entry_hazard_damage(pkmn, idxBattler & 1);
		if (entry_hazard_damage >= pkmn.hp) {
			score -= 50;   // pkmn will just faint
		} else if (entry_hazard_damage > 0) {
			score -= 50 * entry_hazard_damage / pkmn.hp;
		}
		if (!pkmn.hasItem(Items.HEAVYDUTYBOOTS) && !pokemon_airborne(pkmn)) {
			// Toxic Spikes
			if (@user.OwnSide.effects.ToxicSpikes > 0) {
				if (pokemon_can_be_poisoned(pkmn)) score -= 20;
			}
			// Sticky Web
			if (@user.OwnSide.effects.StickyWeb) {
				score -= 15;
			}
		}
		// Predict effectiveness of foe's last used move against pkmn
		each_foe_battler(@user.side) do |b, i|
			if (!b.battler.lastMoveUsed) continue;
			move_data = GameData.Move.try_get(b.battler.lastMoveUsed);
			if (!move_data || move_data.status()) continue;
			move_type = move_data.type;
			eff = Effectiveness.calculate(move_type, *pkmn_types);
			score -= move_data.power * eff / 5;
		}
		// Add power * effectiveness / 10 of all pkmn's moves to score
		foreach (var m in pkmn.moves) { //'pkmn.moves.each' do => |m|
			if (m.power == 0 || (m.pp == 0 && m.total_pp > 0)) continue;
			@battle.battlers[idxBattler].allOpposing.each do |b|
				if (pokemon_can_absorb_move(b.pokemon, m, m.type)) continue;
				bTypes = b.Types(true);
				score += m.power * Effectiveness.calculate(m.type, *bTypes) / 10;
			}
		}
		// Prefer if pkmn has lower HP and its position will be healed by Wish
		position = @battle.positions[idxBattler];
		if (position.effects.Wish > 0) {
			amt = position.effects.WishAmount;
			if (pkmn.totalhp - pkmn.hp > amt * 2 / 3) {
				score += 20 * (int)Math.Min(pkmn.totalhp - pkmn.hp, amt) / pkmn.totalhp;
			}
		}
		// Prefer if user is about to faint from Perish Song
		if (@user.effects.PerishSong == 1) score += 20;
		return score;
	}

	public void calculate_entry_hazard_damage(pkmn, side) {
		if (pkmn.hasAbility(Abilitys.MAGICGUARD) || pkmn.hasItem(Items.HEAVYDUTYBOOTS)) return 0;
		ret = 0;
		// Stealth Rock
		if (@battle.sides[side].effects.StealthRock && GameData.Types.exists(Types.ROCK)) {
			pkmn_types = pkmn.types;
			eff = Effectiveness.calculate(:ROCK, *pkmn_types);
			if (!Effectiveness.ineffective(eff)) ret += pkmn.totalhp * eff / 8;
		}
		// Spikes
		if (@battle.sides[side].effects.Spikes > 0 && !pokemon_airborne(pkmn)) {
			spikes_div = new {8, 6, 4}[@battle.sides[side].effects.Spikes - 1];
			ret += pkmn.totalhp / spikes_div;
		}
		return ret;
	}
}

//===============================================================================
// Pokémon is about to faint because of Perish Song.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:perish_song,
	block: (battler, reserves, ai, battle) => {
		if (battler.effects.PerishSong == 1) {
			Debug.log_ai($"{battler.name} wants to switch because it is about to faint from Perish Song");
			next true;
		}
		next false;
	}
)

//===============================================================================
// Pokémon will take a significant amount of damage at the end of this round, or
// it has an effect that causes it damage at the end of this round which it can
// remove by switching.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:significant_eor_damage,
	block: (battler, reserves, ai, battle) => {
		eor_damage = battler.rough_end_of_round_damage;
		if (eor_damage <= 0) next false;
		// Switch if battler will take significant EOR damage
		if (eor_damage >= battler.hp / 2 || eor_damage >= battler.totalhp / 4) {
			Debug.log_ai($"{battler.name} wants to switch because it will take a lot of EOR damage");
			next true;
		}
		// Switch to remove certain effects that cause the battler EOR damage
		if (ai.trainer.high_skill()) {
			if (battler.effects.LeechSeed >= 0 && ai.AIRandom(100) < 50) {
				Debug.log_ai($"{battler.name} wants to switch to get rid of its Leech Seed");
				next true;
			}
			if (battler.effects.Nightmare) {
				Debug.log_ai($"{battler.name} wants to switch to get rid of its Nightmare");
				next true;
			}
			if (battler.effects.Curse) {
				Debug.log_ai($"{battler.name} wants to switch to get rid of its Curse");
				next true;
			}
			if (battler.status == statuses.POISON && battler.statusCount > 0 && !battler.has_active_ability(abilitys.POISONHEAL)) {
				poison_damage = battler.totalhp / 8;
				next_toxic_damage = battler.totalhp * (battler.effects.Toxic + 1) / 16;
				if ((battler.hp <= next_toxic_damage && battler.hp > poison_damage) ||
					next_toxic_damage > poison_damage * 2) {
					Debug.log_ai($"{battler.name} wants to switch to reduce toxic to regular poisoning");
					next true;
				}
			}
		}
		next false;
	}
)

//===============================================================================
// Pokémon can cure its status problem or heal some HP with its ability by
// switching out. Covers all abilities with an OnSwitchOut AbilityEffects
// handler.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:cure_status_problem_by_switching_out,
	block: (battler, reserves, ai, battle) => {
		if (!battler.ability_active()) next false;
		// Don't try to cure a status problem/heal a bit of HP if entry hazards will
		// KO the battler if it switches back in
		entry_hazard_damage = ai.calculate_entry_hazard_damage(battler.pokemon, battler.side);
		if (entry_hazard_damage >= battler.hp) next false;
		// Check specific abilities
		single_status_cure = {
			IMMUNITY    = :POISON,
			INSOMNIA    = :SLEEP,
			LIMBER      = :PARALYSIS,
			MAGMAARMOR  = :FROZEN,
			VITALSPIRIT = :SLEEP,
			WATERBUBBLE = :BURN,
			WATERVEIL   = :BURN;
		}[battler.ability_id];
		if (battler.ability == abilitys.NATURALCURE || (single_status_cure && single_status_cure == battler.status)) {
			// Cures status problem
			if (battler.wants_status_problem(battler.status)) next false;
			if (battler.status == statuses.SLEEP && battler.statusCount == 1) next false;   // Will wake up this round anyway
			if (entry_hazard_damage >= battler.totalhp / 4) next false;
			// Don't bother curing a poisoning if Toxic Spikes will just re-poison the
			// battler when it switches back in
			if (battler.status == statuses.POISON && reserves.none(pkmn => pkmn.hasType(Types.POISON))) {
				if (battle.field.effects.ToxicSpikes == 2) next false;
				if (battle.field.effects.ToxicSpikes == 1 && battler.statusCount == 0) next false;
			}
			// Not worth curing status problems that still allow actions if at high HP
			if (battler.hp >= battler.totalhp / 2 && !new []{:SLEEP, :FROZEN}.Contains(battler.status)) next false;
			if (ai.AIRandom(100) < 70) {
				Debug.log_ai($"{battler.name} wants to switch to cure its status problem with {battler.ability.name}");
				next true;
			}
		} else if (battler.ability == abilitys.REGENERATOR) {
			// Not worth healing if battler would lose more HP from switching back in later
			if (entry_hazard_damage >= battler.totalhp / 3) next false;
			// Not worth healing HP if already at high HP
			if (battler.hp >= battler.totalhp / 2) next false;
			// Don't bother if a foe is at low HP and could be knocked out instead
			if (battler.check_for_move(m => m.damagingMove())) {
				weak_foe = false;
				ai.each_foe_battler(battler.side) do |b, i|
					if (b.hp < b.totalhp / 3) weak_foe = true;
					if (weak_foe) break;
				}
				if (weak_foe) next false;
			}
			if (ai.AIRandom(100) < 70) {
				Debug.log_ai($"{battler.name} wants to switch to heal with {battler.ability.name}");
				next true;
			}
		}
		next false;
	}
)

//===============================================================================
// Pokémon's position is about to be healed by Wish, and a reserve can benefit
// more from that healing than the Pokémon can.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:wish_healing,
	block: (battler, reserves, ai, battle) => {
		position = battle.positions[battler.index];
		if (position.effects.Wish == 0) next false;
		amt = position.effects.WishAmount;
		if (battler.totalhp - battler.hp >= amt * 2 / 3) next false;   // Want to heal itself instead
		reserve_wants_healing_more = false;
		foreach (var pkmn in reserves) { //'reserves.each' do => |pkmn|
			entry_hazard_damage = ai.calculate_entry_hazard_damage(pkmn, battler.index & 1);
			if (entry_hazard_damage >= pkmn.hp) continue;
			reserve_wants_healing_more = (pkmn.totalhp - pkmn.hp - entry_hazard_damage >= amt * 2 / 3);
			if (reserve_wants_healing_more) break;
		}
		if (reserve_wants_healing_more) {
			Debug.log_ai($"{battler.name} wants to switch because Wish can heal a reserve more");
			next true;
		}
		next false;
	}
)

//===============================================================================
// Pokémon is yawning and can't do anything while asleep.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:yawning,
	block: (battler, reserves, ai, battle) => {
		// Yawning and can fall asleep because of it
		if (battler.effects.Yawn == 0 || !battler.battler.CanSleepYawn()) next false;
		// Doesn't want to be asleep (includes checking for moves usable while asleep)
		if (battler.wants_status_problem(:SLEEP)) next false;
		// Can't cure itself of sleep
		if (battler.ability_active()) {
			if (new []{:INSOMNIA, :NATURALCURE, :REGENERATOR, :SHEDSKIN}.Contains(battler.ability_id)) next false;
			if (battler.ability_id == abilitys.HYDRATION && new []{:Rain, :HeavyRain}.Contains(battler.battler.effectiveWeather)) next false;
		}
		if (battler.has_active_item(new {:CHESTOBERRY, :LUMBERRY}) && battler.battler.canConsumeBerry()) next false;
		// Ally can't cure sleep
		ally_can_heal = false;
		ai.each_ally(battler.index) do |b, i|
			ally_can_heal = b.has_active_ability(abilitys.HEALER);
			if (ally_can_heal) break;
		}
		if (ally_can_heal) next false;
		// Doesn't benefit from being asleep/isn't less affected by sleep
		if (battler.has_active_ability(new {:EARLYBIRD, :MARVELSCALE})) next false;
		// Not trapping another battler in battle
		if (ai.trainer.high_skill()) {
			if (ai.battlers.any() do |b|
				b.effects.JawLock == battler.index ||
				b.effects.MeanLook == battler.index ||
				b.effects.Octolock == battler.index ||
				b.effects.TrappingUser == battler.index) next false;
			}
			trapping = false;
			ai.each_foe_battler(battler.side) do |b, i|
				if (b.ability_active() && Battle.AbilityEffects.triggerCertainSwitching(b.ability, b.battler, battle)) continue;
				if (b.item_active() && Battle.ItemEffects.triggerCertainSwitching(b.item, b.battler, battle)) continue;
				if (Settings.MORE_TYPE_EFFECTS && b.has_type(types.GHOST)) continue;
				if (b.battler.trappedInBattle()) continue;   // Relevant trapping effects are checked above
				if (battler.ability_active()) {
					trapping = Battle.AbilityEffects.triggerTrappingByTarget(battler.ability, b.battler, battler.battler, battle);
					if (trapping) break;
				}
				if (battler.item_active()) {
					trapping = Battle.ItemEffects.triggerTrappingByTarget(battler.item, b.battler, battler.battler, battle);
					if (trapping) break;
				}
			}
			if (trapping) next false;
		}
		// Doesn't have sufficiently raised stats that would be lost by switching
		if (battler.stages.any((key, val) => val >= 2)) next false;
		Debug.log_ai($"{battler.name} wants to switch because it is yawning and can't do anything while asleep");
		next true;
	}
)

//===============================================================================
// Pokémon is asleep, won't wake up soon and can't do anything while asleep.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:asleep,
	block: (battler, reserves, ai, battle) => {
		// Asleep and won't wake up this round or next round
		if (battler.status != statuses.SLEEP || battler.statusCount <= 2) next false;
		// Doesn't want to be asleep (includes checking for moves usable while asleep)
		if (battler.wants_status_problem(:SLEEP)) next false;
		// Doesn't benefit from being asleep
		if (battler.has_active_ability(abilitys.MARVELSCALE)) next false;
		// Doesn't know Rest (if it does, sleep is expected, so don't apply this check)
		if (battler.check_for_move(m => m.function_code == "HealUserFullyAndFallAsleep")) next false;
		// Not trapping another battler in battle
		if (ai.trainer.high_skill()) {
			if (ai.battlers.any() do |b|
				b.effects.JawLock == battler.index ||
				b.effects.MeanLook == battler.index ||
				b.effects.Octolock == battler.index ||
				b.effects.TrappingUser == battler.index) next false;
			}
			trapping = false;
			ai.each_foe_battler(battler.side) do |b, i|
				if (b.ability_active() && Battle.AbilityEffects.triggerCertainSwitching(b.ability, b.battler, battle)) continue;
				if (b.item_active() && Battle.ItemEffects.triggerCertainSwitching(b.item, b.battler, battle)) continue;
				if (Settings.MORE_TYPE_EFFECTS && b.has_type(types.GHOST)) continue;
				if (b.battler.trappedInBattle()) continue;   // Relevant trapping effects are checked above
				if (battler.ability_active()) {
					trapping = Battle.AbilityEffects.triggerTrappingByTarget(battler.ability, b.battler, battler.battler, battle);
					if (trapping) break;
				}
				if (battler.item_active()) {
					trapping = Battle.ItemEffects.triggerTrappingByTarget(battler.item, b.battler, battler.battler, battle);
					if (trapping) break;
				}
			}
			if (trapping) next false;
		}
		// Doesn't have sufficiently raised stats that would be lost by switching
		if (battler.stages.any((key, val) => val >= 2)) next false;
		// A reserve Pokémon is awake and not frozen
		if (reserves.none(pkmn => !new[] {:SLEEP, :FROZEN}.Contains(pkmn.status))) next false;
		// 60% chance to not bother
		if (ai.AIRandom(100) < 60) next false;
		Debug.log_ai($"{battler.name} wants to switch because it is asleep and can't do anything");
		next true;
	}
)

//===============================================================================
// Pokémon can't use any moves and isn't Destiny Bonding/Grudging/hiding behind a
// Substitute.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:battler_is_useless,
	block: (battler, reserves, ai, battle) => {
		if (!ai.trainer.medium_skill()) next false;
		if (battler.turnCount < 2) next false;   // Just switched in, give it a chance
		if (battle.CanChooseAnyMove(battler.index)) next false;
		if (battler.effects.DestinyBond || battler.effects.Grudge) next false;
		if (battler.effects.Substitute) {
			hidden_behind_substitute = true;
			ai.each_foe_battler(battler.side) do |b, i|
				if (!b.check_for_move(m => m.ignoresSubstitute(b.battler))) continue;
				hidden_behind_substitute = false;
				break;
			}
			if (hidden_behind_substitute) next false;
		}
		Debug.log_ai($"{battler.name} wants to switch because it can't do anything");
		next true;
	}
)

//===============================================================================
// Pokémon can't do anything to any foe because its ability absorbs all damage
// the Pokémon can deal out.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:foe_absorbs_all_moves_with_its_ability,
	block: (battler, reserves, ai, battle) => {
		if (battler.battler.turnCount < 2) next false;   // Don't switch out too quickly
		if (battler.battler.hasMoldBreaker()) next false;
		// Check if battler can damage any of its foes
		can_damage_foe = false;
		ai.each_foe_battler(battler.side) do |b, i|
			if (ai.trainer.high_skill() && b.rough_end_of_round_damage > 0) {
				can_damage_foe = true;   // Foe is being damaged already
				break;
			}
			// Check for battler's moves that can damage the foe (b)
			foreach (var move in battler.battler.Moves) { //battler.battler.eachMove do => |move|
				if (move.statusMove()) continue;
				if ((new []{"IgnoreTargetAbility",
						"CategoryDependsOnHigherDamageIgnoreTargetAbility"}.Contains(move.function_code)) {
					can_damage_foe = true;
					break;
				}
				if (!ai.pokemon_can_absorb_move(b, move, move.CalcType(battler.battler))) {
					can_damage_foe = true;
					break;
				}
			}
			if (can_damage_foe) break;
		}
		if (can_damage_foe) next false;
		// Check if a reserve could damage any foe; only switch if one could
		reserve_can_damage_foe = false;
		foreach (var pkmn in reserves) { //'reserves.each' do => |pkmn|
			ai.each_foe_battler(battler.side) do |b, i|
				// Check for reserve's moves that can damage the foe (b)
				foreach (var move in pkmn.moves) { //'pkmn.moves.each' do => |move|
					if (move.status_move()) continue;
					if ((new []{"IgnoreTargetAbility",
							"CategoryDependsOnHigherDamageIgnoreTargetAbility"}.Contains(move.function_code)) {
						reserve_can_damage_foe = true;
						break;
					}
					if (!ai.pokemon_can_absorb_move(b, move, move.type)) {
						reserve_can_damage_foe = true;
						break;
					}
				}
				if (reserve_can_damage_foe) break;
			}
			if (reserve_can_damage_foe) break;
		}
		if (!reserve_can_damage_foe) next false;
		Debug.log_ai($"{battler.name} wants to switch because it can't damage the foe(s)");
		next true;
	}
)

//===============================================================================
// Pokémon doesn't have an ability that makes it immune to a foe's move, but a
// reserve does (see def pokemon_can_absorb_move()). The foe's move is chosen
// randomly, or is their most powerful move if the trainer's skill level is good
// enough.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:absorb_foe_move,
	block: (battler, reserves, ai, battle) => {
		if (!ai.trainer.medium_skill()) next false;
		// Not worth it if the battler is evasive enough
		if (battler.stages[:EVASION] >= 3) next false;
		// Not worth it if abilities are being negated
		if (battle.CheckGlobalAbility(Abilities.NEUTRALIZINGGAS)) next false;
		// Get the foe move with the highest power (or a random damaging move)
		foe_moves = new List<string>();
		ai.each_foe_battler(battler.side) do |b, i|
			foreach (var move in b.moves) { //'b.moves.each' do => |move|
				if (move.statusMove()) continue;
				m_power = move.power;
				if (move.is_a(Battle.Move.OHKO)) m_power = 100;
				m_type = move.CalcType(b.battler);
				foe_moves.Add(new {m_power, m_type, move});
			}
		}
		if (foe_moves.empty()) next false;
		if (ai.trainer.high_skill()) {
			foe_moves.sort! { |a, b| a[0] <=> b[0] };   // Highest power move
			chosen_move = foe_moves.last;
		} else {
			chosen_move = foe_moves[ai.AIRandom(foe_moves.length)];   // Random move
		}
		// Get the chosen move's information
		move_power = chosen_move[0];
		move_type = chosen_move[1];
		move = chosen_move[2];
		// Don't bother if the foe's best move isn't too strong
		if (move_power < 70) next false;
		// Check battler for absorbing ability
		if (ai.pokemon_can_absorb_move(battler, move, move_type)) next false;
		// battler can't absorb move; find a party Pokémon that can
		if (reserves.any(pkmn => ai.pokemon_can_absorb_move(pkmn, move, move_type))) {
			if (ai.AIRandom(100) < 70) next false;
			Debug.log_ai($"{battler.name} wants to switch because it can't absorb a foe's move but a reserve can");
			next true;
		}
		next false;
	}
)

//===============================================================================
// Sudden Death rule (at the end of each round, if one side has more able Pokémon
// than the other side, that side wins). Avoid fainting at all costs.
// NOTE: This rule isn't used anywhere.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:sudden_death,
	block: (battler, reserves, ai, battle) => {
		if (!battle.rules["suddendeath"] || battler.turnCount == 0) next false;
		if (battler.hp <= battler.totalhp / 2) {
			threshold = 100 * (battler.totalhp - battler.hp) / battler.totalhp;
			if (ai.AIRandom(100) < threshold) {
				Debug.log_ai($"{battler.name} wants to switch to avoid being KO'd and losing because of the sudden death rule");
				next true;
			}
		}
		next false;
	}
)

//===============================================================================
// Pokémon is within 5 levels of the foe, and foe's last move was super-effective
// and powerful.
//===============================================================================
Battle.AI.Handlers.ShouldSwitch.add(:high_damage_from_foe,
	block: (battler, reserves, ai, battle) => {
		if (!ai.trainer.high_skill()) next false;
		if (battler.hp >= battler.totalhp / 2) next false;
		big_threat = false;
		ai.each_foe_battler(battler.side) do |b, i|
			if ((b.level - battler.level).abs > 5) continue;
			if (!b.battler.lastMoveUsed || !GameData.Move.exists(b.battler.lastMoveUsed)) continue;
			move_data = GameData.Move.get(b.battler.lastMoveUsed);
			if (move_data.status()) continue;
			eff = battler.effectiveness_of_type_against_battler(move_data.type, b);
			if (!Effectiveness.super_effective(eff) || move_data.power < 70) continue;
			switch_chance = (move_data.power > 90) ? 50 : 25;
			big_threat = (ai.AIRandom(100) < switch_chance);
			if (big_threat) break;
		}
		if (big_threat) {
			Debug.log_ai($"{battler.name} wants to switch because a foe has a powerful super-effective move");
			next true;
		}
		next false;
	}
)

//===============================================================================
//===============================================================================
//===============================================================================

//===============================================================================
// Don't bother switching if the battler will just faint from entry hazard damage
// upon switching back in, and if no reserve can remove the entry hazard(s).
// Switching out in this case means the battler becomes unusable, so it might as
// well stick around instead and do as much as it can.
//===============================================================================
Battle.AI.Handlers.ShouldNotSwitch.add(:lethal_entry_hazards,
	block: (battler, reserves, ai, battle) => {
		if (battle.rules["suddendeath"]) next false;
		// Check whether battler will faint from entry hazard(s)
		entry_hazard_damage = ai.calculate_entry_hazard_damage(battler.pokemon, battler.side);
		if (entry_hazard_damage < battler.hp) next false;
		// Check for Rapid Spin
		reserve_can_remove_hazards = false;
		foreach (var pkmn in reserves) { //'reserves.each' do => |pkmn|
			foreach (var move in pkmn.moves) { //'pkmn.moves.each' do => |move|
				reserve_can_remove_hazards = (move.function_code == "RemoveUserBindingAndEntryHazards");
				if (reserve_can_remove_hazards) break;
			}
			if (reserve_can_remove_hazards) break;
		}
		if (reserve_can_remove_hazards) next false;
		Debug.log_ai($"{battler.name} won't switch after all because it will faint from entry hazards if it switches back in");
		next true;
	}
)

//===============================================================================
// Don't bother switching (50% chance) if the battler knows a super-effective
// move.
//===============================================================================
Battle.AI.Handlers.ShouldNotSwitch.add(:battler_has_super_effective_move,
	block: (battler, reserves, ai, battle) => {
		if (battler.effects.PerishSong == 1) next false;
		if (battler.rough_end_of_round_damage >= battler.hp * 2 / 3) next false;
		if (battle.rules["suddendeath"]) next false;
		has_super_effective_move = false;
		foreach (var move in battler.battler.Moves) { //battler.battler.eachMove do => |move|
			if (move.pp == 0 && move.total_pp > 0) continue;
			if (move.statusMove()) continue;
			// NOTE: Ideally this would ignore moves that are unusable, but that would
			//       be too complicated to implement.
			move_type = move.type;
			if (ai.trainer.medium_skill()) move_type = move.CalcType(battler.battler);
			foreach (var b in ai.each_foe_battler(battler.side)) { //ai.each_foe_battler(battler.side) do => |b|
				// NOTE: Ideally this would ignore foes that move cannot target, but that
				//       is complicated enough to implement that I'm not bothering. It's
				//       also rare that it would matter.
				eff = b.effectiveness_of_type_against_battler(move_type, battler, move);
				has_super_effective_move = Effectiveness.super_effective(eff);
				if (has_super_effective_move) break;
			}
			if (has_super_effective_move) break;
		}
		if (has_super_effective_move && ai.AIRandom(100) < 50) {
			Debug.log_ai($"{battler.name} won't switch after all because it has a super-effective move");
			next true;
		}
		next false;
	}
)

//===============================================================================
// Don't bother switching if the battler has 4 or more positive stat stages.
// Negative stat stages are ignored.
//===============================================================================
Battle.AI.Handlers.ShouldNotSwitch.add(:battler_has_very_raised_stats,
	block: (battler, reserves, ai, battle) => {
		if (battle.rules["suddendeath"]) next false;
		stat_raises = 0;
		battler.stages.each_value(val => { if (val > 0) stat_raises += val; });
		if (stat_raises >= 4) {
			Debug.log_ai($"{battler.name} won't switch after all because it has a lot of raised stats");
			next true;
		}
		next false;
	}
)

//===============================================================================
// Don't bother switching if the battler has Wonder Guard and is immune to the
// foe's damaging attacks.
//===============================================================================
Battle.AI.Handlers.ShouldNotSwitch.add(:battler_is_immune_via_wonder_guard,
	block: (battler, reserves, ai, battle) => {
		if (battler.effects.PerishSong == 1) next false;
		if (battler.rough_end_of_round_damage >= battler.hp / 2) next false;
		if (!battler.has_active_ability(abilitys.WONDERGUARD)) next false;
		super_effective_foe = false;
		foreach (var b in ai.each_foe_battler(battler.side)) { //ai.each_foe_battler(battler.side) do => |b|
			if (!b.check_for_move do |m|
				if (!m.damagingMove()) next false;
				eff = battler.effectiveness_of_type_against_battler(m.CalcType(b.battler), b, m)) continue;
				next Effectiveness.super_effective(eff);
			}
			super_effective_foe = true;
			break;
		}
		if (!super_effective_foe) {
			Debug.log_ai($"{battler.name} won't switch after all because it has Wonder Guard and can't be damaged by foes");
		}
		next !super_effective_foe;
	}
)
