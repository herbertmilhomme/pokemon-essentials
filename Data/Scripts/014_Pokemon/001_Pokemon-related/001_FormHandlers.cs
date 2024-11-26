//===============================================================================
//
//===============================================================================
public static partial class MultipleForms {
	@@formSpecies = new SpeciesHandlerHash();

	public static void copy(sym, *syms) {
		@@formSpecies.copy(sym, *syms);
	}

	public static void register(sym, hash) {
		@@formSpecies.add(sym, hash);
	}

	public static void registerIf(sym, cond, hash) {
		@@formSpecies.addIf(sym, cond, hash);
	}

	public static bool hasFunction(pkmn, func) {
		spec = (pkmn.is_a(Pokemon)) ? pkmn.species : pkmn;
		sp = @@formSpecies[spec];
		return sp && sp[func];
	}

	public static void getFunction(pkmn, func) {
		spec = (pkmn.is_a(Pokemon)) ? pkmn.species : pkmn;
		sp = @@formSpecies[spec];
		return (sp && sp[func]) ? sp[func] : null;
	}

	public static void call(func, pkmn, *args) {
		sp = @@formSpecies[pkmn.species];
		if (!sp || !sp[func]) return null;
		return sp[func].call(pkmn, *args);
	}
}

//===============================================================================
//
//===============================================================================
public void drawSpot(bitmap, spotpattern, x, y, red, green, blue) {
	height = spotpattern.length;
	width  = spotpattern[0].length;
	for (int yy = height; yy < height; yy++) { //for 'height' times do => |yy|
		spot = spotpattern[yy];
		for (int xx = width; xx < width; xx++) { //for 'width' times do => |xx|
			if (spot[xx] != 1) continue;
			xOrg = (x + xx) * 2;
			yOrg = (y + yy) * 2;
			color = bitmap.get_pixel(xOrg, yOrg);
			r = color.red + red;
			g = color.green + green;
			b = color.blue + blue;
			color.red   = (int)Math.Min((int)Math.Max(r, 0), 255);
			color.green = (int)Math.Min((int)Math.Max(g, 0), 255);
			color.blue  = (int)Math.Min((int)Math.Max(b, 0), 255);
			bitmap.set_pixel(xOrg, yOrg, color);
			bitmap.set_pixel(xOrg + 1, yOrg, color);
			bitmap.set_pixel(xOrg, yOrg + 1, color);
			bitmap.set_pixel(xOrg + 1, yOrg + 1, color);
		}
	}
}

