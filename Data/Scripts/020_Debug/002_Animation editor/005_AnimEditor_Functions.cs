//===============================================================================
//
//===============================================================================
public static partial class BattleAnimationEditor {
	#region Class Functions
	#endregion

	//=============================================================================
	// Mini battle scene.
	//=============================================================================
	public partial class MiniBattler {
		public int index		{ get { return _index; } set { _index = value; } }			protected int _index;
		public int pokemon		{ get { return _pokemon; } set { _pokemon = value; } }			protected int _pokemon;

		public void initialize(index) {self.index = index; }
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class MiniBattle {
		public int battlers		{ get { return _battlers; } set { _battlers = value; } }			protected int _battlers;

		public void initialize() {
			@battlers = new List<string>();
			4.times(i => @battlers[i] = new MiniBattler(i));
		}
	}

	//=============================================================================
	// Pop-up menus for buttons in bottom menu.
	//=============================================================================
	public void SelectAnim(canvas, animwin) {
		animfiles = new List<string>();
		RgssChdir(File.join("Graphics", "Animations")) { animfiles.concat(Dir.glob("*.png")) };
		cmdwin = ListWindow(animfiles, 320);
		cmdwin.opacity = 200;
		cmdwin.height = 512;
		bmpwin = new BitmapDisplayWindow(320, 0, 320, 448);
		ctlwin = new ControlWindow(320, 448, 320, 64);
		cmdwin.viewport = canvas.viewport;
		bmpwin.viewport = canvas.viewport;
		ctlwin.viewport = canvas.viewport;
		ctlwin.addSlider(_INTL("Hue:"), 0, 359, 0);
		do { //loop; while (true);
			bmpwin.bitmapname = cmdwin.commands[cmdwin.index];
			Graphics.update;
			Input.update;
			cmdwin.update;
			bmpwin.update;
			ctlwin.update;
			if (ctlwin.changed(0)) bmpwin.hue = ctlwin.value(0);
			if (Input.trigger(Input.USE) && animfiles.length > 0) {
				filename = cmdwin.commands[cmdwin.index];
				bitmap = new AnimatedBitmap("Graphics/Animations/" + filename, ctlwin.value(0)).deanimate;
				canvas.animation.graphic = File.basename(filename, ".*");
				canvas.animation.hue = ctlwin.value(0);
				canvas.animbitmap = bitmap;
				animwin.animbitmap = bitmap;
				break;
			}
			if (Input.trigger(Input.BACK)) {
				break;
			}
		}
		bmpwin.dispose;
		cmdwin.dispose;
		ctlwin.dispose;
		return;
	}

	public void ChangeMaximum(canvas) {
		sliderwin2 = new ControlWindow(0, 0, 320, 32 * 4);
		sliderwin2.viewport = canvas.viewport;
		sliderwin2.addSlider(_INTL("Frames:"), 1, 1000, canvas.animation.length);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		sliderwin2.opacity = 200;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton)) {
				canvas.animation.resize(sliderwin2.value(0));
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin2.dispose;
		return;
	}

