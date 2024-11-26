//===============================================================================
//
//===============================================================================
public static partial class BattleAnimationEditor {
	#region Class Functions
	#endregion

	//=============================================================================
	// Paths and interpolation.
	//=============================================================================
	public partial class ControlPointSprite : Sprite {
		public int dragging		{ get { return _dragging; } set { _dragging = value; } }			protected int _dragging;

		public override void initialize(red, viewport = null) {
			base.initialize(viewport);
			self.bitmap = new Bitmap(6, 6);
			self.bitmap.fill_rect(0, 0, 6, 1, Color.black);
			self.bitmap.fill_rect(0, 0, 1, 6, Color.black);
			self.bitmap.fill_rect(0, 5, 6, 1, Color.black);
			self.bitmap.fill_rect(5, 0, 1, 6, Color.black);
			color = (red) ? new Color(255, 0, 0) : Color.black;
			self.bitmap.fill_rect(2, 2, 2, 2, color);
			self.x = -6;
			self.y = -6;
			self.visible = false;
			@dragging = false;
		}

		public void mouseover() {
			if (Input.time(Input.MOUSELEFT) == 0 || !@dragging) {
				@dragging = false;
				return;
			}
			mouse = Mouse.getMousePos(true);
			if (!mouse) return;
			self.x = (int)Math.Min((int)Math.Max(mouse[0], 0), 512);
			self.y = (int)Math.Min((int)Math.Max(mouse[1], 0), 384);
		}

		public bool hittest() {
			if (!self.visible) return true;
			mouse = Mouse.getMousePos(true);
			if (!mouse) return false;
			return mouse[0] >= self.x && mouse[0] < self.x + 6 &&
						mouse[1] >= self.y && mouse[1] < self.y + 6;
		}

		public void inspect() {
			return $"new {{self.x},{self.y}}";
		}

