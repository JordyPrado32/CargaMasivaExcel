<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerificarQR.aspx.cs" Inherits="Monolito4bm.VerificarQR" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Verificar QR</title>
  <!-- Agregamos SweetAlert2 -->
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-direction: column;
      /* Fondo Lavanda Pastel */
      background: linear-gradient(135deg, #e6e6fa 0%, #d8bfd8 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      padding: 32px 16px;
    }
    .card {
      /* Fondo blanco transparentoso (Glassmorphism) */
      background: rgba(255, 255, 255, 0.65);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border: 1px solid rgba(255, 255, 255, 0.8);
      border-radius: 16px;
      padding: 45px 40px;
      max-width: 500px;
      width: 100%;
      text-align: center;
      box-shadow: 0 20px 50px rgba(0,0,0,0.15);
    }
    .card h2 { margin-bottom: 10px; color: #5d3f6a; font-size: 2rem; font-weight: 700; }
    .card p  { color: #555; font-size: 1rem; margin-bottom: 25px; font-weight: 600;}
    
    #visor {
      width: 100%;
      max-width: 360px;
      border-radius: 10px;
      overflow: hidden;
      margin: 0 auto 20px;
      position: relative;
      background: #000;
      box-shadow: 0 10px 20px rgba(0,0,0,0.2);
    }
    #visor video {
      width: 100%;
      display: block;
      /* Efecto espejo para que se sienta natural */
      transform: scaleX(-1); 
    }
    #visor canvas { display: none; }
    
    /* Marco animado Rosa Pastel */
    .scan-frame {
      position: absolute;
      inset: 0;
      border: 3px solid #ff8da1;
      border-radius: 10px;
      pointer-events: none;
    }
    .scan-line {
      position: absolute;
      left: 0; right: 0;
      height: 3px;
      background: #ff69b4;
      box-shadow: 0 0 10px rgba(255, 105, 180, 0.8);
      animation: scan 2s linear infinite;
    }
    @keyframes scan { 0%{top:0} 100%{top:100%} }

    .estado {
      font-size: 1rem;
      font-weight: 600;
      padding: 10px 14px;
      border-radius: 8px;
      margin-bottom: 15px;
    }
    .estado.espera  { background: rgba(255,255,255,0.7); color: #5d3f6a; }

    #btnActivar {
      padding: 14px 28px;
      /* Botón rosa */
      background: #ff8da1;
      color: #fff;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 1.1rem;
      font-weight: 700;
      transition: background .2s, transform .1s;
      box-shadow: 0 4px 10px rgba(255, 141, 161, 0.3);
    }
    #btnActivar:hover { background: #ff6b8b; transform: translateY(-2px); }

    /* Hidden form para postback */
    #frmOtp { display:none; }
  </style>
