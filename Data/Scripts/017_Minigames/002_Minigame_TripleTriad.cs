//===============================================================================
// "Triple Triad" mini-game.
// By Unknown.
//===============================================================================

//===============================================================================
// Card class.
//===============================================================================
public partial class TriadCard {
	public int species		{ get { return _species; } set { _species = value; } }			protected int _species;
	public int form		{ get { return _form; } }			protected int _form;
	public int north		{ get { return _north; } set { _north = value; } }			protected int _north;
	public int east		{ get { return _east; } }			protected int _east;
	public int south		{ get { return _south; } }			protected int _south;
	public int west		{ get { return _west; } }			protected int _west;
	public int type		{ get { return _type; } }			protected int _type;

	public void initialize(species, form = 0) {
		@species = species;
		@form    = form;
		species_data = GameData.Species.get_species_form(@species, @form);
		baseStats = species_data.base_stats;
		hp      = baseStats[:HP];
		attack  = baseStats[:ATTACK];
		defense = baseStats[:DEFENSE];
		spAtk   = baseStats[:SPECIAL_ATTACK];
		spDef   = baseStats[:SPECIAL_DEFENSE];
		speed   = baseStats[:SPEED];
		@type  = species_data.types[0];
		if (@type == types.NORMAL && species_data.types[1]) @type  = species_data.types[1];
		@west  = baseStatToValue(attack + (speed / 3));
		@east  = baseStatToValue(defense + (hp / 3));
		@north = baseStatToValue(spAtk + (speed / 3));
		@south = baseStatToValue(spDef + (hp / 3));
	}

	public void baseStatToValue(stat) {
		if (stat >= 189) return 10;
		if (stat >= 160) return 9;
		if (stat >= 134) return 8;
		if (stat >= 115) return 7;
		if (stat >= 100) return 6;
		if (stat >= 86) return 5;
		if (stat >= 73) return 4;
		if (stat >= 60) return 3;
		if (stat >= 45) return 2;
		return 1;
	}

	public void attack(panel) {
		return new {@west, @east, @north, @south}[panel];
	}

	public void defense(panel) {
		return new {@east, @west, @south, @north}[panel];
	}

	public void bonus(opponent) {
		effectiveness = Effectiveness.calculate(@type, opponent.type);
		if (Effectiveness.ineffective(effectiveness)) {
			return -2;
		} else if (Effectiveness.not_very_effective(effectiveness)) {
			return -1;
		} else if (Effectiveness.super_effective(effectiveness)) {
			return 1;
		}
		return 0;
	}

	public void price() {
		maxValue = (int)Math.Max(@north, @east, @south, @west);
		ret = (@north * @north) + (@east * @east) + (@south * @south) + (@west * @west);
		ret += maxValue * maxValue * 2;
		ret *= maxValue;
		ret *= (@north + @east + @south + @west);
		ret /= 10;   // Ranges from 2 to 24,000
		// Quantize prices to the next highest "unit"
		if (ret > 10_000) {
			ret = (1 + (ret / 1000)) * 1000;
		} else if (ret > 5000) {
			ret = (1 + (ret / 500)) * 500;
		} else if (ret > 1000) {
			ret = (1 + (ret / 100)) * 100;
		} else if (ret > 500) {
			ret = (1 + (ret / 50)) * 50;
		} else {
			ret = (1 + (ret / 10)) * 10;
		}
		return ret;
	}

	public static void createBack(type = null, noback = false) {
		bitmap = new Bitmap(80, 96);
		if (!noback) {
			cardbitmap = new AnimatedBitmap("Graphics/UI/Triple Triad/card_opponent");
			bitmap.blt(0, 0, cardbitmap.bitmap, new Rect(0, 0, cardbitmap.width, cardbitmap.height));
			cardbitmap.dispose;
		}
		if (type) {
			typebitmap = new AnimatedBitmap(_INTL("Graphics/UI/types"));
			type_number = GameData.Type.get(type).icon_position;
			typerect = new Rect(0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE);
			bitmap.blt(8, 50, typebitmap.bitmap, typerect, 192);
			typebitmap.dispose;
		}
		return bitmap;
	}

	public void createBitmap(owner) {
		if (owner == 0) return TriadCard.createBack;
		bitmap = new Bitmap(80, 96);
		if (owner == 2) {   // Opponent
			cardbitmap = new AnimatedBitmap("Graphics/UI/Triple Triad/card_opponent");
		} else {            // Player
			cardbitmap = new AnimatedBitmap("Graphics/UI/Triple Triad/card_player");
		}
		typebitmap = new AnimatedBitmap(_INTL("Graphics/UI/types"));
		iconbitmap = new AnimatedBitmap(GameData.Species.icon_filename(@species, @form));
		numbersbitmap = new AnimatedBitmap("Graphics/UI/Triple Triad/numbers");
		// Draw card background
		bitmap.blt(0, 0, cardbitmap.bitmap, new Rect(0, 0, cardbitmap.width, cardbitmap.height));
		// Draw type icon
		type_number = GameData.Type.get(@type).icon_position;
		typerect = new Rect(0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE);
		bitmap.blt(8, 50, typebitmap.bitmap, typerect, 192);
		// Draw Pokémon icon
		bitmap.blt(8, 24, iconbitmap.bitmap, new Rect(0, 0, 64, 64));
		// Draw numbers
		bitmap.blt(8, 16, numbersbitmap.bitmap, new Rect(@west * 16, 0, 16, 16));
		bitmap.blt(22, 6, numbersbitmap.bitmap, new Rect(@north * 16, 0, 16, 16));
		bitmap.blt(36, 16, numbersbitmap.bitmap, new Rect(@east * 16, 0, 16, 16));
		bitmap.blt(22, 26, numbersbitmap.bitmap, new Rect(@south * 16, 0, 16, 16));
		cardbitmap.dispose;
		typebitmap.dispose;
		iconbitmap.dispose;
		numbersbitmap.dispose;
		return bitmap;
	}
}

//===============================================================================
// Duel screen visuals.
//===============================================================================
public partial class TriadSquare {
	public int owner		{ get { return _owner; } set { _owner = value; } }			protected int _owner;
	public int card		{ get { return _card; } set { _card = value; } }			protected int _card;
, :type;

	public void initialize() {
		@owner = 0;
		@card  = null;
		@type  = null;
	}

	public void attack(panel) {
		return @card.attack(panel);
	}

	public void bonus(square) {
		return @card.bonus(square.card);
	}

	public void defense(panel) {
		return @card.defense(panel);
	}
}

