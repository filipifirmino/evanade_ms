# Inventory.Tests

Este projeto contém os testes unitários e de integração para o projeto Inventory.

## Estrutura de Testes

```
Inventory.Tests/
├── ApplicationTestes/
│   ├── Entities/           # Testes de entidades de domínio
│   └── UseCasesTests/      # Testes de casos de uso
├── InfraStructureTests/
│   ├── GatewaysTests/      # Testes de gateways
│   ├── RepositoryTests/    # Testes de repositórios
│   └── RabbitTests/        # Testes de mensageria RabbitMQ
├── CoverageReport/         # Relatórios de cobertura de código
└── TestResults/           # Resultados dos testes
```

## Bibliotecas Utilizadas

- **xUnit**: Framework de testes
- **AutoBogus**: Geração automática de dados de teste
- **Moq**: Framework de mocking
- **Coverlet**: Coleta de cobertura de código
- **ReportGenerator**: Geração de relatórios HTML de cobertura
- **Microsoft.EntityFrameworkCore.InMemory**: Banco de dados em memória para testes

## Executando os Testes

### Executar todos os testes
```bash
dotnet test
```

### Executar testes com cobertura de código
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults --settings coverlet.runsettings
```

### Gerar relatório de cobertura
```bash
# Instalar ReportGenerator (se não estiver instalado)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relatório HTML
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html"
```

## Padrões de Teste

### Nomenclatura de Testes
- `[Método]_[Cenário]_[ResultadoEsperado]`
- Exemplo: `Product_WithValidData_ShouldBeValid`

### Estrutura AAA (Arrange, Act, Assert)
```csharp
[Fact]
public void Method_WithValidInput_ShouldReturnExpectedResult()
{
    // Arrange
    var input = AutoFaker.Generate<InputType>();
    
    // Act
    var result = methodUnderTest.Execute(input);
    
    // Assert
    Assert.Equal(expectedValue, result);
}
```

### Uso do AutoBogus
```csharp
// Geração simples
var product = AutoFaker.Generate<Product>();

// Geração com regras específicas
var product = new AutoFaker<Product>()
    .RuleFor(p => p.Price, 100m)
    .RuleFor(p => p.StockQuantity, 50)
    .Generate();
```

## Cobertura de Código

O projeto está configurado para gerar relatórios de cobertura de código que podem ser visualizados no diretório `CoverageReport`. O relatório HTML mostra:

- Cobertura de linhas por classe
- Cobertura de branches
- Linhas não cobertas
- Métricas gerais de cobertura

### Configuração de Cobertura

O arquivo `coverlet.runsettings` configura:
- Formato de saída: Cobertura
- Diretórios incluídos: Inventory.Application e Inventory.InfraStructure
- Exclusões: Testes, bibliotecas externas, etc.

## Boas Práticas

1. **Isolamento**: Cada teste deve ser independente
2. **Nomenclatura clara**: Nomes descritivos que explicam o cenário
3. **Dados de teste**: Use AutoBogus para gerar dados consistentes
4. **Mocks**: Use Moq para isolar dependências externas
5. **Assertions**: Use Assert do xUnit para verificações
6. **Cobertura**: Mantenha alta cobertura de código nos testes

## Troubleshooting

### Problemas comuns:

1. **Testes falhando**: Verifique se todas as dependências estão corretas
2. **Cobertura não gerada**: Execute com as configurações do coverlet.runsettings
3. **Relatório não abre**: Verifique se o ReportGenerator está instalado globalmente

### Logs e Debug:
```bash
# Executar com verbosidade alta
dotnet test --verbosity normal

# Executar apenas um teste específico
dotnet test --filter "FullyQualifiedName~ProductTests"
```
