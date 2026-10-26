# Roteiro de testes em jogo — branch `refactor/robustness`

Testes caixa-preta para validar o mod dentro do Terraria depois da refatoração.
Cada teste tem um ID, os passos e o resultado esperado. Os valores vêm do código da branch.

**Legenda**

- ⚠️ **REGRESSÃO**: o comportamento é diferente do `main`. Confirme se a mudança é desejada.
- 🐞 **CORREÇÃO**: um bug do `main` que deve ter sumido.
- ⏱️ **LONGO**: leva vários minutos de tempo real.
- 🔍 **ATENÇÃO**: possível efeito colateral da refatoração.

---

## 0. Preparação

### Ferramentas recomendadas

- **Mundo Journey Mode (Jornada)**, para:
  - congelar e escolher o horário (dia/noite);
  - controlar a chuva;
  - zerar o spawn de inimigos;
  - usar o God Mode. **Desligue o God Mode nos testes de morte** (Gloom, Scorching, Starved).
- **Cheat Sheet** ou **HERO's Mod**, para gerar itens, NPCs e teleportar.
- Um **cronômetro**. Todas as regras do mod rodam **uma vez por segundo de tempo real**.
- **Config do mod** (Configurações → Mods → Terrarian Basic Needs). Os multiplicadores vão de 0,1 a 5. Aumentar para 5 acelera bastante os testes de limiar.

### Como ler os medidores (não há números na tela)

| Medidor | Valor inicial | Cada sprite muda a cada | Observação |
|---|---|---|---|
| Fome | 1200 (sprite "normal") | 100 pontos | Vai de 0 a 1500. Acima de 1200 é "estufado". |
| Descanso | 1200 (cheio) | 100 pontos | De 0 a 1200 |
| Sanidade | 1200 (cheia) | 100 pontos | De 0 a 1200 |
| Temperatura | 1000 (confortável) | 52 graus | De 0 a 2000. A seta mostra a direção da mudança. |

Os **debuffs** também servem de régua, porque aparecem exatamente nos limiares listados em cada seção.

### Personagens de teste

- **P1**: personagem novo, 100 de vida e 20 de mana.
- **P2**: personagem com 400 de vida e 200 de mana (use Cheat Sheet ou cristais de vida).
- **P3**: um personagem criado **com o mod desativado** (vanilla). Serve para os testes de save.

---

## 1. Compatibilidade de save e config (faça ANTES de qualquer outra coisa)

- [ ] **CMP-01: config preservada.**
  - No `main`, configure: Sanity Multiplier = 2, Warmth Multiplier = 3, X Offset = 100, Y Offset = 50.
  - Feche o jogo, compile a branch nova e abra a config.
  - **Esperado:** os quatro valores continuam iguais.
- [ ] **CMP-02: valores do personagem preservados.**
  - No `main`, jogue com um personagem até os quatro medidores saírem do padrão. Anote os sprites ou tire print.
  - Carregue o mesmo personagem na branch nova.
  - **Esperado:** os medidores aparecem iguais. O debuff correspondente surge em até 1 s.
- [ ] 🐞 **CMP-03: personagem vanilla não desmaia nem morre ao entrar.**
  - Carregue o P3 num mundo com o mod ativo.
  - **Esperado:** descanso e sanidade cheios. Nada de Unconscious (desmaio) imediato nem contagem do Gloom.
  - No `main`, o P3 desmaiava na hora e começava a contagem de morte.
- [ ] **CMP-04: contador de cigarros de saves antigos.** Ver ITM-04. O contador antigo ficava salvo nos itens e volta a 0. Isso é esperado.

---

## 2. Fome

**Limiares**

| Status | Faixa | Efeito / observação |
|---|---|---|
| Bloated | ≥ 1400 | −40% de velocidade |
| Full | ≥ 1300 | −25% de velocidade |
| Well Fed | 901 a 1299 | sem debuff |
| Peckish | ≤ 900 | |
| Hungry | ≤ 600 | |
| Famished | ≤ 300 | |
| Starved | 0 | perde 10 de vida por segundo e a regeneração é anulada |
| Vômito | ≥ 1500 | |

**Perda de fome por segundo**

| Situação | Perda |
|---|---|
| Parado | 1 |
| Em movimento (qualquer velocidade, inclusive caindo) | 2 |
| Com Nourished (Vitamin ou Food Pill) | metade dos valores acima |