//===============================================================================
// Scene class for handling appearance of the screen.
//===============================================================================
public partial class TriadScene {
	public void StartScene(battle) {
		@sprites = new List<string>();
		@bitmaps = new List<string>();
		@battle = battle;
		// Allocate viewport
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		addBackgroundPlane(@sprites, "background", "Triple Triad/bg", @viewport);
		@sprites["helpwindow"] = Window_AdvancedTextPokemon.newWithSize(
			"", 0, Graphics.height - 64, Graphics.width, 64, @viewport
		);
		for (int i = (@battle.width * @battle.height); i < (@battle.width * @battle.height); i++) { //for '(@battle.width * @battle.height)' times do => |i|
			@sprites[$"sprite{i}"] = new Sprite(@viewport);
			@sprites[$"sprite{i}"].x = (Graphics.width / 2) - 118 + ((i % 3) * 78);
			@sprites[$"sprite{i}"].y = 36 + ((i / 3) * 94);
			@sprites[$"sprite{i}"].z = 2;
			bm = TriadCard.createBack(@battle.board[i].type, true);
			@bitmaps.Add(bm);
			@sprites[$"sprite{i}"].bitmap = bm;
		}
		@cardBitmaps         = new List<string>();
		@opponentCardBitmaps = new List<string>();
		@cardIndexes         = new List<string>();
		@opponentCardIndexes = new List<string>();
		@boardSprites        = new List<string>();
		@boardCards          = new List<string>();
		for (int i = @battle.maxCards; i < @battle.maxCards; i++) { //for '@battle.maxCards' times do => |i|
			@sprites[$"player{i}"] = new Sprite(@viewport);
			@sprites[$"player{i}"].x = Graphics.width - 92;
			@sprites[$"player{i}"].y = 44 + (44 * i);
			@sprites[$"player{i}"].z = 2;
			@cardIndexes.Add(i);
		}
		@sprites["overlay"] = new Sprite(@viewport);
		@sprites["overlay"].bitmap = new Bitmap(Graphics.width, Graphics.height);
		SetSystemFont(@sprites["overlay"].bitmap);
		DrawTextPositions(
			@sprites["overlay"].bitmap,
			new {@battle.opponentName, 52, 10, :center, new Color(248, 248, 248), new Color(96, 96, 96)},
			new {@battle.playerName, Graphics.width - 52, 10, :center, new Color(248, 248, 248), new Color(96, 96, 96)}
		);
		@sprites["score"] = new Sprite(@viewport);
		@sprites["score"].bitmap = new Bitmap(Graphics.width, Graphics.height);
		SetSystemFont(@sprites["score"].bitmap);
		BGMPlay("Triple Triad");
		// Fade in all sprites
		FadeInAndShow(@sprites) { Update };
	}

	public void EndScene() {
		BGMFade(1.0);
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@bitmaps.each(bm => bm.dispose);
		@viewport.dispose;
	}

