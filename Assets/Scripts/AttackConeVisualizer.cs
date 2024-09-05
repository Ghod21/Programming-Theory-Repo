using UnityEngine;

public class AttackConeVisualizer : MonoBehaviour
{
    public int segments = 50; // ���������� ��������� ��� ����
    private LineRenderer lineRenderer;

    private Player playerComponent;
    private float attackRange;
    private float attackAngle;
    public static bool visializerConeEnabled;

    void Start()
    {
        // ���� ��������� Player �� ������������ �������
        playerComponent = GetComponentInParent<Player>();

        if (playerComponent != null)
        {
            // �������� ������ � ���� ����� �� ������
            attackRange = playerComponent.attackRange;
            attackAngle = playerComponent.attackAngle;
        }


        // ��������� LineRenderer ��� ������������
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = segments + 2;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.loop = false;

        // ������������� ���� ������ ����� (��������, ������ ��� ������)
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;

        DrawCone();
        lineRenderer.enabled = false; // �� ��������� �������
    }

    void Update()
    {
        // �������� ������������ ��� ������� ������� Alt
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !lineRenderer.enabled)
        {
            visializerConeEnabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            visializerConeEnabled = false;
        }
        if (visializerConeEnabled)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }

        // ��������� ������� ��� �������� �� �������
        if (transform.parent != null)
        {
            transform.position = transform.parent.position;
            DrawCone();
        }
    }

    void DrawCone()
    {
        // ����������� ����� ������ (����������� �������)
        Vector3 forwardDirection = transform.parent.forward;

        // ����, ������� ����� ��������� �� attackAngle
        float halfAngle = attackAngle / 2f;

        // ������� ������ �����
        Vector3 startPos = Vector3.zero; // ��������� ����� ������ (����� ������)

        // ������ �����, ������������ ���� ������
        Vector3[] conePoints = new Vector3[segments + 2]; // �� ���� ������� ������ ��� �������� ����

        // ��������� ������ ����� � ������ (����� ������)
        conePoints[0] = startPos;

        // ������������ ���� ��� ���������
        float angleStep = attackAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            // ���������� ������� ���� ������������ forwardDirection
            float currentAngle = -halfAngle + angleStep * i;

            // ������������ ���������� ����� �� ������� ������
            // ���������� Quaternion ��� �������� forwardDirection ������ ��� Y (0)
            Vector3 rotatedDirection = Quaternion.Euler(0, currentAngle, 0) * forwardDirection;

            // ��� ��������� XZ ���������� rotatedDirection ������ ��� X � Z ����
            Vector3 conePoint = new Vector3(rotatedDirection.x, 0, rotatedDirection.z) * attackRange;

            // ��������� ����������� ����� � ������
            conePoints[i + 1] = conePoint;
        }

        // ������������� ����� � LineRenderer
        lineRenderer.SetPositions(conePoints);
    }


}