Tudo isso é multiplicado pelo Hunger Multiplier.

**Restauração ao comer**

| Buff da comida | Fome restaurada |
|---|---|
| Well Fed | +150 |
| Plenty Satisfied | +250 |
| Exquisitely Stuffed | +350 |

### Perda de fome

- [ ] ⚠️ **FOM-01: fome cai parado.**
  - Use o P1 novo, de dia, multiplicador 1, e fique totalmente parado.
  - **Esperado:** o sprite cai um degrau em ~100 s, e Peckish aparece em **~300 s** (5 min).
  - No `main`, parado **nunca** perdia fome.
- [ ] **FOM-02: em movimento.**
  - Fique andando de um lado para o outro sem parar.
  - **Esperado:** um degrau a cada ~50 s e Peckish em **~150 s**.
- [ ] **FOM-03: Nourished.**
  - Coma uma Vitamin (4 min) ou uma Food Pill (8 min).
  - **Esperado:** parado, um degrau a cada ~200 s. Andando, um degrau a cada ~100 s.
- [ ] **FOM-04: multiplicador.**
  - Hunger Multiplier = 5, andando sem parar a partir de 1200.
  - **Esperado:** Peckish em ~30 s, Hungry em ~60 s, Famished em ~90 s, Starved em ~120 s.
  - Use esse modo para chegar rápido aos limiares nos testes seguintes.
- [ ] **FOM-05: um debuff por vez.**
  - Durante o FOM-04, observe a barra de buffs.
  - **Esperado:** nunca aparecem dois debuffs de fome ao mesmo tempo.
- [ ] **FOM-06: Starved.**
  - Chegue a 0.
  - **Esperado:** Starved aparece, a vida cai 10 por segundo e não há regeneração natural.
  - Coma algo: Starved some em até 1 s.

### Comer

- [ ] ⚠️ **FOM-07: restauração pelo tier da comida comida.**
  - Use o truque do limiar: com multiplicador 5 e andando, pare **no exato segundo** em que Famished aparece (fome = 300). Mude o multiplicador para 0,1 para quase congelar e então coma.
  - **Esperado**, com a fome começando em 300:

    | Comida | Fome final | Status | Sprite (contando a partir de 0) |
    |---|---|---|---|
    | Apple (Well Fed) | 450 | Hungry | 4 |
    | Comida Plenty Satisfied | 550 | Hungry | 5 |
    | Comida Exquisitely Stuffed | 650 | Peckish | 6 |

- [ ] ⚠️ **FOM-08: o buff ativo não influencia mais a restauração.**
  - Com Exquisitely Stuffed ainda ativo (coma-o com fome ≥ 550 para ele não ser encurtado), desça até 300 e coma uma **Apple**.
  - **Esperado:** vai para **Hungry** (+150).
  - No `main`, iria para Peckish (+350), porque usava o melhor buff ativo.
- [ ] **FOM-09: buff de comida encurtado quando ainda se está com fome.**
  - Coma algo que deixe a fome **abaixo de 900**.
  - **Esperado:** **todos** os buffs Well Fed ficam com 3 s.
  - Coma algo que deixe a fome em ≥ 900: o buff fica com a duração normal.
- [ ] **FOM-10: Full e Bloated.**
  - A partir de 1200, coma uma Apple (vai a 1350): **Full**.
  - De 1200, coma algo Plenty Satisfied (vai a 1450): **Bloated**.
  - **Esperado:** a velocidade cai visivelmente.
- [ ] 🐞 **FOM-11: Bloated some corretamente.**
  - Fique parado depois do FOM-10, em 1450.
  - **Esperado:** depois de ~50 s, Bloated é trocado por Full. Os dois **nunca** aparecem juntos.
- [ ] **FOM-12: vômito por limite.**
  - A partir de 1200, coma duas Apples seguidas, ou uma Exquisitely Stuffed (1550, limitado a 1500).
  - **Esperado:**
    - fumaça verde;
    - Throwing Up por 2 s (lento, empurrado para a direita);
    - Nauseous por 60 s;
    - todos os Well Fed removidos;
    - fome em 400 (**Hungry**).
