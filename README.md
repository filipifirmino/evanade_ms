# Avanade Challenge - Microservices Inventory & Sales

Bem-vindo ao projeto **Avanade Challenge**! Este repositório reúne dois microsserviços principais para gestão de inventário e vendas, implementados seguindo os princípios da Clean Architecture e prontos para rodar em ambiente Docker.

## 🏗️ Arquitetura do Projeto

O projeto segue a **Clean Architecture** com separação clara de responsabilidades:

```
├── Inventory/                    # Microsserviço de Inventário
│   ├── Inventory.Web/           # Camada de Apresentação (API)
│   ├── Inventory.Application/   # Camada de Aplicação (Casos de Uso)
│   ├── Inventory.InfraStructure/ # Camada de Infraestrutura (Dados)
│   ├── Inventory.Tests/         # Testes Unitários
│   └── Inventory.sln            # Solução .NET
├── Sales/                       # Microsserviço de Vendas
│   ├── Sales.Web/              # Camada de Apresentação (API)
│   ├── Sales.Application/      # Camada de Aplicação (Casos de Uso)
│   ├── Sales.Infrastructure/   # Camada de Infraestrutura (Dados)
│   ├── Sales.Tests/            # Testes Unitários
│   └── Sales.sln               # Solução .NET
├── docker-compose.yml          # Orquestração dos containers SQL Server e RabbitMQ
```

## 🚀 Funcionalidades Implementadas

### Inventory Microservice
- **Gestão de Produtos**: CRUD completo para produtos
- **Controle de Estoque**: Reserva, liberação e adição de estoque
- **Validações de Negócio**: Regras para quantidade e preços
- **API REST**: Endpoints para todas as operações
- **Testes Unitários**: Cobertura completa dos métodos de negócio

### Endpoints da API Inventory
- `GET /api/v1/Product/all-products` - Listar todos os produtos
- `GET /api/v1/Product/product-by-id` - Buscar produto por ID
- `POST /api/v1/Product/create-product` - Criar novo produto
- `PUT /api/v1/Product/update-product` - Atualizar produto
- `DELETE /api/v1/Product/remove-product` - Remover produto

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 9.0** - Framework principal para desenvolvimento das APIs
- **ASP.NET Core** - Para criação das APIs REST
- **Entity Framework Core 9.0** - ORM para acesso a dados
- **SQL Server 2022** - Banco de dados relacional
- **RabbitMQ** - Message broker para comunicação entre microsserviços
- **Swagger/OpenAPI** - Documentação automática das APIs

### Infraestrutura
- **Docker & Docker Compose** - Containerização e orquestração
- **Clean Architecture** - Padrão arquitetural para separação de responsabilidades

### Testes
- **xUnit** - Framework de testes unitários
- **.NET Testing Framework** - Suporte nativo para testes

## 🚀 Como Executar o Projeto

