using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Web.Script.Services;
using Newtonsoft.Json;
using System.Web.UI.HtmlControls;
using EvernoteSDK;
using EvernoteSDK.Advanced;
using Evernote.EDAM.Type;
using Evernote.EDAM.NoteStore;
using System.Globalization;
using System.IO;

public partial class _Default : System.Web.UI.Page
{
    public DateTime tmpDate;
    public string LSPerformance;
    public string LSLOPerformance;
    public string CCMPPerformance;
    public string NKYPerformance;
    public string HSIPerformance;
    public string SHCOMPPerformance;
    public string SPXPerformance;
    public string IGVPerformance;
    public string XRTPerformance;
    public string SOCLPerformance;
    public string KWEBPerformance;
    public string NQAPerformance;
    public string MXWOPerformance;

    public int ToDoCount;
    public string CurrentUser;
    public string ToDoWhereStatement;
    public string SqlDeadlockPriority = "NORMAL";

    public double CurrentNAV;
    public double CurrentLONAV;

    public double WeeklyWins = 0;
    public double QuarterWins;
    public double OverallWins;

    public double WeeklyWins_Long = 0;
    public double QuarterWins_Long;
    public double OverallWins_Long;

    public double WeeklyWins_Short = 0;
    public double QuarterWins_Short;
    public double OverallWins_Short;

    public double WeeklyLosses = 0;
    public double QuarterLosses;
    public double OverallLosses;

    public double WeeklyLosses_Long = 0;
    public double QuarterLosses_Long;
    public double OverallLosses_Long;

    public double WeeklyLosses_Short = 0;
    public double QuarterLosses_Short;
    public double OverallLosses_Short;

    public double WeeklyPct = 0;
    public double QuarterPct;
    public double OverallPct;

    public double WeeklyPct_Long = 0;
    public double QuarterPct_Long;
    public double OverallPct_Long;

    public double WeeklyPct_Short = 0;
    public double QuarterPct_Short;
    public double OverallPct_Short;

    public string CombAttString;
    public int UpdateEvernote = 0;
    public string UpdateSQLStatement = "";
    public List<string> _TagList = new List<string>();
    public string TagListString;

    public double SeasonAvgWin;
    public double SeasonAvgLoss;
    public double AllTimeAvgLoss;
    public double AllTimeAvgWin;

    public double SeasonAvgWin_Long;
    public double SeasonAvgLoss_Long;
    public double AllTimeAvgLoss_Long;
    public double AllTimeAvgWin_Long;

    public double SeasonAvgWin_Short;
    public double SeasonAvgLoss_Short;
    public double AllTimeAvgLoss_Short;
    public double AllTimeAvgWin_Short;

    public int HMBWin;
    public int HMBLoss;
    public double HMBWinAvg;
    public double HMBLossAvg;
    public double HMBWinPct;
    DataSet ds210 = new DataSet();
    DataTable dt210 = new DataTable();

    DataSet ds200 = new DataSet();
    DataTable dt200 = new DataTable();
    DataSet ds2001 = new DataSet();
    DataTable dt2001 = new DataTable();

    public string page_title = "Portfolio News";
    public string left_menu_id_selector = "lm_port_news";
    public string top_menu_id_selector = "pfol_news";

