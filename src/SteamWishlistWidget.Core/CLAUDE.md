# SteamWishlistWidget.Core

Lógica do widget, independente de sistema operacional e de framework de interface. Alvo `net10.0` (não `net10.0-windows`). O CI compila e testa este projeto em Windows e Ubuntu.

## Regras

- **Nada de Avalonia, WPF ou APIs de um sistema só** (registro, DPAPI, `DispatcherTimer`...). O que depende do sistema entra como interface em `Services/Abstractions` e a implementação fica no app (`src/SteamWishlistWidget/Platform/`).
  - Exceção: `Services/Security/WindowsDataProtector.cs` usa DPAPI, marcado com `[SupportedOSPlatform("windows")]`. Fica aqui para os testes do Windows poderem usá-lo.
- ViewModels recebem **interfaces** pelo construtor; nunca criam serviços concretos. Quem monta tudo é o `App.axaml.cs`.
- ViewModels usam `[ObservableProperty]` em campos `_camelCase` e `[RelayCommand]`. Não referencie o campo gerado diretamente (o analisador do toolkit acusa); use a propriedade.
- Reações a mudanças de propriedade (`partial void OnXChanged`) que salvam ou aplicam algo devem checar `_initialized`, para não disparar ao carregar os valores no construtor.

## Onde mexer

| Quero... | Arquivo |
|---|---|
| Nova opção de configuração | `Models/AppSettings.cs` (+ validar em `Normalize`) e `ViewModels/SettingsViewModel.cs` (construtor, `SaveAsync`, `CopyTo`) |
| Nova chamada à Steam | `Services/SteamApiClient.cs` + interface `ISteamApiClient`. A leitura da resposta fica em método **estático** testável (`ParseXxx`) |
| Novo endereço | `Constants/SteamEndpoints.cs` (sempre HTTPS) |
| Mudar a ordem da lista | `Services/WishlistSorter.cs` |
| Dados do modo demonstração | `Services/Demo/DemoSteamApiClient.cs` (jogos reais da loja, preços fictícios) |
| Nova dependência do sistema | Interface em `Services/Abstractions`, implementação em `src/SteamWishlistWidget/Platform/`, fake em `tests/.../Fakes/Fakes.cs` |

## Detalhes importantes

- `SettingsStore` grava o SteamID criptografado em `SteamIdProtected` e migra o campo antigo `SteamId` (texto puro). A gravação é atômica (`.tmp` + `File.Move`).
- `SteamApiClient.ParseSteamIdFromProfileXml` bloqueia DTD (proteção contra XXE). Não troque por `XDocument.Parse`.
- `ErrorLog.Folder` é `internal` com setter para os testes redirecionarem o log (via `InternalsVisibleTo`).
- APIs da Steam usadas: `IWishlistService/GetWishlist/v1`, `IStoreBrowseService/GetItems/v1` (aceita `include_basic_info`, `include_reviews`, `include_screenshots`, `include_trailers`... já verificados para a futura tela de detalhes) e o XML público do perfil para links `/id/nome`.
