using ezrSquared.Errors;
using ezrSquared.General;
using ezrSquared.Nodes;
using ezrSquared.Helpers;
using static ezrSquared.Constants.constants;
using static ezrSquared.Main.ezr;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System;

namespace ezrSquared.Values
{
    public static class Extras
    {
        [DllImport("libc")]
        public static extern int system(string exec);
    }

    public class ItemDictionary : LinkedList<KeyValuePair<item, item>>
    {
        public int Count => _values.Length;

        private LinkedList<KeyValuePair<item, item>>[] _values;
        private error? firstError;
        private int capacity;
        private int raise;

        public ItemDictionary()
        {
            raise = 4;
            _values = new LinkedList<KeyValuePair<item, item>>[16];
        }

        public void Add(item key, item val, out error? error)
        {
            var hash = GetKeyHashCode(key, out error);
            if (error != null) return;

            if (_values[hash] == null)
                _values[hash] = new LinkedList<KeyValuePair<item, item>>();

            var keyPresent = _values[hash].Any(p => CheckEqual(p, key));
            if (firstError != null)
            {
                error = firstError;
                firstError = null;
                return;
            }

            var newValue = new KeyValuePair<item, item>(key, val);

            if (keyPresent)
            {
                _values[hash] = new LinkedList<KeyValuePair<item, item>>();
                _values[hash].AddLast(newValue);
            }
            else
            {
                _values[hash].AddLast(newValue);

                capacity++;
                if (Count <= capacity)
                    ResizeCollection();
            }
        }

        private bool CheckEqual(KeyValuePair<item, item> pair, item key)
        {
            bool isEqual = pair.Key.ItemEquals(key, out error? error);
            if (error != null && firstError == null)
                firstError = error;

            return isEqual;
        }

        private void ResizeCollection()
        {
            raise++;
            LinkedList<KeyValuePair<item, item>>[] newArray = new LinkedList<KeyValuePair<item, item>>[(int)MathF.Pow(2, raise)];
            Array.Copy(_values, newArray, _values.Length);

            _values = newArray;
        }

        public void Remove(item key, out error? error)
        {
            var hash = GetKeyHashCode(key, out error);
            if (error != null) return;

            var keyPresent = _values[hash].Any(p => CheckEqual(p, key));
            if (firstError != null)
            {
                error = firstError;
                firstError = null;
                return;
            }

            if (keyPresent)
                _values[hash] = new LinkedList<KeyValuePair<item, item>>();
        }

        public bool ContainsKey(item key, out error? error)
        {
            var hash = GetKeyHashCode(key, out error);
            if (error != null) return false;

            bool containsKey = _values[hash] == null ? false : _values[hash].Any(p => CheckEqual(p, key));
            if (firstError != null)
            {
                error = firstError;
                firstError = null;
                return false;
            }

            return containsKey;
        }

        public item? GetValue(item key, out error? error)
        {
            var hash = GetKeyHashCode(key, out error);
            if (error != null) return null;

            item? value = _values[hash] == null ? null : _values[hash].First(m => CheckEqual(m, key)).Value;
            if (firstError != null)
            {
                error = firstError;
                firstError = null;
                return null;
            }

            return value;
        }

        public IEnumerator<KeyValuePair<item, item>> GetEnumerator()
        {
            return (from collections in _values
                    where collections != null
                    from item in collections
                    select item).GetEnumerator();
        }

        public KeyValuePair<item, item>[] GetArray()
        {
            return (from collections in _values
                    where collections != null
                    from item in collections
                    select item).ToArray();
        }

        public int GetKeyHashCode(item key, out error? error)
        {
            error = null;

            var hash = key.GetItemHashCode(out error);
            if (error != null) return 0;

            return (Math.Abs(hash)) % _values.Length;
        }
    }

    public abstract class item
    {
        public position? startPos;
        public position? endPos;
        public context? context;
        public virtual bool UPDATEONACCESS => false;

        public item() { }

        public item setPosition(position? startPos, position? endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            return this;
        }

        public virtual item setContext(context? context)
        {
            this.context = context;
            return this;
        }

        public virtual item? compareEqual(item other, out error? error)
        {
            error = null;
            return new boolean(ItemEquals(other, out error)).setContext(context);
        }

        public virtual item? compareNotEqual(item other, out error? error)
        {
            error = null;
            return new boolean(!ItemEquals(other, out error)).setContext(context);
        }

        public virtual item? compareAnd(item other, out error? error)
        {
            error = null;

            bool boolValue = isTrue(out error);
            if (error != null) return null;

            bool otherBoolValue = other.isTrue(out error);
            if (error != null) return null;

            return new boolean(boolValue && otherBoolValue);
        }

        public virtual item? compareOr(item other, out error? error)
        {
            error = null;

            bool boolValue = isTrue(out error);
            if (error != null) return null;

            bool otherBoolValue = other.isTrue(out error);
            if (error != null) return null;

            return new boolean(boolValue || otherBoolValue);
        }

        public virtual item? checkIn(item other, out error? error)
        {
            error = null;
            if (other is list)
                return new boolean(((list)other).hasElement(this, out error)).setContext(context);
            else if (other is array)
                return new boolean(((array)other).hasElement(this, out error)).setContext(context);

            error = illegalOperation();
            return null;
        }
        public virtual bool isTrue(out error? error) { error = null; return false; }

