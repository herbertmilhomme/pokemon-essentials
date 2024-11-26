//===============================================================================
// Base class for all hardcoded battle animations.
//===============================================================================
public partial class Battle.Scene.Animation {
	public void initialize(sprites, viewport) {
		@sprites  = sprites;
		@viewport = viewport;
		@pictureEx      = new List<string>();   // For all the PictureEx
		@pictureSprites = new List<string>();   // For all the sprites
		@tempSprites    = new List<string>();   // For sprites that exist only for this animation
		@animDone       = false;
		createProcesses;
	}

	public void dispose() {
		@tempSprites.each(s => s&.dispose);
	}

	public void createProcesses() { }
	public bool empty() { return @pictureEx.length == 0; }
	public bool animDone() { return @animDone; }

	public void addSprite(s, origin = PictureOrigin.TOP_LEFT) {
		num = @pictureEx.length;
		picture = new PictureEx(s.z);
		picture.x       = s.x;
		picture.y       = s.y;
		picture.visible = s.visible;
		picture.color   = s.color.clone;
		picture.tone    = s.tone.clone;
		picture.setOrigin(0, origin);
		@pictureEx[num] = picture;
		@pictureSprites[num] = s;
		return picture;
	}

	public void addNewSprite(x, y, name, origin = PictureOrigin.TOP_LEFT) {
		num = @pictureEx.length;
		picture = new PictureEx(num);
		picture.setXY(0, x, y);
		picture.setName(0, name);
		picture.setOrigin(0, origin);
		@pictureEx[num] = picture;
		s = new IconSprite(x, y, @viewport);
		s.setBitmap(name);
		@pictureSprites[num] = s;
		@tempSprites.Add(s);
		return picture;
	}

	public void update() {
		if (@animDone) return;
		@tempSprites.each(s => s&.update);
		finished = true;
		@pictureEx.each_with_index do |p, i|
			if (!p.running()) continue;
			finished = false;
			p.update;
			setPictureIconSprite(@pictureSprites[i], p);
		}
		if (finished) @animDone = true;
	}
}

//===============================================================================
// Mixin module for certain hardcoded battle animations that involve Poké Balls.
//===============================================================================
public static partial class Battle.Scene.Animation.BallAnimationMixin {
	// Returns the color that the Pokémon turns when it goes into or out of its
	// Poké Ball.
	public void getBattlerColorFromPokeBall(poke_ball) {
		switch (poke_ball) {
			case :GREATBALL:    return new Color(132, 189, 247);
			case :SAFARIBALL:   return new Color(189, 247, 165);
			case :ULTRABALL:    return new Color(255, 255, 123);
			case :MASTERBALL:   return new Color(189, 165, 231);
			case :NETBALL:      return new Color(173, 255, 206);
			case :DIVEBALL:     return new Color(99, 206, 247);
			case :NESTBALL:     return new Color(247, 222,  82);
			case :REPEATBALL:   return new Color(255, 198, 132);
			case :TIMERBALL:    return new Color(239, 247, 247);
			case :LUXURYBALL:   return new Color(255, 140,  82);
			case :PREMIERBALL:  return new Color(255,  74,  82);
			case :DUSKBALL:     return new Color(115, 115, 140);
			case :HEALBALL:     return new Color(255, 198, 231);
			case :QUICKBALL:    return new Color(140, 214, 255);
			case :CHERISHBALL:  return new Color(247,  66,  41);
		}
		return new Color(255, 181, 247);   // Poké Ball, Sport Ball, Apricorn Balls, others
	}

	public void addBallSprite(ballX, ballY, poke_ball) {
		file_path = string.Format("Graphics/Battle animations/ball_{0}", poke_ball);
		ball = addNewSprite(ballX, ballY, file_path, PictureOrigin.CENTER);
		@ballSprite = @pictureSprites.last;
		if (@ballSprite.bitmap.width >= @ballSprite.bitmap.height) {
			@ballSprite.src_rect.width = @ballSprite.bitmap.height / 2;
			ball.setSrcSize(0, @ballSprite.bitmap.height / 2, @ballSprite.bitmap.height);
		}
		return ball;
	}

	public void ballTracksHand(ball, traSprite, safariThrow = false) {
		if (!traSprite || !traSprite.bitmap) raise _INTL("Trainer back sprite doesn't exist.");
		// Back sprite isn't animated, no hand-tracking needed
		if (traSprite.bitmap.width < traSprite.bitmap.height * 2) {
			ball.setVisible(7, true);
			ballStartX = traSprite.x;
			if (!safariThrow) ballStartX -= ball.totalDuration * (Graphics.width / 32);
			ballStartY = traSprite.y - (traSprite.bitmap.height / 2);
			return ballStartX, ballStartY;
		}
		// Back sprite is animated, make the Poké Ball track the trainer's hand
		coordSets = new {new {traSprite.x - 44, traSprite.y - 32}, new {-10, -36}, new {118, -4}}
		switch (@trainer.trainer_type) {
			case :POKEMONTRAINER_Leaf:
				coordSets = new {new {traSprite.x - 30, traSprite.y - 30}, new {-18, -36}, new {118, -6}}
				break;
			case :POKEMONTRAINER_Brendan:
				coordSets = new {new {traSprite.x - 46, traSprite.y - 40}, new {-4, -30}, new {118, -2}}
				break;
			case :POKEMONTRAINER_May:
				coordSets = new {new {traSprite.x - 44, traSprite.y - 38}, new {-8, -30}, new {122, 0}}
				break;
		}
		// Arm stretched out behind player
		ball.setVisible(0, true);
		ball.setXY(0, coordSets[0][0], coordSets[0][1]);
		if (!safariThrow) ball.moveDelta(0, 5, -5 * (Graphics.width / 32), 0);
		if (safariThrow) ball.setDelta(0, -12, 0);
		// Arm mid throw
		ball.setDelta(5, coordSets[1][0], coordSets[1][1]);
		if (!safariThrow) ball.moveDelta(5, 2, -2 * (Graphics.width / 32), 0);
		if (safariThrow) ball.setDelta(5, 34, 0);
		// Start of throw
		ball.setDelta(7, coordSets[2][0], coordSets[2][1]);
		if (safariThrow) ball.setDelta(7, -14, 0);
		// Update Poké Ball trajectory's start position
		ballStartX = ballStartY = 0;
		foreach (var c in coordSets) { //'coordSets.each' do => |c|
			ballStartX += c[0];
			ballStartY += c[1];
		}
		if (!safariThrow) ballStartX -= ball.totalDuration * (Graphics.width / 32);
		if (safariThrow) ballStartX += 8;   // -12 + 34 - 14
		return ballStartX, ballStartY;
	}

