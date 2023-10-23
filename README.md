# TechChallenge - HelpDesk API

A HelpDesk API é uma plataforma de gerenciamento de tickets que visa aprimorar a gestão operacional,
oferecendo maior controle e centralização das informações relacionadas aos tickets. Este sistema permite que
os usuários criem, gerenciem e monitorem o status de tickets, enquanto os administradores têm a capacidade de
coordenar e controlar todo o processo. Os analistas desempenham um papel crucial na resolução dos tickets e na
atualização do seu andamento.

## Colaboradores

- [Ailton Alves de Araujo](https://www.linkedin.com/in/ailton-araujo-b4ba0520/) - RM350781
- [Bruno Fecchio Salgado](https://www.linkedin.com/in/bfecchio/) - RM350780
- [Cecília Gonçalves Wlinger](https://www.linkedin.com/in/cec%C3%ADlia-wlinger-6a5459100/) - RM351312
- [Cesar Julio Spaziante](https://www.linkedin.com/in/cesar-spaziante/) - RM351311
- [Paulo Felipe do Nascimento de Sousa](https://www.linkedin.com/in/paulo-felipe06/) - RM351707

## Tecnologias utilizadas

- .NET 7.0
- Entity Framework Core 7.0
- Swashbuckle 6.5
- FluentValidation 11.7
- FluentAssertions 6.12
- NetArchTest 1.3
- SqlServer 2019
- Docker 24.0.5
- Docker Compose 2.20

## Arquitetura, Padrões Arquiteturais e Convenções

- REST Api
- Domain-Driven Design
- EF Code-first
- Service Pattern
- Repository Pattern & Unit Of Work
- Architecture Tests
- Unit Tests

## Definições técnicas

A solução do HelpDesk API é composta pelos seguintes projetos:

| Projeto                               | Descrição                                                                               |
|---------------------------------------|-----------------------------------------------------------------------------------------|
| _TechChallenge.Api_                   | Contém a implementação dos endpoints de comunicação da REST Api.                        |
| _TechChallenge.Application_           | Contém a implementação dos contratos de comunicação e classes de serviços.              |
| _TechChallenge.Domain_                | Contém a implementação das entidades e interfaces do domínio da aplicação.              |
| _TechChallenge.Infrastructure_        | Contém a implementação dos componentes relacionados a infraestrutura da aplicação.      |
| _TechChallenge.Persistence_           | Contém a implementação dos componentes relacionados a consulta e persistencia de dados. |
| _TechChallenge.Application.UnitTests_ | Contém a implementação dos testes unitários focados nas classes de serviço.             |
| _TechChallenge.ArchitectureTests_     | Contém a implementação dos testes de arquitetura.                                       |

## Modelagem de dados

A HelpDesk API utiliza o paradigma de CodeFirst através dos recursos disponibilizados pelo Entity Framework, no entanto para melhor
entendimento da modelagem de dados apresentamos a seguir o MER e suas respectivas definições:

![image](https://github.com/fiap-2nett/tc-fase1/assets/57924071/5471b1a1-d991-467f-be6e-785f6bb74211)

Com base na imagem acima iremos detalhar as tabelas e os dados contidos em cada uma delas:

| Schema | Tabela       | Descrição                                                                                       |
|--------|--------------|-------------------------------------------------------------------------------------------------|
| dbo    | users        | Tabela que contém os dados referentes aos usuários da plataforma.                               |
| dbo    | roles        | Tabela que contém os dados referentes aos tipos de perfis de usuário da plataforma.             |
| dbo    | tickets      | Tabela que contém os dados referentes aos tickets criados na plataforma.                        |
| dbo    | ticketstatus | Tabela que contém os dados referentes aos possíveis status de tickets.                          |
| dbo    | categories   | Tabela que contém os dados referentes as categorias de tickets.                                 |
| dbo    | priorities   | Tabela que contém os dados referentes as prioridades/SLAs relacionado as categorias de tickets. |

## Como executar

A HelpDesk API utiliza como banco de dados o SQL Server 2019 ou superior, toda a infraestrtura necessária para execução do projeto
pode ser provisionada automaticamente através do Docker.

No diretório raíz do projeto, existem os arquivos docker-compose.yml que contém toda a configuração necessária para provisionamento
dos serviços de infraestrutura, caso opte por executar o SQL Server através de container execute o seguinte comando na raíz do projeto:

```sh
$ docker compose up -d tc.db
```

O comando acima irá fazer o download da imagem do SQL Server 2019 e criará automaticamente um container local com o serviço em execução.
Este comando irá configurar o container de SQL Server, todo o processo de criação do banco de dados e carregamento de tabelas padrões será
realizado pelo Entity Framework no momento da execução do projeto.

Caso não queira utilizar o SQL Server através de container Docker, lembre-se de alterar a ConnectionString no arquivo appsettings.json existente
no projeto TechChallenge.Api.

## Como executar os testes

A HelpDesk API disponibiliza testes automatizados para garantir que o processo contempla as regras de negócio pré-definidas no requisito
do projeto. A execução dos testes pode ser feita através de Visual Studio ou via dotnet CLI.

### Obter JWT Bearer Tokens

Para consumir os endpoints é necessário obter o token bearer, por padrão o projeto irá criar alguns usuários fictícios com diferentes perfis
de acesso, são eles:

| Usuário                   | Senha       | Perfil        |
|---------------------------|-------------|---------------|
| admin@techchallenge.app   | Admin@123   | Administrador |
| ailton@techchallenge.app  | Ailton@123  | Geral         |
| bruno@techchallenge.app   | Bruno@123   | Analista      |
| cecilia@techchallenge.app | Cecilia@123 | Geral         |
| cesar@techchallenge.app   | Cesar@123   | Analista      | 
| paulo@techchallenge.app   | Paulo@123   | Geral         |

Caso queira você poderá criar o seu próprio usuário através do endpoint:

```curl
POST /authentication/register
```

*Observação: para novos usuários será atribuído o perfil Geral.*

### Testes unitários, integração e arquiteturais

Para executar os testes através do dotnet CLI, no diretório raíz do projeto execute o seguinte comando:

```sh
$ dotnet test TechChallenge.sln
```

Caso queria uma versão de resultado com mais detalhes, execute o seguinte comando:

```sh
$ dotnet test --logger "console;verbosity=detailed" TechChallenge.sln
```

*Observação: para execução dos testes de integração é necessário ter uma instância do SQL Server em execução.*
