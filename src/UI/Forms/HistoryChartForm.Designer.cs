namespace UI.Forms
{
    partial class HistoryChartForm
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
            panel1 = new Panel();
            label2 = new Label();
            label1 = new Label();
            btnLoad = new Button();
            dtEnd = new DateTimePicker();
            dtStart = new DateTimePicker();
            cartesianChart1 = new LiveCharts.WinForms.CartesianChart();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(btnLoad);
            panel1.Controls.Add(dtEnd);
            panel1.Controls.Add(dtStart);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(921, 63);
            panel1.TabIndex = 4;
            panel1.Paint += panel1_Paint;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(411, 26);
            label2.Name = "label2";
            label2.Size = new Size(79, 20);
            label2.TabIndex = 9;
            label2.Text = "Bitiş Tarihi:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(55, 26);
            label1.Name = "label1";
            label1.Size = new Size(114, 20);
            label1.TabIndex = 8;
            label1.Text = "Başlangıç Tarihi:";
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(775, 12);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(102, 39);
            btnLoad.TabIndex = 7;
            btnLoad.Text = "Filtrele";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // dtEnd
            // 
            dtEnd.Location = new Point(496, 21);
            dtEnd.Name = "dtEnd";
            dtEnd.Size = new Size(210, 27);
            dtEnd.TabIndex = 6;
            // 
            // dtStart
            // 
            dtStart.Location = new Point(175, 21);
            dtStart.Name = "dtStart";
            dtStart.Size = new Size(210, 27);
            dtStart.TabIndex = 5;
            // 
            // cartesianChart1
            // 
            cartesianChart1.Dock = DockStyle.Fill;
            cartesianChart1.Location = new Point(0, 63);
            cartesianChart1.Name = "cartesianChart1";
            cartesianChart1.Size = new Size(921, 387);
            cartesianChart1.TabIndex = 5;
            cartesianChart1.Text = "cartesianChart1";
            // 
            // HistoryChartForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(921, 450);
            Controls.Add(cartesianChart1);
            Controls.Add(panel1);
            Name = "HistoryChartForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TrendForm";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnLoad;
        private DateTimePicker dtEnd;
        private DateTimePicker dtStart;
        private LiveCharts.WinForms.CartesianChart cartesianChart1;
        private Label label2;
        private Label label1;
    }
}