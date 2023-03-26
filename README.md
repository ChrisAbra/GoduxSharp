# GoduxSharp

This is an addon for the Godot engine designed to enable [Redux](https://redux.js.org/) like State Management.

Developed to aid in UI heavy apps and to minimise boilerplate and state/message routing as much as possible.

## Redux

Redux is a State Management library designed for web applications. It centralises state, ensures state is only modified via standard events, and relays changes to specified subscribers. [The docs](https://redux.js.org/tutorials/essentials/part-1-overview-concepts) do a good job of explaining the high-level concepts as does [Firebelly Games' video on the topic.](https://www.youtube.com/watch?v=FGtj7in9UUo)

Generally you need to understand the flow that: ***Actions*** containing data are *dispatched* to a ***Store*** which are handled by ***Reducers*** to update ***State*** and then ***Subscribers*** are notified of the changes.

**State**

All Top-level State should inherit from the State class. This contains methods for checking which fields are changed. States should be records and immutable. Properties should specify `{get;init;}` or Immutable types to ensure values can't be modified but can be overwitten by the Reducers

**Action**

As State is immutable, it cannot be modified directly. Actions are the mechanism similar to Events or Signals. An Action inherits from the StateStore.Action class and can be further inherited as needed to mirror the 'type' property of Redux. We use Pattern Matching on the type to route the Action to the relevant Reducer

**Reducers**

Reducers are pure functions (side-effect free) which modify the state when an Action is dispatched to the Store. They do this by making a copy of the state using the `with` [keyword](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expressionhttps:/) such as `state with {Name = action.NewName}`. The Reduce method of a class which inherits from StateStore returns the full state and can sub-areas of the state can be handled through the use of partial classes and derivation of Actions as needed.

**Store**

Stores (inheriting from StateStore) are the class which Actions are dispatched to, routed to Reducers and ultimately notifies subscribers. Each app should only have one store and it is suggested to Autoload. The issues with this can be mitigated through the use of [partial classes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methodshttps:/), this way different sub-states can be handled in their own files with one primary file which is autoloaded and handles top-level reduction.

## Getting Started

1. Import the addon (`src` folder is all thats necessary)
2. All code is namespaced under `using Godux;`
3. Create a class to handle State which inherits from Godux.State. This should contain primitive data, immutable types, other State classes or Undoable types. All properties should have `{get;init;}`
4. Create a Store class which inherits from `StateStore<T>` where T is the class of the State you created
5. Initilise the `CurrentState` in `InitializeState()` method for example: `CurrentState = new PostingAppState();`
6. Define some Actions in the Store
   ```csharp
    public record UndoPost : Godux.Action;
    public record RedoPost : Godux.Action;
    public record SetPosterName(string PosterName) : Godux.Action;
   ```
7. In the Reduce method define a reducer using a switch and pattern matching to return transformed state
   ```csharp
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
   ```
8. Add the class to an Autoload. The default path set in the StateStore is `/root/AppState` but this can be overriden in your custom Store class with the `Path` property.
9. In classes which want to interact with the state you can interact with the store by either using the static `<CustomStoreClassName>.Instance` or you can add `using <AnotherCustomStoreName> = <CustomStoreClassName>` if you wish for a shorter name.
10. You can dispatch events to the Store Instance `CustomStateStore.Instance.Dispatch(new AppState.UndoPost());`
11. You can Subscribe to state changes by adding a Subscriber to the instance by providing a path through the state
    ```csharp
    CustomStateStore.Instance.AddSubscriber("PlayerState.Health.Value", (prop, state, oldValue, newValue) => {
        Health = (int)newValue;
    });
    ```

## Wire Properties
Within a Node you can use the Attribute `WireToState`. 
When this is added to a property it automatically connects this property to the one on the State and automatically add a subscriber to set the property when the state changes. You can implement the `set` method to complete actions once this value is set.
It has 3 overloads:

1. Providing the name of the property to wire.
    ```csharp
    [WireToState(nameof(ExampleState.PosterName))]
    public string PosterName {get;set;}
    ```
    or
    ```csharp
    [WireToState("PosterName")]
    public string PosterName {get;set;}
    ```
2. Providing the name of the property AND providing a path and property of a child node to additionally write this to. The types of the state, property and child property have to match for this to work.
    ```csharp
    [WireToState(nameof(ExampleState.PosterName),"%NameLabel",nameof(Label.Text))]
    public string PosterName {get;set;}
    ```
    This method provides quick and easy mapping from the state to UI elements within a simplified controller and replaces code to retrieve, subscribe, and propogate state with one line. 
    (The efficiency of this attribute is one of the primary aims of this project)
3. Provided nested state 
    ```csharp
    [WireToState([nameof(ExampleState.PlayerState),nameof(PlayerState.Name)],"%NameLabel",nameof(Label.Text))]
    public string PlayerName {get;set;}
    ```
    or 
    ```csharp
    [WireToState("PlayerState.Name","%NameLabel",nameof(Label.Text))]
    public string PlayerName {get;set;}
    ```
Obviously it's better to use nameof where possible but there is no nice way for significantly nested state.


## Undoable
There is a builtin `Undoable<T>` class which is a type of State which has the functions for Undo and Redo built in. This works slightly differently from the standard `state with {Property = newValue}` reducer and instead should call the Set() method on any undoable properties - this returns a copy of the undoable state such as `state with {UndoableProperty.Set(newValue)}`

Similarly state can be rewound and redone by calling the Undo and Redo methods in a reducer such as `state with {UndoableProperty.Undo()}`

## Code Splitting / Partial classes

By defining your Store class as partial, it means you can divide SubState blocks into different files

```csharp
//Primary file should contain the Reduce method
public record PostingAppState : State {
    public UndoableState<PostsState> Posts {get;init;} = new PostsState();
    public string PosterName {get;init;} = "Name not set";
}

public partial class PostingAppStateStore : StateStore<PostingAppState>
{
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
//Different File
public record PostItem {
    public string Poster {get;init;}
    public string PostText {get;init;}
}
public record PostsState : State {
    public ImmutableArray<PostItem> Posts {get;init;} = ImmutableArray.Create<PostItem>();
}
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
```
