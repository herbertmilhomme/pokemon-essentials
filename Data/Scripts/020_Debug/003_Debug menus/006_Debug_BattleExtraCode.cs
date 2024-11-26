//===============================================================================
// Effect values that can be edited via the battle debug menu.
//===============================================================================
public static partial class Battle.DebugVariables {
	BATTLER_EFFECTS = {
		Effects.AquaRing       => {name: "Aqua Ring applies",                               default: false},
		Effects.Attract        => {name: "Battler that self is attracted to",               default: -1},   // Battler index
		Effects.BanefulBunker  => {name: "Baneful Bunker applies this round",               default: false},
//    Effects.BeakBlast - only applies to use of specific move, not suitable for setting via debug
		Effects.Bide           => {name: "Bide number of rounds remaining",                 default: 0},
		Effects.BideDamage     => {name: "Bide damage accumulated",                         default: 0, max: 999},
		Effects.BideTarget     => {name: "Bide last battler to hurt self",                  default: -1},   // Battler index
		Effects.BurningBulwark => {name: "Burning Bulwark applies this round",              default: false},
		Effects.BurnUp         => {name: "Burn Up has removed self's Fire type",            default: false},
		Effects.Charge         => {name: "Charge number of rounds remaining",               default: 0},
		Effects.ChoiceBand     => {name: "Move locked into by Choice items",                default: null, type: :move},
		Effects.Confusion      => {name: "Confusion number of rounds remaining",            default: 0},
//    Effects.Counter - not suitable for setting via debug
//    Effects.CounterTarget - not suitable for setting via debug
		Effects.Curse          => {name: "Curse damaging applies",                          default: false},
//    Effects.Dancer - only used while Dancer is running, not suitable for setting via debug
		Effects.DefenseCurl    => {name: "Used Defense Curl",                               default: false},
//    Effects.DestinyBond - not suitable for setting via debug
//    Effects.DestinyBondPrevious - not suitable for setting via debug
//    Effects.DestinyBondTarget - not suitable for setting via debug
		Effects.Disable        => {name: "Disable number of rounds remaining",              default: 0},
		Effects.DisableMove    => {name: "Disabled move",                                   default: null, type: :move},
		Effects.DoubleShock    => {name: "Double Shock has removed self's Electric type",   default: false},
		Effects.Electrify      => {name: "Electrify making moves Electric",                 default: false},
		Effects.Embargo        => {name: "Embargo number of rounds remaining",              default: 0},
		Effects.Encore         => {name: "Encore number of rounds remaining",               default: 0},
		Effects.EncoreMove     => {name: "Encored move",                                    default: null, type: :move},
		Effects.Endure         => {name: "Endures all lethal damage this round",            default: false},
//    Effects.FirstPledge - only applies to use of specific move, not suitable for setting via debug
		Effects.FlashFire      => {name: "Flash Fire powering up Fire moves",               default: false},
		Effects.Flinch         => {name: "Will flinch this round",                          default: false},
		Effects.FocusEnergy    => {name: "Focus Energy critical hit stages (0-4)",          default: 0, max: 4},
//    Effects.FocusPunch - only applies to use of specific move, not suitable for setting via debug
		Effects.FollowMe       => {name: "Follow Me drawing in attacks (if 1+)",            default: 0},   // Order of use, lowest takes priority
		Effects.RagePowder     => {name: "Rage Powder applies (use with Follow Me)",        default: false},
		Effects.Foresight      => {name: "Foresight applies (Ghost loses immunities)",      default: false},
		Effects.FuryCutter     => {name: "Fury Cutter power multiplier 2**x (0-4)",         default: 0, max: 4},
		Effects.GastroAcid     => {name: "Gastro Acid is negating self's ability",          default: false},
//    Effects.GemConsumed - only applies during use of move, not suitable for setting via debug
		Effects.Grudge         => {name: "Grudge will apply if self faints",                default: false},
		Effects.HealBlock      => {name: "Heal Block number of rounds remaining",           default: 0},
		Effects.HelpingHand    => {name: "Helping Hand will power up self's move",          default: false},
		Effects.HyperBeam      => {name: "Hyper Beam recharge rounds remaining",            default: 0},
//    Effects.Illusion - is a Pokémon object, too complex to be worth bothering with
		Effects.Imprison       => {name: "Imprison disables others' moves known by self",   default: false},
		Effects.Ingrain        => {name: "Ingrain applies",                                 default: false},
//    Effects.Instruct - only used while Instruct is running, not suitable for setting via debug
//    Effects.Instructed - only used while Instruct is running, not suitable for setting via debug
		Effects.JawLock        => {name: "Battler trapping self with Jaw Lock",             default: -1},   // Battler index
		Effects.KingsShield    => {name: "King's Shield applies this round",                default: false},
		Effects.LaserFocus     => {name: "Laser Focus certain critial hit duration",        default: 0},
		Effects.LeechSeed      => {name: "Battler that used Leech Seed on self",            default: -1},   // Battler index
		Effects.LockOn         => {name: "Lock-On number of rounds remaining",              default: 0},
		Effects.LockOnPos      => {name: "Battler that self is targeting with Lock-On",     default: -1},   // Battler index
//    Effects.MagicBounce - only applies during use of move, not suitable for setting via debug
//    Effects.MagicCoat - only applies to use of specific move, not suitable for setting via debug
		Effects.MagnetRise     => {name: "Magnet Rise number of rounds remaining",          default: 0},
		Effects.MeanLook       => {name: "Battler trapping self with Mean Look, etc.",      default: -1},   // Battler index
//    Effects.MeFirst - only applies to use of specific move, not suitable for setting via debug
		Effects.Metronome      => {name: "Metronome item power multiplier 1 + 0.2*x (0-5)", default: 0, max: 5},
		Effects.MicleBerry     => {name: "Micle Berry boosting next move's accuracy",       default: false},
		Effects.Minimize       => {name: "Used Minimize",                                   default: false},
		Effects.MiracleEye     => {name: "Miracle Eye applies (Dark loses immunities)",     default: false},
//    Effects.MirrorCoat - not suitable for setting via debug
//    Effects.MirrorCoatTarget - not suitable for setting via debug
//    Effects.MoveNext - not suitable for setting via debug
		Effects.MudSport       => {name: "Used Mud Sport (Gen 5 and older)",                default: false},
		Effects.Nightmare      => {name: "Taking Nightmare damage",                         default: false},
		Effects.NoRetreat      => {name: "No Retreat trapping self in battle",              default: false},
		Effects.Obstruct       => {name: "Obstruct applies this round",                     default: false},
		Effects.Octolock       => {name: "Battler trapping self with Octolock",             default: -1},   // Battler index
		Effects.Outrage        => {name: "Outrage number of rounds remaining",              default: 0},
//    Effects.ParentalBond - only applies during use of move, not suitable for setting via debug
		Effects.PerishSong     => {name: "Perish Song number of rounds remaining",          default: 0},
		Effects.PerishSongUser => {name: "Battler that used Perish Song on self",           default: -1},   // Battler index
		Effects.PickupItem     => {name: "Item retrievable by Pickup",                      default: null, type: :item},
		Effects.PickupUse      => {name: "Pickup item consumed time (higher=more recent)",  default: 0},
		Effects.Pinch          => {name: "(Battle Palace) Behavior changed at <50% HP",     default: false},
		Effects.Powder         => {name: "Powder will explode self's Fire move this round", default: false},
//    Effects.PowerTrick - doesn't actually swap the stats therefore does nothing, not suitable for setting via debug
//    Effects.Prankster - not suitable for setting via debug
//    Effects.PriorityAbility - not suitable for setting via debug
//    Effects.PriorityItem - not suitable for setting via debug
		Effects.Protect        => {name: "Protect applies this round",                      default: false},
		Effects.ProtectRate    => {name: "Protect success chance 1/x",                      default: 1, max: 999},
//    Effects.Quash - not suitable for setting via debug
//    Effects.Rage - only applies to use of specific move, not suitable for setting via debug
		Effects.Rollout        => {name: "Rollout rounds remaining (lower=stronger)",       default: 0},
		Effects.Roost          => {name: "Roost removing Flying type this round",           default: false},
//    Effects.ShellTrap - only applies to use of specific move, not suitable for setting via debug
		Effects.SilkTrap       => {name: "Silk Trap applies this round",                    default: false},
//    Effects.SkyDrop - only applies to use of specific move, not suitable for setting via debug
		Effects.SlowStart      => {name: "Slow Start rounds remaining",                     default: 0},
		Effects.SmackDown      => {name: "Smack Down is grounding self",                    default: false},
//    Effects.Snatch - only applies to use of specific move, not suitable for setting via debug
		Effects.SpikyShield    => {name: "Spiky Shield applies this round",                 default: false},
		Effects.Spotlight      => {name: "Spotlight drawing in attacks (if 1+)",            default: 0},
		Effects.Stockpile      => {name: "Stockpile count (0-3)",                           default: 0, max: 3},
		Effects.StockpileDef   => {name: "Def stages gained by Stockpile (0-12)",           default: 0, max: 12},
		Effects.StockpileSpDef => {name: "Sp. Def stages gained by Stockpile (0-12)",       default: 0, max: 12},
		Effects.Substitute     => {name: "Substitute's HP",                                 default: 0, max: 999},
		Effects.TarShot        => {name: "Tar Shot weakening self to Fire",                 default: false},
		Effects.Taunt          => {name: "Taunt number of rounds remaining",                default: 0},
		Effects.Telekinesis    => {name: "Telekinesis number of rounds remaining",          default: 0},
		Effects.ThroatChop     => {name: "Throat Chop number of rounds remaining",          default: 0},
		Effects.Torment        => {name: "Torment preventing repeating moves",              default: false},
//    Effects.Toxic - set elsewhere
//    Effects.Transform - too complex to be worth bothering with
//    Effects.TransformSpecies - too complex to be worth bothering with
		Effects.Trapping       => {name: "Trapping number of rounds remaining",             default: 0},
		Effects.TrappingMove   => {name: "Move that is trapping self",                      default: null, type: :move},
		Effects.TrappingUser   => {name: "Battler trapping self (for Binding Band)",        default: -1},   // Battler index
		Effects.Truant         => {name: "Truant will loaf around this round",              default: false},
//    Effects.TwoTurnAttack - only applies to use of specific moves, not suitable for setting via debug
//    Effects.ExtraType - set elsewhere
		Effects.Unburden       => {name: "Self lost its item (for Unburden)",               default: false},
		Effects.Uproar         => {name: "Uproar number of rounds remaining",               default: 0},
		Effects.WaterSport     => {name: "Used Water Sport (Gen 5 and older)",              default: false},
		Effects.WeightChange   => {name: "Weight change +0.1*x kg",                         default: 0, min: -99_999, max: 99_999},
		Effects.Yawn           => {name: "Yawn rounds remaining until falling asleep",      default: 0}
	}

