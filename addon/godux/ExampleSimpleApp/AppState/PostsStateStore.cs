using Godux;

public partial class PostingAppStateStore : StateStore<PostingAppState>
{
    public record PostAction : Godux.Action;
    public record MakePost(PostItem postItem) : PostAction;

    protected PostsState Reduce_PostAction(PostsState postsState, PostAction postAction)
    {
        return postAction switch
        {
            MakePost makePost => postsState with
            {
                Posts = postsState.Posts.Add(makePost.postItem),
            },
            _ => postsState
        };
    }
}