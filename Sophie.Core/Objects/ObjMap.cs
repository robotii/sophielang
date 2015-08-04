using System.Collections.Generic;
using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    // A hash table mapping keys to values.
    //
    // We use something very simple: open addressing with linear probing. The hash
    // table is an array of entries. Each entry is a key-value pair. If the key is
    // the special UNDEFINED_VAL, it indicates no value is currently in that slot.
    // Otherwise, it's a valid key, and the value is the value associated with it.
    //
    // When entries are added, the array is dynamically scaled by GROW_FACTOR to
    // keep the number of filled slots under MAP_LOAD_PERCENT. Likewise, if the map
    // gets empty enough, it will be resized to a smaller array. When this happens,
    // all existing entries are rehashed and re-added to the new array.
    //
    // When an entry is removed, its slot is replaced with a "tombstone". This is an
    // entry whose key is UNDEFINED_VAL and whose value is TRUE_VAL. When probing
    // for a key, we will continue past tombstones, because the desired key may be
    // found after them if the key that was removed was part of a prior collision.
    // When the array gets resized, all tombstones are discarded.
    public class ObjMap : Obj
    {

        // Pointer to a contiguous array of [capacity] entries.
        Dictionary<Container,Container> entries;

        // Looks up [key] in [map]. If found, returns the value. Otherwise, returns UNDEFINED.
        public Container Get(Container key)
        {
            Container v;
            return entries.TryGetValue(key, out v) ? v : new Container();
        }

        // Creates a new empty map.
        public ObjMap()
        {
            entries = new Dictionary<Container, Container>(new ContainerComparer());
            ClassObj = SophieVM.MapClass;
        }

        public int Count()
        {
            return entries.Count;
        }

        public Container Get(int index)
        {
            if (index < 0 || index >= entries.Count)
                return new Container();
            Container[] v = new Container[entries.Count];
            entries.Values.CopyTo(v, 0);
            return v[index];
        }

        public Container GetKey(int index)
        {
            if (index < 0 || index >= entries.Count)
                return new Container();
            Container[] v = new Container[entries.Count];
            entries.Keys.CopyTo(v, 0);
            return v[index];
        }

        // Associates [key] with [value] in [map].
        public void Set(Container key, Container c)
        {
            entries[key] = c;
        }

        public void Clear()
        {
            entries = new Dictionary<Container, Container>(new ContainerComparer());
        }

        // Removes [key] from [map], if present. Returns the value for the key if found
        // or `NULL_VAL` otherwise.
        public Container Remove(Container key)
        {
            Container v;
            if (entries.TryGetValue(key, out v))
            {
                entries.Remove(key);
                return v;
            }
            return new Container (ContainerType.Null);
        }
    }
}