- [ ] **FOM-13: Nauseous bloqueia comida.**
  - Durante o Nauseous, tente comer qualquer comida.
  - **Esperado:** a comida não é usada.
  - Poções e itens do mod (Vitamin, Caffeine Pill etc.) funcionam normalmente.
- [ ] **FOM-14: Coffee Cup não é comida.**
  - Beba um Coffee Cup.
  - **Esperado:** ganha **Cafeinated** (4 min). A fome não muda e nenhum Well Fed aparece.
- [ ] **FOM-15: tooltip do Coffee Cup.**
  - **Esperado:** sem a linha vanilla "Minor improvements to all stats". Aparece "Slows rest loss." e a frase "'Hello darkness my old friend'".

### Morte e save

- [ ] **FOM-16: respawn com fome baixa.**
  - Morra de Starved.
  - **Esperado:** renasce com fome 300 (**Famished**).
  - Se morrer com fome acima de 300 por outra causa, o valor é mantido.
- [ ] **FOM-17: save/load com Starved.**
  - Salve e saia estando em Starved, depois entre de novo.
  - **Esperado:** Starved some ao carregar e volta em ≤ 1 s. A fome continua 0.

---

## 3. Descanso

**Limiares**

| Status | Faixa | Efeito |
|---|---|---|
| Tired | ≤ 900 | vida e mana máximas ÷1,2 |
| Sleepy | ≤ 600 | vida e mana máximas ÷1,4 |
| Exhausted | ≤ 300 | vida e mana máximas ÷1,6; Confused aleatório |
| Desmaio | 0 | descanso volta a 300 e Unconscious por 10 s |

A vida e a mana máximas são arredondadas para baixo, para um múltiplo de 20.

**Perda:** 4 por segundo × Rest Multiplier, **apenas de noite** ou com Sleep Deprived. Com Cafeinated, a perda cai pela metade. **De dia, o descanso não muda.**

**Ganho** (não é afetado pelo multiplicador):

| Situação | Tempo normal | Tempo acelerado |
|---|---|---|
| Na cama, com ≥ 2 NPCs da cidade por perto | +4/s | +24/s |
| Na cama, com < 2 NPCs | +1/s + debuff Bad Rest | +12/s + Bad Rest |
| Sentado numa cadeira | +1/s + Bad Rest | +1/s + Bad Rest |

### Perda

- [ ] **DES-01: dia não cansa.**
  - Fique 2 min acordado de dia (tempo congelado no Journey).
  - **Esperado:** o medidor não muda.
- [ ] **DES-02: noite cansa.**
  - Noite, multiplicador 1, acordado.
  - **Esperado:** um degrau a cada ~25 s. Tired em ~75 s, Sleepy em ~150 s, Exhausted em ~225 s, desmaio em ~300 s.
- [ ] **DES-03: multiplicador 5.**
  - **Esperado:** Tired em ~15 s e desmaio em ~60 s.
- [ ] **DES-04: cafeína.**
  - Tome um Coffee Cup ou uma Caffeine Pill. Noite, multiplicador 1.
  - **Esperado:** Tired em ~150 s, o dobro do normal.
- [ ] **DES-05: vida e mana máximas.**
  - Confira a vida e a mana máximas exibidas em cada estágio:

    | Base | Tired | Sleepy | Exhausted |
    |---|---|---|---|
    | 100 de vida (P1) | 80 | 60 | 60 |
    | 400 de vida (P2) | 320 | 280 | 240 |
    | 500 de vida | 400 | 340 | 300 |
    | 200 de mana (P2) | 160 | 140 | 120 |
    | **20 de mana (P1)** | **0** | **0** | **0** |

  - 🔍 O caso de 20 de mana zerada é um limite herdado do original (as contas são iguais no `main`). Avalie se é desejado.
- [ ] **DES-06: Exhausted causa Confused.**
  - Fique ~1 min em Exhausted.
  - **Esperado:** Confused (2 s) aparece de vez em quando (~5% de chance por segundo) e nunca se acumula.

### Desmaio

- [ ] **DES-07: desmaio.**
  - Chegue a 0.
  - **Esperado:**
    - Unconscious por 10 s;
    - não anda, não pula, não usa gancho, montaria nem itens (nem comida);
    - fica translúcido;
    - aparece um "Zzz" animado sobre a cabeça;
    - depois, o status vira Exhausted (descanso 300).
