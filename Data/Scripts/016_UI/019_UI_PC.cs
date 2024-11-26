//===============================================================================
//
//===============================================================================
public void PCItemStorage() {
	command = 0;
	do { //loop; while (true);
		command = ShowCommandsWithHelp(null,
																		new {_INTL("Withdraw Item"),
																			_INTL("Deposit Item"),
																			_INTL("Toss Item"),
																			_INTL("Exit")},
																		new {_INTL("Take out items from the PC."),
																			_INTL("Store items in the PC."),
																			_INTL("Throw away items stored in the PC."),
																			_INTL("Go back to the previous menu.")}, -1, command);
		switch (command) {
			case 0:   // Withdraw Item
				if (!Game.GameData.PokemonGlobal.pcItemStorage) {
					Game.GameData.PokemonGlobal.pcItemStorage = new PCItemStorage();
				}
				if (Game.GameData.PokemonGlobal.pcItemStorage.empty()) {
					Message(_INTL("There are no items."));
				} else {
					FadeOutIn do;
						scene = new WithdrawItemScene();
						screen = new PokemonBagScreen(scene, Game.GameData.bag);
						screen.WithdrawItemScreen;
					}
				}
				break;
			case 1:   // Deposit Item
				FadeOutIn do;
					scene = new PokemonBag_Scene();
					screen = new PokemonBagScreen(scene, Game.GameData.bag);
					screen.DepositItemScreen;
				}
				break;
			case 2:   // Toss Item
				if (!Game.GameData.PokemonGlobal.pcItemStorage) {
					Game.GameData.PokemonGlobal.pcItemStorage = new PCItemStorage();
				}
				if (Game.GameData.PokemonGlobal.pcItemStorage.empty()) {
					Message(_INTL("There are no items."));
				} else {
					FadeOutIn do;
						scene = new TossItemScene();
						screen = new PokemonBagScreen(scene, Game.GameData.bag);
						screen.TossItemScreen;
					}
				}
				break;
			} else {
				break;
				break;
		}
	}
}

