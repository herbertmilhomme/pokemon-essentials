//===============================================================================
// Text colors.
//===============================================================================
// Unused
public void ctag(color) {
	return string.Format("<c={0}>", color.to_rgb32(true));
}

// Unused
public void shadowctag(base, shadow) {
	return string.Format("<c2={0}{0}>", base.to_rgb15, shadow.to_rgb15);
}

// base and shadow are either instances of class Color, or are arrays containing
// 3 or 4 integers which are RGB(A) values.
public void shadowc3tag(base, shadow) {
	if (base.is_a(Color)) {
		base_text = base.to_rgb32;
	} else {
		base_text = string.Format("{0:X2}{0:X2}{0:X2}", base[0], base[1], base[2]);
		if (base[3]) base_text += string.Format("{0:X2}", base[3]);
	}
	if (shadow.is_a(Color)) {
		shadow_text = shadow.to_rgb32;
	} else {
		shadow_text = string.Format("{0:X2}{0:X2}{0:X2}", shadow[0], shadow[1], shadow[2]);
		if (shadow[3]) shadow_text += string.Format("{0:X2}", shadow[3]);
	}
	return string.Format("<c3={0},{0}>", base_text, shadow_text);
}

// Unused
public void shadowctagFromColor(color) {
	return shadowc3tag(color, color.get_contrast_color);
}

// Unused
public void shadowctagFromRgb(param) {
	return shadowctagFromColor(Color.new_from_rgb(param));
}

//===============================================================================
// Format text.
//===============================================================================
FORMATREGEXP = @"<(\/?)(c|c2|c3|o|fn|br|fs|i|b|r|pg|pog|u|s|icon|img|ac|ar|al|outln|outln2)(\s*\=\s*([^>]*))?>"; // make case-insensitive

public void fmtEscape(text) {
	if (System.Text.RegularExpressions.Regex.IsMatch(text,@"[&<>]")) {
		text2 = System.Text.RegularExpressions.Regex.Replace(text, "&", "&amp;");
		text2 = System.Text.RegularExpressions.Regex.Replace(text2, "<", "&lt;");
		text2 = System.Text.RegularExpressions.Regex.Replace(text2, ">", "&gt;");
		return text2;
	}
	return text;
}

// Modifies text; does not return a modified copy of it.
public void fmtReplaceEscapes(text) {
	text = System.Text.RegularExpressions.Regex.Replace(text, "&lt;", "<");
	text = System.Text.RegularExpressions.Regex.Replace(text, "&gt;", ">");
	text = System.Text.RegularExpressions.Regex.Replace(text, "&apos;", "'");
	text = System.Text.RegularExpressions.Regex.Replace(text, "&quot;", "\"");
	text = System.Text.RegularExpressions.Regex.Replace(text, "&amp;", "&");
	text = System.Text.RegularExpressions.Regex.Replace(text, "&m;", "♂");
	text = System.Text.RegularExpressions.Regex.Replace(text, "&f;", "♀");
}

public void toUnformattedText(text) {
	text2 = text.gsub(FORMATREGEXP, "");
	fmtReplaceEscapes(text2);
	return text2;
}

public void unformattedTextLength(text) {
	return toUnformattedText(text).scan(/./m).length;
}

public void itemIconTag(item) {
	if (!item) return "";
	if (item.respond_to("icon_name")) {
		return string.Format("<icon={0}>", item.icon_name);
	} else {
		ix = (item.icon_index % 16) * 24;
		iy = (item.icon_index / 16) * 24;
		return string.Format("<img=Graphics/System/Iconset|{0}|{0}|24|24>", ix, iy);
	}
}

public void getFormattedTextForDims(bitmap, xDst, yDst, widthDst, heightDst, text, lineheight,
														newlineBreaks = true, explicitBreaksOnly = false) {
	text2 = System.Text.RegularExpressions.Regex.Replace(text, "<(\/?)(c|c2|c3|o|u|s)(\s*\=\s*([^>]*))?>", "",RegexOptions.IgnoreCase);
	if (newlineBreaks) text2 = System.Text.RegularExpressions.Regex.Replace(text2, "<(\/?)(br)(\s*\=\s*([^>]*))?>", "\n",RegexOptions.IgnoreCase);
	return getFormattedText(bitmap, xDst, yDst, widthDst, heightDst,
													text2, lineheight, newlineBreaks,
													explicitBreaksOnly, true);
}

