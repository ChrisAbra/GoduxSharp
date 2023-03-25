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
            numPosts = value;
            GetNode<Label>("%NumPosts").Text = $"Number of Posts: {numPosts}";
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
