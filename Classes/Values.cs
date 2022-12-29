using ezrSquared.Errors;
using ezrSquared.General;
using ezrSquared.Nodes;
using ezrSquared.Helpers;
using static ezrSquared.Constants.constants;
using static ezrSquared.Main.ezr;
using ezrSquared.Main;
using System.Reflection;

namespace ezrSquared.Values
{
    public abstract class item
    {
        public position? startPos;
        public position? endPos;
        public context? context;

        public item() { }

        public item setPosition(position? startPos, position? endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            return this;
        }

        public item setContext(context? context)
        {
            this.context = context;
            return this;
        }

        public virtual item? compareEqual(item other, out error? error)
        {
            error = null;
            return new boolean(Equals(other)).setContext(context);
        }

        public virtual item? compareNotEqual(item other, out error? error)
        {
            error = null;
            return new boolean(!Equals(other)).setContext(context);
        }

        public virtual item? compareAnd(item other, out error? error)
        {
            error = null;
            return new boolean(isTrue() && other.isTrue());
        }

        public virtual item? compareOr(item other, out error? error)
        {
            error = null;
            return new boolean(isTrue() || other.isTrue());
        }

        public virtual item? checkIn(item other, out error? error)
        {
            error = null;
            if (other is list)
                return new boolean(((list)other).hasElement(this)).setContext(context);
            else if (other is array)
                return new boolean(((array)other).hasElement(this)).setContext(context);

            error = illegalOperation();
            return null;
        }
        public virtual bool isTrue() { return false; }

