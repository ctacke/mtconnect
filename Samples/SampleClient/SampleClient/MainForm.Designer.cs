namespace SampleClient
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.agentTree = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.agentAddress = new System.Windows.Forms.TextBox();
            this.connect = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataPlot = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.dataList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.propertyList = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.recordData = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataPlot)).BeginInit();
            this.SuspendLayout();
            // 
            // agentTree
            // 
            this.agentTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.agentTree.HideSelection = false;
            this.agentTree.Location = new System.Drawing.Point(6, 19);
            this.agentTree.Name = "agentTree";
            this.agentTree.Size = new System.Drawing.Size(242, 273);
            this.agentTree.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(17, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Agent Address:";
            // 
            // agentAddress
            // 
            this.agentAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.agentAddress.Location = new System.Drawing.Point(166, 6);
            this.agentAddress.Name = "agentAddress";
            this.agentAddress.Size = new System.Drawing.Size(303, 23);
            this.agentAddress.TabIndex = 2;
            this.agentAddress.Text = "agent.mtconnect.org";
            // 
            // connect
            // 
            this.connect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connect.Location = new System.Drawing.Point(475, 4);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(76, 26);
            this.connect.TabIndex = 3;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.propertyList);
            this.groupBox1.Controls.Add(this.agentTree);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(254, 445);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Agent Structure";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.recordData);
            this.groupBox2.Controls.Add(this.dataList);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(272, 221);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(500, 256);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data";
            // 
            // dataPlot
            // 
            this.dataPlot.AllowDrop = true;
            chartArea1.Name = "ChartArea1";
            this.dataPlot.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.dataPlot.Legends.Add(legend1);
            this.dataPlot.Location = new System.Drawing.Point(272, 51);
            this.dataPlot.Name = "dataPlot";
            this.dataPlot.Size = new System.Drawing.Size(500, 164);
            this.dataPlot.TabIndex = 6;
            this.dataPlot.Text = "chart1";
            // 
            // dataList
            // 
            this.dataList.AllowDrop = true;
            this.dataList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader6,
            this.columnHeader7});
            this.dataList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.dataList.Location = new System.Drawing.Point(7, 53);
            this.dataList.Name = "dataList";
            this.dataList.Size = new System.Drawing.Size(487, 197);
            this.dataList.TabIndex = 0;
            this.dataList.UseCompatibleStateImageBehavior = false;
            this.dataList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "category";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "id";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "name";
            // 
            // propertyList
            // 
            this.propertyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
            this.propertyList.GridLines = true;
            this.propertyList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.propertyList.Location = new System.Drawing.Point(6, 298);
            this.propertyList.Name = "propertyList";
            this.propertyList.Size = new System.Drawing.Size(242, 141);
            this.propertyList.TabIndex = 1;
            this.propertyList.UseCompatibleStateImageBehavior = false;
            this.propertyList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "type";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "value";
            // 
            // recordData
            // 
            this.recordData.AutoSize = true;
            this.recordData.Location = new System.Drawing.Point(7, 23);
            this.recordData.Name = "recordData";
            this.recordData.Size = new System.Drawing.Size(263, 21);
            this.recordData.TabIndex = 1;
            this.recordData.Text = "Record data to \'C:\\mtc_data.csv\'";
            this.recordData.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 482);
            this.Controls.Add(this.dataPlot);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.connect);
            this.Controls.Add(this.agentAddress);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "MTConnect Sample Client";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataPlot)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView agentTree;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox agentAddress;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView dataList;
        private System.Windows.Forms.DataVisualization.Charting.Chart dataPlot;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView propertyList;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.CheckBox recordData;
    }
}

