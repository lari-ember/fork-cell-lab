Tutorial: Aplicando as Regras de Simulação Pura
===============================================

Este tutorial guia você passo a passo para refatorar código de simulação
que depende de MonoBehaviour para uma arquitetura pura e testável.

> 📖 **Pré-requisito**: Leia primeiro `SIMULATION_RULE.md` para entender o "porquê".
> Este documento foca no "como".

---

Índice
------

1. [Fase 1: Identificar código problemático](#fase-1-identificar-código-problemático)
2. [Fase 2: Criar estruturas de dados puras](#fase-2-criar-estruturas-de-dados-puras)
3. [Fase 3: Extrair lógica para classes puras](#fase-3-extrair-lógica-para-classes-puras)
4. [Fase 4: Criar o SimulationRunner (Bridge)](#fase-4-criar-o-simulationrunner-bridge)
5. [Fase 5: Escrever testes](#fase-5-escrever-testes)
6. [Exemplo completo: Refatorando Cell.cs](#exemplo-completo-refatorando-cellcs)

---

Fase 1: Identificar código problemático
---------------------------------------

### Passo 1.1: Buscar scripts com lógica de simulação

No terminal (na pasta do projeto Unity):

```bash
# Encontrar todos os Update/FixedUpdate
grep -rn "void Update\|void FixedUpdate" Assets/Scripts/

# Encontrar uso de Time.deltaTime (dependência de Unity)
grep -rn "Time.deltaTime" Assets/Scripts/

# Encontrar Instantiate/Destroy (side effects)
grep -rn "Instantiate\|Destroy" Assets/Scripts/
```

### Passo 1.2: Classificar cada arquivo encontrado

Crie uma tabela como esta:

| Arquivo | Tipo | Problema | Prioridade |
|---------|------|----------|------------|
| Cell.cs | Simulação | Lógica em Update(), usa Time.deltaTime | **Alta** |
| Adhesin.cs | Simulação | Lógica em Update(), usa Destroy() | **Alta** |
| Substrate.cs | Input/Spawn | Usa Input + Instantiate | Média |
| CellColor.cs | UI | Usa UnityEngine.UI | Baixa (não tocar) |

### Passo 1.3: Mapear dependências

Para cada arquivo de simulação, liste:

- **O que ele LÊ** (inputs)
- **O que ele MODIFICA** (state)
- **O que ele PRODUZ** (outputs/side effects)

**Exemplo para `Cell.cs`:**

```
INPUTS:
  - Mass (próprio)
  - genome[CellModeIndex].SplitMass
  - transform.position (para cálculo de ganho)
  - Time.deltaTime

STATE MODIFICADO:
  - Mass
  - Alive
  - transform.localScale
  - physics.mass

SIDE EFFECTS:
  - Destroy(gameObject) → morte
  - Instantiate(CellPrefab) → reprodução
  - Cria Adhesin entre filhos
```

---

Fase 2: Criar estruturas de dados puras
---------------------------------------

### Passo 2.1: Criar pasta para código puro

```
Assets/
└── Scripts/
    └── Simulation/
        └── Core/           ← NOVA PASTA (código puro aqui)
            ├── CellState.cs
            ├── GenomeData.cs
            ├── SimulationState.cs
            └── Systems/
                ├── MetabolismSystem.cs
                └── ReproductionSystem.cs
```

### Passo 2.2: Extrair dados para structs/classes puras

**❌ ANTES (misturado com Unity):**

```csharp
public class Cell : MonoBehaviour {
    public float Mass = 3f;
    public Genome genome;
    public int CellModeIndex;
    public bool Alive { get; set; }
    // ... métodos que usam Unity
}
```

**✅ DEPOIS (dados puros):**

```csharp
// Assets/Scripts/Simulation/Core/CellState.cs
using System;

/// <summary>
/// Estado de uma célula. Não referencia Unity.
/// </summary>
[Serializable]
public struct CellState
{
    public int Id;
    public float Mass;
    public int ModeIndex;
    public bool Alive;
    
    // Posição como floats simples (não Vector2 do Unity)
    public float X;
    public float Y;
    public float Rotation;
    
    // Velocidade
    public float VelocityX;
    public float VelocityY;
    
    // Referência ao genoma (por índice ou ID)
    public int GenomeId;
    
    // Raio calculado
    public float Radius => Mass / 4f;
}
```

### Passo 2.3: Criar estado global da simulação

```csharp
// Assets/Scripts/Simulation/Core/SimulationState.cs
using System.Collections.Generic;

/// <summary>
/// Estado completo da simulação. Snapshot serializável.
/// </summary>
public class SimulationState
{
    public float Time;
    public List<CellState> Cells = new List<CellState>();
    public List<AdhesinState> Adhesins = new List<AdhesinState>();
    public List<GenomeData> Genomes = new List<GenomeData>();
    
    private int _nextCellId = 1;
    
    public int GenerateCellId() => _nextCellId++;
    
    public SimulationState Clone()
    {
        var clone = new SimulationState();
        clone.Time = Time;
        clone._nextCellId = _nextCellId;
        clone.Cells = new List<CellState>(Cells);
        clone.Adhesins = new List<AdhesinState>(Adhesins);
        clone.Genomes = new List<GenomeData>(Genomes);
        return clone;
    }
}
```

---

Fase 3: Extrair lógica para classes puras
-----------------------------------------

### Passo 3.1: Identificar "sistemas" (regras de atualização)

Cada responsabilidade vira um sistema:

| Sistema | Responsabilidade |
|---------|------------------|
| MetabolismSystem | Ganho/perda de massa |
| ReproductionSystem | Verificar split, criar filhos |
| DeathSystem | Verificar morte, marcar células |
| PhysicsSystem | Atualizar posições/velocidades |
| AdhesinSystem | Transferência de massa, quebra de ligações |

### Passo 3.2: Criar um sistema (exemplo: Metabolism)

```csharp
// Assets/Scripts/Simulation/Core/Systems/MetabolismSystem.cs
using System;

/// <summary>
/// Atualiza massa das células. Lógica pura, sem Unity.
/// </summary>
public static class MetabolismSystem
{
    // Constantes extraídas do Cell.cs original
    public const float DecayRate = 0.7f;
    public const float LinearGainFactor = 0.08f;
    public const float RadialGainFactor = 0.15f;
    public const float GainThreshold = 3.6f;
    
    /// <summary>
    /// Atualiza a massa de todas as células vivas.
    /// </summary>
    public static void Update(SimulationState state, float deltaTime)
    {
        for (int i = 0; i < state.Cells.Count; i++)
        {
            var cell = state.Cells[i];
            if (!cell.Alive) continue;
            
            cell = UpdateCellMass(cell, deltaTime);
            state.Cells[i] = cell;
        }
    }
    
    private static CellState UpdateCellMass(CellState cell, float deltaTime)
    {
        // Perda passiva
        cell.Mass -= DecayRate * deltaTime;
        
        // Ganho por posição (se abaixo do threshold)
        if (cell.Mass < GainThreshold)
        {
            float magnitude = MathF.Sqrt(cell.X * cell.X + cell.Y * cell.Y);
            float linear = LinearGainFactor * cell.Y * deltaTime;
            float radial = RadialGainFactor * magnitude * deltaTime;
            cell.Mass += linear + radial;
        }
        
        return cell;
    }
}
```

### Passo 3.3: Criar sistema de morte

```csharp
// Assets/Scripts/Simulation/Core/Systems/DeathSystem.cs
using System.Collections.Generic;

/// <summary>
/// Marca células com massa insuficiente como mortas.
/// </summary>
public static class DeathSystem
{
    public const float MinMass = 0.6f;
    
    /// <summary>
    /// Marca células com massa insuficiente como mortas.
    /// Retorna lista de IDs das células que morreram.
    /// </summary>
    public static List<int> Update(SimulationState state)
    {
        var deaths = new List<int>();
        
        for (int i = 0; i < state.Cells.Count; i++)
        {
            var cell = state.Cells[i];
            if (!cell.Alive) continue;
            
            if (cell.Mass < MinMass)
            {
                cell.Alive = false;
                state.Cells[i] = cell;
                deaths.Add(cell.Id);
            }
        }
        
        return deaths;
    }
}
```

### Passo 3.4: Criar a interface principal

```csharp
// Assets/Scripts/Simulation/Core/ISimulation.cs

/// <summary>
/// Interface para simulação pura. Não conhece Unity.
/// </summary>
public interface ISimulation
{
    SimulationState State { get; }
    
    /// <summary>
    /// Avança a simulação por deltaTime segundos.
    /// </summary>
    void Step(float deltaTime);
    
    /// <summary>
    /// Reinicia com um estado inicial.
    /// </summary>
    void Reset(SimulationState initialState);
    
    /// <summary>
    /// Adiciona uma nova célula (comando externo).
    /// </summary>
    int SpawnCell(float x, float y, int genomeId);
}
```

### Passo 3.5: Implementar a simulação

```csharp
// Assets/Scripts/Simulation/Core/CellSimulation.cs
using System;

/// <summary>
/// Implementação da simulação de células. Lógica pura.
/// </summary>
public class CellSimulation : ISimulation
{
    public SimulationState State { get; private set; }
    
    // Eventos para o Unity observar (opcional)
    public event Action<int> OnCellDied;
    public event Action<int, int, int> OnCellSplit; // parentId, child1Id, child2Id
    
    public CellSimulation()
    {
        State = new SimulationState();
    }
    
    public void Reset(SimulationState initialState)
    {
        State = initialState ?? new SimulationState();
    }
    
    public void Step(float deltaTime)
    {
        if (deltaTime <= 0) return;
        
        State.Time += deltaTime;
        
        // Ordem dos sistemas importa!
        MetabolismSystem.Update(State, deltaTime);
        
        var deaths = DeathSystem.Update(State);
        foreach (var id in deaths)
        {
            OnCellDied?.Invoke(id);
        }
        
        // TODO: ReproductionSystem, PhysicsSystem, AdhesinSystem
    }
    
    public int SpawnCell(float x, float y, int genomeId)
    {
        var cell = new CellState
        {
            Id = State.GenerateCellId(),
            Mass = 1f,
            ModeIndex = 0,
            Alive = true,
            X = x,
            Y = y,
            GenomeId = genomeId
        };
        
        State.Cells.Add(cell);
        return cell.Id;
    }
}
```

---

Fase 4: Criar o SimulationRunner (Bridge)
-----------------------------------------

Este MonoBehaviour conecta a simulação pura ao Unity.

### Passo 4.1: Criar o runner

```csharp
// Assets/Scripts/Simulation/SimulationRunner.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bridge entre a simulação pura e o Unity.
/// Apenas este script conhece Unity + Simulação.
/// </summary>
public class SimulationRunner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject CellPrefab;
    
    [Header("Settings")]
    public bool Paused = false;
    public float TimeScale = 1f;
    
    private CellSimulation _simulation;
    private Dictionary<int, GameObject> _cellViews = new Dictionary<int, GameObject>();
    
    private void Awake()
    {
        _simulation = new CellSimulation();
        _simulation.OnCellDied += HandleCellDied;
    }
    
    private void Update()
    {
        if (Paused) return;
        
        // Passa tempo controlado para simulação
        _simulation.Step(Time.deltaTime * TimeScale);
        
        // Sincroniza views com estado
        SyncViews();
    }
    
    private void SyncViews()
    {
        foreach (var cell in _simulation.State.Cells)
        {
            if (!cell.Alive)
            {
                // Remove view se existir
                if (_cellViews.TryGetValue(cell.Id, out var deadView))
                {
                    Destroy(deadView);
                    _cellViews.Remove(cell.Id);
                }
                continue;
            }
            
            // Cria view se não existir
            if (!_cellViews.TryGetValue(cell.Id, out var view))
            {
                view = Instantiate(CellPrefab);
                _cellViews[cell.Id] = view;
            }
            
            // Atualiza posição/escala da view
            view.transform.position = new Vector3(cell.X, cell.Y, 0);
            view.transform.localScale = Vector3.one * cell.Radius;
        }
    }
    
    private void HandleCellDied(int cellId)
    {
        Debug.Log($"[Sim] Célula {cellId} morreu");
    }
    
    // API pública para input
    public void SpawnCellAt(Vector2 position, int genomeId = 0)
    {
        _simulation.SpawnCell(position.x, position.y, genomeId);
    }
    
    // Para testes: rodar N passos de uma vez
    public void FastForward(int steps, float fixedDelta = 0.016f)
    {
        for (int i = 0; i < steps; i++)
        {
            _simulation.Step(fixedDelta);
        }
        SyncViews();
    }
}
```

### Passo 4.2: Criar input handler separado

```csharp
// Assets/Scripts/Simulation/SimulationInputHandler.cs
using UnityEngine;

/// <summary>
/// Captura input do usuário e envia comandos para o SimulationRunner.
/// </summary>
public class SimulationInputHandler : MonoBehaviour
{
    public SimulationRunner Runner;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Runner.SpawnCellAt(new Vector2(worldPos.x, worldPos.y));
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Runner.Paused = !Runner.Paused;
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            Runner.FastForward(100); // Avança 100 frames
        }
    }
}
```

---

Fase 5: Escrever testes
-----------------------

### Passo 5.1: Teste de metabolismo

```csharp
// Assets/Scripts/Tests/MetabolismTests.cs (ou fora do Unity)
using System;

public static class MetabolismTests
{
    public static void RunAll()
    {
        TestMassDecay();
        TestMassGain();
        Console.WriteLine("✓ Todos os testes de metabolismo passaram!");
    }
    
    public static void TestMassDecay()
    {
        var state = new SimulationState();
        state.Cells.Add(new CellState { Id = 1, Mass = 5f, Alive = true, X = 0, Y = 0 });
        
        MetabolismSystem.Update(state, 1f); // 1 segundo
        
        // Esperado: 5 - 0.7 = 4.3
        float expected = 4.3f;
        float actual = state.Cells[0].Mass;
        
        if (Math.Abs(actual - expected) > 0.01f)
            throw new Exception($"TestMassDecay FALHOU: esperado {expected}, obteve {actual}");
        
        Console.WriteLine("  ✓ TestMassDecay passou");
    }
    
    public static void TestMassGain()
    {
        var state = new SimulationState();
        state.Cells.Add(new CellState { Id = 1, Mass = 2f, Alive = true, X = 0, Y = 5f });
        
        MetabolismSystem.Update(state, 1f);
        
        // Abaixo do threshold (3.6), deve ganhar massa
        // linear = 0.08 * 5 = 0.4
        // radial = 0.15 * 5 = 0.75
        // decay = 0.7
        // final = 2 - 0.7 + 0.4 + 0.75 = 2.45
        
        if (state.Cells[0].Mass <= 2f)
            throw new Exception("TestMassGain FALHOU: massa deveria ter aumentado");
        
        Console.WriteLine("  ✓ TestMassGain passou");
    }
}
```

### Passo 5.2: Teste de morte

```csharp
public static class DeathTests
{
    public static void RunAll()
    {
        TestCellDies();
        TestCellSurvives();
        Console.WriteLine("✓ Todos os testes de morte passaram!");
    }
    
    public static void TestCellDies()
    {
        var state = new SimulationState();
        state.Cells.Add(new CellState { Id = 1, Mass = 0.3f, Alive = true });
        state.Cells.Add(new CellState { Id = 2, Mass = 2f, Alive = true });
        
        var deaths = DeathSystem.Update(state);
        
        if (deaths.Count != 1)
            throw new Exception($"TestCellDies FALHOU: esperado 1 morte, obteve {deaths.Count}");
        
        if (deaths[0] != 1)
            throw new Exception("TestCellDies FALHOU: célula 1 deveria morrer");
        
        if (state.Cells[0].Alive)
            throw new Exception("TestCellDies FALHOU: célula 1 ainda está viva");
        
        Console.WriteLine("  ✓ TestCellDies passou");
    }
    
    public static void TestCellSurvives()
    {
        var state = new SimulationState();
        state.Cells.Add(new CellState { Id = 1, Mass = 2f, Alive = true });
        
        var deaths = DeathSystem.Update(state);
        
        if (deaths.Count != 0)
            throw new Exception("TestCellSurvives FALHOU: ninguém deveria morrer");
        
        Console.WriteLine("  ✓ TestCellSurvives passou");
    }
}
```

### Passo 5.3: Runner de testes

```csharp
// Assets/Scripts/Tests/TestRunner.cs
using System;

public static class TestRunner
{
    public static void RunAllTests()
    {
        Console.WriteLine("=== Executando Testes de Simulação ===\n");
        
        try
        {
            MetabolismTests.RunAll();
            DeathTests.RunAll();
            // Adicione mais testes aqui
            
            Console.WriteLine("\n=== TODOS OS TESTES PASSARAM ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n=== TESTE FALHOU ===\n{ex.Message}");
        }
    }
}
```

---

Exemplo completo: Refatorando Cell.cs
-------------------------------------

### Estado atual (problemático)

```csharp
// Cell.cs - ANTES (❌ viola as regras)
public class Cell : MonoBehaviour {
    public float Mass = 3f;
    public Genome genome;
    
    void Update() {
        Mass -= 0.7f * Time.deltaTime;  // ❌ Time.deltaTime
        
        if (Mass < 0.6f) {
            Destroy(gameObject);         // ❌ Destroy
            return;
        }
        
        if (Mass >= genome[CellModeIndex].SplitMass) {
            Split();                      // ❌ Instantiate dentro
        }
    }
}
```

### Refatoração passo a passo

**1. Extrair dados → `CellState.cs`**

```csharp
public struct CellState {
    public int Id;
    public float Mass;
    public bool Alive;
    public float X, Y;
    // ...
}
```

**2. Extrair lógica de metabolismo → `MetabolismSystem.cs`**

```csharp
public static void Update(SimulationState state, float deltaTime) {
    // Lógica de decay e gain aqui
}
```

**3. Extrair lógica de morte → `DeathSystem.cs`**

```csharp
public static List<int> Update(SimulationState state) {
    // Marcar células mortas, retornar IDs
}
```

**4. Extrair lógica de reprodução → `ReproductionSystem.cs`**

```csharp
public static List<SplitEvent> Update(SimulationState state, List<GenomeData> genomes) {
    // Verificar split, criar células filhas no state
}
```

**5. Cell.cs vira apenas View**

```csharp
// CellView.cs - DEPOIS (✓ apenas visual)
public class CellView : MonoBehaviour {
    public int CellId;
    public SpriteRenderer Sprite;
    
    public void UpdateFromState(CellState state) {
        transform.position = new Vector3(state.X, state.Y, 0);
        transform.localScale = Vector3.one * state.Radius;
    }
}
```

---

Checklist de migração
---------------------

Use esta checklist para cada arquivo:

- [ ] Identificar dados → mover para struct/class pura
- [ ] Identificar lógica → mover para System estático
- [ ] Remover `using UnityEngine` do código puro
- [ ] Substituir `Time.deltaTime` → parâmetro `deltaTime`
- [ ] Substituir `Destroy/Instantiate` → retornar eventos/comandos
- [ ] Criar View separada (MonoBehaviour) se necessário
- [ ] Escrever pelo menos 1 teste para a lógica extraída
- [ ] Verificar que simulação roda sem Unity

---

Dicas finais
------------

1. **Migre incrementalmente**: Comece por um sistema pequeno (ex: metabolismo)
2. **Mantenha o código antigo funcionando** até a migração estar completa
3. **Use feature flags** para alternar entre implementação antiga e nova
4. **Teste a cada passo**: Não espere migrar tudo para testar
5. **Documente decisões**: Quando algo não puder ser puro, documente o porquê

---

Próximos passos recomendados
----------------------------

1. [ ] Criar pasta `Assets/Scripts/Simulation/Core/`
2. [ ] Criar `CellState.cs` com dados extraídos de `Cell.cs`
3. [ ] Criar `MetabolismSystem.cs` com lógica de massa
4. [ ] Criar teste para `MetabolismSystem`
5. [ ] Criar `SimulationRunner.cs` básico
6. [ ] Testar spawn de célula com sistema novo

---

Referências
-----------

- `Docs/SIMULATION_RULE.md` — Regras e justificativas
- `Assets/Scripts/Simulation/Cell.cs` — Código atual a refatorar
- `Assets/Scripts/Simulation/Adhesin.cs` — Próximo candidato após Cell

