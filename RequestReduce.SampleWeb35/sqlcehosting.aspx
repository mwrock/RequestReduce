<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
public void Page_Load(object sender, EventArgs e)
{
    AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
}
</script>
<html></html>