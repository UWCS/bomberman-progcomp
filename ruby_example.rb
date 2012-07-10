#!/usr/bin/ruby

require 'socket'

class Game # class to store game information
	attr_accessor :num_players, :players_alive, :mapx, :mapy
	def initialize
		@game_over = false
	end

	def new_info(incoming_message) # strings relating to game info are passed here
		content = incoming_message.split ' '
		if incoming_message.start_with? "MAP"
			@mapy, @mapx = content[1], content[2]
		elsif incoming_message.start_with? "PLAYERS"
			@num_players, @players_alive = content[1], content[1]
		elsif incoming_message.start_with? "DEAD" then
			@players_alive -= content[1]
			@name_context = :dead
		elsif incoming_message.start_with? "END"
			@game_over = true
		elsif incoming_message.start_with? "ACTIONS"
			@name_context = :actions
		elsif incoming_message.start_with? "\d"
			@map += content.map { |s| s.to_i }
		elsif incoming_message.start_with? "[a-z]"
			self.name_info incoming_message
		else
			p "zed0, fix the server! (or more likely MikeCobra fix the Ruby!)"
		end
	end

	def name_info(name_message)

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

socket = TCPSocket.new 'uwcs.co.uk', 8037
game = Game.new
ai = AI.new

player_name = "rubbish-ruby"
password = "reallydamnsecure"

while !game.over? do
	line = socket.gets
	if line.start_with? "INIT"
		socket.print "REGISTER #{player_name} #{password}"
	elsif line.start_with? "MAP", "\d", "ACTIONS", "[a-z]", "PLAYERS", "DEAD", "END"
		game.new_info line
	elsif line.start_with? "TICK"
		socket.print "ACTION " + (ai.response game)
	end
end
		