    // logging
    public readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public LSUtils lu = new LSUtils();
    public List<string> portfolio_tickers = new List<string>();
    public List<string> news_source_databound_skip = new List<string> {"Electrek", "Evernote", "Tegus", "Visible Alpha"};

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.Browser.IsMobileDevice)
        {
            Response.Redirect("DefaultMobile.aspx");
        }
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/webpages/logs/default.config")));
        log.Warn(System.Reflection.MethodBase.GetCurrentMethod().Name + ": Start");
        long startTime = System.DateTime.Now.Ticks;
        portfolio_tickers = lu.getDistinctPositions();

        // change several user settings such as Timer1.Interval to see how that affects deadlock issues, SqlDeadlockPriority
        CurrentUser = System.Web.HttpContext.Current.User.Identity.Name.Replace("@lightstreet.com", "");
        if (CurrentUser == "gonzalo")
        {
            log.Warn("User is gonzalo but setting to glen");
            //CurrentUser = "glen";
        }    
        if (CurrentUser == "glen")
        {
            ToDoWhereStatement = "Glen Kacher";
            //SqlDeadlockPriority = "SET DEADLOCK_PRIORITY HIGH;";
            //Timer1.Interval = 10000;    // 10 seconds
        }
        else if (CurrentUser == "richard" || CurrentUser == "gonzalo")
        {
            ToDoWhereStatement = "";
            //SqlDeadlockPriority = "SET DEADLOCK_PRIORITY LOW;";
            //Timer1.Interval = 300000;  // five mutes
        }
        else if (CurrentUser == "jay" || CurrentUser == "gaurav")
        {
            //Timer1.Interval = 120000;    // all others 2 minutes
            ToDoWhereStatement = "Jay Kahn' OR Analyst='Gaurav Gupta";
        }
        else if (CurrentUser == "sbluestein" || CurrentUser == "bryan")
        {
            ToDoWhereStatement = "Stephen Bluestein' OR Analyst='Bryan Hsu";
            //Timer1.Interval = 120000;    // all others 2 minutes
        }
        else if (CurrentUser == "gordon" || CurrentUser == "kan")
        {
            ToDoWhereStatement = "Gordon Green' OR Analyst='Kan Yuan";
            //Timer1.Interval = 120000;    // all others 2 minutes
        }
        else
            SqlDeadlockPriority = "LOW";

        //log.Warn(string.Format("User {0} is running SQL with: {1}", CurrentUser, SqlDeadlockPriority));

        if (!String.IsNullOrEmpty(Request.QueryString["NewsFilter"]))
        {
            if (Request.QueryString["NewsFilter"] == "BWG Strategy" || Request.QueryString["NewsFilter"] == "Third Bridge")
            {
                SqlDataSource1.SelectCommand = "SET DEADLOCK_PRIORITY " + SqlDeadlockPriority + @"; 
    select top 30 
		source, 
		IIF(source in ('Tegus','Third Bridge'), 'Industry Panel', 'Market News') as 'type',
		name as 'name',
		TRIM(TRIM(',' FROM ticker)) as Ticker,
		IIF(author is null, 'none', author) as 'authortag',
		updatedate as 'updatedate', 
		'' as 'filepath',
		name as 'headline2'
		,msidregion as 'region'
		,TRIM(TRIM(',' FROM ticker)) AS bloombergticker
		,msidtheme as 'theme'
		,TRIM(TRIM(',' FROM ticker)) AS ticker2
	from tmpresearchhome
	where lower(source) = lower('" + Request.QueryString["NewsFilter"] + @"')
	and lower(ticker) not in ('deleted', '')
	order by updatedate desc";
                GridView1.DataBind();
            }
            else if (Request.QueryString["NewsFilter"] == "All")
            {
                //ddlRegion.SelectedValue = "-1";
                //ddlSector.SelectedValue = "-1";
                SqlDataSource1.SelectCommand = "SET DEADLOCK_PRIORITY " + SqlDeadlockPriority + @"; EXEC dbo.proc_defaultaspx_sqldatasource1_fix @source = 'all', @date_far = 5";
                GridView1.DataBind();
            }
            else
            {
                SqlDataSource1.SelectCommand = "SET DEADLOCK_PRIORITY " + SqlDeadlockPriority + @"; EXEC dbo.proc_defaultaspx_sqldatasource1_fix @date_far = 20, @source='" + Request.QueryString["NewsFilter"] + "'";
                //(GridView1.DataSource as DataTable).DefaultView.RowFilter = string.Format("Source = ", Request.QueryString["NewsFilter"]);
                GridView1.DataBind();
            }
        }

        if (Request.QueryString["ExpandedNews"] == "Yes")
        {
            MoreStoriesButton.Visible = false;
            SqlDataSource1.SelectCommand = "SET DEADLOCK_PRIORITY " + SqlDeadlockPriority + @"; EXEC dbo.proc_defaultaspx_sqldatasource1_fix @source = 'all', @date_far = 10";
            GridView1.DataBind();
        }

        SqlDataSource1.UpdateParameters["CurrentUser"].DefaultValue = CurrentUser;
        if (CurrentUser == "richard" || CurrentUser == "glen" || CurrentUser == "hayden" || CurrentUser == "gaurav" || CurrentUser == "gordon" || CurrentUser == "sbluestein" || CurrentUser == "bryan")
        {
            SqlConnection connMM = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True");
            connMM.Open();
            SqlCommand SQL_Command = new SqlCommand("select COUNT(*) from UserViewScorecardTodayTable where Username='" + CurrentUser + "'", connMM);

            if (Convert.ToInt32(SQL_Command.ExecuteScalar()) == 0)
            {
                Response.Redirect("Information/LSPMInfoScorecard.aspx?AnalystRedirect=1");
            }
            connMM.Close();
        }

        SqlConnection connTodo = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True");
        connTodo.Open();
        if (CurrentUser == "jay" || CurrentUser == "gaurav" || CurrentUser == "sbluestein" || CurrentUser == "bryan" || CurrentUser == "glen" || CurrentUser == "gordon" || CurrentUser == "kan" || CurrentUser == "kevin" || CurrentUser == "mario")
        {
            SqlCommand ToDoCountSQL = new SqlCommand("select ISNULL(IIF(Process+MemoReview+PrivateCoTracker+ScenarioManagement+EarningsCount>99,99,Process+MemoReview+PrivateCoTracker+ScenarioManagement+EarningsCount),0) from dashboardcounter WHERE Analyst='" + CurrentUser + "'", connTodo);
            try { ToDoCount = Convert.ToInt32(ToDoCountSQL.ExecuteScalar()); } catch { }
        }
        else
        {
            ToDoCount = 0;
        }

        SqlCommand NAVSQL = new SqlCommand("select MV FROM LSPMSummaryTable WHERE Category_Name='Net Total'", connTodo);
        CurrentNAV = Convert.ToDouble(NAVSQL.ExecuteScalar());
        SqlCommand LONAVSQL = new SqlCommand("select MV FROM LOLSPMSummaryTable WHERE Category_Name='Net Total'", connTodo);
        CurrentLONAV = Convert.ToDouble(LONAVSQL.ExecuteScalar());
        //check if the Last Earnings table Gridview3 is empty
        SqlCommand NAVSQL000 = new SqlCommand(@"SELECT DISTINCT COUNT(*) FROM
(SELECT * FROM
         (SELECT * FROM
         (SELECT TableA.BloombergID, TableA.[Last Earnings], TRY_CAST(TableA.EarningsTime AS TIME) AS EarningsTime, 100*((1+CAST(TableA.[ReturnFromLastEarnings] AS FLOAT)/100)*(1+CAST(TableA.[Afterhoursmove] AS FLOAT)/100)-1) AS [ReturnFromLastEarnings], TableA.[Long/Short] FROM (select BloombergID, [Last Earnings], [ReturnFromLastEarnings], [Long/Short], [Afterhoursmove], EarningsTime from lspmsummaryrank WHERE DATEADD(day,0,[Last Earnings])<DATEADD(hh,-7,GETUTCDATE()) AND [Last Earnings]>=DATEADD(day,-10,DATEADD(hh,-7,GETUTCDATE()))) TableA LEFT JOIN (select MSLongDescription, BloombergTicker FROM MS_ID) TableB ON (TableA.BloombergID=TableB.BloombergTicker+' EQUITY' OR TableA.BloombergID=REPLACE(TableB.BloombergTicker,' US','')+' EQUITY')) a
         LEFT JOIN
         (SELECT * FROM MS_ID) b
         ON b.BloombergTicker=REPLACE(a.BloombergID,' EQUITY','')) aRicky
         LEFT JOIN 
         (select Ticker, PartnerReview, ModelQuarterlyUpdate, [Update Date] from (select *, ROW_NUMBER() OVER(PARTITION BY Ticker ORDER BY [Update Date] DESC) rn from lspmscenariomanagement) a WHERE rn=1 AND Ticker IN (SELECT [Bloomberg ID] FROM PortfolioTest UNION SELECT [Bloomberg ID] FROM LOPortfolioTest)) bRicky
         ON REPLACE(REPLACE(aRicky.BloombergID,' EQUITY',''),' US','')=bRicky.Ticker) a
		 LEFT JOIN
		 (select * from oldpositions where date>DATEADD(dd,-15,GETUTCDATE())) b
		 ON IIF(DATEPART(hh,EarningsTime)<9,DATEADD(dd,-1,a.[Last Earnings]),a.[Last Earnings])=b.[Date] AND REPLACE(REPLACE(a.BloombergID,' EQUITY',''),' US','')=b.Ticker
		 WHERE b.Ticker IS NOT NULL
         ", connTodo);
        if (Convert.ToInt32(NAVSQL000.ExecuteScalar()) == 0)
        {
            ShowNoResultFound();
        }

        string cmd40012 = @"select distinct glenname, irwebsite from ms_id where irwebsite!='' and irwebsite is not null and replace(bloombergticker,' US','') in (select [bloomberg id] from portfoliotest)";
        try
        {
            SqlDataAdapter adp200 = new SqlDataAdapter(cmd40012, connTodo);
            adp200.Fill(ds200);
            dt200 = ds200.Tables[0];
            dt200.PrimaryKey = new DataColumn[] { dt200.Columns["glenname"] };
        }
        catch { }

        string cmd400123 = @"select distinct glenname from ms_id where irwebsite is not null";
        try
        {
            SqlDataAdapter adp2001 = new SqlDataAdapter(cmd400123, connTodo);
            adp2001.Fill(ds2001);
            dt2001 = ds2001.Tables[0];
            dt2001.PrimaryKey = new DataColumn[] { dt2001.Columns["glenname"] };
        }
        catch { }

        //where is this?
        string cmd400 = @"select * from tmpDefaultSummaryTable order by category";
        try
        {
            SqlDataAdapter adp210 = new SqlDataAdapter(cmd400, connTodo);
            adp210.Fill(ds210);
            dt210 = ds210.Tables[0];
            dt210.PrimaryKey = new DataColumn[] { dt210.Columns["category"] };
        }
        catch { }
        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Overall Losses";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            OverallLosses = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { OverallLosses = 0; }


        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Overall Losses Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            OverallLosses_Long = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { OverallLosses_Long = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Overall Losses Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            OverallLosses_Short = Convert.ToInt32(foundrow[1].ToString());

        }
        catch { OverallLosses_Short = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Overall Wins";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            OverallWins = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { OverallWins = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Overall Wins Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            OverallWins_Long = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { OverallWins_Long = 0; }
        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Overall Wins Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            OverallWins_Short = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { OverallWins_Short = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Quarter Losses";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            QuarterLosses = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { QuarterLosses = 0; }


        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Quarter Losses Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            QuarterLosses_Long = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { QuarterLosses_Long = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Quarter Losses Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            QuarterLosses_Short = Convert.ToInt32(foundrow[1].ToString());

        }
        catch { QuarterLosses_Short = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Quarter Wins";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            QuarterWins = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { QuarterWins = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Quarter Wins Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            QuarterWins_Long = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { QuarterWins_Long = 0; }
        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Quarter Wins Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            QuarterWins_Short = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { QuarterWins_Short = 0; }



        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Weekly Losses";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            WeeklyLosses = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { WeeklyLosses = 0; }


        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Weekly Losses Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            WeeklyLosses_Long = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { WeeklyLosses_Long = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Weekly Losses Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            WeeklyLosses_Short = Convert.ToInt32(foundrow[1].ToString());

        }
        catch { WeeklyLosses_Short = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Weekly Wins";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            WeeklyWins = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { WeeklyWins = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Weekly Wins Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            WeeklyWins_Long = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { WeeklyWins_Long = 0; }
        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Weekly Wins Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            WeeklyWins_Short = Convert.ToInt32(foundrow[1].ToString());
        }
        catch { WeeklyWins_Short = 0; }

        SqlCommand NAVSQL8 = new SqlCommand(@"SELECT TOP(1) IIF(DATEDIFF(mi,CONVERT(datetime,DATEADD(hh,-8,GETUTCDATE())),Refreshed)<-60,1,0) FROM EvernoteAPIDatabase ORDER BY LastUpdated DESC", connTodo);
        UpdateEvernote = Convert.ToInt32(NAVSQL8.ExecuteScalar());
        //get winloss averages for the middle table on top

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Season Avg Loss";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            SeasonAvgLoss = Convert.ToDouble(foundrow[1].ToString());
        }
        catch { SeasonAvgLoss = 0; }


        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Season Avg Loss Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            SeasonAvgLoss_Long = Convert.ToDouble(foundrow[1].ToString());
        }
        catch { SeasonAvgLoss_Long = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Season Avg Loss Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            SeasonAvgLoss_Short = Convert.ToDouble(foundrow[1].ToString());

        }
        catch { SeasonAvgLoss_Short = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Season Avg Win";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            SeasonAvgWin = Convert.ToDouble(foundrow[1].ToString());
        }
        catch { SeasonAvgWin = 0; }

        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Season Avg Win Long";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            SeasonAvgWin_Long = Convert.ToDouble(foundrow[1].ToString());
        }
        catch { SeasonAvgWin_Long = 0; }
        try
        {
            object[] findTheseVals = new object[1];
            findTheseVals[0] = "Season Avg Win Short";
            DataRow foundrow = dt210.Rows.Find(findTheseVals);
            SeasonAvgWin_Short = Convert.ToDouble(foundrow[1].ToString());
        }
        catch { SeasonAvgWin_Short = 0; }
        /*
        SeasonAvgWin = Convert.ToDouble(dt210.Rows[21][1]);
        
        SeasonAvgWin_Long = Convert.ToDouble(dt210.Rows[22][1]);
      
        SeasonAvgWin_Short = Convert.ToDouble(dt210.Rows[23][1]);
       
        SeasonAvgLoss = Convert.ToDouble(dt210.Rows[18][1]);
       
        SeasonAvgLoss_Long = Convert.ToDouble(dt210.Rows[19][1]);
        
        SeasonAvgLoss_Short = Convert.ToDouble(dt210.Rows[20][1]);
        */

        AllTimeAvgWin = Convert.ToDouble(dt210.Rows[3][1]);

        AllTimeAvgWin_Long = Convert.ToDouble(dt210.Rows[4][1]);

        AllTimeAvgWin_Short = Convert.ToDouble(dt210.Rows[5][1]);

        AllTimeAvgLoss = Convert.ToDouble(dt210.Rows[0][1]);

        AllTimeAvgLoss_Long = Convert.ToDouble(dt210.Rows[1][1]);

        AllTimeAvgLoss_Short = Convert.ToDouble(dt210.Rows[2][1]);

        connTodo.Close();

        // Timing things
        //long endTime = System.DateTime.Now.Ticks;
        //TimeSpan elapsedSpan = new TimeSpan(endTime - startTime);
        //log.Info(String.Format("It took {0} seconds to get here.", elapsedSpan.TotalSeconds));
        if (UpdateEvernote == 1)
        {
            try
            {
                //update Evernote database

                //S=s634:U=6e7c7a2:E=16fee3497c6:C=168968368c8:P=1cd:A=en-devtoken:V=2:H=32efecbb1d9c82af1cc7a08237ae1a30
                //old S=s634:U=6e7c7a2:E=16fd954bcdb:C=16881a38d70:P=185:A=richard-7601:V=2:H=b8d56650aab38506edc41167f523729b
                //new S=s634:U=6e7c7a2:E=16fef74c195:C=16897c391b8:P=185:A=richard-7601:V=2:H=4bf71ef0aa4c1169514be5c699eb1405

                ENSession.SetSharedSessionDeveloperToken("S=s634:U=6e7c7a2:E=17eb66e37aa:C=1775ebd0958:P=1cd:A=en-devtoken:V=2:H=b21be374c89f930e831af6f303710f98", "https://www.evernote.com/shard/s634/notestore");
                if (ENSession.SharedSession.IsAuthenticated == false)
                {
                    ENSession.SharedSession.AuthenticateToEvernote();
                }
                ENSessionAdvanced.SetSharedSessionDeveloperToken("S=s634:U=6e7c7a2:E=17eb66e37aa:C=1775ebd0958:P=1cd:A=en-devtoken:V=2:H=b21be374c89f930e831af6f303710f98", "https://www.evernote.com/shard/s634/notestore");
                if (ENSessionAdvanced.SharedSession.IsAuthenticated == false)
                {
                    ENSessionAdvanced.SharedSession.AuthenticateToEvernote();
                }

                /*    
                    ENSession.SetSharedSessionConsumerKey("richard-7601", "e143f0355e632250", "S%3Ds634%3AU%3D6e7c7a2%3AE%3D1696424baf7%3AC%3D1620c738c90%3AP%3D185%3AA%3Drichard-7601%3AV%3D2%3AH%3Dd05f026b07bb8fa2ed9600b40d9b257d");
                    if (ENSession.SharedSession.IsAuthenticated == false)
                    {
                        ENSession.SharedSession.AuthenticateToEvernote();
                    }
                    ENSessionAdvanced.SetSharedSessionConsumerKey("richard-7601", "e143f0355e632250", "S%3Ds634%3AU%3D6e7c7a2%3AE%3D1696424baf7%3AC%3D1620c738c90%3AP%3D185%3AA%3Drichard-7601%3AV%3D2%3AH%3Dd05f026b07bb8fa2ed9600b40d9b257d");
                    if (ENSessionAdvanced.SharedSession.IsAuthenticated == false)
                    {
                        ENSessionAdvanced.SharedSession.AuthenticateToEvernote();
                    }
                    */

                string textToFind = "";

                List<ENSessionFindNotesResult> myResultsList = ENSession.SharedSession.FindNotes(ENNoteSearch.NoteSearch(textToFind), null, ENSession.SearchScope.Business, ENSession.SortOrder.RecentlyUpdated, 5);

                foreach (ENSessionFindNotesResult resultSearch in myResultsList)
                {
                    TagListString = "";
                    ENNoteRef noteRef = resultSearch.NoteRef;

                    ENNoteStoreClient noteStore = ENSessionAdvanced.SharedSession.NoteStoreForNoteRef(noteRef);
                    Note edamNote = noteStore.GetNote(noteRef.Guid, true, false, false, false);

                    _TagList = edamNote.TagGuids;

                    try
                    {
                        foreach (var tagGuid in _TagList)
                        {
                            var tag = ENSessionAdvanced.SharedSession.BusinessNoteStore.GetTag(tagGuid);
                            if (1 == 1) //tag.Name.Substring(0, 1) != "_p" IF YOU DO NOT WANT TO INCLUDE PEOPLE TAGS
                            {
                                if (TagListString == "")
                                {
                                    TagListString = tag.Name;
                                }
                                else
                                {
                                    TagListString = TagListString + ", " + tag.Name;
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        UpdateSQLStatement = UpdateSQLStatement + @" IF (SELECT COUNT(*) FROM EvernoteAPIDatabase WHERE [NoteGuid]='" + resultSearch.NoteRef.Guid + "')=0 INSERT INTO EvernoteAPIDatabase ([tags], [NoteGuid], [NoteTitle], [LastUpdated], [Refreshed], [BodyContent], [Author]) VALUES ('" + TagListString + "', CONVERT(VARCHAR(MAX),'" + resultSearch.NoteRef.Guid + "'), CONVERT(VARCHAR(MAX),'" + resultSearch.Title.Replace("'", "") + "'), CONVERT(datetime,'" + resultSearch.Updated + "'), DATEADD(hh,-8,GETUTCDATE()), '" + edamNote.Content.Replace("'", "''") + "', '" + edamNote.Attributes.Author + "') ELSE UPDATE EvernoteAPIDatabase SET [tags]='" + TagListString + "', [NoteTitle]='" + resultSearch.Title.Replace("'", "''") + "', [LastUpdated]=CONVERT(datetime, '" + resultSearch.Updated + "'), [Refreshed]=DATEADD(hh, -8, GETUTCDATE()), [BodyContent]='" + edamNote.Content.Replace("'", "''") + "', [Author]='" + edamNote.Attributes.Author + "' WHERE [NoteGuid]='" + resultSearch.NoteRef.Guid + "'; ";
                    }
                    catch { }
                }
                //insert into the database
                using (var connection = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True"))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(UpdateSQLStatement, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                using (var connectionZ = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSC_Research;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True"))
                {
                    connectionZ.Open();
                    using (var cmdZ = new SqlCommand(UpdateSQLStatement, connectionZ))
                    {
                        cmdZ.ExecuteNonQuery();
                    }
                    connectionZ.Close();
                }
            }
            catch { }
        }

        try
        {
            if (WeeklyWins + WeeklyLosses == 0)
            {
                WeeklyPct = 1000;
                WeeklyPct_Long = 1000;
                WeeklyPct_Short = 1000;
            }
            else
            {
                WeeklyPct = Math.Round(WeeklyWins / (WeeklyWins + WeeklyLosses), 3);
                WeeklyPct_Long = Math.Round(WeeklyWins_Long / (WeeklyWins_Long + WeeklyLosses_Long), 3);
                WeeklyPct_Short = Math.Round(WeeklyWins_Short / (WeeklyWins_Short + WeeklyLosses_Short), 3);
            }
        }
        catch { }
        try
        {
            if (QuarterWins + QuarterLosses == 0)
            {
                QuarterPct = 1000;
                QuarterPct_Long = 1000;
                QuarterPct_Short = 1000;
            }
            else
            {
                QuarterPct = Math.Round((QuarterWins + WeeklyWins) / (QuarterWins + QuarterLosses + WeeklyWins + WeeklyLosses), 3);
                QuarterPct_Long = Math.Round((QuarterWins_Long + WeeklyWins_Long) / (QuarterWins_Long + QuarterLosses_Long + WeeklyWins_Long + WeeklyLosses_Long), 3);
                QuarterPct_Short = Math.Round((QuarterWins_Short + WeeklyWins_Short) / (QuarterWins_Short + QuarterLosses_Short + WeeklyWins_Short + WeeklyLosses_Short), 3);
            }
        }
        catch { }
        try
        {
            OverallPct = Math.Round((OverallWins + WeeklyWins) / (OverallWins + OverallLosses + WeeklyWins + WeeklyLosses), 3);
            OverallPct_Long = Math.Round((OverallWins_Long + WeeklyWins_Long) / (OverallWins_Long + OverallLosses_Long + WeeklyWins_Long + WeeklyLosses_Long), 3);
            OverallPct_Short = Math.Round((OverallWins_Short + WeeklyWins_Short) / (OverallWins_Short + OverallLosses_Short + WeeklyWins_Short + WeeklyLosses_Short), 3);
        }
        catch { }
        /*
        if (!Request.Browser.IsMobileDevice)
        {
            var h2 = new HtmlGenericControl("h2");
            h2.InnerHtml = "Portfolio News";
            //h2.Attributes.Add("style", "padding-top:0px");
            MyServerControlDiv.Controls.Add(h2);

            OrangeNAVBar.Attributes.Remove("class");
            OrangeNAVBar.Attributes.Add("class", "w3-bar w3-orange");

            Index_List.Attributes.Remove("class");
            Index_List.Attributes.Add("class", "w3-navbar2 w3-white");

            mySidenav.Attributes.Remove("class");
            mySidenav.Attributes.Add("class", "w3-sidenav w3-collapse w3-blue w3-card-2");
            TitleBlue1.Attributes.Add("class", "legend");
            TitleBlue2.Attributes.Add("class", "legend");
            TitleBlue3.Attributes.Add("class", "legend");
            TitleBlue4.Attributes.Add("class", "legend");
            TitleBlue5.Attributes.Add("class", "legend");
            RegionLabel.Attributes.Add("class", "ddlLabel");
            SectorLabel.Attributes.Add("class", "ddlLabel");
            //mySidenav.Attributes.Add("style", "border-right:1px solid black;background-color:#dbdbdb;font-size:12px");
            //SpotlightIconDiv.Attributes.Add("style", "padding-top:14px");
        }
        else
        {
            Response.Redirect("DefaultMobile.aspx");
            SpotlightPicture.Height = 30; SpotlightPicture.Width = 240;
            var h1 = new HtmlGenericControl("h1");
            SpotlightIconDiv.Attributes.Remove("class");
            SpotlightIconDiv.Attributes.Add("class", "mobileSpolightIcon");
            h1.InnerHtml = "Portfolio News";
            try { MyServerControlDiv.Controls.Add(h1); } catch { }
            SpotlightIconDiv.Attributes.Remove("class");
            SpotlightIconDiv.Attributes.Add("class", "mobileSpotlightIcon");
            GridView1.Font.Size = 9;
            GridView2.Font.Size = 9;
            GridView3.Font.Size = 9;
            GridView4.Font.Size = 9;
            TitleBlue1.Attributes.Add("class", "legendmobile");
            TitleBlue2.Attributes.Add("class", "legendmobile");
            TitleBlue3.Attributes.Add("class", "legendmobile");
            TitleBlue4.Attributes.Add("class", "legendmobile");
            TitleBlue5.Attributes.Add("class", "legendmobile");
            ddlRegion.Style.Add("font-size", "24px");
            ddlSector.Style.Add("font-size", "24px");
            RegionLabel.Attributes.Add("class", "ddlLabelMobile");
            SectorLabel.Attributes.Add("class", "ddlLabelMobile");
        }*/

        // set up page for PC vs mobile and set the menu selected text in top and left menus
        if (!Request.Browser.IsMobileDevice)
        {
            var h2 = new HtmlGenericControl("h2");
            h2.InnerHtml = page_title;
            try
            {
                MyServerControlDiv.Controls.Add(h2);
            }
            catch { }
            Index_List.Attributes.Remove("class");
            Index_List.Attributes.Add("class", "w3-navbar2 w3-white");

            mySidenav.Attributes.Remove("class");
            mySidenav.Attributes.Add("class", "w3-sidenav w3-collapse w3-blue w3-card-2");
            TitleBlue1.Attributes.Add("class", "legend");
            TitleBlue2.Attributes.Add("class", "legend");
            TitleBlue3.Attributes.Add("class", "legend");
            TitleBlue4.Attributes.Add("class", "legend");
            TitleBlue5.Attributes.Add("class", "legend");
            RegionLabel.Attributes.Add("class", "ddlLabel");
            SectorLabel.Attributes.Add("class", "ddlLabel");
        }
        else
        {
            Response.Redirect("DefaultMobile.aspx");
            // Change classes to accommodate view
            try
            {
                HtmlImage spotlight_picture = (HtmlImage)Page.FindControl("SpotlightPicture");
                spotlight_picture.Height = 30;
                spotlight_picture.Width = 240;
                var h1 = new HtmlGenericControl("h1");
                h1.InnerHtml = page_title;
                try { MyServerControlDiv.Controls.Add(h1); }
                catch { }
            }
            catch { }
            try
            {
                HtmlGenericControl spotlight_icon_div = (HtmlGenericControl)Page.FindControl("SpotlightIconDiv");
                spotlight_icon_div.Attributes.Remove("class");
                spotlight_icon_div.Attributes.Add("class", "mobileSpotlightIcon");
            }
            catch { }
            try
            {
                HtmlGenericControl orange_nav_bar = (HtmlGenericControl)Page.FindControl("OrangeNAVBar");
                orange_nav_bar.Attributes.Remove("class");
                orange_nav_bar.Attributes.Add("class", "w3-barMobile w3-orange");
            }
            catch { }
            try
            {
                HtmlGenericControl my_side_nav = (HtmlGenericControl)Page.FindControl("mySidenav");
                my_side_nav.Attributes.Remove("class");
                my_side_nav.Attributes.Add("class", "w3-sidenavMobile w3-collapse w3-blue w3-card-2");
            }
            catch { }
        }

        // Set the left and top menu link selections. You need to update each menu...
        try
        {
            HyperLink control = (HyperLink)Page.FindControl(left_menu_id_selector);
            control.CssClass = "w3-text-teal";
        }
        catch { }
        try
        {
            HyperLink control = (HyperLink)Page.FindControl(top_menu_id_selector);
            control.CssClass = "w3-bar-item w3-button w3-text-white";
        }
        catch { }
        UpdateTickerTape();
        hdnfldTickerList.Value = GetTickerList();

        ClearQueryStrings();
    }

    /*
     * Use to remove the request.querystring paramters
     * Needed to do so otherwise the region/theme pull downs would not work once the FilterNews query string was set.
     */
    public void ClearQueryStrings()
    {
        string clientCommand = string.Format(
          "document.forms[\"{0}\"].action = \"{1}\";",
             this.Form.Name, Request.Url.Segments[Request.Url.Segments.Length - 1]);

        ClientScript.RegisterStartupScript(this.GetType(), "qr", clientCommand, true);
    }

    protected string GetTickerList()
    {
        DataTable dt = new DataTable();
        using (SqlConnection con = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True"))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT GlenName, MSLongDescription, BloombergTicker FROM MS_ID WHERE MSLongDescription IS NOT NULL ORDER BY GlenName", con))
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in dt.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }

                return JsonConvert.SerializeObject(rows);
                con.Close();
            }
        }
    }

    public void UpdateTickerTape()
    {
        //check if US is not open
        if (Convert.ToDouble(DateTime.Now.ToString("HH")) > 15 || Convert.ToDouble(DateTime.Now.ToString("HH")) < 6 || (Convert.ToDouble(DateTime.Now.ToString("HH")) == 6 && Convert.ToDouble(DateTime.Now.ToString("mm")) < 30))
        {
            //US is closed
            SqlConnection conn = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True");
            conn.Open();
            SqlCommand LS_Command = new SqlCommand("select [Contrib Today] from lspmsummarytable where category_name='Net Total'", conn);
            SqlCommand LSLO_Command = new SqlCommand("select [Contrib Today] from lolspmsummarytable where category_name='Net Total'", conn);
            SqlCommand NKY_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='NKY INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='NKY INDEX'", conn);
            SqlCommand HSI_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='HSI INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='HSI INDEX'", conn);
            SqlCommand SHCOMP_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='SHCOMP INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='SHCOMP INDEX'", conn);
            SqlCommand SPX_Command = new SqlCommand("select Performance from indexperformance WHERE [Index]='SPA INDEX'", conn);
            SqlCommand NQA_Command = new SqlCommand("select Performance from indexperformance WHERE [Index]='NQA INDEX'", conn);
            SqlCommand MXWO_Command = new SqlCommand("select Performance from indexperformance WHERE [Index]='MXWO INDEX'", conn);
            LSPerformance = Convert.ToString(LS_Command.ExecuteScalar());
            LSLOPerformance = Convert.ToString(LSLO_Command.ExecuteScalar());
            NKYPerformance = Convert.ToString(NKY_Command.ExecuteScalar());
            HSIPerformance = Convert.ToString(HSI_Command.ExecuteScalar());
            SHCOMPPerformance = Convert.ToString(SHCOMP_Command.ExecuteScalar());
            SPXPerformance = Convert.ToString(SPX_Command.ExecuteScalar());
            NQAPerformance = Convert.ToString(NQA_Command.ExecuteScalar());
            MXWOPerformance = Convert.ToString(MXWO_Command.ExecuteScalar());
            AddMenuItem("LS-HF", LSPerformance);
            AddMenuItem("LS-LO", LSLOPerformance);
            AddMenuItem("Nikkei", NKYPerformance);
            AddMenuItem("Hang Seng", HSIPerformance);
            AddMenuItem("Shanghai", SHCOMPPerformance);
            AddMenuItem("MSCI World", MXWOPerformance);
            AddMenuItem("S&P Futures", SPXPerformance);
            AddMenuItem("NASDAQ Futures", NQAPerformance);
            conn.Close();
        }
        else
        {
            //US is open
            SqlConnection conn = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True");
            conn.Open();
            SqlCommand LS_Command = new SqlCommand("select [Contrib Today] from lspmsummarytable where category_name='Net Total'", conn);
            SqlCommand LSLO_Command = new SqlCommand("select [Contrib Today] from lolspmsummarytable where category_name='Net Total'", conn);
            SqlCommand CCMP_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='CCMP INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='CCMP INDEX'", conn);
            SqlCommand SPX_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='CCMP INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='SPX INDEX'", conn);
            SqlCommand IGV_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='CCMP INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='IGV EQUITY'", conn);
            SqlCommand XRT_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='CCMP INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='XRT EQUITY'", conn);
            SqlCommand SOCL_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='CCMP INDEX')>0 
                select -100
                else
                select Performance from indexperformance WHERE [Index]='SOCL EQUITY'", conn);
            SqlCommand KWEB_Command = new SqlCommand(@"IF (SELECT COUNT(*) FROM (select MarketIndex from holidaymarkets where market in (select DISTINCT LongMarket from holidayschedule where (date=DATEADD(dd,1,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket!='US' AND ShortMarket!='CA' AND ShortMarket!='FR' AND ShortMarket!='GE' AND ShortMarket!='LS' AND ShortMarket!='SZ')) OR (date=DATEADD(dd,0,CONVERT(date,DATEADD(hh,-8,GETUTCDATE()))) AND (ShortMarket='US' OR ShortMarket='CA' OR ShortMarket='FR' OR ShortMarket='GE' OR ShortMarket='LS' OR ShortMarket='SZ')))) a WHERE MarketIndex='CCMP INDEX')>0 
                select -100
                else                
                select Performance from indexperformance WHERE [Index]='KWEB EQUITY'", conn);
            SqlCommand MXWO_Command = new SqlCommand("select Performance from indexperformance WHERE [Index]='MXWO INDEX'", conn);
            LSPerformance = Convert.ToString(LS_Command.ExecuteScalar());
            LSLOPerformance = Convert.ToString(LSLO_Command.ExecuteScalar());
            CCMPPerformance = Convert.ToString(CCMP_Command.ExecuteScalar());
            SPXPerformance = Convert.ToString(SPX_Command.ExecuteScalar());
            IGVPerformance = Convert.ToString(IGV_Command.ExecuteScalar());
            XRTPerformance = Convert.ToString(XRT_Command.ExecuteScalar());
            SOCLPerformance = Convert.ToString(SOCL_Command.ExecuteScalar());
            KWEBPerformance = Convert.ToString(KWEB_Command.ExecuteScalar());
            MXWOPerformance = Convert.ToString(MXWO_Command.ExecuteScalar());
            AddMenuItem("LS-HF", LSPerformance);
            AddMenuItem("LS-LO", LSLOPerformance);
            AddMenuItem("Nasdaq", CCMPPerformance);
            AddMenuItem("S&P 500", SPXPerformance);
            AddMenuItem("IGV", IGVPerformance);
            AddMenuItem("XRT", XRTPerformance);
            AddMenuItem("SOCL", SOCLPerformance);
            AddMenuItem("KWEB", KWEBPerformance);
            AddMenuItem("MSCI World", MXWOPerformance);
            conn.Close();
        }
    }

    public void AddMenuItem(string IndexName, string performance)
    {
        try
        {
            Label label1 = new Label();
            if (Convert.ToDouble(performance) > 0)
            {
                label1.Text = IndexName + "<pup> \u25b2 </pup>" + String.Format("{0:0.00}", Convert.ToDouble(performance)) + "% &nbsp;&nbsp;&nbsp;";
            }
            else
            {
                label1.Text = IndexName + "<pdown> \u25bc </pdown>" + String.Format("{0:0.00}", Convert.ToDouble(performance)) + "% &nbsp;&nbsp;&nbsp;";
            }
            if (Convert.ToDouble(performance) == -100)
            {
                if (Request.Browser.IsMobileDevice)
                {
                    label1.Text = IndexName + "<img src='../Images/blacksquare.png' height='24' style='padding-bottom:1px;padding-right:3px;padding-left:3px;'>Closed &nbsp;&nbsp;&nbsp;";
                }
                else
                {
                    label1.Text = IndexName + " &#11200; Closed &nbsp;&nbsp;&nbsp;";
                }
            }
            Index_List.Controls.Add(new LiteralControl("<li>"));
            Index_List.Controls.Add(label1);
            Index_List.Controls.Add(new LiteralControl("</li>"));
        }
        catch { }
    }

    /*
     * aspx file line 155 had: <asp:Timer ID="Timer1" runat="server" Interval="30000" ontick="Timer1_Tick"></asp:Timer> 
    protected void Timer1_Tick(object sender, EventArgs e)
    {
        log.Info("Query: " + SqlDataSource1.SelectCommand.ToString());
        SqlDataSource1.SelectCommand = SqlDeadlockPriority + @" EXEC dbo.proc_defaultaspx_sqldatasource1_newsstories @date_near = 3, @date_far = 7";
        log.Info("Query: " + SqlDataSource1.SelectCommand.ToString());
        GridView1.DataBind();
        GridView2.DataBind();
        GridView3.DataBind();
    }
    */

    public override bool EnableEventValidation
    {
        get { return false; }
        set { }
    }
    public int RowCounter = 0;
    public double TmrwAttribution;
    public double TmrwLOAttribution;
    public string AttString;
    public string AttLOString;
    protected void EarningsDataBound1(object sender, GridViewRowEventArgs e)
    {
        if (Request.Browser.IsMobileDevice)
        {
            GridView1.Columns[3].ItemStyle.Width = 500;
            GridView1.Columns[3].ItemStyle.Width = 500;
        }
        try
        {
            e.Row.Cells[4].Style["border-right"] = "1px solid black";
            e.Row.Cells[8].Style["border-right"] = "1px solid black";


            e.Row.Cells[3].Visible = false;
            e.Row.Cells[4].Visible = false;
            e.Row.Cells[5].Visible = false;
            e.Row.Cells[6].Visible = false;
            e.Row.Cells[9].Visible = false;


            e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            if (e.Row.RowType == DataControlRowType.Header)
            {
                RowCounter = 0;
                e.Row.Style["border-bottom"] = "1px solid black";


            }
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                RowCounter = RowCounter + 1;


                tmpDate = Convert.ToDateTime(e.Row.Cells[1].Text);
                e.Row.Cells[8].Text = string.Format("{0:N1}", 100 * Convert.ToDouble(e.Row.Cells[6].Text) / CurrentNAV) + "%";
                if (Convert.ToDouble(e.Row.Cells[8].Text.Replace("%", "")) > 0)
                {
                    e.Row.Cells[8].ForeColor = System.Drawing.Color.Blue;
                }
                if (Convert.ToDouble(e.Row.Cells[8].Text.Replace("%", "")) < 0)
                {
                    e.Row.Cells[8].ForeColor = System.Drawing.Color.Red;
                }
            }
        }
        catch { }
        try
        {
            if (e.Row.Cells[2].Text == "00:00:00")
            {
                e.Row.Cells[2].Text = "TBA";
            }
            if (e.Row.Cells[3].Text.Substring(e.Row.Cells[3].Text.Length - 9) == "US EQUITY")
            {
                if (Convert.ToDouble(e.Row.Cells[2].Text.Substring(0, 2)) <= 6)
                {
                    e.Row.Cells[2].Text = "Bef-mkt";
                }
                if (Convert.ToDouble(e.Row.Cells[2].Text.Substring(0, 2)) >= 13)
                {
                    e.Row.Cells[2].Text = "Aft-mkt";
                }
            }
        }
        catch { }


        try
        {
            if (e.Row.Cells[7].Text.Substring(0, 1) == "#")
            {
                e.Row.Cells[7].Text = "<a target='_blank' href='GoToIRSite.aspx?CompanyName=" + e.Row.Cells[5].Text + "'>No Data</a>";
            }
            else
            {
                e.Row.Cells[7].Text = "<a target='_blank' href='GoToIRSite.aspx?CompanyName=" + e.Row.Cells[5].Text + "'>" + e.Row.Cells[7].Text + "</a>";
            }
        }
        catch { }

        try
        {
            //after market
            if (Convert.ToDouble(DateTime.Now.ToString("HH")) >= 13)
            {
                try
                {
                    if ((e.Row.Cells[2].Text == "Aft-mkt" && Convert.ToDateTime(e.Row.Cells[1].Text).Date == DateTime.Today))
                    {
                        if (Convert.ToDouble(e.Row.Cells[4].Text) > 0)
                        {

                            e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[3].Text.Replace(" US EQUITY", "").Replace(" EQUITY", "") + "'>" + e.Row.Cells[5].Text.Replace(" EQUITY", "") + "</a> <span style='color:green'>\u25b2 " + Convert.ToString(Math.Round(Convert.ToDouble(e.Row.Cells[4].Text), 1)) + "% (Afterhours)</span>";
                        }
                        else
                        {
                            e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[3].Text.Replace(" US EQUITY", "").Replace(" EQUITY", "") + "'>" + e.Row.Cells[5].Text + "</a> <span style='color:red'>\u25bc " + Convert.ToString(Math.Round(Convert.ToDouble(e.Row.Cells[4].Text), 1)) + "% (Afterhours)</span>";
                        }
                        try
                        {
                            e.Row.Cells[6].Text = Convert.ToString(Convert.ToDouble(e.Row.Cells[4].Text) * Convert.ToDouble(e.Row.Cells[6].Text) / CurrentNAV);
                            e.Row.Cells[9].Text = Convert.ToString(Convert.ToDouble(e.Row.Cells[4].Text) * Convert.ToDouble(e.Row.Cells[9].Text) / CurrentLONAV);
                            //hedge fund attribution
                            TmrwAttribution = TmrwAttribution + Convert.ToDouble(e.Row.Cells[6].Text);
                            if (TmrwAttribution > 0)
                            {
                                AttString = "<span style='color:green'>HF Att \u25b2 " + String.Format("{0:0.00}", TmrwAttribution) + "%</span>";
                            }
                            else
                            {
                                AttString = "<span style='color:red'>HF Att \u25bc " + String.Format("{0:0.00}", TmrwAttribution) + "%</span>";
                            }
                            if (TmrwAttribution == 0)
                            {
                                AttString = "<span style='color:black'>HF Att " + String.Format("{0:0.00}", TmrwAttribution) + "%</span>";
                            }

                            //long only attribution
                            TmrwLOAttribution = TmrwLOAttribution + Convert.ToDouble(e.Row.Cells[9].Text);
                            if (TmrwLOAttribution > 0)
                            {
                                AttLOString = "<span style='color:green'>LO Att \u25b2 " + String.Format("{0:0.00}", TmrwLOAttribution) + "%</span>";
                            }
                            else
                            {
                                AttLOString = "<span style='color:red'>LO Att \u25bc " + String.Format("{0:0.00}", TmrwLOAttribution) + "%</span>";
                            }
                            if (TmrwLOAttribution == 0)
                            {
                                AttLOString = "<span style='color:black'>LO Att " + String.Format("{0:0.00}", TmrwLOAttribution) + "%</span>";
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[3].Text.Replace(" US EQUITY", "").Replace(" EQUITY", "") + "'>" + e.Row.Cells[5].Text + "</a>";
                        e.Row.Cells[6].Text = "-";
                    }
                }
                catch { }
            }
            else
            {
                if (((e.Row.Cells[2].Text == "Bef-mkt" && Convert.ToDateTime(e.Row.Cells[1].Text).Date == DateTime.Today) || (e.Row.Cells[2].Text == "Aft-mkt" && (Convert.ToDateTime(e.Row.Cells[1].Text).Date - DateTime.Today).TotalDays < 0)) && ((Convert.ToDouble(DateTime.Now.ToString("HH")) == 6 && Convert.ToDouble(DateTime.Now.ToString("MM")) < 30) || Convert.ToDouble(DateTime.Now.ToString("HH")) < 6))
                {
                    if (Convert.ToDouble(e.Row.Cells[4].Text) > 0)
                    {

                        e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[3].Text.Replace(" US EQUITY", "").Replace(" EQUITY", "") + "'>" + e.Row.Cells[5].Text.Replace(" EQUITY", "") + "</a> <span style='color:green'>\u25b2 " + Convert.ToString(Math.Round(Convert.ToDouble(e.Row.Cells[4].Text), 1)) + "% (Premarket)</span>";
                    }
                    else
                    {
                        e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[3].Text.Replace(" US EQUITY", "").Replace(" EQUITY", "") + "'>" + e.Row.Cells[5].Text + "</a> <span style='color:red'>\u25bc " + Convert.ToString(Math.Round(Convert.ToDouble(e.Row.Cells[4].Text), 1)) + "% (Premarket)</span>";
                    }
                    try
                    {
                        e.Row.Cells[6].Text = Convert.ToString(Convert.ToDouble(e.Row.Cells[4].Text) * Convert.ToDouble(e.Row.Cells[6].Text) / CurrentNAV);

                        TmrwAttribution = TmrwAttribution + Convert.ToDouble(e.Row.Cells[6].Text);
                        if (TmrwAttribution > 0)
                        {
                            AttString = "<span style='color:green'>HF Att \u25b2 " + String.Format("{0:0.00}", TmrwAttribution) + "%</span>";
                        }
                        else
                        {
                            AttString = "<span style='color:red'>HF Att \u25bc " + String.Format("{0:0.00}", TmrwAttribution) + "%</span>";
                        }
                        if (TmrwAttribution == 0)
                        {
                            AttString = "<span style='color:green'>HF Att " + String.Format("{0:0.00}", TmrwAttribution) + "%</span>";
                        }
                        e.Row.Cells[9].Text = Convert.ToString(Convert.ToDouble(e.Row.Cells[4].Text) * Convert.ToDouble(e.Row.Cells[9].Text) / CurrentLONAV);
                        if (Convert.ToDouble(e.Row.Cells[7].Text.Replace("%", "")) > 0)
                        {
                            TmrwLOAttribution = TmrwLOAttribution + Convert.ToDouble(e.Row.Cells[6].Text);
                        }
                        if (TmrwLOAttribution > 0)
                        {
                            AttLOString = "<span style='color:green'>LO Att \u25b2 " + String.Format("{0:0.00}", TmrwLOAttribution) + "%</span>";
                        }
                        else
                        {
                            AttLOString = "<span style='color:red'>LO Att \u25bc " + String.Format("{0:0.00}", TmrwLOAttribution) + "%</span>";
                        }
                        if (TmrwLOAttribution == 0)
                        {
                            AttLOString = "<span style='color:black'>LO Att " + String.Format("{0:0.00}", TmrwLOAttribution) + "%</span>";
                        }
                    }
                    catch { }
                }
                else
                {
                    e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[3].Text.Replace(" US EQUITY", "").Replace(" EQUITY", "") + "'>" + e.Row.Cells[5].Text + "</a>";
                }
            }

            if (!Request.Browser.IsMobileDevice)
            {
                AttString = AttString.Replace("<span style='color:green'>", "<span style='color:green;font-size:10px'>").Replace("<span style='color:red'>", "<span style='color:red;font-size:10px'>").Replace("<span style='color:black'>", "<span style='color:black;font-size:10px'>");
                AttLOString = AttLOString.Replace("<span style='color:green'>", "<span style='color:green;font-size:10px'>").Replace("<span style='color:red'>", "<span style='color:red;font-size:10px'>").Replace("<span style='color:black'>", "<span style='color:black;font-size:10px'>");
                CombAttString = AttString + "<span style='color:black;font-size:10px'>&nbsp;|&nbsp;</span>" + AttLOString;
            }
            else
            {
                AttString = AttString.Replace("<span style='color:green'>", "<span style='color:green;font-size:12px'>").Replace("<span style='color:red'>", "<span style='color:red;font-size:12px'>").Replace("<span style='color:black'>", "<span style='color:black;font-size:12px'>"); ;
                AttLOString = AttLOString.Replace("<span style='color:green'>", "<span style='color:green;font-size:12px'>").Replace("<span style='color:red'>", "<span style='color:red;font-size:12px'>").Replace("<span style='color:black'>", "<span style='color:black;font-size:12px'>"); ;
                CombAttString = AttString + "<span style='color:black;font-size:12px'>&nbsp;|&nbsp;</span>" + AttLOString;
            }
        }
        catch { }
        //put phone icon if it has an irlink in ms_id
        try
        {
            object[] findTheseVals2 = new object[1];
            findTheseVals2[0] = e.Row.Cells[5].Text;
            DataRow foundrow2 = dt200.Rows.Find(findTheseVals2);

            e.Row.Cells[0].Text = e.Row.Cells[0].Text + "<a target='_blank' href='" + foundrow2[1].ToString() + "'><img src='../../Images/transparent.png' width='6px'><i class=\"far fa-phone fa-sm fa-fw\"></i></a>";
        }
        catch { }
    }
    public double WinAvg;
    public double LossAvg;
    public int WinCount;
    public int LossCount;

    public double WinAvg_Long;
    public double LossAvg_Long;
    public int WinCount_Long;
    public int LossCount_Long;

    public double WinAvg_Short;
    public double LossAvg_Short;
    public int WinCount_Short;
    public int LossCount_Short;
    public string tmpRicky;
    protected void EarningsDataBound2(object sender, GridViewRowEventArgs e)
    {
        e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
        e.Row.Cells[3].HorizontalAlign = HorizontalAlign.Right;
        e.Row.Cells[4].HorizontalAlign = HorizontalAlign.Right;
        e.Row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
        if (e.Row.RowType == DataControlRowType.Header)
        {
            /*
            e.Row.Cells[1].Text = "Avg W";
            e.Row.Cells[2].Text = "Avg L";
            e.Row.Cells[3].Text = "W/L";
            e.Row.Cells[4].Text = "Win%";
            e.Row.Cells[5].Text = "ExpΔ";
            */
        }
        if (1 == 1)
        {

            e.Row.Cells[5].Text = "";
            e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[3].HorizontalAlign = HorizontalAlign.Right;
            e.Row.Cells[4].HorizontalAlign = HorizontalAlign.Right;
            if (e.Row.RowType == DataControlRowType.Header)
            {
                RowCounter = 0;
                e.Row.Style["border-bottom"] = "1px solid black";
                e.Row.Cells[4].ToolTip = "Win/Loss";
                e.Row.Cells[5].ToolTip = "Model updated";
            }
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (!string.IsNullOrEmpty(((HyperLink)e.Row.Cells[0].Controls[0]).Text) && ((HyperLink)e.Row.Cells[0].Controls[0]).Text != "Genius Electronic Optical")
                {
                    ((HyperLink)e.Row.Cells[0].Controls[0]).Text = ((HyperLink)e.Row.Cells[0].Controls[0]).Text + "<img src='../../Images/transparent.png' width='6px'><span><a target='_blank' href='GoToIRSite.aspx?CompanyName=" + ((HyperLink)e.Row.Cells[0].Controls[0]).Text + "'><i class=\"far fa-phone fa-sm fa-fw\"></i></a></span>";
                }
                if (((HyperLink)e.Row.Cells[0].Controls[0]).Text == "H&M")
                {
                    tmpRicky = "hi";
                    if (Convert.ToDouble(e.Row.Cells[2].Text) < 0)
                    {
                        HMBWin = 1;
                        HMBWinAvg = Convert.ToDouble(e.Row.Cells[2].Text);
                        HMBWinPct = 1;
                    }
                    else
                    {
                        HMBLoss = 1;
                        HMBLossAvg = Convert.ToDouble(e.Row.Cells[2].Text);
                        HMBWinPct = 0;
                    }
                }
                if (e.Row.Cells[4].Text.Trim() == "In Process")
                {
                    e.Row.Cells[4].Text = "<img src='../Images/red_x_check.png' width='10px'  style='padding-bottom:3px'>";
                    e.Row.Cells[4].ToolTip = "In Process";
                }
                if (e.Row.Cells[4].Text.Trim() == "Complete")
                {
                    e.Row.Cells[4].Text = "<img src='../Images/green_circle_check.png' width='10px'  style='padding-bottom:3px'>";
                    e.Row.Cells[4].ToolTip = "Complete";
                }
                if (e.Row.Cells[4].Text.Trim() == "Not Complete")
                {
                    e.Row.Cells[4].Text = "<img src='../Images/red_x_check.png' width='10px'  style='padding-bottom:3px'>";
                    e.Row.Cells[4].ToolTip = "Not Complete";
                }
                if (e.Row.Cells[3].Text == "L")
                {
                    if (Convert.ToDouble(e.Row.Cells[2].Text) > 0)
                    {
                        WinAvg = WinAvg + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                        WinAvg_Long = WinAvg_Long + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                        WinCount++;
                        WinCount_Long++;
                        e.Row.Cells[3].Text = "<img src='../Images/green_circle_check.png' width='10px'  style='padding-bottom:3px'>";
                    }
                    else
                    {
                        LossAvg = LossAvg + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                        LossAvg_Long = LossAvg_Long + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                        LossCount++;
                        LossCount_Long++;
                        e.Row.Cells[3].Text = "<img src='../Images/red_x_check.png' width='10px'  style='padding-bottom:3px'>";
                    }
                }
                else
                {
                    try
                    {
                        if (Convert.ToDouble(e.Row.Cells[2].Text) < 0)
                        {
                            WinAvg = WinAvg + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                            WinAvg_Short = WinAvg_Short + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                            WinCount++;
                            WinCount_Short++;
                            e.Row.Cells[3].Text = "<img src='../Images/green_circle_check.png' width='10px'  style='padding-bottom:3px'>";
                        }
                        else
                        {
                            LossAvg = LossAvg + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                            LossAvg_Short = LossAvg_Short + Math.Abs(Convert.ToDouble(e.Row.Cells[2].Text));
                            LossCount++;
                            LossCount_Short++;
                            e.Row.Cells[3].Text = "<img src='../Images/red_x_check.png' width='10px' style='padding-bottom:3px'>";
                        }
                    }
                    catch { }
                }
                try
                {
                    if (Convert.ToDouble(e.Row.Cells[2].Text) > 0)
                    {
                        e.Row.Cells[2].ForeColor = System.Drawing.Color.Blue;
                    }
                    else
                    {
                        e.Row.Cells[2].ForeColor = System.Drawing.Color.Red;
                    }
                    e.Row.Cells[2].Text = String.Format("{0:#,##0.0}", Convert.ToDouble(e.Row.Cells[2].Text)) + "%";
                }
                catch { }
            }
        }
        else
        {
            SqlConnection connTodo = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True");
            connTodo.Open();

            SqlCommand LastTenDaysSQL = new SqlCommand(@"SELECT COUNT(*) FROM
(SELECT * FROM
         (SELECT * FROM
         (SELECT TableA.BloombergID, TableA.[Last Earnings], TRY_CAST(TableA.EarningsTime AS TIME) AS EarningsTime, 100*((1+CAST(TableA.[ReturnFromLastEarnings] AS FLOAT)/100)*(1+CAST(TableA.[Afterhoursmove] AS FLOAT)/100)-1) AS [ReturnFromLastEarnings], TableA.[Long/Short] FROM (select BloombergID, [Last Earnings], [ReturnFromLastEarnings], [Long/Short], [Afterhoursmove], EarningsTime from lspmsummaryrank WHERE DATEADD(day,0,[Last Earnings])<DATEADD(hh,-7,GETUTCDATE()) AND [Last Earnings]>=DATEADD(day,-10,DATEADD(hh,-7,GETUTCDATE()))) TableA LEFT JOIN (select MSLongDescription, BloombergTicker FROM MS_ID) TableB ON (TableA.BloombergID=TableB.BloombergTicker+' EQUITY' OR TableA.BloombergID=REPLACE(TableB.BloombergTicker,' US','')+' EQUITY')) a
         LEFT JOIN
         (SELECT * FROM MS_ID) b
         ON b.BloombergTicker=REPLACE(a.BloombergID,' EQUITY','')) aRicky
         LEFT JOIN 
         (select Ticker, PartnerReview, ModelQuarterlyUpdate, [Update Date] from (select *, ROW_NUMBER() OVER(PARTITION BY Ticker ORDER BY [Update Date] DESC) rn from lspmscenariomanagement) a WHERE rn=1 AND Ticker IN (SELECT [Bloomberg ID] FROM PortfolioTest UNION SELECT [Bloomberg ID] FROM LOPortfolioTest)) bRicky
         ON REPLACE(REPLACE(aRicky.BloombergID,' EQUITY',''),' US','')=bRicky.Ticker) a
		 LEFT JOIN
		 (select * from oldpositions where date>DATEADD(dd,-15,GETUTCDATE())) b
		 ON IIF(DATEPART(hh,EarningsTime)<9,DATEADD(dd,-1,a.[Last Earnings]),a.[Last Earnings])=b.[Date] AND REPLACE(REPLACE(a.BloombergID,' EQUITY',''),' US','')=b.Ticker
		 WHERE b.Ticker IS NOT NULL", connTodo);

            if (Convert.ToDouble(LastTenDaysSQL.ExecuteScalar()) == 0)
            {

                GridViewRow row00 = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);

                row00.Cells.AddRange(new TableCell[1] { new TableCell { Text = "No Earnings Last Ten Days", HorizontalAlign = HorizontalAlign.Left, ColumnSpan=6}
                    });
                row00.Style["border-top"] = "1px solid black";
                row00.Style["border-bottom"] = "1px solid black";
                row00.Style["border-left"] = "1px solid black";
                row00.Style["border-right"] = "1px solid black";

                GridViewRow row0 = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row0.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
                row0.Style["font-weight"] = "bold";

                row0.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=04/26/2018&EndDate=12/31/2025'>Overall</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", AllTimeAvgWin), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -AllTimeAvgLoss), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallWins+WeeklyWins + "-" + (Convert.ToDouble(OverallLosses)+Convert.ToDouble(WeeklyLosses))), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallPct), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", (AllTimeAvgWin*(OverallWins+WeeklyWins)*OverallPct)+(AllTimeAvgLoss*(Convert.ToDouble(OverallLosses)+Convert.ToDouble(WeeklyLosses))*(1-OverallPct))), HorizontalAlign = HorizontalAlign.Right},
                    });
                row0.Style["border-left"] = "1px solid black";
                row0.Style["border-right"] = "1px solid black";
                row0.Style["white-space"] = "nowrap";

                GridViewRow row = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
                row.Style["font-weight"] = "bold";
                row.Style["border-bottom"] = "1px solid black";

                GridViewRow rowL = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                // rowL.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");

                GridViewRow rowS = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                rowS.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");
                rowS.Style["border-bottom"] = "1px solid black";

                if (QuarterPct == 1000)
                {
                    rowS.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Short</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Short+WeeklyWins_Short + "-" + (QuarterLosses_Short+WeeklyLosses_Short)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "1.000", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Short), HorizontalAlign = HorizontalAlign.Right}
                    });
                    rowL.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Long</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Long+WeeklyWins_Long + "-" + (QuarterLosses_Long+WeeklyLosses_Long)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "1.000", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Long), HorizontalAlign = HorizontalAlign.Right}
                    });

                    row.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Earnings Seasonc</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins+WeeklyWins + "-" + (QuarterLosses+WeeklyLosses)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "1.000", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin), HorizontalAlign = HorizontalAlign.Right}
                    });
                }
                else
                {
                    rowS.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Short</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Short), HorizontalAlign = HorizontalAlign.Center},

                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Short + "-" + (QuarterLosses_Short)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Short+WeeklyWins_Short + "-" + (QuarterLosses_Short+WeeklyLosses_Short)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterPct_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (SeasonAvgWin_Short*QuarterPct_Short)-(SeasonAvgLoss_Short*(1-QuarterPct_Short))), HorizontalAlign = HorizontalAlign.Right}
                    });
                    rowL.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Long</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss_Long), HorizontalAlign = HorizontalAlign.Center},

                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Long+WeeklyWins_Long + "-" + (QuarterLosses_Long+WeeklyLosses_Long)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterPct_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (SeasonAvgWin_Long*QuarterPct_Long)-(SeasonAvgLoss_Long*(1-QuarterPct_Long))), HorizontalAlign = HorizontalAlign.Right}
                    });
                    row.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Earnings Seasond</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins+WeeklyWins + "-" + (QuarterLosses+WeeklyLosses)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterPct), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (SeasonAvgWin*QuarterPct)-(SeasonAvgLoss*(1-QuarterPct))), HorizontalAlign = HorizontalAlign.Right}
                    });
                }

                row.Style["border-left"] = "1px solid black";
                row.Style["border-right"] = "1px solid black";

                GridViewRow row2 = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row2.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
                row2.Style["font-weight"] = "bold";
                row2.Style["border-bottom"] = "1px solid black";

                GridViewRow row2L = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);

                GridViewRow row2S = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row2S.Style["border-bottom"] = "1px solid black";
                row2S.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");

                if (WeeklyPct == 1000)
                {
                    row2S.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Short", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Short==0?0:WinAvg_Short/WinCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Short==0?0:-LossAvg_Short/LossCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Short + "-" + WeeklyLosses_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "-", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:N1}%",WinAvg_Short/WinCount_Short), HorizontalAlign = HorizontalAlign.Center}
                                            });
                    row2L.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Long", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Long==0?0:WinAvg_Long/WinCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Long==0?0:-LossAvg_Long/LossCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Long + "-" + WeeklyLosses_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "-", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:N1}%",WinAvg_Long/WinCount_Long), HorizontalAlign = HorizontalAlign.Center}
                                            });
                    row2.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Last 10 Daysa", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount==0?0:WinAvg/WinCount), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount==0?0:-LossAvg/LossCount), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins + "-" + WeeklyLosses), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "-", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:N1}%",WinAvg/WinCount), HorizontalAlign = HorizontalAlign.Center}
                    });
                }
                else
                {
                    row2S.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Short", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Short==0?0:WinAvg_Short/WinCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Short==0?0:-LossAvg_Short/LossCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Short + "-" + WeeklyLosses_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyPct_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (WinCount_Short==0?0:(WinAvg_Short/WinCount_Short)*WeeklyPct_Short)-(LossCount_Short==0?0:(LossAvg_Short/LossCount_Short)*(1-WeeklyPct_Short))), HorizontalAlign = HorizontalAlign.Right}
                                            });
                    row2L.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Long", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Long==0?0:WinAvg_Long/WinCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Long==0?0:-LossAvg_Long/LossCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Long + "-" + WeeklyLosses_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyPct_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (WinCount_Long==0?0:(WinAvg_Long/WinCount_Long)*WeeklyPct_Long)-(LossCount_Long==0?0:(LossAvg_Long/LossCount_Long)*(1-WeeklyPct_Long))), HorizontalAlign = HorizontalAlign.Right}
                                            });
                    row2.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Last 10 Daysn", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount==0?0:WinAvg/WinCount), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount==0?0:Math.Abs(LossAvg/LossCount)*-1), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins + "-" + WeeklyLosses), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyPct), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (WinCount==0?0:(WinAvg/WinCount)*WeeklyPct)-(LossCount==0?0:(LossAvg/LossCount)*(1-WeeklyPct))), HorizontalAlign = HorizontalAlign.Right}
                    });
                }




                row2.Style["border-left"] = "1px solid black";
                row2.Style["border-right"] = "1px solid black";

                GridViewRow row03 = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row03.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
                row03.Style["font-weight"] = "bold";

                row03.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Summary Stats", HorizontalAlign = HorizontalAlign.Left},
                                             new TableCell { Text = "Avg W", HorizontalAlign = HorizontalAlign.Center},
                                             new TableCell { Text = "Avg L", HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = "W/L", HorizontalAlign = HorizontalAlign.Right},
                                             new TableCell { Text = "Win%", HorizontalAlign = HorizontalAlign.Right},
                                             new TableCell { Text = "Exp\u25b3", HorizontalAlign = HorizontalAlign.Right}
                    });



                row03.Style["border-top"] = "1px solid black";
                row03.Style["border-bottom"] = "1px solid black";
                row03.Style["border-left"] = "1px solid black";
                row03.Style["border-right"] = "1px solid black";

                GridViewRow row0O = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row0O.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
                row0O.Style["font-weight"] = "bold";

                row0O.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx'>Overall</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", AllTimeAvgWin), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -AllTimeAvgLoss), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallWins+WeeklyWins + "-" + (OverallLosses+WeeklyLosses)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallPct), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (AllTimeAvgWin*OverallPct)-(AllTimeAvgLoss*(1-OverallPct))), HorizontalAlign = HorizontalAlign.Right}
                    });


                row0O.Style["border-left"] = "1px solid black";
                row0O.Style["border-right"] = "1px solid black";

                GridViewRow row0S = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row0S.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");

                row0S.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx'>Short</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", AllTimeAvgWin_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -AllTimeAvgLoss_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallWins_Short+WeeklyWins_Short + "-" + (OverallLosses_Short+WeeklyLosses_Short)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallPct_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (AllTimeAvgWin_Short*OverallPct_Short)-(AllTimeAvgLoss_Short*(1-OverallPct_Short))), HorizontalAlign = HorizontalAlign.Right}
                    });


                row0S.Style["border-left"] = "1px solid black";
                row0S.Style["border-right"] = "1px solid black";
                row0S.Style["border-bottom"] = "1px solid black";

                GridViewRow row0L = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
                row0L.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx'>Long</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", AllTimeAvgWin_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -AllTimeAvgLoss_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallWins_Long+WeeklyWins_Long + "-" + (OverallLosses_Long+WeeklyLosses_Long)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallPct_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (AllTimeAvgWin_Long*OverallPct_Long)-(AllTimeAvgLoss_Long*(1-OverallPct_Long))), HorizontalAlign = HorizontalAlign.Right}
                    });


                row0L.Style["border-left"] = "1px solid black";
                row0L.Style["border-right"] = "1px solid black";


                //add rows
                GridView3.Controls[0].Controls.AddAt(1, row00);
                GridView3.Controls[0].Controls.AddAt(2, row03);
                GridView3.Controls[0].Controls.AddAt(3, rowL);
                GridView3.Controls[0].Controls.AddAt(4, rowS);
                GridView3.Controls[0].Controls.AddAt(5, row);
                GridView3.Controls[0].Controls.AddAt(6, row0L);
                GridView3.Controls[0].Controls.AddAt(7, row0S);
                GridView3.Controls[0].Controls.AddAt(8, row0O);

                connTodo.Close();

            }
        }
        try
        {
            if (e.Row.Cells[2].Text == "00:00:00")
            {
                e.Row.Cells[2].Text = "TBA";
            }
        }
        catch { }
        if (e.Row.RowType == DataControlRowType.Footer)
        {
            e.Row.Style["font-weight"] = "bold";

            GridViewRow row0 = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            row0.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
            row0.Style["font-weight"] = "bold";

            row0.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx'>Overall</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", AllTimeAvgWin), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -AllTimeAvgLoss), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallWins+WeeklyWins + "-" + (OverallLosses+WeeklyLosses)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallPct), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (AllTimeAvgWin*OverallPct)-(AllTimeAvgLoss*(1-OverallPct))), HorizontalAlign = HorizontalAlign.Right}
                    });

            GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row0);
            row0.Style["border-left"] = "1px solid black";
            row0.Style["border-right"] = "1px solid black";

            GridViewRow row0S = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            row0S.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");

            row0S.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx'>Short</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", AllTimeAvgWin_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -AllTimeAvgLoss_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallWins_Short+WeeklyWins_Short + "-" + (OverallLosses_Short+WeeklyLosses_Short)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallPct_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (AllTimeAvgWin_Short*OverallPct_Short)-(AllTimeAvgLoss_Short*(1-OverallPct_Short))), HorizontalAlign = HorizontalAlign.Right}
                    });

            GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row0S);
            row0S.Style["border-left"] = "1px solid black";
            row0S.Style["border-right"] = "1px solid black";
            row0S.Style["border-bottom"] = "1px solid black";

            GridViewRow row0L = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            row0L.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx'>Long</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", AllTimeAvgWin_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -AllTimeAvgLoss_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallWins_Long+WeeklyWins_Long + "-" + (OverallLosses_Long+WeeklyLosses_Long)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", OverallPct_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (AllTimeAvgWin_Long*OverallPct_Long)-(AllTimeAvgLoss_Long*(1-OverallPct_Long))), HorizontalAlign = HorizontalAlign.Right}
                    });

            GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row0L);
            row0L.Style["border-left"] = "1px solid black";
            row0L.Style["border-right"] = "1px solid black";



            GridViewRow row = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            row.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
            row.Style["font-weight"] = "bold";
            row.Style["border-bottom"] = "1px solid black";

            GridViewRow rowL = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            // rowL.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");

            GridViewRow rowS = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            rowS.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");
            rowS.Style["border-bottom"] = "1px solid black";

            if (QuarterPct == 1000)
            {
                rowS.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Short</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", 0), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "0", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Right}
                    });
                rowL.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Long</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", 0), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "0", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Right}
                    });

                row.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Earnings Season</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", 0), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "0", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", 0), HorizontalAlign = HorizontalAlign.Right}
                    });
                /*
                rowS.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Short</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Short+WeeklyWins_Short + "-" + (QuarterLosses_Short+WeeklyLosses_Short)), HorizontalAlign = HorizontalAlign.Right},
                                            
                                            new TableCell { Text = "1.000", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Short), HorizontalAlign = HorizontalAlign.Right}
                    });
                rowL.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Long</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Long+WeeklyWins_Long + "-" + (QuarterLosses_Long+WeeklyLosses_Long)), HorizontalAlign = HorizontalAlign.Right},
                                            
                                            new TableCell { Text = "1.000", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Long), HorizontalAlign = HorizontalAlign.Right}
                    });
                row.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Earnings Seasonz</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins+WeeklyWins + "-" + (QuarterLosses+WeeklyLosses)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "1.000", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin), HorizontalAlign = HorizontalAlign.Right}
                    });
                    */
            }
            else
            {
                rowS.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Short</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Short+WeeklyWins_Short + "-" + (QuarterLosses_Short+WeeklyLosses_Short)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterPct_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (SeasonAvgWin_Short*QuarterPct_Short)-(SeasonAvgLoss_Short*(1-QuarterPct_Short))), HorizontalAlign = HorizontalAlign.Right}
                    });
                rowL.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Long</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin_Long==0?WinCount==0?0:WinAvg/WinCount:SeasonAvgWin_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins_Long+WeeklyWins_Long + "-" + (QuarterLosses_Long+WeeklyLosses_Long)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterPct_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", ((SeasonAvgWin_Long==0?WinCount==0?0:WinAvg/WinCount:SeasonAvgWin_Long)*QuarterPct_Long)-(SeasonAvgLoss_Long*(1-QuarterPct_Long))), HorizontalAlign = HorizontalAlign.Right}
                                            //new TableCell { Text = String.Format("{0:#,##0.0}%", (SeasonAvgWin_Long==0?WinCount==0?0:WinAvg/WinCount:SeasonAvgWin_Long*QuarterPct_Long)-(SeasonAvgLoss_Long*(1-QuarterPct_Long))), HorizontalAlign = HorizontalAlign.Right}
                    });
                row.Cells.AddRange(new TableCell[6] { new TableCell { Text = "<a href='Analytics/EarningsLSReport.aspx?StartDate=1/24/2021&EndDate=12/31/2021'>Earnings Season</a>", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", SeasonAvgWin), HorizontalAlign = HorizontalAlign.Center},

                                            new TableCell { Text = String.Format("{0:#,##0.0}%", -SeasonAvgLoss), HorizontalAlign = HorizontalAlign.Center},
                                            //wrong
                                            //new TableCell { Text = String.Format("{0:#,##0.0}%", -(SeasonAvgLoss_Short+SeasonAvgLoss_Long)/2), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterWins+WeeklyWins + "-" + (QuarterLosses+WeeklyLosses)), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", QuarterPct), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (SeasonAvgWin*QuarterPct)-(SeasonAvgLoss*(1-QuarterPct))), HorizontalAlign = HorizontalAlign.Right}
                                            // dividede by 2 is wrong
                                            //new TableCell { Text = String.Format("{0:#,##0.0}%", ((SeasonAvgWin_Short*QuarterPct_Short)-(SeasonAvgLoss_Short*(1-QuarterPct_Short))+(SeasonAvgWin_Long==0?WinCount==0?0:WinAvg/WinCount:SeasonAvgWin_Long*QuarterPct_Long)-(SeasonAvgLoss_Long*(1-QuarterPct_Long)))/2), HorizontalAlign = HorizontalAlign.Right}
                    });
            }


            GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row);
            GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, rowS);
            GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, rowL);

            row.Style["border-left"] = "1px solid black";
            row.Style["border-right"] = "1px solid black";

            GridViewRow row2 = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            row2.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
            row2.Style["font-weight"] = "bold";
            row2.Style["border-bottom"] = "1px solid black";

            GridViewRow row2L = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);

            GridViewRow row2S = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            row2S.Style["border-bottom"] = "1px solid black";
            row2S.BackColor = System.Drawing.ColorTranslator.FromHtml("#f4f4f4");

            if (WeeklyPct == 1000)
            {
                row2S.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Short", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Short==0?0:WinAvg_Short/WinCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Short==0?0:-LossAvg_Short/LossCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Short + "-" + WeeklyLosses_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "-", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:N1}%",WinAvg_Short/WinCount_Short), HorizontalAlign = HorizontalAlign.Center}
                                            });
                row2L.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Long", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Long==0?0:WinAvg_Long/WinCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Long==0?0:-LossAvg_Long/LossCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Long + "-" + WeeklyLosses_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "-", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:N1}%",WinAvg_Long/WinCount_Long), HorizontalAlign = HorizontalAlign.Center}
                                            });
                row2.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Last 10 Daysm", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount==0?0:WinAvg/WinCount), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount==0?0:-LossAvg/LossCount), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins + "-" + WeeklyLosses), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = "-", HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:N1}%",WinAvg/WinCount), HorizontalAlign = HorizontalAlign.Center}
                    });
            }
            else
            {
                row2S.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Short", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Short==0?0:WinAvg_Short/WinCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Short==0?0:-LossAvg_Short/LossCount_Short), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Short + "-" + WeeklyLosses_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyPct_Short), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (WinCount_Short==0?0:(WinAvg_Short/WinCount_Short)*WeeklyPct_Short)-(LossCount_Short==0?0:(LossAvg_Short/LossCount_Short)*(1-WeeklyPct_Short))), HorizontalAlign = HorizontalAlign.Right}
                                            });
                row2L.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Long", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount_Long==0?0:WinAvg_Long/WinCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount_Long==0?0:-LossAvg_Long/LossCount_Long), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins_Long + "-" + WeeklyLosses_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyPct_Long), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (WinCount_Long==0?0:(WinAvg_Long/WinCount_Long)*WeeklyPct_Long)-(LossCount_Long==0?0:(LossAvg_Long/LossCount_Long)*(1-WeeklyPct_Long))), HorizontalAlign = HorizontalAlign.Right}
                                            });
                row2.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Last 10 Days", HorizontalAlign = HorizontalAlign.Left},
                                            new TableCell { Text = String.Format("{0:N1}%",WinCount==0?0:WinAvg/WinCount), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:N1}%",LossCount==0?0:Math.Abs(LossAvg/LossCount)*-1), HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyWins + "-" + WeeklyLosses), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.000}", WeeklyPct), HorizontalAlign = HorizontalAlign.Right},
                                            new TableCell { Text = String.Format("{0:#,##0.0}%", (WinCount==0?0:(WinAvg/WinCount)*WeeklyPct)-(LossCount==0?0:(LossAvg/LossCount)*(1-WeeklyPct))), HorizontalAlign = HorizontalAlign.Right}
                    });
            }

            if (WeeklyWins_Long + WeeklyLosses_Long + WeeklyWins_Short + WeeklyLosses_Short > 0)
            {
                GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row2);
                GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row2S);
                GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row2L);

                row2.Style["border-left"] = "1px solid black";
                row2.Style["border-right"] = "1px solid black";
            }
            GridViewRow row03 = new GridViewRow(0, 0, DataControlRowType.DataRow, DataControlRowState.Normal);
            row03.BackColor = System.Drawing.ColorTranslator.FromHtml("#dbdbdb");
            row03.Style["font-weight"] = "bold";

            row03.Cells.AddRange(new TableCell[6] { new TableCell { Text = "Summary Stats", HorizontalAlign = HorizontalAlign.Left},
                                             new TableCell { Text = "Avg W", HorizontalAlign = HorizontalAlign.Center},
                                             new TableCell { Text = "Avg L", HorizontalAlign = HorizontalAlign.Center},
                                            new TableCell { Text = "W/L", HorizontalAlign = HorizontalAlign.Right},
                                             new TableCell { Text = "Win%", HorizontalAlign = HorizontalAlign.Right},
                                             new TableCell { Text = "Exp\u25b3", HorizontalAlign = HorizontalAlign.Right}
                    });

            GridView3.Controls[0].Controls.AddAt(GridView3.Rows.Count + 1, row03);

            row03.Style["border-top"] = "1px solid black";
            row03.Style["border-bottom"] = "1px solid black";
            row03.Style["border-left"] = "1px solid black";
            row03.Style["border-right"] = "1px solid black";

        }
    }
    private void ShowNoResultFound()
    {
    }

    public int PastLine = 0;
    protected void EarningsDataBound4(object sender, GridViewRowEventArgs e)
    {
        try
        {
            e.Row.Cells[5].Visible = false;
            e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            if (e.Row.RowType == DataControlRowType.Header)
            {
                e.Row.Style["border-bottom"] = "1px solid black";
            }
        }
        catch { }
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            try
            {
                if (PastLine == 0 && Convert.ToDateTime(e.Row.Cells[1].Text) > DateTime.Now)
                {

                    e.Row.Style["border-top"] = "1px solid black";
                    PastLine = 1;
                }
            }
            catch { }
            try
            {
                if (e.Row.Cells[6].Text == "0.0")
                {
                    e.Row.Cells[6].Text = "";
                }
                if (Convert.ToDouble(e.Row.Cells[6].Text) > 0)
                {
                    e.Row.Cells[6].ForeColor = System.Drawing.Color.Blue;
                    e.Row.Cells[6].Text += "%";
                }
                if (Convert.ToDouble(e.Row.Cells[6].Text) < 0)
                {
                    e.Row.Cells[6].ForeColor = System.Drawing.Color.Red;
                    e.Row.Cells[6].Text += "%";
                }
            }
            catch { }
            double myNum = 0;
            try
            {
                if (Double.TryParse(e.Row.Cells[2].Text, out myNum))
                {

                }
                else
                {
                    e.Row.Cells[2].Text = "-";
                }
            }
            catch { }
            try
            {
                e.Row.Cells[2].Text = String.Format("{0:#,##0}", Convert.ToDouble(e.Row.Cells[2].Text));
            }
            catch { }
            try
            {
                e.Row.Cells[3].Text = String.Format("{0:p0}", Convert.ToDouble(e.Row.Cells[2].Text) / (1000000 * Convert.ToDouble(e.Row.Cells[3].Text)));
            }
            catch { e.Row.Cells[3].Text = "-"; }
            try
            {
                e.Row.Cells[4].Text = String.Format("{0:p0}", Convert.ToDouble(e.Row.Cells[2].Text) / (1000000 * Convert.ToDouble(e.Row.Cells[4].Text)));
            }
            catch { e.Row.Cells[4].Text = "-"; }

            if (e.Row.Cells[5].Text == "0")
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                ((HyperLink)e.Row.Cells[0].Controls[0]).Text = textInfo.ToTitleCase(((HyperLink)e.Row.Cells[0].Controls[0]).Text.ToLower()) + "<i class=\"far fa-lightbulb fa-fw\"></i>";
            }
        }
    }
    public string GlenNameStorage;
    protected void EarningsDataBound5(object sender, GridViewRowEventArgs e)
    {
        try
        {
            e.Row.Cells[5].Visible = false;
            e.Row.Cells[6].Visible = false;
            e.Row.Cells[7].Visible = false;
            e.Row.Cells[8].Visible = false;
            e.Row.Cells[9].Visible = false;
            e.Row.Cells[1].HorizontalAlign = HorizontalAlign.Center;
            e.Row.Cells[2].HorizontalAlign = HorizontalAlign.Center;
            e.Row.Cells[3].HorizontalAlign = HorizontalAlign.Center;
            e.Row.Cells[4].HorizontalAlign = HorizontalAlign.Center;
            e.Row.Cells[5].HorizontalAlign = HorizontalAlign.Center;
        }
        catch { }
        if (e.Row.RowType == DataControlRowType.Header)
        {
            e.Row.Style["border-bottom"] = "1px solid black";
            e.Row.Cells[0].Text = "";
        }
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[1].Text = string.Format("{0:M/d/yyyy}", Convert.ToDateTime(e.Row.Cells[1].Text));
            if (!string.IsNullOrEmpty(e.Row.Cells[6].Text))
            {
                object[] findTheseVals2 = new object[1];
                findTheseVals2[0] = e.Row.Cells[0].Text;
                DataRow foundrow2 = dt2001.Rows.Find(findTheseVals2);
                try
                {
                    if (!string.IsNullOrEmpty(foundrow2[0].ToString()))
                    {

                        try
                        {
                            DateTime call_time = new DateTime(); // Creating the object of DateTime;
                            call_time = Convert.ToDateTime(e.Row.Cells[3].Text);
                            e.Row.Cells[3].Text = "<a target='_blank' href='GoToIRSite.aspx?CompanyName=" + e.Row.Cells[0].Text + "'>" + call_time.ToShortTimeString() + "</a>";
                        }
                        catch
                        {
                            e.Row.Cells[3].Text = "<a target='_blank' href='GoToIRSite.aspx?CompanyName=" + e.Row.Cells[0].Text + "'>" + e.Row.Cells[3].Text + "</a>";
                        }
                    }
                    GlenNameStorage = e.Row.Cells[0].Text;
                    e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[5].Text.Replace(" US", "") + "'>" + e.Row.Cells[0].Text + "</a>";
                    if (!string.IsNullOrEmpty(e.Row.Cells[6].Text))
                    {
                        e.Row.Cells[0].Text += "<img src='../../Images/transparent.png' width='6px'><a target='_blank' href='GoToIRSite.aspx?CompanyName=" + GlenNameStorage + "'><i class=\"far fa-phone fa-sm fa-fw\"></i></a>";
                    }
                }
                catch
                {
                    try
                    {
                        e.Row.Cells[0].Text = "<a href='LSPM/LSPMScenarioManagement.aspx?Ticker=" + e.Row.Cells[5].Text.Replace(" US", "") + "'>" + e.Row.Cells[0].Text + "</a>";
                        DateTime call_time = new DateTime(); // Creating the object of DateTime;
                        call_time = Convert.ToDateTime(e.Row.Cells[3].Text);
                        e.Row.Cells[3].Text = call_time.ToShortTimeString();
                    }
                    catch
                    {
                        
                    }
                }
            }
        }
    }
    public string TitleStore;
    public string[] TickerSplitter;
    public string LinkRef;
    protected void NewsDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            e.Row.Cells[8].Visible = false;
            e.Row.Cells[9].Visible = false;
            e.Row.Cells[10].Visible = false;

            e.Row.Cells[2].Style["white-space"] = "nowrap";
            e.Row.Cells[5].Style["white-space"] = "nowrap";
            e.Row.Cells[6].Style["white-space"] = "nowrap";
            e.Row.Cells[7].Style["white-space"] = "nowrap";
            if (Request.Browser.IsMobileDevice)
            {
                e.Row.Cells[2].Visible = false;
            }
        }
        catch { }

        if (e.Row.RowType == DataControlRowType.Header)
        {
            if (CurrentUser == "glen" || CurrentUser == "dan" || CurrentUser == "mark" || CurrentUser == "mike" || CurrentUser == "ross" || CurrentUser == "richard" || CurrentUser == "gonzalo" || CurrentUser == "" || CurrentUser == "blake" || CurrentUser == "molly")
            {
                e.Row.Cells[0].Visible = true;
            }
            else
            {
                e.Row.Cells[0].Visible = false;
                e.Row.Cells[4].Visible = false;
            }
            e.Row.Style["border-bottom"] = "thin solid black";
            e.Row.Cells[1].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[2].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[3].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[5].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[6].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[7].HorizontalAlign = HorizontalAlign.Right;

            e.Row.Cells[7].Style.Add("padding-left", "28pt");
            e.Row.Cells[7].Style["border-right"] = "thin solid black";
            e.Row.Cells[7].Style["border-bottom"] = "thin solid black";

        }
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            /*
             * Hide certain rows if none of the tickers are not in the current position list.
             * Previously this was done in SQL using CROSS APPLY but it is a LOT slower 4x...
             * news_source_databound_skip has a list that we don't process, i.e. we always show those sources. 
             * SQL view [dbo].[vw_defaultaspx_sqldatasource1] needs to include these rows as well as news_source_databound_skip.
             */
            if (!news_source_databound_skip.Contains(e.Row.Cells[1].Text))
            {
                //without the regex below, the ticker 'LI,  NIO,  TSLA,  XPEV' won't get properly processed
                var ticker_field = System.Text.RegularExpressions.Regex.Replace(e.Row.Cells[10].Text, @",\s+", ",");
                List<string> tickers = ticker_field.Split(',').ToList();    //e.Row.Cells[10].Text.Split(',').ToList();
                List<string> common = tickers.Intersect(portfolio_tickers).ToList();
                if (common.Count == 0)
                    e.Row.Visible = false;
            }

            try
            {
                if ((e.Row.RowState == DataControlRowState.Edit) || (e.Row.RowState == (DataControlRowState.Edit | DataControlRowState.Alternate)))
                {
                    e.Row.Attributes.Add("onkeypress", @"javascript:if (event.keyCode == 13) { __doPostBack('" + GridView1.UniqueID + @"', 'Update$" + e.Row.RowIndex.ToString() + "'); return false; }");
                }
            }
            catch { }
            try
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                e.Row.Cells[6].Text = textInfo.ToTitleCase(e.Row.Cells[6].Text.ToLower());
            }
            catch { }
            try
            {
                if (Request.Browser.IsMobileDevice)
                {
                    //e.Row.Cells[7].Text = string.Format("{0:MM/dd/yy}", Convert.ToDateTime(e.Row.Cells[7].Text));   
                    e.Row.Cells[7].Style["white-space"] = "nowrap";
                }
            }
            catch { }
            if (CurrentUser == "glen" || CurrentUser == "dan" || CurrentUser == "mark" || CurrentUser == "mike" || CurrentUser == "ross" || CurrentUser == "richard" || CurrentUser == "gonzalo" || CurrentUser == "" || CurrentUser == "blake" || CurrentUser == "molly")
            {
                e.Row.Cells[0].Visible = true;
            }
            else
            {
                e.Row.Cells[0].Visible = false;
                e.Row.Cells[4].Visible = false;
            }
            //if (e.Row.Cells[2].Text != "Market News" && e.Row.Cells[2].Text != "Light Street Notes")
            //{
            //    //e.Row.Cells[4].Text = "";
            //}
            if (e.Row.Cells[2].Text.ToLower() == "industry panel notes")
            {
                e.Row.Cells[2].Text = "Industry Panel";
            }
            e.Row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
            e.Row.Cells[7].Style["border-right"] = "thin solid black";
            e.Row.Cells[1].Style["white-space"] = "nowrap";

            try
            {
                HyperLink hp = new HyperLink();
                hp.Text = e.Row.Cells[3].Text;
                //to do: May need this update
                //hp.Text = e.Row.Cells[3].Text.Replace("www.precisiononcologynews.com", "www.precisiononcologynews.com/");

                if (e.Row.Cells[1].Text == "Bloomberg")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bloomberg.png?r=1' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bloomberg.png?r=1' width=""16"" /> Bloomberg";
                    }
                    e.Row.Cells[1].ToolTip = "Bloomberg";
                    //hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[9].Text) + "&Source=&Timestamp=" + e.Row.Cells[7].Text;
                    //string headline = e.Row.Cells[9].Text.Replace("&amp;", "");
                    //headline = headline.Replace("&#160;", " ");
                    //hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + headline + "&Source=&Timestamp=" + e.Row.Cells[7].Text;
                    if (e.Row.Cells[3].Text == "SaaS Multiples")
                    {
                        hp.NavigateUrl = "ResearchSupport/SaaSMetrics.aspx";
                    }
                    else
                    {
                        //dnw hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + hp.Text + "&Source=&Timestamp=" + e.Row.Cells[7].Text;
                        //dnw hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + hp.Text.Replace("&", "&amp;") + "&Source=&Timestamp=" + e.Row.Cells[7].Text;
                        hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(hp.Text) + "&Source=&Timestamp=" + e.Row.Cells[7].Text;
                    }
                }
                if (e.Row.Cells[1].Text == "Visible Alpha")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/visiblealpha.jpg?r=1' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/visiblealpha.jpg?r=1' width=""16"" /> Visible Alpha";
                    }
                    e.Row.Cells[1].ToolTip = "Visible Alpha";
                    hp.NavigateUrl = "ResearchSupport/" + e.Row.Cells[8].Text;
                }
                if (e.Row.Cells[1].Text == "Street Account")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/streetaccount.png?r=1' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/streetaccount.png?r=1' width=""16"" /> Street Account";
                    }
                    e.Row.Cells[1].ToolTip = "Street Account";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[9].Text) + "&Source=&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "Company RSS")
                {

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='https://lscportal.blob.core.windows.net/stockpictures/" + ((Label)e.Row.FindControl("TickerLabel")).Text + ".png?sv=2017-07-29&ss=bfqt&srt=sco&sp=rwdlacup&se=2030-12-31T12:15:54Z&st=2018-03-07T04:15:54Z&spr=https&sig=uYjqEXSdJjZvVPcgwpAg6jCZUkEZ7GxUTL%2FIETybDKU%3D' width='16' />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='https://lscportal.blob.core.windows.net/stockpictures/" + ((Label)e.Row.FindControl("TickerLabel")).Text + ".png?sv=2017-07-29&ss=bfqt&srt=sco&sp=rwdlacup&se=2030-12-31T12:15:54Z&st=2018-03-07T04:15:54Z&spr=https&sig=uYjqEXSdJjZvVPcgwpAg6jCZUkEZ7GxUTL%2FIETybDKU%3D' width='16' /> Company RSS";
                    }
                    e.Row.Cells[1].ToolTip = "Company RSS";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[9].Text) + "&Source=&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "Blue Lotus")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bluelotus.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bluelotus.png' width=""16"" /> Blue Lotus";
                    }
                    e.Row.Cells[1].ToolTip = "Blue Lotus";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[9].Text) + "&Source=Blue Lotus&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "86Research")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/eightysixresearch.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/eightysixresearch.png' width=""16"" /> 86Research";
                    }
                    e.Row.Cells[1].ToolTip = "86Research";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Source=86Research&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "Evernote")
                {
                    if (Request.Browser.IsMobileDevice)
                    {

                        e.Row.Cells[1].Text = @"<img src='../Images/evernotelogo.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/evernotelogo.jpg' width=""16"" /> Evernote";
                    }
                    e.Row.Cells[3].Text = Server.HtmlDecode(e.Row.Cells[3].Text);
                }
                if (e.Row.Cells[1].Text == "Tegus")
                {
                    if (Request.Browser.IsMobileDevice)
                    {

                        e.Row.Cells[1].Text = @"<img src='../Images/tegus_logo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/tegus_logo.png' width=""16"" /> Tegus";
                    }
                    e.Row.Cells[1].ToolTip = "Tegus";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[4].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Source=Tegus&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "YipitData")
                {
                    if (Request.Browser.IsMobileDevice)
                    {

                        e.Row.Cells[1].Text = @"<img src='../Images/yipit.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/yipit.png' width=""16"" /> YipitData";
                    }
                    e.Row.Cells[1].ToolTip = "YipitData";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Source=YipitData&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "Vertical Knowledge")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/vk_logo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/vk_logo.png' width=""16"" /> Vertical Knowledge";
                    }
                    e.Row.Cells[1].ToolTip = "Vertical Knowledge";
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                }
                if (e.Row.Cells[1].Text == "Liftr")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/liftr.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/liftr.png' width=""16"" /> Liftr";
                    }
                    e.Row.Cells[1].ToolTip = "Liftr";
                    hp.NavigateUrl = "ResearchSupport/Underlying" + e.Row.Cells[8].Text;
                }
                if (e.Row.Cells[1].Text == "Edgewater")
                {
                    if (Request.Browser.IsMobileDevice)
                    {

                        e.Row.Cells[1].Text = @"<img src='../Images/edgewater.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/edgewater.png' width=""16"" /> Edgewater";
                    }
                    e.Row.Cells[1].ToolTip = "Edgewater";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[4].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Source=Edgewater&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "GfK")
                {
                    if (Request.Browser.IsMobileDevice)
                    {

                        e.Row.Cells[1].Text = @"<img src='../Images/gfk.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/gfk.jpg' width=""16"" /> GfK";
                    }
                    e.Row.Cells[1].ToolTip = "GfK";
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[4].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Source=GfK&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "Goldman Sachs")
                {
                    if (Request.Browser.IsMobileDevice)
                    {

                        e.Row.Cells[1].Text = @"<img src='../Images/gs-logo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/gs-logo.png' width=""16"" /> Goldman Sachs";
                    }
                    e.Row.Cells[1].ToolTip = "Goldman Sachs";
                    hp.NavigateUrl = "ResearchSupport" + e.Row.Cells[8].Text;
                }
                if (e.Row.Cells[1].Text == "Electrek")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/electrek.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/electrek.png' width=""16"" /> Electrek";
                    }
                    e.Row.Cells[1].ToolTip = "Electrek";

                    e.Row.Cells[3].Text = Server.HtmlDecode(e.Row.Cells[3].Text);
                }
                if (e.Row.Cells[1].Text == "AppleInsider")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/appleinsider.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/appleinsider.jpg' width=""16"" /> Apple Insider";
                    }
                    e.Row.Cells[1].ToolTip = "AppleInsider";

                    e.Row.Cells[3].Text = Server.HtmlDecode(e.Row.Cells[3].Text);

                }
                if (e.Row.Cells[1].Text == "9to5Mac")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/9to5mac.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/9to5mac.jpg' width=""16"" /> 9to5 Mac";
                    }
                    e.Row.Cells[1].ToolTip = "9to5Mac";
                }
                if (e.Row.Cells[1].Text == "MacRumors")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/macrumors.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/macrumors.jpg' width=""16"" /> MacRumors";
                    }
                    e.Row.Cells[1].ToolTip = "MacRumors";
                }
                if (e.Row.Cells[1].Text == "LSC")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS logo box.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS logo box.jpg' width=""16"" /> LSC";
                    }
                }
                if (e.Row.Cells[1].Text == "The Information")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/theinformation.ico' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/theinformation.ico' width=""16"" /> The Information";
                    }
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[4].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Source=The Information&Timestamp=" + e.Row.Cells[7].Text;
                }
                if (e.Row.Cells[1].Text == "Techcrunch RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/techcrunch.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/techcrunch.jpg' width=""16"" /> Techcrunch";
                    }
                }
                if (e.Row.Cells[1].Text == "Appypie")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/appypie.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/appypie.jpg' width=""16"" /> Appypie";
                    }
                }
                if (e.Row.Cells[1].Text == "Bloomberg RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bloomberg.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bloomberg.png' width=""16"" /> Bloomberg";
                    }
                }
                if (e.Row.Cells[1].Text == "Japan BI RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/japanbi.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/japanbi.png' width=""16"" /> Japan BI";
                    }
                }
                if (e.Row.Cells[1].Text == "WSJ Tech RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/wsjtech.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/wsjtech.png' width=""16"" /> WSJ Tech";
                    }
                }
                if (e.Row.Cells[1].Text == "FT Tech RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/financialtimeslogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/financialtimeslogo.png' width=""16"" /> FT Tech";
                    }
                }
                if (e.Row.Cells[1].Text == "NYT Tech RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/newyorktimeslogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/newyorktimeslogo.png' width=""16"" /> NYT Tech";
                    }
                }
                if (e.Row.Cells[1].Text == "Barrons Tech RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/barronstech.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/barronstech.png' width=""16"" /> Barron's Tech";
                    }
                }
                if (e.Row.Cells[1].Text == "Economist RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/economistlogo.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/economistlogo.jpg' width=""16"" /> Economist";
                    }
                }
                if (e.Row.Cells[1].Text == "Fierce Biotech RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/fiercebiotech.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/fiercebiotech.jpg' width=""16"" /> Fierce Biotech";
                    }
                }
                if (e.Row.Cells[1].Text == "Precision Oncology RSS")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/precision.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/precision.png' width=""16"" /> Precision Oncology";
                    }
                }
                if (e.Row.Cells[1].Text == "Stratechery")
                {
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/Stratechery.ico' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/Stratechery.ico' width=""16"" /> Stratechery";
                    }
                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[4].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Source=Stratechery&Timestamp=" + e.Row.Cells[7].Text;
                }

                if (e.Row.Cells[1].Text == "Citron Research")
                {
                    if (e.Row.Cells[3].Text.Length > 250)
                    {
                        e.Row.Cells[3].Text = Server.HtmlDecode(e.Row.Cells[3].Text.Substring(0, 250) + "</a>");
                    }
                    else
                    {
                        e.Row.Cells[3].Text = Server.HtmlDecode(e.Row.Cells[3].Text);
                    }

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/citronresearchlogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/citronresearchlogo.png' width=""16"" /> Citron Research";
                    }
                }
                if (e.Row.Cells[1].Text == "RSS")
                {
                    e.Row.Cells[3].Text = Server.HtmlDecode(e.Row.Cells[3].Text);


                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='https://lscportal.blob.core.windows.net/stockpictures/" + ((Label)e.Row.FindControl("TickerLabel")).Text + ".png?sv=2017-07-29&ss=bfqt&srt=sco&sp=rwdlacup&se=2030-12-31T12:15:54Z&st=2018-03-07T04:15:54Z&spr=https&sig=uYjqEXSdJjZvVPcgwpAg6jCZUkEZ7GxUTL%2FIETybDKU%3D' width='16' />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='https://lscportal.blob.core.windows.net/stockpictures/" + ((Label)e.Row.FindControl("TickerLabel")).Text + ".png?sv=2017-07-29&ss=bfqt&srt=sco&sp=rwdlacup&se=2030-12-31T12:15:54Z&st=2018-03-07T04:15:54Z&spr=https&sig=uYjqEXSdJjZvVPcgwpAg6jCZUkEZ7GxUTL%2FIETybDKU%3D' width='16' /> Company RSS";
                    }
                }
                if (e.Row.Cells[1].Text == "Axios")
                {
                    e.Row.Cells[3].Text = Server.HtmlDecode(e.Row.Cells[3].Text);
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/axioslogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/axioslogo.png' width=""16"" /> Axios";
                    }
                }
                if (e.Row.Cells[1].Text == "Prop Research Team")
                {
                    hp.NavigateUrl = "ResearchSupport/PropResearchUpload.aspx?Timestamp=" + e.Row.Cells[7].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text);
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo Box.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo Box.jpg' width=""16"" /> Prop Research Team";
                    }
                }
                if (e.Row.Cells[1].Text == "Sales Pulse")
                {
                    hp.NavigateUrl = "ResearchSupport/PropResearchUpload.aspx?AltSource=SalesPulse&Timestamp=" + e.Row.Cells[7].Text + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&Timestamp=" + e.Row.Cells[7].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/salespulselogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/salespulselogo.png' width=""16"" /> Sales Pulse";
                    }
                }
                if (e.Row.Cells[1].Text == "1010 Data")
                {
                    hp.NavigateUrl = "ResearchSupport/SpendingDataHome.aspx";
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/1010datalogo.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/1010datalogo.jpg' width=""16"" /> <a href='ResearchSupport/SpendingDataHome.aspx'>1010 Data</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "Second Measure")
                {
                    hp.NavigateUrl = "ResearchSupport/SpendingDataHome.aspx";
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/secondmeasurelogo.ico' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/secondmeasurelogo.ico' width=""16"" /> <a href='ResearchSupport/SpendingDataHome.aspx'>Second Measure</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "BuiltWith")
                {
                    hp.NavigateUrl = "ResearchSupport/BuiltWitheCommerce.aspx";

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/BuiltWith.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/BuiltWith.png' width=""16"" /> BuiltWith";
                    }
                }
                if (e.Row.Cells[1].Text == "USASpending.Gov")
                {
                    hp.NavigateUrl = "ResearchSupport/GovernmentSpending.aspx";

                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/usaspendinggov.ico' width=""16"" /> USASpending.Gov";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/usaspendinggov.ico' width=""16"" />";
                    }
                }
                if (e.Row.Cells[1].Text == "Glassdoor")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/glassdoorlogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/glassdoorlogo.png' width=""16"" /> <a href='ResearchSupport/ResearchHome.aspx?Filter=Glassdoor'>Glassdoor</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "Third Bridge")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/ThirdBridge.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/ThirdBridge.png' width=""16"" /> <a href='ResearchSupport/ResearchHome.aspx?Filter=Third Bridge'>Third Bridge</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "MKM")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/MKMlogo.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/MKMlogo.jpg' width=""16"" /> <a href='ResearchSupport/ResearchHome.aspx?Filter=MKM'>MKM</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "GLG Surveys")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/glg.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/glg.png' width=""16"" /> <a href='ResearchSupport/ResearchHome.aspx?Filter=GLG%20Surveys'>GLG</a>";
                    }
                }
                if (e.Row.Cells[3].Text == "Geo Spending Data - Ridesharing Services")
                {
                    hp.NavigateUrl = "ResearchSupport/UberLyft.aspx";

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/secondmeasurelogo.ico' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/secondmeasurelogo.ico' width=""16"" /> <a href='ResearchSupport/SpendingDataHome.aspx'>Second Measure</a>";
                    }
                }
                if (e.Row.Cells[3].Text == "Geo Spending Data - Online Food Delivery Services")
                {
                    hp.NavigateUrl = "ResearchSupport/DeliveryServices.aspx";

                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/secondmeasurelogo.ico' width=""16"" /> <a href='ResearchSupport/SpendingDataHome.aspx'>Second Measure</a>";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/secondmeasurelogo.ico' width=""16"" />";
                    }
                }
                if (e.Row.Cells[3].Text == "GPU Industry Dashboard")
                {
                    hp.NavigateUrl = "ResearchSupport/GPUPricingandMarketShare.aspx";

                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" /> Multiple Data Sources";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" />";
                    }
                }
                if (e.Row.Cells[3].Text == "OLED TV Pricing Data")
                {
                    hp.NavigateUrl = "ResearchSupport/OLEDPrices.aspx";

                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" /> Multiple Data Sources";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" />";
                    }
                }
                if (e.Row.Cells[3].Text == "Russian Online Search Market Share")
                {
                    hp.NavigateUrl = "ResearchSupport/Yandex.aspx";

                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" /> Liveinternet.ru";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" />";
                    }
                }
                if (e.Row.Cells[1].Text == "Cleveland Research")
                {
                    hp.NavigateUrl = "ResearchSupport/ClevelandResearch.aspx?Ticker=" + Server.UrlEncode(e.Row.Cells[5].Text.Replace("Source: BFW", "")) + "&Headline=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&TimeStamp=" + e.Row.Cells[7].Text;
                    //   e.Row.Cells[6].Text = "Mike Christensen";
                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/clevelandresearch.jpg' width=""16"" /> Cleveland Research";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/clevelandresearch.jpg' width=""16"" />";
                    }
                }
                if (e.Row.Cells[1].Text == "7Park Data")
                {
                    if (e.Row.Cells[3].Text == "7Park Messaging Dashboard")
                    {
                        hp.NavigateUrl = "ResearchSupport/SevenParkMessaging.aspx";

                    }
                    if (e.Row.Cells[3].Text == "7Park OTA Dashboard")
                    {
                        hp.NavigateUrl = "ResearchSupport/SevenParkOTA.aspx";

                    }
                    if (e.Row.Cells[3].Text == "7Park eCommerce Dashboard")
                    {
                        hp.NavigateUrl = "ResearchSupport/SevenParkeCommerce.aspx";

                    }
                    if (e.Row.Cells[3].Text == "7Park Gaming Dashboard")
                    {
                        hp.NavigateUrl = "ResearchSupport/SevenParkGaming.aspx";

                    }
                    if (e.Row.Cells[3].Text == "7Park CyberAgent Dashboard")
                    {
                        hp.NavigateUrl = "ResearchSupport/SevenParkCyberagent.aspx";

                    }
                    if (e.Row.Cells[3].Text == "7Park Social Dashboard")
                    {
                        hp.NavigateUrl = "ResearchSupport/SevenParkSocial.aspx";

                    }
                    if (e.Row.Cells[3].Text == "7Park Wayfair Spending Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/SpendingData.aspx?Ticker=W&Source=7Park";
                    }

                    if (e.Row.Cells[3].Text == "7Park Live Streaming and Online Dating Dashboard")
                    {
                        hp.NavigateUrl = "ResearchSupport/SevenParkMatch.aspx";

                    }
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/7parklogo.ico' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/7parklogo.ico' width=""16"" /> <a href='ResearchSupport/ResearchHome.aspx?Filter=7Park Data'>7Park Data</a>";
                    }
                    //outlook vba 7park related
                    try
                    {
                        if (e.Row.Cells[3].Text.Substring(0, 14) == "Merchant Intel")
                        {
                            hp.NavigateUrl = e.Row.Cells[8].Text;
                        }
                    }
                    catch { }
                    try
                    {
                        if (e.Row.Cells[8].Text.Substring(0, 24) == "http://ema.7parkdata.com")
                        {
                            hp.NavigateUrl = e.Row.Cells[8].Text;
                        }
                    }
                    catch { }
                }
                if (e.Row.Cells[1].Text == "App Annie")
                {
                    if (e.Row.Cells[3].Text == "App Annie Japan Grossing and Download Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieDailyReportJapan.aspx";

                    }
                    if (e.Row.Cells[3].Text == "SoYoung App Download Trends")
                    {
                        hp.NavigateUrl = "ResearchSupport/SoYoung.aspx";

                    }
                    if (e.Row.Cells[3].Text == "BILI: Fate/Grand Order (China) Rankings")
                    {
                        hp.NavigateUrl = "ResearchSupport/FateGrandOrder_BILI.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie Japan Shopping Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieShopping.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie Japan News/Entertainment Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieNewsEntertainment.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie Japan Messaging Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieMessaging.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie Japan Social Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieSocial.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie Japan Gaming Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieGames.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie Japan Streaming Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieStreaming.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie Japan Match Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnieMatch.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Social Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_Social.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Messaging Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_Messaging.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US eCommerce Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_eCommerce.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Specialty Retail Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_SpecialtyRetail.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Travel Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_Travel.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US TV Streaming Services Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_TVStreaming.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Music Streaming Services Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_MusicStreaming.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Online Dating and Live Streaming Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_Dating.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Quick Service Restaurants Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_QSR.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Online Food Delivery Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_FoodDelivery.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Ride Sharing Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_RideSharing.aspx";

                    }
                    if (e.Row.Cells[3].Text == "App Annie US Business Services Data")
                    {
                        hp.NavigateUrl = "ResearchSupport/AppAnnie_USA_BusinessServices.aspx";

                    }
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/appannie.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/appannie.png' width=""16"" /> <a href='ResearchSupport/ResearchHome.aspx?Filter=App Annie'>App Annie</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "California Phantom Drone")
                {
                    hp.NavigateUrl = "ResearchSupport/TeslaDrone.aspx";
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/calphantdrone.ico' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/calphantdrone.ico' width=""16"" /> California Phantom Drone</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "Opendoor")
                {
                    hp.NavigateUrl = "ResearchSupport/Opendoor.aspx";
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/opendoor.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/opendoor.png' width=""16"" /> Opendoor Home Listings</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "Bespoke")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bespokeicon.ico' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bespokeicon.ico' width=""16"" /> Bespoke</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "LS Prop Research")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../../Images/LS Logo box.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../../Images/LS Logo box.jpg' width=""16"" /> LS Prop Research";
                    }
                }
                if (e.Row.Cells[1].Text == "BWG Strategy")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bwglogo.ico' width=""16"" /> ";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/bwglogo.ico' width=""16"" /> <a href='BWGCalls.aspx'>BWG Strategy</a></a>";
                    }
                }
                if (e.Row.Cells[1].Text == "Google Trends")
                {
                    //hp.NavigateUrl = "ResearchSupport/GoogleTrends.aspx";
                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/googletrends.ico' width=""16"" /> Google Trends</a>";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/googletrends.ico' width=""16"" />";
                    }
                }
                if (e.Row.Cells[1].Text == "Steam" || e.Row.Cells[1].Text.ToLower() == "steam")
                {
                    if (!Request.Browser.IsMobileDevice)
                        e.Row.Cells[1].Text = @"<img src='../../Images/steam.jfif' width=""16"" /> Steam";
                    else
                        e.Row.Cells[1].Text = @"<img src='../../Images/steam.jfif' width=""16"" />";
                }
                //if (e.Row.Cells[1].Text == "Robintrack" || e.Row.Cells[1].Text.ToLower() == "robintrack")
                //{
                //    if (!Request.Browser.IsMobileDevice)
                //        e.Row.Cells[1].Text = @"<img src='../../Images/robintrack.png' width=""16"" /> Robintrack";
                //    else
                //        e.Row.Cells[1].Text = @"<img src='../../Images/robintrack.png' width=""16"" />";
                //}
                if (e.Row.Cells[1].Text == "ETF Channel")
                {
                    hp.NavigateUrl = "ResearchSupport/ETFChannel.aspx";
                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/ETFChannel.ico' width=""16"" /> ETF Channel";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/ETFChannel.ico' width=""16"" />";
                    }
                }
                if (e.Row.Cells[1].Text == "IMAX Box Office")
                {
                    hp.NavigateUrl = "ResearchSupport/IMAXBoxOffice.aspx";

                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/IMAXlogo.png' width=""16"" /> IMAX Box Office";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/IMAXlogo.png' width=""16"" />";
                    }
                }
                if (e.Row.Cells[1].Text == "Penserra")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/penserra.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/penserra.png' width=""16"" /> Penserra";
                    }
                }
                if (e.Row.Cells[1].Text == "Tailor Research")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/tailorresearch.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/tailorresearch.png' width=""16"" /> Tailor Research";
                    }
                }
                if (e.Row.Cells[1].Text == "Light Street Surveys")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" /> Light Street Surveys";
                    }
                }
                if (e.Row.Cells[1].Text == "Jefferies")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/Jefferies.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/Jefferies.png' width=""16"" /> Jefferies Surveys";
                    }
                }
                if (e.Row.Cells[1].Text == "Team Blue/Bryan" && e.Row.Cells[3].Text == "SaaS Multiples Updated")
                {
                    hp.NavigateUrl = "ResearchSupport/SaaSMetrics.aspx";
                    e.Row.Cells[6].Text = "Bryan Hsu";
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/LS Logo box.jpg' width=""16"" /> Team Blue/Bryan";
                    }
                }
                if (e.Row.Cells[1].Text == "Marker Advisors")
                {

                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(hp.Text) + "&Source=&Timestamp=" + e.Row.Cells[7].Text;

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/markerlogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/markerlogo.png' width=""16"" /> Marker Advisors";
                    }

                }
                if (e.Row.Cells[1].Text == "Astris Advisory")
                {

                    hp.NavigateUrl = "News/NewsFullStory.aspx?Ticker=" + e.Row.Cells[10].Text + "&Headline=" + Server.UrlEncode(hp.Text) + "&Source=Astris Advisory&Timestamp=" + e.Row.Cells[7].Text;

                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/astris.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/astris.jpg' width=""16"" /> Astris Advisory";
                    }

                }
                if (e.Row.Cells[1].Text == "Lending Club")
                {
                    hp.NavigateUrl = "ResearchSupport/LCDaily.aspx";
                    e.Row.Cells[6].Text = "Ricky Hawkins";
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/lendingclublogo.ico' width=""16""/>";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/lendingclublogo.ico' width=""16""/> Lending Club";
                    }
                }
                if (e.Row.Cells[1].Text == "MKM Surveys")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/MKMlogo.jpg' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/MKMlogo.jpg' width=""16"" /> <a href='ResearchSupport/MKMSurveys.aspx'>MKM Surveys</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "Penserra Surveys")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/penserra.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/penserra.png' width=""16"" /> Penserra Surveys";
                    }
                }
                if (e.Row.Cells[1].Text == "RS Metrics")
                {
                    hp.NavigateUrl = "ResearchSupport/RSMetrics.aspx?Ticker=" + e.Row.Cells[1].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/RSMetricsLogo.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/RSMetricsLogo.png' width=""16"" /> <a href='RSMetricsHome.aspx'>RS Metrics</a>";
                    }
                }
                if (e.Row.Cells[1].Text == "Searchmetrics")
                {
                    hp.NavigateUrl = "ResearchSupport/SearchmetricsHome.aspx";

                    if (!Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/searchmetrics.jpg' width=""16"" /> Searchmetrics</a>";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/searchmetrics.jpg' width=""16"" />";
                    }
                }

                if (e.Row.Cells[1].Text == "TH Capital")
                {
                    hp.NavigateUrl = "ResearchSupport/Underlying/" + e.Row.Cells[8].Text;
                    if (Request.Browser.IsMobileDevice)
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/THdatacapital.png' width=""16"" />";
                    }
                    else
                    {
                        e.Row.Cells[1].Text = @"<img src='../Images/THdatacapital.png' width=""16"" /> <a href='ResearchSupport/ResearchHome.aspx?Filter=TH Capital'>TH Capital</a>";
                    }
                }

                if (e.Row.Cells[1].Text != @"<img src='../Images/evernotelogo.jpg' width=""16"" />" && e.Row.Cells[1].Text != @"<img src='../Images/evernotelogo.jpg' width=""16"" /> Evernote" && e.Row.Cells[1].Text != @"<img src='../Images/citronresearchlogo.png' width=""16"" />" && e.Row.Cells[1].Text != @"<img src='../Images/citronresearchlogo.png' width=""16"" /> Citron Research" && e.Row.Cells[1].Text != @"<img src='../Images/axioslogo.png' width=""16"" />" && e.Row.Cells[1].Text != @"<img src='../Images/axioslogo.png' width=""16"" /> Axios")
                {
                    e.Row.Cells[3].Controls.Add(hp);
                }

                if (((Label)e.Row.FindControl("TickerLabel")).Text != "")
                {
                    TickerSplitter = ((Label)e.Row.FindControl("TickerLabel")).Text.Split(',');
                    for (int i = 0; i < TickerSplitter.Length; i++)
                    {
                        if (i > 3)
                        {
                            ((Label)e.Row.FindControl("TickerLabel")).Text += ", ...";
                            break;
                        }
                        if (e.Row.Cells[1].Text == @"<img src='../Images/1010datalogo.jpg' width=""16"" /> <a href='ResearchSupport/SpendingDataHome.aspx'>1010 Data</a>")
                        {
                            LinkRef = "ResearchSupport/SpendingData";
                        }
                        else
                        {
                            if (e.Row.Cells[1].Text == @"<img src='../Images/1010datalogo.jpg' width=""16"" /> <a href='ResearchSupport/SpendingDataHome.aspx'>1010 Data</a>")
                            {
                                LinkRef = "ResearchSupport/SpendingData";
                            }
                            else
                            {
                                if (e.Row.Cells[1].Text == @"<img src='../Images/secondmeasurelogo.ico' width=""16"" /> <a href='ResearchSupport/SpendingDataHome.aspx'>Second Measure</a>")
                                {
                                    LinkRef = "ResearchSupport/SpendingData";
                                }
                                else
                                {
                                    LinkRef = "Analytics/PositionNews";
                                }
                            }
                        }
                        if (e.Row.Cells[1].Text == @"<img src='../Images/secondmeasurelogo.ico' width=""16"" /> <a href='SpendingDataHome.aspx'>Second Measure</a>")
                        {
                            if (i == 0)
                            {
                                ((Label)e.Row.FindControl("TickerLabel")).Text = "<a href='" + LinkRef + ".aspx?Ticker=" + TickerSplitter[i].Trim() + "&Source=Second Measure'>" + TickerSplitter[i] + "</a>";
                            }
                            else
                            {
                                ((Label)e.Row.FindControl("TickerLabel")).Text = ((Label)e.Row.FindControl("TickerLabel")).Text + ", <a href='" + LinkRef + ".aspx?Ticker=" + TickerSplitter[i].Trim() + "&Source=Second Measure'>" + TickerSplitter[i] + "</a>";
                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                ((Label)e.Row.FindControl("TickerLabel")).Text = "<a href='" + LinkRef + ".aspx?Ticker=" + TickerSplitter[i].Trim() + "'>" + TickerSplitter[i] + "</a>";
                            }
                            else
                            {
                                ((Label)e.Row.FindControl("TickerLabel")).Text = ((Label)e.Row.FindControl("TickerLabel")).Text + ", <a href='" + LinkRef + ".aspx?Ticker=" + TickerSplitter[i].Trim() + "'>" + TickerSplitter[i] + "</a>";
                            }
                        }
                    }
                }
            }
            catch { }
            // Fix & being set to &amp; which is set to %26amp%3b
            //for (int r = 0; r < e.Row.Cells.Count; r++)
            //{
            //    if (((System.Web.UI.WebControls.DataControlFieldCell)(e.Row.Cells[r])).ContainingField.HeaderText == "Headline")
            //    {
            //        e.Row.Cells[r].Text = e.Row.Cells[r].Text.Replace("&amp;", "&");
            //    }
            //}
        }
    }



    public List<string> Current_TagList = new List<string>();
    public List<string> TagList = new List<string>();
    public string[] TagString;
    public string NewTickerText;
    public string tester2;
    public string NoteGUIDString;
    public string NoteGUIDTitle;

    protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        GridViewRow row = GridView1.Rows[index];
        //Label TickerLabel = (Label)row.FindControl("TickerLabel");
        //f (1==1) { 

        if (e.CommandName == "Update")
        {
            //find user entered ticker text
            TextBox tickerTxtBox = (TextBox)row.FindControl("TickerTextBox");
            NewTickerText = tickerTxtBox.Text;
            NoteGUIDString = row.Cells[8].Text;
            NoteGUIDTitle = row.Cells[10].Text;

            //update Evernote note
            TagString = NewTickerText.Split(new[] { ", " }, StringSplitOptions.None);
            for (int r = 0; r < TagString.Length; r++)
            {
                TagList.Add(TagString[r]);
            }
            ENSession.SetSharedSessionDeveloperToken("S=s634:U=6e7c7a2:E=17eb66e37aa:C=1775ebd0958:P=1cd:A=en-devtoken:V=2:H=b21be374c89f930e831af6f303710f98", "https://www.evernote.com/shard/s634/notestore");
            if (ENSession.SharedSession.IsAuthenticated == false)
            {
                ENSession.SharedSession.AuthenticateToEvernote();
            }
            ENSessionAdvanced.SetSharedSessionDeveloperToken("S=s634:U=6e7c7a2:E=17eb66e37aa:C=1775ebd0958:P=1cd:A=en-devtoken:V=2:H=b21be374c89f930e831af6f303710f98", "https://www.evernote.com/shard/s634/notestore");
            if (ENSessionAdvanced.SharedSession.IsAuthenticated == false)
            {
                ENSessionAdvanced.SharedSession.AuthenticateToEvernote();
            }
            tester2 = NoteGUIDTitle;

            try
            {
                List<ENSessionFindNotesResult> myNotesList = ENSession.SharedSession.FindNotes(ENNoteSearch.NoteSearch(NoteGUIDTitle.Replace(":", "")), null, ENSession.SearchScope.Business, ENSession.SortOrder.RecentlyUpdated, 20);
                foreach (ENSessionFindNotesResult result in myNotesList)
                {
                    if (result.NoteRef.Guid == NoteGUIDString)
                    {
                        ENNoteRef noteRef = result.NoteRef;

                        ENNoteStoreClient noteStore = ENSessionAdvanced.SharedSession.NoteStoreForNoteRef(noteRef);
                        Note edamNote = noteStore.GetNote(noteRef.Guid, true, false, false, false);

                        edamNote.Guid = noteRef.Guid;
                        tester2 += "HII";
                        edamNote.TagNames = TagList;
                        noteStore.UpdateNote(edamNote);
                        break;
                    }
                }
            }
            catch
            { }
            //update the SQL Database both LSPM_portfolio and LSC_Research servers
            /*
            using (var connection = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSPM_Portfolio;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True"))
            {
                connection.Open();
                using (var cmd = new SqlCommand("update evernoteapidatabase set ticker='" + NewTickerText + "' where noteguid='" + NoteGUIDString + "'", connection))
                {
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
            using (var connection2 = new SqlConnection("Server=tcp:lscresearch.database.windows.net,1433;Database=LSC_Research;User Id=LSAdministrator;Password=LSCResearch17!;Encrypt=True;Trusted_Connection=False;MultipleActiveResultSets=True"))
            {
                connection2.Open();
                using (var cmd2 = new SqlCommand("update evernoteapidatabase set ticker='" + NewTickerText + "' where noteguid='" + NoteGUIDString + "'", connection2))
                {
                    cmd2.ExecuteNonQuery();
                }
                connection2.Close();
            }
            */
        }
        //}   
    }

    protected void sqlds1_OnSelecting(object sender, SqlDataSourceCommandEventArgs e)
    {
        /*
         * Excellent place to do stuff before selecting data, such as setting the DEADLOCK_PRIORITY setting.
         * You cannot set the SqlDeadlockPriority in Page_Load and use it in the selectCommand. Must be done this way.
        */
        log.Info(System.Reflection.MethodBase.GetCurrentMethod().Name + ": Setting deadlock priority");
        try
        {
            if (CurrentUser == "glen")
            {
                SqlDeadlockPriority = "HIGH";
            }
            else
            {
                SqlDeadlockPriority = "LOW";
            }
            e.Command.Parameters["@DeadlockPriority"].Value = SqlDeadlockPriority;
        }
        catch
        { }
        log.Warn(string.Format("User {0} is running SQL with: {1}", CurrentUser, SqlDeadlockPriority));
    }
}