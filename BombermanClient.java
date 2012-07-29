import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.Socket;
import java.net.UnknownHostException;
import java.util.Hashtable;

public class BombermanClient {
	private enum MoveChoices {
		BOMB, LEFT, RIGHT, UP, DOWN;	
	}

	private enum ServerCommands {
		ERROR, END, ACTION, ACTIONS, DEAD, TICK;
	}

	//Hash table to store the positions of the players in the game
	private static Hashtable<String, Coordinate> playerPositions = new Hashtable<String, Coordinate>();

	private static int port = 8037;
	private static String host = "uwcs.co.uk";
	//private static String host = "localhost";

	public static void main (String[] args) throws IOException {
		Socket serverSock = null;

		//try to connect to the server specified on the port specified
		try {
			serverSock = new Socket(host, port);
		} catch (UnknownHostException hostnotfound) {
			hostnotfound.printStackTrace();
			System.err.println("Failed to find host");
			System.err.println(hostnotfound);
			System.exit(1);
		} catch (IOException IOerr) {
			IOerr.printStackTrace();
			System.err.println("I/O error when creating socket");
			System.err.println(IOerr);
			System.exit(1);
		}

		final Socket finalSock = serverSock;

		BufferedReader fromServer = null;
		PrintWriter toServer = null;
		Socket clientSock = finalSock;

		//set up the communication streams with the server
		try {
			fromServer = new BufferedReader(new InputStreamReader(clientSock.getInputStream()));
			toServer = new PrintWriter(clientSock.getOutputStream(), true);
			System.out.println("successfully made server streams");
		} catch (IOException streamerr) {
			streamerr.printStackTrace();
			System.err.println("error creating streams to/from server");
			System.err.println(streamerr);
			System.exit(1);
		} 

		while(true) {
			try {
				//first server message is an "INIT" to start the game
				while(!fromServer.readLine().equals("INIT")) {System.out.println("not yet received INIT");}
			} catch (IOException e) {
				e.printStackTrace();
				System.err.println("I/O error when reading from socket");
				System.err.println(e);
				System.exit(1);
			}

			//next step is to register with the server
			String me = "alice";
			String myPassword = "supersekritpassword";
			toServer.println("REGISTER " + me + " " + myPassword);

			//next string command should be the player list
			String nextLine = "";
			try {
				nextLine = fromServer.readLine();
				System.out.println("players are " + nextLine);
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

			//next string command should be the map size
			try {
				nextLine = fromServer.readLine();
				System.out.println("map size: " + nextLine);
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

			//parse the string for the coordinates and store these in a 2D array
			String[] coords;
			System.out.println(nextLine);
			coords = nextLine.split(" ", 3);
			int x = Integer.valueOf(coords[1]);
			int y = Integer.valueOf(coords[2]);
			//save the map as an array
			System.out.println("map dimensions: (" + x + ", " + y + ")");
			int[][] map = new int[x][y];
			for(int i = 0; i < x; i++) {
				nextLine = fromServer.readLine();
				String[] thisLine = nextLine.split(" ");
				for(int j = 0; j < y; j++) {
					map[j][i] = Integer.valueOf(thisLine[j]);
				}
			}
			printMap(map, x, y, -1, -1);

			//next command specifies the number of players in the game
			String players = fromServer.readLine();
			int noPlayers = Integer.valueOf(players.substring(8));
			int myX = -1, myY = -1;
			//store the starting position for each player
			for(int i = 0; i<noPlayers; i++) {
				String[] thisLine = (fromServer.readLine()).split(" ", 3);
				for(int j = 0; j < 3; j++) {
					playerPositions.put(thisLine[0], new Coordinate(
							Integer.valueOf(thisLine[1]), Integer.valueOf(thisLine[2])));
				}
				if(thisLine[0].equals(me)) {
					myX = Integer.valueOf(thisLine[2]);
					myY = Integer.valueOf(thisLine[1]);
					System.out.println("My current position is (" + myX + ", " + myY + ")");			
				}
			}

			boolean gameOver = false;

			//Read in the first TICK command
			try {
				nextLine = fromServer.readLine();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

			//the game now begins
			while (gameOver == false) {
				//check if the player is still in the game
				if(myX==-1) {
					System.out.println("I'm not in the game anymore");
					nextLine = fromServer.readLine();
					//check if the game is over
					if(nextLine.equals(ServerCommands.END.toString())) {
						System.out.println("The game is over");
						gameOver = true;
					}
				}
				else {
					//check if the game is over
					if(nextLine.equals(ServerCommands.END.toString())) {
						System.out.println("The game is over");
						gameOver = true;
					}
					else {
						//else chose a random move
						MoveChoices move = randomMove(map, myX, myY, x, y);
						if(move.toString().equals(ServerCommands.ERROR.toString())) {
							System.out.println("Error when choosing move");
						}
						else {
							//write the move chosen to the server
							toServer.println("ACTION " + move.toString());
							//read the next line - this should be the server acknowledging your move
							nextLine = fromServer.readLine();
							String[] thisLine = nextLine.split(" ");
							if(thisLine[0].equals(ServerCommands.TICK.toString())) nextLine = fromServer.readLine(); 
							if(!nextLine.equals(move.toString())) {
								System.out.println("Error - server did not recognise move correctly, instead got " + nextLine);
							}
							else System.out.println("the server's reply was " + nextLine);
							//the next messages will be the moves made by other players if any moves have been made
							String actions = fromServer.readLine();
							String[] commands = actions.split(" ");
							if(commands[0].equals(ServerCommands.ACTIONS.toString())) {
								int noActions = Integer.valueOf(commands[1]);
								//next record the moves of the other players who made moves
								for(int i = 0; i < noActions; i++) {
									thisLine = (fromServer.readLine()).split(" ", 3);
									updateMoves(thisLine);
									Coordinate myCoord = updateMe(thisLine, me, myX, myY);
									myX = myCoord.getX();
									myY = myCoord.getY();
								}
							} else if(commands[0].equals(ServerCommands.ACTION.toString())) {
								thisLine = (fromServer.readLine()).split(" ", 3);
								updateMoves(thisLine);
								Coordinate myCoord = updateMe(thisLine, me, myX, myY);
								myX = myCoord.getX();
								myY = myCoord.getY();
							} else if(commands[0].equals("DEAD")) {
								int noDead = Integer.valueOf(actions.substring(6));
								for(int i = 0; i < noDead; i++) {
									nextLine = fromServer.readLine();
									if(updateDead(nextLine, me, myX)==-1) myX = -1;
								}
							}
							//if there were actions there may now be a list of dead players
							nextLine = fromServer.readLine();
							commands = nextLine.split(" ");
							if(commands[0].equals(ServerCommands.DEAD.toString())) {
								int noDead = Integer.valueOf(commands[1]);
								for(int i = 0; i < noDead; i++) {
									nextLine = fromServer.readLine();
									if(updateDead(nextLine, me, myX)==-1) myX = -1;
								}
								//read in the next tick/end
								fromServer.readLine();
							}
						}
					}
				}
			}
			gameOver = false;
		}
	}

	private static MoveChoices randomMove(int[][] map, int currX, int currY, int x, int y) {
		//2 possible options - move or bomb. Naive agent choses with random probability
		double moveChoice = Math.random();
		printMap(map, x, y, currX, currY);
		if(moveChoice<0.15) {
			return MoveChoices.BOMB;
		} else {
			//need to check for walls. Choose the first direction inspected that is not a wall
			if(currY>0 && map[currX][currY-1]==0) {
				System.out.println("chose left");
				return MoveChoices.LEFT;
			}
			if(currX>0 && map[currX-1][currY]==0) {
				System.out.println("chose up");
				return MoveChoices.UP;
			}
			if(currX<x-1 && map[currX+1][currY]==0) {
				System.out.println("chose down");
				return MoveChoices.DOWN;
			}
			if(currY<y-1 && map[currX][currY+1]==0) {
				System.out.println("chose right");
				return MoveChoices.RIGHT;
			}
			else return MoveChoices.BOMB;
		}
	}

	private static void updateMoves(String[] thisLine) {
		MoveChoices thisChoice = MoveChoices.valueOf(thisLine[1]);
		String name = thisLine[0];
		System.out.println("updating moves for " + name + " who moved " + thisChoice.toString());
		Coordinate currentPosition = playerPositions.get(name);
		switch(thisChoice) {
		case LEFT : currentPosition.setX(currentPosition.getX()-1); break;
		case RIGHT : currentPosition.setX(currentPosition.getX()+1); break;
		case UP : currentPosition.setY(currentPosition.getY()-1); break;
		case DOWN : currentPosition.setY(currentPosition.getY()+1); break;
		case BOMB : System.out.println(name + " is dropping a bomb at " + currentPosition.toString());
		}
	}

	private static Coordinate updateMe(String[] thisLine, String me, int myX, int myY) {
		if(thisLine[0].compareTo(me)==0) {
			MoveChoices thisChoice = MoveChoices.valueOf(thisLine[1]);
			switch(thisChoice) {
			case LEFT : myY-=1; break;
			case RIGHT : myY+=1; break;
			case UP : myX-=1; break;
			case DOWN : myX+=1; break;
			}
			System.out.println("updating my coordinates -  now at (" + myX + "," + myY + ")");
		}
		return new Coordinate(myX, myY);
	}

	private static int updateDead(String nextLine, String me, int myX) {
		String[] thisLine = nextLine.split(" ");
		String name = thisLine[0];
		Coordinate thisCoord = playerPositions.get(name);
		thisCoord.setX(-1);
		thisCoord.setY(-1);
		if(name.equals(me)) {
			System.out.println("I'm dead! :(");
			return -1;
		}
		else System.out.println(name + " is dead!");
		return 0;
	}

	private static void printMap(int[][] map, int x, int y, int currX, int currY) {
		for(int i = 0; i < x; i++) {
			for(int j = 0; j < y; j++) {
				if(i == currX && j == currY)
					System.out.print("* ");
				else System.out.print(map[i][j]+ " ");
			}
			System.out.println();
		}
	}
}
