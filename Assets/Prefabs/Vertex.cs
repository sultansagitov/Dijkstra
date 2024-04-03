using Godot;
using System;

public partial class Vertex : Node2D
{
    private string mark = "\0";

    public string Mark
    {
        get => mark;
        set
        {
            GetNode<Label>("mark").Text = value;
            mark = value;
        }
    }

    public override void _Ready() { }
	public override void _Process(double delta) { }

	public void Select()
	{
		GetNode<Sprite2D>("Select").Visible = true;
	}

	public void Deselect() => GetNode<Sprite2D>("Select").Visible = false;


	public void ClearMark()
	{
		Mark = "";
	}

}
