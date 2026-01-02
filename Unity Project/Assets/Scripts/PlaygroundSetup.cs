using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configura a cena Playground para jogar.
/// Inicializa PrefabSupplier, Microscope, Substrate e GenomeEditor.
/// </summary>
public class PlaygroundSetup : MonoBehaviour
{
    [Header("Prefabs (arraste do Project)")]
    public GameObject CellPrefab;
    public GameObject AdhesinPrefab;

    [Header("Referências da Cena")]
    public Microscope Microscope;
    public GenomeEditor GenomeEditor;
    public ContextManager ContextManager;

    [Header("Configuração Inicial")]
    public bool StartInMicroscopeMode = true;
    
    [Header("UI")]
    public bool AutoCreateUI = true;
    public Text InstructionsText;
    public Text CellCountText;

    private void Awake()
    {
        // Configura PrefabSupplier (estático)
        if (CellPrefab != null)
            PrefabSupplier.CellPrefabReference = CellPrefab;
        if (AdhesinPrefab != null)
            PrefabSupplier.AdhesinPrefabReference = AdhesinPrefab;

        Debug.Log("[PlaygroundSetup] Prefabs configurados");
    }

    private void Start()
    {
        // Auto-encontrar referências se não foram atribuídas
        if (Microscope == null)
        {
            Microscope = FindAnyObjectByType<Microscope>(FindObjectsInactive.Include);
        }
        
        if (GenomeEditor == null)
        {
            GenomeEditor = FindAnyObjectByType<GenomeEditor>(FindObjectsInactive.Include);
        }
        
        // Conectar ContextManager às mesmas referências
        if (ContextManager == null)
        {
            ContextManager = FindAnyObjectByType<ContextManager>(FindObjectsInactive.Include);
        }
        
        if (ContextManager != null)
        {
            ContextManager.Microscope = Microscope;
            ContextManager.GenomeEditor = GenomeEditor;
        }

        // Criar genoma padrão se GenomeEditor não existe ou não tem genoma
        Genome defaultGenome = CreateDefaultGenome();
        
        if (GenomeEditor != null)
        {
            if (GenomeEditor.CurrentGenome.ModeCount == 0)
            {
                GenomeEditor.CurrentGenome = defaultGenome;
            }
            Debug.Log("[PlaygroundSetup] GenomeEditor com " + GenomeEditor.CurrentGenome.ModeCount + " modos");
        }

        // Configura Microscope com o genoma
        if (Microscope != null)
        {
            Genome genomeToUse;
            if (GenomeEditor != null && GenomeEditor.CurrentGenome.ModeCount > 0)
            {
                genomeToUse = GenomeEditor.CurrentGenome.Clone();
            }
            else
            {
                genomeToUse = defaultGenome;
            }
            Microscope.CurrentGenome = genomeToUse;
            Debug.Log("[PlaygroundSetup] Microscope configurado com genoma de " + genomeToUse.ModeCount + " modos");
        }

        // Criar UI automaticamente se necessário
        if (AutoCreateUI && (InstructionsText == null || CellCountText == null))
        {
            CreateUI();
        }

        // Ativa modo inicial
        if (StartInMicroscopeMode)
        {
            ActivateMicroscope();
        }
        else
        {
            ActivateGenomeEditor();
        }

        // Configura texto de instruções
        if (InstructionsText != null)
        {
            InstructionsText.text = "Clique = Criar célula | G = Editor | M = Microscópio | C = Limpar";
        }

        Debug.Log("[PlaygroundSetup] Cena pronta! Clique para criar células.");
    }

    /// <summary>
    /// Cria Canvas e textos de UI automaticamente.
    /// </summary>
    private void CreateUI()
    {
        // Procurar Canvas existente ou criar novo
        Canvas canvas = FindAnyObjectByType<Canvas>();
        GameObject canvasObj;
        
        if (canvas == null)
        {
            canvasObj = new GameObject("PlaygroundCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[PlaygroundSetup] Canvas criado automaticamente");
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // Tentar obter fonte padrão
        Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // Criar texto de instruções (topo centro)
        if (InstructionsText == null)
        {
            GameObject instrObj = new GameObject("Instructions");
            instrObj.transform.SetParent(canvasObj.transform, false);
            InstructionsText = instrObj.AddComponent<Text>();
            InstructionsText.font = defaultFont;
            InstructionsText.fontSize = 18;
            InstructionsText.color = Color.white;
            InstructionsText.alignment = TextAnchor.UpperCenter;
            
            RectTransform rt = instrObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -10);
            rt.sizeDelta = new Vector2(0, 30);
        }

        // Criar texto de contagem (topo esquerda)
        if (CellCountText == null)
        {
            GameObject countObj = new GameObject("CellCount");
            countObj.transform.SetParent(canvasObj.transform, false);
            CellCountText = countObj.AddComponent<Text>();
            CellCountText.font = defaultFont;
            CellCountText.fontSize = 24;
            CellCountText.color = Color.yellow;
            CellCountText.alignment = TextAnchor.UpperLeft;
            
            RectTransform rt = countObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -40);
            rt.sizeDelta = new Vector2(200, 40);
        }

        Debug.Log("[PlaygroundSetup] UI criada automaticamente");
    }

    /// <summary>
    /// Cria um genoma padrão funcional para testes.
    /// </summary>
    private Genome CreateDefaultGenome()
    {
        Genome genome = new Genome(1);
        
        // Configurar o modo 0 com valores que permitem crescimento e divisão
        if (genome.modes != null && genome.modes.Length > 0)
        {
            genome.modes[0].Color = Color.green;
            genome.modes[0].SplitMass = 2.5f;
            genome.modes[0].MakeAdhesin = false;
            genome.modes[0].Child1ModeIndex = 0;
            genome.modes[0].Child2ModeIndex = 0;
        }
        
        genome.InitialModeIndex = 0;
        return genome;
    }

    private void Update()
    {
        // Atualiza contagem de células
        if (CellCountText != null)
        {
            int count = GameObject.FindGameObjectsWithTag("Cell").Length;
            CellCountText.text = "Células: " + count;
        }

        // Atalho: C para limpar todas as células
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllCells();
        }
    }

    public void ActivateMicroscope()
    {
        if (Microscope != null) Microscope.Active = true;
        if (GenomeEditor != null) GenomeEditor.Active = false;
        Debug.Log("[PlaygroundSetup] Modo: Microscópio");
    }

    public void ActivateGenomeEditor()
    {
        if (Microscope != null) Microscope.Active = false;
        if (GenomeEditor != null)
        {
            GenomeEditor.gameObject.SetActive(true); // Ativar o GameObject primeiro
            GenomeEditor.Active = true;
        }
        Debug.Log("[PlaygroundSetup] Modo: Editor de Genoma");
    }

    public void SwitchToMicroscopeWithUpdatedGenome()
    {
        if (Microscope != null && GenomeEditor != null)
        {
            Microscope.CurrentGenome = GenomeEditor.CurrentGenome.Clone();
        }
        ActivateMicroscope();
    }

    public void ClearAllCells()
    {
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (var cell in cells)
        {
            Destroy(cell);
        }
        Debug.Log("[PlaygroundSetup] " + cells.Length + " células removidas");
    }
}

