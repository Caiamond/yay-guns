using Godot;
using System;
using System.Data.Common;
using System.Diagnostics;

public partial class Player : RigidBody3D
{
	[Export]
	public float Speed = 5.0f;
	[Export]
	public float JumpPower = 50f;
	[Export]
	public float RotationOffset = 0f;
	[Export]
	public PackedScene Bullet;
	[Export]
	public float Sensitivity = 1;

	private float yaw = 0;
	private float pitch = 0;
	private bool isFacingForward = false;

	private Pid3D _pid = new();
	private Node3D pivot;
	private Node3D camYaw;
	private Node3D camPitch;
	private SpringArm3D springArm;
	private Camera3D camera;
	private AnimationTree animationTree;
	private Node3D mesh;
	private RayCast3D aimRay;
	private RayCast3D groundRay;

	private void Shoot()
	{
		var targetPosition = Vector3.Zero;
			if (aimRay.IsColliding())
			{
				targetPosition = aimRay.GetCollisionPoint();
			}
			else
			{
				targetPosition = (aimRay.TargetPosition.Z * aimRay.GlobalBasis.Z) + aimRay.GlobalTransform.Origin;
			}

            var newBullet = Bullet.Instantiate<Bullet>();
			//newBullet.LookAtFromPosition(Position, camera.ProjectPosition(new Vector2(576, 324), 3000));
			//newBullet.LookAtFromPosition(Position, targetPosition);
			GetTree().Root.AddChild(newBullet);
			newBullet.GlobalPosition = Position;
			newBullet.LookAt(targetPosition);
			newBullet.LifeTime = 5f;
	}
    public override void _Ready()
    {
        base._Ready();
		pivot = GetNode<Node3D>("CamOrigin");
		camYaw = GetNode<Node3D>("CamOrigin/CamYaw");
		camPitch = GetNode<Node3D>("CamOrigin/CamYaw/CamPitch");
		springArm = GetNode<SpringArm3D>("CamOrigin/CamYaw/CamPitch/SpringArm3D");
		camera = GetNode<Camera3D>("CamOrigin/CamYaw/CamPitch/SpringArm3D/Camera3D");
		mesh = GetNode<Node3D>("Character");
		animationTree = GetNode<AnimationTree>("Character/AnimationTree");
		aimRay = GetNode<RayCast3D>("CamOrigin/CamYaw/CamPitch/SpringArm3D/Camera3D/RayCast3D");
		groundRay = GetNode<RayCast3D>("GroundCheck");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_pid.init(1f, 0.001f, 1f);
		springArm.AddExcludedObject(this.GetRid());
		Input.UseAccumulatedInput = false;

		Debug.WriteLine("양소예 바보");
    }

    public override void _Input (InputEvent @event)
    {
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			//pivot.RotateY(Mathf.DegToRad(-inputEventMouseMotion.Relative.X));
			yaw += -inputEventMouseMotion.Relative.X * Sensitivity;
			pitch = Math.Clamp(pitch - (inputEventMouseMotion.Relative.Y * Sensitivity), -90, 45);

			camYaw.RotationDegrees = new Vector3(0, yaw, 0);
			camPitch.RotationDegrees = new Vector3(pitch, 0, 0); 
		}

		if (@event is InputEventJoypadMotion)
		{
			//yaw += 
		}

/*
		if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				Shoot();
			}
		}
*/
		
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
		if (Input.GetActionStrength("Use") == 1)
		{
			isFacingForward = true;
			Shoot();
			Input.StartJoyVibration(0,0,0.5f,0.05f);
		}
		else if (Input.GetActionStrength("Aim") == 1)
		{
			isFacingForward = true;
		}
		else
		{
			isFacingForward = false;
		}
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

		if (Input.GetActionStrength("Jump") == 1 && groundRay.IsColliding())
		{
			ApplyImpulse(new Vector3(0,JumpPower,0));
        }

		/*
		camYaw.RotationDegrees = new Vector3(0, yaw, 0);
		camPitch.RotationDegrees = new Vector3(pitch, 0, 0); 
		*/
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized().Rotated(Vector3.Up, camYaw.Rotation.Y);
		Vector3 targetVelocity = new Vector3(direction.X, 0, direction.Z) * Speed;
		Vector3 currentVelocityWithoutYAxis = new(LinearVelocity.X, 0, LinearVelocity.Z);
		var velocity_error = targetVelocity - currentVelocityWithoutYAxis;
		var impulse = _pid.update(velocity_error);
		ApplyCentralForce(impulse);
		Vector3 newVelocityWithoutYAxis = new(LinearVelocity.X, 0, LinearVelocity.Z);

		animationTree.Set("parameters/BlendSpace1D/blend_position", newVelocityWithoutYAxis.Length()/Speed);


		if (isFacingForward)
		{
			mesh.Rotation = new Vector3(0, yaw, 0);
		}
		else if (direction != Vector3.Zero)
		{
			mesh.Rotation = new Vector3(0, Mathf.Atan2(direction.X, direction.Z) + Mathf.DegToRad(RotationOffset) ,0); 
		}
    }
}
