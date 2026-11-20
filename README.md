# Ritmo Academia — ASP.NET Core MVC

**Integrantes:** Francisco e Gabrieli  
**Tecnologias:** C#, ASP.NET Core MVC (.NET 10), Entity Framework Core, SQL Server, ASP.NET Core Identity e Bootstrap 5.

## O que o sistema contém

- Cadastro, login e logout com ASP.NET Core Identity.
- Perfis de acesso `Aluno` e `Admin`.
- Cadastro e gerenciamento de alunos, modalidades e matrículas (CRUD).
- Catálogo público de modalidades com busca e imagens.
- Perfil do aluno vinculado à conta autenticada.
- Histórico pessoal de matrículas: o aluno consulta somente seus próprios registros.
- Área administrativa protegida por `[Authorize(Roles = "Admin")]`.
- Validações de formulário e mensagens de resultado.
- Relacionamentos entre Aluno, Modalidade e Matrícula no `ApplicationDbContext`.

## Regras de negócio implementadas

1. **Matrícula sem duplicidade:** um aluno não pode manter duas matrículas ativas na mesma modalidade.
2. **Limite de vagas:** o sistema conta as matrículas ativas e bloqueia novas solicitações quando o limite é atingido.
3. **Modalidade ativa:** somente modalidades marcadas como ativas aceitam novas matrículas.
4. **Valor preservado:** o valor mensal da matrícula é copiado do preço da modalidade no servidor, sem confiar em valor enviado pelo navegador.
5. **Histórico protegido:** cada aluno visualiza apenas as matrículas associadas à sua própria conta.
6. **Exclusão protegida:** aluno/modalidade com matrículas relacionadas não pode ser excluído.

## Requisitos

- Visual Studio 2022 atualizado com a carga de trabalho **ASP.NET e desenvolvimento Web**.
- SDK do .NET 10.
- SQL Server LocalDB (instalado normalmente com o Visual Studio) ou SQL Server/Express.

## Abrir no Visual Studio

1. Extraia o ZIP.
2. Abra `AcademiaMVC.sln`.
3. Aguarde a restauração dos pacotes NuGet.
4. Confira a conexão em `AcademiaMVC/appsettings.json`.
5. Abra **Ferramentas > Gerenciador de Pacotes do NuGet > Console do Gerenciador de Pacotes**.
6. Selecione `AcademiaMVC` como projeto padrão e execute:

```powershell
Add-Migration Inicial
Update-Database
```

Também é possível usar o terminal na pasta do `.csproj`:

```bash
dotnet tool install --global dotnet-ef --version 10.0.8
dotnet ef migrations add Inicial
dotnet ef database update
dotnet run
```

As migrations são geradas no primeiro comando e ficam na pasta `Data/Migrations`. O projeto usa Code First, conforme os slides: as classes definem a estrutura e o EF Core cria/atualiza o banco por migrations.

## SQL Server

A configuração inicial usa LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=AcademiaMVC;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Se usar outra instância, altere `ConnectionStrings:DefaultConnection` no `appsettings.json`. Exemplo para SQL Server Express:

```text
Server=.\SQLEXPRESS;Database=AcademiaMVC;Trusted_Connection=True;TrustServerCertificate=True
```

## Contas de demonstração

O inicializador cria automaticamente um administrador quando o banco já está acessível:

- **E-mail:** `admin@academia.local`
- **Senha inicial:** `Admin@12345`

Altere essa senha antes de qualquer uso fora da demonstração escolar. O cadastro público cria contas comuns; não permite que o visitante escolha o papel de administrador.

Um aluno deve criar sua conta, entrar, abrir **Meu perfil**, preencher os dados e salvar. Depois pode acessar **Modalidades**, solicitar matrícula e consultar **Minhas matrículas**.

## Script SQL de exemplo

O arquivo `database/PopularModalidades.sql` contém inserts de modalidades. Execute-o somente depois de criar o banco e as tabelas pela migration. O sistema também insere três modalidades automaticamente quando a tabela estiver vazia.

## Estrutura

```text
AcademiaMVC/
  Areas/Identity/Pages/       # integração da interface Identity
  Controllers/                # Home, Alunos, Modalidades e Matriculas
  Data/                       # ApplicationDbContext e inicialização
  Models/                     # Aluno, Modalidade e Matricula
  Views/                      # páginas Razor MVC
  wwwroot/css/                # estilos
  Program.cs
  appsettings.json
  AcademiaMVC.csproj
```

## Observação importante sobre a entrega

O ZIP contém o projeto-fonte pronto para abrir e as instruções de migration. A pasta `Data/Migrations` será criada pelo Entity Framework no ambiente do Visual Studio ao executar `Add-Migration Inicial`, pois a migration precisa ser gerada pela versão 10.0.8 do EF Core e aplicada à instância SQL Server disponível.
