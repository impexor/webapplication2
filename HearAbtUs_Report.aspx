<%@ Page Title="" Language="C#" MasterPageFile="~/FFHP.Master" AutoEventWireup="true" CodeBehind="HearAbtUs_Report.aspx.cs" Inherits="WebApplication2.HearAbtUs_Report" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <table width="100%">
<tr>
<td class="lblheading">Delivery Information</td>
</tr>
    <tr>
    <td>
    <table>
<tr>
<td>From Date<asp:TextBox ID="TbxFromDate" Width="75px" runat="server" ></asp:TextBox><asp:Image
                                ID="Image1" ImageUrl="~/Images/cal1.png" runat="server" />
<cc1:CalendarExtender ID="CalendarExtender1"
                                    runat="server" Animated="true" ClearTime="true" DefaultView="Days" Format="MM/dd/yyyy"
                                    PopupPosition="BottomRight" TargetControlID="TbxFromDate" PopupButtonID="Image1">
                                </cc1:CalendarExtender><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" 
        ErrorMessage="*" ControlToValidate="TbxFromDate" 
        ValidationGroup="Date"></asp:RequiredFieldValidator>
To Date
<asp:TextBox ID="TbxToDate" Width="75px" runat="server" ></asp:TextBox><asp:Image
                                ID="Image2" ImageUrl="~/Images/cal1.png" runat="server" /><cc1:CalendarExtender ID="CalendarExtender2"
                                    runat="server" Animated="true" ClearTime="true" DefaultView="Days" Format="MM/dd/yyyy"
                                    PopupPosition="BottomRight" TargetControlID="TbxToDate" PopupButtonID="Image2">
                                </cc1:CalendarExtender><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
        ErrorMessage="*" ControlToValidate="TbxToDate" ValidationGroup="Date"></asp:RequiredFieldValidator>
       <asp:Button ID="btnsubmit" Text="Submit" OnClick="btnsubmit_OnClick" runat="server" ValidationGroup="Date" />
        </td>
</tr>
        <tr>
            <td>
                <br />
                <br />
                 <asp:GridView ID="gvReport" runat="server" Width="80%" Font-Names="Verdana">
                      <AlternatingRowStyle BackColor="#CCCCCC" />
                    
                      <HeaderStyle BackColor="#3399FF" />
                 </asp:GridView>
            </td>
        </tr>
       
        <tr><td>
            <asp:Button ID="btnExportExcel" runat="server" Text="ExportToExcel" OnClick="btnExportExcel_Click" />
            
            <asp:Button ID="btnExportPdf" runat="server" Text="ExportToPdf" OnClick="btnExportPdf_Click" />
            </td>

        </tr>
</table>

  
</asp:Content>
