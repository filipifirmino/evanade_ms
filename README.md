# Avanade Challenge - Microservices Architecture

Sistema de microsserviços para gestão de inventário e vendas com API Gateway, implementado seguindo Clean Architecture, padrões de comunicação assíncrona e com CI/CD automatizado.

## 🏗️ Arquitetura

```
├── APIGateway/                  # Gateway de API (Autenticação, Rate Limiting, Proxy)
│   ├── APIGateway.Web/         # API REST com Swagger
│   ├── APIGateway.ApplicationCore/ # Entidades e DTOs
│   ├── APIGateway.Infra/       # JWT, Rate Limiting, HTTP Proxy
│   └── APIGateway.Tests/       # Testes Unitários e Integração (98 testes - 100% passando)
├── Inventory/                   # Microsserviço de Inventário
│   ├── Inventory.Web/          # API REST
│   ├── Inventory.Application/  # Casos de Uso e Entidades
│   ├── Inventory.InfraStructure/ # Repositórios e Entity Framework
│   └── Inventory.Tests/        # Testes Unitários e Integração
├── Sales/                      # Microsserviço de Vendas
│   ├── Sales.Web/             # API REST
│   ├── Sales.Application/     # Casos de Uso e Entidades
│   ├── Sales.Infrastructure/  # Repositórios, RabbitMQ e HTTP Gateway
│   └── Sales.Tests/           # Testes Unitários e Integração
├── .github/workflows/          # CI/CD com GitHub Actions
└── docker-compose.yml         # SQL Server e RabbitMQ
```

## 🚀 Funcionalidades

### APIGateway
- **Autenticação JWT**: Sistema de autenticação com tokens
- **Rate Limiting**: Controle de taxa de requisições por endpoint
- **Proxy Reverso**: Roteamento para microsserviços downstream
- **Logging**: Middleware de logging de requisições
- **Health Checks**: Monitoramento de saúde dos serviços

### Inventory Microservice
- **CRUD de Produtos**: Gestão completa de produtos
- **Controle de Estoque**: Reserva, liberação e adição de estoque
- **Validações**: Regras de negócio para quantidade e preços
- **Eventos RabbitMQ**: Consumo de eventos OrderCreated e publicação de OrderConfirmed
- **API REST**: `GET/POST/PUT/DELETE /api/v1/Product/*`

### Sales Microservice
- **CRUD de Pedidos**: Gestão completa de pedidos
- **Estados do Pedido**: Created, Confirmed, Cancelled, Failed
- **Integração com Inventory**: Verificação de estoque via HTTP
- **Eventos RabbitMQ**: Publicação de eventos OrderCreated e consumo de OrderConfirmed
- **API REST**: `GET/POST/PUT/DELETE /api/v1/Sales/*`

## 🛠️ Stack Tecnológico

### Core Technologies
- **.NET 9.0** - Framework principal
- **ASP.NET Core** - APIs REST
- **Entity Framework Core** - ORM
- **SQL Server 2022** - Banco de dados
- **RabbitMQ** - Message broker
- **JWT** - Autenticação
- **Docker** - Containerização

### CI/CD & DevOps
- **GitHub Actions** - Pipeline de CI/CD
- **Codecov** - Análise de cobertura de código
- **Ubuntu Latest** - Ambiente de build
- **.NET 9.0.x** - Runtime de build

## 🗄️ Infraestrutura

### Serviços Docker
- **SQL Server**: `localhost:1433` (SA/Teste123!)
- **RabbitMQ**: `localhost:5672` + Management UI `localhost:15672`

### Bancos de Dados
- **InventoryDb**: Tabela `Products` (ProductId, Name, Description, Price, StockQuantity)
- **SalesDb**: Tabelas `Orders` e `OrderItems` (OrderId, CustomerId, TotalAmount, Status)

## 🚀 Execução

### Pré-requisitos
- .NET 9.0 SDK
- Docker Desktop