	SIDE_EFFECTS = {
		Effects.AuroraVeil         => {name: "Aurora Veil duration",                   default: 0},
		Effects.CraftyShield       => {name: "Crafty Shield applies this round",       default: false},
		Effects.EchoedVoiceCounter => {name: "Echoed Voice rounds used (max. 5)",      default: 0, max: 5},
		Effects.EchoedVoiceUsed    => {name: "Echoed Voice used this round",           default: false},
		Effects.LastRoundFainted   => {name: "Round when side's battler last fainted", default: -2},   // Treated as -1, isn't a battler index
		Effects.LightScreen        => {name: "Light Screen duration",                  default: 0},
		Effects.LuckyChant         => {name: "Lucky Chant duration",                   default: 0},
		Effects.MatBlock           => {name: "Mat Block applies this round",           default: false},
		Effects.Mist               => {name: "Mist duration",                          default: 0},
		Effects.QuickGuard         => {name: "Quick Guard applies this round",         default: false},
		Effects.Rainbow            => {name: "Rainbow duration",                       default: 0},
		Effects.Reflect            => {name: "Reflect duration",                       default: 0},
		Effects.Round              => {name: "Round was used this round",              default: false},
		Effects.Safeguard          => {name: "Safeguard duration",                     default: 0},
		Effects.SeaOfFire          => {name: "Sea Of Fire duration",                   default: 0},
		Effects.Spikes             => {name: "Spikes layers (0-3)",                    default: 0, max: 3},
		Effects.StealthRock        => {name: "Stealth Rock exists",                    default: false},
		Effects.StickyWeb          => {name: "Sticky Web exists",                      default: false},
		Effects.Swamp              => {name: "Swamp duration",                         default: 0},
		Effects.Tailwind           => {name: "Tailwind duration",                      default: 0},
		Effects.ToxicSpikes        => {name: "Toxic Spikes layers (0-2)",              default: 0, max: 2},
		Effects.WideGuard          => {name: "Wide Guard applies this round",          default: false}
	}

