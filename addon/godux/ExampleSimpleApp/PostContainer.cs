using Godot;
using System;
using System.Collections.Immutable;
using AppState = PostingAppStateStore;
using Godux;

public partial class PostContainer : VBoxContainer
{
	public ImmutableArray<PostItem> Posts {get => _posts;
		set {
			_posts = value;
			RenderPosts();
		}
	}

	[WireToState]
	public ImmutableArray<PostItem> Posts_Present_Post {get;set;}
	[Export]
	public PackedScene PostScene;
	private ImmutableArray<PostItem> _posts;

	public override void _Ready(){
		AppState.Instance.AddSubscriber("Posts.Present.Posts", (prop, state, oldValue, newValue) => Posts = (ImmutableArray<PostItem>)newValue);
		//AppState.Instance.ConnectWiredAttributes(this);
	}

	public void RenderPosts(){
		foreach(var node in GetChildren()){
			RemoveChild(node);
			node.QueueFree();
		}

		foreach(var post in Posts){
			var postScene = PostScene.Instantiate<Post>();
			postScene.MessageText = post.PostText;
			postScene.Poster = post.Poster;
			AddChild(postScene);
		}
	}
}
