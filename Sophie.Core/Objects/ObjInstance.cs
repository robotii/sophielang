using Sophie.Core.VM;

namespace Sophie.Core.Objects
{
    public class ObjInstance : Obj
    {
        // Creates a new instance of the given [classObj].
        public ObjInstance(ObjClass classObj)
        {
            Fields = new Container[classObj.NumFields];

            // Initialize fields to null.
            for (int i = 0; i < classObj.NumFields; i++)
            {
                Fields[i] = Container.Null;
            }
            ClassObj = classObj;
        }

        public Container[] Fields;
    }
}
