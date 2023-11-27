using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private OdbcConnection _connection;
        private int _jobNum;

        private string _jobId
        {
            get => ViewState["_jobId"] as string ?? string.Empty;
            set => ViewState["_jobId"] = value;
        }

        private Dictionary<string, string> _dataBase
        {
            get => ViewState["_dataBase"] as Dictionary<string, string> ?? new Dictionary<string, string>();
            set => ViewState["_dataBase"] = value;
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
            string strSQL = "SELECT n_izd, name, town FROM pmib0413.j";
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
                    _dataBase = new Dictionary<string, string>();

                    while (i.Read())
                    {
                        string value = $"{i["name"].ToString()}, {i["town"].ToString()}";
                        _dataBase.Add(value, i["n_izd"].ToString());

                        JobDropDownList1.Items.Add(new ListItem(value));
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
                                                                  WHERE n_izd = ?
                                                                  GROUP BY n_det) AS buf ON pmib0413.q.n_det= buf.n_det
                                                 WHERE pmib0413.q.n_izd = ?
                                                 AND (pmib0413.q.kol*(? + (select coalesce(sum(kol), 0) 
                                                                   from pmib0413.w 
                                                                   where n_izd = ?)) > buf.sum OR buf.n_det IS NULL))";

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
            OdbcParameter jobId = new OdbcParameter();
            jobId.OdbcType = OdbcType.Text;
            jobId.Value = _jobId;
            command.Parameters.Add(jobId);

            OdbcParameter jobId1 = new OdbcParameter();
            jobId1.OdbcType = OdbcType.Text;
            jobId1.Value = _jobId;
            command.Parameters.Add(jobId1);

            OdbcParameter jobNum = new OdbcParameter();
            jobNum.OdbcType = OdbcType.Int;
            jobNum.Value = _jobNum;
            command.Parameters.Add(jobNum);

            OdbcParameter jobId2 = new OdbcParameter();
            jobId2.OdbcType = OdbcType.Text;
            jobId2.Value = _jobId;
            command.Parameters.Add(jobId2);
        }

        private bool IsParametersValid()
        {
            return IsJobNameValid() && IsJobNumValid();
        }

        private bool IsJobNameValid()
        {
            if (JobDropDownList1.SelectedIndex >= 0)
            {
                _jobId = _dataBase[JobDropDownList1.SelectedValue];
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