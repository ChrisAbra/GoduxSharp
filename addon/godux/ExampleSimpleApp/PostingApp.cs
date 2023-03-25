using Godot;
using System;
using Godux;
using AppState = PostingAppStateStore;


public partial class PostingApp : Control
{
    [WireToState("NumberOfPosts")]
    public int NumberOfPosts
    {
        get => numPosts;
        set
        {
			GD.Print(value);
            numPosts = value;
            GetNode<Label>("%NumPosts").Text = $"Number of Posts: {_numPosts}";
        }
    }
	private int numPosts;


    public override void _Ready()
    {
        AppState.Instance.ConnectWiredAttributes(this);
    }
    private int _numPosts;
    public void Undo()
    {
        AppState.Instance.Dispatch(new AppState.UndoPost());
    }
    public void Redo()
    {
        AppState.Instance.Dispatch(new AppState.RedoPost());
    }
}