- [ ] 🐞 **DES-08: transparência termina com o desmaio.**
  - Depois do desmaio, tome dano de um inimigo.
  - **Esperado:** o personagem volta a ficar opaco e **pisca normalmente** ao tomar dano. No `main`, o piscar era anulado.
- [ ] 🐞 **DES-09: apenas um "Zzz".**
  - Durante o desmaio, ande com a câmera e observe.
  - **Esperado:** um único efeito, que acompanha a cabeça e some ao fim do debuff.

### Dormir

- [ ] **DES-10: dormir bem.**
  - Durma numa cama dentro da cidade, com ≥ 2 NPCs.
  - **Esperado:**
    - +24/s se o tempo estiver acelerando (um degrau em ~4 s);
    - +4/s se não estiver (um degrau a cada 25 s);
    - sem Bad Rest.
- [ ] **DES-11: dormir mal.**
  - Durma numa cama longe de NPCs.
  - **Esperado:** aparece Bad Rest. O ganho fica em +12/s (acelerado) ou +1/s.
- [ ] **DES-12: sentar.**
  - Sente numa cadeira.
  - **Esperado:** +1/s (um degrau a cada 100 s) e Bad Rest, mesmo de noite.
- [ ] **DES-13: dormir de noite não cansa.**
  - **Esperado:** dormindo ou sentado, nunca há perda, mesmo à noite.

### Sleep Deprived ⏱️

O contador de horas acordado conta **tempo real**, de dia e de noite, e fica pausado durante o desmaio. Em 2400 s (40 min) aparece a mensagem azul e o debuff Sleep Deprived dura 10 min. Cada segundo dormindo tira ~29 s do contador.

- [ ] ⏱️ **DES-14: privação de sono.**
  - Jogue 40 min sem dormir nem sentar.
  - **Esperado:** a mensagem "Your sleep routine has been terrible…" e o debuff Sleep Deprived (10 min) aparecem.
- [ ] **DES-15: Sleep Deprived cansa de dia.**
  - Com o debuff ativo, fique acordado de dia.
  - **Esperado:** o descanso cai 4/s, como se fosse noite.
- [ ] ⏱️⚠️ **DES-16: cochilo curto não zera mais o contador.**
  - Com um personagem recém-descansado, fique ~35 min acordado, durma **5 s** e acorde.
  - **Esperado:** Sleep Deprived aparece **~7–8 min depois**.
  - No `main`, 1 s de sono já cortava o contador para no máximo 1200, e o debuff só viria ~20 min depois.
- [ ] ⏱️ **DES-17: sono longo zera.**
  - Depois de ~35 min acordado, durma **≥ 85 s** seguidos.
  - **Esperado:** é preciso mais 40 min completos até o próximo Sleep Deprived.
- [ ] 🐞 **DES-18: Sleep Deprived atravessa a morte uma vez só.**
  - Morra com o debuff: ele continua no respawn, com o tempo restante.
  - Deixe-o acabar e morra de novo.
  - **Esperado:** ele **não** volta. No `main`, voltava a cada respawn.
- [ ] **DES-19: a Nurse não remove** Sleep Deprived.
- [ ] **DES-20: save/load** mantém o descanso e o contador de horas acordado. Ex.: saia com 30 min acordado, volte e confira que o debuff vem ~10 min depois.

---

## 4. Sanidade

**Limiares**

| Status | Faixa | Efeito |
|---|---|---|
| Stressed | ≤ 900 | −10% de dano, −5% de crítico |
| Scared | ≤ 600 | −20% de dano, −15% de crítico; tela um pouco mais escura |
| Terrified | ≤ 300 | −35% de dano, −20% de crítico; tela mais escura; Flee ao ser atingido |
| Gloom | 0 | tela bem escura e contagem até a morte |

**Variação por segundo** (as perdas são multiplicadas pelo Sanity Multiplier; os ganhos, não):

| Condição | Normal | Com Armored Mind |
|---|---|---|
| Dungeon, Underworld, Corrupção ou Carmesim | −4 | −2,5 |
| Vida ≤ metade da vida máxima **efetiva** | −9 | −6 |
| Lua de Sangue ou Eclipse | −7 | −3,5 |
| Mais de 2 NPCs da cidade por perto | +9 | +9 |
| Altura da superfície | +2 | +2 |

