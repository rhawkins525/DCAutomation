<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<%@ OutputCache Duration="30" Location="Server" VaryByParam="NewsFilter" %>
<!DOCTYPE html>
<html>
   <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
   <meta name="viewport" content="target-densitydpi=device-dpi" />
   <meta name="HandheldFriendly" content="true"/>
   <link rel="stylesheet" href="../../js/stylev2.1.css">
   <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
   <script src="https://code.jquery.com/jquery-1.12.4.js"></script>
   <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>
   <link rel="stylesheet" href="../../js/fontawesome-pro-5.2.0-web/css/all.css">
   <head>
      <link rel="icon" href="../../Images/LS Logo Box.jpg">
      <link rel="apple-touch-icon" href="https://media.licdn.com/mpr/mpr/shrink_200_200/AAEAAQAAAAAAAAW0AAAAJGQyZTBkOTIyLTYyMTEtNGQ5Yi05MjU1LTgyNzNjY2MwMTNlMQ.png">
      <link rel="apple-touch-icon" href="../../Images/LS Logo Box.jpg">
      <META HTTP-EQUIV="REFRESH" CONTENT="43200;URL=../../.auth/logout">
   </head>
   <title><%=page_title%></title>
   <body>
      <style>
          .mobileWidth {
              width: 10px;
          }
          body {
              float:left;
              min-width:100%; 
              }
         label {
         vertical-align:middle;
         font-size:8px;
         display: inline-block;
         width: 100px;
         text-align: right;
         }
         fieldset {
         margin: 0px;
         border: 1px solid white;
         padding: 0px;    
         border-radius: 0px;
         overflow: hidden;
         display: inline-block;
         }
         .legend {
         padding: 0px;    
         background-color:white;
         color:#013D79;  
         font-weight:bold;
         }
         .legendmobile {
         padding: 0px;    
         background-color:white;
         color:#013D79;  
         font-weight:bold;
         font-size:28px;
         }
         .test {
         height: 16px;
         }
         .test1 {
         height: 14px;
         margin-top:5px;
         }
         pup { 
            color: blue;
         }
         pdown { 
            color: red; 
         }
         .attributionlabel {
         font-size:10px;
         text-align:right;
         float:left;
         }
         .attributionlabelmobile {
         font-size:10px;
         text-align:right;
         }
         .ddlLabel {
         font-size:12px;
         font-weight: bold;
         }
         .ddlLabelMobile {
         font-size:20px;
         font-weight: bold;
         }
         #GridView3 {
         white-space:nowrap;
         }
         .auto-style1 {
         width: 100%;
         }
