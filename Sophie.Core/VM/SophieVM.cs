using System;
using System.Collections.Generic;
using Sophie.Core.Bytecode;
using Sophie.Core.Library;
using Sophie.Core.Objects;

namespace Sophie.Core.VM
{
    public delegate string SophieLoadModuleFn(string name);

    public enum InterpretResult
    {
        Success = 0,
        CompileError = 65,
        RuntimeError = 70
    } ;

    public sealed class SophieVM
    {
        public static ObjClass BoolClass;
        public static ObjClass ClassClass;
        public static ObjClass FiberClass;
        public static ObjClass FnClass;
        public static ObjClass ListClass;
        public static ObjClass MapClass;
        public static ObjClass NullClass;
        public static ObjClass NumClass;
        public static ObjClass ObjectClass;
        public static ObjClass RangeClass;
        public static ObjClass StringClass;

        // The fiber that is currently running.
        public ObjFiber Fiber;

        readonly ObjMap _modules;

        public SophieVM()
        {
            MethodNames = new List<string>();
            ObjString name = new ObjString("core");

            // Implicitly create a "core" module for the built in libraries.
            ObjModule coreModule = new ObjModule(name);

            _modules = new ObjMap();
            _modules.Set(Obj.Null, coreModule);

            CoreLibrary core = new CoreLibrary(this);
            core.InitializeCore();

            // Load in System functions
            Library.System.LoadSystemLibrary(this);
        }

        public readonly List<string> MethodNames;

        public Compiler Compiler { get; set; }

        public SophieLoadModuleFn LoadModuleFn { get; set; }

        // Defines [methodValue] as a method on [classObj].
        private static Obj BindMethod(MethodType methodType, int symbol, ObjClass classObj, Obj methodContainer)
        {
            ObjFn methodFn = methodContainer as ObjFn ?? ((ObjClosure)methodContainer).Function;

            // Methods are always bound against the class, and not the metaclass, even
            // for static methods, because static methods don't have instance fields
            // anyway.
            Compiler.BindMethodCode(classObj, methodFn);

            Method method = new Method { MType = MethodType.Block, Obj = methodContainer };

            if (methodType == MethodType.Static)
                classObj = classObj.ClassObj;

            //classObj.Methods[symbol] = method;
            classObj.BindMethod(symbol, method);
            return Obj.Null;
        }

        // Creates a string containing an appropriate method not found error for a
        // method with [symbol] on [classObj].
        static void MethodNotFound(SophieVM vm, ObjClass classObj, int symbol)
        {
            vm.Fiber.Error = Obj.MakeString(string.Format("{0} does not implement '{1}'.", classObj.Name, vm.MethodNames[symbol]));
        }

        // Looks up the previously loaded module with [name].
        // Returns null if no module with that name has been loaded.
        private ObjModule GetModule(Obj name)
        {
            Obj moduleContainer = _modules.Get(name);
            return moduleContainer == Obj.Undefined ? null : moduleContainer as ObjModule;
        }

        // Looks up the core module in the module map.
        private ObjModule GetCoreModule()
        {
            return GetModule(Obj.Null);
        }

        private ObjFiber LoadModule(Obj name, string source)
        {
            ObjModule module = GetModule(name);

            // See if the module has already been loaded.
            if (module == null)
            {
                module = new ObjModule(name as ObjString);

                // Store it in the VM's module registry so we don't load the same module
                // multiple times.
                _modules.Set(name, module);

                // Implicitly import the core module.
                ObjModule coreModule = GetCoreModule();
                foreach (ModuleVariable t in coreModule.Variables)
                {
                    DefineVariable(module, t.Name, t.Container);
                }
            }

            ObjFn fn = Compiler.Compile(this, module, name.ToString(), source, true);
            if (fn == null)
            {
                // TODO: Should we still store the module even if it didn't compile?
                return null;
            }

            ObjFiber moduleFiber = new ObjFiber(fn);


            // Return the fiber that executes the module.
            return moduleFiber;
        }

