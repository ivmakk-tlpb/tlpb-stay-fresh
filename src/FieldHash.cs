namespace StayFresh
{
    // The hash that the game's ModelDefinition gives a field name: the String.GetHashCode of the Mono class library that
    // IL2CPP ships. The .NET hash of the mod differs, and the interop call of the IL2CPP method does not give it either.
    public static class FieldHash
    {
        public static int Of(string name)
        {
            unchecked
            {
                int h1 = 5381, h2 = 5381;
                for (int i = 0; i < name.Length; i += 2)
                {
                    h1 = ((h1 << 5) + h1) ^ name[i];
                    if (i + 1 < name.Length) h2 = ((h2 << 5) + h2) ^ name[i + 1];
                }
                return h1 + h2 * 1566083941;
            }
        }
    }
}
