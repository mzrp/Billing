using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RPNAVConnect
{
    public partial class UserIdentity : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string sUserId = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserId"] != null)
                {
                    sUserId = System.Web.HttpContext.Current.Session["UserId"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserId = "n/a";
            }

            string sUserDisplayName = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserDisplayName"] != null)
                {
                    sUserDisplayName = System.Web.HttpContext.Current.Session["UserDisplayName"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserDisplayName = "n/a";
            }

            string sUserExpirationDateTime = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserExpirationDateTime"] != null)
                {
                    sUserExpirationDateTime = System.Web.HttpContext.Current.Session["UserExpirationDateTime"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserExpirationDateTime = "n/a";
            }

            string sUserAuthToken = "n/a";
            try
            {
                if (System.Web.HttpContext.Current.Session["UserAuthToken"] != null)
                {
                    sUserAuthToken = System.Web.HttpContext.Current.Session["UserAuthToken"].ToString();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                sUserAuthToken = "n/a";
            }

            Response.Write(sUserId + "#$#" + sUserDisplayName + "#$#" + sUserAuthToken + "#$#" + sUserExpirationDateTime);
        }
    }
}