public void SpindaSpots(pkmn, bitmap) {
	// NOTE: These spots are doubled in size when drawing them.
	spot1 = new {
		new {0, 0, 1, 1, 1, 1, 0, 0},
		new {0, 1, 1, 1, 1, 1, 1, 0},
		new {1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1},
		new {0, 1, 1, 1, 1, 1, 1, 0},
		new {0, 0, 1, 1, 1, 1, 0, 0}
	}
	spot2 = new {
		new {0, 0, 1, 1, 1, 0, 0},
		new {0, 1, 1, 1, 1, 1, 0},
		new {1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1},
		new {0, 1, 1, 1, 1, 1, 0},
		new {0, 0, 1, 1, 1, 0, 0}
	}
	spot3 = new {
		new {0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0},
		new {0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
		new {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
		new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
		new {0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
		new {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0}
	}
	spot4 = new {
		new {0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
		new {0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
		new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
		new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
		new {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0},
		new {0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0}
	}
	id = pkmn.personalID;
	h = (id >> 28) & 15;
	g = (id >> 24) & 15;
	f = (id >> 20) & 15;
	e = (id >> 16) & 15;
	d = (id >> 12) & 15;
	c = (id >> 8) & 15;
	b = (id >> 4) & 15;
	a = (id) & 15;
	// NOTE: The coordinates below (b + 33, a + 25 and so on) are doubled when
	//       drawing the spot.
	if (pkmn.shiny()) {
		drawSpot(bitmap, spot1, b + 33, a + 25, -75, -10, -150);
		drawSpot(bitmap, spot2, d + 21, c + 24, -75, -10, -150);
		drawSpot(bitmap, spot3, f + 39, e + 7, -75, -10, -150);
		drawSpot(bitmap, spot4, h + 15, g + 6, -75, -10, -150);
	} else {
		drawSpot(bitmap, spot1, b + 33, a + 25, 0, -115, -75);
		drawSpot(bitmap, spot2, d + 21, c + 24, 0, -115, -75);
		drawSpot(bitmap, spot3, f + 39, e + 7, 0, -115, -75);
		drawSpot(bitmap, spot4, h + 15, g + 6, 0, -115, -75);
	}
}

//===============================================================================
// Regular form differences.
//===============================================================================

MultipleForms.register(:UNOWN, {
	"getFormOnCreation" => pkmn => {
		next rand(28);
	}
});

MultipleForms.register(:DUNSPARCE, {
	"getFormOnCreation" => pkmn => {
		next (rand(100) == 0) ? 1 : 0;   // 99% form 0, 1% form 1
	}
});

MultipleForms.register(:SPINDA, {
	"alterBitmap" => (pkmn, bitmap) => {
		SpindaSpots(pkmn, bitmap);
	}
});

MultipleForms.register(:CASTFORM, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		next 0;
	}
});

MultipleForms.register(:GROUDON, {
	"getPrimalForm" => pkmn => {
		if (pkmn.hasItem(Items.REDORB)) next 1;
		continue;
	}
});

MultipleForms.register(:KYOGRE, {
	"getPrimalForm" => pkmn => {
		if (pkmn.hasItem(Items.BLUEORB)) next 1;
		continue;
	}
});

MultipleForms.register(:BURMY, {
	"getFormOnCreation" => pkmn => {
		switch (GetEnvironment) {
			case :Rock: case :Sand: case :Cave:
				next 1;   // Sandy Cloak
				break;
			case :None:
				next 2;   // Trash Cloak
				break;
			default:
				next 0;   // Plant Cloak
				break;
		}
	},
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (!endBattle || !usedInBattle) continue;
		switch (battle.environment) {
			case :Rock: case :Sand: case :Cave:
				next 1;   // Sandy Cloak
				break;
			case :None:
				next 2;   // Trash Cloak
				break;
			default:
				next 0;   // Plant Cloak
				break;
		}
	}
});

MultipleForms.register(:WORMADAM, {
	"getFormOnCreation" => pkmn => {
		switch (GetEnvironment) {
			case :Rock: case :Sand: case :Cave:
				next 1;   // Sandy Cloak
				break;
			case :None:
				next 2;   // Trash Cloak
				break;
			default:
				next 0;   // Plant Cloak
				break;
		}
	}
});

MultipleForms.register(:CHERRIM, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		next 0;
	}
});

MultipleForms.register(:ROTOM, {
	"onSetForm" => (pkmn, form, oldForm) => {
		form_moves = new {
			:OVERHEAT,    // Heat (microwave oven)
			:HYDROPUMP,   // Wash (washing machine)
			:BLIZZARD,    // Frost (refrigerator)
			:AIRSLASH,    // Fan (electric fan)
			:LEAFSTORM;    // Mow (lawn mower)
		}
		// Find a known move that should be forgotten
		old_move_index = -1;
		pkmn.moves.each_with_index do |move, i|
			if (!form_moves.Contains(move.id)) continue;
			old_move_index = i;
			break;
		}
		// Determine which new move to learn (if any)
		new_move_id = (form > 0) ? form_moves[form - 1] : null;
		if (!GameData.Move.exists(new_move_id)) new_move_id = null;
		if (new_move_id.null() && old_move_index >= 0 && pkmn.numMoves == 1) {
			new_move_id = :THUNDERSHOCK;
			if (!GameData.Move.exists(new_move_id)) new_move_id = null;
			if (new_move_id.null()) raise _INTL("Rotom is trying to forget its last move, but there isn't another move to replace it with.");
		}
		if (pkmn.hasMove(new_move_id)) new_move_id = null;
		// Forget a known move (if relevant) and learn a new move (if relevant)
		if (old_move_index >= 0) {
			old_move_name = pkmn.moves[old_move_index].name;
			if (new_move_id.null()) {
				// Just forget the old move
				pkmn.forget_move_at_index(old_move_index);
				Message(_INTL("{1} forgot {2}...", pkmn.name, old_move_name));
			} else {
				// Replace the old move with the new move (keeps the same index)
				pkmn.moves[old_move_index].id = new_move_id;
				new_move_name = pkmn.moves[old_move_index].name;
				Message(_INTL("{1} forgot {2}...", pkmn.name, old_move_name) + "\1");
				Message("\\se[]" + _INTL("{1} learned {2}!", pkmn.name, new_move_name) + "\\se[Pkmn move learnt]");
			}
		} else if (!new_move_id.null()) {
			// Just learn the new move
			LearnMove(pkmn, new_move_id, true);
		}
	}
});

