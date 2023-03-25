using Godux;
using System.Collections.Immutable;

public record PostingAppState : State {
    public UndoableState<PostsState> Posts {get;init;} = new PostsState();

    public int NumberOfPosts => Posts.Present.Posts.Length;

}