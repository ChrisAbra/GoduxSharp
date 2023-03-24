using System.Collections.Generic;
namespace Godux;

public record Undoable<T>
where T : class
{
    private readonly List<T> past = new();//e.g [1,2,3,4]
    public T Present { get; init; } // 5
    private readonly List<T> future = new(); //[6,7] 

    public bool CanUndo()
    {
        return past.Count > 0;
    }
    public bool CanRedo()
    {
        return future.Count > 0;
    }

    public Undoable(T value)
    {
        Present = value;
    }

    public Undoable<T> Undo()
    {
        if (!CanUndo()) return this;

        var returnValue = this with { Present = past[-1] };
        returnValue.future.Insert(0, Present);
        returnValue.past.RemoveAt(past.Count - 1);
        return returnValue ?? this;
    }
    public Undoable<T> Set(T newValue)
    {
        var returnValue = this with { };
        returnValue.future.Clear();
        returnValue.past.Add(Present);
        return returnValue with { Present = newValue };
    }

    public Undoable<T> Redo()
    {
        if (!CanRedo()) return this;

        var returnValue = this with { Present = future[0] };
        returnValue.past.Add(Present);
        returnValue.future.RemoveAt(0);
        return returnValue;
    }

    public static implicit operator Undoable<T>(T someValue)
    {
        return new Undoable<T>(someValue);
    }
}
