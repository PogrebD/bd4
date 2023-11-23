using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public static class Utils
    {
        public static void SetGoodStatus(System.Web.UI.WebControls.Label label)
        {
            label.ForeColor = Color.Green;
            label.Text = "Ok!";
        }

        public static void SetErrorStatus(System.Web.UI.WebControls.Label label, string what)
        {
            label.ForeColor = Color.Red;
            label.Text = what;
        }
    }
}