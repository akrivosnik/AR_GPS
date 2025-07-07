using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unity ScriptableObject that serves as a persistent data container for a collection of places of interest.
/// This class is designed to be used as a centralized database for storing and managing multiple "PlaceOfinterest" entries,
/// which represent real-world locations with associated metadata such as name, description, address, coordinates, and an icon.
/// The ScriptableObject can be edited in the Unity Editor and referenced by other scripts at runtime, enabling efficient data-driven workflows
/// for location-based AR or tourism applications.
/// </summary>
[CreateAssetMenu(fileName = "Empty Data", menuName = "Places Of interest")]
public class PlacesOfinterest : ScriptableObject
{
    /// <summary>
    /// The main list containing all "PlaceOfinterest" objects managed by this ScriptableObject.
    /// Each entry in this list represents a unique point of interest with its own metadata and geospatial information.
    /// </summary>
    public List<PlaceOfinterest> placesOfInterest = new List<PlaceOfinterest>();

    /// <summary>
    /// Adds a new "PlaceOfinterest" to the collection if it does not already exist.
    /// This method prevents duplicate entries and ensures data integrity.
    /// </summary>
    /// <param name="place">The PlaceOfinterest instance to add to the list.</param>
    public void AddPlace(PlaceOfinterest place)
    {
        if (place != null && !placesOfInterest.Contains(place))
        {
            placesOfInterest.Add(place);
        }
    }

    /// <summary>
    /// Removes an existing "PlaceOfinterest" from the collection if it is present.
    /// This method is used to delete places from the database, supporting dynamic content management.
    /// </summary>
    /// <param name="place">The PlaceOfinterest instance to remove from the list.</param>
    public void RemovePlace(PlaceOfinterest place)
    {
        if (place != null && placesOfInterest.Contains(place))
        {
            placesOfInterest.Remove(place);
        }
    }

    /// <summary>
    /// Searches for a PlaceOfinterest in the collection by its name.
    /// Returns the first matching entry or null if no match is found.
    /// This method enables efficient lookup of places for editing, navigation, or display purposes.
    /// </summary>
    /// <param name="name">The name of the place to search for.</param>
    /// <returns>The matching PlaceOfinterest instance, or null if not found.</returns>
    public PlaceOfinterest GetPlaceByName(string name)
    {
        return placesOfInterest.Find(place => place.Name == name);
    }
}
