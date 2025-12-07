using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Database;
using Core.Models;
using LiveCharts;
using LiveCharts.Definitions.Charts;
using LiveCharts.WinForms;
using LiveCharts.Wpf; // bazı özellikler için

namespace UI.Forms
{
    public partial class HistoryChartForm : Form
    {
        private readonly int _tagId; // Tag ID'sini saklayalım
        private readonly DatabaseService _db;

        public HistoryChartForm(Tag tag)
        {
            InitializeComponent();
            _tagId = tag.TagId;
            _db = new DatabaseService();

            this.Text = $"Geçmiş Değerler - {tag.Name}";

            // Varsayılan tarihleri ayarla (Bugün ve Dün)
            dtStart.Value = DateTime.Today.AddDays(-1);
            dtEnd.Value = DateTime.Now;

            // Form açılınca grafiği ilk kez yükle
            LoadChartData();
        }

        private void LoadChartData()
        {
            // 1. Tarihleri al
            DateTime start = dtStart.Value;
            DateTime end = dtEnd.Value;

            // 2. Veritabanından o aralığı çek
            var history = _db.GetTagHistory(_tagId, start, end);

            var values = new ChartValues<double>();
            var labels = new List<string>();

            foreach (var entry in history)
            {
                values.Add(entry.Value);
                // SORUNUN ÇÖZÜMÜ: Tarih formatını burada değiştiriyoruz
                labels.Add(entry.Timestamp.ToString("dd.MM HH:mm:ss"));
            }

            // 3. Grafiği güncelle
            // Eğer cartesianChart1 tasarım ekranında ekliyse:
            cartesianChart1.Series.Clear();
            cartesianChart1.AxisX.Clear();
            cartesianChart1.AxisY.Clear();

            cartesianChart1.Series = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Değer",
                Values = values,
                PointGeometrySize = 7,
                PointGeometry = DefaultGeometries.Circle
            }
        };

            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "Zaman",
                Labels = labels,
                Separator = new Separator { Step = 10 } // Etiketler üst üste binmesin diye adım ayarı
            });

            cartesianChart1.AxisY.Add(new Axis
            {
                Title = "Değer"
            });
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadChartData();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

