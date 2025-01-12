﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class ViewContent : System.Web.UI.Page
{
    private int ContentID = -1;
    private Content content;
    protected void Page_Load(object sender, EventArgs e)
    {
        string querystring = Request.QueryString["ContentID"];

        if(querystring == null)
        {
            Response.Redirect("~/Frontpage.aspx");
        }
        ContentID = Convert.ToInt32(querystring);

        if(!DBHandler.Content_Exists(ContentID))
        {
            Response.Write("<script>alert('Content does not exist! Redirecting to frontpage..')</script>");
            Server.Transfer("~/Frontpage.aspx");
        }

        content = DBHandler.Content_Fetch(ContentID);
        content.Headers = DBHandler.Content_FetchHeaders(content);
        content.Comments = DBHandler.Content_FetchComments(content);
        content.Tags = DBHandler.Content_FetchTags(content);

        if(content.Headers != null)
        {
            for (int i = 0; i < content.Headers.Count;i++ )
            {
                Label lblTitel_ = new Label();
                lblTitel_.Text = content.Headers[i].Titel;
                pnlContentControls.Controls.Add(lblTitel_);
                pnlContentControls.Controls.Add(new LiteralControl("<br /"));
                /*
                if(content.Headers[i].Titel != "")
                {
                    string control = "<h3>" + content.Headers[i].Titel + "</h2>";
                    pnlContentControls.Controls.Add(new LiteralControl(control));
                    pnlContentControls.Controls.Add(new LiteralControl("<br /"));
                }
                */
                switch(content.Headers[i].SoortContent)
                {
                    case 0: //Text
                        Label lblText = new Label();
                        lblText.Text = content.Headers[i].Text;
                        pnlContentControls.Controls.Add(lblText);
                        pnlContentControls.Controls.Add(new LiteralControl("<br /"));
                        break;
                    case 1: //Picture
                        Label lblPicture = new Label();
                        lblPicture.Text = "[[PICTURE]]: " + content.Headers[i].Path;
                        pnlContentControls.Controls.Add(lblPicture);
                        pnlContentControls.Controls.Add(new LiteralControl("<br /"));
                        break;
                    case 2: //Video
                        Label lblVideo = new Label();
                        lblVideo.Text = "[[VIDEO]]: " + content.Headers[i].Path;
                        pnlContentControls.Controls.Add(lblVideo);
                        pnlContentControls.Controls.Add(new LiteralControl("<br /"));
                        break;
                }
            }
        }

        if (content.Comments != null)
        {
            lblComAmount.Text = Convert.ToString(content.Comments.Count);
            for (int i = 0; i < content.Comments.Count; i++)
            {
                /*
                string control = "<h2>" + content.Comments[i].Uploader + "</h2>";
                pnlComments.Controls.Add(new LiteralControl(control));
                pnlCommentsControls.Controls.Add(new LiteralControl("<br /"));
                 */
                pnlComments.Controls.Add(new LiteralControl("<br /"));
                Label lblCommentID = new Label();
                lblCommentID.Text = content.Comments[i].CommentId + "- ";
                pnlComments.Controls.Add(lblCommentID);

                HyperLink hlProfile = new HyperLink();
                hlProfile.Text = content.Comments[i].Uploader;
                string redire = "~/Profile.aspx?Username=" + content.Comments[i].Uploader;
                hlProfile.NavigateUrl = redire;
                pnlComments.Controls.Add(hlProfile);

                Label lblThumbsD = new Label();
                lblThumbsD.Text = " Thumbs: " + content.Comments[i].Thumbs;
                pnlComments.Controls.Add(lblThumbsD);
                pnlComments.Controls.Add(new LiteralControl("<br /"));

                Label lblPicture = new Label();
                lblPicture.Text = "[[PICTURE]]: " + content.Comments[i].Picture;
                pnlComments.Controls.Add(lblPicture);
                pnlComments.Controls.Add(new LiteralControl("<br /"));

                Label lblDescription = new Label();
                lblDescription.Text = content.Comments[i].Description;
                pnlComments.Controls.Add(lblDescription);
                pnlComments.Controls.Add(new LiteralControl("<br /"));

            }
        }
        else lblComAmount.Text = "0";

        if(content.Tags != null)
        {
            for(int i=0; i<content.Tags.Count; i++)
            {
                Label tag = new Label();
                tag.Text = content.Tags[i].TagName + ", ";
                pnlTags.Controls.Add(tag);
            }
        }


        lblTitel.Text = content.Titel;
        lblConDescription.Text = content.Beschrijving;
        //lblUsername.Text = content.UploaderUsername;
        hlUsername.Text = content.UploaderUsername;
        string redir = "~/Profile.aspx?Username=" + content.UploaderUsername;
        hlUsername.NavigateUrl = redir;
        lblThumbs.Text = " " + Convert.ToString(content.Thumbs);
        lblViews.Text = Convert.ToString(content.Views);
        lblFavorites.Text = Convert.ToString(content.Favorites);
        lblDatum.Text = content.Datum;
    }
    protected void btnHome_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Frontpage.aspx");
    }
    protected void btnComSubmit_Click(object sender, EventArgs e)
    {

        if(tbComText.Text == "" || Session["Logged_In"] == null)
        {
            Response.Write("<script>alert('Please fill in the Titel and Text and make sure yóu're logged in before submitting!')</script>");
            return;
        }
        Content content = DBHandler.Content_Fetch(ContentID);
        User user = Session["Logged_In"] as User;
        DBHandler.Content_CreateComment(content.ContentID, user.Id, -1, tbComPath.Text, tbComText.Text);
        Response.Write("<script>alert('Comment added!')</script>");
        string redir = "~/ViewContent.aspx?ContentID=" + content.ContentID;
        Response.Redirect(redir);
    }
    protected void btnPosThumb_Click(object sender, EventArgs e)
    {
        User user = Session["Logged_In"] as User;
        if (content == null || Session["Logged_In"] == null)
        {
            Response.Write("<script>alert('Please login before thumbing!')</script>");
            return;
        }
        if(user.Id == content.UploaderID)
        {
            Response.Write("<script>alert('You cant thumb your own content!')</script>");
            return;
        }
        if(!DBHandler.User_HasThumbed(content, Session["Logged_In"] as User))
        {
            DBHandler.Content_Thumb(content, Session["Logged_In"] as User, true);
        }
        else
        {
            Response.Write("<script>alert('You have already thumbed this content!')</script>");
            return;
        }
        lblThumbs.Text = Convert.ToString(Convert.ToInt32(lblThumbs.Text) + 1);
    }
    protected void btnNegThumb_Click(object sender, EventArgs e)
    {
        User user = Session["Logged_In"] as User;
        if (content == null || Session["Logged_In"] == null)
        {
            Response.Write("<script>alert('Please login before thumbing!')</script>");
            return;
        }
        if (user.Id == content.UploaderID)
        {
            Response.Write("<script>alert('You cant thumb your own content!')</script>");
            return;
        }
        if (!DBHandler.User_HasThumbed(content, Session["Logged_In"] as User))
        {
            DBHandler.Content_Thumb(content, Session["Logged_In"] as User, false);
        }
        else
        {
            Response.Write("<script>alert('You have already thumbed this content!')</script>");
            return;
        }
        lblThumbs.Text = Convert.ToString(Convert.ToInt32(lblThumbs.Text) - 1);
    }
}