        public virtual item? bitwiseOrdTo(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? bitwiseXOrdTo(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? bitwiseAndedTo(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? bitwiseLeftShiftedTo(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? bitwiseRightShiftedTo(item other, out error? error)
        {
            error = illegalOperation(other);
            return null;
        }

        public virtual item? bitwiseNotted(out error? error)
        {
            error = illegalOperation();
            return null;
        }

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

        public virtual runtimeResult execute(item[] args) { return new runtimeResult().failure(new runtimeError(startPos, endPos, RT_ILLEGALOP, "\"execute\" method called on unsupported object", context)); }
        public virtual runtimeResult get(node node) { return new runtimeResult().failure(new runtimeError(node.startPos, node.endPos, RT_ILLEGALOP, "\"get\" method called on unsupported object", context)); }
        public virtual runtimeResult set(string name, item variable) { return new runtimeResult().failure(new runtimeError(startPos, endPos, RT_ILLEGALOP, "\"set\" method called on unsupported object", context)); }

        public virtual item copy() { throw new Exception($"No copy method defined for \"{GetType().Name}\"!"); }

        public error illegalOperation(item? other = null)
        {
            if (other != null)
                return new runtimeError(this.startPos, other.endPos, RT_ILLEGALOP, $"Illegal operation for types \"{this.GetType().Name}\" and \"{other.GetType().Name}\"", this.context);
            return new runtimeError(this.startPos, this.endPos, RT_ILLEGALOP, $"Illegal operation for type \"{this.GetType().Name}\"", this.context);
        }

        public override string ToString() { throw new Exception($"No ToString method defined for \"{GetType().Name}\"!"); }
        public virtual bool ItemEquals(item obj, out error? error) { throw new Exception($"No Equals method defined for \"{GetType().Name}\"!"); }
        public virtual int GetItemHashCode(out error? error) { throw new Exception($"No GetHashCode method defined for \"{GetType().Name}\"!"); }
    }

    public abstract class value : item
    {
        public dynamic storedValue;
        public context? internalContext;
        private interpreter interpreter;
        public override bool UPDATEONACCESS => true;

        public value(dynamic storedValue)
        {
            this.storedValue = storedValue;
            interpreter = new interpreter();
        }

        public context generateContext()
        {
            context newContext = new context(GetType().Name, context, startPos, false);
            newContext.symbolTable = new symbolTable(newContext.parent.symbolTable);
            return newContext;
        }

        public override runtimeResult execute(item[] args)
        {
            internalContext = generateContext();
            return new runtimeResult().success(this);
        }

        public override runtimeResult get(node node)
        {
            runtimeResult result = new runtimeResult();
            if (internalContext != null)
            {
                item value = result.register(interpreter.visit(node, internalContext));
                if (result.shouldReturn()) return result;
                return result.success(value);
            }

            return base.get(node);
        }

        public override string ToString() { return storedValue.ToString(); }
        public override bool ItemEquals(item obj, out error? error) { error = null; if (GetType() == obj.GetType()) return storedValue == ((value)obj).storedValue; return false; }
        public override int GetItemHashCode(out error? error) { error = null; return storedValue.GetHashCode(); }
    }

    public class boolean : value
    {
        public boolean(bool value) : base(value) { }

        public override item? invert(out error? error)
        {
            error = null;
            return new boolean(!storedValue).setContext(context);
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
        private runtimeResult asInteger(context context, position[] positions) { return new runtimeResult().success(new integer(storedValue ? 1 : 0)); }

        public override bool isTrue(out error? error) { error = null; return storedValue; }
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
        private runtimeResult asInteger(context context, position[] positions) { return new runtimeResult().success(new integer(0)); }
        private runtimeResult asBoolean(context context, position[] positions) { return new runtimeResult().success(new boolean(false)); }

        public override item copy() { return new nothing().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return "nothing"; }
        public override int GetItemHashCode(out error? error) { error = null; return 0; }
    }

    public class integer : value
    {
        public integer(int value) : base(value) { }

        public override item? bitwiseOrdTo(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue | ((integer)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? bitwiseXOrdTo(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue ^ ((integer)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? bitwiseAndedTo(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue & ((integer)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? bitwiseLeftShiftedTo(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue << ((integer)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? bitwiseRightShiftedTo(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue >> ((integer)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? bitwiseNotted(out error? error)
        {
            error = null;
            return new integer(~storedValue).setContext(context);
        }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue + ((integer)other).storedValue).setContext(context);
            else if (other is @float)
                return new @float(storedValue + ((@float)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? subbedBy(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue - ((integer)other).storedValue).setContext(context);
            else if (other is @float)
                return new @float(storedValue - ((@float)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer(storedValue * ((integer)other).storedValue).setContext(context);
            else if (other is @float)
                return new @float(storedValue * ((@float)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }

                if (other is @float)
                    return new @float(storedValue / otherValue.storedValue).setContext(context);
                return new integer(storedValue / otherValue.storedValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? modedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Modulo by zero", context);
                    return null;
                }

                if (other is @float)
                    return new @float(storedValue % otherValue.storedValue).setContext(context);
                return new integer(storedValue % otherValue.storedValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? powedBy(item other, out error? error)
        {
            error = null;
            if (other is integer)
                return new integer((int)MathF.Pow(storedValue, ((integer)other).storedValue)).setContext(context);
            else if (other is @float)
                return new @float(MathF.Pow(storedValue, ((@float)other).storedValue)).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThan(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue < ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThan(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue > ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue <= ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue >= ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? invert(out error? error)
        {
            error = null;
            return new integer((storedValue == 0) ? 1 : 0).setContext(context);
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("abs", new predefined_function("integer_abs", abs, new string[0]));
            internalContext.symbolTable.set("as_string", new predefined_function("integer_as_string", asString, new string[0]));
            internalContext.symbolTable.set("as_character_list", new predefined_function("integer_as_character_list", asCharList, new string[0]));
            internalContext.symbolTable.set("as_float", new predefined_function("integer_as_float", asFloat, new string[0]));
            internalContext.symbolTable.set("as_boolean", new predefined_function("integer_as_boolean", asBoolean, new string[0]));
            return new runtimeResult().success(this);
        }


        private runtimeResult abs(context context, position[] positions) { return new runtimeResult().success(new integer(int.Abs(storedValue))); }
        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }
        private runtimeResult asFloat(context context, position[] positions) { return new runtimeResult().success(new @float(storedValue)); }
        private runtimeResult asBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            bool boolValue = isTrue(out error? error);
            if (error != null) return result.failure(error);
            return result.success(new boolean(boolValue));
        }

        public override bool isTrue(out error? error) { error = null; return storedValue != 0; }
        public override item copy() { return new integer(storedValue).setPosition(startPos, endPos).setContext(context); }
    }

    public class @float : value
    {
        public @float(float value) : base(value) { }
        public @float(int value) : base((float)value) { }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new @float(storedValue + ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? subbedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new @float(storedValue - ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new @float(storedValue * ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value other_ = (value)other;
                if (other_.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }

                return new @float(storedValue / other_.storedValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? modedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value other_ = (value)other;
                if (other_.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Modulo by zero", context);
                    return null;
                }

                return new @float(storedValue % other_.storedValue).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? powedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new @float(MathF.Pow(storedValue, ((value)other).storedValue)).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThan(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue < ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThan(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue > ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue <= ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? compareGreaterThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
                return new boolean(storedValue >= ((value)other).storedValue).setContext(context);

            error = illegalOperation(other);
            return null;
        }

        public override item? invert(out error? error)
        {
            error = null;
            return new @float((storedValue == 0f) ? 1f : 0f).setContext(context);
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("abs", new predefined_function("float_abs", abs, new string[0]));
            internalContext.symbolTable.set("as_string", new predefined_function("float_as_string", asString, new string[0]));
            internalContext.symbolTable.set("as_character_list", new predefined_function("float_as_character_list", asCharList, new string[0]));
            internalContext.symbolTable.set("as_integer", new predefined_function("float_as_integer", asInteger, new string[0]));
            internalContext.symbolTable.set("as_boolean", new predefined_function("float_as_boolean", asBoolean, new string[0]));
            internalContext.symbolTable.set("round_to", new predefined_function("float_round_to", roundTo, new string[1] { "digit" }));
            return new runtimeResult().success(this);
        }

        private runtimeResult abs(context context, position[] positions) { return new runtimeResult().success(new @float(float.Abs(storedValue))); }
        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }
        private runtimeResult asInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (storedValue < int.MinValue || storedValue > int.MaxValue)
                return result.failure(new runtimeError(startPos, endPos, RT_OVERFLOW, "Value either too large or too small to be converted to an integer", context));
            return result.success(new integer(storedValue));
        }
        private runtimeResult asBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            bool boolValue = isTrue(out error? error);
            if (error != null) return result.failure(error);
            return result.success(new boolean(boolValue));
        }

        private runtimeResult roundTo(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item digit = context.symbolTable.get("digit");
            if (digit is not integer)
                return result.failure(new runtimeError(startPos, endPos, RT_TYPE, "Digit must be an integer", context));

            return new runtimeResult().success(new @float(MathF.Round(storedValue, ((integer)digit).storedValue)));
        }

        public override bool isTrue(out error? error) { error = null; return storedValue != 0f; }
        public override item copy() { return new @float(storedValue).setPosition(startPos, endPos).setContext(context); }
    }

    public class @string : value
    {
        public @string(string value) : base(value) { }
        public @string(char value) : base(value.ToString()) { }

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
            if (other is integer || other is @float)
            {
                string result = "";
                for (int i = 0; i < ((value)other).storedValue; i++)
                    result += storedValue;
                return new @string(result).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "String division by negative value", context);
                    return null;
                }

                string result = storedValue.Substring(0, storedValue.Length / otherValue.storedValue);
                return new @string(result).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue.storedValue >= storedValue.Length)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be greater than or equal to length of string", context);
                    return null;
                }

                return new @string(storedValue[(int)otherValue.storedValue]).setContext(context);
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

            internalContext.symbolTable.set("length", new integer(storedValue.ToString().Length));
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
            internalContext.symbolTable.set("try_as_boolean", new predefined_function("string_try_as_boolean", tryAsBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_boolean_value", new predefined_function("string_as_boolean_value", asBooleanValue, new string[0] { }));
            internalContext.symbolTable.set("as_character_list", new predefined_function("string_character_list", asCharList, new string[0] { }));
            internalContext.symbolTable.set("is_null_or_empty", new predefined_function("string_is_null_or_empty", isNullOrEmpty, new string[0] { }));
            internalContext.symbolTable.set("is_null_or_spaces", new predefined_function("string_is_null_or_spaces", isNullOrSpaces, new string[0] { }));

            return new runtimeResult().success(this);
        }

        private runtimeResult stringSlice(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("start");
            item end = context.symbolTable.get("end");

            if (start is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer", context));
            else if (end is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer", context));

            int startAsInt = ((integer)start).storedValue;
            int endAsInt = ((integer)end).storedValue;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be less than zero", context));
            else if (endAsInt > storedValue.ToString().Length)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "End cannot be greater than length of string", context));
            else if (startAsInt > endAsInt)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be greater than end", context));

            return result.success(new @string(storedValue.ToString().Substring(startAsInt, endAsInt)));
        }

        private runtimeResult stringInsert(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("index");
            item substring = context.symbolTable.get("substring");

            if (start is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer", context));
            else if (substring is not @string && substring is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Substring must be a string or character_list", context));

            string subString = (substring is @string) ? ((@string)substring).storedValue : string.Join("", ((character_list)substring).storedValue);
            int startAsInt = ((integer)start).storedValue;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be less than zero", context));
            else if (startAsInt > storedValue.ToString().Length)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be greater than length of string", context));

            return result.success(new @string(storedValue.ToString().Insert(startAsInt, subString)));
        }

        private runtimeResult stringReplace(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item old = context.symbolTable.get("old");
            item new_ = context.symbolTable.get("new");

            if (old is not @string && old is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Old must be a string or character_list", context));
            if (new_ is not @string && new_ is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "New must be a string or character_list", context));
            string oldString = (old is @string) ? ((@string)old).storedValue : string.Join("", ((character_list)old).storedValue);
            string newString = (new_ is @string) ? ((@string)new_).storedValue : string.Join("", ((character_list)new_).storedValue);

            return result.success(new @string(storedValue.ToString().Replace(oldString, newString)));
        }

        private runtimeResult stringSplit(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item substring = context.symbolTable.get("substring");

            if (substring is not @string && substring is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Substring must be a string or character_list", context));

            string subString = (substring is @string) ? ((@string)substring).storedValue : string.Join("", ((character_list)substring).storedValue);
            string[] split = storedValue.ToString().Split(subString);

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
            {
                if (items[i] is @string || items[i] is character_list)
                    arrayAsString[i] = (items[i] is @string) ? ((@string)items[i]).storedValue : string.Join("", ((character_list)items[i]).storedValue);
                else
                    arrayAsString[i] = items[i].ToString();
            }

            return result.success(new @string(string.Join(storedValue.ToString(), arrayAsString)));
        }

        private runtimeResult asInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(storedValue.ToString(), out int integer))
                return result.success(new integer(integer));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to integer", context));
        }

        private runtimeResult asFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(storedValue.ToString(), out float float_))
                return result.success(new @float(float_));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to float", context));
        }

        private runtimeResult tryAsInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(storedValue.ToString(), out int integer))
                return result.success(new integer(integer));
            return result.success(new nothing());
        }

        private runtimeResult tryAsFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(storedValue.ToString(), out float float_))
                return result.success(new @float(float_));
            return result.success(new nothing());
        }

        private runtimeResult asBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (bool.TryParse(storedValue.ToString(), out bool bool_))
                return result.success(new boolean(bool_));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to boolean", context));
        }

        private runtimeResult tryAsBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (bool.TryParse(storedValue.ToString(), out bool bool_))
                return result.success(new boolean(bool_));
            return result.success(new nothing());
        }

        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(storedValue)); }
        private runtimeResult asBooleanValue(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            bool boolValue = isTrue(out error? error);
            if (error != null) return result.failure(error);
            return result.success(new boolean(boolValue));
        }

        private runtimeResult isNullOrEmpty(context context, position[] positions) { return new runtimeResult().success(new boolean(string.IsNullOrEmpty(storedValue))); }
        private runtimeResult isNullOrSpaces(context context, position[] positions) { return new runtimeResult().success(new boolean(string.IsNullOrWhiteSpace(storedValue))); }

        public override bool isTrue(out error? error) { error = null; return storedValue.Length > 0; }
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

        public override item? subbedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue.storedValue >= storedValue.Count)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be greater than or equal to length of character_list", context);
                    return null;
                }

                string removed = storedValue[(int)otherValue.storedValue].ToString();
                storedValue.RemoveAt((int)otherValue.storedValue);
                return new @string(removed).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "CharacterList multiplication by negative value", context);
                    return null;
                }

                char[] multedValues = new char[storedValue.Count * otherValue.storedValue];

                for (int i = 0; i < otherValue.storedValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Count * i);
                return new character_list(multedValues.ToList()).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "CharacterList division by negative value", context);
                    return null;
                }

                char[] divedValues = new char[storedValue.Count / otherValue.storedValue];
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
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue.storedValue >= storedValue.Count)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be greater than or equal to length of character_list", context);
                    return null;
                }

                return new @string(storedValue[(int)otherValue.storedValue].ToString()).setContext(context);
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

            internalContext.symbolTable.set("length", new integer(((List<char>)storedValue).Count));
            internalContext.symbolTable.set("slice", new predefined_function("character_list_slice", charListSlice, new string[2] { "start", "end" }));
            internalContext.symbolTable.set("insert", new predefined_function("character_list_insert", charListInsert, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("set", new predefined_function("character_list_set", charListSet, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("remove", new predefined_function("character_list_remove", charListRemove, new string[1] { "value" }));
            internalContext.symbolTable.set("as_integer", new predefined_function("character_list_as_integer", asInteger, new string[0] { }));
            internalContext.symbolTable.set("as_float", new predefined_function("character_list_as_float", asFloat, new string[0] { }));
            internalContext.symbolTable.set("try_as_integer", new predefined_function("character_list_try_as_integer", tryAsInteger, new string[0] { }));
            internalContext.symbolTable.set("try_as_float", new predefined_function("character_list_try_as_float", tryAsFloat, new string[0] { }));
            internalContext.symbolTable.set("as_boolean", new predefined_function("character_list_as_boolean", asBoolean, new string[0] { }));
            internalContext.symbolTable.set("try_as_boolean", new predefined_function("character_list_try_as_boolean", tryAsBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_boolean_value", new predefined_function("character_list_as_boolean_value", asBooleanValue, new string[0] { }));
            internalContext.symbolTable.set("as_string", new predefined_function("character_list_as_string", asString, new string[0] { }));
            internalContext.symbolTable.set("is_null_or_empty", new predefined_function("character_list_is_null_or_empty", isNullOrEmpty, new string[0] { }));
            internalContext.symbolTable.set("is_null_or_spaces", new predefined_function("character_list_is_null_or_spaces", isNullOrSpaces, new string[0] { }));
            return new runtimeResult().success(this);
        }

        private runtimeResult charListSlice(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item start = context.symbolTable.get("start");
            item end = context.symbolTable.get("end");

            if (start is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer", context));
            else if (end is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer", context));

            int startAsInt = ((integer)start).storedValue;
            int endAsInt = ((integer)end).storedValue + 1;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be less than zero", context));
            else if (endAsInt > ((List<char>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "End cannot be greater than length of character_list", context));
            else if (startAsInt >= endAsInt - 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be greater than or equal to end", context));

            return result.success(new character_list(((List<char>)storedValue).GetRange(startAsInt, endAsInt - startAsInt)));
        }

        private runtimeResult charListInsert(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");

            if (index is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer", context));

            int indexAsInt = ((integer)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be less than zero", context));
            else if (indexAsInt > ((List<char>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be greater than length of character_list", context));

            if (value is not @string && value is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be a string or character_list", context));

            char[] value_ = (value is @string) ? ((@string)value).storedValue.ToCharArray() : ((character_list)value).storedValue.ToArray();
            ((List<char>)storedValue).InsertRange(indexAsInt, value_);
            return result.success(new nothing());
        }

        private runtimeResult charListSet(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");

            if (index is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer", context));

            int indexAsInt = ((integer)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be less than zero", context));
            else if (indexAsInt >= ((List<char>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be greater than length of character_list", context));

            if (value is not @string && value is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Value must be a string or character_list", context));

            string value_ = (value is @string) ? ((@string)value).storedValue : string.Join("", ((character_list)value).storedValue);
            if (value_.Length > 1 || value_.Length < 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_LEN, "Value must be of length 1", context));

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
            char[] chars = string.Join("", storedValue).Replace(value_, string.Empty).ToCharArray();
            storedValue.Clear();
            storedValue.AddRange(chars);
            return result.success(new nothing());
        }

        private runtimeResult asInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(string.Join("", storedValue), out int integer))
                return result.success(new integer(integer));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to integer", context));
        }

        private runtimeResult asFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(string.Join("", storedValue), out float float_))
                return result.success(new @float(float_));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to float", context));
        }

        private runtimeResult tryAsInteger(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (int.TryParse(string.Join("", storedValue), out int integer))
                return result.success(new integer(integer));
            return result.success(new nothing());
        }

        private runtimeResult tryAsFloat(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (float.TryParse(string.Join("", storedValue), out float float_))
                return result.success(new @float(float_));
            return result.success(new nothing());
        }

        private runtimeResult asBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (bool.TryParse(string.Join("", storedValue), out bool bool_))
                return result.success(new boolean(bool_));
            return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Could not convert string to boolean", context));
        }

        private runtimeResult tryAsBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            if (bool.TryParse(string.Join("", storedValue), out bool bool_))
                return result.success(new boolean(bool_));
            return result.success(new nothing());
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(string.Join("", storedValue))); }
        private runtimeResult asBooleanValue(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            bool boolValue = isTrue(out error? error);
            if (error != null) return result.failure(error);
            return result.success(new boolean(boolValue));
        }

        private runtimeResult isNullOrEmpty(context context, position[] positions) { return new runtimeResult().success(new boolean(string.IsNullOrEmpty(string.Join("", storedValue)))); }
        private runtimeResult isNullOrSpaces(context context, position[] positions) { return new runtimeResult().success(new boolean(string.IsNullOrWhiteSpace(string.Join("", storedValue)))); }

        public override bool isTrue(out error? error) { error = null; return storedValue.Count > 0; }
        public override item copy() { return new character_list(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"'{string.Join("", storedValue)}'"; }
        public string ToPureString() { return string.Join("", storedValue); }
        public override bool ItemEquals(item obj, out error? error) { error = null; if (GetType() == obj.GetType()) return ToPureString() == ((character_list)obj).ToPureString(); return false; }
        public override int GetItemHashCode(out error? error) { error = null; return ToPureString().GetHashCode(); }
    }

    public class array : value
    {
        public array(item[] elements) : base(elements) { }

        public bool hasElement(item other, out error? error)
        {
            error = null;
            for (int i = 0; i < storedValue.Length; i++)
            {
                if (storedValue[i].ItemEquals(other, out error)) return true;
                if (error != null) return false;
            }
            return false;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Array multiplication by negative value", context);
                    return null;
                }

                item[] multedValues = new item[storedValue.Length * (int)otherValue.storedValue];

                for (int i = 0; i < (int)otherValue.storedValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Length * i);
                return new array(multedValues).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Array division by negative value", context);
                    return null;
                }

                item[] divedValues = new item[storedValue.Length / (int)otherValue.storedValue];
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
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue.storedValue >= storedValue.Length)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be greater than or equal to length of array", context);
                    return null;
                }

                return storedValue[(int)otherValue.storedValue].setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("length", new integer(((item[])storedValue).Length));
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

            if (start is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer", context));
            else if (end is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer", context));

            int startAsInt = ((integer)start).storedValue;
            int endAsInt = ((integer)end).storedValue + 1;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be less than zero", context));
            else if (endAsInt > ((item[])storedValue).Length)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "End cannot be greater than length of array", context));
            else if (startAsInt > endAsInt)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be greater than end", context));

            return result.success(new array(((item[])storedValue)[startAsInt..endAsInt]));
        }

        private runtimeResult asBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            bool boolValue = isTrue(out error? error);
            if (error != null) return result.failure(error);
            return result.success(new boolean(boolValue));
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }

        public override bool isTrue(out error? error) { error = null; return storedValue.Length > 0; }
        public override item copy() { return new array(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString()
        {
            string[] elementStrings = new string[storedValue.Length];
            for (int i = 0; i < storedValue.Length; i++)
                elementStrings[i] = storedValue[i].ToString();
            return $"({string.Join(", ", elementStrings)})";
        }

        public override int GetItemHashCode(out error? error)
        {
            error = null;
            int hashCode = 0;
            for (int i = 0; i < storedValue.Length; i++)
            {
                hashCode = (((hashCode << 5) + hashCode) ^ storedValue[i].GetItemHashCode(out error));
                if (error != null) return 0;
            }

            return hashCode;
        }

        public override bool ItemEquals(item obj, out error? error)
        {
            error = null;
            if (obj is array)
            {
                int hash = GetItemHashCode(out error);
                if (error != null) return false;

                int otherHash = ((array)obj).GetItemHashCode(out error);
                if (error != null) return false;

                return hash == otherHash;
            }
            return false;
        }
    }

    public class list : value
    {
        public list(item[] elements) : base(elements.ToList()) { }
        public list(List<item> elements) : base(elements) { }

        public bool hasElement(item other, out error? error)
        {
            error = null;
            for (int i = 0; i < storedValue.Count; i++)
            {
                if (storedValue[i].ItemEquals(other, out error)) return true;
                if (error != null) return false;
            }
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
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue.storedValue >= storedValue.Count)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be greater than or equal to length of list", context);
                    return null;
                }

                item removedValue = storedValue[(int)otherValue.storedValue];
                storedValue.RemoveAt((int)otherValue.storedValue);
                return removedValue.setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? multedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "List multiplication by negative value", context);
                    return null;
                }

                item[] multedValues = new item[storedValue.Count * (int)otherValue.storedValue];

                for (int i = 0; i < (int)otherValue.storedValue; i++)
                    storedValue.CopyTo(multedValues, storedValue.Count * i);
                return new list(multedValues).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "List division by negative value", context);
                    return null;
                }

                item[] divedValues = new item[storedValue.Count / (int)otherValue.storedValue];
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
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be negative value", context);
                    return null;
                }
                else if (otherValue.storedValue >= storedValue.Count)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_INDEX, "Index cannot be greater than or equal to length of list", context);
                    return null;
                }

                return storedValue[(int)otherValue.storedValue].setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override runtimeResult execute(item[] args)
        {
            base.execute(args);

            internalContext.symbolTable.set("length", new integer(((List<item>)storedValue).Count));
            internalContext.symbolTable.set("slice", new predefined_function("list_slice", listSlice, new string[2] { "start", "end" }));
            internalContext.symbolTable.set("insert", new predefined_function("list_insert", listInsert, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("set", new predefined_function("list_set", listSet, new string[2] { "index", "value" }));
            internalContext.symbolTable.set("remove", new predefined_function("list_remove", listRemove, new string[1] { "value" }));
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

            if (start is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Start must be an integer", context));
            else if (end is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "End must be an integer", context));

            int startAsInt = ((integer)start).storedValue;
            int endAsInt = ((integer)end).storedValue + 1;

            if (startAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be less than zero", context));
            else if (endAsInt > ((List<item>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "End cannot be greater than length of list", context));
            else if (startAsInt >= endAsInt - 1)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Start cannot be greater than or equal to end", context));

            return result.success(new list(((List<item>)storedValue).GetRange(startAsInt, endAsInt - startAsInt)));
        }

        private runtimeResult listInsert(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");

            if (index is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer", context));

            int indexAsInt = ((integer)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be less than zero", context));
            else if (indexAsInt > ((List<item>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be greater than length of list", context));

            ((List<item>)storedValue).Insert(indexAsInt, value);
            return result.success(new nothing());
        }

        private runtimeResult listSet(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item index = context.symbolTable.get("index");
            item value = context.symbolTable.get("value");

            if (index is not integer)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Index must be an integer", context));

            int indexAsInt = ((integer)index).storedValue;

            if (indexAsInt < 0)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be less than zero", context));
            else if (indexAsInt >= ((List<item>)storedValue).Count)
                return result.failure(new runtimeError(positions[0], positions[1], RT_INDEX, "Index cannot be greater than length of list", context));

            ((List<item>)storedValue)[indexAsInt] = value;
            return result.success(new nothing());
        }

        private runtimeResult listRemove(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();
            item value = context.symbolTable.get("value");

            if (!((List<item>)storedValue).Contains(value))
                return result.failure(new runtimeError(positions[0], positions[1], RT_KEY, "List does not contain value", context));

            ((List<item>)storedValue).Remove(value);
            return result.success(new nothing());
        }

        private runtimeResult asBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            bool boolValue = isTrue(out error? error);
            if (error != null) return result.failure(error);
            return result.success(new boolean(boolValue));
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }

        public override bool isTrue(out error? error) { error = null; return storedValue.Count > 0; }
        public override item copy() { return new list(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString()
        {
            string[] elementStrings = new string[storedValue.Count];
            for (int i = 0; i < storedValue.Count; i++)
                elementStrings[i] = storedValue[i].ToString();
            return $"[{string.Join(", ", elementStrings)}]";
        }
        public override int GetItemHashCode(out error? error)
        {
            error = null;
            int hashCode = 0;
            for (int i = 0; i < storedValue.Count; i++)
            {
                hashCode = (((hashCode << 5) + hashCode) ^ storedValue[i].GetItemHashCode(out error));
                if (error != null) return 0;
            }

            return hashCode;
        }
        public override bool ItemEquals(item obj, out error? error)
        {
            error = null;
            if (obj is list)
            {
                int hash = GetItemHashCode(out error);
                if (error != null) return false;

                int otherHash = ((list)obj).GetItemHashCode(out error);
                if (error != null) return false;

                return hash == otherHash;
            }
            return false;
        }
    }

    public class dictionary : value
    {
        public dictionary(ItemDictionary dictionary) : base(dictionary) { }

        public override item? addedTo(item other, out error? error)
        {
            error = null;
            if (other is dictionary)
            {
                KeyValuePair<item, item>[] otherValue = ((dictionary)other).storedValue.GetArray();
                for (int i = 0; i < otherValue.Length; i++)
                {
                    storedValue.Add(otherValue[i].Key, otherValue[i].Value, out error);
                    if (error != null) return null;
                }

                return new nothing().setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? subbedBy(item other, out error? error)
        {
            error = null;

            bool containsKey = storedValue.ContainsKey(other, out error);
            if (error != null) return null;

            if (containsKey)
            {
                storedValue.Remove(other, out error);
                return new nothing().setContext(context);
            }

            error = new runtimeError(other.startPos, other.endPos, RT_KEY, "Key does not correspond to any value in dictionary", context);
            return null;
        }

        public override item? divedBy(item other, out error? error)
        {
            error = null;
            if (other is integer || other is @float)
            {
                value otherValue = (value)other;
                if (otherValue.storedValue == 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Division by zero", context);
                    return null;
                }
                else if (otherValue.storedValue < 0)
                {
                    error = new runtimeError(other.startPos, other.endPos, RT_MATH, "Dictionary division by negative value", context);
                    return null;
                }

                KeyValuePair<item, item>[] pairs = ((ItemDictionary)storedValue).GetArray();
                ItemDictionary newDict = new ItemDictionary();

                for (int i = 0; i < pairs.Length / (int)otherValue.storedValue; i++)
                {
                    newDict.Add(pairs[i].Key, pairs[i].Value, out error);
                    if (error != null) return null;
                }

                return new dictionary(newDict).setContext(context);
            }

            error = illegalOperation(other);
            return null;
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            error = null;

            item? value = storedValue.GetValue(other, out error);
            if (error != null) return null;

            if (value != null)
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

            KeyValuePair<item, item>[] pairs = storedValue.GetArray();
            item[] keys = new item[pairs.Length];
            item[] values = new item[pairs.Length];
            item[] keyValuePairs = new item[pairs.Length];

            for (int i = 0; i < pairs.Length; i++)
            {
                keys[i] = pairs[i].Key;
                values[i] = pairs[i].Value;
                keyValuePairs[i] = new array(new item[2] { pairs[i].Key, pairs[i].Value }).setPosition(startPos, endPos).setContext(context);
            }

            internalContext.symbolTable.set("length", new integer(pairs.Length));
            internalContext.symbolTable.set("keys", new array(keys));
            internalContext.symbolTable.set("values", new array(values));
            internalContext.symbolTable.set("pairs", new array(keyValuePairs));
            internalContext.symbolTable.set("as_boolean", new predefined_function("dictionary_as_boolean", asBoolean, new string[0] { }));
            internalContext.symbolTable.set("as_string", new predefined_function("dictionary_as_string", asString, new string[0] { }));
            internalContext.symbolTable.set("as_character_list", new predefined_function("dictionary_as_character_list", asCharList, new string[0] { }));
            return new runtimeResult().success(this);
        }

        private runtimeResult asBoolean(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            bool boolValue = isTrue(out error? error);
            if (error != null) return result.failure(error);
            return result.success(new boolean(boolValue));
        }

        private runtimeResult asString(context context, position[] positions) { return new runtimeResult().success(new @string(ToString())); }
        private runtimeResult asCharList(context context, position[] positions) { return new runtimeResult().success(new character_list(ToString())); }

        public override bool isTrue(out error? error) { error = null; return storedValue.Count > 0; }
        public override item copy() { return new dictionary(storedValue).setPosition(startPos, endPos).setContext(context); }

        public override string ToString()
        {
            KeyValuePair<item, item>[] values = storedValue.GetArray();
            string[] elementStrings = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                elementStrings[i] = $"{values[i].Key} : {values[i].Value}";
            return '{' + string.Join(", ", elementStrings) + '}';
        }

        public override int GetItemHashCode(out error? error)
        {
            error = null;
            int hashCode = 0;
            KeyValuePair<item, item>[] pairs = storedValue.GetArray();
            for (int i = 0; i < pairs.Length; i++)
            {
                int keyHash = pairs[i].Key.GetItemHashCode(out error);
                if (error != null) return 0;

                int valueHash = pairs[i].Value.GetItemHashCode(out error);
                if (error != null) return 0;

                int hash1 = ((keyHash << 5) + keyHash) ^ valueHash;
                hashCode = ((hashCode << 5) + hashCode) ^ hash1;
            }
            return hashCode;
        }
        public override bool ItemEquals(item obj, out error? error)
        {
            error = null;
            if (obj is dictionary)
            {
                int hash = GetItemHashCode(out error);
                if (error != null) return false;

                int otherHash = ((dictionary)obj).GetItemHashCode(out error);
                if (error != null) return false;

                return hash == otherHash;
            }
            return false;
        }
    }

    public abstract class baseFunction : item
    {
        public string name;
        public baseFunction(string name) : base()
        { this.name = name; }

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
                return result.failure(new runtimeError(startPos, endPos, RT_ARGS, $"{args.Length - argNames.Length} too many arguments passed into \"{name}\"", context));
            else if (args.Length < argNames.Length)
                return result.failure(new runtimeError(startPos, endPos, RT_ARGS, $"{argNames.Length - args.Length} too few arguments passed into \"{name}\"", context));

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

        public override bool isTrue(out error? error) { error = null; return true; }

        public override int GetItemHashCode(out error? error) { error = null; return ToString().GetHashCode(); }
    }

    public class predefined_function : baseFunction
    {
        private string[] argNames;
        private Func<context, position[], runtimeResult> function;

        public predefined_function(string name, Func<context, position[], runtimeResult> function, string[] argNames) : base(name)
        {
            this.function = function;
            this.argNames = argNames;
        }

        public override runtimeResult execute(item[] args)
        {
            runtimeResult result = new runtimeResult();
            context newContext = generateContext();

            result.register(checkAndPopulateArgs(argNames, args, newContext));
            if (result.shouldReturn()) return result;

            item returnValue = result.register(function.Invoke(newContext, new position[2] { startPos, endPos }));
            if (result.shouldReturn()) return result;

            return result.success(returnValue);
        }

        public override item copy() { return new predefined_function(name, function, argNames).setPosition(startPos, endPos).setContext(context); }

        public override bool ItemEquals(item obj, out error? error) { error = null; if (obj is predefined_function) return ToString() == obj.ToString(); return false; }
        public override string ToString() { return $"<predefined function <{name}>>"; }
    }

    public class builtin_function : baseFunction
    {
        private string[] argNames;
        public builtin_function(string name, string[] argNames) : base(name)
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

        private runtimeResult _simple_show(context context)
        {
            item value = context.symbolTable.get("message");
            if (value is @string)
                Console.Write(((@string)value).ToPureString());
            else if (value is character_list)
                Console.Write(((character_list)value).ToPureString());
            else
                Console.Write(value.ToString());

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
            if (OperatingSystem.IsLinux())
                Extras.system("clear");
            else
                Console.Clear();

            return new runtimeResult().success(new nothing());
        }

        private runtimeResult _hash(context context)
        {
            runtimeResult result = new runtimeResult();

            int hash = context.symbolTable.get("value").GetItemHashCode(out error? error);
            if (error != null) return result.failure(error);

            return result.success(new integer(hash));
        }

        private runtimeResult _type_of(context context)
        {
            item value = context.symbolTable.get("value");

            string type;
            if (value is @object)
                type = ((@object)value).name;
            else
                type = value.GetType().Name;

            return new runtimeResult().success(new @string(type));
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

            context runtimeContext = new context("<main>", globalPredefinedContext, new position(0, 0, 0, "<main>", ""), false);
            runtimeContext.symbolTable = new symbolTable(globalPredefinedContext.symbolTable);

            error? error = easyRun(Path.GetFileName(path), script, runtimeContext, out item? _);
            if (error != null)
                return result.failure(new runtimeRunError(startPos, endPos, $"Failed to execute script \"{path}\"", error.asString(), context));
            return result.success(new nothing());
        }

        public override item copy() { return new builtin_function(name, argNames).setPosition(startPos, endPos).setContext(context); }

        public override bool ItemEquals(item obj, out error? error) { error = null; if (obj is builtin_function) return ToString() == obj.ToString(); return false; }
        public override string ToString() { return $"<builtin function <{name}>>"; }
    }

    public class function : baseFunction
    {
        private node bodyNode;
        private string[] argNames;
        private bool shouldReturnNull;
        private interpreter interpreter;

        public function(string? name, node bodyNode, string[] argNames, bool shouldReturnNull) : base((name != null) ? name : "<anonymous>")
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
        public override bool ItemEquals(item obj, out error? error) { error = null; if (obj is function) return ToString() == obj.ToString(); return false; }
    }

    public class special : baseFunction
    {
        private node bodyNode;
        private string[] argNames;
        private bool shouldReturnNull;
        private interpreter interpreter;

        public special(string name, node bodyNode, string[] argNames, bool shouldReturnNull) : base(name)
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

        public override item copy() { return new special(name, bodyNode, argNames, shouldReturnNull).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<special function {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; if (obj is special) return ToString() == obj.ToString(); return false; }
    }

    public class @class : baseFunction
    {
        public string[] argNames { get; private set; }
        private node bodyNode;
        private @class? parent;

        public @class(string name, @class? inherit, node bodyNode, string[] argNames) : base(name)
        {
            this.bodyNode = bodyNode;
            this.argNames = argNames;
            this.parent = inherit;
        }

        public override runtimeResult execute(item[] args)
        {
            runtimeResult result = new runtimeResult();
            context internalContext = generateContext();

            result.register(checkAndPopulateArgs(argNames, args, internalContext));
            if (result.shouldReturn()) return result;

            if (parent != null)
            {
                item[] parentArgs = new item[parent.argNames.Length];
                for (int i = 0; i < parentArgs.Length; i++)
                    parentArgs[i] = args[Array.IndexOf(argNames, parent.argNames[i])];

                item parentObject = result.register(parent.execute(parentArgs));
                if (result.shouldReturn()) return result;

                internalContext.symbolTable.set("parent", parentObject);
            }

            @object object_ = (@object)new @object(name, internalContext).setPosition(startPos, endPos).setContext(context);
            result.register(object_.initialize(bodyNode));
            if (result.shouldReturn()) return result;

            return result.success(object_);
        }

        public override item copy() { return new @class(name, parent, bodyNode, argNames).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<class {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; if (obj is @class) return ToString() == obj.ToString(); return false; }
    }

    public class @object : baseFunction
    {
        public context internalContext { get; private set; }
        private interpreter interpreter;

        public @object(string name, context internalContext) : base(name)
        {
            this.internalContext = internalContext;
            this.interpreter = new interpreter();
        }

        public runtimeResult initialize(node body)
        {
            runtimeResult result = new runtimeResult();
            internalContext.symbolTable.set("this", this);

            result.register(interpreter.visit(body, internalContext));
            if (result.shouldReturn()) return result;

            return result.success(new nothing());
        }

        public override item setContext(context? context)
        {
            base.setContext(context);
            internalContext.parent = context;
            internalContext.symbolTable.parent = context.symbolTable;
            return this;
        }

        private item? getOutput(item func, item[] args, out error? error)
        {
            error = null;

            runtimeResult result = new runtimeResult();
            item output = result.register(func.execute(args));
            if (result.shouldReturn() && result.error == null) return new nothing().setContext(context);

            if (result.error != null)
            {
                error = result.error;
                return null;
            }
            return output;
        }

        public override item? compareEqual(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_equal");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareEqual(other, out error);
        }

        public override item? compareNotEqual(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_not_equal");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareNotEqual(other, out error);
        }

        public override item? compareAnd(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_and");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareAnd(other, out error);
        }

        public override item? compareOr(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_or");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareOr(other, out error);
        }

        public override item? checkIn(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("check_in");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.checkIn(other, out error);
        }

        public override item? bitwiseOrdTo(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("bitwise_or");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.bitwiseOrdTo(other, out error);
        }

        public override item? bitwiseXOrdTo(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("bitwise_xor");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.bitwiseXOrdTo(other, out error);
        }

        public override item? bitwiseAndedTo(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("bitwise_and");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.bitwiseAndedTo(other, out error);
        }

        public override item? bitwiseLeftShiftedTo(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("bitwise_left_shift");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.bitwiseLeftShiftedTo(other, out error);
        }

        public override item? bitwiseRightShiftedTo(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("bitwise_right_shift");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.bitwiseRightShiftedTo(other, out error);
        }

        public override item? bitwiseNotted(out error? error)
        {
            item? func = internalContext.symbolTable.get("bitwise_not");
            if (func != null && func is special)
                return getOutput(func, new item[0], out error);

            return base.bitwiseNotted(out error);
        }

        public override item? addedTo(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("addition");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.addedTo(other, out error);
        }

        public override item? subbedBy(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("subtraction");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.subbedBy(other, out error);
        }

        public override item? multedBy(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("multiplication");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.multedBy(other, out error);
        }

        public override item? divedBy(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("division");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.divedBy(other, out error);
        }

        public override item? modedBy(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("modulo");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.modedBy(other, out error);
        }

        public override item? powedBy(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("power");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.powedBy(other, out error);
        }

        public override item? compareLessThan(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_less_than");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareLessThan(other, out error);
        }

        public override item? compareGreaterThan(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_greater_than");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareGreaterThan(other, out error);
        }

        public override item? compareLessThanOrEqual(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_less_than_or_equal");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareLessThanOrEqual(other, out error);
        }

        public override item? compareGreaterThanOrEqual(item other, out error? error)
        {
            item? func = internalContext.symbolTable.get("compare_greater_than_or_equal");
            if (func != null && func is special)
                return getOutput(func, new item[] { other }, out error);

            return base.compareGreaterThanOrEqual(other, out error);
        }

        public override item? invert(out error? error)
        {
            item? func = internalContext.symbolTable.get("invert");
            if (func != null && func is special)
                return getOutput(func, new item[0], out error);

            return base.invert(out error);
        }

        public override bool isTrue(out error? error)
        {
            item? func = internalContext.symbolTable.get("is_true");
            if (func != null && func is special)
            {
                item? output = getOutput(func, new item[0], out error);
                if (error != null) return false;
                return output.isTrue(out error);
            }

            return base.isTrue(out error);
        }

        public override int GetItemHashCode(out error? error)
        {
            error = null;
            item? func = internalContext.symbolTable.get("hash");
            if (func != null && func is special)
            {
                item? output = getOutput(func, new item[0], out error);
                if (error != null) return 0;

                if (output is not integer)
                {
                    error = new runtimeError(startPos, endPos, RT_TYPE, "Return type of special function \"hash\" must be an integer", context);
                    return 0;
                }

                return ((integer)output).storedValue;
            }

            return base.GetItemHashCode(out error);
        }

        public override runtimeResult get(node node)
        {
            runtimeResult result = new runtimeResult();

            item value = result.register(interpreter.visit(node, internalContext));
            if (result.shouldReturn()) return result;
            return result.success(value);
        }

        public override runtimeResult set(string name, item variable)
        {
            internalContext.symbolTable.set(name, variable.copy());
            return new runtimeResult().success(variable);
        }

        public override item copy() { return new @object(name, internalContext).setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<object {name}>"; }
        public override bool ItemEquals(item obj, out error? error)
        {
            error = null;

            item? func = internalContext.symbolTable.get("equals");
            if (func != null && func is special)
            {
                item? output = getOutput(func, new item[1] { obj }, out error);
                if (error != null) return false;
                return output.isTrue(out error);
            }
            else if (obj is @object)
            {
                int hash = GetItemHashCode(out error);
                if (error != null) return false;

                int otherHash = ((@object)obj).GetItemHashCode(out error);
                if (error != null) return false;

                return hash == otherHash;
            }
            return false;
        }
    }
}