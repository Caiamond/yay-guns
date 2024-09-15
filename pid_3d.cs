using Godot;
using System;

[GlobalClass]
public partial class Pid3D : RefCounted
{
	float _p;
	float _i;
	float _d;

	Vector3 _prev_error;
	Vector3 _error_integral;

	public void init(float p, float i, float d)
	{
		_p = p;
		_i = i;
		_d = d;
	}

	public Vector3 update(Vector3 error)
	{
		_error_integral += error;
		var error_derivative = (error - _prev_error);
		_prev_error = error;
		return _p * error + _i * _error_integral * _d * error_derivative;
	}
}
