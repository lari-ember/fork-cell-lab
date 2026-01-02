using UnityEngine;

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

    [Header("UI (opcional)")]
    public UnityEngine.UI.Text InstructionsText;
    public UnityEngine.UI.Text CellCountText;

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
        // Nota: FindAnyObjectByType com includeInactive encontra objetos desativados
        if (Microscope == null)
        {
            Microscope = FindAnyObjectByType<Microscope>(FindObjectsInactive.Include);
        }
        
        if (GenomeEditor == null)
        {
            GenomeEditor = FindAnyObjectByType<GenomeEditor>(FindObjectsInactive.Include);
        }
        
        // Conectar ContextManager às mesmas referências
        if (ContextManager != null)
        {
            ContextManager.Microscope = Microscope;
            ContextManager.GenomeEditor = GenomeEditor;
        }

        // Criar genoma padrão se GenomeEditor não existe ou não tem genoma
        Genome defaultGenome = CreateDefaultGenome();
        
        if (GenomeEditor != null)
        {
            // Se o genoma do editor está vazio, usar o padrão
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

