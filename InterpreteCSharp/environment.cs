using System.Collections.Generic;
using InterpreteCSharp.Objects;

namespace InterpreteCSharp.Environment
{
    public class Env
    {
        private readonly Dictionary<string, IObject> store = new();
        private readonly Env outer;

        public Env(Env outer = null)
        {
            this.outer = outer;
        }

        public IObject Get(string name)
        {
            if (store.TryGetValue(name, out var value))
                return value;
            return outer?.Get(name);
        }

        public void Set(string name, IObject value)
        {
            store[name] = value;
        }
    }
}
