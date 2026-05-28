<%@ Page Title="Administrar Fotos de Productos" Language="C#" MasterPageFile="~/Site1.Master"
         AutoEventWireup="true" CodeBehind="FotosProductosGeneral.aspx.cs"
         Inherits="Monolito4bm.FotosProductosGeneral" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet"
          href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css"
          crossorigin="anonymous"/>
    <style>
    :root {
      --accent:#7a4aaa; --accent2:#5a2a8a;
      --danger:#c0392b; --success:#27ae60; --warn:#e67e22;
    }

    /* ── Encabezado ─────────────────────────────────── */
    .page-header {
      display:flex; align-items:center; justify-content:space-between;
      margin-bottom:24px; flex-wrap:wrap; gap:12px;
    }
    .page-title {
      font-size:1.5rem; font-weight:700; color:var(--accent2);
      display:flex; align-items:center; gap:10px;
    }
    .back-link {
      display:inline-flex; align-items:center; gap:7px;
      color:var(--accent); font-weight:700; font-size:.88rem;
      text-decoration:none; padding:8px 18px; border-radius:30px;
      border:1.5px solid rgba(122,74,170,0.3); transition:all .2s;
    }
    .back-link:hover { background:rgba(122,74,170,0.1); transform:translateX(-2px); }

    /* ── Cards ──────────────────────────────────────── */
    .card {
      background:rgba(255,255,255,0.72); backdrop-filter:blur(12px);
      border:1px solid rgba(180,150,220,0.4); border-radius:18px;
      padding:22px 26px; box-shadow:0 4px 24px rgba(120,80,180,0.10);
      margin-bottom:22px;
    }
    .card-title {
      font-size:.98rem; font-weight:700; color:var(--accent2);
      margin-bottom:16px; display:flex; align-items:center; gap:8px;
    }

    /* ── Form Controls ──────────────────────────────── */
    .form-control {
      background:rgba(255,255,255,0.72);
      border:1px solid rgba(180,150,220,0.4);
      color:#2c1a4a;
      font-family:inherit;
      padding:10px 14px;
      border-radius:10px;
      width:100%;
      max-width:400px;
      font-size:.88rem;
      box-shadow:inset 0 1px 3px rgba(0,0,0,0.05);
      transition:border-color .2s, box-shadow .2s;
    }
    .form-control:focus {
      border-color:var(--accent);
      outline:none;
      box-shadow:0 0 0 3px rgba(122,74,170,0.15);
    }
    .form-label {
      font-weight:700;
      color:var(--accent2);
      display:block;
      margin-bottom:6px;
      font-size:.88rem;
    }

    /* ── Upload zone ────────────────────────────────── */
    .upload-zone {
      border:2.5px dashed rgba(122,74,170,0.4); border-radius:14px;
      padding:28px 20px; text-align:center; cursor:pointer;
      transition:all .25s; background:rgba(122,74,170,0.03);
      position:relative;
    }
    .upload-zone:hover, .upload-zone.drag-over {
      border-color:var(--accent); background:rgba(122,74,170,0.08);
    }
    .upload-zone .uz-icon { font-size:2.2rem; color:var(--accent); margin-bottom:8px; }
    .upload-zone p { color:rgba(60,30,90,0.6); font-size:.88rem; margin:3px 0; }
    .upload-zone strong { color:var(--accent2); }
    .upload-zone input[type=file] {
      position:absolute; inset:0; opacity:0; cursor:pointer;
    }

    /* ── Preview mini ───────────────────────────────── */
    .preview-strip {
      display:flex; flex-wrap:wrap; gap:10px; margin-top:14px;
    }
    .preview-thumb {
      width:70px; height:70px; border-radius:8px; overflow:hidden;
      border:2px solid rgba(122,74,170,0.3);
      background:rgba(122,74,170,0.06);
    }
    .preview-thumb img { width:100%; height:100%; object-fit:cover; }
    .preview-count { font-size:.78rem; color:rgba(60,30,90,0.5); margin-top:6px; font-weight:600; }

    /* ── Botones generales ──────────────────────────── */
    .btn {
      padding:9px 20px; border-radius:30px; border:none; cursor:pointer;
      font-family:inherit; font-size:.86rem; font-weight:700;
      display:inline-flex; align-items:center; gap:7px;
      transition:all .2s; white-space:nowrap; text-decoration:none;
    }
    .btn i { pointer-events:none; }
    .btn-primary  { background:var(--accent); color:#fff; }
    .btn-primary:hover  { background:var(--accent2); transform:translateY(-1px); box-shadow:0 4px 12px rgba(90,42,138,.25); }
    .btn-secondary{ background:rgba(122,74,170,0.12); color:var(--accent2); border:1.5px solid rgba(122,74,170,0.3); }
    .btn-secondary:hover{ background:rgba(122,74,170,0.22); }
    .btn-danger   { background:var(--danger); color:#fff; }
    .btn-danger:hover   { background:#a93226; transform:translateY(-1px); }
    .btn-success  { background:var(--success); color:#fff; }
    .btn-success:hover  { background:#1e8449; transform:translateY(-1px); }
    .btn-sm { padding:5px 12px; font-size:.76rem; border-radius:20px; }

    /* ── Alertas ────────────────────────────────────── */
    .alert {
      padding:11px 16px; border-radius:12px; margin-bottom:14px;
      font-size:.86rem; font-weight:600;
      display:flex; align-items:center; gap:9px;
      animation:fadeIn .3s ease;
    }
    .alert-success { background:rgba(39,174,96,.15); color:#1e8449; border:1px solid rgba(39,174,96,.3); }
    .alert-danger  { background:rgba(192,57,43,.12); color:#c0392b; border:1px solid rgba(192,57,43,.25); }
    @keyframes fadeIn { from{opacity:0;transform:translateY(-4px)} to{opacity:1;transform:none} }

    /* ── Tabla ──────────────────────────────────────── */
    .fotos-table {
      width:100%; border-collapse:collapse; font-size:.87rem;
    }
    .fotos-table thead tr {
      background:linear-gradient(90deg,var(--accent),var(--accent2));
      color:#fff;
    }
    .fotos-table thead th {
      padding:11px 14px; text-align:left;
      font-size:.78rem; font-weight:700; letter-spacing:.4px;
      white-space:nowrap;
    }
    .fotos-table thead th:first-child { border-radius:12px 0 0 0; }
    .fotos-table thead th:last-child  { border-radius:0 12px 0 0; }
    .fotos-table tbody tr {
      border-bottom:1px solid rgba(180,150,220,0.2);
      transition:background .15s;
    }
    .fotos-table tbody tr:hover { background:rgba(122,74,170,0.05); }
    .fotos-table tbody td { padding:10px 14px; vertical-align:middle; }

    .foto-thumb {
      width:64px; height:64px; border-radius:8px; overflow:hidden;
      border:2px solid rgba(180,150,220,0.35);
      background:rgba(122,74,170,0.06); flex-shrink:0;
    }
    .foto-thumb img { width:100%; height:100%; object-fit:cover; display:block; }
    
    .prod-name-cell {
      font-weight:600; color:var(--accent2);
      display:flex; align-items:center; gap:8px;
    }
    .prod-name-cell span.sub {
      font-size:.74rem; font-weight:400; color:#aaa; display:block;
    }

    .badge {
      display:inline-block; padding:3px 10px; border-radius:20px;
      font-size:.72rem; font-weight:700; letter-spacing:.3px;
    }
    .badge-activo   { background:rgba(39,174,96,.15);  color:#1e8449; }
    .badge-inactivo { background:rgba(192,57,43,.12);   color:#c0392b; }

    .row-actions { display:flex; gap:7px; flex-wrap:wrap; }

    .empty-state {
      text-align:center; padding:38px 20px;
      color:rgba(60,30,90,0.4); font-size:.93rem;
    }
    .empty-state i { font-size:2.8rem; display:block; margin-bottom:10px; color:rgba(122,74,170,0.22); }

    @media(max-width:600px){
      .card { padding:14px 12px; }
      .fotos-table thead { display:none; }
      .fotos-table tbody tr {
        display:flex; flex-wrap:wrap; padding:12px; gap:8px;
        border-radius:12px; margin-bottom:10px;
        border:1px solid rgba(180,150,220,0.3);
      }
      .fotos-table tbody td { padding:2px 4px; }
    }
    </style>
</asp:Content>

<asp:Content ID="bodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- ══ Encabezado ══════════════════════════════════════════════ -->
    <div class="page-header">
        <div class="page-title">
            <i class="fa-solid fa-images" style="color:var(--accent)"></i>
            Administración General de Fotos
        </div>
        <a href="Productos.aspx" class="back-link">
            <i class="fa-solid fa-arrow-left"></i> Volver a Productos
        </a>
    </div>

    <!-- ══ Mensajes ════════════════════════════════════════════════ -->
    <asp:Literal ID="litMensaje" runat="server"/>

    <!-- ══ Carga Masiva Excel ══════════════════════════════════════ -->
    <div class="card">
        <div class="card-title">
            <i class="fa-solid fa-file-excel" style="color:var(--success)"></i>
            Carga Masiva de Fotos por Excel
        </div>
        <p style="font-size:.88rem;color:rgba(60,30,90,0.6);margin-bottom:16px;">
            Descarga el formato de plantilla de Excel, complétalo con los datos correspondientes, y súbelo para asociar múltiples fotos.
        </p>
        <div style="display:flex;gap:12px;flex-wrap:wrap;align-items:center;">
            <asp:Button ID="btnDescargarFormato" runat="server" CssClass="btn btn-secondary"
                        Text="Descargar Formato" OnClick="btnDescargarFormato_Click" />
            
            <div style="position:relative;display:inline-block;">
                <asp:FileUpload ID="fuExcel" runat="server" accept=".xlsx,.xls" style="display:none;" onchange="document.getElementById('lblExcelName').textContent = this.files[0] ? this.files[0].name : '';" />
                <button type="button" class="btn btn-secondary" onclick="document.getElementById('<%= fuExcel.ClientID %>').click();">
                    <i class="fa-solid fa-file-excel"></i> Seleccionar Excel
                </button>
                <span id="lblExcelName" style="font-size:.82rem;color:rgba(60,30,90,0.55);margin-left:8px;font-weight:600;"></span>
            </div>
            
            <asp:Button ID="btnCargarExcel" runat="server" CssClass="btn btn-success"
                        Text="Cargar Excel" OnClick="btnCargarExcel_Click" />
        </div>
    </div>

    <!-- ══ Subir fotos ════════════════════════════════════════════ -->
    <div class="card">
        <div class="card-title">
            <i class="fa-solid fa-upload"></i> Subir Fotos Individuales/Múltiples
            <span style="font-size:.76rem;color:#aaa;font-weight:400;margin-left:4px;">
                (JPG o PNG &mdash; Máx. 2&nbsp;MB por foto)
            </span>
        </div>

        <div style="margin-bottom:20px;">
            <label class="form-label" for="<%= ddlProducto.ClientID %>">Asociar al Producto:</label>
            <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-control" />
        </div>

        <div class="upload-zone" id="uploadZone">
            <div class="uz-icon"><i class="fa-solid fa-cloud-arrow-up"></i></div>
            <p><strong>Haz clic o arrastra</strong> las imágenes aquí</p>
            <p style="font-size:.78rem;">JPG, PNG hasta 2&nbsp;MB</p>
            <asp:FileUpload ID="fuFotos" runat="server"
                            AllowMultiple="true"
                            accept="image/jpeg,image/png"
                            onchange="previewFotos(this)"/>
        </div>

        <div class="preview-strip" id="previewStrip"></div>
        <div class="preview-count" id="previewCount"></div>

        <div style="margin-top:16px;display:flex;gap:12px;flex-wrap:wrap;">
            <asp:Button ID="btnSubir" runat="server" CssClass="btn btn-primary"
                        Text="Subir fotos" OnClick="btnSubir_Click"/>
        </div>
    </div>

    <!-- ══ Lista de fotos general ══════════════════════════════════ -->
    <div class="card">
        <div class="card-title">
            <i class="fa-solid fa-images"></i> Todas las fotos guardadas
            <span style="margin-left:auto;font-size:.79rem;color:#aaa;font-weight:400;">
                <asp:Literal ID="litTotalFotos" runat="server"/>
            </span>
        </div>

        <div style="overflow-x:auto;border-radius:12px;">
            <asp:Repeater ID="rptFotos" runat="server"
                          OnItemCommand="rptFotos_ItemCommand">

                <HeaderTemplate>
                    <table class="fotos-table">
                        <thead>
                            <tr>
                                <th style="width:76px;">FOTO</th>
                                <th>PRODUCTO</th>
                                <th style="width:80px;">ID FOTO</th>
                                <th style="width:90px;">ESTADO</th>
                                <th>FECHA SUBIDA</th>
                                <th style="width:210px;">ACCIONES</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr>
                        <!-- Miniatura -->
                        <td>
                            <div class="foto-thumb">
                                <img src='<%# ResolveUrl("~/" + Eval("foto_ruta")) %>'
                                     alt="Foto <%# Eval("foto_id") %>"
                                     onerror="this.onerror=null;this.src='ImagenProductoFallback.ashx?id=<%# Eval("foto_id") %>';" />
                            </div>
                        </td>

                        <!-- Nombre del producto -->
                        <td>
                            <div class="prod-name-cell">
                                <i class="fa-solid fa-box" style="color:var(--accent);font-size:.85rem;"></i>
                                <div>
                                    <%# Eval("pro_nombre") %>
                                    <span class="sub">ID prod.: <%# Eval("pro_id") %></span>
                                </div>
                            </div>
                        </td>

                        <!-- ID foto -->
                        <td style="color:#aaa;font-size:.8rem;">#<%# Eval("foto_id") %></td>

                        <!-- Estado -->
                        <td>
                            <span class='badge <%# (char)Eval("foto_estado") == 'A' ? "badge-activo" : "badge-inactivo" %>'>
                                <%# (char)Eval("foto_estado") == 'A' ? "Activa" : "Inactiva" %>
                            </span>
                        </td>

                        <!-- Fecha subida -->
                        <td style="font-size:.8rem;color:#999;">
                            <%# Eval("fecha_subida", "{0:dd/MM/yyyy HH:mm}") %>
                        </td>

                        <!-- Acciones -->
                        <td>
                            <div class="row-actions">
                                <%-- Desactivar / Reactivar segun estado --%>
                                <asp:LinkButton runat="server"
                                    CommandName='<%# Eval("foto_estado").ToString() == "A" ? "Desactivar" : "Reactivar" %>'
                                    CommandArgument='<%# Eval("foto_id") %>'
                                    CssClass='<%# "btn btn-sm " + (Eval("foto_estado").ToString() == "A" ? "btn-secondary" : "btn-success") %>'
                                    OnClientClick='<%# Eval("foto_estado").ToString() == "A"
                                        ? "return confirm(\"¿Desactivar esta foto?\");"
                                        : "return confirm(\"¿Reactivar esta foto?\");" %>'>
                                    <i class='<%# (char)Eval("foto_estado") == 'A' ? "fa-solid fa-eye-slash" : "fa-solid fa-eye" %>'></i>
                                    <%# (char)Eval("foto_estado") == 'A' ? " Desactivar" : " Reactivar" %>
                                </asp:LinkButton>

                                <%-- Eliminar permanente --%>
                                <asp:LinkButton runat="server"
                                    CommandName="ElimFis"
                                    CommandArgument='<%# Eval("foto_id") %>'
                                    CssClass="btn btn-danger btn-sm"
                                    OnClientClick="return confirm('¿Eliminar esta foto PERMANENTEMENTE?');">
                                    <i class="fa-solid fa-trash"></i> Eliminar
                                </asp:LinkButton>
                            </div>
                        </td>
                    </tr>
                </ItemTemplate>

                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>

            </asp:Repeater>
        </div>

        <asp:Literal ID="litSinFotos" runat="server"/>
    </div>

    <script>
        function previewFotos(input) {
            var strip = document.getElementById('previewStrip');
            var count = document.getElementById('previewCount');
            strip.innerHTML = '';

            var files = Array.from(input.files);
            var maxBytes = 2 * 1024 * 1024; // 2 MB en bytes
            var validFiles = [];

            for (var i = 0; i < files.length; i++) {
                // Validar extensión
                var ext = files[i].name.split('.').pop().toLowerCase();
                if (ext !== 'jpg' && ext !== 'jpeg' && ext !== 'png') {
                    Swal.fire('Formato no válido', 'El archivo "' + files[i].name + '" no es JPG ni PNG.', 'warning');
                    input.value = ''; // Limpiamos el input
                    count.textContent = '';
                    return;
                }
                
                // Validar peso
                if (files[i].size > maxBytes) {
                    Swal.fire('Archivo muy pesado', 'La foto "' + files[i].name + '" supera los 2 MB permitidos.', 'warning');
                    input.value = ''; // Limpiamos el input
                    count.textContent = '';
                    return; // Detenemos el preview
                }
                validFiles.push(files[i]);
            }

            count.textContent = validFiles.length + ' foto(s) seleccionada(s)';
            validFiles.forEach(function (file) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var div = document.createElement('div');
                    div.className = 'preview-thumb';
                    div.innerHTML = "<img src='" + e.target.result + "' alt='preview'/>";
                    strip.appendChild(div);
                };
                reader.readAsDataURL(file);
            });
        }

        // Drag & drop sobre la zona
        var zone = document.getElementById('uploadZone');
        if (zone) {
            zone.addEventListener('dragover', function (e) { e.preventDefault(); zone.classList.add('drag-over'); });
            zone.addEventListener('dragleave', function () { zone.classList.remove('drag-over'); });
            zone.addEventListener('drop', function (e) {
                e.preventDefault(); zone.classList.remove('drag-over');
                var inp = zone.querySelector('input[type=file]');
                inp.files = e.dataTransfer.files;
                previewFotos(inp);
            });
        }
    </script>
</asp:Content>
