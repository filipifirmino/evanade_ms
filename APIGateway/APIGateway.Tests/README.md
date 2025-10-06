# APIGateway.Tests

Este projeto contém os testes unitários e de integração para o projeto APIGateway.

## Estrutura de Testes

A estrutura de testes está organizada da seguinte forma:

```
APIGateway.Tests/
├── ApplicationCore.Tests/
│   ├── Entities/           # Testes de entidades de domínio
│   ├── Services/           # Testes de serviços de aplicação
│   └── DTOs/              # Testes de DTOs
├── Infra.Tests/
│   ├── Authentication/    # Testes de autenticação JWT
│   ├── Http/             # Testes de serviços HTTP
│   └── RateLimiting/     # Testes de rate limiting
├── Web.Tests/
│   ├── Controllers/      # Testes de controllers
│   └── Middleware/       # Testes de middleware
├── CoverageReport/       # Relatórios de cobertura de código
└── TestResults/         # Resultados dos testes
```

## Bibliotecas Utilizadas

- **xUnit**: Framework de testes
- **AutoBogus**: Geração automática de dados de teste
- **Moq**: Framework de mocking
- **Coverlet**: Coleta de cobertura de código
- **ReportGenerator**: Geração de relatórios HTML de cobertura
- **Microsoft.AspNetCore.Mvc.Testing**: Testes de integração para ASP.NET Core
- **Microsoft.Extensions.Logging.Testing**: Testes de logging
- **FluentAssertions**: Assertions mais legíveis

## Executando os Testes

### Executar todos os testes com cobertura de código
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults --settings coverlet.runsettings
```

### Gerar relatório de cobertura
```bash
# Instalar ReportGenerator (se não estiver instalado)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relatório HTML
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:"Html"
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
- Diretórios incluídos: APIGateway.ApplicationCore, APIGateway.Infra e APIGateway.Web
- Exclusões: Testes, bibliotecas externas, etc.

## Testes por Camada

### ApplicationCore.Tests
- **Entities**: Testes de validação de entidades de domínio
- **Services**: Testes de lógica de negócio dos serviços
- **DTOs**: Testes de serialização e validação de DTOs

### Infra.Tests
- **Authentication**: Testes de geração e validação de tokens JWT
- **Http**: Testes de proxy HTTP e encaminhamento de requisições
- **RateLimiting**: Testes de controle de taxa de requisições

### Web.Tests
- **Controllers**: Testes de endpoints da API
- **Middleware**: Testes de middleware de autenticação, rate limiting e logging

## Executando Testes Específicos

```bash
# Executar apenas testes unitários
dotnet test --filter "Category=Unit"

# Executar apenas testes de integração
dotnet test --filter "Category=Integration"

# Executar testes de uma camada específica
dotnet test --filter "FullyQualifiedName~ApplicationCore"
dotnet test --filter "FullyQualifiedName~Infra"
dotnet test --filter "FullyQualifiedName~Web"
```