	public void AnimName(animation, cmdwin) {
		window = new ControlWindow(320, 128, 320, 32 * 4);
		window.z = 99999;
		window.addControl(new TextField(_INTL("New Name:"), animation.name));
		Input.text_input = true;
		okbutton = window.addButton(_INTL("OK"));
		cancelbutton = window.addButton(_INTL("Cancel"));
		window.opacity = 224;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			window.update;
			if (window.changed(okbutton) || Input.triggerex(:RETURN)) {
				cmdwin.commands[cmdwin.index] = _INTL("{1} {2}", cmdwin.index, window.controls[0].text);
				animation.name = window.controls[0].text;
				break;
			}
			if (window.changed(cancelbutton) || Input.triggerex(:ESCAPE)) {
				break;
			}
		}
		window.dispose;
		Input.text_input = false;
		return;
	}

	public void AnimList(animations, canvas, animwin) {
		commands = new List<string>();
		for (int i = animations.length; i < animations.length; i++) { //for 'animations.length' times do => |i|
			if (!animations[i]) animations[i] = new Animation();
			commands[commands.length] = _INTL("{1} {2}", i, animations[i].name);
		}
		cmdwin = ListWindow(commands, 320);
		cmdwin.height = 416;
		cmdwin.opacity = 224;
		cmdwin.index = animations.selected;
		cmdwin.viewport = canvas.viewport;
		helpwindow = Window_UnformattedTextPokemon.newWithSize(
			_INTL("Enter: Load/rename an animation\nEsc: Cancel"),
			320, 0, 320, 128, canvas.viewport
		);
		maxsizewindow = new ControlWindow(0, 416, 320, 32 * 3);
		maxsizewindow.addSlider(_INTL("Total Animations:"), 1, 2000, animations.length);
		maxsizewindow.addButton(_INTL("Resize Animation List"));
		maxsizewindow.opacity = 224;
		maxsizewindow.viewport = canvas.viewport;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwin.update;
			maxsizewindow.update;
			helpwindow.update;
			if (maxsizewindow.changed(1)) {
				newsize = maxsizewindow.value(0);
				animations.resize(newsize);
				commands.clear;
				for (int i = animations.length; i < animations.length; i++) { //for 'animations.length' times do => |i|
					commands[commands.length] = _INTL("{1} {2}", i, animations[i].name);
				}
				cmdwin.commands = commands;
				cmdwin.index = animations.selected;
				continue;
			}
			if (Input.trigger(Input.USE) && animations.length > 0) {
				cmd2 = ShowCommands(helpwindow,
															new {_INTL("Load Animation"),
															_INTL("Rename"),
															_INTL("Delete")}, -1);
				switch (cmd2) {
					case 0:   // Load Animation
						canvas.loadAnimation(animations[cmdwin.index]);
						animwin.animbitmap = canvas.animbitmap;
						animations.selected = cmdwin.index;
						//break //set loop to "false", and break when conditional is met
						break;
					case 1:   // Rename
						AnimName(animations[cmdwin.index], cmdwin);
						cmdwin.refresh;
						break;
					case 2:   // Delete
						if (ConfirmMessage(_INTL("Are you sure you want to delete this animation?"))) {
							animations[cmdwin.index] = new Animation();
							cmdwin.commands[cmdwin.index] = _INTL("{1} {2}", cmdwin.index, animations[cmdwin.index].name);
							cmdwin.refresh;
						}
						break;
				}
			}
			if (Input.trigger(Input.BACK)) {
				break;
			}
		}
		helpwindow.dispose;
		maxsizewindow.dispose;
		cmdwin.dispose;
	}

	//=============================================================================
	// Pop-up menus for individual cels.
	//=============================================================================
	public void ChooseNum(cel) {
		ret = cel;
		sliderwin2 = new ControlWindow(0, 0, 320, 32 * 5);
		sliderwin2.z = 99999;
		sliderwin2.addLabel(_INTL("Old Number: {1}", cel));
		sliderwin2.addSlider(_INTL("New Number:"), 2, Animation.MAX_SPRITES, cel);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton)) {
				ret = sliderwin2.value(1);
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				ret = -1;
				break;
			}
		}
		sliderwin2.dispose;
		return ret;
	}

	public void SetTone(cel, previewsprite) {
		sliderwin2 = new ControlWindow(0, 0, 320, 320);
		sliderwin2.z = 99999;
		sliderwin2.addSlider(_INTL("Red Offset:"), -255, 255, cel[AnimFrame.TONERED]);
		sliderwin2.addSlider(_INTL("Green Offset:"), -255, 255, cel[AnimFrame.TONEGREEN]);
		sliderwin2.addSlider(_INTL("Blue Offset:"), -255, 255, cel[AnimFrame.TONEBLUE]);
		sliderwin2.addSlider(_INTL("Gray Tone:"), 0, 255, cel[AnimFrame.TONEGRAY]);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		do { //loop; while (true);
			previewsprite.tone.set(sliderwin2.value(0), sliderwin2.value(1),
														sliderwin2.value(2), sliderwin2.value(3));
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton)) {
				cel[AnimFrame.TONERED] = sliderwin2.value(0);
				cel[AnimFrame.TONEGREEN] = sliderwin2.value(1);
				cel[AnimFrame.TONEBLUE] = sliderwin2.value(2);
				cel[AnimFrame.TONEGRAY] = sliderwin2.value(3);
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin2.dispose;
		return;
	}

	public void SetFlash(cel, previewsprite) {
		sliderwin2 = new ControlWindow(0, 0, 320, 320);
		sliderwin2.z = 99999;
		sliderwin2.addSlider(_INTL("Red:"), 0, 255, cel[AnimFrame.COLORRED]);
		sliderwin2.addSlider(_INTL("Green:"), 0, 255, cel[AnimFrame.COLORGREEN]);
		sliderwin2.addSlider(_INTL("Blue:"), 0, 255, cel[AnimFrame.COLORBLUE]);
		sliderwin2.addSlider(_INTL("Alpha:"), 0, 255, cel[AnimFrame.COLORALPHA]);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		do { //loop; while (true);
			previewsprite.tone.set(sliderwin2.value(0), sliderwin2.value(1),
														sliderwin2.value(2), sliderwin2.value(3));
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton)) {
				cel[AnimFrame.COLORRED] = sliderwin2.value(0);
				cel[AnimFrame.COLORGREEN] = sliderwin2.value(1);
				cel[AnimFrame.COLORBLUE] = sliderwin2.value(2);
				cel[AnimFrame.COLORALPHA] = sliderwin2.value(3);
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin2.dispose;
		return;
	}

	public void CellProperties(canvas) {
		cel = canvas.currentCel.clone; // Clone cell, in case operation is canceled
		if (!cel) return;
		sliderwin2 = new ControlWindow(0, 0, 320, 32 * 16);
		previewwin = new ControlWindow(320, 0, 192, 192);
		sliderwin2.viewport = canvas.viewport;
		previewwin.viewport = canvas.viewport;
		previewsprite = new Sprite(canvas.viewport);
		previewsprite.bitmap = canvas.animbitmap;
		previewsprite.z = previewwin.z + 1;
		sliderwin2.z = previewwin.z + 2;
		set0 = sliderwin2.addSlider(_INTL("Pattern:"), -2, 1000, cel[AnimFrame.PATTERN]);
		set1 = sliderwin2.addSlider(_INTL("X:"), -64, 512 + 64, cel[AnimFrame.X]);
		set2 = sliderwin2.addSlider(_INTL("Y:"), -64, 384 + 64, cel[AnimFrame.Y]);
		set3 = sliderwin2.addSlider(_INTL("Zoom X:"), 5, 1000, cel[AnimFrame.ZOOMX]);
		set4 = sliderwin2.addSlider(_INTL("Zoom Y:"), 5, 1000, cel[AnimFrame.ZOOMY]);
		set5 = sliderwin2.addSlider(_INTL("Angle:"), 0, 359, cel[AnimFrame.ANGLE]);
		set6 = sliderwin2.addSlider(_INTL("Opacity:"), 0, 255, cel[AnimFrame.OPACITY]);
		set7 = sliderwin2.addSlider(_INTL("Blending:"), 0, 2, cel[AnimFrame.BLENDTYPE]);
		set8 = sliderwin2.addTextSlider(_INTL("Flip:"), new {_INTL("False"), _INTL("True")}, cel[AnimFrame.MIRROR]);
		prio = new {_INTL("Back"), _INTL("Front"), _INTL("Behind focus"), _INTL("Above focus")};
		set9 = sliderwin2.addTextSlider(_INTL("Priority:"), prio, cel[AnimFrame.PRIORITY] || 1);
		foc = new {_INTL("User"), _INTL("Target"), _INTL("User and target"), _INTL("Screen")};
		curfoc = new {3, 1, 0, 2, 3}[cel[AnimFrame.FOCUS] || canvas.animation.position || 4];
		set10 = sliderwin2.addTextSlider(_INTL("Focus:"), foc, curfoc);
		flashbutton = sliderwin2.addButton(_INTL("Set Blending Color"));
		tonebutton = sliderwin2.addButton(_INTL("Set Color Tone"));
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		// Set X and Y for preview sprite
		cel[AnimFrame.X] = 320 + 96;
		cel[AnimFrame.Y] = 96;
		canvas.setSpriteBitmap(previewsprite, cel);
		SpriteSetAnimFrame(previewsprite, cel, null, null);
		previewsprite.z = previewwin.z + 1;
		sliderwin2.opacity = 200;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(set0) ||
				sliderwin2.changed(set3) ||
				sliderwin2.changed(set4) ||
				sliderwin2.changed(set5) ||
				sliderwin2.changed(set6) ||
				sliderwin2.changed(set7) ||
				sliderwin2.changed(set8) ||
				sliderwin2.changed(set9) ||
				sliderwin2.changed(set10)) {
				// Update preview sprite
				if (set0 >= 0) cel[AnimFrame.PATTERN] = sliderwin2.value(set0);
				cel[AnimFrame.ZOOMX] = sliderwin2.value(set3);
				cel[AnimFrame.ZOOMY] = sliderwin2.value(set4);
				cel[AnimFrame.ANGLE] = sliderwin2.value(set5);
				cel[AnimFrame.OPACITY] = sliderwin2.value(set6);
				cel[AnimFrame.BLENDTYPE] = sliderwin2.value(set7);
				cel[AnimFrame.MIRROR] = sliderwin2.value(set8);
				cel[AnimFrame.PRIORITY] = sliderwin2.value(set9);
				cel[AnimFrame.FOCUS] = new {2, 1, 3, 4}[sliderwin2.value(set10)];
				canvas.setSpriteBitmap(previewsprite, cel);
				SpriteSetAnimFrame(previewsprite, cel, null, null);
				previewsprite.z = previewwin.z + 1;
			}
			if (sliderwin2.changed(flashbutton)) {
				SetFlash(cel, previewsprite);
				SpriteSetAnimFrame(previewsprite, cel, null, null);
				previewsprite.z = previewwin.z + 1;
			}
			if (sliderwin2.changed(tonebutton)) {
				SetTone(cel, previewsprite);
				SpriteSetAnimFrame(previewsprite, cel, null, null);
				previewsprite.z = previewwin.z + 1;
			}
			if (sliderwin2.changed(okbutton)) {
				if (set0 >= 0) cel[AnimFrame.PATTERN] = sliderwin2.value(set0);
				cel[AnimFrame.X] = sliderwin2.value(set1);
				cel[AnimFrame.Y] = sliderwin2.value(set2);
				cel[AnimFrame.ZOOMX] = sliderwin2.value(set3);
				cel[AnimFrame.ZOOMY] = sliderwin2.value(set4);
				cel[AnimFrame.ANGLE] = sliderwin2.value(set5);
				cel[AnimFrame.OPACITY] = sliderwin2.value(set6);
				cel[AnimFrame.BLENDTYPE] = sliderwin2.value(set7);
				cel[AnimFrame.MIRROR] = sliderwin2.value(set8);
				cel[AnimFrame.PRIORITY] = sliderwin2.value(set9);
				cel[AnimFrame.FOCUS] = new {2, 1, 3, 4}[sliderwin2.value(set10)];
				thiscel = canvas.currentCel;
				// Save by replacing current cell
				thiscel[0, thiscel.length] = cel;
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		previewwin.dispose;
		previewsprite.dispose;
		sliderwin2.dispose;
		return;
	}

	//=============================================================================
	// Pop-up menus for buttons in right hand menu.
	//=============================================================================
	public void TimingList(canvas) {
		commands = new List<string>();
		cmdNewSound = -1;
		cmdNewBG = -1;
		cmdEditBG = -1;
		cmdNewFO = -1;
		cmdEditFO = -1;
		canvas.animation.timing.each(i => commands.Add(i.ToString()));
		commands[cmdNewSound = commands.length] = _INTL("Add: Play Sound...");
		commands[cmdNewBG = commands.length] = _INTL("Add: Set Background Graphic...");
		commands[cmdEditBG = commands.length] = _INTL("Add: Edit Background Color/Location...");
		commands[cmdNewFO = commands.length] = _INTL("Add: Set Foreground Graphic...");
		commands[cmdEditFO = commands.length] = _INTL("Add: Edit Foreground Color/Location...");
		cmdwin = ListWindow(commands, 480);
		cmdwin.x = 0;
		cmdwin.y = 0;
		cmdwin.width = 640;
		cmdwin.height = 384;
		cmdwin.opacity = 200;
		cmdwin.viewport = canvas.viewport;
		framewindow = new ControlWindow(0, 384, 640, 32 * 4);
		framewindow.addSlider(_INTL("Frame:"), 1, canvas.animation.length, canvas.currentframe + 1);
		framewindow.addButton(_INTL("Set Frame"));
		framewindow.addButton(_INTL("Delete Timing"));
		framewindow.opacity = 200;
		framewindow.viewport = canvas.viewport;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwin.update;
			framewindow.update;
			if (cmdwin.index != cmdNewSound &&
				cmdwin.index != cmdNewBG &&
				cmdwin.index != cmdEditBG &&
				cmdwin.index != cmdNewFO &&
				cmdwin.index != cmdEditFO) {
				if (framewindow.changed(1)) {   // Set Frame
					canvas.animation.timing[cmdwin.index].frame = framewindow.value(0) - 1;
					cmdwin.commands[cmdwin.index] = canvas.animation.timing[cmdwin.index].ToString();
					cmdwin.refresh;
					continue;
				}
				if (framewindow.changed(2)) {   // Delete Timing
					canvas.animation.timing.delete_at(cmdwin.index);
					cmdwin.commands.delete_at(cmdwin.index);
					if (cmdNewSound >= 0) cmdNewSound -= 1;
					if (cmdNewBG >= 0) cmdNewBG -= 1;
					if (cmdEditBG >= 0) cmdEditBG -= 1;
					if (cmdNewFO >= 0) cmdNewFO -= 1;
					if (cmdEditFO >= 0) cmdEditFO -= 1;
					cmdwin.refresh;
					continue;
				}
			}
			if (Input.trigger(Input.USE)) {
				redrawcmds = false;
				if (cmdwin.index == cmdNewSound) {   // Add new sound
					newaudio = new AnimTiming(0);
					if (SelectSE(canvas, newaudio)) {
						newaudio.frame = framewindow.value(0) - 1;
						canvas.animation.timing.Add(newaudio);
						redrawcmds = true;
					}
				} else if (cmdwin.index == cmdNewBG) {   // Add new background graphic set
					newtiming = new AnimTiming(1);
					if (SelectBG(canvas, newtiming)) {
						newtiming.frame = framewindow.value(0) - 1;
						canvas.animation.timing.Add(newtiming);
						redrawcmds = true;
					}
				} else if (cmdwin.index == cmdEditBG) {   // Add new background edit
					newtiming = new AnimTiming(2);
					if (EditBG(canvas, newtiming)) {
						newtiming.frame = framewindow.value(0) - 1;
						canvas.animation.timing.Add(newtiming);
						redrawcmds = true;
					}
				} else if (cmdwin.index == cmdNewFO) {   // Add new foreground graphic set
					newtiming = new AnimTiming(3);
					if (SelectBG(canvas, newtiming)) {
						newtiming.frame = framewindow.value(0) - 1;
						canvas.animation.timing.Add(newtiming);
						redrawcmds = true;
					}
				} else if (cmdwin.index == cmdEditFO) {   // Add new foreground edit
					newtiming = new AnimTiming(4);
					if (EditBG(canvas, newtiming)) {
						newtiming.frame = framewindow.value(0) - 1;
						canvas.animation.timing.Add(newtiming);
						redrawcmds = true;
					}
				} else {
					// Edit a timing here
					switch (canvas.animation.timing[cmdwin.index].timingType) {
						case 0:
							SelectSE(canvas, canvas.animation.timing[cmdwin.index]);
							break;
						case 1: case 3:
							SelectBG(canvas, canvas.animation.timing[cmdwin.index]);
							break;
						case 2: case 4:
							EditBG(canvas, canvas.animation.timing[cmdwin.index]);
							break;
					}
					cmdwin.commands[cmdwin.index] = canvas.animation.timing[cmdwin.index].ToString();
					cmdwin.refresh;
				}
				if (redrawcmds) {
					if (cmdNewSound >= 0) cmdwin.commands[cmdNewSound] = null;
					if (cmdNewBG >= 0) cmdwin.commands[cmdNewBG] = null;
					if (cmdEditBG >= 0) cmdwin.commands[cmdEditBG] = null;
					if (cmdNewFO >= 0) cmdwin.commands[cmdNewFO] = null;
					if (cmdEditFO >= 0) cmdwin.commands[cmdEditFO] = null;
					cmdwin.commands.compact!;
					cmdwin.commands.Add(canvas.animation.timing[canvas.animation.timing.length - 1].ToString());
					cmdwin.commands[cmdNewSound = cmdwin.commands.length] = _INTL("Add: Play Sound...");
					cmdwin.commands[cmdNewBG = cmdwin.commands.length] = _INTL("Add: Set Background Graphic...");
					cmdwin.commands[cmdEditBG = cmdwin.commands.length] = _INTL("Add: Edit Background Color/Location...");
					cmdwin.commands[cmdNewFO = cmdwin.commands.length] = _INTL("Add: Set Foreground Graphic...");
					cmdwin.commands[cmdEditFO = cmdwin.commands.length] = _INTL("Add: Edit Foreground Color/Location...");
					cmdwin.refresh;
				}
			} else if (Input.trigger(Input.BACK)) {
				break;
			}
		}
		cmdwin.dispose;
		framewindow.dispose;
		return;
	}

	public void SelectSE(canvas, audio) {
		filename = (audio.name != "") ? audio.name : "";
		displayname = (filename != "") ? filename : _INTL("<user's cry>");
		animfiles = new List<string>();
		ret = false;
		RgssChdir(File.join("Audio", "SE", "Anim")) do;
			animfiles.concat(Dir.glob("*.wav"));
			animfiles.concat(Dir.glob("*.ogg"));
			animfiles.concat(Dir.glob("*.mp3"));
			animfiles.concat(Dir.glob("*.wma"));
		}
		animfiles.uniq!;
		animfiles.sort! { |a, b| a.downcase <=> b.downcase };
		animfiles = [_INTL("[Play user's cry]")] + animfiles;
		cmdwin = ListWindow(animfiles, 320);
		cmdwin.height = 480;
		cmdwin.opacity = 200;
		cmdwin.viewport = canvas.viewport;
		maxsizewindow = new ControlWindow(320, 0, 320, 32 * 8);
		maxsizewindow.addLabel(_INTL("File: \"{1}\"", displayname));
		maxsizewindow.addSlider(_INTL("Volume:"), 0, 100, audio.volume);
		maxsizewindow.addSlider(_INTL("Pitch:"), 20, 250, audio.pitch);
		maxsizewindow.addButton(_INTL("Play Sound"));
		maxsizewindow.addButton(_INTL("Stop Sound"));
		maxsizewindow.addButton(_INTL("OK"));
		maxsizewindow.addButton(_INTL("Cancel"));
		maxsizewindow.opacity = 200;
		maxsizewindow.viewport = canvas.viewport;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwin.update;
			maxsizewindow.update;
			if (maxsizewindow.changed(3) && animfiles.length > 0 && filename != "") {   // Play Sound
				SEPlay(new RPG.AudioFile("Anim/" + filename, maxsizewindow.value(1), maxsizewindow.value(2)));
			}
			if (maxsizewindow.changed(4) && animfiles.length > 0) SEStop;   // Stop Sound
			if (maxsizewindow.changed(5)) { // OK
				audio.name = File.basename(filename, ".*");
				audio.volume = maxsizewindow.value(1);
				audio.pitch = maxsizewindow.value(2);
				ret = true;
				break;
			}
			if (maxsizewindow.changed(6)) break;   // Cancel
			if (Input.trigger(Input.USE) && animfiles.length > 0) {
				filename = (cmdwin.index == 0) ? "" : cmdwin.commands[cmdwin.index];
				displayname = (filename != "") ? filename : _INTL("<user's cry>");
				maxsizewindow.controls[0].text = _INTL("File: \"{1}\"", displayname);
			} else if (Input.trigger(Input.BACK)) {
				break;
			}
		}
		cmdwin.dispose;
		maxsizewindow.dispose;
		return ret;
	}

	public void SelectBG(canvas, timing) {
		filename = timing.name;
		cmdErase = -1;
		animfiles = new List<string>();
		animfiles[cmdErase = animfiles.length] = _INTL("[Erase background graphic]");
		ret = false;
		RgssChdir(File.join("Graphics", "Animations")) do;
			animfiles.concat(Dir.glob("*.png"));
			animfiles.concat(Dir.glob("*.gif"));
	//    animfiles.concat(Dir.glob("*.jpg"))
	//    animfiles.concat(Dir.glob("*.jpeg"))
	//    animfiles.concat(Dir.glob("*.bmp"))
		}
		animfiles.uniq!;
		animfiles.sort! { |a, b| a.downcase <=> b.downcase };
		cmdwin = ListWindow(animfiles, 320);
		cmdwin.height = 480;
		cmdwin.opacity = 200;
		cmdwin.viewport = canvas.viewport;
		maxsizewindow = new ControlWindow(320, 0, 320, 32 * 11);
		maxsizewindow.addLabel(_INTL("File: \"{1}\"", filename));
		maxsizewindow.addSlider(_INTL("X:"), -500, 500, timing.bgX || 0);
		maxsizewindow.addSlider(_INTL("Y:"), -500, 500, timing.bgY || 0);
		maxsizewindow.addSlider(_INTL("Opacity:"), 0, 255, timing.opacity || 0);
		maxsizewindow.addSlider(_INTL("Red:"), 0, 255, timing.colorRed || 0);
		maxsizewindow.addSlider(_INTL("Green:"), 0, 255, timing.colorGreen || 0);
		maxsizewindow.addSlider(_INTL("Blue:"), 0, 255, timing.colorBlue || 0);
		maxsizewindow.addSlider(_INTL("Alpha:"), 0, 255, timing.colorAlpha || 0);
		maxsizewindow.addButton(_INTL("OK"));
		maxsizewindow.addButton(_INTL("Cancel"));
		maxsizewindow.opacity = 200;
		maxsizewindow.viewport = canvas.viewport;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwin.update;
			maxsizewindow.update;
			if (maxsizewindow.changed(8)) {   // OK
				timing.name = File.basename(filename, ".*");
				timing.bgX = maxsizewindow.value(1);
				timing.bgY = maxsizewindow.value(2);
				timing.opacity = maxsizewindow.value(3);
				timing.colorRed = maxsizewindow.value(4);
				timing.colorGreen = maxsizewindow.value(5);
				timing.colorBlue = maxsizewindow.value(6);
				timing.colorAlpha = maxsizewindow.value(7);
				ret = true;
				break;
			}
			if (maxsizewindow.changed(9)) break;   // Cancel
			if (Input.trigger(Input.USE) && animfiles.length > 0) {
				filename = (cmdwin.index == cmdErase) ? "" : cmdwin.commands[cmdwin.index];
				maxsizewindow.controls[0].text = _INTL("File: \"{1}\"", filename);
			} else if (Input.trigger(Input.BACK)) {
				break;
			}
		}
		cmdwin.dispose;
		maxsizewindow.dispose;
		return ret;
	}

	public void EditBG(canvas, timing) {
		ret = false;
		maxsizewindow = new ControlWindow(0, 0, 320, 32 * 11);
		maxsizewindow.addSlider(_INTL("Duration:"), 0, 50, timing.duration);
		maxsizewindow.addOptionalSlider(_INTL("X:"), -500, 500, timing.bgX || 0);
		maxsizewindow.addOptionalSlider(_INTL("Y:"), -500, 500, timing.bgY || 0);
		maxsizewindow.addOptionalSlider(_INTL("Opacity:"), 0, 255, timing.opacity || 0);
		maxsizewindow.addOptionalSlider(_INTL("Red:"), 0, 255, timing.colorRed || 0);
		maxsizewindow.addOptionalSlider(_INTL("Green:"), 0, 255, timing.colorGreen || 0);
		maxsizewindow.addOptionalSlider(_INTL("Blue:"), 0, 255, timing.colorBlue || 0);
		maxsizewindow.addOptionalSlider(_INTL("Alpha:"), 0, 255, timing.colorAlpha || 0);
		maxsizewindow.addButton(_INTL("OK"));
		maxsizewindow.addButton(_INTL("Cancel"));
		maxsizewindow.controls[1].checked = !timing.bgX.null();
		maxsizewindow.controls[2].checked = !timing.bgY.null();
		maxsizewindow.controls[3].checked = !timing.opacity.null();
		maxsizewindow.controls[4].checked = !timing.colorRed.null();
		maxsizewindow.controls[5].checked = !timing.colorGreen.null();
		maxsizewindow.controls[6].checked = !timing.colorBlue.null();
		maxsizewindow.controls[7].checked = !timing.colorAlpha.null();
		maxsizewindow.opacity = 200;
		maxsizewindow.viewport = canvas.viewport;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			maxsizewindow.update;
			if (maxsizewindow.changed(8)) {   // OK
				if (maxsizewindow.controls[1].checked ||
					maxsizewindow.controls[2].checked ||
					maxsizewindow.controls[3].checked ||
					maxsizewindow.controls[4].checked ||
					maxsizewindow.controls[5].checked ||
					maxsizewindow.controls[6].checked ||
					maxsizewindow.controls[7].checked) {
					timing.duration = maxsizewindow.value(0);
					timing.bgX = maxsizewindow.value(1);
					timing.bgY = maxsizewindow.value(2);
					timing.opacity = maxsizewindow.value(3);
					timing.colorRed = maxsizewindow.value(4);
					timing.colorGreen = maxsizewindow.value(5);
					timing.colorBlue = maxsizewindow.value(6);
					timing.colorAlpha = maxsizewindow.value(7);
					ret = true;
				}
				break;
			}
			if (maxsizewindow.changed(9)) break;   // Cancel
			if (Input.trigger(Input.BACK)) {
				break;
			}
		}
		maxsizewindow.dispose;
		return ret;
	}

	public void CopyFrames(canvas) {
		sliderwin2 = new ControlWindow(0, 0, 320, 32 * 6);
		sliderwin2.viewport = canvas.viewport;
		sliderwin2.addSlider(_INTL("First Frame:"), 1, canvas.animation.length, 1);
		sliderwin2.addSlider(_INTL("Last Frame:"), 1, canvas.animation.length, canvas.animation.length);
		sliderwin2.addSlider(_INTL("Copy to:"), 1, canvas.animation.length, canvas.currentframe + 1);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		sliderwin2.opacity = 200;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton)) {
				startvalue = sliderwin2.value(0) - 1;
				endvalue = sliderwin2.value(1) - 1;
				dstvalue = sliderwin2.value(2) - 1;
				length = (endvalue - startvalue) + 1;
				if (length > 0) {   // Ensure correct overlap handling
					if (startvalue < dstvalue) {
						startvalue += length;
						dstvalue += length;
						while (length != 0) {
							canvas.copyFrame(startvalue - 1, dstvalue - 1);
							startvalue -= 1;
							dstvalue -= 1;
							length -= 1;
						}
					} else if (startvalue != dstvalue) {
						while (length != 0) {
							canvas.copyFrame(startvalue, dstvalue);
							startvalue += 1;
							dstvalue += 1;
							length -= 1;
						}
					}
				}
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin2.dispose;
		return;
	}

	public void ClearFrames(canvas) {
		sliderwin2 = new ControlWindow(0, 0, 320, 32 * 5);
		sliderwin2.viewport = canvas.viewport;
		sliderwin2.addSlider(_INTL("First Frame:"), 1, canvas.animation.length, 1);
		sliderwin2.addSlider(_INTL("Last Frame:"), 1, canvas.animation.length, canvas.animation.length);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		sliderwin2.opacity = 200;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton)) {
				startframe = sliderwin2.value(0) - 1;
				endframe = sliderwin2.value(1) - 1;
				(startframe..endframe).each do |i|
					canvas.clearFrame(i);
				}
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin2.dispose;
		return;
	}

	public void Tweening(canvas) {
		sliderwin2 = new ControlWindow(0, 0, 320, 32 * 10);
		sliderwin2.viewport = canvas.viewport;
		sliderwin2.opacity = 200;
		s1set0 = sliderwin2.addSlider(_INTL("Starting Frame:"), 1, canvas.animation.length, 1);
		s1set1 = sliderwin2.addSlider(_INTL("Ending Frame:"), 1, canvas.animation.length, canvas.animation.length);
		s1set2 = sliderwin2.addSlider(_INTL("First Cel:"), 0, Animation.MAX_SPRITES - 1, 0);
		s1set3 = sliderwin2.addSlider(_INTL("Last Cel:"), 0, Animation.MAX_SPRITES - 1, Animation.MAX_SPRITES - 1);
		set0 = sliderwin2.addCheckbox(_INTL("Pattern"));
		set1 = sliderwin2.addCheckbox(_INTL("Position/Zoom/Angle"));
		set2 = sliderwin2.addCheckbox(_INTL("Opacity/Blending"));
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton) || Input.trigger(Input.USE)) {
				startframe = sliderwin2.value(s1set0) - 1;
				endframe = sliderwin2.value(s1set1) - 1;
				if (startframe >= endframe) break;
				frames = endframe - startframe;
				startcel = sliderwin2.value(s1set2);
				endcel = sliderwin2.value(s1set3);
				(startcel..endcel).each do |j|
					cel1 = canvas.animation[startframe][j];
					cel2 = canvas.animation[endframe][j];
					if (!cel1 || !cel2) continue;
					diffPattern = cel2[AnimFrame.PATTERN] - cel1[AnimFrame.PATTERN];
					diffX = cel2[AnimFrame.X] - cel1[AnimFrame.X];
					diffY = cel2[AnimFrame.Y] - cel1[AnimFrame.Y];
					diffZoomX = cel2[AnimFrame.ZOOMX] - cel1[AnimFrame.ZOOMX];
					diffZoomY = cel2[AnimFrame.ZOOMY] - cel1[AnimFrame.ZOOMY];
					diffAngle = cel2[AnimFrame.ANGLE] - cel1[AnimFrame.ANGLE];
					diffOpacity = cel2[AnimFrame.OPACITY] - cel1[AnimFrame.OPACITY];
					diffBlend = cel2[AnimFrame.BLENDTYPE] - cel1[AnimFrame.BLENDTYPE];
					startPattern = cel1[AnimFrame.PATTERN];
					startX = cel1[AnimFrame.X];
					startY = cel1[AnimFrame.Y];
					startZoomX = cel1[AnimFrame.ZOOMX];
					startZoomY = cel1[AnimFrame.ZOOMY];
					startAngle = cel1[AnimFrame.ANGLE];
					startOpacity = cel1[AnimFrame.OPACITY];
					startBlend = cel1[AnimFrame.BLENDTYPE];
					(0..frames).each do |k|
						cel = canvas.animation[startframe + k][j];
						curcel = cel;
						if (!cel) {
							cel = CreateCel(0, 0, 0);
							canvas.animation[startframe + k][j] = cel;
						}
						if (sliderwin2.value(set0) || !curcel) {
							cel[AnimFrame.PATTERN] = startPattern + (diffPattern * k / frames);
						}
						if (sliderwin2.value(set1) || !curcel) {
							cel[AnimFrame.X] = startX + (diffX * k / frames);
							cel[AnimFrame.Y] = startY + (diffY * k / frames);
							cel[AnimFrame.ZOOMX] = startZoomX + (diffZoomX * k / frames);
							cel[AnimFrame.ZOOMY] = startZoomY + (diffZoomY * k / frames);
							cel[AnimFrame.ANGLE] = startAngle + (diffAngle * k / frames);
						}
						if (sliderwin2.value(set2) || !curcel) {
							cel[AnimFrame.OPACITY] = startOpacity + (diffOpacity * k / frames);
							cel[AnimFrame.BLENDTYPE] = startBlend + (diffBlend * k / frames);
						}
					}
				}
				canvas.invalidate;
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin2.dispose;
	}

	public void CellBatch(canvas) {
		sliderwin1 = new ControlWindow(0, 0, 300, 32 * 5);
		sliderwin1.viewport = canvas.viewport;
		sliderwin1.opacity = 200;
		s1set0 = sliderwin1.addSlider(_INTL("First Frame:"), 1, canvas.animation.length, 1);
		s1set1 = sliderwin1.addSlider(_INTL("Last Frame:"), 1, canvas.animation.length, canvas.animation.length);
		s1set2 = sliderwin1.addSlider(_INTL("First Cel:"), 0, Animation.MAX_SPRITES - 1, 0);
		s1set3 = sliderwin1.addSlider(_INTL("Last Cel:"), 0, Animation.MAX_SPRITES - 1, Animation.MAX_SPRITES - 1);
		sliderwin2 = new ControlWindow(300, 0, 340, 32 * 14);
		sliderwin2.viewport = canvas.viewport;
		sliderwin2.opacity = 200;
		set0 = sliderwin2.addOptionalSlider(_INTL("Pattern:"), -2, 1000, 0);
		set1 = sliderwin2.addOptionalSlider(_INTL("X:"), -64, 512 + 64, 0);
		set2 = sliderwin2.addOptionalSlider(_INTL("Y:"), -64, 384 + 64, 0);
		set3 = sliderwin2.addOptionalSlider(_INTL("Zoom X:"), 5, 1000, 100);
		set4 = sliderwin2.addOptionalSlider(_INTL("Zoom Y:"), 5, 1000, 100);
		set5 = sliderwin2.addOptionalSlider(_INTL("Angle:"), 0, 359, 0);
		set6 = sliderwin2.addOptionalSlider(_INTL("Opacity:"), 0, 255, 255);
		set7 = sliderwin2.addOptionalSlider(_INTL("Blending:"), 0, 2, 0);
		set8 = sliderwin2.addOptionalTextSlider(_INTL("Flip:"), new {_INTL("False"), _INTL("True")}, 0);
		prio = new {_INTL("Back"), _INTL("Front"), _INTL("Behind focus"), _INTL("Above focus")};
		set9 = sliderwin2.addOptionalTextSlider(_INTL("Priority:"), prio, 1);
		foc = new {_INTL("User"), _INTL("Target"), _INTL("User and target"), _INTL("Screen")};
		curfoc = new {3, 1, 0, 2, 3}[canvas.animation.position || 4];
		set10 = sliderwin2.addOptionalTextSlider(_INTL("Focus:"), foc, curfoc);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin1.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton) || Input.trigger(Input.USE)) {
				startframe = sliderwin1.value(s1set0) - 1;
				endframe = sliderwin1.value(s1set1) - 1;
				startcel = sliderwin1.value(s1set2);
				endcel = sliderwin1.value(s1set3);
				(startframe..endframe).each do |i|
					(startcel..endcel).each do |j|
						if (!canvas.animation[i][j]) continue;
						cel = canvas.animation[i][j];
						if (sliderwin2.value(set0)) cel[AnimFrame.PATTERN] = sliderwin2.value(set0);
						if (sliderwin2.value(set1)) cel[AnimFrame.X] = sliderwin2.value(set1);
						if (sliderwin2.value(set2)) cel[AnimFrame.Y] = sliderwin2.value(set2);
						if (sliderwin2.value(set3)) cel[AnimFrame.ZOOMX] = sliderwin2.value(set3);
						if (sliderwin2.value(set4)) cel[AnimFrame.ZOOMY] = sliderwin2.value(set4);
						if (sliderwin2.value(set5)) cel[AnimFrame.ANGLE] = sliderwin2.value(set5);
						if (sliderwin2.value(set6)) cel[AnimFrame.OPACITY] = sliderwin2.value(set6);
						if (sliderwin2.value(set7)) cel[AnimFrame.BLENDTYPE] = sliderwin2.value(set7);
						if (sliderwin2.value(set8)) cel[AnimFrame.MIRROR] = sliderwin2.value(set8);
						if (sliderwin2.value(set9)) cel[AnimFrame.PRIORITY] = sliderwin2.value(set9);
						if (sliderwin2.value(set10)) cel[AnimFrame.FOCUS] = new {2, 1, 3, 4}[sliderwin2.value(set10)];
					}
				}
				canvas.invalidate;
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin1.dispose;
		sliderwin2.dispose;
	}

	public void EntireSlide(canvas) {
		sliderwin2 = new ControlWindow(0, 0, 320, 32 * 7);
		sliderwin2.viewport = canvas.viewport;
		sliderwin2.addSlider(_INTL("First Frame:"), 1, canvas.animation.length, 1);
		sliderwin2.addSlider(_INTL("Last Frame:"), 1, canvas.animation.length, canvas.animation.length);
		sliderwin2.addSlider(_INTL("X-Axis Movement"), -500, 500, 0);
		sliderwin2.addSlider(_INTL("Y-Axis Movement"), -500, 500, 0);
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		sliderwin2.opacity = 200;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(okbutton)) {
				startvalue = sliderwin2.value(0) - 1;
				endvalue = sliderwin2.value(1) - 1;
				xoffset = sliderwin2.value(2);
				yoffset = sliderwin2.value(3);
				(startvalue..endvalue).each do |i|
					canvas.offsetFrame(i, xoffset, yoffset);
				}
				break;
			}
			if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		sliderwin2.dispose;
		return;
	}

	public void AnimEditorHelpWindow() {
		helptext = "" +;
							"To add a cel to the scene, click on the canvas. The selected cel will have a black " +;
							"frame. After a cel is selected, you can modify its properties using the keyboard:\n" +;
							"E, R - Rotate left/right.\nP - Open properties screen.\nArrow keys - Move cel 8 pixels " +;
							"(hold ALT for 2 pixels).\n+/- : Zoom in/out.\nL - Lock a cel. Locking a cel prevents it " +;
							"from being moved or deleted.\nDEL - Deletes the cel.\nAlso press TAB to switch the selected cel.";
		cmdwin = Window_UnformattedTextPokemon.newWithSize("", 0, 0, 640, 512);
		cmdwin.opacity = 224;
		cmdwin.z = 99999;
		cmdwin.text = helptext;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwin.update;
			if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) break;
		}
		cmdwin.dispose;
	}

	//=============================================================================
	// Main.
	//=============================================================================
	public void animationEditorMain(animation) {
		viewport = new Viewport(0, 0, Settings.SCREEN_WIDTH + 288, Settings.SCREEN_HEIGHT + 288);
		viewport.z = 99999;
		// Canvas
		canvas = new AnimationCanvas(animation[animation.selected] || animation[0], viewport);
		// Right hand menu
		sidewin = new ControlWindow(512 + 128, 0, 160, 384 + 128);
		sidewin.addButton(_INTL("SE and BG..."));
		sidewin.addButton(_INTL("Cel Focus..."));
		sidewin.addSpace;
		sidewin.addButton(_INTL("Paste Last"));
		sidewin.addButton(_INTL("Copy Frames..."));
		sidewin.addButton(_INTL("Clear Frames..."));
		sidewin.addButton(_INTL("Tweening..."));
		sidewin.addButton(_INTL("Cel Batch..."));
		sidewin.addButton(_INTL("Entire Slide..."));
		sidewin.addSpace;
		sidewin.addButton(_INTL("Play Animation"));
		sidewin.addButton(_INTL("Play Opp Anim"));
		sidewin.addButton(_INTL("Import Anim..."));
		sidewin.addButton(_INTL("Export Anim..."));
		sidewin.addButton(_INTL("Help"));
		sidewin.viewport = canvas.viewport;
		// Bottom left menu
		sliderwin = new ControlWindow(0, 384 + 128, 240, 160);
		sliderwin.addControl(new FrameCountSlider(canvas));
		sliderwin.addControl(new FrameCountButton(canvas));
		sliderwin.addButton(_INTL("Set Animation Sheet"));
		sliderwin.addButton(_INTL("List of Animations"));
		sliderwin.viewport = canvas.viewport;
		// Animation sheet window
		animwin = new CanvasAnimationWindow(canvas, 240, 384 + 128, 512, 96, canvas.viewport);
		// Name window
		bottomwindow = new AnimationNameWindow(canvas, 240, 384 + 128 + 96, 512, 64, canvas.viewport);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin.update;
			canvas.update;
			sidewin.update;
			animwin.update;
			bottomwindow.update;
			if (animwin.changed()) canvas.pattern = animwin.selected;
			if (Input.trigger(Input.BACK)) {
				if (ConfirmMessage(_INTL("Save changes?"))) {
					save_data(animation, "Data/PkmnAnimations.rxdata");
				}
				if (ConfirmMessage(_INTL("Exit from the editor?"))) {
					Game.GameData.game_temp.battle_animations_data = null;
					break;
				}
			}
			if (Input.triggerex(:F5)) {
				AnimEditorHelpWindow;
				continue;
			} else if (Input.trigger(Input.MOUSERIGHT) && sliderwin.hittest(0)) {   // Right mouse button
				commands = new {
					_INTL("Copy Frame"),
					_INTL("Paste Frame"),
					_INTL("Clear Frame"),
					_INTL("Insert Frame"),
					_INTL("Delete Frame");
				}
				hit = TrackPopupMenu(commands);
				switch (hit) {
					case 0: // Copy
						if (canvas.currentframe >= 0) {
							Clipboard.setData(canvas.animation[canvas.currentframe], "AnimFrame");
						}
						break;
					case 1: // Paste
						if (canvas.currentframe >= 0) canvas.pasteFrame(canvas.currentframe);
						break;
					case 2: // Clear Frame
						canvas.clearFrame(canvas.currentframe);
						break;
					case 3: // Insert Frame
						canvas.insertFrame(canvas.currentframe);
						sliderwin.invalidate;
						break;
					case 4: // Delete Frame
						canvas.deleteFrame(canvas.currentframe);
						sliderwin.controls[0].curvalue = canvas.currentframe + 1;
						sliderwin.invalidate;
						break;
				}
				continue;
			} else if (Input.triggerex(:Q)) {
				if (canvas.currentCel) {
					DefinePath(canvas);
					sliderwin.invalidate;
				}
				continue;
			} else if (Input.trigger(Input.MOUSERIGHT)) {   // Right mouse button
				mousepos = Mouse.getMousePos;
				if (!mousepos) mousepos = new {0, 0};
				commands = new {
					_INTL("Properties..."),
					_INTL("Cut"),
					_INTL("Copy"),
					_INTL("Paste"),
					_INTL("Delete"),
					_INTL("Renumber..."),
					_INTL("Extrapolate Path...");
				}
				hit = TrackPopupMenu(commands);
				switch (hit) {
					case 0:   // Properties
						if (canvas.currentCel) {
							CellProperties(canvas);
							canvas.invalidateCel(canvas.currentcel);
						}
						break;
					case 1:   // Cut
						if (canvas.currentCel) {
							Clipboard.setData(canvas.currentCel, "AnimCel");
							canvas.deleteCel(canvas.currentcel);
						}
						break;
					case 2:   // Copy
						if (canvas.currentCel) Clipboard.setData(canvas.currentCel, "AnimCel");
						break;
					case 3:   // Paste
						canvas.pasteCel(mousepos[0], mousepos[1]);
						break;
					case 4:   // Delete
						canvas.deleteCel(canvas.currentcel);
						break;
					case 5:   // Renumber
						if (canvas.currentcel && canvas.currentcel >= 2) {
							cel1 = canvas.currentcel;
							cel2 = ChooseNum(cel1);
							if (cel2 >= 2 && cel1 != cel2) canvas.swapCels(cel1, cel2);
						}
						break;
					case 6:   // Extrapolate Path
						if (canvas.currentCel) {
							DefinePath(canvas);
							sliderwin.invalidate;
						}
						break;
				}
				continue;
			}
			if (sliderwin.changed(0)) {   // Current frame changed
				canvas.currentframe = sliderwin.value(0) - 1;
			}
			if (sliderwin.changed(1)) {   // Change frame count
				ChangeMaximum(canvas);
				if (canvas.currentframe >= canvas.animation.length) {
					canvas.currentframe = canvas.animation.length - 1;
					sliderwin.controls[0].curvalue = canvas.currentframe + 1;
				}
				sliderwin.refresh;
			}
			if (sliderwin.changed(2)) {   // Set Animation Sheet
				SelectAnim(canvas, animwin);
				animwin.refresh;
				sliderwin.refresh;
			}
			if (sliderwin.changed(3)) {   // List of Animations
				AnimList(animation, canvas, animwin);
				sliderwin.controls[0].curvalue = canvas.currentframe + 1;
				bottomwindow.refresh;
				animwin.refresh;
				sliderwin.refresh;
			}
			if (sidewin.changed(0)) TimingList(canvas);
			if (sidewin.changed(1)) {
				positions = new {_INTL("User"), _INTL("Target"), _INTL("User and target"), _INTL("Screen")};
				indexes = new {2, 1, 3, 4};   // Keeping backwards compatibility
				for (int i = positions.length; i < positions.length; i++) { //for 'positions.length' times do => |i|
					selected = "[  ]";
					if (animation[animation.selected].position == indexes[i]) selected = "[X]";
					positions[i] = string.Format("{0} {0}", selected, positions[i]);
				}
				pos = ShowCommands(null, positions, -1);
				if (pos >= 0) {
					animation[animation.selected].position = indexes[pos];
					canvas.update;
				}
			}
			if (sidewin.changed(3)) canvas.pasteLast;
			if (sidewin.changed(4)) CopyFrames(canvas);
			if (sidewin.changed(5)) ClearFrames(canvas);
			if (sidewin.changed(6)) Tweening(canvas);
			if (sidewin.changed(7)) CellBatch(canvas);
			if (sidewin.changed(8)) EntireSlide(canvas);
			if (sidewin.changed(10)) canvas.play;
			if (sidewin.changed(11)) canvas.play(true);
			if (sidewin.changed(12)) {
				ImportAnim(animation, canvas, animwin);
				sliderwin.controls[0].curvalue = canvas.currentframe + 1;
				bottomwindow.refresh;
				animwin.refresh;
				sliderwin.refresh;
			}
			if (sidewin.changed(13)) {
				ExportAnim(animation);
				bottomwindow.refresh;
				animwin.refresh;
				sliderwin.refresh;
			}
			if (sidewin.changed(14)) AnimEditorHelpWindow;
		}
		canvas.dispose;
		animwin.dispose;
		sliderwin.dispose;
		sidewin.dispose;
		bottomwindow.dispose;
		viewport.dispose;
		RPG.Cache.clear;
	}
}

//===============================================================================
// Start.
//===============================================================================
public void AnimationEditor() {
	BGMStop;
	animation = LoadBattleAnimations;
	if (!animation || !animation[0]) {
		animation = new Animations();
		animation[0].graphic = "";
	}
	Graphics.resize_screen(Settings.SCREEN_WIDTH + 288, Settings.SCREEN_HEIGHT + 288);
	SetResizeFactor(1);
	BattleAnimationEditor.animationEditorMain(animation);
	Graphics.resize_screen(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);
	SetResizeFactor(Game.GameData.PokemonSystem.screensize);
	Game.GameData.game_map&.autoplay;
}
