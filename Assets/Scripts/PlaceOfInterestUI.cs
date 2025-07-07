using UnityEngine;
using TMPro;

public class PlaceOfInterestUI : MonoBehaviour
{
    public TextMeshProUGUI PlaceName; // Reference to the TextMeshProUGUI component for the place name

    public void SetPlaceName(string name)
    {
        if (PlaceName != null)
        {
            PlaceName.text = name; // Set the place name text
        }
        else
        {
            Debug.LogWarning("PlaceName TextMeshProUGUI component is not assigned.");
        }
    }
}
