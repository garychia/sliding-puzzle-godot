using Godot;
using System.Collections.Generic;

public class GameManger : Node2D
{
	// Number of blocks in each row.
	public uint BlocksPerLine = 4;

	// Size of a block on the panel.
	private Vector2 _blockSize;
	// Block that is invisible.
	private Block _missingBlock;
	// List of all blocks in the game.
	private List<Block> _blocks = new List<Block>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Create the blocks.
		CreateBlocks();

		// Configure the camera.
		var camera = GetChild<Camera2D>(0);
		camera.GlobalPosition += _blockSize * BlocksPerLine * .5f;
		camera.Zoom = Vector2.One * .092f * BlocksPerLine;


		// Find out the buttons and set up their signals.
		var canvas = GetChild<CanvasLayer>(1);
		var vBoxContainer = canvas.GetChild<VBoxContainer>(0);
		var saveButton = vBoxContainer.GetChild<Button>(0);
		var loadButton = vBoxContainer.GetChild<Button>(1);
		var restartButton = vBoxContainer.GetChild<Button>(2);
		saveButton.Connect("pressed", this, "OnSaveButtonClicked");
		loadButton.Connect("pressed", this, "OnLoadButtonClicked");
		restartButton.Connect("pressed", this, "StartNewGame");
	}

	// Restart the game.
	private void StartNewGame()
	{
		DestroyBlocks();
		CreateBlocks();
	}

	// Create blocks.
	private void CreateBlocks(List<Godot.Collections.Dictionary<string, object>> blocksData = null, uint hiddenNumber = 0)
	{
		// The block is saved as a scene.
		var blockScene = GD.Load<PackedScene>("res://Scenes/Block.tscn");
		// Maximum number of blocks to be created.
		var maximumNumber = BlocksPerLine * BlocksPerLine;
		// If no data is being loaded, randomly initialize the game.
		if (blocksData == null)
		{
			// Record the numbers that have been used.
			var selectedNumber = new HashSet<uint>();
			var rand = new RandomNumberGenerator();
			// Set up the random seed.
			rand.Randomize();
			// For each row
			for (uint y = 0; y < BlocksPerLine; y++)
			{
				// For each column
				for (uint x = 0; x < BlocksPerLine; x++)
				{
					// Instantiate a block.
					var block = blockScene.Instance() as Block;
					// Get the size of the block.
					var blockSize = block.GetChild<ColorRect>(0).RectSize;
					// Generate a number randomly.
					uint blockNumber = rand.Randi() % maximumNumber + 1;
					// Generate a number again if the number has been used.
					while (selectedNumber.Contains(blockNumber))
					{
						blockNumber = rand.Randi() % maximumNumber + 1;
					}
					// Record the number as used.
					selectedNumber.Add(blockNumber);
					// Configure the block.
					SetupBlock(block, y, x, blockNumber);
					// Make it a child of this node.
					AddChild(block);
					// Record the block and check if the block is the
					// blank block.
					_blocks.Add(block);
					if (y == BlocksPerLine - 1 && x == BlocksPerLine - 1)
					{
						_blockSize = blockSize;
						_missingBlock = block;
					}
				}
			}
		}
		// Load a saved game.
		else
		{
			// Each data point represents a block.
			foreach (var blockData in blocksData)
			{
				// Instantiate a block.
				var block = blockScene.Instance() as Block;
				// Restore the block from the data.
				var blockSize = block.GetChild<ColorRect>(0).RectSize;
				SetupBlock(
					block,
					(uint)(float)blockData["CoordY"],
					(uint)(float)blockData["CoordX"],
					(uint)(float)blockData["Number"]);
				// Make it a child of this node.
				AddChild(block);
				_blocks.Add(block);
				if (block.Number == hiddenNumber)
				{
					_missingBlock = block;
					_blockSize = blockSize;
				}
			}
		}
		// Set the blank block to be invisible.
		_missingBlock.Visible = false;
	}

	// Setup a block.
	private void SetupBlock(Block block, uint row, uint col, uint blockNumber)
	{
		// Calculate the position where to place the block.
		var blockSize = block.GetChild<ColorRect>(0).RectSize;
		var blockX = col * blockSize.x;
		var blockY = row * blockSize.y;
		// Set up the coordinates and number of the block.
		block.Setup(new Vector2(col, row), blockNumber);
		block.GlobalPosition = new Vector2(blockX, blockY);
		// Subscribe the OnPressed event.
		block.OnPressed += HandleBlockPressed;
	}

	// Called when a block is pressed.
	private void HandleBlockPressed(Block block)
	{
		// Check if the block is next to the blank block.
		if (block.Coordinates.DistanceTo(_missingBlock.Coordinates) == 1.0f)
		{
			// Swap the positions of the block and the blank block.
			var blockPosition = block.GlobalPosition;
			block.GlobalPosition = _missingBlock.GlobalPosition;
			_missingBlock.GlobalPosition = blockPosition;
			var blockCoord = block.Coordinates;
			block.Coordinates = _missingBlock.Coordinates;
			_missingBlock.Coordinates = blockCoord;
			// Check if the player has won the game.
			if (CheckBlocksInOrder())
				_missingBlock.Visible = true;
		}
	}

	// Check if the player has placed each block at the correct position.
	private bool CheckBlocksInOrder()
	{
		// Check if each block is at the correct position.
		foreach (Block block in _blocks)
		{
			var expectedRow = (block.Number - 1) / BlocksPerLine;
			var expectedCol = (block.Number - 1) % BlocksPerLine;
			if (block.Coordinates != new Vector2(expectedRow, expectedCol))
				return false;
		}
		return true;
	}

	// Called when the save button is pressed.
	private void OnSaveButtonClicked()
	{
		SaveGame();
	}

	// Called when the load button is pressed.
	private void OnLoadButtonClicked()
	{
		LoadGame();
	}

	// Save the current state of the game.
	private void SaveGame()
	{
		// Create a file to save the data.
		var saveFile = new File();
		saveFile.Open("user://slidingpuzzle.save", File.ModeFlags.Write);


		// Record the game state.
		var gameState = new Godot.Collections.Dictionary<string, object>()
		{
			{ "BlocksPerLine", BlocksPerLine },
			{ "HiddenNumber", _missingBlock.Number }
		};
		saveFile.StoreLine(JSON.Print(gameState));
		// Save the state of each block.
		foreach (var block in _blocks)
		{
			saveFile.StoreLine(JSON.Print(block.Save()));
		}
		// Close the file.
		saveFile.Close();
	}

	private void LoadGame()
	{
		// Load the save file.
		var saveFile = new File();
		// If not existing, do nothing.
		if (!saveFile.FileExists("user://slidingpuzzle.save"))
			return;
		// Clear all the blocks in the game.
		DestroyBlocks();

		// Load the game data from the save file.
		saveFile.Open("user://slidingpuzzle.save", File.ModeFlags.Read);

		// Retrieve the state of the last game.
		var gameState = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(saveFile.GetLine()).Result);

		// Record the information from the file.
		BlocksPerLine = (uint)(float)gameState["BlocksPerLine"];
		var hiddenNumber = (uint)(float)gameState["HiddenNumber"];
		// Collect the data of each block.
		var blocksData = new List<Godot.Collections.Dictionary<string, object>>();
		while (saveFile.GetPosition() < saveFile.GetLen())
		{
			var blockData = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(saveFile.GetLine()).Result);
			blocksData.Add(blockData);
		}
		// Restore the blocks from the saved data.
		CreateBlocks(blocksData, hiddenNumber);
		// Close the file.
		saveFile.Close();
	}

	// Destroy all the blocks in the current game.
	private void DestroyBlocks()
	{
		// Remove the blocks from the node tree.
		foreach (Block block in _blocks)
		{
			block.QueueFree();
		}
		// Reset the attributes.
		_blocks.Clear();
		_missingBlock = null;
	}
}