### 1. Infraestrutura
```bash
# Iniciar SQL Server e RabbitMQ
docker-compose up -d
```

### 2. APIs
```bash
# APIGateway (porta 5000)
cd APIGateway/APIGateway.Web
dotnet run

# Inventory (porta 5172)
cd Inventory/Inventory/Inventory.Web
dotnet run

# Sales (porta 5048)
cd Sales/Sales/Sales.Web
dotnet run
```

### 3. Acesso
- **APIGateway**: `https://localhost:5000` (Swagger na raiz)
- **Inventory**: `https://localhost:5172/swagger`
- **Sales**: `https://localhost:5048/swagger`

## 🔄 Comunicação Assíncrona

### Fluxo RabbitMQ
```
Sales → [PUBLICA] → order-created-queue → [CONSUME] → Inventory
Sales ← [CONSUME] ← inventory-stock-update-confirmed ← [PUBLICA] ← Inventory
```

### Eventos
- **OrderCreated**: Publicado pelo Sales quando um pedido é criado
- **OrderConfirmed**: Publicado pelo Inventory após confirmar estoque

### Filas Configuradas
- **order-created-queue**: Sales publica, Inventory consome
- **inventory-stock-update-confirmed**: Inventory publica, Sales consome

## 🏛️ Padrões Arquiteturais

### Clean Architecture
- **Web**: Controllers e DTOs
- **Application**: Casos de uso e entidades de domínio
- **Infrastructure**: Repositórios, HTTP clients e message brokers

### Padrões Implementados
- **Repository Pattern**: Abstração de acesso a dados
- **Gateway Pattern**: Interface entre camadas
- **CQRS**: Separação de comandos e consultas
- **Event-Driven**: Comunicação assíncrona via RabbitMQ
- **Circuit Breaker**: Resiliência em chamadas HTTP
- **Clean Code**: Nomenclatura consistente, DRY principle, código limpo

### Entidades de Domínio

#### Product (Inventory)
```csharp
public class Product
{
    public void Reserve(int quantity)     // Reserva estoque
    public void Release(int quantity)     // Libera estoque
    public void AddStock(int quantity)    // Adiciona estoque
    public void SetPrice(decimal price)   // Define preço
}
```

#### Order (Sales)
```csharp
public class Order
{
    public void AddItem(Guid productId, int quantity, decimal price)
    public void CalculateTotal()
    public void Confirm()
    public void Cancel(string reason)
    public bool IsValid()
}
```

## 🧪 Testes e Cobertura de Código

### ✅ Status dos Testes
- **APIGateway**: 98 testes unitários e de integração ✅
- **Inventory**: Testes unitários e de integração ✅
- **Sales**: Testes unitários e de integração ✅

### 🛠️ Ferramentas e Execução

**Ferramentas Utilizadas:**
- **xUnit**: Framework de testes unitários
- **AutoBogus**: Geração automática de dados de teste
- **Moq**: Framework de mocking
- **FluentAssertions**: Asserções mais legíveis
- **Microsoft.AspNetCore.Mvc.Testing**: Testes de integração
- **Coverlet**: Coleta de cobertura de código
- **ReportGenerator**: Geração de relatórios HTML

**Comandos de Execução:**
```bash
# Executar todos os testes
dotnet test

# Testes com cobertura (APIGateway)
cd APIGateway
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults --settings APIGateway.Tests/coverlet.runsettings

# Gerar relatório HTML de cobertura
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:"Html"
```

### 🎯 Cobertura por Projeto

**APIGateway **
- Entities: User, RateLimitePolicy, ServiceRoute
- DTOs: LoginRequest
- Services: AuthService, JwtTokenService
- Middleware: ExceptionHandling, RateLimiting
- Controllers: AuthController, GatewayController
- Infrastructure: RouteConfiguration, InMemoryRateLimit, HttpProxy

**Inventory**
- Entities: Product com validações de negócio
- Use Cases: CRUD de produtos, controle de estoque
- Infrastructure: Repositórios, Entity Framework, RabbitMQ

