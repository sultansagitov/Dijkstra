using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Main : Node2D
{
	[Export] public Node2D vertices;
	[Export] public Node2D edges;
	private List<Vertex> verticesList = new();
	private List<Edge> edgesList = new();

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
		bool dontCreateExtra = false;

		foreach (var vert in verticesList)
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

		edgesList = edgesList.Where(edge =>
		{
			if (edge.removeInNextFrame)
				edges.RemoveChild(edge);
			return !edge.removeInNextFrame;
		}).ToList();

		List<Edge> toAddEdges = new();

		foreach (var edge in edgesList)
		{
			if (edge.createVertexInNextFrame)
			{
				edge.createVertexInNextFrame = false;
				dontCreateExtra = true; // i love kostyli

				Vertex vert = GD.Load<PackedScene>("res://Assets/Prefabs/Vertex.tscn").Instantiate<Vertex>();
				vert.Position = mousepos;
				vertices.AddChild(vert);
				verticesList.Add(vert);

				toAddEdges.Add(CreateEdge(edge.b, vert, false));
				edge.b = vert;

				MoveSelection(vert);
			}
		}

		edgesList.AddRange(toAddEdges);

		foreach (var edge in edgesList)
		{
			Vector2 diff = edge.a.Position - edge.b.Position;
			float length = diff.Length();

			edge.Position = (edge.a.Position + edge.b.Position) / 2;
			edge.linecenter.Scale = new Vector2(length / 2, 1);
			edge.linecenter.RotationDegrees = Mathf.Atan(diff.Y / diff.X) * 180 / Mathf.Pi;
			edge.len.Text = $"{length:F0}";
		}

		if (Input.IsActionPressed("save_selected") || isHolding)
		{
			if (Input.IsActionPressed("connection"))
			{
				double dist2 = 50f;

				foreach (var vert in verticesList)
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

				secondSelected?.SelectAsSecond();
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
				if (!dontCreateExtra && dist > 40)
				{
					Vertex vert = GD.Load<PackedScene>("res://Assets/Prefabs/Vertex.tscn").Instantiate<Vertex>();
					vert.Position = mousepos;
					vertices.AddChild(vert);
					verticesList.Add(vert);

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

				Vector2 mouseDelta = mousepos - prevmousepos;

				prevmousepos = mousepos;

				selected.Position += mouseDelta;
			}
		}

		if (Input.IsActionJustPressed("rightmouse") && selected != null)
		{
			vertices.RemoveChild(selected);
			verticesList.Remove(selected);

			edgesList = edgesList.Where(edge =>
			{
				if (edge.HasVertex(selected))
					edges.RemoveChild(edge);
				return !edge.HasVertex(selected);
			}).ToList();
			selected = null;
		}

		string t = "";
		t += $"m: {mousepos}";
		if (selected != null)
			t += $"\ns: {selected.Position}";
		if (secondSelected != null)
			t += $"\nb: {secondSelected.Position}";

		GetNode<Label>("Label").Text = t;
	}

	public void MoveSelection(Vertex newVert)
	{
		selected?.Deselect();
		newVert.Select();
		selected = newVert;
	}

	public Edge CreateEdge(Vertex a, Vertex b) => CreateEdge(a, b, true);
	public Edge CreateEdge(Vertex a, Vertex b, bool changeList)
	{
		if (verticesList.Contains(a) && verticesList.Contains(b) && a != b)
		{
			Vector2 delta = a.Position - b.Position;
			float length = delta.Length();

			if (!edgesList.Any(edge => edge.HasVertex(a, b)))
			{
				Edge edge = GD.Load<PackedScene>("res://Assets/Prefabs/Edge.tscn").Instantiate<Edge>();
				edge.Position = (a.Position + b.Position) / 2;
				edge.linecenter.Scale = new(length / 2, 1);
				edge.linecenter.RotationDegrees = Mathf.RadToDeg(Mathf.Atan(delta.Y / delta.X));
				edge.len.Text = length.ToString();
				edge.a = a;
				edge.b = b;

				edges.AddChild(edge);
				if (changeList)
					edgesList.Add(edge);

				return edge;
			}
		}

		return null;
	}
}