MultipleForms.register(:DIALGA, {
	"getForm" => pkmn => {
		next pkmn.hasItem(Items.ADAMANTCRYSTAL) ? 1 : 0;
	}
});

MultipleForms.register(:PALKIA, {
	"getForm" => pkmn => {
		next pkmn.hasItem(Items.LUSTROUSGLOBE) ? 1 : 0;
	}
});

MultipleForms.register(:GIRATINA, {
	"getForm" => pkmn => {
		if (pkmn.hasItem(Items.GRISEOUSCORE)) next 1;
		if (pkmn.hasItem(Items.GRISEOUSORB) && Settings.MECHANICS_GENERATION <= 8) next 1;
		if (Game.GameData.game_map&.metadata&.has_flag("DistortionWorld")) next 1;
		next 0;
	}
});

MultipleForms.register(:SHAYMIN, {
	"getForm" => pkmn => {
		if (pkmn.fainted() || pkmn.status == statuses.FROZEN || DayNight.isNight()) next 0;
	}
});

MultipleForms.register(:ARCEUS, {
	"getForm" => pkmn => {
		if (!pkmn.hasAbility(Abilitys.MULTITYPE)) next null;
		typeArray = {
			1  => new {:FISTPLATE,   :FIGHTINIUMZ},
			2  => new {:SKYPLATE,    :FLYINIUMZ},
			3  => new {:TOXICPLATE,  :POISONIUMZ},
			4  => new {:EARTHPLATE,  :GROUNDIUMZ},
			5  => new {:STONEPLATE,  :ROCKIUMZ},
			6  => new {:INSECTPLATE, :BUGINIUMZ},
			7  => new {:SPOOKYPLATE, :GHOSTIUMZ},
			8  => new {:IRONPLATE,   :STEELIUMZ},
			10 => new {:FLAMEPLATE,  :FIRIUMZ},
			11 => new {:SPLASHPLATE, :WATERIUMZ},
			12 => new {:MEADOWPLATE, :GRASSIUMZ},
			13 => new {:ZAPPLATE,    :ELECTRIUMZ},
			14 => new {:MINDPLATE,   :PSYCHIUMZ},
			15 => new {:ICICLEPLATE, :ICIUMZ},
			16 => new {:DRACOPLATE,  :DRAGONIUMZ},
			17 => new {:DREADPLATE,  :DARKINIUMZ},
			18 => new {:PIXIEPLATE,  :FAIRIUMZ}
		}
		ret = 0;
		typeArray.each do |f, items|
			foreach (var item in items) { //'items.each' do => |item|
				if (!pkmn.hasItem(item)) continue;
				ret = f;
				break;
			}
			if (ret > 0) break;
		}
		next ret;
	}
});

MultipleForms.register(:DARMANITAN, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		next 2 * (pkmn.form / 2);
	}
});

MultipleForms.register(:DEERLING, {
	"getForm" => pkmn => {
		next GetSeason;
	}
});

