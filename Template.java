import java.io.IOException;
import java.net.Socket;
import java.net.UnknownHostException;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.PrintWriter;

public class Template {
	private static int port = 8037;
	//private static String host = "uwcs.co.uk";
	private static String host = "localhost";

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
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		//next string command should be the map size
		try {
			nextLine = fromServer.readLine();
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
		int[][] map = new int[x][y];
		for(int i = 0; i<y; i++) {
			nextLine = fromServer.readLine();
			char[] thisLine = nextLine.toCharArray();
			map[i][0]=thisLine[0];
			for(int j = 2; j < x; j+=2) {
				map[i][j/2] = thisLine[j];
			}
		}

		//next command specifies the number of players in the game
		String players = fromServer.readLine();
		int noPlayers = Integer.valueOf(players.substring(8));
		int myX = -1, myY = -1;
		//store the starting position for each player each player
		String[][] playerPositions = new String[noPlayers][3];
		for(int i = 0; i<noPlayers; i++) {
			String[] thisLine = (fromServer.readLine()).split(" ", 3);
			for(int j = 0; j < 3; j++) {
				playerPositions[i][j] = thisLine[j];
			}
			if(thisLine[0].equals(me)) {
				myX = Integer.valueOf(thisLine[1]);
				myY = Integer.valueOf(thisLine[2]);
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
				System.out.println("I'm not in the game");
			}
			else {
				//check if the game is over
				if(nextLine.equals("END")) {
					System.out.println("The game is over");
					gameOver = true;
				}
				else {
					//else chose a random move
					String move = randomMove(map, myX, myY, x, y);
					if(move.equals("ERROR")) {
						System.out.println("Error when choosing move");
					}
					else {
						//write the move chosen to the server
						toServer.println("ACTION " + move);
						//read the next line - this should be the server acknowledging your move
						nextLine = fromServer.readLine();
						if(!nextLine.equals(move)) {
							System.out.println("Error - server did not recognise move correctly");
						}
						//the next messages will be the moves made by other players if any moves have been made
						String actions = fromServer.readLine();
						String actionCommand = actions.substring(0,6);
						String deadCommand = actions.substring(0,3);
						if(actionCommand.equals("ACTIONS")) {
							int noActions = Integer.valueOf(actions.substring(8));
							//next record the moves of the other players who made moves
							for(int i = 0; i < noActions; i++) {
								String[] thisLine = (fromServer.readLine()).split(" ", 3);
								updateMoves(thisLine, noPlayers, playerPositions);
								updateMe(thisLine, me, myX, myY);
							}
						}
						else if(deadCommand.equals("DEAD") || ((fromServer.readLine()).substring(0,3)).equals("DEAD")) {
							int noDead = Integer.valueOf(actions.substring(5));
							for(int i = 0; i < noDead; i++) {
								nextLine = fromServer.readLine();
								updateDead(nextLine, noDead, playerPositions, me, myX);
							}
						}
						//if there were actions there may now be a list of dead players
						nextLine = fromServer.readLine();
						deadCommand = nextLine.substring(0,3);
						if(deadCommand.equals("DEAD")) {
							int noDead = Integer.valueOf(actions.substring(5));
							for(int i = 0; i < noDead; i++) {
								nextLine = fromServer.readLine();
								updateDead(nextLine, noDead, playerPositions, me, myX);
							}
							//read in the next tick/end
							fromServer.readLine();
						}
					}
				}
			}
		}
	}

	private static String randomMove(int[][] map, int currX, int currY, int x, int y) {
		//2 possible options - move or bomb. Naive agent choses with random probability
		int moveChoice = (int)Math.floor(2*Math.random());
		switch(moveChoice) {
		case(0):
			return "BOMB";
		case(1):
			//need to check for walls. Choose the first direction inspected that is not a wall
			if(currY>0 && map[currX][currY-1]==0) {
				return "UP";
			}
			if(currX>0 && map[currX-1][currY]==0) {
				return "LEFT";
			}
			if(currX<x && map[currX+1][currY]==0) {
				return "RIGHT";
			}
			if(currY<y && map[currX][currY+1]==0) {
				return "DOWN";
			}
		}
		return "ERROR";
	}

	private static void updateMoves(String[] thisLine, int noPlayers, String[][] playerPositions) {
		for(int j = 0; j < noPlayers; j++) {
			if(thisLine[0].compareTo(playerPositions[j][0])==0) {
				for(int k = 0; k < 3; k++) {
					playerPositions[j][k] = thisLine[k];
				}
			}
		}
	}

	private static void updateMe(String[] thisLine, String me, int myX, int myY) {
		if(thisLine[0].compareTo(me)==0) {
			myX = Integer.valueOf(thisLine[1]);
			myY = Integer.valueOf(thisLine[2]);
		}
	}
	
	private static void updateDead(String nextLine, int noDead, String[][] playerPositions, String me, int myX) {
		System.out.println(nextLine);
		for(int i = 0; i < noDead; i++) {
			String thisPlayer = playerPositions[i][0];
			if(thisPlayer.compareTo(nextLine)==0) {
				playerPositions[i][1] = "-1";
				playerPositions[i][2] = "-1";
				if(nextLine.compareTo(me)==0) {
					System.out.println("I'm dead :(");
					myX = -1;
				}
			}

		}
	}
}
