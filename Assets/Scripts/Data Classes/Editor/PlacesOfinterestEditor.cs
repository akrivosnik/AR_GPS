using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO; // For file IO
using System;    // For Exception
using UnityEngine.Video; // Add this for VideoClip

/// <summary>
/// Custom Unity Editor inspector for the "PlacesOfinterest" ScriptableObject.
/// This editor extension provides a user-friendly interface within the Unity Editor for managing a collection of places of interest.
/// It allows designers and developers to add, edit, remove, and search for places directly from the Inspector window.
/// The editor supports editing all properties of each place, including name, description, address, latitude, longitude, height, and an associated icon.
/// This tool streamlines the workflow for content creation and data management in location-based AR applications.
/// </summary>
[CustomEditor(typeof(PlacesOfinterest))]
public class PlacesOfinterestEditor : Editor
{
    /// <summary>
    /// Stores the input value for the name of a new place to be added.
    /// </summary>
    private string newPlaceName = "";
    /// <summary>
    /// Stores the input value for the description of a new place to be added.
    /// </summary>
    private string newPlaceDescription = "";
    /// <summary>
    /// Stores the input value for the address of a new place to be added.
    /// </summary>
    private string newPlaceAddress = "";
    /// <summary>
    /// Stores the input value for the latitude coordinate of a new place to be added.
    /// </summary>
    private float newPlaceLatitude = 0f;
    /// <summary>
    /// Stores the input value for the longitude coordinate of a new place to be added.
    /// </summary>
    private float newPlaceLongitude = 0f;
    /// <summary>
    /// Stores the input value for the height (altitude) of a new place to be added.
    /// </summary>
    private float newPlaceHeight = 0f;
    /// <summary>
    /// Stores the input value for the icon (sprite) of a new place to be added.
    /// </summary>
    private Sprite newPlaceIcon = null;
    /// <summary>
    /// Stores the input value for the video of a new place to be added.
    /// </summary>
    private VideoClip newPlaceVideo = null;

    /// <summary>
    /// Stores the input value for searching a place by its name.
    /// </summary>
    private string searchName = "";
    /// <summary>
    /// Holds a reference to the place found by the search operation.
    /// </summary>
    private PlaceOfinterest foundPlace = null;

    /// <summary>
    /// Custom GUI style for section headers in the inspector.
    /// </summary>
    private GUIStyle headerStyle;
    /// <summary>
    /// Custom GUI style for boxed sections in the inspector.
    /// </summary>
    private GUIStyle boxStyle;
    /// <summary>
    /// Custom GUI style for buttons in the inspector.
    /// </summary>
    private GUIStyle buttonStyle;
    /// <summary>
    /// Custom GUI style for labels in the inspector.
    /// </summary>
    private GUIStyle labelStyle;

