using System;
using System.Collections.Generic;
using Godot;

public partial class Vertex : Node2D
{
    public enum Mode
    {
        Start = 0,
        End = 1,
        Between = 2
    };

    private string mark = "\0";
    public string Mark
    {
        get => mark;
        set
        {
            mark_node.Text = value;
            mark = value;
        }
    }

    private int shortestsum = int.MaxValue;
    public int ShortestSum
    {
        get => shortestsum;
        set
        {
            if (settings.showResults)
            {
                if (value != int.MaxValue)
                    shortestsum_node.Text = value.ToString();
                else
                    shortestsum_node.Text = "";
            }
            else
            {
                shortestsum_node.Visible = false;
            }
            shortestsum = value;
        }
    }

    public Settings settings;
    public Mode mode = Mode.Between;
    public readonly List<Vertex> path = new();

    [Export]
    Sprite2D select;

    [Export]
    Sprite2D circle;

    [Export]
    Label mark_node;

    [Export]
    Label shortestsum_node;

    public override void _Ready() { }

    public override void _Process(double delta) { }

    public void Select()
    {
        select.Visible = true;
    }

    public void Deselect() => select.Visible = false;

    public void ClearMark()
    {
        Mark = "";
    }

    public void UpdateColor()
    {
        switch (mode)
        {
            case Mode.Start:
                select.SelfModulate = circle.SelfModulate = Color.FromHtml("#ff0000");
                break;
            case Mode.End:
                select.SelfModulate = circle.SelfModulate = Color.FromHtml("#0000ff");
                break;
            case Mode.Between:
                select.SelfModulate = circle.SelfModulate = Color.FromHtml("#1a181b");
                break;
        }
    }

    public void UpdateBySettings()
    {
        if (shortestsum_node.Visible = settings.showResults)
        {
            if (ShortestSum != int.MaxValue)
                shortestsum_node.Text = ShortestSum.ToString();
            else
                shortestsum_node.Text = "";
        }
    }
}
