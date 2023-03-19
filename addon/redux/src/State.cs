namespace Redux;

public abstract record State
{
    public object GetValue(string propertyName)
    {
        return this.GetType().GetProperty(propertyName).GetValue(this);
    }
}