public void getFormattedTextFast(bitmap, xDst, yDst, widthDst, heightDst, text, lineheight,
												newlineBreaks = true, explicitBreaksOnly = false) {
	x = y = 0;
	characters = new List<string>();
	textchunks = new List<string>();
	textchunks.Add(text);
	text = textchunks.join;
	textchars = text.scan(/./m);
	lastword = new {0, 0}; // position of last word
	hadspace = false;
	hadnonspace = false;
	bold = bitmap.font.bold;
	italic = bitmap.font.italic;
	colorclone = bitmap.font.color;
	defaultfontname = bitmap.font.name;
	if (defaultfontname.Length > 0) {
		defaultfontname = defaultfontname.find(i => Font.exist(i)) || "Arial";
	} else if (!Font.exist(defaultfontname)) {
		defaultfontname = "Arial";
	}
	defaultfontname = defaultfontname.clone;
	havenl = false;
	position = 0;
	while (position < textchars.length) {
		yStart = 0;
		xStart = 0;
		width = isWaitChar(textchars[position]) ? 0 : bitmap.text_size(textchars[position]).width;
		if (textchars[position] == "\n") {
			if (newlineBreaks) {   // treat newline as break
				havenl = true;
				characters.Add(new {"\n", x, (y * lineheight) + yDst, 0, lineheight, false, false,
												false, colorclone, null, false, false, "", 8, position, null, 0});
				y += 1;
				x = 0;
				hadspace = true;
				hadnonspace = false;
				position += 1;
				continue;
			} else {   // treat newline as space
				textchars[position] = " ";
			}
		}
		isspace = (System.Text.RegularExpressions.Regex.IsMatch(textchars[position],@"\s") || isWaitChar(textchars[position])) ? true : false;
		if (hadspace && !isspace) {
			// set last word to here
			lastword[0] = characters.length;
			lastword[1] = x;
			hadspace = false;
			hadnonspace = true;
		} else if (isspace) {
			hadspace = true;
		}
		texty = (lineheight * y) + yDst + yStart;
		// Push character
		if (heightDst < 0 || yStart < yDst + heightDst) {
			if (isWaitChar(textchars[position])) havenl = true;
			characters.Add(new {textchars[position],
											x + xStart, texty, width + 2, lineheight,
											false, bold, italic, colorclone, null, false, false,
											defaultfontname, bitmap.font.size, position, null, 0});
		}
		x += width;
		if (!explicitBreaksOnly && x + 2 > widthDst && lastword[1] != 0 &&
			(!hadnonspace || !hadspace)) {
			havenl = true;
			characters.insert(lastword[0], new {"\n", x, (y * lineheight) + yDst, 0, lineheight,
																			false, false, false, colorclone, null, false, false, "", 8, position});
			lastword[0] += 1;
			y += 1;
			x = 0;
			for (int i = lastword[0]; i < characters.length; i++) { //each 'characters.length' do => |i|
				characters[i][2] += lineheight;
				charwidth = characters[i][3] - 2;
				characters[i][1] = x;
				x += charwidth;
			}
			lastword[1] = 0;
		}
		position += 1;
	}
	// Eliminate spaces before newlines and pause character
	if (havenl) {
		firstspace = -1;
		for (int i = characters.length; i < characters.length; i++) { //for 'characters.length' times do => |i|
			if (characters[i][5] != false) { // If not a character
				firstspace = -1;
			} else if ((characters[i][0] == "\n" || isWaitChar(characters[i][0])) &&
						firstspace >= 0) {
				for (int j = firstspace; j < i; j++) { //each 'i' do => |j|
					characters[j] = null;
				}
				firstspace = -1;
			} else if (System.Text.RegularExpressions.Regex.IsMatch(characters[i][0],@"[ \r\t]")) {
				if (firstspace < 0) firstspace = i;
			} else {
				firstspace = -1;
			}
		}
		if (firstspace > 0) {
			for (int j = firstspace; j < characters.length; j++) { //each 'characters.length' do => |j|
				characters[j] = null;
			}
		}
		characters.compact!;
	}
	characters.each(char => char[1] = xDst + char[1]);
	// Remove all characters with Y greater or equal to _yDst_+_heightDst_
	if (heightDst >= 0) {
		characters.each_with_index do |char, i|
			if (char[2] >= yDst + heightDst) characters[i] = null;
		}
		characters.compact!;
	}
	return characters;
}

public void isWaitChar(x) {
	return (new []{"\001", "\002"}.Contains(x));
}

public void getLastParam(array, default) {
	i = array.length - 1;
	while (i >= 0) {
		if (array[i]) return array[i];
		i -= 1;
	}
	return default;
}

public void getLastColors(colorstack, opacitystack, defaultcolors) {
	colors = getLastParam(colorstack, defaultcolors);
	opacity = getLastParam(opacitystack, 255);
	if (opacity != 255) {
		colors = new {
			new Color(colors[0].red, colors[0].green, colors[0].blue, colors[0].alpha * opacity / 255),
			colors[1] ? new Color(colors[1].red, colors[1].green, colors[1].blue, colors[1].alpha * opacity / 255) : null;
		}
	}
	return colors;
}

