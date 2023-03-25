using Godux;
using Godot;

public partial class PostingAppStateStore : StateStore<PostingAppState>
{
    public override void InitaliseState()
    {
        CurrentState = new PostingAppState();
    }

    public record UndoPost : Godux.Action;
    public record RedoPost : Godux.Action;

    protected override PostingAppState Reduce(PostingAppState state, Godux.Action action)
    {
        return action switch
        {
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