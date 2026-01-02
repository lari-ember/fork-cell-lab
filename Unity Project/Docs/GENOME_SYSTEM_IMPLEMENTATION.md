# Implementação Técnica do Sistema de Genoma

## Arquitetura Atual

### Visão Geral da Estrutura

O sistema de genoma está implementado usando uma arquitetura híbrida que combina:
- **Estruturas de dados puras** para representação do genoma
- **Componentes Unity** para interface e visualização
- **Integração com simulação** através de referências diretas

### Diagrama de Componentes

```
GenomeEditor (MonoBehaviour)
├── CurrentGenome (Genome struct)
│   └── modes[] (CellMode array)
├── UI Components
│   ├── CellModeSelector
│   ├── CellColor
│   ├── MakeAdhesinCheckbox
│   └── ChildModeSelector
└── Integration
    ├── ContextManager
    └── Substrate
```

## Estruturas de Dados

### Genome Struct

```csharp
[Serializable]
public struct Genome
{
    public IEnumerable<CellMode> Modes { get; }
    public int ModeCount { get; }
    public CellMode InitialMode { get; }
    public int InitialModeIndex;
    public CellMode[] modes;
}
```

**Características:**
- **Struct vs Class**: Escolha de `struct` para semântica de valor
- **Serializable**: Permite serialização Unity para persistência
- **Array dinâmico**: Redimensionamento via `AddMode()`
- **Acesso indexado**: Operador `[]` para acesso direto aos modos

### CellMode Struct

```csharp
[Serializable]
public struct CellMode
{
    // Identidade
    public CellType Type;
    public Color Color;
    
    // Comportamento de divisão
    public float SplitMass;
    public float SplitAngle;
    
    // Herança genética
    public int Child1ModeIndex;
    public int Child2ModeIndex;
    public float Child1Angle;
    public float Child2Angle;
    
    // Sistema de adesinas
    public bool MakeAdhesin;
    public bool Child1KeepAdhesin;
    public bool Child2KeepAdhesin;
}
```

**Design Rationale:**
- **Struct**: Eficiência de memória e semântica de valor
- **Índices vs Referências**: Uso de índices para evitar referências circulares
- **Flags booleanas**: Controle granular de comportamentos
- **Parâmetros físicos**: Valores float para configuração precisa

## Interface de Usuário

### GenomeEditor (Controlador Principal)

```csharp
public class GenomeEditor : MonoBehaviour
{
    public bool Active { get; set; }
    public Genome CurrentGenome;
    public int CurrentMode { get; set; }
}
```

**Responsabilidades:**
- **Gerenciamento de estado**: Controla genoma ativo e modo atual
- **Coordenação UI**: Sincroniza mudanças entre componentes
- **Broadcasting**: Sistema de mensagens para atualização de UI
- **Lifecycle**: Ativação/desativação da interface

### Sistema de Mensagens

O editor usa `BroadcastMessage()` para comunicação:

```csharp
// Quando modo atual muda
BroadcastMessage("EditModeChanged");

// Quando genoma é modificado
BroadcastMessage("GenomeModeAdded", mode);
BroadcastMessage("GenomeModesChanged");
```

**Vantagens:**
- **Desacoplamento**: Componentes não precisam referenciar uns aos outros
- **Flexibilidade**: Fácil adição de novos listeners
- **Unity Integration**: Aproveita sistema nativo do Unity

**Desvantagens:**
- **Performance**: Reflection overhead
- **Type Safety**: Sem verificação de tipos em compile-time
- **Debugging**: Difícil rastrear fluxo de mensagens

### Componentes de UI

#### CellModeSelector
```csharp
public class CellModeSelector : MonoBehaviour
{
    private Dropdown dropdown;
    private GenomeEditor editor;
    
    void GenomeModesChanged() { /* atualiza opções */ }
}
```

#### CellColor
```csharp
public class CellColor : MonoBehaviour
{
    public ColorSlider Red, Green, Blue;
    
    public void EditModeChanged() { /* sincroniza cor */ }
    public void OnSliderValueChanged(float value) { /* aplica mudança */ }
}
```

## Integração com Simulação

### Cell Behavior

```csharp
public class Cell : MonoBehaviour
{
    public Genome genome;
    public int CellModeIndex;
    
    void Update()
    {
        // Crescimento baseado no genoma
        if (Mass >= genome[CellModeIndex].SplitMass)
        {
            Split();
        }
    }
}
```

**Processo de Divisão:**
1. **Verificação**: Massa atinge `SplitMass`
2. **Instanciação**: Cria duas células filhas
3. **Herança**: Clona genoma para filhos
4. **Configuração**: Define modos baseado em `Child1ModeIndex/Child2ModeIndex`
5. **Adesinas**: Cria conexões baseado em `MakeAdhesin`

### Substrate Integration

```csharp
public class Substrate : MonoBehaviour
{
    public Genome CurrentGenome;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Cria célula com genoma atual
            cellData.genome = CurrentGenome;
            cellData.CellModeIndex = cellData.genome.InitialModeIndex;
        }
    }
}
```

## Sistema de Adesinas

### Adhesin Component

```csharp
public class Adhesin : MonoBehaviour
{
    public Cell Cell1, Cell2;
    public SpringJoint2D spring;
    public Vector2 Cell1AnchorPoint, Cell2AnchorPoint;
}
```