	public void Display(text) {
		@sprites["helpwindow"].text = text;
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (System.uptime - timer_start >= 1.5) break;
		}
	}

	public void DisplayPaused(text) {
		@sprites["helpwindow"].letterbyletter = true;
		@sprites["helpwindow"].text           = text + "\1";
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.USE)) {
				if (@sprites["helpwindow"].busy()) {
					if (@sprites["helpwindow"].pausing()) PlayDecisionSE;
					@sprites["helpwindow"].resume;
				} else {
					break;
				}
			}
		}
		@sprites["helpwindow"].letterbyletter = false;
		@sprites["helpwindow"].text           = "";
	}

	public void NotifyCards(playerCards, opponentCards) {
		@playerCards   = playerCards;
		@opponentCards = opponentCards;
	}

	public void ChooseTriadCard(cardStorage) {
		commands    = new List<string>();
		chosenCards = new List<string>();
		foreach (var item in cardStorage) { //'cardStorage.each' do => |item|
			commands.Add(_INTL("{1} x{2}", GameData.Species.get(item[0]).name, item[1]));
		}
		command = Window_CommandPokemonEx.newWithSize(commands, 0, 0, Graphics.width / 2, Graphics.height - 64, @viewport);
		@sprites["helpwindow"].text = _INTL("Choose {1} cards to use for this duel.", @battle.maxCards);
		preview = new Sprite(@viewport);
		preview.x = (Graphics.width / 2) + 20;
		preview.y = 60;
		preview.z = 4;
		index = -1;
		for (int i = @battle.maxCards; i < @battle.maxCards; i++) { //for '@battle.maxCards' times do => |i|
			@sprites[$"player{i}"] = new Sprite(@viewport);
			@sprites[$"player{i}"].x = Graphics.width - 92;
			@sprites[$"player{i}"].y = 44 + (44 * i);
			@sprites[$"player{i}"].z = 2;
		}
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			command.update;
			if (command.index != index) {
				preview.bitmap&.dispose;
				if (command.index < cardStorage.length) {
					item = cardStorage[command.index];
					preview.bitmap = new TriadCard(item[0]).createBitmap(1);
				}
				index = command.index;
			}
			if (Input.trigger(Input.BACK)) {
				if (chosenCards.length > 0) {
					item = chosenCards.pop;
					@battle.Add(cardStorage, item);
					commands = new List<string>();
					foreach (var itm in cardStorage) { //'cardStorage.each' do => |itm|
						commands.Add(_INTL("{1} x{2}", GameData.Species.get(itm[0]).name, itm[1]));
					}
					command.commands = commands;
					index = -1;
				} else {
					PlayBuzzerSE;
				}
			} else if (Input.trigger(Input.USE)) {
				if (chosenCards.length == @battle.maxCards) break;
				item = cardStorage[command.index];
				if (!item || @battle.quantity(cardStorage, item[0]) == 0) {
					PlayBuzzerSE;
				} else {
					PlayDecisionSE;
					sprite = @sprites[$"player{chosenCards.length}"];
					sprite.bitmap&.dispose;
					@cardBitmaps[chosenCards.length] = new TriadCard(item[0]).createBitmap(1);
					sprite.bitmap = @cardBitmaps[chosenCards.length];
					chosenCards.Add(item[0]);
					@battle.Subtract(cardStorage, item[0]);
					commands = new List<string>();
					foreach (var itm in cardStorage) { //'cardStorage.each' do => |itm|
						commands.Add(_INTL("{1} x{2}", GameData.Species.get(itm[0]).name, itm[1]));
					}
					command.commands = commands;
					if (command.index >= commands.length) command.index = commands.length - 1;
					index = -1;
				}
			}
			if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) {
				for (int i = @battle.maxCards; i < @battle.maxCards; i++) { //for '@battle.maxCards' times do => |i|
					@sprites[$"player{i}"].visible = (i < chosenCards.length);
				}
				if (chosenCards.length == @battle.maxCards) {
					@sprites["helpwindow"].text = _INTL("{1} cards have been chosen.", @battle.maxCards);
					command.visible = false;
					command.active  = false;
					preview.visible = false;
				} else {
					@sprites["helpwindow"].text = _INTL("Choose {1} cards to use for this duel.", @battle.maxCards);
					command.visible = true;
					command.active  = true;
					preview.visible = true;
				}
			}
		}
		command.dispose;
		preview.bitmap&.dispose;
		preview.dispose;
		return chosenCards;
	}

	public void ShowPlayerCards(cards) {
		for (int i = @battle.maxCards; i < @battle.maxCards; i++) { //for '@battle.maxCards' times do => |i|
			@sprites[$"player{i}"] = new Sprite(@viewport);
			@sprites[$"player{i}"].x      = Graphics.width - 92;
			@sprites[$"player{i}"].y      = 44 + (44 * i);
			@sprites[$"player{i}"].z      = 2;
			@sprites[$"player{i}"].bitmap = new TriadCard(cards[i]).createBitmap(1);
			@cardBitmaps.Add(@sprites[$"player{i}"].bitmap);
		}
	}

	public void ShowOpponentCards(cards) {
		for (int i = @battle.maxCards; i < @battle.maxCards; i++) { //for '@battle.maxCards' times do => |i|
			@sprites[$"opponent{i}"] = new Sprite(@viewport);
			@sprites[$"opponent{i}"].x      = 12;
			@sprites[$"opponent{i}"].y      = 44 + (44 * i);
			@sprites[$"opponent{i}"].z      = 2;
			@sprites[$"opponent{i}"].bitmap = @battle.openHand ? new TriadCard(cards[i]).createBitmap(2) : TriadCard.createBack;
			@opponentCardBitmaps.Add(@sprites[$"opponent{i}"].bitmap);
			@opponentCardIndexes.Add(i);
		}
	}

	public void ViewOpponentCards(numCards) {
		@sprites["helpwindow"].text = _INTL("Check opponent's cards.");
		choice     = 0;
		lastChoice = -1;
		do { //loop; while (true);
			if (lastChoice != choice) {
				y = 44;
				for (int i = @opponentCardIndexes.length; i < @opponentCardIndexes.length; i++) { //for '@opponentCardIndexes.length' times do => |i|
					@sprites[$"opponent{@opponentCardIndexes[i]}"].bitmap = @opponentCardBitmaps[@opponentCardIndexes[i]];
					@sprites[$"opponent{@opponentCardIndexes[i]}"].x      = (i == choice) ? 28 : 12;
					@sprites[$"opponent{@opponentCardIndexes[i]}"].y      = y;
					@sprites[$"opponent{@opponentCardIndexes[i]}"].z      = 2;
					y += 44;
				}
				lastChoice = choice;
			}
			if (choice == -1) break;
			Graphics.update;
			Input.update;
			Update;
			if (Input.repeat(Input.DOWN)) {
				PlayCursorSE;
				choice += 1;
				if (choice >= numCards) choice = 0;
			} else if (Input.repeat(Input.UP)) {
				PlayCursorSE;
				choice -= 1;
				if (choice < 0) choice = numCards - 1;
			} else if (Input.trigger(Input.BACK)) {
				PlayCancelSE
				choice = -1;
			}
		}
		return choice;
	}

	public void PlayerChooseCard(numCards) {
		if (@battle.openHand) {
			@sprites["helpwindow"].text = _INTL("Choose a card, or check opponent with Z.");
		} else {
			@sprites["helpwindow"].text = _INTL("Choose a card.");
		}
		choice     = 0;
		lastChoice = -1;
		do { //loop; while (true);
			if (lastChoice != choice) {
				y = 44;
				for (int i = @cardIndexes.length; i < @cardIndexes.length; i++) { //for '@cardIndexes.length' times do => |i|
					@sprites[$"player{@cardIndexes[i]}"].bitmap = @cardBitmaps[@cardIndexes[i]];
					@sprites[$"player{@cardIndexes[i]}"].x      = (i == choice) ? Graphics.width - 108 : Graphics.width - 92;
					@sprites[$"player{@cardIndexes[i]}"].y      = y;
					@sprites[$"player{@cardIndexes[i]}"].z      = 2;
					y += 44;
				}
				lastChoice = choice;
			}
			Graphics.update;
			Input.update;
			Update;
			if (Input.repeat(Input.DOWN)) {
				PlayCursorSE;
				choice += 1;
				if (choice >= numCards) choice = 0;
			} else if (Input.repeat(Input.UP)) {
				PlayCursorSE;
				choice -= 1;
				if (choice < 0) choice = numCards - 1;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				break;
			} else if (Input.trigger(Input.ACTION) && @battle.openHand) {
				PlayDecisionSE;
				ViewOpponentCards(numCards);
				@sprites["helpwindow"].text = _INTL("Choose a card, or check opponent with Z.");
				choice     = 0;
				lastChoice = -1;
			}
		}
		return choice;
	}

	public void PlayerPlaceCard(cardIndex) {
		@sprites["helpwindow"].text = _INTL("Place the card.");
		boardX = 0;
		boardY = 0;
		doRefresh = true;
		do { //loop; while (true);
			if (doRefresh) {
				y = 44;
				for (int i = @cardIndexes.length; i < @cardIndexes.length; i++) { //for '@cardIndexes.length' times do => |i|
					if (i == cardIndex) {   // Card being placed
						@sprites[$"player{@cardIndexes[i]}"].x = (Graphics.width / 2) - 118 + (boardX * 78);
						@sprites[$"player{@cardIndexes[i]}"].y = 36 + (boardY * 94);
						@sprites[$"player{@cardIndexes[i]}"].z = 4;
					} else {   // Other cards in hand
						@sprites[$"player{@cardIndexes[i]}"].x = Graphics.width - 92;
						@sprites[$"player{@cardIndexes[i]}"].y = y;
						@sprites[$"player{@cardIndexes[i]}"].z = 2;
						y += 44;
					}
				}
				doRefresh = false;
			}
			Graphics.update;
			Input.update;
			Update;
			if (Input.repeat(Input.DOWN)) {
				PlayCursorSE;
				boardY += 1;
				if (boardY >= @battle.height) boardY = 0;
				doRefresh = true;
			} else if (Input.repeat(Input.UP)) {
				PlayCursorSE;
				boardY -= 1;
				if (boardY < 0) boardY = @battle.height - 1;
				doRefresh = true;
			} else if (Input.repeat(Input.LEFT)) {
				PlayCursorSE;
				boardX -= 1;
				if (boardX < 0) boardX = @battle.width - 1;
				doRefresh = true;
			} else if (Input.repeat(Input.RIGHT)) {
				PlayCursorSE;
				boardX += 1;
				if (boardX >= @battle.width) boardX = 0;
				doRefresh = true;
			} else if (Input.trigger(Input.BACK)) {
				return null;
			} else if (Input.trigger(Input.USE)) {
				if (@battle.isOccupied(boardX, boardY)) {
					PlayBuzzerSE;
				} else {
					PlayDecisionSE;
					@sprites[$"player{@cardIndexes[cardIndex]}"].z = 2;
					break;
				}
			}
		}
		return new {boardX, boardY};
	}

	public void EndPlaceCard(position, cardIndex) {
		spriteIndex = @cardIndexes[cardIndex];
		boardIndex = (position[1] * @battle.width) + position[0];
		@boardSprites[boardIndex] = @sprites[$"player{spriteIndex}"];
		@boardCards[boardIndex] = new TriadCard(@playerCards[spriteIndex]);
		Refresh;
		@cardIndexes.delete_at(cardIndex);
		UpdateScore;
	}

	public void OpponentPlaceCard(triadCard, position, cardIndex) {
		y = 44;
		for (int i = @opponentCardIndexes.length; i < @opponentCardIndexes.length; i++) { //for '@opponentCardIndexes.length' times do => |i|
			sprite = @sprites[$"opponent{@opponentCardIndexes[i]}"];
			if (i == cardIndex) {
				@opponentCardBitmaps[@opponentCardIndexes[i]] = triadCard.createBitmap(2);
				sprite.bitmap&.dispose;
				sprite.bitmap = @opponentCardBitmaps[@opponentCardIndexes[i]];
				sprite.x = (Graphics.width / 2) - 118 + (position[0] * 78);
				sprite.y = 36 + (position[1] * 94);
				sprite.z = 2;
			} else {
				sprite.x = 12;
				sprite.y = y;
				sprite.z = 2;
				y += 44;
			}
		}
	}

	public void EndOpponentPlaceCard(position, cardIndex) {
		spriteIndex = @opponentCardIndexes[cardIndex];
		boardIndex = (position[1] * @battle.width) + position[0];
		@boardSprites[boardIndex] = @sprites[$"opponent{spriteIndex}"];
		@boardCards[boardIndex] = new TriadCard(@opponentCards[spriteIndex]);
		Refresh;
		@opponentCardIndexes.delete_at(cardIndex);
		UpdateScore;
	}

	public void Refresh() {
		for (int i = (@battle.width * @battle.height); i < (@battle.width * @battle.height); i++) { //for '(@battle.width * @battle.height)' times do => |i|
			x = i % @battle.width;
			y = i / @battle.width;
			if (!@boardSprites[i]) continue;
			@boardSprites[i].bitmap&.dispose;
			owner = @battle.getOwner(x, y);
			@boardSprites[i].bitmap = @boardCards[i].createBitmap(owner);
		}
	}

	public void UpdateScore() {
		bitmap = @sprites["score"].bitmap;
		bitmap.clear;
		playerscore = 0;
		oppscore    = 0;
		for (int i = (@battle.width * @battle.height); i < (@battle.width * @battle.height); i++) { //for '(@battle.width * @battle.height)' times do => |i|
			if (@boardSprites[i]) {
				if (@battle.board[i].owner == 1) playerscore += 1;
				if (@battle.board[i].owner == 2) oppscore    += 1;
			}
		}
		if (@battle.countUnplayedCards) {
			playerscore += @cardIndexes.length;
			oppscore    += @opponentCardIndexes.length;
		}
		DrawTextPositions(
			bitmap,
			new {_INTL("{1}-{2}", oppscore, playerscore), Graphics.width / 2, 10, :center, new Color(248, 248, 248), new Color(96, 96, 96)}
		);
	}

	public void Update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
