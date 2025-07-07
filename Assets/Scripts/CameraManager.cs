using UnityEngine;

/// <summary>
/// The CameraManager MonoBehaviour is responsible for dynamically positioning and orienting the main camera in relation to the CesiumGlobeAnchor.
/// This script ensures that the camera maintains a consistent height above the anchor's position and is oriented to look directly down at the anchor.
/// Such functionality is essential in AR and mapping applications where the camera must follow or focus on a specific geospatial location,
/// providing users with an intuitive and context-aware view of the virtual environment.
/// </summary>
[RequireComponent(typeof(GameManager))]
public class CameraManager : MonoBehaviour
{
    /// <summary>
    /// The vertical offset (in meters) above the CesiumGlobeAnchor at which the camera should be positioned.
    /// This allows for adjustable camera elevation to suit different visualization needs.
    /// </summary>
    public float heightOffset = 200f; // Adjustable height above the anchor

    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Updates the camera's position to be directly above the CesiumGlobeAnchor at the specified height offset,
    /// and sets the camera's rotation to look straight down at the anchor's position.
    /// This ensures a top-down view centered on the current geospatial location.
    /// </summary>
    void Update()
    {
        // Example: Adjust camera position based on GameManager's CesiumGlobeAnchor
        if (GameManager.Instance != null && GameManager.Instance.cesiumGlobeAnchor != null)
        {
            // Set the camera position to the Cesium Globe Anchor's position
            if (GameManager.Instance.currentLatitude != null && GameManager.Instance.currentLongitude != null)
            {
                GameManager.Instance.cesiumGlobeAnchor.latitude = GameManager.Instance.currentLatitude;
                GameManager.Instance.cesiumGlobeAnchor.longitude = GameManager.Instance.currentLongitude;
                GameManager.Instance.cesiumGlobeAnchor.height = heightOffset;
            }

            // Make the camera look straight down at the anchor's position
            Transform anchorTransform = GameManager.Instance.cesiumGlobeAnchor.transform;
            if (anchorTransform != null)
            {
                // Position the camera above the anchor
                transform.position = anchorTransform.position + Vector3.up * heightOffset;
                // Look directly down at the anchor
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }
}