MultipleForms.copy(:DEERLING, :SAWSBUCK);

MultipleForms.register(:KYUREM, {
	"getFormOnEnteringBattle" => (pkmn, wild) => {
		if (pkmn.form == 1 || pkmn.form == 2) next pkmn.form + 2;
	},
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (pkmn.form >= 3) next pkmn.form - 2;   // Fused forms stop glowing
	},
	"onSetForm" => (pkmn, form, oldForm) => {
		switch (form) {
			case 0:   // Normal
				pkmn.moves.each_with_index do |move, i|
					switch (move.id) {
						case :ICEBURN: case :FREEZESHOCK:
							if (!GameData.Moves.exists(Moves.GLACIATE)) continue;
							if (pkmn.hasMove(Moves.GLACIATE)) {
								pkmn.moves[i] = null;
							} else {
								move.id = :GLACIATE;
							}
							break;
						case :FUSIONFLARE: case :FUSIONBOLT:
							if (!GameData.Moves.exists(Moves.SCARYFACE)) continue;
							if (pkmn.hasMove(Moves.SCARYFACE)) {
								pkmn.moves[i] = null;
							} else {
								move.id = :SCARYFACE;
							}
							break;
					}
					pkmn.moves.compact!;
				}
				break;
			case 1:   // White
				foreach (var move in pkmn.moves) { //'pkmn.moves.each' do => |move|
					switch (move.id) {
						case :GLACIATE:
							if (!GameData.Moves.exists(Moves.ICEBURN) || pkmn.hasMove(Moves.ICEBURN)) continue;
							move.id = :ICEBURN;
							break;
						case :SCARYFACE:
							if (!GameData.Moves.exists(Moves.FUSIONFLARE) || pkmn.hasMove(Moves.FUSIONFLARE)) continue;
							move.id = :FUSIONFLARE;
							break;
					}
				}
				break;
			case 2:   // Black
				foreach (var move in pkmn.moves) { //'pkmn.moves.each' do => |move|
					switch (move.id) {
						case :GLACIATE:
							if (!GameData.Moves.exists(Moves.FREEZESHOCK) || pkmn.hasMove(Moves.FREEZESHOCK)) continue;
							move.id = :FREEZESHOCK;
							break;
						case :SCARYFACE:
							if (!GameData.Moves.exists(Moves.FUSIONBOLT) || pkmn.hasMove(Moves.FUSIONBOLT)) continue;
							move.id = :FUSIONBOLT;
							break;
					}
				}
				break;
		}
	}
});

MultipleForms.register(:KELDEO, {
	"getForm" => pkmn => {
		if (pkmn.hasMove(Moves.SECRETSWORD)) next 1; // Resolute Form
		next 0;                                // Ordinary Form
	}
});

MultipleForms.register(:MELOETTA, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		next 0;
	}
});

MultipleForms.register(:GENESECT, {
	"getForm" => pkmn => {
		if (pkmn.hasItem(Items.SHOCKDRIVE)) next 1;
		if (pkmn.hasItem(Items.BURNDRIVE)) next 2;
		if (pkmn.hasItem(Items.CHILLDRIVE)) next 3;
		if (pkmn.hasItem(Items.DOUSEDRIVE)) next 4;
		next 0;
	}
});

MultipleForms.register(:GRENINJA, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (pkmn.form == 2 && (pkmn.fainted() || endBattle)) next 1;
	}
});

MultipleForms.register(:SCATTERBUG, {
	"getFormOnCreation" => pkmn => {
		next Game.GameData.player.secret_ID % 18;
	}
});

MultipleForms.copy(:SCATTERBUG, :SPEWPA, :VIVILLON);

MultipleForms.register(:FURFROU, {
	"getForm" => pkmn => {
		if (!pkmn.time_form_set ||
			GetTimeNow.ToInt() > pkmn.time_form_set.ToInt() + (60 * 60 * 24 * 5)) {   // 5 days
			next 0;
		}
	},
	"onSetForm" => (pkmn, form, oldForm) => {
		pkmn.time_form_set = (form > 0) ? GetTimeNow.ToInt() : null;
	}
});

