using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class AttackRadiusVisualizer : MonoBehaviour
{
    public float attackRadius = 5f; // Default attack radius
    public int segments = 50; // Number of segments for drawing the circle
    private LineRenderer lineRenderer;

    private Player playerComponent;
    private Enemy enemyComponent;

    Quaternion initialRotation;

    public static bool visualizerEnabled = false;

    void Start()
    {
        initialRotation = transform.rotation;
        // Create and configure the LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.loop = true;

        // Try to get Player or Enemy components from the parent object
        playerComponent = GetComponentInParent<Player>();
        enemyComponent = GetComponentInParent<Enemy>();
        // Ensure the color is set via code
        SetColor(playerComponent != null ? Color.green : Color.red);

        // Force reset the material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        DrawCircle();
        lineRenderer.enabled = false;

        if (playerComponent != null)
        {
            attackRadius = playerComponent.attackRange * 0.8f; // Get player's attack radius
            SetColor(HexToColor("#2B8A2B", 0.3f)); // Use HEX code for green with 30% transparency
        }
        else if (enemyComponent != null)
        {
            attackRadius = enemyComponent.attackRange * 0.8f; // Get enemy's attack radius
            SetColor(HexToColor("#B74443", 0.3f)); // Use HEX code for red with 30% transparency
        }

        DrawCircle();
        lineRenderer.enabled = false; // Circle is invisible by default
    }

    void Update()
    {
        if (DataPersistence.easyDifficulty && SceneManager.GetActiveScene().name == "MainScene")
        {
            // Check if the Alt key is pressed to toggle the circle visibility
            if (Input.GetKeyDown(KeyCode.LeftAlt) && !lineRenderer.enabled)
            {
                visualizerEnabled = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                visualizerEnabled = false;
            }
            if (visualizerEnabled && playerComponent != null)
            {
                lineRenderer.enabled = true;
            }
            else if (visualizerEnabled && enemyComponent.enemyHealth > 0 && enemyComponent != null)
            {
                lineRenderer.enabled = true;
            }
            else
            {
                lineRenderer.enabled = false;
            }

            // Update the position of the circle to follow the parent (player or enemy)
            if (transform.parent != null)
            {
                transform.position = transform.parent.position;
                transform.rotation = initialRotation;
            }
        }
    }

    // Function to draw the circle
    void DrawCircle()
    {
        float angle = 0f;
        for (int i = 0; i < segments + 1; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * attackRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * attackRadius;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
            angle += 360f / segments;
        }
    }

    // Function to set the color of the circle
    void SetColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public Color HexToColor(string hex, float alpha = 1f)
    {
        // Remove the hash if present
        hex = hex.Replace("#", "");

        // Check the length of the string
        if (hex.Length != 6 && hex.Length != 8)
        {
            Debug.LogError("Invalid HEX color code.");
            return Color.white;
        }

        // Parse color components
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = (byte)(alpha * 255); // Set alpha channel based on alpha

        return new Color32(r, g, b, a);
    }
}
