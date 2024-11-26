//===============================================================================
// Conversions required to support backwards compatibility with old save files
// (within reason).
//===============================================================================

SaveData.register_conversion(:v21_replace_phone_data) do;
	essentials_version 21;
	display_title "Updating Phone data format";
	to_value :global_metadata do |global|
		if (!global.phone) {
			global.instance_eval do;
				@phone = new Phone();
				@phoneTime = null;   // Don't bother using this
				if (@phoneNumbers) {
					@phoneNumbers.each do |contact|
						if (contact.length > 4) {
							// Trainer
							@phone.add(contact[6], contact[7], contact[1], contact[2], contact[5], 0);
							new_contact = @phone.get(contact[1], contact[2], 0);
							new_contact.visible = contact[0];
							new_contact.rematch_flag = (int)Math.Max(contact[4] - 1, 0);
						} else {
							// Non-trainer
							@phone.add(contact[3], contact[2], contact[1]);
						}
					}
					@phoneNumbers = null;
				}
			}
		}
	}
}

//===============================================================================

SaveData.register_conversion(:v21_replace_flute_booleans) do;
	essentials_version 21;
	display_title "Updating Black/White Flute variables";
	to_value :map_metadata do |metadata|
		metadata.instance_eval do;
			if (!@blackFluteUsed.null()) {
				if (Settings.FLUTES_CHANGE_WILD_ENCOUNTER_LEVELS) {
					@higher_level_wild_pokemon = @blackFluteUsed;
				} else {
					@lower_encounter_rate = @blackFluteUsed;
				}
				@blackFluteUsed = null;
			}
			if (!@whiteFluteUsed.null()) {
				if (Settings.FLUTES_CHANGE_WILD_ENCOUNTER_LEVELS) {
					@lower_level_wild_pokemon = @whiteFluteUsed;
				} else {
					@higher_encounter_rate = @whiteFluteUsed;
				}
				@whiteFluteUsed = null;
			}
		}
	}
}

//===============================================================================

SaveData.register_conversion(:v21_add_bump_stat) do;
	essentials_version 21;
	display_title "Adding a bump stat";
	to_value :stats do |stats|
		stats.instance_eval do;
			if (!@bump_count) @bump_count = 0;
		}
	}
}

//===============================================================================

SaveData.register_conversion(:v22_add_primal_reversion_stat) do;
	essentials_version 22;
	display_title "Adding a primal reversion stat";
	to_value :stats do |stats|
		stats.instance_eval do;
			if (!@primal_reversion_count) @primal_reversion_count = 0;
		}
	}
}