// Duel screen logic.
//===============================================================================
public partial class TriadScreen {
	public int openHand		{ get { return _openHand; } set { _openHand = value; } }			protected int _openHand;
	public int countUnplayedCards		{ get { return _countUnplayedCards; } set { _countUnplayedCards = value; } }			protected int _countUnplayedCards;
	public int width		{ get { return _width; } set { _width = value; } }			protected int _width;
	public int height		{ get { return _height; } }			protected int _height;
	public int board		{ get { return _board; } }			protected int _board;
	public int playerName		{ get { return _playerName; } }			protected int _playerName;
	public int opponentName		{ get { return _opponentName; } }			protected int _opponentName;

	public void initialize(scene) {
		@scene = scene;
		@width              = 3;
		@height             = 3;
		@sameWins           = false;
		@openHand           = false;
		@wrapAround         = false;
		@elements           = false;
		@randomHand         = false;
		@countUnplayedCards = false;
		@trade              = 0;
	}

	public void maxCards() {
		numcards = @width * @height;
		if (numcards.odd()) {
			numcards = (numcards / 2) + 1;
		} else {
			numcards /= 2;
		}
		return numcards;
	}

	public bool isOccupied(x, y) {
		return @board[(y * @width) + x].owner != 0;
	}

	public void getOwner(x, y) {
		return @board[(y * @width) + x].owner;
	}

	public void getPanel(x, y) {
		return @board[(y * @width) + x];
	}

	public void quantity(items, item) {
		return ItemStorageHelper.quantity(items, item);
	}

	public void Add(items, item) {
		return ItemStorageHelper.add(items, Game.GameData.PokemonGlobal.triads.maxSize,
																TriadStorage.MAX_PER_SLOT, item, 1);
	}

	public void Subtract(items, item) {
		return ItemStorageHelper.remove(items, item, 1);
	}

