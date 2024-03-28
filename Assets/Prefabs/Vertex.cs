using Godot;
using System;

public partial class Vertex : Node2D
{
	public char letter = ' ';
	public override void _Ready() { }

	public override void _Process(double delta) { }

	public void Select()
	{
		GetNode<Sprite2D>("Select").Visible = true;
		GetNode<Sprite2D>("Select").SelfModulate = new Color(0.1f, 0.09f, 0.1f);
	}

	public void SelectAsSecond()
	{
		GetNode<Sprite2D>("Select").Visible = true;
		GetNode<Sprite2D>("Select").SelfModulate = new Color(0.4f, 0.2f, 0.7f);
	}

	public void Deselect() => GetNode<Sprite2D>("Select").Visible = false;

	public void SetLetter(char letter)
	{
		this.letter = letter;
		GetNode<Label>("letter").Text = letter.ToString();
	}

	public void ClearLetter()
	{
		letter = ' ';
		GetNode<Label>("letter").Text = "";
	}

}
