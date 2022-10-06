using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public enum TicTacToeState{none, cross, circle}

[Serializable] public class WinnerEvent : UnityEvent<int>
{
	
}

public class TicTacToeAI : MonoBehaviour
{
	
	int _aiLevel;

	TicTacToeState[,] boardState;

	[SerializeField]
	public bool _isPlayerTurn;

	[SerializeField]
	private int _gridSize = 3;
	
	[SerializeField]
	private TicTacToeState playerState = TicTacToeState.cross;
	TicTacToeState aiState = TicTacToeState.circle;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	ClickTrigger[,] _triggers;
	
	[SerializeField] private int _moveCount;

	

	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

	public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		_triggers = new ClickTrigger[3,3];
		boardState = new TicTacToeState[3, 3];
		
		onGameStarted.Invoke();
	}

	public void PlayerSelects(int coordX, int coordY)
	{
		if (!_isPlayerTurn) return;
		
		MakeAMove(coordX,coordY,playerState,0);
		_isPlayerTurn = false;
		
		AiTurn();
	}

	private bool CheckWin(TicTacToeState player)
	{
		// check rows
		if (boardState[0, 0] == player && boardState[0, 1] == player && boardState[0, 2] == player) { return true; }
		if (boardState[1, 0] == player && boardState[1, 1] == player && boardState[1, 2] == player) { return true; }
		if (boardState[2, 0] == player && boardState[2, 1] == player && boardState[2, 2] == player) { return true; }

		// check columns
		if (boardState[0, 0] == player && boardState[1, 0] == player && boardState[2, 0] == player) { return true; }
		if (boardState[0, 1] == player && boardState[1, 1] == player && boardState[2, 1] == player) { return true; }
		if (boardState[0, 2] == player && boardState[1, 2] == player && boardState[2, 2] == player) { return true; }

		// check diags
		if (boardState[0, 0] == player && boardState[1, 1] == player && boardState[2, 2] == player) { return true; }
		if (boardState[0, 2] == player && boardState[1, 1] == player && boardState[2, 0] == player) { return true; }

		return false;
	}

	private void AiTurn()
	{
		//prevents ai from selecting if board has been exhausted
		if (_moveCount > 8)
		{
			return;
		}
		else //ai takes his turn
		{
			GenerateMove(out int coordX, out int coordY);
			AiSelects(coordX, coordY);
		}
	}

	private void AiSelects(int coordX, int coordY)
	{
		if (!_isPlayerTurn)
		{
			//Register move
			MakeAMove(coordX,coordY,aiState,1);
			
			//switches turn
			_isPlayerTurn = true;
		}
		
	}

	void GenerateMove(out int coordX, out int coordY)
	{
		List<ClickTrigger> possibleMoves = new List<ClickTrigger>();

		foreach (ClickTrigger move in _triggers)
		{
			if (move.canClick)
			{
				possibleMoves.Add(move);
			}
		}
		Debug.Log(possibleMoves.Count);
		
		int randomMove = UnityEngine.Random.Range(0,possibleMoves.Count);

		coordX = possibleMoves[randomMove]._myCoordX;
		coordY = possibleMoves[randomMove]._myCoordY;
		
	}

	void MakeAMove(int coordX, int coordY, TicTacToeState player, int winner)
	{

		if(boardState[coordX,coordY] == TicTacToeState.none)
		{
			_triggers[coordX, coordY].canClick = false;
			boardState[coordX,coordY] = player;
			SetVisual(coordX, coordY, player);
		}
		_moveCount++;
        
		//check end conditions
        
		//check col
		for(int i = 0; i < _gridSize; i++){
			if(boardState[coordX,i] != player)
				break;
			if(i == _gridSize-1)
			{
				onPlayerWin.Invoke(winner);
			}
		}
        
		//check row
		for(int i = 0; i < _gridSize; i++){
			if(boardState[i,coordY] != player)
				break;
			if(i == _gridSize-1)
			{
				onPlayerWin.Invoke(winner);
			}
		}
        
		//check diag
		if(coordX == coordY){
			//we're on a diagonal
			for(int i = 0; i < _gridSize; i++){
				if(boardState[i,i] != player)
					break;
				if(i == _gridSize-1)
				{
					onPlayerWin.Invoke(winner);
				}
			}
		}
            
		//check anti diag
		if(coordX + coordY == _gridSize - 1){
			for(int i = 0; i < _gridSize; i++){
				if(boardState[i,(_gridSize-1)-i] != player)
					break;
				if(i == _gridSize-1)
				{
					onPlayerWin.Invoke(winner);
				}
			}
		}

		//check draw
		if(_moveCount == (Math.Pow(_gridSize, 2) - 1))onPlayerWin.Invoke(-1);

	}
	

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == aiState ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
	}
}