        public virtual item? addedTo(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? subbedBy(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? multedBy(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? divedBy(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? modedBy(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? powedBy(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? compareLessThan(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? compareGreaterThan(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? compareLessThanOrEqual(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? compareGreaterThanOrEqual(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? invert(out error? error)
        {
            error = illegalOperation();
            return null;
        }

        public virtual runtimeResult execute(item[] args) { return new runtimeResult().failure(illegalOperation()); }
        public virtual runtimeResult retrieve(node node) { return new runtimeResult().failure(illegalOperation()); }

        public virtual item copy() { throw new Exception($"No copy method defined for '{GetType().Name}'!"); }

        public error illegalOperation(item? other = null)
        {
            if (other != null)
                return new runtimeError(this.startPos, other.endPos, RT_ILLEGALOP, $"Illegal operation for types '{this.GetType().Name}' and '{other.GetType().Name}'", this.context);
            return new runtimeError(this.startPos, this.endPos, RT_ILLEGALOP, $"Illegal operation for type '{this.GetType().Name}'", this.context);
        }

        public override string ToString() { throw new Exception($"No ToString method defined for '{GetType().Name}'!"); }
        public override bool Equals(object? obj) { throw new Exception($"No Equals method defined for '{GetType().Name}'!"); }
        public override int GetHashCode() { throw new Exception($"No GetHashCode method defined for '{GetType().Name}'!"); }
    }

    public abstract class value : item
    {
        public dynamic storedValue;
        public context? internalContext;
        private interpreter interpreter;

        public value(dynamic storedValue)
        {
            this.storedValue = storedValue;
            interpreter = new interpreter();
        }

        public context generateContext()
        {
            context newContext = new context($"<<{GetType().Name}> internal>", context, startPos, false);
            newContext.symbolTable = new symbolTable(newContext.parent.symbolTable);
            return newContext;
        }

        public override runtimeResult execute(item[] args)
        {
            internalContext = generateContext();
            return new runtimeResult().success(this);
        }

        public override runtimeResult retrieve(node node)
        {
            runtimeResult result = new runtimeResult();
            if (internalContext != null)
            {
                item value = result.register(interpreter.visit(node, internalContext));
                if (result.shouldReturn()) return result;
                return result.success(value);
            }

            return result.failure(new runtimeError(node.startPos, node.endPos, RT_ILLEGALOP, "'retrieve' method called on uninitialized object", context));
        }

        public override string ToString() { return storedValue.ToString(); }
        public override bool Equals(object? obj) { if (GetType() == obj.GetType()) return storedValue == ((value)obj).storedValue; return false; }
        public override int GetHashCode() { return storedValue.GetHashCode(); }
    }

    public class boolean : value
    {
        public boolean(bool value) : base(value) { }

        public override item? invert(out error? error)
        {
            error = null;
            return new boolean(!(bool)storedValue).setContext(context);
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("as_string", new predefined_function("boolean_as_string", asString, new string[0]));
            internalContext.symbolTable.set("as_character_list", new predefined_function("boolean_as_character_list", asCharList, new string[0]));
            internalContext.symbolTable.set("as_integer", new predefined_function("boolean_as_integer", asInteger, new string[0]));
            return new runtimeResult().success(this);
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }
        private runtimeResult asInteger(context context, position[] positions) { return new runtimeResult().success(new integer_number(isTrue() ? 1 : 0)); }

        public override bool isTrue() { return (bool)storedValue; }
        public override item copy() { return new boolean(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return base.ToString().ToLower(); }
    }

    public class nothing : value
    {
        public nothing() : base(null) { }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("as_string", new predefined_function("nothing_as_string", asString, new string[0]));
            internalContext.symbolTable.set("as_character_list", new predefined_function("nothing_as_character_list", asCharList, new string[0]));
            internalContext.symbolTable.set("as_integer", new predefined_function("nothing_as_integer", asInteger, new string[0]));
            internalContext.symbolTable.set("as_boolean", new predefined_function("nothing_as_boolean", asBoolean, new string[0]));
            return new runtimeResult().success(this);
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }
        private runtimeResult asInteger(context context, position[] positions) { return new runtimeResult().success(new integer_number(0)); }
        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(false)); }

        public override item copy() { return new nothing().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return "nothing"; }
        public override int GetHashCode() { return 0; }
    }

    public class integer_number : value
    {
        public integer_number(int value) : base(value) { }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new integer_number(storedValue + ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new integer_number(storedValue + (int)((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? subbedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new integer_number(storedValue - ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new integer_number(storedValue - (int)((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new integer_number(storedValue * ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new integer_number(storedValue * (int)((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }

                return new integer_number(storedValue / otherValue).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0f)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }

                return new integer_number(storedValue / (int)otherValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? modedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                integer_number other_ = (integer_number)other;
                if (other_.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Modulo by zero", context);
                    return null;
                }

                return new integer_number(storedValue % other_.storedValue).setContext(context);
            }
            else if (other is float_number)
            {
                float_number other_ = (float_number)other;
                if (other_.storedValue == 0f)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Modulo by zero", context);
                    return null;
                }

                return new integer_number(storedValue % (int)other_.storedValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? powedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new integer_number((int)MathF.Pow(storedValue, ((integer_number)other).storedValue)).setContext(context);
            else if (other is float_number)
                return new integer_number((int)MathF.Pow(storedValue, (int)((float_number)other).storedValue)).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThan(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue < ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue < ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThan(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue > ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue > ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue <= ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue <= ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue >= ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue >= ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? invert(out error? error)
        {
            error = null;
            return new integer_number((storedValue == 0) ? 1 : 0).setContext(context);
        }
        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("as_string", new predefined_function("integer_as_string", asString, new string[0]));
            internalContext.symbolTable.set("as_character_list", new predefined_function("integer_as_character_list", asCharList, new string[0]));
            internalContext.symbolTable.set("as_float", new predefined_function("integer_as_float", asFloat, new string[0]));
            internalContext.symbolTable.set("as_boolean", new predefined_function("integer_as_boolean", asBoolean, new string[0]));
            return new runtimeResult().success(this);
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }
        private runtimeResult asFloat(context context, position[] positions) { return new runtimeResult().success(new float_number((float)storedValue)); }
        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(isTrue())); }

        public override bool isTrue() { return storedValue != 0; }
        public override item copy() { return new integer_number(storedValue).setPosition(startPos, endPos).setContext(context); }
    }

    public class float_number : value
    {
        public float_number(float value) : base(value) { }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new float_number(storedValue + ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new float_number(storedValue + ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? subbedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new float_number(storedValue - ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new float_number(storedValue - ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new float_number(storedValue * ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new float_number(storedValue * ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }

                return new float_number(storedValue / otherValue).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0f)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }

                return new float_number(storedValue / otherValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? modedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Modulo by zero", context);
                    return null;
                }

                return new float_number(storedValue % otherValue).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0f)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Modulo by zero", context);
                    return null;
                }

                return new float_number(storedValue % otherValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? powedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new float_number(MathF.Pow(storedValue, ((integer_number)other).storedValue)).setContext(context);
            else if (other is float_number)
                return new float_number(MathF.Pow(storedValue, ((float_number)other).storedValue)).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThan(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue < ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue < ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThan(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue > ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue > ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue <= ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue <= ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
                return new boolean(storedValue >= ((integer_number)other).storedValue).setContext(context);
            else if (other is float_number)
                return new boolean(storedValue >= ((float_number)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? invert(out error? error)
        {
            error = null;
            return new float_number((storedValue == 0f) ? 1f : 0f).setContext(context);
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("as_string", new predefined_function("float_as_string", asString, new string[0]));
            internalContext.symbolTable.set("as_character_list", new predefined_function("float_as_character_list", asCharList, new string[0]));
            internalContext.symbolTable.set("as_integer", new predefined_function("float_as_integer", asInteger, new string[0]));
            internalContext.symbolTable.set("as_boolean", new predefined_function("float_as_boolean", asBoolean, new string[0]));
            internalContext.symbolTable.set("round_to", new predefined_function("float_round_to", roundTo, new string[1] { "digit" }));
            return new runtimeResult().success(this);
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }
        private runtimeResult asInteger(context context, position[] positions) { return new runtimeResult().success(new integer_number((int)storedValue)); }
        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(isTrue())); }
        private runtimeResult roundTo(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item digit = context.symbolTable.get("digit");
            if (digit is not integer_number)
                return result.failure(new runtimeError(startPos, endPos, RT_TYPE, "Digit must be an integer_number", context));

            return new runtimeResult().success(new float_number(MathF.Round((float)storedValue, (int)((integer_number)digit).storedValue)));
        }

        public override bool isTrue() { return storedValue != 0f; }
        public override item copy() { return new float_number(storedValue).setPosition(startPos, endPos).setContext(context); }
    }

    public class @string : value
    {
        public @string(string value) : base(string.IsNullOrEmpty(value) ? "" : value) { }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is @string)
                return new @string(storedValue + ((@string)other).storedValue).setContext(context);
            else if (other is character_list)
                return new @string(storedValue + string.Join("", ((character_list)other).storedValue)).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                string result = "";
                for (int i = 0; i < ((integer_number)other).storedValue; i++)
                    result += storedValue;
                return new @string(result).setContext(context);
            }
            else if (other is float_number)
            {
                string result = "";
                for (int i = 0; i < ((float_number)other).storedValue; i++)
                    result += storedValue;
                return new @string(result).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "String division by negative value", context);
                    return null;
                }

                string result = storedValue.Substring(0, storedValue.Length / otherValue);
                return new @string(result).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "String division by negative value", context);
                    return null;
                }

                string result = storedValue.Substring(0, (int)(storedValue.Length / otherValue));
                return new @string(result).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue >= storedValue.Length)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be greater than or equal to length of string", context);
                    return null;
                }

                return new @string(storedValue[otherValue].ToString()).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? checkIn(item other, out error? error)
        {
            error = null;
            if (other is @string)
                return new boolean(((@string)other).storedValue.Contains(storedValue)).setContext(context);
            else if (other is character_list)
                return new boolean(string.Join("", ((character_list)other).storedValue).Contains(storedValue)).setContext(context);
            return base.checkIn(other, out error);
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("length", new integer_number(storedValue.ToString().Length));
            internalContext.symbolTable.set("slice", new predefined_function("string_slice", stringSlice, new string[2] { "start", "end" }));
            internalContext.symbolTable.set("insert", new predefined_function("string_insert", stringInsert, new string[2] { "index", "substring" }));
            internalContext.symbolTable.set("replace", new predefined_function("string_replace", stringReplace, new string[2] { "old", "new" }));
            internalContext.symbolTable.set("split", new predefined_function("string_split", stringSplit, new string[1] { "substring" }));
            internalContext.symbolTable.set("join", new predefined_function("string_join", stringJoin, new string[1] { "array" }));
            internalContext.symbolTable.set("as_integer", new predefined_function("string_as_integer", asInteger, new string[0] { }));
            internalContext.symbolTable.set("as_float", new predefined_function("string_as_float", asFloat, new string[0] { }));
            internalContext.symbolTable.set("try_as_integer", new predefined_function("string_try_as_integer", tryAsInteger, new string[0] { }));
            internalContext.symbolTable.set("try_as_float", new predefined_function("string_try_as_float", tryAsFloat, new string[0] { }));
            internalContext.symbolTable.set("as_boolean", new predefined_function("string_as_boolean", asBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_character_list", new predefined_function("string_character_list", asCharList, new string[0] { }));
            return new runtimeResult().success(this);
        }

        private runtimeResult stringSlice(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("start");
            item end = context.symbolTable.get("end");

            if (start is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer_number", context));
            else if (end is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer_number", context));

            int startAsInt = (int)((integer_number)start).storedValue;
            int endAsInt = (int)((integer_number)end).storedValue;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be less than zero", context));
            else if (endAsInt > storedValue.ToString().Length)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End cannot be greater than length of string", context));
            else if (startAsInt > endAsInt)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be greater than end", context));

            return result.success(new @string(storedValue.ToString().Substring(startAsInt, endAsInt)));
        }

        private runtimeResult stringInsert(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("index");
            item substring = context.symbolTable.get("substring");

            if (start is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer_number", context));
            else if (substring is not @string)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Substring must be a string", context));

            int startAsInt = (int)((integer_number)start).storedValue;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be less than zero", context));
            else if (startAsInt > storedValue.ToString().Length)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be greater than length of string", context));

            return result.success(new @string(storedValue.ToString().Insert(startAsInt, ((@string)substring).storedValue.ToString())));
        }

        private runtimeResult stringReplace(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item old = context.symbolTable.get("old");
            item new_ = context.symbolTable.get("new");

            if (old is not @string)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Old must be a string", context));
            if (new_ is not @string)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "New must be a string", context));
            return result.success(new @string(storedValue.ToString().Replace(((@string)old).storedValue.ToString(), ((@string)new_).storedValue.ToString())));
        }

        private runtimeResult stringSplit(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item substring = context.symbolTable.get("substring");

            if (substring is not @string)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Substring must be a string", context));
            string[] split = storedValue.ToString().Split(((@string)substring).storedValue.ToString());

            item[] elements = new item[split.Length];
            for (int i = 0; i < split.Length; i++)
                elements[i] = new @string(split[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(elements));
        }

        private runtimeResult stringJoin(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item array = context.symbolTable.get("array");

            item[] items;
            if (array is array)
                items = (item[])((array)array).storedValue;
            else if (array is list)
                items = ((List<item>)((list)array).storedValue).ToArray();
            else
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Array must be an array or list", context));

            string[] arrayAsString = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
                arrayAsString[i] = items[i].ToString();

            return result.success(new @string(string.Join(storedValue.ToString(), arrayAsString)));
        }

        private runtimeResult asInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(storedValue.ToString(), out int integer))
                return result.success(new integer_number(integer));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to integer_number", context));
        }

        private runtimeResult asFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(storedValue.ToString(), out float float_))
                return result.success(new float_number(float_));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to float_number", context));
        }

        private runtimeResult tryAsInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(storedValue.ToString(), out int integer))
                return result.success(new integer_number(integer));
            return result.success(new nothing());
        }

        private runtimeResult tryAsFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(storedValue.ToString(), out float float_))
                return result.success(new float_number(float_));
            return result.success(new nothing());
        }

        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(storedValue)); }
        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(isTrue())); }

        public override bool isTrue() { return storedValue.Length > 0; }
        public override item copy() { return new @string(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"\"{storedValue}\""; }
        public string ToPureString() { return storedValue; }
    }
    
    public class character_list : value
    {
        public character_list(string value) : base(value.ToCharArray().ToList()) { }
        public character_list(List<char> value) : base(value) { }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is @string)
            {
                ((List<char>)storedValue).AddRange(((@string)other).storedValue.ToString().ToCharArray());
                return new nothing().setContext(context);
            }
            else if (other is character_list)
            {
                ((List<char>)storedValue).AddRange(((character_list)other).storedValue);
                return new nothing().setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "CharacterList multiplication by negative value", context);
                    return null;
                }

                char[] multedValues = new char[storedValue.Count * otherValue];

                for (int i = 0; i < otherValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Count * i);
                return new character_list(multedValues.ToList()).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "CharacterList multiplication by negative value", context);
                    return null;
                }

                char[] multedValues = new char[storedValue.Count * (int)otherValue];

                for (int i = 0; i < (int)otherValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Count * i);
                return new character_list(multedValues.ToList()).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "CharacterList division by negative value", context);
                    return null;
                }

                char[] divedValues = new char[storedValue.Count / otherValue];
                for (int i = 0; i < divedValues.Length; i++)
                    divedValues[i] = storedValue[i];

                return new character_list(divedValues.ToList()).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "CharacterList division by negative value", context);
                    return null;
                }

