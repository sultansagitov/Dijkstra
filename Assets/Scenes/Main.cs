using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	public Node2D vertices;
	public Node2D edges;
	private List<Vertex> verticesLst = new();

	public Vertex selected = null;
	private float holdingFor = 0f;
	private bool isHolding = false;
	public Vertex secondSelected = null;

	private Vector2 prevmousepos;

	public override void _Ready()
	{
		vertices = GetNode<Node2D>("Vertices");
		edges = GetNode<Node2D>("Edges");
		prevmousepos = GetGlobalMousePosition();
	}

	public override void _Process(double delta)
	{
		var mousepos = GetGlobalMousePosition();
		double dist = 50.0;
		Vertex nearest = null;

		foreach (var vert in verticesLst)
		{
			double d = (mousepos - vert.Position).Length();

			if (d < dist)
			{
				dist = d;
				nearest = vert;
			}
		}

		if (secondSelected != null && (mousepos - secondSelected.Position).Length() > 50.0)
		{
			secondSelected.Deselect();
			secondSelected = null;
		}

		if (Input.IsActionPressed("save_selected") || isHolding)
		{
			if (Input.IsActionPressed("connection"))
			{
				double dist2 = 50f;

				foreach (var vert in verticesLst)
				{
					if (vert != selected)
					{
						double d = (mousepos - vert.Position).Length();

						if (d < dist2)
						{
							dist2 = d;
							secondSelected = vert;
						}
					}
				}

				if (secondSelected != null)
				{
					secondSelected.SelectAsSecond();
				}
			}
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

		if (Input.IsActionJustReleased("leftmouse"))
		{
			holdingFor = 0f;
			isHolding = false;
		}

		if (Input.IsActionPressed("leftmouse"))
		{
			holdingFor += (float)delta;

			if (Input.IsActionJustPressed("leftmouse"))
			{
				prevmousepos = GetGlobalMousePosition();

				// Create new vertex
				if (dist > 40)
				{
					Vertex vert = GD.Load<PackedScene>("res://Assets/Prefabs/Vertex.tscn").Instantiate<Vertex>();
					vert.Position = mousepos;
					vertices.AddChild(vert);
					verticesLst.Add(vert);

					if (selected != null)
						CreateEdge(vert, selected);

					MoveSelection(vert);
				}

				if (secondSelected != null)
				{
					CreateEdge(selected, secondSelected);
				}
			}
			else if (selected != null)
			{
				isHolding = true;

				Vector2 mouseDelta;

				if (prevmousepos != null)
					mouseDelta = mousepos - prevmousepos;
				else
					mouseDelta = new Vector2();

				prevmousepos = mousepos;

				selected.Position += mouseDelta;
			}
		}

		if (Input.IsActionJustPressed("rightmouse") && selected != null)
		{
			vertices.RemoveChild(selected);
			verticesLst.Remove(selected);
			selected = null;
		}
	}

	public void MoveSelection(Vertex newVert)
	{
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
