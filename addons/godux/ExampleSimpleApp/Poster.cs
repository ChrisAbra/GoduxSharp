using Godot;
using System;
using Godux;
using static PostingAppStateStore;

public partial class Poster : Control
{
    public enum PosterUpdateType
    {
        POST,
        POSTER
    }

    [Export]
    public PosterUpdateType UpdateType;

    public string PosterName {get;set;}
    public void PressPost()
    {
        string enteredText = GetNode<TextEdit>("%TextEditor").Text;
        if (UpdateType == PosterUpdateType.POST)
        {
            var postItem = new PostItem { PostText = enteredText, Poster = PosterName};
            AppState.Store?.Dispatch(new MakePost(postItem));
        }
        if (UpdateType == PosterUpdateType.POSTER)
        {
            AppState.Store?.Dispatch(new SetPosterName(enteredText));
        }
        GetNode<TextEdit>("%TextEditor").Text = "";
    }
}
