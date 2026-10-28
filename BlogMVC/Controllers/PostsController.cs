
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using BlogMVC.Models;

namespace BlogMVC.Controllers
{
    public class PostsController : Controller
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings[
                "BlogMVCConnection"
            ].ConnectionString;

        // LIST: Danh sach bai viet
        public ActionResult List()
        {
            List<Post> posts = new List<Post>();

            using (SqlConnection conn =
                new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT PostId, Title, Content, CreatedAt
                    FROM Posts
                    ORDER BY CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        posts.Add(new Post
                        {
                            PostId = Convert.ToInt32(reader["PostId"]),
                            Title = reader["Title"].ToString(),
                            Content = reader["Content"].ToString(),
                            CreatedAt = Convert.ToDateTime(
                                reader["CreatedAt"])
                        });
                    }
                }
            }

            return View(posts);
        }

        // DETAILS: Chi tiet bai viet va danh sach binh luan
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Post post = null;

            using (SqlConnection conn =
                new SqlConnection(connectionString))
            {
                conn.Open();

                string postSql = @"
                    SELECT PostId, Title, Content, CreatedAt
                    FROM Posts
                    WHERE PostId = @PostId";

                using (SqlCommand cmd =
                    new SqlCommand(postSql, conn))
                {
                    cmd.Parameters.AddWithValue("@PostId", id.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            post = new Post
                            {
                                PostId = Convert.ToInt32(
                                    reader["PostId"]),
                                Title = reader["Title"].ToString(),
                                Content = reader["Content"].ToString(),
                                CreatedAt = Convert.ToDateTime(
                                    reader["CreatedAt"])
                            };
                        }
                    }
                }

                if (post == null)
                    return HttpNotFound();

                string commentSql = @"
                    SELECT CommentId, PostId, Author,
                           Content, CreatedAt
                    FROM Comments
                    WHERE PostId = @PostId
                    ORDER BY CreatedAt DESC";

                using (SqlCommand cmd =
                    new SqlCommand(commentSql, conn))
                {
                    cmd.Parameters.AddWithValue("@PostId", id.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            post.Comments.Add(new Comment
                            {
                                CommentId = Convert.ToInt32(
                                    reader["CommentId"]),
                                PostId = Convert.ToInt32(
                                    reader["PostId"]),
                                Author = reader["Author"].ToString(),
                                Content = reader["Content"].ToString(),
                                CreatedAt = Convert.ToDateTime(
                                    reader["CreatedAt"])
                            });
                        }
                    }
                }
            }

            return View(post);
        }

        // COMMENT: Them binh luan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(
            int PostId, string Author, string Content)
        {
            Author = (Author ?? "").Trim();
            Content = (Content ?? "").Trim();

            if (Author.Length == 0 ||
                Author.Length > 100 ||
                Content.Length == 0 ||
                Content.Length > 1000)
            {
                TempData["Error"] =
                    "Vui long nhap ten va binh luan hop le.";

                return RedirectToAction("Details",
                    new { id = PostId });
            }

            using (SqlConnection conn =
                new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO Comments (PostId, Author, Content)
                    SELECT PostId, @Author, @Content
                    FROM Posts
                    WHERE PostId = @PostId";

                using (SqlCommand cmd =
                    new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PostId", PostId);
                    cmd.Parameters.AddWithValue("@Author", Author);
                    cmd.Parameters.AddWithValue("@Content", Content);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                        return HttpNotFound();
                }
            }

            TempData["Success"] = "Gui binh luan thanh cong!";

            return RedirectToAction("Details",
                new { id = PostId });
        }
    }
}
