# Evanade Challenge - Microservices Inventory & Sales

Bem-vindo ao projeto **Evanade Challenge**! Este repositório reúne dois microsserviços principais para gestão de inventário e vendas, prontos para rodar em ambiente Docker.

## Estrutura do Projeto

```
├── Inventory/         # Microsserviço de Inventário
│   ├── Inventory.API/ # API para operações de estoque
│   └── Inventory.sln  # Solução .NET
├── Sales/             # Microsserviço de Vendas
│   ├── Sales.API/     # API para operações de vendas
│   └── Sales.sln      # Solução .NET
├── docker-compose.yml # Orquestração dos containers SQL Server e RabbitMQ
```

## Tecnologias Utilizadas
- **.NET** para APIs REST
- **SQL Server** para persistência de dados
- **RabbitMQ** para mensageria entre microsserviços
- **Docker Compose** para facilitar o setup do ambiente

## Como iniciar o ambiente
1. Certifique-se de ter o Docker instalado.
2. Na raiz do projeto, execute:
   ```sh
   docker-compose up -d
   ```
3. As APIs podem ser iniciadas via Visual Studio, Rider ou CLI .NET.

## Dicas para Desenvolvedores
- Os arquivos `appsettings.json` e `appsettings.Development.json` contêm as configurações de conexão.
- O RabbitMQ está disponível em `localhost:15672` (usuário/padrão: guest/guest).
- O SQL Server está disponível em `localhost:1433` (usuário: SA, senha: teste123).

## Colabore!
Sinta-se à vontade para abrir issues, sugerir melhorias ou enviar pull requests. Este projeto é um ponto de partida para soluções modernas e escaláveis!

---

> "Transformando ideias em soluções escaláveis, um microserviço de cada vez."

