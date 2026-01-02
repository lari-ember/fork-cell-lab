# Tutorial: Sistema de Edição de Genoma

## Introdução

Este tutorial ensina como usar o sistema de edição de genoma do Fork Cell Lab para criar e modificar comportamentos celulares. Você aprenderá a definir como as células crescem, se dividem e interagem entre si.

## Pré-requisitos

- Unity Editor aberto com o projeto Fork Cell Lab
- Cena Playground configurada
- Conhecimento básico de navegação no Unity

## Parte 1: Primeiros Passos

### Iniciando o Editor

1. **Abra a cena Playground**
   - File → Open Scene
   - Navegue até `Assets/Scenes/Playground.unity`

2. **Execute a simulação**
   - Clique no botão Play no Unity Editor
   - A cena deve carregar com uma área circular vazia

3. **Abra o editor de genoma**
   - Pressione a tecla `G`
   - Uma interface de edição deve aparecer na tela

### Interface do Editor

O editor de genoma contém os seguintes elementos:

- **Mode Selector**: Dropdown para escolher qual modo celular editar
- **Color Sliders**: Controles RGB para definir a cor da célula
- **Make Adhesin**: Checkbox para criar conexões entre células filhas
- **Child Selectors**: Dropdowns para definir modos dos filhos após divisão
- **Keep Adhesin**: Checkboxes para manter conexões dos pais nos filhos

## Parte 2: Criando seu Primeiro Genoma

### Configuração Básica

1. **Selecione o Mode 0**
   - No dropdown "Mode Selector", certifique-se que "Mode 0" está selecionado
   - Este será o modo inicial das suas células

2. **Defina uma cor**
   - Use os sliders Red, Green, Blue para escolher uma cor
   - Exemplo: R=1.0, G=0.5, B=0.0 para laranja

3. **Configure divisão básica**
   - Por padrão, `SplitMass` está configurada para divisão automática
   - Child 1 e Child 2 devem apontar para "Mode 0" (auto-referência)

### Teste Inicial

1. **Feche o editor de genoma**
   - Pressione `G` novamente para fechar

2. **Crie uma célula**
   - Clique em qualquer lugar dentro da área circular
   - Uma célula laranja deve aparecer

3. **Observe o crescimento**
   - A célula deve crescer gradualmente
   - Células na parte superior crescem mais rápido (mais luz)

4. **Aguarde a divisão**
   - Quando atingir massa suficiente, a célula se divide
   - Duas células filhas idênticas são criadas

## Parte 3: Modos Múltiplos

### Criando Comportamento Alternado

Vamos criar um padrão onde células alternam entre dois comportamentos:

1. **Abra o editor de genoma** (`G`)

2. **Configure Mode 0 (Vermelho)**
   - Mode Selector: "Mode 0"
   - Color: R=1.0, G=0.0, B=0.0 (vermelho)
   - Child 1: "Mode 1"
   - Child 2: "Mode 1"

3. **Configure Mode 1 (Azul)**
   - Mode Selector: "Mode 1"
   - Color: R=0.0, G=0.0, B=1.0 (azul)
   - Child 1: "Mode 0"
   - Child 2: "Mode 0"

### Teste do Comportamento Alternado

1. **Feche o editor** (`G`)
2. **Crie uma célula vermelha**
3. **Observe o padrão**:
   - Célula vermelha → divide em duas azuis
   - Células azuis → dividem em vermelhas
   - Cria padrão alternado de cores

## Parte 4: Adesinas e Conexões

### Configurando Adesinas

As adesinas criam conexões físicas entre células:

1. **Abra o editor de genoma** (`G`)

2. **Configure Mode 0 para criar adesinas**
   - Mode Selector: "Mode 0"
   - Make Adhesin: ✓ (marcado)
   - Child 1 Keep Adhesin: ✓
   - Child 2 Keep Adhesin: ✓

### Teste de Adesinas

1. **Crie algumas células**
2. **Observe as conexões**:
   - Células filhas ficam conectadas por "molas"
   - Conexões se estendem mas não quebram facilmente
   - Formam estruturas multicelulares

## Parte 5: Padrões Complexos

### Exemplo: Cadeia Linear

Vamos criar células que formam cadeias:

1. **Mode 0 (Iniciador - Verde)**
   - Color: R=0.0, G=1.0, B=0.0
   - Make Adhesin: ✓
   - Child 1: Mode 1
   - Child 2: Mode 1
   - Child 1 Keep Adhesin: ✓
   - Child 2 Keep Adhesin: ✗

2. **Mode 1 (Crescimento - Amarelo)**
   - Color: R=1.0, G=1.0, B=0.0
   - Make Adhesin: ✓
   - Child 1: Mode 1
   - Child 2: Mode 1
   - Child 1 Keep Adhesin: ✓
   - Child 2 Keep Adhesin: ✗

### Exemplo: Estrutura Ramificada

Para criar ramificações:

1. **Mode 0 (Tronco - Marrom)**
   - Color: R=0.6, G=0.3, B=0.1
   - Make Adhesin: ✓
   - Child 1: Mode 0 (continua tronco)
   - Child 2: Mode 1 (inicia ramo)

2. **Mode 1 (Ramo - Verde)**
   - Color: R=0.0, G=1.0, B=0.0
   - Make Adhesin: ✓
   - Child 1: Mode 1
   - Child 2: Mode 1

## Parte 6: Controles Avançados

### Ângulos de Divisão

Os ângulos controlam a direção da divisão celular:

- **SplitAngle**: Ângulo principal da divisão
- **Child1Angle/Child2Angle**: Ajustes para cada filho

*Nota: Atualmente estes parâmetros são definidos no código, não na UI*

### Massa de Divisão

- **SplitMass**: Define quando a célula se divide
- Valores menores = divisão mais rápida
- Valores maiores = células maiores antes da divisão

*Nota: Atualmente definido no código como padrão*

## Parte 7: Dicas e Estratégias

### Planejamento de Genoma

1. **Comece simples**: Use poucos modos inicialmente
2. **Teste incrementalmente**: Adicione complexidade gradualmente
3. **Use cores distintas**: Para identificar facilmente diferentes modos

### Padrões Úteis

1. **Auto-referência**: Mode aponta para si mesmo (crescimento contínuo)
2. **Ciclo binário**: Dois modos que alternam entre si
3. **Especialização**: Modos diferentes para funções específicas
4. **Terminação**: Modos que param de se dividir

### Debugando Comportamentos

1. **Use cores**: Para rastrear linhagens celulares
2. **Observe adesinas**: Para entender conexões
3. **Conte gerações**: Para verificar ciclos
4. **Teste isoladamente**: Um modo por vez

## Parte 8: Limitações e Soluções

### Limitações Atuais

1. **UI Limitada**: Nem todos os parâmetros são editáveis via interface
2. **Sem persistência**: Genomas não são salvos automaticamente
3. **Validação limitada**: Configurações inválidas podem causar problemas

### Soluções Temporárias

1. **Documente**: Anote configurações que funcionam bem
2. **Teste cuidadosamente**: Verifique se comportamentos são como esperado
3. **Backup de cenas**: Salve cenas com genomas interessantes

## Parte 9: Exemplos Prontos

### Genoma: Colônia Simples
- Mode 0: Vermelho, auto-referência, sem adesinas
- Resultado: Células individuais vermelhas

### Genoma: Cadeia Conectada
- Mode 0: Azul, auto-referência, com adesinas
- Resultado: Estruturas lineares conectadas

### Genoma: Padrão Zebra
- Mode 0: Preto, filhos → Mode 1
- Mode 1: Branco, filhos → Mode 0
- Resultado: Padrão alternado preto-branco

### Genoma: Estrutura Ramificada
- Mode 0: Marrom, Child1 → Mode 0, Child2 → Mode 1
- Mode 1: Verde, auto-referência
- Resultado: Tronco com ramos verdes

## Conclusão

O sistema de edição de genoma oferece grande flexibilidade para experimentar com comportamentos celulares. Com prática, você pode criar padrões complexos que simulam aspectos da vida real como crescimento direcionado, especialização celular e formação de estruturas multicelulares.

### Próximos Passos

1. **Experimente**: Teste diferentes combinações
2. **Documente**: Registre descobertas interessantes
3. **Compartilhe**: Mostre criações para outros usuários
4. **Contribua**: Sugira melhorias para o sistema

### Recursos Adicionais

- `GENOME_EDITOR_DOCUMENTATION.md` - Documentação técnica completa
- `SIMULATION_RULE.md` - Regras de arquitetura do sistema
- Código fonte em `Assets/Scripts/` - Para modificações avançadas
