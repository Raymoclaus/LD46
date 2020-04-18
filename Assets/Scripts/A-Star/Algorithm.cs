using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AStar
{
	public class Algorithm
	{
		private List<List<Node>> Grid;

		public Algorithm(List<List<Node>> grid)
		{
			Grid = grid;
		}

		public Stack<Node> FindPath(Vector2Int Start, Vector2Int End)
		{
			Node start = new Node(new Vector2Int(Start.x, Start.y), true);
			Node end = new Node(new Vector2Int(End.x, End.y), true);

			Stack<Node> Path = new Stack<Node>();
			List<Node> OpenList = new List<Node>();
			List<Node> ClosedList = new List<Node>();
			List<Node> adjacencies;
			Node current = start;

			// add start node to Open List
			OpenList.Add(start);

			while (OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
			{
				current = OpenList[0];
				OpenList.Remove(current);
				ClosedList.Add(current);
				adjacencies = GetAdjacentNodes(current);


				foreach (Node n in adjacencies)
				{
					if (!ClosedList.Contains(n) && n.Walkable)
					{
						if (!OpenList.Contains(n))
						{
							n.Parent = current;
							n.DistanceToTarget = Math.Abs(n.Position.x - end.Position.x) + Math.Abs(n.Position.y - end.Position.y);
							n.Cost = 1 + n.Parent.Cost;
							OpenList.Add(n);
							OpenList = OpenList.OrderBy(node => node.F).ToList<Node>();
						}
					}
				}
			}

			// construct path, if end was not closed return null
			if (!ClosedList.Exists(x => x.Position == end.Position))
			{
				return null;
			}

			// if all good, return path
			Node temp = ClosedList[ClosedList.IndexOf(current)];
			while (temp.Parent != start && temp != null)
			{
				Path.Push(temp);
				temp = temp.Parent;
			}
			return Path;
		}

		private int GridRows => Grid[0].Count;

		private int GridCols => Grid.Count;

		private List<Node> GetAdjacentNodes(Node n)
		{
			List<Node> temp = new List<Node>();

			int row = n.Position.y;
			int col = n.Position.x;

			if (row + 1 < GridRows)
			{
				temp.Add(Grid[col][row + 1]);
			}
			if (row - 1 >= 0)
			{
				temp.Add(Grid[col][row - 1]);
			}
			if (col - 1 >= 0)
			{
				temp.Add(Grid[col - 1][row]);
			}
			if (col + 1 < GridCols)
			{
				temp.Add(Grid[col + 1][row]);
			}

			return temp;
		}
	} 
}