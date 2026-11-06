# Simplified Payments

API de pagamentos simplificada, desenvolvida como solução do [desafio técnico de backend do PicPay](https://github.com/PicPay/picpay-desafio-backend). Ela permite cadastrar usuários (comuns e lojistas), consultar, atualizar e remover cadastros, e transferir dinheiro entre eles, consultando um serviço autorizador externo e notificando quem recebe.

## Sumário

- [Regras de negócio](#regras-de-negócio)
- [Stack](#stack)
- [Arquitetura](#arquitetura)
- [Fluxo da transferência](#fluxo-da-transferência)
- [Decisões técnicas](#decisões-técnicas)
- [Como executar](#como-executar)
- [Configuração](#configuração)
- [API](#api)
- [Testes](#testes)
- [Integração contínua](#integração-contínua)
- [Limitações e próximos passos](#limitações-e-próximos-passos)
- [Licença](#licença)

## Regras de negócio

- Existem dois tipos de usuário: **comum** (identificado por CPF) e **lojista** (identificado por CNPJ).
- Todo usuário tem nome completo, documento, e-mail e senha. **Documento e e-mail são únicos** no sistema.
- Cada usuário tem uma carteira, criada junto com o cadastro, com saldo inicial zero.
- Usuários comuns enviam e recebem dinheiro. **Lojistas só recebem.**
- O pagador precisa ter saldo suficiente, e não é possível transferir para si mesmo.
- Antes de finalizar, a transferência é **consultada em um serviço autorizador externo**.
- A operação é **transacional**: se algo falhar, o dinheiro permanece na carteira do pagador.
- Quem recebe é **notificado** por um serviço externo. Esse serviço é instável, e a falha dele **não desfaz** a transferência.

## Stack

| Área | Tecnologia |
|---|---|
| Plataforma | .NET 10, ASP.NET Core (controllers) |
| Banco de dados | SQL Server 2022 |
| Acesso a dados | Entity Framework Core 10 |
| Migrations | [Evolve](https://github.com/lecaillon/Evolve), com scripts SQL versionados |
| Resiliência HTTP | `Microsoft.Extensions.Http.Resilience` (timeout e retry) |
| Senhas | BCrypt.Net-Next |
| Logs | Serilog |
| Documentação da API | Swashbuckle (geração do OpenAPI) e [Scalar](https://scalar.com/) (interface interativa) |
| Testes | xUnit, FluentAssertions, Testcontainers |
| Infra local e CI | Docker Compose, GitHub Actions |

## Arquitetura

O projeto segue a **Clean Architecture**, organizada em pastas dentro de um único projeto de API:

```
SimplifiedPayments.slnx
├── SimplifiedPayments/                 # API
│   ├── Domain/                         # Entidades, value objects, enums e regras de negócio
│   ├── Application/                    # Casos de uso e contratos (interfaces)
│   ├── Infrastructure/
│   │   ├── Persistence/                # DbContext, mapeamentos, repositórios, Evolve e migrations
│   │   └── ExternalServices/           # Clientes HTTP do autorizador e do notificador
│   ├── Security/                       # Implementação do hash de senhas (BCrypt)
│   ├── Controllers/                    # Endpoints e contratos HTTP
│   ├── Middlewares/                    # Tratamento global de exceções (ProblemDetails)
│   ├── Configurations/                 # CORS, logging, OpenAPI e Scalar
│   ├── docker-compose.yaml             # SQL Server para desenvolvimento
│   └── .env.example
└── SimplifiedPayments.Tests/           # Testes unitários e de integração
```

Regra de dependência: `Controllers → Application → Domain`. A `Infrastructure` implementa as interfaces definidas na `Application`, e o `Domain` não depende de nada. Como tudo está em um único projeto, essa regra é mantida por convenção, e não pelo compilador.

## Fluxo da transferência

`POST /api/transfers/v1` executa, nesta ordem:

1. Carrega pagador e recebedor, cada um com sua carteira (`404` se algum não existir).
2. Valida as regras no Domain, **sem alterar nada**: valor, mesmo usuário, lojista como pagador e saldo.
3. Consulta o **autorizador** externo. Como nada foi gravado até aqui, uma negativa ou uma falha não exige desfazer operação alguma.
4. Debita o pagador, credita o recebedor e registra a transferência **no mesmo `SaveChanges`**, ou seja, em uma única transação.
5. Depois do commit, envia a **notificação** ao recebedor. Se falhar, apenas registra um aviso no log.

## Decisões técnicas

- **Autorizador fora da transação.** Manter uma transação aberta esperando um serviço externo seguraria locks. A chamada acontece antes de qualquer escrita.
- **Falha fechada.** Se o autorizador estiver indisponível, a transferência é recusada (`503`). Só um `authorization: true` explícito autoriza.
- **Concorrência otimista na carteira.** A tabela de carteiras tem uma coluna `rowversion`, usada como token de concorrência. Se duas transferências disputarem o mesmo saldo, a segunda gravação falha com `409` em vez de sobrescrever a primeira.
- **Migrations em SQL puro com Evolve.** O schema é versionado em `Infrastructure/Persistence/Migrations`. O EF Core é usado apenas para mapeamento e consultas, sem `Migrate()` nem `EnsureCreated()`.
- **Constraints no banco.** Documento e e-mail únicos, saldo não negativo, valor da transferência positivo e pagador diferente do recebedor também são garantidos pelo banco, como segunda barreira além do Domain.
- **Resiliência nos serviços externos.** O autorizador tem timeout de 3 s por tentativa e até 2 retries (12 s no total). O notificador tem 2 s por tentativa e 1 retry (6 s no total).
- **Notificação *best effort*.** Roda depois do commit e com `CancellationToken.None`, para que um cliente que desconecta não cancele o aviso de uma transferência já concluída.
- **Erros no padrão `ProblemDetails`.** Um handler global traduz exceções de domínio e de aplicação para os status HTTP corretos e nunca expõe detalhes de erros inesperados.
- **Senhas com BCrypt.** O Domain recebe apenas o hash. A API não devolve senha nem hash em nenhuma resposta.
- **Documentos normalizados.** CPF e CNPJ são validados (inclusive os dígitos verificadores) e gravados somente com dígitos. E-mails são gravados em minúsculas.

## Como executar

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (para o SQL Server local e para os testes de integração)

### Passo a passo

A partir da raiz do repositório:

```bash
# 1. Variáveis do container do SQL Server
cd SimplifiedPayments
cp .env.example .env
```

Edite o `.env`, definindo `ACCEPT_EULA=Y` e uma senha forte em `DB_SA_PASSWORD` (mínimo de 8 caracteres, com maiúscula, minúscula, número e símbolo). Evite `$` na senha.

```bash
# 2. Sobe o SQL Server e espera ficar "healthy"
docker compose up -d
docker compose ps

# 3. Connection string via User Secrets (a senha é a mesma do .env)
dotnet user-secrets set "ConnectionStrings:SqlServer" "Data Source=localhost,1433;Initial Catalog=simplified_payments;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;"

# 4. Executa a API
dotnet run --launch-profile http
```

A API sobe em `http://localhost:5097`, e a documentação interativa fica em `http://localhost:5097/scalar`. Em `Development`, na inicialização ela **cria o banco** `simplified_payments` (se não existir) e **aplica as migrations** e o seed.

Para recomeçar do zero, apague o volume do banco: `docker compose down -v`.

### Usuários de seed

A migration `V3` cria usuários para testes manuais. **Todos usam a senha `Dev@12345`.** Em um banco novo, os ids são os da tabela abaixo; em caso de dúvida, consulte `select id, email, user_type from dbo.tb_users`.

| Id | Nome | E-mail | Tipo | Saldo inicial |
|---|---|---|---|---|
| 1 | Maria Souza | maria@example.com | Comum | 1000,00 |
| 2 | João Pereira | joao@example.com | Comum | 1000,00 |
| 3 | Ana Lima | ana@example.com | Comum | 1000,00 |
| 4 | Padaria Pão Quente | padaria@example.com | Lojista | 0,00 |
| 5 | Mercado Bom Preço | mercado@example.com | Lojista | 0,00 |

## Configuração

| Chave | Descrição | Onde definir |
|---|---|---|
| `ConnectionStrings:SqlServer` | Connection string do SQL Server | User Secrets (desenvolvimento) ou variável de ambiente |
| `Cors:AllowedOrigins:0`, `:1`, ... | Origens permitidas pelo CORS | User Secrets, `appsettings` ou variável de ambiente |
| `ExternalServices:AuthorizerUrl` | URL do autorizador (padrão: mock do desafio) | `appsettings.json` |
| `ExternalServices:NotifierUrl` | URL do notificador (padrão: mock do desafio) | `appsettings.json` |

Fora de `Development` (Docker, produção), os User Secrets não são carregados. Use variáveis de ambiente, trocando `:` por `__`:

```bash
ConnectionStrings__SqlServer="Data Source=...;"
Cors__AllowedOrigins__0="https://meu-front.exemplo.com"
ExternalServices__AuthorizerUrl="https://..."
```

Observações:

- As migrations rodam **apenas em `Development`**. Em outros ambientes, o schema deve ser aplicado por um processo de deploy.
- Além do CORS padrão, a API **responde `403` a qualquer requisição que envie um header `Origin` fora da lista e diferente da própria origem da API** (por isso o Scalar, servido pela própria API, continua funcionando). Clientes que não enviam `Origin` (curl, Postman, outros serviços) não são afetados, então isso não substitui autenticação.

## API

Todos os endpoints usam JSON. Os erros seguem o formato `ProblemDetails` (RFC 9457).

| Método | Rota | Descrição | Sucesso |
|---|---|---|---|
| `POST` | `/api/users/v1` | Cria um usuário (e sua carteira) | `201` |
| `GET` | `/api/users/v1/{id}` | Consulta um usuário | `200` |
| `PUT` | `/api/users/v1/{id}` | Atualiza nome e e-mail | `200` |
| `DELETE` | `/api/users/v1/{id}` | Remove um usuário sem histórico de transferências | `204` |
| `POST` | `/api/transfers/v1` | Realiza uma transferência | `201` |

O arquivo [`SimplifiedPayments.http`](SimplifiedPayments/SimplifiedPayments.http) traz uma requisição pronta para cada caso.

### Documentação interativa

Com a API rodando em `Development`, a documentação pode ser explorada e testada pelo navegador:

- **Scalar:** `http://localhost:5097/scalar`
- **Documento OpenAPI (JSON):** `http://localhost:5097/swagger/v1/swagger.json`

### Criar usuário

```bash
curl -X POST http://localhost:5097/api/users/v1 \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Carlos Almeida",
    "document": "529.982.247-25",
    "email": "carlos@example.com",
    "password": "Senha@123",
    "type": "Common"
  }'
```

- `type` aceita apenas `"Common"` ou `"Shopkeeper"` (números são rejeitados).
- `document` aceita com ou sem formatação. Usuário comum exige CPF, e lojista exige CNPJ.
- `password` deve ter de 8 caracteres a 72 bytes.

```json
{
  "id": 6,
  "fullName": "Carlos Almeida",
  "document": "52998224725",
  "email": "carlos@example.com",
  "type": "Common"
}
```

### Transferir

```bash
curl -X POST http://localhost:5097/api/transfers/v1 \
  -H "Content-Type: application/json" \
  -d '{ "value": 100.0, "payer": 1, "payee": 4 }'
```

- `value`: de 0,01 a 1.000.000.000, com no máximo 2 casas decimais.
- `payer` e `payee`: ids de usuários existentes.

```json
{
  "id": 1,
  "payer": 1,
  "payee": 4,
  "value": 100.0,
  "createdAt": "2026-11-04T14:08:05.123Z"
}
```

### Códigos de erro

| Status | Quando acontece |
|---|---|
| `400` | Corpo malformado, campo obrigatório ausente, `type` inválido ou valor fora da faixa |
| `403` | O autorizador negou a transferência, ou a `Origin` da requisição não é permitida |
| `404` | Usuário, pagador ou recebedor inexistente |
| `409` | E-mail ou documento já cadastrado, disputa de saldo (tente novamente) ou usuário com histórico de transferências |
| `422` | Documento inválido, senha fraca, e-mail inválido, lojista como pagador, saldo insuficiente, transferência para si mesmo ou valor com mais de 2 casas |
| `503` | O serviço autorizador está indisponível |
| `500` | Erro inesperado (o detalhe fica apenas no log) |

Exemplo de resposta de erro:

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "Email already registered!"
}
```

## Testes

```bash
dotnet test                                     # todos (exige Docker)
dotnet test --filter "Category!=Integration"    # apenas unitários, sem Docker
dotnet test --filter "Category=Integration"     # apenas integração
```

**Testes unitários** cobrem as regras do Domain (CPF, CNPJ, carteira, usuário e transferência), os casos de uso (com fakes escritos à mão) e os clientes HTTP externos (com um `HttpMessageHandler` falso).

**Testes de integração** usam o [Testcontainers](https://testcontainers.com/) para subir um **SQL Server descartável**, separado do container de desenvolvimento (porta e senha aleatórias, removido ao final). A API real é iniciada em memória com `WebApplicationFactory`, e o Evolve aplica as migrations de verdade. Eles validam:

- os endpoints de usuário, com os status HTTP esperados e a persistência no banco;
- o comportamento do CORS;
- o mapeamento do EF Core, o seed e as constraints do banco;
- a concorrência (várias requisições simultâneas com o mesmo e-mail resultam em exatamente um cadastro);
- a geração do documento OpenAPI (`/swagger/v1/swagger.json`).

Os testes de integração não chamam os serviços externos reais, então não dependem de internet.

## Integração contínua

O workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml) roda a cada push e a cada pull request para a branch `master`:

1. restaura os pacotes e compila a solução em `Release`;
2. executa os testes unitários (falha rápida, sem Docker);
3. baixa a imagem do SQL Server e executa os testes de integração com o Testcontainers;
4. publica os resultados (`.trx`) como artefato da execução.

Execuções antigas da mesma branch são canceladas automaticamente quando chega um commit novo.

## Limitações e próximos passos

- **Disputa na carteira de um lojista muito acessado.** A concorrência otimista evita perda de saldo, mas duas transferências simultâneas para o mesmo recebedor disputam a mesma linha, e uma recebe `409`. As saídas são um retry no servidor ou um `UPDATE ... SET balance = balance + @valor` atômico.
- **A notificação roda dentro da requisição**, com latência limitada a cerca de 6 s e sem reenvio posterior. O próximo passo é uma fila em segundo plano (`Channel<T>` com `BackgroundService`) ou um *outbox* gravado na mesma transação.
- **O `POST` de transferência não é idempotente.** Se o cliente repetir a chamada após um timeout, a transferência pode ser duplicada. Um header `Idempotency-Key` resolveria.
- **Não há autenticação.** As senhas são guardadas com hash, mas não existe endpoint de login nem autorização por usuário.
- **Não há consulta de extrato** nem listagem de transferências.
- **Projeto único.** Separar `Domain`, `Application`, `Infrastructure` e `Api` em projetos faria o compilador impor a regra de dependência.
- **Sem Dockerfile da API.** O `docker-compose.yaml` sobe apenas o banco.

## Licença

Distribuído sob a licença MIT. Veja o arquivo [`LICENSE`](LICENSE).