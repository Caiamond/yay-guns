using Godot;
using System;

public partial class Bullet : RigidBody3D
{
	[Export]
	public float LifeTime;
	private bool hasAlreadyFired = false;
	private Basis startingBasis;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		base._Ready();	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		
		LifeTime -= (float)delta;
		if (LifeTime <= 0)
		{
			this.QueueFree();
		}
	}

    public override void _PhysicsProcess(double delta)
    {
		if (!hasAlreadyFired)
		{
			hasAlreadyFired = true;
			startingBasis = Basis;
			Position += startingBasis * Vector3.Forward;
			ApplyCentralImpulse(startingBasis * Vector3.Forward * 500);
		}

        base._PhysicsProcess(delta);
		
		//ApplyCentralForce(startingBasis * Vector3.Forward * 100);
    }
}
