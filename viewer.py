import copy, os, socket, sys

TCP_IP = 'uwcs.co.uk'
TCP_PORT = 8037
BUFFER_SIZE = 1024gameOn = False
grid = []
players = {}
rows = 0cols = 0
last = []
z = ''
bombs = [] # row, col, timer
explosions = set()
scores = []


try:
	s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	s.connect((TCP_IP, TCP_PORT))
except:
	print "Failed"

def clear():
	if sys.platform == "win32" or sys.platform == "win64":
		os.system('CLS')
	else:
		os.system('clear')
		
def displayScores():
	#Score for last game
	
	if scores != []:
		print "\nScores:"
		for i in scores:
			print i[0], i[1]

def display():
	clear()
	global grid, last, explosions
	
	# Players
	if len(players) != 0:
		print "Players:"
		k = 65
		j = []
		max = 0
		for i in players:
			j.append(i)
			if len(i) > max:
				max = len(i)
		
		for i in j:
			c = " " * (max - len(i))
			if players[i][2]:
				d = chr(k)
			else:
				d = "Dead"
			print ''.join([i, ": ", c, d])
			k += 1
	
	# Grid
	if grid != []:
		g = copy.deepcopy(grid)
		
		for i in bombs:
			g[i[0]][i[1]] = "X"
		
		k = 65
		for i in players:
			if players[i][2]: 
				g[players[i][0]][players[i][1]] = chr(k)
			k += 1
		
		for i in explosions:
			g[i[0]][i[1]] = "*"
		
		explosions = set()
			
		print z
		for j in g:
			s = " ".join(j)
			# Changes output to a more readable format
			s = s.replace("0", " ").replace("1", "+").replace("2", "#")
			print s
		print z, "\n"
	
	#Server Commands
	for i in last:
		print "Server: ", i
		
	last = []			
			
	displayScores()	

def read_line(a = 1):
	ret = ''

	while True:
		c = s.recv(1)

		if c == '\n' or c == '':
			break
		else:
			ret = ''.join([ret,c])
	if a:
		last.append(ret)
		
	return ret

def createMap(x):
	global rows, cols, z	rows = int(x[1])
	cols = int(x[2])
	z = "-" * ((cols * 2) - 1)

	for i in range(rows):
		y = read_line(0).split()
		grid.append(y)
		def placeBomb(r, c):
	for i in bombs:
		if i[0] == r and i[1] == c: #pointless bomb
			return 0
	
	return 1
	
def action(x):
	global players
	r = players[x[0]][0]
	c = players[x[0]][1]
	
	if x[1] == "LEFT" and c != 0:
		if grid[r][c-1] == "0":
			players[x[0]][1] -= 1
	if x[1] == "RIGHT" and c != (cols - 1):
		if grid[r][c+1] == "0":
			players[x[0]][1] += 1
	if x[1] == "UP" and r != 0:
		if grid[r-1][c] == "0":
			players[x[0]][0] -= 1
	if x[1] == "DOWN" and r != (rows - 1):
		if grid[r+1][c] == "0":
			players[x[0]][0] += 1
	if x[1] == "BOMB" and placeBomb(players[x[0]][0], players[x[0]][1]):
		bombs.append([players[x[0]][0], players[x[0]][1], 4])	
		def explode(r,c):
	global grid
	
	if r < 0 or r >= rows or c < 0 or c >= cols:
		return 1	
	
	if grid[r][c] == "0":
		explosions.add((r,c))
		for i in bombs:
			if i[0] == r and i[1] == c:
				explodeBomb(i)
				break  # If there are multiple bombs on the same square then the next explosion will trigger it
		return 0
	elif grid[r][c] == "1":
		explosions.add((r,c))
		grid[r][c] = "0"
		return 1
	elif grid[r][c] == "2":
		return 1
		def explodeBomb(x):
	r = x[0]
	c = x[1]
	bombs.remove(x)
	explode(r,c)
	
	#Left
	for i in range(1,4):
		if explode(r, c-i):
			break
			#Right
	for i in range(1,4):
		if explode(r, c+i):
			break
			
	#Up
	for i in range(1,4):
		if explode(r-i, c):
			break
		
	#Down
	for i in range(1,4):
		if explode(r+i, c):
			break
			def checkBombs():
	for i in bombs:
		i[2] -= 1
		if i[2] == 0:
			explodeBomb(i)
		def parse(data, s):
	global gameOn, grid, players, last, scores
	x = data.split()
	y = x[0]
	
	if y == "INIT":
		gameOn = True
		grid = []
		players = {}
	elif y == "MAP":
		createMap(x)
	elif y == "PLAYERS":
		if int(x[1]) != 0:
			for i in range(int(x[1])):
				d = read_line().split()
				players[d[0]] = [int(d[1]), int(d[2]), 1]
		else:		
			gameOn = False
			clear()	
			print "No game in progress\n"
			displayScores()			
	elif y == "ACTIONS":
		for i in range(int(x[1])):
			d = read_line().split()
			action(d)
		checkBombs()
		display()
	elif y == "TICK":
		pass
	elif y == "DEAD":
		for i in range(int(x[1])):
			d = read_line().split()
			players[d[0]][2] = 0
	elif y == "STOP":
		clear()
		last = []
		gameOn = False
	elif y == "SCORES":
		if int(x[1]) == 0:
			last = []	
		else:	
			gameOn = False
			scores = []
			
			for i in range(int(x[1])):
				d = read_line().split()
				scores.append(d)		
		while 1:	
	data = read_line()
	parse(data, s)
	

