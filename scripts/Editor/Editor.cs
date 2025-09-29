using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class Editor : Node3D
{
    [Export] public PackedScene BlockScene;
    private List<Block> blocks = new List<Block>();
    private Camera3D camera;
    private BlockType currentBlockType = BlockType.Stone;
    [Export] public float Speed = 5f;
    [Export] public float SpeedBoost = 3f;
    [Export] public float MouseSensitivity = 0.1f;

    private Vector3 _velocity = Vector3.Zero;
    private float _yaw = 0f;
    private float _pitch = 0f;
    private bool _rotating = false;
    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera3D");
        Input.MouseMode = Input.MouseModeEnum.Visible;

        // Vincular OptionButton de la UI
        var selector = GetNode<OptionButton>("UI/BlockSelector");
        selector.AddItem("Stone", (int)BlockType.Stone);
        selector.AddItem("Wood", (int)BlockType.Wood);
        selector.AddItem("Metal", (int)BlockType.Metal);
        selector.Connect("item_selected", new Callable(this, nameof(OnBlockSelected)));

        var saveMapButton = GetNode<Button>("UI/SaveMapButton");
        saveMapButton.Pressed += () => SaveMap();
    }

    public override void _Process(double delta)
    {
        Vector3 dir = Vector3.Zero;

        if (Input.IsActionPressed("move_forward")) dir -= camera.Transform.Basis.Z;
        if (Input.IsActionPressed("move_back")) dir += camera.Transform.Basis.Z;
        if (Input.IsActionPressed("move_left")) dir -= camera.Transform.Basis.X;
        if (Input.IsActionPressed("move_right")) dir += camera.Transform.Basis.X;
        if (Input.IsActionPressed("move_up")) dir += camera.Transform.Basis.Y;
        if (Input.IsActionPressed("move_down")) dir -= camera.Transform.Basis.Y;

        if (dir != Vector3.Zero)
        {
            dir = dir.Normalized();
            float currentSpeed = Speed * (Input.IsActionPressed("ui_shift") ? SpeedBoost : 1f);
            camera.GlobalTranslate(dir * currentSpeed * (float)delta);
        }
    }

    private void OnBlockSelected(long index)
    {
        currentBlockType = (BlockType)index;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                _rotating = mouseButton.Pressed;
                Input.MouseMode = _rotating ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
            }
        }

        // Rotación con el mouse
        if (@event is InputEventMouseMotion motion && _rotating)
        {
            _yaw -= motion.Relative.X * MouseSensitivity;
            _pitch -= motion.Relative.Y * MouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            camera.RotationDegrees = new Vector3(_pitch, _yaw, 0);
        }
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GD.Print("Mouse button pressed");
            var spaceState = GetWorld3D().DirectSpaceState;
            var from = camera.ProjectRayOrigin(mouseEvent.Position);
            var to = from + camera.ProjectRayNormal(mouseEvent.Position) * 20;

            var parameters = new PhysicsRayQueryParameters3D
            {
                From = from,
                To = to
            };
            var result = spaceState.IntersectRay(parameters);
            GD.Print($"Raycast result: {result}");
            if (result.Count > 0)
            {
                Vector3 hitPos = ((Vector3)result["position"]).Snapped(Vector3.One);

                if (mouseEvent.ButtonIndex == MouseButton.Left)
                    PlaceBlock(hitPos, currentBlockType);
                else if (mouseEvent.ButtonIndex == MouseButton.Right)
                    RemoveBlock(hitPos);
            }
        }
        else if (@event.IsActionPressed("rotate_block"))
        {
            if (blocks.Count > 0)
            {
                blocks[^1].RotateY(Mathf.DegToRad(90));
                GD.Print("Rotated last block");
            }
        }
    }

    private void PlaceBlock(Vector3 pos, BlockType type)
    {
        var block = BlockScene.Instantiate<Block>();
        AddChild(block);
        block.SetGridPosition(pos);
        block.SetBlockType(type); // le asignamos material
        blocks.Add(block);
        GD.Print($"Placed {type} block at {pos}");
    }

    private void RemoveBlock(Vector3 pos)
    {
        foreach (var block in blocks)
        {
            if (block.GridPosition == new Vector3I((int)pos.X, (int)pos.Y, (int)pos.Z))
            {
                block.QueueFree();
                blocks.Remove(block);
                GD.Print($"Removed block at {pos}");
                break;
            }
        }
    }

    public void SaveMap(string path = "user://map.json")
    {
        var data = new List<Dictionary<string, object>>();
        foreach (var block in blocks)
        {
            data.Add(new Dictionary<string, object> {
                {"x", block.GridPosition.X},
                {"y", block.GridPosition.Y},
                {"z", block.GridPosition.Z},
                {"type", (int)block.CurrentType}
            });
        }
        string json = JsonSerializer.Serialize(data);
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(json);
        file.Close();
        GD.Print($"JSON Data: {json}");
        GD.Print($"Map saved to {path}");
    }
}