using System;
using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    public sealed class ObjClass : Obj
    {
        private const int InitialMethodSize = 256;

        public static ObjClass ClassClass;

        public int NumFields;

        public readonly ObjString Name;

        public Method[] Methods;

        public ObjClass Superclass;

        public bool IsSealed;

        // Creates a new class object as well as its associated metaclass.
        public ObjClass(ObjClass superclass, int numFields, ObjString name)
        {
            Methods = new Method[InitialMethodSize];
            Superclass = superclass;
            NumFields = numFields;
            Name = name;

            // Create the metaclass.
            ObjString metaclassName = MakeString(name + " metaclass");

            ObjClass metaclass = new ObjClass(0, metaclassName) { ClassObj = ClassClass };

            // Metaclasses always inherit Class and do not parallel the non-metaclass
            // hierarchy.
            metaclass.BindSuperclass(ClassClass);

            ClassObj = metaclass;
            BindSuperclass(superclass);
        }

        // Creates a new "raw" class. It has no metaclass or superclass whatsoever.
        // This is only used for bootstrapping the initial Object and Class classes,
        // which are a little special.
        public ObjClass(int numFields, ObjString name)
        {
            Methods = new Method[InitialMethodSize];
            NumFields = numFields;
            Name = name;
        }

        // Makes [superclass] the superclass of [subclass], and causes subclass to
        // inherit its methods. This should be called before any methods are defined
        // on subclass.
        public void BindSuperclass(ObjClass sc)
        {
            if (sc == null)
            {
                throw new Exception("Must have superclass.");
            }

            Superclass = sc;

            // Include the superclass in the total number of fields.
            NumFields += sc.NumFields;

            // Inherit methods from its superclass.
            Methods = new Method[sc.Methods.Length];
            sc.Methods.CopyTo(Methods,0);
        }

        public void BindMethod(int symbol, Method method)
        {
            if (symbol >= Methods.Length)
            {
                ResizeMethods(symbol);
            }
            Methods[symbol] = method;
        }

        private void ResizeMethods(int symbol)
        {
            int i = Methods.Length;
            while (i <= symbol)
                i *= 2;
            Method[] m = new Method[i];
            Methods.CopyTo(m,0);
            Methods = m;
        }
    }

    public delegate bool Primitive(SophieVM vm, Obj[] stack, int argStart);

    public enum MethodType
    {
        // A primitive method implemented in the VM.
        // this can directly manipulate the fiber's stack.
        Primitive,

        // A normal user-defined method.
        Block,

        // Special call type
        Call
    };

    public sealed class Method : Obj
    {
        public MethodType MType;

        // The method function itself. The [type] determines which field of the union
        // is used.
        public Primitive Primitive;

        // May be a [ObjFn] or [ObjClosure].
        public Obj Obj;
    } ;
}
