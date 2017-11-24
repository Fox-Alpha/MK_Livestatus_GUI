namespace MK_Livestatus_GUI
{
    partial class FormMainWindow
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose ();
            }
            base.Dispose (disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent ()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainWindow));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.buttQueryOptions = new System.Windows.Forms.Button();
            this.buttConnection = new System.Windows.Forms.Button();
            this.buttStartQuery = new System.Windows.Forms.Button();
            this.tbLivestatusQuery = new System.Windows.Forms.TextBox();
            this.lvLivestatusData = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitter = new System.Windows.Forms.Splitter();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsddButtConnections = new System.Windows.Forms.ToolStripDropDownButton();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.buttQueryOptions);
            this.panel1.Controls.Add(this.buttConnection);
            this.panel1.Controls.Add(this.buttStartQuery);
            this.panel1.Controls.Add(this.tbLivestatusQuery);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(765, 125);
            this.panel1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(518, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttQueryOptions
            // 
            this.buttQueryOptions.Location = new System.Drawing.Point(243, 96);
            this.buttQueryOptions.Name = "buttQueryOptions";
            this.buttQueryOptions.Size = new System.Drawing.Size(109, 23);
            this.buttQueryOptions.TabIndex = 3;
            this.buttQueryOptions.Text = "Abfrage Optionen";
            this.buttQueryOptions.UseVisualStyleBackColor = true;
            // 
            // buttConnection
            // 
            this.buttConnection.Location = new System.Drawing.Point(678, 12);
            this.buttConnection.Name = "buttConnection";
            this.buttConnection.Size = new System.Drawing.Size(75, 23);
            this.buttConnection.TabIndex = 2;
            this.buttConnection.Text = "Verbindung";
            this.buttConnection.UseVisualStyleBackColor = true;
            this.buttConnection.Click += new System.EventHandler(this.buttConnection_Click);
            // 
            // buttStartQuery
            // 
            this.buttStartQuery.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttStartQuery.Location = new System.Drawing.Point(242, 12);
            this.buttStartQuery.Name = "buttStartQuery";
            this.buttStartQuery.Size = new System.Drawing.Size(110, 78);
            this.buttStartQuery.TabIndex = 1;
            this.buttStartQuery.Text = "Abfrage Starten";
            this.buttStartQuery.UseVisualStyleBackColor = true;
            this.buttStartQuery.Click += new System.EventHandler(this.buttStartQuery_Click);
            // 
            // tbLivestatusQuery
            // 
            this.tbLivestatusQuery.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbLivestatusQuery.Location = new System.Drawing.Point(0, 0);
            this.tbLivestatusQuery.Multiline = true;
            this.tbLivestatusQuery.Name = "tbLivestatusQuery";
            this.tbLivestatusQuery.Size = new System.Drawing.Size(236, 125);
            this.tbLivestatusQuery.TabIndex = 0;
            // 
            // lvLivestatusData
            // 
            this.lvLivestatusData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvLivestatusData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lvLivestatusData.FullRowSelect = true;
            this.lvLivestatusData.GridLines = true;
            this.lvLivestatusData.Location = new System.Drawing.Point(0, 125);
            this.lvLivestatusData.MultiSelect = false;
            this.lvLivestatusData.Name = "lvLivestatusData";
            this.lvLivestatusData.ShowItemToolTips = true;
            this.lvLivestatusData.Size = new System.Drawing.Size(765, 340);
            this.lvLivestatusData.TabIndex = 1;
            this.lvLivestatusData.UseCompatibleStateImageBehavior = false;
            this.lvLivestatusData.View = System.Windows.Forms.View.Details;
            this.lvLivestatusData.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvLivestatusData_ColumnClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Pfad";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "TimeStamp";
            // 
            // splitter
            // 
            this.splitter.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter.Location = new System.Drawing.Point(0, 125);
            this.splitter.Name = "splitter";
            this.splitter.Size = new System.Drawing.Size(765, 3);
            this.splitter.TabIndex = 3;
            this.splitter.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tsddButtConnections});
            this.statusStrip1.Location = new System.Drawing.Point(0, 468);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(765, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // tsddButtConnections
            // 
            this.tsddButtConnections.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsddButtConnections.Image = ((System.Drawing.Image)(resources.GetObject("tsddButtConnections.Image")));
            this.tsddButtConnections.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddButtConnections.Name = "tsddButtConnections";
            this.tsddButtConnections.Size = new System.Drawing.Size(29, 20);
            this.tsddButtConnections.Text = "toolStripDropDownButton1";
            // 
            // FormMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 490);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.lvLivestatusData);
            this.Controls.Add(this.panel1);
            this.Name = "FormMainWindow";
            this.Text = "MK_Livestatus - GUI";
            this.Load += new System.EventHandler(this.FormMainWindow_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView lvLivestatusData;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.Button buttQueryOptions;
        private System.Windows.Forms.Button buttConnection;
        private System.Windows.Forms.Button buttStartQuery;
        private System.Windows.Forms.TextBox tbLivestatusQuery;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripDropDownButton tsddButtConnections;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}