.button1 {
  font-size: 12px;
  text-decoration: none;
  background-color: #EEEEEE;
  color: #333333;
  padding: 2px 6px 2px 6px;
  border-top: 1px solid #CCCCCC;
  border-right: 1px solid #333333;
  border-bottom: 1px solid #333333;
  border-left: 1px solid #CCCCCC;
  margin-top:8px;
  margin-bottom: 12px;
}
      </style>

       <!-- #include file="~/webpages/templates/left_menu.html" -->

      <form id="form1" runat="server">
         <div class="w3-main" style="margin-left:160px">
         <header class="w3-container w3-light-blue">
            <span class="w3-opennav w3-xlarge w3-hide-large" style="float:left;padding-top:17px;padding-right:22px" onclick="w3_open()"><img src="../../Images/threelines.png" height="40" /></span>
            <div class="cls" runat="server" id="MyServerControlDiv" style="float:left"></div>
         </header>

             <!-- #include file="~/webpages/templates/top_menu_pfol_news.html" -->

         <asp:HiddenField ID="hdnfldTickerList" runat="server" />
         <input type="hidden" id="project-id" />        
         <asp:ScriptManager ID ="ScriptManager1" runat="server"> </asp:ScriptManager>
         <asp:UpdatePanel ID="UpdatePanel1" runat="server">
             
             

            <ContentTemplate>
               <ul class="w3-navbar2Mobile w3-white" runat="server" ID="Index_List"></ul>
               <div class="w3-container" style="padding-left:12px">
                   
                  <table>
                     <tr>
                        <td>
                           <div id="RegionLabel" runat="server">&nbsp;Region:</div>
                        </td>
                        <td>
                            <asp:dropDownList runat="server" AutoPostBack="true" ID="ddlRegion" style="font-size:12px; width:100px">
                                <asp:ListItem Text="All" Value="-1" />
                                <asp:ListItem Text="America" Value="NORTH AMERICA" />
                                <asp:ListItem Text="Japan" Value="JAPAN" />
                                <asp:ListItem Text="China" Value="CHINA" />
                                <asp:ListItem Text="China (Ex-Taiwan)" Value="China (Ex-Taiwan)" />
                                <asp:ListItem Text="Europe" Value="EUROPE" />
                                <asp:ListItem Text="Other" Value="Other" />
                            </asp:dropDownList>
                        </td>
                        <td>
                           <div id="SectorLabel" runat="server">&nbsp;&nbsp;&nbsp;Sector:</div>
                        </td>
                        <td>
                            <asp:dropDownList runat="server" AutoPostBack="true" ID="ddlSector" style="font-size:12px; width:100px">
                                <asp:ListItem Text="All" Value="-1" />
                                <asp:ListItem Text="Cloud" Value="Cloud" />
                                <asp:ListItem Text="eCommerce" Value="eCommerce" />
                                <asp:ListItem Text="Mobile" Value="Mobile" />
                                <asp:ListItem Text="Other" Value="Other" />
                                <asp:ListItem Text="Sharing & Transport" Value="Sharing" />
                                <asp:ListItem Text="Social" Value="Social" />
                            </asp:dropDownList>
                        </td>
                     </tr>
                  </table>
                   
                  <table id="EarningsTables">
                     <tr style="vertical-align:top">
                        <td style="width:33%">
                           <fieldset style="width:100%;padding-left:0;margin-left:0">
                              <table>
                                 <tr>
                                    <td>
                                       <div id="TitleBlue1" style="white-space:nowrap;" runat="server">Upcoming Portfolio Earnings</div>
                                    </td>
                                    <td style="white-space:nowrap;">
                                       <%=CombAttString%>
                                    </td>
                                 </tr>
                              </table>
                            
                              <asp:GridView ID="GridView2" runat="server" ShowHeaderWhenEmpty="true" width="100%" AutoGenerateColumns="False" onrowdatabound="EarningsDataBound1" DataSourceID="SqlDataSource2" AllowSorting="True" BackColor="White" BorderColor="black" BorderStyle="solid" BorderWidth="1px" CellPadding="3" Font-Size="XSmall" GridLines="None">
                                 <AlternatingRowStyle BackColor="#f4f4f4" />
                                 <Columns>
                                    <asp:BoundField DataField="MSLongDescription" HeaderText="" ItemStyle-Wrap="false" SortExpression="MSLongDescription" ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N}" />
                                    <asp:BoundField DataField="Report Date" HeaderText="Date" ItemStyle-Wrap="false" SortExpression="Report Date" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:d}" />
                                    <asp:BoundField DataField="Report Time" HeaderText="Pacific Time" ItemStyle-Wrap="false" SortExpression="Report Time" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:T}" />
                                    <asp:BoundField DataField="BloombergID" HeaderText="BloombergID" ItemStyle-Wrap="false" SortExpression="BloombergID" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:T}" />
                                    <asp:BoundField DataField="Afterhoursmove" HeaderText="" ItemStyle-Wrap="false" SortExpression="Afterhoursmove" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                    <asp:BoundField DataField="GlenName" HeaderText="" ItemStyle-Wrap="false" SortExpression="GlenName" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                    <asp:BoundField DataField="DV" HeaderText="" ItemStyle-Wrap="false" SortExpression="DV" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                     <asp:BoundField DataField="CallTime" HeaderText="Call Link" ItemStyle-Wrap="false" SortExpression="DV" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                    <asp:BoundField DataField="ModelQuarterlyUpdate" HeaderText="%Equity" ItemStyle-Wrap="false" SortExpression="DV" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                    <asp:BoundField DataField="LODV" HeaderText="" ItemStyle-Wrap="false" SortExpression="LODV" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                 </Columns>
                                 <EmptyDataTemplate>No upcoming earnings</EmptyDataTemplate>
                                  <EmptyDataRowStyle BorderColor="Black" BorderWidth="1px" />
                                 <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
                                 <HeaderStyle BackColor="#dbdbdb" Font-Bold="True" ForeColor="Black" />
                                 <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                 <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
                                 <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                 <SortedDescendingCellStyle BackColor="#CAC9C9" />
                              </asp:GridView>
                           </fieldset>
                           <br />     
                             <fieldset style="width:100%;padding-left:0;margin-left:0">
                              <table>
                                 <tr>
                                    <td>
                                       <div id="TitleBlue5" style="white-space:nowrap;" runat="server">Upcoming Industry Earnings</div>
                                    </td>
                                    <td style="white-space:nowrap;">
                                       
                                    </td>
                                 </tr>
                              </table>
                            
                              <asp:GridView ID="GridView5" runat="server" ShowHeaderWhenEmpty="true" width="100%" AutoGenerateColumns="True" onrowdatabound="EarningsDataBound5" DataSourceID="SqlDataSource5" AllowSorting="True" BackColor="White" BorderColor="black" BorderStyle="solid" BorderWidth="1px" CellPadding="3" Font-Size="XSmall" GridLines="None">
                                 <AlternatingRowStyle BackColor="#f4f4f4" />
                                 <Columns>
                                    
                                 </Columns>
                                 <EmptyDataTemplate>No upcoming earnings</EmptyDataTemplate>
                                  <EmptyDataRowStyle BorderColor="Black" BorderWidth="1px" />
                                 <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
                                 <HeaderStyle BackColor="#dbdbdb" Font-Bold="True" ForeColor="Black" />
                                 <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                 <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
                                 <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                 <SortedDescendingCellStyle BackColor="#CAC9C9" />
                              </asp:GridView>
                           </fieldset>
                        </td>
                        <td style="width:33%">
                          
                           <fieldset style="width:100%">
                               <table>
                                   <tr>
                                       <td>
                              <div id="TitleBlue2" runat="server">Last Ten Days' Earnings</div>
                                           </td>
                                       <td></td>
                                       </tr>
                                   </table>
                              <asp:GridView ID="GridView3" runat="server" ShowHeaderWhenEmpty="true" width="100%" AutoGenerateColumns="False" ShowFooter="true" onrowdatabound="EarningsDataBound2" DataSourceID="SqlDataSource3" AllowSorting="True" BackColor="White" BorderColor="black" BorderStyle="solid" BorderWidth="1px" CellPadding="3" Font-Size="XSmall" GridLines="None">
                                 <AlternatingRowStyle BackColor="#f4f4f4" />
                                 <Columns>
                                    <asp:HyperLinkField HeaderText="" SortExpression="GlenName" datatextfield="GlenName" ItemStyle-Wrap="false" DataNavigateUrlFields="MSLongDescription, BloombergTicker" DataNavigateUrlFormatString="../webpages/LSPM/LSPMScenarioManagement.aspx?SecName={0}&Ticker={1}" />
                                    <asp:BoundField DataField="Last Earnings" HeaderText="Date" ItemStyle-Wrap="false" SortExpression="Last Earnings" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:d}" />
                                    <asp:BoundField DataField="ReturnFromLastEarnings" HeaderText="%Since" ItemStyle-Wrap="false" SortExpression="ReturnFromLastEarnings" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                    <asp:BoundField DataField="Long/Short" HeaderText="W/L" ItemStyle-Wrap="false" SortExpression="Long/Short" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                    <asp:BoundField DataField="ModelQuarterlyUpdate" HeaderText="MdlUp" ItemStyle-Wrap="false" SortExpression="ModelQuarterlyUpdate" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                     <asp:BoundField DataField="Long/Short" HeaderText="W/L" ItemStyle-Wrap="false" SortExpression="Long/Short" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                 </Columns>
                             
                                 <FooterStyle BackColor="#dbdbdb" ForeColor="Black" />
                                 <HeaderStyle BackColor="#dbdbdb" Font-Bold="True" ForeColor="Black" />
                                 <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                 <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
                                 <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                 <SortedDescendingCellStyle BackColor="#CAC9C9" />
                              </asp:GridView>
                           </fieldset>
                        </td>
                        <td style="width:33%">
                           <fieldset style="width:100%">
                               <table>
                                   <tr>
                                       <td>
                              <div id="TitleBlue3" runat="server">Lockup Expiries</div>
                                           </td>
                                       <td></td>
                                       </tr>
                                   </table>
                              <asp:GridView ID="GridView4" runat="server" ShowHeaderWhenEmpty="true" width="100%" AutoGenerateColumns="False" onrowdatabound="EarningsDataBound4" DataSourceID="SqlDataSource4" AllowSorting="True" BackColor="White" BorderColor="black" BorderStyle="solid" BorderWidth="1px" CellPadding="3" Font-Size="XSmall" GridLines="None">
                                 <AlternatingRowStyle BackColor="#f4f4f4" />
                                 <Columns>
                                    <asp:HyperLinkField HeaderText="" SortExpression="MSLongDescription" datatextfield="MSLongDescription" ItemStyle-Wrap="false" DataNavigateUrlFields="BloombergID" DataNavigateUrlFormatString="../webpages/LSPM/LSPMScenarioManagement.aspx?Ticker={0}" />
                                    <asp:BoundField DataField="LockupExpiry" HeaderText="Date" ItemStyle-Wrap="false" SortExpression="LockupExpiry" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:d}" />
                                     <asp:BoundField DataField="OFFERING_LOCKUP_SHARES" HeaderText="Shares" ItemStyle-Wrap="false" SortExpression="OFFERING_LOCKUP_SHARES" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N0}" />
                                     <asp:BoundField DataField="SHARESOUT" HeaderText="%ofShares" ItemStyle-Wrap="false" SortExpression="SHARESOUT" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N0}" />
                                     <asp:BoundField DataField="EQY_FLOAT" HeaderText="%ofFloat" ItemStyle-Wrap="false" SortExpression="EQY_FLOAT" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N0}" />
                                     <asp:BoundField DataField="in_portfolio" HeaderText="in_portfolio" ItemStyle-Wrap="false" SortExpression="in_portfolio" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N0}" />
                                     <asp:BoundField DataField="DV" HeaderText="%Equity" ItemStyle-Wrap="false" SortExpression="DV" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:N1}" />
                                 </Columns>
                                 <EmptyDataTemplate>No upcoming lockup expiries</EmptyDataTemplate>
                                 <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
                                 <HeaderStyle BackColor="#dbdbdb" Font-Bold="True" ForeColor="Black" />
                                 <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                 <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
                                 <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                 <SortedDescendingCellStyle BackColor="#CAC9C9" />
                              </asp:GridView>
                           </fieldset>
                        </td>
                     </tr>
                     <tr>
                        <td colspan="3">
                           <fieldset id="NewsStoriesSection" style="padding-left:0;margin-left:0;" class="auto-style1">
                               <table>
                                   <tr>
                                       <td>
                                            <div id="TitleBlue4" runat="server">News Stories</div>
                                       </td>
                                       <td></td>
                                       </tr>
                                   <tr>
                                       <td rowspan="2" style="padding: 0; margin: 0">
                                            <%-- Don't add buttons for: Bloomberg, The Information, StreetAccount, WSJ, TechCrunch, NY Times--%>
                                            <span style="font-size:12px"><b>Top Sources:</b></span>
                                           <asp:Button id="Button1" class="button1" runat="server" Text="All Stories" PostBackUrl="Default.aspx?NewsFilter=All" />
                                           <asp:Button id="btn_86research" class="button1" runat="server" Text="86Research" PostBackUrl="Default.aspx?NewsFilter=86Research" />
                                           <asp:Button id="btn_bloomberg" class="button1" runat="server" Text="Bloomberg" PostBackUrl="Default.aspx?NewsFilter=Bloomberg" />
                                           <asp:Button id="btn_blue_lotus" class="button1" runat="server" Text="Blue Lotus" PostBackUrl="Default.aspx?NewsFilter=Blue%20Lotus" />
                                           <asp:Button id="btn_bwg" class="button1" runat="server" Text="BWG" PostBackUrl="Default.aspx?NewsFilter=BWG%20Strategy" />
                                           <asp:Button id="btn_stratchery" class="button1" runat="server" Text="Stratechery" PostBackUrl="Default.aspx?NewsFilter=Stratechery" />
                                           <asp:Button id="btn_tegus" class="button1" runat="server" Text="Tegus" PostBackUrl="Default.aspx?NewsFilter=Tegus" />
                                           <asp:Button id="btn_third_bridge" class="button1" runat="server" Text="Third Bridge" PostBackUrl="Default.aspx?NewsFilter=Third%20Bridge" />
                                           
                                       </td>
                                   </tr>
                                   </table>
                              <asp:GridView ID="GridView1" ShowHeaderWhenEmpty="true" onrowcommand="GridView1_RowCommand" runat="server" AutoGenerateColumns="False" Width="100%" datakeynames="UpdateDate, Name, Source"  OnRowDataBound="NewsDataBound" DataSourceID="SqlDataSource1" AllowSorting="True" BackColor="White" BorderColor="black" BorderStyle="solid" BorderWidth="1px" CellPadding="3" Font-Size="XSmall" GridLines="None">
                                 <AlternatingRowStyle BackColor="#f4f4f4" />
                                 <Columns>
                               <asp:TemplateField ShowHeader="False">
                                <ItemTemplate>
                                    <asp:ImageButton ID="DeleteButton" runat="server" ImageUrl="../../Images/trashcan.png" CssClass="test1"
                                                CommandName="Delete"  CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" OnClientClick="return confirm('Are you sure you want to delete this news story?');"
                                                AlternateText="Delete" />               
                                </ItemTemplate>
                                </asp:TemplateField>  
                                    <asp:BoundField DataField="Source" HeaderText="Source" SortExpression="Source" ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N0}" ReadOnly="true" />
                                    <asp:BoundField DataField="Type" HeaderText="Type" SortExpression="Type" ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N0}" ReadOnly="true" />
                                    <asp:BoundField DataField="Name" HeaderText="Headline" SortExpression="Name" ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N0}" HtmlEncode="false" ReadOnly="true" />       
	                                <asp:CommandField ShowEditButton="true" ButtonType="Image" EditImageUrl="../../Images/pencil.png?random=1" ControlStyle-Height="12" UpdateImageUrl="../../Images/saveicon.jpg" CancelImageUrl="../../Images/cancelicon.png" /> 
                                    <asp:TemplateField HeaderText ="Ticker">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TickerTextBox" runat="server" Text='<%# Bind("Ticker") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="TickerLabel" runat="server" Text='<%# Bind("Ticker") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="AuthorTag" HeaderText="Tagged By" SortExpression="AuthorTag"  ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N0}" ReadOnly="true" />               
                                    <asp:BoundField DataField="UpdateDate" HeaderText="Updated" SortExpression="UpdateDate" ItemStyle-HorizontalAlign="Left" readonly="true" />
                                    <asp:BoundField DataField="Filepath" HeaderText="Filepath" SortExpression="Filepath" ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N0}" />
                                    <asp:BoundField DataField="Headline2" HeaderText="Headline2" SortExpression="Headline2" ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N0}" ReadOnly="true" />
                                    <asp:BoundField DataField="Ticker2" HeaderText="Ticker2" SortExpression="Ticker2" ItemStyle-HorizontalAlign="Left" DataFormatString="{0:N0}" ReadOnly="true"/>     
                                 </Columns>
                                 <EmptyDataTemplate>No news stories.</EmptyDataTemplate>
                                 <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
                                 <HeaderStyle BackColor="#dbdbdb" Font-Bold="True" ForeColor="Black" />
                                 <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                 <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
                                 <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                 <SortedDescendingCellStyle BackColor="#CAC9C9" />
                              </asp:GridView>
                               <!--<a id="MoreStoriesLink" runat="server" href="Default.aspx?ExpandedNews=Yes" style="font-size:10px;">More stories...</a>-->
                               <asp:Button ID="MoreStoriesButton" runat="server" Text="More Stories..." PostBackUrl="Default.aspx?ExpandedNews=Yes" />
                           </fieldset>
                        </td>
                     </tr>
                  </table>
               </div>
            </ContentTemplate>
         </asp:UpdatePanel>
             <a id="bottom"></a>
      </form>
      <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings: SqlDataSource1 %>" 
        SelectCommand="SET DEADLOCK_PRIORITY @DeadlockPriority; EXEC [dbo].[proc_defaultaspx_sqldatasource1_fix] @source = 'all', @date_far = 5"
        OnSelecting="sqlds1_OnSelecting"
        UpdateCommand="IF @Source='Axios'
    UPDATE News_Stories SET [Ticker]=@Ticker, AuthorTag=@CurrentUser, TagEditTimestamp=DATEADD(hh,-8,GETUTCDATE()) WHERE [Headline]=@Name;
    ELSE
          BEGIN
    UPDATE News_Stories SET [Ticker]=@Ticker, AuthorTag=@CurrentUser, TagEditTimestamp=DATEADD(hh,-8,GETUTCDATE()) WHERE [Headline]=@Name AND convert(date,[Timestamp])=convert(date,@UpdateDate)
    DELETE DUP