### Variação

- [ ] **SAN-01: Underworld.**
  - Multiplicador 1, vida cheia.
  - **Esperado:** um degrau a cada ~25 s. Stressed em ~75 s, Scared em ~150 s, Terrified em ~225 s, Gloom em ~300 s.
- [ ] **SAN-02: escurecimento.**
  - **Esperado:** a tela escurece um pouco em Scared, mais em Terrified e bastante em Gloom.
  - Volta ao normal acima de 600.
- [ ] **SAN-03: Armored Mind.**
  - Fume um Cigarette (4 min) ou use uma Adrenaline Injection (8 min) e repita o SAN-01.
  - **Esperado:** um degrau a cada ~40 s.
- [ ] **SAN-04: o multiplicador afeta só as perdas.**
  - Multiplicador 5 na cidade de dia (+11/s).
  - **Esperado:** sobe na mesma velocidade que com o multiplicador 1, um degrau a cada ~9 s.
- [ ] **SAN-05: Corrupção na superfície.**
  - **Esperado:** saldo de −2/s (−4 da Corrupção + 2 da superfície), ou seja, um degrau a cada 50 s.
- [ ] **SAN-06: Lua de Sangue** (Journey ou Bloody Tear), na superfície longe da cidade.
  - **Esperado:** saldo de −5/s.
- [ ] ⚠️ **SAN-07: "vida baixa" usa a vida máxima efetiva.**
  - P2 (400 de vida) de dia, na superfície, longe de NPCs: o saldo base é +2/s.
  - **Com Lifeforce Potion** (máximo de 480), fique com vida entre **201 e 240**.
    - **Esperado:** a sanidade **cai** (−7/s).
    - No `main`, subia (+2/s), porque o limiar era 200.
  - **Com Tired** (máximo de 320), fique com vida entre **161 e 200**.
    - **Esperado:** a sanidade **sobe**.
    - No `main`, caía.

### Gloom (desligue o God Mode)

- [ ] **SAN-08: contagem e morte.**
  - Chegue a 0.
  - **Esperado:**
    - mensagem vermelha "You feel that something terrible is about to happen";
    - **4 s sem nada**;
    - "5...", "4...", "3...", "2...", "1..." (um por segundo);
    - morte com a mensagem "**<nome> got obliterated**".
- [ ] 🐞 **SAN-09: escapar e voltar recomeça a contagem.**
  - No Underworld, em 0, quando aparecer "3...", use Magic Mirror ou Recall Potion para ir à cidade.
  - **Esperado:** "You got lucky this time".
  - Volte ao Underworld até zerar de novo.
  - **Esperado:** o **aviso aparece outra vez** e a contagem recomeça do zero (4 s de silêncio e depois 5..1).
  - No `main`, continuava de "2..." e sem aviso.
- [ ] 🐞 **SAN-10: morrer por outra causa durante o Gloom.**
  - Em contagem, morra na lava.
  - **Esperado:** renasce com sanidade cheia. O próximo Gloom começa do zero, com aviso.
- [ ] **SAN-11: respawn** sempre com sanidade cheia.

### Flee

- [ ] **SAN-12: fuga quando aterrorizado.**
  - Em Terrified, deixe-se atingir várias vezes.
  - **Esperado:**
    - às vezes (~20%), ganha Flee por 2 s;
    - corre para o lado **oposto** ao golpe, sem controle de esquerda/direita;
    - não recebe um novo Flee enquanto o atual estiver ativo.

---

## 5. Temperatura

**Limiares**

| Status | Faixa | Efeito |
|---|---|---|
| Freezing | ≤ 200 | Frostburn aleatório; Frozen aleatório (15%/s, 2 s) |
| Very Cold | ≤ 500 | Frostburn aleatório (5%/s) |
| Cold | ≤ 800 | |
| Confortável | 801 a 1199 | |
| Warm | ≥ 1200 | |
| Hot | ≥ 1500 | On Fire aleatório |
| Scorching | ≥ 1800 | **15 de dano por segundo** e On Fire aleatório |

**Variação por segundo** (multiplicada pelo Warmth Multiplier):

| Fontes de frio | Por segundo |
|---|---|
| Noite na superfície | −3 |
| Neve | −8 |
| Chuva na superfície | −3 |
| Chuva forte na superfície | −5 |

