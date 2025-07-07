using UnityEngine;
using Vuforia;
using System;

/// <summary>
/// This MonoBehaviour acts as an event dispatcher for Vuforia target status changes.
/// It listens for changes in the tracking status of Vuforia ObserverBehaviours (such as Image Targets or Model Targets)
/// and exposes these changes as C# events that other scripts can subscribe to.
/// This enables decoupled and modular handling of AR target detection, loss, and updates,
/// which is essential for building interactive AR experiences where UI or logic must respond to target visibility.
/// </summary>
[RequireComponent(typeof(VuforiaManager))]
public class VuforiaTargetEvents : MonoBehaviour
{
    /// <summary>
    /// Event triggered when a Vuforia target is found (i.e., when tracking status transitions from NO_POSE to TRACKED or EXTENDED_TRACKED).
    /// Subscribers can use this event to activate AR content or UI when a target becomes visible.
    /// </summary>
    public event Action<ObserverBehaviour> OnTargetFound;
    /// <summary>
    /// Event triggered when a Vuforia target is lost (i.e., when tracking status transitions from TRACKED/EXTENDED_TRACKED to NO_POSE).
    /// Subscribers can use this event to hide AR content or UI when a target is no longer visible.
    /// </summary>
    public event Action<ObserverBehaviour> OnTargetLost;
    /// <summary>
    /// Event triggered whenever a Vuforia target's status is updated, regardless of the specific transition.
    /// This can be used for continuous feedback or analytics.
    /// </summary>
    public event Action<ObserverBehaviour, TargetStatus> OnTargetUpdated;
    /// <summary>
    /// Event triggered on any change in the target's status, providing both the previous and new status.
    /// This allows for detailed tracking of status transitions for debugging or advanced logic.
    /// </summary>
    public event Action<ObserverBehaviour, TargetStatus, TargetStatus> OnTargetStatusChanged;

    /// <summary>
    /// Stores the previous tracking status of the observed target.
    /// This is used to detect transitions between different tracking states.
    /// </summary>
    private TargetStatus previousStatus;

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Subscribes to the Vuforia ObserverBehaviour's OnTargetStatusChanged event to monitor tracking changes.
    /// </summary>
    void Start()
    {
        var observer = FindAnyObjectByType<ObserverBehaviour>();


        previousStatus = observer.TargetStatus;
        observer.OnTargetStatusChanged += HandleTargetStatusChanged;

    }

    /// <summary>
    /// Unity lifecycle method called when the MonoBehaviour will be destroyed.
    /// Unsubscribes from the Vuforia ObserverBehaviour's event to prevent memory leaks or null references.
    /// </summary>
    void OnDestroy()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
        {
            observer.OnTargetStatusChanged -= HandleTargetStatusChanged;
        }
    }

    /// <summary>
    /// Handles the logic for all Vuforia target status transitions.
    /// Invokes the appropriate C# events based on the change in tracking status, enabling other scripts to react accordingly.
    /// This method distinguishes between target found, lost, updated, and general status changes.
    /// </summary>
    /// <param name="behaviour">The Vuforia ObserverBehaviour whose status has changed.</param>
    /// <param name="status">The new tracking status of the target.</param>
    private void HandleTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        // OnTargetStatusChanged (previous, new)
        OnTargetStatusChanged?.Invoke(behaviour, previousStatus, status);

        // OnTargetFound
        if ((previousStatus.Status == Status.NO_POSE) &&
            (status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED))
        {
            OnTargetFound?.Invoke(behaviour);
        }
        // OnTargetLost
        else if ((previousStatus.Status == Status.TRACKED || previousStatus.Status == Status.EXTENDED_TRACKED) &&
                 status.Status == Status.NO_POSE)
        {
            OnTargetLost?.Invoke(behaviour);
        }

        // OnTargetUpdated
        OnTargetUpdated?.Invoke(behaviour, status);

        previousStatus = status;
    }
}