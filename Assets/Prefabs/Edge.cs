using Godot;
using System;

public partial class Edge : Node2D
{
	[Export]
	public Node2D linecenter;
	[Export]
	public Label len;

	public Vertex a;
	public Vertex b;
	
	public override void _Ready()
	{
		linecenter = GetNode<Node2D>("linecenter");
		len = GetNode<Label>("len");
	}

	public override void _Process(double _delta)
	{

		Vector2 delta = a.Position - b.Position;
		float length = delta.Length();

		Position = (a.Position + b.Position) / 2;
		linecenter.Scale = new Vector2(length / 2, 1);
		linecenter.RotationDegrees = Mathf.Atan(delta.Y / delta.X) * 180 / Mathf.Pi;
		len.Text = length.ToString();

		if (a == b) {
			GetParent().RemoveChild(this);
		}

		if (!(a.IsInsideTree() && b.IsInsideTree()))
			GetParent().RemoveChild(this);
	}
}
