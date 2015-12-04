using System;
using System.Globalization;
using Sophie.Core.Objects;
using Sophie.Core.VM;

namespace Sophie.Core.Library
{
    sealed class CoreLibrary
    {
        private readonly SophieVM _vm;

        // This string literal is generated automatically from core. Do not edit.
        const string CoreLibSource =
        "class Bool {}\n"
        + "class Fiber {}\n"
        + "class Fn {}\n"
        + "class Null {}\n"
        + "class Num {}\n"
        + "\n"
        + "class Sequence {\n"
        + "  all(f) {\n"
        + "    var result = true\n"
        + "    for (element in this) {\n"
        + "      result = f.call(element)\n"
        + "      if (!result) return result\n"
        + "    }\n"
        + "    return result\n"
        + "  }\n"
        + "\n"
        + "  any(f) {\n"
        + "    var result = false\n"
        + "    for (element in this) {\n"
        + "      result = f.call(element)\n"
        + "      if (result) return result\n"
        + "    }\n"
        + "    return result\n"
        + "  }\n"
        + "\n"
        + "  contains(element) {\n"
        + "    for (item in this) {\n"
        + "      if (element == item) return true\n"
        + "    }\n"
        + "    return false\n"
        + "  }\n"
        + "\n"
        + "  count {\n"
        + "    var result = 0\n"
        + "    for (element in this) {\n"
        + "      result = result + 1\n"
        + "    }\n"
        + "    return result\n"
        + "  }\n"
        + "\n"
        + "  count(f) {\n"
        + "    var result = 0\n"
        + "    for (element in this) {\n"
        + "      if (f.call(element)) result = result + 1\n"
        + "    }\n"
        + "    return result\n"
        + "  }\n"
        + "\n"
        + "  each(f) {\n"
        + "    for (element in this) {\n"
        + "      f.call(element)\n"
        + "    }\n"
        + "  }\n"
        + "\n"
        + "  map(transformation) { new MapSequence(this, transformation) }\n"
        + "\n"
        + "  where(predicate) { new WhereSequence(this, predicate) }\n"
        + "\n"
        + "  reduce(acc, f) {\n"
        + "    for (element in this) {\n"
        + "      acc = f.call(acc, element)\n"
        + "    }\n"
        + "    return acc\n"
        + "  }\n"
        + "\n"
        + "  reduce(f) {\n"
        + "    var iter = iterate(null)\n"
        + "    if (!iter) Fiber.abort(\"Can't reduce an empty sequence.\")\n"
        + "\n"
        + "    // Seed with the first element.\n"
        + "    var result = iteratorValue(iter)\n"
        + "    while (iter = iterate(iter)) {\n"
        + "      result = f.call(result, iteratorValue(iter))\n"
        + "    }\n"
        + "\n"
        + "    return result\n"
        + "  }\n"
        + "\n"
        + "  join { join(\"\") }\n"
        + "\n"
        + "  join(sep) {\n"
        + "    var first = true\n"
        + "    var result = \"\"\n"
        + "\n"
        + "    for (element in this) {\n"
        + "      if (!first) result = result + sep\n"
        + "      first = false\n"
        + "      result = result + element.toString\n"
        + "    }\n"
        + "\n"
        + "    return result\n"
        + "  }\n"
        + "\n"
        + "  toList {\n"
        + "    var result = new List\n"
        + "    for (element in this) {\n"
        + "      result.add(element)\n"
        + "    }\n"
        + "    return result\n"
        + "  }\n"
        + "}\n"
        + "\n"
        + "class MapSequence is Sequence {\n"
        + "  new(sequence, fn) {\n"
        + "    _sequence = sequence\n"
        + "    _fn = fn\n"
        + "  }\n"
        + "\n"
        + "  iterate(iterator) { _sequence.iterate(iterator) }\n"
        + "  iteratorValue(iterator) { _fn.call(_sequence.iteratorValue(iterator)) }\n"
        + "}\n"
        + "\n"
        + "class WhereSequence is Sequence {\n"
        + "  new(sequence, fn) {\n"
        + "    _sequence = sequence\n"
        + "    _fn = fn\n"
        + "  }\n"
        + "\n"
        + "  iterate(iterator) {\n"
        + "    while (iterator = _sequence.iterate(iterator)) {\n"
        + "      if (_fn.call(_sequence.iteratorValue(iterator))) break\n"
        + "    }\n"
        + "    return iterator\n"
        + "  }\n"
        + "\n"
        + "  iteratorValue(iterator) { _sequence.iteratorValue(iterator) }\n"
        + "}\n"
        + "\n"
        + "class String is Sequence {}\n"
        + "\n"
        + "class List is Sequence {\n"
        + "  addAll(other) {\n"
        + "    for (element in other) {\n"
        + "      add(element)\n"
        + "    }\n"
        + "    return other\n"
        + "  }\n"
        + "\n"
        + "  toString { \"[\" + join(\", \") + \"]\" }\n"
        + "\n"
        + "  +(other) {\n"
        + "    var result = this[0..-1]\n"
        + "    for (element in other) {\n"
        + "      result.add(element)\n"
        + "    }\n"
        + "    return result\n"
        + "  }\n"
        + "}\n"
        + "\n"
        + "class Map {\n"
        + "  keys { new MapKeySequence(this) }\n"
        + "  values { new MapValueSequence(this) }\n"
        + "\n"
        + "  toString {\n"
        + "    var first = true\n"
        + "    var result = \"{\"\n"
        + "\n"
        + "    for (key in keys) {\n"
        + "      if (!first) result = result + \", \"\n"
        + "      first = false\n"
        + "      result = result + key.toString + \": \" + this[key].toString\n"
        + "    }\n"
        + "\n"
        + "    return result + \"}\"\n"
        + "  }\n"
        + "}\n"
        + "\n"
        + "class MapKeySequence is Sequence {\n"
        + "  new(map) {\n"
        + "    _map = map\n"
        + "  }\n"
        + "\n"
        + "  iterate(n) { _map.iterate_(n) }\n"
        + "  iteratorValue(iterator) { _map.keyIteratorValue_(iterator) }\n"
        + "}\n"
        + "\n"
        + "class MapValueSequence is Sequence {\n"
        + "  new(map) {\n"
        + "    _map = map\n"
        + "  }\n"
        + "\n"
        + "  iterate(n) { _map.iterate_(n) }\n"
        + "  iteratorValue(iterator) { _map.valueIteratorValue_(iterator) }\n"
        + "}\n"
        + "\n"
        + "class Range is Sequence {}\n";

        static bool prim_bool_not(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(stack[argStart] != Obj.True);
            return true;
        }

        static bool prim_bool_toString(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = stack[argStart] == Obj.True ? ObjString.TrueString : ObjString.FalseString;
            return true;
        }

        static bool prim_class_instantiate(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new ObjInstance(stack[argStart] as ObjClass);
            return true;
        }

        static bool prim_class_name(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = ((ObjClass)stack[argStart]).Name;
            return true;
        }

        static bool prim_class_supertype(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjClass classObj = (ObjClass)stack[argStart];

            // Object has no superclass.
            if (classObj.Superclass == null)
            {
                stack[argStart] = Obj.Null;
            }
            else
            {
                stack[argStart] = classObj.Superclass;
            }
            return true;
        }