</head>
<body>
  <div class="card">
    <h2>Verificación de dos pasos</h2>
    <p>Revisa tu correo electrónico y escanea el código QR con la cámara de tu computadora, o en tu whatsapp el código de verificación.</p>

    <div id="visor">
      <video id="video" autoplay playsinline muted></video>
      <canvas id="canvas"></canvas>
      <div class="scan-frame"><div class="scan-line"></div></div>
    </div>

    <div id="divEstado" class="estado espera">Presiona el botón para activar la cámara</div>
    <button id="btnActivar" type="button" onclick="activarCamara()">Activar cámara</button>
      <!-- AGREGADO: Ingreso manual del OTP -->
        <div style="margin-top: 20px;">
            <p style="color:#5d3f6a; font-weight:600; font-size:1rem; margin-bottom:10px;">
                ¿No puedes escanear? Ingresa el código manualmente:
            </p>
            <div style="display:flex; gap:10px; justify-content:center; flex-wrap:wrap;">
                <input type="text" id="txtOtpManual" maxlength="8"
                       placeholder="Ej: AB3F7KZP"
                       style="padding:12px 16px; border:2px solid #ffb6c1; border-radius:10px;
                              font-size:1.1rem; font-weight:700; text-transform:uppercase;
                              text-align:center; letter-spacing:3px; outline:none;
                              background:rgba(255,255,255,0.95); width:200px;
                              transition: border-color 0.3s ease;"
                       onfocus="this.style.borderColor='#ff69b4'"
                       onblur="this.style.borderColor='#ffb6c1'" />
                <button type="button" onclick="enviarOtpManual()"
                        style="padding:12px 22px; background:#ff8da1; color:#fff; border:none;
                               border-radius:10px; font-size:1.05rem; font-weight:700;
                               cursor:pointer; transition: background .2s, transform .1s;
                               box-shadow: 0 4px 10px rgba(255,141,161,0.3);"
                        onmouseover="this.style.background='#ff6b8b'"
                        onmouseout="this.style.background='#ff8da1'">
                    Verificar
                </button>
            </div>
        </div>
  </div>

  <!-- Form oculto que hace postback cuando se lee el OTP -->
  <form id="frmOtp" runat="server">
    <asp:HiddenField ID="hdnOtp" runat="server" ClientIDMode="Static"/>
    <asp:Button ID="btnValidar" runat="server" Text="Validar"
                ClientIDMode="Static" OnClick="btnValidar_Click"/>
  </form>

  <!-- jsQR: librería ligera sin dependencias para decodificar QR -->
  <script src="https://cdn.jsdelivr.net/npm/jsqr@1.4.0/dist/jsQR.min.js"></script>
  
  <script>
      let video = document.getElementById('video');
      let canvas = document.getElementById('canvas');
      let ctx = canvas.getContext('2d');
      let estado = document.getElementById('divEstado');
      let escaneando = false;
      let yaDetectado = false;

      function activarCamara() {
          // Ocultamos el botón mientras lee
          document.getElementById('btnActivar').style.display = 'none';

          navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' } })
              .then(stream => {
                  video.srcObject = stream;
                  video.play();
                  escaneando = true;
                  estado.textContent = 'Cámara activa — apunta el QR al visor…';
                  requestAnimationFrame(tick);
              })
              .catch(err => {
                  // SweetAlert de Error en la cámara
                  Swal.fire({
                      icon: 'error',
                      title: 'Error de Cámara',
                      text: 'No se pudo acceder a la cámara. Revisa los permisos.'
                  });
                  document.getElementById('btnActivar').style.display = 'inline-block';
                  estado.textContent = 'Inténtalo de nuevo';
              });
      }

      function tick() {
          if (!escaneando || yaDetectado) return;

          if (video.readyState === video.HAVE_ENOUGH_DATA) {
              canvas.height = video.videoHeight;
              canvas.width = video.videoWidth;
              ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
              let imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
              let code = jsQR(imageData.data, imageData.width, imageData.height, {
                  inversionAttempts: 'dontInvert'
              });

              if (code && code.data.startsWith('OTP:')) {
                  let otp = code.data.replace('OTP:', '').trim();
                  yaDetectado = true;
                  escaneando = false;
                  video.srcObject.getTracks().forEach(t => t.stop());

                  // SweetAlert de Éxito Visual
                  Swal.fire({
                      icon: 'success',
                      title: '¡QR Detectado!',
                      text: 'Verificando tu acceso...',
                      showConfirmButton: false,
                      timer: 1500
                  });

                  estado.textContent = 'Verificando código...';

                  // Mandamos al servidor C# tras 1 segundo para que la gente alcance a ver el SweetAlert
                  document.getElementById('hdnOtp').value = otp;
                  setTimeout(() => { document.getElementById('btnValidar').click(); }, 1200);
                  return;
              }
          }
          requestAnimationFrame(tick);
      }
      function enviarOtpManual() {
          var otp = document.getElementById('txtOtpManual').value.trim().toUpperCase();

          if (otp.length !== 8) {
              Swal.fire({
                  icon: 'warning',
                  title: 'Código inválido',
                  text: 'El código OTP debe tener exactamente 8 caracteres.'
              });
              return;
          }

          Swal.fire({
              icon: 'info',
              title: 'Verificando...',
              text: 'Procesando tu código OTP',
              showConfirmButton: false,
              timer: 1200
          });

          document.getElementById('hdnOtp').value = otp;
          setTimeout(function () {
              document.getElementById('btnValidar').click();
          }, 1000);
      }
  </script>
</body>
</html>