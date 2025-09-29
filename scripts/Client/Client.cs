using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class Client : Node3D
{
    [Export] public PackedScene BlockScene;

    public override void _Ready()
    {
        LoadMap("user://map.json");
    }

    private void LoadMap(string path)
    {
        if (!FileAccess.FileExists(path)) return;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        var data = JsonSerializer.Deserialize<List<Dictionary<string, float>>>(json);

        foreach (var blockData in data)
        {
            var pos = new Vector3(blockData["x"], blockData["y"], blockData["z"]);
            var block = BlockScene.Instantiate<Block>();
            AddChild(block);
            block.SetGridPosition(pos);
            block.SetBlockType((BlockType)(int)blockData["type"]);

        }
    }
}
