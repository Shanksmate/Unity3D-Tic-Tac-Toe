using System;
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
class Move
{
	public int row, col;
};

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
		//check win here
		

		AiTurn(boardState);
	}
	

	private void AiTurn(TicTacToeState[,] boardCopy)
	{
		boardCopy = boardState;
		
		//are you sure when this board is sent recursively, this conditions will still be met?
		//_move count hasn't 
		if (!_isPlayerTurn && _moveCount >= 4)
		{
			GetBestMove(boardCopy, aiState);
		}
		else
		{
			if (!_isPlayerTurn)
			{
				List<ClickTrigger> possibleMoves = GeneratePossibleMoves(boardCopy);
				
				int randomMove = UnityEngine.Random.Range(0,possibleMoves.Count);

				int coordX = possibleMoves[randomMove]._myCoordX;
				int coordY = possibleMoves[randomMove]._myCoordY;
				
				MakeAMove(coordX,coordY,aiState);
				
				_isPlayerTurn = true;
			}
		}
	}

	private bool IsMovesLeft(TicTacToeState[,] boardCopy)
	{
		for (int i = 0; i < _gridSize; i++)
		for (int j = 0; j < _gridSize; j++)
			if (boardCopy[i, j] == TicTacToeState.none)
				return true;
		return false;
	}

	private int Minimax(TicTacToeState[,] boardCopy, int depth, bool isMax)
	{
		int score = GetBoardState(boardCopy);
 
		// If Maximizer has won the game
		// return his/her evaluated score
		if (score == 10)
			return score;
 
		// If Minimizer has won the game
		// return his/her evaluated score
		if (score == -10)
			return score;
 
		// If there are no more moves and
		// no winner then it is a tie
		if (IsMovesLeft(boardCopy) == false)
			return 0;
 
		// If this maximizer's move
		if (isMax)
		{
			int best = -1000;
 
			// Traverse all cells
			for (int i = 0; i < _gridSize; i++)
			{
				for (int j = 0; j < _gridSize; j++)
				{
					// Check if cell is empty
					if (boardCopy[i, j] == TicTacToeState.none)
					{
						// Make the move
						boardCopy[i, j] = aiState;
 
						// Call minimax recursively and choose
						// the maximum value
						best = Math.Max(best, Minimax(boardCopy,
							depth + 1, !isMax));
 
						// Undo the move
						boardCopy[i, j] = TicTacToeState.none;
					}
				}
			}
			return best;
		}
 
		// If this minimizer's move
		else
		{
			int best = 1000;
 
			// Traverse all cells
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					// Check if cell is empty
					if (boardCopy[i, j] == TicTacToeState.none)
					{
						// Make the move
						boardCopy[i, j] = playerState;
 
						// Call minimax recursively and choose
						// the minimum value
						best = Math.Min(best, Minimax(boardCopy,
							depth + 1, !isMax));
 
						// Undo the move
						boardCopy[i, j] = TicTacToeState.none;
					}
				}
			}
			return best;
		}
	}

	private Move GetBestMove( TicTacToeState[,] boardCopy)
	{
		int bestVal = -1000;
		Move bestMove = new Move
		{
			row = -1,
			col = -1
		};

		// Traverse all cells, evaluate minimax function
		// for all empty cells. And return the cell
		// with optimal value.
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				// Check if cell is empty
				if (boardCopy[i, j] == TicTacToeState.none)
				{
					// Make the move
					boardCopy[i, j] = aiState;
 
					// compute evaluation function for this
					// move.
					int moveScore = Minimax(boardCopy, 0, false);
 
					// Undo the move
					boardCopy[i, j] = TicTacToeState.none;
 
					// If the value of the current move is
					// more than the best value, then update
					// best/
					if (moveScore > bestVal)
					{
						bestMove.row = i;
						bestMove.col = j;
						bestVal = moveScore;
					}
				}
			}
		}
 
		Debug.Log($"The value of the best Move is : {{0}}\n\n" bestVal);
 
		return bestMove;
	}

	private int GetBoardState(TicTacToeState[,] boardCopy)
	{
		// Checking for Rows for X or O victory.
		for (int row = 0; row < 3; row++)
		{
			if (boardCopy[row, 0] == boardCopy[row, 1] && boardCopy[row, 1] == boardCopy[row, 2])
			{
				if (boardCopy[row, 0] == TicTacToeState.cross)
					return +10;
				else if (boardCopy[row, 0] == TicTacToeState.circle)
					return -10;
			}
		}
 
		// Checking for Columns for X or O victory.
		for (int col = 0; col < 3; col++)
		{
			if (boardCopy[0, col] == boardCopy[1, col] && boardCopy[1, col] == boardCopy[2, col])
			{
				if (boardCopy[0, col] == TicTacToeState.cross)
					return +10;
				else if (boardCopy[0, col] == TicTacToeState.circle)
					return -10;
			}
		}
 
		// Checking for Diagonals for X or O victory.
		if (boardCopy[0, 0] == boardCopy[1, 1] && boardCopy[1, 1] == boardCopy[2, 2])
		{
			if (boardCopy[0, 0] == TicTacToeState.cross)
				return +10;
			else if (boardCopy[0, 0] == TicTacToeState.cross)
				return -10;
		}
		if (boardCopy[0, 2] == boardCopy[1, 1] && boardCopy[1, 1] == boardCopy[2, 0])
		{
			if (boardCopy[0, 2] == TicTacToeState.cross)
				return +10;
			else if (boardCopy[0, 2] == TicTacToeState.circle)
				return -10;
		}
 
		// Else if none of them have won then return 0
		return 0;
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
	
	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == aiState ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
	}
}