FROM
(
 SELECT ROW_NUMBER() OVER (ORDER BY Headline ) AS Val
 FROM news_stories WHERE [Headline]=@Name AND convert(date,[Timestamp])=convert(date,@UpdateDate)
) DUP
WHERE DUP.Val > 1;
    UPDATE EvernoteAPIDatabase SET tags=@Ticker, Author=@CurrentUser WHERE NoteGuid=SUBSTRING(@Name,35,36)
    UPDATE tmpresearchhome SET [Ticker]=@Ticker, Author=@CurrentUser WHERE [Name]=@Name AND convert(date,[UpdateDate])=convert(date,@UpdateDate)
          END "
        DeleteCommand="update news_stories set [ticker]='Deleted' where [Headline] = @Name and [Timestamp] >= dateadd(dd, -5, getdate()); UPDATE EvernoteAPIDatabase SET tags='Deleted', Author=@CurrentUser WHERE NoteGuid=SUBSTRING(@Name,35,36); UPDATE tmpresearchhome SET [Ticker]='Deleted', Author=@CurrentUser WHERE [Name]=@Name AND convert(date,[UpdateDate])=convert(date,@UpdateDate)"
        FilterExpression="IIF('{0}'='Asia/Other',Region='China' OR Region='Japan' OR Region='Other',IIF('{0}'='China (ex-Taiwan)',Region='China' AND BloombergTicker NOT LIKE '%TT%',(Region='{0}' OR '{0}'='-1'))) AND (Theme='{1}' OR '{1}'='-1')">
        <SelectParameters>
            <asp:Parameter Name="DeadlockPriority" Type="String"/>
        </SelectParameters>
         <FilterParameters>
            <asp:ControlParameter Name="Region" ControlId="ddlRegion" PropertyName="SelectedValue"/>
            <asp:ControlParameter Name="Sector" ControlId="ddlSector" PropertyName="SelectedValue"/>
         </FilterParameters>
         <UpdateParameters>
            <asp:Parameter Name="Headline" Type="String" />
            <asp:Parameter Name="Ticker" Type="String" />
             <asp:Parameter Name="Source" Type="String" />
            <asp:Parameter Name="UpdateDate" Type="String" />
             <asp:Parameter Name="CurrentUser" Type="String" />
         </UpdateParameters>
        <DeleteParameters>
            <asp:Parameter Name="Headline" Type="String" />
            <asp:Parameter Name="Ticker" Type="String" />
             <asp:Parameter Name="Source" Type="String" />
            <asp:Parameter Name="UpdateDate" Type="String" />
             <asp:Parameter Name="CurrentUser" Type="String" />
         </DeleteParameters>
      </asp:SqlDataSource>
      <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings: SqlDataSource1 %>" SelectCommand="
  IF DATEPART(hour,DATEADD(hh,-7,GETUTCDATE()))<6 OR (DATEPART(hour,DATEADD(hh,-7,GETUTCDATE()))=6 AND DATEPART(minute,DATEADD(hh,-8,GETUTCDATE()))<30)
         SELECT DISTINCT * FROM
         (SELECT * FROM
         (SELECT IIF(TableB.BloombergID IS NULL, TableC.BloombergID, TableB.BloombergID) AS BloombergID, TableB.Region1, TableB.Sector1, TableB.CallTime, IIF(TableB.[Report Date] IS NULL, TableC.[Report Date], TableB.[Report Date]) AS [Report Date], IIF(TableB.[Report Time] IS NULL, TableC.[Report Time], TableB.[Report Time]) AS [Report Time], IIF(TableB.Afterhoursmove IS NULL, TableC.Afterhoursmove, TableB.Afterhoursmove) AS Afterhoursmove, ISNULL(TableB.DV,0) AS DV, ISNULL(TableC.DV,0) AS LODV FROM
         (SELECT TableA.BloombergID, Region1, Sector1, TableA.[Report Date], TableA.[Report Time], TableA.[Afterhoursmove], TableA.DV, CallTime FROM (select BloombergID, [Report Time], Region as Region1, Sector AS Sector1, [Report Date], [Afterhoursmove], DV, CallTime from lspmsummaryrank WHERE ([Report Date]>=DATEADD(day, DATEDIFF(day, 0, DATEADD(hh,-7,GETUTCDATE())), 0) AND [Report Date]<=DATEADD(day,10,DATEADD(hh,-7,GETUTCDATE())))) TableA
		 UNION 
		 SELECT TableA.BloombergID, Region1, Sector1, convert(date,DATEADD(dd,-1,DATEADD(hh,-8,GETUTCDATE()))) AS [Report Date], '13:00:00' AS [Report Time], TableA.[Afterhoursmove], TableA.DV, CallTime FROM (select BloombergID, Region as Region1, Sector AS Sector1, [Report Time], [Report Date], [Afterhoursmove], DV, CallTime from lspmsummaryrank WHERE bloombergid in (select ticker from afterhoursportfoliocompanies)) TableA) TableB
         FULL OUTER JOIN
         (SELECT TableA.BloombergID, Region1, Sector1, TableA.[Report Date], TableA.[Report Time], TableA.[Afterhoursmove], TableA.DV, CallTime FROM (select BloombergID, Region as Region1, Sector AS Sector1, [Report Time], [Report Date], [Afterhoursmove], DV, '' AS CallTime from lolspmsummaryrank WHERE [Report Date]>=DATEADD(day, DATEDIFF(day, 0, DATEADD(hh,-7,GETUTCDATE())), 0) AND [Report Date]<=DATEADD(day,7,DATEADD(hh,-7,GETUTCDATE()))) TableA) TableC
         ON TableB.BloombergID=TableC.BloombergID) a
         LEFT JOIN
         (SELECT * FROM MS_ID) b
         ON REPLACE(b.BloombergTicker,' US','')=REPLACE(REPLACE(a.BloombergID,' EQUITY',''),' US','')) aRicky
         LEFT JOIN
         (select Ticker, PartnerReview, ModelQuarterlyUpdate, [Update Date] from (select *, ROW_NUMBER() OVER(PARTITION BY Ticker ORDER BY [Update Date] DESC) rn from lspmscenariomanagement) a WHERE rn=1 AND Ticker IN (SELECT [Bloomberg ID] FROM PortfolioTest UNION SELECT [Bloomberg ID] FROM LOPortfolioTest)) bRicky
         on REPLACE(REPLACE(aRicky.BloombergID,' EQUITY',''),' US','')=bRicky.Ticker WHERE replace(replace(BloombergID,' EQUITY',''),' US','') not in (select ticker from spaclist) ORDER BY [Report Date] ASC, [Report Time] ASC, [MSLongDescription] ASC
         ELSE
         SELECT DISTINCT * FROM
         (SELECT * FROM
         (SELECT IIF(TableB.BloombergID IS NULL, TableC.BloombergID, TableB.BloombergID) AS BloombergID, TableB.Region1, TableB.CallTime, TableB.Sector1, IIF(TableB.[Report Date] IS NULL, TableC.[Report Date], TableB.[Report Date]) AS [Report Date], IIF(TableB.[Report Time] IS NULL, TableC.[Report Time], TableB.[Report Time]) AS [Report Time], IIF(TableB.Afterhoursmove IS NULL, TableC.Afterhoursmove, TableB.Afterhoursmove) AS Afterhoursmove, ISNULL(TableB.DV,0) AS DV, ISNULL(TableC.DV,0) AS LODV FROM
         (SELECT TableA.BloombergID, Region1, Sector1, TableA.[Report Date], TableA.[Report Time], TableA.[Afterhoursmove], TableA.DV, CallTime FROM (select BloombergID, Region as Region1, Sector AS Sector1, [Report Time], [Report Date], [Afterhoursmove], DV, CallTime from lspmsummaryrank WHERE [Report Date]>=DATEADD(day, DATEDIFF(day, 0, DATEADD(hh,-7,GETUTCDATE())), 0) AND [Report Date]<=DATEADD(day,10,DATEADD(hh,-7,GETUTCDATE()))) TableA) TableB
         FULL OUTER JOIN
         (SELECT TableA.BloombergID, Region1, Sector1, TableA.[Report Date], TableA.[Report Time], TableA.[Afterhoursmove], TableA.DV, CallTime FROM (select BloombergID, Region as Region1, Sector AS Sector1, [Report Time], [Report Date], [Afterhoursmove], DV, '' AS CallTime from lolspmsummaryrank WHERE [Report Date]>=DATEADD(day, DATEDIFF(day, 0, DATEADD(hh,-7,GETUTCDATE())), 0) AND [Report Date]<=DATEADD(day,7,DATEADD(hh,-7,GETUTCDATE()))) TableA) TableC
         ON TableB.BloombergID=TableC.BloombergID) a
         LEFT JOIN
         (SELECT * FROM MS_ID) b
         ON REPLACE(b.BloombergTicker,' US','')=REPLACE(REPLACE(a.BloombergID,' EQUITY',''),' US','')) aRicky
         LEFT JOIN
         (select Ticker, PartnerReview, ModelQuarterlyUpdate, [Update Date] from (select *, ROW_NUMBER() OVER(PARTITION BY Ticker ORDER BY [Update Date] DESC) rn from lspmscenariomanagement) a WHERE rn=1 AND Ticker IN (SELECT [Bloomberg ID] FROM PortfolioTest UNION SELECT [Bloomberg ID] FROM LOPortfolioTest)) bRicky
         on REPLACE(REPLACE(aRicky.BloombergID,' EQUITY',''),' US','')=bRicky.Ticker WHERE replace(replace(BloombergID,' EQUITY',''),' US','') not in (select ticker from spaclist) AND ([Report Date]>CONVERT(date,DATEADD(hh,-8,GETUTCDATE())) OR ([Report Time]='00:00:00.0000000' OR DATEPART(HOUR, [Report Time])>6 and [Report Date]=CONVERT(date,DATEADD(hh,-8,GETUTCDATE())))) ORDER BY [Report Date] ASC, [Report Time] ASC, [MSLongDescription] ASC"  FilterExpression="IIF('{0}'='Asia/Other',Region1='China' OR Region1='Japan' OR Region1='Other',IIF('{0}'='China (ex-Taiwan)',Region1='China' AND BloombergID NOT LIKE '%TT EQUITY',(Region1='{0}' OR '{0}'='-1'))) AND (Sector1='{1}' OR '{1}'='-1')">
         <FilterParameters>
            <asp:ControlParameter Name="Region" ControlId="ddlRegion" PropertyName="SelectedValue"/>
            <asp:ControlParameter Name="Sector" ControlId="ddlSector" PropertyName="SelectedValue"/>
         </FilterParameters>
      </asp:SqlDataSource>
      <asp:SqlDataSource ID="SqlDataSource3" runat="server" ConnectionString="<%$ ConnectionStrings: SqlDataSource1 %>" SelectCommand="