	public void trainerThrowingFrames(ball, trainer, traSprite) {
		ball.setZ(0, traSprite.z - 1);
		// Change trainer's frames
		size = traSprite.src_rect.width;   // Width per frame
		trainer.setSrc(0, size, 0);
		trainer.setSrc(5, size * 2, 0);
		trainer.setSrc(7, size * 3, 0);
		trainer.setSrc(9, size * 4, 0);
		trainer.setSrc(18, 0, 0);
		// Alter trainer's positioning
		trainer.setDelta(0, -12, 0);
		trainer.setDelta(5, 34, 0);
		trainer.setDelta(7, -14, 0);
		trainer.setDelta(9, 28, 0);
		trainer.moveDelta(10, 3, -6, 6);
		trainer.setDelta(18, -4, 0);
		trainer.setDelta(19, -26, -6);
		// Make ball track the trainer's hand
		ballStartX, ballStartY = ballTracksHand(ball, traSprite, true);
		return ballStartX, ballStartY;
	}

	public void createBallTrajectory(ball, delay, duration, startX, startY, midX, midY, endX, endY) {
		// NOTE: This trajectory is the same regardless of whether the player's
		//       sprite is being shown on-screen (and sliding off while animating a
		//       throw). Instead, that throw animation and initialDelay are designed
		//       to make sure the Ball's trajectory starts at the same position.
		ball.setVisible(delay, true);
		a = (2 * startY) - (4 * midY) + (2 * endY);
		b = (4 * midY) - (3 * startY) - endY;
		c = startY;
		(1..duration).each do |i|
			t = i.to_f / duration;                // t ranges from 0 to 1
			x = startX + ((endX - startX) * t);   // Linear in t
			y = (a * (t**2)) + (b * t) + c;       // Quadratic in t
			ball.moveXY(delay + i - 1, 1, x, y);
		}
		createBallTumbling(ball, delay, duration);
	}

	public void createBallTumbling(ball, delay, duration) {
		// Animate ball frames
		numTumbles = 1;
		numFrames  = 1;
		if (@ballSprite && @ballSprite.bitmap.width >= @ballSprite.bitmap.height) {
			// 2* because each frame is twice as tall as it is wide
			numFrames = 2 * @ballSprite.bitmap.width / @ballSprite.bitmap.height;
		}
		if (numFrames > 1) {
			curFrame = 0;
			(1..duration).each do |i|
				thisFrame = numFrames * numTumbles * i / duration;
				if (thisFrame > curFrame) {
					curFrame = thisFrame;
					ball.setSrc(delay + i - 1, (curFrame % numFrames) * @ballSprite.bitmap.height / 2, 0);
				}
			}
			ball.setSrc(delay + duration, 0, 0);
		}
		// Rotate ball
		ball.moveAngle(delay, duration, 360 * 3);
		ball.setAngle(delay + duration, 0);
	}

	public void ballSetOpen(ball, delay, poke_ball) {
		file_path = string.Format("Graphics/Battle animations/ball_{0}_open", poke_ball);
		ball.setName(delay, file_path);
		if (@ballSprite && @ballSprite.bitmap.width >= @ballSprite.bitmap.height) {
			ball.setSrcSize(delay, @ballSprite.bitmap.height / 2, @ballSprite.bitmap.height);
		}
	}

	public void ballSetClosed(ball, delay, poke_ball) {
		file_path = string.Format("Graphics/Battle animations/ball_{0}", poke_ball);
		ball.setName(delay, file_path);
		if (@ballSprite && @ballSprite.bitmap.width >= @ballSprite.bitmap.height) {
			ball.setSrcSize(delay, @ballSprite.bitmap.height / 2, @ballSprite.bitmap.height);
		}
	}

	public void ballOpenUp(ball, delay, poke_ball, showSquish = true, playSE = true) {
		if (showSquish) {
			ball.moveZoomXY(delay, 1, 120, 80);   // Squish
			ball.moveZoom(delay + 5, 1, 100);     // Unsquish
			delay += 6;
		}
		if (playSE) ball.setSE(delay, "Battle recall");
		ballSetOpen(ball, delay, poke_ball);
	}

	public void battlerAppear(battler, delay, battlerX, battlerY, batSprite, color) {
		battler.setVisible(delay, true);
		battler.setOpacity(delay, 255);
		battler.moveXY(delay, 5, battlerX, battlerY);
		battler.moveZoom(delay, 5, 100, new {batSprite, :PlayIntroAnimation});
		// NOTE: As soon as the battler sprite finishes zooming, and just as it
		//       starts changing its tone to normal, it plays its intro animation.
		color.alpha = 0;
		battler.moveColor(delay + 5, 10, color);
	}

	public void battlerAbsorb(battler, delay, battlerX, battlerY, color) {
		color.alpha = 255;
		battler.moveColor(delay, 10, color);   // Change color of battler to a solid shade
		delay = battler.totalDuration;
		battler.moveXY(delay, 5, battlerX, battlerY);
		battler.moveZoom(delay, 5, 0);   // Shrink battler into Poké Ball
		battler.setVisible(delay + 5, false);
	}

	// NOTE: This array makes the Ball Burst animation differ between types of Poké
	//       Ball in certain simple ways. The HGSS animations occasionally have
	//       additional differences, which haven't been coded yet in Essentials as
	//       they're more complex and I couldn't be bothered.
	BALL_BURST_VARIANCES = {
		// [ray start tone, ray end tone,
		//  top particle filename, top particle start tone, top particle end tone,
		//  bottom particle filename, bottom particle start tone, bottom particle end tone,
		//  top glare filename, top glare start tone, top glare end tone,
		//  bottom glare filename, bottom glare start tone, bottom glare end tone}
		POKEBALL    = new {new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, -128, -248),   // Yellow, dark orange
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, 0, -192)},   // Light yellow, yellow
		GREATBALL   = new {new Tone(0, 0, 0), new Tone(-128, -64, 0),   // White, blue
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(-128, 0, 0), new Tone(-248, -64, 0),   // Cyan, dark cyan
										"particle", new Tone(0, 0, 0), new Tone(-96, -48, 0),   // White, light blue
										"particle", new Tone(-96, -48, 0), new Tone(-192, -96, 0)},   // Blue, dark blue
		SAFARIBALL  = new {new Tone(0, 0, -32), new Tone(-128, 0, -128),   // Pale yellow, green
										"particle", new Tone(0, 0, -64), new Tone(-160, 0, -160),   // Beige, darker green
										"particle", new Tone(0, 0, -64), new Tone(-160, 0, -160),   // Beige, darker green
										"particle", new Tone(0, 0, 0), new Tone(-80, 0, -80),   // White, light green
										"particle", new Tone(-32, 0, -96), new Tone(-160, 0, -160)},   // Pale green, darker green
		ULTRABALL   = new {new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -64), new Tone(0, 0, -224),   // Pale yellow, yellow
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, -128),   // White, light yellow
										"particle", new Tone(0, 0, -64), new Tone(0, 0, -224)},   // Pale yellow, yellow
		MASTERBALL  = new {new Tone(0, 0, 0), new Tone(-48, -200, -56),   // White, magenta
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(-48, -200, -56),   // White, magenta
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(-48, -200, -56), new Tone(-48, -200, -56)},   // Magenta, magenta
		NETBALL     = new {new Tone(0, 0, 0), new Tone(0, -64, 0),   // White, lilac
										"particle", new Tone(0, 0, 0), new Tone(0, -64, 0),   // White, lilac
										"particle", new Tone(0, 0, 0), new Tone(0, -64, 0),   // White, lilac
										"particle", new Tone(0, 0, 0), new Tone(0, -64, 0),   // White, lilac
										"web", new Tone(-32, -64, -32), new Tone(-64, -128, -64)},   // Light purple, purple
		DIVEBALL    = new {new Tone(0, 0, 0), new Tone(-192, -128, -32),   // White, dark blue
										"bubble", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(-184, -40, 0), new Tone(-184, -40, 0),   // Cyan, cyan
										"dazzle", new Tone(-184, -40, 0), new Tone(-184, -40, 0),   // Cyan, cyan
										"particle", new Tone(0, 0, 0), new Tone(-184, -40, 0)},   // White, cyan
		NESTBALL    = new {new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, -128, -248),   // Light yellow, dark orange
										"dazzle", new Tone(0, 0, 0), new Tone(-96, 0, -96),   // White, green
										"particle", new Tone(-96, 0, -96), new Tone(-192, 0, -192)},   // Green, dark green
		REPEATBALL  = new {new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"ring3", new Tone(-16, -16, -88), new Tone(-32, -32, -176),   // Yellow, yellow
										"particle", new Tone(-144, -144, -144), new Tone(-160, -160, -160),   // Grey, grey
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, -128, -248), new Tone(0, -128, -248)},   // Dark orange, dark orange
		TIMERBALL   = new {new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, -128, -248),   // Yellow, dark orange
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, 0, -192)},   // Light yellow, yellow
		LUXURYBALL  = new {new Tone(0, 0, 0), new Tone(0, -128, -160),   // White, orange
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, -128, -248),   // Yellow, dark orange
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, -64, -144), new Tone(0, -192, -248)},   // Light orange, red
		PREMIERBALL = new {new Tone(0, -160, -148), new Tone(0, 0, 0),   // Red, white
										"particle", new Tone(0, -192, -152), new Tone(0, -192, -152),   // Red, red
										"particle", new Tone(0, 0, 0), new Tone(0, -192, -152),   // White, red
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0)},   // White, white
		DUSKBALL    = new {new Tone(-48, -200, -56), new Tone(-160, -224, -160),   // Magenta, dark purple
										"particle", new Tone(-248, -248, -248), new Tone(-248, -248, -248),   // Black, black
										"particle", new Tone(-24, -96, -32), new Tone(-24, -96, -32),   // Light magenta, light magenta
										"particle", new Tone(-248, -248, -248), new Tone(-248, -248, -248),   // Black, black
										"whirl", new Tone(-160, -224, -160), new Tone(-160, -224, -160)},   // Dark purple, dark purple
		HEALBALL    = new {new Tone(-8, -48, -8), new Tone(-16, -128, -112),   // Pink, dark pink
										"diamond", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"diamond", new Tone(0, -96, -104), new Tone(-160, -64, 0),   // Pink/orange, cyan
										"dazzle", new Tone(0, 0, 0), new Tone(-32, -112, -80),   // White, magenta
										"particle", new Tone(-8, -48, -8), new Tone(-64, -224, -160)},   // Pink, dark magenta
		QUICKBALL   = new {new Tone(-64, 0, 0), new Tone(-192, -96, 0),   // Light cyan, dark blue
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, -128, -248),   // Yellow, dark orange
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(-96, 0, 0), new Tone(-192, -96, 0)},   // Cyan, dark blue
		CHERISHBALL = new {new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white (unused; see below)
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White ,yellow
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, -96), new Tone(0, 0, -192)};   // Light yellow, yellow
	}

	// The regular Poké Ball burst animation, for when a Pokémon appears from a
	// Poké Ball.
	public void ballBurst(delay, ball, ballX, ballY, poke_ball) {
		num_particles = 15;
		num_rays = 10;
		glare_fade_duration = 8;   // Lifetimes/durations are in 20ths of a second
		particle_lifetime = 15;
		particle_fade_duration = 8;
		ray_lifetime = 13;
		ray_fade_duration = 5;
		ray_min_radius = 24;   // How far out from the center a ray starts
		cherish_ball_ray_tones = new {new Tone(-104, -144, -8),   // Indigo
															new Tone(-64, -144, -24),   // Purple
															new Tone(-8, -144, -64),   // Pink
															new Tone(-8, -48, -152),   // Orange
															new Tone(-8, -32, -160)};   // Yellow
		// Get array of things that vary for each kind of Poké Ball
		variances = BALL_BURST_VARIANCES[poke_ball] || BALL_BURST_VARIANCES[:POKEBALL];
		// Set up glare particles
		glare1 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[11]}", PictureOrigin.CENTER);
		glare2 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[8]}", PictureOrigin.CENTER);
		new {glare1, glare2}.each_with_index do |particle, num|
			particle.setZ(0, 105 + num);
			particle.setZoom(0, 0);
			particle.setTone(0, variances[12 - (3 * num)]);
			particle.setVisible(0, false);
		}
		new {glare1, glare2}.each_with_index do |particle, num|
			particle.moveTone(delay + glare_fade_duration + 3, glare_fade_duration / 2, variances[13 - (3 * num)]);
		}
		// Animate glare particles
		new {glare1, glare2}.each(p => p.setVisible(delay, true));
		if (poke_ball == Items.MASTER_BALL) {
			glare1.moveAngle(delay, 19, -135);
			glare1.moveZoom(delay, glare_fade_duration, 250);
		} else if (poke_ball == Items.DUSK_BALL) {
			glare1.moveAngle(delay, 19, -270);
		} else if (["whirl"].Contains(variances[11])) {
			glare1.moveZoom(delay, glare_fade_duration, 200);
		} else {
			glare1.moveZoom(delay, glare_fade_duration, (new []{"dazzle", "ring3", "web"}.Contains(variances[11])) ? 100 : 250);
		}
		glare1.moveOpacity(delay + glare_fade_duration + 3, glare_fade_duration, 0);
		if (poke_ball == Items.MASTER_BALL) {
			glare2.moveAngle(delay, 19, -135);
			glare2.moveZoom(delay, glare_fade_duration, 200);
		} else {
			glare2.moveZoom(delay, glare_fade_duration, (new []{"dazzle", "ring3", "web"}.Contains(variances[8])) ? 125 : 200);
		}
		glare2.moveOpacity(delay + glare_fade_duration + 3, glare_fade_duration - 2, 0);
		new {glare1, glare2}.each(p => p.setVisible(delay + 19, false));
		// Rays
		for (int i = num_rays; i < num_rays; i++) { //for 'num_rays' times do => |i|
			// Set up ray
			angle = rand(360);
			radian = (angle + 90) * Math.PI / 180;
			start_zoom = rand(50...100);
			ray = addNewSprite(ballX + (ray_min_radius * Math.cos(radian)),
												ballY - (ray_min_radius * Math.sin(radian)),
												"Graphics/Battle animations/ballBurst_ray", PictureOrigin.BOTTOM);
			ray.setZ(0, 100);
			ray.setZoomXY(0, 200, start_zoom);
			if (poke_ball != Items.CHERISH_BALL) ray.setTone(0, variances[0]);
			ray.setOpacity(0, 0);
			ray.setVisible(0, false);
			ray.setAngle(0, angle);
			// Animate ray
			start = delay + (i / 2);
			ray.setVisible(start, true);
			ray.moveZoomXY(start, ray_lifetime, 200, start_zoom * 6);
			ray.moveOpacity(start, 2, 255);   // Quickly fade in
			ray.moveOpacity(start + ray_lifetime - ray_fade_duration, ray_fade_duration, 0);   // Fade out
			if (poke_ball == Items.CHERISH_BALL) {
				for (int frame = ray_lifetime; frame < ray_lifetime; frame++) { //for 'ray_lifetime' times do => |frame|
					ray.setTone(start + frame, cherish_ball_ray_tones[frame % cherish_ball_ray_tones.length]);
				}
			} else {
				ray.moveTone(start + ray_lifetime - ray_fade_duration, ray_fade_duration, variances[1]);
			}
			ray.setVisible(start + ray_lifetime, false);
		}
		// Particles
		for (int i = num_particles; i < num_particles; i++) { //for 'num_particles' times do => |i|
			// Set up particles
			particle1 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[5]}", PictureOrigin.CENTER);
			particle2 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[2]}", PictureOrigin.CENTER);
			new {particle1, particle2}.each_with_index do |particle, num|
				particle.setZ(0, 110 + num);
				particle.setZoom(0, (80 - (num * 20)) / (["ring2"].Contains(variances[5 - (3 * num)]) ? 2 : 1));
				particle.setTone(0, variances[6 - (3 * num)]);
				particle.setVisible(0, false);
			}
			// Animate particles
			start = delay + (i / 4);
			max_radius = rand(256...384);
			angle = rand(360);
			radian = angle * Math.PI / 180;
			new {particle1, particle2}.each_with_index do |particle, num|
				particle.setVisible(start, true);
				particle.moveDelta(start, particle_lifetime, max_radius * Math.cos(radian), max_radius * Math.sin(radian));
				particle.moveZoom(start, particle_lifetime, 10);
				particle.moveTone(start + particle_lifetime - particle_fade_duration,
													particle_fade_duration / 2, variances[7 - (3 * num)]);
				particle.moveOpacity(start + particle_lifetime - particle_fade_duration,
														particle_fade_duration,
														0);   // Fade out at end
				particle.setVisible(start + particle_lifetime, false);
			}
		}
	}

	// NOTE: This array makes the Ball Burst capture animation differ between types
	//       of Poké Ball in certain simple ways. The HGSS animations occasionally
	//       have additional differences, which haven't been coded yet in
	//       Essentials as they're more complex and I couldn't be bothered.
	BALL_BURST_CAPTURE_VARIANCES = {
		// [top glare filename, top particle start tone, top particle end tone,
		//  middle glare filename, middle glare start tone, middle glare end tone,
		//  bottom glare filename, bottom glare start tone, bottom glare end tone,
		//  top particle filename, top particle start tone, top particle end tone,
		//  bottom particle filename, bottom particle start tone, bottom particle end tone,
		//  ring tone start, ring tone end}
		POKEBALL    = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle_s", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle_s", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										new Tone(0, 0, -96), new Tone(0, 0, -96)},   // Light yellow, light yellow
		GREATBALL   = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle_s", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle_s", new Tone(0, 0, 0), new Tone(-128, -64, 0),   // White, blue
										new Tone(-128, -32, 0), new Tone(-128, -32, 0)},   // Blue, blue
		SAFARIBALL  = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle_s", new Tone(-48, 0, -48), new Tone(-48, 0, -48),   // Light green, light green
										"particle_s", new Tone(-48, 0, -48), new Tone(-128, 0, -128),   // Light green, green
										new Tone(-48, 0, -48), new Tone(-128, 0, -128)},   // Light green, green
		ULTRABALL   = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										new Tone(0, 0, -128), new Tone(0, 0, -128)},   // Light yellow, light yellow
		MASTERBALL  = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(-64, -128, -64), new Tone(-96, -160, -96),   // Purple, darker purple
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, -80, 0), new Tone(0, -128, -64),   // Purple, hot pink
										new Tone(0, 0, 0), new Tone(-48, -200, -80)},   // White, magenta
		NETBALL     = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle_s", new Tone(-128, -56, 0), new Tone(-128, -56, 0),   // Blue, blue
										"particle_s", new Tone(-128, -56, 0), new Tone(-128, -56, 0),   // Blue, blue
										new Tone(-160, -64, 0), new Tone(-128, -56, 0)},   // Cyan, blue
		DIVEBALL    = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"bubble", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(-128, -48, 0), new Tone(-128, -48, 0),   // Aqua, aqua
										new Tone(-64, 0, 0), new Tone(-180, -32, -32)},   // Light blue, turquoise
		NESTBALL    = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"ring3", new Tone(-32, 0, -104), new Tone(-104, -16, -128),   // Lime green, green
										"ring3", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow (unused)
										new Tone(-48, 0, -48), new Tone(-128, 0, -128)},   // Light green, green
		REPEATBALL  = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"ring3", new Tone(-16, -16, -88), new Tone(-32, -32, -176),   // Yellow, yellow
										"particle", new Tone(-144, -144, -144), new Tone(-160, -160, -160),   // Grey, grey
										new Tone(0, 0, -96), new Tone(0, 0, -96)},   // Light yellow, light yellow
		TIMERBALL   = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle_s", new Tone(0, 0, 0), new Tone(0, 0, -96),   // White, light yellow
										"particle_s", new Tone(0, -48, -160), new Tone(0, 0, -96),   // Orange, light yellow
										new Tone(0, -48, -128), new Tone(0, -160, -248)},   // Light orange, dark orange
		LUXURYBALL  = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, -192, -248),   // White, red
										"particle_s", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle_s", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										new Tone(0, -48, -128), new Tone(0, -192, -248)},   // Light orange, red
		PREMIERBALL = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"ring4", new Tone(-16, -40, -80), new Tone(-16, -136, -176),   // Light orange, dark orange
										new Tone(0, 0, 0), new Tone(0, 0, 0)},   // White, white
		DUSKBALL    = new {"particle", new Tone(-255, -255, -255), new Tone(-255, -255, -255),   // Black, black
										"whirl", new Tone(-112, -184, -128), new Tone(-255, -255, -255),   // Purple, black
										"whirl", new Tone(-112, -184, -128), new Tone(-112, -184, -128),   // Purple, purple
										"particle", new Tone(-112, -184, -128), new Tone(-255, -255, -255),   // Purple, black
										"particle_s", new Tone(-112, -184, -128), new Tone(-255, -255, -255),   // Purple, black
										new Tone(0, 0, -96), new Tone(0, 0, -96)},   // Light yellow, light yellow
		HEALBALL    = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, -32, 0), new Tone(0, -32, 0),   // Light pink, light pink
										"diamond", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"diamond", new Tone(0, 0, 0), new Tone(-160, -64, 0),   // White, cyan
										new Tone(0, 0, 0), new Tone(0, -32, 0)},   // White, light pink
		QUICKBALL   = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, -192),   // White, yellow
										"particle_s", new Tone(0, 0, 0), new Tone(0, 0, -96),   // White, light yellow
										"particle_s", new Tone(0, -48, -160), new Tone(0, 0, -96),   // Orange, light yellow
										new Tone(0, -48, -128), new Tone(0, -160, -248)},   // Light orange, dark orange
		CHERISHBALL = new {"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"dazzle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"particle", new Tone(0, 0, 0), new Tone(0, 0, 0),   // White, white
										"ring4", new Tone(-16, -40, -80), new Tone(-16, -136, -176),   // Light orange, dark orange
										new Tone(0, 0, 0), new Tone(0, 0, 0)};   // White, white
	}

	// The Poké Ball burst animation used when absorbing a wild Pokémon during a
	// capture attempt.
	public void ballBurstCapture(delay, ball, ballX, ballY, poke_ball) {
		particle_duration = 10;
		ring_duration = 5;
		num_particles = 9;
		base_angle = 270;
		base_radius = (poke_ball == Items.MASTER_BALL) ? 192 : 144;   // How far out from the Poké Ball the particles go
		// Get array of things that vary for each kind of Poké Ball
		variances = BALL_BURST_CAPTURE_VARIANCES[poke_ball] || BALL_BURST_CAPTURE_VARIANCES[:POKEBALL];
		// Set up glare particles
		glare1 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[6]}", PictureOrigin.CENTER);
		glare2 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[3]}", PictureOrigin.CENTER);
		glare3 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[0]}", PictureOrigin.CENTER);
		new {glare1, glare2, glare3}.each_with_index do |particle, num|
			particle.setZ(0, 100 + num);
			particle.setZoom(0, 0);
			particle.setTone(0, variances[7 - (3 * num)]);
			particle.setVisible(0, false);
		}
		glare2.setOpacity(0, 160);
		if (poke_ball != Items.DUSK_BALL) glare3.setOpacity(0, 160);
		// Animate glare particles
		new {glare1, glare2, glare3}.each(p => p.setVisible(delay, true));
		switch (poke_ball) {
			case :MASTERBALL:
				glare1.moveZoom(delay, particle_duration, 1200);
				break;
			case :DUSKBALL:
				glare1.moveZoom(delay, particle_duration, 350);
				break;
			default:
				glare1.moveZoom(delay, particle_duration, 600);
				break;
		}
		glare1.moveOpacity(delay + (particle_duration / 2), particle_duration / 2, 0);
		new {glare1, glare2, glare3}.each_with_index do |particle, num|
			particle.moveTone(delay, particle_duration, variances[8 - (3 * num)]);
		}
		if (poke_ball == Items.DUSK_BALL) {
			glare2.moveZoom(delay, particle_duration, 350);
			glare3.moveZoom(delay, particle_duration, 500);
			new {glare2, glare3}.each_with_index do |particle, num|
				particle.moveOpacity(delay + (particle_duration / 2), particle_duration / 2, 0);
			}
		} else {
			glare2.moveZoom(delay, particle_duration, (poke_ball == Items.MASTER_BALL) ? 400 : 250);
			glare2.moveOpacity(delay + (particle_duration / 2), particle_duration / 3, 0);
			glare3.moveZoom(delay, particle_duration, (poke_ball == Items.MASTER_BALL) ? 800 : 500);
			glare3.moveOpacity(delay + (particle_duration / 2), particle_duration / 3, 0);
		}
		new {glare1, glare2, glare3}.each(p => p.setVisible(delay + particle_duration, false));
		// Burst particles
		for (int i = num_particles; i < num_particles; i++) { //for 'num_particles' times do => |i|
			// Set up particle that keeps moving out
			particle1 = addNewSprite(ballX, ballY, "Graphics/Battle animations/ballBurst_particle", PictureOrigin.CENTER);
			particle1.setZ(0, 105);
			particle1.setZoom(0, 150);
			particle1.setOpacity(0, 160);
			particle1.setVisible(0, false);
			// Set up particles that curve back in
			particle2 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[12]}", PictureOrigin.CENTER);
			particle3 = addNewSprite(ballX, ballY, $"Graphics/Battle animations/ballBurst_{variances[9]}", PictureOrigin.CENTER);
			new {particle2, particle3}.each_with_index do |particle, num|
				particle.setZ(0, 110 + num);
				particle.setZoom(0, (poke_ball == Items.NEST_BALL) ? 50 : 0);
				particle.setTone(0, variances[13 - (3 * num)]);
				particle.setVisible(0, false);
				if (poke_ball == Items.PREMIER_BALL) particle.setAngle(0, rand(360));
			}
			if (poke_ball == Items.DIVE_BALL) particle3.setOpacity(0, 128);
			// Particle animations
			new {particle1, particle2, particle3}.each(p => p.setVisible(delay, true));
			if (poke_ball == Items.NEST_BALL) particle2.setVisible(delay, false);
			start_angle = base_angle + (i * 360 / num_particles);
			p1_x_offset = base_radius * Math.cos(start_angle * Math.PI / 180);
			p1_y_offset = base_radius * Math.sin(start_angle * Math.PI / 180);
			for (int j = particle_duration; j < particle_duration; j++) { //for 'particle_duration' times do => |j|
				index = j + 1;
				angle = start_angle + (index * (360 / num_particles) / particle_duration);
				radian = angle * Math.PI / 180;
				radius = base_radius;
				prop = index.to_f / (particle_duration / 2);
				if (index > particle_duration / 2) prop = 2 - prop;
				radius *= prop;
				particle1.moveXY(delay + j, 1,
												ballX + (p1_x_offset * index * 2 / particle_duration),
												ballY - (p1_y_offset * index * 2 / particle_duration));
				new {particle2, particle3}.each do |particle|
					particle.moveXY(delay + j, 1,
													ballX + (radius * Math.cos(radian)),
													ballY - (radius * Math.sin(radian)));
				}
			}
			particle1.moveZoom(delay, particle_duration, 0);
			particle1.moveOpacity(delay, particle_duration, 0);
			new {particle2, particle3}.each_with_index do |particle, num|
				// Zoom in
				if (num == 0 && poke_ball == Items.MASTER_BALL) {
					particle.moveZoom(delay, particle_duration / 2, 225);
				} else if (num == 0 && poke_ball == Items.DIVE_BALL) {
					particle.moveZoom(delay, particle_duration / 2, 125);
				} else if (["particle"].Contains(variances[12 - (3 * num)])) {
					particle.moveZoom(delay, particle_duration / 2, (poke_ball == Items.PREMIER_BALL) ? 50 : 80);
				} else if (["ring3"].Contains(variances[12 - (3 * num)])) {
					particle.moveZoom(delay, particle_duration / 2, 50);
				} else if (new []{"dazzle", "ring4", "diamond"}.Contains(variances[12 - (3 * num)])) {
					particle.moveZoom(delay, particle_duration / 2, 60);
				} else {
					particle.moveZoom(delay, particle_duration / 2, 100);
				}
				// Zoom out
				if (new []{"particle", "dazzle", "ring3", "ring4", "diamond"}.Contains(variances[12 - (3 * num)])) {
					particle.moveZoom(delay + (particle_duration * 2 / 3), particle_duration / 3, 10);
				} else {
					particle.moveZoom(delay + (particle_duration * 2 / 3), particle_duration / 3, 25);
				}
				// Rotate (for Premier Ball)
				if (poke_ball == Items.PREMIER_BALL) particle.moveAngle(delay, particle_duration, -180);
				// Change tone, fade out
				particle.moveTone(delay + (particle_duration / 3), (particle_duration.to_f / 3).ceil, variances[14 - (3 * num)]);
				particle.moveOpacity(delay + particle_duration - 3, 3, 128);   // Fade out at end
			}
			new {particle1, particle2, particle3}.each(p => p.setVisible(delay + particle_duration, false));
		}
		// Web sprite (for Net Ball)
		if (poke_ball == Items.NET_BALL) {
			web = addNewSprite(ballX, ballY, "Graphics/Battle animations/ballBurst_web", PictureOrigin.CENTER);
			web.setZ(0, 123);
			web.setZoom(0, 120);
			web.setOpacity(0, 0);
			web.setTone(0, new Tone(-32, -32, -128));
			web.setVisible(0, false);
			start = particle_duration / 2;
			web.setVisible(delay + start, true);
			web.moveOpacity(delay + start, 2, 160);
			web_duration = particle_duration + ring_duration - (particle_duration / 2);
			for (int i = (web_duration / 4); i < (web_duration / 4); i++) { //for '(web_duration / 4)' times do => |i|
				web.moveZoom(delay + start + (i * 4), 2, 150);
				web.moveZoom(delay + start + (i * 4) + 2, 2, 120);
			}
			now = start + ((web_duration / 4) * 4);
			web.moveZoom(delay + now, particle_duration + ring_duration - now, 150);
			web.moveOpacity(delay + particle_duration, ring_duration, 0);
			web.setVisible(delay + particle_duration + ring_duration, false);
		}
		// Ring particle
		ring = addNewSprite(ballX, ballY, "Graphics/Battle animations/ballBurst_ring1", PictureOrigin.CENTER);
		ring.setZ(0, 110);
		ring.setZoom(0, 0);
		ring.setTone(0, variances[15]);
		ring.setVisible(0, false);
		// Ring particle animation
		ring.setVisible(delay + particle_duration, true);
		ring.moveZoom(delay + particle_duration - 2, ring_duration + 2, 125);   // Start slightly early
		ring.moveTone(delay + particle_duration, ring_duration, variances[16]);
		ring.moveOpacity(delay + particle_duration, ring_duration, 0);
		ring.setVisible(delay + particle_duration + ring_duration, false);
		// Mark the end of the burst animation
		ball.setDelta(delay + particle_duration + ring_duration, 0, 0);
	}

	// The animation shown over a thrown Poké Ball when it has successfully caught
	// a Pokémon.
	public void ballCaptureSuccess(ball, delay, ballX, ballY) {
		ball.setSE(delay, "Battle catch click");
		ball.moveTone(delay, 4, new Tone(-128, -128, -128));   // Ball goes darker
		delay = ball.totalDuration;
		star_duration = 12;   // In 20ths of a second
		y_offsets = new {new {0, 74, 52}, new {0, 62, 28}, new {0, 74, 48}}
		for (int i = 3; i < 3; i++) { //for '3' times do => |i|   // Left, middle, right
			// Set up particle
			star = addNewSprite(ballX, ballY, "Graphics/Battle animations/ballBurst_star", PictureOrigin.CENTER);
			star.setZ(0, 110);
			star.setZoom(0, new {50, 50, 33}[i]);
			start_angle = new {0, 345, 15}[i];
			star.setAngle(0, start_angle);
			star.setOpacity(0, 0);
			star.setVisible(0, false);
			// Particle animation
			star.setVisible(delay, true);
			y_pos = y_offsets[i];
			for (int j = star_duration; j < star_duration; j++) { //for 'star_duration' times do => |j|
				index = j + 1;
				x = 72 * index / star_duration;
				proportion = index.to_f / star_duration;
				a = (2 * y_pos[2]) - (4 * y_pos[1]);
				b = y_pos[2] - a;
				y = ((a * proportion) + b) * proportion;
				star.moveXY(delay + j, 1, ballX + (new {-1, 0, 1}[i] * x), ballY - y);
			}
			if (i.even()) star.moveAngle(delay, star_duration, start_angle + new {144, 0, 45}[i]);
			star.moveOpacity(delay, 4, 255);   // Fade in
			star.moveTone(delay + 3, 3, new Tone(0, 0, -96));   // Light yellow
			star.moveTone(delay + 6, 3, new Tone(0, 0, 0));   // White
			star.moveOpacity(delay + 8, 4, 0);   // Fade out
		}
	}

	// The Poké Ball burst animation used when recalling a Pokémon. In HGSS, this
	// is the same for all types of Poké Ball except for the color that the battler
	// turns - see def getBattlerColorFromPokeBall.
	public void ballBurstRecall(delay, ball, ballX, ballY, poke_ball) {
		color_duration = 10;   // Change color of battler to a solid shade - see def battlerAbsorb
		shrink_duration = 5;   // Shrink battler into Poké Ball - see def battlerAbsorb
		burst_duration = color_duration + shrink_duration;
		// Burst particles
		num_particles = 5;
		base_angle = 55;
		base_radius = 64;   // How far out from the Poké Ball the particles go
		for (int i = num_particles; i < num_particles; i++) { //for 'num_particles' times do => |i|
			// Set up particle
			particle = addNewSprite(ballX, ballY, "Graphics/Battle animations/ballBurst_particle", PictureOrigin.CENTER);
			particle.setZ(0, 110);
			particle.setZoom(0, 150);
			particle.setOpacity(0, 0);
			particle.setVisible(0, false);
			// Particle animation
			particle.setVisible(delay, true);
			particle.moveOpacity(delay, 2, 255);   // Fade in quickly
			for (int j = burst_duration; j < burst_duration; j++) { //for 'burst_duration' times do => |j|
				angle = base_angle + (i * 360 / num_particles) + (135.0 * j / burst_duration);
				radian = angle * Math.PI / 180;
				radius = base_radius;
				if (j < burst_duration / 5) {
					prop = j.to_f / (color_duration / 3);
					radius *= 0.75 + (prop / 4);
				} else if (j >= burst_duration / 2) {
					prop = (j.to_f - (burst_duration / 2)) / (burst_duration / 2);
					radius *= 1 - prop;
				}
				if (j == 0) {
					particle.setXY(delay + j, ballX + (radius * Math.cos(radian)), ballY - (radius * Math.sin(radian)));
				} else {
					particle.moveXY(delay + j, 1, ballX + (radius * Math.cos(radian)), ballY - (radius * Math.sin(radian)));
				}
			}
			particle.moveZoom(delay, burst_duration, 0);
			particle.moveTone(delay + (color_duration / 2), color_duration / 2, new Tone(0, 0, -192));   // Yellow
			particle.moveTone(delay + color_duration, shrink_duration, new Tone(0, -128, -248));   // Dark orange
			particle.moveOpacity(delay + color_duration, shrink_duration, 0);   // Fade out at end
			particle.setVisible(delay + burst_duration, false);
		}
		// Ring particles
		ring1 = addNewSprite(ballX, ballY, "Graphics/Battle animations/ballBurst_ring1", PictureOrigin.CENTER);
		ring1.setZ(0, 110);
		ring1.setZoom(0, 0);
		ring1.setVisible(0, false);
		ring2 = addNewSprite(ballX, ballY, "Graphics/Battle animations/ballBurst_ring2", PictureOrigin.CENTER);
		ring2.setZ(0, 110);
		ring2.setVisible(0, false);
		// Ring particle animations
		ring1.setVisible(delay + burst_duration - 2, true);
		ring1.moveZoom(delay + burst_duration - 2, 4, 100);
		ring1.setVisible(delay + burst_duration + 2, false);
		ring2.setVisible(delay + burst_duration + 2, true);
		ring2.moveZoom(delay + burst_duration + 2, 4, 200);
		ring2.moveOpacity(delay + burst_duration + 2, 4, 0);
	}
}
