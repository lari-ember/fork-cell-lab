# Documentação do Sistema de Edição de Genoma

## Visão Geral

O sistema de edição de genoma do Fork Cell Lab permite aos usuários criar e modificar genomas que definem o comportamento das células na simulação. O sistema é composto por uma estrutura de dados de genoma e uma interface de usuário para edição visual.

## Arquitetura do Sistema

### 1. Estrutura de Dados

#### Genome (`Assets/Scripts/Genome.cs`)

O `Genome` é um struct que contém uma coleção de modos celulares:

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

**Funcionalidades:**
- Armazena múltiplos modos celulares em um array
- Define o modo inicial através do `InitialModeIndex`
- Suporta clonagem para herança genética
- Permite adicionar novos modos dinamicamente

#### CellMode (`Assets/Scripts/Genome.cs`)

O `CellMode` define o comportamento específico de uma célula em um determinado estado:

```csharp
[Serializable]
public struct CellMode
{
    public CellType Type;
    public Color Color;
    public bool MakeAdhesin;
    public float SplitMass;
    public float SplitAngle;
    public int Child1ModeIndex;
    public int Child2ModeIndex;
    public bool Child1KeepAdhesin;
    public bool Child2KeepAdhesin;
    public float Child1Angle;
    public float Child2Angle;
}
```

**Propriedades:**
- **Type**: Tipo da célula (atualmente apenas `Photocyte`)
- **Color**: Cor visual da célula
- **MakeAdhesin**: Se deve criar adesinas entre células filhas
- **SplitMass**: Massa necessária para divisão celular
- **SplitAngle**: Ângulo de divisão celular
- **Child1/Child2ModeIndex**: Índices dos modos dos filhos após divisão
- **Child1/Child2KeepAdhesin**: Se os filhos mantêm adesinas existentes
- **Child1/Child2Angle**: Ângulos relativos dos filhos

### 2. Editor de Interface

#### GenomeEditor (`Assets/Scripts/GenomeEditor/GenomeEditor.cs`)

Componente principal que gerencia a interface de edição:

```csharp
public class GenomeEditor : MonoBehaviour
{
    public bool Active { get; set; }
    public Genome CurrentGenome;
    public int CurrentMode { get; set; }
}
```

**Funcionalidades:**
- Ativa/desativa a interface de edição
- Gerencia o genoma atualmente sendo editado
- Controla qual modo está sendo editado
- Sincroniza mudanças entre componentes UI

#### Componentes de Interface

1. **CellModeSelector**: Dropdown para seleção do modo atual
2. **CellColor**: Controles RGB para cor da célula
3. **MakeAdhesinCheckbox**: Checkbox para criação de adesinas
4. **KeepAdhesinCheckbox**: Checkbox para manutenção de adesinas
5. **ChildModeSelector**: Seletores para modos dos filhos

### 3. Integração com Simulação

#### Cell (`Assets/Scripts/Simulation/Cell.cs`)

Cada célula na simulação possui uma referência ao genoma e ao modo atual:

```csharp
public class Cell : MonoBehaviour
{
    public Genome genome;
    public int CellModeIndex;
}
```

**Comportamentos baseados no genoma:**
- **Crescimento**: Ganha massa baseada na posição (fotossíntese simulada)
- **Divisão**: Divide quando atinge `SplitMass` do modo atual
- **Herança**: Filhos recebem cópias do genoma com seus respectivos modos
- **Adesinas**: Conecta células baseado nas configurações do modo

#### Substrate (`Assets/Scripts/Simulation/Substrate.cs`)

Permite criar novas células com o genoma atual do editor:

```csharp
public class Substrate : MonoBehaviour
{
    public Genome CurrentGenome;
}
```

## Fluxo de Funcionamento

### 1. Edição de Genoma

1. **Ativação**: Pressione `G` para abrir o editor de genoma
2. **Seleção de Modo**: Use o dropdown para selecionar qual modo editar
3. **Configuração**: Ajuste propriedades usando os controles UI:
   - Sliders RGB para cor
   - Checkboxes para comportamento de adesinas
   - Dropdowns para seleção de modos filhos
4. **Aplicação**: Mudanças são aplicadas instantaneamente ao genoma

### 2. Criação de Células

1. **Substrato Ativo**: Com o microscópio ativo, clique para criar células
2. **Genoma Aplicado**: Novas células recebem o genoma atual do editor
3. **Modo Inicial**: Células começam no modo definido por `InitialModeIndex`

### 3. Simulação de Vida Celular

1. **Crescimento**: Células ganham massa baseada na luz (posição Y)
2. **Divisão**: Quando atingem `SplitMass`, dividem-se em duas células filhas
3. **Herança Genética**: Filhos herdam genoma dos pais com possíveis variações
4. **Morte**: Células com massa muito baixa morrem

## Tipos de Células e Comportamentos

### Photocyte (Fotócito)

Atualmente o único tipo disponível:

- **Metabolismo**: Ganha energia através de fotossíntese simulada
- **Crescimento Linear**: `0.08f * posição.y * deltaTime`
- **Crescimento Radial**: `0.15f * distância_centro * deltaTime`
- **Divisão**: Massa mínima configurável por modo
- **Morte**: Massa < 0.6f

### Adesinas

Sistema de conexão entre células:

- **Criação**: Baseada na configuração `MakeAdhesin`
- **Manutenção**: Filhos podem manter conexões dos pais
- **Física**: Implementada com `SpringJoint2D`
- **Auto-destruição**: Remove-se se células ficam muito distantes

## Configurações Avançadas

### Ângulos de Divisão

- **SplitAngle**: Ângulo principal de divisão
- **Child1Angle/Child2Angle**: Ajustes relativos para cada filho
- **Resultado**: Permite padrões complexos de crescimento

### Herança de Adesinas

- **Child1KeepAdhesin/Child2KeepAdhesin**: Controla se filhos mantêm conexões
- **Reconstrução**: Adesinas são recriadas com novos pontos de ancoragem
- **Seletividade**: Cada filho pode ter comportamento diferente

### Modos Recursivos

- Modos podem referenciar a si mesmos ou outros modos
- Permite ciclos de desenvolvimento complexos
- Facilita criação de padrões emergentes

## Limitações Atuais

1. **Tipos de Células**: Apenas `Photocyte` implementado
2. **Propriedades Fixas**: Algumas propriedades ainda não são editáveis via UI
3. **Validação**: Limitada validação de configurações inválidas
4. **Persistência**: Genomas não são salvos automaticamente

## Arquivos Relacionados

### Core
- `Genome.cs` - Estruturas de dados principais
- `Assets/Scripts/Genome.cs` - Implementação no Unity

### Editor de Interface
- `GenomeEditor/GenomeEditor.cs` - Controlador principal
- `GenomeEditor/CellModeSelector.cs` - Seletor de modo
- `GenomeEditor/CellColor.cs` - Editor de cores
- `GenomeEditor/MakeAdhesinCheckbox.cs` - Controle de adesinas
- `GenomeEditor/ChildModeSelector.cs` - Seletor de modos filhos

### Simulação
- `Simulation/Cell.cs` - Comportamento celular
- `Simulation/Adhesin.cs` - Sistema de conexões
- `Simulation/Substrate.cs` - Criação de células

### Gerenciamento
- `ContextManager.cs` - Coordenação entre sistemas
- `PrefabSupplier.cs` - Fornecimento de prefabs

Este sistema fornece uma base sólida para experimentação com vida artificial, permitindo aos usuários explorar como diferentes configurações genéticas afetam o comportamento emergente de colônias celulares.
