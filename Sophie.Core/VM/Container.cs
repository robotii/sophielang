using System.Collections.Generic;
using Sophie.Core.Objects;

namespace Sophie.Core.VM
{
    public class ContainerComparer : IEqualityComparer<Container>
    {
        public bool Equals(Container x, Container y)
        {
            return x != null && Container.Equals(x, y);
        }

        public int GetHashCode(Container obj)
        {
            return obj.GetHashCode();
        }
    }

    public class Container
    {
        public static Container Null = new Container(ContainerType.Null);

        public Container()
        {
            Type = ContainerType.Undefined;
        }

        public Container(ContainerType t)
        {
            Type = t;
        }

        public Container(Container v)
        {
            Type = v.Type;
            Num = v.Num;
            Obj = v.Obj;
        }

        public Container(Obj o)
        {
            Type = ContainerType.Obj;
            Obj = o;
        }

        public Container(double n)
        {
            Type = ContainerType.Num;
            Num = n;
        }

        public Container(string s)
        {
            Type = ContainerType.Obj;
            Obj = new ObjString(s);
        }

        public Container(bool b)
        {
            Type = b ? ContainerType.True : ContainerType.False;
        }

        public readonly ContainerType Type;
        public readonly double Num;
        public readonly Obj Obj;

        public ObjClass GetClass()
        {
            switch (Type)
            {
                case ContainerType.True:
                case ContainerType.False:
                    return SophieVM.BoolClass;
                case ContainerType.Num:
                    return SophieVM.NumClass;
                case ContainerType.Null:
                case ContainerType.Undefined:
                    return SophieVM.NullClass;
                default:
                    return Obj.ClassObj;
            }
        }

        // Returns true if [a] and [b] are equivalent. Immutable values (null, bools,
        // numbers, ranges, and strings) are equal if they have the same data. All
        // other values are equal if they are identical objects.
        public static bool Equals(Container a, Container b)
        {
            if (a.Type != b.Type) return false;
            if (a.Type == ContainerType.Num) return a.Num == b.Num;
            if (a.Obj == b.Obj) return true;

            // If we get here, it's only possible for two heap-allocated immutable objects
            // to be equal.
            if (a.Type != ContainerType.Obj) return false;

            Obj aObj = a.Obj;
            Obj bObj = b.Obj;

            // Must be the same type.
            if (aObj.Type != bObj.Type) return false;

            if (aObj.Type == ObjType.String)
            {
                ObjString aString = (ObjString)aObj;
                ObjString bString = (ObjString)bObj;
                return aString.Value.Equals(bString.Value);
            }

            if (aObj.Type == ObjType.Range)
            {
                ObjRange aRange = (ObjRange)aObj;
                ObjRange bRange = (ObjRange)bObj;
                return ObjRange.Equals(aRange, bRange);
            }
            // All other types are only equal if they are same, which they aren't if
            // we get here.
            return false;
        }

        public override int GetHashCode()
        {
            switch (Type)
            {
                case ContainerType.Num:
                    return Num.GetHashCode();
                case ContainerType.Obj:
                    return Obj.GetHashCode();
                default:
                    return Type.GetHashCode();
            }
        }
    }

    public enum ContainerType
    {
        False,
        Null,
        Num,
        True,
        Undefined,
        Obj
    };
}
