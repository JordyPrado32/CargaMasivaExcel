<%@ Page Title="Proveedores" Language="C#" MasterPageFile="~/Site1.Master"
         AutoEventWireup="true" CodeBehind="Proveedores.aspx.cs" Inherits="Monolito4bm.Proveedores" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<link rel="stylesheet"
      href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css"
      crossorigin="anonymous"/>
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.3/dist/chart.umd.min.js"></script>
<style>
:root {
  --card-bg: rgba(255,255,255,0.72);
  --radius: 18px;
  --accent: #2563eb;
  --accent2: #1d4ed8;
  --accent3: #0f172a;
  --danger: #c0392b;
  --success: #27ae60;
  --warn: #e67e22;
  --row-hover: rgba(37,99,235,0.07);
}

.page-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:26px; flex-wrap:wrap; gap:12px; }
.page-title { font-size:1.65rem; font-weight:700; color:var(--accent2); display:flex; align-items:center; gap:10px; }

.card {
  background:var(--card-bg); backdrop-filter:blur(12px);
  border:1px solid rgba(147,197,253,0.55);
  border-radius:var(--radius); padding:22px 28px;
  box-shadow:0 10px 30px rgba(37,99,235,0.10); margin-bottom:24px;
}
.card-title { font-size:1rem; font-weight:700; color:var(--accent2); margin-bottom:16px; display:flex; align-items:center; gap:8px; }

