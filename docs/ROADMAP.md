# 🗺️ Implementações futuras

Ideias para as próximas versões do **Steam Wishlist Widget**. Nada aqui tem data marcada: é uma lista do que faz sentido para o projeto, com pistas de como implementar.

Quer pegar uma delas? Abra uma issue dizendo qual, para combinarmos antes, e siga o [guia para contribuidores](CONTRIBUTING.md).

**Legenda de dificuldade:** 🟢 fácil · 🟡 média · 🔴 grande

---

## Resumo

| Ideia | Dificuldade | Bom para começar |
|---|---|---|
| [Novos idiomas e países da loja](#-novos-idiomas-e-países-da-loja) | 🟢 | ✅ |
| [Novos temas](#-novos-temas) | 🟢 | ✅ |
| [Mais testes e documentação](#-mais-testes-e-documentação) | 🟢 | ✅ |
| [Filtro por preço máximo](#-filtro-por-preço-máximo) | 🟢 | ✅ |
| [Mostrar até quando vai a promoção](#-mostrar-até-quando-vai-a-promoção) | 🟡 | |
| [Notificação quando um jogo entrar em promoção](#-notificação-quando-um-jogo-entrar-em-promoção) | 🟡 | |
| [Melhorias de acessibilidade](#-melhorias-de-acessibilidade) | 🟡 | |
| [Verificar e instalar atualizações](#-verificar-e-instalar-atualizações) | 🟡 | |
| [Tela de detalhes do jogo](#-tela-de-detalhes-do-jogo) | 🔴 | |
| [Tradução da interface](#-tradução-da-interface) | 🔴 | |
| [Suporte a Linux](#-suporte-a-linux) | 🔴 | |
| [Login com a Steam](#-login-com-a-steam-em-estudo) | 🔴 | em estudo |

---

## 🌎 Novos idiomas e países da loja

**Dificuldade:** 🟢 fácil

Hoje a loja pode ser consultada em português, inglês e espanhol. A Steam aceita muitos outros idiomas (`french`, `german`, `japanese`...), que mudam o nome dos jogos e as capas regionais.

**Como fazer**
- Adicionar o código do idioma em `AppSettings.Languages`
- Adicionar a opção em `Views/SettingsWindow.axaml`
- A receita completa está no [CONTRIBUTING](CONTRIBUTING.md#adicionar-um-novo-idioma-da-loja)

**Ponto de atenção:** o controle segmentado tem espaço para poucas opções. Com muitos idiomas, ele pode precisar virar uma lista suspensa.

---

## 🎨 Novos temas

**Dificuldade:** 🟢 fácil

Novos esquemas de cor além de Escuro, Claro e Vidro. Exemplos: um tema com as cores da própria Steam ou um tema de alto contraste.

**Como fazer**
- Criar `Themes/NomeDoTema.axaml` com **todas** as chaves de cor dos temas existentes
- A receita completa está no [CONTRIBUTING](CONTRIBUTING.md#adicionar-um-novo-tema)

**Ponto de atenção:** conferir o tema na tela principal, na tela de configurações e no menu de clique direito. Prints no modo demonstração (`--demo --theme=NomeDoTema`).

---

## 🧪 Mais testes e documentação

**Dificuldade:** 🟢 fácil

**Ideias**
- Testes para `ThemeManager` e para o `ValueEqualsConverter`
- Testes do `SteamApiClient` com um `HttpMessageHandler` falso, cobrindo tempo limite e respostas de erro
- Versão do README em inglês
- GIF curto mostrando o widget em uso, gerado no modo demonstração

---

## 💸 Filtro por preço máximo

**Dificuldade:** 🟢 fácil

Mostrar só os jogos abaixo de um valor, por exemplo "só jogos abaixo de R$ 50". Funciona junto com a opção "Mostrar só promoções".

**Como fazer**
- Nova configuração `MaxPriceCents` (vazia = sem limite) em `AppSettings`, validada em `Normalize`
- Filtrar em `WishlistSorter.Sort` usando `GameDeal.FinalCents`
- Campo na tela de configurações, seção **Lista**
- Seguir a receita [Adicionar uma nova opção de configuração](CONTRIBUTING.md#adicionar-uma-nova-opção-de-configuração)

**Ponto de atenção:** o valor é na moeda do país escolhido. Se o usuário trocar de país, o limite pode não fazer mais sentido; vale mostrar a moeda ao lado do campo.

---

## ⏳ Mostrar até quando vai a promoção

**Dificuldade:** 🟡 média

Exibir algo como "termina em 3 dias" nos jogos em promoção.

**Como fazer**
- A resposta de `IStoreBrowseService/GetItems` já traz a data em `best_purchase_option.active_discounts[].discount_end_date` (timestamp Unix)
- Ler esse campo em `SteamApiClient.ParseStoreItems` e adicionar `DiscountEndsAt` em `GameDeal`
- Mostrar no tooltip do jogo ou como texto pequeno abaixo do preço
- Testes em `SteamApiClientParsingTests` com e sem a data

**Ponto de atenção:** a linha da lista tem altura fixa (`WidgetViewModel.RowHeight`). Se o texto for na linha, a altura precisa ser revista.

---

## 🔔 Notificação quando um jogo entrar em promoção

**Dificuldade:** 🟡 média

Uma notificação do Windows quando, numa atualização automática, um jogo que estava sem desconto aparecer em promoção.

**Como fazer**
- Guardar os AppIDs que estavam em promoção na última atualização (no `WidgetViewModel`, ou salvos para sobreviver ao reinício)
- Comparar com a nova lista e notificar só os jogos que entraram agora
- Criar um `INotificationService` em `Services/Abstractions` para os ViewModels continuarem testáveis
- Para a notificação: no Windows, toast nativo do sistema; no Linux, a central de notificações do ambiente gráfico (via D-Bus). Um `INotificationService` por sistema em `Platform/` resolve isso. Alternativa simples e multiplataforma: o `WindowNotificationManager` do próprio Avalonia (aviso dentro do widget). Nova dependência precisa ser discutida antes
- Opção para ligar e desligar nas configurações

**Ponto de atenção:** não notificar na primeira carga nem depois de trocar de perfil, senão todos os jogos em promoção viram notificação de uma vez.

---

## ♿ Melhorias de acessibilidade

**Dificuldade:** 🟡 média

**Ideias**
- **Teclado:** navegar pela lista com as setas e abrir o jogo com Enter; hoje os botões do cabeçalho só aparecem com o mouse por cima
- **Leitor de tela:** `AutomationProperties.Name` nos botões de ícone e nas linhas da lista (ex.: "Hades, 50% de desconto, R$ 36,99")
- **Contraste:** revisar as cores de texto secundário (`TextDim`) no tema Vidro, que fica sobre fundos variados
- **Tamanho do texto:** respeitar a configuração de tamanho de texto do Windows

---

## 🔄 Verificar e instalar atualizações

**Dificuldade:** 🟡 média · **Viabilidade:** ✅ confirmada

Avisar quando existir uma versão nova do widget e, com a confirmação do usuário, baixar e instalar sozinho.

**O que foi verificado**
- A API pública do GitHub (`GET https://api.github.com/repos/<dono>/SteamWishlistWidget/releases/latest`) devolve, sem login:
  - a versão mais recente (`tag_name`, ex.: `v1.2.0`)
  - as notas da versão (`body`)
  - o link do `.exe` (`assets[].browser_download_url`)
  - o **SHA-256 de cada arquivo** (`assets[].digest`), que permite conferir que o download não foi corrompido nem alterado
- O workflow de release já publica o `.exe` em cada tag `v*`, então os dados já estariam lá.

**Como fazer**
1. `IUpdateService` em `Services/Abstractions` com `CheckAsync()` e `DownloadAndInstallAsync()`
2. Comparar `tag_name` com a versão do app (`<Version>` do `Directory.Build.props`, lida do assembly)
3. Verificar ao abrir o widget e a cada 24 h, mais um botão **Verificar atualizações** nas configurações
4. Quando houver versão nova: aviso discreto no widget ("Nova versão 1.2.0 disponível") com as notas e os botões **Atualizar** e **Agora não**
5. Ao atualizar:
   - baixar o `.exe` para uma pasta temporária
   - conferir o SHA-256 com o `digest` da release; se não bater, descartar e avisar
   - renomear o `.exe` em uso para `SteamWishlistWidget.old.exe` (o Windows permite renomear um programa aberto, mas não sobrescrever)
   - colocar o novo no lugar, abrir a nova versão e fechar a atual
   - na próxima abertura, apagar o `.old.exe`
6. Opção nas configurações: **Verificar atualizações automaticamente** (ligada por padrão)

**Pontos de atenção**
- **Segurança:** só aceitar downloads de `github.com` / `objects.githubusercontent.com` por HTTPS e sempre conferir o SHA-256. O ideal, no futuro, é assinar o `.exe` digitalmente.
- A API do GitHub sem login tem limite de 60 consultas por hora por IP; uma verificação a cada 24 h fica muito abaixo disso.
- Se o widget estiver numa pasta sem permissão de escrita (ex.: `C:\Program Files`), a troca falha; nesse caso, abrir a página da release para download manual.
- O caminho de "Iniciar com o Windows" continua o mesmo, porque o nome do arquivo não muda.
- Alternativa pronta: a biblioteca [Velopack](https://velopack.io) faz instalação e atualização automáticas, mas muda a forma de distribuir (instalador em vez de `.exe` solto). Nova dependência precisa ser discutida antes.

---

## 🎬 Tela de detalhes do jogo

**Dificuldade:** 🔴 grande · **Viabilidade:** ✅ confirmada (trailer completo com ressalvas)

Ao clicar num jogo, abrir uma tela do próprio widget com mais informações, em vez de abrir a loja no navegador. Uma opção nas configurações define o comportamento:

- **Ao clicar num jogo:** `Abrir tela de detalhes` · `Abrir loja no navegador`

**O que foi verificado**

A API oficial que o widget já usa (`IStoreBrowseService/GetItems`) devolve tudo o que a tela precisa, só pedindo mais campos em `data_request`:

| Pedido | Dados | Testado com Hades |
|---|---|---|
| `include_basic_info` | Descrição curta, desenvolvedoras, distribuidoras, franquia | ✅ |
| `include_reviews` | % de avaliações positivas e o rótulo ("Extremamente positivas") | ✅ 97%, 287 mil avaliações |
| `include_release` | Data de lançamento | ✅ |
| `include_tag_count` | Tags populares (ação, roguelike...) | ✅ |
| `include_screenshots` | Capturas de tela em tamanho médio e cheio | ✅ 11 imagens |
| `include_trailers` | Trailers: prévia curta em **MP4/WebM** e trailer completo em **HLS/DASH** | ✅ 3 trailers |

Sobre os trailers:
- O Avalonia **não tem player de vídeo embutido**. A prévia curta (`microtrailer.mp4`, uns 3 MB) e o trailer completo precisam de uma biblioteca de vídeo.
- O **trailer completo** só existe em streaming adaptativo (HLS `.m3u8` ou DASH `.mpd`). Opções:
  1. **LibVLCSharp.Avalonia** (VLC embutido): toca MP4, HLS e DASH, em Windows e Linux. Adiciona uma dependência grande (bibliotecas do VLC, dezenas de MB).
  2. Mostrar a capa e as capturas de tela, com um botão **Ver trailer na Steam**. Mais simples e sem dependência.
- O endpoint não oficial `store.steampowered.com/api/appdetails` também traz requisitos de sistema e nota do Metacritic, mas tem limite de consultas mais rígido. Preferir o `GetItems` oficial.

**Como fazer**
1. `GameDetails` em `Models` e `GetGameDetailsAsync(appId)` no `ISteamApiClient`, com a leitura em método estático testável (como os outros)
2. `GameDetailsViewModel` e `Views/GameDetailsWindow.axaml`, com o mesmo visual e temas do widget
3. Conteúdo sugerido: capa grande, nome, preço/desconto, avaliações, descrição, tags, data de lançamento, desenvolvedora, galeria de capturas, trailer e botão **Abrir na Steam**
4. Nova configuração `OpenGameIn` (`Details` ou `Browser`) em `AppSettings` e na tela de configurações
5. `WidgetViewModel.OpenDeal` passa a decidir entre `IStoreLauncher` e `IDialogService.ShowGameDetails`
6. No modo demonstração, `DemoSteamApiClient` devolve detalhes fictícios para prints e testes

**Pontos de atenção**
- Carregar os detalhes só quando a tela abrir (e guardar em memória por alguns minutos), para não multiplicar as chamadas à Steam.
- Imagens e vídeos só dos servidores da Steam (`*.steamstatic.com`), mantendo a mesma regra de segurança atual.
- Vídeo não deve tocar com som automaticamente.

---

## 🗣️ Tradução da interface

**Dificuldade:** 🔴 grande

Hoje todos os textos da interface estão fixos em português, espalhados pelos arquivos XAML e pelos ViewModels. Traduzir para outros idiomas é uma contribuição grande e útil.

**Como fazer**
1. Separar todos os textos num arquivo de recursos (`Resources/Strings.resx`) e usar nos XAML e ViewModels
2. Criar `Strings.en.resx`, `Strings.es.resx`...
3. Escolher o idioma pelo Windows, com opção de trocar nas configurações
4. Traduzir também as mensagens de erro do `WidgetViewModel` e do `SettingsViewModel`

**Ponto de atenção:** o idioma da interface é diferente do idioma da loja (que muda o nome dos jogos). Os dois podem coexistir nas configurações.

---

## 🐧 Suporte a Linux

**Dificuldade:** 🔴 grande · **Viabilidade:** ✅ base pronta

Distribuir o widget também para Linux, aumentando o público e a quantidade de pessoas que podem contribuir.

> **macOS não será suportado** (decisão de outubro de 2026). O foco fica em Windows e Linux. O visual continua inspirado no macOS; só o suporte ao sistema ficou de fora.

**O que já está pronto**
- A interface foi migrada de WPF para **Avalonia**, que roda em Windows e Linux com o mesmo XAML.
- Toda a lógica (ViewModels, acesso à Steam, configurações, testes) fica no `SteamWishlistWidget.Core`, sem nada exclusivo de um sistema.
- O que depende do sistema já está atrás de interfaces: `ISecretProtector` (criptografia), `IStartupRegistration` (iniciar com o sistema) e `IUiTimer`.
- O CI já compila a solução e roda os testes no **Ubuntu**, além do Windows.
- O **modo demonstração** (`--demo`) já abre no Linux, porque não usa criptografia nem registro.
- Hoje, sem `--demo`, o app avisa no Linux que ainda não é suportado (`Platform/PlatformServices.cs`).

**O que falta**

| Parte | Linux |
|---|---|
| **Criptografia do SteamID** (`ISecretProtector`) | Secret Service / libsecret (GNOME Keyring, KWallet), via D-Bus ou `secret-tool` |
| **Iniciar com o sistema** (`IStartupRegistration`) | Arquivo `.desktop` em `~/.config/autostart` |
| **Efeito vidro** | Depende do compositor; muitas vezes só há transparência sem desfoque. Mostrar o tema Escuro como alternativa |
| **Posição da janela** | No **Wayland**, o app não pode escolher a posição da janela nem garantir "sempre visível"; funciona no X11 e pelo XWayland |
| **Pastas de dados** | `~/.config` e `~/.local/share` (o .NET já resolve) |
| **Distribuição** | AppImage primeiro; Flatpak depois (veja abaixo) |
| **CI de release** | `dotnet publish -r linux-x64` |

**Como fazer**
1. Criar `Platform/Linux/` com as implementações de `ISecretProtector` e `IStartupRegistration`
2. Escolher a implementação em `PlatformServices` por `OperatingSystem.IsLinux()`
3. Trocar o texto "Iniciar com o Windows" das configurações por "Iniciar com o sistema"
4. Detectar quando o desfoque não está disponível (`ActualTransparencyLevel`) e cair para o tema Escuro
5. Adicionar o AppImage ao workflow de release
6. Testar em pelo menos: Pop!_OS 24.04 (COSMIC), Ubuntu com GNOME (Wayland) e uma distro com KDE

**Pontos de atenção**
- A regra de privacidade continua valendo: o SteamID só pode ser salvo usando o cofre de senhas do próprio sistema, nunca em texto puro.
- O Wayland limita posição e "sempre visível" por segurança; isso é do sistema, não do app. Documentar no README.

**AppImage no Pop!_OS (e outras distros baseadas no Ubuntu)**

O AppImage é o primeiro formato para Linux. Pontos a cobrir no README e nos testes:
- O usuário precisa dar permissão de execução (`chmod +x` ou "Permitir executar como programa").
- O AppImage usa a **FUSE 2**, que não vem mais por padrão nas versões recentes do Ubuntu e do Pop!_OS: `sudo apt install libfuse2t64` (24.04) ou `libfuse2` (22.04). Sem ela, dá para rodar com `--appimage-extract-and-run`.
- O **Pop!_OS 24.04 usa o ambiente COSMIC**, sobre Wayland. O Avalonia roda nele pelo XWayland; testar posição, "sempre visível" e o efeito vidro (provavelmente só transparência, sem desfoque).

### Melhoria futura: Flatpak

> Só depois que a versão AppImage estiver funcionando corretamente.

Publicar também como **Flatpak**, de preferência no [Flathub](https://flathub.org).

**Vantagens**
- Aparece nas lojas do sistema (incluindo a loja do COSMIC no Pop!_OS, o GNOME Software e o Discover do KDE)
- Atualiza sozinho junto com o sistema
- Não depende da FUSE 2

**Como fazer**
1. Criar o manifesto Flatpak (`io.github.<usuario>.SteamWishlistWidget.yml`) usando o runtime `org.freedesktop.Platform` e o SDK de .NET (`org.freedesktop.Sdk.Extension.dotnet10`)
2. Declarar só as permissões necessárias: rede (`--share=network`), janela (`--socket=x11` / `--socket=wayland`) e acesso ao cofre de senhas (`--talk-name=org.freedesktop.secrets`)
3. "Iniciar com o sistema" dentro do Flatpak deve usar o portal de segundo plano (`org.freedesktop.portal.Background`), já que o app não pode escrever direto em `~/.config/autostart`
4. Gerar o pacote no CI e enviar para revisão no Flathub

**Pontos de atenção**
- O Flathub tem um processo de revisão e exige metadados (ícone, descrição, capturas de tela no formato AppStream).
- O isolamento do Flatpak pode exigir ajustes no caminho do `config.json` e do log (ficam dentro de `~/.var/app/<id>/`).

---

## 🔐 Login com a Steam (em estudo)

**Dificuldade:** 🔴 grande · **Viabilidade:** ✅ possível, com escopo a definir

Permitir entrar com a conta Steam em vez de colar o link do perfil, abrindo caminho para funcionalidades que dependem de saber quem é o usuário.

**Princípio obrigatório: os dados ficam só no PC da pessoa.**
- Nada de servidor próprio: o widget conversa apenas com a Steam.
- O widget **nunca** vê nem guarda a senha da Steam.
- Qualquer identificador ou chave fica criptografado com DPAPI (como o SteamID já fica hoje) e pode ser apagado com **Sair** nas configurações.

**Formas de login possíveis**

| Forma | Como funciona | O que libera | Avaliação |
|---|---|---|---|
| **Steam OpenID** (oficial) | Abre a página de login da própria Steam no navegador; a Steam devolve o SteamID para o widget por um endereço local (`http://localhost:<porta>`) | Identifica o usuário com segurança, sem colar link | ✅ Recomendado como primeiro passo. A senha é digitada só no site da Steam |
| **OpenID + API key do usuário** | Além do login, a pessoa gera uma chave pessoal em `steamcommunity.com/dev/apikey` e cola nas configurações | APIs oficiais da Web API: jogos que possui, tempo de jogo, amigos | 🟡 Possível, mas exige um passo técnico do usuário; a chave é sensível e precisa ficar criptografada |
| Login com usuário e senha dentro do widget | O app recebe a senha e fala com a Steam como se fosse o cliente oficial | Quase tudo | ❌ Não recomendado: o app passaria a manipular a senha e o Steam Guard, usa APIs não oficiais e pode quebrar ou violar os termos da Steam |

**Ideias que o login pode liberar** (a confirmar em cada caso)
- Entrar sem precisar deixar a lista de desejos pública (depende de a Steam aceitar a leitura da lista privada com a identificação do próprio dono; precisa ser testado)
- Esconder da lista jogos que a pessoa já possui (ex.: comprou e ainda não tirou da lista)
- Mostrar quais amigos já têm o jogo
- Notificações mais confiáveis, ligadas à conta

**Como fazer (primeiro passo: OpenID)**
1. `ISteamAuthService` com `SignInAsync()` e `SignOut()`
2. Abrir `https://steamcommunity.com/openid/login` no navegador padrão com `openid.return_to` apontando para um `HttpListener` local temporário
3. Validar a resposta com a própria Steam (`openid.mode=check_authentication`) antes de confiar no SteamID
4. Salvar o SteamID com `DataProtector` e mostrar **Conectado como …** e **Sair** nas configurações
5. Manter a opção atual de colar o link para quem não quiser fazer login

**Pontos de atenção**
- O OpenID sozinho só devolve o SteamID; funcionalidades extras dependem de cada API e precisam ser verificadas uma a uma antes de prometer.
- Antes de implementar, atualizar a seção de privacidade do README e a política de contribuição.

---

## Tem outra ideia?

Abra uma issue com o modelo **💡 Sugerir uma melhoria**. Se fizer sentido para o projeto, ela entra nesta lista.
