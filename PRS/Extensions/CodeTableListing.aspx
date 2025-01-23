<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CodeTableListing.aspx.cs" Inherits="IP.Extensions.CodeTableListing" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="row">
            <div class="col-md-2">
                <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="true" OnSelectedIndexChanged="gvList_SelectedIndexChanged" AutoGenerateSelectButton="True" ShowHeaderWhenEmpty="True">
                </asp:GridView>
            </div>
            <div class="col-md-10">
                <asp:GridView ID="gvData" runat="server" AutoGenerateColumns="true" Width="100%" ShowHeaderWhenEmpty="true">
                </asp:GridView>
            </div>

        </div>
    </form>
</body>
</html>
