

using System.Collections.Immutable;

public partial class ExampleStateStore : StateStore<ExampleState>
{
    protected override string Path => "/root/AppStateStore";

    new public static ExampleStateStore Instance {get;set;}
    public static ExampleState GetCurrentState()
    {
        return Instance.CurrentState;
    }

    public record ChangedHeaderText(string HeaderText) : Action;
    public record PageAdded(string PageName) : Action;

    public override void _Ready()
    {
        Instance = this.GetNode<ExampleStateStore>(Path);

        On(typeof(ChangedHeaderText), (state, action) =>
        {
            var headerUpdateAction = action as ChangedHeaderText;
            return state with { HeaderText = headerUpdateAction.HeaderText };
        });

        On(typeof(PageAdded), (state, action) =>
        {
            var pageAdded = action as PageAdded;
            ImmutableArray<string> pages = state.Pages.Add(pageAdded.PageName);
            return state with { Pages = pages };
        });
    }
}