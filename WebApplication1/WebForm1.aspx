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
             <asp:Label ID="Label1" runat="server" Text="Задание №1"></asp:Label>
            <br />
            <asp:Label ID="Label2" runat="server" Text="Получить информацию о деталях, которых в настоящий момент
не хватает для изготовления заданного количества указанного изделия."></asp:Label>
            <br />
            <asp:Label ID="Label3" runat="server" Text="Введите название изделия:"></asp:Label>
            <br />
            <asp:DropDownList ID="JobDropDownList1" runat="server" OnSelectedIndexChanged="JobDropDownList1_SelectedIndexChanged"> 

            </asp:DropDownList>
            <br />
            <asp:Label ID="Label4" runat="server" Text="Введите Количество изделий:"></asp:Label>
            <br />
            <asp:TextBox ID="TextBox2" runat="server" ></asp:TextBox>

        </div>
    </form>
</body>
</html>
