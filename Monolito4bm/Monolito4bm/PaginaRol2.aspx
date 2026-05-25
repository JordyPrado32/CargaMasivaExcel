<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaginaRol2.aspx.cs" Inherits="Monolito4bm.PaginaRol2" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Juego Star Dash</title>

  <link href="https://fonts.googleapis.com/css2?family=Orbitron:wght@400;700;900&family=Exo+2:wght@300;600&display=swap" rel="stylesheet"/>
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

  <style>
    :root {
      --rosa:   #ff8da1;
      --morado: #e0c3fc;
      --glass:  rgba(255,255,255,0.65);
      --borde:  rgba(255,255,255,0.8);
      --text:   #5d3f6a;
      --muted:  #777;
    }

    * { box-sizing: border-box; margin: 0; padding: 0; }

    body {
      background: linear-gradient(135deg, #e6e6fa 0%, #d8bfd8 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: var(--text);
      min-height: 100vh;
      overflow-x: hidden;
    }

    #app {
      position: relative; z-index: 1;
      max-width: 1100px;
      margin: 0 auto;
      padding: 20px;
    }

    header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px 24px;
      background: var(--glass);
      border: 1px solid var(--borde);
      border-radius: 16px;
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      margin-bottom: 24px;
      box-shadow: 0 10px 30px rgba(0,0,0,0.1);
    }
    header h1 {
      font-family: 'Orbitron', monospace;
      font-size: 1.6rem;
      font-weight: 900;
      color: var(--text);
      letter-spacing: 2px;
    }
    .header-stats { display: flex; gap: 20px; }
    .stat-pill {
      background: rgba(255,255,255,0.8);
      border: 2px solid #ffb6c1;
      border-radius: 20px;
      padding: 6px 16px;
      font-size: .85rem;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 6px;
    }
    .stat-pill span { color: var(--rosa); font-family: 'Orbitron', monospace; }

    #selectorNiveles {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }
    .nivel-card {
      background: rgba(255,255,255,0.8);
      border: 2px solid var(--borde);
      border-radius: 14px;
      padding: 20px;
      text-align: center;
      cursor: pointer;
      transition: all .3s ease;
      position: relative;
      overflow: hidden;
      box-shadow: 0 5px 15px rgba(0,0,0,0.05);
    }
    .nivel-card:hover:not(.bloqueado) {
      border-color: var(--rosa);
      transform: translateY(-4px);
      box-shadow: 0 10px 30px rgba(255,141,161,0.2);
    }
    .nivel-card.activo {
      border-color: var(--rosa);
      background: rgba(255,141,161,0.15);
    }
    .nivel-card.bloqueado {
      opacity: .5;
      cursor: not-allowed;
      background: rgba(230,230,230,0.8);
    }
    .nivel-card .nivel-titulo {
      font-family: 'Orbitron', monospace;
      font-size: 1rem;
      font-weight: 700;
      color: var(--text);
      margin-bottom: 4px;
    }
    .nivel-card .nivel-sub { font-size: .85rem; color: var(--muted); }
    .nivel-card .lock-icon {
      position: absolute;
      top: 50%; left: 50%;
      transform: translate(-50%,-50%);
      font-size: 1.2rem;
      color: #999;
      font-weight: bold;
    }
    .nivel-badge {
      position: absolute;
      top: 8px; right: 8px;
      background: var(--rosa);
      color: #fff;
      font-size: .65rem;
      font-weight: 700;
      padding: 4px 10px;
      border-radius: 10px;
      font-family: 'Orbitron', monospace;
    }

    #juegoWrapper {
      background: var(--glass);
      border: 1px solid var(--borde);
      border-radius: 16px;
      overflow: hidden;
      margin-bottom: 24px;
      box-shadow: 0 15px 40px rgba(0,0,0,0.1);
    }
    #juegoHeader {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 20px;
      background: rgba(255,255,255,0.7);
      border-bottom: 1px solid var(--borde);
    }
    #juegoHeader .hud {
      display: flex;
      gap: 24px;
      font-family: 'Orbitron', monospace;
      font-size: .85rem;
    }
    .hud-item { color: var(--text); font-weight: 600;}
    .hud-item b { color: var(--rosa); font-size: 1.1rem; }
    #btnMusica {
      background: rgba(255,141,161,0.15);
      border: 1px solid rgba(255,141,161,0.3);
      border-radius: 20px;
      color: var(--rosa);
      padding: 6px 14px;
      cursor: pointer;
      font-size: .8rem;
      font-weight: bold;
      transition: all .2s;
    }
    #btnMusica:hover { background: rgba(255,141,161,0.3); }

    canvas#gameCanvas {
      display: block;
      width: 100%;
      image-rendering: pixelated;
    }

    #pantallaOverlay { position: relative; }
    #pantallaInicio, #pantallaFin {
      position: absolute;
      inset: 0;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      background: rgba(255,255,255,0.9);
      backdrop-filter: blur(5px);
      z-index: 10;
      gap: 16px;
      padding: 30px;
    }
    #pantallaFin { display: none; }

    .overlay-titulo {
      font-family: 'Orbitron', monospace;
      font-size: 2rem;
      font-weight: 900;
      color: var(--text);
      text-align: center;
    }
    .overlay-sub {
      color: var(--muted);
      font-size: .95rem;
      text-align: center;
      font-weight: 600;
    }
    .btn-juego {
      background: #ff8da1;
      color: #fff;
      border: none;
      border-radius: 12px;
      padding: 14px 36px;
      font-family: 'Orbitron', monospace;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      transition: all .2s;
      box-shadow: 0 4px 20px rgba(255,141,161,.35);
      letter-spacing: 1px;
    }
    .btn-juego:hover { transform: translateY(-2px); box-shadow: 0 8px 30px rgba(255,141,161,.5); }
    
    .fin-stats { display: flex; gap: 24px; flex-wrap: wrap; justify-content: center; }
    .fin-stat {
      background: #fff;
      border: 2px solid #ffb6c1;
      border-radius: 10px;
      padding: 12px 20px;
      text-align: center;
      box-shadow: 0 4px 10px rgba(0,0,0,0.05);
    }
    .fin-stat .val { font-family: 'Orbitron', monospace; font-size: 1.4rem; color: var(--rosa); font-weight: 700; }
    .fin-stat .lbl { font-size: .75rem; color: var(--muted); margin-top: 4px; font-weight: bold; }

    #instrucciones {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 10px;
      margin-bottom: 24px;
    }
    .inst-item {
      background: var(--glass);
      border: 1px solid var(--borde);
      border-radius: 10px;
      padding: 12px;
      text-align: center;
      font-size: .85rem;
      font-weight: 600;
      color: var(--text);
    }
    .inst-item .key {
      background: #fff;
      border: 1px solid #ffb6c1;
      border-radius: 6px;
      padding: 4px 10px;
      font-family: 'Orbitron', monospace;
      font-size: .85rem;
      color: var(--rosa);
      display: inline-block;
      margin-bottom: 6px;
    }

    #rankingPanel {
      background: var(--glass);
      border: 1px solid var(--borde);
      border-radius: 16px;
      padding: 25px;
      box-shadow: 0 10px 30px rgba(0,0,0,0.05);
    }
    #rankingPanel h3 {
      font-family: 'Orbitron', monospace;
      color: var(--text);
      margin-bottom: 16px;
      font-size: 1.2rem;
    }
    #tablaRanking { width: 100%; border-collapse: collapse; font-size: .9rem; background: #fff; border-radius: 10px; overflow: hidden; }
    #tablaRanking th {
      background: #e0c3fc;
      color: var(--text);
      font-weight: 700;
      padding: 12px;
      text-align: left;
      font-family: 'Orbitron', monospace;
      font-size: .8rem;
    }
    #tablaRanking td {
      padding: 12px;
      border-bottom: 1px solid #f0e6ff;
      color: var(--muted);
      font-weight: 600;
    }
    #tablaRanking tr:hover { background: #fdf5ff; }

    @media (max-width: 700px) {
      #selectorNiveles { grid-template-columns: 1fr; }
      #instrucciones   { grid-template-columns: repeat(2,1fr); }
      header h1        { font-size: 1.1rem; }
    }
    .btn-salir {
      background: rgba(255,255,255,0.8);
      border: 2px solid #e74c3c;
      color: #e74c3c;
      border-radius: 20px;
      padding: 6px 16px;
      font-size: .85rem;
      font-weight: 700;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 8px;
      transition: all 0.2s;
      text-decoration: none;
    }
    .btn-salir:hover {
      background: #e74c3c;
      color: white;
    }
  </style>
