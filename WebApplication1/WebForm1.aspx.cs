using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private string _jobName
        {
            get => ViewState["_jobName"] as string ?? string.Empty;
            set => ViewState["_jobName"] = value;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            // Создаем объект подключения
            OdbcConnection conn = new OdbcConnection();
            // Задаем параметр подключения – имя ODBC-источника
            conn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
        
            // Подключаемся к БД
            conn.Open();
            // Определяем строку с текстом запроса
            string strSQL = "SELECT name FROM pmib0413.j";
            // Создаем объект запроса
            OdbcCommand cmd = new OdbcCommand(strSQL, conn);

            OdbcTransaction tx = null;
            try
            {
                // Начинаем транзакцию и извлекаем объект транзакции из объекта подключения.
                tx = conn.BeginTransaction();
                // Включаем объект SQL-команды в транзакцию
                cmd.Transaction = tx;
                // Выполняем SQL-команду и получаем количество обработанных записей
                OdbcDataReader i = cmd.ExecuteReader();
                // Подтверждаем транзакцию  
                tx.Commit();
                while(i.Read())
                {
                    var jobName = i["name"].ToString();

                    JobDropDownList1.Items.Add(new ListItem(jobName));
                }

            }
            catch (Exception ex)
            {
                // При возникновении любой ошибки 
                // Формируем сообщение об ошибке 
                Label1.Text = ex.Message;
                // выполняем откат транзакции 
                tx.Rollback();
            }

            //закрываем соединение
            conn.Close();

            
        }

        protected void JobDropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(JobDropDownList1.SelectedIndex >= 0)
            {
                _jobName = JobDropDownList1.SelectedValue;
                Label4.Text = _jobName;
            }
        }
    }
}