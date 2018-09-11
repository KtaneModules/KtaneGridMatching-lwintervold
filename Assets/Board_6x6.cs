using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Board_6x6{
	private long boardstate6x6;
	private long solution;
	private FocusBoard_4x4 focus;

	public Board_6x6(long boardstate6x6){
		this.boardstate6x6 = boardstate6x6;

		int xshift = Random.Range (0, 3);
		int yshift = Random.Range (0, 3);
		int state = (int)
			(
				(((boardstate6x6 >> (6 * yshift + xshift +  0)) & 0xFL) <<  0) |
				(((boardstate6x6 >> (6 * yshift + xshift +  6)) & 0xFL) <<  4) |
				(((boardstate6x6 >> (6 * yshift + xshift + 12)) & 0xFL) <<  8) |
				(((boardstate6x6 >> (6 * yshift + xshift + 18)) & 0xFL) << 12)
			);
		FocusBoard_4x4 focus = new FocusBoard_4x4 (state, xshift, yshift);
		this.focus = focus;
		this.solution = focus.translateToBoardState6x6 ();
	}
	public Board_6x6(FocusBoard_4x4 focus){
		this.focus = focus;
		this.boardstate6x6 = focus.translateToBoardState6x6();
		this.solution = 0;
	}

	public Board_6x6 generateRandomDisplay(){
		FocusBoard_4x4 displayfocus = new FocusBoard_4x4(this.focus.getBoard4x4State(), Random.Range(0,3), Random.Range(0,3));
		int random_rotation = Random.Range (0, 4);
		for (int i = 0; i < random_rotation; i++) {
			displayfocus.rotateClockwise ();
			}
		return new Board_6x6(displayfocus);
	}

	public void translate(Direction direction){
        int focus_x = this.focus.getSectorX();
        int focus_y = this.focus.getSectorY();
		if (direction == Direction.UP && focus_y != 0) {
			this.boardstate6x6 = this.boardstate6x6 >> 6;
			this.focus.setSectorY(focus_y - 1);
		} else if (direction == Direction.DOWN && focus_y != 2){
			this.boardstate6x6 = this.boardstate6x6 << 6;
			this.focus.setSectorY(focus_y + 1);
		} else if (direction == Direction.RIGHT && focus_x != 2){
			this.boardstate6x6 = this.boardstate6x6 << 1;
			this.focus.setSectorX(focus_x + 1);
		} else if (direction == Direction.LEFT && focus_x != 0) {
			this.boardstate6x6 = this.boardstate6x6 >> 1;
			this.focus.setSectorX(focus_x - 1);
		}
	}

	public bool checkProposedSolution(Board_6x6 playboard){
		return (playboard.getBoardState () ^ this.solution) == 0;
	}

	public void rotateFocusClockwise(){
		long newstate = 0;
		int startx = this.focus.getSectorX();
		int starty = this.focus.getSectorY();
		for (int i = startx; i < 4 + startx; i++) {
			for (int j = starty; j < 4 + starty; j++) {
				long cellval = this.boardstate6x6 & ((1L << i) << (6 * j));
				if (cellval > 0) {
					newstate |= (1L << (3 - j + startx + starty)) << (6  * (i - startx + starty));
				}
			}
		}
		this.boardstate6x6 = newstate;
	}

	public void rotateFocusCounterClockwise(){
		this.rotateFocusClockwise();
		this.rotateFocusClockwise();
		this.rotateFocusClockwise();
	}

	public Vector3 getFocusBoxCoords(){
		int x = this.focus.getSectorX();
		int z = this.focus.getSectorY();
		return new Vector3 ((float) (0.02 * x), (float)  0, (float) (0.02 - 0.02 * z));
	}

	public long setBoardState(long newboardstate6x6){
		this.boardstate6x6 = newboardstate6x6;
		return this.boardstate6x6;
	}

	public long getBoardState(){
		return this.boardstate6x6;
	}

	public long getSolution(){
		return this.solution;
	}

    public void printBoard(long board) {
        string prnt = "";
        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 6; j++) {
                if ((board & 0x1L) == 0x1L)
                    prnt += "W";
                else
                    prnt += "O";
                board = board >> 1;
            }
            prnt += "\n";
        }
        Debug.LogFormat(prnt);
    }
}