| Fontes de calor | Por segundo |
|---|---|
| Praia de dia | +3 |
| Deserto de dia | +5 |
| Underworld | +8 |
| Espaço | +30 |

**Recuperação e normalização**

| Situação | Efeito por segundo |
|---|---|
| Ambiente frio (inclui estar na água) com temperatura > 1000 | −20 extra |
| Ambiente quente (inclui fonte de calor a até 15 blocos) com temperatura < 1000 | +20 extra |
| Nenhuma fonte de calor ou frio | volta 10 em direção a 1000 |

Fontes de calor: Furnace, Hellforge, Adamantite/Titanium Forge, Fireplace, Glass Kiln, Lihzahrd Furnace e Campfire (qualquer modelo).

### Variação

- [ ] **TMP-01: neve de dia.**
  - Multiplicador 1.
  - **Esperado:** Cold em ~25 s, Very Cold em ~63 s, Freezing em ~100 s. O mínimo (0) chega em ~125 s.
- [ ] **TMP-02: neve à noite.**
  - **Esperado:** −11/s, com Cold em ~18 s.
- [ ] **TMP-03: chuva.** Use o controle de clima do Journey.
  - **Esperado:** chuva normal −3/s e chuva forte −5/s, só na superfície.
  - No subsolo, a chuva não afeta.
- [ ] **TMP-04: espaço.**
  - **Esperado:** Warm em ~7 s, Hot em ~17 s, Scorching em ~27 s. Depois toma 15 de dano por segundo.
  - Desligue o God Mode e morra.
  - **Esperado:** mensagem "**<nome> burned to death**". No `main`, era só "Burned to death".
- [ ] **TMP-05: Underworld.**
  - **Esperado:** Warm em ~25 s, Hot em ~63 s, Scorching em ~100 s.
- [ ] **TMP-06: normalização.**
  - Saia do frio com temperatura 0 e vá para um lugar neutro (subsolo, sem fontes).
  - **Esperado:** volta a 1000 em ~100 s e para exatamente lá. O sprite base, sem seta, aparece ao estabilizar.
- [ ] **TMP-07: fogueira na neve.**
  - Fique a até 15 blocos de uma Campfire, na neve, partindo do frio.
  - **Esperado:** esquenta +12/s até ~1000 e fica oscilando perto de confortável. Nunca passa para Warm.
- [ ] **TMP-08: fogueira não esquenta acima de 1000.**
  - **Esperado:** perto de uma Campfire num lugar neutro, a temperatura não sobe acima de confortável.
- [ ] **TMP-09: água esfria quem está quente.**
  - Aquecido (> 1000), entre na água.
  - **Esperado:** −20/s até ~1000.
  - Com temperatura < 1000, a água não esfria mais. Ela só normaliza.
- [ ] **TMP-10: setas do medidor.**
  - **Esperado:**
    - seta para cima enquanto esquenta;
    - seta para baixo enquanto esfria;
    - parado fora do confortável, a seta aponta para o confortável.

### Multiplicador (foi corrigido)

- [ ] ⚠️🐞 **TMP-11: multiplicador 0,5.**
  - **Esperado:** na neve de dia, −4/s, com Cold em ~50 s.
  - No `main`, valores entre 0,1 e 0,99 **nunca** mudavam a temperatura.
- [ ] ⚠️🐞 **TMP-12: multiplicador 0,1.**
  - **Esperado:** na neve, −0,8/s, com Cold em ~250 s. A temperatura **não volta sozinha** para confortável enquanto estiver na neve.
- [ ] ⚠️🐞 **TMP-13: multiplicador 1,5.**
  - **Esperado:** na neve, −12/s, com Cold em ~17 s. No `main`, funcionava como 1.
- [ ] **TMP-14: o multiplicador não afeta a normalização.**
  - Com 0,1, saia do frio.
  - **Esperado:** volta a 10/s, igual ao multiplicador 1.

### Poções

- [ ] ⚠️ **TMP-15: Warmth Potion reduz o frio pela metade.**
  - Repita o TMP-01 com a poção.
  - **Esperado:** −4/s, com Cold em ~50 s, Very Cold em ~125 s e Freezing em ~200 s.
  - Repita o TMP-09 com a poção. **Esperado:** continua −20/s (a poção não mexe na recuperação).
