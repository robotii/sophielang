namespace Sophie.Core.VM
{
    // This defines the bytecode instructions used by the VM.

    // Note that the order of instructions here affects the order of the dispatch
    // table in the VM's interpreter loop. That in turn affects caching which
    // affects overall performance. Take care to run benchmarks if you change the
    // order here.
    internal enum Instruction
    {
        // Pushes the value in the given local slot.
        LoadLocal0 = 0,
        LoadLocal1 = 1,
        LoadLocal2 = 2,
        LoadLocal3 = 3,
        LoadLocal4 = 4,
        LoadLocal5 = 5,
        LoadLocal6 = 6,
        LoadLocal7 = 7,
        LoadLocal8 = 8,

        // Load the constant at index [arg].
        SmallConstant,
        Constant,

        // Push Null onto the stack
        Null,

        // Push false onto the stack.
        False,

        // Push true onto the stack.
        True,

        // Note: The compiler assumes the following _STORE instructions always
        // immediately follow their corresponding _LOAD ones.

        // Pushes the value in local slot [arg].
        LoadLocal,

        // Stores the top of stack in local slot [arg]. Does not pop it.
        StoreLocal,

        // Pushes the value in upvalue [arg].
        LoadUpvalue,

        // Stores the top of stack in upvalue [arg]. Does not pop it.
        StoreUpvalue,

        // Pushes the value of the top-level variable in slot [arg].
        LoadModuleVar,

        // Stores the top of stack in top-level variable slot [arg]. Does not pop it.
        StoreModuleVar,

        // Pushes the value of the field in slot [arg] of the receiver of the current
        // function. This is used for regular field accesses on "this" directly in
        // methods. This instruction is faster than the more general CODE_LOAD_FIELD
        // instruction.
        LoadFieldThis,

        // Stores the top of the stack in field slot [arg] in the receiver of the
        // current value. Does not pop the value. This instruction is faster than the
        // more general CODE_LOAD_FIELD instruction.
        StoreFieldThis,

        // Pops an instance and pushes the value of the field in slot [arg] of it.
        LoadField,

        // Pops an instance and stores the subsequent top of stack in field slot
        // [arg] in it. Does not pop the value.
        StoreField,

        // Pop and discard the top of stack.
        Pop,

        // Push a copy of the value currently on the top of the stack.
        Dup,

        // Invoke the method with symbol [arg]. The number indicates the number of
        // arguments (not including the receiver,.
        Call0 = 65,
        Call1 = 66,
        Call2 = 67,
        Call3 = 68,
        Call4 = 69,
        Call5 = 70,
        Call6 = 71,
        Call7 = 72,
        Call8 = 73,
        Call9 = 74,
        Call10 = 75,
        Call11 = 76,
        Call12 = 77,
        Call13 = 78,
        Call14 = 79,
        Call15 = 80,
        Call16 = 81,

        // Invoke a superclass method with symbol [arg]. The number indicates the
        // number of arguments (not including the receiver,.
        Super0 = 129,
        Super1 = 130,
        Super2 = 131,
        Super3 = 132,
        Super4 = 133,
        Super5 = 134,
        Super6 = 135,
        Super7 = 136,
        Super8 = 137,
        Super9 = 138,
        Super10 = 139,
        Super11 = 140,
        Super12 = 141,
        Super13 = 142,
        Super14 = 143,
        Super15 = 144,
        Super16 = 145,

        // Jump the instruction pointer [arg] forward.
        Jump,

        // Jump the instruction pointer [arg] backward. Pop and discard the top of
        // the stack.
        Loop,

        // Pop and if not truthy then jump the instruction pointer [arg] forward.
        JumpIf,

        // If the top of the stack is false, jump [arg] forward. Otherwise, pop and
        // continue.
        And,

        // If the top of the stack is non-false, jump [arg] forward. Otherwise, pop
        // and continue.
        Or,

        // Close the upvalue for the local on the top of the stack, then pop it.
        CloseUpvalue,

        // Exit from the current function and return the value on the top of the
        // stack.
        Return,

        // Creates a closure for the function stored at [arg] in the constant table.
        //
        // Following the function argument is a number of arguments, two for each
        // upvalue. The first is true if the variable being captured is a local (as
        // opposed to an upvalue,, and the second is the index of the local or
        // upvalue being captured.
        //
        // Pushes the created closure.
        Closure,

        // Creates a class. Top of stack is the superclass, or `null` if the class
        // inherits Object. Below that is a string for the name of the class. Byte
        // [arg] is the number of fields in the class.
        Class,

        // Define a method for symbol [arg]. The class receiving the method is popped
        // off the stack, then the function defining the body is popped.
        MethodInstance,

        // Define a method for symbol [arg]. The class whose metaclass will receive
        // the method is popped off the stack, then the function defining the body is
        // popped.
        MethodStatic,

        // Load the module whose name is stored in string constant [arg]. Pushes
        // NULL onto the stack. If the module has already been loaded, does nothing
        // else. Otherwise, it creates a fiber to run the desired module and switches
        // to that. When that fiber is done, the current one is resumed.
        LoadModule,

        // Reads a top-level variable from another module. [arg1] is a string
        // constant for the name of the module, and [arg2] is a string constant for
        // the variable name. Pushes the variable if found, or generates a runtime
        // error otherwise.
        ImportVariable,

        // This pseudo-instruction indicates the end of the bytecode. It should
        // always be preceded by a `CODE_RETURN`, so is never actually executed.
        End
    };
}
