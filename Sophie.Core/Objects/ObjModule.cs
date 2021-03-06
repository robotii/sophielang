﻿using System.Collections.Generic;

namespace Sophie.Core.Objects
{
    // A loaded module and the top-level variables it defines.
    public sealed class ObjModule : Obj
    {
        public const int MaxModuleVars = 65536;

        // The currently defined top-level variables.
        public readonly List<ModuleVariable> Variables;

        // The name of the module.
        public ObjString Name;

        // Creates a new module.
        public ObjModule(ObjString name)
        {
            Name = name;
            Variables = new List<ModuleVariable>();
        }
    }

    public sealed class ModuleVariable
    {
        public string Name;

        internal Obj Container;
    }
}
