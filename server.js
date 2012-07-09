#!/home/zed0/node-v0.8.2/node
var net = require('net');
var crypto = require('crypto');
var fs = require('fs');

var authFile = './auth.json';
var tickLength = 500;
var maxGameLength = 1000;
var port = 8037;
var mapSize = [13,13];
var maxPlayers = 6;
var bombTimer = 5;
var bombRadius = 3;

var auth = {};
try {
	if(fs.statSync(authFile).isFile()) {
		auth = JSON.parse(fs.readFileSync(authFile));
	}
} catch(e) {
	if(e.code == 'ENOENT') {
		console.log('No auth file, one will be created.');
	}
}

process.on('uncaughtException', function (err) {
	console.log('Caught exception: ' + err);
});

var clients = {};

var currentGame = new game(Object.keys(clients));

var server = net.createServer(function(c) { //'connection' listener
	console.log('client connected');
	if(this.clientID === undefined) {
		this.clientID = 0;
	}
	var id = this.clientID++;
	console.log(id);
	clients[id] = c;
	currentGame.addPlayer(id);

	c.on('end', function() {
	});

	c.on('data', function(data) {
		var cleanData = data.toString().trim();
		currentGame.handleCommand(id, cleanData);
	});

	c.on('error', function(err) {
		console.log('client error');
	});

	c.on('close', function() {
		removeClient(id);
	});

	var removeClient = function(id) {
		try{
			clients[id].destroy();
			delete clients[id];
			currentGame.removeClient(id);
			console.log('client disconnected');
		} catch(e) {
			console.log('Error!');
		}
	};
	this.removeClient = removeClient;
});

server.listen(port, function() { //'listening' listener
	console.log('Server listening on port ' + port);
});


