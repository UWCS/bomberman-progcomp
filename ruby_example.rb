#!/usr/bin/ruby

require 'socket'

class Game # class to store game information
	attr_accessor :num_players, :players_alive, :mapx, :mapy, :map, :player_x, :player_y
	def initialize(my_name)
		@game_over = false
		@players = []
		@player_x = {}
		@player_y = {}
		@map = Hash.new(2)
		@bombs = Hash.new(-1) 
		# @bombs[[x, y]] is -1 if no bomb, 1..4 is the number of turns remaining on the bomb
		@own_name = my_name
		@map_current_row = 0
	end

	def new_info(incoming_message) # strings relating to game info are passed here
		content = incoming_message.split ' '
		if incoming_message.start_with? "MAP"
			@map_y, @map_x = content[1], content[2]
		elsif incoming_message.start_with? "PLAYERS"
			@num_players, @players_alive = content[1], content[1]
			@name_context = :players
		elsif incoming_message.start_with? "DEAD" then
			@players_alive.delete content[1]
			@name_context = :dead
		elsif incoming_message.start_with? "END"
			@game_over = true
		elsif incoming_message.start_with? "ACTIONS" then
			tick_bombs()
			@name_context = :actions
		elsif incoming_message =~ /^\d.*/ then
			content.length.times do |i| 
				@map[[@map_current_row, i]] = content[i]
			end
			@map_current_row.succ
			p @map
		elsif incoming_message =~ /^[a-z].*/
			self.name_info incoming_message
		else
			p "zed0, fix the server! (or more likely MikeCobra fix the Ruby!)"
		end
	end

	def name_info(name_message)
		content = name_message.split ' '
		if @name_context == :players then
			@players.push content[0] 
			@player_y[content[0]] = content[1].to_i
			@player_x[content[0]] = content[2].to_i
		elsif @name_context == :dead
			@players.delete content[0]
		elsif @name_context == :actions then
			if content[1] == "BOMB"
				self.update_bombs(content[0])
			elsif ["UP", "LEFT", "DOWN", "RIGHT"].include? content[1]
				self.move(content[0], content[1])
			end
		end

	end

	def tick_bombs
		@bombs.each do |key, value| 
			@bombs[key] = -1 if value == 1
			@bombs[key] = value - 1 if value != -1
		end
	end

	def move(player, direction)
		x = @player_x[player]
		y = @player_y[player]
		if direction == "UP" then
			@player_y = (y - 1) if can_move?([x, y - 1])
		elsif direction == "RIGHT"
			@player_x = (x + 1) if can_move?([x + 1, y])
		elsif direction == "DOWN"
			@player_y = (y + 1) if can_move?([x, y + 1])
		elsif direction == "LEFT"
			@player_x = (x - 1) if can_move?([x - 1, y])
		end 
	end

	def can_move?(square)
		((@map[square] == 0 ) & (@bombs[square] == -1))
	end

	def update_bombs(player)
		x = @player_x[player]
		y = @player_y[player]
		@bombs[[x, y]] = 4 if @bombs[[x, y]] == -1
	end

	def my_x
		@player_x[@own_name]
	end

	def my_y
		@player_y[@own_name]
	end

	def over?
		@game_over
	end
end

class Array

 def random_element
    self[rand(self.length)]
 end

end

class AI # the AI class
	# Insert Magical AI System Here
	def response(game_state) # this is called when it's time to move, game_state contains info about the game
		["BOMB", "UP", "LEFT", "DOWN", "RIGHT"].random_element # Don't even bother to check I can move there
	end
end

player_name = "rubbish-ruby"
password = "reallydamnsecure"

while true do
	socket = TCPSocket.new 'uwcs.co.uk', 8037
	game = Game.new player_name
	ai = AI.new

	while !game.over? do
		line = socket.gets
		p line
		if line.start_with? "INIT"
			socket.print "REGISTER #{player_name} #{password}"
		elsif line.start_with? "MAP", "ACTIONS", "PLAYERS", "DEAD", "END"
			game.new_info line
		elsif line =~ /^\d.*/
			game.new_info line
		elsif line =~ /^[a-z].*/
			game.new_info line
		elsif line.start_with? "TICK"
			socket.print "ACTION " + (ai.response game)
		end
	end
end