                char[] divedValues = new char[(int)(storedValue.Count / otherValue)];
                for (int i = 0; i < divedValues.Length; i++)
                    divedValues[i] = storedValue[i];

                return new character_list(divedValues.ToList()).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue >= storedValue.Count)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be greater than or equal to length of character_list", context);
                    return null;
                }

                return new @string(storedValue[otherValue].ToString()).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? checkIn(item other, out error? error)
        {
            error = null;
            if (other is @string)
                return new boolean(((@string)other).storedValue.Contains(storedValue)).setContext(context);
            else if (other is character_list)
                return new boolean(string.Join("", ((character_list)other).storedValue).Contains(string.Join("", storedValue))).setContext(context);
            return base.checkIn(other, out error);
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("length", new integer_number(((List<char>)storedValue).Count));
            internalContext.symbolTable.set("slice", new predefined_function("character_list_slice", charListSlice, new string[2] { "start", "end" }));
            internalContext.symbolTable.set("insert", new predefined_function("character_list_insert", charListInsert, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("set", new predefined_function("character_list_set", charListSet, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("remove", new predefined_function("character_list_remove", charListRemove, new string[1] { "value" }));
            internalContext.symbolTable.set("remove_at", new predefined_function("character_list_remove_at", charListRemoveAt, new string[1] { "index" }));
            internalContext.symbolTable.set("as_integer", new predefined_function("character_list_as_integer", asInteger, new string[0] { }));
            internalContext.symbolTable.set("as_float", new predefined_function("character_list_as_float", asFloat, new string[0] { }));
            internalContext.symbolTable.set("try_as_integer", new predefined_function("character_list_try_as_integer", tryAsInteger, new string[0] { }));
            internalContext.symbolTable.set("try_as_float", new predefined_function("character_list_try_as_float", tryAsFloat, new string[0] { }));
            internalContext.symbolTable.set("as_boolean", new predefined_function("character_list_as_boolean", asBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_string", new predefined_function("character_list_as_string", asString, new string[0] { }));
            return new runtimeResult().success(this);
        }

        private runtimeResult charListSlice(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("start");
            item end = context.symbolTable.get("end");

            if (start is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer_number", context));
            else if (end is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer_number", context));

            int startAsInt = (int)((integer_number)start).storedValue;
            int endAsInt = (int)((integer_number)end).storedValue + 1;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be less than zero", context));
            else if (endAsInt > ((List<char>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End cannot be greater than length of character_list", context));
            else if (startAsInt >= endAsInt - 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be greater than or equal to end", context));

            return result.success(new character_list(((List<char>)storedValue).GetRange(startAsInt, endAsInt - startAsInt)));
        }

        private runtimeResult charListInsert(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");

            if (index is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer_number", context));

            int indexAsInt = (int)((integer_number)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be less than zero", context));
            else if (indexAsInt > ((List<char>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be greater than length of character_list", context));

            if (value is not @string && value is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be a string or character_list", context));

            string value_ = (value is @string) ? ((@string)value).storedValue : string.Join("", ((character_list)value).storedValue);
            if (value_.Length > 1 || value_.Length < 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be of length 1", context));

            ((List<char>)storedValue).Insert(indexAsInt, value_[0]);
            return result.success(new nothing());
        }

        private runtimeResult charListSet(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");
            
            if (index is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer_number", context));

            int indexAsInt = (int)((integer_number)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be less than zero", context));
            else if (indexAsInt >= ((List<char>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be greater than length of character_list", context));

            if (value is not @string && value is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be a string or character_list", context));

            string value_ = (value is @string) ? ((@string)value).storedValue : string.Join("", ((character_list)value).storedValue);
            if (value_.Length > 1 || value_.Length < 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be of length 1", context));

            ((List<char>)storedValue)[indexAsInt] = value_[0];
            return result.success(new nothing());
        }

        private runtimeResult charListRemove(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item value = context.symbolTable.get("value");

            if (value is not @string && value is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be a string or character_list", context));

            string value_ = (value is @string) ? ((@string)value).storedValue : string.Join("", ((character_list)value).storedValue);
            if (value_.Length > 1 || value_.Length < 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be of length 1", context));

            char charValue = value_[0];
            if (!((List<char>)storedValue).Contains(charValue))
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "CharacterList does not contain value", context));

            ((List<char>)storedValue).Remove(charValue);
            return result.success(new nothing());
        }

        private runtimeResult charListRemoveAt(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");

            if (index is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer_number", context));

            int indexAsInt = (int)((integer_number)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be less than zero", context));
            else if (indexAsInt >= ((List<char>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be greater than length of character_list", context));

            ((List<char>)storedValue).RemoveAt(indexAsInt);
            return result.success(new nothing());
        }

        private runtimeResult asInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(string.Join("", storedValue), out int integer))
                return result.success(new integer_number(integer));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to integer_number", context));
        }

        private runtimeResult asFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(string.Join("", storedValue), out float float_))
                return result.success(new float_number(float_));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to float_number", context));
        }

        private runtimeResult tryAsInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(string.Join("", storedValue), out int integer))
                return result.success(new integer_number(integer));
            return result.success(new nothing());
        }

        private runtimeResult tryAsFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(string.Join("", storedValue), out float float_))
                return result.success(new float_number(float_));
            return result.success(new nothing());
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(string.Join("", storedValue))); }
        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(isTrue())); }

        public override bool isTrue() { return storedValue.Count > 0; }
        public override item copy() { return new character_list(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"'{string.Join("", storedValue)}'"; }
        public string ToPureString() { return string.Join("", storedValue); }
    }

    public class array : value
    {
        public array(item[] elements) : base(elements) { }

        public bool hasElement(item other)
        {
            for (int i = 0; i < storedValue.Length; i++)
                if (storedValue[i].Equals(other)) return true;
            return false;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Array multiplication by negative value", context);
                    return null;
                }

                item[] multedValues = new item[storedValue.Length * otherValue];

                for (int i = 0; i < otherValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Length * i);
                return new array(multedValues).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Array multiplication by negative value", context);
                    return null;
                }

                item[] multedValues = new item[storedValue.Length * (int)otherValue];

                for (int i = 0; i < (int)otherValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Length * i);
                return new array(multedValues).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Array division by negative value", context);
                    return null;
                }

                item[] divedValues = new item[storedValue.Length / otherValue];
                for (int i = 0; i < divedValues.Length; i++)
                    divedValues[i] = storedValue[i];

                return new array(divedValues).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Array division by negative value", context);
                    return null;
                }

                item[] divedValues = new item[(int)(storedValue.Length / otherValue)];
                for (int i = 0; i < divedValues.Length; i++)
                    divedValues[i] = storedValue[i];

                return new array(divedValues).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue >= storedValue.Length)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be greater than or equal to length of array", context);
                    return null;
                }

                return storedValue[otherValue].setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("length", new integer_number(((item[])storedValue).Length));
            internalContext.symbolTable.set("slice", new predefined_function("array_slice", arraySlice, new string[2] { "start", "end" }));
            internalContext.symbolTable.set("as_boolean", new predefined_function("array_as_boolean", asBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_string", new predefined_function("array_as_string", asString, new string[0] { }));
            internalContext.symbolTable.set("as_character_list", new predefined_function("array_as_character_list", asCharList, new string[0] { }));
            return new runtimeResult().success(this);
        }

        private runtimeResult arraySlice(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("start");
            item end = context.symbolTable.get("end");

            if (start is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer_number", context));
            else if (end is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer_number", context));

            int startAsInt = (int)((integer_number)start).storedValue;
            int endAsInt = (int)((integer_number)end).storedValue + 1;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be less than zero", context));
            else if (endAsInt > ((item[])storedValue).Length)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End cannot be greater than length of array", context));
            else if (startAsInt > endAsInt)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be greater than end", context));

            return result.success(new array(((item[])storedValue)[startAsInt..endAsInt]));
        }

        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(isTrue())); }
        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }

        public override bool isTrue() { return storedValue.Length > 0; }
        public override item copy() { return new array(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString()
        {
            string[] elementStrings = new string[storedValue.Length];
            for (int i = 0; i < storedValue.Length; i++)
                elementStrings[i] = storedValue[i].ToString();
            return $"({string.Join(", ", elementStrings)})";
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            for (int i = 0; i < storedValue.Length; i++)
                hashCode ^= storedValue[i].GetHashCode() << (int)MathF.Pow(2, i);
            return hashCode;
        }

        public override bool Equals(object? obj) { if (obj is item) return ((item)obj).GetHashCode() == GetHashCode(); return false; }
    }

    public class list : value
    {
        public list(item[] elements) : base(elements.ToList()) { }
        public list(List<item> elements) : base(elements) { }

        public bool hasElement(item other)
        {
            for (int i = 0; i < storedValue.Count; i++)
                if (storedValue[i].Equals(other)) return true;
            return false;
        }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is list)
            {
                list otherAsArrayLike = (list)other;
                storedValue.AddRange(otherAsArrayLike.storedValue);
                return new nothing().setContext(context);
            }

            storedValue.Add(other);
            return new nothing().setContext(context);
        }

        public override item? subbedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue >= storedValue.Count)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be greater than or equal to length of list", context);
                    return null;
                }

                item removedValue = storedValue[otherValue];
                storedValue.RemoveAt(otherValue);
                return removedValue.setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "List multiplication by negative value", context);
                    return null;
                }

                item[] multedValues = new item[storedValue.Count * otherValue];

                for (int i = 0; i < otherValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Count * i);
                return new list(multedValues).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "List multiplication by negative value", context);
                    return null;
                }

                item[] multedValues = new item[storedValue.Count * (int)otherValue];

                for (int i = 0; i < (int)otherValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Count * i);
                return new list(multedValues).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "List division by negative value", context);
                    return null;
                }

                item[] divedValues = new item[storedValue.Count / otherValue];
                for (int i = 0; i < divedValues.Length; i++)
                    divedValues[i] = storedValue[i];

                return new list(divedValues).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "List division by negative value", context);
                    return null;
                }

                item[] divedValues = new item[(int)(storedValue.Count / otherValue)];
                for (int i = 0; i < divedValues.Length; i++)
                    divedValues[i] = storedValue[i];

                return new list(divedValues).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue >= storedValue.Count)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Index cannot be greater than or equal to length of list", context);
                    return null;
                }

                return storedValue[otherValue].setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("length", new integer_number(((List<item>)storedValue).Count));
            internalContext.symbolTable.set("slice", new predefined_function("list_slice", listSlice, new string[2] { "start", "end" }));
            internalContext.symbolTable.set("insert", new predefined_function("list_insert", listInsert, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("set", new predefined_function("list_set", listSet, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("remove", new predefined_function("list_remove", listRemove, new string[1] { "value" }));
            internalContext.symbolTable.set("remove_at", new predefined_function("list_remove_at", listRemoveAt, new string[1] { "index" }));
            internalContext.symbolTable.set("as_boolean", new predefined_function("list_as_boolean", asBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_string", new predefined_function("list_as_string", asString, new string[0] { }));
            internalContext.symbolTable.set("as_character_list", new predefined_function("list_as_character_list", asCharList, new string[0] { }));
            return new runtimeResult().success(this);
        }

        private runtimeResult listSlice(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("start");
            item end = context.symbolTable.get("end");

            if (start is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer_number", context));
            else if (end is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer_number", context));

            int startAsInt = (int)((integer_number)start).storedValue;
            int endAsInt = (int)((integer_number)end).storedValue + 1;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be less than zero", context));
            else if (endAsInt > ((List<item>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End cannot be greater than length of list", context));
            else if (startAsInt >= endAsInt - 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start cannot be greater than or equal to end", context));

            return result.success(new list(((List<item>)storedValue).GetRange(startAsInt, endAsInt - startAsInt)));
        }

        private runtimeResult listInsert(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");

            if (index is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer_number", context));

            int indexAsInt = (int)((integer_number)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be less than zero", context));
            else if (indexAsInt > ((List<item>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be greater than length of list", context));

            ((List<item>)storedValue).Insert(indexAsInt, value);
            return result.success(new nothing());
        }

        private runtimeResult listSet(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");

            if (index is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer_number", context));

            int indexAsInt = (int)((integer_number)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be less than zero", context));
            else if (indexAsInt >= ((List<item>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be greater than length of list", context));

            ((List<item>)storedValue)[indexAsInt] = value;
            return result.success(new nothing());
        }

        private runtimeResult listRemove(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item value = context.symbolTable.get("value");

            if (!((List<item>)storedValue).Contains(value))
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "List does not contain value", context));

            ((List<item>)storedValue).Remove(value);
            return result.success(new nothing());
        }

        private runtimeResult listRemoveAt(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");

            if (index is not integer_number)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer_number", context));

            int indexAsInt = (int)((integer_number)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be less than zero", context));
            else if (indexAsInt >= ((List<item>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index cannot be greater than length of list", context));

            ((List<item>)storedValue).RemoveAt(indexAsInt);
            return result.success(new nothing());
        }

        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(isTrue())); }
        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }

        public override bool isTrue() { return storedValue.Count > 0; }
        public override item copy() { return new list(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString()
        {
            string[] elementStrings = new string[storedValue.Count];
            for (int i = 0; i < storedValue.Count; i++)
                elementStrings[i] = storedValue[i].ToString();
            return $"[{string.Join(", ", elementStrings)}]";
        }
        public override int GetHashCode()
        {
            int hashCode = 0;
            for (int i = 0; i < storedValue.Count; i++)
                hashCode ^= storedValue[i].GetHashCode() << (int)MathF.Pow(2, i);
            return hashCode;
        }
        public override bool Equals(object? obj) { if (obj is item) return ((item)obj).GetHashCode() == GetHashCode(); return false; }
    }

    public class dictionary : value
    {
        public dictionary(Dictionary<item, item> dictionary) : base(dictionary) { }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is dictionary)
            {
                KeyValuePair<item, item>[] otherValue = ((Dictionary<item, item>)((dictionary)other).storedValue).AsEnumerable().ToArray();
                for (int i = 0; i < otherValue.Length; i++)
                {
                    if (storedValue.ContainsKey(otherValue[i].Key))
                        storedValue[otherValue[i].Key] = otherValue[i].Value;
                    else
                        storedValue.Add(otherValue[i].Key, otherValue[i].Value);
                }

                return new nothing().setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? subbedBy(item other, out error? error)
        {
            error = null;
            if (storedValue.ContainsKey(other))
            {
                storedValue.Remove(other);
                return new nothing().setContext(context);
            }

            error = new runtimeError(other.startPos, other.endPos, RT_KEY, "Key does not correspond to any value in dictionary", context);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer_number)
            {
                int otherValue = ((integer_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Dictionary division by negative value", context);
                    return null;
                }

                KeyValuePair<item, item>[] pairs = ((Dictionary<item, item>)storedValue).AsEnumerable().ToArray();
                Dictionary<item, item> newDict = new Dictionary<item, item>();

                for (int i = 0; i < storedValue.Count / otherValue; i++)
                    newDict.Add(pairs[i].Key, pairs[i].Value);

                return new dictionary(newDict).setContext(context);
            }
            else if (other is float_number)
            {
                float otherValue = ((float_number)other).storedValue;
                if (otherValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Dictionary division by negative value", context);
                    return null;
                }

                KeyValuePair<item, item>[] pairs = ((Dictionary<item, item>)storedValue).AsEnumerable().ToArray();
                Dictionary<item, item> newDict = new Dictionary<item, item>();

                for (int i = 0; i < storedValue.Count / otherValue; i++)
                    newDict.Add(pairs[i].Key, pairs[i].Value);

                return new dictionary(newDict).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (storedValue.TryGetValue(other, out item value))
                return value.setContext(context);
            else
            {
                error = new runtimeError(other.startPos, other.endPos, RT_KEY, "Key does not correspond to any value in dictionary", context);
                return null;
            }
        }
        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            KeyValuePair<item, item>[] pairs = ((Dictionary<item, item>)storedValue).AsEnumerable().ToArray();
            item[] keys = new item[pairs.Length];
            item[] values = new item[pairs.Length];
            item[] keyValuePairs = new item[pairs.Length];

            for (int i = 0; i < pairs.Length; i++)
            {
                keys[i] = pairs[i].Key;
                values[i] = pairs[i].Value;
                keyValuePairs[i] = new array(new item[2] { pairs[i].Key, pairs[i].Value }).setPosition(startPos, endPos).setContext(context);
            }

            internalContext.symbolTable.set("length", new integer_number(storedValue.Count));
            internalContext.symbolTable.set("keys", new array(keys));
            internalContext.symbolTable.set("values", new array(values));
            internalContext.symbolTable.set("pairs", new array(keyValuePairs));
            internalContext.symbolTable.set("as_boolean", new predefined_function("dictionary_as_boolean", asBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_string", new predefined_function("dictionary_as_string", asString, new string[0] { }));
            internalContext.symbolTable.set("as_character_list", new predefined_function("dictionary_as_character_list", asCharList, new string[0] { }));
            return new runtimeResult().success(this);
        }

        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(isTrue())); }
        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }

        public override bool isTrue() { return storedValue.Count > 0; }
        public override item copy() { return new dictionary(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString()
        {
            string[] elementStrings = new string[storedValue.Count];
            KeyValuePair<item, item>[] values = ((Dictionary<item, item>)storedValue).AsEnumerable().ToArray();
            for (int i = 0; i < values.Length; i++)
                elementStrings[i] = $"{values[i].Key} : {values[i].Value}";
            return '{' + string.Join(", ", elementStrings) + '}';
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            KeyValuePair<item, item>[] pairs = ((Dictionary<item, item>)storedValue).AsEnumerable().ToArray();
            for (int i = 0; i < pairs.Length; i++)
                hashCode ^= (pairs[i].Key.GetHashCode() << 2) ^ (pairs[i].Value.GetHashCode() << 4);
            return hashCode;
        }
        public override bool Equals(object? obj) { if (obj is item) return ((item)obj).GetHashCode() == GetHashCode(); return false; }
    }

    public abstract class baseFunction : item
    {
        public string name;
        public baseFunction(string? name) : base()
        { this.name = (name != null) ? name : "<anonymous>"; }

        public context generateContext()
        {
            context newContext = new context(name, context, startPos, false);
            newContext.symbolTable = new symbolTable(newContext.parent.symbolTable);
            return newContext;
        }

        public runtimeResult checkArgs(string[] argNames, item[] args)
        {
            runtimeResult result = new runtimeResult();
            if (args.Length > argNames.Length)
                return result.failure(new runtimeError(startPos, endPos, RT_ARGS, $"{args.Length - argNames.Length} too many arguments passed into '{name}'", context));
            else if (args.Length < argNames.Length)
                return result.failure(new runtimeError(startPos, endPos, RT_ARGS, $"{argNames.Length - args.Length} too few arguments passed into '{name}'", context));

            return result.success(new nothing());
        }

        public void populateArgs(string[] argNames, item[] args, context context)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string argName = argNames[i];
                item argValue = args[i];

                argValue.setContext(context);
                context.symbolTable.set(argName, argValue);
            }
        }

        public runtimeResult checkAndPopulateArgs(string[] argNames, item[] args, context context)
        {
            runtimeResult result = new runtimeResult();
            item returnValue = result.register(checkArgs(argNames, args));
            if (result.shouldReturn()) return result;
            populateArgs(argNames, args, context);
            return result.success(returnValue);
        }

        public override bool isTrue() { return true; }
    }

    public class predefined_function : baseFunction
    {
        private string[] argNames;
        private Func<context, position[], runtimeResult> function;

        public predefined_function(string? name, Func<context, position[], runtimeResult> function, string[] argNames) : base(name)
        {
            this.function = function;
            this.argNames = argNames;
        }

        public override runtimeResult execute(item[] args)
        {
            runtimeResult result = new runtimeResult();

            result.register(checkAndPopulateArgs(argNames, args, context));
            if (result.shouldReturn()) return result;

            item returnValue = result.register(function.Invoke(context, new position[2] { startPos, endPos }));
            if (result.shouldReturn()) return result;

            return result.success(returnValue.setPosition(startPos, endPos).setContext(context));
        }

        public override item copy() { return new predefined_function(name, function, argNames).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<predefined function <{name}>>"; }
    }

    public class builtin_function : baseFunction
    {
        private string[] argNames;
        public builtin_function(string? name, string[] argNames) : base(name)
        { this.argNames = argNames; }

        public override runtimeResult execute(item[] args)
        {
            runtimeResult result = new runtimeResult();
            context newContext = generateContext();

            string methodName = $"_{name}";
            MethodInfo? info = GetType().GetMethod(methodName, (BindingFlags.NonPublic | BindingFlags.Instance));
            if (info != null)
            {
                result.register(checkAndPopulateArgs(argNames, args, newContext));
                if (result.shouldReturn()) return result;

                item returnValue = result.register((runtimeResult)info.Invoke(this, new object[] { newContext }));
                if (result.shouldReturn()) return result;
                return result.success(returnValue.setPosition(startPos, endPos).setContext(newContext));
            }
            throw new Exception($"No {methodName} method defined!");
        }

        private runtimeResult _show(context context)
        {
            item value = context.symbolTable.get("message");
            if (value is @string)
                Console.WriteLine(((@string)value).ToPureString());
            else if (value is character_list)
                Console.WriteLine(((character_list)value).ToPureString());
            else
                Console.WriteLine(value.ToString());

            return new runtimeResult().success(new nothing());
        }

        private runtimeResult _show_error(context context)
        {
            runtimeResult result = new runtimeResult();

            item tag = context.symbolTable.get("tag");
            item message = context.symbolTable.get("message");

            if (tag is not @string && tag is not character_list)
                return result.failure(new runtimeError(startPos, endPos, RT_TYPE, "Tag must be a string or character_list", context));
            if (message is not @string && message is not character_list)
                return result.failure(new runtimeError(startPos, endPos, RT_TYPE, "Message must be a string or character_list", context));

            string msg = (message is @string) ? ((@string)message).ToPureString() : ((character_list)message).ToPureString();
            string tg = (tag is @string) ? ((@string)tag).ToPureString() : ((character_list)tag).ToPureString();

            return new runtimeResult().failure(new runtimeError(startPos, endPos, tg, msg, context));
        }

        private runtimeResult _get(context context)
        {
            item message = context.symbolTable.get("message");
            if (message is not nothing)
            {
                if (message is @string)
                    Console.Write(((@string)message).ToPureString());
                else if (message is character_list)
                    Console.Write(((character_list)message).ToPureString());
                else
                    Console.Write(message.ToString());
            }

            string? input = Console.ReadLine();
            return new runtimeResult().success(new @string(input == null ? "" : input));
        }

        private runtimeResult _clear(context context)
        {
            Console.Clear();
            return new runtimeResult().success(new nothing());
        }

        private runtimeResult _hash(context context)
        {
            return new runtimeResult().success(new integer_number(context.symbolTable.get("value").GetHashCode()));
        }

        private runtimeResult _type_of(context context)
        {
            return new runtimeResult().success(new @string(context.symbolTable.get("value").GetType().Name));
        }

        private runtimeResult _run(context context)
        {
            runtimeResult result = new runtimeResult();
            item file = context.symbolTable.get("file");
            if (file is not @string && file is not character_list)
                return result.failure(new runtimeError(startPos, endPos, RT_TYPE, "File must be a string or character_list", context));

            string path = (file is @string) ? ((@string)file).storedValue : string.Join("", ((character_list)file).storedValue);
            if (!File.Exists(path))
                return result.failure(new runtimeError(startPos, endPos, RT_IO, $"Script \"{path}\" does not exist", context));

            string script;
            try
            {
                script = string.Join('\n', File.ReadAllLines(path));
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(startPos, endPos, RT_IO, $"Failed to load script \"{path}\"\n{exception.Message}", context));
            }

            context runtimeContext = new context("<main>", ezr.globalPredefinedContext, new position(0, 0, 0, "<main>", ""), false);
            runtimeContext.symbolTable = new symbolTable(ezr.globalPredefinedSymbolTable);

            error? error = new Main.ezr().run(Path.GetFileName(path), script, runtimeContext, out item? _);
            if (error != null)
                return result.failure(new runtimeError(startPos, endPos, RT_RUN, $"Failed to execute script \"{path}\"\n\n{error.asString()}", context));
            return result.success(new nothing());
        }

        public override item copy() { return new builtin_function(name, argNames).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin function <{name}>>"; }
    }

    public class function : baseFunction
    {
        private node bodyNode;
        private string[] argNames;
        private bool shouldReturnNull;
        private interpreter interpreter;

        public function(string? name, node bodyNode, string[] argNames, bool shouldReturnNull) : base(name)
        {
            this.bodyNode = bodyNode;
            this.argNames = argNames;
            this.shouldReturnNull = shouldReturnNull;
            this.interpreter = new interpreter();
        }

        public override runtimeResult? execute(item[] args)
        {
            runtimeResult result = new runtimeResult();
            context newContext = generateContext();

            result.register(checkAndPopulateArgs(argNames, args, newContext));
            if (result.shouldReturn()) return result;

            item? value = result.register(interpreter.visit(bodyNode, newContext));
            if (result.shouldReturn() && result.functionReturnValue == null) return result;

            if (!shouldReturnNull && value != null)
                return result.success(value);
            else if (result.functionReturnValue != null)
                return result.success(result.functionReturnValue);
            return result.success(new nothing());
        }

        public override item copy() { return new function(name, bodyNode, argNames, shouldReturnNull).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<function {name}>"; }
        public override int GetHashCode() { return ToString().GetHashCode(); }
        public override bool Equals(object? obj) { if (obj is function) return GetHashCode() == ((function)obj).GetHashCode(); return false; }
    }

    public class @class : baseFunction
    {
        private node bodyNode;
        private string[] argNames;
        private interpreter interpreter;

        public @class(string name, node bodyNode, string[] argNames) : base(name)
        {
            this.bodyNode = bodyNode;
            this.argNames = argNames;
            this.interpreter = new interpreter();
        }

        public override runtimeResult execute(item[] args)
        {
            runtimeResult result = new runtimeResult();
            context internalContext = generateContext();

            result.register(checkAndPopulateArgs(argNames, args, internalContext));
            if (result.shouldReturn()) return result;

            result.register(interpreter.visit(bodyNode, internalContext));
            if (result.shouldReturn()) return result;

            return result.success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        public override item copy() { return new @class(name, bodyNode, argNames).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<class {name}>"; }
        public override int GetHashCode() { return ToString().GetHashCode(); }
        public override bool Equals(object? obj) { if (obj is @class) return GetHashCode() == ((@class)obj).GetHashCode(); return false; }
    }

    public class @object : baseFunction
    {
        private context internalContext;
        private interpreter interpreter;

        public @object(string name, context internalContext) : base(name)
        {
            this.internalContext = internalContext;
            this.interpreter = new interpreter();
        }

        public override runtimeResult retrieve(node node)
        {
            runtimeResult result = new runtimeResult();

            item value = result.register(interpreter.visit(node, internalContext));
            if (result.shouldReturn()) return result;
            return result.success(value);
        }

        public override item copy() { return new @object(name, internalContext).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<object {name}>"; }
        public override int GetHashCode() { return ToString().GetHashCode(); }
        public override bool Equals(object? obj) { if (obj is @object) return GetHashCode() == ((@object)obj).GetHashCode(); return false; }
    }
}