.search-bar {
  display:flex; align-items:center; gap:10px;
  background:rgba(255,255,255,0.9); border:1.5px solid rgba(59,130,246,0.28);
  border-radius:40px; padding:9px 18px;
  box-shadow:0 2px 10px rgba(37,99,235,.08); margin-bottom:14px; transition:border-color .2s, box-shadow .2s;
}
.search-bar:focus-within { border-color:var(--accent); box-shadow:0 0 0 3px rgba(37,99,235,0.12); }
.search-bar input { border:none; background:transparent; font-family:inherit; font-size:.95rem; color:#1e293b; flex:1; outline:none; }
.search-bar .si { color:var(--accent); font-size:1rem; }

.filtros-toggle { background:none; border:none; cursor:pointer; font-family:inherit; font-size:.83rem; font-weight:700; color:var(--accent); display:flex; align-items:center; gap:6px; padding:4px 0; margin-bottom:10px; }
.filtros-panel { display:none; gap:14px; flex-wrap:wrap; padding:16px; background:rgba(37,99,235,0.04); border-radius:14px; border:1px solid rgba(59,130,246,0.14); margin-bottom:14px; }
.filtros-panel.open { display:flex; animation:fadeIn .22s ease; }
@keyframes fadeIn { from{opacity:0;transform:translateY(-4px)} to{opacity:1;transform:none} }
.fg { display:flex; flex-direction:column; gap:5px; min-width:160px; flex:1; }
.fg label { font-size:.76rem; font-weight:700; color:var(--accent2); letter-spacing:.3px; }
.form-control { padding:9px 13px; border-radius:10px; border:1.5px solid rgba(59,130,246,0.24); background:rgba(255,255,255,0.88); font-family:inherit; font-size:.88rem; color:#1e293b; width:100%; transition:border-color .2s, box-shadow .2s; }
.form-control:focus { outline:none; border-color:var(--accent); box-shadow:0 0 0 3px rgba(37,99,235,0.13); }

.btn { padding:9px 20px; border-radius:30px; border:none; cursor:pointer; font-family:inherit; font-size:.86rem; font-weight:700; display:inline-flex; align-items:center; gap:7px; transition:all .2s; white-space:nowrap; text-decoration:none; }
.btn-primary { background:var(--accent); color:#fff; }
.btn-primary:hover { background:var(--accent2); transform:translateY(-1px); box-shadow:0 4px 14px rgba(29,78,216,.28); }
.btn-secondary { background:rgba(37,99,235,0.10); color:var(--accent2); border:1.5px solid rgba(37,99,235,0.18); }
.btn-secondary:hover { background:rgba(37,99,235,0.18); }
.btn-success { background:var(--success); color:#fff; }
.btn-success:hover { background:#1e8449; transform:translateY(-1px); }
.btn-danger { background:var(--danger); color:#fff; }
.btn-danger:hover { background:#a93226; transform:translateY(-1px); }
.btn-warn { background:var(--warn); color:#fff; }
.btn-warn:hover { background:#d35400; }
.btn-sm { padding:5px 13px; font-size:.76rem; border-radius:20px; }

.alert { padding:11px 16px; border-radius:12px; margin-bottom:14px; font-size:.86rem; font-weight:600; display:flex; align-items:center; gap:9px; animation:fadeIn .3s ease; }
.alert-success { background:rgba(39,174,96,.14); color:#1e8449; border:1px solid rgba(39,174,96,.28); }
.alert-danger { background:rgba(192,57,43,.11); color:#c0392b; border:1px solid rgba(192,57,43,.24); }

.dashboard-grid {
  display:grid;
  grid-template-columns:repeat(auto-fit, minmax(280px, 1fr));
  gap:18px;
}
.dashboard-card {
  background:linear-gradient(180deg, rgba(239,246,255,0.96) 0%, rgba(255,255,255,0.95) 100%);
  border:1px solid rgba(96,165,250,0.26);
  border-radius:18px;
  padding:20px;
  box-shadow:inset 0 1px 0 rgba(255,255,255,0.7);
  min-height:340px;
  display:flex;
  flex-direction:column;
}
.dashboard-card.wide { grid-column:span 2; }
.dashboard-header { display:flex; align-items:flex-start; justify-content:space-between; gap:12px; margin-bottom:16px; }
.dashboard-title { font-size:1rem; font-weight:700; color:var(--accent3); margin:0; }
.dashboard-subtitle { font-size:.8rem; color:#64748b; margin-top:4px; }
.dashboard-badge {
  display:inline-flex;
  align-items:center;
  gap:6px;
  padding:6px 10px;
  border-radius:999px;
  background:rgba(37,99,235,0.10);
  color:var(--accent2);
  font-size:.74rem;
  font-weight:700;
}
.chart-shell {
  position:relative;
  flex:1;
  min-height:240px;
}
.chart-shell canvas {
  width:100% !important;
  height:100% !important;
}
.chart-empty {
  display:none;
  height:100%;
  align-items:center;
  justify-content:center;
  text-align:center;
  color:#64748b;
  border:1px dashed rgba(59,130,246,0.25);
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
.gauge-wrap {
  display:flex;
  align-items:center;
  justify-content:center;
}
.gauge-ring {
  width:180px;
  height:180px;
  border-radius:50%;
  background:conic-gradient(var(--accent) 0deg, rgba(191,219,254,0.9) 0deg);
  display:flex;
  align-items:center;
  justify-content:center;
  box-shadow:0 16px 26px rgba(37,99,235,0.12);
}
.gauge-ring::before {
  content:"";
  width:132px;
  height:132px;
  border-radius:50%;
  background:linear-gradient(180deg, #ffffff 0%, #eff6ff 100%);
  box-shadow:inset 0 0 0 1px rgba(148,163,184,0.16);
}
.gauge-center {
  position:absolute;
  text-align:center;
}
.gauge-value {
  display:block;
  font-size:2rem;
  font-weight:800;
  color:var(--accent3);
  line-height:1;
}
.gauge-label {
  display:block;
  margin-top:6px;
  font-size:.82rem;
  color:#64748b;
}
.summary-metrics {
  display:grid;
  grid-template-columns:repeat(2, minmax(120px, 1fr));
  gap:12px;
}
.summary-item {
  background:rgba(255,255,255,0.88);
  border:1px solid rgba(148,163,184,0.18);
  border-radius:14px;
  padding:14px;
}
.summary-item .label {
  display:block;
  font-size:.78rem;
  color:#64748b;
  margin-bottom:6px;
}
.summary-item .value {
  display:block;
  font-size:1.38rem;
  font-weight:800;
  color:var(--accent3);
}
.summary-footnote {
  margin-top:14px;
  padding-top:14px;
  border-top:1px solid rgba(148,163,184,0.16);
  font-size:.8rem;
  color:#64748b;
}

.grid-wrapper { overflow-x:auto; border-radius:14px; }
.prov-grid { width:100%; border-collapse:collapse; font-size:.88rem; }
.preview-grid { width:100%; border-collapse:collapse; font-size:.85rem; }
.preview-grid thead tr { background:linear-gradient(90deg,#0f172a,#1d4ed8); color:#fff; }
.preview-grid thead th,
.preview-grid tbody td { padding:10px 12px; text-align:left; border-bottom:1px solid rgba(148,163,184,.18); }
.preview-grid tbody tr:nth-child(even) { background:rgba(248,250,252,.72); }
.massive-layout { display:grid; grid-template-columns:1.2fr .8fr; gap:18px; align-items:start; }
.upload-drop {
  border:1.5px dashed rgba(37,99,235,.35); border-radius:18px; padding:20px;
  background:linear-gradient(180deg, rgba(239,246,255,.9), rgba(255,255,255,.95));
}
.upload-drop small { display:block; color:#64748b; margin-top:8px; }
.preview-shell { border:1px solid rgba(148,163,184,.18); border-radius:14px; overflow:hidden; background:rgba(255,255,255,.92); }
.preview-meta { display:flex; justify-content:space-between; gap:12px; flex-wrap:wrap; margin:12px 0 0; font-size:.8rem; color:#64748b; }
.empty-preview {
  padding:28px; text-align:center; color:#94a3b8; border:1px dashed rgba(148,163,184,.25);
  border-radius:14px; background:rgba(248,250,252,.82);
}
.prov-grid thead tr { background:linear-gradient(90deg,var(--accent),var(--accent2)); color:#fff; }
.prov-grid thead th { padding:12px 15px; text-align:left; font-size:.79rem; font-weight:700; letter-spacing:.5px; white-space:nowrap; }
.prov-grid thead th:first-child { border-radius:13px 0 0 0; }
.prov-grid thead th:last-child { border-radius:0 13px 0 0; }
.prov-grid tbody tr { border-bottom:1px solid rgba(180,150,220,0.2); transition:background .15s; }
.prov-grid tbody tr:hover { background:var(--row-hover); }
.prov-grid tbody td { padding:11px 15px; vertical-align:middle; }
.carousel-cell {
  position:relative; width:110px; height:80px;
  border-radius:10px; overflow:hidden;
  background:rgba(37,99,235,0.08);
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
.no-foto, .provider-slide-inline-empty {
  width:110px; height:80px; border-radius:10px;
  display:flex; align-items:center; justify-content:center;
  background:rgba(37,99,235,0.07); color:rgba(37,99,235,0.35); font-size:1.8rem;
}

.badge { display:inline-flex; align-items:center; gap:5px; padding:3px 11px; border-radius:20px; font-size:.72rem; font-weight:700; letter-spacing:.3px; }
.badge-activo { background:rgba(39,174,96,.14); color:#1e8449; }
.badge-inactivo { background:rgba(192,57,43,.11); color:#c0392b; }

.pager-wrap { display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:12px; margin-top:16px; }
.pager-info { font-size:.8rem; color:#999; }
.pager-btns { display:flex; align-items:center; gap:6px; }
.pager-btn { min-width:34px; height:34px; border-radius:50%; border:1.5px solid rgba(37,99,235,0.22); background:rgba(255,255,255,0.8); color:var(--accent2); font-weight:700; font-size:.84rem; cursor:pointer; display:flex; align-items:center; justify-content:center; transition:all .2s; font-family:inherit; text-decoration:none; }
.pager-btn:hover, .pager-btn.active { background:var(--accent); color:#fff; border-color:var(--accent); }
.pager-btn:disabled { opacity:.4; cursor:default; }

.modal-overlay { display:none; position:fixed; inset:0; background:rgba(15,23,42,0.52); backdrop-filter:blur(6px); z-index:999; align-items:center; justify-content:center; }
.modal-overlay.open { display:flex; }
.modal-box { background:rgba(248,250,252,0.98); border-radius:22px; padding:32px 34px; max-width:430px; width:92%; box-shadow:0 20px 60px rgba(15,23,42,0.24); animation:popIn .3s cubic-bezier(.34,1.56,.64,1); }
@keyframes popIn { from { opacity:0; transform:scale(.88) translateY(18px); } to { opacity:1; transform:none; } }
.modal-title { font-size:1.08rem; font-weight:700; color:var(--accent2); margin-bottom:18px; }
.modal-row { display:flex; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
.modal-actions { display:flex; gap:10px; justify-content:flex-end; margin-top:4px; }

@media(max-width:900px){
  .dashboard-card.wide { grid-column:span 1; }
  .summary-layout { grid-template-columns:1fr; }
  .massive-layout { grid-template-columns:1fr; }
}

@media(max-width:600px){
  .card { padding:14px 12px; }
  .modal-box { padding:20px 16px; }
  .modal-row { flex-direction:column; }
  .summary-metrics { grid-template-columns:1fr; }
}
</style>
</asp:Content>

<asp:Content ID="bodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <div class="page-header">
    <div class="page-title">
      <i class="fa-solid fa-industry" style="color:var(--accent)"></i> Proveedores
    </div>
    <button class="btn btn-primary" onclick="abrirModal(); return false;">
      <i class="fa-solid fa-plus"></i> Nuevo Proveedor
    </button>
  </div>

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
            <small>Formatos permitidos: .csv, .xlsx y .xls. La primera fila debe tener encabezados.</small>
          </div>

          <div style="display:flex;gap:10px;flex-wrap:wrap;margin-top:16px;">
            <asp:LinkButton ID="btnDescargarFormato" runat="server"
                            CssClass="btn btn-secondary" CausesValidation="false"
                            OnClick="btnDescargarFormato_Click">
              <i class="fa-solid fa-download"></i> Descargar Formato
            </asp:LinkButton>
            
            <asp:LinkButton ID="btnPrevisualizarCarga" runat="server"
                            CssClass="btn btn-primary" CausesValidation="false"
                            OnClick="btnPrevisualizarCarga_Click">
              <i class="fa-solid fa-eye"></i> Visualizar archivo
            </asp:LinkButton>

            <asp:LinkButton ID="btnLimpiarCarga" runat="server"
                            CssClass="btn btn-secondary" CausesValidation="false"
                            OnClick="btnLimpiarCarga_Click">
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
          <small style="color:#64748b;margin-top:8px;display:block;">
            La segunda opcion reinicia los IDs desde 1, limpia proveedores y deja que SQL genere el autoincremento. Luego reasigna productos con la nueva secuencia 1..n.
          </small>
        </div>

        <div style="display:flex;gap:10px;flex-wrap:wrap;margin-top:16px;">
          <asp:LinkButton ID="btnProcesarCargaMasiva" runat="server"
                          CssClass="btn btn-success" CausesValidation="false"
                          OnClick="btnProcesarCargaMasiva_Click">
            <i class="fa-solid fa-database"></i> Procesar carga masiva
          </asp:LinkButton>
        </div>
      </div>
    </div>

    <div style="margin-top:18px;">
      <asp:PlaceHolder ID="phPreviewVacia" runat="server">
        <div class="empty-preview">
          <i class="fa-solid fa-table-list" style="font-size:1.6rem;display:block;margin-bottom:10px;"></i>
          Selecciona un archivo y presiona "Visualizar archivo" para revisar los datos antes de importarlos.
        </div>
      </asp:PlaceHolder>

      <div class="preview-shell">
        <asp:GridView ID="gvPreviewCarga" runat="server"
                      AutoGenerateColumns="false" CssClass="preview-grid"
                      GridLines="None" Visible="false">
          <Columns>
            <asp:BoundField DataField="NumeroFilaArchivo" HeaderText="FILA" />
            <asp:BoundField DataField="ProveedorIdTexto" HeaderText="ID" />
            <asp:BoundField DataField="NombreProveedor" HeaderText="NOMBRE" />
            <asp:BoundField DataField="EstadoTexto" HeaderText="ESTADO" />
          </Columns>
        </asp:GridView>
      </div>
    </div>
  </div>

  <div class="card">
    <div class="search-bar">
      <span class="si"><i class="fa-solid fa-magnifying-glass"></i></span>
      <asp:TextBox ID="txtBuscar" runat="server"
                   placeholder="Buscar proveedor por nombre..."
                   AutoPostBack="false" OnTextChanged="Buscar_Changed"/>
    </div>

    <button class="filtros-toggle" onclick="toggleFiltros(); return false;">
      <i class="fa-solid fa-sliders"></i> Filtros avanzados
      <span id="arrowFilt"><i class="fa-solid fa-chevron-down"></i></span>
    </button>

    <div class="filtros-panel" id="filtrosPanel">
      <div class="fg">
        <label>Estado</label>
        <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control"
                          AutoPostBack="true" OnSelectedIndexChanged="Buscar_Changed">
          <asp:ListItem Value="" Text="Todos los estados"/>
          <asp:ListItem Value="A" Text="Solo activos"/>
          <asp:ListItem Value="I" Text="Solo inactivos"/>
        </asp:DropDownList>
      </div>
      <div style="display:flex;align-items:flex-end;gap:8px;">
        <asp:LinkButton ID="btnLimpiarFiltros" runat="server"
                        CssClass="btn btn-secondary btn-sm" CausesValidation="false"
                        OnClick="btnLimpiarFiltros_Click">
          <i class="fa-solid fa-eraser"></i> Limpiar
        </asp:LinkButton>
      </div>
    </div>
  </div>

  <asp:HiddenField ID="hfPagina" runat="server" Value="1"/>
  <asp:HiddenField ID="hfTotalPags" runat="server" Value="1"/>

  <asp:UpdatePanel ID="upDashboardProveedores" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:HiddenField ID="hfDistribucionProductosJson" runat="server" />
      <asp:HiddenField ID="hfStockProveedoresJson" runat="server" />
      <asp:HiddenField ID="hfResumenProveedoresJson" runat="server" />

      <div class="card" id="cardDashboardProveedores">
        <div class="card-title">
          <i class="fa-solid fa-chart-line"></i> Panel analítico de proveedores
        </div>

        <div class="dashboard-grid">
          <section class="dashboard-card" aria-labelledby="ttlDistribucion">
            <div class="dashboard-header">
              <div>
                <h3 class="dashboard-title" id="ttlDistribucion">Distribución de productos</h3>
                <div class="dashboard-subtitle">Participación por proveedor basada en productos activos.</div>
              </div>
              <span class="dashboard-badge"><i class="fa-solid fa-chart-pie"></i> Donut</span>
            </div>
            <div class="chart-shell">
              <canvas id="chartDistribucionProductos" aria-label="Distribución de productos por proveedor"></canvas>
              <div class="chart-empty" id="emptyDistribucionProductos">No hay productos activos para graficar por proveedor.</div>
            </div>
          </section>

          <section class="dashboard-card wide" aria-labelledby="ttlStock">
            <div class="dashboard-header">
              <div>
                <h3 class="dashboard-title" id="ttlStock">Volumen de stock por proveedor</h3>
                <div class="dashboard-subtitle">Suma total de unidades activas asociadas al catálogo de cada proveedor.</div>
              </div>
              <span class="dashboard-badge"><i class="fa-solid fa-chart-column"></i> Barras</span>
            </div>
            <div class="chart-shell">
              <canvas id="chartStockProveedores" aria-label="Volumen de stock por proveedor"></canvas>
              <div class="chart-empty" id="emptyStockProveedores">No hay stock activo disponible para este análisis.</div>
            </div>
          </section>

          <section class="dashboard-card wide" aria-labelledby="ttlResumen">
            <div class="dashboard-header">
              <div>
                <h3 class="dashboard-title" id="ttlResumen">Estado operativo de proveedores</h3>
                <div class="dashboard-subtitle">Resumen instantáneo del padrón activo e inactivo registrado en la base.</div>
              </div>
              <span class="dashboard-badge"><i class="fa-solid fa-gauge-high"></i> Resumen</span>
            </div>

            <div class="summary-layout">
              <div class="gauge-wrap">
                <div class="gauge-ring" id="gaugeProveedores">
                  <div class="gauge-center">
                    <span class="gauge-value" id="gaugePercent">0%</span>
                    <span class="gauge-label">Proveedores activos</span>
                  </div>
                </div>
              </div>

              <div>
                <div class="summary-metrics">
                  <div class="summary-item">
                    <span class="label">Activos</span>
                    <span class="value" id="summaryActivos">0</span>
                  </div>
                  <div class="summary-item">
                    <span class="label">Inactivos</span>
                    <span class="value" id="summaryInactivos">0</span>
                  </div>
                  <div class="summary-item">
                    <span class="label">Total proveedores</span>
                    <span class="value" id="summaryTotal">0</span>
                  </div>
                  <div class="summary-item">
                    <span class="label">Con productos activos</span>
                    <span class="value" id="summaryConProductos">0</span>
                  </div>
                </div>

                <div class="summary-footnote" id="summaryFootnote">
                  Sin datos suficientes para calcular la razón operativa de proveedores.
                </div>
              </div>
            </div>
          </section>
        </div>
      </div>

      <div class="card" id="cardCrudProveedores">
        <div class="card-title">
          <i class="fa-solid fa-list"></i> Lista de Proveedores
          <span style="margin-left:auto;font-size:.78rem;color:#94a3b8;font-weight:400;">
            <asp:Literal ID="litTotal" runat="server"/> &mdash; Ordenado por más reciente
          </span>
        </div>

        <div class="grid-wrapper">
          <asp:GridView ID="gvProveedores" runat="server"
                        AutoGenerateColumns="false" CssClass="prov-grid"
                        DataKeyNames="prov_id" GridLines="None"
                        OnRowCommand="gvProveedores_RowCommand">
            <Columns>
              <asp:BoundField DataField="prov_id" HeaderText="ID" ItemStyle-Width="60px"/>
              <asp:BoundField DataField="prov_nombre" HeaderText="NOMBRE" ItemStyle-Width="450px"/>
              <asp:TemplateField HeaderText="PRODUCTOS" ItemStyle-Width="140px">
                <ItemTemplate>
                  <%# GenerarCarruselProveedor(Eval("prov_id")) %>
                </ItemTemplate>
              </asp:TemplateField>

              <asp:TemplateField HeaderText="ESTADO" ItemStyle-Width="110px">
                <ItemTemplate>
                  <span class='badge <%# Convert.ToString(Eval("prov_estado")) == "A" ? "badge-activo":"badge-inactivo" %>'>
                    <i class='fa-solid <%# Convert.ToString(Eval("prov_estado")) == "A" ? "fa-circle-check":"fa-circle-xmark" %>'></i>
                    <%# Convert.ToString(Eval("prov_estado")) == "A" ? "Activo":"Inactivo" %>
                  </span>
                </ItemTemplate>
              </asp:TemplateField>

              <asp:TemplateField HeaderText="ACCIONES" ItemStyle-Width="290px">
                <ItemTemplate>
                  <asp:LinkButton runat="server" CommandName="Editar"
                                  CommandArgument='<%# Eval("prov_id") %>'
                                  CssClass="btn btn-warn btn-sm">
                    <i class="fa-solid fa-pen"></i> Editar
                  </asp:LinkButton>

                  <asp:LinkButton runat="server"
                                  CommandName='<%# Convert.ToString(Eval("prov_estado")) == "A" ? "ElimLog" : "Activar" %>'
                                  CommandArgument='<%# Eval("prov_id") %>'
                                  CssClass='<%# "btn btn-sm " + (Convert.ToString(Eval("prov_estado")) == "A" ? "btn-secondary":"btn-success") %>'
                                  OnClientClick='<%# Convert.ToString(Eval("prov_estado")) == "A"
                                      ? "return confirm(\"¿Desactivar este proveedor?\");"
                                      : "return confirm(\"¿Reactivar este proveedor?\");" %>'>
                    <i class='fa-solid <%# Convert.ToString(Eval("prov_estado")) == "A" ? "fa-toggle-off":"fa-toggle-on" %>'></i>
                    <%# Convert.ToString(Eval("prov_estado")) == "A" ? " Desactivar":" Activar" %>
                  </asp:LinkButton>

                  <asp:LinkButton runat="server" CommandName="ElimFis"
                                  CommandArgument='<%# Eval("prov_id") %>'
                                  CssClass="btn btn-danger btn-sm"
                                  OnClientClick="return confirm('ELIMINAR permanentemente. Esta accion no se puede deshacer.');">
                    <i class="fa-solid fa-trash"></i> Eliminar
                  </asp:LinkButton>
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
              <div style="padding:34px;text-align:center;color:#94a3b8;font-size:.93rem;">
                <i class="fa-solid fa-box-open" style="font-size:2rem;display:block;margin-bottom:10px;color:rgba(37,99,235,0.18)"></i>
                No hay proveedores que coincidan con los filtros.
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
      </div>

  <div class="modal-overlay" id="modalProveedor">
    <div class="modal-box">
      <div class="modal-title">
        <i class="fa-solid fa-industry" style="color:var(--accent)"></i>
        <asp:Literal ID="litTituloForm" runat="server" Text=" Nuevo Proveedor"/>
      </div>

      <asp:HiddenField ID="hfProvId" runat="server" Value="0"/>

      <div class="modal-row">
        <div class="fg" style="min-width:100%">
          <label>Nombre del proveedor *</label>
          <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control"
                       placeholder="Ej. Distribuidora El Dorado" MaxLength="150"/>
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombre"
               ErrorMessage="El nombre es obligatorio." ForeColor="#c0392b"
               Display="Dynamic" ValidationGroup="vgProv"
               Style="font-size:.76rem;margin-top:3px"/>
        </div>
      </div>

      <div class="modal-actions">
        <button type="button" class="btn btn-secondary" onclick="cerrarModal(); return false;">
          <i class="fa-solid fa-xmark"></i> Cancelar
        </button>
        <asp:LinkButton ID="btnGuardar" runat="server"
                        CssClass="btn btn-primary" ValidationGroup="vgProv"
                        OnClick="btnGuardar_Click">
          <i class="fa-solid fa-floppy-disk"></i> Guardar
        </asp:LinkButton>
      </div>
    </div>
  </div>

  <asp:HiddenField ID="hfModalAbierto" runat="server" Value="0"/>
  <asp:HiddenField ID="hfFiltrosAbiertos" runat="server" Value="0"/>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="txtBuscar" EventName="TextChanged" />
      <asp:AsyncPostBackTrigger ControlID="ddlFiltroEstado" EventName="SelectedIndexChanged" />
      <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
      <asp:AsyncPostBackTrigger ControlID="btnGuardar" EventName="Click" />
    </Triggers>
  </asp:UpdatePanel>

  <script>
  function abrirModal() {
    document.getElementById('modalProveedor').classList.add('open');
    document.getElementById('<%= hfModalAbierto.ClientID %>').value = '1';
  }

  function cerrarModal() {
    document.getElementById('modalProveedor').classList.remove('open');
    document.getElementById('<%= hfModalAbierto.ClientID %>').value = '0';
  }

  document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
      cerrarModal();
    }
  });

  document.getElementById('modalProveedor').addEventListener('click', function(e) {
    if (e.target === this) {
      cerrarModal();
    }
  });

  window.providerDashboardCharts = window.providerDashboardCharts || {};

  function inicializarBusquedaPredictivaProveedor() {
    var input = document.getElementById('<%= txtBuscar.ClientID %>');
    if (!input || input.dataset.liveSearchBound === '1') {
      return;
    }

    var timeoutId = 0;
    input.dataset.liveSearchBound = '1';
    input.addEventListener('input', function() {
      window.clearTimeout(timeoutId);
      timeoutId = window.setTimeout(function() {
        __doPostBack('<%= txtBuscar.UniqueID %>', '');
      }, 250);
    });
  }

  function reubicarDashboardProveedores() {
    var dashboard = document.getElementById('cardDashboardProveedores');
    var crud = document.getElementById('cardCrudProveedores');
    if (!dashboard || !crud || !crud.parentNode) {
      return;
    }

    if (crud.nextElementSibling !== dashboard) {
      crud.parentNode.insertBefore(dashboard, crud.nextSibling);
    }
  }

  function readDashboardData(fieldId, fallback) {
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

  function toggleChartEmptyState(canvasId, emptyId, hasData) {
    var canvas = document.getElementById(canvasId);
    var empty = document.getElementById(emptyId);
    if (!canvas || !empty) {
      return;
    }

    canvas.style.display = hasData ? 'block' : 'none';
    empty.classList.toggle('visible', !hasData);
  }

  function destroyChart(chartKey) {
    if (window.providerDashboardCharts[chartKey]) {
      window.providerDashboardCharts[chartKey].destroy();
      window.providerDashboardCharts[chartKey] = null;
    }
  }

  function renderDonutChart(data) {
    var labels = data.map(function(item) { return item.Label; });
    var values = data.map(function(item) { return item.Value; });
    var hasData = values.some(function(value) { return value > 0; });

    toggleChartEmptyState('chartDistribucionProductos', 'emptyDistribucionProductos', hasData);
    destroyChart('distribucion');

    if (!hasData) {
      return;
    }

    window.providerDashboardCharts.distribucion = new Chart(document.getElementById('chartDistribucionProductos'), {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [{
          data: values,
          backgroundColor: ['#2563eb', '#38bdf8', '#1d4ed8', '#60a5fa', '#0f172a', '#93c5fd', '#0284c7', '#7dd3fc'],
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
            labels: {
              usePointStyle: true,
              padding: 16
            }
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

  function renderBarChart(data) {
    var labels = data.map(function(item) { return item.Label; });
    var values = data.map(function(item) { return item.Value; });
    var hasData = values.some(function(value) { return value > 0; });

    toggleChartEmptyState('chartStockProveedores', 'emptyStockProveedores', hasData);
    destroyChart('stock');

    if (!hasData) {
      return;
    }

    window.providerDashboardCharts.stock = new Chart(document.getElementById('chartStockProveedores'), {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Unidades en stock',
          data: values,
          backgroundColor: 'rgba(37, 99, 235, 0.82)',
          borderColor: '#1d4ed8',
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
            ticks: { color: '#475569' }
          },
          y: {
            beginAtZero: true,
            ticks: { color: '#475569', precision: 0 },
            grid: { color: 'rgba(148, 163, 184, 0.18)' }
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

  function renderSummaryCard(summary) {
    var total = summary.TotalProviders || 0;
    var active = summary.ActiveProviders || 0;
    var inactive = summary.InactiveProviders || 0;
    var withProducts = summary.ProvidersWithProducts || 0;
    var percentage = summary.ActivePercentage || 0;

    document.getElementById('summaryActivos').textContent = active;
    document.getElementById('summaryInactivos').textContent = inactive;
    document.getElementById('summaryTotal').textContent = total;
    document.getElementById('summaryConProductos').textContent = withProducts;
    document.getElementById('gaugePercent').textContent = percentage + '%';
    document.getElementById('gaugeProveedores').style.background =
      'conic-gradient(var(--accent) ' + (percentage * 3.6) + 'deg, rgba(191,219,254,0.9) 0deg)';

    var footnote = 'Sin datos suficientes para calcular la razón operativa de proveedores.';
    if (total > 0) {
      footnote = active + ' de ' + total + ' proveedor(es) permanecen activos. '
        + inactive + ' se encuentran inactivos y '
        + withProducts + ' ya tienen productos activos asociados.';
    }

    document.getElementById('summaryFootnote').textContent = footnote;
  }

  function renderProvidersDashboard() {
    var distribucion = readDashboardData('<%= hfDistribucionProductosJson.ClientID %>', []);
    var stock = readDashboardData('<%= hfStockProveedoresJson.ClientID %>', []);
    var resumen = readDashboardData('<%= hfResumenProveedoresJson.ClientID %>', {});

    renderDonutChart(distribucion);
    renderBarChart(stock);
    renderSummaryCard(resumen);
  }

  function inicializarCarruselProductoProveedor() {
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

  function inicializarComponentes() {
    if (document.getElementById('<%= hfModalAbierto.ClientID %>').value === '1') {
      document.getElementById('modalProveedor').classList.add('open');
    }

    if (document.getElementById('<%= hfFiltrosAbiertos.ClientID %>').value === '1') {
      document.getElementById('filtrosPanel').classList.add('open');
      document.getElementById('arrowFilt').innerHTML = '<i class="fa-solid fa-chevron-up"></i>';
    }

    inicializarBusquedaPredictivaProveedor();
    inicializarCarruselProductoProveedor();
    reubicarDashboardProveedores();
    renderProvidersDashboard();
  }

  window.addEventListener('DOMContentLoaded', inicializarComponentes);

  if (typeof Sys !== 'undefined') {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(inicializarComponentes);
  }

  function toggleFiltros() {
    var p = document.getElementById('filtrosPanel');
    var a = document.getElementById('arrowFilt');
    var hf = document.getElementById('<%= hfFiltrosAbiertos.ClientID %>');
    p.classList.toggle('open');
    var open = p.classList.contains('open');
    a.innerHTML = open ? '<i class="fa-solid fa-chevron-up"></i>' : '<i class="fa-solid fa-chevron-down"></i>';
    hf.value = open ? '1' : '0';
  }
  </script>

</asp:Content>
