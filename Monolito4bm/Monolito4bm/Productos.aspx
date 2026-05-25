<%@ Page Title="Productos" Language="C#" MasterPageFile="~/Site1.Master"
         AutoEventWireup="true" CodeBehind="Productos.aspx.cs" Inherits="Monolito4bm.Productos" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<link rel="stylesheet"
      href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css"
      crossorigin="anonymous"/>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<style>
:root {
  --card-bg:  rgba(255,255,255,0.72);
  --radius:   18px;
  --accent:   #7a4aaa;
  --accent2:  #5a2a8a;
  --danger:   #c0392b;
  --success:  #27ae60;
  --warn:     #e67e22;
}

/* ── Encabezado ─────────────────────────────────────────────── */
.page-header {
  display:flex; align-items:center; justify-content:space-between;
  margin-bottom:24px; flex-wrap:wrap; gap:12px;
}
.page-title {
  font-size:1.65rem; font-weight:700; color:var(--accent2);
  display:flex; align-items:center; gap:10px;
}

/* ── Cards ──────────────────────────────────────────────────── */
.card {
  background:var(--card-bg); backdrop-filter:blur(12px);
  border:1px solid rgba(180,150,220,0.4);
  border-radius:var(--radius); padding:22px 28px;
  box-shadow:0 4px 24px rgba(120,80,180,0.10);
  margin-bottom:24px;
}
.card-title {
  font-size:.98rem; font-weight:700; color:var(--accent2);
  margin-bottom:16px; display:flex; align-items:center; gap:8px;
}

/* ── Buscador ───────────────────────────────────────────────── */
.search-bar {
  display:flex; align-items:center; gap:10px;
  background:rgba(255,255,255,0.9); border:1.5px solid rgba(122,74,170,0.3);
  border-radius:40px; padding:9px 18px;
  box-shadow:0 2px 10px rgba(120,80,180,.07); margin-bottom:14px;
  transition:border-color .2s, box-shadow .2s;
}
.search-bar:focus-within {
  border-color:var(--accent); box-shadow:0 0 0 3px rgba(122,74,170,0.12);
}
.search-bar input {
  border:none; background:transparent; font-family:inherit;
  font-size:.95rem; color:#2c1a4a; flex:1; outline:none;
}
.search-bar .si { color:var(--accent); font-size:1rem; }

/* ── Filtros ────────────────────────────────────────────────── */
.filtros-toggle {
  background:none; border:none; cursor:pointer; font-family:inherit;
  font-size:.83rem; font-weight:700; color:var(--accent);
  display:flex; align-items:center; gap:6px; padding:4px 0; margin-bottom:10px;
}
.filtros-panel {
  display:none; gap:14px; flex-wrap:wrap; padding:16px;
  background:rgba(122,74,170,0.04); border-radius:14px;
  border:1px solid rgba(122,74,170,0.14); margin-bottom:14px;
}
.filtros-panel.open { display:flex; animation:fadeIn .22s ease; }
@keyframes fadeIn { from{opacity:0;transform:translateY(-4px)} to{opacity:1;transform:none} }

.fg { display:flex; flex-direction:column; gap:5px; min-width:150px; flex:1; }
.fg label { font-size:.76rem; font-weight:700; color:var(--accent2); letter-spacing:.3px; }