	public void flipBoard(x, y, attackerParam = null, recurse = false) {
		panels = new {x - 1, y, x + 1, y, x, y - 1, x, y + 1};
		if (panels[0] < 0) panels[0] = (@wrapAround ? @width - 1 : 0);            // left
		if (panels[2] > @width - 1) panels[2] = (@wrapAround ? 0 : @width - 1);     // right
		if (panels[5] < 0) panels[5] = (@wrapAround ? @height - 1 : 0);           // top
		if (panels[7] > @height - 1) panels[7] = (@wrapAround ? 0 : @height - 1);   // bottom
		attacker = attackerParam.null() ? @board[(y * @width) + x] : attackerParam;
		flips = new List<string>();
		if (attackerParam && @board[(y * @width) + x].owner != 0) return null;
		if (!attacker.card || attacker.owner == 0) return null;
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			defenderX = panels[i * 2];
			defenderY = panels[(i * 2) + 1];
			defender  = @board[(defenderY * @width) + defenderX];
			if (!defender.card) continue;
			if (attacker.owner != defender.owner) {
				attack  = attacker.attack(i);
				defense = defender.defense(i);
				if (@elements) {
					// If attacker's type matches the tile's element, add
					// a bonus of 1 (only for original attacker, not combos)
					if (!recurse && attacker.type == attacker.card.type) attack += 1;
				} else {
					// Modifier depends on opponent's Pokémon type:
					// +1 - Super effective
					// -1 - Not very effective
					// -2 - Immune
//         attack += attacker.bonus(defender)
				}
				if (attack > defense || (attack == defense && @sameWins)) {
					flips.Add(new {defenderX, defenderY});
					if (attackerParam.null()) {
						defender.owner = attacker.owner;
						if (@sameWins) {
							// Combo with the "sameWins" rule
							ret = flipBoard(defenderX, defenderY, null, true);
							if (ret) flips.concat(ret);
						}
					} else {
						if (@sameWins) {
							// Combo with the "sameWins" rule
							ret = flipBoard(defenderX, defenderY, attackerParam, true);
							if (ret) flips.concat(ret);
						}
					}
				}
			}
		}
		return flips;
	}

	// If StartScreen includes parameters, it should
	// pass the parameters to StartScene.
	public void StartScreen(opponentName, minLevel, maxLevel, rules = null, oppdeck = null, prize = null) {
		if (minLevel < 0 || minLevel > 9) raise _INTL("Minimum level must be 0 through 9.");
		if (maxLevel < 0 || maxLevel > 9) raise _INTL("Maximum level must be 0 through 9.");
		if (maxLevel < minLevel) raise _INTL("Maximum level shouldn't be less than the minimum level.");
		if (rules.Length > 0 && rules.length > 0) {
			foreach (var rule in rules) { //'rules.each' do => |rule|
				if (rule == "samewins")		@sameWins			= true;
				if (rule == "openhand")		@openHand			= true;
				if (rule == "wrap")			@wrapAround			= true;
				if (rule == "elements")		@elements			= true;
				if (rule == "randomhand")	@randomHand			= true;
				if (rule == "countunplayed")@countUnplayedCards	= true;
				if (rule == "direct")		@trade				= 1   ;
				if (rule == "winall")		@trade				= 2   ;
				if (rule == "noprize")		@trade				= 3   ;
			}
		}
		@triadCards = new List<string>();
		count = 0;
		for (int i = Game.GameData.PokemonGlobal.triads.length; i < Game.GameData.PokemonGlobal.triads.length; i++) { //for 'Game.GameData.PokemonGlobal.triads.length' times do => |i|
			item = Game.GameData.PokemonGlobal.triads[i];
			ItemStorageHelper.add(@triadCards, Game.GameData.PokemonGlobal.triads.maxSize,
														TriadStorage.MAX_PER_SLOT, item[0], item[1]);
			count += item[1];   // Add item count to total count
		}
		@board = new List<string>();
		@playerName   = Game.GameData.player ? Game.GameData.player.name : "Trainer";
		@opponentName = opponentName;
		type_keys = GameData.Type.keys;
		for (int i = (@width * @height); i < (@width * @height); i++) { //for '(@width * @height)' times do => |i|
			square = new TriadSquare();
			if (@elements) {
				do { //loop; while (true);
					trial_type = type_keys.sample;
					type_data = GameData.Type.get(trial_type);
					if (type_data.pseudo_type) continue;
					square.type = type_data.id;
					break;
				}
			}
			@board.Add(square);
		}
		@scene.StartScene(self);   // (param1, param2)
		// Check whether there are enough cards.
		if (count < self.maxCards) {
			@scene.DisplayPaused(_INTL("You don't have enough cards."));
			@scene.EndScene;
			return 0;
		}
		// Set the player's cards.
		cards = new List<string>();
		if (@randomHand) {   // Determine hand at random
			self.maxCards.times do;
				randCard = @triadCards[rand(@triadCards.length)];
				Subtract(@triadCards, randCard[0]);
				cards.Add(randCard[0]);
			}
			@scene.ShowPlayerCards(cards);
		} else {
			cards = @scene.ChooseTriadCard(@triadCards);
		}
		// Set the opponent's cards.
		if (oppdeck.Length > 0 && oppdeck.length == self.maxCards) {   // Preset
			opponentCards = new List<string>();
			foreach (var species in oppdeck) { //'oppdeck.each' do => |species|
				species_data = GameData.Species.try_get(species);
				if (!species_data) {
					@scene.DisplayPaused(_INTL("Opponent has an illegal card, \"{1}\".", species));
					@scene.EndScene;
					return 0;
				}
				opponentCards.Add(species_data.id);
			}
		} else {
			species_keys = GameData.Species.keys;
			candidates = new List<string>();
			while (candidates.length < 200) {
				card = species_keys.sample;
				card_data = GameData.Species.get(card);
				card = card_data.id;   // Make sure it's a symbol
				triad = new TriadCard(card);
				total = triad.north + triad.south + triad.east + triad.west;
				// Add random species and its total point count
				candidates.Add(new {card, total});
				if (candidates.length < 200 && Game.GameData.player.owned(card_data.species)) {
					// Add again if player owns the species
					candidates.Add(new {card, total});
				}
			}
			// sort by total point count
			candidates.sort! { |a, b| a[1] <=> b[1] };
			opponentCards = new List<string>();
			self.maxCards.times do;
				// Choose random card from candidates based on trainer's level
				index = minLevel + rand(20);
				opponentCards.Add(candidates[index][0]);
			}
		}
		originalCards = cards.clone;
		originalOpponentCards = opponentCards.clone;
		@scene.NotifyCards(cards.clone, opponentCards.clone);
		@scene.ShowOpponentCards(opponentCards);
		@scene.Display(_INTL("Choosing the starting player..."));
		@scene.UpdateScore;
		playerTurn = (rand(2) == 0);
		@scene.Display(_INTL("{1} will go first.", (playerTurn) ? @playerName : @opponentName));
		for (int i = (@width * @height); i < (@width * @height); i++) { //for '(@width * @height)' times do => |i|
			position = null;
			triadCard = null;
			cardIndex = 0;
			if (playerTurn) {
				// Player's turn
				until position;
					cardIndex = @scene.PlayerChooseCard(cards.length);
					triadCard = new TriadCard(cards[cardIndex]);
					position = @scene.PlayerPlaceCard(cardIndex);
				}
			} else {
				// Opponent's turn
				@scene.Display(_INTL("{1} is making a move...", @opponentName));
				scores = new List<string>();
				for (int cardIdx = opponentCards.length; cardIdx < opponentCards.length; cardIdx++) { //for 'opponentCards.length' times do => |cardIdx|
					square = new TriadSquare();
					square.card = new TriadCard(opponentCards[cardIdx]);
					square.owner = 2;
					for (int j = (@width * @height); j < (@width * @height); j++) { //for '(@width * @height)' times do => |j|
						x = j % @width;
						y = j / @width;
						square.type = @board[j].type;
						flips = flipBoard(x, y, square);
						if (flips) scores.Add(new {cardIdx, x, y, flips.length});
					}
				}
				// Sort by number of flips
				scores.sort! { |a, b| (b[3] == a[3]) ? rand(-1..1) : b[3] <=> a[3] };
				scores = scores[0, opponentCards.length];   // Get the best results
				if (scores.length == 0) {
					@scene.Display(_INTL("{1} can't move somehow...", @opponentName));
					playerTurn = !playerTurn;
					continue;
				}
				result = scores[rand(scores.length)];
				cardIndex = result[0];
				triadCard = new TriadCard(opponentCards[cardIndex]);
				position = new {result[1], result[2]};
				@scene.OpponentPlaceCard(triadCard, position, cardIndex);
			}
			boardIndex = (position[1] * @width) + position[0];
			board[boardIndex].card  = triadCard;
			board[boardIndex].owner = playerTurn ? 1 : 2;
			flipBoard(position[0], position[1]);
			if (playerTurn) {
				cards.delete_at(cardIndex);
				@scene.EndPlaceCard(position, cardIndex);
			} else {
				opponentCards.delete_at(cardIndex);
				@scene.EndOpponentPlaceCard(position, cardIndex);
			}
			playerTurn = !playerTurn;
		}
		// Determine the winner
		playerCount   = 0;
		opponentCount = 0;
		for (int i = (@width * @height); i < (@width * @height); i++) { //for '(@width * @height)' times do => |i|
			if (board[i].owner == 1) playerCount   += 1;
			if (board[i].owner == 2) opponentCount += 1;
		}
		if (@countUnplayedCards) {
			playerCount   += cards.length;
			opponentCount += opponentCards.length;
		}
		result = 0;
		if (playerCount == opponentCount) {
			@scene.DisplayPaused(_INTL("The game is a draw."));
			result = 3;
			if (@trade == 1) {
				// Keep only cards of your color
				originalCards.each(crd => Game.GameData.PokemonGlobal.triads.remove(crd));
				cards.each(crd => Game.GameData.PokemonGlobal.triads.add(crd));
				for (int i = (@width * @height); i < (@width * @height); i++) { //for '(@width * @height)' times do => |i|
					if (board[i].owner == 1) {
						crd = GameData.Species.get_species_form(board[i].card.species, board[i].card.form).id;
						Game.GameData.PokemonGlobal.triads.add(crd);
					}
				}
				@scene.DisplayPaused(_INTL("Kept all cards of your color."));
			}
		} else if (playerCount > opponentCount) {
			@scene.DisplayPaused(_INTL("{1} won against {2}.", @playerName, @opponentName));
			result = 1;
			if (prize) {
				species_data = GameData.Species.try_get(prize);
				if (species_data && Game.GameData.PokemonGlobal.triads.add(species_data.id)) {
					@scene.DisplayPaused(_INTL("Got opponent's {1} card.", species_data.name));
				}
			} else {
				switch (@trade) {
					case 0:   // Gain 1 random card from opponent's deck
						card = originalOpponentCards[rand(originalOpponentCards.length)];
						if (Game.GameData.PokemonGlobal.triads.add(card)) {
							cardname = GameData.Species.get(card).name;
							@scene.DisplayPaused(_INTL("Got opponent's {1} card.", cardname));
						}
						break;
					case 1:   // Keep only cards of your color
						originalCards.each(crd => Game.GameData.PokemonGlobal.triads.remove(crd));
						cards.each(crd => Game.GameData.PokemonGlobal.triads.add(crd));
						for (int i = (@width * @height); i < (@width * @height); i++) { //for '(@width * @height)' times do => |i|
							if (board[i].owner == 1) {
								card = GameData.Species.get_species_form(board[i].card.species, board[i].card.form).id;
								Game.GameData.PokemonGlobal.triads.add(card);
							}
						}
						@scene.DisplayPaused(_INTL("Kept all cards of your color."));
						break;
					case 2:   // Gain all opponent's cards
						originalOpponentCards.each(crd => Game.GameData.PokemonGlobal.triads.add(crd));
						@scene.DisplayPaused(_INTL("Got all opponent's cards."));
						break;
				}
			}
		} else {
			@scene.DisplayPaused(_INTL("{1} lost against {2}.", @playerName, @opponentName));
			result = 2;
			switch (@trade) {
				case 0:   // Lose 1 random card from your deck
					card = originalCards[rand(originalCards.length)];
					Game.GameData.PokemonGlobal.triads.remove(card);
					cardname = GameData.Species.get(card).name;
					@scene.DisplayPaused(_INTL("Opponent won your {1} card.", cardname));
					break;
				case 1:   // Keep only cards of your color
					originalCards.each(crd => Game.GameData.PokemonGlobal.triads.remove(card));
					cards.each(crd => Game.GameData.PokemonGlobal.triads.add(crd));
					for (int i = (@width * @height); i < (@width * @height); i++) { //for '(@width * @height)' times do => |i|
						if (board[i].owner == 1) {
							card = GameData.Species.get_species_form(board[i].card.species, board[i].card.form).id;
							Game.GameData.PokemonGlobal.triads.add(card);
						}
					}
					@scene.DisplayPaused(_INTL("Kept all cards of your color.", cardname));
					break;
				case 2:   // Lose all your cards
					originalCards.each(crd => Game.GameData.PokemonGlobal.triads.remove(crd));
					@scene.DisplayPaused(_INTL("Opponent won all your cards."));
					break;
			}
		}
		@scene.EndScene;
		return result;
	}
}

