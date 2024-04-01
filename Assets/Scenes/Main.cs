using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Main : Node2D
{
	[Export] public Node2D vertices;
	[Export] public Node2D edges;
	[Export] public Node2D menu;
	[Export] public Node2D helptext;

	private List<Vertex> verticesList = new();
	private List<Edge> edgesList = new();

	private Vertex nearest = null;
	private Vertex startConnect = null;
	private Vertex holded = null;
	private bool isHolding = false;
	private Vertex tempMouseVertex = null;
	private bool possiblyConnect = false;
	private char? pressedKey = null;

	private Vector2 _prevmousepos;
	private Vector2 _menu_start;
	private Vector2 _menu_end;
	private Vector2 _help_start;
	private Vector2 _help_end;


	public override void _Ready()
	{
		// vertices = GetNode<Node2D>("Vertices");
		// edges = GetNode<Node2D>("Edges");
		_prevmousepos = GetGlobalMousePosition();

		_menu_start = menu.GetNode<ColorRect>("bg").Position;
		_menu_end = _menu_start + menu.GetNode<ColorRect>("bg").Size;

		_help_start = helptext.GetNode<ColorRect>("bg").Position;
		_help_end = _help_start + helptext.GetNode<ColorRect>("bg").Size;
	}


	public override void _Process(double delta)
	{
		var winsize = DisplayServer.WindowGetSize();
		var mousepos = GetGlobalMousePosition();

		bool dontCreateExtra = false;
		bool allowEditGraph = true;

		// Dont allow editing when mouse is under menu
		if (mousepos.X < _menu_start.X || mousepos.Y < _menu_start.Y || mousepos.X > _menu_end.X || mousepos.Y > _menu_end.Y)
		{
			allowEditGraph = true;
		}
		else
		{
			allowEditGraph = false;
		}

		// Find nearest vertex to mouse
		if (!isHolding)
		{
			Vertex n = null;

			double dist = 50d;

			foreach (var vert in verticesList)
			{
				if (vert == tempMouseVertex) continue;

				double d = (mousepos - vert.Position).Length();

				if (d < dist)
				{
					dist = d;
					n = vert;
				}
			}

			if (n == null)
			{
				nearest?.Deselect();
				nearest = null;
			}
			else if (nearest != n)
			{
				nearest?.Deselect();
				n.Select();
				nearest = n;
			}
		}

		// Clear edges that removed by right click
		edgesList = edgesList.Where(edge =>
		{
			if (edge.removeInNextFrame)
				edges.RemoveChild(edge);
			return !edge.removeInNextFrame;
		}).ToList();

		// Create vertex on center of edge :  A-----B  -->  A--C--B  : C = new vertex
		if (allowEditGraph)
		{
			List<Edge> toAddEdges = new();

			foreach (var edge in edgesList)
			{
				if (edge.createVertexInNextFrame)
				{
					edge.createVertexInNextFrame = false;
					dontCreateExtra = true; // i love kostyli

					Vertex vert = CreateVertex(mousepos);

					toAddEdges.Add(CreateEdge(edge.b, vert, false));
					edge.b = vert;

					nearest?.Deselect();
					vert.Select();
					nearest = vert;
				}
			}

			edgesList.AddRange(toAddEdges);
		}

		// Deselect when mouse too far
		if (allowEditGraph && !isHolding && nearest != null && (mousepos - nearest.Position).Length() > 50.0)
		{
			nearest.Deselect();
			nearest = null;
		}


		//==== Mouse buttons ====//

		// Left button
		if (allowEditGraph && Input.IsActionPressed("leftmouse"))
		{
			if (Input.IsActionJustPressed("leftmouse"))
			{
				_prevmousepos = GetGlobalMousePosition();

				if (!dontCreateExtra && nearest == null)
					holded = CreateVertex(mousepos);
				else
					holded = nearest;

				isHolding = true;
			}

			if (isHolding)
			{
				holded.Position += mousepos - _prevmousepos;
				_prevmousepos = mousepos;
			}

			UpdateEdges();
		}

		if (Input.IsActionJustReleased("leftmouse") || !allowEditGraph)
			isHolding = false;


		// Middle button
		if (allowEditGraph && Input.IsActionPressed("middlemouse"))
		{
			if (Input.IsActionJustPressed("middlemouse"))
			{
				startConnect = nearest;
				RemoveVertex(tempMouseVertex);
				tempMouseVertex = CreateVertex(mousepos, startConnect);
				tempMouseVertex.Visible = false;
			}

			if (tempMouseVertex != null)
			{
				if (nearest != null)
				{
					tempMouseVertex.Position = nearest.Position;
					possiblyConnect = true;
				}
				else
				{
					tempMouseVertex.Position = mousepos;
					possiblyConnect = false;
				}
			}

			UpdateEdges();
		}

		if (Input.IsActionJustReleased("middlemouse") || !allowEditGraph)
		{
			if (possiblyConnect)
			{
				RemoveVertex(tempMouseVertex);
				if (startConnect != null)
					CreateEdge(startConnect, nearest);
			}
			else if (tempMouseVertex != null)
			{
				tempMouseVertex.Visible = true;
				tempMouseVertex = null;
			}

			startConnect = null;
		}

		// Right button
		if (allowEditGraph && Input.IsActionJustPressed("rightmouse") && nearest != null)
		{
			RemoveVertex(nearest);
			UpdateEdges();
		}


		//==== Debuging ====//
		string t = "";
		t += $"m: {mousepos} \n";
		if (nearest != null)
			t += $"s: {nearest.Position} \n";
		if (startConnect != null)
			t += $"n: {startConnect.Position} \n";

		if (dontCreateExtra) t += "dontCreateExtra \n";
		if (allowEditGraph) t += "allowEditGraph \n";
		if (possiblyConnect) t += "possiblyConnect \n";
		if (isHolding) t += "isHolding \n";
		if (pressedKey != null) t += pressedKey + " \n";

		GetNode<Label>("Label").Text = t;

		helptext.Position = new Vector2(winsize.X, winsize.Y / 2);
	}

	public override void _Input(InputEvent ev)
	{
		string lets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		for (int i = 0; i < 26; i++)
			if (Input.IsKeyPressed((Key)((int)Key.A + i)))
			{
				pressedKey = lets[i];
				return;
			}

		pressedKey = null;
	}


	//==== Most used medhods ====//
	public void UpdateEdges()
	{
		foreach (var edge in edgesList)
		{
			Vector2 delta = edge.a.Position - edge.b.Position;
			float length = delta.Length();

			edge.Position = (edge.a.Position + edge.b.Position) / 2;
			edge.linecenter.Scale = new Vector2(length / 2, 1);
			edge.linecenter.RotationDegrees = Mathf.RadToDeg(Mathf.Atan(delta.Y / delta.X));
			edge.Size = (int)length;
		}
	}
	public Vertex CreateVertex(Vector2 pos)
	{
		Vertex vert = GD.Load<PackedScene>("res://Assets/Prefabs/Vertex.tscn").Instantiate<Vertex>();
		vert.Position = pos;
		vertices.AddChild(vert);
		verticesList.Add(vert);
		return vert;
	}
	public Vertex CreateVertex(Vector2 pos, Vertex other)
	{
		Vertex vert = CreateVertex(pos);
		CreateEdge(vert, other);
		return vert;
	}

	public void RemoveVertex(Vertex vert)
	{
		edgesList = edgesList.Where(edge =>
		{
			if (edge.HasVertex(vert))
				edges.RemoveChild(edge);
			return !edge.HasVertex(vert);
		}).ToList();

		if (vert != null) vertices.RemoveChild(vert);
		if (vert != null) verticesList.Remove(vert);

		if (nearest == vert) nearest = null;
		if (tempMouseVertex == vert) tempMouseVertex = null;
	}

	public Edge CreateEdge(Vertex a, Vertex b) => CreateEdge(a, b, true);
	public Edge CreateEdge(Vertex a, Vertex b, bool changeList)
	{
		if (a == b)
		{
			GD.PushWarning("WARNING: Unable to create edge between same vertices");
			return null;
		}

		if (verticesList.Contains(a) && verticesList.Contains(b))
		{
			Vector2 delta = a.Position - b.Position;
			float length = delta.Length();

			if (!edgesList.Any(edge => edge.HasVertex(a, b)))
			{
				Edge edge = GD.Load<PackedScene>("res://Assets/Prefabs/Edge.tscn").Instantiate<Edge>();
				edge.Position = (a.Position + b.Position) / 2;
				edge.linecenter.Scale = new(length / 2, 1);
				edge.linecenter.RotationDegrees = Mathf.RadToDeg(Mathf.Atan(delta.Y / delta.X));
				edge.Size = (int)length;
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

	public void RemoveEdge(Edge edge)
	{
		//later (maybe)
	}


	//==== Signals ====//
	public void Helpbtn_Pressed()
	{
		helptext.Visible ^= true;
	}

	public void Quitbtn_Pressed()
	{
		GetTree().Quit();
	}
}