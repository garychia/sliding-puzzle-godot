using Godot;
using System;

public class Block : Node2D
{
	// The location of this block on the panel (row and column).
	private Vector2 _coord;
	public Vector2 Coordinates
	{
		get => _coord;
		set => _coord = value;
	}

	// The number this block represent.
	private uint _number;
	public uint Number
	{
		get => _number;
		set => _number = value;
	}

	// Event invoked when this block is pressed.
	public event Action<Block> OnPressed;

	// Called when this node is ready.
	public override void _Ready()
	{
		// Find the label that displays the number.
		var numberLabel = GetChild<Label>(1);
		numberLabel.Text = _number.ToString();

		// Find the button associated with this block.
		var button = GetChild<Button>(2);
		button.Connect("pressed", this, "_onPressed");
	}

	// Set the coordinates and number of this block.
	public void Setup(Vector2 Coordinate, uint Number)
	{
		_coord = Coordinate;
		_number = Number;
	}

	// When pressed, invoke the OnPressed event.
	void _onPressed()
	{
		OnPressed?.Invoke(this);
	}

	// Return a dictionary filled with the coordinates and number
	// of this block.
	public Godot.Collections.Dictionary<string, object> Save()
	{
		return new Godot.Collections.Dictionary<string, object>()
		{
			{ "CoordX", Coordinates.x },
			{ "CoordY", Coordinates.y },
			{ "Number", Number }	
		};
	}
}