### Pré-requisitos
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [JetBrains Rider](https://www.jetbrains.com/rider/) (recomendado)

### 1. Configuração do Ambiente
```bash
# Clone o repositório
git clone <url-do-repositorio>
cd evanade_ms

# Inicie os serviços de infraestrutura
docker-compose up -d
```

### 2. Executando as APIs

#### Opção A: Visual Studio/Rider
1. Abra a solução `Inventory/Inventory.sln` ou `Sales/Sales.sln`
2. Configure o projeto `*.Web` como projeto de inicialização
3. Execute o projeto (F5)

#### Opção B: CLI .NET
```bash
# Inventory API
cd Inventory/Inventory/Inventory.Web
dotnet run

# Sales API (em outro terminal)
cd Sales/Sales/Sales.Web
dotnet run
```

### 3. Acessando as APIs
- **Inventory API**: `https://localhost:5001` ou `http://localhost:5000`
- **Sales API**: `https://localhost:5003` ou `http://localhost:5002`
- **Swagger UI**: Adicione `/swagger` à URL da API

## 🔧 Configurações de Desenvolvimento

### Serviços de Infraestrutura
- **SQL Server**: `localhost:1433`
  - Usuário: `SA`
  - Senha: `Teste123!`
- **RabbitMQ Management**: `http://localhost:15672`
  - Usuário: `guest`
  - Senha: `guest`

### Arquivos de Configuração
- `appsettings.json` - Configurações de produção
- `appsettings.Development.json` - Configurações de desenvolvimento
- `docker-compose.yml` - Orquestração dos containers

## 🏛️ Clean Architecture

Este projeto implementa os princípios da **Clean Architecture**, garantindo:

### Camadas da Aplicação
- **Web (Apresentação)**: Controllers, DTOs e configurações da API
- **Application (Aplicação)**: Casos de uso, interfaces e regras de negócio
- **Infrastructure (Infraestrutura)**: Acesso a dados, repositórios e serviços externos
- **Tests**: Testes unitários e de integração

### Benefícios
- ✅ **Independência de Frameworks**: A lógica de negócio não depende de frameworks externos
- ✅ **Testabilidade**: Fácil criação de testes unitários
- ✅ **Independência de UI**: A interface pode mudar sem afetar o sistema
- ✅ **Independência de Banco de Dados**: Pode trocar de SQL Server para outro SGBD
- ✅ **Independência de Agentes Externos**: A lógica de negócio não conhece o mundo externo

### Princípios Aplicados
- **Dependency Inversion**: Dependências apontam para abstrações, não implementações
- **Separation of Concerns**: Cada camada tem uma responsabilidade específica
- **Single Responsibility**: Cada classe tem apenas uma razão para mudar

## 🧪 Testes Unitários

O projeto implementa uma estratégia robusta de testes unitários utilizando **xUnit** e **Microsoft.NET.Test.Sdk**.

### Cobertura de Testes Implementada

#### Product Entity Tests
- ✅ **Construtor**: Validação de criação com parâmetros válidos
- ✅ **Reserve()**: Testes para reserva de estoque
  - Quantidade válida (decrementa estoque)
  - Quantidade inválida (0, negativa) - lança `ArgumentException`
  - Quantidade maior que estoque - lança `InvalidOperationException`
  - Quantidade exata do estoque disponível
- ✅ **Release()**: Testes para liberação de estoque
  - Quantidade válida (incrementa estoque)
  - Quantidade inválida (0, negativa) - lança `ArgumentException`
- ✅ **AddStock()**: Testes para adição de estoque
  - Quantidade válida (incrementa estoque)
  - Quantidade inválida (0, negativa) - lança `ArgumentException`
- ✅ **SetPrice()**: Testes para definição de preço
  - Preço válido (atualiza preço)
  - Preço negativo - lança `ArgumentException`
  - Preço zero (caso limite)
- ✅ **Teste de Integração**: Múltiplas operações em sequência

### Executando os Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes de um projeto específico
dotnet test Inventory/Inventory/Inventory.Tests/
dotnet test Sales/Sales/Sales.Tests/

# Executar testes com cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Executar testes com output detalhado
dotnet test --verbosity normal
```

### Estrutura dos Testes
```
Inventory.Tests/
├── ApplicationTestes/
│   └── Entities/
│       └── ProductTest.cs    # Testes da entidade Product
└── Inventory.Tests.csproj    # Configuração do projeto de testes
```

### Tecnologias de Teste
- **xUnit 2.9.2** - Framework de testes
- **Microsoft.NET.Test.Sdk 17.12.0** - SDK de testes
- **coverlet.collector 6.0.2** - Coleta de cobertura de código
- **xunit.runner.visualstudio 2.8.2** - Runner para Visual Studio

## 🔧 Configurações de Desenvolvimento

### Banco de Dados
- **SQL Server 2022**: `localhost:1433`
- **Database**: `InventoryDb`
- **Usuário**: `SA`
- **Senha**: `Teste123!`
- **Connection String**: Configurada em `appsettings.json`

### Message Broker
- **RabbitMQ Management**: `http://localhost:15672`
- **Usuário**: `guest`
- **Senha**: `guest`
- **Porta AMQP**: `5672`

### Estrutura de Dados
```sql
-- Tabela Products
CREATE TABLE Products (
    ProductId UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL
);
```

## 🏛️ Padrões e Boas Práticas

### Clean Architecture Implementada
- **Separation of Concerns**: Cada camada tem responsabilidade específica
- **Dependency Inversion**: Dependências apontam para abstrações
- **Repository Pattern**: Abstração do acesso a dados
- **Gateway Pattern**: Interface entre camadas de aplicação e infraestrutura
- **Mapper Pattern**: Conversão entre entidades de domínio e persistência

### Validações de Negócio
- **Product.Reserve()**: Valida quantidade positiva e disponibilidade de estoque
- **Product.Release()**: Valida quantidade positiva para liberação
- **Product.AddStock()**: Valida quantidade positiva para adição
- **Product.SetPrice()**: Valida preço não negativo

### Tratamento de Exceções
- **DataAccessException**: Exceções customizadas para acesso a dados
- **ArgumentException**: Validações de parâmetros de entrada
- **InvalidOperationException**: Operações inválidas de negócio

## 🤝 Contribuindo
Sinta-se à vontade para abrir issues, sugerir melhorias ou enviar pull requests. Este projeto é um ponto de partida para soluções modernas e escaláveis!

---

> "Transformando ideias em soluções escaláveis, um microserviço de cada vez."

