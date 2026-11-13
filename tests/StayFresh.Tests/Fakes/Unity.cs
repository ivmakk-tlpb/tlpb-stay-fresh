// Fake Unity types: only the members that the mod source reaches.

namespace UnityEngine
{
    public static class Time
    {
        public static int frameCount;
        public static float realtimeSinceStartup;
    }

    public class Object : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        // The objects that FindObjectOfType finds. A test adds them.
        public static readonly System.Collections.Generic.List<Object> Scene = new System.Collections.Generic.List<Object>();

        public static T FindObjectOfType<T>() where T : Object
        {
            foreach (var o in Scene)
                if (o is T found) return found;
            return null;
        }
    }

    public class GameObject : Object
    {
        public bool activeSelf = true;
    }

    // A transform with its children in sibling order and the components of its game object.
    public class Transform : Object
    {
        private readonly System.Collections.Generic.List<Transform> children = new System.Collections.Generic.List<Transform>();
        public readonly System.Collections.Generic.List<Component> Components = new System.Collections.Generic.List<Component>();
        public readonly GameObject gameObject = new GameObject();

        public Transform parent { get; private set; }
        public int childCount => children.Count;

        public Transform GetChild(int index) => children[index];

        public void Add(Transform child)
        {
            child.parent = this;
            children.Add(child);
        }

        public int GetSiblingIndex() => parent.children.IndexOf(this);

        public void SetSiblingIndex(int index)
        {
            parent.children.Remove(this);
            parent.children.Insert(index, this);
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var c in Components)
                if (c is T found) return found;
            return null;
        }
    }

    public class Component : Object
    {
        public Component()
        {
            transform = new Transform();
            transform.Components.Add(this);
        }

        public Transform transform { get; }
        public GameObject gameObject => transform.gameObject;
    }
}

namespace TMPro
{
    public class TextMeshProUGUI : UnityEngine.Component
    {
        public string text;
    }
}
