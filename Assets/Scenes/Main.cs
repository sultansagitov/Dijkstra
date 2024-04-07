using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Main : Node2D
{
    [Export]
    public Node2D vertices;

    [Export]
    public Node2D edges;

    [Export]
    public Node2D menu;

    [Export]
    public Node2D helptext;

    [Export]
    public Node2D makenew;

    [Export]
    public Node2D startenderr_node;

    [Export]
    public Label shortpath;

    private readonly List<Vertex> verticesList = new();
    private List<Edge> edgesList = new();

    private readonly Settings settings = new();
    private Vertex pathstart = null;
    private Vertex pathend = null;

    private Vertex nearest = null;
    private Vertex startConnect = null;
    private Vertex holded = null;
    private bool isHolding = false;
    private Vertex tempMouseVertex = null;
    private bool possiblyConnect = false;
    private bool startenderr = false;

    public bool _remove_extra_edges;
    private Vector2 _prevmousepos;
    private Vector2 _menu_start;
    private Vector2 _menu_end;
    private Vector2 _help_start;
    private Vector2 _help_end;

    public override void _Ready()
    {
        shortpath.Text = "";
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

        _help_start = helptext.GetNode<ColorRect>("bg").Position;
        _help_end = _help_start + helptext.GetNode<ColorRect>("bg").Size;

        bool dontCreateExtra = false;
        bool allowEditGraph = true;

        // Dont allow editing when mouse is under menu
        if (
            mousepos.X < _menu_start.X
            || mousepos.Y < _menu_start.Y
            || mousepos.X > _menu_end.X
            || mousepos.Y > _menu_end.Y
        )
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

            double dist = 23d;

            foreach (var vert in verticesList)
            {
                if (vert == tempMouseVertex)
                    continue;

                double d = (mousepos - vert.Position).Length();

                if (d < dist)
                {
                    dist = d;
                    n = vert;
                }
            }

            if (n == null)
            {
                nearest = null;
            }
            else if (nearest != n)
            {
                nearest = n;
            }
        }

        // Clear edges that removed by right click
        if (_remove_extra_edges)
        {
            _remove_extra_edges = false;
            FindingReset();
            edgesList = edgesList
                .Where(edge =>
                {
                    if (edge.removeInNextFrame)
                        edges.RemoveChild(edge);
                    return !edge.removeInNextFrame;
                })
                .ToList();
        }
        // Create vertex on center of edge :  A-----B  -->  A--C--B  : C = new vertex
        if (allowEditGraph)
        {
            List<Edge> toAddEdges = new();

            foreach (var edge in edgesList)
            {
                if (edge.createVertexInNextFrame)
                {
                    edge.createVertexInNextFrame = false;
                    dontCreateExtra = true;

                    Vertex vert = CreateVertex(mousepos);

                    toAddEdges.Add(CreateEdge(edge.b, vert, false));
                    edge.b = vert;
                    nearest = vert;
                }
            }

            edgesList.AddRange(toAddEdges);
        }

        if (Input.IsActionJustPressed("help"))
        {
            Helpbtn_Pressed();
        }

        if (Input.IsActionJustPressed("startalgorithm"))
        {
            Findpathbtn_Pressed();
        }

        if (Input.IsActionJustPressed("removeall"))
        {
            RemoveAllbtn_Pressed();
        }

        //==== Deselect when mouse too far ====//
        if (
            allowEditGraph
            && !isHolding
            && nearest != null
            && (mousepos - nearest.Position).Length() > 23.0
        )
        {
            nearest = null;
        }

        UpdateSelection();

        //==== Set mode of vertex (start / end) ====//
        if (allowEditGraph && nearest != null)
        {
            if (Input.IsActionJustPressed("makestart"))
            {
                if (pathstart != null)
                    pathstart.mode = Vertex.Mode.Between;
                pathstart.UpdateColor();

                pathstart = nearest;
                if (pathstart == pathend)
                    pathend = null;
                pathstart.mode = Vertex.Mode.Start;
                pathstart.UpdateColor();
            }

            if (Input.IsActionJustPressed("makeend"))
            {
                if (pathend != null)
                    pathend.mode = Vertex.Mode.Between;
                pathend.UpdateColor();

                pathend = nearest;
                if (pathstart == pathend)
                    pathstart = null;
                pathend.mode = Vertex.Mode.End;
                pathend.UpdateColor();
            }
        }

        //==== Mouse buttons ====//
        // Left button
        if (allowEditGraph && Input.IsActionPressed("leftmouse"))
        {
            if (Input.IsActionJustPressed("leftmouse"))
            {
                _prevmousepos = GetGlobalMousePosition();

                if (
                    mousepos.X > _help_start.X
                    && mousepos.Y > _help_start.Y
                    && mousepos.X < _help_end.X
                    && mousepos.Y > _help_end.Y
                )
                {
                    helptext.Visible = false;
                }

                if (!dontCreateExtra && nearest == null)
                    holded = CreateVertex(mousepos);
                else
                    holded = nearest;

                isHolding = true;
            }

            if (isHolding)
            {
                if (holded == null)
                    GD.Print("qwerty");
                holded.Position += mousepos - _prevmousepos;
                _prevmousepos = mousepos;
            }

            UpdateEdges();
        }

        if (Input.IsActionJustReleased("leftmouse") || !allowEditGraph)
        {
            isHolding = false;
            holded = null;
        }

        // Middle button
        if (allowEditGraph && Input.IsActionPressed("middlemouse"))
        {
            if (Input.IsActionJustPressed("middlemouse"))
            {
                if (nearest != null)
                {
                    startConnect = nearest;
                    RemoveVertex(tempMouseVertex);
                    tempMouseVertex = CreateVertex(mousepos, startConnect);
                    tempMouseVertex.Visible = false;
                }
                else
                {
                    holded = CreateVertex(mousepos);
                }
            }

            if (tempMouseVertex != null)
            {
                if (startConnect != nearest && nearest != null)
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
                if (startConnect != null && nearest != null)
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

        //==== Help texts ====//
        if (makenew.Visible = verticesList.Count == 0)
            makenew.Position = winsize / 2;

        helptext.Position = new Vector2(winsize.X, 0);
        if (startenderr_node.Visible = startenderr)
            startenderr_node.Position = new Vector2(winsize.X / 2, winsize.Y);

        //==== Debuging ====//
        string t = "";
        t += $"m: {mousepos} \n";
        if (nearest != null)
        {
            t += $"n: {nearest.Mark} {nearest} {nearest.Position} \n";

            if (nearest.path.Count != 0)
            {
                t += $"path \n";
                foreach (var v in nearest.path)
                    t += $"{v.Mark} {v} \n";
            }
        }

        if (startConnect != null)
            t += $"s: {startConnect.Mark} {startConnect} {startConnect.Position} \n";
        if (dontCreateExtra)
            t += "dontCreateExtra \n";
        if (allowEditGraph)
            t += "allowEditGraph \n";
        if (possiblyConnect)
            t += "possiblyConnect \n";
        if (isHolding)
            t += "isHolding \n";

        GetNode<Label>("Label").Text = t;
    }

    //==== Most used medhods ====//
    public static string FindMissingLetter(List<string> marks)
    {
        int expected = 'A';
        string mark = "A";

        while (marks.Contains(mark))
        {
            if (expected > 'z')
                mark = (expected - 122).ToString();
            else
                mark = ((char)expected).ToString();

            if (expected == 'Z')
                expected += 6;

            expected++;
        }

        return mark;
    }

    public void FindingReset()
    {
        shortpath.Text = "";

        if (startenderr = pathend == null || pathend == null)
            return;

        foreach (var vert in verticesList)
        {
            vert.path.Clear();
            vert.ShortestSum = int.MaxValue;
        }

        foreach (var edge in edgesList)
        {
            edge.HighlightReset();
        }
    }

    public void UpdateSelection()
    {
        foreach (var vert in verticesList)
        {
            if (vert == nearest || vert == startConnect)
                vert.Select();
            else
                vert.Deselect();
        }
    }

    public void UpdateEdges()
    {
        FindingReset();

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

    public Vertex CreateVertex(Vector2 pos, Vertex other)
    {
        Vertex vert = CreateVertex(pos);
        CreateEdge(vert, other);
        return vert;
    }

    public Vertex CreateVertex(Vector2 pos)
    {
        Vertex vert = GD.Load<PackedScene>("res://Assets/Prefabs/Vertex.tscn")
            .Instantiate<Vertex>();
        vert.settings = settings;

        if (pathstart == null)
        {
            pathstart = vert;
            vert.mode = Vertex.Mode.Start;
        }
        else if (pathend == null)
        {
            pathend = vert;
            vert.mode = Vertex.Mode.End;
        }
        else
        {
            vert.mode = Vertex.Mode.Between;
        }

        vert.UpdateColor();

        vert.Position = pos;
        vertices.AddChild(vert);
        verticesList.Add(vert);

        List<string> ch = new();
        foreach (var v in verticesList)
            ch.Add(v.Mark);

        vert.Mark = FindMissingLetter(ch).ToString();

        return vert;
    }

    public void RemoveVertex(Vertex vert)
    {
        edgesList = edgesList
            .Where(edge =>
            {
                if (edge.HasVertex(vert))
                    edges.RemoveChild(edge);
                return !edge.HasVertex(vert);
            })
            .ToList();

        if (vert != null)
        {
            vertices.RemoveChild(vert);
            verticesList.Remove(vert);
        }

        if (nearest == vert)
            nearest = null;
        if (tempMouseVertex == vert)
            tempMouseVertex = null;
        if (startConnect == vert)
            startConnect = null;
        if (holded == vert)
            holded = null;
        if (pathstart == vert)
            pathstart = null;
        if (pathend == vert)
            pathend = null;
    }

    public Edge CreateEdge(Vertex a, Vertex b) => CreateEdge(a, b, true);

    public Edge CreateEdge(Vertex a, Vertex b, bool changeList)
    {
        if (a == b)
        {
            GD.PushWarning("Алеееее, зачем? это лишнее");
            return null;
        }

        if (verticesList.Contains(a) && verticesList.Contains(b))
        {
            Vector2 delta = a.Position - b.Position;
            float length = delta.Length();

            if (!edgesList.Any(edge => edge.HasVertex(a, b)))
            {
                Edge edge = GD.Load<PackedScene>("res://Assets/Prefabs/Edge.tscn")
                    .Instantiate<Edge>();
                edge.main = this;
                edge.settings = settings;
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

    // later (maybe)
    public void RemoveEdge(Edge edge) { }

    //==== Signals ====//
    public void Helpbtn_Pressed()
    {
        helptext.Visible ^= true;
    }

    public void Findpathbtn_Pressed()
    {
        FindingReset();

        if (startenderr)
            return;

        pathstart.path.Add(pathstart);
        pathstart.ShortestSum = 0;

        List<Vertex> Q = new() { pathstart };
        List<Vertex> Q_tmp;

        do
        {
            Q_tmp = new();

            List<Vertex> reversed = new();
            reversed.AddRange(Q);
            reversed.Reverse();

            foreach (Vertex vert in reversed)
            {
                if (vert == pathend)
                    continue;

                List<Vertex> neighbors = new();
                List<Edge> neighborEdges = edgesList.Where(edge => edge.HasVertex(vert)).ToList();

                foreach (Edge edge in neighborEdges)
                {
                    Vertex tmp = edge.a != vert ? edge.a : edge.b;
                    if (!Q.Contains(tmp) && !neighbors.Contains(tmp))
                        neighbors.Add(tmp);
                }

                Q_tmp.AddRange(neighbors);
            }

            Q.AddRange(Q_tmp);
        } while (Q_tmp.Count != 0);

        List<Vertex> C = new();

        foreach (Vertex vert in Q)
        {
            if (C.Contains(vert))
                continue;

            List<Vertex> neighbors = new();
            List<Edge> neighborEdges = edgesList.Where(edge => edge.HasVertex(vert)).ToList();

            foreach (Edge edge in neighborEdges)
            {
                Vertex neighbor = edge.a != vert ? edge.a : edge.b;
                if (!C.Contains(neighbor))
                    neighbors.Add(neighbor);

                if (vert.ShortestSum + edge.Size < neighbor.ShortestSum)
                {
                    neighbor.ShortestSum = vert.ShortestSum + edge.Size;
                    neighbor.path.Clear();
                    neighbor.path.AddRange(vert.path);
                    neighbor.path.Add(neighbor);
                }
            }

            C.Add(vert);
        }

        List<Vertex> path = pathend.path;

        string t = pathstart.Mark;
        foreach (var item in path.GetRange(1, path.Count - 1))
            t += " - " + item.Mark;

        shortpath.Text = t;

        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];

            var f = edgesList.Where(edge => edge.HasVertex(a, b)).ToList();
            if (f.Count == 1)
                f[0].Highlight();
            else if (f.Count == 0)
                GD.PushWarning("У самурая нет пути, только цель");
            else
                GD.PushWarning("У самурая нет цели (есть), только путь (пути)");
        }
    }

    public void Quitbtn_Pressed()
    {
        GetTree().Quit();
    }

    public void ShowSmallcheck_Toggled(bool toggled)
    {
        settings.showSmallEdgeLength = toggled;
        UpdateEdges();
    }

    public void RemoveAllbtn_Pressed()
    {
        foreach (var vert in verticesList)
            vertices.RemoveChild(vert);
        verticesList.Clear();

        foreach (var edge in edgesList)
            edges.RemoveChild(edge);
        edgesList.Clear();
    }
}