//===============================================================================
// Formats a string of text and returns an array containing a list of formatted
// characters.
//
// Parameters:
//   bitmap:         Source bitmap. Will be used to determine the default font of
//                   the text.
//   xDst:           X coordinate of the text's top left corner.
//   yDst:           Y coordinate of the text's top left corner.
//   widthDst:       Width of the text. Used to determine line breaks.
//   heightDst:      Height of the text. If -1, there is no height restriction.
//                   If 1 or greater, any characters exceeding the height are
//                   removed from the returned list.
//   newLineBreaks:  If true, newline characters will be treated as line breaks.
//                   The default is true.
//
// Return Values:
//   A list of formatted characters. Returns an empty array if _bitmap_ is null
//   or disposed, or if _widthDst_ is 0 or less or _heightDst_ is 0.
//
// Formatting Specification:
// This function uses the following syntax when formatting the text.
//
//   <b> ... </b>       - Formats the text in bold.
//   <i> ... </i>       - Formats the text in italics.
//   <u> ... </u>       - Underlines the text.
//   <s> ... </s>       - Draws a strikeout line over the text.
//   <al> ... </al>     - Left-aligns the text. Causes line breaks before and
//                        after the text.
//   <r>                - Right-aligns the text until the next line break.
//   <ar> ... </ar>     - Right-aligns the text. Causes line breaks before and
//                        after the text.
//   <ac> ... </ac>     - Centers the text. Causes line breaks before and after
//                        the text.
//   <br>               - Causes a line break.
//   <c=X> ... </c>     - Color specification. A total of four formats are
//                        supported: RRGGBBAA, RRGGBB, 16-bit RGB, and
//                        Window_Base color numbers.
//   <c2=X> ... </c2>   - Color specification where the first half is the base
//                        color and the second half is the shadow color. 16-bit
//                        RGB is supported.
//
// Added 2009-10-20
//
//   <c3=B,S> ... </c3> - Color specification where B is the base color and S is
//                        the shadow color. B and/or S can be omitted. A total of
//                        four formats are supported: RRGGBBAA, RRGGBB, 16-bit
//                        RGB, and Window_Base color numbers.
//
// Added 2009-9-12
//
//   <o=X>              - Displays the text in the given opacity (0-255)
//
// Added 2009-10-19
//
//   <outln>            - Displays the text in outline format.
//
// Added 2010-05-12
//
//   <outln2>           - Displays the text in outline format (outlines more
//                        exaggerated.
//   <fn=X> ... </fn>   - Formats the text in the specified font, or Arial if the
//                        font doesn't exist.
//   <fs=X> ... </fs>   - Changes the font size to X.
//   <icon=X>           - Displays the icon X (in Graphics/Icons/).
//
// In addition, the syntax supports the following:
//   &apos; - Converted to "'".
//   &lt;   - Converted to "<".
//   &gt;   - Converted to ">".
//   &amp;  - Converted to "&".
//   &quot; - Converted to double quotation mark.
//
// To draw the characters, pass the returned array to the
// _drawFormattedChars_ function.
//===============================================================================
public void getFormattedText(bitmap, xDst, yDst, widthDst, heightDst, text, lineheight = 32,
										newlineBreaks = true, explicitBreaksOnly = false,
										collapseAlignments = false) {
	dummybitmap = null;
	if (!bitmap || bitmap.disposed()) {   // allows function to be called with null bitmap
		dummybitmap = new Bitmap(1, 1);
		bitmap = dummybitmap;
		return;
	}
	if (!bitmap || bitmap.disposed() || widthDst <= 0 || heightDst == 0 || text.length == 0) {
		return [];
	}
	textchunks = new List<string>();
	controls = new List<string>();
//  oldtext = text
	while (text[FORMATREGEXP]) {
		textchunks.Add($~.pre_match);
		if ($~[3]) {
			controls.Add(new {$~[2].downcase, $~[4], -1, $~[1] == "/"});
		} else {
			controls.Add(new {$~[2].downcase, "", -1, $~[1] == "/"});
		}
		text = $~.post_match;
	}
	if (controls.length == 0) {
		ret = getFormattedTextFast(bitmap, xDst, yDst, widthDst, heightDst, text, lineheight,
															newlineBreaks, explicitBreaksOnly);
		dummybitmap&.dispose;
		return ret;
	}
	x = y = 0;
	characters = new List<string>();
	charactersInternal = new List<string>();
//  realtext = null
//  realtextStart = ""
//  if (!explicitBreaksOnly && textchunks.join.length == 0) {
//    // All commands occurred at the beginning of the text string
//    realtext = (newlineBreaks) ? text : System.Text.RegularExpressions.Regex.Replace(text, "\n", " ")
//    realtextStart = oldtext[0, oldtext.length - realtext.length]
//  }
	textchunks.Add(text);
	textchunks.each(chunk => fmtReplaceEscapes(chunk));
	textlen = 0;
	controls.each_with_index do |control, i|
		textlen += textchunks[i].scan(/./m).length;
		control[2] = textlen;
	}
	text = textchunks.join;
	textchars = text.scan(/./m);
	colorstack = new List<string>();
	boldcount = 0;
	italiccount = 0;
	outlinecount = 0;
	underlinecount = 0;
	strikecount = 0;
	rightalign = 0;
	outline2count = 0;
	opacitystack = new List<string>();
	oldfont = bitmap.font.clone;
	defaultfontname = bitmap.font.name;
	defaultfontsize = bitmap.font.size;
	fontsize = defaultfontsize;
	fontnamestack = new List<string>();
	fontsizestack = new List<string>();
	defaultcolors = new {oldfont.color.clone, null};
	if (defaultfontname.Length > 0) {
		defaultfontname = defaultfontname.find(i => Font.exist(i)) || "Arial";
	} else if (!Font.exist(defaultfontname)) {
		defaultfontname = "Arial";
	}
	defaultfontname = defaultfontname.clone;
	fontname = defaultfontname;
	alignstack = new List<string>();
	lastword = new {0, 0}; // position of last word
	hadspace = false;
	hadnonspace = false;
	havenl = false;
	position = 0;
	while (position <= textchars.length) {
		nextline = 0;
		graphic = null;
		graphicX = 0;
		graphicY = 4;
		graphicWidth = null;
		graphicHeight = null;
		graphicRect = null;
		for (int i = controls.length; i < controls.length; i++) { //for 'controls.length' times do => |i|
			if (!controls[i] || controls[i][2] != position) continue;
			control = controls[i][0];
			param = controls[i][1];
			endtag = controls[i][3];
			switch (control) {
				case "c":
					if (endtag) {
						colorstack.pop;
					} else {
						color = Color.new_from_rgb(param);
						colorstack.Add(new {color, null});
					}
					break;
				case "c2":
					if (endtag) {
						colorstack.pop;
					} else {
						base = Color.new_from_rgb(param[0, 4]);
						shadow = Color.new_from_rgb(param[4, 4]);
						colorstack.Add(new {base, shadow});
					}
					break;
				case "c3":
					if (endtag) {
						colorstack.pop;
					} else {
						param = param.split(",");
						// get pure colors unaffected by opacity
						oldColors = getLastParam(colorstack, defaultcolors);
						base = (param[0] && param[0] != "") ? Color.new_from_rgb(param[0]) : oldColors[0];
						shadow = (param[1] && param[1] != "") ? Color.new_from_rgb(param[1]) : oldColors[1];
						colorstack.Add(new {base, shadow});
					}
					break;
				case "o":
					if (endtag) {
						opacitystack.pop;
					} else {
						opacitystack.Add(System.Text.RegularExpressions.Regex.Replace(param, "\s+$", "", count: 1).ToInt());
					}
					break;
				case "b":
					boldcount += (endtag ? -1 : 1);
					break;
				case "i":
					italiccount += (endtag ? -1 : 1);
					break;
				case "u":
					underlinecount += (endtag ? -1 : 1);
					break;
				case "s":
					strikecount += (endtag ? -1 : 1);
					break;
				case "outln":
					outlinecount += (endtag ? -1 : 1);
					break;
				case "outln2":
					outline2count += (endtag ? -1 : 1);
					break;
				case "fs":   // Font size
					if (endtag) {
						fontsizestack.pop;
					} else {
						fontsizestack.Add(System.Text.RegularExpressions.Regex.Replace(param, "\s+$", "", count: 1).ToInt());
					}
					fontsize = getLastParam(fontsizestack, defaultfontsize);
					bitmap.font.size = fontsize;
					break;
				case "fn":   // Font name
					if (endtag) {
						fontnamestack.pop;
					} else {
						fontname = System.Text.RegularExpressions.Regex.Replace(param, "\s+$", "", count: 1);
						fontnamestack.Add(Font.exist(fontname) ? fontname : "Arial");
					}
					fontname = getLastParam(fontnamestack, defaultfontname);
					bitmap.font.name = fontname;
					break;
				case "ar":   // Right align
					if (endtag) {
						alignstack.pop;
					} else {
						alignstack.Add(1);
					}
					if (x > 0 && nextline == 0) nextline = 1;
					break;
				case "al":   // Left align
					if (endtag) {
						alignstack.pop;
					} else {
						alignstack.Add(0);
					}
					if (x > 0 && nextline == 0) nextline = 1;
					break;
				case "ac":   // Center align
					if (endtag) {
						alignstack.pop;
					} else {
						alignstack.Add(2);
					}
					if (x > 0 && nextline == 0) nextline = 1;
					break;
				case "icon":   // Icon
					if (!endtag) {
						param = System.Text.RegularExpressions.Regex.Replace(param, "\s+$", "", count: 1);
						graphic = $"Graphics/Icons/{param}";
						controls[i] = null;
						break;
					}
					break;
				case "img":   // Icon
					if (!endtag) {
						param = System.Text.RegularExpressions.Regex.Replace(param, "\s+$", "", count: 1);
						param = param.split("|");
						graphic = param[0];
						if (param.length > 1) {
							graphicX = param[1].ToInt();
							graphicY = param[2].ToInt();
							graphicWidth = param[3].ToInt();
							graphicHeight = param[4].ToInt();
						}
						controls[i] = null;
						break;
					}
					break;
				case "br":   // Line break
					if (!endtag) nextline += 1;
					break;
				case "r":   // Right align this line
					if (!endtag) {
						x = 0;
						rightalign = 1;
						lastword = new {characters.length, x};
					}
					break;
			}
			controls[i] = null;
		}
		bitmap.font.bold = (boldcount > 0);
		bitmap.font.italic = (italiccount > 0);
		if (graphic) {
			if (!graphicWidth) {
				tempgraphic = new Bitmap(graphic);
				graphicWidth = tempgraphic.width;
				graphicHeight = tempgraphic.height;
				tempgraphic.dispose;
			}
			width = graphicWidth;   // +8  // No padding
			xStart = 0;   // 4
			yStart = (int)Math.Max((lineheight / 2) - (graphicHeight / 2), 0);
			yStart += 4;   // TEXT OFFSET
			graphicRect = new Rect(graphicX, graphicY, graphicWidth, graphicHeight);
		} else {
			xStart = 0;
			yStart = 0;
			width = 0;
			if (textchars[position]) {
				width = isWaitChar(textchars[position]) ? 0 : bitmap.text_size(textchars[position]).width;
			}
			if (width > 0 && outline2count > 0) width += 2;
		}
		if (rightalign == 1 && nextline == 0) {
			alignment = 1;
		} else {
			alignment = getLastParam(alignstack, 0);
		}
		nextline.times do;
			havenl = true;
			characters.Add(new {"\n", x, (y * lineheight) + yDst, 0, lineheight, false, false, false,
											defaultcolors[0], defaultcolors[1], false, false, "", 8, position, null, 0});
			charactersInternal.Add(new {alignment, y, 0});
			y += 1;
			x = 0;
			rightalign = 0;
			lastword = new {characters.length, x};
			hadspace = false;
			hadnonspace = false;
		}
		if (textchars[position] == "\n") {
			if (newlineBreaks) {
				if (nextline == 0) {
					havenl = true;
					characters.Add(new {"\n", x, (y * lineheight) + yDst, 0, lineheight, false, false, false,
													defaultcolors[0], defaultcolors[1], false, false, "", 8, position, null, 0});
					charactersInternal.Add(new {alignment, y, 0});
					y += 1;
					x = 0;
				}
				rightalign = 0;
				hadspace = true;
				hadnonspace = false;
				position += 1;
				continue;
			} else {
				textchars[position] = " ";
				if (!graphic) {
					width = bitmap.text_size(textchars[position]).width;
					if (width > 0 && outline2count > 0) width += 2;
				}
			}
		}
		isspace = false;
		if (textchars[position]) {
			isspace = (System.Text.RegularExpressions.Regex.IsMatch(textchars[position],@"\s") || isWaitChar(textchars[position])) ? true : false;
		}
		if (hadspace && !isspace) {
			// set last word to here
			lastword[0] = characters.length;
			lastword[1] = x;
			hadspace = false;
			hadnonspace = true;
		} else if (isspace) {
			hadspace = true;
		}
		texty = (lineheight * y) + yDst + yStart - 2;   // TEXT OFFSET
		colors = getLastColors(colorstack, opacitystack, defaultcolors);
		// Push character
		if (heightDst < 0 || texty < yDst + heightDst) {
			if (!graphic && isWaitChar(textchars[position])) havenl = true;
			extraspace = (!graphic && italiccount > 0) ? 2 + (width / 2) : 2;
			characters.Add(new {graphic || textchars[position],
											x + xStart, texty, width + extraspace, lineheight,
											graphic ? true : false,
											(boldcount > 0), (italiccount > 0), colors[0], colors[1],
											(underlinecount > 0), (strikecount > 0), fontname, fontsize,
											position, graphicRect,
											if (graphic || textchars[position]) ((outlinecount > 0) ? 1 : 0) + ((outline2count > 0) ? 2 : 0)});
			charactersInternal.Add(new {alignment, y, xStart, textchars[position], extraspace});
		}
		x += width;
		if (!explicitBreaksOnly && x + 2 > widthDst && lastword[1] != 0 &&
			(!hadnonspace || !hadspace)) {
			havenl = true;
			characters.insert(lastword[0], new {"\n", x, (y * lineheight) + yDst, 0, lineheight,
																			false, false, false,
																			defaultcolors[0], defaultcolors[1],
																			false, false, "", 8, position, null});
			charactersInternal.insert(lastword[0], new {alignment, y, 0});
			lastword[0] += 1;
			y += 1;
			x = 0;
			for (int i = lastword[0]; i < characters.length; i++) { //each 'characters.length' do => |i|
				characters[i][2] += lineheight;
				charactersInternal[i][1] += 1;
				extraspace = (charactersInternal[i][4]) ? charactersInternal[i][4] : 0;
				charwidth = characters[i][3] - extraspace;
				characters[i][1] = x + charactersInternal[i][2];
				x += charwidth;
			}
			lastword[1] = 0;
		}
		if (!graphic) position += 1;
	}
	// This code looks at whether the text occupies exactly two lines when
	// displayed. If it does, it balances the length of each line.
