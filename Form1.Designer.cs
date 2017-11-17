﻿namespace MK_Livestatus_GUI
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lvLIvestatusData = new System.Windows.Forms.ListView();
            this.splitter = new System.Windows.Forms.Splitter();
            this.tbLivestatusQuery = new System.Windows.Forms.TextBox();
            this.buttStartQuery = new System.Windows.Forms.Button();
            this.buttConnection = new System.Windows.Forms.Button();
            this.buttQueryOptions = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
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
            // lvLIvestatusData
            // 
            this.lvLIvestatusData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLIvestatusData.FullRowSelect = true;
            this.lvLIvestatusData.GridLines = true;
            this.lvLIvestatusData.Location = new System.Drawing.Point(0, 125);
            this.lvLIvestatusData.MultiSelect = false;
            this.lvLIvestatusData.Name = "lvLIvestatusData";
            this.lvLIvestatusData.ShowItemToolTips = true;
            this.lvLIvestatusData.Size = new System.Drawing.Size(765, 365);
            this.lvLIvestatusData.TabIndex = 1;
            this.lvLIvestatusData.UseCompatibleStateImageBehavior = false;
            this.lvLIvestatusData.View = System.Windows.Forms.View.Details;
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
            // tbLivestatusQuery
            // 
            this.tbLivestatusQuery.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbLivestatusQuery.Location = new System.Drawing.Point(0, 0);
            this.tbLivestatusQuery.Multiline = true;
            this.tbLivestatusQuery.Name = "tbLivestatusQuery";
            this.tbLivestatusQuery.Size = new System.Drawing.Size(236, 125);
            this.tbLivestatusQuery.TabIndex = 0;
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
            // 
            // buttConnection
            // 
            this.buttConnection.Location = new System.Drawing.Point(678, 12);
            this.buttConnection.Name = "buttConnection";
            this.buttConnection.Size = new System.Drawing.Size(75, 23);
            this.buttConnection.TabIndex = 2;
            this.buttConnection.Text = "Verbindung";
            this.buttConnection.UseVisualStyleBackColor = true;
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
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 468);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(765, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // FormMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 490);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.lvLIvestatusData);
            this.Controls.Add(this.panel1);
            this.Name = "FormMainWindow";
            this.Text = "MK_Livestatus - GUI";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView lvLIvestatusData;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.Button buttQueryOptions;
        private System.Windows.Forms.Button buttConnection;
        private System.Windows.Forms.Button buttStartQuery;
        private System.Windows.Forms.TextBox tbLivestatusQuery;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}

