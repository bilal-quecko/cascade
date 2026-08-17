using System;
using UnityEngine;

namespace Cascade.Core
{
    public readonly struct ReactionEvent
    {
        public readonly string eventId;
        public readonly GameObject source;
        public readonly GameObject target;
        public readonly Vector3 worldPosition;
        public readonly float magnitude;

        public ReactionEvent(string eventId, GameObject source, GameObject target, Vector3 worldPosition, float magnitude)
        {
            this.eventId = eventId;
            this.source = source;
            this.target = target;
            this.worldPosition = worldPosition;
            this.magnitude = Mathf.Max(0f, magnitude);
        }
    }

    public sealed class ReactionEventBus : MonoBehaviour
    {
        public event Action<ReactionEvent> EventRaised;

        public void Publish(ReactionEvent evt) => EventRaised?.Invoke(evt);
        public void Publish(string eventId, GameObject source, GameObject target, Vector3 point, float magnitude = 1f)
            => Publish(new ReactionEvent(eventId, source, target, point, magnitude));
    }
}
