
using Godux;
using System.Collections.Immutable;

public record PostItem {
    public string Poster {get;init;}
    public string PostText {get;init;}
}
public record PostsState : State {
    public ImmutableArray<PostItem> Posts {get;init;} = ImmutableArray.Create<PostItem>();
}