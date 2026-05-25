<%@ Page Title="Proveedores" Language="C#" MasterPageFile="~/Site1.Master"
         AutoEventWireup="true" CodeBehind="Proveedores.aspx.cs" Inherits="Monolito4bm.Proveedores" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<link rel="stylesheet"
      href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css"
      crossorigin="anonymous"/>
<style>
:root {
  --card-bg:   rgba(255,255,255,0.72);
  --radius:    18px;
  --accent:    #7a4aaa;
  --accent2:   #5a2a8a;
  --danger:    #c0392b;
  --success:   #27ae60;
  --warn:      #e67e22;
  --row-hover: rgba(122,74,170,0.07);
}

/* ── Encabezado ─────────────────────────────────────────────── */
.page-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:26px; flex-wrap:wrap; gap:12px; }
.page-title { font-size:1.65rem; font-weight:700; color:var(--accent2); display:flex; align-items:center; gap:10px; }

/* ── Cards ──────────────────────────────────────────────────── */
.card {
  background:var(--card-bg); backdrop-filter:blur(12px);
  border:1px solid rgba(180,150,220,0.4);
  border-radius:var(--radius); padding:22px 28px;
  box-shadow:0 4px 24px rgba(120,80,180,0.10); margin-bottom:24px;
}
.card-title { font-size:1rem; font-weight:700; color:var(--accent2); margin-bottom:16px; display:flex; align-items:center; gap:8px; }

