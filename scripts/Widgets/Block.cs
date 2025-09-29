using Godot;
using System;

public partial class Block : StaticBody3D
{
	public Vector3I GridPosition { get; private set; }
	public BlockType CurrentType { get; private set; }

	public void SetGridPosition(Vector3 pos)
	{
		GridPosition = new Vector3I((int)pos.X, (int)pos.Y, (int)pos.Z);
		GlobalPosition = GridPosition;
	}

	public void SetBlockType(BlockType type)
	{
		CurrentType = type;
		var mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		switch (type)
		{
			case BlockType.Stone:
				mesh.MaterialOverride = GD.Load<Material>("res://assets/materials/Stone.tres");
				break;
			case BlockType.Wood:
				mesh.MaterialOverride = GD.Load<Material>("res://assets/materials/Wood.tres");
				break;
			case BlockType.Metal:
				mesh.MaterialOverride = GD.Load<Material>("res://assets/materials/Metal.tres");
				break;
		}
	}
}
