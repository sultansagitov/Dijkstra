using Godot;
using System;

public partial class Vertex : Node2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void Select()
	{
		GetNode<Sprite2D>("Select").Visible = true;
	}

	public void Deselect()
	{
		GetNode<Sprite2D>("Select").Visible = false;
	}
}