MultipleForms.register(:ESPURR, {
	"getForm" => pkmn => {
		next pkmn.gender;
	}
});

MultipleForms.copy(:ESPURR, :MEOWSTIC);

MultipleForms.register(:AEGISLASH, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		next 0;
	}
});

MultipleForms.register(:PUMPKABOO, {
	"getFormOnCreation" => pkmn => {
		r = rand(100);
		if (r < 5) next 3;    // Super Size (5%)
		if (r < 20) next 2;   // Large (15%)
		if (r < 65) next 1;   // Average (45%)
		next 0;             // Small (35%)
	}
});

MultipleForms.copy(:PUMPKABOO, :GOURGEIST);

MultipleForms.register(:XERNEAS, {
	"getFormOnStartingBattle" => (pkmn, wild) => {
		next 1;
	},
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (endBattle) next 0;
	}
});

MultipleForms.register(:ZYGARDE, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (pkmn.form >= 2 && (pkmn.fainted() || endBattle)) next pkmn.form - 2;
	}
});

MultipleForms.register(:HOOPA, {
	"getForm" => pkmn => {
		if (!pkmn.time_form_set ||
			GetTimeNow.ToInt() > pkmn.time_form_set.ToInt() + (60 * 60 * 24 * 3)) {   // 3 days
			next 0;
		}
	},
	"onSetForm" => (pkmn, form, oldForm) => {
		pkmn.time_form_set = (form > 0) ? GetTimeNow.ToInt() : null;
	}
});

MultipleForms.register(:ROCKRUFF, {
	"getForm" => pkmn => {
		if (pkmn.form_simple >= 2) continue;   // Own Tempo Rockruff cannot become another form
		if (DayNight.isNight()) next 1;
		next 0;
	}
});

MultipleForms.register(:LYCANROC, {
	"getFormOnCreation" => pkmn => {
		if (DayNight.isEvening()) next 2;   // Dusk
		if (DayNight.isNight()) next 1;     // Midnight
		next 0;                            // Midday
	}
});

MultipleForms.register(:WISHIWASHI, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		next 0;
	}
});

MultipleForms.register(:SILVALLY, {
	"getForm" => pkmn => {
		if (!pkmn.hasAbility(Abilitys.RKSSYSTEM)) next null;
		typeArray = {
			1  => [:FIGHTINGMEMORY],
			2  => [:FLYINGMEMORY],
			3  => [:POISONMEMORY],
			4  => [:GROUNDMEMORY],
			5  => [:ROCKMEMORY],
			6  => [:BUGMEMORY],
			7  => [:GHOSTMEMORY],
			8  => [:STEELMEMORY],
			10 => [:FIREMEMORY],
			11 => [:WATERMEMORY],
			12 => [:GRASSMEMORY],
			13 => [:ELECTRICMEMORY],
			14 => [:PSYCHICMEMORY],
			15 => [:ICEMEMORY],
			16 => [:DRAGONMEMORY],
			17 => [:DARKMEMORY],
			18 => [:FAIRYMEMORY];
		}
		ret = 0;
		typeArray.each do |f, items|
			foreach (var item in items) { //'items.each' do => |item|
				if (!pkmn.hasItem(item)) continue;
				ret = f;
				break;
			}
			if (ret > 0) break;
		}
		next ret;
	}
});

MultipleForms.register(:MINIOR, {
	"getFormOnCreation" => pkmn => {
		next rand(7..13);   // Meteor forms are 0-6, Core forms are 7-13
	},
	"getFormOnEnteringBattle" => (pkmn, wild) => {
		if (pkmn.form >= 7 && wild) next pkmn.form - 7;   // Wild Minior always appear in Meteor form
	},
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (pkmn.form < 7) next pkmn.form + 7;
	}
});

MultipleForms.register(:MIMIKYU, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (pkmn.fainted() || endBattle) next 0;
	}
});

