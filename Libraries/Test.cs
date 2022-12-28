public class test : baseFunction
{
    public test() : base("test") { }

    public override runtimeResult execute(item[] args)
    {
        context internalContext = base.generateContext();
        internalContext.symbolTable.set("test", new @string("TEST!!!"));
        
        return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
    }

    public override item copy() { return new test().setPosition(startPos, endPos).setContext(context); }

    public override string ToString() { return $"<builtin library <{name}>>"; }
    public override int GetHashCode() { return ToString().GetHashCode(); }
    public override bool Equals(object? obj) { if (obj is test) return GetHashCode() == ((test)obj).GetHashCode(); return false; }
}

return new test();