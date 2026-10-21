# Política de segurança

## Versões com suporte

Só a versão mais recente publicada em [Releases](https://github.com/Azincourt-tech/Widget-Steam-Wishlist/releases) recebe correções de segurança.

## Como reportar uma falha

**Não abra uma issue pública** para falhas de segurança.

Use o reporte privado do GitHub: na aba **Security** do repositório, clique em **Report a vulnerability**. Só os mantenedores veem o relato.

Inclua, se puder:

- o que acontece e qual o impacto;
- os passos para reproduzir;
- a versão do widget e do Windows.

Nunca envie o link do seu perfil, seu SteamID, o conteúdo do `config.json` ou dados reais da sua conta no relato.

## O que esperar

Este é um projeto pessoal, mantido no tempo livre. O objetivo é responder em até 7 dias e, confirmada a falha, publicar a correção numa nova versão o quanto antes, dando o crédito a quem reportou (se quiser).

## Escopo

Interessam especialmente:

- vazamento do SteamID ou do perfil (arquivo de configuração, criptografia, logs);
- imagens ou páginas carregadas de endereços fora da Steam;
- leitura insegura das respostas da Steam (JSON e XML do perfil);
- qualquer forma de dados do usuário saírem do computador dele.

Ficam de fora problemas nos serviços da Steam/Valve em si: reporte direto a eles.
