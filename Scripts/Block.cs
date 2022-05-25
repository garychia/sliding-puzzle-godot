using Godot;
using System;

public class Block : Node2D
{
	private Vector2 _coord;
	private int _number;

	public override void _Ready()
	{
		var numberLabel = GetChild<Label>(1);
		numberLabel.Text = _number.ToString();
	}

	public void Setup(Vector2 Coordinate, int Number)
	{
		_coord = Coordinate;
		_number = Number;
	}
}