        private Obj ImportModule(Obj name)
        {
            // If the module is already loaded, we don't need to do anything.
            if (_modules.Get(name) != Obj.Undefined) return Obj.Null;

            // Load the module's source code from the embedder.
            string source = LoadModuleFn(name.ToString());
            if (source == null)
            {
                // Couldn't load the module.
                return new ObjString(string.Format("Could not find module '{0}'.", name));
            }

            ObjFiber moduleFiber = LoadModule(name, source);

            // Return the fiber that executes the module.
            return moduleFiber;
        }


        private bool ImportVariable(Obj moduleName, Obj variableName, out Obj result)
        {
            ObjModule module = GetModule(moduleName);
            if (module == null)
            {
                result = Obj.MakeString("Could not load module");
                return false; // Should only look up loaded modules
            }

            ObjString variable = variableName as ObjString;
            if (variable == null)
            {
                result = Obj.MakeString("Variable name must be a string");
                return false;
            }

            int variableEntry = module.Variables.FindIndex(v => v.Name == variable.ToString());

            // It's a runtime error if the imported variable does not exist.
            if (variableEntry != -1)
            {
                result = module.Variables[variableEntry].Container;
                return true;
            }

            result = Obj.MakeString(string.Format("Could not find a variable named '{0}' in module '{1}'.", variableName, moduleName));
            return false;
        }

        // Verifies that [superclass] is a valid object to inherit from. That means it
        // must be a class and cannot be the class of any built-in type.
        //
        // If successful, returns null. Otherwise, returns a string for the runtime
        // error message.
        private static Obj ValidateSuperclass(Obj name, Obj superclassContainer)
        {
            // Make sure the superclass is a class.
            if (!(superclassContainer is ObjClass))
            {
                return Obj.MakeString("Must inherit from a class.");
            }

            // Make sure it doesn't inherit from a sealed built-in type. Primitive methods
            // on these classes assume the instance is one of the other Obj___ types and
            // will fail horribly if it's actually an ObjInstance.
            ObjClass superclass = superclassContainer as ObjClass;

            return superclass.IsSealed ? Obj.MakeString(string.Format("{0} cannot inherit from {1}.", name as ObjString, (superclass.Name))) : null;
        }

        // The main bytecode interpreter loop. This is where the magic happens. It is
        // also, as you can imagine, highly performance critical. Returns `true` if the
        // fiber completed without error.
        private bool RunInterpreter()
        {
            Instruction instruction;
            int index;

            /* Load Frame */
            CallFrame frame = Fiber.Frames[Fiber.NumFrames - 1];
            int ip = frame.Ip;
            int stackStart = frame.StackStart;
            Obj[] stack = Fiber.Stack;

            ObjFn fn = frame.Fn as ObjFn ?? ((ObjClosure)frame.Fn).Function;
            byte[] bytecode = fn.Bytecode;

            while (true)
            {
                switch (instruction = (Instruction)bytecode[ip++])
                {
                    case Instruction.LoadLocal0:
                    case Instruction.LoadLocal1:
                    case Instruction.LoadLocal2:
                    case Instruction.LoadLocal3:
                    case Instruction.LoadLocal4:
                    case Instruction.LoadLocal5:
                    case Instruction.LoadLocal6:
                    case Instruction.LoadLocal7:
                    case Instruction.LoadLocal8:
                        index = stackStart + (int)instruction; // LOAD_LOCAL_0 has code 0
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop++] = stack[index];
                        break;

                    case Instruction.LoadLocal:
                        index = stackStart + bytecode[ip++];
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop++] = stack[index];
                        break;

                    case Instruction.LoadFieldThis:
                        {
                            byte field = bytecode[ip++];
                            Obj receiver = stack[stackStart];
                            ObjInstance instance = receiver as ObjInstance;
                            if (Fiber.StackTop >= Fiber.Capacity)
                                Fiber.IncreaseStack();
                            stack[Fiber.StackTop++] = instance.Fields[field];
                            break;
                        }

                    case Instruction.Pop:
                        Fiber.StackTop--;
                        break;

                    case Instruction.Dup:
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop] = stack[Fiber.StackTop - 1];
                        Fiber.StackTop++;
                        break;

