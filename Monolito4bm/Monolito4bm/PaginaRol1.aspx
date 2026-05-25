<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaginaRol1.aspx.cs" Inherits="Monolito4bm.PaginaRol1" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Panel Administrador</title>
    
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #e6e6fa 0%, #d8bfd8 100%); /* Lavanda Pastel */
            color: #4a4a4a;
            min-height: 100vh;
        }
        
        .page {
            max-width: 1300px;
            margin: 0 auto;
            padding: 40px 20px;
        }
        
        /* Glassmorphism Header */
        .header {
            background: rgba(255, 255, 255, 0.55);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            border: 1px solid rgba(255, 255, 255, 0.8);
            border-radius: 16px;
            padding: 25px 30px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 16px;
            margin-bottom: 25px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
        }
        
        .title h1 {
            margin: 0 0 8px;
            color: #5d3f6a;
            font-size: 2rem;
            font-weight: 700;
        }
        
        .title p { margin: 0; color: #666; font-weight: 600; }
        
        .btn {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 12px 20px;
            border-radius: 10px;
            border: none;
            cursor: pointer;
            font-weight: 700;
            font-size: 1rem;
            transition: .2s ease;
            text-decoration: none;
        }
        
        .btn-primary {
            background: #ff8da1; 
            color: #fff;
            box-shadow: 0 4px 10px rgba(255, 141, 161, 0.3);
        }
        .btn-primary:hover { background: #ff6b8b; transform: translateY(-2px); }
        
        .btn-secondary {
            background: rgba(255, 255, 255, 0.8);
            color: #ff6b8b;
            border: 2px solid #ff8da1;
        }
        .btn-secondary:hover { background: #ff8da1; color: #fff; }
        
        .card {
            background: rgba(255, 255, 255, 0.7);
            backdrop-filter: blur(12px);
            border: 1px solid rgba(255, 255, 255, 0.8);
            border-radius: 16px;
            padding: 30px;
            box-shadow: 0 15px 40px rgba(0,0,0,0.15);
        }
        
        .table-wrap { overflow-x: auto; border-radius: 10px;}
        
        .grid {
            width: 100%;
            border-collapse: collapse;
            min-width: 1000px;
            background: rgba(255,255,255,0.9);
            border-radius: 10px;
            overflow: hidden;
        }
        .grid th, .grid td {
            padding: 16px 15px;
            border-bottom: 1px solid #f0e6ff;
            text-align: left;
            vertical-align: middle;
            color: #555;
            font-weight: 500;
        }
        .grid th {
            background: #e0c3fc; 
            color: #5d3f6a;
            font-size: 0.95rem;
            font-weight: 700;
        }
        .grid tr:hover { background: #fdf5ff; }
        
        .badge {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 999px;
            font-size: .85rem;
            font-weight: 700;
        }
        .badge.ok { background: #dcfce7; color: #166534; border: 1px solid #86efac; }
        .badge.warn { background: #ffe4e6; color: #e11d48; border: 1px solid #fda4af; }
        
        .actions { display: flex; gap: 8px; flex-wrap: wrap; }
        
        .inline-btn {
            padding: 8px 12px;
            border-radius: 8px;
            border: none;
            cursor: pointer;
            font-size: .85rem;
            font-weight: 700;
            transition: 0.2s;
        }
        .inline-reset { background: #f3e8ff; color: #7e22ce; border: 1px solid #d8b4fe; }
        .inline-reset:hover { background: #e9d5ff; }
        
        .inline-unlock { background: #ff8da1; color: #fff; }
        .inline-unlock:hover { background: #ff6b8b; }
        
        @media (max-width: 720px) {
            .header { flex-direction: column; align-items: stretch; text-align: center; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page">
            <div class="header">
                <div class="title">
                    <h1><i class="fa-solid fa-user-shield"></i> Administrador</h1>
                    <p>Revisa intentos fallidos y desbloquea usuarios.</p>
                </div>
                <asp:Button ID="btnCerrarSesion" runat="server" Text="Cerrar sesion" CssClass="btn btn-secondary" OnClick="btnCerrarSesion_Click" CausesValidation="false" />
            </div>

            <div class="card">
                <div style="margin-bottom:20px; display: flex; justify-content: flex-end;">
                    <asp:Button ID="btnRecargar" runat="server" Text="Actualizar listado" CssClass="btn btn-primary" OnClick="btnRecargar_Click" CausesValidation="false" />
                </div>

                <div class="table-wrap">
                    <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="grid"
                        GridLines="None" OnRowCommand="gvUsuarios_RowCommand" EmptyDataText="No se encontraron cuentas registradas.">
                        <Columns>
                            <asp:BoundField DataField="usu_id" HeaderText="ID" />
                            <asp:BoundField DataField="usu_nombres" HeaderText="Nombre" />
                            <asp:BoundField DataField="usu_nickname" HeaderText="Usuario" />
                            <asp:BoundField DataField="correo_electronico" HeaderText="Correo" />
                            <asp:BoundField DataField="rol_id" HeaderText="Rol" />
                            <asp:BoundField DataField="intentos_fallidos" HeaderText="Intentos" />
                            
                            <asp:BoundField DataField="ultimo_intento" HeaderText="Ultimo intento" DataFormatString="{0:dd/MM/yyyy HH:mm}" HtmlEncode="false" />
                            
                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate>
                                    <span class='badge <%# EsCuentaBloqueada(Eval("estado_cuenta")) ? "warn" : "ok" %>'>
                                        <%# Eval("estado_cuenta") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <div class="actions">
                                        
                                        <!-- SOLO visible si NO está bloqueado (!EsCuentaBloqueada) -->
                                        <!-- Sin tildes ni símbolos raros en el texto de JavaScript -->
                                        <asp:Button ID="btnResetear" runat="server" Text="Resetear intentos"
                                            CssClass="inline-btn inline-reset"
                                            CommandName="ResetearIntentos"
                                            CommandArgument='<%# Eval("usu_id") %>'
                                            CausesValidation="false" 
                                            Visible='<%# !EsCuentaBloqueada(Eval("estado_cuenta")) %>'
                                            OnClientClick="return confirmarAccion(this, 'Resetear Intentos', 'Seguro que quiere resetear los intentos de este usuario?');" />
                                            
                                        <!-- SOLO visible si SÍ está bloqueado (EsCuentaBloqueada) -->
                                        <asp:Button ID="btnDesbloquear" runat="server" Text="Desbloquear y enviar clave"
                                            CssClass="inline-btn inline-unlock"
                                            CommandName="DesbloquearCuenta"
                                            CommandArgument='<%# Eval("usu_id") %>'
                                            CausesValidation="false"
                                            Visible='<%# EsCuentaBloqueada(Eval("estado_cuenta")) %>'
                                            OnClientClick="return confirmarAccion(this, 'Desbloquear Cuenta', 'Seguro quiere desbloquear a este usuario? Se resetearan sus intentos y se le enviara una clave temporal.');" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>

    <script>
        function confirmarAccion(btn, titulo, texto) {
            if (btn.dataset.confirmado === 'true') {
                return true;
            }

            event.preventDefault();

            Swal.fire({
                title: titulo,
                text: texto,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ff8da1',
                cancelButtonColor: '#6c757d',
                // Sin tilde en el "Sí"
                confirmButtonText: 'Si, continuar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    btn.dataset.confirmado = 'true';
                    btn.click();
                }
            });

            return false;
        }
    </script>
</body>
</html>