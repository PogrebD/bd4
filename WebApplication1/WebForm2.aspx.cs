﻿using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace WebApplication1
{
    public partial class WebForm2 : System.Web.UI.Page
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

            const string SQL = @"update pmib0413.spj1 
                                set kol = kol + ? 
                                where n_spj in (select n_spj 
                                                from pmib0413.spj1 
                                                join (select max(distinct date_post), n_det 
                                                      from pmib0413.spj1 
                                                      where n_izd = ?
                                                      group by n_det) as buf on (pmib0413.spj1.date_post = buf.max and pmib0413.spj1.n_det = buf.n_det))";

            using (OdbcCommand _command = new OdbcCommand(SQL, _connection))
            {
                SetParameters(_command);

                OdbcTransaction transaction = null;

                try
                {
                    transaction = _connection.BeginTransaction();
                    _command.Transaction = transaction;

                    int dataReader = _command.ExecuteNonQuery();

                    Label6.Text = "Обновлено записей:" + dataReader.ToString();

                    transaction.Commit();
                    Utils.SetGoodStatus(Label5);
                }
                catch (Exception ex)
                {
                    Label5.Text = ex.Message;
                    transaction.Rollback();
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
        }

        private void SetParameters(OdbcCommand command)
        {
            OdbcParameter jobNum = new OdbcParameter();
            jobNum.OdbcType = OdbcType.Int;
            jobNum.Value = _jobNum;
            command.Parameters.Add(jobNum);

            OdbcParameter jobName = new OdbcParameter();
            jobName.OdbcType = OdbcType.Text;
            jobName.Value = _jobId;
            command.Parameters.Add(jobName);
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
            Page.Response.Redirect("WebForm1.aspx");
        }
    }
}