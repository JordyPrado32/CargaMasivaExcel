<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Monolito4bm.Default" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Iniciar Sesión</title>
  <!-- Agregamos FontAwesome para los iconos -->
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
  <!-- AGREGADO: Librería SweetAlert2 -->
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    
    body {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #e6e6fa 0%, #d8bfd8 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      padding: 20px; 
    }

    .card {
      position: relative; 
      background: rgba(255, 255, 255, 0.55);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border: 1px solid rgba(255, 255, 255, 0.8);
      border-radius: 16px;
      padding: 50px 60px;
      width: 100%;
      max-width: 800px;
      box-shadow: 0 20px 50px rgba(0,0,0,0.15);
    }

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
      margin-bottom: 30px;
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

    .field input[type=text],
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

    .pass-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }
    
    .toggle-password {
      position: absolute;
      right: 20px;
      color: #ff8da1;
      font-size: 1.3rem;
      cursor: pointer;
      transition: color 0.3s;
    }

    .toggle-password:hover { color: #ff69b4; }

    .remember-wrapper {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-top: -10px;
      margin-bottom: 25px;
      color: #555;
      font-weight: 600;
      font-size: 1rem;
    }
    
    .remember-wrapper input[type=checkbox] {
      width: 18px;
      height: 18px;
      accent-color: #ff8da1; 
      cursor: pointer;
    }

    .remember-wrapper label { cursor: pointer; }

    .actions {
      display: flex;
      flex-direction: column;
      gap: 18px;
    }

    .secondary-actions {
      display: grid;
      grid-template-columns: 1fr 1fr; 
      gap: 20px; 
      width: 100%;
    }

    .btn-login {
      width: 100%;
      padding: 18px;
      background: #ff8da1; 
      color: #fff;
      border: none;
      border-radius: 10px;
      font-size: 1.2rem;
      font-weight: 700;
      cursor: pointer;
      transition: background .2s, transform .1s;
      box-shadow: 0 4px 10px rgba(255, 141, 161, 0.3);
    }

    .btn-login:hover { 
      background: #ff6b8b; 
      transform: translateY(-2px);
    }

    .btn-secondary {
      width: 100%;
      padding: 15px 10px; 
      background: rgba(255, 255, 255, 0.7);
      color: #ff6b8b;
      border: 2px solid #ff8da1;
      border-radius: 10px;
      font-size: 1.05rem;
      font-weight: 700;
      cursor: pointer;
      white-space: normal; 
      transition: all 0.3s ease;
    }

    .btn-secondary:hover {
      background: #ff8da1;
      color: #fff;
      box-shadow: 0 4px 10px rgba(255, 141, 161, 0.3);
    }

    @media (max-width: 650px) {
      .secondary-actions {
        grid-template-columns: 1fr;
      }
      .card { padding: 40px 30px; }
    }
  </style>
</head>
<body>
  <form id="frmLogin" runat="server">
    <div class="card">
      
      <div class="progress-bar-inline"></div>

      <h2>Acceso al Sistema</h2>

      <div class="field">
        <label for="txtNick">
          <i class="fa-solid fa-user"></i> Ingrese nickname/correo
        </label>
        <asp:TextBox ID="txtNick" runat="server" ClientIDMode="Static"/>
        <asp:RequiredFieldValidator ID="rfvNick" runat="server"
          ControlToValidate="txtNick" ErrorMessage="El correo o nickname es obligatorio."
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <div class="field">
        <label for="txtPass">
          <i class="fa-solid fa-lock"></i> Ingrese contraseña
        </label>
        <div class="pass-wrapper">
            <asp:TextBox ID="txtPass" runat="server" TextMode="Password" ClientIDMode="Static" style="padding-right: 50px;"/>
            <i class="fa-solid fa-eye toggle-password" onclick="togglePass()"></i>
        </div>
        <asp:RequiredFieldValidator ID="rfvPass" runat="server"
          ControlToValidate="txtPass" ErrorMessage="La contraseña es obligatoria."
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <div class="remember-wrapper">
        <asp:CheckBox ID="chkRecordar" runat="server" />
        <label for="chkRecordar">Recuérdame en este equipo</label>
      </div>

      <div class="actions">
        <asp:Button ID="btnLogin" runat="server" Text="Entrar"
          CssClass="btn-login" OnClick="btnLogin_Click"/>
        
        <div class="secondary-actions">
            <asp:Button ID="btnRecuperar" runat="server" Text="Recuperar contraseña"
              CssClass="btn-secondary" OnClick="btnRecuperar_Click" CausesValidation="false" />
            <asp:Button ID="btnRegister" runat="server" Text="Crear cuenta nueva"
              CssClass="btn-secondary" OnClick="btnRegister_Click" CausesValidation="false" />
        </div>
      </div>
    </div>
  </form>

  <script>
      function togglePass() {
          var input = document.getElementById('<%= txtPass.ClientID %>');
          var icon = document.querySelector('.toggle-password');

          if (input.type === "password") {
              input.type = "text";
              icon.classList.remove('fa-eye');
              icon.classList.add('fa-eye-slash');
          } else {
              input.type = "password";
              icon.classList.remove('fa-eye-slash');
              icon.classList.add('fa-eye');
          }
      }
  </script>
</body>
</html>