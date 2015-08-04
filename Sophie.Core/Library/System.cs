﻿using System;
using Sophie.Core.Objects;
using Sophie.Core.VM;

namespace Sophie.Core.Library
{
    class System
    {
        const string SystemLibSource =
        "class System {\n"
        + "  static print {\n"
        + "    System.writeString_(\"\n\")\n"
        + "  }\n"
        + "\n"
        + "  static print(obj) {\n"
        + "    System.writeObject_(obj)\n"
        + "    System.writeString_(\"\n\")\n"
        + "    return obj\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2) {\n"
        + "    printList_([a1, a2])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3) {\n"
        + "    printList_([a1, a2, a3])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4) {\n"
        + "    printList_([a1, a2, a3, a4])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5) {\n"
        + "    printList_([a1, a2, a3, a4, a5])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9, a10])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15])\n"
        + "  }\n"
        + "\n"
        + "  static print(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16) {\n"
        + "    printList_([a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16])\n"
        + "  }\n"
        + "\n"
        + "  static printList_(objects) {\n"
        + "    for (object in objects) System.writeObject_(object)\n"
        + "    System.writeString_(\"\n\")\n"
        + "  }\n"
        + "\n"
        + "  static write(obj) {\n"
        + "    System.writeObject_(obj)\n"
        + "    return obj\n"
        + "  }\n"
        + "\n"
        + "  static read(prompt) {\n"
        + "    if (!(prompt is String)) Fiber.abort(\"Prompt must be a string.\")\n"
        + "    System.write(prompt)\n"
        + "    return System.read\n"
        + "  }\n"
        + "\n"
        + "  static writeObject_(obj) {\n"
        + "    var string = obj.toString\n"
        + "    if (string is String) {\n"
        + "      System.writeString_(string)\n"
        + "    } else {\n"
        + "      System.writeString_(\"[invalid toString]\")\n"
        + "    }\n"
        + "  }\n"
        + "\n"
        + "}\n";

        static PrimitiveResult WriteString(SophieVM vm, ObjFiber fiber, Container[] args)
        {
            if (args[1] != null && args[1].Type == ContainerType.Obj)
            {
                string s = args[1].Obj.ToString();
                Console.Write(s);
            }
            args[0] = new Container (ContainerType.Null);
            return PrimitiveResult.Value;
        }

        static PrimitiveResult Read(SophieVM vm, ObjFiber fiber, Container[] args)
        {
            args[0] = new Container(Console.ReadLine());
            if (((ObjString)args[0].Obj).Value == "")
            {
                args[0] = new Container (ContainerType.Null);
            }
            return PrimitiveResult.Value;
        }

        static PrimitiveResult Clock(SophieVM vm, ObjFiber fiber, Container[] args)
        {
            args[0] = new Container((double)DateTime.Now.Ticks / 10000000);
            return PrimitiveResult.Value;
        }

        static PrimitiveResult Time(SophieVM vm, ObjFiber fiber, Container[] args)
        {
            args[0] = new Container((double)DateTime.Now.Ticks / 10000000);
            return PrimitiveResult.Value;
        }

        public static void LoadSystemLibrary(SophieVM vm)
        {
            vm.Interpret("", SystemLibSource);
            ObjClass system = (ObjClass)vm.FindVariable("System").Obj;
            vm.Primitive(system.ClassObj, "writeString_(_)", WriteString);
            vm.Primitive(system.ClassObj, "read", Read);
            vm.Primitive(system.ClassObj, "clock", Clock);
            vm.Primitive(system.ClassObj, "time", Time);
        }
    }
}