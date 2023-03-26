using System.Collections.Generic;
namespace Godux;

public record UndoableState<T> : State
{
    private readonly List<T> past = new(); //e.g [1,2,3,4]
    public T Present { get; init; } // 5
    private readonly List<T> future = new(); //[6,7] 


    public bool CanUndo => past.Count > 0;
    public bool CanRedo => future.Count > 0;

    public UndoableState(T value)
    {
        Present = value;
    }

    public UndoableState<T> Undo()
    {
        if (!CanUndo) return this;

        var returnValue = this with { Present = past.Last() };
        returnValue.future.Insert(0, Present);
        returnValue.past.RemoveAt(past.Count - 1);
        return returnValue ?? this;
    }
    public UndoableState<T> Set(T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(newValue, Present))
        {
            return this;
        }
        var returnValue = this with { };
        returnValue.future.Clear();
        returnValue.past.Add(Present);
        return returnValue with { Present = newValue };
    }

    public UndoableState<T> Redo()
    {
        if (!CanRedo) return this;

        var returnValue = this with { Present = future[0] };
        returnValue.past.Add(Present);
        returnValue.future.RemoveAt(0);
        return returnValue;
    }

    public static implicit operator UndoableState<T>(T someValue)
    {
        return new UndoableState<T>(someValue);
    }
}
