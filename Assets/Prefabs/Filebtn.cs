using System;
using Godot;

public partial class Filebtn : Button
{
	[Export] public string filename;
	public Main main;

    public void Deletebtn_Pressed() {
		using DirAccess dir = DirAccess.Open("user://savedgraphs/");
		dir.Remove("user://savedgraphs/" + filename);
		main.UpdateLoadfiles();
	}
}
