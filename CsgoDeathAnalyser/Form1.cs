using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CsgoDeathAnalyser
{
    public partial class Form1 : Form
    {
        Gradient gradient;

        public Form1()
        {
            InitializeComponent();   
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<Color> gradientColors = new List<Color>();
            gradientColors.Add(Color.FromArgb(204, 204, 204));
            gradientColors.Add(Color.FromArgb(230, 217, 179));
            gradientColors.Add(Color.FromArgb(255, 229, 153));
            gradientColors.Add(Color.FromArgb(245, 191, 153));
            gradientColors.Add(Color.FromArgb(234, 153, 153));

            this.gradient = new Gradient(gradientColors.ToArray());

            int rowCount = 15;

            for (int i = 0; i < rowCount; i++)
            {
                // большая таблица
                string[] rowData = new string[deathTable.Columns.Count];

                for (int j = 0; j < rowData.Length; j++)
                    rowData[j] = string.Empty;

                deathTable.Rows[deathTable.Rows.Add(rowData)].DefaultCellStyle.Alignment
                    = DataGridViewContentAlignment.MiddleCenter;

                // kdr таблица
                rowData = new string[kdrTable.Columns.Count];

                for (int j = 0; j < rowData.Length; j++)
                    rowData[j] = string.Empty;

                kdrTable.Rows[kdrTable.Rows.Add(rowData)].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            // украшаем средний столбец kdr таблицы
            foreach (DataGridViewCell cell in kdrTable.Rows.Cast<DataGridViewRow>().Select(r => r.Cells[1]))
                cell.Style.BackColor = Color.LightGray;

            // сводная таблица
            for (int i = 0; i < 2; i++)
            {
                string[] rowData = new string[summaryTable.Columns.Count];

                for (int j = 0; j < rowData.Length; j++)
                    rowData[j] = string.Empty;

                int index = summaryTable.Rows.Add(rowData);
                summaryTable.Rows[index].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Восстанавливаем данные из файла
            XmlSerializer serializer = new XmlSerializer(typeof(MatchStats[]));
            string path = "MatchStats.xml";

            if (File.Exists(path))
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    MatchStats[] matchStatsArray = (MatchStats[])serializer.Deserialize(fs);
                    
                    for (int i = 0; i < matchStatsArray.Length; i++)
                    {
                        kdrTable.Rows[i].Cells[0].Value = matchStatsArray[i].Kills;
                        kdrTable.Rows[i].Cells[2].Value = matchStatsArray[i].Deaths;

                        for (int j = 0; j < matchStatsArray[i].DeathsArray.Length; j++)
                        {
                            int deathCount = matchStatsArray[i].DeathsArray[j];
                            deathTable.Rows[i].Cells[j].Value = deathCount > 0 ? deathCount.ToString() : "";
                        }
                    }
                }                    
            }            

            UpdateData();
        }        

        void UpdateData()
        {
            int totalKills = 0;
            int totalDeaths = 0;            

            foreach (DataGridViewRow row in kdrTable.Rows)
            {
                row.Cells[0].Style.BackColor = row.Cells[2].Style.BackColor = Color.White;

                //string value1, value2;

                string value1 = row.Cells[0].Value == null? string.Empty : row.Cells[0].Value.ToString();
                string value2 = row.Cells[2].Value == null ? string.Empty : row.Cells[2].Value.ToString();

                if (value1 != string.Empty && value2 == string.Empty)
                    row.Cells[2].Style.BackColor = Color.Coral;
                else if (value2 != string.Empty && value1 == string.Empty)
                    row.Cells[0].Style.BackColor = Color.Coral;
                else if (value1 == string.Empty && value2 == string.Empty)
                {
                    if (deathTable.Rows[row.Index].Cells.Cast<DataGridViewCell>().Any(c => c.Value.ToString() != string.Empty))
                        deathTable.Rows[row.Index].DefaultCellStyle.BackColor = Color.DarkSalmon;
                    else
                        deathTable.Rows[row.Index].DefaultCellStyle.BackColor = Color.White;
                }                    
                else
                {
                    int kills = int.Parse(value1);
                    int deaths = int.Parse(value2);

                    totalKills += kills;
                    totalDeaths += deaths;

                    double kdr = (double) kills / deaths;

                    if (kdr < 1) row.Cells[1].Style.BackColor = Color.DarkSalmon;
                    else row.Cells[1].Style.BackColor = Color.LightGreen;

                    row.Cells[1].Value = kdr.ToString("F1");

                    // Проверяем, что указано нужное число киллов
                    int actualMatchDeathCount;
                    int deathsIncluded = 0;

                    foreach (DataGridViewCell cell in deathTable.Rows[row.Index].Cells)
                    {
                        int deathsInCell;

                        if (int.TryParse(cell.Value.ToString(), out deathsInCell))
                            deathsIncluded += deathsInCell;
                    }                    

                    if (int.TryParse(kdrTable.Rows[row.Index].Cells[2].Value.ToString(), out actualMatchDeathCount)
                        && deathsIncluded != actualMatchDeathCount)
                        deathTable.Rows[row.Index].DefaultCellStyle.BackColor = Color.DarkSalmon;
                    else
                        deathTable.Rows[row.Index].DefaultCellStyle.BackColor = Color.White;
                }
            }

            // Левый верхний угол
            if (totalKills != 0 && totalDeaths != 0)
                label1.Text = $"У: {totalKills}, С: {totalDeaths}";
            else
                label1.Text = "";
            

            if (totalDeaths > 0)
            {
                double rate = (double)totalKills / totalDeaths;
                label2.Text = rate.ToString("F1");

                if (rate < 1) label2.ForeColor = Color.Crimson;
                else label2.ForeColor = Color.DarkSeaGreen;
            }
            else
            {
                label2.Text = "-";
                label2.ForeColor = Color.Black;
            }

            // сводная таблица    
            if (summaryTable.Rows.Count > 0)       
            {
                // Проходим и заполняем значениями
                int maxCount = 0;

                for (int i = 0; i < summaryTable.Columns.Count; i++)
                {
                    int deathCount = 0;

                    foreach (DataGridViewRow row in deathTable.Rows)
                    {
                        string value = row.Cells[i].Value.ToString();

                        if (value == string.Empty) continue;

                        int currentDeathCount;

                        if (int.TryParse(value, out currentDeathCount))
                            deathCount += currentDeathCount;
                        else
                            row.Cells[i].Value = "";
                    }

                    maxCount = deathCount > maxCount ? deathCount : maxCount;

                    summaryTable.Rows[0].Cells[i].Value = deathCount;
                    if (deathCount > 0)
                    {
                        double rate = (double)deathCount / totalDeaths;
                        DataGridViewCell cell = summaryTable.Rows[1].Cells[i];
                        cell.Value = $"{Math.Round(rate * 100)}%"; //$"{(rate * 100).ToString("F1")}%";

                        //Color resultColor = gradient.GetColorAt(rate); //ColorBetween(leastRateColor, mostRateColor, rate);
                        //cell.Style.BackColor = resultColor;
                    }                        
                    else
                    {
                        summaryTable.Rows[1].Cells[i].Value = $"0%";
                        //summaryTable.Rows[1].Cells[i].Style.BackColor = leastRateColor;
                    }                        
                }

                // Задаем цвета, если не будет деления на ноль
                if (maxCount > 0)
                {
                    for (int i = 0; i < summaryTable.Columns.Count; i++)
                    {
                        //string deathValue = summaryTable.Rows[0].Cells[i].Value as string;
                        //if (deathValue == string.Empty) break;

                        int deathCount;// = double.Parse(deathValue) / maxCount;
                        if (int.TryParse(summaryTable.Rows[0].Cells[i].Value.ToString(), out deathCount))
                        {
                            double rate = (double)deathCount / maxCount;
                            Color resultColor = gradient.GetColorAt(rate);
                            summaryTable.Rows[1].Cells[i].Style.BackColor = resultColor;
                        }
                        else
                            break; // Значения еще не заданы
                    }
                }
                else
                {
                    // Иначе сбрасываем цвета
                    foreach (DataGridViewCell cell in summaryTable.Rows[1].Cells)
                        cell.Style.BackColor = Color.LightGray;
                }
            }            
        }

        private void kdrTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1) return;
            DataGridViewRow row = kdrTable.Rows[e.RowIndex];
            
            if (row.Cells[e.ColumnIndex].Value == null)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = "";
                    if (cell.ColumnIndex == 1) cell.Style.BackColor = Color.LightGray;
                }
                foreach (DataGridViewCell cell in deathTable.Rows[row.Index].Cells)
                    cell.Value = "";

            }
            UpdateData();
        }

        private void kdrTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            kdrTable.BeginEdit(true);
        }

        private void deathTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1) return;
            DataGridViewCell cell = deathTable.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (cell.Value == null) cell.Value = "";
            
            UpdateData();
        }

        private void deathTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            deathTable.BeginEdit(true);
        }
        
        private void summaryTable_SelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewCell selectedCell in summaryTable.SelectedCells)
                selectedCell.Selected = false;
        }
            
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MatchStats[]));
            List<MatchStats> matchStatsList = new List<MatchStats>();

            foreach (DataGridViewRow kdrRow in kdrTable.Rows)
            {
                string kills = kdrRow.Cells[0].Value.ToString();
                string deaths = kdrRow.Cells[2].Value.ToString();

                if (kills == string.Empty || deaths == string.Empty) continue;

                MatchStats stats = new MatchStats();
                stats.Kills = int.Parse(kills);
                stats.Deaths = int.Parse(deaths);

                int[] deathArray = new int[deathTable.ColumnCount];

                foreach (DataGridViewCell cell in deathTable.Rows[kdrRow.Index].Cells)
                {
                    string value = cell.Value.ToString();

                    if (value != string.Empty)
                        deathArray[cell.ColumnIndex] = int.Parse(value);
                    else
                        deathArray[cell.ColumnIndex] = 0;
                }
                stats.DeathsArray = deathArray;
                matchStatsList.Add(stats);
            }

            string path = "MatchStats.xml";
            if (File.Exists(path)) File.Delete(path);

            using (FileStream fs = File.OpenWrite(path))
                serializer.Serialize(fs, matchStatsList.ToArray());
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.ShowDialog();
        }

        public class MatchStats
        {
            public int Kills { get; set; }
            public int Deaths { get; set; }
            public int[] DeathsArray { get; set; }
        }
    }
}
