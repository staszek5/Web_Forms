<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WorkTimeEstimateReport.aspx.cs" ValidateRequest="False"
    Inherits="Firmeo.UI.ZUK.Reports.WorkTimeEstimateReport" MasterPageFile="../../../MasterPages/ReportingMasterPage.master" %>

<asp:Content ID="P" ContentPlaceHolderID="P" runat="Server">
<telerik:RadCodeBlock runat="server"> 
<script language="javascript" type="text/javascript">

    function RaportTypeChanged(arg) {
        var divAll = $("#divAllDep");
        var divInstall = $("#divInstallationDep");
        var divWorking = $("#divWorkingDep");
    
        if (arg.value == "rbMontaz") {
            divAll.hide();
            divWorking.hide();
            divInstall.show();
        }
        else if (arg.value == "rbObrobka") {
            divAll.hide();
            divInstall.hide();
            divWorking.show();
        }
        else {
            divInstall.hide();
            divWorking.hide();
            divAll.show();
        }
        return true;
    }

</script>
</telerik:RadCodeBlock>
    <div class="section">
        <label>
            <asp:Label ID="lblParameters" runat="server" Text="Parametry"/>
        </label>
        <div>
            <div class="column">
                <div class="row">
                    <div class="label-120px">
                        <asp:Label ID="lblProject" runat="server" Text="Zlecenie:" />
                    </div>
                    <div class="input">
                        <firmeo:webcombobox id="project" runat="server" fillonload="false" addemptyrow="true"
                            usewebservice="true" identifiercolumnname="PRO_ID" tablename="WBV_PROJECTS_COMBO"
                            fieldrequired="false">
                            <Columns>
                                <firmeo:ComboColumn DisplayedColumnName="Number" ViewColumnName="PRO_NUMBER" Width="100px" />
                                <firmeo:ComboColumn DisplayedColumnName="Nazwa" ViewColumnName="PRO_SUMMARY" Width="100" />                
                                <firmeo:ComboColumn DisplayedColumnName="Data" ViewColumnName="PRO_DATE" Width="100" />                
                                <firmeo:ComboColumn DisplayedColumnName="Status" ViewColumnName="PRO_STATUS" Width="100" />                
                            </Columns>
                        </firmeo:webcombobox>
                    </div>
                </div>
                <div class="row">
                    <div class="label-120px">
                        <asp:Label ID="Label5" runat="server" Text="Dział:" />
                    </div>
                    <div class="input" id="divAllDep">
                        <firmeo:WebLightComboBox id="departamentAll" runat="server" IdentifierColumnName="DCODE" TableName="WBV_WORK_DEPARTMENTS_ALL_COMBO"
                            ViewColumnName="DNAME" AddEmptyRow="true" FieldRequired="false"/>
                    </div>
                    <div class="input" id="divInstallationDep" style="display: none">
                        <firmeo:WebLightComboBox id="departamentInstl" runat="server" IdentifierColumnName="DCODE" TableName="WBV_WORK_DEPA_MONTAGE_COMBO"
                            ViewColumnName="DNAME" AddEmptyRow="true" FieldRequired="false"/>
                    </div>
                    <div class="input" id="divWorkingDep" style="display: none">
                        <firmeo:WebLightComboBox id="departamentWork" runat="server" IdentifierColumnName="DCODE" TableName="WBV_WORK_DEPA_WORKING_COMBO"
                            ViewColumnName="DNAME" AddEmptyRow="true" FieldRequired="false"/>
                    </div>
                </div>
                <asp:Panel ID="searchingPanel" runat="server" Visible="false" >
                    <div class="row">
                        <div class="label-120px">
                            <asp:Label ID="Label1" runat="server" Text="Nr Planu:" />
                        </div>                   
                        <div class="input">
                            <firmeo:webtextbox id="tbPlanNo" runat="server" />
                        </div>                 
                    </div>
                <div class="row">
                    <div class="label-120px">
                        <asp:Label ID="Label2" runat="server" Text="Materiał:" />
                    </div>                    
                    <div class="input">
                        <firmeo:webtextbox id="tbMaterial" runat="server" />
                    </div>                                        
                </div>
                <div class="row">
                    <div class="label-120px">
                        <asp:Label ID="Label3" runat="server" Text="Wymiar:" />
                    </div>                     
                    <div class="input">
                        <firmeo:webtextbox id="tbSize" runat="server" />
                    </div> 
                </div>
                </asp:Panel>
                
            </div>
            <div class="column">
               
                    <div class="row">
                        <div class="label-120px">
                            <asp:Label ID="Label8" runat="server" Text="Pokaż:" />
                        </div>
                        <div class="input">
                            <asp:RadioButton ID="rbAll" runat="server" Checked="true" Text="Zbiorczo" GroupName="reportType" 
                                OnClick="RaportTypeChanged(this);" />
                            <asp:RadioButton ID="rbMontaz" runat="server" Text="Szczegóły montażu" GroupName="reportType" 
                                OnClick="RaportTypeChanged(this);"/>
                            <asp:RadioButton ID="rbObrobka" runat="server" Text="Szczegóły obróbki" GroupName="reportType" 
                                OnClick="RaportTypeChanged(this);"/>
                        </div>
                    </div> 
                    
                    <asp:Panel ID="sortingPanel" runat="server" Visible="false">               
                    <div class="row">
                        <div class="label-120px">
                            <asp:Label ID="Label4" runat="server" Text="Sortowanie:" />
                        </div>                                    
                    </div>
                    <div class="row">
                        <div class="label-120px">
                            <asp:Label ID="Label6" runat="server" Text="Materiał:" />
                        </div> 
                         <div class="input">
                            <asp:DropDownList id="dbMaterialSorting" AutoPostBack="False" runat="server">
                                <asp:ListItem Selected="True" Value=""></asp:ListItem>
                                <asp:ListItem Value="Ascending"> Rosnąco </asp:ListItem>
                                <asp:ListItem Value="Descending"> Malejąco </asp:ListItem>
                            </asp:DropDownList>
                        </div>                
                    </div>
                    <div class="row">
                        <div class="label-120px">
                            <asp:Label ID="Label7" runat="server" Text="Wymiar:" />
                        </div>  
                        <div class="input">
                            <asp:DropDownList id="dbSizeSorting" AutoPostBack="False" runat="server">
                                <asp:ListItem Selected="True" Value=""></asp:ListItem>
                                <asp:ListItem Value="Ascending"> Rosnąco </asp:ListItem>
                                <asp:ListItem Value="Descending"> Malejąco </asp:ListItem>
                            </asp:DropDownList>
                        </div>            
                    </div>
                </asp:Panel>              
                
            </div>
            <em></em>
        </div>
    </div>   
</asp:Content>
