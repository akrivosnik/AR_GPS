using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;

/// <summary>
/// This MonoBehaviour provides geocoding functionality, allowing the application to convert a human-readable address string
/// into precise geographic coordinates (latitude and longitude) using the OpenStreetMap Nominatim API.
/// It is designed to be used both at runtime and in the Unity Editor for data entry and validation.
/// The script demonstrates how to perform asynchronous web requests, parse JSON responses, and integrate third-party geocoding services into Unity.
/// </summary>
public class GeocodeAddress : MonoBehaviour
{
    /// <summary>
    /// The address string to be geocoded. This field is typically set via the Unity Editor or by other scripts.
    /// </summary>
    [HideInInspector]
    public string addressInput = "";
    /// <summary>
    /// The result of the geocoding operation, formatted as a string for display in the UI or editor.
    /// This field is updated after each geocoding request.
    /// </summary>
    [HideInInspector]
    public string resultText = "";

    /// <summary>
    /// The latitude value obtained from the geocoding service.
    /// This value is set after a successful geocoding request and can be used for AR placement or map visualization.
    /// </summary>
    public float latitude;
    /// <summary>
    /// The longitude value obtained from the geocoding service.
    /// This value is set after a successful geocoding request and can be used for AR placement or map visualization.
    /// </summary>
    public float longitude;

    /// <summary>
    /// Initiates the geocoding process for the specified address.
    /// This method starts a coroutine that sends a request to the Nominatim API and processes the response.
    /// </summary>
    /// <param name="address">The address string to be converted into coordinates.</param>
    public void SearchAddress(string address)
    {
        if (!string.IsNullOrEmpty(address))
        {
            StartCoroutine(GetCoordinatesFromAddress(address));
        }
    }

    /// <summary>
    /// Coroutine that sends an HTTP GET request to the OpenStreetMap Nominatim API to geocode the provided address.
    /// It handles network errors, parses the JSON response, and updates the latitude and longitude fields accordingly.
    /// This method demonstrates best practices for asynchronous web requests and JSON parsing in Unity.
    /// </summary>
    /// <param name="address">The address string to geocode.</param>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    IEnumerator GetCoordinatesFromAddress(string address)
    {
        string url = $"https://nominatim.openstreetmap.org/search?q={UnityWebRequest.EscapeURL(address)}&format=json";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("User-Agent", "UnityGeocodeTest/1.0 (nikosakr99@gmail.com)"); // Replace with your email

        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.LogError("Request error: " + request.error);
            resultText = "Error: " + request.error;
        }
        else
        {
            string json = request.downloadHandler.text;
            GeocodeResult[] results = JsonHelper.FromJson<GeocodeResult>(json);

            if (results != null && results.Length > 0)
            {
                float lat = float.Parse(results[0].lat);
                float lon = float.Parse(results[0].lon);
                latitude = lat;
                longitude = lon;
                resultText = $"Lat: {lat}, Lon: {lon}";
                Debug.Log($"Lat: {lat}, Lon: {lon}");
            }
            else
            {
                resultText = "No results found.";
                latitude = 0f;
                longitude = 0f;
            }
        }
    }

    /// <summary>
    /// Represents a single geocoding result returned by the Nominatim API.
    /// Each result contains latitude and longitude as strings, which are parsed into floats for use in the application.
    /// </summary>
    [System.Serializable]
    public class GeocodeResult
    {
        /// <summary>
        /// The latitude value as a string, as returned by the API.
        /// </summary>
        public string lat;
        /// <summary>
        /// The longitude value as a string, as returned by the API.
        /// </summary>
        public string lon;
    }

    /// <summary>
    /// Static helper class for parsing JSON arrays using Unity's JsonUtility.
    /// Since JsonUtility does not natively support top-level arrays, this class wraps the array in an object for parsing.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Parses a JSON array string into an array of objects of type T.
        /// This method is essential for handling API responses that return arrays at the root level.
        /// </summary>
        /// <typeparam name="T">The type of objects in the array.</typeparam>
        /// <param name="json">The JSON array string to parse.</param>
        /// <returns>An array of objects of type T.</returns>
        public static T[] FromJson<T>(string json)
        {
            string wrapped = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}

// Custom Editor for GeocodeAddress
#if UNITY_EDITOR
/// <summary>
/// Custom Unity Editor inspector for the GeocodeAddress MonoBehaviour.
/// This editor extension provides a convenient interface for testing geocoding functionality directly within the Unity Editor.
/// Users can input an address, trigger the geocoding process, and view the resulting coordinates and status messages.
/// This tool is valuable for validating address data and ensuring correct geocoding before runtime.
/// </summary>
[CustomEditor(typeof(GeocodeAddress))]
public class GeocodeAddressEditor : Editor
{
    /// <summary>
    /// Overrides the default inspector GUI to provide a custom interface for geocoding addresses.
    /// Includes input fields for the address, a search button, and a display area for results.
    /// </summary>
    public override void OnInspectorGUI()
    {
        GeocodeAddress script = (GeocodeAddress)target;

        EditorGUILayout.LabelField("Geocode Address Tester", EditorStyles.boldLabel);

        script.addressInput = EditorGUILayout.TextField("Address", script.addressInput);

        if (GUILayout.Button("Search"))
        {
            script.SearchAddress(script.addressInput);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Result:");
        EditorGUILayout.HelpBox(script.resultText, MessageType.None);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
