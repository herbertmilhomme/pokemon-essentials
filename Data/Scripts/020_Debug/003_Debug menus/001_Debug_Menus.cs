//===============================================================================
//
//===============================================================================
public partial class CommandMenuList {
	public int currentList		{ get { return _currentList; } set { _currentList = value; } }			protected int _currentList;

	public void initialize() {
		@commands    = new List<string>();
		@currentList = :main;
	}

	public void add(option, hash, name = null, description = null) {
		@commands.Add(new {option, hash["parent"], name || hash["name"], description || hash["description"]});
	}

	public void list() {
		ret = new List<string>();
		@commands.each(cmd => { if (cmd[1] == @currentList) ret.Add(cmd[2]); });
		return ret;
	}

	public void getCommand(index) {
		count = 0;
		@commands.each do |cmd|
			if (cmd[1] != @currentList) continue;
			if (count == index) return cmd[0];
			count += 1;
		}
		return null;
	}

	public void getDesc(index) {
		count = 0;
		@commands.each do |cmd|
			if (cmd[1] != @currentList) continue;
			if (count == index && cmd[3]) return cmd[3];
			if (count == index) break;
			count += 1;
		}
		return "<No description available>";
	}

	public bool hasSubMenu(check_cmd) {
		@commands.each(cmd => { if (cmd[1] == check_cmd) return true; });
		return false;
	}

	public void getParent() {
		ret = null;
		@commands.each do |cmd|
			if (cmd[0] != @currentList) continue;
			ret = cmd[1];
			break;
		}
		if (!ret) return null;
		count = 0;
		@commands.each do |cmd|
			if (cmd[1] != ret) continue;
			if (cmd[0] == @currentList) return new {ret, count};
			count += 1;
		}
		return new {ret, 0};
	}
}

//===============================================================================
//
//===============================================================================
public void DebugMenu(show_all = true) {
	// Get all commands
	commands = new CommandMenuList();
	MenuHandlers.each_available(:debug_menu) do |option, hash, name|
		if (!show_all && !hash["always_show"].null() && !hash["always_show"]) continue;
		if (hash["description"].is_a(Proc)) {
			description = hash["description"].call;
		} else if (!hash["description"].null()) {
			description = _INTL(hash["description"]);
		}
		commands.add(option, hash, name, description);
	}
	// Setup windows
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	sprites = new List<string>();
	sprites["textbox"] = CreateMessageWindow;
	sprites["textbox"].letterbyletter = false;
	sprites["cmdwindow"] = new Window_CommandPokemonEx(commands.list);
	cmdwindow = sprites["cmdwindow"];
	cmdwindow.x        = 0;
	cmdwindow.y        = 0;
	cmdwindow.width    = Graphics.width;
	cmdwindow.height   = Graphics.height - sprites["textbox"].height;
	cmdwindow.viewport = viewport;
	cmdwindow.visible  = true;
	sprites["textbox"].text = commands.getDesc(cmdwindow.index);
	FadeInAndShow(sprites);
	// Main loop
	ret = -1;
	refresh = true;
	do { //loop; while (true);
		do { //loop; while (true);
			oldindex = cmdwindow.index;
			cmdwindow.update;
			if (refresh || cmdwindow.index != oldindex) {
				sprites["textbox"].text = commands.getDesc(cmdwindow.index);
				refresh = false;
			}
			Graphics.update;
			Input.update;
			if (Input.trigger(Input.BACK)) {
				parent = commands.getParent;
				if (parent) {
					PlayCancelSE
					commands.currentList = parent[0];
					cmdwindow.commands = commands.list;
					cmdwindow.index = parent[1];
					refresh = true;
				} else {
					ret = -1;
					break;
				}
			} else if (Input.trigger(Input.USE)) {
				ret = cmdwindow.index;
				break;
			}
		}
		if (ret < 0) break;
		cmd = commands.getCommand(ret);
		if (commands.hasSubMenu(cmd)) {
			PlayDecisionSE;
			commands.currentList = cmd;
			cmdwindow.commands = commands.list;
			cmdwindow.index = 0;
			refresh = true;
		} else if (cmd == :warp) {
			if (MenuHandlers.call(:debug_menu, cmd, "effect", sprites, viewport)) return;
		} else {
			MenuHandlers.call(:debug_menu, cmd, "effect");
		}
	}
	PlayCloseMenuSE;
	FadeOutAndHide(sprites);
	DisposeMessageWindow(sprites["textbox"]);
	DisposeSpriteHash(sprites);
	viewport.dispose;
}