//  // Count total number of lines
//  numlines = (x==0 && y>0) ? y : y+1
//  if (numlines==2 && realtext && !System.Text.RegularExpressions.Regex.IsMatch(realtext,@"\n") && realtext.length>=50) {
//    // Set half to middle of text (known to contain no formatting)
//    half = realtext.length/2
//    leftSearch  = 0
//    rightSearch = 0
//    // Search left for a space
//    i = half
//    while i>=0
//      break if System.Text.RegularExpressions.Regex.IsMatch(realtext[i,1],@"\s")||isWaitChar(realtext[i,1])   // found a space
//      leftSearch += 1
//      i -= 1
//    }
//    // Search right for a space
//    i = half
//    while i<realtext.length
//      break if System.Text.RegularExpressions.Regex.IsMatch(realtext[i,1],@"\s")||isWaitChar(realtext[i,1])   // found a space
//      rightSearch += 1
//      i += 1
//    }
//    // Move half left or right whichever is closer
//    trialHalf = half+((rightSearch<leftSearch) ? rightSearch : -leftSearch)
//    if (trialHalf!=0 && trialHalf!=realtext.length) {
//      // Insert newline and re-call this function (force newlineBreaksOnly)
//      newText = realtext.clone
//      if (isWaitChar(newText[trialHalf,1])) {
//        // insert after wait character
//        newText.insert(trialHalf+1,"\n")
//      else
//        // remove spaces after newline
//        newText.insert(trialHalf,"\n")
//        newText = System.Text.RegularExpressions.Regex.Replace(newText, "\n\s+","\n")
//      }
//      bitmap.font = oldfont
//      dummybitmap.dispose if dummybitmap
//      return getFormattedText(dummybitmap ? null : bitmap,xDst,yDst,
//         widthDst,heightDst,realtextStart+newText,
//         lineheight,true,explicitBreaksOnly)
//    }
//  }
	if (havenl) {
		// Eliminate spaces before newlines and pause character
		firstspace = -1;
		for (int i = characters.length; i < characters.length; i++) { //for 'characters.length' times do => |i|
			if (characters[i][5] != false) { // If not a character
				firstspace = -1;
			} else if ((characters[i][0] == "\n" || isWaitChar(characters[i][0])) &&
						firstspace >= 0) {
				for (int j = firstspace; j < i; j++) { //each 'i' do => |j|
					characters[j] = null;
					charactersInternal[j] = null;
				}
				firstspace = -1;
			} else if (System.Text.RegularExpressions.Regex.IsMatch(characters[i][0],@"[ \r\t]")) {
				if (firstspace < 0) firstspace = i;
			} else {
				firstspace = -1;
			}
		}
		if (firstspace > 0) {
			for (int j = firstspace; j < characters.length; j++) { //each 'characters.length' do => |j|
				characters[j] = null;
				charactersInternal[j] = null;
			}
		}
		characters.compact!;
		charactersInternal.compact!;
	}
	// Calculate Xs based on alignment
	// First, find all text runs with the same alignment on the same line
	totalwidth = 0;
	widthblocks = new List<string>();
	lastalign = 0;
	lasty = 0;
	runstart = 0;
	for (int i = characters.length; i < characters.length; i++) { //for 'characters.length' times do => |i|
		c = characters[i];
		if (i > 0 && (charactersInternal[i][0] != lastalign ||
			charactersInternal[i][1] != lasty)) {
			// Found end of run
			widthblocks.Add(new {runstart, i, lastalign, totalwidth, lasty});
			runstart = i;
			totalwidth = 0;
		}
		lastalign = charactersInternal[i][0];
		lasty = charactersInternal[i][1];
		extraspace = (charactersInternal[i][4]) ? charactersInternal[i][4] : 0;
		totalwidth += c[3] - extraspace;
	}
	widthblocks.Add(new {runstart, characters.length, lastalign, totalwidth, lasty});
	if (collapseAlignments) {
		// Calculate the total width of each line
		totalLineWidths = new List<string>();
		foreach (var block in widthblocks) { //'widthblocks.each' do => |block|
			y = block[4];
			if (!totalLineWidths[y]) totalLineWidths[y] = 0;
			if (totalLineWidths[y] != 0) {
				// padding in case more than one line has different alignments
				totalLineWidths[y] += 16;
			}
			totalLineWidths[y] += block[3];
		}
		// Calculate a new width for the next step
		widthDst = (int)Math.Min(widthDst, (totalLineWidths.compact.max || 0));
	}
	// Now, based on the text runs found, recalculate Xs
	foreach (var block in widthblocks) { //'widthblocks.each' do => |block|
		if (block[0] >= block[1]) continue;
		for (int i = block[0]; i < block[1]; i++) { //each 'block[1]' do => |i|
			switch (block[2]) {
				case 1:  characters[i][1] = xDst + (widthDst - block[3] - 4) + characters[i][1]; break;
				case 2:  characters[i][1] = xDst + ((widthDst / 2) - (block[3] / 2)) + characters[i][1]; break;
				default:        characters[i][1] = xDst + characters[i][1]; break;
			}
		}
	}
	// Remove all characters with Y greater or equal to _yDst_+_heightDst_
	if (heightDst >= 0) characters.delete_if(ch => ch[2] >= yDst + heightDst);
	bitmap.font = oldfont;
	dummybitmap&.dispose;
	return characters;
}

