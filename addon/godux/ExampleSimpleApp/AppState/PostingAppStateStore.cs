using Godux;
using Godot;

public record PostingAppState : State {
    public UndoableState<PostsState> Posts {get;init;} = new PostsState();
    public string PosterName {get;init;} = "Name not set";
    public int NumberOfPosts => Posts.Present.Posts.Length;

}

public partial class PostingAppStateStore : StateStore<PostingAppState>
{
    public override void InitalizeState()
    {
        CurrentState = new PostingAppState();
    }

    public record UndoPost : Godux.Action;
    public record RedoPost : Godux.Action;
    public record SetPosterName(string PosterName) : Godux.Action;

    protected override PostingAppState Reduce(PostingAppState state, Godux.Action action)
    {
        return action switch
        {
            SetPosterName setPosterName => state with {PosterName = setPosterName.PosterName},
            UndoPost _ => state with {Posts = state.Posts.Undo()},
            RedoPost _ => state with {Posts = state.Posts.Redo()},
            MakePost makePost => state with
            {
                Posts = state.Posts.Set(Reduce_PostAction(state.Posts.Present, makePost))
            },
            _ => state
        };
    }
}