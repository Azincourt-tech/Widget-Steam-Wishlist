# Steam Wishlist Widget

Widget de área de trabalho que mostra a lista de desejos da Steam do usuário, com as promoções no topo. Interface em **Avalonia 12** sobre **.NET 10**, arquitetura **MVVM** (CommunityToolkit.Mvvm). Sistemas suportados: **Windows** (hoje) e **Linux** (próximo, no roadmap). **macOS não será suportado** (decisão do usuário, outubro de 2026). O visual continua inspirado no macOS; só o suporte ao sistema ficou de fora.

Documentação completa para desenvolvedores: [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md). Ideias planejadas: [docs/ROADMAP.md](docs/ROADMAP.md). Leia os dois antes de mudanças grandes.

## Como trabalhar com o usuário

- **Converse sempre em português (Brasil).** Comentários no código, textos da interface e documentação também em português.
- O usuário é desenvolvedor .NET e decide o rumo do projeto. Antes de mudanças grandes (trocar tecnologia, nova dependência, mudar arquitetura), explique as opções e espere a decisão.
- **Nunca use dados reais do usuário** (SteamID, link de perfil, lista de desejos) em testes, exemplos, documentação ou prints. Use os fictícios: SteamID `76561190000000001` / `76561190000000002` e perfil `perfil-exemplo`. Prints sempre no modo demonstração (`--demo`).
- Depois de testar a geração do `.exe` (`dotnet publish`), **apague o executável gerado** e as pastas `win-x64` intermediárias.
- Ao testar a interface, prefira automação (UI Automation pelo PowerShell) a simular cliques do mouse: o usuário costuma estar usando o PC.
- Ao final de uma mudança visual, confira de verdade na tela (capturar a janela e olhar), nos três temas quando fizer sentido.

## Comandos

```bash
dotnet build SteamWishlistWidget.sln            # precisa terminar com 0 avisos (avisos são erro)
dotnet test SteamWishlistWidget.sln             # 61 testes hoje, rodam em qualquer sistema
dotnet run --project src/SteamWishlistWidget    # abre o widget com o perfil real do usuário
dotnet run --project src/SteamWishlistWidget -- --demo --theme=Dark   # dados fictícios (Dark|Light|Glass)
```

Gerar o executável único (o mesmo comando do CI):

```bash
dotnet publish src/SteamWishlistWidget -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
```

O widget só abre uma cópia por vez (mutex). Antes de recompilar, feche o widget aberto (`Stop-Process -Name SteamWishlistWidget`), senão o `.exe` fica travado. O modo `--demo` tem mutex próprio e pode rodar junto.

## Estrutura

| Pasta | O que é | Regras |
|---|---|---|
| `src/SteamWishlistWidget.Core` | Lógica: Models, ViewModels, Services, Constants. `net10.0` puro | Nada de Avalonia nem API exclusiva de um sistema. Veja o CLAUDE.md da pasta |
| `src/SteamWishlistWidget` | Interface Avalonia, temas, estilos e `Platform/` (o que é de cada sistema) | Só comportamento visual nas Views. Veja o CLAUDE.md da pasta |
| `tests/SteamWishlistWidget.Tests` | xUnit, referencia só o Core | Veja o CLAUDE.md da pasta |
| `docs/` | CONTRIBUTING, ROADMAP e imagens do README | |
| `.github/` | CI (Windows + Ubuntu), modelos de issue e de PR | |

- `README.md` é **só para o usuário final** (quem baixa o `.exe` da Release). Coisas técnicas vão no `docs/CONTRIBUTING.md`.
- Configurações comuns de build ficam no `Directory.Build.props` (versão, `TreatWarningsAsErrors`, autor).

## Regras do projeto