//===============================================================================
// Start duel.
//===============================================================================
public bool CanTriadDuel() {
	card_count = Game.GameData.PokemonGlobal.triads.total_cards;
	return card_count >= PokemonGlobalMetadata.MINIMUM_TRIAD_CARDS;
}

public void TriadDuel(name, minLevel, maxLevel, rules = null, oppdeck = null, prize = null) {
	ret = 0;
	FadeOutInWithMusic do;
		scene = new TriadScene();
		screen = new TriadScreen(scene);
		ret = screen.StartScreen(name, minLevel, maxLevel, rules, oppdeck, prize);
	}
	return ret;
}

//===============================================================================
// Card storage.
//===============================================================================
public partial class PokemonGlobalMetadata {
	public int triads		{ get { return _triads; } }			protected int _triads;

	public const int MINIMUM_TRIAD_CARDS = 5;

	public void triads() {
		if (!@triads) @triads = new TriadStorage();
		return @triads;
	}
}

//===============================================================================
//
//===============================================================================
public partial class TriadStorage {
	public int items		{ get { return _items; } }			protected int _items;

	public const int MAX_PER_SLOT = 999;   // Max. number of items per slot

	public void initialize() {
		@items = new List<string>();
	}

	public int this[int i] { get {
		return @items[i];
		}
	}

