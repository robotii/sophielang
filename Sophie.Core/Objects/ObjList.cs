using System.Collections.Generic;
using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    public class ObjList : Obj
    {
        // The elements in the list.
        readonly List<Container> elements;

        // Creates a new list with [numElements] elements (which are left
        // uninitialized.)
        public ObjList(int numElements)
        {
            elements = new List<Container>(numElements);
            ClassObj = SophieVM.ListClass;
        }

        public void Clear()
        {
            elements.Clear();
        }

        public int Count()
        {
            return elements.Count;
        }

        public Container Get(int index)
        {
            return elements[index];
        }

        public void Set(Container v, int index)
        {
            elements[index] = v;
        }

        // Inserts [value] in [list] at [index], shifting down the other elements.
        public void Insert(Container c, int index)
        {
            elements.Insert(index, c);
        }

        public void Add(Container v)
        {
            elements.Add(v);
        }

        // Removes and returns the item at [index] from [list].
        public Container RemoveAt(int index)
        {
            if (elements.Count > index)
            {
                Container v = elements[index];
                elements.RemoveAt(index);
                return v;
            }
            return new Container (ContainerType.Null);
        }

    }
}