</head>
<body>

<form id="frmJuego" runat="server">
  <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
  
  <asp:UpdatePanel ID="upPartida" runat="server">
      <ContentTemplate>
          <asp:HiddenField ID="hdnNivel"      runat="server" ClientIDMode="Static"/>
          <asp:HiddenField ID="hdnPuntuacion" runat="server" ClientIDMode="Static"/>
          <asp:HiddenField ID="hdnMonedas"    runat="server" ClientIDMode="Static"/>
          <asp:Button ID="btnGuardarPartida" runat="server" Text="g"
                      ClientIDMode="Static" OnClick="btnGuardarPartida_Click"
                      style="display:none"/>
      </ContentTemplate>
  </asp:UpdatePanel>

  <div id="app">

    <header>
      <h1>STAR DASH</h1>
      <div class="header-stats">
        <div class="stat-pill">Record: <span id="statRecord">0</span></div>
        <div class="stat-pill">Monedas: <span id="statMonedas">0</span></div>
        <div class="stat-pill">Nivel: <span id="statNivel">1</span></div>
          <asp:LinkButton ID="btnCerrarSesion" runat="server" CssClass="btn-salir" OnClick="btnCerrarSesion_Click" CausesValidation="false">
              <i class="fa-solid fa-right-from-bracket"></i> Salir
          </asp:LinkButton>
      </div>
    </header>

    <div id="selectorNiveles">
      <div class="nivel-card activo" id="cardNivel1" onclick="seleccionarNivel(1)">
        <div class="nivel-titulo">NIVEL 1</div>
        <div class="nivel-sub">Zona de juego</div>
        <div class="nivel-badge">500 pts</div>
      </div>
      <div class="nivel-card bloqueado" id="cardNivel2" onclick="seleccionarNivel(2)">
        <div class="nivel-titulo">NIVEL 2</div>
        <div class="nivel-sub">Abismo Neon</div>
        <div class="nivel-badge">1100 pts</div>
        <div class="lock-icon">Bloqueado</div>
      </div>
      <div class="nivel-card bloqueado" id="cardNivel3" onclick="seleccionarNivel(3)">
        <div class="nivel-titulo">NIVEL 3</div>
        <div class="nivel-sub">Caos Final</div>
        <div class="nivel-badge">2400 pts</div>
        <div class="lock-icon">Bloqueado</div>
      </div>
    </div>

    <div id="juegoWrapper">
      <div id="juegoHeader">
        <div class="hud">
          <div class="hud-item">PUNTOS <b id="hudPuntos">0</b></div>
          <div class="hud-item">MONEDAS <b id="hudMonedas">0</b></div>
          <div class="hud-item">PROGRESO <b id="hudProgreso">0%</b></div>
        </div>
        <button id="btnMusica" onclick="toggleMusica()" type="button">Musica OFF</button>
      </div>
      <div id="pantallaOverlay" style="position:relative">
        <canvas id="gameCanvas" width="900" height="400"></canvas>

        <div id="pantallaInicio">
          <div class="overlay-titulo" id="tituloInicio">ZONA ESTELAR</div>
          <div class="overlay-sub">Presiona ESPACIO o toca la pantalla para saltar.<br/>Evita pinchos, no caigas en vacios, supera los bloques.</div>
          <div class="fin-stats">
            <div class="fin-stat">
              <div class="val" id="inicioMeta">400</div>
              <div class="lbl">Puntos para pasar</div>
            </div>
          </div>
          <button class="btn-juego" onclick="iniciarJuego()" type="button">JUGAR</button>
        </div>

        <div id="pantallaFin">
          <div class="overlay-titulo" id="finTitulo">GAME OVER</div>
          <div class="fin-stats">
            <div class="fin-stat">
              <div class="val" id="finPuntos">0</div>
              <div class="lbl">Puntuacion</div>
            </div>
            <div class="fin-stat">
              <div class="val" id="finMonedas">0</div>
              <div class="lbl">Monedas</div>
            </div>
            <div class="fin-stat">
              <div class="val" id="finProgreso">0%</div>
              <div class="lbl">Progreso</div>
            </div>
          </div>
          <button class="btn-juego" onclick="reiniciarJuego()" type="button">REINTENTAR</button>
          <button class="btn-juego" style="background:rgba(255,141,161,.15);border:2px solid var(--rosa);color:var(--rosa);box-shadow:none" onclick="volverMenu()" type="button">MENU PRINCIPAL</button>
        </div>
      </div>
    </div>

    <div id="instrucciones">
      <div class="inst-item"><div class="key">ESPACIO</div><br/>Saltar</div>
      <div class="inst-item"><div class="key">TAP</div><br/>Saltar</div>
      <div class="inst-item"><div class="key">ESC</div><br/>Pausar</div>
    </div>

    <div id="rankingPanel">
      <h3>RANKING GLOBAL</h3>
      <table id="tablaRanking">
        <thead>
          <tr>
            <th>#</th><th>JUGADOR</th><th>PUNTUACION</th><th>NIVEL</th><th>MONEDAS</th>
          </tr>
        </thead>
        <tbody id="tbodyRanking">
          <tr><td colspan="5" style="color:var(--muted);text-align:center;padding:20px">Cargando ranking...</td></tr>
        </tbody>
      </table>
    </div>

  </div>
