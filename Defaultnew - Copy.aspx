<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Defaultnew.aspx.cs" Inherits="WebApplication2._Defaultnew" %>

<%--<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
   <div>
       <asp:GridView ID="GridView1" runat="server"></asp:GridView>
       <asp:GridView ID="gvWeight" runat="server"></asp:GridView>
       <asp:Button ID="Butbtnsendexcel" runat="server" Text="Button" OnClick="Butbtnsendexcel_Click" />
       <br />
       <br />
       <asp:Label ID="lblerror" runat="server" ></asp:Label>
       </div>

</asp:Content>