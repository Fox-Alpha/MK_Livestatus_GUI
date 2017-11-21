using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NagiosConectionManager;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassHelperUtils;
using static System.Windows.Forms.ListView;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MK_Livestatus_GUI
{

    public enum E_MK_LiveStatusObjectTypes : int
    {
        None = 0,
        String = 1,
        Integer,
        Boolean,
        Time,
        Float,
        List,
        Float2Vector,
        Float3Vector,
        Dict
    }

    public enum E_MK_LivestatusTables : int
    {
        None = 0,
        Columns,
        Comments,
        Commands,
        Contactgroups,
        Contacts,
        Downtimes,
        Hostgroups,
        Hosts,
        Hostsbygroup,
        Log,
        Servicegroups,
        Services,
        Servicesbygroup,
        Servicesbyhostgroup,
        Statehist,
        Status,
        Timeperiods
    }

    public partial class FormMainWindow : Form
    {
        #region Properties
        // Determine whether Windows XP or a later
        // operating system is present.
        private bool isRunningXPOrLater =
            OSFeature.Feature.IsPresent (OSFeature.Themes);

        // Declare a Hashtable array in which to store the groups.
        private Hashtable [] groupTables;

        // Declare a variable to store the current grouping column.
        int groupColumn = 0;

        public string strNagiosKonfigFile { get; private set; } = "ServerKonfiguration.bin";

        //	Menüeintrag für ConnectionManager Formaufruf
        ToolStripMenuItem tsiNagiosConnectionManager;

        Socket s = null;
        IPEndPoint hostEndPoint;
        IPAddress hostAddress;
        IPAddress [] IPaddresses = null;

        string _nagiosHost;

        public string nagiosHost
        {
            get { return _nagiosHost; }
            set { _nagiosHost = value; }
        }
        int _nagiosLivePort;

        public int nagiosLivePort
        {
            get { return _nagiosLivePort; }
            set { _nagiosLivePort = value; }
        }

        int _activeConnectionSet;

        public int activeConnectionSet
        {
            get { return _activeConnectionSet; }
            set { _activeConnectionSet = value; }
        }

        public string StrKonfigFile
        {
            get
            {
                return strKonfigFile;
            }

            set
            {
                strKonfigFile = value;
            }
        }
        
        private List<MK_Livestatus> MKLivestatusList;

        private MK_Livestatus mklivestatus;
        public MK_Livestatus Mklivestatus
        {
            get
            {
                return mklivestatus;
            }

            set
            {
                mklivestatus = value;
            }
        }

        private string strKonfigFile;

        ArrayList nagiosKonfigList;


        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent (false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent (false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent (false);

        // The response from the remote device.
        private static String response = String.Empty;

        #endregion

        #region FormFunktions
        public FormMainWindow ()
        {
            InitializeComponent ();

            StrKonfigFile = Path.GetDirectoryName( Application.ExecutablePath) + "\\cfg.JSON";
            mklivestatus = new MK_Livestatus ();
            MKLivestatusList = new List<MK_Livestatus> ();
        }


        private void FormMainWindow_Load (object sender, EventArgs e)
        {
            tsiNagiosConnectionManager = new ToolStripMenuItem ("Verbindungen Verwaltung",
                                    null,
                                    verbindungenVerwaltenToolStripMenuItem_Click,
                                    "tsiNagiosConnectionManager");
            //Laden der Nagios Konfigurationen und Darstellen im Menü der Statuszeile
            loadKonfigFromFile ();

            //	Setzen der ersten Eintrags als standard Verbindung
            activeConnectionSet = nagiosKonfigList.Count > 0 ? 0 : -1;

            setConnectionList ();

            //TODO: Hostauflösung wenn keine IP eingetragen ist
            //	Auslagern in eigene Funktion

            if (activeConnectionSet >= 0)
            {
                nagiosHost = ((NagiosServer) nagiosKonfigList [activeConnectionSet]).hostname;
                //	prüfen ob es sich bei Hostname um eine IP Adresse handelt
                if (!Regex.IsMatch (nagiosHost, "192.162.[0-9]{1,3}-[0-9]{1,3}") || !IPAddress.TryParse (nagiosHost, out hostAddress))
                {
                    // Get DNS host information.
                    IPHostEntry hostInfo = Dns.GetHostEntry (nagiosHost);
                    // Get the DNS IP addresses associated with the host.
                    IPaddresses = hostInfo.AddressList;

                    hostAddress = IPaddresses [0];
                    //					hostEndPoint = new IPEndPoint(hostAddress, port);
                }
                if (((NagiosServer) nagiosKonfigList [activeConnectionSet]).enableLivestatus) //&& ((NagiosServer)nagiosKonfigList[activeConnectionSet]).enableLivestatus)
                {
                    nagiosLivePort = ((NagiosServer) nagiosKonfigList [activeConnectionSet]).mklivePort;
                    //					hostEndPoint = new IPEndPoint(hostAddress, nagiosLivePort);

                    Debug.WriteLine ("IPADRESS.TryParse()", "MainForm_Load()");

                }

                hostEndPoint = new IPEndPoint (hostAddress, nagiosLivePort);
            }

            if (isRunningXPOrLater)
            {
                // Create the groupsTable array and populate it with one 
                // hash table for each column.
                groupTables = new Hashtable [lvLivestatusData.Columns.Count];
                for (int column = 0; column < lvLivestatusData.Columns.Count; column++)
                {
                    // Create a hash table containing all the groups 
                    // needed for a single column.
                    groupTables [column] = CreateGroupsTable (column);
                    lvLivestatusData.Columns [column].ImageKey = "FullGreen";
                }

                // Start with the groups created for the Title column.
                SetGroups (0);
            }


            //  JSON Load Test
            loadFromCFGFile ();

        }

        #endregion

        //  Funktionen zum Auswerten, aufbereiten und Anzeigen der Daten
        #region DataDisplay

        // Creates a Hashtable object with one entry for each unique
        // subitem value (or initial letter for the parent item)
        // in the specified column.
        private Hashtable CreateGroupsTable (int column)
        {
            // Create a Hashtable object.
            Hashtable groups = new Hashtable ();

            // Iterate through the items in myListView.
            foreach (ListViewItem item in lvLivestatusData.Items)
            {
                // Retrieve the text value for the column.
                string subItemText = item.SubItems [column].Text;

                if (!string.IsNullOrWhiteSpace (subItemText))
                {
                    // Use the initial letter instead if it is the first column.
                    if (column == 0)
                    {
                        subItemText = subItemText.Substring (0, 1);
                    }

                    // If the groups table does not already contain a group
                    // for the subItemText value, add a new group using the 
                    // subItemText value for the group header and Hashtable key.
                    if (!groups.Contains (subItemText))
                    {
                        groups.Add (subItemText, new ListViewGroup (subItemText,
                            HorizontalAlignment.Left));
                    }
                }
                else    //	Eine Gruppe für Leere Einträge erzeugen
                {
                    if (!groups.Contains ("Leer"))
                    {
                        groups.Add ("Leer", new ListViewGroup ("Leer",
                            HorizontalAlignment.Left));
                    }
                }
            }

            // Return the Hashtable object.
            return groups;
        }

        // Sets myListView to the groups created for the specified column.
        //	TODO: Funktion erweitern das diese dynamisch mit jedem ListView verwendet werden kann
        private void SetGroups (int column)
        {
            // Remove the current groups.
            lvLivestatusData.Groups.Clear ();

            // TODO: Columns Checken
            // Retrieve the hash table corresponding to the column.
            if (groupTables.Length == 0)
            {
                return;
            }
            Hashtable groups = (Hashtable) groupTables [column];

            // Copy the groups for the column to an array.
            ListViewGroup [] groupsArray = new ListViewGroup [groups.Count];
            groups.Values.CopyTo (groupsArray, 0);

            // Sort the groups and add them to myListView.
            Array.Sort (groupsArray, new ListViewGroupSorter (lvLivestatusData.Sorting));
            lvLivestatusData.Groups.AddRange (groupsArray);

            // Iterate through the items in myListView, assigning each 
            // one to the appropriate group.
            foreach (ListViewItem item in lvLivestatusData.Items)
            {
                // Retrieve the subitem text corresponding to the column.
                string subItemText = item.SubItems [column].Text;

                if (!string.IsNullOrWhiteSpace (subItemText))
                {
                    // For the Title column, use only the first letter.
                    if (column == 0)
                    {
                        subItemText = subItemText.Substring (0, 1);
                    }

                    // Assign the item to the matching group.
                    item.Group = (ListViewGroup) groups [subItemText];
                }
                else
                {
                    item.Group = (ListViewGroup) groups ["Leer"];
                }
            }
        }

        /// <summary>
        /// Sortiert den Inhalt per Klick auf den Spaltennamen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvLivestatusData_ColumnClick (object sender, ColumnClickEventArgs e)
        {

            // Set the sort order to ascending when changing
            // column groups; otherwise, reverse the sort order.

            lvLivestatusData.Sorting = lvLivestatusData.Sorting == SortOrder.Descending ? SortOrder.Ascending : SortOrder.Descending;

            //	Zurücksetzen der Icons für die SortOrder
            if (lvLivestatusData.SmallImageList != null)
            {
                foreach (ColumnHeader col in lvLivestatusData.Columns)
                {
                    col.ImageKey = "FullGreen";
                }

                lvLivestatusData.Columns [e.Column].ImageKey = lvLivestatusData.Sorting == SortOrder.Descending ? "SortDesc" : "SortAscend";
            }

            //if (lvLivestatusData.Sorting == SortOrder.Descending ||
            //    (isRunningXPOrLater && (e.Column != groupColumn)))
            //{
            //    lvLivestatusData.Sorting = SortOrder.Ascending;
            //    if (lvLivestatusData.SmallImageList != null)
            //        lvLivestatusData.Columns [e.Column].ImageKey = "SortAscend";
            //}
            //else
            //{
            //    lvLivestatusData.Sorting = SortOrder.Descending;
            //    if (lvLivestatusData.SmallImageList != null)
            //        lvLivestatusData.Columns [e.Column].ImageKey = "SortDesc";

            //}

            // Set the groups to those created for the clicked column.
            if (isRunningXPOrLater)
            {
                SetGroups (e.Column);
            }

            groupColumn = e.Column;

        }

        #endregion

        //  Funktionen die aus dem UI z.B. per Button aufgerufen werden
        #region UIFunktons

        /// <summary>
        /// Ruft den ConnectionManager per Button auf
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttConnection_Click (object sender, EventArgs e)
        {
            ConnectionManager ncm = new ConnectionManager ();

            ncm.strNagiosKonfigfile = strNagiosKonfigFile;

            if (ncm.ShowDialog () == DialogResult.OK)
            {
                //Neuladen der Konfigurationsliste
                refreshConnectionList ();
            }

            ncm.Dispose ();

        }

        /// <summary>
        /// Starten die Abfrage mit dem einegegebenen Query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttStartQuery_Click (object sender, EventArgs e)
        {

            Thread thread = new Thread (DoWork);
            //	Gruppen können nur ab Windows XP und aufwärts erstellt werden
            if (isRunningXPOrLater)
            {
                // Create the groupsTable array and populate it with one 
                // hash table for each column.
                groupTables = new Hashtable [lvLivestatusData.Columns.Count];
                for (int column = 0; column < lvLivestatusData.Columns.Count; column++)
                {
                    // Create a hash table containing all the groups 
                    // needed for a single column.
                    groupTables [column] = CreateGroupsTable (column);
                    lvLivestatusData.Columns [column].ImageKey = "FullGreen";
                }

                // Start with the groups created for the Title column.
                SetGroups (0);
            }

            thread.Start ();

        }

        #endregion

        //  Verwaltung der Server Verbindungsdaten
        #region ConnectionManager
        /// <summary>
        /// Anzeigen der Verbindungsverwaltung aus dem Assembly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void verbindungenVerwaltenToolStripMenuItem_Click (object sender, EventArgs e)
        {
            ConnectionManager ncm = new ConnectionManager ();

            ncm.strNagiosKonfigfile = strNagiosKonfigFile;

            if (ncm.ShowDialog () == DialogResult.OK)
            {
                //Neuladen der Konfigurationsliste
                refreshConnectionList ();
            }

            ncm.Dispose ();
        }

        /// <summary>
        /// Setzen der zu verwendenden Verbindungskonfiguration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void setActiveConnectionToolStripMenuItem_Click (object sender, EventArgs e)
        {
            int lastConnectionSet = activeConnectionSet;

            activeConnectionSet = Convert.ToInt32 ((sender as ToolStripMenuItem).Tag);

            //	Checked aus vorherigem MenuItem entfernen
            foreach (ToolStripItem tsmi in tsddButtConnections.DropDownItems)
            {
                if (tsmi as ToolStripMenuItem != null)
                    ((ToolStripMenuItem) tsmi).Checked = false;
            }

            (sender as ToolStripMenuItem).Checked = true;

            if (activeConnectionSet >= 0)
            {
                //	prüfen ob es sich bei Hostname um eine IP Adresse handelt
                nagiosHost = ((NagiosServer) nagiosKonfigList [activeConnectionSet]).hostname;
                //if (!Regex.IsMatch(((NagiosServer)nagiosKonfigList[activeConnectionSet]).hostname, "192.162.[0-9]{1,3}-[0-9]{1,3}"))
                if (!Regex.IsMatch (nagiosHost, "192.162.[0-9]{1,3}-[0-9]{1,3}") || !IPAddress.TryParse (nagiosHost, out hostAddress))
                {

                    // Get DNS host information.
                    IPHostEntry hostInfo = Dns.GetHostEntry (nagiosHost);
                    // Get the DNS IP addresses associated with the host.
                    IPaddresses = hostInfo.AddressList;

                    hostAddress = IPaddresses [0];
                }
                if (((NagiosServer) nagiosKonfigList [activeConnectionSet]).enableLivestatus)
                //if (IPAddress.TryParse(((NagiosServer)nagiosKonfigList[activeConnectionSet]).hostname, out hostAddress) && ((NagiosServer)nagiosKonfigList[activeConnectionSet]).enableLivestatus)
                {
                    nagiosLivePort = ((NagiosServer) nagiosKonfigList [activeConnectionSet]).mklivePort;
                    Debug.WriteLine ("Setzen der MKLive Verbindung", "setActiveConnectionToolStripMenuItem_Click()");
                }

                hostEndPoint = new IPEndPoint (hostAddress, nagiosLivePort);

            }
        }

        /// <summary>
        /// Reload der Verbindungseinträge
        /// </summary>
        void refreshConnectionList ()
        {
            clearConnectionList ();
            loadKonfigFromFile ();
            setConnectionList ();
            //	TODO: Aufruf eigener Funktion zum Setzen der aktiven Verbindung
        }

        /// <summary>
        /// Alle Verbindungseinträge im Menü entfernen 
        /// </summary>
        void clearConnectionList ()
        {
            if (nagiosKonfigList.Count > 0)
            {
                //Bereinigen der Menüliste
                tsddButtConnections.DropDown.Items.Clear ();
            }
        }

        /// <summary>
        /// Eingelesene Verbindungen als Eintrag hinzufügen
        /// </summary>
        void setConnectionList ()
        {
            ToolStripMenuItem tsmiConnect;
            tsddButtConnections.DropDownItems.Add (tsiNagiosConnectionManager);

            tsddButtConnections.DropDownItems.Add (new ToolStripSeparator ());

            foreach (object obj in nagiosKonfigList)
            {
                tsmiConnect = new ToolStripMenuItem ((obj as NagiosServer).svrConfigName, null, setActiveConnectionToolStripMenuItem_Click, "tsiNagiosConnection" + nagiosKonfigList.IndexOf (obj));
                tsmiConnect.Tag = nagiosKonfigList.IndexOf (obj);
                if (activeConnectionSet == nagiosKonfigList.IndexOf (obj))
                {
                    tsmiConnect.Checked = true;
                }

                tsddButtConnections.DropDownItems.Add (tsmiConnect);
            }
        }

        /// <summary>
        /// Laden der gespeicherten Konfigurationen
        /// </summary>
        void loadKonfigFromFile ()
        {
            if (!File.Exists (strNagiosKonfigFile))
                return;

            using (FileStream fs = new FileStream (strNagiosKonfigFile, FileMode.Open))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter ();
                    nagiosKonfigList = (ArrayList) formatter.Deserialize (fs);
                }
                catch (SerializationException e)
                {
                    // die Datei kann nicht deserialisiert werden
                    Debug.WriteLine (e.Message, "loadKonfigFromFile().SerializationException");
                }
                catch (IOException e)
                {
                    // Beim Versuch, die Datei zu öffnen, ist ein Fehler aufgetreten.
                    Debug.WriteLine (e.Message, "loadKonfigFromFile().IOException");
                }

                //			    foreach (object obj in nagiosKonfigList) {
                //			    	if(obj != null)
                //			    	{
                //			    		
                //			    	}
                //			    		listBox1.Items.Add((obj as NagiosServer).svrConfigName);
                //			    }
                //			    activeConfigIndex = listBox1.Items.Count > 0 ? 0 : -1;
            }
        }
        #endregion

        private void button1_Click (object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew ();

            lvLivestatusData.Items.Clear ();
            //foreach (string item in GetDirGreaterThen24h(@"c:\temp\08154711"))
            foreach (string item in GetDirGreaterThen24h(@"\\HH-VS-PR-002\verarb\"))
            {
                lvLivestatusData.Items.Add (new ListViewItem(item)).SubItems.Add(new DirectoryInfo(item).CreationTime.ToString());

            }

            Debug.WriteLine (sw.Elapsed.ToString ());

            if (isRunningXPOrLater)
            {
                // Create the groupsTable array and populate it with one 
                // hash table for each column.
                groupTables = new Hashtable [lvLivestatusData.Columns.Count];
                for (int column = 0; column < lvLivestatusData.Columns.Count; column++)
                {
                    // Create a hash table containing all the groups 
                    // needed for a single column.
                    groupTables [column] = CreateGroupsTable (column);
                    lvLivestatusData.Columns [column].ImageKey = "FullGreen";
                }

                // Start with the groups created for the Title column.
                SetGroups (0);
            }

            toolStripStatusLabel1.Text = string.Format("Es wurden {0} Verzeichnisse gefunden",  lvLivestatusData.Items.Count.ToString ());
        }

        public static string [] GetDirGreaterThen24h (string sourceDir)
        {
            string [] Jobs = System.IO.Directory.GetDirectories (sourceDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            List<string> result = new List<string> ();

            foreach (string JobPath in Jobs)
            {
                if (Directory.Exists(Path.Combine (JobPath, "buf\\")))
                {
                    string [] Jobbuf = System.IO.Directory.GetDirectories (Path.Combine(JobPath, "buf\\"), "*.*", System.IO.SearchOption.TopDirectoryOnly);
                    foreach (string buf in Jobbuf)
                    {
                        System.IO.DirectoryInfo diJob = new System.IO.DirectoryInfo (buf);
                        if (diJob.CreationTime < DateTime.Now.AddHours (-24))
                        {
                            Debug.WriteLine (@"Der Batch: " + buf + "ist älter wie 24h: " + diJob.CreationTime.ToString ());
                            result.Add (buf);
                        }
                    }
                }
            }
            
            return result.ToArray ();
        }

        /// <summary>
        /// Diese Methode wird von einem Background Thread (nicht UI Thread) aufgerufen
        /// </summary>
        /// <param name="max">Maximale Anzahl der Durchläufe</param>
        //private void BackgroundThreadAction (int max)
        //{
        //    //if (max < 0)
        //    //{
        //    //    InvokeIfRequired (this.textBoxStatus, (MethodInvoker) delegate ()
        //    //    {
        //    //        this.textBoxStatus.Text = "max value must be greater than zero";
        //    //    });
        //    //    return;
        //    //}
        //    for (int i = 0; i < max; i++)
        //    {
        //        InvokeIfRequired (this.textBoxStatus, (MethodInvoker) delegate ()
        //        {
        //            this.textBoxStatus.Text = String.Format ("Processing value {0}", i);
        //        });
        //        Thread.Sleep (10);
        //    }
        //}

        /// <summary>
        /// Methode die die UI Interaktionen durchführt.
        /// </summary>
        /// <param name="target">Control welches geändert wird, werden mehrere Controls geändert muss hier das Parent Control übergeben werden</param>
        /// <param name="methodToInvoke">der Delegate der ausgeführt werden soll (UI Aktionen)</param>
        private void InvokeIfRequired (Control target, Delegate methodToInvoke)
        {
            /* Mit Hilfe von InvokeRequired wird geprüft ob der Aufruf direkt an die UI gehen kann oder
             * ob ein Invokeing hier von Nöten ist
             */
            if (target.InvokeRequired)
            {
                // Das Control muss per Invoke geändert werden, weil der aufruf aus einem Backgroundthread kommt
                target.Invoke (methodToInvoke);
            }
            else
            {
                // Die Änderung an der UI kann direkt aufgerufen werden.
                methodToInvoke.DynamicInvoke ();


            }
        }

        private bool _shouldStop = false;

        public void DoWork ()
        {

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.GetHostEntry (nagiosHost);
                IPAddress ipAddress = ipHostInfo.AddressList [0];
                IPEndPoint remoteEP = new IPEndPoint (ipAddress, nagiosLivePort);

                string GET = string.Empty;

                // Create a TCP/IP socket.
                Socket client = new Socket (AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                client.SetSocketOption (SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
                client.ReceiveTimeout = 300000;

                // Connect to the remote endpoint.
                client.BeginConnect (remoteEP,
                    new AsyncCallback (ConnectCallback), client);
                connectDone.WaitOne ();

                // Send test data to the remote device.
                //Send (client, "This is a test<EOF>");
                //Send (client, "GET status\nResponseHeader: fixed16\nColumnHeaders: on\n");
                GET =

                //"GET columns\nFilter: table = comments\nColumns: name\nResponseHeader: fixed16\n");
                //"GET status\n" +
                //"Columns: livecheck_overflows livecheck_overflows_rate livechecks livechecks_rate livestatus_active_connections " +
                //"livestatus_queued_connections livestatus_threads livestatus_version num_hosts num_services program_start program_version\n" +
                //"ColumnHeaders: on\n" +
                //"ResponseHeader: fixed16\n"

                //"GET comments\n" +
                //"Columns: author comment host_alias\n" +
                //"ColumnHeaders: on\n" +
                //"ResponseHeader: fixed16\n";

                "GET services\n" +
                "Columns:  host_name description last_state state state_type last_state_change next_check plugin_output\n" +
                //"Filter: description = TEST Port Check mit Antwort Pruefung\n" +
                "WaitTrigger: state\n" +
                //"Filter: host_name = HH-WS-SD-003\n";// +
                "Filter: state > 0\n" +
                "WaitTimeout: 3000\n" +
                //"ResponseHeader: fixed16"
                "ColumnHeaders: on\n"

                //"OutputFormat: json\n"
                ;


                // Syncron Receive
                Encoding ASCII = Encoding.ASCII;
                Byte [] ByteGet = ASCII.GetBytes (GET);
                Byte [] RecvBytes = new Byte [256];

                Int32 bytesend = 0;
                //String strRetPage = null;

                //bool b = client.Poll (100, SelectMode.SelectRead);

                if (client.Connected)
                {
                    //Send (client, GET);
                    //sendDone.WaitOne ();

                    //stopWatch.Start ();

                    bytesend = client.Send (ByteGet, ByteGet.Length, SocketFlags.None);
                    client.Shutdown (SocketShutdown.Send);
                    // Receive the host home page content and loop until all the data is received.
                    //Int32 bytes = client.Receive (RecvBytes, RecvBytes.Length, 0);

                    //while (bytes > 0)
                    //{
                    //    response = response + ASCII.GetString (RecvBytes, 0, bytes);
                    //    bytes = client.Receive (RecvBytes, RecvBytes.Length, 0);
                    //}
                    //Receive the response from the remote device.
                    Receive (client);
                    receiveDone.WaitOne ();

                    if (client != null)
                    {
                        client.Shutdown (SocketShutdown.Both);
                        client.Close ();
                    }

                    //stopWatch.Stop ();
                }

                //  convert Unix Timestams to DateTimes
                Debug.WriteLine (response, "DEBUG");

                #region DataAusgabe

                if (!string.IsNullOrWhiteSpace (response))
                {
                    List<String> result = new List<String> ();
                    result.AddRange (response.Split ('\n'));

                    for (int i = 0; i < result.Count; i++)
                    {

                        //  ColumnHeader Beschriftungen setzen
                        if (i==0)
                        {   
                            String [] header = result [0].ToString ().Split (';'); 

                            InvokeIfRequired (lvLivestatusData, (MethodInvoker) delegate ()
                            {
                                //lvLivestatusData.Items.Add (String.Format ("Processing value {0}", i));
                                lvLivestatusData.Columns.Clear ();

                                foreach (string item in header)
                                {
                                    lvLivestatusData.Columns.Add (new ColumnHeader ().Text = item);
                                }

                                //lvLivestatusData.Columns.AddRange (new ColumnHeaderCollection [] { });

                            });
                            Thread.Sleep (10);
                            continue;
                        }


                        String [] temp = result [i].ToString ().Split (';');

                        int iTmp;
                        //int.TryParse (temp [2], out iTmp);
                        //DateTimeOffset dto;
                        //= UnixDateTime.FromUnixTimeSeconds (iTmp);
                        //temp [2] = dto.DateTime.ToString ();

                        if (temp.Length >= 4)
                        {
                            int.TryParse (temp [4], out iTmp);
                            DateTimeOffset dto1 = UnixDateTime.FromUnixTimeSeconds (iTmp);
                            temp [4] = dto1.LocalDateTime.ToString ();

                            int.TryParse (temp [5], out iTmp);
                            DateTimeOffset dto2 = UnixDateTime.FromUnixTimeSeconds (iTmp);
                            temp [5] = dto2.DateTime.ToString ();

                            result [i] = string.Join (";", temp);

                        }
                        else
                            result [i] = string.Join (";", temp);

                    }

                    //// Write the response to the console.
                    foreach (string item in result)
                    {
                        //Console.WriteLine ("Response received : {0}", item);
                        InvokeIfRequired (lvLivestatusData, (MethodInvoker) delegate ()
                        {
                            //lvLivestatusData.Items.Add (String.Format ("Processing value {0}", i));
                            lvLivestatusData.Items.Add (new ListViewItem(item.Split(';')));
                        });
                        Thread.Sleep (10);
                    }

                }
                else
                    Console.WriteLine ("Response received : Empty Response");


                //int max = 10;

                //while (!_shouldStop)
                //{
                //    for (int i = 0; i < max; i++)
                //    {
                //    }
                //}
                #endregion

                //TimeSpan ts = stopWatch.Elapsed;
                //string elapsedTime = String.Format ("{0:00}:{1:00}:{2:00}.{3:00}",
                //ts.Hours, ts.Minutes, ts.Seconds,
                //ts.Milliseconds / 10);
                //Console.WriteLine ("RunTime " + elapsedTime);
                //Console.ReadKey ();




                //Console.WriteLine ("Starte zweiten durchlauf: ");

                //client.BeginConnect (remoteEP,
                //    new AsyncCallback (ConnectCallback), client);
                //connectDone.WaitOne ();

                //GET =
                //"GET comments\n" +
                //"Columns: author comment host_alias\n" +
                //"ColumnHeaders: on\n" +
                //"ResponseHeader: fixed16\n"
                //;
                //bytesend = client.Send (ByteGet, ByteGet.Length, SocketFlags.None);
                //client.Shutdown (SocketShutdown.Send);

                //Receive (client);
                //receiveDone.WaitOne ();

                //Console.WriteLine ("Response received : {0}", response);


                //// Release the socket.
                //if (client != null)
                //{
                //    client.Shutdown (SocketShutdown.Both);
                //    client.Close ();
                //}

            }
            catch (Exception e)
            {
                Console.WriteLine (e.ToString ());
                Console.ReadKey ();
            }
        }

        #region Socket

        private static void ConnectCallback (IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket) ar.AsyncState;

                // Complete the connection.
                client.EndConnect (ar);

                Console.WriteLine ("Socket connected to {0}",
                    client.RemoteEndPoint.ToString ());

                // Signal that the connection has been made.
                connectDone.Set ();
            }
            catch (Exception e)
            {
                Console.WriteLine (e.ToString ());
            }
        }

        private static void Receive (Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject ();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback (ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine (e.ToString ());
            }
        }

        private static void ReceiveCallback (IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject) ar.AsyncState;
                Socket client = state.workSocket;


                // Read data from the remote device.
                int bytesRead = client.EndReceive (ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append (Encoding.ASCII.GetString (state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback (ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString ();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set ();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine (e.ToString ());
            }
        }

        private static void Send (Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte [] byteData = Encoding.ASCII.GetBytes (data);

            // Begin sending the data to the remote device.
            client.BeginSend (byteData, 0, byteData.Length, 0,
                new AsyncCallback (SendCallback), client);
        }

        private static void SendCallback (IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend (ar);
                Console.WriteLine ("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set ();
            }
            catch (Exception e)
            {
                Console.WriteLine (e.ToString ());
            }
        }

        #endregion

        #region JSON
        public bool loadFromCFGFile ()
        {
            if (!string.IsNullOrWhiteSpace (StrKonfigFile) && File.Exists (StrKonfigFile))
            {
                readJSON ();
                return true;
            }
            return false;
        }

        public bool readJSON ()
        {
            try
            {
                //JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                //{
                //    Formatting = Formatting.Indented
                    
                //};
                JsonConvert.DefaultSettings = (() =>
                {
                    var settings = new JsonSerializerSettings ();
                    settings.Converters.Add (new StringEnumConverter ());
                    settings.Formatting = Formatting.Indented;
                    return settings;
                });

                using (StreamReader file = File.OpenText (StrKonfigFile))
                {
                    //JsonSerializer serializer = new JsonSerializer ();
                    //Mklivestatus = 
                    MKLivestatusList = JsonConvert.DeserializeObject<List<MK_Livestatus>>(File.ReadAllText(StrKonfigFile));

                    List<MK_Livestatus> tableLIst = MKLivestatusList.Where (s => s.LivefieldTable == E_MK_LivestatusTables.Commands).ToList();
                   
                    //			    Settings = (appSettings)JsonConvert.DeserializeObject(GetJSON());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine (e.Message, "Error: readJSON()");
                return false;
            }

            return true;
        }

        #endregion
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class MK_Livestatus
    {
        private int livefieldID;
        [JsonProperty ("livefieldID", Required = Required.Default)]
        public int LivefieldID
        {
            get
            {
                return livefieldID;
            }

            set
            {
                livefieldID = value;
            }
        }

        private E_MK_LivestatusTables livefieldTable;
        [JsonProperty ("livefieldTable", Required = Required.Default)]
        [JsonConverter (typeof (StringEnumConverter))]
        public E_MK_LivestatusTables LivefieldTable
        {
            get
            {
                return livefieldTable;
            }

            set
            {
                livefieldTable = value;
            }
        }

        private string livefieldName;
        [JsonProperty ("livefieldName", Required = Required.Default)]
        public string LivefieldName
        {
            get
            {
                return livefieldName;
            }

            set
            {
                livefieldName = value;
            }
        }

        private E_MK_LiveStatusObjectTypes livefieldTypeID;
        [JsonProperty ("livefieldTypeID", Required = Required.Default)]
        [JsonConverter (typeof (StringEnumConverter))]
        public E_MK_LiveStatusObjectTypes LivefieldTypeID
        {
            get
            {
                return livefieldTypeID;
            }

            set
            {
                livefieldTypeID = value;
            }
        }

        private string livefieldDescription;
        [JsonProperty ("livefieldDescription", Required = Required.Default)]
        public string LivefieldDescription
        {
            get
            {
                return livefieldDescription;
            }

            set
            {
                livefieldDescription = value;
            }
        }

        private string liveFieldTypeName;
        [JsonProperty ("livefieldTypeName", Required = Required.Default)]
        public string LiveFieldTypeName
        {
            get
            {
                return liveFieldTypeName;
            }

            set
            {
                liveFieldTypeName = value;
            }
        }
    }


    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte [] buffer = new byte [BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder ();
    }

    //class Worker
    //{
    //    private volatile bool _shouldStop = false;

    //    public Worker() { }

    //    public void DoWork()
    //    {
    //        while(!_shouldStop)
    //        {
    //            for (int i = 0; i < max; i++)
    //            {
    //                InvokeIfRequired (this.textBoxStatus, (MethodInvoker) delegate ()
    //                {
    //                    this.textBoxStatus.Text = String.Format ("Processing value {0}", i);
    //                });
    //                Thread.Sleep (10);
    //            }
    //        }

    //    }

    //    public void StopWork()
    //    {
    //        _shouldStop = true;
    //    }
    //}


    // Sorts ListViewGroup objects by header value.
    /// <summary>
    /// Klasse zum Festlegen der Sortierfolge und durchführen der Sortierung
    /// </summary>
    class ListViewGroupSorter : IComparer
    {
        private SortOrder order;	//TODO: Als Propertie anlegen ?

        // Stores the sort order.
        public ListViewGroupSorter (SortOrder theOrder)
        {
            order = theOrder;
        }

        // Compares the groups by header value, using the saved sort
        // order to return the correct value.
        public int Compare (object x, object y)
        {
            int result = String.Compare (
                ((ListViewGroup) x).Header,
                ((ListViewGroup) y).Header, StringComparison.CurrentCultureIgnoreCase
            );
            if (order == SortOrder.Ascending)
            {
                return result;
            }
            else
            {
                return -result;
            }
        }
    }
}