DECLARE @LastEarningsCount FLOAT = (
		SELECT DISTINCT count(*)
		FROM (
			SELECT *
			FROM (
				SELECT *
				FROM (
					SELECT TableA.BloombergID
						,Region1
						,Sector1
						,TableA.[Last Earnings]
						,TRY_CAST(TableA.EarningsTime AS TIME) AS EarningsTime
						,100 * ((1 + CAST(TableA.[ReturnFromLastEarnings] AS FLOAT) / 100) * (1 + CAST(TableA.[Afterhoursmove] AS FLOAT) / 100) - 1) AS [ReturnFromLastEarnings]
						,TableA.[Long/Short]
					FROM (
						SELECT BloombergID
							,Region AS Region1
							,Sector AS Sector1
							,[Last Earnings]
							,[ReturnFromLastEarnings]
							,[Long/Short]
							,[Afterhoursmove]
							,EarningsTime
						FROM lspmsummaryrank
						WHERE DATEADD(day, 0, [Last Earnings]) < DATEADD(hh, - 7, GETUTCDATE())
							AND [Last Earnings] >= DATEADD(day, - 10, DATEADD(hh, - 7, GETUTCDATE()))
						
						UNION
						
						SELECT BloombergID
							,Region AS Region1
							,Sector AS Sector1
							,[Last Earnings]
							,[ReturnFromLastEarnings]
							,[Long/Short]
							,[Afterhoursmove]
							,EarningsTime
						FROM lolspmsummaryrank
						WHERE BloombergID NOT IN (
								SELECT BloombergID
								FROM lspmsummaryrank
								)
							AND DATEADD(day, 0, [Last Earnings]) < DATEADD(hh, - 7, GETUTCDATE())
							AND [Last Earnings] >= DATEADD(day, - 10, DATEADD(hh, - 7, GETUTCDATE()))
						) TableA
					LEFT JOIN (
						SELECT MSLongDescription
							,BloombergTicker
						FROM MS_ID
						) TableB ON (
							TableA.BloombergID = TableB.BloombergTicker + ' EQUITY'
							OR TableA.BloombergID = REPLACE(TableB.BloombergTicker, ' US', '') + ' EQUITY'
							)
					) a
				LEFT JOIN (
					SELECT *
					FROM MS_ID
					) b ON REPLACE(b.BloombergTicker, ' US', '') = REPLACE(REPLACE(a.BloombergID, ' EQUITY', ''), ' US', '')
				) aRicky
			LEFT JOIN (
				SELECT Ticker
					,PartnerReview
					,ModelQuarterlyUpdate
					,[Update Date]
				FROM (
					SELECT *
						,ROW_NUMBER() OVER (
							PARTITION BY Ticker ORDER BY [Update Date] DESC
							) rn
					FROM lspmscenariomanagement
					) a
				WHERE rn = 1
					AND Ticker IN (
						SELECT [Bloomberg ID]
						FROM PortfolioTest
						
						UNION
						
						SELECT [Bloomberg ID]
						FROM LOPortfolioTest
						)
				) bRicky ON REPLACE(REPLACE(aRicky.BloombergID, ' EQUITY', ''), ' US', '') = bRicky.Ticker
			) a
		LEFT JOIN (
			SELECT *
			FROM oldpositions
			WHERE DATE > DATEADD(dd, - 15, GETUTCDATE())
			) b ON IIF(DATEPART(hh, EarningsTime) < 9, DATEADD(dd, - 1, a.[Last Earnings]), a.[Last Earnings]) = b.[Date]
			AND REPLACE(REPLACE(a.BloombergID, ' EQUITY', ''), ' US', '') = b.Ticker
		)

