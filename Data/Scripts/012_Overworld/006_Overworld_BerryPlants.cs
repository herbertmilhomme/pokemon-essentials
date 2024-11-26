//===============================================================================
// Represents a planted berry. Stored in Game.GameData.PokemonGlobal.eventvars.
//===============================================================================
public partial class BerryPlantData {
	/// <summary>false for Gen 3, true for Gen 4</summary>
	public int new_mechanics		{ get { return _new_mechanics; } set { _new_mechanics = value; } }			protected int _new_mechanics;
	public int berry_id		{ get { return _berry_id; } set { _berry_id = value; } }			protected int _berry_id;
	/// <summary>Gen 4 mechanics</summary>
	public int mulch_id		{ get { return _mulch_id; } set { _mulch_id = value; } }			protected int _mulch_id;
	public int time_alive		{ get { return _time_alive; } set { _time_alive = value; } }			protected int _time_alive;
	public int time_last_updated		{ get { return _time_last_updated; } set { _time_last_updated = value; } }			protected int _time_last_updated;
	public int growth_stage		{ get { return _growth_stage; } set { _growth_stage = value; } }			protected int _growth_stage;
	public int replant_count		{ get { return _replant_count; } set { _replant_count = value; } }			protected int _replant_count;
	/// <summary>Gen 3 mechanics</summary>
	public int watered_this_stage		{ get { return _watered_this_stage; } set { _watered_this_stage = value; } }			protected int _watered_this_stage;
	/// <summary>Gen 3 mechanics</summary>
	public int watering_count		{ get { return _watering_count; } set { _watering_count = value; } }			protected int _watering_count;
	/// <summary>Gen 4 mechanics</summary>
	public int moisture_level		{ get { return _moisture_level; } set { _moisture_level = value; } }			protected int _moisture_level;
	/// <summary>Gen 4 mechanics</summary>
	public int yield_penalty		{ get { return _yield_penalty; } set { _yield_penalty = value; } }			protected int _yield_penalty;

	public void initialize() {
		reset;
	}

	public void reset(planting = false) {
		@new_mechanics      = Settings.NEW_BERRY_PLANTS;
		@berry_id           = null;
		if (!planting) @mulch_id           = null;
		@time_alive         = 0;
		@time_last_updated  = 0;
		@growth_stage       = 0;
		@replant_count      = 0;
		@watered_this_stage = false;
		@watering_count     = 0;
		@moisture_level     = 100;
		@yield_penalty      = 0;
	}

	public void plant(berry_id) {
		reset(true);
		@berry_id          = berry_id;
		@growth_stage      = 1;
		@time_last_updated = GetTimeNow.ToInt();
	}

	public void replant() {
		@time_alive         = 0;
		@growth_stage       = 2;
		@replant_count      += 1;
		@watered_this_stage = false;
		@watering_count     = 0;
		@moisture_level     = 100;
		@yield_penalty      = 0;
	}

	public bool planted() {
		return @growth_stage > 0;
	}

	public bool growing() {
		return @growth_stage > 0 && @growth_stage < 5;
	}

	public bool grown() {
		return @growth_stage >= 5;
	}

	public bool replanted() {
		return @replant_count > 0;
	}

	public void moisture_stage() {
		if (!@new_mechanics) return 0;
		if (@moisture_level > 50) return 2;
		if (@moisture_level > 0) return 1;
		return 0;
	}

	public void water() {
		@moisture_level = 100;
		if (!@watered_this_stage) {
			@watered_this_stage = true;
			@watering_count += 1;
		}
	}

	public void berry_yield() {
		data = GameData.BerryPlant.get(@berry_id);
		if (@new_mechanics) {
			return (int)Math.Max(data.maximum_yield * (5 - @yield_penalty) / 5, data.minimum_yield);
		} else if (@watering_count > 0) {
			ret = (data.maximum_yield - data.minimum_yield) * (@watering_count - 1);
			ret += rand(1 + data.maximum_yield - data.minimum_yield);
			return (ret / 4) + data.minimum_yield;
		}
		return data.minimum_yield;
	}