//===============================================================================
// Draw text and images on a bitmap.
//===============================================================================
public void getLineBrokenText(bitmap, value, width, dims) {
	x = 0;
	y = 0;
	textheight = 0;
	ret = new List<string>();
	if (dims) {
		dims[0] = 0;
		dims[1] = 0;
	}
	line = 0;
	position = 0;
	column = 0;
	if (!bitmap || bitmap.disposed() || width <= 0) return ret;
	textmsg = value.clone;
	ret.Add(new {"", 0, 0, 0, bitmap.text_size("X").height, 0, 0, 0, 0});
	while ((c = textmsg.slice!(/\n|(\S*([ \r\t\f]?))/)) != null) {
		if (c == "") break;
		length = c.scan(/./m).length;
		ccheck = c;
		if (ccheck == "\n") {
			ret.Add(new {"\n", x, y, 0, textheight, line, position, column, 0});
			x = 0;
			y += (textheight == 0) ? bitmap.text_size("X").height : textheight;
			line += 1;
			textheight = 0;
			column = 0;
			position += length;
			ret.Add(new {"", x, y, 0, textheight, line, position, column, 0});
			continue;
		}
		words = [ccheck];
		for (int i = words.length; i < words.length; i++) { //for 'words.length' times do => |i|
			word = words[i];
			if (nil_or_empty(word)) continue;
			textSize = bitmap.text_size(word);
			textwidth = textSize.width;
			if (x > 0 && x + textwidth >= width - 2) {
				// Zero-length word break
				ret.Add(new {"", x, y, 0, textheight, line, position, column, 0});
				x = 0;
				column = 0;
				y += (textheight == 0) ? bitmap.text_size("X").height : textheight;
				line += 1;
				textheight = 0;
			}
			textheight = (int)Math.Max(textheight, textSize.height);
			ret.Add(new {word, x, y, textwidth, textheight, line, position, column, length});
			x += textwidth;
			if (dims && dims[0] < x) dims[0] = x;
		}
		position += length;
		column += length;
	}
	if (dims) dims[1] = y + textheight;
	return ret;
}

