using UnityEngine;

namespace Cascade.Core
{
    // Intentionally small subsystem shells. They reserve scene responsibilities without creating a monolithic GameManager.
    public sealed class GameplayContext : MonoBehaviour { }
    public sealed class PlacementController : MonoBehaviour { }
    public sealed class ObjectiveManager : MonoBehaviour { }
    public sealed class CascadeScoreManager : MonoBehaviour { }
    public sealed class ReactionEventBus : MonoBehaviour { }
    public sealed class FeedbackManager : MonoBehaviour { }
    public sealed class AudioManager : MonoBehaviour { }
    public sealed class HapticManager : MonoBehaviour { }
    public sealed class PoolManager : MonoBehaviour { }
    public sealed class GameplayUIManager : MonoBehaviour { }
}
