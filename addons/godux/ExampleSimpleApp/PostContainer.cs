using Godot;
using System;
using System.Collections.Immutable;
using Godux;

public partial class PostContainer : VBoxContainer
{
    public ImmutableArray<PostItem> Posts { get; set; }

    [Export]
    public PackedScene PostScene;

    public override void _Ready()
    {
        AppState.Store?.AddSubscriber("Posts.Present.Posts", (prop, state, oldValue, newValue) =>
        {
            Posts = (ImmutableArray<PostItem>)newValue;
            RenderPosts();
        });
    }

    public void RenderPosts()
    {
        foreach (var node in GetChildren())
        {
            RemoveChild(node);
            node.QueueFree();
        }

        foreach (var post in Posts)
        {
            var postScene = PostScene.Instantiate<Post>();
            postScene.MessageText = post.PostText;
            postScene.Poster = post.Poster;
            AddChild(postScene);
        }
    }
}