**Sales**
- Entities: Order com estados e validações
- Use Cases: CRUD de pedidos, processamento de pedidos
- Infrastructure: Repositórios, RabbitMQ, HTTP Gateway
- Events: OrderCreated, OrderConfirmed

## 🔧 Melhorias Implementadas

### 🚀 CI/CD e Automação
- **GitHub Actions**: Pipeline automatizado para testes e cobertura
- **Coverlet**: Coleta de cobertura de código em todos os projetos
- **ReportGenerator**: Relatórios HTML automáticos
- **Codecov**: Upload automático de métricas de cobertura
- **Multi-Project**: Suporte para múltiplos projetos na mesma solução

### 🧹 Clean Code
- **Nomenclatura**: Padronização de nomes de variáveis e métodos
- **DRY Principle**: Eliminação de código duplicado
- **Correções**: Correção de typos em nomes de classes e métodos
- **Interfaces**: Simplificação e alinhamento de contratos
- **Testes Limpos**: Remoção de comentários desnecessários nos testes

### 🔄 RabbitMQ Otimizado
- **Configuração Otimizada**: Separação clara entre publishers e consumers
- **Error Handling**: Tratamento robusto de erros de deserialização
- **Logging**: Logs detalhados para debugging
- **Performance**: Configuração otimizada de consumers assíncronos
- **Testes**: Cobertura completa dos consumers e producers

### 📁 Estrutura de Projeto
- **Organização**: Melhor organização de arquivos e namespaces
- **Dependências**: Configuração limpa de injeção de dependência
- **Configurações**: Centralização de configurações RabbitMQ
- **Testes**: Estrutura padronizada de testes em todos os projetos
- **Documentação**: READMEs específicos para cada projeto de teste


## ⚡ Performance

- **Request Timing**: Middleware customizado mede tempo de resposta
- **Headers**: `X-Response-Time-ms` em todas as respostas
- **Rate Limiting**: Controle de taxa no APIGateway
- **Circuit Breaker**: Resiliência em chamadas HTTP
- **Test Performance**: Testes otimizados para execução rápida

## 🔧 Configuração

### APIGateway
- **JWT Secret**: Configurado em `appsettings.json`
- **Rate Limits**: Por endpoint (auth: 5/min, sales: 50/min, inventory: 100/min)
- **Service URLs**: Inventory (5172), Sales (5048)

### Connection Strings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=InventoryDb;User Id=SA;Password=Teste123!;TrustServerCertificate=true;"
  }
}
```

### RabbitMQ Configuration
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Port": 5672,
    "AutomaticRecoveryEnabled": true,
    "NetworkRecoveryInterval": "00:00:10",
    "Queues": [
      {
        "Name": "order-created-queue",
        "Exchange": "order-exchange",
        "RoutingKey": "order.created"
      },
      {
        "Name": "inventory-stock-update-confirmed",
        "Exchange": "inventory-exchange", 
        "RoutingKey": "inventory.stock.updated"
      }
    ]
  }
}
```

---

## 🎯 Resumo do Projeto

Este projeto implementa uma **arquitetura de microsserviços completa** com:

### ✅ **Qualidade e Confiabilidade**
- **CI/CD automatizado** com GitHub Actions
- **Relatórios de cobertura** automáticos em HTML
- **Testes unitários e de integração** em todos os projetos

### 🏗️ **Arquitetura Robusta**
- **Clean Architecture** em todos os microsserviços
- **Comunicação assíncrona** via RabbitMQ
- **API Gateway** com autenticação JWT e rate limiting
- **Padrões de resiliência** e circuit breaker

### 🚀 **Tecnologias Modernas**
- **.NET 9.0** com ASP.NET Core
- **Entity Framework Core** para persistência
- **Docker** para containerização
- **GitHub Actions** para automação

---

> **Avanade Challenge** - Arquitetura de microsserviços com Clean Architecture, comunicação assíncrona, padrões de resiliência e com CI/CD automatizado.

