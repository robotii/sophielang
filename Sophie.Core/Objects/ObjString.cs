using System;
using System.Collections.Generic;
using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    public class ObjString : Obj
    {
        private static readonly List<ObjString> strings = new List<ObjString>();
        private static bool initCompleted;

        public static void InitClass()
        {
            foreach (ObjString s in strings)
            {
                s.ClassObj = SophieVM.StringClass;
            }
            initCompleted = true;
            strings.Clear();
        }

        // Inline array of the string's bytes followed by a null terminator.

        public ObjString(string s)
        {
            Value = s;
            ClassObj = SophieVM.StringClass;
            if (!initCompleted)
                strings.Add(this);
            Type = ObjType.String;
        }

        public readonly string Value;

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        // Creates a new string containing the UTF-8 encoding of [value].
        public static Container FromCodePoint(int v)
        {
            return new Container("" + Convert.ToChar(v));
        }

        // Creates a new string containing the code point in [string] starting at byte
        // [index]. If [index] points into the middle of a UTF-8 sequence, returns an
        // empty string.
        public Container CodePointAt(int index)
        {
            return index > Value.Length ? new Container() : new Container(Value[index]);
        }
    }
}
