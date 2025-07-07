using UnityEngine;
using Vuforia;
using System;

public class VuforiaTargetEvents : MonoBehaviour
{
    // Events to expose
    public event Action<ObserverBehaviour> OnTargetFound;
    public event Action<ObserverBehaviour> OnTargetLost;
    public event Action<ObserverBehaviour, TargetStatus> OnTargetUpdated;
    public event Action<ObserverBehaviour, TargetStatus, TargetStatus> OnTargetStatusChanged;

    private TargetStatus previousStatus;

    void Awake()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
        {
            previousStatus = observer.TargetStatus;
            observer.OnTargetStatusChanged += HandleTargetStatusChanged;
        }
    }

    void OnDestroy()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
        {
            observer.OnTargetStatusChanged -= HandleTargetStatusChanged;
        }
    }

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