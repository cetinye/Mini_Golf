using UnityEngine;

namespace GrandTour
{
	public class Obstacle : Block, IClickable
	{
		public Rotation rotation;

		public void OnClick()
		{
			transform.Rotate(0, 90, 0);
			rotation = rotation == Rotation.RightUp ? Rotation.RightDown : Rotation.RightUp;
		}

		public Direction GetDirection(Direction incomingDirection)
		{
			if (rotation == Rotation.RightDown)
			{
				if (incomingDirection == Direction.UP)
				{
					return Direction.LEFT;
				}
				if (incomingDirection == Direction.DOWN)
				{
					return Direction.RIGHT;
				}
				if (incomingDirection == Direction.RIGHT)
				{
					return Direction.DOWN;
				}
				if (incomingDirection == Direction.LEFT)
				{
					return Direction.UP;
				}
			}
			else if (rotation == Rotation.RightUp)
			{
				if (incomingDirection == Direction.DOWN)
				{
					return Direction.LEFT;
				}

				if (incomingDirection == Direction.UP)
				{
					return Direction.RIGHT;
				}

				if (incomingDirection == Direction.RIGHT)
				{
					return Direction.UP;
				}

				if (incomingDirection == Direction.LEFT)
				{
					return Direction.DOWN;
				}
			}

			return Direction.UP;
		}
	}

	public enum Rotation
	{
		RightDown,
		RightUp
	}
}