- [ ] ⚠️ **TMP-16: Obsidian Skin reduz o calor pela metade.**
  - No inferno, partindo de 1000, com a poção.
  - **Esperado:** +4/s em vez de +8/s.
  - Repita o TMP-07 com a poção. **Esperado:** continua +12/s (a poção não afeta o frio da neve nem a fogueira).

### Respawn e save

- [ ] **TMP-18:** morrer **zera** a temperatura para 1000.
- [ ] **TMP-19:** save/load mantém a temperatura.

---

## 6. Itens, lojas e drops

- [ ] **ITM-01: Cigarette.**
  - **Esperado:**
    - animação de 4 s, com o cigarro na boca e som próprio;
    - Armored Mind por 4 min;
    - consome 1 unidade.
- [ ] 🔍 **ITM-02: tooltip do Cigarette.**
  - Verifique se ainda aparece a linha de duração ("4 minute duration").
  - O `Item.buffTime` foi removido do cigarro, então ela deve ter sumido. Decida se era desejada.
- [ ] **ITM-03: câncer no 50º cigarro.**
  - **Esperado:** no 50º cigarro, aparece a mensagem "You haven't been smoking in moderation, have you?" e o debuff Cancer (50 min, efeitos pesados). O contador volta a 0.
- [ ] 🐞 **ITM-04: contador por jogador.**
  - Fume 20 cigarros de uma pilha e acabe com ela. Fume 10 de outra pilha, comprada ou dividida.
  - Salve, saia e entre de novo. Fume mais 20.
  - **Esperado:** Cancer aparece no 50º no total.
  - No `main`, o contador era perdido quando a pilha acabava ou era dividida.
- [ ] 🐞 **ITM-05: câncer e morte.**
  - Morra com Cancer: ele continua.
  - Vá à Nurse e cure.
  - **Esperado:** o preço tem **+2 platinas**.
  - Morra de novo.
  - **Esperado:** Cancer **não** volta. No `main`, voltava a cada respawn.
- [ ] **ITM-06: outros consumíveis.**
  - **Esperado:**

    | Item | Buff |
    |---|---|
    | Vitamin | Nourished 4 min |
    | Food Pill | Nourished 8 min |
    | Caffeine Pill | Cafeinated 8 min |
    | Adrenaline Injection | Armored Mind 8 min |

  - As tooltips dizem "Slows …".
- [ ] **ITM-07: Arms Dealer** vende Cigarette por 3 ouro e 50 prata.
- [ ] **ITM-08: Traveling Merchant.** Faça vários dias.
  - **Esperado:**
    - sempre **2 itens diferentes** do mod;
    - pré-Hardmode: Cigarette, Vitamin ou Coffee Cup;
    - Hardmode: inclui também Caffeine Pill, Food Pill e Adrenaline Injection;
    - nunca repete o mesmo item.
- [ ] **ITM-09: drops** (é demorado; dá para usar spawn rate alto ou estatuetas):

  | Inimigo | Drops |
  |---|---|
  | Skeleton Commando | Cigarette 1/30, Adrenaline 1/30 |
  | Undead Viking, Snow Flinx | Cigarette 1/50 |
  | Armored Viking | Cigarette 1/50, Adrenaline 1/50 |
  | Necromancer | Vitamin 1/30, Food Pill 1/30 |
  | Desert Ghoul | Vitamin 1/50, Food Pill 1/50 |
  | Antlions (andador e voador) | Vitamin 1/50 |
  | Angry Trapper | Caffeine Pill 1/50 |
  | Bone Lee | Caffeine Pill 1/30 |

  Confira também no Bestiário.
- [ ] **ITM-10: itens iniciais.** Um personagem novo começa com **5 Apples**.

---

## 7. HUD (medidores)

- [ ] **HUD-01: posição padrão.** Quatro medidores no topo, nesta ordem: temperatura, sanidade, descanso e fome.
- [ ] 🐞 **HUD-02: termômetro único.** O termômetro não aparece duplicado nem com "fantasma". No `main`, era desenhado duas vezes.
- [ ] **HUD-03: offset em tempo real.**
  - Mude X Offset e Y Offset com o jogo aberto.
  - **Esperado:** os quatro medidores se movem juntos.
  - Teste os limites 0 e 1200.