//===============================================================================
//
//===============================================================================
public void PCMailbox() {
	if (!Game.GameData.PokemonGlobal.mailbox || Game.GameData.PokemonGlobal.mailbox.length == 0) {
		Message(_INTL("There's no Mail here."));
	} else {
		do { //loop; while (true);
			command = 0;
			commands = new List<string>();
			foreach (var mail in Game.GameData.PokemonGlobal.mailbox) { //'Game.GameData.PokemonGlobal.mailbox.each' do => |mail|
				commands.Add(mail.sender);
			}
			commands.Add(_INTL("Cancel"));
			command = ShowCommands(null, commands, -1, command);
			if (command >= 0 && command < Game.GameData.PokemonGlobal.mailbox.length) {
				mailIndex = command;
				commandMail = Message(
					_INTL("What do you want to do with {1}'s Mail?", Game.GameData.PokemonGlobal.mailbox[mailIndex].sender),
					new {_INTL("Read"),
					_INTL("Move to Bag"),
					_INTL("Give"),
					_INTL("Cancel")}, -1
				);
				switch (commandMail) {
					case 0:   // Read
						FadeOutIn do;
							DisplayMail(Game.GameData.PokemonGlobal.mailbox[mailIndex]);
						}
						break;
					case 1:   // Move to Bag
						if (ConfirmMessage(_INTL("The message will be lost. Is that OK?"))) {
							if (Game.GameData.bag.add(Game.GameData.PokemonGlobal.mailbox[mailIndex].item)) {
								Message(_INTL("The Mail was returned to the Bag with its message erased."));
								Game.GameData.PokemonGlobal.mailbox.delete_at(mailIndex);
							} else {
								Message(_INTL("The Bag is full."));
							}
						}
						break;
					case 2:   // Give
						FadeOutIn do;
							sscene = new PokemonParty_Scene();
							sscreen = new PokemonPartyScreen(sscene, Game.GameData.player.party);
							sscreen.PokemonGiveMailScreen(mailIndex);
						}
						break;
				}
			} else {
				break;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public void TrainerPC() {
	Message("\\se[PC open]" + _INTL("{1} booted up the PC.", Game.GameData.player.name));
	TrainerPCMenu;
	SEPlay("PC close");
}

public void TrainerPCMenu() {
	command = 0;
	do { //loop; while (true);
		command = Message(_INTL("What do you want to do?"),
												new {_INTL("Item Storage"),
												_INTL("Mailbox"),
												_INTL("Turn Off")}, -1, null, command);
		switch (command) {
			case 0:  PCItemStorage; break;
			case 1:  PCMailbox; break;
			default:        break; break;
		}
	}
}

//===============================================================================
//
//===============================================================================
public void PokeCenterPC() {
	Message("\\se[PC open]" + _INTL("{1} booted up the PC.", Game.GameData.player.name));
	// Get all commands
	command_list = new List<string>();
	commands = new List<string>();
	MenuHandlers.each_available(:pc_menu) do |option, hash, name|
		command_list.Add(name);
		commands.Add(hash);
	}
	// Main loop
	command = 0;
	do { //loop; while (true);
		choice = Message(_INTL("Which PC should be accessed?"), command_list, -1, null, command);
		if (choice < 0) {
			PlayCloseMenuSE;
			break;
		}
		if (commands[choice]["effect"].call) break;
	}
	SEPlay("PC close");
}

public void GetStorageCreator() {
	return GameData.Metadata.get.storage_creator;
}

//===============================================================================
//
//===============================================================================

MenuHandlers.add(:pc_menu, :pokemon_storage, {
	"name"      => () => {
		next (Game.GameData.player.seen_storage_creator) ? _INTL("{1}'s PC", GetStorageCreator) : _INTL("Someone's PC");
	},
	"order"     => 10,
	"effect"    => menu => {
		Message("\\se[PC access]" + _INTL("The Pokémon Storage System was opened."));
		command = 0;
		do { //loop; while (true);
			command = ShowCommandsWithHelp(null,
																			new {_INTL("Organize Boxes"),
																				_INTL("Withdraw Pokémon"),
																				_INTL("Deposit Pokémon"),
																				_INTL("See ya!")},
																			new {_INTL("Organize the Pokémon in Boxes and in your party."),
																				_INTL("Move Pokémon stored in Boxes to your party."),
																				_INTL("Store Pokémon in your party in Boxes."),
																				_INTL("Return to the previous menu.")}, -1, command);
			if (command < 0) break;
			switch (command) {
				case 0:   // Organize
					FadeOutIn do;
						scene = new PokemonStorageScene();
						screen = new PokemonStorageScreen(scene, Game.GameData.PokemonStorage);
						screen.StartScreen(0);
					}
					break;
				case 1:   // Withdraw
					if (Game.GameData.PokemonStorage.party_full()) {
						Message(_INTL("Your party is full!"));
						continue;
					}
					FadeOutIn do;
						scene = new PokemonStorageScene();
						screen = new PokemonStorageScreen(scene, Game.GameData.PokemonStorage);
						screen.StartScreen(1);
					}
					break;
				case 2:   // Deposit
					count = 0;
					foreach (var p in Game.GameData.PokemonStorage.party) { //'Game.GameData.PokemonStorage.party.each' do => |p|
						if (p && !p.egg() && p.hp > 0) count += 1;
					}
					if (count <= 1) {
						Message(_INTL("Can't deposit the last Pokémon!"));
						continue;
					}
					FadeOutIn do;
						scene = new PokemonStorageScene();
						screen = new PokemonStorageScreen(scene, Game.GameData.PokemonStorage);
						screen.StartScreen(2);
					}
					break;
				} else {
					break;
					break;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pc_menu, :player_pc, {
	"name"      => () => next _INTL("{1}'s PC", Game.GameData.player.name),
	"order"     => 20,
	"effect"    => menu => {
		Message("\\se[PC access]" + _INTL("Accessed {1}'s PC.", Game.GameData.player.name));
		TrainerPCMenu;
		next false;
	}
});

MenuHandlers.add(:pc_menu, :close, {
	"name"      => _INTL("Log off"),
	"order"     => 100,
	"effect"    => menu => {
		next true;
	}
});