                    case Instruction.Null:
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop++] = Obj.Null;
                        break;

                    case Instruction.False:
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop++] = Obj.False;
                        break;

                    case Instruction.True:
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop++] = Obj.True;
                        break;

                    case Instruction.Call0:
                    case Instruction.Call1:
                    case Instruction.Call2:
                    case Instruction.Call3:
                    case Instruction.Call4:
                    case Instruction.Call5:
                    case Instruction.Call6:
                    case Instruction.Call7:
                    case Instruction.Call8:
                    case Instruction.Call9:
                    case Instruction.Call10:
                    case Instruction.Call11:
                    case Instruction.Call12:
                    case Instruction.Call13:
                    case Instruction.Call14:
                    case Instruction.Call15:
                    case Instruction.Call16:
                    // Handle Super calls
                    case Instruction.Super0:
                    case Instruction.Super1:
                    case Instruction.Super2:
                    case Instruction.Super3:
                    case Instruction.Super4:
                    case Instruction.Super5:
                    case Instruction.Super6:
                    case Instruction.Super7:
                    case Instruction.Super8:
                    case Instruction.Super9:
                    case Instruction.Super10:
                    case Instruction.Super11:
                    case Instruction.Super12:
                    case Instruction.Super13:
                    case Instruction.Super14:
                    case Instruction.Super15:
                    case Instruction.Super16:
                        {
                            int numArgs = instruction - (instruction >= Instruction.Super0 ? Instruction.Super0 : Instruction.Call0) + 1;
                            int symbol = (bytecode[ip] << 8) + bytecode[ip + 1];
                            ip += 2;

                            // The receiver is the first argument.
                            int argStart = Fiber.StackTop - numArgs;
                            Obj receiver = stack[argStart];
                            ObjClass classObj;

                            if (instruction < Instruction.Super0)
                            {
                                if (receiver.Type == ObjType.Obj)
                                    classObj = receiver.ClassObj;
                                else if (receiver.Type == ObjType.Num)
                                    classObj = NumClass;
                                else if (receiver == Obj.True || receiver == Obj.False)
                                    classObj = BoolClass;
                                else
                                    classObj = NullClass;
                            }
                            else
                            {
                                // The superclass is stored in a constant.
                                classObj = fn.Constants[(bytecode[ip] << 8) + bytecode[ip + 1]] as ObjClass;
                                ip += 2;
                            }

                            // If the class's method table doesn't include the symbol, bail.
                            Method method = symbol < classObj.Methods.Length ? classObj.Methods[symbol] : null;

                            if (method == null)
                            {
                                /* Method not found */
                                frame.Ip = ip;
                                MethodNotFound(this, classObj, symbol);
                                if (!HandleRuntimeError())
                                    return false;
                                frame = Fiber.Frames[Fiber.NumFrames - 1];
                                ip = frame.Ip;
                                stackStart = frame.StackStart;
                                stack = Fiber.Stack;
                                fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                bytecode = fn.Bytecode;
                                break;
                            }

                            if (method.MType == MethodType.Primitive)
                            {
                                // After calling this, the result will be in the first arg slot.
                                if (method.Primitive(this, stack, argStart))
                                {
                                    Fiber.StackTop = argStart + 1;
                                }
                                else
                                {
                                    frame.Ip = ip;

                                    if (Fiber.Error != null && Fiber.Error != Obj.Null)
                                    {
                                        if (!HandleRuntimeError())
                                            return false;
                                    }
                                    else
                                    {
                                        // If we don't have a fiber to switch to, stop interpreting.
                                        if (stack[argStart] == Obj.Null)
                                            return true;
                                        Fiber = stack[argStart] as ObjFiber;
                                        if (Fiber == null)
                                            return false;
                                    }

                                    /* Load Frame */
                                    frame = Fiber.Frames[Fiber.NumFrames - 1];
                                    ip = frame.Ip;
                                    stackStart = frame.StackStart;
                                    stack = Fiber.Stack;
                                    fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                    bytecode = fn.Bytecode;
                                }
                                break;
                            }

                            frame.Ip = ip;

                            if (method.MType == MethodType.Block)
                            {
                                receiver = method.Obj;
                            }
                            else if (!CheckArity(stack, numArgs, argStart))
                            {
                                if (!HandleRuntimeError())
                                    return false;

                                frame = Fiber.Frames[Fiber.NumFrames - 1];
                                ip = frame.Ip;
                                stackStart = frame.StackStart;
                                stack = Fiber.Stack;
                                fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                bytecode = fn.Bytecode;
                                break;
                            }

                            Fiber.Frames.Add(frame = new CallFrame { Fn = receiver, StackStart = argStart, Ip = 0 });
                            Fiber.NumFrames++;
                            /* Load Frame */
                            ip = 0;
                            stackStart = argStart;
                            fn = (receiver as ObjFn) ?? (receiver as ObjClosure).Function;
                            bytecode = fn.Bytecode;
                            break;
                        }

                    case Instruction.StoreLocal:
                        index = stackStart + bytecode[ip++];
                        stack[index] = stack[Fiber.StackTop - 1];
                        break;

                    case Instruction.Constant:
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop++] = fn.Constants[(bytecode[ip] << 8) + bytecode[ip + 1]];
                        ip += 2;
                        break;

                    case Instruction.LoadUpvalue:
                        {
                            if (Fiber.StackTop >= Fiber.Capacity)
                                stack = Fiber.IncreaseStack();
                            stack[Fiber.StackTop++] = ((ObjClosure)frame.Fn).Upvalues[bytecode[ip++]].Container;
                            break;
                        }

                    case Instruction.StoreUpvalue:
                        {
                            ObjUpvalue[] upvalues = ((ObjClosure)frame.Fn).Upvalues;
                            upvalues[bytecode[ip++]].Container = stack[Fiber.StackTop - 1];
                            break;
                        }

                    case Instruction.LoadModuleVar:
                        if (Fiber.StackTop >= Fiber.Capacity)
                            stack = Fiber.IncreaseStack();
                        stack[Fiber.StackTop++] = fn.Module.Variables[(bytecode[ip] << 8) + bytecode[ip + 1]].Container;
                        ip += 2;
                        break;

                    case Instruction.StoreModuleVar:
                        fn.Module.Variables[(bytecode[ip] << 8) + bytecode[ip + 1]].Container = stack[Fiber.StackTop - 1];
                        ip += 2;
                        break;

                    case Instruction.StoreFieldThis:
                        {
                            byte field = bytecode[ip++];
                            Obj receiver = stack[stackStart];
                            ObjInstance instance = receiver as ObjInstance;
                            instance.Fields[field] = stack[Fiber.StackTop - 1];
                            break;
                        }

                    case Instruction.LoadField:
                        {
                            byte field = bytecode[ip++];
                            Obj receiver = stack[--Fiber.StackTop];
                            ObjInstance instance = receiver as ObjInstance;
                            if (Fiber.StackTop >= Fiber.Capacity)
                                stack = Fiber.IncreaseStack();
                            stack[Fiber.StackTop++] = instance.Fields[field];
                            break;
                        }

                    case Instruction.StoreField:
                        {
                            byte field = bytecode[ip++];
                            Obj receiver = stack[--Fiber.StackTop];
                            ObjInstance instance = receiver as ObjInstance;
                            instance.Fields[field] = stack[Fiber.StackTop - 1];
                            break;
                        }

                    case Instruction.Jump:
                        {
                            int offset = (bytecode[ip] << 8) + bytecode[ip + 1];
                            ip += offset + 2;
                            break;
                        }

                    case Instruction.Loop:
                        {
                            // Jump back to the top of the loop.
                            int offset = (bytecode[ip] << 8) + bytecode[ip + 1];
                            ip += 2;
                            ip -= offset;
                            break;
                        }

                    case Instruction.JumpIf:
                        {
                            int offset = (bytecode[ip] << 8) + bytecode[ip + 1];
                            ip += 2;
                            Obj condition = stack[--Fiber.StackTop];

                            if (condition == Obj.False || condition == Obj.Null) ip += offset;
                            break;
                        }

                    case Instruction.And:
                        {
                            int offset = (bytecode[ip] << 8) + bytecode[ip + 1];
                            ip += 2;
                            ObjType condition = stack[Fiber.StackTop - 1].Type;

                            switch (condition)
                            {
                                case ObjType.Null:
                                case ObjType.False:
                                    ip += offset;
                                    break;
                                default:
                                    Fiber.StackTop--;
                                    break;
                            }
                            break;
                        }

                    case Instruction.Or:
                        {
                            int offset = (bytecode[ip] << 8) + bytecode[ip + 1];
                            ip += 2;
                            Obj condition = stack[Fiber.StackTop - 1];

                            switch (condition.Type)
                            {
                                case ObjType.Null:
                                case ObjType.False:
                                    Fiber.StackTop--;
                                    break;
                                default:
                                    ip += offset;
                                    break;
                            }
                            break;
                        }

                    case Instruction.CloseUpvalue:
                        Fiber.CloseUpvalue();
                        Fiber.StackTop--;
                        break;

                    case Instruction.Return:
                        {
                            Fiber.Frames.RemoveAt(--Fiber.NumFrames);
                            Obj result = stack[--Fiber.StackTop];
                            // Close any upvalues still in scope.
                            if (Fiber.StackTop > stackStart)
                            {
                                Obj first = stack[stackStart];
                                while (Fiber.OpenUpvalues != null &&
                                       Fiber.OpenUpvalues.Container != first)
                                {
                                    Fiber.CloseUpvalue();
                                }
                                Fiber.CloseUpvalue();
                            }

                            // If the fiber is complete, end it.
                            if (Fiber.NumFrames == 0)
                            {
                                // If this is the main fiber, we're done.
                                if (Fiber.Caller == null) return true;

                                // We have a calling fiber to resume.
                                Fiber = Fiber.Caller;
                                stack = Fiber.Stack;
                                // Store the result in the resuming fiber.
                                stack[Fiber.StackTop - 1] = result;
                            }
                            else
                            {
                                // Discard the stack slots for the call frame (leaving one slot for the result).
                                Fiber.StackTop = stackStart + 1;

                                // Store the result of the block in the first slot, which is where the
                                // caller expects it.
                                stack[Fiber.StackTop - 1] = result;
                            }

                            /* Load Frame */
                            frame = Fiber.Frames[Fiber.NumFrames - 1];
                            ip = frame.Ip;
                            stackStart = frame.StackStart;
                            fn = frame.Fn as ObjFn ?? (frame.Fn as ObjClosure).Function;
                            bytecode = fn.Bytecode;
                            break;
                        }

                    case Instruction.Closure:
                        {
                            ObjFn prototype = fn.Constants[(bytecode[ip] << 8) + bytecode[ip + 1]] as ObjFn;
                            ip += 2;

                            // Create the closure and push it on the stack before creating upvalues
                            // so that it doesn't get collected.
                            ObjClosure closure = new ObjClosure(prototype);
                            if (Fiber.StackTop >= Fiber.Capacity)
                                stack = Fiber.IncreaseStack();
                            stack[Fiber.StackTop++] = closure;

                            // Capture upvalues.
                            for (int i = 0; i < prototype.NumUpvalues; i++)
                            {
                                byte isLocal = bytecode[ip++];
                                index = bytecode[ip++];
                                if (isLocal > 0)
                                {
                                    // Make an new upvalue to close over the parent's local variable.
                                    closure.Upvalues[i] = Fiber.CaptureUpvalue(stackStart + index);
                                }
                                else
                                {
                                    // Use the same upvalue as the current call frame.
                                    closure.Upvalues[i] = ((ObjClosure)frame.Fn).Upvalues[index];
                                }
                            }

                            break;
                        }

                    case Instruction.Class:
                        {
                            Obj name = stack[Fiber.StackTop - 2];
                            ObjClass superclass = ObjectClass;

                            // Use implicit Object superclass if none given.
                            if (stack[Fiber.StackTop - 1] != Obj.Null)
                            {
                                Obj error = ValidateSuperclass(name, stack[Fiber.StackTop - 1]);
                                if (error != null)
                                {
                                    frame.Ip = ip;
                                    RUNTIME_ERROR(Fiber, error);
                                    if (Fiber == null)
                                        return false;
                                    /* Load Frame */
                                    frame = Fiber.Frames[Fiber.NumFrames - 1];
                                    ip = frame.Ip;
                                    stackStart = frame.StackStart;
                                    stack = Fiber.Stack;
                                    fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                    bytecode = fn.Bytecode;
                                    break;
                                }
                                superclass = stack[Fiber.StackTop - 1] as ObjClass;
                            }

                            int numFields = bytecode[ip++];

                            Obj classObj = new ObjClass(superclass, numFields, name as ObjString);

                            // Don't pop the superclass and name off the stack until the subclass is
                            // done being created, to make sure it doesn't get collected.
                            Fiber.StackTop -= 2;

                            // Now that we know the total number of fields, make sure we don't overflow.
                            if (superclass.NumFields + numFields > Compiler.MaxFields)
                            {
                                frame.Ip = ip;
                                RUNTIME_ERROR(Fiber, Obj.MakeString(string.Format("Class '{0}' may not have more than 255 fields, including inherited ones.", name)));
                                if (Fiber == null)
                                    return false;
                                /* Load Frame */
                                frame = Fiber.Frames[Fiber.NumFrames - 1];
                                ip = frame.Ip;
                                stackStart = frame.StackStart;
                                stack = Fiber.Stack;
                                fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                bytecode = fn.Bytecode;
                                break;
                            }

                            if (Fiber.StackTop >= Fiber.Capacity)
                                stack = Fiber.IncreaseStack();
                            stack[Fiber.StackTop++] = classObj;
                            break;
                        }

                    case Instruction.MethodInstance:
                    case Instruction.MethodStatic:
                        {
                            int symbol = (bytecode[ip] << 8) + bytecode[ip + 1];
                            ip += 2;
                            ObjClass classObj = stack[Fiber.StackTop - 1] as ObjClass;
                            Obj method = stack[Fiber.StackTop - 2];
                            MethodType methodType = instruction == Instruction.MethodInstance ? MethodType.None : MethodType.Static;
                            Obj error = BindMethod(methodType, symbol, classObj, method);
                            if ((error is ObjString))
                            {
                                frame.Ip = ip;
                                RUNTIME_ERROR(Fiber, error);
                                if (Fiber == null)
                                    return false;
                                /* Load Frame */
                                frame = Fiber.Frames[Fiber.NumFrames - 1];
                                ip = frame.Ip;
                                stackStart = frame.StackStart;
                                stack = Fiber.Stack;
                                fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                bytecode = fn.Bytecode;
                                break;
                            }
                            Fiber.StackTop -= 2;
                            break;
                        }

                    case Instruction.LoadModule:
                        {
                            Obj name = fn.Constants[(bytecode[ip] << 8) + bytecode[ip + 1]];
                            ip += 2;
                            Obj result = ImportModule(name);

                            // If it returned a string, it was an error message.
                            if ((result is ObjString))
                            {
                                frame.Ip = ip;
                                RUNTIME_ERROR(Fiber, result);
                                if (Fiber == null)
                                    return false;
                                /* Load Frame */
                                frame = Fiber.Frames[Fiber.NumFrames - 1];
                                ip = frame.Ip;
                                stackStart = frame.StackStart;
                                stack = Fiber.Stack;
                                fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                bytecode = fn.Bytecode;
                                break;
                            }

                            // Make a slot that the module's fiber can use to store its result in.
                            // It ends up getting discarded, but CODE_RETURN expects to be able to
                            // place a value there.
                            if (Fiber.StackTop >= Fiber.Capacity)
                                stack = Fiber.IncreaseStack();
                            stack[Fiber.StackTop++] = Obj.Null;

                            // If it returned a fiber to execute the module body, switch to it.
                            if (result is ObjFiber)
                            {
                                // Return to this module when that one is done.
                                (result as ObjFiber).Caller = Fiber;

                                frame.Ip = ip;
                                Fiber = (result as ObjFiber);
                                /* Load Frame */
                                frame = Fiber.Frames[Fiber.NumFrames - 1];
                                ip = frame.Ip;
                                stackStart = frame.StackStart;
                                stack = Fiber.Stack;
                                fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                bytecode = fn.Bytecode;
                            }

                            break;
                        }

                    case Instruction.ImportVariable:
                        {
                            Obj module = fn.Constants[(bytecode[ip] << 8) + bytecode[ip + 1]];
                            ip += 2;
                            Obj variable = fn.Constants[(bytecode[ip] << 8) + bytecode[ip + 1]];
                            ip += 2;
                            Obj result;
                            if (ImportVariable(module, variable, out result))
                            {
                                if (Fiber.StackTop >= Fiber.Capacity)
                                    stack = Fiber.IncreaseStack();
                                stack[Fiber.StackTop++] = result;
                            }
                            else
                            {
                                frame.Ip = ip;
                                RUNTIME_ERROR(Fiber, result);
                                if (Fiber == null)
                                    return false;
                                /* Load Frame */
                                frame = Fiber.Frames[Fiber.NumFrames - 1];
                                ip = frame.Ip;
                                stackStart = frame.StackStart;
                                stack = Fiber.Stack;
                                fn = (frame.Fn as ObjFn) ?? (frame.Fn as ObjClosure).Function;
                                bytecode = fn.Bytecode;
                            }
                            break;
                        }

                    case Instruction.End:
                        // A CODE_END should always be preceded by a CODE_RETURN. If we get here,
                        // the compiler generated wrong code.
                        return false;
                }
            }

            // We should only exit this function from an explicit return from CODE_RETURN
            // or a runtime error.
        }

        // Execute [source] in the context of the core module.
        private InterpretResult LoadIntoCore(string source)
        {
            ObjModule coreModule = GetCoreModule();

            ObjFn fn = Compiler.Compile(this, coreModule, "", source, true);
            if (fn == null) return InterpretResult.CompileError;

            Fiber = new ObjFiber(fn);

            return RunInterpreter() ? InterpretResult.Success : InterpretResult.RuntimeError;
        }

        public InterpretResult Interpret(string sourcePath, string source)
        {
            if (sourcePath.Length == 0) return LoadIntoCore(source);

            // TODO: Better module name.
            Obj name = new ObjString("main");

            ObjFiber f = LoadModule(name, source);
            if (f == null)
            {
                return InterpretResult.CompileError;
            }

            Fiber = f;

            bool succeeded = RunInterpreter();

            return succeeded ? InterpretResult.Success : InterpretResult.RuntimeError;
        }

        public Obj FindVariable(string name)
        {
            ObjModule coreModule = GetCoreModule();
            int symbol = coreModule.Variables.FindIndex(v => v.Name == name);
            return coreModule.Variables[symbol].Container;
        }

        internal int DeclareVariable(ObjModule module, string name)
        {
            if (module == null) module = GetCoreModule();
            if (module.Variables.Count == ObjModule.MaxModuleVars) return -2;

            module.Variables.Add(new ModuleVariable { Name = name, Container = Obj.Undefined });
            return module.Variables.Count - 1;
        }

        internal int DefineVariable(ObjModule module, string name, Obj c)
        {
            if (module == null) module = GetCoreModule();
            if (module.Variables.Count == ObjModule.MaxModuleVars) return -2;

            // See if the variable is already explicitly or implicitly declared.
            int symbol = module.Variables.FindIndex(m => m.Name == name);

            if (symbol == -1)
            {
                // Brand new variable.
                module.Variables.Add(new ModuleVariable { Name = name, Container = c });
                symbol = module.Variables.Count - 1;
            }
            else if (module.Variables[symbol].Container == Obj.Undefined)
            {
                // Explicitly declaring an implicitly declared one. Mark it as defined.
                module.Variables[symbol].Container = c;
            }
            else
            {
                // Already explicitly declared.
                symbol = -1;
            }

            return symbol;
        }

        /* Dirty Hack */
        private void RUNTIME_ERROR(ObjFiber f, Obj v)
        {
            if (f.Error != null)
            {
                Console.Error.WriteLine("Can only fail once.");
                return;
            }

            if (f.CallerIsTrying)
            {
                f.Caller.SetReturnValue(v);
                Fiber = f.Caller;
                f.Error = v as ObjString;
                return;
            }
            Fiber = null;

            // TODO: Fix this so that there is no dependancy on the console
            if (v == null || v as ObjString == null)
            {
                v = new ObjString("Error message must be a string.");
            }
            f.Error = v as ObjString;
            Console.Error.WriteLine(v as ObjString);
        }

        /* Dirty Hack */
        private bool HandleRuntimeError()
        {
            ObjFiber f = Fiber;
            if (f.CallerIsTrying)
            {
                f.Caller.SetReturnValue(f.Error);
                Fiber = f.Caller;
                return true;
            }
            Fiber = null;

            // TODO: Fix this so that there is no dependancy on the console
            if (!(f.Error is ObjString))
            {
                f.Error = Obj.MakeString("Error message must be a string.");
            }
            Console.Error.WriteLine(f.Error as ObjString);
            return false;
        }

        /* Anotehr Dirty Hack */
        public void Primitive(ObjClass objClass, string s, Primitive func)
        {
            if (!MethodNames.Contains(s))
            {
                MethodNames.Add(s);
            }
            int symbol = MethodNames.IndexOf(s);

            Method m = new Method { Primitive = func, MType = MethodType.Primitive };
            objClass.BindMethod(symbol, m);
        }

        public void Call(ObjClass objClass, string s)
        {
            if (!MethodNames.Contains(s))
            {
                MethodNames.Add(s);
            }
            int symbol = MethodNames.IndexOf(s);

            objClass.BindMethod(symbol, new Method { MType = MethodType.Call });
        }

        bool CheckArity(Obj[] args, int numArgs, int stackStart)
        {
            ObjFn fn = args[stackStart] as ObjFn;
            ObjClosure c = args[stackStart] as ObjClosure;

            if (c != null)
            {
                fn = c.Function;
            }

            if (fn == null)
            {
                Fiber.Error = (ObjString)Obj.MakeString("Receiver must be a function or closure.");
                return false;
            }

            if (numArgs - 1 < fn.Arity)
            {
                Fiber.Error = (ObjString)Obj.MakeString("Function expects more arguments.");
                return false;
            }

            return true;
        }

    }
}
