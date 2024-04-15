using System;
using Godot;

public partial class Edge : Node2D
{
    [Export]
    public Node2D linecenter;

    [Export]
    public Label len;

    [Export]
    public ColorRect line;

    [Export]
    public ColorRect len_bg;

    public Main main;
    public Settings settings;
    public bool removeInNextFrame = false;
    public bool createVertexInNextFrame = false;

    public Vertex a;
    public Vertex b;
    private int _size;
    public int Size
    {
        get => _size;
        set
        {
            _size = value;

            if (
                len_bg.Visible = len.Visible = 
                    settings.showSmallEdgeLength || (value > 140)
            )
                len.Text = value.ToString();
        }
    }

    public override void _Ready() { }

    public override void _Process(double delta) { }

    public bool HasVertex(Vertex x) => a == x || b == x;

    public bool HasVertex(Vertex a, Vertex b) =>
        this.a == a && this.b == b || this.b == a && this.a == b;

    public void Highlight()
    {
        linecenter.GetNode<ColorRect>("line").Color = Color.FromHtml("#7f19f7");
    }

    public void HighlightReset()
    {
        linecenter.GetNode<ColorRect>("line").Color = Color.FromHtml("#0d0f19");
    }

    public void Gui_Input(InputEvent ev)
    {
        if (ev is InputEventMouseButton @e)
        {
            createVertexInNextFrame |= @e.ButtonIndex == MouseButton.Left && @e.Pressed;
            main._remove_extra_edges = removeInNextFrame |=
                @e.ButtonIndex == MouseButton.Right && @e.Pressed;
        }
    }
}