**Mecânica:**
- **SpringJoint2D**: Física Unity para conexão elástica
- **Anchor Points**: Pontos específicos de conexão nas células
- **Auto-destruição**: Remove-se quando células ficam muito distantes
- **Herança**: Reconstrói conexões dos pais nos filhos

### Algoritmo de Herança de Adesinas

```csharp
void ReBuildAdhesins(Cell child, float childAngle)
{
    foreach (Adhesin ad in adhesins)
    {
        Cell otherCell = (ad.Cell1 != this) ? ad.Cell1 : ad.Cell2;
        
        // Cria nova adesina entre filho e célula conectada ao pai
        GameObject adhesin = Instantiate(PrefabSupplier.AdhesinPrefabReference);
        Adhesin adhesinData = adhesin.GetComponent<Adhesin>();
        adhesinData.Cell1 = child;
        adhesinData.Cell2 = otherCell;
        
        // Rotaciona anchor point baseado no ângulo do filho
        adhesinData.Cell1AnchorPoint = ad.Cell1AnchorPoint.Rotate(-childAngle);
    }
}
```

## Análise de Arquitetura

### Pontos Fortes

1. **Simplicidade**: Estrutura direta e fácil de entender
2. **Flexibilidade**: Fácil adição de novos parâmetros
3. **Unity Integration**: Aproveita sistemas nativos (serialização, UI, física)
4. **Performance**: Structs eficientes para dados

### Pontos de Melhoria

1. **Separação de Responsabilidades**: Lógica de simulação misturada com Unity
2. **Testabilidade**: Difícil testar sem Unity Test Runner
3. **Type Safety**: Sistema de mensagens não type-safe
4. **Validation**: Limitada validação de dados de entrada

### Conformidade com SIMULATION_RULE.md

**Violações Identificadas:**
- ❌ `Cell.cs` herda de `MonoBehaviour` e contém lógica de simulação
- ❌ Uso direto de `Time.deltaTime` em lógica de crescimento
- ❌ `Transform` e `Physics` misturados com regras celulares
- ❌ Sem separação clara entre modelo e visualização

**Sugestões de Refatoração:**
```csharp
// Estado puro (seguindo regra)
public struct CellState
{
    public float Mass;
    public Vector2 Position;
    public Genome Genome;
    public int ModeIndex;
}

// Lógica pura (seguindo regra)
public class CellSimulation
{
    public CellState Update(CellState state, float deltaTime)
    {
        // Lógica sem dependência de Unity
    }
}

// Adapter Unity (conecta regra com Unity)
public class CellView : MonoBehaviour
{
    public CellState State;
    private CellSimulation simulation;
    
    void Update()
    {
        State = simulation.Update(State, Time.deltaTime);
        SyncUnityTransform();
    }
}
```

## Extensibilidade

### Adicionando Novos Parâmetros

1. **Modificar CellMode**:
```csharp
public struct CellMode
{
    // ...parâmetros existentes...
    public float NewParameter;
}
```

2. **Criar UI Component**:
```csharp
public class NewParameterSlider : MonoBehaviour
{
    public void EditModeChanged() { /* sync */ }
    public void OnValueChanged() { /* apply */ }
}
```

3. **Integrar na Simulação**:
```csharp
// Em Cell.cs
float value = genome[CellModeIndex].NewParameter;
```

### Adicionando Novos Tipos de Célula

1. **Expandir Enum**:
```csharp
public enum CellType
{
    Photocyte,
    Chemocyte,  // Nova
    Neurocyte   // Nova
}
```

2. **Implementar Comportamentos**:
```csharp
switch (genome[CellModeIndex].Type)
{
    case CellType.Photocyte:
        PhotosynthesisUpdate();
        break;
    case CellType.Chemocyte:
        ChemosynthesisUpdate();
        break;
}
```

## Considerações de Performance

### Otimizações Atuais

1. **Structs**: Evitam garbage collection desnecessário
2. **Array Indexing**: Acesso O(1) aos modos
3. **Object Pooling**: Unity gerencia pools de GameObjects

### Gargalos Identificados

1. **BroadcastMessage**: Reflection overhead
2. **Instantiate/Destroy**: Criação frequente de objetos
3. **Physics Updates**: SpringJoint2D pode ser custoso em escala

### Sugestões de Otimização

1. **Event System**: Substituir BroadcastMessage por eventos C#
2. **Object Pooling**: Implementar pool para células e adesinas
3. **Spatial Partitioning**: Para detecção eficiente de colisões
4. **Job System**: Para processamento paralelo de células

## Debugging e Desenvolvimento

### Ferramentas Atuais

1. **Unity Inspector**: Visualização de propriedades
2. **Debug.Log**: Logging básico (limitado no código atual)
3. **Scene View**: Visualização em tempo real

### Ferramentas Sugeridas

1. **Genome Visualizer**: Gráfico de dependências entre modos
2. **Cell Lineage Tracker**: Rastreamento de linhagens celulares
3. **Performance Profiler**: Métricas específicas de simulação
4. **Validation System**: Verificação automática de genomas

Este documento fornece uma visão completa da implementação atual, identificando tanto os pontos fortes quanto as áreas que podem ser melhoradas para seguir as melhores práticas estabelecidas no projeto.
