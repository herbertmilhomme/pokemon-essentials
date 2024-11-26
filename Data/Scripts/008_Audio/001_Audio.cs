//===============================================================================
// Methods that determine the duration of an audio file.
//===============================================================================
public void getOggPage(file) {
	fgetdw = block: (f) => {
		(f.eof() ? 0 : (f.read(4).unpack("V")[0] || 0))
	}
	dw = fgetdw.call(file);
	if (dw != 0x5367674F) return null;
	header = file.read(22);
	bodysize = 0;
	hdrbodysize = (file.read(1)[0].ord rescue 0);
	hdrbodysize.times do;
		bodysize += (file.read(1)[0].ord rescue 0);
	}
	ret = new {header, file.pos, bodysize, file.pos + bodysize};
	return ret;
}

// internal function
public void oggfiletime(file) {
	fgetdw = block: (f) => {
		(f.eof() ? 0 : (f.read(4).unpack("V")[0] || 0))
	}
	pages = new List<string>();
	page = null;
	do { //loop; while (true);
		page = getOggPage(file);
		if (!page) break;
		pages.Add(page);
		file.pos = page[3];
	}
	if (pages.length == 0) return -1;
	curserial = null;
	i = -1;
	pcmlengths = new List<string>();
	rates = new List<string>();
	foreach (var pg in pages) { //'pages.each' do => |pg|
		header = pg[0];
		serial = header[10, 4].unpack("V");
		frame = header[2, 8].unpack("C*");
		frameno = frame[7];
		frameno = (frameno << 8) | frame[6];
		frameno = (frameno << 8) | frame[5];
		frameno = (frameno << 8) | frame[4];
		frameno = (frameno << 8) | frame[3];
		frameno = (frameno << 8) | frame[2];
		frameno = (frameno << 8) | frame[1];
		frameno = (frameno << 8) | frame[0];
		if (serial != curserial) {
			curserial = serial;
			file.pos = pg[1];
			packtype = (file.read(1)[0].ord rescue 0);
			string = file.read(6);
			if (string != "vorbis") return -1;
			if (packtype != 1) return -1;
			i += 1;
			version = fgetdw.call(file);
			if (version != 0) return -1;
			rates[i] = fgetdw.call(file);
		}
		pcmlengths[i] = frameno;
	}
	ret = 0.0;
	pcmlengths.each_with_index((length, j) => ret += length.to_f / rates[j]);
	return ret * 256.0;
}

// Gets the length of an audio file in seconds. Supports WAV, MP3, and OGG files.
public void getPlayTime(filename) {
	if (FileTest.exist(filename)) {
		return (int)Math.Max(getPlayTime2(filename), 0);
	} else if (FileTest.exist(filename + ".wav")) {
		return (int)Math.Max(getPlayTime2(filename + ".wav"), 0);
	} else if (FileTest.exist(filename + ".mp3")) {
		return (int)Math.Max(getPlayTime2(filename + ".mp3"), 0);
	} else if (FileTest.exist(filename + ".ogg")) {
		return (int)Math.Max(getPlayTime2(filename + ".ogg"), 0);
	}
	return 0;
}

public void getPlayTime2(filename) {
	if (!FileTest.exist(filename)) return -1;
	time = -1;
	fgetdw = block: (file) => {
		(file.eof() ? 0 : (file.read(4).unpack("V")[0] || 0))
	}
	fgetw = block: (file) => {
		(file.eof() ? 0 : (file.read(2).unpack("v")[0] || 0))
	}
	File.open(filename, "rb") do |file|
		file.pos = 0;
		fdw = fgetdw.call(file);
		switch (fdw) {
			case 0x46464952:   // "RIFF"
				filesize = fgetdw.call(file);
				wave = fgetdw.call(file);
				if (wave != 0x45564157) return -1;   // "WAVE"
				fmt = fgetdw.call(file);
				if (fmt != 0x20746d66) return -1;   // "fmt "
				fgetdw.call(file);   // fmtsize
				fgetw.call(file);   // format
				fgetw.call(file);   // channels
				fgetdw.call(file);   // rate
				bytessec = fgetdw.call(file);
				if (bytessec == 0) return -1;
				fgetw.call(file);   // bytessample
				fgetw.call(file);   // bitssample
				data = fgetdw.call(file);
				if (data != 0x61746164) return -1;   // "data"
				datasize = fgetdw.call(file);
				time = datasize.to_f / bytessec;
				return time;
			case 0x5367674F:   // "OggS"
				file.pos = 0;
				time = oggfiletime(file);
				return time;
		}
		file.pos = 0;
		// Find the length of an MP3 file
		do { //loop; while (true);
			rstr = "";
			ateof = false;
			until file.eof();
				if ((file.read(1)[0] rescue 0) == 0xFF) {
					begin;
						rstr = file.read(3);
					rescue;
						ateof = true;
					}
					break;
				}
			}
			if (ateof || !rstr || rstr.length != 3) break;
			if (rstr[0] == 0xFB) {
				t = rstr[1] >> 4;
				if (new []{0, 15}.Contains(t)) continue;
				freqs = new {44_100, 22_050, 11_025, 48_000};
				bitrates = new {32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320};
				bitrate = bitrates[t];
				t = (rstr[1] >> 2) & 3;
				freq = freqs[t];
				t = (rstr[1] >> 1) & 1;
				filesize = FileTest.size(filename);
				frameLength = ((144_000 * bitrate) / freq) + t;
				numFrames = filesize / (frameLength + 4);
				time = (numFrames * 1152.0 / freq);
				break;
			}
		}
	}
	return time;
}
