using UnityEngine;

namespace MiniGolf
{
	public class Pipe : Block
	{
		public Direction lookingDirection;
		public Direction enterDirection;
		public Pipe exitPipe;
		public MeshRenderer meshRenderer;

		public void SetLookingDirection(Direction lookingDirection)
		{
			this.lookingDirection = lookingDirection;

			switch (this.lookingDirection)
			{
				case Direction.UP:
					transform.rotation = Quaternion.Euler(0, 90, 0);
					enterDirection = Direction.DOWN;
					break;
				case Direction.DOWN:
					transform.rotation = Quaternion.Euler(0, -90, 0);
					enterDirection = Direction.UP;
					break;
				case Direction.LEFT:
					transform.rotation = Quaternion.Euler(0, 0, 0);
					enterDirection = Direction.RIGHT;
					break;
				case Direction.RIGHT:
					transform.rotation = Quaternion.Euler(0, 180, 0);
					enterDirection = Direction.LEFT;
					break;
			}
		}

		public void SetColor(Color color)
		{
			meshRenderer.materials[0].color = color;
		}

		public void SetExitPipe(Pipe exitPipe)
		{
			this.exitPipe = exitPipe;
			exitPipe.meshRenderer.materials[0] = meshRenderer.materials[0];
		}

		public void BallEnter(Ball ball)
		{
			ball.x = exitPipe.x;
			ball.z = exitPipe.z;
			ball.direction = exitPipe.lookingDirection;
		}
	}
}