MultipleForms.register(:NECROZMA, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		// Fused forms are 1 and 2, Ultra form is 3 or 4 depending on which fusion
		if (pkmn.form >= 3 && (pkmn.fainted() || endBattle)) next pkmn.form - 2;
	},
	"onSetForm" => (pkmn, form, oldForm) => {
		if (form > 2 || oldForm > 2) continue;   // Ultra form changes don't affect moveset
		form_moves = new {
			:SUNSTEELSTRIKE,   // Dusk Mane (with Solgaleo) (form 1)
			:MOONGEISTBEAM;     // Dawn Wings (with Lunala) (form 2)
		}
		if (form == 0) {   // Normal
			// Turned back into the base form; forget form-specific moves
			foreach (var move in form_moves) { //'form_moves.each' do => |move|
				if (!pkmn.hasMove(move)) continue;
				pkmn.forget_move(move);
				Message(_INTL("{1} forgot {2}...", pkmn.name, GameData.Move.get(move).name));
			}
			if (pkmn.numMoves == 0) LearnMove(pkmn, :CONFUSION);
		} else {   // Dusk Mane, Dawn Wings
			// Turned into an alternate form; try learning that form's unique move
			new_move_id = form_moves[form - 1];
			LearnMove(pkmn, new_move_id, true);
		}
	}
});

MultipleForms.register(:CRAMORANT, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		next 0;
	}
});

MultipleForms.register(:TOXEL, {
	"getFormOnCreation" => pkmn => {
		if ((new []{:LONELY, :BOLD, :RELAXED, :TIMID, :SERIOUS, :MODEST, :MILD,
							:QUIET, :BASHFUL, :CALM, :GENTLE, :CAREFUL}.Contains(pkmn.nature_id)) next 1;
		next 0;
	}
});

MultipleForms.copy(:TOXEL, :TOXTRICITY);

MultipleForms.register(:POLTEAGEIST, {
	"getFormOnCreation" => pkmn => {
		if (rand(100) < 10) next 1;   // Antique
		next 0;                     // Phony
	}
});

MultipleForms.copy(:POLTEAGEIST, :SINISTEA);

// A Milcery will always have the same flavor, but it is randomly chosen.
MultipleForms.register(:MILCERY, {
	"getForm" => pkmn => {
		num_flavors = 9;
		sweets = new {:STRAWBERRYSWEET, :BERRYSWEET, :LOVESWEET, :STARSWEET,
							:CLOVERSWEET, :FLOWERSWEET, :RIBBONSWEET};
		if (sweets.Contains(pkmn.item_id)) {
			next sweets.index(pkmn.item_id) + ((pkmn.personalID % num_flavors) * sweets.length);
		}
		next 0;
	}
});

MultipleForms.register(:ALCREMIE, {
	"getFormOnCreation" => pkmn => {
		num_flavors = 9;
		num_sweets = 7;
		next rand(num_flavors * num_sweets);
	}
});

MultipleForms.register(:EISCUE, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (pkmn.fainted() || endBattle) next 0;
	}
});

MultipleForms.register(:INDEEDEE, {
	"getForm" => pkmn => {
		next pkmn.gender;
	}
});

MultipleForms.register(:MORPEKO, {
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (pkmn.fainted() || endBattle) next 0;
	}
});

MultipleForms.register(:ZACIAN, {
	"getFormOnStartingBattle" => (pkmn, wild) => {
		if (pkmn.hasItem(Items.RUSTEDSWORD)) next 1;
		next 0;
	},
	"changePokemonOnStartingBattle" => (pkmn, battle) => {
		if (GameData.Moves.exists(Moves.BEHEMOTHBLADE) && pkmn.hasItem(Items.RUSTEDSWORD)) {
			pkmn.moves.each(move => { if (move.id == moves.IRONHEAD) move.id = :BEHEMOTHBLADE; });
		}
	},
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (endBattle) next 0;
	},
	"changePokemonOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (endBattle) {
			pkmn.moves.each(move => { if (move.id == moves.BEHEMOTHBLADE) move.id = :IRONHEAD; });
		}
	}
});

