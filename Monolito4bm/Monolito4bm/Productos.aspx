<%@ Page Title="Productos" Language="C#" MasterPageFile="~/Site1.Master"
         AutoEventWireup="true" CodeBehind="Productos.aspx.cs" Inherits="Monolito4bm.Productos" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<link rel="stylesheet"
      href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css"
      crossorigin="anonymous"/>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.3/dist/chart.umd.min.js"></script>
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

/* ── GridView ───────────────────────────────────────────────── */
.grid-wrapper { overflow-x:auto; border-radius:14px; }
.prod-grid { width:100%; border-collapse:collapse; font-size:.87rem; }
.preview-grid { width:100%; border-collapse:collapse; font-size:.85rem; }
.preview-grid thead tr { background:linear-gradient(90deg,#3f1c68,#7a4aaa); color:#fff; }
.preview-grid thead th, .preview-grid tbody td { padding:10px 12px; text-align:left; border-bottom:1px solid rgba(180,150,220,.18); }
.preview-grid tbody tr:nth-child(even) { background:rgba(248,244,255,.72); }
.massive-layout { display:grid; grid-template-columns:1.2fr .8fr; gap:18px; align-items:start; }
.upload-drop {
  border:1.5px dashed rgba(122,74,170,.35); border-radius:18px; padding:20px;
  background:linear-gradient(180deg, rgba(248,244,255,.92), rgba(255,255,255,.96));
}
.upload-drop small { display:block; color:#7b6a94; margin-top:8px; }
.preview-shell { border:1px solid rgba(180,150,220,.18); border-radius:14px; overflow:hidden; background:rgba(255,255,255,.92); }
.preview-meta { display:flex; justify-content:space-between; gap:12px; flex-wrap:wrap; margin:12px 0 0; font-size:.8rem; color:#7b6a94; }
.empty-preview {
  padding:28px; text-align:center; color:#9f93b1; border:1px dashed rgba(180,150,220,.25);
  border-radius:14px; background:rgba(248,244,255,.82);
}
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

.dashboard-grid {
  display:grid;
  grid-template-columns:repeat(auto-fit, minmax(280px, 1fr));
  gap:18px;
}
.dashboard-card {
  background:linear-gradient(180deg, rgba(248,244,255,0.96) 0%, rgba(255,255,255,0.95) 100%);
  border:1px solid rgba(180,150,220,0.26);
  border-radius:18px;
  padding:20px;
  box-shadow:inset 0 1px 0 rgba(255,255,255,0.7);
  min-height:340px;
  display:flex;
  flex-direction:column;
}
.dashboard-card.wide { grid-column:span 2; }
.dashboard-header { display:flex; align-items:flex-start; justify-content:space-between; gap:12px; margin-bottom:16px; }
.dashboard-title { font-size:1rem; font-weight:700; color:#3b245f; margin:0; }
.dashboard-subtitle { font-size:.8rem; color:#7b6a94; margin-top:4px; }
.dashboard-badge {
  display:inline-flex;
  align-items:center;
  gap:6px;
  padding:6px 10px;
  border-radius:999px;
  background:rgba(122,74,170,0.10);
  color:var(--accent2);
  font-size:.74rem;
  font-weight:700;
}
.chart-shell { position:relative; flex:1; min-height:240px; }
.chart-shell canvas { width:100% !important; height:100% !important; }
.chart-empty {
  display:none;
  height:100%;
  align-items:center;
  justify-content:center;
  text-align:center;
  color:#7b6a94;
  border:1px dashed rgba(122,74,170,0.25);
  border-radius:14px;
  padding:18px;
  background:rgba(255,255,255,0.72);
}
.chart-empty.visible { display:flex; }
.summary-layout {
  display:grid;
  grid-template-columns:minmax(180px, 220px) 1fr;
  gap:18px;
  align-items:center;
  flex:1;
}
.gauge-wrap { display:flex; align-items:center; justify-content:center; }
.gauge-ring {
  width:180px;
  height:180px;
  border-radius:50%;
  background:conic-gradient(var(--accent) 0deg, rgba(224,210,240,0.95) 0deg);
  display:flex;
  align-items:center;
  justify-content:center;
  box-shadow:0 16px 26px rgba(122,74,170,0.12);
}
.gauge-ring::before {
  content:"";
  width:132px;
  height:132px;
  border-radius:50%;
  background:linear-gradient(180deg, #ffffff 0%, #f7f2ff 100%);
  box-shadow:inset 0 0 0 1px rgba(148,120,180,0.16);
}
.gauge-center { position:absolute; text-align:center; }
.gauge-value { display:block; font-size:2rem; font-weight:800; color:#3b245f; line-height:1; }
.gauge-label { display:block; margin-top:6px; font-size:.82rem; color:#7b6a94; }
.summary-metrics {
  display:grid;
  grid-template-columns:repeat(2, minmax(120px, 1fr));
  gap:12px;
}
.summary-item {
  background:rgba(255,255,255,0.88);
  border:1px solid rgba(180,150,220,0.18);
  border-radius:14px;
  padding:14px;
}
.summary-item .label { display:block; font-size:.78rem; color:#7b6a94; margin-bottom:6px; }
.summary-item .value { display:block; font-size:1.38rem; font-weight:800; color:#3b245f; }
.summary-footnote {
  margin-top:14px;
  padding-top:14px;
  border-top:1px solid rgba(180,150,220,0.16);
  font-size:.8rem;
  color:#7b6a94;
}

@media(max-width:600px){
  .card { padding:14px 12px; }
  .modal-box { padding:20px 16px; }
  .modal-row { flex-direction:column; }
  .range-group { flex-direction:column; }
  .summary-metrics { grid-template-columns:1fr; }
}

@media(max-width:900px){
  .dashboard-card.wide { grid-column:span 1; }
  .summary-layout { grid-template-columns:1fr; }
  .massive-layout { grid-template-columns:1fr; }
}
</style>
</asp:Content>

<asp:Content ID="bodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <!-- ══ Encabezado ════════════════════════════════════════════ -->
  <div class="page-header">
    <div class="page-title">
      <i class="fa-solid fa-box" style="color:var(--accent)"></i> Productos
    </div>
    <asp:LinkButton ID="btnNuevo" runat="server" CssClass="btn btn-primary"
                    OnClick="btnNuevo_Click" CausesValidation="false">
      <i class="fa-solid fa-plus"></i> Nuevo Producto
    </asp:LinkButton>
  </div>

  <!-- ══ Mensajes ══════════════════════════════════════════════ -->
  <asp:Literal ID="litMensaje" runat="server"/>

  <div class="card">
    <div class="card-title">
      <i class="fa-solid fa-file-arrow-up"></i> Carga Masiva Excel
    </div>

    <div class="massive-layout">
      <div>
        <div class="upload-drop">
          <div class="fg" style="min-width:100%">
            <label>Seleccionar archivo</label>
            <asp:FileUpload ID="fuCargaMasiva" runat="server" CssClass="form-control" />
            <small>Formatos permitidos: .csv, .xlsx y .xls. Encabezados sugeridos: nombre, cantidad, precio, prov_id, estado.</small>
          </div>
          <div style="display:flex;gap:10px;flex-wrap:wrap;margin-top:16px;">
            <asp:LinkButton ID="btnDescargarFormato" runat="server" CssClass="btn btn-secondary"
                            CausesValidation="false" OnClick="btnDescargarFormato_Click">
              <i class="fa-solid fa-download"></i> Descargar Formato
            </asp:LinkButton>
            <asp:LinkButton ID="btnPrevisualizarCarga" runat="server" CssClass="btn btn-primary"
                            CausesValidation="false" OnClick="btnPrevisualizarCarga_Click">
              <i class="fa-solid fa-eye"></i> Visualizar archivo
            </asp:LinkButton>
            <asp:LinkButton ID="btnLimpiarCarga" runat="server" CssClass="btn btn-secondary"
                            CausesValidation="false" OnClick="btnLimpiarCarga_Click">
              <i class="fa-solid fa-broom"></i> Limpiar carga
            </asp:LinkButton>
          </div>
        </div>

        <div class="preview-meta">
          <span><asp:Literal ID="litArchivoCarga" runat="server" Text="Sin archivo cargado." /></span>
          <span><asp:Literal ID="litResumenCarga" runat="server" Text="Aun no hay vista previa." /></span>
        </div>
      </div>

      <div>
        <div class="fg">
          <label>Tipo de insercion</label>
          <asp:DropDownList ID="ddlTipoInsercionMasiva" runat="server" CssClass="form-control">
            <asp:ListItem Value="1" Text="Anadir sin borrar"/>
            <asp:ListItem Value="2" Text="Borrar todo y volver a cargar"/>
          </asp:DropDownList>
          <small style="color:#7b6a94;margin-top:8px;display:block;">
            En reemplazo total se respaldan las fotos, se reinician productos y fotos, y luego se restauran las fotos cuyos IDs de producto sigan existiendo en la nueva secuencia. Si un proveedor no existe, el producto queda sin proveedor.
          </small>
        </div>
        <div style="display:flex;gap:10px;flex-wrap:wrap;margin-top:16px;">
          <asp:LinkButton ID="btnProcesarCargaMasiva" runat="server" CssClass="btn btn-success"
                          CausesValidation="false" OnClick="btnProcesarCargaMasiva_Click">
            <i class="fa-solid fa-database"></i> Procesar carga masiva
          </asp:LinkButton>
        </div>
      </div>
    </div>

    <div style="margin-top:18px;">
      <asp:PlaceHolder ID="phPreviewVacia" runat="server">
        <div class="empty-preview">
          <i class="fa-solid fa-table-list" style="font-size:1.6rem;display:block;margin-bottom:10px;"></i>
          Selecciona un archivo y presiona "Visualizar archivo" para revisar los productos antes de importarlos.
        </div>
      </asp:PlaceHolder>

      <div class="preview-shell">
        <asp:GridView ID="gvPreviewCarga" runat="server" AutoGenerateColumns="false" CssClass="preview-grid"
                      GridLines="None" Visible="false">
          <Columns>
            <asp:BoundField DataField="NumeroFilaArchivo" HeaderText="FILA" />
            <asp:BoundField DataField="ProductoIdTexto" HeaderText="ID" />
            <asp:BoundField DataField="NombreProducto" HeaderText="NOMBRE" />
            <asp:BoundField DataField="Cantidad" HeaderText="CANTIDAD" />
            <asp:BoundField DataField="Precio" HeaderText="PRECIO" />
            <asp:BoundField DataField="ProveedorIdTexto" HeaderText="PROVEEDOR" />
            <asp:BoundField DataField="EstadoTexto" HeaderText="ESTADO" />
          </Columns>
        </asp:GridView>
      </div>
    </div>
  </div>

  <!-- ══ Buscador + Filtros ════════════════════════════════════ -->
  <div class="card">

    <div class="search-bar">
      <span class="si"><i class="fa-solid fa-magnifying-glass"></i></span>
      <asp:TextBox ID="txtBuscar" runat="server"
                   placeholder="Buscar producto por nombre..."
                   AutoPostBack="false" OnTextChanged="Buscar_Changed"/>
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
  <asp:UpdatePanel ID="upGridProductos" runat="server" UpdateMode="Conditional">
      <ContentTemplate>
    <asp:HiddenField ID="hfPagina"    runat="server" Value="1"/>
    <asp:HiddenField ID="hfTotalPags" runat="server" Value="1"/>
    <div class="card">
      <div class="card-title">
        <i class="fa-solid fa-table-list"></i> Lista de Productos
        <span style="margin-left:auto;font-size:.78rem;color:#aaa;font-weight:400;">
          <asp:Literal ID="litTotal" runat="server"/> &mdash; ordenado por mas reciente
        </span>
      </div>
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

    <asp:HiddenField ID="hfDistribucionProductosJson" runat="server" />
    <asp:HiddenField ID="hfStockProductosJson" runat="server" />
    <asp:HiddenField ID="hfResumenProductosJson" runat="server" />

    <div class="card" id="cardDashboardProductos" style="margin-top:24px;">
      <div class="card-title">
        <i class="fa-solid fa-chart-line"></i> Panel analitico de productos
      </div>

      <div class="dashboard-grid">
        <section class="dashboard-card" aria-labelledby="ttlDistribucionProductos">
          <div class="dashboard-header">
            <div>
              <h3 class="dashboard-title" id="ttlDistribucionProductos">Distribucion por proveedor</h3>
              <div class="dashboard-subtitle">Participacion del catalogo activo por proveedor.</div>
            </div>
            <span class="dashboard-badge"><i class="fa-solid fa-chart-pie"></i> Donut</span>
          </div>
          <div class="chart-shell">
            <canvas id="chartDistribucionProductosGrid" aria-label="Distribucion de productos por proveedor"></canvas>
            <div class="chart-empty" id="emptyDistribucionProductosGrid">No hay productos activos para graficar por proveedor.</div>
          </div>
        </section>

        <section class="dashboard-card wide" aria-labelledby="ttlStockProductos">
          <div class="dashboard-header">
            <div>
              <h3 class="dashboard-title" id="ttlStockProductos">Top productos por stock</h3>
              <div class="dashboard-subtitle">Productos activos con mayor cantidad disponible.</div>
            </div>
            <span class="dashboard-badge"><i class="fa-solid fa-chart-column"></i> Barras</span>
          </div>
          <div class="chart-shell">
            <canvas id="chartStockProductos" aria-label="Top productos por stock"></canvas>
            <div class="chart-empty" id="emptyStockProductos">No hay stock activo disponible para este analisis.</div>
          </div>
        </section>

        <section class="dashboard-card wide" aria-labelledby="ttlResumenProductos">
          <div class="dashboard-header">
            <div>
              <h3 class="dashboard-title" id="ttlResumenProductos">Estado operativo de productos</h3>
              <div class="dashboard-subtitle">Resumen del catalogo, disponibilidad activa y productos con fotos.</div>
            </div>
            <span class="dashboard-badge"><i class="fa-solid fa-gauge-high"></i> Resumen</span>
          </div>

          <div class="summary-layout">
            <div class="gauge-wrap">
              <div class="gauge-ring" id="gaugeProductos">
                <div class="gauge-center">
                  <span class="gauge-value" id="gaugeProductosPercent">0%</span>
                  <span class="gauge-label">Productos activos</span>
                </div>
              </div>
            </div>

            <div>
              <div class="summary-metrics">
                <div class="summary-item">
                  <span class="label">Activos</span>
                  <span class="value" id="summaryProductosActivos">0</span>
                </div>
                <div class="summary-item">
                  <span class="label">Inactivos</span>
                  <span class="value" id="summaryProductosInactivos">0</span>
                </div>
                <div class="summary-item">
                  <span class="label">Total productos</span>
                  <span class="value" id="summaryProductosTotal">0</span>
                </div>
                <div class="summary-item">
                  <span class="label">Con fotos activas</span>
                  <span class="value" id="summaryProductosConFotos">0</span>
                </div>
              </div>

              <div class="summary-footnote" id="summaryProductosFootnote">
                Sin datos suficientes para calcular el estado operativo del catalogo.
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
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
                       placeholder="Ej. Laptop Dell Inspiron" MaxLength="50"/>
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
          <label>Proveedor</label>
          <asp:DropDownList ID="ddlProveedor" runat="server" CssClass="form-control"/>
        </div>
      </div>

      <div class="modal-actions">
        <button type="button" class="btn btn-secondary" onclick="cerrarModal(); return false;">
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
        <asp:AsyncPostBackTrigger ControlID="btnGuardarProd" EventName="Click" />
    </Triggers>
    </asp:UpdatePanel>

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

  window.productDashboardCharts = window.productDashboardCharts || {};

  function readProductDashboardData(fieldId, fallback) {
    var field = document.getElementById(fieldId);
    if (!field || !field.value) {
      return fallback;
    }

    try {
      return JSON.parse(field.value);
    } catch (error) {
      return fallback;
    }
  }

  function toggleProductChartEmptyState(canvasId, emptyId, hasData) {
    var canvas = document.getElementById(canvasId);
    var empty = document.getElementById(emptyId);
    if (!canvas || !empty) {
      return;
    }

    canvas.style.display = hasData ? 'block' : 'none';
    empty.classList.toggle('visible', !hasData);
  }

  function destroyProductChart(chartKey) {
    if (window.productDashboardCharts[chartKey]) {
      window.productDashboardCharts[chartKey].destroy();
      window.productDashboardCharts[chartKey] = null;
    }
  }

  function renderProductDonutChart(data) {
    var labels = data.map(function(item) { return item.Label; });
    var values = data.map(function(item) { return item.Value; });
    var hasData = values.some(function(value) { return value > 0; });

    toggleProductChartEmptyState('chartDistribucionProductosGrid', 'emptyDistribucionProductosGrid', hasData);
    destroyProductChart('distribucion');

    if (!hasData) return;

    window.productDashboardCharts.distribucion = new Chart(document.getElementById('chartDistribucionProductosGrid'), {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [{
          data: values,
          backgroundColor: ['#7a4aaa', '#9d6dd1', '#5a2a8a', '#c39ae7', '#3f1c68', '#d8c0ef', '#8f5cc5', '#eadcf8'],
          borderColor: '#ffffff',
          borderWidth: 2,
          hoverOffset: 10
        }]
      },
      options: {
        maintainAspectRatio: false,
        cutout: '62%',
        plugins: {
          legend: {
            position: 'bottom',
            labels: { usePointStyle: true, padding: 16 }
          },
          tooltip: {
            callbacks: {
              label: function(context) {
                return context.label + ': ' + context.raw + ' producto(s)';
              }
            }
          }
        }
      }
    });
  }

  function renderProductBarChart(data) {
    var labels = data.map(function(item) { return item.Label; });
    var values = data.map(function(item) { return item.Value; });
    var hasData = values.some(function(value) { return value > 0; });

    toggleProductChartEmptyState('chartStockProductos', 'emptyStockProductos', hasData);
    destroyProductChart('stock');

    if (!hasData) return;

    window.productDashboardCharts.stock = new Chart(document.getElementById('chartStockProductos'), {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Unidades en stock',
          data: values,
          backgroundColor: 'rgba(122, 74, 170, 0.82)',
          borderColor: '#5a2a8a',
          borderWidth: 1.5,
          borderRadius: 10,
          maxBarThickness: 48
        }]
      },
      options: {
        maintainAspectRatio: false,
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: '#5d4a78' }
          },
          y: {
            beginAtZero: true,
            ticks: { color: '#5d4a78', precision: 0 },
            grid: { color: 'rgba(180, 150, 220, 0.18)' }
          }
        },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: function(context) {
                return context.raw + ' unidad(es) en stock';
              }
            }
          }
        }
      }
    });
  }

  function renderProductSummary(summary) {
    var total = summary.TotalProducts || 0;
    var active = summary.ActiveProducts || 0;
    var inactive = summary.InactiveProducts || 0;
    var withPhotos = summary.ProductsWithPhotos || 0;
    var percentage = summary.ActivePercentage || 0;

    document.getElementById('summaryProductosActivos').textContent = active;
    document.getElementById('summaryProductosInactivos').textContent = inactive;
    document.getElementById('summaryProductosTotal').textContent = total;
    document.getElementById('summaryProductosConFotos').textContent = withPhotos;
    document.getElementById('gaugeProductosPercent').textContent = percentage + '%';
    document.getElementById('gaugeProductos').style.background =
      'conic-gradient(var(--accent) ' + (percentage * 3.6) + 'deg, rgba(224,210,240,0.95) 0deg)';

    var footnote = 'Sin datos suficientes para calcular el estado operativo del catalogo.';
    if (total > 0) {
      footnote = active + ' de ' + total + ' producto(s) permanecen activos. '
        + inactive + ' estan inactivos y '
        + withPhotos + ' ya tienen al menos una foto activa.';
    }

    document.getElementById('summaryProductosFootnote').textContent = footnote;
  }

  function renderProductsDashboard() {
    var distribucion = readProductDashboardData('<%= hfDistribucionProductosJson.ClientID %>', []);
    var stock = readProductDashboardData('<%= hfStockProductosJson.ClientID %>', []);
    var resumen = readProductDashboardData('<%= hfResumenProductosJson.ClientID %>', {});

    renderProductDonutChart(distribucion);
    renderProductBarChart(stock);
    renderProductSummary(resumen);
  }

  function inicializarBusquedaPredictivaProducto() {
    var input = document.getElementById('<%= txtBuscar.ClientID %>');
    if (!input || input.dataset.liveSearchBound === '1') return;

    var timeoutId = 0;
    input.dataset.liveSearchBound = '1';
    input.addEventListener('input', function() {
      window.clearTimeout(timeoutId);
      timeoutId = window.setTimeout(function() {
        __doPostBack('<%= txtBuscar.UniqueID %>', '');
      }, 250);
    });
  }

  function inicializarCarruseles() {
    document.querySelectorAll('.carousel-cell').forEach(function(c) {
      if (c.dataset.carouselBound === '1') return;

      c.dataset.carouselBound = '1';
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

      if (slides.length > 1) {
        window.setInterval(function() { goTo(cur + 1); }, 3000);
      }

      var prev = c.querySelector('.prev'), next = c.querySelector('.next');
      if (prev) prev.addEventListener('click', function (e) { e.stopPropagation(); goTo(cur - 1); });
      if (next) next.addEventListener('click', function (e) { e.stopPropagation(); goTo(cur + 1); });
      dots.forEach(function (d, i) { d.addEventListener('click', function () { goTo(i); }); });
    });
  }

  function inicializarComponentesProductos() {
    if (document.getElementById('<%= hfModalAbierto.ClientID %>').value === '1')
      document.getElementById('modalProducto').classList.add('open');
    if (document.getElementById('<%= hfFiltrosAbiertos.ClientID %>').value === '1') {
      document.getElementById('filtrosPanel').classList.add('open');
      document.getElementById('arrowFilt').innerHTML = '<i class="fa-solid fa-chevron-up"></i>';
    }
    inicializarBusquedaPredictivaProducto();
    inicializarCarruseles();
    renderProductsDashboard();
  }

  window.addEventListener('DOMContentLoaded', inicializarComponentesProductos);

  if (typeof Sys !== 'undefined') {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(inicializarComponentesProductos);
  }

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

  </script>

</asp:Content>