        static bool prim_fiber_instantiate(SophieVM vm, Obj[] stack, int argStart)
        {
            // Return the Fiber class itself. When we then call "new" on it, it will
            // create the fiber.
            return true;
        }

        static bool prim_fiber_new(SophieVM vm, Obj[] stack, int argStart)
        {
            Obj o = stack[argStart + 1];
            if (o is ObjFn || o is ObjClosure)
            {
                ObjFiber newFiber = new ObjFiber(o);

                // The compiler expects the first slot of a function to hold the receiver.
                // Since a fiber's stack is invoked directly, it doesn't have one, so put it
                // in here.
                newFiber.Push(Obj.Null);

                stack[argStart] = newFiber;
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Argument must be a function.");
            return false;
        }

        static bool prim_fiber_abort(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1] == Obj.Null)
                return true;
            vm.Fiber.Error = stack[argStart + 1];
            return false;
        }

        static bool prim_fiber_call(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFiber runFiber = stack[argStart] as ObjFiber;

            if (runFiber != null)
            {
                if (runFiber.NumFrames != 0)
                {
                    if (runFiber.Caller == null)
                    {
                        // Remember who ran it.
                        runFiber.Caller = vm.Fiber;

                        // If the fiber was yielded, make the yield call return null.
                        if (runFiber.StackTop > 0)
                        {
                            runFiber.StoreValue(-1, Obj.Null);
                        }

                        return false;
                    }

                    vm.Fiber.Error = Obj.MakeString("Fiber has already been called.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Cannot call a finished fiber.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Trying to call a non-fiber");
            return false;
        }

        static bool prim_fiber_call1(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFiber runFiber = stack[argStart] as ObjFiber;

            if (runFiber != null)
            {
                if (runFiber.NumFrames != 0)
                {
                    if (runFiber.Caller == null)
                    {
                        // Remember who ran it.
                        runFiber.Caller = vm.Fiber;

                        // If the fiber was yielded, make the yield call return the value passed to
                        // run.
                        if (runFiber.StackTop > 0)
                        {
                            runFiber.StoreValue(-1, stack[argStart + 1]);
                        }

                        // When the calling fiber resumes, we'll store the result of the run call
                        // in its stack. Since fiber.run(value) has two arguments (the fiber and the
                        // value) and we only need one slot for the result, discard the other slot
                        // now.
                        vm.Fiber.StackTop--;
                        return false;
                    }

                    vm.Fiber.Error = Obj.MakeString("Fiber has already been called.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Cannot call a finished fiber.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Trying to call a non-fiber");
            return false;
        }

        static bool prim_fiber_current(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = vm.Fiber;
            return true;
        }

        static bool prim_fiber_error(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFiber runFiber = (ObjFiber)stack[argStart];
            stack[argStart] = runFiber.Error ?? Obj.Null;
            return true;
        }

        static bool prim_fiber_isDone(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFiber runFiber = (ObjFiber)stack[argStart];
            stack[argStart] = Obj.Bool(runFiber.NumFrames == 0 || runFiber.Error != null);
            return true;
        }

        static bool prim_fiber_run(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFiber runFiber = (ObjFiber)stack[argStart];

            if (runFiber.NumFrames != 0)
            {
                if (runFiber.Caller == null && runFiber.StackTop > 0)
                {
                    runFiber.StoreValue(-1, Obj.Null);
                }

                // Unlike run, this does not remember the calling fiber. Instead, it
                // remember's *that* fiber's caller. You can think of it like tail call
                // elimination. The switched-from fiber is discarded and when the switched
                // to fiber completes or yields, control passes to the switched-from fiber's
                // caller.
                runFiber.Caller = vm.Fiber.Caller;

                return false;
            }

            // If the fiber was yielded, make the yield call return null.
            vm.Fiber.Error = Obj.MakeString("Cannot run a finished fiber.");
            return false;
        }

        static bool prim_fiber_run1(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFiber runFiber = (ObjFiber)stack[argStart];

            if (runFiber.NumFrames != 0)
            {
                if (runFiber.Caller == null && runFiber.StackTop > 0)
                {
                    runFiber.StoreValue(-1, stack[argStart + 1]);
                }

                // Unlike run, this does not remember the calling fiber. Instead, it
                // remember's *that* fiber's caller. You can think of it like tail call
                // elimination. The switched-from fiber is discarded and when the switched
                // to fiber completes or yields, control passes to the switched-from fiber's
                // caller.
                runFiber.Caller = vm.Fiber.Caller;

                vm.Fiber = runFiber;
                return false;
            }

            // If the fiber was yielded, make the yield call return the value passed to
            // run.
            vm.Fiber.Error = Obj.MakeString("Cannot run a finished fiber.");
            return false;
        }

        static bool prim_fiber_try(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFiber runFiber = (ObjFiber)stack[argStart];

            if (runFiber.NumFrames != 0)
            {
                if (runFiber.Caller == null)
                {
                    // Remember who ran it.
                    runFiber.Caller = vm.Fiber;
                    runFiber.CallerIsTrying = true;

                    // If the fiber was yielded, make the yield call return null.
                    if (runFiber.StackTop > 0)
                    {
                        runFiber.StoreValue(-1, Obj.Null);
                    }

                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Fiber has already been called.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Cannot try a finished fiber.");
            return false;
        }

        static bool prim_fiber_yield(SophieVM vm, Obj[] stack, int argStart)
        {
            // Unhook this fiber from the one that called it.
            ObjFiber caller = vm.Fiber.Caller;
            vm.Fiber.Caller = null;
            vm.Fiber.CallerIsTrying = false;

            // If we don't have any other pending fibers, jump all the way out of the
            // interpreter.
            if (caller == null)
            {
                stack[argStart] = Obj.Null;
            }
            else
            {
                // Make the caller's run method return null.
                caller.StoreValue(-1, Obj.Null);

                // Return the fiber to resume.
                stack[argStart] = caller;
            }

            return false;
        }

        static bool prim_fiber_yield1(SophieVM vm, Obj[] stack, int argStart)
        {
            // Unhook this fiber from the one that called it.
            ObjFiber caller = vm.Fiber.Caller;
            vm.Fiber.Caller = null;
            vm.Fiber.CallerIsTrying = false;

            // If we don't have any other pending fibers, jump all the way out of the
            // interpreter.
            if (caller == null)
            {
                stack[argStart] = Obj.Null;
            }
            else
            {
                // Make the caller's run method return the argument passed to yield.
                caller.StoreValue(-1, stack[argStart + 1]);

                // When the yielding fiber resumes, we'll store the result of the yield call
                // in its stack. Since Fiber.yield(value) has two arguments (the Fiber class
                // and the value) and we only need one slot for the result, discard the other
                // slot now.
                vm.Fiber.StackTop--;

                // Return the fiber to resume.
                stack[argStart] = caller;
            }

            return false;
        }

        static bool prim_fn_instantiate(SophieVM vm, Obj[] stack, int argStart)
        {
            // Return the Fn class itself. When we then call "new" on it, it will return
            // the block.
            return true;
        }

        static bool prim_fn_new(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1] == null || stack[argStart + 1] as ObjFn == null && stack[argStart + 1] as ObjClosure == null)
            {
                vm.Fiber.Error = Obj.MakeString("Argument must be a function.");
                return false;
            }

            // The block argument is already a function, so just return it.
            stack[argStart] = stack[argStart + 1];
            return true;
        }

        static bool prim_fn_arity(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjFn fn = stack[argStart] as ObjFn;
            stack[argStart] = fn != null ? new Obj(fn.Arity) : new Obj(0.0);
            return true;
        }

        static bool prim_fn_toString(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.MakeString("<fn>");
            return true;
        }

        static bool prim_list_instantiate(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new ObjList(0);
            return true;
        }

        static bool prim_list_add(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = stack[argStart] as ObjList;
            if (list == null)
            {
                vm.Fiber.Error = Obj.MakeString("Trying to add to a non-list");
                return false;
            }
            list.Add(stack[argStart + 1]);
            stack[argStart] = stack[argStart + 1];
            return true;
        }

        static bool prim_list_clear(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = stack[argStart] as ObjList;
            if (list == null)
            {
                vm.Fiber.Error = Obj.MakeString("Trying to clear a non-list");
                return false;
            }
            list.Clear();

            stack[argStart] = Obj.Null;
            return true;
        }

        static bool prim_list_count(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = stack[argStart] as ObjList;
            if (list != null)
            {
                stack[argStart] = new Obj(list.Count());
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Trying to clear a non-list");
            return false;
        }

        static bool prim_list_insert(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = stack[argStart] as ObjList;
            if (list != null)
            {
                if (stack[argStart + 1].Type == ObjType.Num)
                {
                    if (stack[argStart + 1].Num == (int)stack[argStart + 1].Num)
                    {
                        int index = (int)stack[argStart + 1].Num;

                        if (index < 0)
                            index += list.Count() + 1;
                        if (index >= 0 && index <= list.Count())
                        {
                            list.Insert(stack[argStart + 2], index);
                            stack[argStart] = stack[argStart + 2];
                            return true;
                        }
                        vm.Fiber.Error = Obj.MakeString("Index out of bounds.");
                        return false;
                    }

                    // count + 1 here so you can "insert" at the very end.
                    vm.Fiber.Error = Obj.MakeString("Index must be an integer.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Index must be a number.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("List cannot be null");
            return false;
        }

        static bool prim_list_iterate(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = (ObjList)stack[argStart];

            // If we're starting the iteration, return the first index.
            if (stack[argStart + 1] == Obj.Null)
            {
                if (list.Count() != 0)
                {
                    stack[argStart] = new Obj(0.0);
                    return true;
                }

                stack[argStart] = Obj.Bool(false);
                return true;
            }

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                if (stack[argStart + 1].Num == ((int)stack[argStart + 1].Num))
                {
                    double index = stack[argStart + 1].Num;
                    if (!(index < 0) && !(index >= list.Count() - 1))
                    {
                        stack[argStart] = new Obj(index + 1);
                        return true;
                    }

                    // Otherwise, move to the next index.
                    stack[argStart] = Obj.Bool(false);
                    return true;
                }

                // Stop if we're out of bounds.
                vm.Fiber.Error = Obj.MakeString("Iterator must be an integer.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_list_iteratorValue(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = (ObjList)stack[argStart];

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                if (stack[argStart + 1].Num == ((int)stack[argStart + 1].Num))
                {
                    int index = (int)stack[argStart + 1].Num;

                    if (index >= 0 && index < list.Count())
                    {
                        stack[argStart] = list.Get(index);
                        return true;
                    }

                    vm.Fiber.Error = Obj.MakeString("Iterator out of bounds.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Iterator must be an integer.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_list_removeAt(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = stack[argStart] as ObjList;

            if (list != null)
            {
                if (stack[argStart + 1].Type == ObjType.Num)
                {
                    if (stack[argStart + 1].Num == ((int)stack[argStart + 1].Num))
                    {
                        int index = (int)stack[argStart + 1].Num;
                        if (index < 0)
                            index += list.Count();
                        if (index >= 0 && index < list.Count())
                        {
                            stack[argStart] = list.RemoveAt(index);
                            return true;
                        }

                        vm.Fiber.Error = Obj.MakeString("Index out of bounds.");
                        return false;
                    }

                    vm.Fiber.Error = Obj.MakeString("Index must be an integer.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Index must be a number.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("List cannot be null");
            return false;
        }

        static bool prim_list_subscript(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = stack[argStart] as ObjList;

            if (list == null)
                return false;

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                int index = (int)stack[argStart + 1].Num;
                if (index == stack[argStart + 1].Num)
                {
                    if (index < 0)
                    {
                        index += list.Count();
                    }
                    if (index >= 0 && index < list.Count())
                    {
                        stack[argStart] = list.Get(index);
                        return true;
                    }

                    vm.Fiber.Error = Obj.MakeString("Subscript out of bounds.");
                    return false;
                }
                vm.Fiber.Error = Obj.MakeString("Subscript must be an integer.");
                return false;
            }

            ObjRange r = stack[argStart + 1] as ObjRange;

            if (r == null)
            {
                vm.Fiber.Error = Obj.MakeString("Subscript must be a number or a range.");
                return false;
            }

            // TODO: This is seriously broken and needs a rewrite
            int from = (int)r.From;
            if (from != r.From)
            {
                vm.Fiber.Error = Obj.MakeString("Range start must be an integer.");
                return false;
            }
            int to = (int)r.To;
            if (to != r.To)
            {
                vm.Fiber.Error = Obj.MakeString("Range end must be an integer.");
                return false;
            }

            if (from < 0)
                from += list.Count();
            if (to < 0)
                to += list.Count();

            int step = to < from ? -1 : 1;

            if (step > 0 && r.IsInclusive)
                to += 1;
            if (step < 0 && !r.IsInclusive)
                to += 1;

            // Handle copying an empty list
            if (list.Count() == 0 && to == (r.IsInclusive ? -1 : 0))
            {
                to = 0;
                step = 1;
            }

            int count = (to - from) * step + (step < 0 ? 1 : 0);

            if (to < 0 || from + (count * step) > list.Count())
            {
                vm.Fiber.Error = Obj.MakeString("Range end out of bounds.");
                return false;
            }
            if (from < 0 || (from >= list.Count() && from > 0))
            {
                vm.Fiber.Error = Obj.MakeString("Range start out of bounds.");
                return false;
            }

            ObjList result = new ObjList(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(list.Get(from + (i * step)));
            }

            stack[argStart] = result;
            return true;
        }

        static bool prim_list_subscriptSetter(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjList list = (ObjList)stack[argStart];
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                int index = (int)stack[argStart + 1].Num;

                if (index == stack[argStart + 1].Num)
                {
                    if (index < 0)
                    {
                        index += list.Count();
                    }

                    if (list != null && index >= 0 && index < list.Count())
                    {
                        list.Set(stack[argStart + 2], index);
                        stack[argStart] = stack[argStart + 2];
                        return true;
                    }

                    vm.Fiber.Error = Obj.MakeString("Subscript out of bounds.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Subscript must be an integer.");
                return false;
            }
            vm.Fiber.Error = Obj.MakeString("Subscript must be a number.");
            return false;
        }

        static bool prim_map_instantiate(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new ObjMap();
            return true;
        }

        static bool prim_map_subscript(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap map = stack[argStart] as ObjMap;

            if (ValidateKey(stack[argStart + 1]))
            {
                if (map != null)
                {
                    stack[argStart] = map.Get(stack[argStart + 1]);
                    if (stack[argStart] == Obj.Undefined)
                    {
                        stack[argStart] = Obj.Null;
                    }
                }
                else
                {
                    stack[argStart] = Obj.Null;
                }
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Key must be a value type or fiber.");
            return false;
        }

        static bool prim_map_subscriptSetter(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap map = stack[argStart] as ObjMap;

            if (ValidateKey(stack[argStart + 1]))
            {
                if (map != null)
                {
                    map.Set(stack[argStart + 1], stack[argStart + 2]);
                }
                stack[argStart] = stack[argStart + 2];
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Key must be a value type or fiber.");
            return false;
        }

        static bool prim_map_clear(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap m = stack[argStart] as ObjMap;
            if (m != null)
                m.Clear();
            stack[argStart] = Obj.Null;
            return true;
        }

        static bool prim_map_containsKey(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap map = (ObjMap)stack[argStart];

            if (ValidateKey(stack[argStart + 1]))
            {
                Obj v = map.Get(stack[argStart + 1]);

                stack[argStart] = Obj.Bool(v != Obj.Undefined);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Key must be a value type or fiber.");
            return false;
        }

        static bool prim_map_count(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap m = (ObjMap)stack[argStart];
            stack[argStart] = new Obj(m.Count());
            return true;
        }

        private static bool prim_map_iterate(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap map = (ObjMap)stack[argStart];

            if (map.Count() == 0)
            {
                stack[argStart] = Obj.Bool(false);
                return true;
            }

            // Start one past the last entry we stopped at.
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                if (stack[argStart + 1].Num < 0)
                {
                    stack[argStart] = Obj.Bool(false);
                    return true;
                }
                int index = (int)stack[argStart + 1].Num;

                if (index == stack[argStart + 1].Num)
                {
                    stack[argStart] = index > map.Count() || map.Get(index) == Obj.Undefined ? Obj.Bool(false) : new Obj(index + 1);
                    return true;
                }

                vm.Fiber.Error = Obj.MakeString("Iterator must be an integer.");
                return false;
            }

            // If we're starting the iteration, start at the first used entry.
            if (stack[argStart + 1] == Obj.Null)
            {
                stack[argStart] = new Obj(1);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_map_remove(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap map = (ObjMap)stack[argStart];

            if (ValidateKey(stack[argStart + 1]))
            {
                stack[argStart] = map != null ? map.Remove(stack[argStart + 1]) : Obj.Null;
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Key must be a value type or fiber.");
            return false;
        }

        static bool prim_map_keyIteratorValue(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap map = (ObjMap)stack[argStart];

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                int index = (int)stack[argStart + 1].Num;

                if (index == stack[argStart + 1].Num)
                {
                    if (map != null && index >= 0)
                    {
                        stack[argStart] = map.GetKey(index - 1);
                        return true;
                    }
                    vm.Fiber.Error = Obj.MakeString("Error in prim_map_keyIteratorValue.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Iterator must be an integer.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_map_valueIteratorValue(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjMap map = (ObjMap)stack[argStart];

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                int index = (int)stack[argStart + 1].Num;

                if (index == stack[argStart + 1].Num)
                {
                    if (map != null && index >= 0 && index < map.Count())
                    {
                        stack[argStart] = map.Get(index - 1);
                        return true;
                    }
                    vm.Fiber.Error = Obj.MakeString("Error in prim_map_valueIteratorValue.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Iterator must be an integer.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_null_not(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(true);
            return true;
        }

        static bool prim_null_toString(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.MakeString("null");
            return true;
        }

        static bool prim_num_fromString(SophieVM vm, Obj[] stack, int argStart)
        {

            ObjString s = stack[argStart + 1] as ObjString;

            if (s != null)
            {
                if (s.Str.Length != 0)
                {
                    double n;

                    if (double.TryParse(s.Str, out n))
                    {
                        stack[argStart] = new Obj(n);
                        return true;
                    }

                    stack[argStart] = Obj.Null;
                    return true;
                }

                stack[argStart] = Obj.Null;
                return true;
            }

            // Corner case: Can't parse an empty string.
            vm.Fiber.Error = Obj.MakeString("Argument must be a string.");
            return false;
        }

        static bool prim_num_pi(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(3.14159265358979323846);
            return true;
        }

        static bool prim_num_minus(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj(stack[argStart].Num - stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_plus(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj(stack[argStart].Num + stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_multiply(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj(stack[argStart].Num * stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_divide(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj(stack[argStart].Num / stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_lt(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = Obj.Bool(stack[argStart].Num < stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_gt(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = Obj.Bool(stack[argStart].Num > stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_lte(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = Obj.Bool(stack[argStart].Num <= stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_gte(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = Obj.Bool(stack[argStart].Num >= stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_And(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj((Int64)stack[argStart].Num & (Int64)stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }
        static bool prim_num_Or(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj((Int64)stack[argStart].Num | (Int64)stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_Xor(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj((Int64)stack[argStart].Num ^ (Int64)stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }
        static bool prim_num_LeftShift(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj((Int64)stack[argStart].Num << (int)stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }
        static bool prim_num_RightShift(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj((Int64)stack[argStart].Num >> (int)stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_abs(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Abs(stack[argStart].Num));
            return true;
        }
        static bool prim_num_acos(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Acos(stack[argStart].Num));
            return true;
        }
        static bool prim_num_asin(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Asin(stack[argStart].Num));
            return true;
        }
        static bool prim_num_atan(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Atan(stack[argStart].Num));
            return true;
        }
        static bool prim_num_ceil(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Ceiling(stack[argStart].Num));
            return true;
        }
        static bool prim_num_cos(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Cos(stack[argStart].Num));
            return true;
        }
        static bool prim_num_floor(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Floor(stack[argStart].Num));
            return true;
        }
        static bool prim_num_negate(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(-stack[argStart].Num);
            return true;
        }
        static bool prim_num_sin(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Sin(stack[argStart].Num));
            return true;
        }
        static bool prim_num_sqrt(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Sqrt(stack[argStart].Num));
            return true;
        }
        static bool prim_num_tan(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Tan(stack[argStart].Num));
            return true;
        }

        static bool prim_num_mod(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = new Obj(stack[argStart].Num % stack[argStart + 1].Num);
                return true;
            }
            vm.Fiber.Error = Obj.MakeString("Right operand must be a number.");
            return false;
        }

        static bool prim_num_eqeq(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = Obj.Bool(stack[argStart].Num == (stack[argStart + 1].Num));
                return true;
            }

            stack[argStart] = Obj.Bool(false);
            return true;
        }

        static bool prim_num_bangeq(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                stack[argStart] = Obj.Bool(stack[argStart].Num != stack[argStart + 1].Num);
                return true;
            }

            stack[argStart] = Obj.Bool(true);
            return true;
        }

        static bool prim_num_bitwiseNot(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(~(Int64)stack[argStart].Num);
            // Bitwise operators always work on 64-bit signed ints.
            return true;
        }

        static bool prim_num_dotDot(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                double from = stack[argStart].Num;
                double to = stack[argStart + 1].Num;
                stack[argStart] = new ObjRange(@from, to, true);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Right hand side of range must be a number.");
            return false;
        }

        static bool prim_num_dotDotDot(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                double from = stack[argStart].Num;
                double to = stack[argStart + 1].Num;
                stack[argStart] = new ObjRange(from, to, false);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Right hand side of range must be a number.");
            return false;
        }

        static bool prim_num_atan2(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Atan2(stack[argStart].Num, stack[argStart + 1].Num));
            return true;
        }

        static bool prim_num_fraction(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(stack[argStart].Num - Math.Truncate(stack[argStart].Num));
            return true;
        }

        static bool prim_num_isNan(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(double.IsNaN(stack[argStart].Num));
            return true;
        }

        static bool prim_num_sign(SophieVM vm, Obj[] stack, int argStart)
        {
            double value = stack[argStart].Num;
            stack[argStart] = new Obj(Math.Sign(value));
            return true;
        }

        static bool prim_num_toString(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.MakeString(stack[argStart].Num.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        static bool prim_num_truncate(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(Math.Truncate(stack[argStart].Num));
            return true;
        }

        static bool prim_object_same(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(Obj.Equals(stack[argStart + 1], stack[argStart + 2]));
            return true;
        }

        static bool prim_object_not(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(false);
            return true;
        }

        static bool prim_object_eqeq(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(Obj.Equals(stack[argStart], stack[argStart + 1]));
            return true;
        }

        static bool prim_object_bangeq(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(!Obj.Equals(stack[argStart], stack[argStart + 1]));
            return true;
        }

        static bool prim_object_is(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1] as ObjClass != null)
            {
                ObjClass classObj = stack[argStart].GetClass();
                ObjClass baseClassObj = stack[argStart + 1] as ObjClass;

                // Walk the superclass chain looking for the class.
                do
                {
                    if (baseClassObj == classObj)
                    {
                        stack[argStart] = Obj.Bool(true);
                        return true;
                    }

                    classObj = classObj.Superclass;
                } while (classObj != null);

                stack[argStart] = Obj.Bool(false);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Right operand must be a class.");
            return false;
        }

        static bool prim_object_new(SophieVM vm, Obj[] stack, int argStart)
        {
            // This is the default argument-less constructor that all objects inherit.
            // It just returns "this".
            return true;
        }

        static bool prim_object_toString(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjClass cClass = stack[argStart] as ObjClass;
            ObjInstance instance = stack[argStart] as ObjInstance;
            if (cClass != null)
            {
                stack[argStart] = cClass.Name;
            }
            else if (instance != null)
            {
                ObjString name = instance.ClassObj.Name;
                stack[argStart] = Obj.MakeString(string.Format("instance of {0}", name));
            }
            else
            {
                stack[argStart] = Obj.MakeString("<object>");
            }
            return true;
        }

        static bool prim_object_type(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = stack[argStart].GetClass();
            return true;
        }

        static bool prim_object_instantiate(SophieVM vm, Obj[] stack, int argStart)
        {
            vm.Fiber.Error = Obj.MakeString("Must provide a class to 'new' to construct.");
            return false;
        }

        static bool prim_string_instantiate(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.MakeString("");
            return true;
        }

        static bool prim_range_from(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(((ObjRange)stack[argStart]).From);
            return true;
        }

        static bool prim_range_to(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(((ObjRange)stack[argStart]).To);
            return true;
        }

        static bool prim_range_min(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjRange range = (ObjRange)stack[argStart];
            stack[argStart] = range.From < range.To ? new Obj(range.From) : new Obj(range.To);
            return true;
        }

        static bool prim_range_max(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjRange range = (ObjRange)stack[argStart];
            stack[argStart] = range.From > range.To ? new Obj(range.From) : new Obj(range.To);
            return true;
        }

        static bool prim_range_isInclusive(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = Obj.Bool(((ObjRange)stack[argStart]).IsInclusive);
            return true;
        }

        static bool prim_range_iterate(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjRange range = (ObjRange)stack[argStart];

            // Special case: empty range.
            if (range.From == range.To && !range.IsInclusive)
            {
                stack[argStart] = Obj.Bool(false);
                return true;
            }

            // Start the iteration.
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                double iterator = stack[argStart + 1].Num;

                // Iterate towards [to] from [from].
                if (range.From < range.To)
                {
                    iterator++;
                    if (iterator > range.To)
                    {
                        stack[argStart] = Obj.Bool(false);
                        return true;
                    }
                }
                else
                {
                    iterator--;
                    if (iterator < range.To)
                    {
                        stack[argStart] = Obj.Bool(false);
                        return true;
                    }
                }

                if (!range.IsInclusive && iterator == range.To)
                {
                    stack[argStart] = Obj.Bool(false);
                    return true;
                }

                stack[argStart] = new Obj(iterator);
                return true;
            }
            if (stack[argStart + 1] == Obj.Null)
            {
                stack[argStart] = new Obj(range.From);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_range_iteratorValue(SophieVM vm, Obj[] stack, int argStart)
        {
            // Assume the iterator is a number so that is the value of the range.
            stack[argStart] = stack[argStart + 1];
            return true;
        }

        static bool prim_range_toString(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjRange range = stack[argStart] as ObjRange;

            if (range != null)
                stack[argStart] = Obj.MakeString(string.Format("{0}{1}{2}", range.From, range.IsInclusive ? ".." : "...", range.To));
            return true;
        }

        static bool prim_string_fromCodePoint(SophieVM vm, Obj[] stack, int argStart)
        {
            if (stack[argStart + 1].Type == ObjType.Num)
            {
                int codePoint = (int)stack[argStart + 1].Num;

                if (codePoint == stack[argStart + 1].Num)
                {
                    if (codePoint >= 0)
                    {
                        if (codePoint <= 0x10ffff)
                        {
                            stack[argStart] = ObjString.FromCodePoint(codePoint);
                            return true;
                        }

                        vm.Fiber.Error = Obj.MakeString("Code point cannot be greater than 0x10ffff.");
                        return false;
                    }
                    vm.Fiber.Error = Obj.MakeString("Code point cannot be negative.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Code point must be an integer.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Code point must be a number.");
            return false;
        }

        static bool prim_string_byteAt(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = stack[argStart] as ObjString;

            if (s == null)
            {
                return false;
            }

            int index = (int)(stack[argStart].Type == ObjType.Num ? stack[argStart].Num : 0);

            stack[argStart] = new Obj(s.ToString()[index]);
            return true;
        }

        static bool prim_string_codePointAt(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = stack[argStart] as ObjString;

            if (s == null)
            {
                return false;
            }

            if (stack[argStart + 1].Type != ObjType.Num)
            {
                vm.Fiber.Error = Obj.MakeString("Index must be a number.");
                return false;
            }

            int index = (int)stack[argStart + 1].Num;

            if (index != stack[argStart + 1].Num)
            {
                vm.Fiber.Error = Obj.MakeString("Index must be an integer.");
                return false;
            }

            if (index < 0 || index >= s.Str.Length)
            {
                vm.Fiber.Error = Obj.MakeString("Index out of bounds.");
                return false;
            }

            stack[argStart] = new Obj(s.Str[index]);
            return true;
        }

        static bool prim_string_contains(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = (ObjString)stack[argStart];
            ObjString search = stack[argStart + 1] as ObjString;

            if (search == null)
            {
                vm.Fiber.Error = Obj.MakeString("Argument must be a string.");
                return false;
            }

            stack[argStart] = Obj.Bool(s.Str.Contains(search.Str));
            return true;
        }

        static bool prim_string_count(SophieVM vm, Obj[] stack, int argStart)
        {
            stack[argStart] = new Obj(stack[argStart].ToString().Length);
            return true;
        }

        static bool prim_string_endsWith(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = (ObjString)stack[argStart];
            ObjString search = stack[argStart + 1] as ObjString;

            if (search == null)
            {
                vm.Fiber.Error = Obj.MakeString("Argument must be a string.");
                return false;
            }

            stack[argStart] = Obj.Bool(s.Str.EndsWith(search.Str));
            return true;
        }

        static bool prim_string_indexOf(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = (ObjString)stack[argStart];
            ObjString search = stack[argStart + 1] as ObjString;

            if (search != null)
            {
                int index = s.Str.IndexOf(search.Str, StringComparison.Ordinal);
                stack[argStart] = new Obj(index);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Argument must be a string.");
            return false;
        }

        static bool prim_string_iterate(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = (ObjString)stack[argStart];

            // If we're starting the iteration, return the first index.
            if (stack[argStart + 1] == Obj.Null)
            {
                if (s.Str.Length != 0)
                {
                    stack[argStart] = new Obj(0.0);
                    return true;
                }
                stack[argStart] = Obj.Bool(false);
                return true;
            }

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                if (stack[argStart + 1].Num < 0)
                {
                    stack[argStart] = Obj.Bool(false);
                    return true;
                }
                int index = (int)stack[argStart + 1].Num;

                if (index == stack[argStart + 1].Num)
                {
                    index++;
                    if (index >= s.Str.Length)
                    {
                        stack[argStart] = Obj.Bool(false);
                        return true;
                    }

                    stack[argStart] = new Obj(index);
                    return true;
                }

                // Advance to the beginning of the next UTF-8 sequence.
                vm.Fiber.Error = Obj.MakeString("Iterator must be an integer.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_string_iterateByte(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = (ObjString)stack[argStart];

            // If we're starting the iteration, return the first index.
            if (stack[argStart + 1] == Obj.Null)
            {
                if (s.Str.Length == 0)
                {
                    stack[argStart] = Obj.Bool(false);
                    return true;
                }
                stack[argStart] = new Obj(0.0);
                return true;
            }

            if (stack[argStart + 1].Type != ObjType.Num) return false;

            if (stack[argStart + 1].Num < 0)
            {
                stack[argStart] = Obj.Bool(false);
                return true;
            }
            int index = (int)stack[argStart + 1].Num;

            // Advance to the next byte.
            index++;
            if (index >= s.Str.Length)
            {
                stack[argStart] = Obj.Bool(false);
                return true;
            }

            stack[argStart] = new Obj(index);
            return true;
        }

        static bool prim_string_iteratorValue(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = (ObjString)stack[argStart];

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                int index = (int)stack[argStart + 1].Num;

                if (index == stack[argStart + 1].Num)
                {
                    if (index < s.Str.Length && index >= 0)
                    {
                        stack[argStart] = Obj.MakeString("" + s.Str[index]);
                        return true;
                    }

                    vm.Fiber.Error = Obj.MakeString("Iterator out of bounds.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Iterator must be an integer.");
                return false;
            }
            vm.Fiber.Error = Obj.MakeString("Iterator must be a number.");
            return false;
        }

        static bool prim_string_startsWith(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s = (ObjString)stack[argStart];
            ObjString search = stack[argStart + 1] as ObjString;

            if (search != null)
            {
                stack[argStart] = Obj.Bool(s.Str.StartsWith(search.Str));
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Argument must be a string.");
            return false;
        }

        static bool prim_string_toString(SophieVM vm, Obj[] stack, int argStart)
        {
            return true;
        }

        static bool prim_string_plus(SophieVM vm, Obj[] stack, int argStart)
        {
            ObjString s1 = stack[argStart + 1] as ObjString;
            if (s1 != null)
            {
                stack[argStart] = Obj.MakeString(((ObjString)stack[argStart]).Str + s1.Str);
                return true;
            }

            vm.Fiber.Error = Obj.MakeString("Right operand must be a string.");
            return false;
        }

        static bool prim_string_subscript(SophieVM vm, Obj[] stack, int argStart)
        {
            string s = ((ObjString)stack[argStart]).Str;

            if (stack[argStart + 1].Type == ObjType.Num)
            {
                int index = (int)stack[argStart + 1].Num;

                if (index == stack[argStart + 1].Num)
                {
                    if (index < 0)
                    {
                        index += s.Length;
                    }

                    if (index >= 0 && index < s.Length)
                    {
                        stack[argStart] = ObjString.FromCodePoint(s[index]);
                        return true;
                    }

                    vm.Fiber.Error = Obj.MakeString("Subscript out of bounds.");
                    return false;
                }

                vm.Fiber.Error = Obj.MakeString("Subscript must be an integer.");
                return false;
            }

            if (stack[argStart + 1] as ObjRange != null)
            {
                vm.Fiber.Error = Obj.MakeString("Subscript ranges for strings are not implemented yet.");
                return false;
            }

            vm.Fiber.Error = Obj.MakeString("Subscript must be a number or a range.");
            return false;
        }

        // Creates either the Object or Class class in the core library with [name].
        static ObjClass DefineClass(SophieVM vm, string name)
        {
            ObjString nameString = new ObjString(name);

            ObjClass classObj = new ObjClass(0, nameString);

            vm.DefineVariable(null, name, classObj);

            return classObj;
        }

        static bool ValidateKey(Obj arg)
        {
            return arg == Obj.False
                   || arg == Obj.True
                   || arg.Type == ObjType.Num
                   || arg == Obj.Null
                   || arg is ObjClass || arg is ObjFiber
                   || arg is ObjRange || arg is ObjString;
        }

        public CoreLibrary(SophieVM v)
        {
            _vm = v;
        }

        public void InitializeCore()
        {
            // Define the root Object class. This has to be done a little specially
            // because it has no superclass.
            SophieVM.ObjectClass = DefineClass(_vm, "Object");
            _vm.Primitive(SophieVM.ObjectClass, "!", prim_object_not);
            _vm.Primitive(SophieVM.ObjectClass, "==(_)", prim_object_eqeq);
            _vm.Primitive(SophieVM.ObjectClass, "!=(_)", prim_object_bangeq);
            _vm.Primitive(SophieVM.ObjectClass, "new", prim_object_new);
            _vm.Primitive(SophieVM.ObjectClass, "is(_)", prim_object_is);
            _vm.Primitive(SophieVM.ObjectClass, "toString", prim_object_toString);
            _vm.Primitive(SophieVM.ObjectClass, "type", prim_object_type);
            _vm.Primitive(SophieVM.ObjectClass, "<instantiate>", prim_object_instantiate);

            // Now we can define Class, which is a subclass of Object.
            SophieVM.ClassClass = DefineClass(_vm, "Class");
            SophieVM.ClassClass.BindSuperclass(SophieVM.ObjectClass);
            // Store a copy of the class in ObjClass
            ObjClass.ClassClass = SophieVM.ClassClass;
            // Define the primitives
            _vm.Primitive(SophieVM.ClassClass, "<instantiate>", prim_class_instantiate);
            _vm.Primitive(SophieVM.ClassClass, "name", prim_class_name);
            _vm.Primitive(SophieVM.ClassClass, "supertype", prim_class_supertype);

            // Finally, we can define Object's metaclass which is a subclass of Class.
            ObjClass objectMetaclass = DefineClass(_vm, "Object metaclass");

            // Wire up the metaclass relationships now that all three classes are built.
            SophieVM.ObjectClass.ClassObj = objectMetaclass;
            objectMetaclass.ClassObj = SophieVM.ClassClass;
            SophieVM.ClassClass.ClassObj = SophieVM.ClassClass;

            // Do this after wiring up the metaclasses so objectMetaclass doesn't get
            // collected.
            objectMetaclass.BindSuperclass(SophieVM.ClassClass);

            _vm.Primitive(objectMetaclass, "same(_,_)", prim_object_same);

            // The core class diagram ends up looking like this, where single lines point
            // to a class's superclass, and double lines point to its metaclass:
            //
            //        .------------------------------------. .====.
            //        |                  .---------------. | #    #
            //        v                  |               v | v    #
            //   .---------.   .-------------------.   .-------.  #
            //   | Object  |==>| Object metaclass  |==>| Class |=="
            //   '---------'   '-------------------'   '-------'
            //        ^                                 ^ ^ ^ ^
            //        |                  .--------------' # | #
            //        |                  |                # | #
            //   .---------.   .-------------------.      # | # -.
            //   |  Base   |==>|  Base metaclass   |======" | #  |
            //   '---------'   '-------------------'        | #  |
            //        ^                                     | #  |
            //        |                  .------------------' #  | Example classes
            //        |                  |                    #  |
            //   .---------.   .-------------------.          #  |
            //   | Derived |==>| Derived metaclass |=========="  |
            //   '---------'   '-------------------'            -'

            // The rest of the classes can now be defined normally.
            _vm.Interpret("", CoreLibSource);

            SophieVM.BoolClass = (ObjClass)_vm.FindVariable("Bool");
            _vm.Primitive(SophieVM.BoolClass, "toString", prim_bool_toString);
            _vm.Primitive(SophieVM.BoolClass, "!", prim_bool_not);

            SophieVM.FiberClass = (ObjClass)_vm.FindVariable("Fiber");
            _vm.Primitive(SophieVM.FiberClass.ClassObj, "<instantiate>", prim_fiber_instantiate);
            _vm.Primitive(SophieVM.FiberClass.ClassObj, "new(_)", prim_fiber_new);
            _vm.Primitive(SophieVM.FiberClass.ClassObj, "abort(_)", prim_fiber_abort);
            _vm.Primitive(SophieVM.FiberClass.ClassObj, "current", prim_fiber_current);
            _vm.Primitive(SophieVM.FiberClass.ClassObj, "yield()", prim_fiber_yield);
            _vm.Primitive(SophieVM.FiberClass.ClassObj, "yield(_)", prim_fiber_yield1);
            _vm.Primitive(SophieVM.FiberClass, "call()", prim_fiber_call);
            _vm.Primitive(SophieVM.FiberClass, "call(_)", prim_fiber_call1);
            _vm.Primitive(SophieVM.FiberClass, "error", prim_fiber_error);
            _vm.Primitive(SophieVM.FiberClass, "isDone", prim_fiber_isDone);
            _vm.Primitive(SophieVM.FiberClass, "run()", prim_fiber_run);
            _vm.Primitive(SophieVM.FiberClass, "run(_)", prim_fiber_run1);
            _vm.Primitive(SophieVM.FiberClass, "try()", prim_fiber_try);

            SophieVM.FnClass = (ObjClass)_vm.FindVariable("Fn");
            _vm.Primitive(SophieVM.FnClass.ClassObj, "<instantiate>", prim_fn_instantiate);
            _vm.Primitive(SophieVM.FnClass.ClassObj, "new(_)", prim_fn_new);

            _vm.Primitive(SophieVM.FnClass, "arity", prim_fn_arity);

            _vm.Call(SophieVM.FnClass, "call()");
            _vm.Call(SophieVM.FnClass, "call(_)");
            _vm.Call(SophieVM.FnClass, "call(_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_,_,_,_,_,_,_)");
            _vm.Call(SophieVM.FnClass, "call(_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_)");

            _vm.Primitive(SophieVM.FnClass, "toString", prim_fn_toString);

            SophieVM.NullClass = (ObjClass)_vm.FindVariable("Null");
            _vm.Primitive(SophieVM.NullClass, "!", prim_null_not);
            _vm.Primitive(SophieVM.NullClass, "toString", prim_null_toString);

            SophieVM.NumClass = (ObjClass)_vm.FindVariable("Num");
            _vm.Primitive(SophieVM.NumClass.ClassObj, "fromString(_)", prim_num_fromString);
            _vm.Primitive(SophieVM.NumClass.ClassObj, "pi", prim_num_pi);
            _vm.Primitive(SophieVM.NumClass, "-(_)", prim_num_minus);
            _vm.Primitive(SophieVM.NumClass, "+(_)", prim_num_plus);
            _vm.Primitive(SophieVM.NumClass, "*(_)", prim_num_multiply);
            _vm.Primitive(SophieVM.NumClass, "/(_)", prim_num_divide);
            _vm.Primitive(SophieVM.NumClass, "<(_)", prim_num_lt);
            _vm.Primitive(SophieVM.NumClass, ">(_)", prim_num_gt);
            _vm.Primitive(SophieVM.NumClass, "<=(_)", prim_num_lte);
            _vm.Primitive(SophieVM.NumClass, ">=(_)", prim_num_gte);
            _vm.Primitive(SophieVM.NumClass, "&(_)", prim_num_And);
            _vm.Primitive(SophieVM.NumClass, "|(_)", prim_num_Or);
            _vm.Primitive(SophieVM.NumClass, "^(_)", prim_num_Xor);
            _vm.Primitive(SophieVM.NumClass, "<<(_)", prim_num_LeftShift);
            _vm.Primitive(SophieVM.NumClass, ">>(_)", prim_num_RightShift);
            _vm.Primitive(SophieVM.NumClass, "abs", prim_num_abs);
            _vm.Primitive(SophieVM.NumClass, "acos", prim_num_acos);
            _vm.Primitive(SophieVM.NumClass, "asin", prim_num_asin);
            _vm.Primitive(SophieVM.NumClass, "atan", prim_num_atan);
            _vm.Primitive(SophieVM.NumClass, "ceil", prim_num_ceil);
            _vm.Primitive(SophieVM.NumClass, "cos", prim_num_cos);
            _vm.Primitive(SophieVM.NumClass, "floor", prim_num_floor);
            _vm.Primitive(SophieVM.NumClass, "-", prim_num_negate);
            _vm.Primitive(SophieVM.NumClass, "sin", prim_num_sin);
            _vm.Primitive(SophieVM.NumClass, "sqrt", prim_num_sqrt);
            _vm.Primitive(SophieVM.NumClass, "tan", prim_num_tan);
            _vm.Primitive(SophieVM.NumClass, "%(_)", prim_num_mod);
            _vm.Primitive(SophieVM.NumClass, "~", prim_num_bitwiseNot);
            _vm.Primitive(SophieVM.NumClass, "..(_)", prim_num_dotDot);
            _vm.Primitive(SophieVM.NumClass, "...(_)", prim_num_dotDotDot);
            _vm.Primitive(SophieVM.NumClass, "atan(_)", prim_num_atan2);
            _vm.Primitive(SophieVM.NumClass, "fraction", prim_num_fraction);
            _vm.Primitive(SophieVM.NumClass, "isNan", prim_num_isNan);
            _vm.Primitive(SophieVM.NumClass, "sign", prim_num_sign);
            _vm.Primitive(SophieVM.NumClass, "toString", prim_num_toString);
            _vm.Primitive(SophieVM.NumClass, "truncate", prim_num_truncate);

            // These are defined just so that 0 and -0 are equal, which is specified by
            // IEEE 754 even though they have different bit representations.
            _vm.Primitive(SophieVM.NumClass, "==(_)", prim_num_eqeq);
            _vm.Primitive(SophieVM.NumClass, "!=(_)", prim_num_bangeq);

            SophieVM.StringClass = (ObjClass)_vm.FindVariable("String");
            _vm.Primitive(SophieVM.StringClass.ClassObj, "fromCodePoint(_)", prim_string_fromCodePoint);
            _vm.Primitive(SophieVM.StringClass.ClassObj, "<instantiate>", prim_string_instantiate);
            _vm.Primitive(SophieVM.StringClass, "+(_)", prim_string_plus);
            _vm.Primitive(SophieVM.StringClass, "[_]", prim_string_subscript);
            _vm.Primitive(SophieVM.StringClass, "byteAt(_)", prim_string_byteAt);
            _vm.Primitive(SophieVM.StringClass, "codePointAt(_)", prim_string_codePointAt);
            _vm.Primitive(SophieVM.StringClass, "contains(_)", prim_string_contains);
            _vm.Primitive(SophieVM.StringClass, "count", prim_string_count);
            _vm.Primitive(SophieVM.StringClass, "endsWith(_)", prim_string_endsWith);
            _vm.Primitive(SophieVM.StringClass, "indexOf(_)", prim_string_indexOf);
            _vm.Primitive(SophieVM.StringClass, "iterate(_)", prim_string_iterate);
            _vm.Primitive(SophieVM.StringClass, "iterateByte_(_)", prim_string_iterateByte);
            _vm.Primitive(SophieVM.StringClass, "iteratorValue(_)", prim_string_iteratorValue);
            _vm.Primitive(SophieVM.StringClass, "startsWith(_)", prim_string_startsWith);
            _vm.Primitive(SophieVM.StringClass, "toString", prim_string_toString);

            SophieVM.ListClass = (ObjClass)_vm.FindVariable("List");
            _vm.Primitive(SophieVM.ListClass.ClassObj, "<instantiate>", prim_list_instantiate);
            _vm.Primitive(SophieVM.ListClass, "[_]", prim_list_subscript);
            _vm.Primitive(SophieVM.ListClass, "[_]=(_)", prim_list_subscriptSetter);
            _vm.Primitive(SophieVM.ListClass, "add(_)", prim_list_add);
            _vm.Primitive(SophieVM.ListClass, "clear()", prim_list_clear);
            _vm.Primitive(SophieVM.ListClass, "count", prim_list_count);
            _vm.Primitive(SophieVM.ListClass, "insert(_,_)", prim_list_insert);
            _vm.Primitive(SophieVM.ListClass, "iterate(_)", prim_list_iterate);
            _vm.Primitive(SophieVM.ListClass, "iteratorValue(_)", prim_list_iteratorValue);
            _vm.Primitive(SophieVM.ListClass, "removeAt(_)", prim_list_removeAt);

            SophieVM.MapClass = (ObjClass)_vm.FindVariable("Map");
            _vm.Primitive(SophieVM.MapClass.ClassObj, "<instantiate>", prim_map_instantiate);
            _vm.Primitive(SophieVM.MapClass, "[_]", prim_map_subscript);
            _vm.Primitive(SophieVM.MapClass, "[_]=(_)", prim_map_subscriptSetter);
            _vm.Primitive(SophieVM.MapClass, "clear()", prim_map_clear);
            _vm.Primitive(SophieVM.MapClass, "containsKey(_)", prim_map_containsKey);
            _vm.Primitive(SophieVM.MapClass, "count", prim_map_count);
            _vm.Primitive(SophieVM.MapClass, "remove(_)", prim_map_remove);
            _vm.Primitive(SophieVM.MapClass, "iterate_(_)", prim_map_iterate);
            _vm.Primitive(SophieVM.MapClass, "keyIteratorValue_(_)", prim_map_keyIteratorValue);
            _vm.Primitive(SophieVM.MapClass, "valueIteratorValue_(_)", prim_map_valueIteratorValue);

            SophieVM.RangeClass = (ObjClass)_vm.FindVariable("Range");
            _vm.Primitive(SophieVM.RangeClass, "from", prim_range_from);
            _vm.Primitive(SophieVM.RangeClass, "to", prim_range_to);
            _vm.Primitive(SophieVM.RangeClass, "min", prim_range_min);
            _vm.Primitive(SophieVM.RangeClass, "max", prim_range_max);
            _vm.Primitive(SophieVM.RangeClass, "isInclusive", prim_range_isInclusive);
            _vm.Primitive(SophieVM.RangeClass, "iterate(_)", prim_range_iterate);
            _vm.Primitive(SophieVM.RangeClass, "iteratorValue(_)", prim_range_iteratorValue);
            _vm.Primitive(SophieVM.RangeClass, "toString", prim_range_toString);

            // While bootstrapping the core types and running the core library, a number
            // of string objects have been created, many of which were instantiated
            // before stringClass was stored in the VM. Some of them *must* be created
            // first -- the ObjClass for string itself has a reference to the ObjString
            // for its name.
            //
            // These all currently have a NULL classObj pointer, so go back and assign
            // them now that the string class is known.
            ObjString.InitClass();

            SophieVM.ClassClass.IsSealed = true;
            SophieVM.FiberClass.IsSealed = true;
            SophieVM.FnClass.IsSealed = true;
            SophieVM.ListClass.IsSealed = true;
            SophieVM.MapClass.IsSealed = true;
            SophieVM.RangeClass.IsSealed = true;
            SophieVM.StringClass.IsSealed = true;
        }
    }
}
