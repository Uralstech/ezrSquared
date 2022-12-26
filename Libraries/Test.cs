public class test : ezrSquared.Classes.Values.value
{
    public test() : base("test") { }

    public override ezrSquared.Classes.Values.item copy() { return new test().setPosition(startPos, endPos).setContext(context); }

    public override string ToString() { return storedValue.ToString(); }
    public override bool Equals(object? obj) { if (obj is test) return storedValue == ((test)obj).storedValue; return false; }
    public override int GetHashCode() { return storedValue.GetHashCode(); }
}

return new test();