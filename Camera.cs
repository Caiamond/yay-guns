using Godot;
using System;

public partial class Camera : Camera3D
{
	[Export]
	public Node3D Target;

	private Pid3D _pid = new Pid3D();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position = Target.Position + new Vector3(0,2,5);
	}
}
