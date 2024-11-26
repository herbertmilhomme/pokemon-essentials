//===============================================================================
//
//===============================================================================
public void findBottom(bitmap) {
	if (!bitmap) return 0;
	(1..bitmap.height).each do |i|
		for (int j = bitmap.width; j < bitmap.width; j++) { //for 'bitmap.width' times do => |j|
			if (bitmap.get_pixel(j, bitmap.height - i).alpha > 0) return bitmap.height - i;
		}
	}
	return 0;
}

public void AutoPositionAll() {
	t = System.uptime;
	foreach (var sp in GameData.Species) { //'GameData.Species.each' do => |sp|
		if (System.uptime - t >= 5) {
			t += 5;
			Graphics.update;
		}
		metrics = GameData.SpeciesMetrics.get_species_form(sp.species, sp.form);
		bitmap1 = GameData.Species.sprite_bitmap(sp.species, sp.form, null, null, null, true);
		bitmap2 = GameData.Species.sprite_bitmap(sp.species, sp.form);
		if (bitmap1&.bitmap) {   // Player's y
			metrics.back_sprite[0] = 0;
			metrics.back_sprite[1] = (bitmap1.height - (findBottom(bitmap1.bitmap) + 1)) / 2;
		}
		if (bitmap2&.bitmap) {   // Foe's y
			metrics.front_sprite[0] = 0;
			metrics.front_sprite[1] = (bitmap2.height - (findBottom(bitmap2.bitmap) + 1)) / 2;
			metrics.front_sprite[1] += 4;   // Just because
		}
		metrics.front_sprite_altitude = 0;   // Shouldn't be used
		metrics.shadow_x              = 0;
		metrics.shadow_size           = 2;
		bitmap1&.dispose;
		bitmap2&.dispose;
	}
	GameData.SpeciesMetrics.save;
	Compiler.write_pokemon_metrics;
}

//===============================================================================
//
//===============================================================================
public partial class SpritePositioner {
	public void Open() {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		battlebg   = "Graphics/Battlebacks/indoor1_bg";
		playerbase = "Graphics/Battlebacks/indoor1_base0";
		enemybase  = "Graphics/Battlebacks/indoor1_base1";
		@sprites["battle_bg"] = new AnimatedPlane(@viewport);
		@sprites["battle_bg"].setBitmap(battlebg);
		@sprites["battle_bg"].z = 0;
		baseX, baseY = Battle.Scene.BattlerPosition(0);
		@sprites["base_0"] = new IconSprite(baseX, baseY, @viewport);
		@sprites["base_0"].setBitmap(playerbase);
		@sprites["base_0"].x -= @sprites["base_0"].bitmap.width / 2 if @sprites["base_0"].bitmap;
		if (@sprites["base_0"].bitmap) @sprites["base_0"].y -= @sprites["base_0"].bitmap.height;
		@sprites["base_0"].z = 1;
		baseX, baseY = Battle.Scene.BattlerPosition(1);
		@sprites["base_1"] = new IconSprite(baseX, baseY, @viewport);
		@sprites["base_1"].setBitmap(enemybase);
		@sprites["base_1"].x -= @sprites["base_1"].bitmap.width / 2 if @sprites["base_1"].bitmap;
		@sprites["base_1"].y -= @sprites["base_1"].bitmap.height / 2 if @sprites["base_1"].bitmap;
		@sprites["base_1"].z = 1;
		@sprites["messageBox"] = new IconSprite(0, Graphics.height - 96, @viewport);
		@sprites["messageBox"].setBitmap("Graphics/UI/Debug/battle_message");
		@sprites["messageBox"].z = 2;
		@sprites["shadow_1"] = new IconSprite(0, 0, @viewport);
		@sprites["shadow_1"].z = 3;
		@sprites["pokemon_0"] = new PokemonSprite(@viewport);
		@sprites["pokemon_0"].setOffset(PictureOrigin.BOTTOM);
		@sprites["pokemon_0"].z = 4;
		@sprites["pokemon_1"] = new PokemonSprite(@viewport);
		@sprites["pokemon_1"].setOffset(PictureOrigin.BOTTOM);
		@sprites["pokemon_1"].z = 4;
		@sprites["info"] = new Window_UnformattedTextPokemon("");
		@sprites["info"].viewport = @viewport;
		@sprites["info"].visible  = false;
		@oldSpeciesIndex = 0;
		@species = null;   // This cannot be a species_form
		@form = 0;
		@metricsChanged = false;
		refresh;
		@starting = true;
	}

