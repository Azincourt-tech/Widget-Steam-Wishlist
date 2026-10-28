# SteamWishlistWidget.Tests

Testes xUnit do `SteamWishlistWidget.Core`. Alvo `net10.0`: rodam em Windows e Linux (o CI roda nos dois).

## Regras

- Referencie só o Core. Nada de Avalonia aqui; a interface é conferida rodando o app no modo demonstração.
- **Dados fictícios sempre:** SteamID `76561190000000001` (e `76561190000000002` quando precisar de outro), perfil `perfil-exemplo`. Nunca use IDs ou perfis reais.
- Teste ViewModels com os fakes de `Fakes/Fakes.cs`: `FakeSteamApiClient`, `FakeSettingsStore`, `FakeStoreLauncher`, `FakeStartupRegistration`, `FakeThemeService`, `FakeDialogService`, `FakeUiTimer` (dispara com `Fire()`), `FakeNotificationService` (guarda os avisos em `Notified`) e `FakeSecretProtector` (reversível, funciona em qualquer sistema). Ao criar uma interface nova no Core, crie o fake aqui.
- `TestSetup.cs` (`[ModuleInitializer]`) redireciona o `ErrorLog` para uma pasta temporária. Os testes nunca podem escrever no log real do usuário.
- Testes que dependem do Windows (ex.: `WindowsDataProtectorTests`, DPAPI) começam com `if (!OperatingSystem.IsWindows()) return;`.
- Testes de `SettingsStore` usam um arquivo temporário por teste (apagado no `Dispose`).
- Leitura de respostas da Steam é testada com JSON/XML de exemplo nos métodos estáticos `SteamApiClient.ParseXxx`, sem internet. Inclua casos de resposta vazia, inesperada e maliciosa.

## Rodar

```bash
dotnet test SteamWishlistWidget.sln
```
