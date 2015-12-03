using System.Collections.Generic;
using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    public sealed class ObjComparer : IEqualityComparer<Obj>
    {
        public bool Equals(Obj x, Obj y)
        {
            return x != null && Obj.Equals(x, y);
        }

        public int GetHashCode(Obj obj)
        {
            return obj.GetHashCode();
        }
    }

    // Base class for all heap-allocated objects.
    public class Obj
    {
        public static readonly Obj Null = new Obj(ObjType.Null);
        public static readonly Obj False = new Obj(ObjType.False);
        public static readonly Obj True = new Obj(ObjType.True);
        public static readonly Obj Undefined = new Obj(ObjType.Undefined);

        // The object's class.
        public ObjClass ClassObj;
        public readonly ObjType Type;
        public readonly double Num;

        protected Obj()
        {
            Type = ObjType.Obj;
        }

        private Obj(ObjType t)
        {
            Type = t;
        }

        public Obj(double n)
        {
            Type = ObjType.Num;
            Num = n;
        }

        public static Obj MakeString(string s)
        {
            return new ObjString(s);
        }

        public static Obj Bool(bool b)
        {
            return b ? True : False;
        }

        public override int GetHashCode()
        {
            switch (Type)
            {
                case ObjType.Num:
                    return Num.GetHashCode();
                case ObjType.Obj:
                    return base.GetHashCode();
                default:
                    return Type.GetHashCode();
            }
        }

        public ObjClass GetClass()
        {
            switch (Type)
            {
                case ObjType.True:
                case ObjType.False:
                    return SophieVM.BoolClass;
                case ObjType.Num:
                    return SophieVM.NumClass;
                case ObjType.Null:
                case ObjType.Undefined:
                    return SophieVM.NullClass;
                default:
                    return ClassObj;
            }
        }

        // Returns true if [a] and [b] are equivalent. Immutable values (null, bools,
        // numbers, ranges, and strings) are equal if they have the same data. All
        // other values are equal if they are identical objects.
        public static bool Equals(Obj a, Obj b)
        {
            if (a == b) return true;
            if (a.Type != b.Type) return false;
            if (a.Type == ObjType.Num) return a.Num == b.Num;


            // If we get here, it's only possible for two heap-allocated immutable objects
            // to be equal.
            if (a.Type != ObjType.Obj) return true;

            // Must be the same type.
            if (a.GetType() != b.GetType()) return false;

            ObjString aString = a as ObjString;
            if (aString != null)
            {
                ObjString bString = (ObjString)b;
                return aString.Str.Equals(bString.Str);
            }

            ObjRange aRange = a as ObjRange;
            if (aRange != null)
            {
                ObjRange bRange = (ObjRange)b;
                return ObjRange.Equals(aRange, bRange);
            }
            // All other types are only equal if they are same, which they aren't if
            // we get here.
            return false;
        }

    }

    public enum ObjType
    {
        False,
        Null,
        Num,
        True,
        Undefined,
        Obj
    };
}
