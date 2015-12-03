using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    // A first-class function object. A raw ObjFn can be used and invoked directly
    // if it has no upvalues (i.e. [numUpvalues] is zero). If it does use upvalues,
    // it must be wrapped in an [ObjClosure] first. The compiler is responsible for
    // emitting code to ensure that that happens.
    public sealed class ObjFn : Obj
    {
        public readonly byte[] Bytecode;
        public readonly int NumUpvalues;

        // Creates a new function object with the given code and constants. The new
        // function will take over ownership of [bytecode] and [sourceLines]. It will
        // copy [constants] into its own array.
        public ObjFn(ObjModule module,
            Obj[] constants,
            int numUpvalues, int arity,
            byte[] bytecode)
        {
            Bytecode = bytecode;
            Constants = constants;
            Module = module;
            NumUpvalues = numUpvalues;
            Arity = arity;

            ClassObj = SophieVM.FnClass;
        }

        public readonly ObjModule Module;

        public readonly Obj[] Constants;

        public readonly int Arity;
    }
}
