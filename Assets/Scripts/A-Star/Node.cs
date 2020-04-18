using UnityEngine;

namespace AStar
{
	public class Node
	{
		public Node Parent;
		public Vector2Int Position;
		public float DistanceToTarget;
		public float Cost;
		public bool Walkable;

		public Node(Vector2Int pos, bool walkable)
		{
			Parent = null;
			Position = pos;
			DistanceToTarget = -1;
			Cost = 1;
			Walkable = walkable;
		}

		public float F
		{
			get
			{
				if (DistanceToTarget != -1 && Cost != -1)
					return DistanceToTarget + Cost;
				else
					return -1;
			}
		}
	} 
}