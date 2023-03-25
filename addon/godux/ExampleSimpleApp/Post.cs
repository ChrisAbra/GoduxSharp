using Godot;
using System;

public partial class Post : Control
{
	public string MessageText {get;set;}
	public string Poster {get;set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<RichTextLabel>("%PostText").Text = MessageText;
		GetNode<Label>("%Poster").Text = Poster;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
