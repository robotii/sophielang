using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    // An instance of a first-class function and the environment it has closed over.
    // Unlike [ObjFn], this has captured the upvalues that the function accesses.
    public sealed class ObjClosure : Obj
    {
        // The function that this closure is an instance of.

        // The upvalues this function has closed over.

        // Creates a new closure object that invokes [fn]. Allocates room for its
        // upvalues, but assumes outside code will populate it.
        public ObjClosure(ObjFn fn)
        {
            Function = fn;
            Upvalues = new ObjUpvalue[fn.NumUpvalues];
            ClassObj = SophieVM.FnClass;
        }

        public readonly ObjFn Function;

        public readonly ObjUpvalue[] Upvalues;
    }
}
