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

            // Retrieve the hash table corresponding to the column.
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
            //	Zurücksetzen der Icons für die SortOrder
            foreach (ColumnHeader col in lvLivestatusData.Columns)
            {
                col.ImageKey = "FullGreen";
            }

            // Set the sort order to ascending when changing
            // column groups; otherwise, reverse the sort order.
            if (lvLivestatusData.Sorting == SortOrder.Descending ||
                (isRunningXPOrLater && (e.Column != groupColumn)))
            {
                lvLivestatusData.Sorting = SortOrder.Ascending;
                lvLivestatusData.Columns [e.Column].ImageKey = "SortAscend";
            }
            else
            {
                lvLivestatusData.Sorting = SortOrder.Descending;
                lvLivestatusData.Columns [e.Column].ImageKey = "SortDesc";

            }

            // Set the groups to those created for the clicked column.
            if (isRunningXPOrLater)
            {
                SetGroups (e.Column);
            }

            groupColumn = e.Column;

        }
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
