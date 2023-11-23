using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private OdbcConnection _connection;
        private int _jobNum;

        private string _jobName
        {
            get => ViewState["_jobName"] as string ?? string.Empty;
            set => ViewState["_jobName"] = value;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            // Создаем объект подключения
            _connection = new OdbcConnection();
            // Задаем параметр подключения – имя ODBC-источника
            _connection.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["connection"].ConnectionString;

            // Подключаемся к БД
            _connection.Open();
            // Определяем строку с текстом запроса
            string strSQL = "SELECT name FROM pmib0413.j";
            // Создаем объект запроса
            OdbcCommand cmd = new OdbcCommand(strSQL, _connection);

            OdbcTransaction tx = null;
            try
            {
                // Начинаем транзакцию и извлекаем объект транзакции из объекта подключения.
                tx = _connection.BeginTransaction();
                // Включаем объект SQL-команды в транзакцию
                cmd.Transaction = tx;
                // Выполняем SQL-команду и получаем количество обработанных записей
                OdbcDataReader i = cmd.ExecuteReader();
                // Подтверждаем транзакцию  


                if (JobDropDownList1.Items.Count == 0)
                {
                    while (i.Read())
                    {
                        var jobName = i["name"].ToString();

                        JobDropDownList1.Items.Add(new ListItem(jobName));
                    }
                }

                tx.Commit();

            }
            catch (Exception ex)
            {
                // При возникновении любой ошибки 
                // Формируем сообщение об ошибке 
                Utils.SetErrorStatus(Label5, ex.Message);
                // выполняем откат транзакции 
                tx.Rollback();
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            _connection.Close();
            _connection.Dispose();
        }

        protected void Button1_OnClick(object sender, EventArgs e)
        {
            if (!IsParametersValid())
            {
                return;
            }

            const string SQL = @"SELECT name, cvet, ves, town
                                 FROM pmib0413.p
                                 WHERE n_det in (SELECT pmib0413.q.n_det
                                                 FROM pmib0413.q
                                                 LEFT OUTER JOIN (SELECT pmib0413.spj1.n_det, sum(kol)
                                                                  FROM pmib0413.spj1
                                                                  WHERE n_izd = (SELECT n_izd
                                                                                 FROM pmib0413.j
                                                                                 WHERE name = ?)
                                                                  GROUP BY n_det) AS buf ON pmib0413.q.n_det= buf.n_det
                                                 WHERE pmib0413.q.n_izd = (SELECT n_izd
                                                                  FROM pmib0413.j
                                                                  WHERE name = ?)
                                                 AND (pmib0413.q.kol*? > buf.sum OR buf.n_det IS NULL))";

            using (OdbcCommand _command = new OdbcCommand(SQL, _connection))
            {
                SetParameters(_command);

                OdbcTransaction transaction = null;

                try
                {
                    transaction = _connection.BeginTransaction();
                    _command.Transaction = transaction;

                    OdbcDataReader dataReader = _command.ExecuteReader();

                    Table1.Rows.Clear();
                    Label7.Text = string.Empty;
                    if (dataReader.HasRows)
                    {
                        Table1.Rows.Add(GetHeadRow());
                        while (dataReader.Read())
                        {
                            HtmlTableRow row = new HtmlTableRow();
                            HtmlTableCell cell = new HtmlTableCell();

                            cell.InnerText = dataReader["name"].ToString();
                            row.Cells.Add(cell);

                            cell = new HtmlTableCell();
                            cell.InnerText = dataReader["cvet"].ToString();
                            row.Cells.Add(cell);

                            cell = new HtmlTableCell();
                            cell.InnerText = dataReader["ves"].ToString();
                            row.Cells.Add(cell);

                            cell = new HtmlTableCell();
                            cell.InnerText = dataReader["town"].ToString();
                            row.Cells.Add(cell);

                            Table1.Rows.Add(row);
                        }
                    }
                    else
                    {
                        Label7.Text = "Данных не найдено";
                    }
                
                    transaction.Commit();
                    Utils.SetGoodStatus(Label5);
                }
                catch (Exception ex)
                {
                    Utils.SetErrorStatus(Label5, ex.Message);
                    transaction.Rollback();
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
        }

        private HtmlTableRow GetHeadRow()
        {
            HtmlTableRow row = new HtmlTableRow();
            HtmlTableCell cell = new HtmlTableCell();

            cell.InnerText = "name";
            row.Cells.Add(cell);

            cell = new HtmlTableCell();
            cell.InnerText = "cvet";
            row.Cells.Add(cell);

            cell = new HtmlTableCell();
            cell.InnerText = "ves";
            row.Cells.Add(cell);

            cell = new HtmlTableCell();
            cell.InnerText = "town";
            row.Cells.Add(cell);

            return row;
        }

        private void SetParameters(OdbcCommand command)
        {
            OdbcParameter jobName = new OdbcParameter();
            jobName.OdbcType = OdbcType.Text;
            jobName.Value = _jobName;
            command.Parameters.Add(jobName);

            OdbcParameter jobName1 = new OdbcParameter();
            jobName1.OdbcType = OdbcType.Text;
            jobName1.Value = _jobName;
            command.Parameters.Add(jobName1);

            OdbcParameter jobNum = new OdbcParameter();
            jobNum.OdbcType = OdbcType.Int;
            jobNum.Value = _jobNum;
            command.Parameters.Add(jobNum);
        }

        private bool IsParametersValid()
        {
            return IsJobNameValid() && IsJobNumValid();
        }

        private bool IsJobNameValid()
        {
            if (JobDropDownList1.SelectedIndex >= 0)
            {
                _jobName = JobDropDownList1.SelectedValue;
                return true;
            }
            else
            {
                Utils.SetErrorStatus(Label5, "Picked item has wrong index");
                JobDropDownList1.SelectedIndex = 0;

                return false;
            }
        }

        private bool IsJobNumValid()
        {
            if (Int32.TryParse(TextBox2.Text, out _jobNum))
            {
                return true;
            }
            else
            {
                Utils.SetErrorStatus(Label5, "Jobs num must be set as Int");
                TextBox2.Text = "";

                return false;
            }
        }

        protected void Button2_OnClick(object sender, EventArgs e)
        {
            Page.Response.Redirect("WebForm2.aspx");
        }
    }
}