Regra: Simulação não pode depender de MonoBehaviour
=================================================

Resumo rápido
-------------

- Simulação deve ser lógica pura em C# (sem herdar de MonoBehaviour, sem usar UnityEngine diretamente dentro da lógica de simulação).
- Dados de estado e lógica devem estar separados: POCOs/structs para estado; classes puras para regras/atualizações.
- Tempo deve ser controlado explicitamente pela simulação (método Step/ Tick que recebe deltaTime ou uma contagem de passos).

Por que isso importa
--------------------

- Facilita testes unitários (simulação testável sem Unity Test Runner ou cenas).
- Permite fast-forward: rodar muitos passos rapidamente em CPU sem overhead de Unity.
- Permite gravação e replay determinístico (ou quase determinístico) se a simulação for isolada.
- Possibilita IA que interage com a simulação fora do loop de frame.
- Importante para projetos "Cell Lab-like" onde execução massiva e reprodutibilidade importam.

Regras concretas
----------------

1. Não usar `MonoBehaviour`, `Time`, `Transform`, `GameObject`, `Debug.Log` ou qualquer API de Unity dentro da lógica de simulação.
2. Separar claramente:
   - Modelos de dados (POCOs/structs): representam o estado da simulação (ex.: `CellState`, `GenomeData`, `WorldState`).
   - Computação / regras (classes puras): recebem estado e produzem novo estado (ex.: `CellPhysics`, `MetabolismSystem`).
   - Adapters/Bridges (em `Assets`/`MonoBehaviour`): apenas conectam Unity ao núcleo puro (render, input, persistência).
3. Tempo explícito: expor um método como `Step(float deltaTime)` ou `Tick(int steps)` na interface da simulação; nunca confiar em `Update()` internamente.
4. Tornar a simulação determinística tanto quanto possível: controle de RNG (inject a seeded RNG), evitar leituras de tempo global sem controle.

Contrato mínimo recomendado (entrada/saída)
------------------------------------------

- Inputs
  - Estado inicial (POCO): por exemplo `SimulationState`.
  - Sequência de entradas externas/eventos (opcional) ou um handler que aplica comandos por passo.
  - deltaTime (float) ou steps (int).

- Outputs
  - Estado atualizado (mutado ou novo objeto retornado, dependendo do design).
  - Eventos/Logs de mudança (opcional): colisões, mortes, divisão, etc.

- Erros
  - deltaTime <= 0 deve ser validado (ou documentado como permitido apenas para pausar).
  - Entradas inválidas devem produzir exceções controladas ou códigos de erro.

Exemplo de API (sugestão)
-------------------------

```csharp
// Representa apenas dados. Não referencia UnityEngine.
public class SimulationState
{
    public List<CellState> Cells = new List<CellState>();
    // ... outros campos puros
}

public interface ISimulation
{
    SimulationState State { get; }
    /// Avança a simulação em deltaTime segundos
    void Step(float deltaTime);
    void Reset(SimulationState initialState);
}

// Um runner Unity que apenas liga o loop de frames à simulação pura
public class SimulationRunner : MonoBehaviour
{
    private ISimulation _sim;

    private void Start()
    {
        _sim = new MySimulation(); // MySimulation é puro C# e não conhece Unity
    }

    private void Update()
    {
        _sim.Step(Time.deltaTime);
    }
}
```

Notas importantes de implementação
---------------------------------

- Injeção de dependências: passe interfaces (por exemplo `IRandom`, `ITimeProvider`) para permitir testes e replay.
- RNG controlado: use um RNG injetado com seed conhecido para obter execução repetível quando necessário.
- Determinismo e precisão numérica:
  - Float vs double: documente expectativas; diferenças de plataforma podem afetar determinismo absoluto.
  - Ordem de iteração: mantenha ordens previsíveis (não iterar sobre HashSet sem ordenação).
- Estado mutável x imutável: prefira mutação controlada no `SimulationState` para performance, mas documente pontos onde cópia/clone é necessária (por exemplo para replay ou snapshot).

Fast-forward e Replay
---------------------

- Fast-forward: chame `Step(fixedDelta)` em um loop em testes ou batch jobs, sem precisar de Unity frames.
- Replay: grave a sequência de entradas/ações (e, se necessário, a seed do RNG) por passo; para replay execute Step com a mesma sequência de entradas e seed.

Checklist de auditoria / refatoração (próximos passos)
-----------------------------------------------------

- [ ] Encontrar scripts que contenham `Update`, `FixedUpdate`, `Simulate`, `Tick`, `Step` e avaliar se executam lógica de simulação dentro de `MonoBehaviour`.
- [ ] Identificar classes que tocam `Cell` e verificar se fazem isso via lógica pura ou via Unity API.
- [ ] Refatorar uma parte pequena (por exemplo o loop de física das células) para a API `ISimulation` como prova de conceito.
- [ ] Escrever testes unitários para a POC (fast-forward + assert estados esperados).

Casos limites / problemas conhecidos
----------------------------------

- Dependências ocultas: cuidado com classes que aparentemente não usam Unity mas instanciam `GameObject`/`MonoBehaviour` internamente.
- Precauções com scene lifecycle: adaptadores Unity devem lidar com carregar/descartar cenas e persistência de estado.
- Precisão numérica em diferentes CPUs/architectures pode quebrar replay estritamente determinístico.

Conformidade com a regra (como medir)
-------------------------------------

- Lista negativa: arquivos que referenciam `UnityEngine` dentro de classes de domínio são suspeitos.
- Testes: capacidade de rodar toda a simulação headless (fora do Unity) por um runner de testes.

Onde colocar esta documentação
------------------------------

Sugestão: `Docs/SIMULATION_RULE.md` (este arquivo). Linkar no README do projeto e em PR templates quando alterações na simulação forem propostas.

📚 Tutorial prático
-------------------

Para um guia passo a passo de como aplicar estas regras, veja:

**[TUTORIAL_SIMULATION_RULES.md](TUTORIAL_SIMULATION_RULES.md)**

O tutorial inclui:
- Comandos para identificar código problemático
- Exemplos de código para cada fase da refatoração
- Testes unitários prontos para usar
- Checklist de migração por arquivo

Próximo passo opcional que posso executar agora
---------------------------------------------

- Rodar buscas no repositório pelos termos que você pediu (Update, FixedUpdate, Simulate, Tick, Step, Genome, Gene, Mutation, Adhesin, UnityEngine.UI, Dropdown, Slider, Toggle) e produzir um relatório anotado indicando scripts que mexem em células e os que alteram estado por frame.


---

Arquivo criado: `Docs/SIMULATION_RULE.md`

