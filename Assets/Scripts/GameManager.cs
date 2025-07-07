using UnityEngine;
using CesiumForUnity;
using System.Collections;
using UnityEngine.SceneManagement; // Added for scene loading
using System;
using TMPro;

/// <summary>
/// The GameManager class is a singleton MonoBehaviour that acts as the central controller for the application's global state and logic.
/// It manages references to key components such as the CesiumGlobeAnchor (for geospatial AR placement), the current GPS coordinates,
/// the collection of places of interest, and UI elements. The GameManager is responsible for orchestrating proximity checks between the user's location
/// and stored places, updating UI elements, handling scene transitions, and synchronizing geocoded data.
/// This class demonstrates best practices for centralized state management in Unity-based AR tourism applications, enabling modularity and scalability.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the CesiumGlobeAnchor component, which is used to anchor virtual content to real-world geospatial coordinates.
    /// This enables accurate placement of AR objects based on latitude, longitude, and height.
    /// </summary>
    public CesiumGlobeAnchor cesiumGlobeAnchor;

    /// <summary>
    /// The current latitude value, typically updated by the GPSLocation component.
    /// This value is used for proximity calculations and AR content placement.
    /// </summary>
    public float currentLatitude = 37.7749f; // Example: San Francisco latitude

    /// <summary>
    /// The current longitude value, typically updated by the GPSLocation component.
    /// This value is used for proximity calculations and AR content placement.
    /// </summary>
    public float currentLongitude = -122.4194f; // Example: San Francisco longitude

    /// <summary>
    /// Reference to the currently active or nearby place of interest.
    /// This field is updated when the user is within a certain distance of a stored place.
    /// </summary>
    [Header("Local Place of Interest")]
    public PlaceOfinterest loacalPlaceOfInterest; // Reference to the local PlaceOfinterest scriptable object

    /// <summary>
    /// Reference to the ScriptableObject containing the collection of all places of interest.
    /// This enables centralized management and querying of location data.
    /// </summary>
    public PlacesOfinterest placesOfinterest; // Reference to the PlaceOfinterest scriptable object

    /// <summary>
    /// Reference to the GeocodeAddress component, used for converting addresses to coordinates.
    /// This enables dynamic updating of place data based on user input or external sources.
    /// </summary>
    public GeocodeAddress geocodeAddress; // Reference to the GeocodeAddress component

    /// <summary>
    /// Singleton instance of the GameManager, ensuring only one instance exists throughout the application's lifecycle.
    /// This pattern facilitates global access to game state and logic.
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Event invoked whenever the currently active place of interest is updated.
    /// Other components can subscribe to this event to react to changes in proximity or selection.
    /// </summary>
    public Action<PlaceOfinterest> OnPlaceOfInterestUpdated; // Action to notify when a place of interest is updated

    /// <summary>
    /// Reference to the TextMeshProUGUI component used to display the name of the current place of interest in the UI.
    /// </summary>
    [Header("UI Stuff")]
    public TextMeshProUGUI placeNameText; // Reference to the UI TextMeshProUGUI for displaying the place name

    /// <summary>
    /// Reference to the GameObject representing the map pin or marker in the scene.
    /// This object is shown or hidden based on scene transitions and user interactions.
    /// </summary>
    public GameObject pin;

    /// <summary>
    /// Reference to the GameObject representing the AR panel UI.
    /// This panel is activated when the user is near a place of interest and deactivated otherwise.
    /// </summary>
    public GameObject arPanel;

    public GameObject POIUI;

    public GameObject POIList;

    public GameObject mainCanvas;


    [Header("Cesium Geo Reference")]
    public CesiumGeoreference cesiumGeoreference; // Reference to the CesiumGeoreference component

    /// <summary>
    /// Unity lifecycle method called when the script is enabled.
    /// Subscribes the UpdateButtonUI method to the OnPlaceOfInterestUpdated event, ensuring the UI is refreshed when the active place changes.
    /// </summary>
    void OnEnable()
    {
        OnPlaceOfInterestUpdated += UpdateButtonUI; // Subscribe to the event
    }

    /// <summary>
    /// Unity lifecycle method called when the script is disabled.
    /// Unsubscribes the UpdateButtonUI method from the OnPlaceOfInterestUpdated event to prevent memory leaks.
    /// </summary>
    void OnDisable()
    {
        OnPlaceOfInterestUpdated -= UpdateButtonUI; // Unsubscribe from the event
    }

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Implements the singleton pattern by ensuring only one instance of GameManager exists and persists across scene loads.
    /// </summary>
    void Awake()
    {
        // Ensure that there is only one instance of GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

    }

    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Starts coroutines for updating geocoded data and checking proximity to places of interest.
    /// Also ensures the AR panel is initially hidden.
    /// </summary>
    void Start()
    {
        // Prevent device from sleeping/locking
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        StartCoroutine(UpdateAllPlacesLatLonFromAddress());
        StartCoroutine(ProximityCheckCoroutine());

        arPanel.SetActive(false); // Initially deactivate the AR panel


        foreach (PlaceOfinterest placeOfinterest in placesOfinterest.placesOfInterest)
        {
            PlaceOfInterestUI clone = Instantiate(POIUI, POIList.transform).GetComponent<PlaceOfInterestUI>(); // Instantiate the POI UI for each place of interest

            clone.SetPlaceName(placeOfinterest.Name); // Set the place name in the UI
        }

    }


    /// <summary>
    /// Coroutine that periodically checks the user's proximity to all stored places of interest.
    /// If the user is within a specified distance (e.g., 10 meters) of a place, the AR panel is activated and the place is set as active.
    /// This enables context-aware AR experiences based on real-world location.
    /// </summary>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    private IEnumerator ProximityCheckCoroutine()
    {
        while (true)
        {
            CheckProximityToPlaces();
            yield return new WaitForSeconds(1f); // Check every 1 second
        }
    }

    /// <summary>
    /// Coroutine that iterates through all places of interest and updates their latitude and longitude fields by geocoding their addresses.
    /// This ensures that all places have valid coordinates for AR placement and proximity calculations.
    /// </summary>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    private IEnumerator UpdateAllPlacesLatLonFromAddress()
    {
        if (geocodeAddress == null)
        {
            geocodeAddress = GetComponent<GeocodeAddress>();
            if (geocodeAddress == null)
            {
                Debug.LogError("No GeocodeAddress component found in the Game Manager.");
                yield break;
            }
        }

        foreach (var place in placesOfinterest.placesOfInterest)
        {
            if ((place.Latitude == 0f && place.Longitude == 0f) && !string.IsNullOrEmpty(place.Address))
            {
                geocodeAddress.latitude = 0f;
                geocodeAddress.longitude = 0f;
                geocodeAddress.resultText = "";
                geocodeAddress.SearchAddress(place.Address);

                // Wait until the geocoding completes (resultText is set or timeout)
                float timeout = 10f;
                float timer = 0f;
                while (geocodeAddress.resultText == "" && timer < timeout)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                // Update place with found coordinates
                place.Latitude = geocodeAddress.latitude;
                place.Longitude = geocodeAddress.longitude;
            }
        }
    }

    /// <summary>
    /// Updates the UI button or label to display the name of the currently active place of interest.
    /// This method is typically called in response to proximity changes or user selection.
    /// </summary>
    /// <param name="placeOfinterest">The place of interest to display in the UI.</param>
    public void UpdateButtonUI(PlaceOfinterest placeOfinterest)
    {
        placeNameText.text = placeOfinterest.Name;
    }

    /// <summary>
    /// Updates the CesiumGlobeAnchor and current coordinates to navigate to a specific place of interest.
    /// This method is used to reposition AR content or the camera based on user selection or navigation requests.
    /// </summary>
    /// <param name="place">The place of interest to navigate to.</param>
    public void NavigateToPlace(PlaceOfinterest place)
    {
        // Logic to navigate to a specific place using the provided latitude and longitude
        Debug.Log($"Navigating to Place at Latitude: {place.Latitude}, Longitude: {place.Longitude}");

        // Here you would typically update the CesiumGlobeAnchor or other navigation logic
        currentLatitude = place.Latitude;
        currentLongitude = place.Longitude;

        cesiumGlobeAnchor.latitude = place.Latitude;
        cesiumGlobeAnchor.longitude = place.Longitude;
        cesiumGlobeAnchor.height = place.Height; // Use the place's height
    }

    /// <summary>
    /// Loads a new Unity scene additively by its name and hides the map pin.
    /// This method is used to transition to AR or detail scenes while preserving the main scene context.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void AddScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        pin.SetActive(false); // Activate the pin when a scene is added
        cesiumGeoreference.gameObject.SetActive(false); // Optionally deactivate the CesiumGeoreference if not needed in the new scene
        mainCanvas.SetActive(false); // Optionally deactivate the main canvas if not needed in the new scene
        POIList.SetActive(false); // Optionally deactivate the POI list if not needed in the new scene
        Debug.Log($"Scene '{sceneName}' loaded.");
    }

    /// <summary>
    /// Unloads a Unity scene by its name and shows the map pin.
    /// This method is used to return from AR or detail scenes to the main map or overview scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene to unload.</param>
    public void RemoveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
            cesiumGeoreference.gameObject.SetActive(true); // Reactivate the CesiumGeoreference if it was deactivated
            pin.SetActive(true); // Deactivate the pin when a scene is removed
            mainCanvas.SetActive(true); // Reactivate the main canvas if it was deactivated
            POIList.SetActive(true); // Reactivate the POI list if it was deactivated
            Debug.Log($"Scene '{sceneName}' unloaded.");
        }
        else
        {
            Debug.LogWarning($"Scene '{sceneName}' is not loaded and cannot be unloaded.");
        }
    }

    /// <summary>
    /// Checks if the user's current location is within a specified distance (e.g., 10 meters) of any stored place of interest.
    /// If a nearby place is found, it is set as the active place, the AR panel is shown, and the OnPlaceOfInterestUpdated event is invoked.
    /// If no places are nearby, the AR panel is hidden and the active place is cleared.
    /// </summary>
    private void CheckProximityToPlaces()
    {
        if (placesOfinterest == null || placesOfinterest.placesOfInterest == null)
            return;

        bool foundNearby = false;
        foreach (var place in placesOfinterest.placesOfInterest)
        {
            if (place.Latitude == 0f && place.Longitude == 0f)
                continue;

            double distance = HaversineDistance(currentLatitude, currentLongitude, place.Latitude, place.Longitude);
            if (distance <= 10.0)
            {
                Debug.Log($"You are within 10 meters of {place.Name}!"); // Added log message
                loacalPlaceOfInterest = place; // Update the local place of interest
                foundNearby = true;

                OnPlaceOfInterestUpdated?.Invoke(place); // Notify subscribers about the updated place of interest

                arPanel.SetActive(true); // Activate the AR panel when within proximity
                break;
            }
        }
        if (!foundNearby)
        {
            loacalPlaceOfInterest = null;
            arPanel.SetActive(false); // Deactivate the AR panel if no places are nearby
        }
    }

    /// <summary>
    /// Calculates the great-circle distance in meters between two geographic coordinates using the Haversine formula.
    /// This method is used for accurate proximity detection in location-based AR applications.
    /// </summary>
    /// <param name="lat1">Latitude of the first point in decimal degrees.</param>
    /// <param name="lon1">Longitude of the first point in decimal degrees.</param>
    /// <param name="lat2">Latitude of the second point in decimal degrees.</param>
    /// <param name="lon2">Longitude of the second point in decimal degrees.</param>
    /// <returns>The distance between the two points in meters.</returns>
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371000; // Earth radius in meters
        double dLat = Mathf.Deg2Rad * (float)(lat2 - lat1);
        double dLon = Mathf.Deg2Rad * (float)(lon2 - lon1);

        double a =
            Mathf.Sin((float)dLat / 2) * Mathf.Sin((float)dLat / 2) +
            Mathf.Cos(Mathf.Deg2Rad * (float)lat1) * Mathf.Cos(Mathf.Deg2Rad * (float)lat2) *
            Mathf.Sin((float)dLon / 2) * Mathf.Sin((float)dLon / 2);

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        double distance = R * c;
        return distance;
    }
}