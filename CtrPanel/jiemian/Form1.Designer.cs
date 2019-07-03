namespace TorsionalTest
{
    partial class CtrPanel
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.comboBox_Device = new System.Windows.Forms.ComboBox();
            this.SearchGW = new System.Windows.Forms.Button();
            this.OpenGW = new System.Windows.Forms.Button();
            this.CloseGW = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.colGroup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colNode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.TextBox_SiteID = new System.Windows.Forms.TextBox();
            this.setEnable = new System.Windows.Forms.Button();
            this.setOff = new System.Windows.Forms.Button();
            this.setBegin = new System.Windows.Forms.Button();
            this.StopMotor = new System.Windows.Forms.Button();
            this.AutoSpd = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TextBox_JOGSpd = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.GetSpd = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cobProtocol = new System.Windows.Forms.ComboBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button1 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox_Device
            // 
            this.comboBox_Device.DropDownHeight = 45;
            this.comboBox_Device.DropDownWidth = 93;
            this.comboBox_Device.FormattingEnabled = true;
            this.comboBox_Device.IntegralHeight = false;
            this.comboBox_Device.ItemHeight = 15;
            this.comboBox_Device.Location = new System.Drawing.Point(407, 20);
            this.comboBox_Device.Name = "comboBox_Device";
            this.comboBox_Device.Size = new System.Drawing.Size(109, 23);
            this.comboBox_Device.TabIndex = 1;
            // 
            // SearchGW
            // 
            this.SearchGW.Location = new System.Drawing.Point(15, 20);
            this.SearchGW.Name = "SearchGW";
            this.SearchGW.Size = new System.Drawing.Size(97, 28);
            this.SearchGW.TabIndex = 2;
            this.SearchGW.Text = "查找网关";
            this.SearchGW.UseVisualStyleBackColor = true;
            this.SearchGW.Click += new System.EventHandler(this.SearchGW_Click);
            // 
            // OpenGW
            // 
            this.OpenGW.Location = new System.Drawing.Point(146, 20);
            this.OpenGW.Name = "OpenGW";
            this.OpenGW.Size = new System.Drawing.Size(97, 28);
            this.OpenGW.TabIndex = 3;
            this.OpenGW.Text = "打开网关";
            this.OpenGW.UseVisualStyleBackColor = true;
            this.OpenGW.Click += new System.EventHandler(this.OpenGW_Click);
            // 
            // CloseGW
            // 
            this.CloseGW.Location = new System.Drawing.Point(272, 20);
            this.CloseGW.Name = "CloseGW";
            this.CloseGW.Size = new System.Drawing.Size(97, 28);
            this.CloseGW.TabIndex = 4;
            this.CloseGW.Text = "关闭网关";
            this.CloseGW.UseVisualStyleBackColor = true;
            this.CloseGW.Click += new System.EventHandler(this.CloseGW_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 173);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "站点列表:";
            // 
            // listView1
            // 
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGroup,
            this.colNode,
            this.colType,
            this.colVersion});
            this.listView1.Font = new System.Drawing.Font("宋体", 10F);
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(15, 191);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(501, 112);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
            // 
            // colGroup
            // 
            this.colGroup.Text = "组号";
            this.colGroup.Width = 90;
            // 
            // colNode
            // 
            this.colNode.Text = "站点";
            this.colNode.Width = 90;
            // 
            // colType
            // 
            this.colType.Text = "型号";
            this.colType.Width = 90;
            // 
            // colVersion
            // 
            this.colVersion.Text = "固件";
            this.colVersion.Width = 100;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(257, 135);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "当前站点：";
            // 
            // TextBox_SiteID
            // 
            this.TextBox_SiteID.Location = new System.Drawing.Point(361, 127);
            this.TextBox_SiteID.Name = "TextBox_SiteID";
            this.TextBox_SiteID.ReadOnly = true;
            this.TextBox_SiteID.Size = new System.Drawing.Size(155, 25);
            this.TextBox_SiteID.TabIndex = 9;
            this.TextBox_SiteID.TextChanged += new System.EventHandler(this.TextBox_SiteID_TextChanged);
            // 
            // setEnable
            // 
            this.setEnable.Location = new System.Drawing.Point(15, 332);
            this.setEnable.Name = "setEnable";
            this.setEnable.Size = new System.Drawing.Size(95, 45);
            this.setEnable.TabIndex = 12;
            this.setEnable.Text = "使能";
            this.setEnable.UseVisualStyleBackColor = true;
            this.setEnable.Click += new System.EventHandler(this.setEnable_Click);
            // 
            // setOff
            // 
            this.setOff.Location = new System.Drawing.Point(149, 395);
            this.setOff.Name = "setOff";
            this.setOff.Size = new System.Drawing.Size(94, 45);
            this.setOff.TabIndex = 13;
            this.setOff.Text = "脱机";
            this.setOff.UseVisualStyleBackColor = true;
            this.setOff.Click += new System.EventHandler(this.setOff_Click);
            // 
            // setBegin
            // 
            this.setBegin.Location = new System.Drawing.Point(149, 332);
            this.setBegin.Name = "setBegin";
            this.setBegin.Size = new System.Drawing.Size(94, 45);
            this.setBegin.TabIndex = 14;
            this.setBegin.Text = "开始";
            this.setBegin.UseVisualStyleBackColor = true;
            this.setBegin.Click += new System.EventHandler(this.setBegin_Click);
            // 
            // StopMotor
            // 
            this.StopMotor.Location = new System.Drawing.Point(15, 395);
            this.StopMotor.Name = "StopMotor";
            this.StopMotor.Size = new System.Drawing.Size(95, 45);
            this.StopMotor.TabIndex = 15;
            this.StopMotor.Text = "停止";
            this.StopMotor.UseVisualStyleBackColor = true;
            this.StopMotor.Click += new System.EventHandler(this.StopMotor_Click);
            // 
            // AutoSpd
            // 
            this.AutoSpd.Location = new System.Drawing.Point(288, 333);
            this.AutoSpd.Name = "AutoSpd";
            this.AutoSpd.Size = new System.Drawing.Size(95, 44);
            this.AutoSpd.TabIndex = 16;
            this.AutoSpd.Text = "自动测试";
            this.AutoSpd.UseVisualStyleBackColor = true;
            this.AutoSpd.Click += new System.EventHandler(this.AutoSpd_Click_1);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.dataGridView1.Location = new System.Drawing.Point(12, 483);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(389, 361);
            this.dataGridView1.TabIndex = 17;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick_1);
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.FillWeight = 91.37056F;
            this.Column1.HeaderText = "转速 r/min";
            this.Column1.Name = "Column1";
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column2.FillWeight = 108.6294F;
            this.Column2.HeaderText = "加速度有效值/g";
            this.Column2.Name = "Column2";
            // 
            // TextBox_JOGSpd
            // 
            this.TextBox_JOGSpd.Location = new System.Drawing.Point(422, 395);
            this.TextBox_JOGSpd.Multiline = true;
            this.TextBox_JOGSpd.Name = "TextBox_JOGSpd";
            this.TextBox_JOGSpd.Size = new System.Drawing.Size(94, 45);
            this.TextBox_JOGSpd.TabIndex = 19;
            this.TextBox_JOGSpd.TextChanged += new System.EventHandler(this.TextBox_JOGSpd_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(530, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 15);
            this.label4.TabIndex = 20;
            this.label4.Text = "时域信号：";
            // 
            // GetSpd
            // 
            this.GetSpd.Location = new System.Drawing.Point(288, 395);
            this.GetSpd.Name = "GetSpd";
            this.GetSpd.Size = new System.Drawing.Size(95, 45);
            this.GetSpd.TabIndex = 22;
            this.GetSpd.Text = "速度查询";
            this.GetSpd.UseVisualStyleBackColor = true;
            this.GetSpd.Click += new System.EventHandler(this.GetSpd_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(12, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 15);
            this.label5.TabIndex = 24;
            this.label5.Text = "协议类型：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(257, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 15);
            this.label1.TabIndex = 25;
            this.label1.Text = "本地IP地址：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(12, 135);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 26;
            this.label6.Text = "端口号：";
            // 
            // cobProtocol
            // 
            this.cobProtocol.FormattingEnabled = true;
            this.cobProtocol.Items.AddRange(new object[] {
            "TCP Server"});
            this.cobProtocol.Location = new System.Drawing.Point(100, 76);
            this.cobProtocol.Name = "cobProtocol";
            this.cobProtocol.Size = new System.Drawing.Size(143, 23);
            this.cobProtocol.TabIndex = 27;
            this.cobProtocol.SelectedIndexChanged += new System.EventHandler(this.cobProtocol_SelectedIndexChanged);
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(361, 74);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(155, 25);
            this.txtIP.TabIndex = 28;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(100, 127);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(143, 25);
            this.txtPort.TabIndex = 29;
            this.txtPort.Text = "8081";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(404, 465);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 15);
            this.label7.TabIndex = 32;
            this.label7.Text = "数据接收：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(530, 487);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(150, 15);
            this.label8.TabIndex = 34;
            this.label8.Text = "转速-加速度关系图：";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBox1.Location = new System.Drawing.Point(407, 483);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(109, 361);
            this.richTextBox1.TabIndex = 39;
            this.richTextBox1.Text = "";
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(533, 505);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.Name = "Acceleration";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(729, 339);
            this.chart1.TabIndex = 40;
            this.chart1.Text = "chart1";
            // 
            // timer1
            // 
            this.timer1.Interval = 2;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Interval = 20000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // timer3
            // 
            this.timer3.Interval = 10000;
            this.timer3.Tick += new System.EventHandler(this.timer3_Tick);
            // 
            // chart2
            // 
            this.chart2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.chart2.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.chart2.Legends.Add(legend2);
            this.chart2.Location = new System.Drawing.Point(533, 25);
            this.chart2.Name = "chart2";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "Acceleration";
            this.chart2.Series.Add(series2);
            this.chart2.Size = new System.Drawing.Size(729, 450);
            this.chart2.TabIndex = 41;
            this.chart2.Text = "chart2";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(422, 332);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(94, 45);
            this.button1.TabIndex = 42;
            this.button1.Text = "中止测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 465);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(135, 15);
            this.label9.TabIndex = 43;
            this.label9.Text = "转速-加速度表格：";
            // 
            // CtrPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1278, 856);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.chart2);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.cobProtocol);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.GetSpd);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TextBox_JOGSpd);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.AutoSpd);
            this.Controls.Add(this.StopMotor);
            this.Controls.Add(this.setBegin);
            this.Controls.Add(this.setOff);
            this.Controls.Add(this.setEnable);
            this.Controls.Add(this.TextBox_SiteID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CloseGW);
            this.Controls.Add(this.OpenGW);
            this.Controls.Add(this.SearchGW);
            this.Controls.Add(this.comboBox_Device);
            this.Name = "CtrPanel";
            this.Text = " ";
            this.Load += new System.EventHandler(this.CtrPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBox_Device;
        private System.Windows.Forms.Button SearchGW;
        private System.Windows.Forms.Button OpenGW;
        private System.Windows.Forms.Button CloseGW;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader colGroup;
        private System.Windows.Forms.ColumnHeader colNode;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextBox_SiteID;
        private System.Windows.Forms.Button setEnable;
        private System.Windows.Forms.Button setOff;
        private System.Windows.Forms.Button setBegin;
        private System.Windows.Forms.Button StopMotor;
        private System.Windows.Forms.Button AutoSpd;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox TextBox_JOGSpd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button GetSpd;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cobProtocol;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
    }
}