/* ── Buscador ───────────────────────────────────────────────── */
.search-bar {
  display:flex; align-items:center; gap:10px;
  background:rgba(255,255,255,0.9); border:1.5px solid rgba(122,74,170,0.3);
  border-radius:40px; padding:9px 18px;
  box-shadow:0 2px 10px rgba(120,80,180,.07); margin-bottom:14px; transition:border-color .2s, box-shadow .2s;
}
.search-bar:focus-within { border-color:var(--accent); box-shadow:0 0 0 3px rgba(122,74,170,0.12); }
.search-bar input { border:none; background:transparent; font-family:inherit; font-size:.95rem; color:#2c1a4a; flex:1; outline:none; }
.search-bar .si { color:var(--accent); font-size:1rem; }

/* ── Filtros ────────────────────────────────────────────────── */
.filtros-toggle { background:none; border:none; cursor:pointer; font-family:inherit; font-size:.83rem; font-weight:700; color:var(--accent); display:flex; align-items:center; gap:6px; padding:4px 0; margin-bottom:10px; }
.filtros-panel { display:none; gap:14px; flex-wrap:wrap; padding:16px; background:rgba(122,74,170,0.04); border-radius:14px; border:1px solid rgba(122,74,170,0.14); margin-bottom:14px; }
.filtros-panel.open { display:flex; animation:fadeIn .22s ease; }
@keyframes fadeIn { from{opacity:0;transform:translateY(-4px)} to{opacity:1;transform:none} }
.fg { display:flex; flex-direction:column; gap:5px; min-width:160px; flex:1; }
.fg label { font-size:.76rem; font-weight:700; color:var(--accent2); letter-spacing:.3px; }
.form-control { padding:9px 13px; border-radius:10px; border:1.5px solid rgba(122,74,170,0.28); background:rgba(255,255,255,0.88); font-family:inherit; font-size:.88rem; color:#2c1a4a; width:100%; transition:border-color .2s, box-shadow .2s; }
.form-control:focus { outline:none; border-color:var(--accent); box-shadow:0 0 0 3px rgba(122,74,170,0.13); }

/* ── Botones ────────────────────────────────────────────────── */
.btn { padding:9px 20px; border-radius:30px; border:none; cursor:pointer; font-family:inherit; font-size:.86rem; font-weight:700; display:inline-flex; align-items:center; gap:7px; transition:all .2s; white-space:nowrap; text-decoration:none; }
.btn-primary   { background:var(--accent);  color:#fff; }
.btn-primary:hover   { background:var(--accent2); transform:translateY(-1px); box-shadow:0 4px 14px rgba(90,42,138,.28); }
.btn-secondary { background:rgba(122,74,170,0.11); color:var(--accent2); border:1.5px solid rgba(122,74,170,0.28); }
.btn-secondary:hover { background:rgba(122,74,170,0.2); }
.btn-success   { background:var(--success); color:#fff; }
.btn-success:hover   { background:#1e8449; transform:translateY(-1px); }
.btn-danger    { background:var(--danger);  color:#fff; }
.btn-danger:hover    { background:#a93226; transform:translateY(-1px); }
.btn-warn      { background:var(--warn);    color:#fff; }
.btn-warn:hover      { background:#d35400; }
.btn-sm  { padding:5px 13px; font-size:.76rem; border-radius:20px; }

/* ── Alertas ────────────────────────────────────────────────── */
.alert { padding:11px 16px; border-radius:12px; margin-bottom:14px; font-size:.86rem; font-weight:600; display:flex; align-items:center; gap:9px; animation:fadeIn .3s ease; }
.alert-success { background:rgba(39,174,96,.14); color:#1e8449; border:1px solid rgba(39,174,96,.28); }
.alert-danger  { background:rgba(192,57,43,.11); color:#c0392b; border:1px solid rgba(192,57,43,.24); }

/* ── Tabla ──────────────────────────────────────────────────── */
.grid-wrapper { overflow-x:auto; border-radius:14px; }
.prov-grid { width:100%; border-collapse:collapse; font-size:.88rem; }
.prov-grid thead tr { background:linear-gradient(90deg,var(--accent),var(--accent2)); color:#fff; }
.prov-grid thead th { padding:12px 15px; text-align:left; font-size:.79rem; font-weight:700; letter-spacing:.5px; white-space:nowrap; }
.prov-grid thead th:first-child { border-radius:13px 0 0 0; }
.prov-grid thead th:last-child  { border-radius:0 13px 0 0; }
.prov-grid tbody tr { border-bottom:1px solid rgba(180,150,220,0.2); transition:background .15s; }
.prov-grid tbody tr:hover { background:var(--row-hover); }
.prov-grid tbody td { padding:11px 15px; vertical-align:middle; }

/* ── Badge estado ───────────────────────────────────────────── */
.badge { display:inline-flex; align-items:center; gap:5px; padding:3px 11px; border-radius:20px; font-size:.72rem; font-weight:700; letter-spacing:.3px; }
.badge-activo   { background:rgba(39,174,96,.14); color:#1e8449; }
.badge-inactivo { background:rgba(192,57,43,.11); color:#c0392b; }

/* ── Paginador ──────────────────────────────────────────────── */
.pager-wrap { display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:12px; margin-top:16px; }
.pager-info { font-size:.8rem; color:#999; }
.pager-btns { display:flex; align-items:center; gap:6px; }
.pager-btn { min-width:34px; height:34px; border-radius:50%; border:1.5px solid rgba(122,74,170,0.28); background:rgba(255,255,255,0.8); color:var(--accent2); font-weight:700; font-size:.84rem; cursor:pointer; display:flex; align-items:center; justify-content:center; transition:all .2s; font-family:inherit; text-decoration:none; }
.pager-btn:hover, .pager-btn.active { background:var(--accent); color:#fff; border-color:var(--accent); }
.pager-btn:disabled { opacity:.4; cursor:default; }

/* ── Modal ──────────────────────────────────────────────────── */
.modal-overlay { display:none; position:fixed; inset:0; background:rgba(40,20,70,0.52); backdrop-filter:blur(6px); z-index:999; align-items:center; justify-content:center; }
.modal-overlay.open { display:flex; }
.modal-box { background:rgba(246,242,255,0.97); border-radius:22px; padding:32px 34px; max-width:430px; width:92%; box-shadow:0 20px 60px rgba(90,42,138,0.28); animation:popIn .3s cubic-bezier(.34,1.56,.64,1); }
@keyframes popIn { from { opacity:0; transform:scale(.88) translateY(18px); } to   { opacity:1; transform:none; } }
.modal-title   { font-size:1.08rem; font-weight:700; color:var(--accent2); margin-bottom:18px; }
.modal-row     { display:flex; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
.modal-actions { display:flex; gap:10px; justify-content:flex-end; margin-top:4px; }

@media(max-width:600px){ .card { padding:14px 12px; } .modal-box { padding:20px 16px; } .modal-row { flex-direction:column; } }
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
    <div class="search-bar">
      <span class="si"><i class="fa-solid fa-magnifying-glass"></i></span>
      <asp:TextBox ID="txtBuscar" runat="server"
                   placeholder="Buscar proveedor por nombre..."
                   AutoPostBack="true" OnTextChanged="Buscar_Changed"/>
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
          <asp:ListItem Value=""  Text="Todos los estados"/>
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

  <div class="card">
      <div class="card-title">
          <i class="fa-solid fa-list"></i> Lista de Proveedores
          <span style="margin-left:auto;font-size:.78rem;color:#aaa;font-weight:400;">
            <asp:Literal ID="litTotal" runat="server"/> &mdash; Ordenado por más reciente
          </span>
      </div>

      <asp:HiddenField ID="hfPagina"    runat="server" Value="1"/>
      <asp:HiddenField ID="hfTotalPags" runat="server" Value="1"/>

      <asp:UpdatePanel ID="upGridProveedores" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
              <div class="grid-wrapper">
                  <asp:GridView ID="gvProveedores" runat="server"
                                AutoGenerateColumns="false" CssClass="prov-grid"
                                DataKeyNames="prov_id" GridLines="None"
                                OnRowCommand="gvProveedores_RowCommand">
                      <Columns>
                          <asp:BoundField DataField="prov_id"     HeaderText="ID"     ItemStyle-Width="60px"/>
                          <asp:BoundField DataField="prov_nombre" HeaderText="NOMBRE" ItemStyle-Width="450px"/>
                          
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
                          <div style="padding:34px;text-align:center;color:#bbb;font-size:.93rem;">
                              <i class="fa-solid fa-box-open" style="font-size:2rem;display:block;margin-bottom:10px;color:rgba(122,74,170,0.2)"></i>
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
          </ContentTemplate>
          <Triggers>
              <%-- Triggers para que los filtros despierten la actualización parcial --%>
              <asp:AsyncPostBackTrigger ControlID="txtBuscar" EventName="TextChanged" />
              <asp:AsyncPostBackTrigger ControlID="ddlFiltroEstado" EventName="SelectedIndexChanged" />
              <asp:AsyncPostBackTrigger ControlID="btnLimpiarFiltros" EventName="Click" />
          </Triggers>
      </asp:UpdatePanel>
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
        <button class="btn btn-secondary" onclick="cerrarModal(); return false;">
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

  <asp:HiddenField ID="hfModalAbierto"    runat="server" Value="0"/>
  <asp:HiddenField ID="hfFiltrosAbiertos" runat="server" Value="0"/>

  <script>
  function abrirModal() {
    document.getElementById('modalProveedor').classList.add('open');
    document.getElementById('<%= hfModalAbierto.ClientID %>').value = '1';
  }
  function cerrarModal() {
    document.getElementById('modalProveedor').classList.remove('open');
    document.getElementById('<%= hfModalAbierto.ClientID %>').value = '0';
  }
  document.addEventListener('keydown', e => { if (e.key === 'Escape') cerrarModal(); });
  document.getElementById('modalProveedor').addEventListener('click', function(e){ if(e.target===this) cerrarModal(); });

  function inicializarComponentes() {
    if (document.getElementById('<%= hfModalAbierto.ClientID %>').value === '1')
      document.getElementById('modalProveedor').classList.add('open');
    if (document.getElementById('<%= hfFiltrosAbiertos.ClientID %>').value === '1') {
      document.getElementById('filtrosPanel').classList.add('open');
      document.getElementById('arrowFilt').innerHTML = '<i class="fa-solid fa-chevron-up"></i>';
    }
  }

  window.addEventListener('DOMContentLoaded', inicializarComponentes);

  // Re-inicializar scripts nativos tras la ejecución de AJAX del UpdatePanel
  if (typeof Sys !== 'undefined') {
      Sys.WebForms.PageRequestManager.getInstance().add_endRequest(inicializarComponentes);
  }

  function toggleFiltros() {
    var p  = document.getElementById('filtrosPanel');
    var a  = document.getElementById('arrowFilt');
    var hf = document.getElementById('<%= hfFiltrosAbiertos.ClientID %>');
          p.classList.toggle('open');
          var open = p.classList.contains('open');
          a.innerHTML = open ? '<i class="fa-solid fa-chevron-up"></i>' : '<i class="fa-solid fa-chevron-down"></i>';
          hf.value = open ? '1' : '0';
      }
  </script>

</asp:Content>