	public void Close() {
		if (@metricsChanged && ConfirmMessage(_INTL("Some metrics have been edited. Save changes?"))) {
			SaveMetrics;
			@metricsChanged = false;
		} else {
			GameData.SpeciesMetrics.load;   // Clear all changes to metrics
		}
		FadeOutAndHide(@sprites) { update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void SaveMetrics() {
		GameData.SpeciesMetrics.save;
		Compiler.write_pokemon_metrics;
	}

	public void update() {
		UpdateSpriteHash(@sprites);
	}

	public void refresh() {
		if (!@species) {
			@sprites["pokemon_0"].visible = false;
			@sprites["pokemon_1"].visible = false;
			@sprites["shadow_1"].visible = false;
			return;
		}
		metrics_data = GameData.SpeciesMetrics.get_species_form(@species, @form);
		for (int i = 2; i < 2; i++) { //for '2' times do => |i|
			pos = Battle.Scene.BattlerPosition(i, 1);
			@sprites[$"pokemon_{i}"].x = pos[0];
			@sprites[$"pokemon_{i}"].y = pos[1];
			metrics_data.apply_metrics_to_sprite(@sprites[$"pokemon_{i}"], i);
			@sprites[$"pokemon_{i}"].visible = true;
			if (i != 1) continue;
			@sprites["shadow_1"].x = pos[0];
			@sprites["shadow_1"].y = pos[1];
			if (@sprites["shadow_1"].bitmap) {
				@sprites["shadow_1"].x -= @sprites["shadow_1"].bitmap.width / 2;
				@sprites["shadow_1"].y -= @sprites["shadow_1"].bitmap.height / 2;
			}
			metrics_data.apply_metrics_to_sprite(@sprites["shadow_1"], i, true);
			@sprites["shadow_1"].visible = true;
		}
	}

	public void AutoPosition() {
		metrics_data = GameData.SpeciesMetrics.get_species_form(@species, @form);
		old_back_y         = metrics_data.back_sprite[1];
		old_front_y        = metrics_data.front_sprite[1];
		old_front_altitude = metrics_data.front_sprite_altitude;
		bitmap1 = @sprites["pokemon_0"].bitmap;
		bitmap2 = @sprites["pokemon_1"].bitmap;
		new_back_y  = (bitmap1.height - (findBottom(bitmap1) + 1)) / 2;
		new_front_y = (bitmap2.height - (findBottom(bitmap2) + 1)) / 2;
		new_front_y += 4;   // Just because
		if (new_back_y != old_back_y || new_front_y != old_front_y || old_front_altitude != 0) {
			metrics_data.back_sprite[1]        = new_back_y;
			metrics_data.front_sprite[1]       = new_front_y;
			metrics_data.front_sprite_altitude = 0;
			@metricsChanged = true;
			refresh;
		}
	}

	public void ChangeSpecies(species, form) {
		@species = species;
		@form = form;
		species_data = GameData.Species.get_species_form(@species, @form);
		if (!species_data) return;
		@sprites["pokemon_0"].setSpeciesBitmap(@species, 0, @form, false, false, true);
		@sprites["pokemon_1"].setSpeciesBitmap(@species, 0, @form);
		@sprites["shadow_1"].setBitmap(GameData.Species.shadow_filename(@species, @form));
	}

	public void ShadowSize() {
		ChangeSpecies(@species, @form);
		refresh;
		metrics_data = GameData.SpeciesMetrics.get_species_form(@species, @form);
		if (ResolveBitmap(string.Format("Graphics/Pokemon/Shadow/{0}_{0}", metrics_data.species, metrics_data.form)) ||
			ResolveBitmap(string.Format("Graphics/Pokemon/Shadow/{0}", metrics_data.species))) {
			Message("This species has its own shadow sprite in Graphics/Pokemon/Shadow/. The shadow size metric cannot be edited.");
			return false;
		}
		oldval = metrics_data.shadow_size;
		cmdvals = [0];
		commands = [_INTL("None")];
		defindex = 0;
		i = 0;
		do { //loop; while (true);
			i += 1;
			fn = string.Format("Graphics/Pokemon/Shadow/{0}", i);
			if (!ResolveBitmap(fn)) break;
			cmdvals.Add(i);
			commands.Add(i.ToString());
			if (oldval == i) defindex = cmdvals.length - 1;
		}
		cw = new Window_CommandPokemon(commands);
		cw.index    = defindex;
		cw.viewport = @viewport;
		ret = false;
		oldindex = cw.index;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cw.update;
			self.update;
			if (cw.index != oldindex) {
				oldindex = cw.index;
				metrics_data.shadow_size = cmdvals[cw.index];
				ChangeSpecies(@species, @form);
				refresh;
			}
			if (Input.trigger(Input.ACTION)) {   // Cycle to next option
				PlayDecisionSE;
				if (metrics_data.shadow_size != oldval) @metricsChanged = true;
				ret = true;
				break;
			} else if (Input.trigger(Input.BACK)) {
				metrics_data.shadow_size = oldval;
				PlayCancelSE
				break;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				if (metrics_data.shadow_size != oldval) @metricsChanged = true;
				break;
			}
		}
		cw.dispose;
		return ret;
	}

	public void SetParameter(param) {
		if (!@species) return;
		if (param == 2) return ShadowSize;
		if (param == 4) {
			AutoPosition;
			return false;
		}
		metrics_data = GameData.SpeciesMetrics.get_species_form(@species, @form);
		switch (param) {
			case 0:
				sprite = @sprites["pokemon_0"];
				xpos = metrics_data.back_sprite[0];
				ypos = metrics_data.back_sprite[1];
				break;
			case 1:
				sprite = @sprites["pokemon_1"];
				xpos = metrics_data.front_sprite[0];
				ypos = metrics_data.front_sprite[1];
				break;
			case 3:
				sprite = @sprites["shadow_1"];
				xpos = metrics_data.shadow_x;
				ypos = 0;
				break;
		}
		oldxpos = xpos;
		oldypos = ypos;
		@sprites["info"].visible = true;
		ret = false;
		do { //loop; while (true);
			sprite.visible = ((System.uptime * 8).ToInt() % 4) < 3;   // Flash the selected sprite
			Graphics.update;
			Input.update;
			self.update;
			switch (param) {
				case 0:  @sprites["info"].setTextToFit($"Ally Position = {xpos},{ypos}"); break;
				case 1:  @sprites["info"].setTextToFit($"Enemy Position = {xpos},{ypos}"); break;
				case 3:  @sprites["info"].setTextToFit($"Shadow Position = {xpos}"); break;
			}
			if ((Input.repeat(Input.UP) || Input.repeat(Input.DOWN)) && param != 3) {
				ypos += (Input.repeat(Input.DOWN)) ? 1 : -1;
				switch (param) {
					case 0:  metrics_data.back_sprite[1]  = ypos; break;
					case 1:  metrics_data.front_sprite[1] = ypos; break;
				}
				refresh;
			}
			if (Input.repeat(Input.LEFT) || Input.repeat(Input.RIGHT)) {
				xpos += (Input.repeat(Input.RIGHT)) ? 1 : -1;
				switch (param) {
					case 0:  metrics_data.back_sprite[0]  = xpos; break;
					case 1:  metrics_data.front_sprite[0] = xpos; break;
					case 3:  metrics_data.shadow_x        = xpos; break;
				}
				refresh;
			}
			if (Input.repeat(Input.ACTION) && param != 3) {   // Cycle to next option
				if (xpos != oldxpos || ypos != oldypos) @metricsChanged = true;
				ret = true;
				PlayDecisionSE;
				break;
			} else if (Input.repeat(Input.BACK)) {
				switch (param) {
					case 0:
						metrics_data.back_sprite[0] = oldxpos;
						metrics_data.back_sprite[1] = oldypos;
						break;
					case 1:
						metrics_data.front_sprite[0] = oldxpos;
						metrics_data.front_sprite[1] = oldypos;
						break;
					case 3:
						metrics_data.shadow_x = oldxpos;
						break;
				}
				PlayCancelSE
				refresh;
				break;
			} else if (Input.repeat(Input.USE)) {
				if (xpos != oldxpos || (param != 3 && ypos != oldypos)) @metricsChanged = true;
				PlayDecisionSE;
				break;
			}
		}
		@sprites["info"].visible = false;
		sprite.visible = true;
		return ret;
	}

	public void Menu() {
		refresh;
		cw = new Window_CommandPokemon(
			new {_INTL("Set Ally Position"),
			_INTL("Set Enemy Position"),
			_INTL("Set Shadow Size"),
			_INTL("Set Shadow Position"),
			_INTL("Auto-Position Sprites")}
		);
		cw.x        = Graphics.width - cw.width;
		cw.y        = Graphics.height - cw.height;
		cw.viewport = @viewport;
		ret = -1;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cw.update;
			self.update;
			if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				ret = cw.index;
				break;
			} else if (Input.trigger(Input.BACK)) {
				PlayCancelSE
				break;
			}
		}
		cw.dispose;
		return ret;
	}

	public void ChooseSpecies() {
		if (@starting) {
			FadeInAndShow(@sprites) { update };
			@starting = false;
		}
		cw = Window_CommandPokemonEx.newEmpty(0, 0, 260, 176, @viewport);
		cw.rowHeight = 24;
		SetSmallFont(cw.contents);
		cw.x = Graphics.width - cw.width;
		cw.y = Graphics.height - cw.height;
		allspecies = new List<string>();
		foreach (var sp in GameData.Species) { //'GameData.Species.each' do => |sp|
			name = (sp.form == 0) ? sp.name : _INTL("{1} (form {2})", sp.real_name, sp.form);
			if (name && !name.empty()) allspecies.Add(new {sp.id, sp.species, sp.form, name});
		}
		allspecies.sort! { |a, b| a[3] <=> b[3] };
		commands = new List<string>();
		allspecies.each(sp => commands.Add(sp[3]));
		cw.commands = commands;
		cw.index    = @oldSpeciesIndex;
		ret = false;
		oldindex = -1;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cw.update;
			if (cw.index != oldindex) {
				oldindex = cw.index;
				ChangeSpecies(allspecies[cw.index][1], allspecies[cw.index][2]);
				refresh;
			}
			self.update;
			if (Input.trigger(Input.BACK)) {
				ChangeSpecies(null, null);
				refresh;
				break;
			} else if (Input.trigger(Input.USE)) {
				ChangeSpecies(allspecies[cw.index][1], allspecies[cw.index][2]);
				ret = true;
				break;
			}
		}
		@oldSpeciesIndex = cw.index;
		cw.dispose;
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpritePositionerScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void Start() {
		@scene.Open;
		do { //loop; while (true);
			species = @scene.ChooseSpecies;
			if (!species) break;
			do { //loop; while (true);
				command = @scene.Menu;
				if (command < 0) break;
				do { //loop; while (true);
					par = @scene.SetParameter(command);
					if (!par) break;
					command = (command + 1) % 3;
				}
			}
		}
		@scene.Close;
	}
}
