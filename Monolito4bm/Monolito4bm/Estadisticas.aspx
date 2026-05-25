<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Estadisticas.aspx.cs" Inherits="Monolito4bm.Estadisticas" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Contenedor principal con temática Lavanda */
        .dashboard-wrapper {
            background-color: #F8F5FA; /* Fondo muy suave lavanda clarito */
            padding: 20px;
            /* Usamos calc para restar el tamaño aproximado de tu barra de navegación (ej. 80px) */
            height: calc(100vh - 80px); 
            display: flex;
            flex-direction: column;
            border-radius: 12px;
            box-shadow: 0 8px 20px rgba(147, 112, 219, 0.15); /* Sombra elegante tono lavanda */
            box-sizing: border-box;
        }

        /* Título estilizado */
        .dashboard-title {
            color: #6A5ACD; /* SlateBlue / Lavanda oscuro */
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-weight: 700;
            margin-top: 0;
            margin-bottom: 20px;
            text-align: center;
            text-transform: uppercase;
            letter-spacing: 1.5px;
        }

        /* Contenedor del reporte para que sea 100% responsivo */
        .report-container {
            flex-grow: 1; /* Toma todo el espacio sobrante hacia abajo */
            background: white;
            border: 2px solid #D8BFD8; /* Borde Thistle (Lavanda pastel) */
            border-radius: 8px;
            overflow: hidden; /* Evita dobles barras de desplazamiento */
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    
    <div class="dashboard-wrapper">
        <h2 class="dashboard-title">Panel de Estadísticas</h2>
        
        <div class="report-container">
            <rsweb:ReportViewer ID="ReportViewerDashboard" runat="server" Width="100%" Height="100%" ZoomMode="PageWidth" SizeToReportContent="false" AsyncRendering="true">
                <LocalReport ReportPath="PanelEstadisticas.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="ODS_Proveedores" Name="DataSetProveedores" />
                        <rsweb:ReportDataSource DataSourceId="ODS_TopCaros" Name="DataSetTopCaros" />
                        <rsweb:ReportDataSource DataSourceId="ODS_TopStock" Name="DataSetTopStock" />
                        <rsweb:ReportDataSource DataSourceId="ODS_Estados" Name="DataSetEstados" />
                        <rsweb:ReportDataSource DataSourceId="ODS_Salud" Name="DataSetSalud" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>
        </div>
    </div>

    <asp:ObjectDataSource ID="ODS_Proveedores" runat="server" 
        SelectMethod="ObtenerEstadisticasPorProveedor" TypeName="Capa_Negocios.CN_Estadisticas"></asp:ObjectDataSource>

    <asp:ObjectDataSource ID="ODS_TopCaros" runat="server" 
        SelectMethod="ObtenerTop5MasCaros" TypeName="Capa_Negocios.CN_Estadisticas"></asp:ObjectDataSource>

    <asp:ObjectDataSource ID="ODS_TopStock" runat="server" 
        SelectMethod="ObtenerTop5MayorStock" TypeName="Capa_Negocios.CN_Estadisticas"></asp:ObjectDataSource>

    <asp:ObjectDataSource ID="ODS_Estados" runat="server" 
        SelectMethod="ObtenerDistribucionEstados" TypeName="Capa_Negocios.CN_Estadisticas"></asp:ObjectDataSource>

    <asp:ObjectDataSource ID="ODS_Salud" runat="server" 
        SelectMethod="ObtenerSaludInventario" TypeName="Capa_Negocios.CN_Estadisticas"></asp:ObjectDataSource>
</asp:Content>