- [ ] 🔍 **HUD-04: camada da fome.**
  - Abra o inventário e mova os medidores (offset) para cima dele.
  - **Esperado:** a fome agora fica **na frente** do inventário, como os outros. No `main`, ficava atrás.
- [ ] **HUD-05: tooltips.**
  - Passe o mouse sobre cada medidor em cada status.
  - **Esperado:**
    - título, estado e dica;
    - sem dica quando está tudo bem;
    - o texto bate com o debuff ativo.
  - Casos especiais:
    - descanso 0 mostra "exhausted";
    - sanidade 0 mostra "terrified";
    - a temperatura diz "comfortable", sem o erro de digitação antigo "confortable".
- [ ] **HUD-06: medidores escondidos** enquanto o personagem está morto.
- [ ] **HUD-07: escala da interface.**
  - Mude a UI Scale do Terraria.
  - **Esperado:** os medidores acompanham e o hover continua acertando.

---

## 8. Idiomas

- [ ] **LOC-01: português (Brasil).** Troque o idioma do jogo e revise:
  - nomes e descrições de todos os buffs;
  - itens e tooltips (inclusive Coffee Cup e poções);
  - config;
  - tooltips dos medidores;
  - mensagens de chat (Sleep Deprived, Gloom, câncer);
  - mensagens de morte.
- [ ] **LOC-02: textos faltando.** Nenhum texto aparece como chave crua (ex.: `Mods.ChallengingTerrariaMod.UI.Hunger.Title`) nos dois idiomas.
- [ ] **LOC-03: Warm vs. Cold.** A descrição de Warm agora diz "slightly", como a de Cold. Os efeitos são iguais.

---

## 9. Multiplayer

Precisa de dois clientes (você e um amigo) ou de um cliente com servidor dedicado.

- [ ] **MP-01: servidor dedicado.**
  - Inicie um servidor dedicado com o mod.
  - **Esperado:** sobe sem erros, e um cliente entra normalmente.
- [ ] 🐞 **MP-02: medidores independentes.**
  - Jogador A na neve à noite, jogador B na cidade de dia.
  - **Esperado:** cada um vê apenas os próprios medidores mudando, com as taxas das seções acima. **Nada anda em dobro.**
- [ ] 🐞 **MP-03: mensagens só para o jogador afetado.**
  - Gloom, Sleep Deprived e câncer de A.
  - **Esperado:** só A vê as mensagens de contagem e de aviso. Os dois veem a mensagem de morte.
- [ ] **MP-04: debuffs visíveis para o outro.**
  - B vê os debuffs de A, por exemplo ao passar o mouse sobre A, se houver suporte.
  - B vê também o "Zzz" quando A desmaia.
- [ ] 🐞 **MP-05: dano do Scorching visível.**
  - **Esperado:** B vê os números de dano de A em Scorching. No `main`, o dano era "silencioso" e só aparecia para A.
- [ ] **MP-06: movimento forçado.**
  - Flee e Throwing Up de A aparecem suaves para B, sem teleportes ou tremidas.
- [ ] **MP-07: config de servidor.**
  - Altere os multiplicadores como host.
  - **Esperado:** valem para todos os jogadores.
- [ ] **MP-08: Nurse e câncer** funcionam para um cliente que não é o host.
- [ ] **MP-09: idiomas diferentes.**
  - Clientes em idiomas diferentes.
  - **Esperado:** cada um vê a mensagem de morte (Gloom e Scorching) **no próprio idioma**.

---

## Resumo dos pontos ⚠️ para decidir

| ID | Mudança em relação ao `main` |
|---|---|
| FOM-01 | Fome passa a cair parado (1/s) |
| FOM-08 | Restauração depende da comida comida, não do melhor buff ativo |
| DES-16 | Cochilo curto não zera mais metade do contador de Sleep Deprived |
| SAN-07 | "Vida baixa" usa a vida máxima efetiva (interage com descanso e poções de vida) |
| TMP-11 a TMP-13 | Multiplicador de temperatura agora funciona com valores fracionários |
| TMP-15 e TMP-16 | Warmth Potion e Obsidian Skin agora reduzem pela metade o frio e o calor dos biomas |
| ITM-02 | Linha de duração possivelmente removida da tooltip do Cigarette |
| HUD-04 | Medidor de fome agora fica na frente do inventário |