IF @LastEarningsCount > 0
BEGIN
	SELECT DISTINCT a.*
		,b.Ticker
	FROM (
		SELECT *
		FROM (
			SELECT *
			FROM (
				SELECT TableA.BloombergID
					,Region1
					,Sector1
					,TableA.[Last Earnings]
					,TRY_CAST(TableA.EarningsTime AS TIME) AS EarningsTime
					,100 * ((1 + CAST(TableA.[ReturnFromLastEarnings] AS FLOAT) / 100) * (1 + CAST(TableA.[Afterhoursmove] AS FLOAT) / 100) - 1) AS [ReturnFromLastEarnings]
					,TableA.[Long/Short]
				FROM (
					SELECT BloombergID
						,Region AS Region1
						,Sector AS Sector1
						,[Last Earnings]
						,[ReturnFromLastEarnings]
						,[Long/Short]
						,[Afterhoursmove]
						,EarningsTime
					FROM lspmsummaryrank
					WHERE DATEADD(day, 0, [Last Earnings]) < DATEADD(hh, - 7, GETUTCDATE())
						AND [Last Earnings] >= DATEADD(day, - 10, DATEADD(hh, - 7, GETUTCDATE()))
					
					UNION
					
					SELECT BloombergID
						,Region AS Region1
						,Sector AS Sector1
						,[Last Earnings]
						,[ReturnFromLastEarnings]
						,[Long/Short]
						,[Afterhoursmove]
						,EarningsTime
					FROM lolspmsummaryrank
					WHERE BloombergID NOT IN (
							SELECT BloombergID
							FROM lspmsummaryrank
							)
						AND DATEADD(day, 0, [Last Earnings]) < DATEADD(hh, - 7, GETUTCDATE())
						AND [Last Earnings] >= DATEADD(day, - 10, DATEADD(hh, - 7, GETUTCDATE()))
					) TableA
				LEFT JOIN (
					SELECT MSLongDescription
						,BloombergTicker
					FROM MS_ID
					) TableB ON (
						TableA.BloombergID = TableB.BloombergTicker + ' EQUITY'
						OR TableA.BloombergID = REPLACE(TableB.BloombergTicker, ' US', '') + ' EQUITY'
						)
				) a
			LEFT JOIN (
				SELECT *
				FROM MS_ID
				) b ON REPLACE(b.BloombergTicker, ' US', '') = REPLACE(REPLACE(a.BloombergID, ' EQUITY', ''), ' US', '')
			) aRicky
		LEFT JOIN (
			SELECT Ticker
				,PartnerReview
				,ModelQuarterlyUpdate
				,[Update Date]
			FROM (
				SELECT *
					,ROW_NUMBER() OVER (
						PARTITION BY Ticker ORDER BY [Update Date] DESC
						) rn
				FROM lspmscenariomanagement
				) a
			WHERE rn = 1
				AND Ticker IN (
					SELECT [Bloomberg ID]
					FROM PortfolioTest
					
					UNION
					
					SELECT [Bloomberg ID]
					FROM LOPortfolioTest
					)
			) bRicky ON REPLACE(REPLACE(aRicky.BloombergID, ' EQUITY', ''), ' US', '') = bRicky.Ticker
			WHERE REPLACE(REPLACE(aRicky.BloombergID, ' EQUITY', ''), ' US', '') NOT IN (select ticker from spaclist)
		) a
	LEFT JOIN (
		SELECT *
		FROM oldpositions
		WHERE DATE > DATEADD(dd, - 15, GETUTCDATE())
		) b ON IIF(DATEPART(hh, EarningsTime) < 9, DATEADD(dd, - 1, a.[Last Earnings]), a.[Last Earnings]) = b.[Date]
		AND REPLACE(REPLACE(a.BloombergID, ' EQUITY', ''), ' US', '') = b.Ticker
	-- WHERE b.Ticker IS NOT NULL OR convert(date,[last earnings])=convert(date,dateadd(hh,-8,getutcdate()))
	WHERE MSLongDescription NOT LIKE '%Warrant%'
	ORDER BY [Last Earnings] DESC
