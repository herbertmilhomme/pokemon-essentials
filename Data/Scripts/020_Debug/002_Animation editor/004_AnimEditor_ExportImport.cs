//===============================================================================
//
//===============================================================================
public static partial class BattleAnimationEditor {
	#region Class Functions
	#endregion

	//=============================================================================
	// Importing and exporting.
	//=============================================================================
	public void RgssChdir(dir) {
		RTP.eachPathFor(dir, path => { Dir.chdir(path) { yield; }); };
	}

	public void tryLoadData(file) {
		begin;
			return load_data(file);
		rescue;
			return null;
		}
	}

	public void dumpBase64Anim(s) {
		return [Zlib.Deflate.deflate(Marshal.dump(s))].pack("m").gsub(/\n/, "\r\n");
	}

	public void loadBase64Anim(s) {
		return Marshal.restore(Zlib.Inflate.inflate(s.unpack("m")[0]));
	}

	public void ExportAnim(animations) {
		filename = MessageFreeText(_INTL("Enter a filename."), "", false, 32);
		if (filename != "") {
			begin;
				filename += ".anm";
				File.open(filename, "wb") do |f|
					f.write(dumpBase64Anim(animations[animations.selected]));
				}
				failed = false;
			rescue;
				Message(_INTL("Couldn't save the animation to {1}.", filename));
				failed = true;
			}
			if (!failed) {
				Message(_INTL("Animation was saved to {1} in the game folder.", filename));
				Message(_INTL("It's a text file, so it can be transferred to others easily."));
			}
		}
	}

	public void ImportAnim(animations, canvas, animwin) {
		animfiles = new List<string>();
		RgssChdir(".") { animfiles.concat(Dir.glob("*.anm")) };
		cmdwin = ListWindow(animfiles, 320);
		cmdwin.opacity = 200;
		cmdwin.height = 480;
		cmdwin.viewport = canvas.viewport;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwin.update;
			if (Input.trigger(Input.USE) && animfiles.length > 0) {
				begin;
					textdata = loadBase64Anim(IO.read(animfiles[cmdwin.index]));
					if (!textdata.is_a(Animation)) throw "Bad data";
					textdata.id = -1; // this is not an RPG Maker XP animation
					ConvertAnimToNewFormat(textdata);
					animations[animations.selected] = textdata;
				rescue;
					Message(_INTL("The animation is invalid or could not be loaded."));
					continue;
				}
				graphic = animations[animations.selected].graphic;
				graphic = $"Graphics/Animations/{graphic}";
				if (graphic && graphic != "" && !FileTest.image_exist(graphic)) {
					Message(_INTL("The animation file {1} was not found. The animation will load anyway.", graphic));
				}
				canvas.loadAnimation(animations[animations.selected]);
				animwin.animbitmap = canvas.animbitmap;
				break;
			}
			if (Input.trigger(Input.BACK)) {
				break;
			}
		}
		cmdwin.dispose;
		return;
	}

	//=============================================================================
	// Format conversion.
	//=============================================================================
	public void ConvertAnimToNewFormat(textdata) {
		needconverting = false;
		for (int i = textdata.length; i < textdata.length; i++) { //for 'textdata.length' times do => |i|
			if (!textdata[i]) continue;
			for (int j = Animation.MAX_SPRITES; j < Animation.MAX_SPRITES; j++) { //for 'Animation.MAX_SPRITES' times do => |j|
				if (!textdata[i][j]) continue;
				if (textdata[i][j][AnimFrame.FOCUS].null()) needconverting = true;
				if (needconverting) break;
			}
			if (needconverting) break;
		}
		if (needconverting) {
			for (int i = textdata.length; i < textdata.length; i++) { //for 'textdata.length' times do => |i|
				if (!textdata[i]) continue;
				for (int j = Animation.MAX_SPRITES; j < Animation.MAX_SPRITES; j++) { //for 'Animation.MAX_SPRITES' times do => |j|
					if (!textdata[i][j]) continue;
					if (textdata[i][j][AnimFrame.PRIORITY].null()) textdata[i][j][AnimFrame.PRIORITY] = 1;
					switch (j) {
						case 0:      // User battler
							textdata[i][j][AnimFrame.FOCUS] = 2;
							textdata[i][j][AnimFrame.X] = Battle.Scene.FOCUSUSER_X;
							textdata[i][j][AnimFrame.Y] = Battle.Scene.FOCUSUSER_Y;
							break;
						case 1:   // Target battler
							textdata[i][j][AnimFrame.FOCUS] = 1;
							textdata[i][j][AnimFrame.X] = Battle.Scene.FOCUSTARGET_X;
							textdata[i][j][AnimFrame.Y] = Battle.Scene.FOCUSTARGET_Y;
							break;
						default:
							textdata[i][j][AnimFrame.FOCUS] = (textdata.position || 4);
							if (textdata.position == 1) {
								textdata[i][j][AnimFrame.X] += Battle.Scene.FOCUSTARGET_X;
								textdata[i][j][AnimFrame.Y] += Battle.Scene.FOCUSTARGET_Y - 2;
							}
							break;
					}
				}
			}
		}
		return needconverting;
	}

	public void ConvertAnimsToNewFormat() {
		Message(_INTL("Will convert animations now."));
		count = 0;
		animations = LoadBattleAnimations;
		if (!animations || !animations[0]) {
			Message(_INTL("No animations exist."));
			return;
		}
		for (int k = animations.length; k < animations.length; k++) { //for 'animations.length' times do => |k|
			if (!animations[k]) continue;
			ret = ConvertAnimToNewFormat(animations[k]);
			if (ret) count += 1;
		}
		if (count > 0) {
			save_data(animations, "Data/PkmnAnimations.rxdata");
			Game.GameData.game_temp.battle_animations_data = null;
		}
		Message(_INTL("{1} animations converted to new format.", count));
	}
}
