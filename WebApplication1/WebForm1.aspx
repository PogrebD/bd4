<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication1.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div>
                <asp:Label ID="Label1" runat="server" Text="Задание №1"></asp:Label>
            </div>
             
            <br />
            
            <div>
                <asp:Label ID="Label2" runat="server" Text="Получить информацию о деталях, которых в настоящий момент
                не хватает для изготовления заданного количества указанного изделия."></asp:Label>
            </div>
            
            <br />
            
            <div>
                <asp:Label ID="Label3" runat="server" Text="Введите название изделия:"></asp:Label>
                <br />
                <asp:DropDownList ID="JobDropDownList1" runat="server" AutoPostBack="False"> 
                </asp:DropDownList>

            </div>
            
            <br />

            <div>
                <asp:Label ID="Label4" runat="server" Text="Введите количество изделий:"></asp:Label>
                <br />
                <asp:TextBox ID="TextBox2" runat="server" AutoPostBack="False"></asp:TextBox>
            </div>
            
            <br />
            
            <div>
                <asp:Label ID="Label6" runat="server" Text="Статус:"></asp:Label>
                <asp:Label ID="Label5" runat="server" Text=""></asp:Label>
            </div>
            
            <br />
            
            <div>
                <asp:Button ID="Button1" runat="server" OnClick="Button1_OnClick" Text="Отправить запрос" />
                <asp:Button ID="Button2" runat="server" OnClick="Button2_OnClick" Text="Задание 2" />
            </div>
            
            <br />
            
            <div>
                <table ID="Table1" runat="server"></table>
                <asp:Label ID="Label7" runat="server" Text=""></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>
