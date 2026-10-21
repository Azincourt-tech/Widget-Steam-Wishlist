# SteamWishlistWidget (interface Avalonia)

Interface do widget em **Avalonia 12.1.4**, alvo `net10.0`. A lógica está no `SteamWishlistWidget.Core`; aqui ficam só telas, temas, estilos e o que é de cada sistema (`Platform/`).

## Regras

- Views (`Views/*.axaml.cs`) têm **só comportamento visual**: arrastar a janela, efeito vidro, sombra por tema, manter a borda direita ao minimizar. Estado e ações ficam nos ViewModels do Core.
- `App.axaml.cs` é o único lugar que cria serviços (composition root). O modo `--demo` troca store, Steam e startup por versões em memória.
- `Platform/PlatformServices.cs` escolhe a implementação por sistema. Hoje só Windows (`Platform/Windows/`); no Linux, fora do `--demo`, o app lança `PlatformNotSupportedException` com mensagem amigável.
- Compiled bindings estão ligados (`AvaloniaUseCompiledBindingsByDefault`): toda janela e todo `DataTemplate` precisam de `x:DataType`.
- Cores sempre com `{DynamicResource ...}` (o tema troca em tempo real). Estilos e ícones com `{StaticResource ...}`.
- Botões só com ícone precisam de `AutomationProperties.Name` (senão o leitor de tela anuncia "Avalonia.Controls.PathIcon").
- Ícones são caminhos vetoriais em `Styles/Icons.axaml` (Material Design Icons). Não use fontes de ícones do Windows (Segoe), para funcionar em qualquer sistema.

## Armadilhas já resolvidas (não repita)

- **Troca de tema:** o Avalonia compila os `ResourceInclude` do XAML e eles deixam de ser identificáveis. Por isso o tema **não** fica no `App.axaml`; o `Themes/ThemeManager.cs` adiciona o dicionário no fim de `MergedDictionaries` e guarda a referência para remover depois. Ele também ajusta `RequestedThemeVariant` (Dark/Light) para os controles Fluent.
- **Chave repetida:** nunca use a mesma chave para uma cor e para um estilo/ControlTheme. Uma sobrescreve a outra e a tela quebra ao abrir (já aconteceu com `SegmentTrack`; o estilo virou a classe `segmentTrack`).
- **Cantos do tema Vidro:** o desfoque do Windows cobre o retângulo inteiro da janela e ignora cantos desenhados pela interface, `SetWindowRgn` e `DwmEnableBlurBehindWindow`. A solução: no Vidro, `WindowDecorations = BorderOnly` (o sistema arredonda com 8 px e corta o desfoque) e o painel usa `WindowsCorners.SystemCornerRadius` (8 px). `Platform/Windows/WindowsCorners.cs` desliga o contorno claro do sistema. Nos temas Escuro e Claro, `WindowDecorations = None` e cantos de 20 px.
  - Para conferir cantos, capture a janela **sobre um fundo listrado**. Fundo liso desfocado continua igual e esconde o problema.
- **Avalonia 12 renomeou/removeu propriedades:** `SystemDecorations` → `WindowDecorations`; `ExtendClientAreaChromeHints` não existe mais. Com `WindowDecorations = Full` aparece a barra de título do Windows.
- **Minimizar mantendo a borda direita:** ao trocar painel ↔ ícone, a janela muda de tamanho em mais de um passo (a moldura do Vidro liga/desliga). O ajuste de posição fica ativo por 400 ms (`_keepRightEdge` + `DispatcherTimer.RunOnce`) e é reaplicado a cada mudança de `ClientSize`.
- **Imagens da internet:** o Avalonia não carrega URL em `Image` sozinho. Use `ctl:RemoteImage.Url="{Binding ...}"` (`Controls/RemoteImage.cs`), que tem cache e só aceita hosts confiáveis da Steam.
- **Arrastar:** o painel inteiro arrasta (`BeginMoveDrag`), exceto botões, campos de texto e barra de rolagem. Nas linhas de jogo, só arrasta depois de 4 px de movimento; clique simples abre a loja. No ícone minimizado, `BeginMoveDrag` bloqueia até soltar o mouse no Windows, e a posição antes/depois decide entre "arrastou" e "clicou".
- **Diálogos são assíncronos:** `ShowDialog<bool>(owner)`; por isso `IDialogService.ShowSettingsAsync`.

## Testar visualmente

```powershell
dotnet build src/SteamWishlistWidget -c Release
# Abre no modo demonstração; capture a janela pelo retângulo da UI Automation e confira a imagem.
Start-Process src/SteamWishlistWidget/bin/Release/net10.0/SteamWishlistWidget.exe -ArgumentList "--demo","--theme=Glass"
```

Os botões do cabeçalho podem ser acionados por UI Automation pelo nome (`Configurações`, `Atualizar`, `Fixar (sempre visível)`, `Minimizar para ícone`, `Fechar`), sem mover o mouse do usuário. As imagens do README (`docs/*.png`) são geradas assim, no modo demonstração.