	FIELD_EFFECTS = {
		Effects.AmuletCoin      => {name: "Amulet Coin doubling prize money", default: false},
		Effects.FairyLock       => {name: "Fairy Lock trapping duration",     default: 0},
		Effects.FusionBolt      => {name: "Fusion Bolt was used",             default: false},
		Effects.FusionFlare     => {name: "Fusion Flare was used",            default: false},
		Effects.Gravity         => {name: "Gravity duration",                 default: 0},
		Effects.HappyHour       => {name: "Happy Hour doubling prize money",  default: false},
		Effects.IonDeluge       => {name: "Ion Deluge making moves Electric", default: false},
		Effects.MagicRoom       => {name: "Magic Room duration",              default: 0},
		Effects.MudSportField   => {name: "Mud Sport duration (Gen 6+)",      default: 0},
		Effects.PayDay          => {name: "Pay Day additional prize money",   default: 0, max: Settings.MAX_MONEY},
		Effects.TrickRoom       => {name: "Trick Room duration",              default: 0},
		Effects.WaterSportField => {name: "Water Sport duration (Gen 6+)",    default: 0},
		Effects.WonderRoom      => {name: "Wonder Room duration",             default: 0}
	}

	POSITION_EFFECTS = {
//    Effects.FutureSightCounter - too complex to be worth bothering with
//    Effects.FutureSightMove - too complex to be worth bothering with
//    Effects.FutureSightUserIndex - too complex to be worth bothering with
//    Effects.FutureSightUserPartyIndex - too complex to be worth bothering with
		Effects.HealingWish => {name: "Whether Healing Wish is waiting to apply", default: false},
		Effects.LunarDance  => {name: "Whether Lunar Dance is waiting to apply",  default: false}
//    Effects.Wish - too complex to be worth bothering with
//    Effects.WishAmount - too complex to be worth bothering with
//    Effects.WishMaker - too complex to be worth bothering with
	}
}

