<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecuperarClave.aspx.cs" Inherits="Monolito4bm.RecuperarClave" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Recuperar Contraseña</title>
  
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
      /* Usamos 600px aquí porque tiene menos campos que el login, se ve más proporcionado */
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

    .field input[type=text] {
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

    .btn-recuperar {
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

    .btn-recuperar:hover { 
      background: #ff6b8b; 
      transform: translateY(-2px);
    }

    .volver {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      margin-top: 25px; 
      color: #5d3f6a;
      font-size: 1.05rem; 
      font-weight: 600;
      text-decoration: none;
      transition: color 0.3s;
    }

    .volver:hover { 
      color: #ff69b4; 
      text-decoration: underline; 
    }
  </style>
</head>
<body>
  <form id="frmRecuperar" runat="server">
    <div class="card">
      
      <div class="progress-bar-inline"></div>

      <h2>Recupera tu contraseña</h2>
      <p class="subtitulo">Ingresa nickname o correo electrónico y recibira<br/>una clave temporal para ingresar en el inicio de sesión </p>

      <div class="field">
        <label>
            <i class="fa-solid fa-envelope"></i> Nickname o correo electrónico
        </label>
        <asp:TextBox ID="txtNickOCorreo" runat="server" ClientIDMode="Static"/>
        <asp:RequiredFieldValidator ID="rfv" runat="server"
          ControlToValidate="txtNickOCorreo" ErrorMessage="Campo requerido."
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <asp:Button ID="btnEnviar" runat="server" Text="Enviar clave temporal"
                  CssClass="btn-recuperar" OnClick="btnEnviar_Click"/>

      <a href="Default.aspx" class="volver"><i class="fa-solid fa-arrow-left"></i> Volver al login</a>
    </div>
  </form>
</body>
</html>