public void getLineBrokenChunks(bitmap, value, width, dims, plain = false) {
	x = 0;
	y = 0;
	ret = new List<string>();
	if (dims) {
		dims[0] = 0;
		dims[1] = 0;
	}
	re = @"<c=([^>]+)>";
	reNoMatch = @"<c=[^>]+>";
	if (!bitmap || bitmap.disposed() || width <= 0) return ret;
	textmsg = value.clone;
	color = Font.default_color;
	while ((c = textmsg.slice!(/\n|[^ \r\t\f\n\-]*\-+|(\S*([ \r\t\f]?))/)) != null) {
		if (c == "") break;
		ccheck = c;
		if (ccheck == "\n") {
			x = 0;
			y += 32;
			continue;
		}
		textcols = new List<string>();
		if (System.Text.RegularExpressions.Regex.IsMatch(ccheck,@"<") && !plain) {
			ccheck.scan(re) { textcols.Add(Color.new_from_rgb(Game.GameData.1)) };
			words = ccheck.split(reNoMatch); // must have no matches because split can include match
		} else {
			words = [ccheck];
		}
		for (int i = words.length; i < words.length; i++) { //for 'words.length' times do => |i|
			word = words[i];
			if (word && word != "") {
				textSize = bitmap.text_size(word);
				textwidth = textSize.width;
				if (x > 0 && x + textwidth > width) {
					minTextSize = bitmap.text_size(System.Text.RegularExpressions.Regex.Replace(word, "\s*", ""));
					if (x > 0 && x + minTextSize.width > width) {
						x = 0;
						y += 32;
					}
				}
				ret.Add(new {word, x, y, textwidth, 32, color});
				x += textwidth;
				if (dims && dims[0] < x) dims[0] = x;
			}
			if (textcols[i]) color = textcols[i];
		}
	}
	if (dims) dims[1] = y + 32;
	return ret;
}

public void renderLineBrokenChunks(bitmap, xDst, yDst, normtext, maxheight = 0) {
	foreach (var text in normtext) { //'normtext.each' do => |text|
		width = text[3];
		textx = text[1] + xDst;
		texty = text[2] + yDst;
		if (maxheight == 0 || text[2] < maxheight) {
			bitmap.font.color = text[5];
			bitmap.draw_text(textx, texty, width + 2, text[4], text[0]);
		}
	}
}

