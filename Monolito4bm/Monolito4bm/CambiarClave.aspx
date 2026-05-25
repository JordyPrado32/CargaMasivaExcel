<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CambiarClave.aspx.cs" Inherits="Monolito4bm.CambiarClave" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Cambiar Contraseña</title>
  
  <!-- FontAwesome y SweetAlert2 -->
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    
    body {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      /* Fondo Lavanda Pastel */
      background: linear-gradient(135deg, #e6e6fa 0%, #d8bfd8 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      padding: 20px;
    }

    .card {
      position: relative; 
      /* Fondo blanco transparentoso (Glassmorphism) */
      background: rgba(255, 255, 255, 0.55);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border: 1px solid rgba(255, 255, 255, 0.8);
      border-radius: 16px;
      padding: 50px 60px;
      width: 100%;
      max-width: 600px; 
      box-shadow: 0 20px 50px rgba(0,0,0,0.15);
    }

    /* BARRA DE PROGRESO ANIMADA */
    .progress-bar-inline {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 8px;
      background: linear-gradient(90deg, #ff8da1, #e0c3fc, #ff8da1, #e0c3fc);
      background-size: 300% 100%;
      border-radius: 16px 16px 0 0;
      animation: moverDegradado 3s linear infinite;
    }

    @keyframes moverDegradado {
      0% { background-position: 100% 0; }
      100% { background-position: -100% 0; }
    }

    .card h2 {
      text-align: center;
      color: #5d3f6a;
      font-size: 2.2rem;
      font-weight: 700;
      margin-top: 10px;
      margin-bottom: 10px;
    }
    
    .card .subtitulo {
      text-align: center; 
      color: #555; 
      font-size: 1.05rem;
      font-weight: 600;
      margin-bottom: 30px; 
      line-height: 1.5;
    }

    .field { margin-bottom: 25px; }
    
    .field label {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 10px;
      color: #555;
      font-size: 1.05rem;
      font-weight: 600;
    }

    .field label i {
      color: #ff8da1; 
      font-size: 1.25rem;
    }

    .field input[type=password] {
      width: 100%;
      padding: 16px 20px; 
      border: 2px solid #ffb6c1; 
      border-radius: 10px; 
      font-size: 1.1rem; 
      background: rgba(255, 255, 255, 0.95);
      transition: all 0.3s ease;
      outline: none;
    }

    .field input:focus { 
      border-color: #ff69b4; 
      box-shadow: 0 0 10px rgba(255, 105, 180, 0.3);
    }

    /* NUEVO: Estilos para la lista de reglas con los íconos */
    .rule-list { 
        list-style: none; 
        padding: 0; 
        margin-top: 12px; 
    }
    .rule-list li { 
        color: #555; /* Texto neutral siempre */
        margin-bottom: 8px; 
        display: flex; 
        align-items: center; 
        gap: 10px; 
        font-size: 0.95rem; 
        font-weight: 600; 
    }
    
    /* Configuración del ícono dentro de la lista */
    .rule-list li i {
        font-size: 1.1rem;
        width: 20px;
        text-align: center;
        transition: transform 0.3s ease, color 0.3s ease;
    }
    
    .icon-error { color: #c0392b; transform: scale(1); }
    .icon-success { color: #27ae60; transform: scale(1.2); } /* Hace un pequeño salto al ponerse verde */

    .btn-cambiar {
      width: 100%;
      padding: 18px;
      background: #ff8da1; 
      color: #fff;
      border: none;
      border-radius: 10px;
      font-size: 1.2rem;
      font-weight: 700;
      cursor: pointer;
      margin-top: 10px;
      transition: background .2s, transform .1s;
      box-shadow: 0 4px 10px rgba(255, 141, 161, 0.3);
    }

    .btn-cambiar:hover { 
      background: #ff6b8b; 
      transform: translateY(-2px);
    }
  </style>
</head>
<body>
  <form id="frmCambiar" runat="server">
    <div class="card">
      
      <div class="progress-bar-inline"></div>

      <h2>Cambio de Clave</h2>
      <p class="subtitulo">Ingresaste con una clave temporal.<br/>Debes establecer una nueva contraseña segura para continuar.</p>

      <div class="field">
        <label><i class="fa-solid fa-key"></i> Nueva contraseña</label>
        <asp:TextBox ID="txtNueva" runat="server" TextMode="Password" ClientIDMode="Static" onkeyup="validarReglas()"/>
        
        <!-- Lista de reglas -->
        <ul class="rule-list" id="listaReglas">
            <li id="rule-length"><i class="fa-solid fa-times icon-error"></i> Entre 8 y 16 caracteres</li>
            <li id="rule-upper"><i class="fa-solid fa-times icon-error"></i> Al menos una letra mayúscula</li>
            <li id="rule-lower"><i class="fa-solid fa-times icon-error"></i> Al menos una letra minúscula</li>
            <li id="rule-number"><i class="fa-solid fa-times icon-error"></i> Al menos un número</li>
            <li id="rule-special"><i class="fa-solid fa-times icon-error"></i> Al menos un carácter especial (@$!%*?&._-)</li>
        </ul>
      </div>

      <div class="field">
        <label><i class="fa-solid fa-check-double"></i> Confirmar contraseña</label>
        <asp:TextBox ID="txtConfirmar" runat="server" TextMode="Password" ClientIDMode="Static"/>
      </div>

      <asp:Button ID="btnCambiar" runat="server" Text="Cambiar contraseña"
                  CssClass="btn-cambiar" OnClick="btnCambiar_Click" OnClientClick="return validarSubmit();" />
    </div>
  </form>

  <script>
      // Función para cambiar el ícono de cruz roja a visto verde
      function toggleIcono(idRegla, esValido) {
          const item = document.getElementById(idRegla);
          const icono = item.querySelector('i');

          if (esValido) {
              icono.className = 'fa-solid fa-check icon-success';
          } else {
              icono.className = 'fa-solid fa-times icon-error';
          }
      }

      function validarReglas() {
          const pass = document.getElementById('txtNueva').value;

          // 8 a 16 caracteres
          toggleIcono('rule-length', pass.length >= 8 && pass.length <= 16);
          // Mayúscula
          toggleIcono('rule-upper', /[A-Z]/.test(pass));
          // Minúscula
          toggleIcono('rule-lower', /[a-z]/.test(pass));
          // Número
          toggleIcono('rule-number', /\d/.test(pass));
          // Especial
          toggleIcono('rule-special', /[@$!%*?&._-]/.test(pass));
      }

      // Evita que manden el formulario y muestra SweetAlert si faltan reglas
      function validarSubmit() {
          const pass = document.getElementById('txtNueva').value;
          const pass2 = document.getElementById('txtConfirmar').value;
          const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,16}$/;

          if (!regex.test(pass)) {
              Swal.fire({
                  icon: 'warning',
                  title: 'Contraseña débil',
                  text: 'Asegúrate de que todos los iconos de la lista estén en verde (✔).'
              });
              return false;
          }

          if (pass !== pass2) {
              Swal.fire({
                  icon: 'error',
                  title: 'No coinciden',
                  text: 'La confirmación no es igual a tu nueva contraseña.'
              });
              return false;
          }

          return true;
      }
  </script>
</body>
</html>