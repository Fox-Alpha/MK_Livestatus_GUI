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
using System.Threading.Tasks;
using System.Windows.Forms;
using NagiosConectionManager;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MK_Livestatus_GUI
{

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

        ArrayList nagiosKonfigList;


        #endregion

        public FormMainWindow ()
        {
            InitializeComponent ();
        }

        private void buttStartQuery_Click (object sender, EventArgs e)
        {
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

        }

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

        private void buttConnection_Click (object sender, EventArgs e)
        {
            ConnectionManager ncm = new ConnectionManager ();

            ncm.strNagiosKonfigfile = strNagiosKonfigFile;

            if (ncm.ShowDialog () == DialogResult.OK)
            {
                //Neuladen der Konfigurationsliste
                //refreshConnectionList ();
            }

            ncm.Dispose ();

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

        }

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
    }


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