public void renderLineBrokenChunksWithShadow(bitmap, xDst, yDst, normtext, maxheight, baseColor, shadowColor) {
	foreach (var text in normtext) { //'normtext.each' do => |text|
		width = text[3];
		textx = text[1] + xDst;
		texty = text[2] + yDst;
		if (maxheight != 0 && text[2] >= maxheight) continue;
		height = text[4];
		text = text[0];
		bitmap.font.color = shadowColor;
		bitmap.draw_text(textx + 2, texty, width + 2, height, text);
		bitmap.draw_text(textx, texty + 2, width + 2, height, text);
		bitmap.draw_text(textx + 2, texty + 2, width + 2, height, text);
		bitmap.font.color = baseColor;
		bitmap.draw_text(textx, texty, width + 2, height, text);
	}
}

public void drawBitmapBuffer(chars) {
	width = 1;
	height = 1;
	foreach (var ch in chars) { //'chars.each' do => |ch|
		chx = ch[1] + ch[3];
		chy = ch[2] + ch[4];
		if (width < chx) width = chx;
		if (height < chy) height = chy;
	}
	buffer = new Bitmap(width, height);
	drawFormattedChars(buffer, chars);
	return buffer;
}

public void drawSingleFormattedChar(bitmap, ch) {
	if (ch[5]) {   // If a graphic
		graphic = new Bitmap(ch[0]);
		graphicRect = ch[15];
		bitmap.blt(ch[1], ch[2], graphic, graphicRect, ch[8].alpha);
		graphic.dispose;
		return;
	}
	if (bitmap.font.size != ch[13]) bitmap.font.size = ch[13];
	if (ch[9]) {   // shadow
		if (ch[10]) {   // underline
			bitmap.fill_rect(ch[1], ch[2] + ch[4] - (int)Math.Max((ch[4] - bitmap.font.size) / 2, 0) - 2, ch[3], 4, ch[9]);
		}
		if (ch[11]) {   // strikeout
			bitmap.fill_rect(ch[1], ch[2] + 2 + (ch[4] / 2), ch[3], 4, ch[9]);
		}
	}
	if (ch[0] == "\n" || ch[0] == "\r" || ch[0] == " " || isWaitChar(ch[0])) {
		if (bitmap.font.color != ch[8]) bitmap.font.color = ch[8];
	} else {
		if (bitmap.font.bold != ch[6]) bitmap.font.bold = ch[6];
		if (bitmap.font.italic != ch[7]) bitmap.font.italic = ch[7];
		if (bitmap.font.name != ch[12]) bitmap.font.name = ch[12];
		offset = 0;
		if (ch[9]) {   // shadow
			bitmap.font.color = ch[9];
			if ((ch[16] & 1) != 0) {   // outline
				offset = 1;
				bitmap.draw_text(ch[1], ch[2], ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1], ch[2] + 1, ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1], ch[2] + 2, ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 1, ch[2], ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 1, ch[2] + 2, ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 2, ch[2], ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 2, ch[2] + 1, ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 2, ch[2] + 2, ch[3] + 2, ch[4], ch[0]);
			} else if ((ch[16] & 2) != 0) {   // outline 2
				offset = 2;
				bitmap.draw_text(ch[1], ch[2], ch[3] + 4, ch[4], ch[0]);
				bitmap.draw_text(ch[1], ch[2] + 2, ch[3] + 4, ch[4], ch[0]);
				bitmap.draw_text(ch[1], ch[2] + 4, ch[3] + 4, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 2, ch[2], ch[3] + 4, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 2, ch[2] + 4, ch[3] + 4, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 4, ch[2], ch[3] + 4, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 4, ch[2] + 2, ch[3] + 4, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 4, ch[2] + 4, ch[3] + 4, ch[4], ch[0]);
			} else {
				bitmap.draw_text(ch[1] + 2, ch[2], ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1], ch[2] + 2, ch[3] + 2, ch[4], ch[0]);
				bitmap.draw_text(ch[1] + 2, ch[2] + 2, ch[3] + 2, ch[4], ch[0]);
			}
		}
		if (bitmap.font.color != ch[8]) bitmap.font.color = ch[8];
		bitmap.draw_text(ch[1] + offset, ch[2] + offset, ch[3], ch[4], ch[0]);
	}
	if (ch[10]) {   // underline
		bitmap.fill_rect(ch[1], ch[2] + ch[4] - (int)Math.Max((ch[4] - bitmap.font.size) / 2, 0) - 2, ch[3] - 2, 2, ch[8]);
	}
	if (ch[11]) {   // strikeout
		bitmap.fill_rect(ch[1], ch[2] + 2 + (ch[4] / 2), ch[3] - 2, 2, ch[8]);
	}
}

public void drawFormattedChars(bitmap, chars) {
	if (chars.length == 0 || !bitmap || bitmap.disposed()) return;
	oldfont = bitmap.font.clone;
	foreach (var ch in chars) { //'chars.each' do => |ch|
		drawSingleFormattedChar(bitmap, ch);
	}
	bitmap.font = oldfont;
}

// Unused
public void drawTextTable(bitmap, x, y, totalWidth, rowHeight, columnWidthPercents, table) {
	yPos = y;
	for (int i = table.length; i < table.length; i++) { //for 'table.length' times do => |i|
		row = table[i];
		xPos = x;
		for (int j = row.length; j < row.length; j++) { //for 'row.length' times do => |j|
			cell = row[j];
			cellwidth = columnWidthPercents[j] * totalWidth / 100;
			chars = getFormattedText(bitmap, xPos, yPos, cellwidth, -1, cell, rowHeight);
			drawFormattedChars(bitmap, chars);
			xPos += cellwidth;
		}
		yPos += rowHeight;
	}
}