	// Old mechanics only update a plant when its map is loaded. New mechanics
	// update it every frame while its map is loaded.
	public void update() {
		if (!planted()) return;
		time_now = GetTimeNow;
		time_delta = time_now.ToInt() - @time_last_updated;
		if (time_delta <= 0) return;
		new_time_alive = @time_alive + time_delta;
		// Get all growth data
		plant_data = GameData.BerryPlant.get(@berry_id);
		time_per_stage = plant_data.hours_per_stage * 3600;   // In seconds
		drying_per_hour = plant_data.drying_per_hour;
		max_replants = GameData.BerryPlant.NUMBER_OF_REPLANTS;
		stages_growing = GameData.BerryPlant.NUMBER_OF_GROWTH_STAGES;
		stages_fully_grown = GameData.BerryPlant.NUMBER_OF_FULLY_GROWN_STAGES;
		switch (@mulch_id) {
			case :GROWTHMULCH:
				time_per_stage = (time_per_stage * 0.75).ToInt();
				drying_per_hour = (drying_per_hour * 1.5).ceil;
				break;
			case :DAMPMULCH:
				time_per_stage = (time_per_stage * 1.25).ToInt();
				drying_per_hour /= 2;
				break;
			case :GOOEYMULCH:
				max_replants = (max_replants * 1.5).ceil;
				break;
			case :STABLEMULCH:
				stages_fully_grown = (stages_fully_grown * 1.5).ceil;
				break;
		}
		// Do replants
		done_replant = false;
		do { //loop; while (true);
			stages_this_life = stages_growing + stages_fully_grown - (replanted() ? 1 : 0);
			if (new_time_alive < stages_this_life * time_per_stage) break;
			if (@replant_count >= max_replants) {
				reset;
				return;
			}
			replant;
			done_replant = true;
			new_time_alive -= stages_this_life * time_per_stage;
		}
		// Update how long plant has been alive for
		old_growth_stage = @growth_stage;
		@time_alive = new_time_alive;
		@growth_stage = 1 + (@time_alive / time_per_stage);
		if (replanted()) @growth_stage += 1;   // Replants start at stage 2
		@time_last_updated = time_now.ToInt();
		// Record watering (old mechanics), and apply drying out per hour (new mechanics)
		if (@new_mechanics) {
			old_growth_hour = (done_replant) ? 0 : (@time_alive - time_delta) / 3600;
			new_growth_hour = @time_alive / 3600;
			if (new_growth_hour > old_growth_hour) {
				(new_growth_hour - old_growth_hour).times do
					if (@moisture_level > 0) {
						@moisture_level -= drying_per_hour;
					} else {
						@yield_penalty += 1;
					}
				}
			}
		} else {
			if (done_replant) old_growth_stage = 0;
			new_growth_stage = (int)Math.Min(@growth_stage, stages_growing + 1);
			if (new_growth_stage > old_growth_stage) @watered_this_stage = false;
			if (Game.GameData.game_screen && GameData.Weather.get(Game.GameData.game_screen.weather_type).category == :Rain) water;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class BerryPlantMoistureSprite {
	public void initialize(event, map, viewport = null) {
		@event          = event;
		@map            = map;
		@sprite         = new IconSprite(0, 0, viewport);
		@sprite.ox      = 16;
		@sprite.oy      = 24;
		@moisture_stage = -1;   // -1 = none, 0 = dry, 1 = damp, 2 = wet
		@disposed       = false;
		update_graphic;
	}

	public void dispose() {
		@sprite.dispose;
		@map      = null;
		@event    = null;
		@disposed = true;
	}

	public bool disposed() {
		return @disposed;
	}

	public void update_graphic() {
		switch (@moisture_stage) {
			case -1:  @sprite.setBitmap(""); break;
			case 0:   @sprite.setBitmap("Graphics/Characters/berrytreedry"); break;
			case 1:   @sprite.setBitmap("Graphics/Characters/berrytreedamp"); break;
			case 2:   @sprite.setBitmap("Graphics/Characters/berrytreewet"); break;
		}
	}

	public void update() {
		if (!@sprite || !@event) return;
		new_moisture = -1;
		berry_plant = @event.variable;
		if (berry_plant.is_a(BerryPlantData) && berry_plant.planted()) {
			new_moisture = berry_plant.moisture_stage;
		}
		if (new_moisture != @moisture_stage) {
			@moisture_stage = new_moisture;
			update_graphic;
		}
		@sprite.update;
		@sprite.x      = ScreenPosHelper.ScreenX(@event);
		@sprite.y      = ScreenPosHelper.ScreenY(@event);
		@sprite.zoom_x = ScreenPosHelper.ScreenZoomX(@event);
		@sprite.zoom_y = @sprite.zoom_x;
		DayNightTint(@sprite);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BerryPlantSprite {
	public void initialize(event, map, _viewport) {
		@event     = event;
		@map       = map;
		@old_stage = 0;
		@disposed  = false;
		berry_plant = event.variable;
		if (!berry_plant) return;
		@old_stage = berry_plant.growth_stage;
		@event.character_name = "";
		update_plant(berry_plant);
		set_event_graphic(berry_plant, true);   // Set the event's graphic
	}

	public void dispose() {
		@event    = null;
		@map      = null;
		@disposed = true;
	}

	public bool disposed() {
		@disposed;
	}

	public void set_event_graphic(berry_plant, full_check = false) {
		if (!berry_plant || (berry_plant.growth_stage == @old_stage && !full_check)) return;
		switch (berry_plant.growth_stage) {
			case 0:
				@event.character_name = "";
				break;
			default:
				if (berry_plant.growth_stage == 1) {
					@event.character_name = "berrytreeplanted";   // Common to all berries
					@event.turn_down;
				} else {
					filename = string.Format("berrytree_{0}", GameData.Item.get(berry_plant.berry_id).id.ToString());
					if (ResolveBitmap("Graphics/Characters/" + filename)) {
						@event.character_name = filename;
						switch (berry_plant.growth_stage) {
							case 2:  @event.turn_down; break;    // X sprouted
							case 3:  @event.turn_left; break;    // X taller
							case 4:  @event.turn_right; break;   // X flowering
							default:
								if (berry_plant.growth_stage >= 5) @event.turn_up;   // X berries
								break;
						}
					} else {
						@event.character_name = "Object ball";
					}
				}
				if (berry_plant.new_mechanics && @old_stage != berry_plant.growth_stage &&
					@old_stage > 0 && berry_plant.growth_stage <= GameData.BerryPlant.NUMBER_OF_GROWTH_STAGES + 1) {
					@event.animation_id = Settings.PLANT_SPARKLE_ANIMATION_ID;
					@event.animation_height = 1;
					@event.animation_regular_tone = true;
				}
				break;
		}
		@old_stage = berry_plant.growth_stage;
	}

	public void update_plant(berry_plant, initial = false) {
		if (berry_plant.planted() && (initial || berry_plant.new_mechanics)) berry_plant.update;
	}

	public void update() {
		berry_plant = @event.variable;
		if (!berry_plant) return;
		update_plant(berry_plant);
		set_event_graphic(berry_plant);
	}
}

//===============================================================================
//
//===============================================================================
EventHandlers.add(:on_new_spriteset_map, :add_berry_plant_graphics,
	block: (spriteset, viewport) => {
		map = spriteset.map;
		foreach (var event in map.events) { //'map.events.each' do => |event|
			if (!System.Text.RegularExpressions.Regex.IsMatch(event[1].name,@"berryplant",RegexOptions.IgnoreCase)) continue;
			spriteset.addUserSprite(new BerryPlantMoistureSprite(event[1], map, viewport));
			spriteset.addUserSprite(new BerryPlantSprite(event[1], map, viewport));
		}
	}
)

//===============================================================================
//
//===============================================================================
public void BerryPlant() {
	interp = MapInterpreter;
	this_event = interp.get_self;
	berry_plant = interp.getVariable;
	if (!berry_plant) {
		berry_plant = new BerryPlantData();
		interp.setVariable(berry_plant);
	}
	berry = berry_plant.berry_id;
	// Interact with the event based on its growth
	if (berry_plant.grown()) {
		this_event.turn_up;   // Stop the event turning towards the player
		if (PickBerry(berry, berry_plant.berry_yield)) berry_plant.reset;
		return;
	} else if (berry_plant.growing()) {
		berry_name = GameData.Item.get(berry).name;
		switch (berry_plant.growth_stage) {
			case 1:   // X planted
				this_event.turn_down;   // Stop the event turning towards the player
				if (berry_name.starts_with_vowel()) {
					Message(_INTL("An {1} was planted here.", berry_name));
				} else {
					Message(_INTL("A {1} was planted here.", berry_name));
				}
				break;
			case 2:   // X sprouted
				this_event.turn_down;   // Stop the event turning towards the player
				Message(_INTL("The {1} has sprouted.", berry_name));
				break;
			case 3:   // X taller
				this_event.turn_left;   // Stop the event turning towards the player
				Message(_INTL("The {1} plant is growing bigger.", berry_name));
				break;
			default:     // X flowering
				this_event.turn_right;   // Stop the event turning towards the player
				if (Settings.NEW_BERRY_PLANTS) {
					Message(_INTL("This {1} plant is in bloom!", berry_name));
				} else {
					switch (berry_plant.watering_count) {
						case 4:
							Message(_INTL("This {1} plant is in fabulous bloom!", berry_name));
							break;
						case 3:
							Message(_INTL("This {1} plant is blooming very beautifully!", berry_name));
							break;
						case 2:
							Message(_INTL("This {1} plant is blooming prettily!", berry_name));
							break;
						case 1:
							Message(_INTL("This {1} plant is blooming cutely!", berry_name));
							break;
						default:
							Message(_INTL("This {1} plant is in bloom!", berry_name));
							break;
					}
				}
		}
		// Water the growing plant
		foreach (var item in GameData.BerryPlant.WATERING_CANS) { //'GameData.BerryPlant.WATERING_CANS.each' do => |item|
			if (!Game.GameData.bag.has(item)) continue;
			if ((!ConfirmMessage(_INTL("Want to sprinkle some water with the {1}?",
																			GameData.Item.get(item).name))) break;
			berry_plant.water;
			Message("\\se[Water berry plant]" + _INTL("{1} watered the plant.", Game.GameData.player.name) + "\\wtnp[40]");
			if (Settings.NEW_BERRY_PLANTS) {
				Message(_INTL("There! All happy!"));
			} else {
				Message(_INTL("The plant seemed to be delighted."));
			}
			break;
		}
		return;
	}
	// Nothing planted yet
	ask_to_plant = true;
	if (Settings.NEW_BERRY_PLANTS) {
		// New mechanics
		if (berry_plant.mulch_id) {
			Message(_INTL("{1} has been laid down.", GameData.Item.get(berry_plant.mulch_id).name));
		} else {
			switch (Message(_INTL("It's soft, earthy soil."),) {
										new {_INTL("Fertilize"), _INTL("Plant Berry"), _INTL("Exit")}, -1);
			break;
			case 0:   // Fertilize
				mulch = null;
				FadeOutIn do;
					scene = new PokemonBag_Scene();
					screen = new PokemonBagScreen(scene, Game.GameData.bag);
					mulch = screen.ChooseItemScreen(block: (item) => { GameData.Item.get(item).is_mulch() });
				}
				if (!mulch) return;
				mulch_data = GameData.Item.get(mulch);
				if (mulch_data.is_mulch()) {
					berry_plant.mulch_id = mulch;
					Game.GameData.bag.remove(mulch);
					Message(_INTL("The {1} was scattered on the soil.", mulch_data.name));
				} else {
					Message(_INTL("That won't fertilize the soil!"));
					return;
				}
			break;
			case 1:   // Plant Berry
				ask_to_plant = false;
			} else {   // Exit/cancel
				return;
			}
		}
	} else {
		// Old mechanics
		if (!ConfirmMessage(_INTL("It's soft, loamy soil. Want to plant a berry?"))) return;
		ask_to_plant = false;
	}
	if (!ask_to_plant || ConfirmMessage(_INTL("Want to plant a Berry?"))) {
		FadeOutIn do;
			scene = new PokemonBag_Scene();
			screen = new PokemonBagScreen(scene, Game.GameData.bag);
			berry = screen.ChooseItemScreen(block: (item) => { GameData.Item.get(item).is_berry() });
		}
		if (berry) {
			Game.GameData.stats.berries_planted += 1;
			berry_plant.plant(berry);
			Game.GameData.bag.remove(berry);
			if (Settings.NEW_BERRY_PLANTS) {
				Message(_INTL("The {1} was planted in the soft, earthy soil.",
												GameData.Item.get(berry).name));
			} else if (GameData.Item.get(berry).name.starts_with_vowel()) {
				Message(_INTL("{1} planted an {2} in the soft loamy soil.",
												Game.GameData.player.name, GameData.Item.get(berry).name));
			} else {
				Message(_INTL("{1} planted a {2} in the soft loamy soil.",
												Game.GameData.player.name, GameData.Item.get(berry).name));
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public void PickBerry(berry, qty = 1) {
	berry = GameData.Item.get(berry);
	berry_name = (qty > 1) ? berry.portion_name_plural : berry.portion_name;
	if (qty > 1) {
		message = _INTL("There are {1} \\c[1]{2}\\c[0]!\nWant to pick them?", qty, berry_name);
	} else {
		message = _INTL("There is 1 \\c[1]{1}\\c[0]!\nWant to pick it?", berry_name);
	}
	if (!ConfirmMessage(message)) return false;
	if (!Game.GameData.bag.can_add(berry, qty)) {
		Message(_INTL("Too bad...\nThe Bag is full..."));
		return false;
	}
	Game.GameData.stats.berry_plants_picked += 1;
	if (qty >= GameData.BerryPlant.get(berry.id).maximum_yield) {
		Game.GameData.stats.max_yield_berry_plants += 1;
	}
	Game.GameData.bag.add(berry, qty);
	if (qty > 1) {
		Message("\\me[Berry get]" + _INTL("You picked the {1} \\c[1]{2}\\c[0].", qty, berry_name) + "\\wtnp[30]");
	} else {
		Message("\\me[Berry get]" + _INTL("You picked the \\c[1]{1}\\c[0].", berry_name) + "\\wtnp[30]");
	}
	pocket = berry.pocket;
	Message(_INTL("You put the {1} in\nyour Bag's <icon=bagPocket{2}>\\c[1]{3}\\c[0] pocket.",
									berry_name, pocket, PokemonBag.pocket_names[pocket - 1]) + "\1");
	if (Settings.NEW_BERRY_PLANTS) {
		Message(_INTL("The soil returned to its soft and earthy state."));
	} else {
		Message(_INTL("The soil returned to its soft and loamy state."));
	}
	this_event = MapInterpreter.get_self;
	SetSelfSwitch(this_event.id, "A", true);
	return true;
}
