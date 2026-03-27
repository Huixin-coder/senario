using UnityEngine;

public class CameraBlanker : MonoBehaviour
{
    public Camera[] cameras; // Assign your 4 cameras in the Inspector

    private CameraClearFlags[] defaultClearFlags;
    private Color[] defaultBackgroundColors;
    private int[] defaultCullingMasks;
    public bool isBlanked = false;

    void Start()
    {
        int camCount = cameras.Length;

        // Initialize arrays to store default values for each camera
        defaultClearFlags = new CameraClearFlags[camCount];
        defaultBackgroundColors = new Color[camCount];
        defaultCullingMasks = new int[camCount];

        for (int i = 0; i < camCount; i++)
        {
            defaultClearFlags[i] = cameras[i].clearFlags;
            defaultBackgroundColors[i] = cameras[i].backgroundColor;
            defaultCullingMasks[i] = cameras[i].cullingMask;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            isBlanked = !isBlanked;
            ToggleBlankScreen(isBlanked);
        }
    }

    public void ToggleBlankScreen(bool blank)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (blank)
            {
                cameras[i].clearFlags = CameraClearFlags.SolidColor;
                cameras[i].backgroundColor = Color.black;
                cameras[i].cullingMask = 0; // Hide all layers
            }
            else
            {
                // Restore original values
                cameras[i].clearFlags = defaultClearFlags[i];
                cameras[i].backgroundColor = defaultBackgroundColors[i];
                cameras[i].cullingMask = defaultCullingMasks[i];
            }
        }
    }
}
