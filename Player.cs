using Godot;
using System;
using System.Diagnostics;

public partial class Player : RigidBody3D
{
	[Export]
	public float Speed = 5.0f;
	[Export]
	public float JumpVelocity = 4.5f;
	private Node3D pivot;

	private float yaw = 0;
	private float pitch = 0;

	private Node3D camYaw;
	private Node3D camPitch;
	private SpringArm3D springArm;
	private Pid3D _pid = new Pid3D();
	
	private MeshInstance3D mesh;

    public override void _Ready()
    {
        base._Ready();
		pivot = GetNode<Node3D>("CamOrigin");
		camYaw = GetNode<Node3D>("CamOrigin/CamYaw");
		camPitch = GetNode<Node3D>("CamOrigin/CamYaw/CamPitch");
		springArm = GetNode<SpringArm3D>("CamOrigin/CamYaw/CamPitch/SpringArm3D");
		mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_pid.init(1f, 0.001f, 1f);
		springArm.AddExcludedObject(this.GetRid());
		Input.UseAccumulatedInput = false;
    }

    public override void _Input(InputEvent @event)
    {
        //base._Input(@event);
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			//pivot.RotateY(Mathf.DegToRad(-inputEventMouseMotion.Relative.X));
			yaw += -inputEventMouseMotion.Relative.X;
			pitch = Math.Clamp(pitch - inputEventMouseMotion.Relative.Y, -90, 45);
		}
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
		
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

		camYaw.RotationDegrees = new Vector3(0, yaw, 0);
		camPitch.RotationDegrees = new Vector3(pitch, 0, 0); 

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized().Rotated(Vector3.Up, camYaw.Rotation.Y);
		Vector3 targetVelocity = new Vector3(direction.X, 0, direction.Z) * Speed;
		Vector3 currentVelocityWithoutYAxis = new(LinearVelocity.X, 0, LinearVelocity.Z);
		var velocity_error = targetVelocity - currentVelocityWithoutYAxis;
		var impulse = _pid.update(velocity_error);
		ApplyCentralForce(impulse);

		if (direction != Vector3.Zero)
		{
			mesh.Rotation = new Vector3(0,Mathf.Atan2(direction.X, direction.Z) ,0); 
		}
    }
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        //base._IntegrateForces(state);

		
    }
}
