# Avanade Challenge - Microservices Architecture

Sistema de microsserviços para gestão de inventário e vendas com API Gateway, implementado seguindo Clean Architecture e padrões de comunicação assíncrona.

## 🏗️ Arquitetura

```
├── APIGateway/                  # Gateway de API (Autenticação, Rate Limiting, Proxy)
├── Inventory/                   # Microsserviço de Inventário
│   ├── Inventory.Web/          # API REST
│   ├── Inventory.Application/  # Casos de Uso e Entidades
│   └── Inventory.InfraStructure/ # Repositórios e Entity Framework
├── Sales/                      # Microsserviço de Vendas
│   ├── Sales.Web/             # API REST
│   ├── Sales.Application/     # Casos de Uso e Entidades
│   └── Sales.Infrastructure/  # Repositórios, RabbitMQ e HTTP Gateway
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

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - APIs REST
- **Entity Framework Core** - ORM
- **SQL Server 2022** - Banco de dados
- **RabbitMQ** - Message broker
- **JWT** - Autenticação
- **Docker** - Containerização

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

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Testes com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

**Cobertura**: Testes unitários para entidades de domínio (Product, Order) com validações de negócio.

## 🔧 Melhorias Implementadas

### Clean Code
- **Nomenclatura**: Padronização de nomes de variáveis e métodos
- **DRY Principle**: Eliminação de código duplicado
- **Correções**: Correção de typos em nomes de classes e métodos
- **Interfaces**: Simplificação e alinhamento de contratos

### RabbitMQ
- **Configuração Otimizada**: Separação clara entre publishers e consumers
- **Error Handling**: Tratamento robusto de erros de deserialização
- **Logging**: Logs detalhados para debugging
- **Performance**: Configuração otimizada de consumers assíncronos

### Estrutura de Projeto
- **Organização**: Melhor organização de arquivos e namespaces
- **Dependências**: Configuração limpa de injeção de dependência
- **Configurações**: Centralização de configurações RabbitMQ

## ⚡ Performance

- **Request Timing**: Middleware customizado mede tempo de resposta
- **Headers**: `X-Response-Time-ms` em todas as respostas
- **Rate Limiting**: Controle de taxa no APIGateway
- **Circuit Breaker**: Resiliência em chamadas HTTP

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

> **Avanade Challenge** - Arquitetura de microsserviços com Clean Architecture, comunicação assíncrona e padrões de resiliência.

