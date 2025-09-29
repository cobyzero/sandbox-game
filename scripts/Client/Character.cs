using Godot;
using System;

public partial class Character : CharacterBody3D
{
    [Export] public float WalkSpeed = 5f;
    [Export] public float RunSpeed = 10f;
    [Export] public float JumpForce = 6f;
    [Export] public float Gravity = 12f;
    [Export] public float MouseSensitivity = 0.1f;

    private Camera3D camera;
    private float yaw = 0f;
    private float pitch = 0f;
    private Vector3 velocity = Vector3.Zero;
    private bool rotating = false;
    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera3D");
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                rotating = mouseButton.Pressed;
                Input.MouseMode = rotating ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
            }
        }

        if (@event is InputEventMouseMotion motion && rotating)
        {
            yaw -= motion.Relative.X * MouseSensitivity;
            pitch -= motion.Relative.Y * MouseSensitivity;
            pitch = Mathf.Clamp(pitch, -60, 60);

            RotationDegrees = new Vector3(0, yaw, 0);
            camera.RotationDegrees = new Vector3(pitch, 0, 0);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Gravedad
        if (!IsOnFloor())
            velocity.Y -= Gravity * dt;
        else
            velocity.Y = -0.1f;

        // Movimiento en el plano
        Vector3 inputDir = Vector3.Zero;
        if (Input.IsActionPressed("move_forward")) inputDir.Z -= 1;
        if (Input.IsActionPressed("move_back")) inputDir.Z += 1;
        if (Input.IsActionPressed("move_left")) inputDir.X -= 1;
        if (Input.IsActionPressed("move_right")) inputDir.X += 1;

        inputDir = inputDir.Normalized();

        float speed = Input.IsActionPressed("ui_shift") ? RunSpeed : WalkSpeed;

        // Dirección global
        Vector3 direction = (Transform.Basis * inputDir).Normalized();
        velocity.X = direction.X * speed;
        velocity.Z = direction.Z * speed;

        // Saltar
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            velocity.Y = JumpForce;

        Velocity = velocity;
        MoveAndSlide();
    }
}