public void drawTextEx(bitmap, x, y, width, numlines, text, baseColor, shadowColor) {
	normtext = getLineBrokenChunks(bitmap, text, width, null, true);
	renderLineBrokenChunksWithShadow(bitmap, x, y, normtext, numlines * 32,
																	baseColor, shadowColor);
}

public void drawFormattedTextEx(bitmap, x, y, width, text, baseColor = null, shadowColor = null, lineheight = 32) {
	base = baseColor ? baseColor.clone : new Color(96, 96, 96);
	shadow = shadowColor ? shadowColor.clone : new Color(208, 208, 200);
	text = shadowc3tag(base, shadow) + text;
	chars = getFormattedText(bitmap, x, y, width, -1, text, lineheight);
	drawFormattedChars(bitmap, chars);
}

// Unused
public void DrawShadow(bitmap, x, y, width, height, string) {
	if (!bitmap || !string) return;
	DrawShadowText(bitmap, x, y, width, height, string, null, bitmap.font.color);
}

public void DrawPlainText(bitmap, x, y, width, height, string, baseColor, align = 0) {
	if (!bitmap || !string) return;
	width = (width < 0) ? bitmap.text_size(string).width + 1 : width;
	height = (height < 0) ? bitmap.text_size(string).height + 1 : height;
	if (baseColor && baseColor.alpha > 0) {
		bitmap.font.color = baseColor;
		bitmap.draw_text(x, y, width, height, string, align);
	}
}

public void DrawShadowText(bitmap, x, y, width, height, string, baseColor, shadowColor = null, align = 0) {
	if (!bitmap || !string) return;
	width = (width < 0) ? bitmap.text_size(string).width + 1 : width;
	height = (height < 0) ? bitmap.text_size(string).height + 1 : height;
	if (shadowColor && shadowColor.alpha > 0) {
		bitmap.font.color = shadowColor;
		bitmap.draw_text(x + 2, y, width, height, string, align);
		bitmap.draw_text(x, y + 2, width, height, string, align);
		bitmap.draw_text(x + 2, y + 2, width, height, string, align);
	}
	if (baseColor && baseColor.alpha > 0) {
		bitmap.font.color = baseColor;
		bitmap.draw_text(x, y, width, height, string, align);
	}
}

public void DrawOutlineText(bitmap, x, y, width, height, string, baseColor, shadowColor = null, align = 0) {
	if (!bitmap || !string) return;
	width = (width < 0) ? bitmap.text_size(string).width + 4 : width;
	height = (height < 0) ? bitmap.text_size(string).height + 4 : height;
	if (shadowColor && shadowColor.alpha > 0) {
		bitmap.font.color = shadowColor;
		bitmap.draw_text(x - 2, y - 2, width, height, string, align);
		bitmap.draw_text(x, y - 2, width, height, string, align);
		bitmap.draw_text(x + 2, y - 2, width, height, string, align);
		bitmap.draw_text(x - 2, y, width, height, string, align);
		bitmap.draw_text(x + 2, y, width, height, string, align);
		bitmap.draw_text(x - 2, y + 2, width, height, string, align);
		bitmap.draw_text(x, y + 2, width, height, string, align);
		bitmap.draw_text(x + 2, y + 2, width, height, string, align);
	}
	if (baseColor && baseColor.alpha > 0) {
		bitmap.font.color = baseColor;
		bitmap.draw_text(x, y, width, height, string, align);
	}
}

// Draws text on a bitmap. _textpos_ is an array of text commands. Each text
// command is an array that contains the following:
//  0 - Text to draw
//  1 - X coordinate
//  2 - Y coordinate
//  3 - Text alignment. Is one of :left (or false or 0), :right (or true or 1) or
//      :center (or 2). If anything else, the text is left aligned.
//  4 - Base color
//  5 - Shadow color. If null, there is no shadow.
//  6 - If :outline (or true or 1), the text has a full outline. If :none (or the
//      shadow color is null), there is no shadow. Otherwise, the text has a shadow.
public void DrawTextPositions(bitmap, textpos) {
	foreach (var i in textpos) { //'textpos.each' do => |i|
		textsize = bitmap.text_size(i[0]);
		x = i[1];
		y = i[2];
		switch (i[3]) {
			case :right, true, 1:   // right align
				x -= textsize.width;
				break;
			case :center, 2:   // centered
				x -= (textsize.width / 2);
				break;
		}
		if (!i[5]) i[6] = :none;   // No shadow color given, draw plain text
		switch (i[6]) {
			case :outline, true, 1:   // outline text
				DrawOutlineText(bitmap, x, y, textsize.width, textsize.height, i[0], i[4], i[5]);
				break;
			case :none:
				DrawPlainText(bitmap, x, y, textsize.width, textsize.height, i[0], i[4]);
				break;
			default:
				DrawShadowText(bitmap, x, y, textsize.width, textsize.height, i[0], i[4], i[5]);
				break;
		}
	}
}

//===============================================================================
// Draw images on a bitmap.
//===============================================================================
public void CopyBitmap(dstbm, srcbm, x, y, opacity = 255) {
	rc = new Rect(0, 0, srcbm.width, srcbm.height);
	dstbm.blt(x, y, srcbm, rc, opacity);
}

public void DrawImagePositions(bitmap, textpos) {
	foreach (var i in textpos) { //'textpos.each' do => |i|
		srcbitmap = new AnimatedBitmap(BitmapName(i[0]));
		x = i[1];
		y = i[2];
		srcx = i[3] || 0;
		srcy = i[4] || 0;
		width = (i[5] && i[5] >= 0) ? i[5] : srcbitmap.width;
		height = (i[6] && i[6] >= 0) ? i[6] : srcbitmap.height;
		srcrect = new Rect(srcx, srcy, width, height);
		bitmap.blt(x, y, srcbitmap.bitmap, srcrect);
		srcbitmap.dispose;
	}
}