	public void length() {
		return @items.length;
	}

	public bool empty() {
		return @items.length == 0;
	}

	public void maxSize() {
		return @items.length + 1;
	}

	public void clear() {
		@items.clear;
	}

	public void get_item(index) {
		if (index < 0 || index >= @items.length) return null;
		return @items[index][0];
	}

	// Number of the item in the given index
	public void get_item_count(index) {
		if (index < 0 || index >= @items.length) return 0;
		return @items[index][1];
	}

	public void quantity(item) {
		return ItemStorageHelper.quantity(@items, item);
	}

	public bool can_add(item, qty = 1) {
		return ItemStorageHelper.can_add(@items, self.maxSize, MAX_PER_SLOT, item, qty);
	}

	public void add(item, qty = 1) {
		return ItemStorageHelper.add(@items, self.maxSize, MAX_PER_SLOT, item, qty);
	}

	public void remove(item, qty = 1) {
		return ItemStorageHelper.remove(@items, item, qty);
	}

	public void total_cards() {
		ret = 0;
		@items.each(itm => ret += itm[1]);
		return ret;
	}
}

//===============================================================================
// Card shop screen.
//===============================================================================
public void BuyTriads() {
	commands = new List<string>();
	realcommands = new List<string>();
	foreach (var s in GameData.Species) { //GameData.Species.each_species do => |s|
		if (!Game.GameData.player.owned(s.species)) continue;
		price = new TriadCard(s.id).price;
		commands.Add(new {price, s.name, _INTL("{1} - ${2}", s.name, price.to_s_formatted), s.id});
	}
	if (commands.length == 0) {
		Message(_INTL("There are no cards that you can buy."));
		return;
	}
	commands.sort! { |a, b| a[1] <=> b[1] };   // Sort alphabetically
	foreach (var command in commands) { //'commands.each' do => |command|
		realcommands.Add(command[2]);
	}
	// Scroll right before showing screen
	ScrollMap(4, 3, 5);
	cmdwindow = Window_CommandPokemonEx.newWithSize(realcommands, 0, 0, Graphics.width / 2, Graphics.height);
	cmdwindow.z = 99999;
	goldwindow = Window_UnformattedTextPokemon.newWithSize(
		_INTL("Money:\n{1}", GetGoldString), 0, 0, 32, 32
	);
	goldwindow.resizeToFit(goldwindow.text, Graphics.width);
	goldwindow.x = Graphics.width - goldwindow.width;
	goldwindow.y = 0;
	goldwindow.z = 99999;
	preview = new Sprite();
	preview.x = (Graphics.width * 3 / 4) - 40;
	preview.y = (Graphics.height / 2) - 48;
	preview.z = 4;
	preview.bitmap = new TriadCard(commands[cmdwindow.index][3]).createBitmap(1);
	olditem = commands[cmdwindow.index][3];
	Graphics.frame_reset;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		cmdwindow.active = true;
		cmdwindow.update;
		goldwindow.update;
		if (commands[cmdwindow.index][3] != olditem) {
			preview.bitmap&.dispose;
			preview.bitmap = new TriadCard(commands[cmdwindow.index][3]).createBitmap(1);
			olditem = commands[cmdwindow.index][3];
		}
		if (Input.trigger(Input.BACK)) {
			break;
		} else if (Input.trigger(Input.USE)) {
			price    = commands[cmdwindow.index][0];
			item     = commands[cmdwindow.index][3];
			itemname = commands[cmdwindow.index][1];
			cmdwindow.active = false;
			cmdwindow.update;
			if (Game.GameData.player.money < price) {
				Message(_INTL("You don't have enough money."));
				continue;
			}
			maxafford = (price <= 0) ? 99 : Game.GameData.player.money / price;
			if (maxafford > 99) maxafford = 99;
			params = new ChooseNumberParams();
			params.setRange(1, maxafford);
			params.setInitialValue(1);
			params.setCancelValue(0);
			quantity = MessageChooseNumber(
				_INTL("The {1} card? Certainly. How many would you like?", itemname), params
			);
			if (quantity <= 0) continue;
			price *= quantity;
			if (!ConfirmMessage(_INTL("{1}, and you want {2}. That will be ${3}. OK?", itemname, quantity, price.to_s_formatted))) continue;
			if (Game.GameData.player.money < price) {
				Message(_INTL("You don't have enough money."));
				continue;
			}
			if (!Game.GameData.PokemonGlobal.triads.can_add(item, quantity)) {
				Message(_INTL("You have no room for more cards."));
				continue;
			}
			Game.GameData.PokemonGlobal.triads.add(item, quantity);
			Game.GameData.player.money -= price;
			goldwindow.text = _INTL("Money:\n{1}", GetGoldString);
			Message(_INTL("Here you are! Thank you!") + "\\se[Mart buy item]");
		}
	}
	cmdwindow.dispose;
	goldwindow.dispose;
	preview.bitmap&.dispose;
	preview.dispose;
	Graphics.frame_reset;
	// Scroll right before showing screen
	ScrollMap(6, 3, 5);
}

