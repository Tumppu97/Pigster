using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Transform cam; // Reference to the camera's transform.
    Vector3 camStartPos; // Stores the initial position of the camera.
    float distance; // The distance the camera has moved from its starting position.

    GameObject[] backgrounds; // Array of background game objects.
    Material[] mat; // Array of materials used for each background.
    float[] backSpeed; // Array of parallax speeds for each background.

    float farthestBack; // The farthest distance of a background from the camera.

    public float parallaxSpeed; // The base speed for the parallax effect.

    void Start()
    {
        // Initialize camera transform and starting position.
        cam = Camera.main.transform;
        camStartPos = cam.position;

        // Initialize arrays based on the number of children (backgrounds).
        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        // Populate arrays with background objects and materials.
        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        // Calculate the speed for the parallax effect for each background.
        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        // Determine the farthest back distance of any background.
        for (int i = 0; i < backCount; i++)
        {
            float backDistance = backgrounds[i].transform.position.z - cam.position.z;
            if (backDistance > farthestBack)
            {
                farthestBack = backDistance;
            }
        }

        // Calculate parallax speed for each background based on its distance.
        for (int i = 0; i < backCount; i++)
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        // Calculate the distance the camera has moved.
        distance = cam.position.x - camStartPos.x;

        // Apply the parallax effect to each background.
        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}
