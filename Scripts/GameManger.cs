using Godot;
using System;

public class GameManger : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public int BlocksPerLine = 4;

	private Vector2 _blockSize;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateBlocks();
		var camera = GetChild<Camera2D>(0);
		camera.GlobalPosition += _blockSize * BlocksPerLine * .5f;
		camera.Zoom = Vector2.One * .092f * BlocksPerLine;
	}

	private void CreateBlocks()
	{
		var blockScene = GD.Load<PackedScene>("res://Scenes/Block.tscn");
		for (int y = 0; y < BlocksPerLine; y++)
		{
			for(int x = 0; x < BlocksPerLine; x++)
			{
				var block = blockScene.Instance() as Block;
				var blockSize = block.GetChild<ColorRect>(0).RectSize;
				var blockX = x * blockSize.x;
				var blockY = y * blockSize.y;
				block.Setup(new Vector2(x, y), 1 + x + BlocksPerLine * y);
				block.GlobalPosition = new Vector2(blockX, blockY);
				AddChild(block);
				_blockSize = blockSize;
			}
		}
	}
}