END
ELSE
BEGIN
	SELECT '' AS bloombergid
		,'' AS region1
		,'' AS sector1
		,'No earnings in the portfolio in last ten days' AS [last earnings]
		,'' AS earningstime
		,'' AS returnfromlastearnings
		,'' AS [long/short]
		,'' AS mslongdescription
		,'' AS bloombergticker
		,'' AS region
		,'' AS theme
		,'' AS lsregion
		,'' AS chinanonchina
		,'' AS industry
		,'' AS country
		,'' AS glenname
		,'' AS marketcap
		,'' AS irwebsite
		,'' AS eventrss_url
		,'' AS slack_channel
		,'' AS iremail
		,'' AS irsubscribed
		,'' AS lastiremailreceived
		,'' AS ticker
		,'' AS partnerreview
		,'' AS modelquarterlyupdate
		,'' AS [update date]
		,'' AS ticker
END          "  
             FilterExpression="IIF('{0}'='Asia/Other',Region1='China' OR Region1='Japan' OR Region1='Other',IIF('{0}'='China (ex-Taiwan)',Region1='China' AND BloombergID NOT LIKE '%TT EQUITY',(Region1='{0}' OR '{0}'='-1'))) AND (Sector1='{1}' OR '{1}'='-1')">
         <FilterParameters>
            <asp:ControlParameter Name="Region" ControlId="ddlRegion" PropertyName="SelectedValue"/>
            <asp:ControlParameter Name="Sector" ControlId="ddlSector" PropertyName="SelectedValue"/>
         </FilterParameters>
      </asp:SqlDataSource>
      <asp:SqlDataSource ID="SqlDataSource4" runat="server" ConnectionString="<%$ ConnectionStrings: SqlDataSource1 %>" SelectCommand="
          
