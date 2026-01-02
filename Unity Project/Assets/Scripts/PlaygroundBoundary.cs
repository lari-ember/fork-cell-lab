using UnityEngine;

/// <summary>
/// Cria bordas circulares para conter as células na área de simulação.
/// Adicione este componente a um GameObject vazio com EdgeCollider2D.
/// </summary>
[RequireComponent(typeof(EdgeCollider2D))]
public class PlaygroundBoundary : MonoBehaviour
{
    [Header("Configuração da Borda")]
    [Tooltip("Raio do círculo de contenção")]
    public float Radius = 5f;
    
    [Tooltip("Número de segmentos do círculo (mais = mais suave)")]
    public int Segments = 64;

    [Header("Visualização")]
    [Tooltip("Mostrar a borda visualmente durante o jogo")]
    public bool ShowVisualBorder = true;
    
    [Tooltip("Cor da borda")]
    public Color BorderColor = Color.cyan;
    
    [Tooltip("Espessura da linha")]
    public float LineWidth = 0.05f;

    private LineRenderer lineRenderer;

    private void Start()
    {
        CreateCircularBoundary();
        
        if (ShowVisualBorder)
        {
            CreateVisualBorder();
        }
    }

    /// <summary>
    /// Gera os pontos do EdgeCollider2D em formato circular.
    /// </summary>
    private void CreateCircularBoundary()
    {
        var edgeCollider = GetComponent<EdgeCollider2D>();
        var points = new Vector2[Segments + 1];

        for (int i = 0; i <= Segments; i++)
        {
            float angle = (i / (float)Segments) * Mathf.PI * 2f;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Radius;
        }

        edgeCollider.points = points;
        Debug.Log("[PlaygroundBoundary] Borda circular criada com raio " + Radius);
    }

    /// <summary>
    /// Cria um LineRenderer para mostrar a borda visualmente durante o jogo.
    /// </summary>
    private void CreateVisualBorder()
    {
        // Criar ou obter LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Configurar material simples
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = BorderColor;
        lineRenderer.endColor = BorderColor;
        lineRenderer.startWidth = LineWidth;
        lineRenderer.endWidth = LineWidth;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        // Criar pontos do círculo
        lineRenderer.positionCount = Segments;
        for (int i = 0; i < Segments; i++)
        {
            float angle = (i / (float)Segments) * Mathf.PI * 2f;
            Vector3 point = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * Radius;
            lineRenderer.SetPosition(i, point);
        }

        Debug.Log("[PlaygroundBoundary] Borda visual criada");
    }

    /// <summary>
    /// Desenha o círculo no editor para visualização.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = BorderColor;
        
        for (int i = 0; i < Segments; i++)
        {
            float angle1 = (i / (float)Segments) * Mathf.PI * 2f;
            float angle2 = ((i + 1) / (float)Segments) * Mathf.PI * 2f;
            
            Vector3 point1 = transform.position + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * Radius;
            Vector3 point2 = transform.position + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * Radius;
            
            Gizmos.DrawLine(point1, point2);
        }
    }

    /// <summary>
    /// Atualiza a borda em runtime se o raio mudar.
    /// </summary>
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (GetComponent<EdgeCollider2D>() != null)
            {
                CreateCircularBoundary();
            }
            if (ShowVisualBorder && lineRenderer != null)
            {
                CreateVisualBorder();
            }
        }
    }
}

