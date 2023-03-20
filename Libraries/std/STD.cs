using ezrSquared.Values;
using ezrSquared.Helpers;
using ezrSquared.Errors;
using ezrSquared.General;
using static ezrSquared.Constants.constants;
using System;

namespace ezrSquared.Libraries.STD
{
    public class integer_class : baseFunction
    {
        public override bool UPDATEONACCESS => true;

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
        public override bool UPDATEONACCESS => true;

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
        public override bool UPDATEONACCESS => true;

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
        public override bool UPDATEONACCESS => true;

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

    public class random : baseFunction
    {
        public override bool UPDATEONACCESS => true;

        private Random random_;
        public random() : base("<random <random>>")
        { random_ = new Random(); }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("get", new predefined_function("random_get", randomNumber, new string[0]));
            internalContext.symbolTable.set("get_limited", new predefined_function("random_get_limited", randomNumberLimited, new string[2] { "minimum", "maximum" }));
            internalContext.symbolTable.set("get_float", new predefined_function("random_get_float", randomNumberFloat, new string[0]));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult randomNumber(context context, position[] positions) { return new runtimeResult().success(new integer(random_.Next())); }

        private runtimeResult randomNumberLimited(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item minimum = context.symbolTable.get("minimum");
            item maximum = context.symbolTable.get("maximum");

            if (minimum is not integer && minimum is not @float)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Minimum must be an integer or float", context));
            if (maximum is not integer && maximum is not @float)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Maximum must be an integer or float", context));

            int min = (minimum is integer) ? ((integer)minimum).storedValue : (int)((@float)minimum).storedValue;
            int max = (maximum is integer) ? ((integer)maximum).storedValue : (int)((@float)maximum).storedValue;
            return result.success(new integer(random_.Next(min, max)));
        }

        private runtimeResult randomNumberFloat(context context, position[] positions) { return new runtimeResult().success(new @float(random_.NextSingle())); }

        public override item copy() { return new random().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is random; }
    }
}
