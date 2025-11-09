[![codecov](https://codecov.io/github/leandrobattochio/SoftCep/graph/badge.svg?token=5GEA6H1B0B)](https://codecov.io/github/leandrobattochio/SoftCep)
![build pipeline](https://github.com/leandrobattochio/SoftCep/actions/workflows/main.yml/badge.svg)

# SoftCep API

API REST intermediária (camada anti-corrupção) para consultas de CEP utilizando o provedor público ViaCep, oferecendo
resiliência, padronização de contrato, caching, limitação de requisições.

## Sumário

- [Contexto do Problema](#contexto-do-problema)
- [Objetivos do MVP](#objetivos-do-mvp)
- [Escopo Entregue](#escopo-entregue)
- [Arquitetura & Camadas](#arquitetura--camadas)
- [Principais Decisões Técnicas](#principais-decisões-técnicas)
- [Fluxo de Requisição](#fluxo-de-requisição)
- [Endpoints](#endpoints)
- [Versionamento](#versionamento)
- [Contratos de Resposta](#contratos-de-resposta)
- [Resiliência & Confiabilidade](#resiliência--confiabilidade)
- [Caching](#caching)
- [Rate Limiting](#rate-limiting)
- [Validações](#validações)
- [Observabilidade & Logging](#observabilidade--logging)
- [Testes](#testes)
- [Scalar](#scalar)
- [Como Executar](#como-executar)
- [Configurações](#configurações)

## Contexto do Problema

O sistema cliente (ex: SoftFront) consome diretamente a API pública ViaCep. O provedor tem apresentado instabilidade,
degradando a experiência dos usuários e aumentando a complexidade de tratamento de falhas nos consumidores.

## Objetivos do MVP

1. Uniformizar contrato para consulta de CEP.
2. Adicionar resiliência (retry com backoff exponencial).
3. Reduzir carga e dependência direta do ViaCep via caching.
4. Proteger contra abuso (rate limiting por IP).
5. Facilitar observabilidade e extensões futuras.

## Escopo Entregue

- .NET 9
- Endpoints: consulta por CEP e por endereço.
- Integração com ViaCep usando Refit.
- Retry com Polly (exponencial) até 5 tentativas.
- Cache em memória (HybridCache) pronto para expansão distribuída.
- Rate Limiting por IP (1 req/seg, demonstrativo de controle).
- Validação de CEP (8 dígitos obrigatórios).
- Documentação OpenAPI + UI (Scalar) em Development.
- Testes unitários e de integração (incluindo rate limiting).
- Logging estruturado com Serilog.

## Arquitetura & Camadas

```text
SoftCep.Api
 ├─ Controllers        -> Exposição HTTP
 ├─ Application        -> Handlers (casos de uso)
 ├─ Domain             -> Modelos de domínio / mapeamentos (Mapperly)
 ├─ Infrastructure     -> Clientes externos (ViaCep) e DTOs externos
 ├─ Core               -> Cross-cutting (Consts, validações, logging, rate limiting)
 └─ Program.cs         -> Composition Root (DI + pipeline)
```

Estilo orientado a camadas / ports & adapters leve. Domínio simples, preparado para futura evolução (ex: normalização,
enriquecimento, persistência, auditoria).

## Principais Decisões Técnicas

- Plataforma: .NET 9;
- HTTP Client: Refit (declaração concisa dos endpoints externos, reduz boilerplate).
- Resiliência: Polly (WaitAndRetry exponencial para falhas transitórias).
- Mapeamento: Mapperly (geração compile-time, performance e menos reflection).
- Caching: HybridCache (API moderna preparada para Redis / distribuído).
- Rate Limiting: Fixed Window por IP (simplicidade no MVP, fácil troca para Sliding Window).
- Observabilidade: Serilog (estrutura, enriquecimento, base para OpenTelemetry).
- Documentação: OpenAPI + Scalar UI (dev-friendly).
- Testes: xUnit + Shouldly + WebApplicationFactory (confiança em handlers e pipeline HTTP).

## Fluxo de Requisição

1. Cliente chama `/api/v1/cep/{cep}`.
2. Middleware Rate Limiting avalia IP.
3. Validação de CEP (atributo).
4. Handler tenta obter do cache.
5. Cache miss -> ViaCep (Refit + HttpClient + Polly Retry).
6. Mapeamento DTO externo -> modelo domínio.
7. Headers de cache-control adicionados.
8. Retorno 200 ou 204.
   Fluxo por endereço similar retornando lista.

## Endpoints

### GET /api/cep/{cep}

Consulta um CEP específico.

Respostas:

- 200 (CepResult)
- 204 (não encontrado)
- 400 (CEP inválido)
- 429 (rate limit)
- 5xx (falhas)

### GET /api/v1/cep/{uf}/{cidade}/{termo}

Busca lista de CEPs por UF + cidade + termo parcial.

Respostas:

- 200 (lista)
- 204 (vazio)
- 429 (rate limit)
- 5xx (falhas)

OpenAPI em Development:

- `/openapi/v1.json`
- `/openapi/v2.json`
- `/scalar`

## Versionamento

Versão padrão: `v1`.

O MVP possui apenas `v1`, mas a estrutura suporta futuras versões via URL segmentada.
Como exemplo, foi adicionado para demonstração uma rota v2 que retorna um texto simples.

## Contratos de Resposta

```json
{
  "cep": "01001-000",
  "logradouro": "Praça da Sé",
  "complemento": "lado ímpar",
  "bairro": "Sé",
  "localidade": "São Paulo",
  "uf": "SP",
  "regiao": "Sudeste",
  "ddd": "11"
}
```

Lista: array deste modelo.

## Resiliência & Confiabilidade

- Retry exponencial (2^n) até 5 tentativas (`Consts.HttpRetryCount`).
- Timeout HTTP de 10s.

## Caching

- Duração: 24h (`Consts.CepCacheTime`).
- Chaves: `cache/cep/{cep}` e `cache/cep/address/{uf}/{cidade}/{termo}`.

## Rate Limiting

- Política `PerIp20Rps` (20 requisição por segundo por IP) via Fixed Window.
- Teste de integração cobre saturação inter-endpoints.

## Validações

- Atributo `CepValidationAttribute` (8 dígitos numéricos).

## Observabilidade & Logging

- Serilog Console sink.
- Enriquecimento com `ApplicationName`.
- Logs para cache miss e startup.

## Testes

- Unitários (`SoftCep.Tests`): handlers, mapeamento, validações.
- Integração (`SoftCep.Integration.Tests`): endpoints, rate limiting.
- Cobertura: coverlet (`dotnet test /p:CollectCoverage=true`).

## Scalar

Todos os endpoints estão completamente documentados na UI do Scalar, com status de resposta, payload de entrada e saída,
bem como o enum `state` pode ser facilmente visualizado quais são as opções válidas.

![endpoint-1](./docs/doc1.png)
![endpoint-2](./docs/doc2.png)

## Como Executar

Pré-requisito: .NET 9 SDK.

```bash
dotnet restore
dotnet build -c Release
dotnet test --no-build
```

Executar API:

```bash
cd src/SoftCep.Api
dotnet run
```

Documentação:

- http://localhost:5266/scalar
- http://localhost:5266/openapi/v1.json

Exemplo:

```bash
curl http://localhost:5266/api/cep/01001000
```

## Configurações

`appsettings.json` esperado com a URL do ViaCep:

```json
{
  "Infrastructure": {
    "ViaCep": {
      "BaseUrl": "https://viacep.com.br/ws"
    }
  }
}
```