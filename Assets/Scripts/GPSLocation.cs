using UnityEngine;
using System.Collections;

/// <summary>
/// This MonoBehaviour is responsible for accessing the device's GPS hardware and retrieving the current geographic coordinates (latitude and longitude).
/// It requests location permissions at runtime (especially on Android), starts the location service, and continuously updates the coordinates.
/// The script also synchronizes the retrieved GPS data with the GameManager, enabling location-based logic such as proximity detection and AR content placement.
/// This component is essential for any AR application that relies on real-world positioning, such as tourism guides or location-based games.
/// </summary>
public class GPSLocation : MonoBehaviour
{
    /// <summary>
    /// The most recently retrieved latitude value from the device's GPS sensor.
    /// This value is updated every frame while the location service is running.
    /// </summary>
    public float latitude;

    /// <summary>
    /// The most recently retrieved longitude value from the device's GPS sensor.
    /// This value is updated every frame while the location service is running.
    /// </summary>
    public float longitude;

    /// <summary>
    /// Unity lifecycle method called once before the first frame update.
    /// Handles requesting location permissions (on Android) and starts the coroutine to initialize the location service.
    /// This ensures that the application has the necessary permissions and that GPS data is available as soon as possible.
    /// </summary>
    void Start()
    {
#if UNITY_ANDROID
        // Request location permission at runtime on Android
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
        }
#endif
        StartCoroutine(StartLocationService());
    }

    /// <summary>
    /// Coroutine that manages the initialization and retrieval of GPS data from the device.
    /// It waits for user permission, checks if location services are enabled, and handles timeouts or errors gracefully.
    /// Upon successful initialization, it retrieves the initial latitude and longitude values.
    /// </summary>
    IEnumerator StartLocationService()
    {
#if UNITY_ANDROID
        // Wait until the user responds to the permission dialog
        while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            Debug.Log("Waiting for location permission...");
            yield return null;
        }
#endif
        // Check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location service is not enabled by user.");
            yield break;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in time
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            Debug.Log("Location: " + latitude + " " + longitude);
        }
    }

    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Continuously updates the latitude and longitude fields with the latest GPS data if the location service is running.
    /// Also updates the GameManager's coordinates to keep the global game state in sync with the device's location.
    /// </summary>
    void Update()
    {
        // Optionally, print location every frame if available
        if (Input.location.status == LocationServiceStatus.Running)
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            Debug.Log("Lat: " + latitude + " Lon: " + longitude);

            // Update GameManager's lat/lon if instance exists
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentLatitude = latitude;
                GameManager.Instance.currentLongitude = longitude;
            }
        }
    }
}
