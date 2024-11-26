//===============================================================================
// Pokédex Regional Dexes list menu screen
// * For choosing which region list to view. Only appears when there is more
//   than one accessible region list to choose from, and if
//   Settings.USE_CURRENT_REGION_DEX is false.
//===============================================================================
public partial class Window_DexesList : Window_CommandPokemon {
	public override void initialize(commands, commands2, width) {
		@commands2 = commands2;
		base.initialize(commands, width);
		@selarrow = new AnimatedBitmap("Graphics/UI/sel_arrow_white");
		self.baseColor   = new Color(248, 248, 248);
		self.shadowColor = Color.black;
		self.windowskin  = null;
	}

	public override void drawItem(index, count, rect) {
		base.drawItem(index, count, rect);
		if (index >= 0 && index < @commands2.length) {
			DrawShadowText(self.contents, rect.x + 254, rect.y + (self.contents.text_offset_y || 0),
											64, rect.height, @commands2[index][0].ToString(), self.baseColor, self.shadowColor, 1);
			DrawShadowText(self.contents, rect.x + 350, rect.y + (self.contents.text_offset_y || 0),
											64, rect.height, @commands2[index][1].ToString(), self.baseColor, self.shadowColor, 1);
			allseen = (@commands2[index][0] >= @commands2[index][2]);
			allown  = (@commands2[index][1] >= @commands2[index][2]);
			DrawImagePositions(
				self.contents,
				new {new {"Graphics/UI/Pokedex/icon_menuseenown", rect.x + 236, rect.y + 6, (allseen) ? 24 : 0, 0, 24, 24},
				new {"Graphics/UI/Pokedex/icon_menuseenown", rect.x + 332, rect.y + 6, (allown) ? 24 : 0, 24, 24, 24}}
			);
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPokedexMenu_Scene {
	SEEN_OBTAINED_TEXT_BASE   = new Color(248, 248, 248);
	SEEN_OBTAINED_TEXT_SHADOW = new Color(192, 32, 40);

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(commands, commands2) {
		@commands = commands;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["background"].setBitmap(_INTL("Graphics/UI/Pokedex/bg_menu"));
		text_tag = shadowc3tag(SEEN_OBTAINED_TEXT_BASE, SEEN_OBTAINED_TEXT_SHADOW);
		@sprites["headings"] = Window_AdvancedTextPokemon.newWithSize(
			text_tag + _INTL("SEEN") + "<r>" + _INTL("OBTAINED") + "</c3>", 286, 136, 208, 64, @viewport
		);
		@sprites["headings"].windowskin = null;
		@sprites["commands"] = new Window_DexesList(commands, commands2, Graphics.width - 84);
		@sprites["commands"].x      = 40;
		@sprites["commands"].y      = 192;
		@sprites["commands"].height = 192;
		@sprites["commands"].viewport = @viewport;
		FadeInAndShow(@sprites) { Update };
	}

	public void Scene() {
		ret = -1;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				break;
			} else if (Input.trigger(Input.USE)) {
				ret = @sprites["commands"].index;
				(ret == @commands.length - 1) ? PlayCloseMenuSE : SEPlay("GUI pokedex open")
				break;
			}
		}
		return ret;
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPokedexMenuScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		commands  = new List<string>();
		commands2 = new List<string>();
		dexnames = Settings.pokedex_names;
		foreach (var dex in Game.GameData.player.pokedex.accessible_dexes) { //'Game.GameData.player.pokedex.accessible_dexes.each' do => |dex|
			if (dexnames[dex].null()) {
				commands.Add(_INTL("Pokédex"));
			} else if (dexnames[dex].Length > 0) {
				commands.Add(dexnames[dex][0]);
			} else {
				commands.Add(dexnames[dex]);
			}
			commands2.Add(new {Game.GameData.player.pokedex.seen_count(dex),
											Game.GameData.player.pokedex.owned_count(dex),
											GetRegionalDexLength(dex)});
		}
		commands.Add(_INTL("Exit"));
		@scene.StartScene(commands, commands2);
		do { //loop; while (true);
			cmd = @scene.Scene;
			if (cmd < 0 || cmd >= commands2.length) break;   // Cancel/Exit
			Game.GameData.PokemonGlobal.pokedexDex = Game.GameData.player.pokedex.accessible_dexes[cmd];
			FadeOutIn do;
				scene = new PokemonPokedex_Scene();
				screen = new PokemonPokedexScreen(scene);
				screen.StartScreen;
			}
		}
		@scene.EndScene;
	}
}
