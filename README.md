# ChuBank API - Challenge BMP Tech

![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)
![Redis](https://img.shields.io/badge/Redis-7-red)
![Docker](https://img.shields.io/badge/Docker-Supported-blue)

API bancária desenvolvida em ASP.NET Core 6 seguindo os princípios de Clean Architecture, implementando funcionalidades de gestão de contas, transferências e extratos.

## 🚀 Funcionalidades

### 📋 Endpoints Principais

#### AccountsController (`/api/v1/accounts`)
- **POST** `/api/v1/accounts` - Cadastrar nova conta
- **GET** `/api/v1/accounts/{id}` - Obter dados de uma conta

#### TransfersController (`/api/v1/transfers`)
- **POST** `/api/v1/transfers` - Realizar transferência entre contas
  - ✅ Validação de dias úteis via [BrasilAPI](https://brasilapi.com.br/api/feriados/v1/2025)
  - ✅ Validação de saldo suficiente
  - ✅ Atualização automática dos saldos

#### StatementsController (`/api/v1/statements`)
- **GET** `/api/v1/statements?accountId=&startDate=&endDate=` - Gerar extrato por período
  - ✅ Cache Redis para otimização
  - ✅ Histórico detalhado de transações

## 🏗️ Arquitetura

### Estrutura de Camadas

```
├── ChuBank.Api/           # Camada de apresentação (Controllers, Middleware)
├── ChuBank.Application/   # Camada de aplicação (Services, DTOs, Validators)
├── ChuBank.Domain/        # Camada de domínio (Entities, Interfaces)
├── ChuBank.Infrastructure/ # Camada de infraestrutura (Repositories, External Services)
└── ChuBank.Tests/         # Testes unitários
```

### 🛠️ Stack Tecnológica

- **Framework**: ASP.NET Core 6
- **ORM**: Entity Framework Core com PostgreSQL
- **Cache**: Redis (StackExchange.Redis)
- **Validação**: FluentValidation
- **Autenticação**: JWT Bearer
- **Versionamento**: API Versioning
- **Documentação**: Swagger/OpenAPI
- **Testes**: xUnit, Moq, FluentAssertions
- **Containerização**: Docker & Docker Compose

## 🔧 Configuração e Execução

### Pré-requisitos

- .NET 6 SDK
- Docker e Docker Compose
- Git

### 🐳 Execução com Docker (Recomendado)

1. **Clone o repositório**
   ```bash
   git clone https://github.com/icl00ud/backend-challenge-bmptech.git
   cd backend-challenge-bmptech
   ```

2. **Execute com Docker Compose**
   ```bash
   docker-compose up -d
   ```

3. **Acesse a aplicação**
   - API: http://localhost:8080
   - Swagger: http://localhost:8080/swagger
   - PostgreSQL: localhost:5432
   - Redis: localhost:6379

### 🔨 Execução Local

1. **Configure as dependências**
   ```bash
   # Inicie PostgreSQL e Redis localmente
   # PostgreSQL: localhost:5432
   # Redis: localhost:6379
   ```

2. **Configure a string de conexão**
   ```json
   // appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=chubank_dev;Username=chubank_user;Password=chubank_pass",
       "Redis": "localhost:6379"
     }
   }
   ```

3. **Execute a aplicação**
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project ChuBank.Api
   ```

## 🔐 Autenticação

A API utiliza JWT Bearer Token. Para obter um token:

```bash
POST /api/v1/auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "password"
}
```

Resposta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Use o token nos headers das requisições:
```
Authorization: Bearer {token}
```

## 📝 Exemplos de Uso

### 1. Criar Conta

```bash
POST /api/v1/accounts
Authorization: Bearer {token}
Content-Type: application/json

{
  "holderName": "João Silva",
  "initialBalance": 1000.00
}
```

### 2. Realizar Transferência

```bash
POST /api/v1/transfers
Authorization: Bearer {token}
Content-Type: application/json

{
  "fromAccountNumber": "123456",
  "toAccountNumber": "654321",
  "amount": 150.00,
  "description": "Pagamento serviços"
}
```

### 3. Gerar Extrato

```bash
GET /api/v1/statements?accountId=550e8400-e29b-41d4-a716-446655440000&startDate=2025-01-01&endDate=2025-01-31
Authorization: Bearer {token}
```

## 🧪 Testes

Execute os testes unitários:

```bash
dotnet test
```

### Cobertura de Testes

- ✅ AccountService
- ✅ TransferService  
- ✅ Validators
- ✅ Repository patterns

## 🗄️ Banco de Dados

### Schema Principal

- **Accounts**: Gestão de contas bancárias
- **Transfers**: Registro de transferências
- **Statements**: Extratos gerados
- **StatementEntries**: Entradas detalhadas dos extratos

### Migrations

```bash
# Gerar migration
dotnet ef migrations add InitialCreate --project ChuBank.Infrastructure --startup-project ChuBank.Api

# Aplicar migrations
dotnet ef database update --project ChuBank.Infrastructure --startup-project ChuBank.Api
```

## 📊 Cache Redis

O sistema utiliza Redis para cache de:

- ✅ Feriados nacionais (BrasilAPI)
- ✅ Extratos bancários (1 hora)
- ✅ Consultas frequentes

## 🔄 API Versioning

A API suporta versionamento através de:

- URL Segment: `/api/v1/accounts`
- Query String: `/api/accounts?version=1.0`
- Header: `X-Version: 1.0`

## 🌐 Integração Externa

### BrasilAPI - Validação de Feriados

A API integra com a [BrasilAPI](https://brasilapi.com.br/api/feriados/v1/2025) para validar dias úteis antes de processar transferências.

```csharp
// Exemplo de uso
var isBusinessDay = await _holidayService.IsBusinessDayAsync(DateTime.Today);
```

## 📋 Variáveis de Ambiente

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `ConnectionStrings__DefaultConnection` | String de conexão PostgreSQL | `Host=localhost;Database=chubank;Username=chubank_user;Password=chubank_pass` |
| `ConnectionStrings__Redis` | String de conexão Redis | `localhost:6379` |
| `Jwt__Key` | Chave secreta JWT | `YourSuperSecretKeyForJWTToken...` |
| `Jwt__Issuer` | Emissor do token | `ChuBank.Api` |
| `Jwt__Audience` | Audiência do token | `ChuBank.Api` |

## 🚀 Deploy

### Docker Compose (Produção)

```yaml
version: '3.8'
services:
  api:
    image: chubank-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=chubank;Username=chubank_user;Password=chubank_pass
    ports:
      - "80:80"
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👨‍💻 Autor

**Seu Nome**
- GitHub: [@icl00ud](https://github.com/icl00ud)
- LinkedIn: [Seu LinkedIn](https://linkedin.com/in/seu-linkedin)

---

⭐ Se este projeto te ajudou, considere dar uma estrela!

## 📞 Suporte

Para dúvidas ou suporte, abra uma [issue](https://github.com/icl00ud/backend-challenge-bmptech/issues) no GitHub.
