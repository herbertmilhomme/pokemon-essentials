// All types except Shadow have Shadow as a weakness.
// Shadow has Shadow as a resistance.
// On a side note, the Shadow moves in Colosseum will not be affected by
// Weaknesses or Resistances, while in XD the Shadow-type is Super-Effective
// against all other types.
// 2/5 - display nature
//
// XD - Shadow Rush -- 55, 100 - Deals damage.
// Colosseum - Shadow Rush -- 90, 100
// If this attack is successful, user loses half of HP lost by opponent due to
// this attack (recoil). If user is in Hyper Mode, this attack has a good chance
// for a critical hit.

//===============================================================================
// Purify a Shadow Pokémon.
//===============================================================================
public void Purify(pkmn, scene) {
	if (!pkmn.shadowPokemon() || pkmn.heart_gauge != 0) return;
	Game.GameData.stats.shadow_pokemon_purified += 1;
	pkmn.shadow = false;
	pkmn.hyper_mode = false;
	pkmn.giveRibbon(:NATIONAL);
	scene.Display(_INTL("{1} opened the door to its heart!", pkmn.name));
	old_moves = new List<string>();
	pkmn.moves.each(m => old_moves.Add(m.id));
	pkmn.update_shadow_moves;
	pkmn.moves.each_with_index do |m, i|
		if (m.id == old_moves[i]) continue;
		scene.Display(_INTL("{1} regained the move {2}!", pkmn.name, m.name));
	}
	pkmn.record_first_moves;
	if (pkmn.saved_ev) {
		pkmn.add_evs(pkmn.saved_ev);
		pkmn.saved_ev = null;
	}
	if (pkmn.saved_exp) {
		newexp = pkmn.growth_rate.add_exp(pkmn.exp, (pkmn.saved_exp * 4 / 5) || 0);
		pkmn.saved_exp = null;
		newlevel = pkmn.growth_rate.level_from_exp(newexp);
		curlevel = pkmn.level;
		if (newexp != pkmn.exp) {
			scene.Display(_INTL("{1} regained {2} Exp. Points!", pkmn.name, newexp - pkmn.exp));
		}
		if (newlevel == curlevel) {
			pkmn.exp = newexp;
			pkmn.calc_stats;
		} else {
			ChangeLevel(pkmn, newlevel, scene);   // for convenience
			pkmn.exp = newexp;
		}
	}
	if (Game.GameData.PokemonSystem.givenicknames == 0 &&
		scene.Confirm(_INTL("Would you like to give a nickname to {1}?", pkmn.speciesName))) {
		newname = EnterPokemonName(_INTL("{1}'s nickname?", pkmn.speciesName),
																0, Pokemon.MAX_NAME_SIZE, "", pkmn);
		pkmn.name = newname;
	}
}

//===============================================================================
// Relic Stone scene.
//===============================================================================
public partial class RelicStoneScene {
	public void Purify() { }

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void Display(msg, brief = false) {
		UIHelper.Display(@sprites["msgwindow"], msg, brief) { Update };
	}

	public void Confirm(msg) {
		UIHelper.Confirm(@sprites["msgwindow"], msg) { Update };
	}

	public void StartScene(pokemon) {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@pokemon = pokemon;
		addBackgroundPlane(@sprites, "bg", "relicstonebg", @viewport);
		@sprites["msgwindow"] = new Window_AdvancedTextPokemon("");
		@sprites["msgwindow"].viewport = @viewport;
		@sprites["msgwindow"].x        = 0;
		@sprites["msgwindow"].y        = Graphics.height - 96;
		@sprites["msgwindow"].width    = Graphics.width;
		@sprites["msgwindow"].height   = 96;
		@sprites["msgwindow"].text     = "";
		@sprites["msgwindow"].visible  = true;
		DeactivateWindows(@sprites);
		FadeInAndShow(@sprites) { Update };
	}
}