</form>

<script>
    const PROGRESO = <%=ProgresoJson%>;
    const CONFIG_N1 = <%=ConfigNivel1Json%>;
    const CONFIG_N2 = <%=ConfigNivel2Json%>;
    const CONFIG_N3 = <%=ConfigNivel3Json%>;
    let RANKING_DATA = <%=RankingJson%>; // Se actualiza dinámicamente con AJAX
    const CONFIGS = { 1: CONFIG_N1, 2: CONFIG_N2, 3: CONFIG_N3 };

    let audioCtx = null, musicaActiva = false, oscArray = [];

    function initAudio() {
        if (audioCtx) return;
        audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    }

    function playChiptune() {
        if (!audioCtx) return;
        stopChiptune();

        const notas = [523, 659, 784, 1047, 784, 659, 880, 1047,
            523, 659, 784, 1047, 698, 880, 1047, 880];
        let t = audioCtx.currentTime;

        notas.forEach((freq, i) => {
            const osc = audioCtx.createOscillator();
            const gain = audioCtx.createGain();
            osc.connect(gain);
            gain.connect(audioCtx.destination);
            osc.type = 'square';
            osc.frequency.value = freq;
            gain.gain.setValueAtTime(0.04, t + i * 0.15);
            gain.gain.setValueAtTime(0, t + i * 0.15 + 0.12);
            osc.start(t + i * 0.15);
            osc.stop(t + i * 0.15 + 0.13);
            oscArray.push(osc);
        });

        setTimeout(() => { if (musicaActiva) playChiptune(); }, notas.length * 150 + 200);
    }

    function stopChiptune() {
        oscArray.forEach(o => { try { o.stop(); } catch (e) { } });
        oscArray = [];
    }

    function toggleMusica() {
        initAudio();
        musicaActiva = !musicaActiva;
        document.getElementById('btnMusica').textContent = musicaActiva ? 'Musica ON' : 'Musica OFF';
        if (musicaActiva) playChiptune();
        else stopChiptune();
    }

    const canvas = document.getElementById('gameCanvas');
    const ctx = canvas.getContext('2d');

    const W = canvas.width;
    const H = canvas.height;
    const SUELO_Y = H - 60;
    const JUGADOR_W = 36;
    const JUGADOR_H = 36;
    const GRAVEDAD = 0.55;
    const FUERZA_SALTO = -13;

    let estado = 'menu';
    let nivelActual = 1;
    let configActual = CONFIG_N1;
    let camX = 0;
    let puntuacion = 0;
    let monedasPartida = 0;
    let frameCount = 0;
    let animFrame = null;

    let jugador = {};
    let obstaculos = [];
    let monedas = [];
    let particulas = [];

    function inicializarEstado() {
        actualizarUI();
        desbloquearNiveles(PROGRESO.nivelDesbloqueado);
    }

    function desbloquearNiveles(maxNivel) {
        for (let n = 1; n <= 3; n++) {
            const card = document.getElementById('cardNivel' + n);
            if (n <= maxNivel) {
                card.classList.remove('bloqueado');
                const lock = card.querySelector('.lock-icon');
                if (lock) lock.remove();
            }
        }
    }

    function actualizarUI() {
        document.getElementById('statRecord').textContent = PROGRESO.mejorPuntuacion.toLocaleString();
        document.getElementById('statMonedas').textContent = PROGRESO.monedasTotales.toLocaleString();
        document.getElementById('statNivel').textContent = PROGRESO.nivelDesbloqueado;
    }

    function seleccionarNivel(n) {
        if (n > PROGRESO.nivelDesbloqueado) {
            Swal.fire({
                icon: 'warning',
                title: 'Bloqueado',
                text: `Completa el nivel ${n - 1} con ${CONFIGS[n - 1].puntajeMinimo} puntos para desbloquear este nivel.`,
                confirmButtonColor: '#ff8da1'
            });
            return;
        }
        nivelActual = n;
        configActual = CONFIGS[n];

        document.querySelectorAll('.nivel-card').forEach((c, i) => {
            c.classList.toggle('activo', i + 1 === n);
        });

        document.getElementById('tituloInicio').textContent = configActual.nombre.toUpperCase();
        document.getElementById('inicioMeta').textContent = configActual.puntajeMinimo;

        mostrarPantalla('inicio');
    }

    function construirNivel(cfg) {
        obstaculos = [];
        monedas = [];
        cfg.obstaculos.forEach(o => { obstaculos.push({ ...o }); });
        cfg.monedas.forEach(m => { monedas.push({ x: m.x, y: m.y, recogida: false, anim: 0 }); });
    }

    function resetJugador() {
        jugador = {
            x: 80, y: SUELO_Y - JUGADOR_H, vy: 0,
            enSuelo: true, vivo: true, saltando: false, angulo: 0
        };
    }

    function saltar() {
        if (estado !== 'jugando') return;
        if (jugador.enSuelo) {
            jugador.vy = FUERZA_SALTO;
            jugador.enSuelo = false;
            jugador.saltando = true;
            spawnParticulas(jugador.x + JUGADOR_W / 2, jugador.y + JUGADOR_H, 5, configActual.colorJugador);
        }
    }

    document.addEventListener('keydown', e => {
        if (e.code === 'Space' || e.code === 'ArrowUp' || e.key === 'w' || e.key === 'W') {
            e.preventDefault();
            if (estado === 'menu' || estado === 'fin') return;
            saltar();
        }
        if (e.code === 'Escape') {
            if (estado === 'jugando') pausar();
            else if (estado === 'pausa') reanudar();
        }
    });

    canvas.addEventListener('click', () => { if (estado === 'jugando') saltar(); });
    canvas.addEventListener('touchstart', e => {
        e.preventDefault();
        if (estado === 'jugando') saltar();
    }, { passive: false });

    function iniciarJuego() {
        estado = 'jugando';
        camX = 0;
        puntuacion = 0;
        monedasPartida = 0;
        frameCount = 0;
        particulas = [];

        construirNivel(configActual);
        resetJugador();
        mostrarPantalla('ninguna');

        if (animFrame) cancelAnimationFrame(animFrame);
        loop();
    }

    function reiniciarJuego() { iniciarJuego(); }
    function volverMenu() {
        estado = 'menu';
        if (animFrame) cancelAnimationFrame(animFrame);
        mostrarPantalla('inicio');
    }
    function pausar() {
        estado = 'pausa';
        if (animFrame) cancelAnimationFrame(animFrame);
        dibujarPausa();
    }
    function reanudar() { estado = 'jugando'; loop(); }

    function mostrarPantalla(cual) {
        document.getElementById('pantallaInicio').style.display = cual === 'inicio' ? 'flex' : 'none';
        document.getElementById('pantallaFin').style.display = cual === 'fin' ? 'flex' : 'none';
    }

    function loop() {
        if (estado !== 'jugando') return;
        actualizar();
        dibujar();
        animFrame = requestAnimationFrame(loop);
    }

    function actualizar() {
        frameCount++;
        camX += configActual.velocidad;
        puntuacion = Math.floor(camX / 8);

        document.getElementById('hudPuntos').textContent = puntuacion;
        document.getElementById('hudMonedas').textContent = monedasPartida;

        const meta = obstaculos.find(o => o.tipo === 'meta');
        if (meta) {
            const pct = Math.min(100, Math.floor((camX / (meta.x - 200)) * 100));
            document.getElementById('hudProgreso').textContent = pct + '%';
        }

        jugador.vy += GRAVEDAD;
        jugador.y += jugador.vy;
        jugador.angulo += 4;

        if (jugador.y + JUGADOR_H >= SUELO_Y) {
            jugador.y = SUELO_Y - JUGADOR_H;
            jugador.vy = 0;
            jugador.enSuelo = true;
            jugador.saltando = false;
            jugador.angulo = 0;
        }

        const jugX = jugador.x;
        const jugY = jugador.y;

        for (let o of obstaculos) {
            const ox = o.x - camX + 200;

            if (o.tipo === 'spike') {
                if (rectOverlap(jugX + 4, jugY + 4, JUGADOR_W - 8, JUGADOR_H - 4, ox + 4, SUELO_Y - 38, 30, 38)) {
                    morir(); return;
                }
            }
            if (o.tipo === 'bloque') {
                const bH = o.h || 80;
                const bY = SUELO_Y - bH;
                if (rectOverlap(jugX + 2, jugY + 2, JUGADOR_W - 4, JUGADOR_H - 4, ox, bY, 40, bH)) {
                    if (jugador.vy > 0 && jugY + JUGADOR_H <= bY + 12) {
                        jugador.y = bY - JUGADOR_H;
                        jugador.vy = 0;
                        jugador.enSuelo = true;
                        jugador.angulo = 0;
                    } else { morir(); return; }
                }
            }
            if (o.tipo === 'vacio') {
                const vAncho = o.ancho || 100;
                if (jugX + JUGADOR_W > ox && jugX < ox + vAncho) {
                    if (jugador.enSuelo) jugador.enSuelo = false;
                }
            }
            if (o.tipo === 'meta') {
                if (jugX + JUGADOR_W > ox - 10) {
                    victoria(); return;
                }
            }
        }

        if (jugador.y > H + 50) { morir(); return; }

        for (let m of monedas) {
            if (m.recogida) continue;
            const mx = m.x - camX + 200;
            const my = m.y;
            if (rectOverlap(jugX, jugY, JUGADOR_W, JUGADOR_H, mx - 10, my - 10, 20, 20)) {
                m.recogida = true;
                monedasPartida++;
                spawnParticulas(mx, my, 8, '#ffd700');
            }
            m.anim = (m.anim + 2) % 360;
        }

        particulas = particulas.filter(p => p.vida > 0);
        particulas.forEach(p => {
            p.x += p.vx; p.y += p.vy; p.vy += 0.15; p.vida -= 2;
        });
    }

    function rectOverlap(ax, ay, aw, ah, bx, by, bw, bh) {
        return ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
    }

    function morir() {
        estado = 'fin';
        cancelAnimationFrame(animFrame);
        spawnParticulas(jugador.x + JUGADOR_W / 2, jugador.y + JUGADOR_H / 2, 20, configActual.colorJugador);
        dibujar();

        setTimeout(() => {
            document.getElementById('finTitulo').textContent = 'GAME OVER';
            document.getElementById('finPuntos').textContent = puntuacion.toLocaleString();
            document.getElementById('finMonedas').textContent = monedasPartida;
            const meta = obstaculos.find(o => o.tipo === 'meta');
            const pct = meta ? Math.min(100, Math.floor((camX / (meta.x - 200)) * 100)) : 0;
            document.getElementById('finProgreso').textContent = pct + '%';

            // LA MAGIA: Guardar el progreso al servidor silenciosamente mientras mostramos la pantalla
            mostrarPantalla('fin');
            guardarPartida();
        }, 600);
    }

    function victoria() {
        estado = 'fin';
        cancelAnimationFrame(animFrame);

        document.getElementById('finTitulo').textContent = 'NIVEL SUPERADO';
        document.getElementById('finPuntos').textContent = puntuacion.toLocaleString();
        document.getElementById('finMonedas').textContent = monedasPartida;
        document.getElementById('finProgreso').textContent = '100%';

        mostrarPantalla('fin');
        guardarPartida();
    }

    function dibujar() {
        const cfg = configActual;

        ctx.fillStyle = cfg.colorFondo;
        ctx.fillRect(0, 0, W, H);

        dibujarSuelo(cfg);
        dibujarObstaculos(cfg);
        dibujarMonedas();
        if (jugador.vivo !== false) dibujarJugador(cfg);
        dibujarParticulas();
        dibujarBarraProgreso();
    }

    function dibujarSuelo(cfg) {
        const grad = ctx.createLinearGradient(0, SUELO_Y, 0, H);
        grad.addColorStop(0, cfg.colorSuelo);
        grad.addColorStop(1, 'rgba(0,0,0,0.8)');
        ctx.fillStyle = grad;
        ctx.fillRect(0, SUELO_Y, W, H - SUELO_Y);

        ctx.strokeStyle = 'rgba(0,0,0,0.3)';
        ctx.lineWidth = 1;
        const offset = camX % 40;
        for (let gx = -offset; gx < W; gx += 40) {
            ctx.beginPath(); ctx.moveTo(gx, SUELO_Y); ctx.lineTo(gx, H); ctx.stroke();
        }
        for (let gy = SUELO_Y; gy < H; gy += 20) {
            ctx.beginPath(); ctx.moveTo(0, gy); ctx.lineTo(W, gy); ctx.stroke();
        }

        ctx.strokeStyle = cfg.colorSuelo;
        ctx.lineWidth = 2;
        ctx.shadowColor = cfg.colorSuelo;
        ctx.shadowBlur = 8;
        ctx.beginPath(); ctx.moveTo(0, SUELO_Y); ctx.lineTo(W, SUELO_Y); ctx.stroke();
        ctx.shadowBlur = 0;
    }

    function dibujarObstaculos(cfg) {
        obstaculos.forEach(o => {
            const ox = o.x - camX + 200;
            if (ox < -100 || ox > W + 100) return;

            ctx.save();

            if (o.tipo === 'spike') {
                ctx.shadowColor = '#ff4444';
                ctx.shadowBlur = 10;
                ctx.fillStyle = '#ff4444';
                ctx.beginPath();
                ctx.moveTo(ox + 19, SUELO_Y - 40);
                ctx.lineTo(ox + 2, SUELO_Y);
                ctx.lineTo(ox + 36, SUELO_Y);
                ctx.closePath();
                ctx.fill();
                ctx.strokeStyle = '#ff8888';
                ctx.lineWidth = 1;
                ctx.stroke();
            }

            if (o.tipo === 'bloque') {
                const bH = o.h || 80;
                const bY = SUELO_Y - bH;
                const grad = ctx.createLinearGradient(ox, bY, ox + 40, bY + bH);
                grad.addColorStop(0, '#4a4aaa');
                grad.addColorStop(1, '#222255');
                ctx.fillStyle = grad;
                ctx.shadowColor = '#8888ff';
                ctx.shadowBlur = 8;
                ctx.fillRect(ox, bY, 40, bH);
                ctx.strokeStyle = '#6666cc';
                ctx.lineWidth = 2;
                ctx.strokeRect(ox, bY, 40, bH);
                ctx.strokeStyle = 'rgba(255,255,255,0.1)';
                ctx.lineWidth = 1;
                for (let by = bY + 10; by < bY + bH; by += 20) {
                    ctx.beginPath(); ctx.moveTo(ox, by); ctx.lineTo(ox + 40, by); ctx.stroke();
                }
            }

            if (o.tipo === 'vacio') {
                const vAncho = o.ancho || 100;
                ctx.fillStyle = 'rgba(0,0,0,0.9)';
                ctx.fillRect(ox, SUELO_Y, vAncho, H - SUELO_Y);
                ctx.strokeStyle = '#ff4444';
                ctx.lineWidth = 2;
                ctx.shadowColor = '#ff4444';
                ctx.shadowBlur = 6;
                ctx.beginPath(); ctx.moveTo(ox, SUELO_Y); ctx.lineTo(ox + vAncho, SUELO_Y); ctx.stroke();
            }

            if (o.tipo === 'meta') {
                ctx.shadowColor = '#ffd700';
                ctx.shadowBlur = 20;
                ctx.strokeStyle = '#ffd700';
                ctx.lineWidth = 3;
                ctx.beginPath(); ctx.moveTo(ox, SUELO_Y); ctx.lineTo(ox, SUELO_Y - 120); ctx.stroke();
                ctx.fillStyle = '#ffd700';
                ctx.beginPath(); ctx.moveTo(ox, SUELO_Y - 120); ctx.lineTo(ox + 40, SUELO_Y - 100); ctx.lineTo(ox, SUELO_Y - 80);
                ctx.closePath(); ctx.fill();
                ctx.fillStyle = '#fff';
                ctx.font = 'bold 14px Orbitron';
                ctx.shadowBlur = 0;
                ctx.fillText('META', ox - 8, SUELO_Y - 130);
            }

            ctx.restore();
        });
    }

    function dibujarMonedas() {
        monedas.forEach(m => {
            if (m.recogida) return;
            const mx = m.x - camX + 200;
            if (mx < -30 || mx > W + 30) return;
            const bob = Math.sin((frameCount + m.x) * 0.05) * 4;
            const my = m.y + bob;

            ctx.save();
            ctx.shadowColor = '#ffd700';
            ctx.shadowBlur = 12;
            ctx.fillStyle = '#ffd700';
            ctx.beginPath();
            for (let i = 0; i < 5; i++) {
                const ang = (i * 4 * Math.PI / 5) - Math.PI / 2;
                const angI = ((i * 4 + 2) * Math.PI / 5) - Math.PI / 2;
                if (i === 0) ctx.moveTo(mx + Math.cos(ang) * 10, my + Math.sin(ang) * 10);
                else ctx.lineTo(mx + Math.cos(ang) * 10, my + Math.sin(ang) * 10);
                ctx.lineTo(mx + Math.cos(angI) * 5, my + Math.sin(angI) * 5);
            }
            ctx.closePath(); ctx.fill();
            ctx.restore();
        });
    }

    function dibujarJugador(cfg) {
        ctx.save();
        ctx.translate(jugador.x + JUGADOR_W / 2, jugador.y + JUGADOR_H / 2);
        if (!jugador.enSuelo) ctx.rotate(jugador.angulo * Math.PI / 180);

        ctx.shadowColor = cfg.colorJugador;
        ctx.shadowBlur = 15;

        const grad = ctx.createLinearGradient(-JUGADOR_W / 2, -JUGADOR_H / 2, JUGADOR_W / 2, JUGADOR_H / 2);
        grad.addColorStop(0, cfg.colorJugador);
        grad.addColorStop(1, 'rgba(0,0,0,0.5)');
        ctx.fillStyle = grad;
        ctx.fillRect(-JUGADOR_W / 2, -JUGADOR_H / 2, JUGADOR_W, JUGADOR_H);

        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 1.5;
        ctx.strokeRect(-JUGADOR_W / 2, -JUGADOR_H / 2, JUGADOR_W, JUGADOR_H);

        ctx.fillStyle = '#fff';
        ctx.beginPath(); ctx.arc(5, -4, 6, 0, Math.PI * 2); ctx.fill();
        ctx.fillStyle = '#000';
        ctx.beginPath(); ctx.arc(6, -4, 3, 0, Math.PI * 2); ctx.fill();

        ctx.restore();
    }

    function dibujarParticulas() {
        particulas.forEach(p => {
            ctx.globalAlpha = p.vida / 100;
            ctx.fillStyle = p.color;
            ctx.fillRect(p.x, p.y, p.size, p.size);
        });
        ctx.globalAlpha = 1;
    }

    function dibujarBarraProgreso() {
        const meta = obstaculos.find(o => o.tipo === 'meta');
        if (!meta) return;
        const pct = Math.min(1, camX / (meta.x - 200));

        ctx.fillStyle = 'rgba(0,0,0,0.4)';
        ctx.fillRect(0, 0, W, 6);
        const grad = ctx.createLinearGradient(0, 0, W * pct, 0);
        grad.addColorStop(0, configActual.colorJugador);
        grad.addColorStop(1, '#ffd700');
        ctx.fillStyle = grad;
        ctx.shadowColor = configActual.colorJugador;
        ctx.shadowBlur = 4;
        ctx.fillRect(0, 0, W * pct, 6);
        ctx.shadowBlur = 0;
    }

    function dibujarPausa() {
        ctx.fillStyle = 'rgba(0,0,0,0.6)';
        ctx.fillRect(0, 0, W, H);
        ctx.fillStyle = '#fff';
        ctx.font = 'bold 48px Orbitron';
        ctx.textAlign = 'center';
        ctx.fillText('PAUSA', W / 2, H / 2 - 10);
        ctx.font = '18px Exo 2';
        ctx.fillStyle = 'rgba(255,255,255,.9)';
        ctx.fillText('Presiona ESC para continuar', W / 2, H / 2 + 30);
        ctx.textAlign = 'left';
    }

    function spawnParticulas(x, y, cantidad, color) {
        for (let i = 0; i < cantidad; i++) {
            particulas.push({
                x, y,
                vx: (Math.random() - 0.5) * 6,
                vy: (Math.random() - 0.5) * 6 - 2,
                size: Math.random() * 4 + 2,
                vida: 80 + Math.random() * 40,
                color
            });
        }
    }

    // El guardado se envía silenciosamente por AJAX (UpdatePanel)
    function guardarPartida() {
        document.getElementById('hdnNivel').value = nivelActual;
        document.getElementById('hdnPuntuacion').value = puntuacion;
        document.getElementById('hdnMonedas').value = monedasPartida;
        document.getElementById('btnGuardarPartida').click();
    }

    function mostrarRanking(datos) {
        const tbody = document.getElementById('tbodyRanking');
        if (!datos || datos.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;color:var(--muted);padding:20px">Sin datos aun</td></tr>';
            return;
        }
        tbody.innerHTML = datos.map((r, i) => `
        <tr>
            <td>${i + 1}</td>
            <td>${r.Nickname || r.Nombre}</td>
            <td>${(r.MejorPuntuacion || 0).toLocaleString()}</td>
            <td>${r.Nivel || 1}</td>
            <td>Monedas: ${r.Monedas || 0}</td>
        </tr>`).join('');
    }

    window.addEventListener('load', () => {
        inicializarEstado();
        mostrarRanking(RANKING_DATA);
    });
</script>
</body>
</html>