using System.Windows.Forms;

namespace EngineeringGUI
{
    partial class Form1
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
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ErymanthosSearchBtn = new System.Windows.Forms.Button();
            this.ErymanBoardsCmbBx = new System.Windows.Forms.ComboBox();
            this.ErymanBoardLbl = new System.Windows.Forms.Label();
            this.StatusLbl = new System.Windows.Forms.Label();
            this.SetTempTxtBx = new System.Windows.Forms.TextBox();
            this.SelectBayNumberCmbBx = new System.Windows.Forms.ComboBox();
            this.BayPositonLbl = new System.Windows.Forms.Label();
            this.SetTempLbl = new System.Windows.Forms.Label();
            this.SetSelectedBayBtn = new System.Windows.Forms.Button();
            this.DegreeLbl = new System.Windows.Forms.Label();
            this.ReadTemperaturesBtn = new System.Windows.Forms.Button();
            this.ModuleTempsDataGridView = new System.Windows.Forms.DataGridView();
            this.SetAllBaysBtn = new System.Windows.Forms.Button();
            this.ModuleTempBx = new System.Windows.Forms.GroupBox();
            this.TimeLbl = new System.Windows.Forms.Label();
            this.RefreshAtLbl = new System.Windows.Forms.Label();
            this.LivePIDChrt = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.ModuleTempsDataGridView)).BeginInit();
            this.ModuleTempBx.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LivePIDChrt)).BeginInit();
            this.SuspendLayout();
            // 
            // ErymanthosSearchBtn
            // 
            this.ErymanthosSearchBtn.Location = new System.Drawing.Point(545, 6);
            this.ErymanthosSearchBtn.Margin = new System.Windows.Forms.Padding(4);
            this.ErymanthosSearchBtn.Name = "ErymanthosSearchBtn";
            this.ErymanthosSearchBtn.Size = new System.Drawing.Size(313, 52);
            this.ErymanthosSearchBtn.TabIndex = 80;
            this.ErymanthosSearchBtn.Text = "Scan For Erymanthos Boards";
            this.ErymanthosSearchBtn.UseVisualStyleBackColor = true;
            this.ErymanthosSearchBtn.Click += new System.EventHandler(this.ErmanthosSearchBtn);
            // 
            // ErymanBoardsCmbBx
            // 
            this.ErymanBoardsCmbBx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ErymanBoardsCmbBx.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErymanBoardsCmbBx.FormattingEnabled = true;
            this.ErymanBoardsCmbBx.Items.AddRange(new object[] {
            "SET CRC 0"});
            this.ErymanBoardsCmbBx.Location = new System.Drawing.Point(239, 15);
            this.ErymanBoardsCmbBx.Margin = new System.Windows.Forms.Padding(4);
            this.ErymanBoardsCmbBx.Name = "ErymanBoardsCmbBx";
            this.ErymanBoardsCmbBx.Size = new System.Drawing.Size(297, 33);
            this.ErymanBoardsCmbBx.TabIndex = 74;
            this.ErymanBoardsCmbBx.SelectedIndexChanged += new System.EventHandler(this.AvailErymanBoards_SelectedIndexChanged);
            // 
            // ErymanBoardLbl
            // 
            this.ErymanBoardLbl.AutoSize = true;
            this.ErymanBoardLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErymanBoardLbl.Location = new System.Drawing.Point(31, 17);
            this.ErymanBoardLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ErymanBoardLbl.Name = "ErymanBoardLbl";
            this.ErymanBoardLbl.Size = new System.Drawing.Size(185, 25);
            this.ErymanBoardLbl.TabIndex = 73;
            this.ErymanBoardLbl.Text = "Eryman Base Port";
            // 
            // StatusLbl
            // 
            this.StatusLbl.AutoSize = true;
            this.StatusLbl.BackColor = System.Drawing.Color.Red;
            this.StatusLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLbl.Location = new System.Drawing.Point(812, 23);
            this.StatusLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.StatusLbl.Name = "StatusLbl";
            this.StatusLbl.Size = new System.Drawing.Size(151, 25);
            this.StatusLbl.TabIndex = 86;
            this.StatusLbl.Text = "Not connected";
            // 
            // SetTempTxtBx
            // 
            this.SetTempTxtBx.Location = new System.Drawing.Point(23, 126);
            this.SetTempTxtBx.Margin = new System.Windows.Forms.Padding(4);
            this.SetTempTxtBx.Name = "SetTempTxtBx";
            this.SetTempTxtBx.Size = new System.Drawing.Size(95, 22);
            this.SetTempTxtBx.TabIndex = 103;
            // 
            // SelectBayNumberCmbBx
            // 
            this.SelectBayNumberCmbBx.FormattingEnabled = true;
            this.SelectBayNumberCmbBx.Items.AddRange(new object[] {
            "UART",
            "CUSTOMER",
            "SWD"});
            this.SelectBayNumberCmbBx.Location = new System.Drawing.Point(56, 58);
            this.SelectBayNumberCmbBx.Margin = new System.Windows.Forms.Padding(4);
            this.SelectBayNumberCmbBx.Name = "SelectBayNumberCmbBx";
            this.SelectBayNumberCmbBx.Size = new System.Drawing.Size(95, 24);
            this.SelectBayNumberCmbBx.TabIndex = 104;
            // 
            // BayPositonLbl
            // 
            this.BayPositonLbl.AutoSize = true;
            this.BayPositonLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BayPositonLbl.Location = new System.Drawing.Point(64, 34);
            this.BayPositonLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BayPositonLbl.Name = "BayPositonLbl";
            this.BayPositonLbl.Size = new System.Drawing.Size(77, 20);
            this.BayPositonLbl.TabIndex = 105;
            this.BayPositonLbl.Text = "Position";
            // 
            // SetTempLbl
            // 
            this.SetTempLbl.AutoSize = true;
            this.SetTempLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetTempLbl.Location = new System.Drawing.Point(54, 102);
            this.SetTempLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SetTempLbl.Name = "SetTempLbl";
            this.SetTempLbl.Size = new System.Drawing.Size(89, 20);
            this.SetTempLbl.TabIndex = 106;
            this.SetTempLbl.Text = "Set Temp";
            // 
            // SetSelectedBayBtn
            // 
            this.SetSelectedBayBtn.Enabled = false;
            this.SetSelectedBayBtn.Location = new System.Drawing.Point(23, 168);
            this.SetSelectedBayBtn.Margin = new System.Windows.Forms.Padding(4);
            this.SetSelectedBayBtn.Name = "SetSelectedBayBtn";
            this.SetSelectedBayBtn.Size = new System.Drawing.Size(161, 31);
            this.SetSelectedBayBtn.TabIndex = 107;
            this.SetSelectedBayBtn.Text = "Set Selected Bay";
            this.SetSelectedBayBtn.UseVisualStyleBackColor = true;
            this.SetSelectedBayBtn.Click += new System.EventHandler(this.SetSelectedBayBtn_Click);
            // 
            // DegreeLbl
            // 
            this.DegreeLbl.AutoSize = true;
            this.DegreeLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DegreeLbl.Location = new System.Drawing.Point(136, 126);
            this.DegreeLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DegreeLbl.Name = "DegreeLbl";
            this.DegreeLbl.Size = new System.Drawing.Size(48, 20);
            this.DegreeLbl.TabIndex = 108;
            this.DegreeLbl.Text = "degC";
            // 
            // ReadTemperaturesBtn
            // 
            this.ReadTemperaturesBtn.Enabled = false;
            this.ReadTemperaturesBtn.Location = new System.Drawing.Point(569, 23);
            this.ReadTemperaturesBtn.Margin = new System.Windows.Forms.Padding(4);
            this.ReadTemperaturesBtn.Name = "ReadTemperaturesBtn";
            this.ReadTemperaturesBtn.Size = new System.Drawing.Size(161, 31);
            this.ReadTemperaturesBtn.TabIndex = 111;
            this.ReadTemperaturesBtn.Text = "Read All Bays";
            this.ReadTemperaturesBtn.UseVisualStyleBackColor = true;
            this.ReadTemperaturesBtn.Click += new System.EventHandler(this.ReadTemperaturesBtn_Click);
            // 
            // ModuleTempsDataGridView
            // 
            this.ModuleTempsDataGridView.AllowUserToAddRows = false;
            this.ModuleTempsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ModuleTempsDataGridView.Location = new System.Drawing.Point(206, 62);
            this.ModuleTempsDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.ModuleTempsDataGridView.Name = "ModuleTempsDataGridView";
            this.ModuleTempsDataGridView.RowHeadersWidth = 51;
            this.ModuleTempsDataGridView.Size = new System.Drawing.Size(741, 185);
            this.ModuleTempsDataGridView.TabIndex = 112;
            // 
            // SetAllBaysBtn
            // 
            this.SetAllBaysBtn.Enabled = false;
            this.SetAllBaysBtn.Location = new System.Drawing.Point(23, 207);
            this.SetAllBaysBtn.Margin = new System.Windows.Forms.Padding(4);
            this.SetAllBaysBtn.Name = "SetAllBaysBtn";
            this.SetAllBaysBtn.Size = new System.Drawing.Size(161, 31);
            this.SetAllBaysBtn.TabIndex = 114;
            this.SetAllBaysBtn.Text = "Set All Bays";
            this.SetAllBaysBtn.UseVisualStyleBackColor = true;
            this.SetAllBaysBtn.Click += new System.EventHandler(this.SetAllBaysBtn_Click);
            // 
            // ModuleTempBx
            // 
            this.ModuleTempBx.Controls.Add(this.TimeLbl);
            this.ModuleTempBx.Controls.Add(this.RefreshAtLbl);
            this.ModuleTempBx.Controls.Add(this.SetAllBaysBtn);
            this.ModuleTempBx.Controls.Add(this.ModuleTempsDataGridView);
            this.ModuleTempBx.Controls.Add(this.ReadTemperaturesBtn);
            this.ModuleTempBx.Controls.Add(this.DegreeLbl);
            this.ModuleTempBx.Controls.Add(this.SetSelectedBayBtn);
            this.ModuleTempBx.Controls.Add(this.SetTempLbl);
            this.ModuleTempBx.Controls.Add(this.BayPositonLbl);
            this.ModuleTempBx.Controls.Add(this.SelectBayNumberCmbBx);
            this.ModuleTempBx.Controls.Add(this.SetTempTxtBx);
            this.ModuleTempBx.Location = new System.Drawing.Point(16, 66);
            this.ModuleTempBx.Margin = new System.Windows.Forms.Padding(4);
            this.ModuleTempBx.Name = "ModuleTempBx";
            this.ModuleTempBx.Padding = new System.Windows.Forms.Padding(4);
            this.ModuleTempBx.Size = new System.Drawing.Size(958, 268);
            this.ModuleTempBx.TabIndex = 107;
            this.ModuleTempBx.TabStop = false;
            this.ModuleTempBx.Text = "Module Temperatures table";
            // 
            // TimeLbl
            // 
            this.TimeLbl.AutoSize = true;
            this.TimeLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TimeLbl.Location = new System.Drawing.Point(848, 27);
            this.TimeLbl.Name = "TimeLbl";
            this.TimeLbl.Size = new System.Drawing.Size(91, 20);
            this.TimeLbl.TabIndex = 116;
            this.TimeLbl.Text = "HH:mm:ss";
            this.TimeLbl.Visible = false;
            // 
            // RefreshAtLbl
            // 
            this.RefreshAtLbl.AutoSize = true;
            this.RefreshAtLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RefreshAtLbl.Location = new System.Drawing.Point(737, 27);
            this.RefreshAtLbl.Name = "RefreshAtLbl";
            this.RefreshAtLbl.Size = new System.Drawing.Size(105, 20);
            this.RefreshAtLbl.TabIndex = 115;
            this.RefreshAtLbl.Text = "Refreshed at";
            this.RefreshAtLbl.Visible = false;
            // 
            // LivePIDChrt
            // 
            chartArea1.AxisX.Maximum = 6D;
            chartArea1.AxisX.Title = "Bay";
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.AxisY.Title = "Tuning Rate";
            chartArea1.Name = "PID chart";
            this.LivePIDChrt.ChartAreas.Add(chartArea1);
            legend1.Name = "Tuning Rate";
            this.LivePIDChrt.Legends.Add(legend1);
            this.LivePIDChrt.Location = new System.Drawing.Point(16, 319);
            this.LivePIDChrt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.LivePIDChrt.Name = "LivePIDChrt";
            this.LivePIDChrt.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "PID chart";
            series1.Color = System.Drawing.Color.Black;
            series1.Legend = "Tuning Rate";
            series1.Name = "TECLoop";
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "PID chart";
            series2.Color = System.Drawing.Color.Red;
            series2.Legend = "Tuning Rate";
            series2.Name = "Set";
            series3.ChartArea = "PID chart";
            series3.Color = System.Drawing.Color.DarkGray;
            series3.Legend = "Tuning Rate";
            series3.Name = "Case";
            series4.ChartArea = "PID chart";
            series4.Color = System.Drawing.Color.Blue;
            series4.Legend = "Tuning Rate";
            series4.Name = "Water";
            this.LivePIDChrt.Series.Add(series1);
            this.LivePIDChrt.Series.Add(series2);
            this.LivePIDChrt.Series.Add(series3);
            this.LivePIDChrt.Series.Add(series4);
            this.LivePIDChrt.Size = new System.Drawing.Size(958, 324);
            this.LivePIDChrt.TabIndex = 108;
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Underline);
            title1.Name = "Live chart";
            title1.Text = "PID temperature chart";
            this.LivePIDChrt.Titles.Add(title1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 658);
            this.Controls.Add(this.LivePIDChrt);
            this.Controls.Add(this.ModuleTempBx);
            this.Controls.Add(this.StatusLbl);
            this.Controls.Add(this.ErymanBoardsCmbBx);
            this.Controls.Add(this.ErymanthosSearchBtn);
            this.Controls.Add(this.ErymanBoardLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = " ";
            this.Text = "Erymanthos GUI";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ModuleTempsDataGridView)).EndInit();
            this.ModuleTempBx.ResumeLayout(false);
            this.ModuleTempBx.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LivePIDChrt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Button ErymanthosSearchBtn;
        private ComboBox ErymanBoardsCmbBx;
        private Label ErymanBoardLbl;
        private Label StatusLbl;
        private TextBox SetTempTxtBx;
        private ComboBox SelectBayNumberCmbBx;
        private Label BayPositonLbl;
        private Label SetTempLbl;
        private Button SetSelectedBayBtn;
        private Label DegreeLbl;
        private Button ReadTemperaturesBtn;
        private DataGridView ModuleTempsDataGridView;
        private Button SetAllBaysBtn;
        private GroupBox ModuleTempBx;
        private System.Windows.Forms.DataVisualization.Charting.Chart LivePIDChrt;
        private Label RefreshAtLbl;
        private Label TimeLbl;
    }
}

