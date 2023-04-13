<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WordMerger._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Формирование документа слияния</title>
    <base target="_self" />
</head>
<body>
    <form id="form1" runat="server">
        <div style="height: 300px">
            <table style="width: 100%; height: 100%;">
                <tr style="height: 30px;">
                    <td style="font-family: Arial, Helvetica, sans-serif; color: #808080;">
                        <strong>Формирование документа слияния </strong>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="Label1" runat="server" Text="Label" Font-Names="Arial"
                            Font-Size="Small"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="TextBox1" runat="server" TextMode="MultiLine" Width="100%" Height="160px"
                            ReadOnly="True" BorderStyle="Inset" Font-Size="Small"></asp:TextBox>
                    </td>
                </tr>
                <tr style="height: 40px">
                    <td style="border-top: 1px solid #ffffff; text-align: center;">&nbsp;
                    <button onclick="window.opener='x';window.open('', '_self', '');window.close();">
                        Закрыть
                    </button>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
