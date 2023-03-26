using Godot;
using System;
using Godux;
using static PostingAppStateStore;

public partial class PostingApp : Control
{
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
        AppState.Store?.ConnectWiredAttributes(this);
    }
    private int _numPosts;
    public void Undo()
    {
        AppState.Store?.Dispatch(new UndoPost());
    }
    public void Redo()
    {
        AppState.Store?.Dispatch(new RedoPost());
    }
}