//===============================================================================
//
//===============================================================================
public partial class RelicStoneScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void Display(x) {
		@scene.Display(x);
	}

	public void Confirm(x) {
		@scene.Confirm(x);
	}

	public void Update() { }

	public void Refresh() { }

	public void StartScreen(pokemon) {
		@scene.StartScene(pokemon);
		@scene.Purify;
		Purify(pokemon, self);
		@scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================
public void RelicStoneScreen(pkmn) {
	retval = true;
	FadeOutIn do;
		scene = new RelicStoneScene();
		screen = new RelicStoneScreen(scene);
		retval = screen.StartScreen(pkmn);
	}
	return retval;
}

//===============================================================================
//
//===============================================================================
public void RelicStone() {
	if (Game.GameData.player.party.none(pkmn => pkmn.purifiable())) {
		Message(_INTL("You have no Pokémon that can be purified."));
		return;
	}
	Message(_INTL("There's a Pokémon that may open the door to its heart!"));
	// Choose a purifiable Pokemon
	ChoosePokemon(1, 2, block: (pkmn) => {
		pkmn.able() && pkmn.shadowPokemon() && pkmn.heart_gauge == 0;
	});
	if (Game.GameData.game_variables[1] >= 0) {
		RelicStoneScreen(Game.GameData.player.party[Game.GameData.game_variables[1]]);
	}
}

//===============================================================================
// Shadow Pokémon in battle.
//===============================================================================
public partial class Battle {
	unless (method_defined(:__shadow__pbCanUseItemOnPokemon())) {
		alias __shadow__pbCanUseItemOnPokemon() CanUseItemOnPokemon();
	}