MultipleForms.register(:ZAMAZENTA, {
	"getFormOnStartingBattle" => (pkmn, wild) => {
		if (pkmn.hasItem(Items.RUSTEDSHIELD)) next 1;
		next 0;
	},
	"changePokemonOnStartingBattle" => (pkmn, battle) => {
		if (GameData.Moves.exists(Moves.BEHEMOTHBASH) && pkmn.hasItem(Items.RUSTEDSHIELD)) {
			pkmn.moves.each(move => { if (move.id == moves.IRONHEAD) move.id = :BEHEMOTHBASH; });
		}
	},
	"getFormOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (endBattle) next 0;
	},
	"changePokemonOnLeavingBattle" => (pkmn, battle, usedInBattle, endBattle) => {
		if (endBattle) {
			pkmn.moves.each(move => { if (move.id == moves.BEHEMOTHBASH) move.id = :IRONHEAD; });
		}
	}
});

MultipleForms.register(:URSHIFU, {
	"getFormOnCreation" => pkmn => {
		next rand(2);
	}
});

MultipleForms.register(:CALYREX, {
	"onSetForm" => (pkmn, form, oldForm) => {
		form_moves = new {
			:GLACIALLANCE,   // Ice Rider (with Glastrier) (form 1)
			:ASTRALBARRAGE;   // Shadow Rider (with Spectrier) (form 2)
		}
		if (form == 0) {   // Normal
			// Forget special form moves
			foreach (var move in form_moves) { //'form_moves.each' do => |move|
				if (!pkmn.hasMove(move)) continue;
				pkmn.forget_move(move);
				Message(_INTL("{1} forgot {2}...", pkmn.name, GameData.Move.get(move).name));
			}
			// Forget all other moves not accessible to the base form
			sp_data = pkmn.species_data;
			pkmn.moves.each_with_index do |move, i|
				if (sp_data.moves.any(learn_move => learn_move[1] == move.id)) continue;
				if (sp_data.tutor_moves.Contains(move.id)) continue;
				if (sp_data.egg_moves.Contains(move.id)) continue;
				Message(_INTL("{1} forgot {2}...", pkmn.name, move.name));
				pkmn.moves[i] = null;
			}
			pkmn.moves.compact!;
			// Ensure pkmn has at least one move in the end
			if (pkmn.numMoves == 0) LearnMove(pkmn, :CONFUSION);
		} else {   // Ice Rider, Shadow Rider
			new_move = form_moves[form - 1];
			LearnMove(pkmn, new_move, true);
		}
	}
});

MultipleForms.register(:BASCULEGION, {
	"getForm" => pkmn => {
		next (pkmn.female()) ? 3 : 2;
	}
});

MultipleForms.register(:LECHONK, {
	"getForm" => pkmn => {
		next pkmn.gender;
	}
});

MultipleForms.copy(:LECHONK, :OINKOLOGNE);

MultipleForms.register(:TANDEMAUS, {
	"getFormOnCreation" => pkmn => {
		next (rand(100) == 0) ? 1 : 0;   // 99% form 0, 1% form 1
	}
});

MultipleForms.copy(:TANDEMAUS, :MAUSHOLD);

MultipleForms.register(:SQUAWKABILLY, {
	"getFormOnCreation" => pkmn => {
		next rand(4);
	}
});

MultipleForms.register(:TATSUGIRI, {
	"getFormOnCreation" => pkmn => {
		next rand(3);
	}
});

// NOTE: Wild Dudunsparce is always form 0.
// NOTE: Wild Gimmighoul is always form 0.

MultipleForms.register(:POLTCHAGEIST, {
	"getFormOnCreation" => pkmn => {
		if (rand(100) < 10) next 1;   // Artisan
		next 0;                     // Counterfeit
	}
});

MultipleForms.copy(:POLTCHAGEIST, :SINISCHA);

