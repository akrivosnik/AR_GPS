using System;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// A serializable data class that encapsulates all relevant information about a single place of interest.
/// This class is intended to represent real-world locations in AR tourism or mapping applications,
/// providing both geospatial coordinates and descriptive metadata for each location.
/// Each instance can be stored in a "PlacesOfinterest" ScriptableObject and referenced at runtime.
/// </summary>
[Serializable]
public class PlaceOfinterest
{
    /// <summary>
    /// The human-readable name of the place of interest.
    /// This is used for display in the UI and as a unique identifier for searching and referencing.
    /// </summary>
    public string Name;
    /// <summary>
    /// A textual description providing additional information about the place.
    /// This can include historical context, significance, or visitor information.
    /// </summary>
    public string Description;
    /// <summary>
    /// The physical or postal address of the place, if available.
    /// This field can be used for geocoding to obtain latitude and longitude coordinates.
    /// </summary>
    public string Address;
    /// <summary>
    /// The latitude coordinate (in decimal degrees) of the place's geographic location.
    /// Used for positioning in AR scenes and proximity calculations.
    /// </summary>
    public float Latitude;
    /// <summary>
    /// The longitude coordinate (in decimal degrees) of the place's geographic location.
    /// Used for positioning in AR scenes and proximity calculations.
    /// </summary>
    public float Longitude;
    /// <summary>
    /// The height or altitude (in meters) above sea level for the place.
    /// This value is used for accurate 3D placement in AR or mapping environments.
    /// </summary>
    public float Height;

    /// <summary>
    /// A visual icon or sprite representing the place of interest.
    /// This can be displayed in the UI or on AR markers to help users identify the location.
    /// </summary>
    public Sprite Icon;

    /// <summary>
    /// A video clip representing the place of interest.
    /// This can be used to show a video about the location in the UI or AR experience.
    /// </summary>
    public VideoClip Video;
}
