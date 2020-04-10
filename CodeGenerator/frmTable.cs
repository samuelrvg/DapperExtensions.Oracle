﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using RazorEngine;

namespace CodeGenerator
{
    public partial class frmTable : Form
    {

        #region Method

        List<TableEntity> tableList;

        private List<TableEntity> GetSelectTableList()
        {
            List<TableEntity> tables = new List<TableEntity>();
            // DataGridCell cel=(sender as DataGridCell).
            int count = Convert.ToInt16(this.dataGridView1.Rows.Count.ToString());
            for (int i = 0; i < count; i++)
            {
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["Check"];
                Boolean flag = Convert.ToBoolean(checkCell.Value);
                if (flag == true) //查找被选择的数据行
                {
                    DataGridViewTextBoxCell name = (DataGridViewTextBoxCell)dataGridView1.Rows[i].Cells["TableName"];
                    DataGridViewTextBoxCell descript = (DataGridViewTextBoxCell)dataGridView1.Rows[i].Cells["TableDescript"];
                    TableEntity table = tableList.FirstOrDefault(f => f.Name == name.Value.ToString());
                    try
                    {
                        table.Comment = descript.Value.ToString();
                    }
                    catch { }
                    table.NameUpper = MyUtils.ToUpper(table.Name);
                    table.NameLower = MyUtils.ToLower(table.Name);
                    if (string.IsNullOrEmpty(table.IsIdentity))
                    {
                        table.IsIdentity = "false";
                    }
                    tables.Add(table);
                }
                continue;
            }

            return tables;
        }

        //全选
        private void SelectAll()
        {
            int count = Convert.ToInt16(this.dataGridView1.Rows.Count.ToString());
            for (int i = 0; i < count; i++)
            {
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["Check"];
                Boolean flag = Convert.ToBoolean(checkCell.Value);
                if (flag == false) //查找被选择的数据行
                {
                    checkCell.Value = true;
                }
                continue;
            }

        }

        //全不选
        private void UnSelectAll()
        {
            int count = Convert.ToInt16(this.dataGridView1.Rows.Count.ToString());
            for (int i = 0; i < count; i++)
            {
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["Check"];
                Boolean flag = Convert.ToBoolean(checkCell.Value);
                if (flag == true) //查找被选择的数据行
                {
                    checkCell.Value = false;

                }
                continue;
            }

        }

        private void InitTableList(List<TableEntity> tableList)
        {
            for (int i = 0; i < tableList.Count(); i++)
            {
                TableEntity table = tableList[i];
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[1].Value = table.Name;
                dataGridView1.Rows[i].Cells[2].Value = table.Comment;
            }

        }

        #endregion

        public frmTable()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }



        private void frmTable_Load(object sender, EventArgs e)
        {
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            //禁止以列排序;
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            try
            {

                using (var conn = DbHelper.GetConn())
                {
                    tableList = DbHelper.GetBuilder().GetTableList();
                }

                InitTableList(tableList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }

        //行鼠标点击事件
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            //checkbox 勾上
            if ((bool)dataGridView1.Rows[e.RowIndex].Cells[0].EditedFormattedValue == true)
            {
                this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = false;
            }
            else
            {
                this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = true;
            }
        }

        //全选
        private void button3_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        //不选
        private void button2_Click(object sender, EventArgs e)
        {
            UnSelectAll();
        }

        //添加行号
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new Rectangle(e.RowBounds.Location.X, e.RowBounds.Y, this.dataGridView1.RowHeadersWidth - 4, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(), this.dataGridView1.RowHeadersDefaultCellStyle.Font, rectangle, this.dataGridView1.RowHeadersDefaultCellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        //退出
        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        //开始
        private void button1_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(ConfigHelper.Template))
            {
                MessageBox.Show("can no find template---> " + ConfigHelper.Template);
                return;
            }
            string content = System.IO.File.ReadAllText(ConfigHelper.Template, System.Text.Encoding.UTF8);
            List<TableEntity> tables = GetSelectTableList();
            if (tables.Count == 0)
            {
                MessageBox.Show("请选择表(please select table)");
                return;
            }

            if (!System.IO.Directory.Exists(ConfigHelper.OutPutDir))
                System.IO.Directory.CreateDirectory(ConfigHelper.OutPutDir);

            System.Diagnostics.Process.Start(ConfigHelper.OutPutDir);
            System.Text.Encoding utf8;
            if (ConfigHelper.FileEncoding == "utf8 with bom")
            {
                utf8 = System.Text.Encoding.UTF8;
            }
            else
            {
                utf8 = new UTF8Encoding(false);
            }

            //开启一个线程来生成代码
            new Thread(() =>
            {
                string error = null;
                string errorFile = ConfigHelper.ApplicationPath + "\\error.txt";
                int i = 0;
                foreach (var table in tables)
                {
                    try
                    {
                        string className = table.NameUpper + ConfigHelper.ClassSuffix;
                        string fileName = ConfigHelper.OutPutDir + "\\" + className + ConfigHelper.FileType;
                        List<ColumnEntity> columnList;
                        using (var conn = DbHelper.GetConn())
                        {
                            columnList = DbHelper.GetBuilder().GetColumnList(table);
                        }
                        string result = Razor.Parse(content, new { Table = table, ColumnList = columnList, ClassName = className, NameSpace = ConfigHelper.NameSpace });
                        System.IO.File.WriteAllText(fileName, result, utf8);
                    }
                    catch (Exception ex)
                    {
                        i++;
                        error += "===================================" + (i) + "========================================\r\n";
                        error += table.Name + "\r\n";
                        error += ex.Message + "\r\n\r\n";
                    }

                    Thread.Sleep(1);
                }

                if (!string.IsNullOrEmpty(error))
                {
                    System.IO.File.WriteAllText(errorFile, error, utf8);
                    MessageBox.Show(this, i + " error,please see error.txt");
                    System.Diagnostics.Process.Start(ConfigHelper.ApplicationPath);
                }
                else
                {

                    MessageBox.Show(this, "ok");
                }


            }) { IsBackground = true }.Start();



        }


    }
}
