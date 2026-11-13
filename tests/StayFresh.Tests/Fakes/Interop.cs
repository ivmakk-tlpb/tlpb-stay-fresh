// Fake IL2CPP interop types and the fake IL2CPP collections.

namespace Il2CppInterop.Runtime.InteropTypes
{
    public class Il2CppObjectBase
    {
        private static long nextPointer = 0x10000;

        public IntPtr Pointer { get; } = new IntPtr(nextPointer += 8);

        public T TryCast<T>() where T : Il2CppObjectBase => this as T;

        // The boxed value of an IL2CPP object of a value type.
        public T Unbox<T>() where T : unmanaged => (T)((Il2CppSystem.Object)this).Value;
    }
}

namespace Il2CppSystem
{
    // A boxed IL2CPP value, with the name of its IL2CPP type.
    public class Object : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        private static readonly System.Collections.Generic.Dictionary<IntPtr, Object> byPointer = new System.Collections.Generic.Dictionary<IntPtr, Object>();

        internal readonly object Value;

        public Object(object value)
        {
            Value = value;
            byPointer[Pointer] = this;
        }

        internal static Object Find(IntPtr pointer) => byPointer.TryGetValue(pointer, out var found) ? found : null;

        public Type GetIl2CppType() => new Type(Value switch
        {
            float => "Single",
            double => "Double",
            int => "Int32",
            string => "String",
            _ => Value.GetType().Name,
        });
    }

    public class Type
    {
        public Type(string name) { Name = name; }

        public string Name { get; }
    }

}

namespace Il2CppInterop.Runtime
{
    public static class IL2CPP
    {
        // The managed text of a native string, here the boxed text with that pointer.
        public static string Il2CppStringToManaged(IntPtr pointer) => Il2CppSystem.Object.Find(pointer)?.Value as string;
    }
}

namespace Il2CppSystem.Collections.Generic
{
    public class List<T> : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        private readonly System.Collections.Generic.List<T> items = new System.Collections.Generic.List<T>();

        public int Count => items.Count;
        public T this[int index] => items[index];

        public void Add(T item) { items.Add(item); }
    }

    // Keeps the order of Add, as a test expects.
    public class Dictionary<TKey, TValue> : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        private readonly System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<TKey, TValue>> entries =
            new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<TKey, TValue>>();

        public void Add(TKey key, TValue value) { entries.Add(new System.Collections.Generic.KeyValuePair<TKey, TValue>(key, value)); }

        public Enumerator GetEnumerator() => new Enumerator(entries);

        public sealed class Enumerator
        {
            private readonly System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<TKey, TValue>> entries;
            private int index = -1;

            public Enumerator(System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<TKey, TValue>> entries) { this.entries = entries; }

            public System.Collections.Generic.KeyValuePair<TKey, TValue> Current => entries[index];
            public bool MoveNext() => ++index < entries.Count;
        }
    }
}