- **Privacidade:** os dados do usuário ficam **somente no PC dele**. Sem servidor próprio; o app só conversa com a Steam (e, no futuro, com o GitHub para atualizações). O SteamID é salvo criptografado (`ISecretProtector`). Nunca grave SteamID no log.
- **Segurança:** toda URL nova vai em `Constants/SteamEndpoints.cs` e usa HTTPS. Imagens só de `*.steamstatic.com` (`TrustedImageHosts`). A página da loja é montada a partir do AppID numérico. Todo valor novo de configuração é validado em `AppSettings.Normalize()`.
- **Login com a Steam** não é proibido: está "em estudo" no roadmap, sempre com dados só no PC e sem o app ver a senha (OpenID oficial).
- Toda mudança de comportamento vem com teste. Build com 0 avisos.
- Uma classe ou interface por arquivo; namespaces por arquivo; campos privados `_camelCase` (`.editorconfig`).
- Atualize o `CONTRIBUTING.md` (e o README, se afetar o usuário) junto com a mudança.

## Estado atual (outubro de 2026)

- Projeto em `C:\Dev\SteamWishlistWidget` no PC original. Projeto irmão com o mesmo padrão: `C:\Dev\AiUsageWidget` (Widget Ai Usage).
- **Git:** repositório `https://github.com/Azincourt-tech/Widget-Steam-Wishlist` (licença MIT). Trabalho na branch `develop`; a `main` só recebe o que for lançado (cada push na `main` gera uma release).
  - **Nome e e-mail dos commits ficam só neste repositório** (`.git/config`, nome `Willian Almeida` e o e-mail pessoal do usuário, o mesmo do repositório Widget-Ai-Usage), porque o git global deste PC usa o e-mail do trabalho, que **não pode ir para o histórico**. Antes de commitar, confira com `git config --show-origin user.email` (tem que vir de `file:.git/config`). Num clone novo em outro PC, configure de novo com `git config --local` (pergunte o e-mail ao usuário ou copie do `.git/config` do Widget-Ai-Usage). Não escreva o e-mail em arquivos do repositório.
  - **Commit e push só quando o usuário pedir.**
  - **Nada sensível no repositório:** antes de qualquer commit, procure nos arquivos que vão subir por e-mails, SteamIDs reais (`7656119` seguido de dígitos que não sejam os fictícios), `secret`, `token`, `password`, `eyJ`, caminhos `C:\Users\` e o nome da empresa do usuário. Na dúvida, pergunte.
- **GitHub Actions:** `ci.yml` (build e testes em Windows e Ubuntu, em push na `develop` e em PR) e `release.yml` (push na `main`: testes, `.exe` único e release `v<Version>` com `SteamWishlistWidget-v<Version>-win-x64.exe`; falha se a release já existir, então suba a `<Version>` do `Directory.Build.props` antes). `dependabot.yml` abre PRs semanais para a `develop`; `SECURITY.md` aponta o reporte privado de falhas.
- Pendências no GitHub (feitas pelo usuário na interface web): ligar o reporte privado de falhas, alertas e correções do Dependabot, secret scanning com push protection, regras de proteção da `main`/`develop` e criar as etiquetas `sugestão` e `good first issue`.
- Histórico relevante: começou em WPF, foi migrado para MVVM, depois para .NET 10 e por fim para Avalonia 12 (o WPF foi removido). A separação em `Core` foi feita para permitir o Linux.
- Próximos passos possíveis (detalhes no ROADMAP): verificar e instalar atualizações, tela de detalhes do jogo, suporte a Linux (AppImage primeiro, Flatpak depois), filtro por preço, notificações, tradução, acessibilidade, login com a Steam (em estudo).

## Arquivos do usuário (fora do repositório)

- Configurações: `%AppData%\SteamWishlistWidget\config.json` (SteamID em `SteamIdProtected`, criptografado com DPAPI; só pode ser lido pela mesma conta do Windows).
- Log de erros: `%LocalAppData%\SteamWishlistWidget\logs\error.log`.
- Em outro PC, o `SteamIdProtected` do PC antigo não abre: o usuário cola o link do perfil de novo.
