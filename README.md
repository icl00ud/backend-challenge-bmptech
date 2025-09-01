# ChuBank API

API bancária em .NET 6 com PostgreSQL e Redis. Permite criar contas, realizar transferências e gerar extratos.

## Funcionalidades

- **Contas**: Criar e consultar contas bancárias (sem cache para garantir dados atualizados)
- **Transferências**: Transferir valores entre contas (apenas em dias úteis, sem cache para integridade transacional)
- **Extratos**: Gerar extratos por período com cache Redis (dados históricos imutáveis)

## Como executar

```bash
docker-compose up -d
```

## Testando a API

- **Swagger**: http://localhost:8080/swagger
- **Collection Insomnia**: Importe o arquivo `ChuBank-API-Collection.json`

### Autenticação

Use `/api/v1/auth/token` com username/password para obter o JWT token.I

## 🚀 Como executar

```bash
docker-compose up -d
```

## � Testando a API

- **Swagger**: http://localhost:8080/swagger, não esqueça de colocar 'Bearer' antes do token
- **Collection Insomnia**: Importe o arquivo `ChuBank-API-Collection.json` no Insomnia