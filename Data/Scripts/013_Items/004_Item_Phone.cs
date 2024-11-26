//===============================================================================
//
//===============================================================================
using System;

public partial class Phone {
	public int contacts		{ get { return _contacts; } set { _contacts = value; } }			protected int _contacts;
	public int rematch_variant		{ get { return _rematch_variant; } set { _rematch_variant = value; } }			protected int _rematch_variant;
	public int rematches_enabled		{ get { return _rematches_enabled; } set { _rematches_enabled = value; } }			protected int _rematches_enabled;
	public int time_to_next_call		{ get { return _time_to_next_call; } set { _time_to_next_call = value; } }			protected int _time_to_next_call;
	public int last_refresh_time		{ get { return _last_refresh_time; } set { _last_refresh_time = value; } }			protected int _last_refresh_time;

	public void initialize() {
		@contacts = new List<string>();
		@rematch_variant = 0;   // Original battle is 0, first rematch is 1, etc.
		@rematches_enabled = Settings.PHONE_REMATCHES_POSSIBLE_FROM_BEGINNING;
		@time_to_next_call = 0.0;
		@last_refresh_time = 0;
	}

	// Returns a visible contact only.
	public void get(trainer, *args) {
		@contacts.each do |contact|
			if (!contact.visible()) continue;
			if (contact.trainer() != trainer) continue;
			if (trainer) {
				if (contact.trainer_type != args[0] ||
								contact.name != args[1] || contact.start_version != (args[2] || 0)) continue;
			} else {
				if (contact.name != args[0]) continue;
			}
			return contact;
		}
		return null;
	}

	public void get_version(trainer_type, name, start_version = 0) {
		if (!GameData.TrainerType.exists(trainer_type)) return 0;
		trainer_type = GameData.TrainerType.get(trainer_type).id;
		contact = get(true, trainer_type, name, start_version);
		return (contact) ? contact.version : 0;
	}

	// Trainer type, name[, start_version]
	// Name
	public bool can_add(*args) {
		if (!Game.GameData.player.has_pokegear) return false;
		if (args.length == 1) {
			// Non-trainer (name only)
			if (get(false, args[0])) return false;
		} else {
			// Trainer (has at least trainer type and name)
			if (!GameData.TrainerType.exists(args[0])) return false;
			trainer_type = GameData.TrainerType.get(args[0]).id;
			if (get(true, trainer_type, args[1], args[2] || 0)) return false;
		}
		return true;
	}

	// Event, trainer type, name, versions_count = 1, start_version = 0, common event ID = 0
	// Map ID, event ID, trainer type, name, versions_count = 1, start_version = 0, common event ID = 0
	// Map ID, name, common event ID
	public void add(*args) {
		if (args[0].is_a(Game_Event)) {
			// Trainer
			if (!GameData.TrainerType.exists(args[1])) return false;
			trainer_type = GameData.TrainerType.get(args[1]).id;
			name = args[2];
			contact = get(true, trainer_type, name, args[3] || 0);
			if (contact) {
				contact.visible = true;
				@contacts.delete(contact);
			} else {
				contact = new Contact(true, args[0].map_id, args[0].id,
															trainer_type, name, args[3], args[4], args[5]);
			}
		} else if (args[1].is_a(Numeric)) {
			// Trainer
			if (!GameData.TrainerType.exists(args[2])) return false;
			trainer_type = GameData.TrainerType.get(args[2]).id;
			name = args[3];
			contact = get(true, trainer_type, name, args[4] || 0);
			if (contact) {
				contact.visible = true;
				@contacts.delete(contact);
			} else {
				contact = new Contact(true, args[0], args[1],
															trainer_type, name, args[4], args[5], args[6]);
			}
		} else {
			// Non-trainer
			name = args[1];
			contact = get(false, name);
			if (contact) {
				contact.visible = true;
				@contacts.delete(contact);
			} else {
				contact = new Contact(false, *args);
			}
		}
		@contacts.Add(contact);
		sort_contacts;
		return true;
	}

	// Rearranges the list of phone contacts to put all visible contacts first,
	// followed by all invisible contacts.
	public void sort_contacts() {
		new_contacts = new List<string>();
		for (int i = 2; i < 2; i++) { //for '2' times do => |i|
			@contacts.each do |con|
				if ((i == 0 && !con.visible()) || (i == 1 && con.visible())) continue;
				new_contacts.Add(con);
			}
		}
		@contacts = new_contacts;
	}

	//-----------------------------------------------------------------------------

	// Checks once every second.
	public void refresh_ready_trainers() {
		if (!@rematches_enabled) return;
		time = GetTimeNow.ToInt();
		if (@last_refresh_time == time) return;
		@last_refresh_time = time;
		@contacts.each do |contact|
			if (!contact.trainer() || !contact.visible()) continue;
			if (contact.rematch_flag > 0) continue;   // Already ready for rematch
			if (contact.time_to_ready <= 0) {
				contact.time_to_ready = rand(20...40) * 60;   // 20-40 minutes
			}
			contact.time_to_ready -= 1;
			if (contact.time_to_ready > 0) continue;
			contact.rematch_flag = 1;   // Ready for rematch
			contact.set_trainer_event_ready_for_rematch;
		}
	}

	public void reset_after_win(trainer_type, name, start_version = 0) {
		if (!GameData.TrainerType.exists(trainer_type)) return;
		trainer_type = GameData.TrainerType.get(trainer_type).id;
		contact = get(true, trainer_type, name, start_version);
		if (!contact) return;
		contact.increment_version;
		contact.rematch_flag = 0;
		contact.time_to_ready = 0;
	}

	//-----------------------------------------------------------------------------

	public static void rematch_variant() {
		return Game.GameData.PokemonGlobal.phone.rematch_variant;
	}

	public static void rematch_variant() {=(value)
		Game.GameData.PokemonGlobal.phone.rematch_variant = value;
	}

	public static void rematches_enabled() {
		return Game.GameData.PokemonGlobal.phone.rematches_enabled;
	}

	public static void rematches_enabled() {=(value)
		Game.GameData.PokemonGlobal.phone.rematches_enabled = value;
	}

	public static void get_trainer(*args) {
		return Game.GameData.PokemonGlobal.phone.get(true, *args);
	}

	public static bool can_add(*args) {
		return Game.GameData.PokemonGlobal.phone.can_add(*args);
	}

	public static void add(*args) {
		ret = Game.GameData.PokemonGlobal.phone.add(*args);
		if (ret) {
			if (args[0].is_a(Game_Event)) {
				contact = Game.GameData.PokemonGlobal.phone.get(true, args[1], args[2], (args[4] || 0));
			} else if (args[1].is_a(Numeric)) {
				contact = Game.GameData.PokemonGlobal.phone.get(true, args[2], args[3], (args[5] || 0));
			} else {
				contact = Game.GameData.PokemonGlobal.phone.get(false, args[1]);
			}
			Message("\\me[Register phone]" + _INTL("Registered {1} in the Pokégear!", contact.display_name) + "\\wtnp[60]");
		}
		return ret;
	}

	public static void add_silent(*args) {
		return Game.GameData.PokemonGlobal.phone.add(*args);
	}

	public static void variant(trainer_type, name, start_version = 0) {
		contact = Game.GameData.PokemonGlobal.phone.get(trainer_type, name, start_version);
		return (contact) ? contact.variant : 0;
	}

	public static void increment_version(trainer_type, name, start_version = 0) {
		contact = Game.GameData.PokemonGlobal.phone.get(trainer_type, name, start_version);
		if (contact) contact.increment_version;
	}

	public static void battle(trainer_type, name, start_version = 0) {
		contact = Game.GameData.PokemonGlobal.phone.get(true, trainer_type, name, start_version);
		if (!contact) return false;
		return TrainerBattle.start(trainer_type, name, contact.next_version);
	}

	public static void reset_after_win(trainer_type, name, start_version = 0) {
		Game.GameData.PokemonGlobal.phone.reset_after_win(trainer_type, name, start_version);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Phone {
	public partial class Contact {
		public int map_id		{ get { return _map_id; } set { _map_id = value; } }			protected int _map_id;
		public int event_id		{ get { return _event_id; } set { _event_id = value; } }			protected int _event_id;
		public int name		{ get { return _name; } set { _name = value; } }			protected int _name;
		public int trainer_type		{ get { return _trainer_type; } set { _trainer_type = value; } }			protected int _trainer_type;
		public int start_version		{ get { return _start_version; } set { _start_version = value; } }			protected int _start_version;
		public int versions_count		{ get { return _versions_count; } set { _versions_count = value; } }			protected int _versions_count;
		/// <summary>:version is the last trainer version that was beaten</summary>
		public int version      { get { return _version; } set { _version = value; } }			protected int _version;
		public int time_to_ready		{ get { return _time_to_ready; } set { _time_to_ready = value; } }			protected int _time_to_ready;
		public int rematch_flag		{ get { return _rematch_flag; } set { _rematch_flag = value; } }			protected int _rematch_flag;
		public int common_event_id		{ get { return _common_event_id; } set { _common_event_id = value; } }			protected int _common_event_id;
		public int visible		{ get { return _visible; } }			protected int _visible;

		// Map ID, event ID, trainer type, name, versions count = 1, start version = 0
		// Map ID, name, common event ID
		public void initialize(trainer, *args) {
			@trainer = trainer;
			@map_id = args[0];
			if (@trainer) {
				// Trainer
				@event_id        = args[1];
				@trainer_type    = args[2];
				@name            = args[3];
				@versions_count  = (int)Math.Max(args[4] || 1, 1);   // Includes the original version
				@start_version   = args[5] || 0;
				@version         = @start_version;
				@time_to_ready   = 0;
				@rematch_flag    = 0;   // 0=counting down, 1=ready for rematch, 2=ready and told player
				@common_event_id = args[6] || 0;
			} else {
				// Non-trainer
				@name            = args[1];
				@common_event_id = args[2] || 0;
			}
			@visible = true;
		}

		public bool trainer() {
			return @trainer;
		}

		public bool visible() {
			return @visible;
		}

		public int visible { set {
			if (@visible == value) return;
			@visible = value;
			if (!value && trainer()) {
				@time_to_ready = 0;
				@rematch_flag = 0;
				Game.GameData.game_self_switches[new {@map_id, @event_id, "A"	}] = true;
				Game.GameData.game_map.need_refresh = true;
			}
			}
		}

		public bool can_hide() {
			return trainer();
		}

		public bool common_event_call() {
			return @common_event_id > 0;
		}

		public bool can_rematch() {
			return trainer() && @rematch_flag >= 1;
		}

		public void display_name() {
			if (trainer()) {
				return string.Format("{0} {0}", GameData.TrainerType.get(@trainer_type).name,
											GetMessageFromHash(MessageTypes.TRAINER_NAMES, @name));
			}
			return _INTL(@name);
		}

		// Original battle is 0, first rematch is 1, etc.
		public void variant() {
			if (!trainer()) return 0;
			return @version - @start_version;
		}

		// Returns the version of this trainer to be battled next.
		public void next_version() {
			var = variant + 1;
			var = (int)Math.Min(var, Game.GameData.PokemonGlobal.phone.rematch_variant, @versions_count - 1);
			return @start_version + var;
		}

		public void increment_version() {
			if (!trainer()) return;
			@version = next_version;
		}

		public void set_trainer_event_ready_for_rematch() {
			if (!@trainer) return;
			Game.GameData.game_self_switches[new {@map_id, @event_id, "A"}] = false;
			Game.GameData.game_self_switches[new {@map_id, @event_id, "B"}] = true;
			Game.GameData.game_map.need_refresh = true;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class Phone {
	public static partial class Call {
		#region Class Functions
		#endregion

		public bool can_make() {
			if (Game.GameData.game_map.metadata&.has_flag("NoPhoneSignal")) return false;
			return true;
		}

		// For the player initiating the call.
		public bool can_call_contact(contact) {
			if (!contact) return false;
			if (!can_make()) {
				Message(_INTL("There is no phone signal here..."));
				return false;
			}
			if (!contact.trainer()) return true;
			if (contact.map_id == Game.GameData.game_map.map_id) {
				Message(_INTL("The Trainer is close by.\nTalk to the Trainer in person!"));
				return false;
			}
			caller_map_metadata = GameData.MapMetadata.try_get(contact.map_id);
			this_map_metadata = Game.GameData.game_map.metadata;
			if (!caller_map_metadata || !caller_map_metadata.town_map_position ||
				!this_map_metadata || !this_map_metadata.town_map_position ||
				caller_map_metadata.town_map_position[0] != this_map_metadata.town_map_position[0]) {
				Message(_INTL("The Trainer is out of range."));
				return false;
			}
			return true;
		}

		// Get a random trainer contact from the region the player is currently in,
		// but is not in the same map as the player.
		public void get_random_trainer_for_incoming_call() {
			player_location = Game.GameData.game_map.metadata&.town_map_position;
			if (!player_location) return null;
			player_region = player_location[0];
			valid_contacts = new List<string>();
			foreach (var contact in Game.GameData.PokemonGlobal.phone.contacts) { //'Game.GameData.PokemonGlobal.phone.contacts.each' do => |contact|
				if (!contact.trainer() || !contact.visible()) continue;
				if (contact.map_id == Game.GameData.game_map.map_id ||
								GetMapNameFromId(contact.map_id) == Game.GameData.game_map.name) continue;
				caller_map_metadata = GameData.MapMetadata.try_get(contact.map_id);
				if (!caller_map_metadata || !caller_map_metadata.town_map_position) continue;
				if (caller_map_metadata.town_map_position[0] != player_region) continue;
				valid_contacts.Add(contact);
			}
			return valid_contacts.sample;
		}

		//-----------------------------------------------------------------------------

		public void make_incoming() {
			if (!can_make()) return;
			contact = get_random_trainer_for_incoming_call;
			if (!contact) return;
			if (contact.common_event_call()) {
				if (!CommonEvent(contact.common_event_id)) {
					Message(_INTL("{1}'s messages not defined.\nCouldn't call common event {2}.",
													contact.display_name, contact.common_event_id));
				}
			} else {
				call = generate_trainer_dialogue(contact);
				play(call, contact);
			}
		}

		// Phone.Contact
		// Trainer type, name[, start_version]
		// Name (for non-trainers)
		public void make_outgoing(*args) {
			if (args[0].is_a(Phone.Contact)) {
				contact = args[0];
			} else if (args.length > 1) {
				contact = Phone.get(true, args[0], args[1], args[2] || 0);   // Trainer
			} else {
				contact = Phone.get(false, args[0]);   // Non-trainer
			}
			if (!contact) raise _INTL("Couldn't find phone contact given: {1}.", args.inspect);
			if (!can_call_contact(contact)) return;
			if (contact.common_event_call()) {
				if (!CommonEvent(contact.common_event_id)) {
					Message(_INTL("{1}'s messages not defined.\nCouldn't call common event {2}.",
													contact.display_name, contact.common_event_id));
				}
			} else {
				call = generate_trainer_dialogue(contact);
				play(call, contact);
			}
		}

		public void start_message(contact = null) {
			Message("......\\wt[5] ......\1");
		}

		public void play(dialogue, contact) {
			start_message(contact);
			contact_pokemon_species  = get_random_contact_pokemon_species(contact);
			random_encounter_species = get_random_encounter_species(contact);
			contact_map_name         = get_map_name(contact);
			gender_colour_text       = "";
			if (Settings.COLOR_PHONE_CALL_MESSAGES_BY_CONTACT_GENDER && contact.trainer()) {
				data = GameData.TrainerType.try_get(contact.trainer_type);
				if (data) {
					switch (data.gender) {
						case 0:  gender_colour_text = "\\b"; break;
						case 1:  gender_colour_text = "\\r"; break;
					}
				}
			}
			messages = dialogue.split("\\m");
			messages.each_with_index do |message, i|
				message = System.Text.RegularExpressions.Regex.Replace(message, "\\TN", _INTL(contact.name));
				message = System.Text.RegularExpressions.Regex.Replace(message, "\\TP", contact_pokemon_species);
				message = System.Text.RegularExpressions.Regex.Replace(message, "\\TE", random_encounter_species);
				message = System.Text.RegularExpressions.Regex.Replace(message, "\\TM", contact_map_name);
				if (i < messages.length - 1) message += "\1";
				Message(gender_colour_text + message);
			}
			end_message(contact);
		}

		public void end_message(contact = null) {
			Message(_INTL("Click!") + "\\wt[10]\n......\\wt[5] ......\1");
		}

		//-----------------------------------------------------------------------------

		public void generate_trainer_dialogue(contact) {
			validate contact => Phone.Contact;
			// Get the set of messages to be used by the contact
			messages = GameData.PhoneMessage.try_get(contact.trainer_type, contact.name, contact.version);
			if (!messages) messages = GameData.PhoneMessage.try_get(contact.trainer_type, contact.name, contact.start_version);
			if (!messages) messages = GameData.PhoneMessage.DATA["default"];
			// Create lambda for choosing a random message and translating it
			get_random_message = lambda do |msgs|
				if (!msgs) return "";
				msg = msgs.sample;
				if (!msg) return "";
				return GetMessageFromHash(MessageTypes.PHONE_MESSAGES, msg);
			}
			// Choose random greeting depending on time of day
			ret = get_random_message.call(messages.intro);
			time = GetTimeNow;
			if (DayNight.isMorning(time)) {
				mod_call = get_random_message.call(messages.intro_morning);
				if (!nil_or_empty(mod_call)) ret = mod_call;
			} else if (DayNight.isAfternoon(time)) {
				mod_call = get_random_message.call(messages.intro_afternoon);
				if (!nil_or_empty(mod_call)) ret = mod_call;
			} else if (DayNight.isEvening(time)) {
				mod_call = get_random_message.call(messages.intro_evening);
				if (!nil_or_empty(mod_call)) ret = mod_call;
			}
			ret += "\\m";
			// Choose main message set
			if (Phone.rematches_enabled && contact.rematch_flag > 0) {
				// Trainer is ready for a rematch, so tell/remind the player
				switch (contact.rematch_flag) {
					case 1:   // Tell the player
						ret += get_random_message.call(messages.battle_request);
						contact.rematch_flag = 2;   // Ready for rematch and told player
						break;
					case 2:   // Remind the player
						if (messages.battle_remind) {
							ret += get_random_message.call(messages.battle_remind);
						} else {
							ret += get_random_message.call(messages.battle_request);
						}
						break;
				}
			} else {
				// Standard messages
				if (messages.body1 && messages.body2 && (!messages.body || rand(100) < 75)) {
					// Choose random pair of body messages
					ret += get_random_message.call(messages.body1);
					ret += "\\m";
					ret += get_random_message.call(messages.body2);
				} else {
					// Choose random full body message
					ret += get_random_message.call(messages.body);
				}
				// Choose end message
				mod_call = get_random_message.call(messages.end);
				if (!nil_or_empty(mod_call)) ret += "\\m" + mod_call;
			}
			return ret;
		}

		public void get_random_contact_pokemon_species(contact) {
			if (!contact.trainer()) return "";
			version = (int)Math.Max(contact.version, contact.start_version);
			trainer_data = GameData.Trainer.try_get(contact.trainer_type, contact.name, version);
			if (!trainer_data) return "";
			pkmn = trainer_data.pokemon.sample.species;
			return GameData.Species.get(pkmn).name;
		}

		public void get_random_encounter_species(contact) {
			if (!contact.trainer()) return "";
			encounter_data = GameData.Encounter.get(contact.map_id, Game.GameData.PokemonGlobal.encounter_version);
			if (!encounter_data) return "";
			get_species_from_table = lambda do |encounter_table|
				if (!encounter_table || encounter_table.length == 0) return null;
				len = (int)Math.Min(encounter_table.length, 4);   // From first 4 slots only
				return encounter_table[rand(len)][1];
			}
			enc_tables = encounter_data.types;
			species = get_species_from_table.call(enc_tables.Land);
			if (!species) {
				species = get_species_from_table.call(enc_tables.Cave);
				if (!species) species = get_species_from_table.call(enc_tables.Water);
			}
			if (!species) return "";
			return GameData.Species.get(species).name;
		}

		public void get_map_name(contact) {
			return GetMapNameFromId(contact.map_id);
		}
	}
}

//===============================================================================
//
//===============================================================================
EventHandlers.add(:on_frame_update, :phone_call_counter,
	block: () => {
		if (!Game.GameData.player&.has_pokegear) continue;
		// Don't count down various phone times if other things are happening
		if (Game.GameData.game_temp.in_menu || Game.GameData.game_temp.in_battle || Game.GameData.game_temp.message_window_showing) continue;
		if (Game.GameData.game_player.move_route_forcing || MapInterpreterRunning()) continue;
		// Count down time to next can-battle for each trainer contact
		Game.GameData.PokemonGlobal.phone.refresh_ready_trainers;
		// Count down time to next phone call
		if (Game.GameData.PokemonGlobal.phone.time_to_next_call <= 0) {
			Game.GameData.PokemonGlobal.phone.time_to_next_call = rand(20...40) * 60.0;   // 20-40 minutes
		}
		Game.GameData.PokemonGlobal.phone.time_to_next_call -= Graphics.delta;
		if (Game.GameData.PokemonGlobal.phone.time_to_next_call > 0) continue;
		// Time for a random phone call; generate one
		Phone.Call.make_incoming;
	}
)
