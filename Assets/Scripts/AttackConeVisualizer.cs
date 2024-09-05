using UnityEngine;

public class AttackConeVisualizer : MonoBehaviour
{
    public int segments = 50; // Количество сегментов для дуги
    private LineRenderer lineRenderer;

    private Player playerComponent;
    private float attackRange;
    private float attackAngle;
    public static bool visializerConeEnabled;

    void Start()
    {
        // Ищем компонент Player на родительском объекте
        playerComponent = GetComponentInParent<Player>();

        if (playerComponent != null)
        {
            // Получаем радиус и угол атаки от игрока
            attackRange = playerComponent.attackRange;
            attackAngle = playerComponent.attackAngle;
        }


        // Настройка LineRenderer для визуализации
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = segments + 2;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.loop = false;

        // Устанавливаем цвет конуса атаки (например, зелёный для игрока)
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;

        DrawCone();
        lineRenderer.enabled = false; // По умолчанию невидим
    }

    void Update()
    {
        // Включаем визуализацию при нажатии клавиши Alt
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

        // Обновляем позицию для слежения за игроком
        if (transform.parent != null)
        {
            transform.position = transform.parent.position;
            DrawCone();
        }
    }

    void DrawCone()
    {
        // Центральная линия конуса (направление взгляда)
        Vector3 forwardDirection = transform.parent.forward;

        // Угол, который будет половиной от attackAngle
        float halfAngle = attackAngle / 2f;

        // Вершина конуса атаки
        Vector3 startPos = Vector3.zero; // Начальная точка конуса (центр игрока)

        // Массив точек, определяющих края конуса
        Vector3[] conePoints = new Vector3[segments + 2]; // На один сегмент больше для закрытия дуги

        // Добавляем первую точку в начало (центр игрока)
        conePoints[0] = startPos;

        // Рассчитываем углы для сегментов
        float angleStep = attackAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            // Определяем текущий угол относительно forwardDirection
            float currentAngle = -halfAngle + angleStep * i;

            // Рассчитываем координаты точки на границе конуса
            // Используем Quaternion для вращения forwardDirection вокруг оси Y (0)
            Vector3 rotatedDirection = Quaternion.Euler(0, currentAngle, 0) * forwardDirection;

            // Для плоскости XZ используем rotatedDirection только для X и Z осей
            Vector3 conePoint = new Vector3(rotatedDirection.x, 0, rotatedDirection.z) * attackRange;

            // Сохраняем вычисленную точку в массив
            conePoints[i + 1] = conePoint;
        }

        // Устанавливаем точки в LineRenderer
        lineRenderer.SetPositions(conePoints);
    }


}