//===============================================================================
//
//===============================================================================
public static partial class PokemonDebugMixin {
	public void PokemonDebug(pkmn, pkmnid, heldpoke = null, settingUpBattle = false) {
		// Get all commands
		commands = new CommandMenuList();
		MenuHandlers.each_available(:pokemon_debug_menu) do |option, hash, name|
			if (settingUpBattle && !hash["always_show"].null() && !hash["always_show"]) continue;
			commands.add(option, hash, name);
		}
		// Main loop
		command = 0;
		do { //loop; while (true);
			command = ShowCommands(_INTL("Do what with {1}?", pkmn.name), commands.list, command);
			if (command < 0) {
				parent = commands.getParent;
				if (!parent) break;
				commands.currentList = parent[0];
				command = parent[1];
			} else {
				cmd = commands.getCommand(command);
				if (commands.hasSubMenu(cmd)) {
					commands.currentList = cmd;
					command = 0;
				} else if (MenuHandlers.call(:pokemon_debug_menu, cmd, "effect", pkmn, pkmnid, heldpoke, settingUpBattle, self)) {
					break;
				}
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public static partial class Battle.DebugMixin {
	public void BattleDebug(battle, show_all = true) {
		// Get all commands
		commands = new CommandMenuList();
		MenuHandlers.each_available(:battle_debug_menu) do |option, hash, name|
			if (!show_all && !hash["always_show"].null() && !hash["always_show"]) continue;
			if (hash["description"].is_a(Proc)) {
				description = hash["description"].call;
			} else if (!hash["description"].null()) {
				description = _INTL(hash["description"]);
			}
			commands.add(option, hash, name, description);
		}
		// Setup windows
		viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		viewport.z = 99999;
		sprites = new List<string>();
		sprites["textbox"] = CreateMessageWindow;
		sprites["textbox"].letterbyletter = false;
		sprites["cmdwindow"] = new Window_CommandPokemonEx(commands.list);
		cmdwindow = sprites["cmdwindow"];
		cmdwindow.x        = 0;
		cmdwindow.y        = 0;
		cmdwindow.width    = Graphics.width / 2;
		cmdwindow.height   = Graphics.height - sprites["textbox"].height;
		cmdwindow.viewport = viewport;
		cmdwindow.visible  = true;
		sprites["textbox"].text = commands.getDesc(cmdwindow.index);
		// Main loop
		ret = -1;
		refresh = true;
		do { //loop; while (true);
			do { //loop; while (true);
				oldindex = cmdwindow.index;
				cmdwindow.update;
				if (refresh || cmdwindow.index != oldindex) {
					sprites["textbox"].text = commands.getDesc(cmdwindow.index);
					refresh = false;
				}
				Graphics.update;
				Input.update;
				if (Input.trigger(Input.BACK)) {
					parent = commands.getParent;
					if (parent) {
						PlayCancelSE
						commands.currentList = parent[0];
						cmdwindow.commands = commands.list;
						cmdwindow.index = parent[1];
						refresh = true;
					} else {
						ret = -1;
						break;
					}
				} else if (Input.trigger(Input.USE)) {
					ret = cmdwindow.index;
					break;
				}
			}
			if (ret < 0) break;
			cmd = commands.getCommand(ret);
			if (commands.hasSubMenu(cmd)) {
				PlayDecisionSE;
				commands.currentList = cmd;
				cmdwindow.commands = commands.list;
				cmdwindow.index = 0;
				refresh = true;
			} else {
				MenuHandlers.call(:battle_debug_menu, cmd, "effect", battle);
			}
		}
		PlayCloseMenuSE;
		DisposeMessageWindow(sprites["textbox"]);
		DisposeSpriteHash(sprites);
		viewport.dispose;
	}

	public void BattleDebugBattlerInfo(battler) {
		ret = "";
		if (battler.null()) return ret;
		// Battler index, name
		ret += string.Format("[{0}] {0}", battler.index, battler.ToString());
		ret += "\n";
		// Species
		ret += _INTL("Species: {1}", GameData.Species.get(battler.species).name);
		ret += "\n";
		// Form number
		ret += _INTL("Form: {1}", battler.form);
		ret += "\n";
		// Level, gender, shininess
		ret += _INTL("Level {1}, {2}", battler.level,
								(battler.pokemon.male()) ? "♂" : (battler.pokemon.female()) ? "♀" : _INTL("genderless"))
		if (battler.pokemon.shiny()) ret += ", " + _INTL("shiny");
		ret += "\n";
		// HP
		ret += _INTL("HP: {1}/{2} ({3}%)", battler.hp, battler.totalhp, (100.0 * battler.hp / battler.totalhp).ToInt());
		ret += "\n";
		// Status
		ret += _INTL("Status: {1}", GameData.Status.get(battler.status).name);
		switch (battler.status) {
			case :SLEEP:
				ret += " " + _INTL("({1} rounds left)", battler.statusCount);
				break;
			case :POISON:
				if (battler.statusCount > 0) {
					ret += " " + _INTL("(toxic, {1}/16)", battler.effects.Toxic);
				}
				break;
		}
		ret += "\n";
		// Stat stages
		stages = new List<string>();
		foreach (var stat in GameData.Stat) { //GameData.Stat.each_battle do => |stat|
			if (battler.stages[stat.id] == 0) continue;
			stage_text = "";
			if (battler.stages[stat.id] > 0) stage_text += "+";
			stage_text += battler.stages[stat.id].ToString();
			stage_text += " " + stat.name_brief;
			stages.Add(stage_text);
		}
		ret += _INTL("Stat stages: {1}", (stages.empty()) ? "-" : stages.join(", "));
		ret += "\n";
		// Ability
		ret += _INTL("Ability: {1}", (battler.ability) ? battler.abilityName : "-");
		ret += "\n";
		// Held item
		ret += _INTL("Item: {1}", (battler.item) ? battler.itemName : "-");
		return ret;
	}

	public void BattleDebugPokemonInfo(pkmn) {
		ret = "";
		if (pkmn.null()) return ret;
		sp_data = pkmn.species_data;
		// Name, species
		ret += string.Format("{0} ({0})", pkmn.name, sp_data.name);
		ret += "\n";
		// Form number
		ret += _INTL("Form: {1}", sp_data.form);
		ret += "\n";
		// Level, gender, shininess
		ret += _INTL("Level {1}, {2}", pkmn.level,
								(pkmn.male()) ? "♂" : (pkmn.female()) ? "♀" : _INTL("genderless"))
		if (pkmn.shiny()) ret += ", " + _INTL("shiny");
		ret += "\n";
		// HP
		ret += _INTL("HP: {1}/{2} ({3}%)", pkmn.hp, pkmn.totalhp, (100.0 * pkmn.hp / pkmn.totalhp).ToInt());
		ret += "\n";
		// Status
		ret += _INTL("Status: {1}", GameData.Status.get(pkmn.status).name);
		switch (pkmn.status) {
			case :SLEEP:
				ret += " " + _INTL("({1} rounds left)", pkmn.statusCount);
				break;
			case :POISON:
				if (pkmn.statusCount > 0) ret += " " + _INTL("(toxic)");
				break;
		}
		ret += "\n";
		// Ability
		ret += _INTL("Ability: {1}", pkmn.ability&.name || "-");
		ret += "\n";
		// Held item
		ret += _INTL("Item: {1}", pkmn.item&.name || "-");
		return ret;
	}

	public void BattlePokemonDebug(pkmn, battler = null) {
		// Get all commands
		commands = new CommandMenuList();
		MenuHandlers.each_available(:battle_pokemon_debug_menu) do |option, hash, name|
			if (battler && hash["usage"] == :pokemon) continue;
			if (!battler && hash["usage"] == :battler) continue;
			commands.add(option, hash, name);
		}
		// Setup windows
		viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		viewport.z = 99999;
		sprites = new List<string>();
		sprites["infowindow"] = new Window_AdvancedTextPokemon("");
		infowindow = sprites["infowindow"];
		infowindow.x        = 0;
		infowindow.y        = 0;
		infowindow.width    = Graphics.width / 2;
		infowindow.height   = Graphics.height;
		infowindow.viewport = viewport;
		infowindow.visible  = true;
		sprites["dummywindow"] = new Window_AdvancedTextPokemon("");
		sprites["dummywindow"].y = Graphics.height;
		sprites["dummywindow"].width = Graphics.width;
		sprites["dummywindow"].height = 0;
		// Main loop
		need_refresh = true;
		cmd = 0;
		do { //loop; while (true);
			if (need_refresh) {
				if (battler) {
					sprites["infowindow"].text = BattleDebugBattlerInfo(battler);
				} else {
					sprites["infowindow"].text = BattleDebugPokemonInfo(pkmn);
				}
				need_refresh = false;
			}
			// Choose a command
			cmd = Kernel.ShowCommands(sprites["dummywindow"], commands.list, -1, cmd);
			if (cmd < 0) {   // Cancel
				parent = commands.getParent;
				if (parent) {   // Go up a level
					commands.currentList = parent[0];
					cmd = parent[1];
				} else {   // Exit
					break;
				}
			} else {
				real_cmd = commands.getCommand(cmd);
				if (commands.hasSubMenu(real_cmd)) {
					commands.currentList = real_cmd;
					cmd = 0;
				} else {
					MenuHandlers.call(:battle_pokemon_debug_menu, real_cmd, "effect", pkmn, battler, self);
					need_refresh = true;
				}
			}
		}
		DisposeSpriteHash(sprites);
		viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPartyScreen {
	include PokemonDebugMixin;
}

//===============================================================================
//
//===============================================================================
public partial class PokemonStorageScreen {
	include PokemonDebugMixin;
}

//===============================================================================
//
//===============================================================================
public partial class PokemonDebugPartyScreen {
	include PokemonDebugMixin;
}

//===============================================================================
//
//===============================================================================
public partial class Battle {
	include Battle.DebugMixin;
}
