﻿using ezrSquared.Values;
using System.Collections.Generic;

namespace ezrSquared.General
{
    public enum TOKENTYPE : int
    {
        INT,
        FLOAT,
        STRING,
        CHARLIST,
        ID,
        KEY,
        QEY,
        PLUS,
        MINUS,
        MUL,
        DIV,
        MOD,
        POW,
        BITAND,
        BITOR,
        BITXOR,
        BITNOT,
        BITLSHIFT,
        BITRSHIFT,
        COLON,
        ASSIGNPLUS,
        ASSIGNMINUS,
        ASSIGNMUL,
        ASSIGNDIV,
        ASSIGNMOD,
        ASSIGNPOW,
        ASSIGNBITAND,
        ASSIGNBITOR,
        ASSIGNBITXOR,
        ASSIGNBITLSHIFT,
        ASSIGNBITRSHIFT,
        LPAREN,
        RPAREN,
        LSQUARE,
        RSQUARE,
        LCURLY,
        RCURLY,
        ISEQUAL,
        NOTEQUAL,
        LESSTHAN,
        GREATERTHAN,
        LESSTHANOREQUAL,
        GREATERTHANOREQUAL,
        COMMA,
        PERIOD,
        ARROW,
        NEWLINE,
        ENDOFFILE
    }

    public class position
    {
        public int index;
        public int line;
        public int column;
        public string file;
        public string text;
        private int prevLineColumn = 0;

        public position(int index, int line, int column, string file, string text)
        {
            this.index = index;
            this.line = line;
            this.column = column;
            this.file = file;
            this.text = text;
        }

        public position advance(char? currentChar = null)
        {
            index++;
            column++;

            if (currentChar == '\n')
            {
                line++;
                prevLineColumn = column;
                column = 0;
            }

            return this;
        }

        public position reverse(char? currentChar, int count = 1)
        {
            index--;
            column--;

            if (currentChar == '\n')
            {
                line--;
                column = prevLineColumn;
            }

            return this;
        }

        public position copy() { return new position(index, line, column, file, text); }
    }

    public class token
    {
        public TOKENTYPE type;
        public object? value;
        public position startPos;
        public position endPos;

        public token(TOKENTYPE type, position startPos, position? endPos = null)
        {
            this.type = type;
            this.value = null;

            this.startPos = startPos.copy();
            if (endPos != null)
                this.endPos = endPos.copy();
            else
            {
                this.endPos = startPos.copy();
                this.endPos.advance();
            }
        }

        public token(TOKENTYPE type, object value, position startPos, position? endPos = null)
        {
            this.type = type;
            this.value = value;

            this.startPos = startPos.copy();
            if (endPos != null)
                this.endPos = endPos.copy();
            else
            {
                this.endPos = startPos.copy();
                this.endPos.advance();
            }
        }

        public bool matchString(TOKENTYPE type, string value)
        {
            if (this.type == (TOKENTYPE.INT | TOKENTYPE.FLOAT) || this.value == null) return false;

            return this.type == type && (string)this.value == value;
        }
    }

    public class context
    {
        public string name;
        public context? parent;
        public position? parentEntryPos;
        public symbolTable? symbolTable;
        public bool locked;

        public context(string name, context? parent = null, position? parentEntryPos = null, bool locked = false)
        {
            this.name = name;
            this.parent = parent;
            this.parentEntryPos = parentEntryPos;
            this.locked = locked;
            this.symbolTable = null;
        }
    }

    public class symbolTable
    {
        public symbolTable? parent;

        private Dictionary<string, item> symbols;

        public symbolTable(symbolTable? parent = null)
        {
            this.symbols = new Dictionary<string, item>();
            this.parent = parent;
        }

        public item? get(string name)
        {
            if (symbols.TryGetValue(name, out item? value))
                return value;
            else if (parent != null)
                return parent.get(name);
            return null;
        }

        public void set(string name, item value)
        {
            if (symbols.ContainsKey(name))
                symbols[name] = value;
            else
                symbols.Add(name, value);
        }

        public void remove(string name)
        {
            if (symbols.ContainsKey(name))
                symbols.Remove(name);
        }
    }
}
