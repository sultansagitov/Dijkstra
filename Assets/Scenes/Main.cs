using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	public Node2D vertices;
	public Node2D edges;
	public List<Vertex> verticesLst = new();

	public Vertex selected = null;

	public override void _Ready()
	{
		vertices = GetNode<Node2D>("Vertices");
		edges = GetNode<Node2D>("Edges");
	}

	public override void _Process(double delta)
	{
		var mousepos = GetGlobalMousePosition();
		double dist = 50.0;
		Vertex nearest = null;

		foreach (var ver in verticesLst)
		{
			double d = (mousepos - ver.Position).Length();

			if (d < dist)
			{
				dist = d;
				nearest = ver;
			}
		}

        if (Input.IsActionPressed("save_selected"))
        {
        }
        else
        {
            if (nearest != null)
            {
				// Select other Vertex
                if (selected != nearest)
                {
					MoveSelection(nearest);
                }
            }

            if (selected != null)
            {
				// Deselect when mouse too far
				if ((mousepos - selected.Position).Length() > 50.0)
				{
					selected.Deselect();
					selected = null;
				}
			}
        }


		if (Input.IsActionJustPressed("leftmouse"))
		{
			Vertex vert;

			if (dist > 40)
			{
				// Create new vertex
				vert = GD.Load<PackedScene>("res://Assets/Prefabs/Vertex.tscn").Instantiate<Vertex>();
				vert.Position = mousepos;
				vertices.AddChild(vert);
				verticesLst.Add(vert);
			}
			else
			{
				nearest.Position = (nearest.Position + mousepos) / 2;
				vert = nearest;
			}

			if (selected != null)
				CreateEdge(vert, selected);
				
			MoveSelection(vert);
		}


		if (Input.IsActionJustPressed("rightmouse") && selected != null)
		{
			vertices.RemoveChild(selected);
			verticesLst.Remove(selected);
			selected = null;
		}
	}

	public void MoveSelection(Vertex newVert) {
		selected?.Deselect();
		newVert.Select();
		selected = newVert;
	}

	public void CreateEdge(Vertex a, Vertex b)
	{
		Vector2 delta = a.Position - b.Position;
		float length = delta.Length();

		var edge = GD.Load<PackedScene>("res://Assets/Prefabs/Edge.tscn").Instantiate<Edge>();
		edge.Position = (a.Position + b.Position) / 2;
		edge.linecenter.Scale = new Vector2(length / 2, 1);
		edge.linecenter.RotationDegrees = Mathf.RadToDeg(Mathf.Atan(delta.Y / delta.X));
		edge.len.Text = length.ToString();
		edge.a = a;
		edge.b = b;

		edges.AddChild(edge);
	}
}