//===============================================================================
// Screen for listing the above battle variables for modifying.
//===============================================================================
public partial class SpriteWindow_DebugBattleFieldEffects : Window_DrawableCommand {
	BASE_TEXT_COLOR   = new Color(96, 96, 96);
	RED_TEXT_COLOR    = new Color(168, 48, 56);
	GREEN_TEXT_COLOR  = new Color(0, 144, 0);
	TEXT_SHADOW_COLOR = new Color(208, 208, 200);

	public override void initialize(viewport, battle, variables, variables_data) {
		@battle         = battle;
		@variables      = variables;
		@variables_data = variables_data;
		base.initialize(0, 0, Graphics.width, Graphics.height, viewport);
	}

	public void itemCount() {
		return @variables_data.length;
	}

	public void shadowtext(x, y, w, h, t, align = 0, colors = 0) {
		width = self.contents.text_size(t).width;
		switch (align) {
			case 1:   // Right aligned
				x += w - width;
				break;
			case 2:   // Centre aligned
				x += (w - width) / 2;
				break;
		}
		base_color = BASE_TEXT_COLOR;
		switch (colors) {
			case 1:  base_color = RED_TEXT_COLOR; break;
			case 2:  base_color = GREEN_TEXT_COLOR; break;
		}
		DrawShadowText(self.contents, x, y, (int)Math.Max(width, w), h, t, base_color, TEXT_SHADOW_COLOR);
	}