	public bool CanUseItemOnPokemon(item, pkmn, battler, scene, showMessages = true) {
		ret = __shadow__pbCanUseItemOnPokemon(item, pkmn, battler, scene, showMessages);
		if (ret && pkmn.hyper_mode && !new []{:JOYSCENT, :EXCITESCENT, :VIVIDSCENT}.Contains(item)) {
			scene.Display(_INTL("This item can't be used on that Pokémon."));
			return false;
		}
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	unless (method_defined(:__shadow__pbInitPokemon)) alias __shadow__pbInitPokemon InitPokemon;

	public void InitPokemon(*arg) {
		if (self.pokemonIndex > 0 && inHyperMode()) self.pokemon.hyper_mode = false;
		__shadow__pbInitPokemon(*arg);
		// Called into battle
		if (shadowPokemon()) {
			if (GameData.Types.exists(Types.SHADOW)) self.types = [:SHADOW];
			if (OwnedByPlayer()) self.pokemon.change_heart_gauge("battle");
		}
	}

	public bool shadowPokemon() {
		p = self.pokemon;
		return p&.shadowPokemon();
	}

	public bool inHyperMode() {
		if (fainted()) return false;
		p = self.pokemon;
		return p&.hyper_mode;
	}

	public void HyperMode() {
		if (fainted() || !shadowPokemon() || inHyperMode() || !OwnedByPlayer()) return;
		p = self.pokemon;
		if (@battle.Random(p.heart_gauge) <= p.max_gauge_size / 4) {
			p.hyper_mode = true;
			@battle.Display(_INTL("{1}'s emotions rose to a fever pitch!\nIt entered Hyper Mode!", self.ToString()));
		}
	}

	public void HyperModeObedience(move) {
		if (!inHyperMode()) return true;
		if (!move || move.type == types.SHADOW) return true;
		return rand(100) < 20;
	}
}

//===============================================================================
// Shadow item effects.
//===============================================================================
public void RaiseHappinessAndReduceHeart(pkmn, scene, multiplier, show_fail_message = true) {
	if (!pkmn.shadowPokemon() || (pkmn.happiness == 255 && pkmn.heart_gauge == 0)) {
		if (show_fail_message) scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	old_gauge = pkmn.heart_gauge;
	old_happiness = pkmn.happiness;
	pkmn.changeHappiness("vitamin");
	pkmn.change_heart_gauge("scent", multiplier);
	if (pkmn.heart_gauge == old_gauge) {
		scene.Display(_INTL("{1} turned friendly.", pkmn.name));
	} else if (pkmn.happiness == old_happiness) {
		scene.Display(_INTL("{1} adores you!\nThe door to its heart opened a little.", pkmn.name));
		pkmn.check_ready_to_purify;
	} else {
		scene.Display(_INTL("{1} turned friendly.\nThe door to its heart opened a little.", pkmn.name));
		pkmn.check_ready_to_purify;
	}
	return true;
}

ItemHandlers.UseOnPokemon.add(:JOYSCENT, block: (item, qty, pkmn, scene) => {
	ret = false;
	if (pkmn.hyper_mode) {
		scene.Display(_INTL("{1} came to its senses from the {2}.", pkmn.name, GameData.Item.get(item).name));
		pkmn.hyper_mode = false;
		ret = true;
	}
	next RaiseHappinessAndReduceHeart(pkmn, scene, 1, !ret) || ret;
});

ItemHandlers.UseOnPokemon.add(:EXCITESCENT, block: (item, qty, pkmn, scene) => {
	ret = false;
	if (pkmn.hyper_mode) {
		scene.Display(_INTL("{1} came to its senses from the {2}.", pkmn.name, GameData.Item.get(item).name));
		pkmn.hyper_mode = false;
		ret = true;
	}
	next RaiseHappinessAndReduceHeart(pkmn, scene, 2, !ret) || ret;
});

ItemHandlers.UseOnPokemon.add(:VIVIDSCENT, block: (item, qty, pkmn, scene) => {
	ret = false;
	if (pkmn.hyper_mode) {
		scene.Display(_INTL("{1} came to its senses from the {2}.", pkmn.name, GameData.Item.get(item).name));
		pkmn.hyper_mode = false;
		ret = true;
	}
	next RaiseHappinessAndReduceHeart(pkmn, scene, 3, !ret) || ret;
});

ItemHandlers.UseOnPokemon.add(:TIMEFLUTE, block: (item, qty, pkmn, scene) => {
	if (!pkmn.shadowPokemon() || pkmn.heart_gauge == 0 || pkmn.isSpecies(Speciess.LUGIA)) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	Purify(pkmn, scene);
	next true;
});

ItemHandlers.CanUseInBattle.add(:JOYSCENT, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!pokemon.shadowPokemon() || (pokemon.happiness == 255 && pokemon.heart_gauge == 0)) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.copy(:JOYSCENT, :EXCITESCENT, :VIVIDSCENT);

ItemHandlers.BattleUseOnPokemon.add(:JOYSCENT, block: (item, pokemon, battler, choices, scene) => {
	if (pokemon.hyper_mode) {
		pokemon.hyper_mode = false;
		scene.Display(_INTL("{1} came to its senses from the {2}!",
													battler&.ToString() || pokemon.name, GameData.Item.get(item).name));
	}
	RaiseHappinessAndReduceHeart(pokemon, scene, 1, false);
	next true;
});

ItemHandlers.BattleUseOnPokemon.add(:EXCITESCENT, block: (item, pokemon, battler, choices, scene) => {
	if (pokemon.hyper_mode) {
		pokemon.hyper_mode = false;
		scene.Display(_INTL("{1} came to its senses from the {2}!",
													battler&.ToString() || pokemon.name, GameData.Item.get(item).name));
	}
	RaiseHappinessAndReduceHeart(pokemon, scene, 2, false);
	next true;
});

ItemHandlers.BattleUseOnPokemon.add(:VIVIDSCENT, block: (item, pokemon, battler, choices, scene) => {
	if (pokemon.hyper_mode) {
		pokemon.hyper_mode = false;
		scene.Display(_INTL("{1} came to its senses from the {2}!",
													battler&.ToString() || pokemon.name, GameData.Item.get(item).name));
	}
	RaiseHappinessAndReduceHeart(pokemon, scene, 3, false);
	next true;
});

//===============================================================================
// Two turn attack. On first turn, halves the HP of all active Pokémon.
// Skips second turn (if successful). (Shadow Half)
//===============================================================================
public partial class Battle.Move.AllBattlersLoseHalfHPUserSkipsNextTurn : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.allBattlers.none(b => b.hp > 1)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.allBattlers.each do |b|
			if (b.hp > 1) b.ReduceHP(b.hp / 2, false);
		}
		@battle.Display(_INTL("Each Pokémon's HP was halved!"));
		@battle.allBattlers.each(b => b.ItemHPHealCheck);
		user.effects.HyperBeam = 2;
		user.currentMove = @id;
	}
}

