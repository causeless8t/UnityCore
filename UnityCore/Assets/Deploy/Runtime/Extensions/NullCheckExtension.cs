using JetBrains.Annotations;

namespace Causeless3t.Core
{
    public static class NullCheckExtension
    {
        public static bool IsReferenceNull([CanBeNull] this UnityEngine.Object obj) => ReferenceEquals(obj, null);

        // ReSharper restore once Unity.ExpensiveCode
        public static bool IsUnityNull([CanBeNull] this UnityEngine.Object obj) => !obj;

#if UNITY_EDITOR
        public static bool IsMissing([CanBeNull] this UnityEngine.Object obj) => !obj.IsReferenceNull() && obj.IsUnityNull();
#endif
    }
}