		public override void dispose() {
			self.bitmap.dispose;
			base.dispose();
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class PointSprite : Sprite {
		public override void initialize(x, y, viewport = null) {
			base.initialize(viewport);
			self.bitmap = new Bitmap(2, 2);
			self.bitmap.fill_rect(0, 0, 2, 2, Color.black);
			self.x = x;
			self.y = y;
		}

		public override void dispose() {
			self.bitmap.dispose;
			base.dispose();
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class PointPath {
		include Enumerable;

		public void initialize() {
			@points = new List<string>();
			@distances = new List<string>();
			@totaldist = 0;
		}

		public int this[int x] { get {
			return @points[x].clone;
			}
		}

		public void each() {
			@points.each(o => yield o.clone);
		}

		public void size() {
			return @points.size;
		}

		public void length() {
			return @points.length;
		}

		public void totalDistance() {
			return @totaldist;
		}

		public void inspect() {
			p = new List<string>();
			@points.each do |point|
				p.Add(new {point[0].ToInt(), point[1].ToInt()});
			}
			return p.inspect;
		}

		public bool isEndPoint(x, y) {
			if (@points.length == 0) return false;
			index = @points.length - 1;
			return @points[index][0] == x &&
						@points[index][1] == y;
		}

		public void addPoint(x, y) {
			@points.Add(new {x, y});
			if (@points.length > 1) {
				len = @points.length;
				dx = @points[len - 2][0] - @points[len - 1][0];
				dy = @points[len - 2][1] - @points[len - 1][1];
				dist = Math.sqrt((dx * dx) + (dy * dy));
				@distances.Add(dist);
				@totaldist += dist;
			}
		}

		public void clear() {
			@points.clear;
			@distances.clear;
			@totaldist = 0;
		}

		public void smoothPointPath(frames, roundValues = false) {
			if (frames < 0) raise new ArgumentError($"frames out of range: {frames}");
			ret = new PointPath();
			if (@points.length == 0) return ret;
			step = 1.0 / frames;
			t = 0.0;
			(frames + 2).times do
				point = pointOnPath(t);
				if (roundValues) {
					ret.addPoint(point[0].round, point[1].round);
				} else {
					ret.addPoint(point[0], point[1]);
				}
				t += step;
				t = (int)Math.Min(1.0, t);
			}
			return ret;
		}

		public void pointOnPath(t) {
			if (t < 0 || t > 1) {
				Debug.LogError(new ArgumentError($"t out of range for pointOnPath: {t}"));
				//throw new Exception(new ArgumentError($"t out of range for pointOnPath: {t}"));
			}
			if (@points.length == 0) return null;
			ret = @points[@points.length - 1].clone;
			if (@points.length == 1) return ret;
			curdist = 0;
			distForT = @totaldist * t;
			i = 0;
			@distances.each do |dist|
				curdist += dist;
				if (dist > 0.0 && curdist >= distForT) {
					distT = 1.0 - ((curdist - distForT) / dist);
					dx = @points[i + 1][0] - @points[i][0];
					dy = @points[i + 1][1] - @points[i][1];
					ret = new {@points[i][0] + (dx * distT),
								@points[i][1] + (dy * distT)};
					break;
				}
				i += 1;
			}
			return ret;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public void catmullRom(p1, p2, p3, p4, t) {
		// p1=prevPoint, p2=startPoint, p3=endPoint, p4=nextPoint, t is from 0 through 1
		t2 = t * t;
		t3 = t2 * t;
		return 0.5 * ((2 * p2) + (t * (p3 - p1)) +;
					(t2 * ((2 * p1) - (5 * p2) + (4 * p3) - p4)) +
					(t3 * (p4 - (3 * p3) + (3 * p2) - p1)))
	}

	public void getCatmullRomPoint(src, t) {
		x = 0, y = 0;
		t *= 3.0;
		if (t < 1.0) {
			x = catmullRom(src[0].x, src[0].x, src[1].x, src[2].x, t);
			y = catmullRom(src[0].y, src[0].y, src[1].y, src[2].y, t);
		} else if (t < 2.0) {
			t -= 1.0;
			x = catmullRom(src[0].x, src[1].x, src[2].x, src[3].x, t);
			y = catmullRom(src[0].y, src[1].y, src[2].y, src[3].y, t);
		} else {
			t -= 2.0;
			x = catmullRom(src[1].x, src[2].x, src[3].x, src[3].x, t);
			y = catmullRom(src[1].y, src[2].y, src[3].y, src[3].y, t);
		}
		return new {x, y};
	}

	public void getCurvePoint(src, t) {
		return getCatmullRomPoint(src, t);
	}

	public void curveToPointPath(curve, numpoints) {
		if (numpoints < 2) return null;
		path = new PointPath();
		step = 1.0 / (numpoints - 1);
		t = 0.0;
		numpoints.times do;
			point = getCurvePoint(curve, t);
			path.addPoint(point[0], point[1]);
			t += step;
		}
		return path;
	}

	public void DefinePath(canvas) {
		sliderwin2 = new ControlWindow(0, 0, 320, 320);
		sliderwin2.viewport = canvas.viewport;
		sliderwin2.addSlider(_INTL("Number of frames:"), 2, 500, 20);
		sliderwin2.opacity = 200;
		defcurvebutton = sliderwin2.addButton(_INTL("Define Smooth Curve"));
		defpathbutton = sliderwin2.addButton(_INTL("Define Freehand Path"));
		okbutton = sliderwin2.addButton(_INTL("OK"));
		cancelbutton = sliderwin2.addButton(_INTL("Cancel"));
		points = new List<string>();
		path = null;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			sliderwin2.update;
			if (sliderwin2.changed(0)) {   // Number of frames
				if (path) {
					path = path.smoothPointPath(sliderwin2.value(0), false);
					i = 0;
					foreach (var point in path) { //'path.each' do => |point|
						if (i < points.length) {
							points[i].x = point[0];
							points[i].y = point[1];
						} else {
							points.Add(new PointSprite(point[0], point[1], canvas.viewport));
						}
						i += 1;
					}
					for (int j = i; j < points.length; j++) { //each 'points.length' do => |j|
						points[j].dispose;
						points[j] = null;
					}
					points.compact!;
				}
			} else if (sliderwin2.changed(defcurvebutton)) {
				foreach (var point in points) { //'points.each' do => |point|
					point.dispose;
				}
				points.clear;
				30.times do;
					point = new PointSprite(0, 0, canvas.viewport);
					point.visible = false;
					points.Add(point);
				}
				curve = new {
					new ControlPointSprite(true, canvas.viewport),
					new ControlPointSprite(false, canvas.viewport),
					new ControlPointSprite(false, canvas.viewport),
					new ControlPointSprite(true, canvas.viewport);
				}
				showline = false;
				sliderwin2.visible = false;
				// This window displays the mouse's current position
				window = Window_UnformattedTextPokemon.newWithSize(
					"", 0, 320 - 64, 128, 64, canvas.viewport
				);
				do { //loop; while (true);
					Graphics.update;
					Input.update;
					if (Input.trigger(Input.BACK)) {
						break;
					}
					if (Input.trigger(Input.MOUSELEFT)) {
						for (int j = 4; j < 4; j++) { //for '4' times do => |j|
							if (!curve[j].hittest()) continue;
							if (new []{1, 2}.Contains(j) && (!curve[0].visible || !curve[3].visible)) {
								continue;
							}
							curve[j].visible = true;
							for (int k = 4; k < 4; k++) { //for '4' times do => |k|
								curve[k].dragging = (k == j);
							}
							break;
						}
					}
					for (int j = 4; j < 4; j++) { //for '4' times do => |j|
						curve[j].mouseover;
					}
					mousepos = Mouse.getMousePos(true);
					newtext = (mousepos) ? string.Format("({0},{0})", mousepos[0], mousepos[1]) : "(??,??)";
					if (window.text != newtext) window.text = newtext;
					if (curve[0].visible && curve[3].visible &&
						!curve[0].dragging && !curve[3].dragging) {
						foreach (var point in points) { //'points.each' do => |point|
							point.visible = true;
						}
						if (!showline) {
							curve[1].visible = true;
							curve[2].visible = true;
							curve[1].x = curve[0].x + (0.3333 * (curve[3].x - curve[0].x));
							curve[1].y = curve[0].y + (0.3333 * (curve[3].y - curve[0].y));
							curve[2].x = curve[0].x + (0.6666 * (curve[3].x - curve[0].x));
							curve[2].y = curve[0].y + (0.6666 * (curve[3].y - curve[0].y));
						}
						showline = true;
					}
					if (showline) {
						step = 1.0 / (points.length - 1);
						t = 0.0;
						for (int j = points.length; j < points.length; j++) { //for 'points.length' times do => |j|
							point = getCurvePoint(curve, t);
							points[j].x = point[0];
							points[j].y = point[1];
							t += step;
						}
					}
				}
				window.dispose;
				// dispose temporary path
				foreach (var point in points) { //'points.each' do => |point|
					point.dispose;
				}
				points.clear;
				if (showline) {
					path = curveToPointPath(curve, sliderwin2.value(0));
//					File.open("pointpath.txt", "wb", f => { f.write(path.inspect); });
					foreach (var point in path) { //'path.each' do => |point|
						points.Add(new PointSprite(point[0], point[1], canvas.viewport));
					}
				}
				foreach (var point in curve) { //'curve.each' do => |point|
					point.dispose;
				}
				sliderwin2.visible = true;
				continue;
			} else if (sliderwin2.changed(defpathbutton)) {
				canceled = false;
				pointpath = new PointPath();
				foreach (var point in points) { //'points.each' do => |point|
					point.dispose;
				}
				points.clear;
				window = Window_UnformattedTextPokemon.newWithSize(
					"", 0, 320 - 64, 128, 64, canvas.viewport
				);
				sliderwin2.visible = false;
				do { //loop; while (true);
					Graphics.update;
					Input.update;
					if (Input.triggerex(:ESCAPE)) {
						canceled = true;
						break;
					}
					if (Input.trigger(Input.MOUSELEFT)) {
						break;
					}
					mousepos = Mouse.getMousePos(true);
					window.text = (mousepos) ? string.Format("({0},{0})", mousepos[0], mousepos[1]) : "(??,??)";
				}
				until canceled;
					mousepos = Mouse.getMousePos(true);
					if (mousepos && !pointpath.isEndPoint(mousepos[0], mousepos[1])) {
						pointpath.addPoint(mousepos[0], mousepos[1]);
						points.Add(new PointSprite(mousepos[0], mousepos[1], canvas.viewport));
					}
					window.text = (mousepos) ? string.Format("({0},{0})", mousepos[0], mousepos[1]) : "(??,??)";
					Graphics.update;
					Input.update;
					if (Input.triggerex(:ESCAPE) || Input.time(Input.MOUSELEFT) == 0) {
						break;
					}
				}
				window.dispose;
				// dispose temporary path
				foreach (var point in points) { //'points.each' do => |point|
					point.dispose;
				}
				points.clear;
				// generate smooth path from temporary path
				path = pointpath.smoothPointPath(sliderwin2.value(0), true);
				// redraw path from smooth path
				foreach (var point in path) { //'path.each' do => |point|
					points.Add(new PointSprite(point[0], point[1], canvas.viewport));
				}
//				File.open("pointpath.txt", "wb", f => { f.write(path.inspect); });
				sliderwin2.visible = true;
				continue;
			} else if (sliderwin2.changed(okbutton) && path) {
//				File.open("pointpath.txt", "wb", f => { f.write(path.inspect); });
				neededsize = canvas.currentframe + sliderwin2.value(0);
				if (neededsize > canvas.animation.length) {
					canvas.animation.resize(neededsize);
				}
				thiscel = canvas.currentCel;
				celnumber = canvas.currentcel;
				(canvas.currentframe...neededsize).each do |j|
					cel = canvas.animation[j][celnumber];
					if (!canvas.animation[j][celnumber]) {
						cel = CreateCel(0, 0, thiscel[AnimFrame.PATTERN], canvas.animation.position);
						canvas.animation[j][celnumber] = cel;
					}
					cel[AnimFrame.X] = path[j - canvas.currentframe][0];
					cel[AnimFrame.Y] = path[j - canvas.currentframe][1];
				}
				break;
			} else if (sliderwin2.changed(cancelbutton) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		// dispose all points
		foreach (var point in points) { //'points.each' do => |point|
			point.dispose;
		}
		points.clear;
		sliderwin2.dispose;
		return;
	}
}