    /// <summary>
    /// Initializes custom GUI styles for the inspector if they have not already been created.
    /// This method ensures consistent and visually appealing formatting for all custom UI elements.
    /// </summary>
    private void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                normal = { textColor = new Color(0.2f, 0.5f, 0.8f) }
            };
        }
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 5, 5)
            };
        }
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                fixedHeight = 28
            };
        }
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12
            };
        }
    }

    // Foldout and scroll state
    private bool placesFoldout = true;
    private Vector2 placesScroll;
    private List<bool> expandedStates = new List<bool>();

    /// <summary>
    /// Overrides the default Unity inspector GUI for the "PlacesOfinterest" ScriptableObject.
    /// Provides a comprehensive interface for viewing, editing, adding, and searching places of interest.
    /// The inspector displays all existing places in a scrollable list, each with editable fields for all properties.
    /// Users can add new places by filling out the provided form and clicking the "Add Place" button.
    /// The search section allows users to quickly locate a place by name and view its details.
    /// All changes are recorded for undo functionality and marked as dirty for proper asset saving.
    /// </summary>
    public override void OnInspectorGUI()
    {
        InitStyles();
        PlacesOfinterest poiAsset = (PlacesOfinterest)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📍 Places Of Interest", headerStyle);
        EditorGUILayout.Space();

        // Backup/Restore buttons in a horizontal group, centered
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("⬇ Backup to JSON", GUILayout.Width(140), GUILayout.Height(24)))
        {
            string path = EditorUtility.SaveFilePanel("Backup Places Of Interest", "", "places_backup.json", "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = JsonUtility.ToJson(new SerializationWrapper<PlaceOfinterest>(poiAsset.placesOfInterest), true);
                    File.WriteAllText(path, json);
                    EditorUtility.DisplayDialog("Backup Complete", "Places backed up to JSON successfully.", "OK");
                }
                catch (Exception ex)
                {
                    EditorUtility.DisplayDialog("Backup Failed", ex.Message, "OK");
                }
            }
        }
        if (GUILayout.Button("⬆ Restore from JSON", GUILayout.Width(140), GUILayout.Height(24)))
        {
            string path = EditorUtility.OpenFilePanel("Restore Places Of Interest", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var wrapper = JsonUtility.FromJson<SerializationWrapper<PlaceOfinterest>>(json);
                    if (wrapper != null && wrapper.items != null)
                    {
                        Undo.RecordObject(poiAsset, "Restore Places");
                        poiAsset.placesOfInterest = new List<PlaceOfinterest>(wrapper.items);
                        EditorUtility.SetDirty(poiAsset);
                        expandedStates = new List<bool>(new bool[poiAsset.placesOfInterest.Count]);
                        EditorUtility.DisplayDialog("Restore Complete", "Places restored from JSON successfully.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    EditorUtility.DisplayDialog("Restore Failed", ex.Message, "OK");
                }
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Foldout for places list
        placesFoldout = EditorGUILayout.Foldout(placesFoldout, $"Places List ({poiAsset.placesOfInterest.Count})", true, headerStyle);

        // Expand/Collapse All
        if (placesFoldout && poiAsset.placesOfInterest.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Expand All", GUILayout.Width(90)))
            {
                EnsureExpandedStates(poiAsset.placesOfInterest.Count);
                for (int i = 0; i < expandedStates.Count; i++) expandedStates[i] = true;
            }
            if (GUILayout.Button("Collapse All", GUILayout.Width(90)))
            {
                EnsureExpandedStates(poiAsset.placesOfInterest.Count);
                for (int i = 0; i < expandedStates.Count; i++) expandedStates[i] = false;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // List all places in scroll view
        if (placesFoldout)
        {
            if (poiAsset.placesOfInterest.Count == 0)
            {
                EditorGUILayout.HelpBox("No places added yet.", MessageType.Info);
            }
            else
            {
                EnsureExpandedStates(poiAsset.placesOfInterest.Count);
                placesScroll = EditorGUILayout.BeginScrollView(placesScroll, GUILayout.MinHeight(180), GUILayout.MaxHeight(320));
                for (int i = 0; i < poiAsset.placesOfInterest.Count; i++)
                {
                    var place = poiAsset.placesOfInterest[i];
                    expandedStates[i] = EditorGUILayout.Foldout(expandedStates[i], $"#{i + 1}  {place.Name}", true, EditorStyles.foldoutHeader);
                    if (expandedStates[i])
                    {
                        EditorGUILayout.BeginVertical(boxStyle);

                        // Top row: Remove button, Name, Icon
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("🗑", GUILayout.Width(28), GUILayout.Height(28)))
                        {
                            Undo.RecordObject(poiAsset, "Remove Place");
                            poiAsset.placesOfInterest.RemoveAt(i);
                            EditorUtility.SetDirty(poiAsset);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            expandedStates.RemoveAt(i);
                            i--;
                            continue;
                        }
                        EditorGUILayout.LabelField("Name", GUILayout.Width(38));
                        place.Name = EditorGUILayout.TextField(place.Name, GUILayout.Width(120));
                        GUILayout.FlexibleSpace();
                        place.Icon = (Sprite)EditorGUILayout.ObjectField(place.Icon, typeof(Sprite), false, GUILayout.Width(60), GUILayout.Height(60));
                        EditorGUILayout.EndHorizontal();

                        // Video
                        place.Video = (VideoClip)EditorGUILayout.ObjectField("Video", place.Video, typeof(VideoClip), false);

                        // Description
                        EditorGUILayout.LabelField("Description", labelStyle);
                        place.Description = EditorGUILayout.TextArea(place.Description, GUILayout.MinHeight(80), GUILayout.MaxHeight(120));

                        // Address
                        place.Address = EditorGUILayout.TextField("Address", place.Address);

                        // Lat/Lon/Height row
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Lat", GUILayout.Width(24));
                        EditorGUILayout.SelectableLabel(place.Latitude.ToString("F6"), EditorStyles.textField, GUILayout.Width(70));
                        if (GUILayout.Button("Copy", GUILayout.Width(36)))
                            EditorGUIUtility.systemCopyBuffer = place.Latitude.ToString("F6");
                        EditorGUILayout.LabelField("Lon", GUILayout.Width(24));
                        EditorGUILayout.SelectableLabel(place.Longitude.ToString("F6"), EditorStyles.textField, GUILayout.Width(70));
                        if (GUILayout.Button("Copy", GUILayout.Width(36)))
                            EditorGUIUtility.systemCopyBuffer = place.Longitude.ToString("F6");
                        EditorGUILayout.LabelField("H", GUILayout.Width(14));
                        place.Height = EditorGUILayout.FloatField(place.Height, GUILayout.Width(48));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(2);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        // Add new place section
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("➕ Add New Place", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", GUILayout.Width(38));
        newPlaceName = EditorGUILayout.TextField(newPlaceName, GUILayout.Width(120));
        GUILayout.FlexibleSpace();
        newPlaceIcon = (Sprite)EditorGUILayout.ObjectField(newPlaceIcon, typeof(Sprite), false, GUILayout.Width(60), GUILayout.Height(60));
        EditorGUILayout.EndHorizontal();

        // Video
        newPlaceVideo = (VideoClip)EditorGUILayout.ObjectField("Video", newPlaceVideo, typeof(VideoClip), false);

        EditorGUILayout.LabelField("Description", labelStyle);
        newPlaceDescription = EditorGUILayout.TextArea(newPlaceDescription, GUILayout.MinHeight(80), GUILayout.MaxHeight(120));
        newPlaceAddress = EditorGUILayout.TextField("Address", newPlaceAddress);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Lat", GUILayout.Width(24));
        newPlaceLatitude = EditorGUILayout.FloatField(newPlaceLatitude, GUILayout.Width(70));
        EditorGUILayout.LabelField("Lon", GUILayout.Width(24));
        newPlaceLongitude = EditorGUILayout.FloatField(newPlaceLongitude, GUILayout.Width(70));
        EditorGUILayout.LabelField("H", GUILayout.Width(14));
        newPlaceHeight = EditorGUILayout.FloatField(newPlaceHeight, GUILayout.Width(48));
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUILayout.Button("Add Place", GUILayout.Height(28)))
        {
            var newPlace = new PlaceOfinterest
            {
                Name = newPlaceName,
                Description = newPlaceDescription,
                Address = newPlaceAddress,
                Latitude = newPlaceLatitude,
                Longitude = newPlaceLongitude,
                Height = newPlaceHeight,
                Icon = newPlaceIcon,
                Video = newPlaceVideo
            };
            Undo.RecordObject(poiAsset, "Add Place");
            poiAsset.AddPlace(newPlace);
            EditorUtility.SetDirty(poiAsset);
            newPlaceName = "";
            newPlaceDescription = "";
            newPlaceAddress = "";
            newPlaceLatitude = 0f;
            newPlaceLongitude = 0f;
            newPlaceHeight = 0f;
            newPlaceIcon = null;
            newPlaceVideo = null;
            expandedStates.Add(true);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        // Search section
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("🔎 Find Place By Name", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        searchName = EditorGUILayout.TextField("Search Name", searchName);
        if (GUILayout.Button("Find", GUILayout.Height(24)))
        {
            foundPlace = poiAsset.GetPlaceByName(searchName);
        }
        if (foundPlace != null)
        {
            EditorGUILayout.HelpBox(
                $"Found: {foundPlace.Name}\nDescription: {foundPlace.Description}", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Lat", GUILayout.Width(24));
            EditorGUILayout.SelectableLabel(foundPlace.Latitude.ToString("F6"), EditorStyles.textField, GUILayout.Width(70));
            if (GUILayout.Button("Copy", GUILayout.Width(36)))
                EditorGUIUtility.systemCopyBuffer = foundPlace.Latitude.ToString("F6");
            EditorGUILayout.LabelField("Lon", GUILayout.Width(24));
            EditorGUILayout.SelectableLabel(foundPlace.Longitude.ToString("F6"), EditorStyles.textField, GUILayout.Width(70));
            if (GUILayout.Button("Copy", GUILayout.Width(36)))
                EditorGUIUtility.systemCopyBuffer = foundPlace.Longitude.ToString("F6");
            EditorGUILayout.LabelField("H", GUILayout.Width(14));
            EditorGUILayout.SelectableLabel(foundPlace.Height.ToString("F2"), EditorStyles.textField, GUILayout.Width(48));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(poiAsset);
        }
    }

    // Ensure expandedStates matches the number of places
    private void EnsureExpandedStates(int count)
    {
        while (expandedStates.Count < count) expandedStates.Add(true);
        while (expandedStates.Count > count) expandedStates.RemoveAt(expandedStates.Count - 1);
    }
}

// Helper for serializing lists with JsonUtility
[Serializable]
public class SerializationWrapper<T>
{
    public List<T> items;
    public SerializationWrapper(List<T> items) { this.items = items; }
}