MultipleForms.register(:OGERPON, {
	"getForm" => pkmn => {
		if (pkmn.hasItem(Items.WELLSPRINGMASK)) next 1;
		if (pkmn.hasItem(Items.HEARTHFLAMEMASK)) next 2;
		if (pkmn.hasItem(Items.CORNERSTONEMASK)) next 3;
		next 0;
	}
});

//===============================================================================
// Regional forms.
// This code is for determining the form of a Pokémon in an egg created at the
// Day Care, where that Pokémon's species has regional forms. The regional form
// chosen depends on the region in which the egg was produced (not where it
// hatches). The form should have a flag called "EggInRegion_2" where the number
// is the number of the region in which the egg was produced.
//===============================================================================

MultipleForms.register(:RATTATA, {
	"getFormOnEggCreation" => pkmn => {
		if (Game.GameData.game_map) {
			map_pos = Game.GameData.game_map.metadata&.town_map_position;
			region_num = map_pos[0];
			found_form = -1;
			foreach (var sp_data in GameData.Species.each_form_for_species(pkmn.species)) { //GameData.Species.each_form_for_species(pkmn.species) do => |sp_data|
				foreach (var flag in sp_data.flags) { //'sp_data.flags.each' do => |flag|
					if (System.Text.RegularExpressions.Regex.IsMatch(flag,@"^EggInRegion_(\d+)$",RegexOptions.IgnoreCase) && $~[1].ToInt() == region_num) {
						found_form = sp_data.form;
						break;
					}
				}
				if (found_form >= 0) break;
			}
			if (found_form >= 0) next found_form;
		}
		next 0;
	}
});

MultipleForms.copy(
	// Alolan forms
	:RATTATA, :SANDSHREW, :VULPIX, :DIGLETT, :MEOWTH, :GEODUDE, :GRIMER,
	// Galarian forms (excluding Meowth which is above)
	:PONYTA, :SLOWPOKE, :FARFETCHD, :ARTICUNO, :ZAPDOS, :MOLTRES, :CORSOLA,
	:ZIGZAGOON, :DARUMAKA, :YAMASK, :STUNFISK,
	// Hisuian forms
	:GROWLITHE, :VOLTORB, :QWILFISH, :SNEASEL, :ZORUA,
	// Paldean forms
	:TAUROS, :WOOPER
)

//===============================================================================
// Regional forms.
// These species don't have visually different regional forms, but they need to
// evolve into different forms depending on the location where they evolve.
//===============================================================================

// Alolan forms.
MultipleForms.register(:PIKACHU, {
	"getForm" => pkmn => {
		if (pkmn.form_simple >= 2) continue;
		if (Game.GameData.game_map) {
			map_pos = Game.GameData.game_map.metadata&.town_map_position;
			if (map_pos && map_pos[0] == 1) next 1;   // Tiall region
		}
		next 0;
	}
});

MultipleForms.copy(:PIKACHU, :EXEGGCUTE, :CUBONE);

// Galarian forms.
MultipleForms.register(:KOFFING, {
	"getForm" => pkmn => {
		if (pkmn.form_simple >= 2) continue;
		if (Game.GameData.game_map) {
			map_pos = Game.GameData.game_map.metadata&.town_map_position;
			if (map_pos && map_pos[0] == 2) next 1;   // Galar region
		}
		next 0;
	}
});

MultipleForms.copy(:KOFFING, :MIMEJR);

// Hisuian forms.
MultipleForms.register(:QUILAVA, {
	"getForm" => pkmn => {
		if (pkmn.form_simple >= 2) continue;
		if (Game.GameData.game_map) {
			map_pos = Game.GameData.game_map.metadata&.town_map_position;
			if (map_pos && map_pos[0] == 3) next 1;   // Hisui region
		}
		next 0;
	}
});

MultipleForms.copy(:QUILAVA,
									:DEWOTT, :PETILILL, :RUFFLET, :GOOMY, :BERGMITE, :DARTRIX);

// Paldean forms
// None!
