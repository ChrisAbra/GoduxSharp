using Godot;
using System;
using AppState = PostingAppStateStore;

public partial class Poster : Control
{
    public enum PosterUpdateType{
        POST,
        POSTER
    }

    [Export]
    public PosterUpdateType UpdateType;
    public void PressPost()
    {
        string enteredText = GetNode<TextEdit>("%TextEditor").Text;
        if(UpdateType == PosterUpdateType.POST){
            var postItem = new PostItem { PostText = enteredText, Poster = AppState.Instance.CurrentState.PosterName };
            AppState.Instance.Dispatch(new AppState.MakePost(postItem));
        }
        if(UpdateType == PosterUpdateType.POSTER){
            AppState.Instance.Dispatch(new AppState.SetPosterName(enteredText));
        }
        GetNode<TextEdit>("%TextEditor").Text = "";
    }
}