	public void drawItem(index, _count, rect) {
		SetNarrowFont(self.contents);
		variable_data = @variables_data[@variables_data.keys[index]];
		variable = @variables[@variables_data.keys[index]];
		// Variables which aren't their default value are colored differently
		default = variable_data.default;
		if (default == -2) default = -1;
		different = (variable || default) != default;
		color = (different) ? 2 : 0;
		// Draw cursor
		rect = drawCursor(index, rect);
		// Get value's text to draw
		variable_text = variable.ToString();
		switch (variable_data.default) {
			case -1:   // Battler
				if (variable >= 0) {
					battler_name = @battle.battlers[variable].name;
					if (nil_or_empty(battler_name)) battler_name = "-";
					variable_text = string.Format("[{0}] {0}", variable, battler_name);
				} else {
					variable_text = _INTL("[None]");
				}
				break;
			case null:   // Move, item
				if (!variable) variable_text = _INTL("[None]");
				break;
		}
		// Draw text
		total_width = rect.width;
		name_width  = total_width * 80 / 100;
		value_width = total_width * 20 / 100;
		self.shadowtext(rect.x, rect.y + 8, name_width, rect.height, variable_data.name, 0, color);
		self.shadowtext(rect.x + name_width, rect.y + 8, value_width, rect.height, variable_text, 1, color);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.DebugSetEffects {
	public void initialize(battle, mode, side = 0) {
		@battle = battle;
		@mode = mode;
		@side = side;
		switch (@mode) {
			case :field:
				@variables_data = Battle.DebugVariables.FIELD_EFFECTS;
				@variables = @battle.field.effects;
				break;
			case :side:
				@variables_data = Battle.DebugVariables.SIDE_EFFECTS;
				@variables = @battle.sides[@side].effects;
				break;
			case :position:
				@variables_data = Battle.DebugVariables.POSITION_EFFECTS;
				@variables = @battle.positions[@side].effects;
				break;
			case :battler:
				@variables_data = Battle.DebugVariables.BATTLER_EFFECTS;
				@variables = @battle.battlers[@side].effects;
				break;
		}
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@window = new SpriteWindow_DebugBattleFieldEffects(@viewport, @battle, @variables, @variables_data);
		@window.active = true;
	}

	public void dispose() {
		@window.dispose;
		@viewport.dispose;
	}

	public void choose_number(default, min, max) {
		params = new ChooseNumberParams();
		params.setRange(min, max);
		params.setDefaultValue(default);
		if (min < 0) params.setNegativesAllowed(true);
		return MessageChooseNumber(_INTL("Set value ({1}-{2}).", min, max), params);
	}

	public void choose_battler(default) {
		commands = [_INTL("[None]")];
		cmds = [-1];
		cmd = 0;
		@battle.battlers.each_with_index do |battler, i|
			if (battler.null()) continue;   // Position doesn't exist
			name = battler.ToString();
			if (battler.fainted() || nil_or_empty(name)) name = "-";
			commands.Add(string.Format("[{0}] {0}", i, name));
			cmds.Add(i);
			if (default == i) cmd = cmds.length - 1;
		}
		cmd = Message("\\ts[]" + _INTL("Choose a battler/position."), commands, -1, null, cmd);
		return (cmd >= 0) ? cmds[cmd] : default;
	}

	public void update_input_for_boolean(effect, variable_data) {
		if (Input.trigger(Input.USE)) {
			PlayDecisionSE;
			@variables[effect] = !@variables[effect];
			return true;
		} else if (Input.trigger(Input.ACTION) && @variables[effect]) {
			PlayDecisionSE;
			@variables[effect] = false;
			return true;
		} else if (Input.repeat(Input.LEFT) && @variables[effect]) {
			PlayCursorSE;
			@variables[effect] = false;
			return true;
		} else if (Input.repeat(Input.RIGHT) && !@variables[effect]) {
			PlayCursorSE;
			@variables[effect] = true;
			return true;
		}
		return false;
	}

	public void update_input_for_integer(effect, default, variable_data) {
		true_default = (default == -2) ? -1 : default;
		min = variable_data.min || true_default;
		max = variable_data.max || 99;
		if (Input.trigger(Input.USE)) {
			PlayDecisionSE;
			new_value = choose_number(@variables[effect], min, max);
			if (new_value != @variables[effect]) {
				@variables[effect] = new_value;
				return true;
			}
		} else if (Input.trigger(Input.ACTION) && @variables[effect] != true_default) {
			PlayDecisionSE;
			@variables[effect] = true_default;
			return true;
		} else if (Input.repeat(Input.LEFT) && @variables[effect] > min) {
			PlayCursorSE;
			@variables[effect] -= 1;
			return true;
		} else if (Input.repeat(Input.RIGHT) && @variables[effect] < max) {
			PlayCursorSE;
			@variables[effect] += 1;
			return true;
		}
		return false;
	}

	public void update_input_for_battler_index(effect, variable_data) {
		if (Input.trigger(Input.USE)) {
			PlayDecisionSE;
			new_value = choose_battler(@variables[effect]);
			if (new_value != @variables[effect]) {
				@variables[effect] = new_value;
				return true;
			}
		} else if (Input.trigger(Input.ACTION) && @variables[effect] != -1) {
			PlayDecisionSE;
			@variables[effect] = -1;
			return true;
		} else if (Input.repeat(Input.LEFT)) {
			if (@variables[effect] > -1) {
				PlayCursorSE;
				do { //loop; while (true);
					@variables[effect] -= 1;
					if (@variables[effect] == -1 || @battle.battlers[@variables[effect]]) break;
				}
				return true;
			}
		} else if (Input.repeat(Input.RIGHT)) {
			if (@variables[effect] < @battle.battlers.length - 1) {
				PlayCursorSE;
				do { //loop; while (true);
					@variables[effect] += 1;
					if (@battle.battlers[@variables[effect]]) break;
				}
				return true;
			}
		}
		return false;
	}

	public void update_input_for_move(effect, variable_data) {
		if (Input.trigger(Input.USE)) {
			PlayDecisionSE;
			new_value = ChooseMoveList(@variables[effect]);
			if (new_value && new_value != @variables[effect]) {
				@variables[effect] = new_value;
				return true;
			}
		} else if (Input.trigger(Input.ACTION) && @variables[effect]) {
			PlayDecisionSE;
			@variables[effect] = null;
			return true;
		}
		return false;
	}

	public void update_input_for_item(effect, variable_data) {
		if (Input.trigger(Input.USE)) {
			PlayDecisionSE;
			new_value = ChooseItemList(@variables[effect]);
			if (new_value && new_value != @variables[effect]) {
				@variables[effect] = new_value;
				return true;
			}
		} else if (Input.trigger(Input.ACTION) && @variables[effect]) {
			PlayDecisionSE;
			@variables[effect] = null;
			return true;
		}
		return false;
	}

	public void update() {
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			@window.update;
			if (Input.trigger(Input.BACK)) {
				PlayCancelSE
				break;
			}
			index = @window.index;
			effect = @variables_data.keys[index];
			variable_data = @variables_data[effect];
			if (variable_data.default == false) {
				if (update_input_for_boolean(effect, variable_data)) @window.refresh;
			} else if (new []{0, 1, -2}.Contains(variable_data.default)) {
				if (update_input_for_integer(effect, variable_data.default, variable_data)) @window.refresh;
			} else if (variable_data.default == -1) {
				if (update_input_for_battler_index(effect, variable_data)) @window.refresh;
			} else if (variable_data.default.null()) {
				switch (variable_data.type) {
					case :move:
						if (update_input_for_move(effect, variable_data)) @window.refresh;
						break;
					case :item:
						if (update_input_for_item(effect, variable_data)) @window.refresh;
						break;
					default:
						Debug.LogError("Unknown kind of variable!");
						//throw new ArgumentException("Unknown kind of variable!");
						break;
				}
			} else {
				Debug.LogError("Unknown kind of variable!");
				//throw new ArgumentException("Unknown kind of variable!");
			}
		}
	}
}