DECLARE @NAV FLOAT;
SET @NAV=(SELECT MV FROM LSPMSummaryTable WHERE Category_Name='Net Total');
select * from (
select *, ROW_NUMBER() OVER (PARTITION BY BloombergID, Lockupexpiry order by lockupexpiry) as rn22 from (
select b.*, a.Region AS Region1 from (
select * from ezesecuritiesdefinitions) a
RIGHT JOIN (
select MsLongDescription, 
Sector1, 
LockupExpiry, BloombergID, Theme, Region, LongTicker, OFFERING_LOCKUP_DATE, IIF(ISNUMERIC(OFFERING_LOCKUP_SHARES)=1,IIF(CAST(OFFERING_LOCKUP_SHARES AS FLOAT)<1000000,CAST(OFFERING_LOCKUP_SHARES AS FLOAT)*1000000,OFFERING_LOCKUP_SHARES),0) AS OFFERING_LOCKUP_SHARES, SHARESOUT, EQY_FLOAT, in_portfolio, rn, 100*DV/@NAV AS DV from
(select *, ROW_NUMBER() OVER (PARTITION BY BloombergID ORDER BY in_portfolio DESC) AS rn from
(select distinct *, '1' AS in_portfolio from
(
SELECT a.MSLongDescription, LockupExpiry, BloombergID, b.Theme, b.Region, DV, Region1, Sector1 FROM
        (SELECT TableA.BloombergID, Region1, Sector1, TableB.GlenName AS MSLongDescription, TableA.[LockupExpiry], TableA.DV FROM (select BloombergID, Region AS Region1, Sector AS Sector1, [LockupExpiry], DV from lspmsummaryrank WHERE LockupExpiry!='#N/A N/A' and LockupExpiry!='' UNION select LongTicker AS BloombergID, '' AS Region1, '' AS Sector1, OFFERING_LOCKUP_DATE AS [LockupExpiry], '0' AS DV from [em template] WHERE LongTicker not in (select bloombergid from lspmsummaryrank) and OFFERING_LOCKUP_DATE!='#N/A N/A') TableA LEFT JOIN (select MSLongDescription, BloombergTicker, GlenName FROM MS_ID where replace(Bloombergticker,' US','') in (select [Bloomberg ID] from portfoliotest)) TableB ON (TableA.BloombergID=TableB.BloombergTicker+' EQUITY' OR TableA.BloombergID=REPLACE(TableB.BloombergTicker,' US','')+' EQUITY')) a
        LEFT JOIN
        (SELECT * FROM MS_ID where replace(Bloombergticker,' US','') in (select [Bloomberg ID] from portfoliotest)) b
        ON b.BloombergTicker=REPLACE(a.BloombergID,' EQUITY','')
         ) a
         left JOIN
         (select LongTicker, OFFERING_LOCKUP_DATE, OFFERING_LOCKUP_SHARES, SHARESOUT, EQY_FLOAT from [em template] where replace(ticker,' US','') in (select [Bloomberg ID] from portfoliotest)) b
         ON a.BloombergID=b.LongTicker
		 UNION
         
		 select SHORTNAME AS MSLongDescription, '' AS Region1, '' AS Sector1, OFFERING_LOCKUP_DATE AS LockupExpiry, LongTicker AS BloombergID, '' AS Theme, '' AS Region, '0' AS DV, LongTicker, OFFERING_LOCKUP_DATE, OFFERING_LOCKUP_SHARES, SHARESOUT, EQY_FLOAT,'0' AS in_portfolio FROM [em template] where SHORTNAME!='SHORTNAME' AND OFFERING_LOCKUP_DATE!='#N/A N/A') bbTable where mslongdescription!='') c 
		 WHERE rn=1 and ISDATE(OFFERING_LOCKUP_DATE)=1) b
		 ON a.BloombergTicker=REPLACE(REPLACE(b.BloombergID, ' EQUITY',''),' US','')
		 union
		
select a.GlenName, a.Theme, a.Date, a.Ticker, a.Theme, a.Region, a.Ticker, a.Date, a.Shares, a.SHARESOUT, a.EQY_FLOAT, a.in_portfolio, a.rn, b.dv*100 as dv, b.region from (
		 select a.GlenName, Theme, Date, Ticker, Theme as theme2, Region, Ticker AS Ticker2, Date AS Date2, Shares, SHARESOUT, EQY_FLOAT, '1' as in_portfolio, '1' as rn, '0' as dv, region as region2 from (
select a.*, b.SHARESOUT, b.EQY_FLOAT from (
select * from lockupexpirypast) a
LEFT JOIN (
select * from [em template] where longticker in (select ticker from lockupexpirypast)) b
ON a.ticker=b.Longticker) a
LEFT JOIN (
select * from ms_Id where bloombergticker in (select replace(ticker,' EQUITY','') from lockupexpirypast)) b
ON replace(a.ticker,' EQUITY','')=b.bloombergticker) a
LEFT JOIN (
select [Underlying Security], region, [Delta Adjusted Exposure]/@NAV as dv from portfoliotest) b
on a.ticker=b.[Underlying Security] where a.Date>DATEADD(dd,-21,GETUTCDATE())
		 ) a) az
		 where rn22=1
		 ORDER BY TRY_CAST(LockupExpiry AS DATE) ASC
		 
 "  FilterExpression="IIF('{0}'='Asia/Other',Region1='China' OR Region1='Japan' OR Region1='Other',IIF('{0}'='China (ex-Taiwan)',Region1='China' AND BloombergID NOT LIKE '%TT EQUITY',(Region1='{0}' OR '{0}'='-1')))  AND (Theme='{1}' OR '{1}'='-1')">
         <FilterParameters>
            <asp:ControlParameter Name="Region" ControlId="ddlRegion" PropertyName="SelectedValue"/>
            <asp:ControlParameter Name="Sector" ControlId="ddlSector" PropertyName="SelectedValue"/>
         </FilterParameters>
      </asp:SqlDataSource>
         <asp:SqlDataSource ID="SqlDataSource5" runat="server" ConnectionString="<%$ ConnectionStrings: SqlDataSource1 %>" SelectCommand="
