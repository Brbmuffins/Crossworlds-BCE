using UnityEngine;

// Keep the attachable MonoBehaviour associated with a same-named Unity script
// asset. Its implementation remains alongside EnemyPatrolRoute so the runtime
// behavior is unchanged, while scene/prefab serialization receives a stable
// script reference in player builds.
public sealed partial class EnemyPatrolAgent : MonoBehaviour
{
}
