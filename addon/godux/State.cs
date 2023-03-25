namespace Godux;

public abstract record State : IComparable<State>
{
    public int CompareTo(State other)
    {
        if (this != other)
        {
            return 1;
        }
        return 0;
    }
}
