# Simulador de Órbitas Planetárias

Este é um simulador de órbitas planetárias desenvolvido em C# que modela a dinâmica de corpos celestes no Sistema Solar. O projeto utiliza o OxyPlot para visualização das trajetórias e geração de gráficos, e calcula as órbitas com base nas leis da gravitação universal de Newton.

![img.png](assets/images/formules.png)

## Funcionalidades

- F1 - Simular o movimento dos planetas e de algumas luas do Sistema Solar.
- F2 - Calcular as forças gravitacionais entre os corpos com base nas massas e distâncias.
- F3 - Gerar um gráfico 2D mostrando as trajetórias orbitais dos corpos.
- F4 - Exibir o nome de cada corpo na posição final de sua órbita.

## Tecnologias Utilizadas

- **C#**: Linguagem de programação principal.
- **OxyPlot**: Biblioteca para gráficos e visualizações.
- **Math.NET Numerics**: Biblioteca para cálculos matemáticos.
- **SkiaSharp**: Biblioteca para renderização de gráficos em PNG.

## Como Funciona

O simulador funciona em uma simulação temporal, onde os corpos (planetas, luas, etc.) interagem entre si por forças gravitacionais. O código utiliza a fórmula de gravitação universal para calcular as forças de atração entre os corpos e, a partir disso, calcula suas posições e velocidades ao longo do tempo.

### Passos da Simulação

1. **Inicialização**: Definição dos corpos celestes com suas massas, posições iniciais e velocidades.
2. **Cálculo das Forças**: Para cada passo de tempo, a gravidade é calculada entre todos os pares de corpos.
3. **Atualização das Posições**: A posição de cada corpo é atualizada com base nas forças e velocidades calculadas.
4. **Geração de Gráfico**: As trajetórias de cada corpo são armazenadas e plotadas em um gráfico 2D.
5. **Exportação do Gráfico**: O gráfico gerado é salvo como uma imagem PNG.

## Estrutura do Projeto

- **`Simulator.cs`**: Contém a lógica de simulação, incluindo os cálculos de gravitação e atualização de posições.
- **`Plotter.cs`**: Responsável por gerar e salvar o gráfico das trajetórias orbitais.
- **`Program.cs`**: Ponto de entrada para a execução do simulador.
- **`Body.cs`**: Define os corpos celestes, suas propriedades e como interagem na simulação.

## Exemplo de Uso

### Inicializando o Simulador

O simulador é configurado com os seguintes corpos do Sistema Solar:

- **Sol**
- **Mercúrio**
- **Vênus**
- **Terra**
- **Marte**
- **Júpiter**
- **Saturno**
- **Urano**
- **Netuno**
- **Lua** (satélite da Terra)
- **Io, Europa, Ganimedes, Calisto** (satélites de Júpiter)

### Executando a Simulação

A simulação é executada com o seguinte código no `Program.cs`:

```csharp
Simulator simulator = new();
simulator.Execute();
```

Isso cria e simula os movimentos dos corpos celestes durante um período de um ano (365 dias), com um intervalo de tempo de 86400 segundos (1 dia).

### Gerando o Gráfico
O gráfico gerado será salvo como uma imagem PNG no diretório Plots, e pode ser visualizado ou compartilhado.

![gráfico](sources/Plots/solar_system_simulation.png)