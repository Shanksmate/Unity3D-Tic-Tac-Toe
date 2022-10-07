﻿using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public enum TicTacToeState{none, cross, circle}

[Serializable] public class WinnerEvent : UnityEvent<int>
{
	
}

public class TicTacToeAI : MonoBehaviour
{
	
	int _aiLevel;

	[SerializeField]
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
		boardState = new [,]
		{
			{TicTacToeState.none,TicTacToeState.none,TicTacToeState.none},
			{TicTacToeState.none,TicTacToeState.none,TicTacToeState.none},
			{TicTacToeState.none,TicTacToeState.none,TicTacToeState.none}
		};
		
		
		
		onGameStarted.Invoke();
	}

	public void PlayerSelects(int coordX, int coordY)
	{
		if (!_isPlayerTurn) return;
		
		MakeAMove(coordX,coordY,playerState);
		_isPlayerTurn = false;
		
		AiTurn(boardState);
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

	private void AiTurn(TicTacToeState[,] boardCopy)
	{
		//are you sure when this board is sent recursively, this conditions will still be met?
		//_move count hasn't 
		if (!_isPlayerTurn && _moveCount >= 4)
		{
			boardCopy = boardState;
			GetBestAiMove(boardCopy);
		}
	}

	private void GetBestAiMove( TicTacToeState[,] boardCopy)
	{
		//canClick value of all moves must be true to get on the list
		List<ClickTrigger> moves = GeneratePossibleMoves(boardCopy);
		
		foreach (ClickTrigger move in moves)
		{
			//The canClick value of this clicktrigger coords have to change also
			//The bool canClick value controls the generation of the moves in the GetBestAiMove
			boardCopy[move._myCoordX, move._myCoordY] = aiState;
				
			//Because just as we send this updated boardCopy recursively to GetBestAiMove,
			//We equally pass EACH these of moves to the GetResult function,
			int grade = GetResult(boardCopy, aiState, move);
			
			if (grade == 1)
			{
				//Better luck next time human!
				//stop the loop! Ignore all other possible moves (break?)
				
				MakeAMove(move._myCoordX,move._myCoordY,aiState);
				_isPlayerTurn = true;
				onPlayerWin.Invoke(1);
				break;
				

			}else if (grade == 0)
			{
				//Well done human, tie game!
				//stop the loop! Ignore all other possible moves (break?)
				MakeAMove(move._myCoordX,move._myCoordY,aiState);
				_isPlayerTurn = true;
				onPlayerWin.Invoke(-1);
				break;
			}else
			{
				//Ai digs deeper
				//send the corresponding board recursively to the getBestMove function.
				GetBestAiMove(boardCopy);
			}
				
		}
	}

	List<ClickTrigger> GeneratePossibleMoves(TicTacToeState[,] boardCopy)
	{
		List<ClickTrigger> list = new List<ClickTrigger>();

		foreach (ClickTrigger move in _triggers)
		{
			//This is the condition for generating possible moves from the triggers
			if (boardCopy[move._myCoordX,move._myCoordY] == TicTacToeState.none)
			{
				//We need to continuously prune this list as we test in the GetBestAiMove function
				////particularly if it is a list 
				list.Add(move);
			}
		}
		return list;
	}

	void MakeAMove(int coordX, int coordY, TicTacToeState player)
	{

		if(boardState[coordX,coordY] == TicTacToeState.none)
		{
			_triggers[coordX, coordY].canClick = false;
			boardState[coordX,coordY] = player;
			SetVisual(coordX, coordY, player);
		}
		_moveCount++;
	}

	private int GetResult(TicTacToeState[,] boardCopy,TicTacToeState player,ClickTrigger move)
	{
		int coordX = move._myCoordX;
		int coordY = move._myCoordY;
		
		//check end conditions
		
		//check row
		for (int i = 0; i < _gridSize; i++)
		{
			if (boardCopy[i, coordY] != player)
				break;
			if (i == _gridSize - 1)
			{
				return player == playerState ? -1 : 1;
			}
		}

		//check col
		for (int i = 0; i < _gridSize; i++)
		{
			if (boardCopy[coordX, i] != player)
				break;
			if (i == _gridSize - 1)
			{
				return player == playerState ? -1 : 1;
			}
		}

		//check diag
		if (coordX == coordY)
		{
			//we're on a diagonal
			for (int i = 0; i < _gridSize; i++)
			{
				if (boardCopy[i, i] != player)
					break;
				if (i == _gridSize - 1)
				{
					return player == playerState ? -1 : 1;
				}
			}
		}

		//check anti diag
		if (coordX + coordY == _gridSize - 1)
		{
			for (int i = 0; i < _gridSize; i++)
			{
				if (boardCopy[i, (_gridSize - 1) - i] != player)
					break;
				if (i == _gridSize - 1)
				{
					return player == playerState ? -1 : 1;
				}
			}
		}
		
		//check draw
		if (_moveCount == 9 && !CheckWin(player)) return 0;

		return _moveCount;
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