//===============================================================================
// User takes recoil damage equal to 1/2 of its current HP. (Shadow End)
//===============================================================================
public partial class Battle.Move.UserLosesHalfHP : Battle.Move.RecoilMove {
	public void RecoilDamage(user, target) {
		return (int)Math.Round(user.hp / 2.0);
	}

	public void EffectAfterAllHits(user, target) {
		if (user.fainted() || target.damageState.unaffected) return;
		// NOTE: This move's recoil is not prevented by Rock Head/Magic Guard.
		amt = RecoilDamage(user, target);
		if (amt < 1) amt = 1;
		if (user.pokemon.isSpecies(Speciess.BASCULIN) && new []{2, 3}.Contains(user.pokemon.form)) {
			user.pokemon.evolution_counter += amt;
		}
		user.ReduceHP(amt, false);
		@battle.Display(_INTL("{1} is damaged by recoil!", user.ToString()));
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Starts shadow weather. (Shadow Sky)
//===============================================================================
public partial class Battle.Move.StartShadowSkyWeather : Battle.Move.WeatherMove {
	public override void initialize(battle, move) {
		base.initialize();
		@weatherType = Types.ShadowSky;
	}
}

//===============================================================================
// Ends the effects of Light Screen, Reflect and Safeguard on both sides.
// (Shadow Shed)
//===============================================================================
public partial class Battle.Move.RemoveAllScreensAndSafeguard : Battle.Move {
	public bool MoveFailed(user, targets) {
		will_fail = true;
		@battle.sides.each do |side|
			if (side.effects.AuroraVeil > 0 ||
													side.effects.LightScreen > 0 ||
													side.effects.Reflect > 0 ||
													side.effects.Safeguard > 0) will_fail = false;
		}
		if (will_fail) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.sides.each do |i|
			i.effects.AuroraVeil  = 0;
			i.effects.LightScreen = 0;
			i.effects.Reflect     = 0;
			i.effects.Safeguard   = 0;
		}
		@battle.Display(_INTL("It broke all barriers!"));
	}
}

//===============================================================================
//
//===============================================================================
public partial class Game_Temp {
	public int party_heart_gauges_before_battle		{ get { return _party_heart_gauges_before_battle; } set { _party_heart_gauges_before_battle = value; } }			protected int _party_heart_gauges_before_battle;
}

//===============================================================================
//
//===============================================================================

// Record current heart gauges of Pokémon in party, to see if they drop to zero
// during battle and need to say they're ready to be purified afterwards.
EventHandlers.add(:on_start_battle, :record_party_heart_gauges,
	block: () => {
		Game.GameData.game_temp.party_heart_gauges_before_battle = new List<string>();
		Game.GameData.player.party.each_with_index do |pkmn, i|
			Game.GameData.game_temp.party_heart_gauges_before_battle[i] = pkmn.heart_gauge;
		}
	}
)

EventHandlers.add(:on_end_battle, :check_ready_to_purify,
	block: (_outcome, _canLose) => {
		Game.GameData.game_temp.party_heart_gauges_before_battle.each_with_index do |value, i|
			pkmn = Game.GameData.player.party[i];
			if (!pkmn || !value || value == 0) continue;
			if (pkmn.heart_gauge == 0) pkmn.check_ready_to_purify;
		}
	}
)

EventHandlers.add(:on_player_step_taken, :lower_heart_gauges,
	block: () => {
		foreach (var pkmn in Game.GameData.player.able_party) { //'Game.GameData.player.able_party.each' do => |pkmn|
			if (pkmn.heart_gauge == 0) continue;
			if (!pkmn.heart_gauge_step_counter) pkmn.heart_gauge_step_counter = 0;
			pkmn.heart_gauge_step_counter += 1;
			if (pkmn.heart_gauge_step_counter < 256) continue;
			old_stage = pkmn.heartStage;
			pkmn.change_heart_gauge("walking");
			new_stage = pkmn.heartStage;
			if (new_stage == 0) {
				pkmn.check_ready_to_purify;
			} else if (new_stage != old_stage) {
				pkmn.update_shadow_moves;
			}
			pkmn.heart_gauge_step_counter = 0;
		}
		Game.GameData.PokemonGlobal.purifyChamber&.update;
	}
)
