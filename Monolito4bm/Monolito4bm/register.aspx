<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="register.aspx.cs" Inherits="Monolito4bm.register"%>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <title>Registro de Usuario</title>
    <!-- Librerías -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background: linear-gradient(135deg, #e6e6fa 0%, #d8bfd8 100%); /* Lavanda Pastel */
            display: flex; 
            justify-content: center; 
            align-items: center;
            padding: 30px 20px; 
            min-height: 100vh;
        }

        .card { 
            position: relative;
            background: rgba(255, 255, 255, 0.55);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            border: 1px solid rgba(255, 255, 255, 0.8);
            border-radius: 16px; 
            padding: 15px 50px; 
            box-shadow: 0 20px 50px rgba(0,0,0,0.15); 
            
            /* Tarjeta Gigante */
            width: 98%; 
            max-width: 1350px; 
        }
        .card h2 { 
            text-align: center; 
            color: #5d3f6a; 
            font-size: 2.2rem;
            font-weight: 700;
            margin-top: 5px;
            margin-bottom: 25px; 
        }
        .main-layout {
            display: grid;
            grid-template-columns: 2fr 1fr; /* 2 tercios para el form, 1 tercio para las fotos */
            gap: 40px;
        }

        /* GRID PARA LOS CAMPOS DEL FORMULARIO */
        .form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px 25px;
            align-content: start;
        }

        /* SIDEBAR DE FOTOS Y BOTONES FINALES (Panel derecho) */
        .photo-sidebar {
            background: rgba(255, 255, 255, 0.4);
            border: 2px dashed #e0c3fc;
            border-radius: 16px;
            padding: 25px;
            display: flex;
            flex-direction: column;
            gap: 15px;
            height: 100%; /* Se estira para igualar a la columna izquierda */
        }

        .field label { 
            display: flex; 
            align-items: center;
            gap: 8px;
            margin-bottom: 8px; 
            font-weight: 600; 
            color: #555; 
            font-size: 1rem;
        }

        .field label i {
            color: #ff8da1;
            font-size: 1.15rem;
            width: 20px;
            text-align: center;
        }

        input[type="text"], input[type="password"], input[type="date"], select { 
            width: 100%; 
            padding: 14px 18px; 
            border: 2px solid #ffb6c1; 
            border-radius: 10px; 
            background: rgba(255, 255, 255, 0.9);
            font-size: 1rem;
            outline: none;
            transition: all 0.3s ease;
        }

        input:focus { 
            border-color: #ff69b4; 
            box-shadow: 0 0 10px rgba(255, 105, 180, 0.3);
        }

        /* Inputs bloqueados */
        input[readonly] { 
            background-color: rgba(233, 236, 239, 0.7) !important; 
            color: #666;
            border-color: #ddd;
            cursor: not-allowed;
        }

        /* Validación visual JS */
        .input-soft-error { border-color: #e74c3c !important; box-shadow: 0 0 8px rgba(231, 76, 60, 0.2); }
        .input-soft-ok { border-color: #2ecc71 !important; box-shadow: 0 0 8px rgba(46, 204, 113, 0.2); }

        .pass-wrapper { position: relative; display: flex; align-items: center; }
        .toggle-password { 
            position: absolute; 
            right: 15px; 
            cursor: pointer; 
            color: #ff8da1; 
            font-size: 1.2rem;
            transition: color 0.3s;
        }
        .toggle-password:hover { color: #ff69b4; }

        /* Estilo para subir fotos */
        input[type="file"] {
            width: 100%;
            padding: 12px;
            background: rgba(255, 255, 255, 0.8);
            border: 1px solid #ffb6c1;
            border-radius: 10px;
            color: #555;
            cursor: pointer;
            margin-bottom: 10px;
        }

        /* Panel de Fotos Previas */
        .preview-help { color: #5d3f6a; font-size: 0.95rem; font-weight: 600; display: block; margin-bottom: 15px;}
        .preview-grid { display: flex; flex-wrap: wrap; gap: 15px; justify-content: center;}
        
        .preview-card { 
            width: 110px; 
            background: #fff; 
            border: 1px solid #ffb6c1; 
            border-radius: 10px; 
            padding: 8px; 
            text-align: center; 
            box-shadow: 0 4px 10px rgba(0,0,0,0.05);
        }
        .preview-card img { width: 100%; height: 90px; object-fit: cover; border-radius: 6px; margin-bottom: 8px;}
        .preview-name { display: block; font-size: 0.7rem; color: #555; word-break: break-all; margin-bottom: 8px; height: 26px; overflow: hidden;}
        
        .btn-delete { 
            background-color: #ff4757; color: white; border: none; border-radius: 6px; 
            padding: 6px 10px; cursor: pointer; font-size: 0.8rem; width: 100%; font-weight: bold;
        }
        .btn-delete:hover { background-color: #ff6b81; }

        /* Botones */
        .btn-secondary { 
            background-color: rgba(255, 255, 255, 0.7); 
            color: #ff6b8b; 
            border: 2px solid #ff8da1; 
            padding: 12px 16px; 
            border-radius: 10px; 
            cursor: pointer; 
            font-weight: 700;
            font-size: 1rem;
            transition: all 0.2s;
            width: 100%;
        }
        .btn-secondary:hover { background-color: #ff8da1; color: white; }

        /* Contenedor Final pegado al fondo */
        .footer-actions { 
            margin-top: auto; /* LA MAGIA: Empuja los botones al fondo de la columna */
            padding-top: 20px;
            border-top: 1px solid rgba(224, 195, 252, 0.8);
            display: flex; 
            flex-direction: column; 
            align-items: center;
            gap: 15px; 
            width: 100%;
        }

        .btn { 
            background-color: #ff8da1; 
            color: white; 
            padding: 18px 20px; 
            border: none; 
            border-radius: 10px; 
            width: 100%; 
            cursor: pointer; 
            font-size: 1.25rem; 
            font-weight: 700;
            transition: all 0.2s;
            box-shadow: 0 4px 10px rgba(255, 141, 161, 0.3);
        }
        .btn:hover { background-color: #ff6b8b; transform: translateY(-2px); }

        .link-volver { display: block; text-align: center; color: #5d3f6a; text-decoration: none; font-weight: 600; transition: color 0.3s;}
        .link-volver:hover { color: #ff69b4; text-decoration: underline; }

        /* Responsivo si abren la ventana pequeña */
        @media (max-width: 1024px) {
            .main-layout { grid-template-columns: 1fr; }
            .photo-sidebar { height: auto; } /* En móviles vuelve a ajustarse */
        }
        @media (max-width: 768px) {
            .form-grid { grid-template-columns: 1fr; }
            .card { padding: 30px 25px; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="card">
                    <h2>Registro Nuevo</h2>
                    <div class="main-layout">
                        <div class="form-grid">
                            <div class="field">
                                <label><i class="fa-solid fa-id-card"></i> Nombres (Mínimo uno)</label>
                                <asp:TextBox ID="txtNombres" runat="server" placeholder="Ej: Juan o María del Carmen" onblur="validarNombres(this); sugerirCorreo();"></asp:TextBox>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-users"></i> Apellidos (Exactamente dos)</label>
                                <asp:TextBox ID="txtApellidos" runat="server" placeholder="Ej: Pérez Gómez" onblur="validarApellidos(this); sugerirCorreo();"></asp:TextBox>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-address-card"></i> Cédula Ecuatoriana</label>
                                <asp:TextBox ID="txtCedula" runat="server" MaxLength="10" placeholder="10 dígitos" onblur="validarCedula(this)"></asp:TextBox>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-envelope"></i> Correo Electrónico</label>
                                <asp:TextBox ID="txtCorreo" runat="server" placeholder="@dominio.com" onblur="validarCorreo(this)"></asp:TextBox>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-lock"></i> Contraseña Segura</label>
                                <div class="pass-wrapper">
                                    <asp:TextBox ID="txtPass" runat="server" TextMode="Password" placeholder="Mayúscula, número y símbolo" style="padding-right: 45px;" onblur="validarPass(this)"></asp:TextBox>
                                    <i class="fa-solid fa-eye toggle-password" onclick="togglePass()"></i>
                                </div>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-at"></i> Nickname (Autogenerado)</label>
                                <asp:TextBox ID="txtNickname" runat="server" placeholder="Se genera al guardar..."></asp:TextBox>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-calendar-days"></i> Fecha de Nacimiento</label>
                                <asp:TextBox ID="txtFechaNac" runat="server" TextMode="Date" onblur="validarEdad(this)"></asp:TextBox>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-cake-candles"></i> Edad Calculada</label>
                                <asp:TextBox ID="txtEdad" runat="server" placeholder="Debe ser mayor de 18"></asp:TextBox>
                            </div>

                            <div class="field">
                                <label><i class="fa-solid fa-mobile-screen"></i> Celular</label>
                                <asp:TextBox ID="txtCelular" runat="server" MaxLength="10" placeholder="Ej: 0991234567" onblur="validarCelular(this)"></asp:TextBox>
                            </div>
                        </div>

                        <!-- LADO DERECHO: FOTOS Y BOTONES FINALES -->
                        <div class="photo-sidebar">
                            <div class="field">
                                <label><i class="fa-solid fa-camera-retro"></i> Fotografías de Perfil</label>
                                <span style="font-size:0.85rem; color:#666; display:block; margin-bottom:10px;">Selecciona de 3 a 5 imágenes (Máx 2MB c/u).</span>
                                
                                <asp:FileUpload ID="fuFotos" runat="server" AllowMultiple="true" onchange="validarFotos(this)" />
                                <asp:Button ID="btnPrevisualizar" runat="server" Text="Subir y Previsualizar" OnClick="btnPrevisualizar_Click" formnovalidate="formnovalidate" CssClass="btn-secondary" />
                            </div>

                            <asp:Panel ID="pnlPreview" runat="server" Visible="false">
                                <asp:Label ID="lblFotosInfo" runat="server" CssClass="preview-help"></asp:Label>
                                <asp:Repeater ID="rptFotosPreview" runat="server" OnItemCommand="rptFotosPreview_ItemCommand">
                                    <HeaderTemplate><div class="preview-grid"></HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="preview-card">
                                            <asp:Image ID="imgPreview" runat="server" ImageUrl='<%# Eval("PreviewUrl") %>' AlternateText='<%# Eval("NombreArchivo") %>' />
                                            <span class="preview-name"><%# Eval("NombreArchivo") %></span>
                                            <asp:Button ID="btnEliminarFoto" runat="server" Text="Quitar" CssClass="btn-delete" CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>' CausesValidation="false" UseSubmitBehavior="false" />
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                            </asp:Panel>

                            <!-- BOTONES AL FINAL DE LA COLUMNA DERECHA -->
                            <div class="footer-actions">
                                <asp:Button ID="btnRegistrar" runat="server" Text="Finalizar Registro Seguro" CssClass="btn" OnClick="btnRegistrar_Click" />
                                <a href="Default.aspx" class="link-volver"><i class="fa-solid fa-arrow-left"></i> Ya tengo cuenta, ir al Login</a>
                            </div>

                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnPrevisualizar" />
                <asp:PostBackTrigger ControlID="btnRegistrar" />
            </Triggers>
        </asp:UpdatePanel>
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

        function setFieldState(input, isValid, message) {
            input.classList.remove('input-soft-error', 'input-soft-ok');
            input.removeAttribute('title');
            if (input.value.trim() === '') return;
            if (isValid) {
                input.classList.add('input-soft-ok');
                return;
            }
            input.classList.add('input-soft-error');
            if (message) input.title = message;
        }

        function obtenerPrimerBloque(valor) {
            let limpio = valor.trim().replace(/\s+/g, ' ');
            if (limpio === '') return '';
            let palabras = limpio.split(' ');
            let conectores = ['de', 'del', 'la', 'las', 'los', 'san', 'santa', 'da', 'das', 'do', 'dos', 'van', 'von'];
            if (palabras.length === 1) return palabras[0];
            if (conectores.indexOf(palabras[0].toLowerCase()) >= 0) return limpio;
            if (palabras.length > 1 && conectores.indexOf(palabras[1].toLowerCase()) >= 0) return palabras.join(' ');
            return palabras[0];
        }

        function validarNombres(input) {
            let valor = input.value.trim().replace(/\s+/g, ' ');
            if (valor === '') { setFieldState(input, true, ''); return true; }
            let soloLetras = valor.replace(/[^A-Za-zÁÉÍÓÚáéíóúÑñ ]/g, '').replace(/\s+/g, '');
            let valido = soloLetras.length >= 3;
            setFieldState(input, valido, 'Debe ingresar al menos un nombre válido (mín. 3 letras).');
            return valido;
        }

        function validarApellidos(input) {
            let valor = input.value.trim().replace(/\s+/g, ' ');
            if (valor === '') { setFieldState(input, true, ''); return true; }
            let palabras = valor.split(' ');
            let conectores = ['de', 'del', 'la', 'las', 'los', 'san', 'santa', 'da', 'das', 'do', 'dos', 'van', 'von'];
            let apellidosReales = palabras.filter(p => conectores.indexOf(p.toLowerCase()) === -1);
            let valido = apellidosReales.length === 2;
            setFieldState(input, valido, 'Debe ingresar exactamente DOS apellidos.');
            return valido;
        }

        function sugerirCorreo() {
            let nombres = obtenerPrimerBloque(document.getElementById('<%= txtNombres.ClientID %>').value);
            let apellidos = obtenerPrimerBloque(document.getElementById('<%= txtApellidos.ClientID %>').value);
            let correo = document.getElementById('<%= txtCorreo.ClientID %>');
            if (nombres && apellidos && correo.value.trim() === '') {
                let normalizadoNombre = nombres.toLowerCase().replace(/\s+/g, '');
                let normalizadoApellido = apellidos.toLowerCase().replace(/\s+/g, '');
                correo.value = normalizadoNombre + "." + normalizadoApellido + "@gmail.com";
                validarCorreo(correo);
            }
        }

        function validarCedula(input) {
            let cedula = input.value.trim();
            if (cedula === '') { setFieldState(input, true, ''); return true; }
            if (cedula.length !== 10 || isNaN(cedula)) {
                setFieldState(input, false, 'La cédula debe tener 10 números.');
                return false;
            }
            let prov = parseInt(cedula.substring(0, 2));
            if (prov < 1 || prov > 24 || parseInt(cedula[2]) >= 6) {
                setFieldState(input, false, 'Estructura de cédula inválida.');
                return false;
            }
            let coef = [2, 1, 2, 1, 2, 1, 2, 1, 2];
            let suma = 0;
            for (let i = 0; i < 9; i++) {
                let val = parseInt(cedula[i]) * coef[i];
                suma += (val > 9) ? val - 9 : val;
            }
            let verificador = parseInt(cedula[9]);
            let dsuperior = (Math.ceil(suma / 10) * 10);
            let calculado = dsuperior - suma;
            if (calculado === 10) calculado = 0;
            if (calculado !== verificador) {
                setFieldState(input, false, 'Número de cédula no válido.');
                return false;
            }
            setFieldState(input, true, '');
            return true;
        }

        function validarCorreo(input) {
            let valor = input.value.trim();
            if (valor === '') { setFieldState(input, true, ''); return true; }
            let regex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
            let valido = regex.test(valor);
            setFieldState(input, valido, 'Formato de correo inválido.');
            return valido;
        }

        function validarPass(input) {
            let valor = input.value.trim();
            if (valor === '') { setFieldState(input, true, ''); return true; }
            let regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$/;
            let valido = regex.test(valor);
            setFieldState(input, valido, 'Use mínimo 8 caracteres con mayúscula, minúscula, número y símbolo.');
            return valido;
        }

        function validarEdad(input) {
            if (!input.value) {
                document.getElementById('<%= txtEdad.ClientID %>').value = '';
                setFieldState(input, true, '');
                return true;
            }
            let partes = input.value.split('-');
            if (partes.length !== 3) return false;
            let anio = parseInt(partes[0], 10);
            let mes = parseInt(partes[1], 10) - 1; 
            let dia = parseInt(partes[2], 10);
            let fechaNac = new Date(anio, mes, dia);
            let hoy = new Date();
            let edad = hoy.getFullYear() - fechaNac.getFullYear();
            let m = hoy.getMonth() - fechaNac.getMonth();
            if (m < 0 || (m === 0 && hoy.getDate() < fechaNac.getDate())) { edad--; }

            document.getElementById('<%= txtEdad.ClientID %>').value = edad;

            if (edad < 18 || edad > 120 || isNaN(edad)) {
                document.getElementById('<%= txtEdad.ClientID %>').value = '';
                setFieldState(input, false, 'La edad debe estar entre 18 y 120 años.');
                return false;
            }
            setFieldState(input, true, '');
            return true;
        }

        function validarCelular(input) {
            let valor = input.value.trim();
            if (valor === '') { setFieldState(input, true, ''); return true; }
            let valido = valor.length === 10 && !isNaN(valor);
            setFieldState(input, valido, 'Celular debe tener 10 números.');
            return valido;
        }

        function validarFotos(input) {
            let files = input.files;
            if (!files || files.length === 0) {
                input.removeAttribute('title');
                return true;
            }
            let maxPeso = 2 * 1024 * 1024;
            for (let i = 0; i < files.length; i++) {
                if (!files[i].type.startsWith('image/')) {
                    input.title = 'El archivo ' + files[i].name + ' no es una imagen válida.';
                    return false;
                }
                if (files[i].size > maxPeso) {
                    input.title = 'La imagen ' + files[i].name + ' supera los 2MB.';
                    return false;
                }
            }
            input.removeAttribute('title');
            return true;
        }

        var scrollPosition = 0;
        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function () {
            scrollPosition = document.documentElement.scrollTop || document.body.scrollTop;
        });
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            window.scrollTo(0, scrollPosition);
        });
    </script>
</body>
</html>