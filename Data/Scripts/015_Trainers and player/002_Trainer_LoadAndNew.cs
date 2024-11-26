//===============================================================================
//
//===============================================================================
public void LoadTrainer(tr_type, tr_name, tr_version = 0) {
	tr_type_data = GameData.TrainerType.try_get(tr_type);
	if (!tr_type_data) raise _INTL("Trainer type {1} does not exist.", tr_type);
	tr_type = tr_type_data.id;
	trainer_data = GameData.Trainer.try_get(tr_type, tr_name, tr_version);
	return (trainer_data) ? trainer_data.to_trainer : null;
}

public void NewTrainer(tr_type, tr_name, tr_version, save_changes = true) {
	party = new List<string>();
	for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
		if (i == 0) {
			Message(_INTL("Please enter the first Pokémon.", i));
		} else if (!ConfirmMessage(_INTL("Add another Pokémon?"))) {
			break;
		}
		do { //loop; while (true);
			species = ChooseSpeciesList;
			if (species) {
				params = new ChooseNumberParams();
				params.setRange(1, GameData.GrowthRate.max_level);
				params.setDefaultValue(10);
				level = MessageChooseNumber(_INTL("Set the level for {1} (max. {2}).",
																						GameData.Species.get(species).name, params.maxNumber), params);
				party.Add(new {species, level});
				break;
			} else {
				if (i > 0) break;
				Message(_INTL("This trainer must have at least 1 Pokémon!"));
			}
		}
	}
	trainer = new {tr_type, tr_name, new List<string>(), party, tr_version};
	if (save_changes) {
		trainer_hash = {
			trainer_type = tr_type,
			real_name    = tr_name,
			version      = tr_version,
			pokemon      = [];
		}
		foreach (var pkmn in party) { //'party.each' do => |pkmn|
			trainer_hash.pokemon.Add(
				{
					species = pkmn[0],
					level   = pkmn[1];
				}
			);
		}
		// Add trainer's data to records
		trainer_hash.id = new {trainer_hash.trainer_type, trainer_hash.real_name, trainer_hash.version};
		GameData.Trainer.register(trainer_hash);
		GameData.Trainer.save;
		ConvertTrainerData;
		Message(_INTL("The Trainer's data was added to the list of battles and in PBS/trainers.txt."));
	}
	return trainer;
}

public void ConvertTrainerData() {
	tr_type_names = new List<string>();
	GameData.TrainerType.each(t => tr_type_names.Add(t.real_name));
	MessageTypes.setMessagesAsHash(MessageTypes.TRAINER_TYPE_NAMES, tr_type_names);
	Compiler.write_trainer_types;
	Compiler.write_trainers;
}

public void TrainerTypeCheck(trainer_type) {
	if (!Core.DEBUG) return true;
	if (GameData.TrainerType.exists(trainer_type)) return true;
	if (ConfirmMessage(_INTL("Add new trainer type {1}?", trainer_type.ToString()))) {
		TrainerTypeEditorNew(trainer_type.ToString());
	}
	MapInterpreter&.command_end;
	return false;
}

// Called from trainer events to ensure the trainer exists
public void TrainerCheck(tr_type, tr_name, max_battles, tr_version = 0) {
	if (!Core.DEBUG) return true;
	// Check for existence of trainer type
	TrainerTypeCheck(tr_type);
	tr_type_data = GameData.TrainerType.try_get(tr_type);
	if (!tr_type_data) return false;
	tr_type = tr_type_data.id;
	// Check for existence of trainer with given ID number
	if (GameData.Trainer.exists(tr_type, tr_name, tr_version)) return true;
	// Add new trainer
	if ((ConfirmMessage(_INTL("Add new trainer variant {1} (of {2}) for {3} {4}?",
														tr_version, max_battles, tr_type.ToString(), tr_name))) {
		NewTrainer(tr_type, tr_name, tr_version);
	}
	return true;
}

public void GetFreeTrainerParty(tr_type, tr_name) {
	tr_type_data = GameData.TrainerType.try_get(tr_type);
	if (!tr_type_data) raise _INTL("Trainer type {1} does not exist.", tr_type);
	tr_type = tr_type_data.id;
	for (int i = 256; i < 256; i++) { //for '256' times do => |i|
		if (!GameData.Trainer.try_get(tr_type, tr_name, i)) return i;
	}
	return -1;
}

public void MissingTrainer(tr_type, tr_name, tr_version) {
	tr_type_data = GameData.TrainerType.try_get(tr_type);
	if (!tr_type_data) raise _INTL("Trainer type {1} does not exist.", tr_type);
	tr_type = tr_type_data.id;
	if (!Core.DEBUG) {
		Debug.LogError(_INTL("Can't find trainer ({1}, {2}, ID {3})", tr_type.ToString(), tr_name, tr_version));
		//throw new ArgumentException(_INTL("Can't find trainer ({1}, {2}, ID {3})", tr_type.ToString(), tr_name, tr_version));
	}
	message = "";
	if (tr_version == 0) {
		message = _INTL("Add new trainer ({1}, {2})?", tr_type.ToString(), tr_name);
	} else {
		message = _INTL("Add new trainer ({1}, {2}, ID {3})?", tr_type.ToString(), tr_name, tr_version);
	}
	cmd = Message(message, new {_INTL("Yes"), _INTL("No")}, 2);
	if (cmd == 0) NewTrainer(tr_type, tr_name, tr_version);
	return cmd;
}
