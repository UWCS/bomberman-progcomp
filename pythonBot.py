import copy, random, socket, sys

class Bomberman:

	def __init__(self, account, password):
	
		self.TCP_IP = 'uwcs.co.uk'
		self.TCP_PORT = 8037
		self.BUFFER_SIZE = 1024	
		self.account = account
		self.password = password
		self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.connect()
		self.grid = []
		self.numberPlayers = 0
		self.playerInfo = {}
		self.inGame = False
		self.sentAction = False
		self.bombs = []
		self.explosions = set()
		self.out = False
		
		#receive sever data
		self.getMessages() 
					
	def connect(self):
		try:			
			self.s.connect((self.TCP_IP, self.TCP_PORT))
		except:
			print "Failed: Could not connect"
			sys.exit()
			
	def sendMessage(self, message):
		print "Sent: ", message, "\n"
		self.s.send(message)
			def getMessages(self):
		while 1:	
			data = self.read_line()
			self.parseline(data)
			
	def sendAction(self, action):
		#Add in a check so only valid actions are sent.
		if not self.sentAction and self.inGame:
			self.sendMessage(" ".join(["ACTION", action]))
			self.sentAction = True
			
	def printMap(self):
		if self.out:
			if self.grid != []:
				g = copy.deepcopy(self.grid)
				
				for i in self.bombs:
					g[i[0]][i[1]] = "B"
				
				for i in self.playerInfo:
					if self.playerInfo[i].alive:
						g[self.playerInfo[i].position[0]][self.playerInfo[i].position[1]] = "X"
							
				for i in self.explosions:
					g[i[0]][i[1]] = "*"
					
				print "-" * ((self.cols * 2) - 1)
				for j in g:
					s = " ".join(j)
					# Changes output to a more readable format
					s = s.replace("0", " ").replace("1", "+").replace("2", "#")
					print s
				print "-" * ((self.cols * 2) - 1)
			
	def read_line(self):
		ret = ''

		while True:
			c = self.s.recv(1)

			if c == '\n' or c == '':
				break
			else:
				ret = ''.join([ret,c])
		
		# Prints all data sent from server
		if self.out:
			print "Server: ", ret
		
		return ret
			
	def parseline(self, l):
		phrases = { "DEAD": self.dead, "INIT": self.register, "MAP": self.createMap, "PLAYERS": self.players, "REGISTERED": self.registered, "STOP": self.stop, "END": self.stop, "TICK": self.tick, "ACTIONS": self.actions, "LEFT": self.confirmation, "RIGHT": self.confirmation, "DOWN": self.confirmation, "UP": self.confirmation, "BOMB": self.confirmation } 
		
		# { "END": end, "SCORES": scores }
		
		x = l.split()
		phrases[x[0]](x)			
		
	def register(self, x):
		m = "REGISTER " + self.account + " " + self.password
		self.sendMessage(m)
		self.out = True
		
	def registered(self, x):
		self.inGame = True
	
	def stop(self, x):
		#reset everything and wait for next round
		self.grid = []
		self.numberPlayers = 0
		self.playerInfo = {}
		self.inGame = False
		self.sentAction = False	
		self.rows = 0
		self.cols = 0
		self.bombs = []
		self.explosions = set()
		self.out = False
		
	def confirmation(self, x):
		pass
		
	def actions(self, x):
		# Update with last turns actions
		if x[1] != "0":
			for i in range(int(x[1])):
				y = self.read_line().split()
				if y[1] == "BOMB":
					self.dropBomb(self.playerInfo[y[0]].position)
				elif self.isLegal(self.playerInfo[y[0]].position,y[1]):					
					self.playerInfo[y[0]].move(y[1])
				
		# Simulate Bombs
		self.updateBombs()
		
		# Perform a move
		if self.inGame:
			self.performTurn()
			
	def isLegal(self, position, move):
		r = position[0]
		c = position[1]
		
		if move == "LEFT" and c != 0:
			if self.grid[r][c-1] == "0":
				return 1
		elif move == "RIGHT" and c != (self.cols - 1):
			if self.grid[r][c+1] == "0":
				return 1
		elif move == "UP" and r != 0:
			if self.grid[r-1][c] == "0":
				return 1
		elif move == "DOWN" and r != (self.rows - 1):
			if self.grid[r+1][c] == "0":
				return 1
			
		
	def dropBomb(self, x):
		self.bombs.append([x[0],x[1],4])
		
	def updateBombs(self):
		for i in self.bombs:
			i[2] -= 1
			if i[2] == 0:				
				self.explodeBomb(i)
				
	def explodeBomb(self, x):
		self.bombs.remove(x)
		self.explode(x[0], x[1])
		
		#Left
		for i in range(1,4):
			if self.explode(x[0], x[1]-i):
				break
			# print "Left ", i 
			
		#Right
		for i in range(1,4):
			if self.explode(x[0], x[1]+i):
				break
			# print "Right ", i 
		#Up
		for i in range(1,4):
			if self.explode(x[0]-i, x[1]):
				break
			# print "Up ", i 
			
		#Down
		for i in range(1,4):
			if self.explode(x[0]+i, x[1]):
				break
			# print "Down ", i 
				
	def explode(self, r, c):
		
		if r < 0 or r >= self.rows or c < 0 or c >= self.cols:
			return 1
			
		for i in self.playerInfo:
			if self.playerInfo[i].position[0] == r and self.playerInfo[i].position[1] == c:
				self.playerInfo[i].kill()
				if self.playerInfo[i].name == self.account:
					self.inGame = False

		if self.grid[r][c] == "0":
			self.explosions.add((r,c))
			for i in self.bombs:
				if i[0] == r and i[1] == c:
					self.explodeBomb(i)
					break								
			return 0
		elif self.grid[r][c] == "1":
			self.explosions.add((r,c))
			self.grid[r][c] = "0"
			return 1
		elif self.grid[r][c] == "2":
			return 1
	
	def dead(self, x):
		for i in range(int(x[1])):
			y = self.read_line().split()
			self.playerInfo[y[0]].kill()
			if y[0] == self.account:
				self.inGame = False
				self.out = False
				
			
	def createMap(self, x):
		self.rows = int(x[1])
		self.cols = int(x[2])
		for i in range(self.rows):
			y = self.read_line().split()
			self.grid.append(y)
			
	def players(self, x):
		self.numberPlayers = int(x[1])
		for i in range(self.numberPlayers):
			y = self.read_line().split()
			self.playerInfo[y[0]] = Player(y[0],int(y[1]),int(y[2]))		
	def tick(self, x):
		if self.inGame:
			num = int(x[1])
			self.sentAction = False
			
	def performTurn(self):
		if self.inGame:	
			self.performAction()
			
		self.printMap()
		
		print "\n---|||----\n"
			
	def performAction(self):
		i = random.randint(0,5)
		
		if i == 0:
			self.plantBomb()
		# elif i == 1:
			# pass
		else:
			self.moveToEmptySpace()			
			
			
	
	def moveToEmptySpace(self):
		directions = ["LEFT", "UP", "RIGHT", "DOWN"]
		r = self.playerInfo[self.account].position[0]
		c =	self.playerInfo[self.account].position[1]
		
		if r == 0:
			directions.remove("UP")
		elif r == (self.rows - 1):
			directions.remove("DOWN")
			
		if c == 0:
			directions.remove("LEFT")
		elif c == (self.cols - 1):
			directions.remove("RIGHT")
		
		random.shuffle(directions)
		
		for i in directions:
			if self.tryMove(r, c, i):
				# self.read_line()
				break
				
	def tryMove(self, r, c, d):
		if d == "LEFT":
			if self.grid[r][c-1] == "0":
				self.sendAction(d)
				return 1
		elif d == "RIGHT":
			if self.grid[r][c+1] == "0":
				self.sendAction(d)
				return 1
		elif d == "UP":
			if self.grid[r-1][c] == "0":
				self.sendAction(d)
				return 1
		elif d == "DOWN":
			if self.grid[r+1][c] == "0":
				self.sendAction(d)
				return 1				
			
		return 0
		
	def plantBomb(self):
		self.sendAction("BOMB")

		
class Player:
	
	def __init__(self, name, r, c):
		
		self.name = name
		self.position = [r, c]
		self.alive = True
	
	def getData(self):
		print "Name: ", self.name
		print "Position: ", self.position
		print "Alive: ", self.alive
		
	def move(self, d):
		if d == "LEFT":
			self.position[1] -= 1
		elif d == "RIGHT":
			self.position[1] += 1
		elif d == "UP":
			self.position[0] -= 1
		elif d == "DOWN":
			self.position[0] += 1
			
	def kill(self):
		self.alive = False
		
		
x = Bomberman("PythonBot", "abcde")