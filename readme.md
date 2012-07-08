Server is running on uwcs.co.uk:8124
All messages are sent via TCP, each message is sent followed by a newline, each piece of a message is delimited by spaces.

The server is currently on a tick time of 500ms, if there are issues with this (such as latency for certain players) then it may be increased.
The turn limit is currently set at 1000, if too many games are reaching this then it may be increased.

Bombs: When a bomb is placed (using ACTION BOMB) it is placed on the square the player is currently standing on.  You may not move onto a square with a bomb on it although you can move off them.  A bomb will stay in place for 4 turns and then explode; when it explodes it will blast in each compass direction up to 3 squares away.  If there is a destructable rock in the way then it will be destroyed but will also stop the explosion in that direction.  Any players who are hit by an explosion will die.

Scoring: Every time a player dies all surviving players gain one point.  If you are the last player in the game you gain an additional point.

## Messages: ##
### Server -> Client: ###
* INIT

	This starts the init phase at the start of each game.
* MAP \<rows\> \<cols\>

	This gives the initial state of the map immediately after the init phase.  This is followd by a number of lines (stated by \<rows\>), each containing a number of pieces of data (stated by \<cols\>) which describe the map:
	- 0:	Passable terrain, you may move here freely.
	- 1:	Destructable terrain, you may not move here but it may be destroyed by explosions turning it into type 0 terrain.
	- 2:	Indestrucable terrain, you can't move here, nor can they be destroyed.
* PLAYERS \<num\>

	This will immediately follow the MAP message and states how many players are in the new game, this will be followed by \<num\> lines of data in the format:
		\<name\> \<row\> \<col\>
	This shows the name of the player and their starting position in the game.  This coordinate is zero indexed.
* TICK \<num\>

	This states that a new tick has started, you can now send any action you would like to take in this tick.
* ACTIONS \<num\>

	This will be sent at the end of a tick and will be followed by \<num\> lines, each containing the name of a player and the action they took in the format \<name\> \<action\>.
	Actions may be: UP, DOWN, LEFT, RIGHT, BOMB.
* DEAD \<num\>

	This states that \<num\> players have died this tick, this will be followed by \<num\> lines, each containing the name of a player who has died this tick.
* END

	This states that the game has ended.  The game will end when there is only one surviving player or when the turn limit has been reached.
* SCORES \<num\>

	Shows \<num\> lines in the format \<name\> \<points\> stating how many points each player gained this game.

### Client -> Server: ###
* REGISTER \<name\> \<password\>

	This states your intent to take part in a game, your name and password will be automatically registered the first time you use them, from then on you will recieve E_WRONG_PASS if the password does not mach the previously recorded one.  Note that the server has little to no security on the password so use a disposable one.
* ACTION \<action\>

	This states that you wish to make an action, valid actions are:
	UP, DOWN, LEFT, RIGHT, BOMB
	You may only do one action per tick.


## Example game: ##

```
< INIT
> REGISTER alice supersekritpassword
< REGISTERED
< MAP 5 5
< 1 1 1 0 0
< 0 2 0 2 0
< 1 0 1 1 1
< 1 2 0 2 0
< 1 0 0 0 1
< PLAYERS 2
< alice 0 4
< bob 4 2
< TICK 1
> ACTION LEFT
< LEFT
< ACTIONS 2
< alice LEFT
< bob UP
< TICK 2
> ACTION BOMB
< BOMB
> ACTIONS 2
> alice BOMB
> bob BOMB
> TICK 3
< ACTION RIGHT
> RIGHT
> ACTIONS 2
> alice RIGHT
> bob DOWN
> TICK 4
< ACTION DOWN
> DOWN
> ACTIONS 1
> alice DOWN
> TICK 5
> TICK 6
> DEAD 1
> bob
> END
> SCORES 2
> alice 1
> bob 0
```