public void SellTriads() {
	total_cards = 0;
	commands = new List<string>();
	for (int i = Game.GameData.PokemonGlobal.triads.length; i < Game.GameData.PokemonGlobal.triads.length; i++) { //for 'Game.GameData.PokemonGlobal.triads.length' times do => |i|
		item = Game.GameData.PokemonGlobal.triads[i];
		speciesname = GameData.Species.get(item[0]).name;
		commands.Add(_INTL("{1} x{2}", speciesname, item[1]));
		total_cards += item[1];
	}
	commands.Add(_INTL("CANCEL"));
	if (total_cards == 0) {
		Message(_INTL("You have no cards."));
		return;
	}
	// Scroll right before showing screen
	ScrollMap(4, 3, 5);
	cmdwindow = Window_CommandPokemonEx.newWithSize(commands, 0, 0, Graphics.width / 2, Graphics.height);
	cmdwindow.z = 99999;
	goldwindow = Window_UnformattedTextPokemon.newWithSize(
		_INTL("Money:\n{1}", GetGoldString), 0, 0, 32, 32
	);
	goldwindow.resizeToFit(goldwindow.text, Graphics.width);
	goldwindow.x = Graphics.width - goldwindow.width;
	goldwindow.y = 0;
	goldwindow.z = 99999;
	preview = new Sprite();
	preview.x = (Graphics.width * 3 / 4) - 40;
	preview.y = (Graphics.height / 2) - 48;
	preview.z = 4;
	item = Game.GameData.PokemonGlobal.triads.get_item(cmdwindow.index);
	preview.bitmap = new TriadCard(item).createBitmap(1);
	olditem = Game.GameData.PokemonGlobal.triads.get_item(cmdwindow.index);
	done = false;
	Graphics.frame_reset;
	until done;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwindow.active = true;
			cmdwindow.update;
			goldwindow.update;
			item = Game.GameData.PokemonGlobal.triads.get_item(cmdwindow.index);
			if (olditem != item) {
				preview.bitmap&.dispose;
				if (item) preview.bitmap = new TriadCard(item).createBitmap(1);
				olditem = item;
			}
			if (Input.trigger(Input.BACK)) {
				done = true;
				break;
			}
			if (Input.trigger(Input.USE)) {
				if (cmdwindow.index >= Game.GameData.PokemonGlobal.triads.length) {
					done = true;
					break;
				}
				item = Game.GameData.PokemonGlobal.triads.get_item(cmdwindow.index);
				itemname = GameData.Species.get(item).name;
				quantity = Game.GameData.PokemonGlobal.triads.quantity(item);
				price = new TriadCard(item).price;
				if (price == 0) {
					DisplayPaused(_INTL("The {1} card? Oh, no. I can't buy that.", itemname));
					break;
				}
				cmdwindow.active = false;
				cmdwindow.update;
				if (quantity > 1) {
					params = new ChooseNumberParams();
					params.setRange(1, quantity);
					params.setInitialValue(1);
					params.setCancelValue(0);
					quantity = MessageChooseNumber(
						_INTL("The {1} card? How many would you like to sell?", itemname), params
					);
				}
				if (quantity > 0) {
					price /= 4;
					price *= quantity;
					if (ConfirmMessage(_INTL("I can pay ${1}. Would that be OK?", price.to_s_formatted))) {
						Game.GameData.player.money += price;
						goldwindow.text = _INTL("Money:\n{1}", GetGoldString);
						Game.GameData.PokemonGlobal.triads.remove(item, quantity);
						Message(_INTL("Turned over the {1} card and received ${2}.", itemname, price.to_s_formatted) + "\\se[Mart buy item]");
						commands = new List<string>();
						for (int i = Game.GameData.PokemonGlobal.triads.length; i < Game.GameData.PokemonGlobal.triads.length; i++) { //for 'Game.GameData.PokemonGlobal.triads.length' times do => |i|
							item = Game.GameData.PokemonGlobal.triads[i];
							speciesname = GameData.Species.get(item[0]).name;
							commands.Add(_INTL("{1} x{2}", speciesname, item[1]));
						}
						commands.Add(_INTL("CANCEL"));
						cmdwindow.commands = commands;
						break;
					}
				}
			}
		}
	}
	cmdwindow.dispose;
	goldwindow.dispose;
	preview.bitmap&.dispose;
	preview.dispose;
	Graphics.frame_reset;
	// Scroll right before showing screen
	ScrollMap(6, 3, 5);
}

public void TriadList() {
	total_cards = 0;
	commands = new List<string>();
	for (int i = Game.GameData.PokemonGlobal.triads.length; i < Game.GameData.PokemonGlobal.triads.length; i++) { //for 'Game.GameData.PokemonGlobal.triads.length' times do => |i|
		item = Game.GameData.PokemonGlobal.triads[i];
		speciesname = GameData.Species.get(item[0]).name;
		commands.Add(_INTL("{1} x{2}", speciesname, item[1]));
		total_cards += item[1];
	}
	commands.Add(_INTL("CANCEL"));
	if (total_cards == 0) {
		Message(_INTL("You have no cards."));
		return;
	}
	cmdwindow = Window_CommandPokemonEx.newWithSize(commands, 0, 0, Graphics.width / 2, Graphics.height);
	cmdwindow.z = 99999;
	sprite = new Sprite();
	sprite.x = (Graphics.width / 2) + 40;
	sprite.y = 48;
	sprite.z = 99999;
	done = false;
	lastIndex = -1;
	until done;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			cmdwindow.update;
			if (lastIndex != cmdwindow.index) {
				sprite.bitmap&.dispose;
				if (cmdwindow.index < Game.GameData.PokemonGlobal.triads.length) {
					sprite.bitmap = new TriadCard(Game.GameData.PokemonGlobal.triads.get_item(cmdwindow.index)).createBitmap(1);
				}
				lastIndex = cmdwindow.index;
			}
			if (Input.trigger(Input.BACK) ||
				(Input.trigger(Input.USE) && cmdwindow.index >= Game.GameData.PokemonGlobal.triads.length)) {
				done = true;
				break;
			}
		}
	}
	cmdwindow.dispose;
	sprite.dispose;
}

//===============================================================================
// Give the player a particular card.
//===============================================================================
public void GiveTriadCard(species, quantity = 1) {
	sp = GameData.Species.try_get(species);
	if (!sp) return false;
	if (!Game.GameData.PokemonGlobal.triads.can_add(sp.id, quantity)) return false;
	Game.GameData.PokemonGlobal.triads.add(sp.id, quantity);
	return true;
}
