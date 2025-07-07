using UnityEngine;
using Vuforia;

/// <summary>
/// This MonoBehaviour manages the interaction between Vuforia image targets and the application's UI.
/// It listens for Vuforia target detection and loss events, and controls the visibility of a UI panel that displays information about the detected target.
/// The script demonstrates how to bridge AR tracking events with user interface elements, which is a common requirement in AR tourism and educational apps.
/// </summary>
public class VuforiaManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the Vuforia ImageTargetBehaviour component in the scene.
    /// This component is responsible for detecting and tracking a specific image target in the real world.
    /// </summary>
    public ImageTargetBehaviour imageTargetBehaviour;
    /// <summary>
    /// Reference to the GameObject representing the UI panel that displays information about the currently detected target.
    /// This panel is shown or hidden based on the target's tracking status.
    /// </summary>
    public GameObject targetInfoPanel; // Reference to the UI panel that displays target information

    /// <summary>
    /// Holds a reference to the VuforiaTargetEvents component, which dispatches target tracking events.
    /// </summary>
    private VuforiaTargetEvents vuforiaTargetEvents;

    /// <summary>
    /// Unity lifecycle method called on the frame when the script is enabled.
    /// Sets up event subscriptions for Vuforia target found and lost events, and ensures the info panel is initially hidden.
    /// This method demonstrates best practices for initializing AR event handling and UI state.
    /// </summary>
    void Start()
    {

        if (imageTargetBehaviour != null)
        {
            // Add or get the VuforiaTargetEvents component
            vuforiaTargetEvents = imageTargetBehaviour.GetComponent<VuforiaTargetEvents>();
            if (vuforiaTargetEvents == null)
                vuforiaTargetEvents = imageTargetBehaviour.gameObject.AddComponent<VuforiaTargetEvents>();

            // Subscribe to target events
            vuforiaTargetEvents.OnTargetFound += TargetFound;
            vuforiaTargetEvents.OnTargetLost += TargetLost;
            // Optionally subscribe to OnTargetUpdated and OnTargetStatusChanged if needed
        }


        // Ensure the target info panel is initially hidden
        if (targetInfoPanel == null)
        {
            // Try to auto-assign by searching for a GameObject named "TargetInfoPanel"
            var foundPanel = GameObject.Find("TargetInfoPanel");
            if (foundPanel != null)
            {
                targetInfoPanel = foundPanel;
                Debug.LogWarning("Target info panel was not assigned in the inspector. Found and assigned GameObject named 'TargetInfoPanel'.");
            }
        }

        if (targetInfoPanel != null)
        {
            targetInfoPanel.SetActive(false);
        }

    }

    /// <summary>
    /// Unity lifecycle method called when the MonoBehaviour will be destroyed.
    /// Cleans up event subscriptions to prevent memory leaks and unintended behavior.
    /// </summary>
    void OnDestroy()
    {
        if (vuforiaTargetEvents != null)
        {
            vuforiaTargetEvents.OnTargetFound -= TargetFound;
            vuforiaTargetEvents.OnTargetLost -= TargetLost;
        }
    }

    /// <summary>
    /// Callback method invoked when a Vuforia target is detected and tracked.
    /// Activates the target information panel and logs the event for debugging or analytics.
    /// </summary>
    /// <param name="observer">The ObserverBehaviour representing the detected target.</param>
    void TargetFound(ObserverBehaviour observer)
    {
        // Display the target information panel when a target is found
        if (targetInfoPanel != null)
            targetInfoPanel.SetActive(true);

        // Optionally, log or use observer.TargetName if needed
        Debug.Log("Target found: " + observer.TargetName);
    }

    /// <summary>
    /// Callback method invoked when a Vuforia target is lost (no longer tracked).
    /// Deactivates the target information panel and logs the event for debugging or analytics.
    /// </summary>
    /// <param name="observer">The ObserverBehaviour representing the lost target.</param>
    void TargetLost(ObserverBehaviour observer)
    {
        // Hide the target information panel when a target is lost
        if (targetInfoPanel != null)
            targetInfoPanel.SetActive(false);

        Debug.Log("Target lost: " + observer.TargetName);
    }
}
