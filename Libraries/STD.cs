using ezrSquared.Values;
using ezrSquared.Helpers;
using ezrSquared.Errors;
using ezrSquared.General;
using static ezrSquared.Constants.constants;

namespace ezrSquared.Libraries.STD
{
    public class integer_class : baseFunction
    {
        public integer_class() : base("<std <integer>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("maximum", new integer(int.MaxValue));
            internalContext.symbolTable.set("minimum", new integer(int.MinValue));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        public override item copy() { return new integer_class().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is integer_class; }
    }

    public class float_class : baseFunction
    {
        public float_class() : base("<std <float>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("maximum", new @float(float.MaxValue));
            internalContext.symbolTable.set("minimum", new @float(float.MinValue));
            internalContext.symbolTable.set("epsilon", new @float(float.Epsilon));
            internalContext.symbolTable.set("nan", new @float(float.NaN));
            internalContext.symbolTable.set("infinity", new @float(float.PositiveInfinity));
            internalContext.symbolTable.set("negative_infinity", new @float(float.NegativeInfinity));
            internalContext.symbolTable.set("negative_zero", new @float(float.NegativeZero));
            internalContext.symbolTable.set("pi", new @float(MathF.PI));
            internalContext.symbolTable.set("tau", new @float(MathF.Tau));
            internalContext.symbolTable.set("e", new @float(MathF.E));
            

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        public override item copy() { return new float_class().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is float_class; }
    }

    public class string_class : baseFunction
    {
        public string_class() : base("<std <string>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("empty", new @string(string.Empty));
            internalContext.symbolTable.set("concatenate", new predefined_function("string_class_concatenate", concatenate, new string[1] { "values" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult concatenate(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item values = context.symbolTable.get("values");
            if (values is not list && values is not array)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Values must be a list or array", context));

            item[] values_ = (values is list) ? ((list)values).storedValue.ToArray() : ((array)values).storedValue;
            string[] strings = new string[values_.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                if (values_[i] is not @string && values_[i] is not character_list)
                    return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "All items in values must be a string or character_list", context));
                strings[i] = (values_[i] is @string) ? ((@string)values_[i]).storedValue : string.Join("", ((character_list)values_[i]).storedValue);
            }

            return result.success(new @string(string.Concat(strings)));
        }

        public override item copy() { return new string_class().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is string_class; }
    }

    public class character_list_class : baseFunction
    {
        public character_list_class() : base("<std <character_list>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("empty", new character_list(string.Empty));
            internalContext.symbolTable.set("concatenate", new predefined_function("character_list_class_concatenate", concatenate, new string[1] { "values" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult concatenate(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item values = context.symbolTable.get("values");
            if (values is not list && values is not array)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Values must be a list or array", context));

            item[] values_ = (values is list) ? ((list)values).storedValue.ToArray() : ((array)values).storedValue;
            string[] strings = new string[values_.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                if (values_[i] is not @string && values_[i] is not character_list)
                    return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "All items in values must be a string or character_list", context));
                strings[i] = (values_[i] is @string) ? ((@string)values_[i]).storedValue : string.Join("", ((character_list)values_[i]).storedValue);
            }

            return result.success(new character_list(string.Concat(strings)));
        }

        public override item copy() { return new character_list_class().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is character_list_class; }
    }
}
