using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

/// <summary>
/// The ARUI MonoBehaviour manages user interface interactions specific to the AR experience.
/// In particular, it handles the functionality of the Map button, allowing users to exit the AR scene and return to the main map or overview.
/// This script demonstrates how to connect UI elements to application logic, enabling seamless transitions between AR and non-AR views in tourism or navigation apps.
/// </summary>
public class ARUI : MonoBehaviour
{
    /// <summary>
    /// Reference to the Unity UI Button component representing the Map button.
    /// This button is used by users to exit the AR scene and return to the main map.
    /// </summary>
    public Button MapButton; // Reference to the Map button (remove static)

    public TextMeshProUGUI PlaceName; // Reference to the Map button text
    public TextMeshProUGUI PlaceDescription; // Reference to the Map button description

    public VideoPlayer VideoPlayer; // Reference to the Video Player component for displaying videos related to the place of interest

    public Image PlaceImage; // Reference to the Image component for displaying the icon of the place of interest
    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Subscribes the Map button's click event to the RemoveScene method of the GameManager, ensuring that clicking the button unloads the AR scene.
    /// This enables intuitive navigation and scene management for end users.
    /// </summary>
    void Start()
    {
        StartCoroutine(SetupMapButton());
    }

    private System.Collections.IEnumerator SetupMapButton()
    {
        if (GameManager.Instance == null)
        {
            Debug.Log("<color=aqua> GameManager instance is not available. Ensure it is initialized before ARUI.</color>");
            yield break; // Exit if GameManager is not available
        }
        yield return new WaitForSeconds(0.1f);
        if (MapButton != null) MapButton.onClick.AddListener(() => GameManager.Instance.RemoveScene("AR")); // Add listener to the Map button

        if (PlaceName != null)
        {
            PlaceName.text = GameManager.Instance.loacalPlaceOfInterest.Name; // Set the place name text
        }
        if (PlaceDescription != null)
        {
            PlaceDescription.text = GameManager.Instance.loacalPlaceOfInterest.Description; // Set the place description text
        }
        if (VideoPlayer != null && GameManager.Instance.loacalPlaceOfInterest.Video != null)
        {
            VideoPlayer.clip = GameManager.Instance.loacalPlaceOfInterest.Video; // Set the video clip for the Video Player
            VideoPlayer.Play(); // Start playing the video
        }
        if (PlaceImage != null && GameManager.Instance.loacalPlaceOfInterest.Icon != null)
        {
            PlaceImage.sprite = GameManager.Instance.loacalPlaceOfInterest.Icon; // Set the place icon image
        }


    }
}
