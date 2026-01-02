# Cena Playground - Guia de Montagem

Esta cena permite testar células em tempo real: criar, observar reprodução/morte, e editar genomas.

## Arquivos Criados

- `Assets/Scripts/PlaygroundSetup.cs` — Configura prefabs e conecta componentes
- `Assets/Scripts/PlaygroundBoundary.cs` — Cria bordas circulares para conter células

## ⚠️ IMPORTANTE: Criar Tags Primeiro!

Antes de montar a cena, crie as tags necessárias:

1. **Edit → Project Settings → Tags and Layers**
2. Expanda "Tags"
3. Clique "+" e adicione:
   - `Microscope`
   - `Genome Editor`
4. A tag `Cell` já deve existir (usada pelo prefab)

## Como Montar a Cena no Unity Editor

### 1. Criar a Cena

1. **File → New Scene**
2. **File → Save As** → `Assets/Scenes/Playground.unity`
3. Delete objetos default exceto **Main Camera**

### 2. Configurar Main Camera

Selecione **Main Camera** e configure no Inspector:

| Propriedade | Valor |
|-------------|-------|
| Position | (0, 0, -10) |
| Projection | **Orthographic** |
| Size | **7** |
| Background | Cor escura (#1a1a2e) |

### 3. Criar GameController

1. **GameObject → Create Empty** → Renomear para `GameController`
2. **Add Component**: `PlaygroundSetup`
3. **Add Component**: `ContextManager`

### 4. Criar Microscope

1. **GameObject → Create Empty** → Renomear para `Microscope`
2. **Tag**: Selecionar `Microscope` (criada no passo anterior)
3. **Add Component**: `Microscope`
4. Criar filho **Substrate**:
   - Clique direito em Microscope → Create Empty
   - Renomear para `Substrate`
   - **Add Component**: `Substrate`

### 5. Criar Boundary (Borda Circular)

1. **GameObject → Create Empty** → Renomear para `Boundary`
2. **Add Component**: `PlaygroundBoundary`
3. No Inspector, configurar:
   - **Radius**: 5
   - **Segments**: 64

### 6. Criar GenomeEditor (IMPORTANTE!)

1. **GameObject → Create Empty** → Renomear para `GenomeEditor`
2. **Tag**: Selecionar `Genome Editor`
3. **Add Component**: `GenomeEditor`
4. **NÃO desative o GameObject ainda** - deixe ativo para o setup inicial

### 7. Conectar Referências no PlaygroundSetup (CRÍTICO!)

Selecione **GameController** e no componente `PlaygroundSetup`:

| Campo | O que arrastar |
|-------|----------------|
| **Cell Prefab** | `Assets/Prefabs/Cell.prefab` (do Project) |
| **Adhesin Prefab** | `Assets/Prefabs/Adhesin.prefab` (do Project) |
| **Microscope** | GameObject `Microscope` (da Hierarchy) |
| **Genome Editor** | GameObject `GenomeEditor` (da Hierarchy) |
| **Context Manager** | O próprio `GameController` ou deixe vazio |

### 8. Conectar Referências no ContextManager

Ainda no **GameController**, no componente `ContextManager`:

| Campo | O que arrastar |
|-------|----------------|
| **Microscope** | GameObject `Microscope` (da Hierarchy) |
| **Genome Editor** | GameObject `GenomeEditor` (da Hierarchy) |

### 9. (Opcional) Adicionar UI para Contagem de Células

1. **GameObject → UI → Canvas**
2. Criar Text filho (GameObject → UI → Text):
   - Nome: `Instructions`
   - Anchor Preset: Top Center
   - Text: `Clique = Criar célula | G = Editor | M = Microscópio | C = Limpar`
3. Criar outro Text:
   - Nome: `CellCount`  
   - Anchor Preset: Top Left
   - Text: `Células: 0`
4. No `PlaygroundSetup`, conectar:
   - **Instructions Text** → `Instructions`
   - **Cell Count Text** → `CellCount`

## Hierarquia Final

```
Playground (cena)
├── Main Camera
├── GameController
│   ├── PlaygroundSetup ✓ (com prefabs e referências conectados)
│   └── ContextManager ✓ (com referências conectados)
├── Microscope (tag: Microscope)
│   ├── Microscope ✓
│   └── Substrate
│       └── Substrate ✓
├── Boundary
│   └── PlaygroundBoundary ✓
├── GenomeEditor (tag: Genome Editor)
│   └── GenomeEditor ✓
└── Canvas (opcional)
    ├── Instructions
    └── CellCount
```

## Controles

| Tecla | Ação |
|-------|------|
| **Clique** | Criar célula na posição do mouse |
| **G** | Alternar para Editor de Genoma |
| **M** | Alternar para Microscópio |
| **C** | Limpar todas as células |

## Testar

1. **Play** (Ctrl+P)
2. Verifique no Console se aparece:
   - `[PlaygroundSetup] Prefabs configurados`
   - `[PlaygroundSetup] Microscope configurado com genoma de X modos`
   - `[PlaygroundSetup] Modo: Microscópio`
   - `[PlaygroundSetup] Cena pronta!`
3. Clique dentro do círculo cyan para criar células
4. Observe as células crescerem, dividirem e morrerem

## Troubleshooting

| Problema | Solução |
|----------|---------|
| **NullReferenceException no ContextManager** | Conectar Microscope e GenomeEditor no Inspector do ContextManager |
| **Células não aparecem ao clicar** | 1. Verificar se Cell Prefab está conectado no PlaygroundSetup<br>2. Verificar se clicou dentro do raio 5 (círculo cyan)<br>3. Verificar Console para erros |
| **Erro "tag not found"** | Criar tags em Edit → Project Settings → Tags and Layers |
| **GenomeEditor não aparece** | Verificar se o GameObject está ativo e tag está correta |
| **Células escapam da borda** | Verificar se Boundary está na posição (0,0,0) |

## Verificação Rápida

No Console (Window → General → Console), você deve ver estas mensagens ao dar Play:

```
[PlaygroundSetup] Prefabs configurados
[PlaygroundBoundary] Borda circular criada com raio 5
[PlaygroundSetup] GenomeEditor com 20 modos
[PlaygroundSetup] Microscope configurado com genoma de 20 modos
[PlaygroundSetup] Modo: Microscópio
[PlaygroundSetup] Cena pronta! Clique para criar células.
```

Se alguma mensagem não aparecer ou houver erros, revise os passos de configuração.