SELECT glenname
	,NEXTEARNINGS AS [Date]
	,EarningsReportTime AS [Pacific Time]
	,EarningsCallTime AS [Call Link]
	,'0.0%' AS [%Equity]
	,b.BloombergTicker
	,IRWebsite
	,region AS Region1
	,theme
	, CASE 
		WHEN lower(EarningsReportTime) = 'bef-mkt' THEN '06:00'
		WHEN lower(EarningsReportTime) = 'aft-mkt' THEN '13:30'
		ELSE EarningsReportTime
	  END AS 'time_adj'
FROM (
	SELECT *
	FROM [em template]
	WHERE replace(ticker, ' US', '') NOT IN (
			SELECT [Bloomberg ID]
			FROM portfoliotest
			)
		AND (
			try_convert(DATE, NEXTEARNINGS) = try_convert(DATE, dateadd(hh, - 8, getutcdate()))
			OR try_convert(DATE, NEXTEARNINGS) = try_convert(DATE, dateadd(dd, 1, dateadd(hh, - 8, getutcdate())))
			)
	) a
LEFT JOIN (
	SELECT *
	FROM ms_id
	) b ON a.ticker = b.BloombergTicker
WHERE glenname IS NOT NULL 
AND b.IsPrivate <> 1 AND (len(EarningsReportTime) > 1 OR len(EarningsCallTime) > 1) AND (replace(bloombergticker,' US','') not in (select ticker from spaclist)) ORDER BY date asc, time_adj asc, EarningsCallTime asc"  
             
         FilterExpression="IIF('{0}'='Asia/Other',Region1='China' OR Region1='Japan' OR Region1='Other',IIF('{0}'='China (ex-Taiwan)',Region1='China' AND BloombergTicker NOT LIKE '%TT EQUITY',(Region1='{0}' OR '{0}'='-1')))  AND (Theme='{1}' OR '{1}'='-1')">
         <FilterParameters>
            <asp:ControlParameter Name="Region" ControlId="ddlRegion" PropertyName="SelectedValue"/>
            <asp:ControlParameter Name="Sector" ControlId="ddlSector" PropertyName="SelectedValue"/>
         </FilterParameters>
      </asp:SqlDataSource>
      <script>
         $("#project").keypress(function (e) {
             if (e.which == 13) {
                 window.location.href = "Analytics/PositionNews.aspx?Ticker=" + document.getElementById("project").value;
                 return false;
             }
         });
      </script>
      <script>
         function w3_open() {
             document.getElementById("mySidenav").style.display = "block";
             
         }
         function w3_close() {
             document.getElementById("mySidenav").style.display = "none";
             
         }
         function myAccFunc() {
             var x = document.getElementById("demoAcc");
             if (x.className.indexOf("w3-show") == -1) {
                 x.className += " w3-show";
                 x.previousElementSibling.className += " w3-black";
             } else {
                 x.className = x.className.replace(" w3-show", "");
                 x.previousElementSibling.className =
                 x.previousElementSibling.className.replace(" w3-black", "");
             }
         }
         function myAccFunc2() {
             var x = document.getElementById("demoAcc2");
             if (x.className.indexOf("w3-show") == -1) {
                 x.className += " w3-show";
                 x.previousElementSibling.className += " w3-black";
             } else {
                 x.className = x.className.replace(" w3-show", "");
                 x.previousElementSibling.className =
                 x.previousElementSibling.className.replace(" w3-black", "");
             }
         }
      </script>
      <script>
          $(function () {
              var projects = JSON.parse(document.getElementById("hdnfldTickerList").value);

              function lightwell(request, response) {
                  function hasMatch(s) {
                      return s.toLowerCase().indexOf(request.term.toLowerCase()) !== -1;
                  }
                  var i, l, obj, matches = [];

                  if (request.term === "") {
                      response([]);
                      return;
                  }

                  for (i = 0, l = projects.length; i < l; i++) {
                      obj = projects[i];
                      if (hasMatch(obj.MSLongDescription) || hasMatch(obj.BloombergTicker)) {
                          matches.push(obj);
                      }
                  }
                  response(matches);
              }

              $("#project").autocomplete({
                  minLength: 1,
                  source: lightwell,
                  focus: function (event, ui) {
                      $("#project").val(ui.item.GlenName);
                      return false;
                  },
                  select: function (event, ui) {
                      $("#project").val(ui.item.GlenName);
                      $("#project-id").val(ui.item.BloombergTicker);
                      location.href = "Analytics/PositionNews.aspx?Ticker=" + ui.item.BloombergTicker

                      return false;
                  }
              })

                  .data("ui-autocomplete")._renderItem = function (ul, item) {
                      return $("<li>")
                          .append("<a style='margin-bottom:0;margin-top:0;padding-bottom:0;padding-top:0;display: block;'>\xa0" + item.GlenName +
                              "<br><span style='font-size: 75%;margin-bottom:0;margin-top:0;padding-bottom:0;padding-top:0;display: block;'>\xa0Ticker: " + item.BloombergTicker + "</span></a>" +
                              "<div style='border-bottom:1px solid Gainsboro;margin-bottom:0;padding-bottom:0;padding-top:0;display: block;margin-top:0'></div>")
                          .appendTo(ul);
                  };
          });

      </script>
   </body>
</html>