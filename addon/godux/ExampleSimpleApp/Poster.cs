using Godot;
using System;
using AppState = PostingAppStateStore;

public partial class Poster : Control
{
    public void PressPost()
    {
        string enteredText = GetNode<TextEdit>("%TextEditor").Text;
		var postItem = new PostItem { PostText = enteredText, Poster = "Chris" };
        AppState.Instance.Dispatch(new AppState.MakePost(postItem));
        GetNode<TextEdit>("%TextEditor").Text = "";
    }
}