function game(initialPlayers) {
	var players = {};
	var bombs = new bombMap(mapSize);
	var state = 'INIT';
	var currentMap = new map(mapSize);
	var timers = [];
	var intervals = [];

	var init = function() {
		console.log('Initialising game:');

		//Choose players:
		var registeredPlayers = Object.keys(players).filter(function(id){
			return players[id].status == 'REGISTERED';
		});
		//Sort randomly:
		registeredPlayers.sort(function(a,b){return 0.5 - Math.random()});
		//Take first results from resultant array:
		var playingPlayers = registeredPlayers.slice(0,maxPlayers);
		playingPlayers.forEach(function(id){
			players[id].status = 'PLAYING';
			players[id].row = Math.floor(Math.random()*currentMap.getX()/2)*2;
			players[id].col = Math.floor(Math.random()*currentMap.getY()/2)*2;
			currentMap.clearCross(players[id].row, players[id].col);
		});

		currentMap.print();
		broadcast('MAP ' + currentMap.getX() + ' ' + currentMap.getY());
		broadcast(currentMap.describe());

		broadcast('PLAYERS ' + playingPlayers.length);
		playingPlayers.forEach(function(id){
			broadcast(players[id].name + ' ' + players[id].row + ' ' + players[id].col);
		});

		console.log('Connected players:');
		console.log(players);
		state = 0;
		intervals.push(setInterval(tick, tickLength));
	};

	var tick = function() {
		if(++state > maxGameLength) {
			stop();
		} else {
			console.log(players);

			//Evaluate movements:
			var moves = updatePlayers();
			var numMoves = Object.keys(moves).length
			if(numMoves != 0)
			{
				broadcast('ACTIONS ' + numMoves);
				Object.keys(moves).forEach(function(id){
					broadcast(players[id].name + ' ' + moves[id]);
				});
			}

			//Evaluate bombs:
			var explosions = bombs.update();
			var dead = evaluateExplosions(explosions);

			//Clear actions:
			Object.keys(players).forEach(function(id){
				if(players[id].status == 'PLAYING')
				{
					delete players[id].action;
				}
			});

			if(dead.length != 0) {
				broadcast('DEAD ' + dead.length);
				dead.forEach(function(id){
					broadcast(unescape(players[id].name));
					players[id].status = 'DEAD';
				});
			}
			//currentMap.print();
			console.log('TICK ' + state);
			broadcast('TICK ' + state);
		}
	};

	var evaluateExplosions = function(explosionList) {
		var dead = [];
		var explosions = new explosionMap(mapSize, currentMap);
		explosionList.forEach(function(pos){
			console.log('Explosion at ' + pos[0] +','+ pos[1]);
			explosions.explode(pos[0], pos[1]);
		});

		for(var i=0; i<mapSize[0]; ++i) {
			for(var j=0; j<mapSize[1]; ++j) {
				if(explosions.getTile([i],[j])) {
					currentMap.destroyBlock(i,j);
				}
			}
		}
		var map = explosions.getTiles();
		Object.keys(players).forEach(function(id){
			if(players[id].status == 'PLAYING') {
				if(explosions.getTile(players[id].row,players[id].col)) {
					dead.push(id);
				}
			}
		});
		return dead;
	};

	var updatePlayers = function() {
		var moves = {};
		Object.keys(players).forEach(function(id){
			if(players[id].status == 'PLAYING' && players[id].action != undefined)
			{
				moves[id] = players[id].action;
				if(players[id].action == 'BOMB') {
					bombs.addBomb(players[id].row, players[id].col);
				} else {
					var dest = [players[id].row, players[id].col];
					if(players[id].action == 'UP') {
						dest[0]--;
					} else if(players[id].action == 'DOWN') {
						dest[0]++;
					} else if(players[id].action == 'LEFT') {
						dest[1]--;
					} else if(players[id].action == 'RIGHT') {
						dest[1]++;
					}
					if(canMove(dest[0], dest[1])) {
						players[id].row = dest[0];
						players[id].col = dest[1];
					}
				}
			}
		});
		return moves;
	};

	var addPlayer = function(client) {
		players[client] = {};
	};
	this.addPlayer = addPlayer;

	this.removeClient = function(client) {
		delete players[client];
		console.log('removed');
	};

	var canMove = function(row, col) {
		if((row>=0 && row<currentMap.getX()) && (col>=0 && col<currentMap.getY())) {
			if(currentMap.getTile(row,col) == 0) {
				if(bombs.getTile([row],[col]) == undefined || bombs.getTile([row],[col]) < 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	var broadcast = function(message) {
		Object.keys(players).forEach(function(player){
			sendMessage(player, message);
		});
	};

	var sendMessage = function(player, message) {
		if(message.substr(-1) != '\n') {
			message += '\n';
		}
		try {
			clients[player].write(message);
		} catch(e) {
			console.log('Catch: Warning: ' + player + ' has disconnected');
			server.removeClient(player);
		}
	};

	var stop = function() {
		console.log('STOP');
		broadcast('STOP');
		timers.forEach(function(timer){
			clearTimeout(timer);
		});
		intervals.forEach(function(interval){
			clearInterval(interval);
		});
		currentGame = new game(Object.keys(clients));
	};
	this.stop = stop;

	var registerPlayer = function(id, strings) {
		console.log('currentState:' + state);
		if(state != 'INIT') {
			sendMessage(id, 'E_NOT_INIT');
			return;
		}
		if(strings[0] == undefined) {
			sendMessage(id, 'E_NO_NAME');
			return;
		}
		if(strings[1] == undefined) {
			sendMessage(id, 'E_NO_PASS');
			return;
		}
		if(checkPass(strings[0], strings[1])) {
			players[id].status = 'REGISTERED';
			players[id].name = escape(strings[0]);
			sendMessage(id, 'REGISTERED');
		} else {
			sendMessage(id, 'E_WRONG_PASS');
		}
	};

	var checkPass = function(name, password) {
		if(auth[name] != undefined) {
			var hash = crypto.createHash('sha512');
			salt = auth[name].salt;
			hash.update(salt + escape(password));
			var digest = hash.digest('hex');
			if(digest == auth[name].hash) {
				return true;
			}
		} else {
			var hash = crypto.createHash('sha512');
			var salt = crypto.randomBytes(32).toString('hex');
			hash.update(salt + escape(password));
			var digest = hash.digest('hex');
			auth[name] = {
				'salt': salt,
				'hash': digest
			};
			fs.writeFileSync(authFile, JSON.stringify(auth));
			return true;
		}
		return false;
	};

	var setPlayerAction = function(id, strings) {
		if(players[id].status == 'PLAYING') {
			if(players[id].action == undefined) {
				var validActions = ['UP', 'DOWN', 'LEFT', 'RIGHT', 'BOMB'];
				if(validActions.indexOf(strings[0]) != -1) {
					players[id].action = strings[0];
					sendMessage(id, strings[0]);
				} else {
					sendMessage(id, 'E_INVALID_ACTION');
				}
			} else {
				sendMessage(id, 'E_TOO_MANY_ACTIONS');
			}
		} else {
			sendMessage(id, 'E_NOT_PLAYING');
		}
	};

	this.handleCommand = function(id, command) {
		var strings = command.split(' ');
		switch(strings[0]){
			case 'STOP':
				stop();
				break;
			case 'REGISTER':
				registerPlayer(id, strings.slice(1));
				break;
			case 'ACTION':
				setPlayerAction(id, strings.slice(1));
				break;
		}
	};

	console.log('INIT');
	initialPlayers.forEach(function(element){
		addPlayer(element);
	});
	broadcast('INIT');
	timers.push(setTimeout(init, tickLength));
}

function map(size,players) {
	var tiles = [];
	var x = size[0];
	var y = size[1];

	this.getX = function(){return x};
	this.getY = function(){return y};
	this.getTile = function(row, col){return tiles[row][col]};

	var generate = function() {
		var wallChance = 0.7;
		for(var i=0; i<x; ++i) {
			tiles[i] = [];
			for(var j=0; j<y; ++j) {
				if(i%2 && j%2) {
					tiles[i][j] = 2;
				} else {
					if(Math.random() > wallChance) {
						tiles[i][j] = 0;
					} else {
						tiles[i][j] = 1;
					}
				}
			}
		}
	};

	var clearCross = function(xPos, yPos) {
		console.log('clearing:' + xPos + ',' + yPos);
		destroyBlock(xPos-1,yPos);
		destroyBlock(xPos,yPos-1);
		destroyBlock(xPos,yPos);
		destroyBlock(xPos,yPos+1);
		destroyBlock(xPos+1,yPos);
	};
	this.clearCross = clearCross;

	this.describe = function() {
		var result = '';
		for(var i=0; i<x; ++i) {
			if(i != 0) {
				result += '\n';
			}
			for(var j=0; j<y; ++j) {
				if(j != 0) {
					result += ' ';
				}
				result += tiles[i][j].toString();
			}
		}
		return result;
	};

	this.print = function() {
		tiles.forEach(function(row){
			row.forEach(function(cell){
				if(cell == 0) {
					process.stdout.write(' ');
				} else if(cell == 1) {
					process.stdout.write('0');
				} else if(cell == 2) {
					process.stdout.write('#');
				} else {
					process.stdout.write('?');
				}
			});
			process.stdout.write('\n');
		});
	};

	var destroyBlock = function(xPos, yPos) {
		if((xPos >= 0 && xPos < x) && (yPos >= 0 && yPos < y)) {
			if(tiles[xPos][yPos] != 2) {
				tiles[xPos][yPos] = 0;
			}
		}
	}
	this.destroyBlock = destroyBlock;

	generate();
}

function bombMap(mapSize) {
	var sizeX = mapSize[0];
	var sizeY = mapSize[1];
	var bombs = new Array();

	this.getTile = function(posX, posY) {
		return bombs[posX][posY];
	};

	this.addBomb = function(posX, posY) {
		if(!bombs[posX][posY]) {
			bombs[posX][posY] = 0;
		}
	};

	this.update = function() {
		var explosions = [];
		for(var i=0; i<bombs.length; ++i) {
			for(var j=0; j<bombs[i].length; ++j) {
				if(bombs[i][j] !== undefined)
				{
					bombs[i][j]++;
					if(bombs[i][j] > bombTimer)
					{
						explosions.push([i,j]);
						//explode!
						delete bombs[i][j];
					}
				}
			}
		}
		return explosions;
	};

	this.print = function() {
		console.log(bombs);
	};

	for(var i=0; i<sizeX; ++i) {
		bombs[i] = new Array(sizeY);
	}
}

function explosionMap(mapSize, currentMap) {
	var explosions = [];
	var currentMap = currentMap;

	var getTiles = function(){return explosions};
	this.getTiles = getTiles;

	var getTile = function(row, col) {
		return explosions[row][col];
	};
	this.getTile = getTile;

	this.explode = function(row, col) {
		explosions[row][col] = 1;
		explodeInDirection(row, col, 1, 0);
		explodeInDirection(row, col, -1, 0);
		explodeInDirection(row, col, 0, 1);
		explodeInDirection(row, col, 0, -1);
	};

	var explodeInDirection = function(row, col, dir0, dir1) {
		for(var i=1; i<=bombRadius; ++i) {
			curRow = row+(i*dir0);
			curCol = col+(i*dir1);
			if(curRow >=0 && curRow < mapSize[0] && curCol >= 0 && curCol < mapSize[1]) {
				explosions[curRow][curCol] = 1;
				//stop exploding in this direction if there is a block:
				if(currentMap.getTile([curRow],[curCol]) > 0) {
					break;
				}
			} else {
				break;
			}
		}
	}

	for(var i=0; i<mapSize[0]; ++i) {
		explosions[i] = new Array(mapSize[1]);
		for(var j=0; j<mapSize[1]; ++j) {
			explosions[i][j] = 0;
		}
	}
}