/* Precio / stock — grupo de rango */
.range-group {
  display:flex; align-items:center; gap:6px; flex:1; min-width:220px;
}
.range-group .fg { min-width:80px; }
.range-sep { font-size:.8rem; color:#aaa; margin-top:20px; }

.form-control {
  padding:9px 13px; border-radius:10px;
  border:1.5px solid rgba(122,74,170,0.28);
  background:rgba(255,255,255,0.88); font-family:inherit;
  font-size:.88rem; color:#2c1a4a; width:100%;
  transition:border-color .2s, box-shadow .2s;
}
.form-control:focus {
  outline:none; border-color:var(--accent);
  box-shadow:0 0 0 3px rgba(122,74,170,0.13);
}

/* ── Botones ────────────────────────────────────────────────── */
.btn {
  padding:9px 20px; border-radius:30px; border:none; cursor:pointer;
  font-family:inherit; font-size:.86rem; font-weight:700;
  display:inline-flex; align-items:center; gap:7px;
  transition:all .2s; white-space:nowrap; text-decoration:none;
}
.btn-primary   { background:var(--accent);  color:#fff; }
.btn-primary:hover   { background:var(--accent2); transform:translateY(-1px); box-shadow:0 4px 14px rgba(90,42,138,.28); }
.btn-secondary { background:rgba(122,74,170,0.11); color:var(--accent2); border:1.5px solid rgba(122,74,170,0.28); }
.btn-secondary:hover { background:rgba(122,74,170,0.2); }
.btn-success   { background:var(--success); color:#fff; }
.btn-success:hover   { background:#1e8449; transform:translateY(-1px); }
.btn-danger    { background:var(--danger);  color:#fff; }
.btn-danger:hover    { background:#a93226;  transform:translateY(-1px); }
.btn-warn      { background:var(--warn);    color:#fff; }
.btn-warn:hover      { background:#d35400; }
.btn-sm  { padding:5px 13px; font-size:.76rem; border-radius:20px; }

/* ── Alertas ────────────────────────────────────────────────── */
.alert {
  padding:11px 16px; border-radius:12px; margin-bottom:14px;
  font-size:.86rem; font-weight:600; display:flex; align-items:center; gap:9px;
  animation:fadeIn .3s ease;
}
.alert-success { background:rgba(39,174,96,.14); color:#1e8449; border:1px solid rgba(39,174,96,.28); }
.alert-danger  { background:rgba(192,57,43,.11); color:#c0392b; border:1px solid rgba(192,57,43,.24); }
.alert-warning { background:rgba(230,126,34,.12); color:#9a5b00; border:1px solid rgba(230,126,34,.28); }

.bulk-upload-layout {
  display:grid; grid-template-columns:minmax(0, 1.4fr) minmax(280px, .9fr);
  gap:18px; align-items:start;
}
.bulk-upload-box {
  position:relative; border:2px dashed rgba(122,74,170,0.32); border-radius:18px;
  padding:22px 20px; background:linear-gradient(135deg, rgba(122,74,170,0.05), rgba(255,255,255,0.92));
  min-height:180px;
}
.bulk-upload-box:hover { border-color:var(--accent); }
.bulk-upload-box input[type=file] {
  position:absolute; inset:0; opacity:0; cursor:pointer;
}
.bulk-upload-copy {
  display:flex; flex-direction:column; gap:8px; max-width:460px;
}
.bulk-upload-copy strong { color:var(--accent2); font-size:1.02rem; }
.bulk-upload-copy p { margin:0; color:#5f4a7f; font-size:.88rem; line-height:1.5; }
.bulk-file-pill {
  display:inline-flex; align-items:center; gap:8px; width:fit-content;
  max-width:100%; padding:8px 14px; border-radius:999px; font-size:.82rem; font-weight:700;
  background:rgba(122,74,170,0.12); color:var(--accent2); border:1px solid rgba(122,74,170,0.18);
}
.bulk-meta {
  display:grid; gap:12px;
}
.bulk-note {
  border-radius:14px; padding:14px 16px; font-size:.83rem; line-height:1.5;
  border:1px solid rgba(122,74,170,0.16); background:rgba(122,74,170,0.04); color:#4c356d;
}
.bulk-note strong { color:var(--accent2); }
.bulk-actions {
  display:flex; gap:10px; flex-wrap:wrap; margin-top:16px;
}
.bulk-warning {
  border-left:4px solid var(--warn); background:rgba(230,126,34,0.1);
}
.bulk-columns {
  margin:0; padding-left:18px; color:#4c356d; font-size:.82rem; line-height:1.55;
}

/* ── GridView ───────────────────────────────────────────────── */
.grid-wrapper { overflow-x:auto; border-radius:14px; }
.prod-grid { width:100%; border-collapse:collapse; font-size:.87rem; }
.prod-grid thead tr {
  background:linear-gradient(90deg,var(--accent),var(--accent2)); color:#fff;
}
.prod-grid thead th {
  padding:12px 14px; text-align:left;
  font-size:.78rem; font-weight:700; letter-spacing:.5px; white-space:nowrap;
}
.prod-grid thead th:first-child { border-radius:13px 0 0 0; }
.prod-grid thead th:last-child  { border-radius:0 13px 0 0; }
.prod-grid tbody tr { border-bottom:1px solid rgba(180,150,220,0.2); transition:background .15s; }
.prod-grid tbody tr:hover { background:rgba(122,74,170,0.05); }
.prod-grid tbody td { padding:10px 14px; vertical-align:middle; }

/* ── Carrusel ───────────────────────────────────────────────── */
.carousel-cell {
  position:relative; width:110px; height:80px;
  border-radius:10px; overflow:hidden;
  background:rgba(122,74,170,0.08);
}
.carousel-cell .slide { position:absolute; inset:0; opacity:0; transition:opacity .5s; }
.carousel-cell .slide.active { opacity:1; }
.carousel-cell img { width:100%; height:100%; object-fit:cover; border-radius:10px; }
.carousel-cell .prev, .carousel-cell .next {
  position:absolute; top:50%; transform:translateY(-50%);
  background:rgba(0,0,0,0.45); color:#fff; border:none; cursor:pointer;
  border-radius:50%; width:22px; height:22px; font-size:.6rem;
  display:flex; align-items:center; justify-content:center;
  transition:background .2s; z-index:2;
}
.carousel-cell .prev { left:3px; }
.carousel-cell .next { right:3px; }
.carousel-cell .prev:hover, .carousel-cell .next:hover { background:rgba(0,0,0,0.75); }
.carousel-cell .dots { position:absolute; bottom:4px; left:50%; transform:translateX(-50%); display:flex; gap:4px; }
.carousel-cell .dot { width:5px; height:5px; border-radius:50%; background:rgba(255,255,255,0.5); cursor:pointer; transition:background .2s; }
.carousel-cell .dot.on { background:#fff; }
.no-foto {
  width:110px; height:80px; border-radius:10px;
  display:flex; align-items:center; justify-content:center;
  background:rgba(122,74,170,0.07); color:rgba(122,74,170,0.35); font-size:1.8rem;
}

/* ── Badge ──────────────────────────────────────────────────── */
.badge {
  display:inline-flex; align-items:center; gap:5px;
  padding:3px 11px; border-radius:20px;
  font-size:.72rem; font-weight:700; letter-spacing:.3px;
}
.badge-activo   { background:rgba(39,174,96,.14);  color:#1e8449; }
.badge-inactivo { background:rgba(192,57,43,.11);   color:#c0392b; }

/* ── Paginador ──────────────────────────────────────────────── */
.pager-wrap {
  display:flex; align-items:center; justify-content:space-between;
  flex-wrap:wrap; gap:12px; margin-top:16px;
}
.pager-info { font-size:.8rem; color:#999; }
.pager-btns { display:flex; align-items:center; gap:6px; }
.pager-btn {
  min-width:34px; height:34px; border-radius:50%;
  border:1.5px solid rgba(122,74,170,0.28); background:rgba(255,255,255,0.8);
  color:var(--accent2); font-weight:700; font-size:.84rem; cursor:pointer;
  display:flex; align-items:center; justify-content:center;
  transition:all .2s; font-family:inherit;
}
.pager-btn:hover { background:var(--accent); color:#fff; border-color:var(--accent); }
.pager-btn.active { background:var(--accent); color:#fff; border-color:var(--accent); }
.pager-btn:disabled { opacity:.4; cursor:default; }

/* ── Modal ──────────────────────────────────────────────────── */
.modal-overlay {
  display:none; position:fixed; inset:0;
  background:rgba(40,20,70,0.52); backdrop-filter:blur(6px);
  z-index:999; align-items:center; justify-content:center;
}
.modal-overlay.open { display:flex; }
.modal-box {
  background:rgba(246,242,255,0.97); border-radius:22px;
  padding:32px 34px; max-width:490px; width:93%;
  box-shadow:0 20px 60px rgba(90,42,138,0.28);
  animation:popIn .3s cubic-bezier(.34,1.56,.64,1);
  max-height:90vh; overflow-y:auto;
}
@keyframes popIn {
  from { opacity:0; transform:scale(.88) translateY(18px); }
  to   { opacity:1; transform:none; }
}
.modal-title   { font-size:1.08rem; font-weight:700; color:var(--accent2); margin-bottom:18px; }
.modal-row     { display:flex; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
.modal-actions { display:flex; gap:10px; justify-content:flex-end; margin-top:6px; }

@media(max-width:600px){
  .card { padding:14px 12px; }
  .modal-box { padding:20px 16px; }
  .modal-row { flex-direction:column; }
  .range-group { flex-direction:column; }
  .bulk-upload-layout { grid-template-columns:1fr; }
  .bulk-actions { flex-direction:column; }
  .bulk-actions .btn { justify-content:center; width:100%; }
}
</style>
</asp:Content>

<asp:Content ID="bodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <!-- ══ Encabezado ════════════════════════════════════════════ -->
  <div class="page-header">
    <div class="page-title">
      <i class="fa-solid fa-box" style="color:var(--accent)"></i> Productos
    </div>
    <button class="btn btn-primary" onclick="abrirModal(); return false;">
      <i class="fa-solid fa-plus"></i> Nuevo Producto
    </button>
  </div>

  <!-- ══ Mensajes ══════════════════════════════════════════════ -->
  <asp:Literal ID="litMensaje" runat="server"/>

  <div class="card">
    <div class="card-title">
      <i class="fa-solid fa-file-arrow-up"></i> Carga masiva de productos
    </div>

    <asp:HiddenField ID="hfModoCarga" runat="server" Value="" />

    <div class="bulk-upload-layout">
      <div>
        <label class="bulk-upload-box" for="<%= fuCargaProductos.ClientID %>">
          <asp:FileUpload ID="fuCargaProductos" runat="server"
                          accept=".csv,.xlsx,.xls"
                          onchange="validarArchivoCarga(this);" />
          <div class="bulk-upload-copy">
            <strong>Selecciona un archivo Excel o CSV</strong>
            <p>Formatos permitidos: <code>.csv</code>, <code>.xlsx</code> y <code>.xls</code>. La validación del formato se ejecuta antes de cualquier envío al servidor.</p>
            <span class="bulk-file-pill" id="bulkFileName">
              <i class="fa-solid fa-file-circle-plus"></i> Ningún archivo seleccionado
            </span>
          </div>
        </label>

        <div class="bulk-actions">
          <asp:Button ID="btnCargaIncremental" runat="server" CssClass="btn btn-success"
                      Text="Carga incremental"
                      OnClientClick="return prepararCarga('append', false);"
                      OnClick="btnCargaMasiva_Click" />
          <asp:Button ID="btnCargaTotal" runat="server" CssClass="btn btn-danger"
                      Text="Sobrescribir todo"
                      OnClientClick="return prepararCarga('overwrite', true);"
                      OnClick="btnCargaMasiva_Click" />
        </div>
      </div>

      <div class="bulk-meta">
        <div class="bulk-note">
          <strong>Columnas esperadas</strong>
          <ol class="bulk-columns">
            <li><code>NombreProducto</code> o <code>Producto</code></li>
            <li><code>Proveedor</code></li>
            <li><code>Cantidad</code> o <code>Stock</code></li>
            <li><code>Precio</code></li>
            <li><code>ImagenReferencia</code> o <code>FotoRuta</code> (opcional)</li>
          </ol>
        </div>
        <div class="bulk-note bulk-warning">
          <strong>Sobrescritura total</strong>
          <div>Este flujo limpia primero <code>FotosProducto</code>, reinicia <code>Producto</code>, reindexa identidad y vuelve a insertar productos y referencias de imagen dentro de una única transacción.</div>
        </div>
      </div>
    </div>
  </div>

  <!-- ══ Buscador + Filtros ════════════════════════════════════ -->
  <div class="card">

    <div class="search-bar">
      <span class="si"><i class="fa-solid fa-magnifying-glass"></i></span>
      <asp:TextBox ID="txtBuscar" runat="server"
                   placeholder="Buscar producto por nombre..."
                   AutoPostBack="true" OnTextChanged="Buscar_Changed"/>
    </div>

    <button class="filtros-toggle" onclick="toggleFiltros(); return false;">
      <i class="fa-solid fa-sliders"></i> Filtros avanzados
      <span id="arrowFilt"><i class="fa-solid fa-chevron-down"></i></span>
    </button>

    <div class="filtros-panel" id="filtrosPanel">

      
      <div class="fg">
        <label>Proveedor</label>
        <asp:DropDownList ID="ddlFiltroProveedor" runat="server" CssClass="form-control"
                          AutoPostBack="true" OnSelectedIndexChanged="Buscar_Changed"/>
      </div>

      
      <div class="fg" style="max-width:170px;">
        <label>Estado</label>
        <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control"
                          AutoPostBack="true" OnSelectedIndexChanged="Buscar_Changed">
          <asp:ListItem Value=""  Text="Todos los estados"/>
          <asp:ListItem Value="A" Text="Solo activos"/>
          <asp:ListItem Value="I" Text="Solo inactivos"/>
        </asp:DropDownList>
      </div>

     
      <div class="range-group">
        <div class="fg">
          <label>Precio minimo</label>
          <asp:TextBox ID="txtPrecioMin" runat="server" CssClass="form-control"
                       placeholder="0.00" TextMode="Number"
                       AutoPostBack="true" OnTextChanged="Buscar_Changed"/>
        </div>
        <span class="range-sep">—</span>
        <div class="fg">
          <label>Precio maximo</label>
          <asp:TextBox ID="txtPrecioMax" runat="server" CssClass="form-control"
                       placeholder="9999" TextMode="Number"
                       AutoPostBack="true" OnTextChanged="Buscar_Changed"/>
        </div>
      </div>

      
      <div class="range-group">
        <div class="fg">
          <label>Stock minimo</label>
          <asp:TextBox ID="txtStockMin" runat="server" CssClass="form-control"
                       placeholder="0" TextMode="Number"
                       AutoPostBack="true" OnTextChanged="Buscar_Changed"/>
        </div>
        <span class="range-sep">—</span>
        <div class="fg">
          <label>Stock maximo</label>
          <asp:TextBox ID="txtStockMax" runat="server" CssClass="form-control"
                       placeholder="9999" TextMode="Number"
                       AutoPostBack="true" OnTextChanged="Buscar_Changed"/>
        </div>
      </div>

      
      <div style="display:flex;align-items:flex-end;">
        <asp:LinkButton ID="btnLimpiarFiltros" runat="server"
                        CssClass="btn btn-secondary btn-sm" CausesValidation="false"
                        OnClick="btnLimpiarFiltros_Click">
          <i class="fa-solid fa-eraser"></i> Limpiar
        </asp:LinkButton>
      </div>

    </div>
  </div>

  <!-- ══ Grid + Paginador ══════════════════════════════════════ -->
  <div class="card">
    <div class="card-title">
      <i class="fa-solid fa-table-list"></i> Lista de Productos
      <span style="margin-left:auto;font-size:.78rem;color:#aaa;font-weight:400;">
        <asp:Literal ID="litTotal" runat="server"/> &mdash; ordenado por mas reciente
      </span>
    </div>

    <asp:HiddenField ID="hfPagina"    runat="server" Value="1"/>
    <asp:HiddenField ID="hfTotalPags" runat="server" Value="1"/>
    <asp:UpdatePanel ID="upGridProductos" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
    <div class="grid-wrapper">
      <asp:GridView ID="gvProductos" runat="server"
                    AutoGenerateColumns="false" CssClass="prod-grid"
                    DataKeyNames="pro_id" GridLines="None"
                    OnRowCommand="gvProductos_RowCommand">
        <Columns>

          <asp:BoundField DataField="pro_id"     HeaderText="ID"      ItemStyle-Width="50px"/>
          <asp:BoundField DataField="pro_nombre" HeaderText="PRODUCTO"/>

         
          <asp:TemplateField HeaderText="FOTOS" ItemStyle-Width="130px">
            <ItemTemplate>
              <%# GenerarCarrusel(Eval("pro_id"), Eval("tbl_pro_fotos")) %>
            </ItemTemplate>
          </asp:TemplateField>

          
          <asp:TemplateField HeaderText="PROVEEDOR">
            <ItemTemplate>
              <span style="font-weight:600;color:var(--accent2);">
                <%# Eval("tbl_proveedor.prov_nombre") %>
              </span>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:BoundField DataField="pro_cantidad" HeaderText="STOCK"
                          DataFormatString="{0:N0}" ItemStyle-Width="80px"/>
          <asp:BoundField DataField="pro_precio"   HeaderText="PRECIO"
                          DataFormatString="${0:N2}" ItemStyle-Width="90px"/>

          
         <asp:TemplateField HeaderText="ESTADO" ItemStyle-Width="110px">
            <ItemTemplate>
              <span class='badge <%# Convert.ToString(Eval("pro_estado")) == "A" ? "badge-activo":"badge-inactivo" %>'>
                <i class='fa-solid <%# Convert.ToString(Eval("pro_estado")) == "A" ? "fa-circle-check":"fa-circle-xmark" %>'></i>
                <%# Convert.ToString(Eval("pro_estado")) == "A" ? "Activo":"Inactivo" %>
              </span>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="ACCIONES" ItemStyle-Width="240px">
            <ItemTemplate>

              <asp:LinkButton runat="server" CommandName="Editar"
                  CommandArgument='<%# Eval("pro_id") %>'
                  CssClass="btn btn-warn btn-sm">
                <i class="fa-solid fa-pen"></i> Editar
              </asp:LinkButton>

              <asp:LinkButton runat="server"
                  CommandName='<%# Convert.ToString(Eval("pro_estado")) == "A" ? "ElimLog" : "Activar" %>'
                  CommandArgument='<%# Eval("pro_id") %>'
                  CssClass='<%# "btn btn-sm " + (Convert.ToString(Eval("pro_estado")) == "A" ? "btn-secondary":"btn-success") %>'
                  OnClientClick='<%# Convert.ToString(Eval("pro_estado")) == "A"
                      ? "return confirm(\"Desactivar este producto?\");"
                      : "return confirm(\"Reactivar este producto?\");" %>'>
                <i class='fa-solid <%# Convert.ToString(Eval("pro_estado")) == "A" ? "fa-toggle-off":"fa-toggle-on" %>'></i>
                <%# Convert.ToString(Eval("pro_estado")) == "A" ? " Desactivar":" Activar" %>
              </asp:LinkButton>

              <asp:LinkButton runat="server" CommandName="ElimFis"
                  CommandArgument='<%# Eval("pro_id") %>'
                  CssClass="btn btn-danger btn-sm"
                  OnClientClick="return confirm('ELIMINAR permanentemente. No se puede deshacer.');">
                <i class="fa-solid fa-trash"></i>
              </asp:LinkButton>

              <a href='FotosProducto.aspx?id=<%# Eval("pro_id") %>'
                 class="btn btn-primary btn-sm" title="Administrar fotos">
                <i class="fa-solid fa-camera"></i>
              </a>

            </ItemTemplate>
          </asp:TemplateField>

        </Columns>
        <EmptyDataTemplate>
          <div style="padding:38px;text-align:center;color:#bbb;font-size:.93rem;">
            <i class="fa-solid fa-box-open" style="font-size:2.2rem;display:block;margin-bottom:10px;color:rgba(122,74,170,0.2)"></i>
            No se encontraron productos con los filtros actuales.
          </div>
        </EmptyDataTemplate>
      </asp:GridView>
    </div>

    <div class="pager-wrap">
      <span class="pager-info"><asp:Literal ID="litPagerInfo" runat="server"/></span>
      <div class="pager-btns">
        <asp:Button ID="btnPrev" runat="server" Text="&#8249;" CssClass="pager-btn"
                    CausesValidation="false" OnClick="btnPrev_Click"/>
        <asp:Repeater ID="rptPager" runat="server" OnItemCommand="rptPager_ItemCommand">
          <ItemTemplate>
            <asp:LinkButton runat="server" CommandName="Paginar"
                CommandArgument='<%# Container.DataItem %>'
                CssClass='<%# "pager-btn" + ((int)Container.DataItem == int.Parse(hfPagina.Value) ? " active":"") %>'>
              <%# Container.DataItem %>
            </asp:LinkButton>
          </ItemTemplate>
        </asp:Repeater>
        <asp:Button ID="btnNext" runat="server" Text="&#8250;" CssClass="pager-btn"
                    CausesValidation="false" OnClick="btnNext_Click"/>
      </div>
    </div>
    </ContentTemplate>
       <Triggers>
        <asp:AsyncPostBackTrigger ControlID="txtBuscar" EventName="TextChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddlFiltroProveedor" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="ddlFiltroEstado" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="txtPrecioMin" EventName="TextChanged" />
        <asp:AsyncPostBackTrigger ControlID="txtPrecioMax" EventName="TextChanged" />
        <asp:AsyncPostBackTrigger ControlID="txtStockMin" EventName="TextChanged" />
        <asp:AsyncPostBackTrigger ControlID="txtStockMax" EventName="TextChanged" />
        <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
    </Triggers>
    </asp:UpdatePanel>
  </div>

  <!-- ══ Modal Crear / Editar ══════════════════════════════════ -->
  <div class="modal-overlay" id="modalProducto">
    <div class="modal-box">
      <div class="modal-title">
        <i class="fa-solid fa-box" style="color:var(--accent)"></i>
        <asp:Literal ID="litTituloModal" runat="server" Text=" Nuevo Producto"/>
      </div>

      <asp:HiddenField ID="hfProdId" runat="server" Value="0"/>

      <div class="modal-row">
        <div class="fg" style="min-width:100%">
          <label>Nombre del producto *</label>
          <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control"
                       placeholder="Ej. Laptop Dell Inspiron" MaxLength="200"/>
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombre"
               ErrorMessage="El nombre es obligatorio." ForeColor="#c0392b"
               Display="Dynamic" ValidationGroup="vgProd" Style="font-size:.75rem;margin-top:3px"/>
        </div>
      </div>
    <div class="modal-row">
      <div class="fg">
        <label>Cantidad *</label>
        <asp:TextBox ID="txtCantidad" runat="server" CssClass="form-control"
                     TextMode="Number" min="0" step="1" placeholder="0"/>
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCantidad"
             ErrorMessage="Requerido." ForeColor="#c0392b"
             Display="Dynamic" ValidationGroup="vgProd" Style="font-size:.75rem"/>
        <asp:RangeValidator runat="server" ControlToValidate="txtCantidad"
             MinimumValue="0" MaximumValue="2147483647" Type="Integer"
             ErrorMessage="Debe ser un entero positivo." ForeColor="#c0392b"
             Display="Dynamic" ValidationGroup="vgProd" Style="font-size:.75rem"/>
      </div>
      <div class="fg">
        <label>Precio *</label>
        <asp:TextBox ID="txtPrecio" runat="server" CssClass="form-control" placeholder="0.00"/>
        
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPrecio"
             ErrorMessage="Requerido." ForeColor="#c0392b"
             Display="Dynamic" ValidationGroup="vgProd" Style="font-size:.75rem"/>
        
        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPrecio"
             ValidationExpression="^\d+([.,]\d+)?$"
             ErrorMessage="Ingresa un valor positivo válido (ej. 30.8 o 30,8)." ForeColor="#c0392b"
             Display="Dynamic" ValidationGroup="vgProd" Style="font-size:.75rem"/>
      </div>
</div>
      <div class="modal-row">
        <div class="fg" style="min-width:100%">
          <label>Proveedor *</label>
          <asp:DropDownList ID="ddlProveedor" runat="server" CssClass="form-control"/>
          <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlProveedor"
               InitialValue="" ErrorMessage="Selecciona un proveedor."
               ForeColor="#c0392b" Display="Dynamic" ValidationGroup="vgProd"
               Style="font-size:.75rem"/>
        </div>
      </div>

      <div class="modal-actions">
        <button class="btn btn-secondary" onclick="cerrarModal(); return false;">
          <i class="fa-solid fa-xmark"></i> Cancelar
        </button>
        <asp:LinkButton ID="btnGuardarProd" runat="server"
                        CssClass="btn btn-primary" ValidationGroup="vgProd"
                        OnClick="btnGuardar_Click">
          <i class="fa-solid fa-floppy-disk"></i> Guardar
        </asp:LinkButton>
      </div>
    </div>
  </div>

  <asp:HiddenField ID="hfModalAbierto"    runat="server" Value="0"/>
  <asp:HiddenField ID="hfFiltrosAbiertos" runat="server" Value="0"/>

  <script>
  // ── Modal ────────────────────────────────────────────────────
  function abrirModal() {
    document.getElementById('modalProducto').classList.add('open');
    document.getElementById('<%= hfModalAbierto.ClientID %>').value = '1';
  }
  function cerrarModal() {
    document.getElementById('modalProducto').classList.remove('open');
    document.getElementById('<%= hfModalAbierto.ClientID %>').value = '0';
  }
  document.addEventListener('keydown', e => { if (e.key === 'Escape') cerrarModal(); });
  document.getElementById('modalProducto')
          .addEventListener('click', function(e){ if(e.target===this) cerrarModal(); });

  window.addEventListener('DOMContentLoaded', function(){
    if (document.getElementById('<%= hfModalAbierto.ClientID %>').value === '1')
      document.getElementById('modalProducto').classList.add('open');
    if (document.getElementById('<%= hfFiltrosAbiertos.ClientID %>').value === '1') {
      document.getElementById('filtrosPanel').classList.add('open');
      document.getElementById('arrowFilt').innerHTML = '<i class="fa-solid fa-chevron-up"></i>';
    }
  });

  // ── Filtros ──────────────────────────────────────────────────
  function toggleFiltros() {
    var p  = document.getElementById('filtrosPanel');
    var a  = document.getElementById('arrowFilt');
    var hf = document.getElementById('<%= hfFiltrosAbiertos.ClientID %>');
          p.classList.toggle('open');
          var open = p.classList.contains('open');
          a.innerHTML = open ? '<i class="fa-solid fa-chevron-up"></i>'
              : '<i class="fa-solid fa-chevron-down"></i>';
          hf.value = open ? '1' : '0';
      }

      // ── Carrusel ─────────────────────────────────────────────────
      document.addEventListener('DOMContentLoaded', function () {
          document.querySelectorAll('.carousel-cell').forEach(function (c) {
              var slides = c.querySelectorAll('.slide');
              var dots = c.querySelectorAll('.dot');
              if (!slides.length) return;
              var cur = 0;
              function goTo(n) {
                  slides[cur].classList.remove('active');
                  if (dots[cur]) dots[cur].classList.remove('on');
                  cur = (n + slides.length) % slides.length;
                  slides[cur].classList.add('active');
                  if (dots[cur]) dots[cur].classList.add('on');
              }
              if (slides.length > 1) setInterval(function () { goTo(cur + 1); }, 3000);
              var prev = c.querySelector('.prev'), next = c.querySelector('.next');
              if (prev) prev.addEventListener('click', function (e) { e.stopPropagation(); goTo(cur - 1); });
              if (next) next.addEventListener('click', function (e) { e.stopPropagation(); goTo(cur + 1); });
              dots.forEach(function (d, i) { d.addEventListener('click', function () { goTo(i); }); });
          });
      });

      function validarArchivoCarga(input) {
          var pill = document.getElementById('bulkFileName');
          var file = input && input.files && input.files.length ? input.files[0] : null;
          if (!file) {
              pill.innerHTML = '<i class="fa-solid fa-file-circle-plus"></i> Ningún archivo seleccionado';
              return false;
          }

          var nombre = file.name || '';
          var extension = nombre.indexOf('.') >= 0 ? nombre.substring(nombre.lastIndexOf('.')).toLowerCase() : '';
          var permitidas = ['.csv', '.xlsx', '.xls'];

          if (permitidas.indexOf(extension) === -1) {
              input.value = '';
              pill.innerHTML = '<i class="fa-solid fa-file-circle-plus"></i> Ningún archivo seleccionado';
              Swal.fire({
                  title: 'Formato no permitido',
                  text: 'Solo se aceptan archivos .csv, .xlsx o .xls para la carga masiva.',
                  icon: 'warning',
                  confirmButtonColor: '#7a4aaa'
              });
              return false;
          }

          pill.innerHTML = '<i class="fa-solid fa-file-lines"></i> ' + nombre;
          return true;
      }

      function prepararCarga(modo, confirmarSobrescritura) {
          var input = document.getElementById('<%= fuCargaProductos.ClientID %>');
          document.getElementById('<%= hfModoCarga.ClientID %>').value = modo;

          if (!input || !input.files || !input.files.length) {
              Swal.fire({
                  title: 'Archivo requerido',
                  text: 'Selecciona un archivo válido antes de iniciar la carga masiva.',
                  icon: 'warning',
                  confirmButtonColor: '#7a4aaa'
              });
              return false;
          }

          if (!validarArchivoCarga(input)) {
              return false;
          }

          if (!confirmarSobrescritura) {
              return true;
          }

          return confirm('Esta acción eliminará productos y fotos actuales antes de reinsertar la nueva carga. ¿Deseas continuar?');
      }
  </script>

</asp:Content>
