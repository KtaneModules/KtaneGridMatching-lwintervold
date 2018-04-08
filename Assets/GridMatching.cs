using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMatching : MonoBehaviour {

	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMAudio Audio;
	public KMSelectable[] DirectionalButtons;
	public KMSelectable[] RotationalButtons;
	public KMSelectable[] ScrollerButtons;
	public KMSelectable ButtonVerifySolution;
	public KMSelectable[] DisplayGrid;
	public KMSelectable FocusBox;
	public TextMesh ScrollerText;

	//16 boards version
	private static readonly List<string> solutionNames = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P"};
	private static readonly List<long> solutionStates = new List<long> {0x92B1F4E41L, 0xF047DF33AL, 0x79035474BL, 0xAF4315D30L, 0x3FBC31DF8L, 0xA3212A56L, 0xC1A9A421BL, 0x35E71B3D7L,
		0x5FDB664B4L, 0x573F48D44L, 0x657725AAFL, 0xA4AFBE0ADL, 0x1ACA28687L, 0x13E1AA4B5L, 0x854956717L, 0xE7B1B09AL};

	private Board_6x6 solutionboard;
	private Board_6x6 displayboard;
	private FocusBoard_4x4 focus;
	private Scroller scroller;
	private string solutionName;
	private long solutionState;
	private const float buttonbump = 0.2f;

	private static int _moduleIdCounter = 1;
	private int _moduleId;

	void Start () {
		_moduleId = _moduleIdCounter++;

		for (int i = 0; i < 4; i++) {
			int j = i;
			DirectionalButtons [i].OnInteract += delegate () {
				HandleDirectionalButton (j); return false;
			};
		}
		for (int i = 0; i < 2; i++){
			int j = i;
			RotationalButtons [i].OnInteract += delegate () {
				HandleRotationalButton (j); return false;
			};
		}
		for (int i = 0; i < 2; i++) {
			int j = i;
			ScrollerButtons [i].OnInteract += delegate () {
				HandleScrollerButton (j); return false;
			};
		}
		ButtonVerifySolution.OnInteract += HandleVerifySolution;

		int solution_index = Random.Range (0, solutionNames.Count);
		solutionState = solutionStates [solution_index];
		solutionName = solutionNames [solution_index];

		scroller = new Scroller (solutionNames, ScrollerText);
		solutionboard = new Board_6x6 (solutionState);

		displayboard = solutionboard.generateRandomDisplay ();

		UpdateGrid (displayboard.getBoardState ());
		FocusBox.transform.localPosition = displayboard.getFocusBoxCoords ();

		Debug.LogFormat ("[GridMatching #{0}] Seed Grid: {1} Seed Label: {2}", _moduleId, solutionboard.getBoardState(), solutionName);
		Debug.LogFormat ("[GridMatching #{0}] Solution Grid: {1}  Solution Label: {2}", _moduleId, solutionboard.getSolution(), solutionName);
		Debug.LogFormat ("[GridMatching #{0}] Current Grid: {1} Current Label: {2}", _moduleId, displayboard.getBoardState (), scroller.getState());
		}

	private void HandleDirectionalButton(int i){
		Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, DirectionalButtons[i].transform);
		DirectionalButtons[i].AddInteractionPunch (buttonbump);
		Vector3 currentposition = FocusBox.transform.localPosition;
		switch(i){
		case 0:
			if (currentposition.z > -0.02f) {
				currentposition.z -= 0.02f;
				displayboard.translate (Direction.DOWN);
			}
			break;
		
		case 1:
			if (currentposition.x > 0.00f) {
				currentposition.x -= 0.02f;
				displayboard.translate (Direction.LEFT);
			}
			break;
		
		case 2:
			if (currentposition.z < 0.02f){
				currentposition.z += 0.02f;
				displayboard.translate (Direction.UP);
			}
			break;
		
		case 3:
			if (currentposition.x < 0.04f){
				currentposition.x += 0.02f;
				displayboard.translate (Direction.RIGHT);
			}
			break;
		}
		FocusBox.transform.localPosition = currentposition;
		UpdateGrid (displayboard.getBoardState ());
	}

	private void HandleRotationalButton(int i){
		Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, RotationalButtons[i].transform);
		RotationalButtons[i].AddInteractionPunch (buttonbump);
		if (i == 0)
			displayboard.rotateFocusCounterClockwise ();
		else
			displayboard.rotateFocusClockwise ();
		UpdateGrid (displayboard.getBoardState());
	}

	private void HandleScrollerButton(int i){
		Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, ScrollerButtons[i].transform);
		ScrollerButtons[i].AddInteractionPunch (buttonbump);
		if (i == 0)
			scroller.decrementState ();
		else
			scroller.incrementState ();
	}

	private bool HandleVerifySolution(){
		Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, ButtonVerifySolution.transform);
		ButtonVerifySolution.AddInteractionPunch (buttonbump);
		Debug.LogFormat ("[GridMatching #{0}] Solution Grid: {1}  Solution Label: {2}", _moduleId, solutionboard.getSolution(), solutionName);
		Debug.LogFormat ("[GridMatching #{0}] Current Grid: {1} Current Label: {2}", _moduleId, displayboard.getBoardState (), scroller.getState());
		if (solutionboard.checkProposedSolution(displayboard) && ScrollerText.text.Equals(solutionName)){
			Debug.LogFormat ("[GridMatching #{0}] Entered correct Solution and Label.", _moduleId);
			BombModule.HandlePass ();
		} 
		else {
			Debug.LogFormat ("[GridMatching #{0}] Entered incorrect Solution or Label.", _moduleId);
			BombModule.HandleStrike ();
		}
		return false;
	}

	void UpdateGrid(long boardstate6x6){
		Color32 white = new Color32(0xFF,0xFF,0xFF,0xFF);
		Color32 black = new Color32(0x00,0x00,0x00,0x00);
		for (int i = 0; i < DisplayGrid.Length; i++){
			DisplayGrid [i].GetComponent<MeshRenderer> ().material.color = black;
		}
			
		int j = 0;
		while (boardstate6x6 > 0) {
			if (boardstate6x6 % 2 == 1) {
				DisplayGrid [j].GetComponent<MeshRenderer> ().material.color = white;
			}
			j++;
			boardstate6x6 = boardstate6x6 >> 1;
		}
		
	}
}