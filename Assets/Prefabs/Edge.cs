using Godot;
using System;

public partial class Edge : Node2D
{
	[Export] public Node2D linecenter;
	[Export] public Label len;

	public bool removeInNextFrame = false; // i love kostyli
	public bool createVertexInNextFrame = false; // i love kostyli

	public Vertex a;
	public Vertex b;
	private int size;
	public int Size
	{
		get => size;
		set
		{
			size = value;
			len.Text = value.ToString();
		}
	}


	public bool HasVertex(Vertex x) => a == x || b == x;
	public bool HasVertex(Vertex a, Vertex b) => this.a == a && this.b == b || this.b == a && this.a == b;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void Gui_Input(InputEvent ev)
	{
		if (ev is InputEventMouseButton @e)
		{
			createVertexInNextFrame |= @e.ButtonIndex == MouseButton.Left && @e.Pressed;
			removeInNextFrame |= @e.ButtonIndex == MouseButton.Right && @e.Pressed;
		}
	}
}
