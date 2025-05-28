using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using CustomControls.RJControls;  // ✅ Add this to access RJTextBox

using System.Data.SqlClient;
using System.IO;
using iTextSharp.text.pdf;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Devices;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
//using iTextSharp.text;

namespace courseApp
{
    public partial class Form1 : Form
    {
        bool click_check = false;
        bool click_check2 = false;
        private int panel2OriginalY;
        private int panel3OriginalY;
        private int chatriginalY;
        ///////Global Varibales
        int RE_ID = 0;
        int SE_ID = 0;
        string RoleID;
        string NameID;
        string Subject_temp;
        string Subject_temp2;

        bool check_text = false;

        private int course_lesson_ID;
        /////
        ///
        Dictionary<string, (string question, string query)> reports = new Dictionary<string, (string, string)>()
        {
            ["Top Registered Course"] = ("What is the course with the highest number of registered students?",
        @"SELECT TOP 1 C.Title, COUNT(R.UserId) AS StudentCount
          FROM Course C
          JOIN Register R ON C.CourseId = R.CourseId
          GROUP BY C.Title
          ORDER BY StudentCount DESC"),

            ["Courses Not in Second Semester"] = ("What are the courses that are not assigned to the current semester?",
        @"SELECT Title, Semester FROM Course WHERE Semester <> 2"),

            ["Top 5 Students Per Course"] = ("Who are the top five students for each course in a specific year?",
        @"SELECT C.Title AS Course, U.FName + ' ' + U.LName AS StudentName, R.Grade
          FROM Register R
          JOIN Course C ON R.CourseId = C.CourseId
          JOIN Userr U ON R.UserId = U.UserId
          WHERE Studying_Year = 2025
          ORDER BY C.Title, R.Grade DESC"),

            ["Least Popular Category"] = ("What is the category with the least number of students?",
        @"SELECT Category, COUNT(R.UserId) AS StudentCount
          FROM Course C
          JOIN Register R ON R.CourseId = C.CourseId
          GROUP BY Category
          ORDER BY StudentCount ASC"),

            ["Registered Without Exam"] = (
            "Get the names of registered students in a specific course who didn’t apply for any exam yet.",
            @"SELECT 
                U.FName + ' ' + U.LName AS StudentName,
                C.Title AS Course
              FROM 
                Register R
              JOIN 
                Userr U ON R.UserId = U.UserId
              JOIN 
                Course C ON R.CourseId = C.CourseId
              WHERE 
                NOT EXISTS (
                    SELECT 1
                    FROM Exam E
                    WHERE E.Answers LIKE '%User ID: ' + CAST(R.UserId AS NVARCHAR) + ' %'
                )"
        )
,

            ["Course Exam Report"] = ("Get a report of each course with its name, number of exams, and the highest grade.",
        @"SELECT C.Title, COUNT(E.ExamId) AS ExamCount, MAX(R.Grade) AS HighestGrade
          FROM Course C
          LEFT JOIN Exam E ON C.ExamId = E.ExamId
          LEFT JOIN Register R ON C.CourseId = R.CourseId
          GROUP BY C.Title"),

            ["Student Exam Participation"] = ("List students with how many exams they participated in.",
        @"	SELECT 
                U.FName + ' ' + U.LName AS StudentName,
                COUNT(E.ExamId) AS ExamCount
            FROM 
                Userr U
            JOIN 
                Exam E ON E.Answers LIKE '%User ID: ' + CAST(U.UserId AS NVARCHAR) + ' %'
            GROUP BY 
                U.FName, U.LName"),

            ["Classwork Participation"] = ("Courses with number of classworks and average duration.",
        @"SELECT C.Title, COUNT(CW.ExId) AS NumClassworks
          FROM Course C
          JOIN ClassWork CW ON C.CourseId = CW.CourseId
          GROUP BY C.Title")
        };



        ///
        void handleicon(PictureBox icon)
        {
            List<PictureBox> iconList = new List<PictureBox> { homeicom2, examicon2, coursesicon2, classworkicon2, chaticon2, usericon2 };
            foreach (var icons in iconList)
            {
                icons.SendToBack();
            }

            icon.BringToFront();
            if (icon != usericon2)
            {
                click_check = false;
            }

        }
        void handleicon2(PictureBox icon)
        {

            List<PictureBox> iconList = new List<PictureBox> { Home_th2, exam_th2, classwork_th2, Grades2, chat_th2, user_th2, qbouns2 };
            foreach (var icons in iconList)
            {
                icons.SendToBack();
            }

            icon.BringToFront();
            if (icon != user_th2)
            {
                click_check2 = false;
            }

        }

        private PictureBox selectedChatPic = null;
        private void deleteicon_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;

            if (pic != null)
            {
                Panel parentPanel = pic.Parent as Panel;
                int courseId = Convert.ToInt32(parentPanel.Tag);

                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this course?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string deleteQuery = "DELETE FROM Course WHERE CourseId = @CourseId";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@CourseId", courseId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Optional: remove the UI element from the form/panel
                    pic.Parent?.Dispose(); // or pic.Dispose() if it's the only thing to remove

                    MessageBox.Show("Course deleted successfully.");
                }
            }
        }
        private void deleteiconCW_Click(object sender, EventArgs e)
        {
            PictureBox btn = sender as PictureBox;
            if (btn != null && btn.Tag is int ExId)
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this classwork?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string deleteQuery = "DELETE FROM ClassWork WHERE ExId = @ExId";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@ExId", ExId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Remove the UI element (assuming the button is inside a panel)


                    MessageBox.Show("ClassWork deleted successfully.");
                    LoadClassWork_th(SE_ID);
                }
            }
            else
            {
                MessageBox.Show("Invalid ClassWork ID.");
            }
        }
        private void deleteiconEX_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            if (pic != null && pic.Tag is int ExamId)
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this exam?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // Step 1: Set ExamId to NULL in Course table for all courses linked to this exam
                                string updateCourseQuery = "UPDATE Course SET ExamId = NULL WHERE ExamId = @ExamId";
                                using (SqlCommand updateCmd = new SqlCommand(updateCourseQuery, conn, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@ExamId", ExamId);
                                    updateCmd.ExecuteNonQuery();
                                }

                                // Step 2: Delete the exam from Exam table
                                string deleteExamQuery = "DELETE FROM Exam WHERE ExamId = @ExamId";
                                using (SqlCommand deleteCmd = new SqlCommand(deleteExamQuery, conn, transaction))
                                {
                                    deleteCmd.Parameters.AddWithValue("@ExamId", ExamId);
                                    deleteCmd.ExecuteNonQuery();
                                }

                                transaction.Commit();

                                MessageBox.Show("Exam deleted successfully.");


                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show("Error deleting exam: " + ex.Message);
                            }
                        }
                    }
                }
                LoadExams_th(SE_ID);
            }
            else
            {
                MessageBox.Show("Invalid Exam ID.");
            }
        }
        private void viewicon_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            if (pic != null)
            {
                Panel parentPanel = pic.Parent as Panel;
                if (parentPanel != null && parentPanel.Tag != null)
                {
                    int courseId = Convert.ToInt32(parentPanel.Tag);

                    DialogResult result = MessageBox.Show(
                        "Set course privacy:\n\nYes = Public\nNo = Private",
                        "Change Privacy",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel)
                        return;

                    int isHidden = (result == DialogResult.Yes) ? 1 : 0; // 1 = Public, 0 = Private

                    string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string updateQuery = "UPDATE Course SET is_hidden = @isHidden WHERE CourseId = @CourseId";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@isHidden", isHidden);
                            cmd.Parameters.AddWithValue("@CourseId", courseId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Course privacy updated successfully.");
                }
            }
        }
        private void editicon_Click(object sender, EventArgs e)
        {

            PictureBox pic = sender as PictureBox;
            if (pic != null)
            {
                Panel parentPanel = pic.Parent as Panel;
                if (parentPanel != null && parentPanel.Tag != null)
                {
                    int courseId = Convert.ToInt32(parentPanel.Tag);
                    string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string selectQuery = @"
                            SELECT CourseId, PassedExam_ID, Title, Description, Category, Studying_Year, 
                                   Semester, is_hidden, Cover 
                            FROM Course 
                            WHERE CourseId = @CourseId";

                        using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@CourseId", courseId); // Assume courseId is defined

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Retrieve text fields
                                    string title = reader["Title"].ToString();
                                    string description = reader["Description"].ToString();
                                    string category = reader["Category"].ToString();
                                    string year = reader["Studying_Year"].ToString();
                                    string semester = reader["Semester"].ToString();
                                    bool isHidden = Convert.ToBoolean(reader["is_hidden"]);
                                    int? examId = reader["PassedExam_ID"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(reader["PassedExam_ID"]);
                                    int course_id = Convert.ToInt32(reader["CourseId"]);
                                    // Retrieve image from database
                                    byte[] imageData = reader["Cover"] as byte[];
                                    Image coverImage = null;

                                    if (imageData != null)
                                    {
                                        using (MemoryStream ms = new MemoryStream(imageData))
                                        {
                                            coverImage = Image.FromStream(ms);
                                        }
                                    }


                                    titlebar_ed.Texts = title;
                                    descbar_ed.Texts = description;
                                    categorybar_ed.Texts = category;
                                    yearbar_ed.Texts = year;
                                    semsterbar_ed.Texts = semester;
                                    exambar_ed.Texts = examId.ToString();
                                    coursebar_ed.Texts = courseId.ToString();
                                    privebar_ed.SelectedItem = isHidden ? "Private" : "Public";
                                    imagepreview_ed.Image = coverImage;
                                    update_course.BringToFront();
                                    //MessageBox.Show("Course loaded successfully.");
                                }
                                else
                                {
                                    MessageBox.Show("Course not found.");
                                }
                            }
                        }
                    }





                }

            }
        }
        private void addlesson_icon_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;

            if (pic != null)
            {
                Panel parentPanel = pic.Parent as Panel;
                course_lesson_ID = Convert.ToInt32(parentPanel.Tag);
                datelesson.Texts = DateTime.Now.ToShortDateString();

                addlesson.BringToFront();
            }
        }
        private void Chatpic_Click(object sender, EventArgs e, Panel chat)
        {

            PictureBox pic = sender as PictureBox;
            if (pic != null)
            {
                RE_ID = (int)pic.Tag;
                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Use parameters to avoid SQL injection and use the method arguments
                    SqlDataAdapter adapter = new SqlDataAdapter(
                        @"SELECT Subject
                         FROM Chat 
                         WHERE (Sender_id = @SenderId AND Reciever_id = @ReceiverId) ", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@SenderId", SE_ID);
                    adapter.SelectCommand.Parameters.AddWithValue("@ReceiverId", RE_ID);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    foreach (DataRow row in table.Rows)
                    {
                        string Subject = row["Subject"]?.ToString() ?? string.Empty;
                        if (chat.Name == "ChatContainer")
                        {
                            Subject_label.Text = Subject;
                        }
                        else
                        {
                            subject_label2.Text = Subject;
                        }


                    }
                }
                if (selectedChatPic != null)
                    selectedChatPic.Enabled = true;

                pic.Enabled = false;
                selectedChatPic = pic;


                if (chat.Name == "ChatContainer")
                {
                    loadMessages(panel5, (int)pic.Tag, SE_ID);
                }
                else
                {
                    loadMessages(panel14, (int)pic.Tag, SE_ID);
                }
            }

        }
        //////to reigster course 

        private void Preview_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null)
            {
                MessageBox.Show("ExamId not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int examId = Convert.ToInt32(btn.Tag);
            string answers = "";

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Answers FROM Exam WHERE ExamId = @ExamId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", examId);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        answers = result.ToString();
                    }
                    else
                    {
                        MessageBox.Show("No answers found for this ExamId.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.Title = "Save PDF";
                saveFileDialog.FileName = $"Exam_{examId}_Answers.pdf";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
                    using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        pdfDoc.Add(new iTextSharp.text.Paragraph($"Answers for Exam ID: {examId}\n\n"));
                        pdfDoc.Add(new iTextSharp.text.Paragraph(answers));

                        pdfDoc.Close();
                        stream.Close();
                    }

                    MessageBox.Show("PDF saved successfully!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void Preview2_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null)
            {
                MessageBox.Show("ExId not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int exId = Convert.ToInt32(btn.Tag);
            string answers = "";

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT AnswerPdf FROM ClassWork WHERE ExId = @ExId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ExId", exId);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        answers = result.ToString();
                    }
                    else
                    {
                        MessageBox.Show("No answers found for this ExId.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.Title = "Save PDF";
                saveFileDialog.FileName = $"ClassWork_{exId}_Answers.pdf";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
                    using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        pdfDoc.Add(new iTextSharp.text.Paragraph($"Answers for Exercise ID: {exId}\n\n"));
                        pdfDoc.Add(new iTextSharp.text.Paragraph(answers));

                        pdfDoc.Close();
                        stream.Close();
                    }

                    MessageBox.Show("PDF saved successfully!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void coursepic_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            if (pic != null)
            {
                if (selectedChatPic != null)
                    selectedChatPic.Enabled = true;

                pic.Enabled = false;
                selectedChatPic = pic;
                RE_ID = (int)pic.Tag;
                loadMessages(panel5, (int)pic.Tag, SE_ID);

            }
        }
        //////////////
        private void LoadExams_th(int userId)
        {
            examadmin_cont.Controls.Clear();
            int yOffset = 50;

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string query = @"
            SELECT E.ExamId, E.Title, E.Description, E.Duration, E.Date, C.Title AS CourseTitle
            FROM Exam E
            INNER JOIN Course C ON E.ExamId = C.ExamId
            WHERE C.UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int examId = Convert.ToInt32(reader["ExamId"]);

                            Panel card = new Panel
                            {
                                Size = new Size(750, 200),
                                Location = new Point(0, yOffset),
                                BackColor = Color.FromArgb(200, 200, 200),
                                BorderStyle = BorderStyle.Fixed3D,
                                Padding = new Padding(10),
                                //Tag = examId
                            };

                            Label lblCourse = new Label
                            {
                                Text = $"📚 Course: {reader["CourseTitle"]}",
                                Location = new Point(10, 10),
                                Font = new Font("Montserrat light", 11, FontStyle.Bold),
                                AutoSize = true
                            };

                            Label lblTitle = new Label
                            {
                                Text = $"📝 Type: {reader["Title"]}",
                                Location = new Point(10, 45),
                                Font = new Font("Montserrat light", 11),
                                AutoSize = true
                            };

                            Label lblDuration = new Label
                            {
                                Text = $"⏱ Duration: {reader["Duration"]}",
                                Location = new Point(10, 80),
                                Font = new Font("Montserrat light", 11),
                                AutoSize = true
                            };

                            Label lblDate = new Label
                            {
                                Text = $"📅 Date: {Convert.ToDateTime(reader["Date"]).ToShortDateString()}",
                                Location = new Point(10, 115),
                                Font = new Font("Montserrat light", 11),
                                AutoSize = true
                            };

                            Label lblDesc = new Label
                            {
                                Text = $"🧾 Description: {reader["Description"]}",
                                Location = new Point(10, 150),
                                Font = new Font("Montserrat light", 11),
                                AutoSize = true,
                            };

                            Button startBtn = new Button
                            {
                                Text = "Edit Exam",
                                Location = new Point(600, 100),
                                Size = new Size(120, 30),
                                BackColor = Color.White,
                                ForeColor = Color.FromArgb(5, 12, 22),
                                FlatStyle = FlatStyle.Flat,
                                Cursor = Cursors.Hand,
                                Font = new Font("Montserrat ExtraBold", 9),
                                Tag = examId
                            };
                            Button See_answers = new Button
                            {
                                Text = "Review Answers",
                                Location = new Point(600, 140),
                                Size = new Size(122, 30),
                                BackColor = Color.FromArgb(5, 12, 22),
                                ForeColor = Color.White,
                                FlatStyle = FlatStyle.Flat,
                                Cursor = Cursors.Hand,
                                Font = new Font("Montserrat ExtraBold", 9),
                                Tag = examId
                            };
                            PictureBox icon_de = new PictureBox
                            {
                                Size = new Size(61, 45),
                                Location = new Point(625, 40),
                                BackgroundImageLayout = ImageLayout.Zoom,
                                BackgroundImage = Image.FromStream(new MemoryStream(Properties.Resources.delete_icon)),
                                Cursor = Cursors.Hand,
                                Tag = examId
                            };
                            icon_de.Click += deleteiconEX_Click;
                            See_answers.Click += Preview_Click;

                            startBtn.FlatAppearance.BorderSize = 0;
                            startBtn.Click += EditExam_Click;
                            card.Controls.Add(lblCourse);
                            card.Controls.Add(lblTitle);
                            card.Controls.Add(lblDuration);
                            card.Controls.Add(lblDate);
                            card.Controls.Add(lblDesc);
                            card.Controls.Add(startBtn);
                            card.Controls.Add(icon_de);
                            card.Controls.Add(See_answers);
                            examadmin_cont.Controls.Add(card);
                            yOffset += 200;
                        }
                    }
                }
            }
        }

        private void LoadClassWork_th(int userId)
        {
            panel10.Controls.Clear();
            int yOffset = 50;


            // الاتصال بقاعدة البيانات

            //string connectionString = "Server=LAPTOP-I23IVTH3;Database=Course_system;Trusted_Connection=True;";



            string connectionString = "Server=LAPTOP-I23IVTH3;Database=Course_system;Trusted_Connection=True;";


            // Only get classwork where Course.UserId = @UserId
            string query = @"
                SELECT C.CourseId, CW.ExId, CW.Title, CW.Duration, CW.Date, CW.Description, C.Title AS CourseTitle
                FROM ClassWork CW
                INNER JOIN Course C ON CW.CourseId = C.CourseId
                WHERE C.UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Panel card = new Panel
                            {
                                Size = new Size(700, 120),
                                Location = new Point(0, yOffset),
                                BackColor = Color.FromArgb(200, 200, 200),
                                Font = new Font("Montserrat ExtraBold", 10),
                                Cursor = Cursors.Hand
                            };

                            Label lblCourse = new Label
                            {
                                Text = $"Course: {reader["CourseTitle"]}",
                                Location = new Point(10, 10),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            Label lblTitle = new Label
                            {
                                Text = $"Title: {reader["Title"]}",
                                Location = new Point(10, 40),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            Label lblDesc = new Label
                            {
                                Text = $"Description: {reader["Description"]}",
                                Location = new Point(10, 70),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            Label lblDate = new Label
                            {
                                Text = $"Date: {Convert.ToDateTime(reader["Date"]).ToShortDateString()}",
                                Location = new Point(400, 10),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            Label lblDuration = new Label
                            {
                                Text = $"Duration: {reader["Duration"]}",
                                Location = new Point(400, 40),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            PictureBox icon_de = new PictureBox
                            {
                                Size = new Size(61, 45),
                                Location = new Point(595, 20),
                                BackgroundImageLayout = ImageLayout.Zoom,
                                BackgroundImage = Image.FromStream(new MemoryStream(Properties.Resources.delete_icon)),
                                Cursor = Cursors.Hand,
                                Tag = Convert.ToInt32(reader["ExId"])
                            };
                            icon_de.Click += deleteiconCW_Click;

                            Button uploadBtn = new Button
                            {
                                Text = "Edit",
                                Location = new Point(580, 80),
                                Size = new Size(90, 30),
                                BackColor = Color.FromArgb(5, 12, 22),
                                ForeColor = Color.White,
                                FlatStyle = FlatStyle.Flat,
                                Tag = Convert.ToInt32(reader["ExId"])
                            };
                            uploadBtn.FlatAppearance.BorderSize = 0;
                            uploadBtn.Click += EditClasswork_Click;

                            Button Previewbtn = new Button
                            {
                                Text = "Preview Submitted",
                                Location = new Point(450, 78),
                                Size = new Size(120, 34),
                                BackColor = Color.White,
                                ForeColor = Color.FromArgb(5, 12, 22),
                                Tag = Convert.ToInt32(reader["ExId"])
                            };
                            Previewbtn.Click += Preview2_Click;

                            card.Controls.Add(lblCourse);
                            card.Controls.Add(lblTitle);
                            card.Controls.Add(lblDesc);
                            card.Controls.Add(lblDate);
                            card.Controls.Add(lblDuration);
                            card.Controls.Add(uploadBtn);
                            card.Controls.Add(icon_de);
                            card.Controls.Add(Previewbtn);

                            panel10.Controls.Add(card);
                            yOffset += 140;
                        }
                    }
                }
            }
        }

        //////////////
        private void EditExam_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is int examId)
            {
                MessageBox.Show($"Edit exam with ID: {examId}", "Edit Exam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                label44.Text = examId.ToString();
                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                string query = "SELECT E.Title, E.Description, E.Duration, E.Date, E.Question " +
                               "FROM Exam E " +
                               "INNER JOIN Course C ON E.ExamId = @examid";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@examid", examId); // Add the parameter BEFORE executing the reader

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                title_edex.Texts = reader["Title"].ToString();
                                dateTimePicker3.Value = Convert.ToDateTime(reader["Date"]);
                                dur_edex.Texts = reader["Duration"].ToString();
                                desc_edex.Texts = reader["Description"].ToString();
                                ques_edex.Texts = reader["Question"].ToString();

                                editexam.BringToFront();
                            }
                        }
                    }
                }

            }
        }
        private void EditClasswork_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is int EXID)
            {
                MessageBox.Show($"Edit ClassWork with Course ID: {EXID}", "Edit ClassWork", MessageBoxButtons.OK, MessageBoxIcon.Information);
                label44.Text = EXID.ToString();

                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                string query = @"
                    SELECT Title, Description, Duration, Date 
                    FROM ClassWork 
                    WHERE CourseId = @courseId";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseId", EXID);
                        label61.Text = EXID.ToString();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Use if instead of while since CourseId should be unique
                            {
                                title_edcw.Texts = reader["Title"].ToString();
                                dateTimePicker2.Value = Convert.ToDateTime(reader["Date"]);
                                dur_edcw.Texts = reader["Duration"].ToString();
                                desc_edcw.Texts = reader["Description"].ToString();

                                editclasswork.BringToFront();
                            }
                            else
                            {
                                MessageBox.Show("No ClassWork found for this Course ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
        }
        /////
        //-----------start exam button----------------
        private void StartExam_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is int examId)
            {
                MessageBox.Show($"Starting exam with ID: {examId}", "Start Exam", MessageBoxButtons.OK, MessageBoxIcon.Information);

                exam.Controls.Clear();

                // حمّلي الامتحان في نفس البانل
                LoadExamDetails(examId);

            }
        }


        private void AddCoursePanel(string courseName, Panel currentpanel, int course_id, Image cover)
        {
            // 1. Create the new panel
            Panel newCoursePanel = new Panel();
            newCoursePanel.Size = new Size(302, 286); // Adjust as needed
            newCoursePanel.BackColor = Color.White;
            newCoursePanel.Tag = course_id;
            // 2. Find position for the new panel
            int newY = 10;
            int newx = 0;
            if (currentpanel.Controls.Count > 0)
            {
                // Get the last panel's bottom position
                Control last = currentpanel.Controls[currentpanel.Controls.Count - 1];
                if (last.Location.X + 396 < currentpanel.Width)
                {
                    newx = last.Location.X + 379;
                    newY = last.Location.Y;
                }
                else
                {
                    newx = 17;
                    newY = last.Location.Y + 306;
                    panel2.Height += +406;
                }
            }

            newCoursePanel.Location = new Point(newx, newY);



            // 4. Add the panel to panel2
            newCoursePanel.Cursor = Cursors.Hand;
            panel2.Controls.Add(newCoursePanel);
            PictureBox newpic = new PictureBox();
            newpic.Size = new Size(251, 132);

            newpic.Location = new Point(25, 26);
            newpic.BackgroundImageLayout = ImageLayout.Zoom;
            newpic.BackgroundImage = cover;
            newpic.Tag = course_id;
            newCoursePanel.Controls.Add(newpic);
            newpic.Click += coursepic_Click;
            currentpanel.Controls.Add(newCoursePanel);
            RichTextBox newtitle = new RichTextBox();
            newtitle.Size = new Size(251, 94);

            newtitle.Location = new Point(25, 176);
            newtitle.Text = courseName;
            newtitle.ForeColor = Color.FromArgb(5, 12, 22);
            newtitle.Font = new Font("Montserrat Light", 18, FontStyle.Regular);
            newtitle.ReadOnly = true;
            newtitle.BackColor = Color.White;
            newtitle.BorderStyle = BorderStyle.None;
            newCoursePanel.Controls.Add(newtitle);
            // 5. Update scrollbar

            newCoursePanel.Click += (s, e) =>
            {
                int selectedCourseId = (int)newCoursePanel.Tag;
                Register.BringToFront();
                LoadCourseDetails(selectedCourseId); // دالة جديدة نكتبها لعرض التفاصيل
                panel1.BringToFront();
            };

        }

        private void AddCoursePanel3(string courseName, Panel currentpanel, int course_id, Image cover, string grade)
        {
            // إعدادات أبعاد ومسافات البطاقة
            int cardWidth = 302;
            int cardHeight = 286;
            int spacingX = 48;   // المسافة الأفقية بين البطاقات
            int spacingY = 34;   // المسافة الرأسية بين الصفوف
            int topPadding = 10;   // مسافة من أعلى الحاوية

            // 1. إنشاء البانل الجديد
            Panel newCoursePanel = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.White,
                Tag = course_id,
                Cursor = Cursors.Hand
            };

            // 2. حساب موضعه (من اليمين إلى اليسار)
            int index = currentpanel.Controls.Count;

            // عدد الأعمدة الممكنة فى السطر الحالى
            int maxCols = Math.Max(1, (currentpanel.Width - spacingX) / (cardWidth + spacingX));

            int colIndex = index % maxCols;        // ترتيب البطاقة فى السطر
            int rowIndex = index / maxCols;        // رقم الصف

            int newX = currentpanel.Width                       // حافة الحاوية اليمنى
                       - (colIndex + 1) * cardWidth             // عرض البطاقات قبلها
                       - colIndex * spacingX                    // المسافات بينها
                       - spacingX;                              // مسافة يمين أول بطاقة

            int newY = topPadding + rowIndex * (cardHeight + spacingY);

            newCoursePanel.Location = new Point(newX, newY);

            // 3. صورة المقرر
            PictureBox newpic = new PictureBox
            {
                Size = new Size(251, 132),
                Location = new Point(25, 26),
                BackgroundImage = cover,
                BackgroundImageLayout = ImageLayout.Zoom,
                Tag = course_id,
                Cursor = Cursors.Hand
            };
            newpic.Click += coursepic_Click;
            newCoursePanel.Controls.Add(newpic);

            // 4. عنوان المقرر
            RichTextBox newtitle = new RichTextBox
            {
                Size = new Size(251, 50),
                Location = new Point(25, 168),
                Text = courseName,
                ForeColor = Color.FromArgb(5, 12, 22),
                Font = new Font("Montserrat Light", 14, FontStyle.Bold),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            newCoursePanel.Controls.Add(newtitle);

            // 5. خانة الدرجة
            Label gradeLabel = new Label
            {
                Text = "📊 Grade: " + grade,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(25, 228),
                AutoSize = true
            };
            newCoursePanel.Controls.Add(gradeLabel);

            // 6. إضافة البانل إلى الحاوية
            currentpanel.Controls.Add(newCoursePanel);
            currentpanel.BringToFront();
            panel1.BringToFront(); // لو كنت بحاجة لإبقاء panel1 فى الأعلى
        }



        private void AddCoursePanel2(string courseName, Panel currentpanel, int course_id, Image coverImage)
        {

            // 1. Create the new panel
            Panel newCoursePanel2 = new Panel();
            newCoursePanel2.Size = new Size(520, 235);
            newCoursePanel2.BackColor = Color.White;
            newCoursePanel2.Tag = course_id;

            // 2. Find position for the new panel
            int newY = 67;
            int newx = 48;
            if (currentpanel.Controls.Count > 0)
            {
                // Get the last panel's bottom position
                Control last = currentpanel.Controls[currentpanel.Controls.Count - 1];
                if (last.Location.X + last.Width + 542 < currentpanel.Width)
                {
                    newx = last.Location.X + 542;
                    newY = last.Location.Y;
                }
                else
                {
                    newx = 48;
                    newY = last.Location.Y + 246;
                }
            }

            newCoursePanel2.Location = new Point(newx, newY);



            // 4. Add the panel to panel2

            currentpanel.Controls.Add(newCoursePanel2);
            PictureBox newpic = new PictureBox();
            newpic.Size = new Size(241, 204);
            newpic.Location = new Point(13, 14);
            newpic.BackgroundImageLayout = ImageLayout.Zoom;
            newpic.BackgroundImage = coverImage;
            newpic.Tag = course_id;
            newCoursePanel2.Controls.Add(newpic);
            //newpic.Click += coursepic_Click;
            RichTextBox newtitle = new RichTextBox();
            newtitle.Size = new Size(231, 92);

            newtitle.Location = new Point(275, 25);
            newtitle.Text = courseName;
            newtitle.ForeColor = Color.FromArgb(5, 12, 22);
            newtitle.Font = new Font("Montserrat Light", 16, FontStyle.Regular);
            newtitle.ReadOnly = true;
            newtitle.BackColor = Color.White;
            newtitle.BorderStyle = BorderStyle.None;
            newCoursePanel2.Controls.Add(newtitle);
            ////////////////
            PictureBox newicon1 = new PictureBox();
            newicon1.Size = new Size(61, 45);
            newicon1.Location = new Point(444, 164);
            newicon1.BackgroundImageLayout = ImageLayout.Zoom;
            newicon1.BackgroundImage = Image.FromStream(new MemoryStream(Properties.Resources.delete_icon));
            newicon1.Cursor = Cursors.Hand;
            newicon1.Click += deleteicon_Click;
            newCoursePanel2.Controls.Add(newicon1);
            /////////
            PictureBox newicon2 = new PictureBox();
            newicon2.Size = new Size(61, 45);
            newicon2.Location = new Point(385, 164);
            newicon2.BackgroundImageLayout = ImageLayout.Zoom;
            newicon2.BackgroundImage = Image.FromStream(new MemoryStream(Properties.Resources.view_icon));
            newicon2.Cursor = Cursors.Hand;
            newCoursePanel2.Controls.Add(newicon2);
            newicon2.Click += viewicon_Click;

            ////////////
            PictureBox newicon3 = new PictureBox();
            newicon3.Size = new Size(61, 45);
            newicon3.Location = new Point(326, 164);
            newicon3.BackgroundImageLayout = ImageLayout.Zoom;
            newicon3.BackgroundImage = Image.FromStream(new MemoryStream(Properties.Resources.edit_icon));
            newicon3.Cursor = Cursors.Hand;

            newCoursePanel2.Controls.Add(newicon3);
            newicon3.Click += editicon_Click;
            ////////////
            PictureBox newicon4 = new PictureBox();
            newicon4.Size = new Size(61, 45);
            newicon4.Location = new Point(267, 164);
            newicon4.BackgroundImageLayout = ImageLayout.Zoom;
            newicon4.BackgroundImage = Image.FromStream(new MemoryStream(Properties.Resources.addlesson_icon));
            newicon4.Cursor = Cursors.Hand;

            newCoursePanel2.Controls.Add(newicon4);
            newicon4.Click += addlesson_icon_Click;
        }

        private void loadChat(Panel chat, string Name, int ID)
        {
            Panel newchat = new Panel();
            newchat.Size = new Size(282, 67);
            newchat.BackColor = Color.White;
            int newY = 43;
            int newx = 12;
            if (chat.Controls.Count > 0)
            {
                // Get the last panel's bottom position  
                Control last = chat.Controls[chat.Controls.Count - 1];
                newY = last.Location.Y + 86;
            }
            newchat.Location = new Point(newx, newY);
            newchat.Cursor = Cursors.Hand;
            chat.Controls.Add(newchat);

            PictureBox icon = new PictureBox();
            icon.Size = new Size(81, 61);
            icon.BackColor = Color.White;
            icon.Location = new Point(3, 3);
            icon.BackgroundImage = Image.FromStream(new MemoryStream(Properties.Resources.User_Circle));
            icon.BackgroundImageLayout = ImageLayout.Zoom;
            icon.Tag = ID;
            newchat.Controls.Add(icon);
            icon.Click += (sender, e) => Chatpic_Click(sender, e, chat);

            Label name = new Label();
            name.Location = new Point(74, 22);
            name.Size = new Size(200, 26);
            name.ForeColor = Color.FromArgb(5, 12, 22);
            name.BackColor = Color.White;
            name.Font = new Font("Montserrat Light", 14, FontStyle.Regular);
            name.Text = Name;
            newchat.Controls.Add(name);
            name.BringToFront();
        }

        private void loadsender_msg(Panel msg2, string content)
        {
            RichTextBox msg = new RichTextBox();
            msg.Location = new Point(25, 176);
            msg.Text = content;
            msg.ForeColor = Color.FromArgb(5, 12, 22);
            msg.Font = new Font("Montserrat Light", 16, FontStyle.Regular);
            msg.ReadOnly = true;
            msg.BackColor = SystemColors.ActiveCaption;
            msg.BorderStyle = BorderStyle.None;
            // Set a maximum width for the message box
            int maxWidth = 400;
            Size preferredSize = TextRenderer.MeasureText(msg.Text, msg.Font, new Size(maxWidth, int.MaxValue), TextFormatFlags.WordBreak);

            msg.Width = Math.Min(preferredSize.Width + 10, maxWidth);
            msg.Height = preferredSize.Height + 10;
            int newY = 43;
            int newx = 595;
            if (msg2.Controls.Count > 0)
            {
                Control last = msg2.Controls[msg2.Controls.Count - 1];
                newY = last.Bottom + 15;

            }
            if (newx + msg.Width + 20 > msg2.Width)
            {
                newx = msg2.Width - msg.Width - 37;
                if (newx < 0) newx = 0;
            }

            msg.Location = new Point(newx, newY);

            msg2.Controls.Add(msg);

        }
        private void loadreciver_msg(Panel msg4, string content)
        {

            RichTextBox msg2 = new RichTextBox();
            msg2.Location = new Point(25, 176);
            msg2.Text = content;
            msg2.ForeColor = Color.FromArgb(5, 12, 22);
            msg2.Font = new Font("Montserrat Light", 16, FontStyle.Regular);
            msg2.ReadOnly = true;
            msg2.BackColor = Color.Gainsboro;
            msg2.BorderStyle = BorderStyle.None;
            // Set a maximum width for the message box
            int maxWidth = 400;
            Size preferredSize = TextRenderer.MeasureText(msg2.Text, msg2.Font, new Size(maxWidth, int.MaxValue), TextFormatFlags.WordBreak);

            msg2.Width = Math.Min(preferredSize.Width + 10, maxWidth);
            msg2.Height = preferredSize.Height + 10;
            int newY = 43;
            int newx = 32;
            if (msg4.Controls.Count > 0)
            {
                Control last = msg4.Controls[msg4.Controls.Count - 1];
                newY = last.Bottom + 15;
            }
            if (newx + msg2.Width + 20 > panel5.Width)
            {
                newx = msg4.Width - msg2.Width - 37;
                if (newx < 0) newx = 0;
            }

            msg2.Location = new Point(newx, newY);

            msg4.Controls.Add(msg2);
        }

        private void loadMessages(Panel msg, int receiver, int sender)
        {
            msg.Controls.Clear();

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Use parameters to avoid SQL injection and use the method arguments
                SqlDataAdapter adapter = new SqlDataAdapter(
                    @"SELECT Sender_id, Reciever_id, Content, Datee, timee 
              FROM Message 
              WHERE (Sender_id = @SenderId AND Reciever_id = @ReceiverId) 
                 OR (Sender_id = @ReceiverId AND Reciever_id = @SenderId)
              ORDER BY Datee, timee", conn);
                adapter.SelectCommand.Parameters.AddWithValue("@SenderId", sender);
                adapter.SelectCommand.Parameters.AddWithValue("@ReceiverId", receiver);
                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string content = row["Content"]?.ToString() ?? string.Empty;
                    int senderId = Convert.ToInt32(row["Sender_id"]);

                    if (senderId == sender)
                        loadsender_msg(msg, content);
                    else
                        loadreciver_msg(msg, content);
                }
            }
        }
        private void loadChats_inpanel(int sender, Panel chat)
        {
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Use parameters to avoid SQL injection and use the method arguments
                SqlDataAdapter adapter = new SqlDataAdapter(
                    @"SELECT Userr.FName + ' ' + Userr.LName AS fullname , Chat.Reciever_id
                    FROM Userr
                    INNER JOIN Chat ON Chat.Reciever_id = Userr.UserId
                    WHERE Chat.Sender_id = @SenderId", conn);
                adapter.SelectCommand.Parameters.AddWithValue("@SenderId", sender);
                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string name = row["fullname"]?.ToString() ?? string.Empty;
                    int rID = Convert.ToInt32(row["Reciever_id"]);
                    loadChat(chat, name, rID);
                }
            }

        }


        private void LoadClassWork()
        {
            // مسح المحتوى القديم في panel2 (ClassWork)
            //classwork.Controls.Clear();

            // مسافة بداية الكروت
            int yOffset = 0;

            // الاتصال بقاعدة البيانات


            //string connectionString = "Server=LAPTOP-I23IVTH3;Database=Course_system;Trusted_Connection=True;";






            string connectionString = "Server=LAPTOP-I23IVTH3;Database=Course_system;Trusted_Connection=True;";

            // SQL Query لقراءة البيانات من جدول ClassWork
            string query = "SELECT C.CourseId,CW.ExId, CW.Title, CW.Duration, CW.Date, CW.Description, C.Title AS CourseTitle " +
                           "FROM ClassWork CW " +
                           "INNER JOIN Course C ON CW.CourseId = C.CourseId";

            // الاتصال بقاعدة البيانات باستخدام SqlConnection
            using (SqlConnection conn = new SqlConnection(connectionString))
            {


                conn.Open(); // فتح الاتصال

                // تنفيذ الاستعلام باستخدام SqlCommand
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // قراءة البيانات باستخدام SqlDataReader
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // إنشاء كرت جديد لكل ClassWork
                            Panel card = new Panel
                            {
                                Size = new Size(700, 200), // تحديد حجم الكارت
                                Location = new Point(0, yOffset),
                                BackColor = Color.FromArgb(200, 200, 200),
                                Font = new Font("Montserrat ExtraBold", 10),
                            };

                            // عنوان الكورس
                            Label lblCourse = new Label
                            {
                                Text = $"Course: {reader["CourseTitle"]}",
                                Location = new Point(10, 10),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            // عنوان التمرين
                            Label lblTitle = new Label
                            {
                                Text = $"Title: {reader["Title"]}",
                                Location = new Point(10, 40),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            // الوصف
                            Label lblDesc = new Label
                            {
                                Text = $"Description: {reader["Description"]}",
                                Location = new Point(10, 70),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            // التاريخ
                            Label lblDate = new Label
                            {
                                Text = $"Date: {Convert.ToDateTime(reader["Date"]).ToShortDateString()}",
                                Location = new Point(400, 10),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            // المدة
                            Label lblDuration = new Label
                            {
                                Text = $"Duration: {reader["Duration"]}",
                                Location = new Point(400, 40),
                                AutoSize = true,
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };

                            // زرار رفع ملف
                            Button uploadBtn = new Button
                            {
                                Text = "Add Link",
                                Location = new Point(460, 140),
                                Size = new Size(190, 40),
                                BackColor = Color.FromArgb(5, 12, 22),
                                ForeColor = Color.White,
                                FlatStyle = FlatStyle.Flat
                            };
                            TextBox linktxt = new TextBox
                            {
                                PlaceholderText = "Add Link of Drive",
                                Location = new Point(30, 145),
                                Size = new Size(400, 40),
                                BackColor = Color.White,
                                ForeColor = Color.FromArgb(5, 12, 22),
                                Font = new Font("Montserrat Light", 12, FontStyle.Bold)
                            };
                            uploadBtn.FlatAppearance.BorderSize = 0;

                            // احصل على المفتاح الأساسي للكارت (CourseId و ExId)
                            int courseId = Convert.ToInt32(reader["CourseId"]);
                            int exId = Convert.ToInt32(reader["ExId"]);
                            uploadBtn.Tag = (courseId, exId);

                            // الحدث عند الضغط على الزر
                            uploadBtn.Click += (s, e) =>
                            {
                                if (!linktxt.ReadOnly)
                                {
                                    string link = linktxt.Text.Trim();


                                    string connectionString = "Server=LAPTOP-I23IVTH3;Database=Course_system;Trusted_Connection=True;";
                                    string existingData = "";

                                    using (SqlConnection conn = new SqlConnection(connectionString))
                                    {
                                        conn.Open();


                                        using (SqlCommand getCmd = new SqlCommand("SELECT AnswerPdf FROM ClassWork WHERE CourseId = @CourseId AND ExId = @ExId", conn))

                                        {
                                            getCmd.Parameters.AddWithValue("@CourseId", courseId);
                                            getCmd.Parameters.AddWithValue("@ExId", exId);

                                            var resultData = getCmd.ExecuteScalar();
                                            if (resultData != null && resultData != DBNull.Value)
                                            {
                                                existingData = resultData.ToString();


                                                string marker = $"Submit of UserID: {SE_ID}";
                                                int markerIndex = existingData.IndexOf(marker);

                                                if (markerIndex != -1)
                                                {

                                                    int linkStart = existingData.IndexOf("\n", markerIndex) + 1;
                                                    int linkEnd = existingData.IndexOf("\n", linkStart);

                                                    if (linkStart > 0 && linkEnd > linkStart)
                                                    {
                                                        string userLink = existingData.Substring(linkStart, linkEnd - linkStart).Trim();
                                                        linktxt.Text = userLink;
                                                    }

                                                    MessageBox.Show("You have already submitted your link.", "Duplicate Submission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                    linktxt.ReadOnly = true;
                                                    return;
                                                }
                                            }
                                        }

                                        if (string.IsNullOrWhiteSpace(link))
                                        {
                                            MessageBox.Show("Please enter the Google Drive link first.");
                                            return;
                                        }

                                        DialogResult confirm = MessageBox.Show(
                                            "Are you sure the Drive link is correct and publicly accessible?",
                                            "Confirm Link",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question
                                        );

                                        if (confirm != DialogResult.Yes)
                                            return;

                                        // Prepare formatted entry
                                        string formattedEntry =
                                            $"/////////////////////// Submit of UserID: {SE_ID} ///////////////////////////////////\n" +
                                            $"{link}\n\n";

                                        string updatedData = existingData + formattedEntry;

                                        // Update database
                                        using (SqlCommand updateCmd = new SqlCommand("UPDATE ClassWork SET AnswerPdf = @PDF WHERE CourseId = @CourseId AND ExId = @ExId", conn))
                                        {
                                            updateCmd.Parameters.AddWithValue("@PDF", updatedData);
                                            updateCmd.Parameters.AddWithValue("@CourseId", courseId);
                                            updateCmd.Parameters.AddWithValue("@ExId", exId);
                                            updateCmd.ExecuteNonQuery();
                                        }
                                    }

                                    MessageBox.Show("Link added successfully!");
                                    linktxt.ReadOnly = true;
                                }
                                else
                                {
                                    MessageBox.Show("You have already submitted your link.", "Duplicate Submission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            };

                            // إضافة الزر إلى الكارت


                            // إضافة العناصر للكرت
                            card.Controls.Add(lblCourse);
                            card.Controls.Add(lblTitle);
                            card.Controls.Add(lblDesc);
                            card.Controls.Add(lblDate);
                            card.Controls.Add(lblDuration);
                            card.Controls.Add(uploadBtn);
                            card.Controls.Add(linktxt);
                            // إضافة الكارت للـ panel2
                            panel9.Controls.Add(card);

                            // تحديث المسافة بين الكروت
                            yOffset += 230;
                        }
                    }


                }
            }


        }
        private void LoadCourses_admin()
        {

            panel6.Controls.Clear();


            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            //////Load Course of the admin
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string selectQuery = @"SELECT CourseId,Cover,Title
                      FROM Course c 
                      WHERE c.UserId = @SenderId ";
                using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                {
                    selectCmd.Parameters.AddWithValue("@SenderId", SE_ID);

                    // Replace this block inside the `add_course_sql_Click` method
                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        Image image = null;

                        while (reader.Read())
                        {
                            int courseId = reader["CourseId"] != DBNull.Value ? Convert.ToInt32(reader["CourseId"]) : 0;
                            string title = reader["Title"] != DBNull.Value ? Convert.ToString(reader["Title"]) : string.Empty;
                            // Convert the Cover column (byte[]) to an Image
                            if (reader["Cover"] != DBNull.Value)
                            {
                                byte[] coverImageBytes = (byte[])reader["Cover"]; // Renamed variable to avoid conflict
                                using (MemoryStream ms = new MemoryStream(coverImageBytes))
                                {
                                    image = Image.FromStream(ms);
                                }
                            }

                            AddCoursePanel2(title, panel6, courseId, image);
                        }
                    }
                }
            }
        }
        private void LoadCourses_user()
        {

            // home.BringToFront();
            // panel1.BringToFront();
            panel2.Controls.Clear();


            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string selectQuery = @"
                SELECT 
                    c.CourseId,
                    c.UserId,
                    c.Cover,
                    c.Title,
                    c.Description,
                    c.Category,
                    c.Studying_Year,
                    c.Semester,
                    c.PassedExam_ID,
                    u.Fname + ' ' + u.Lname AS FullName
                FROM 
                    Course c
                INNER JOIN 
                    Userr u ON c.UserId = u.UserId
                WHERE 
                    c.is_hidden = 1 ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                using (SqlDataReader reader = selectCmd.ExecuteReader())
                {

                    int yOffset = 10;

                    //int yOffset = 10;



                    while (reader.Read())
                    {
                        byte[] coverBytes = reader["Cover"] as byte[];
                        Image coverImage = null;

                        if (coverBytes != null && coverBytes.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(coverBytes))
                            {
                                coverImage = Image.FromStream(ms);
                            }
                        }
                        // Example fields
                        string title = reader["Title"].ToString();
                        string description = reader["Description"].ToString();
                        string category = reader["Category"].ToString();
                        string fullName = reader["FullName"].ToString();
                        string courseid = reader["CourseId"].ToString();
                        AddCoursePanel(title, panel2, Convert.ToInt32(courseid), coverImage);

                        //yOffset += 160;
                        //Panel coursePanel = new Panel
                        //{
                        //    Size = new Size(700, 150),
                        //    Location = new Point(10, yOffset),
                        //    BorderStyle = BorderStyle.FixedSingle,
                        //    BackColor = Color.White
                        //};

                        //Label lblTitle = new Label
                        //{
                        //    Text = "📚 " + title,
                        //    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        //    Location = new Point(10, 10),
                        //    AutoSize = true
                        //};

                        //Label lblDescription = new Label
                        //{
                        //    Text = "📝 " + description,
                        //    Font = new Font("Segoe UI", 10),
                        //    Location = new Point(10, 40),
                        //    Size = new Size(650, 40),
                        //    ForeColor = Color.Gray
                        //};

                        //Label lblCategory = new Label
                        //{
                        //    Text = "📂 Category: " + category,
                        //    Font = new Font("Segoe UI", 10),
                        //    Location = new Point(10, 90),
                        //    AutoSize = true
                        //};

                        //Label lblFullName = new Label
                        //{
                        //    Text = "👤 Instructor: " + fullName,
                        //    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                        //    Location = new Point(10, 115),
                        //    AutoSize = true
                        //};

                        //coursePanel.Controls.Add(lblTitle);
                        //coursePanel.Controls.Add(lblDescription);
                        //coursePanel.Controls.Add(lblCategory);
                        //coursePanel.Controls.Add(lblFullName);


                        ///////////-----------------------------------------------

                    }
                }
            }
        }

        private void LoadCourseDetails(int courseId)
        {
            Register.Controls.Clear(); // نظف البانل

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string selectQuery = @"
              SELECT 
                  c.CourseId,
                  c.Title,
                  c.Description,
                  c.Category,
                  c.Studying_Year,
                  c.Semester,
                  u.Fname + ' ' + u.Lname AS FullName
              FROM 
                  Course c
               Inner JOIN 
                  Userr u ON c.UserId = u.UserId
              WHERE 
                  c.CourseId = @CourseId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Panel coursePanel = new Panel
                            {
                                Size = new Size(700, 200),
                                Location = new Point(200, 120),
                                BorderStyle = BorderStyle.FixedSingle,
                                BackColor = Color.White
                            };

                            Label lblTitle = new Label
                            {
                                Text = "📚 Title: " + reader["Title"],
                                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                                Location = new Point(10, 10),
                                AutoSize = true
                            };

                            Label lblDescription = new Label
                            {
                                Text = "📝 Description: " + reader["Description"],
                                Font = new Font("Segoe UI", 10),
                                Location = new Point(10, 40),
                                Size = new Size(650, 40),
                                ForeColor = Color.Gray
                            };

                            Label lblCategory = new Label
                            {
                                Text = "📂 Category: " + reader["Category"],
                                Font = new Font("Segoe UI", 10),
                                Location = new Point(10, 90),
                                AutoSize = true
                            };

                            Label lblInstructor = new Label
                            {
                                Text = "👤 Instructor: " + reader["FullName"],
                                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                                Location = new Point(10, 120),
                                AutoSize = true
                            };
                            Button registerBtn = new Button
                            {
                                Text = "Register",
                                Location = new Point(580, 115),
                                Size = new Size(90, 30),
                                BackColor = Color.FromArgb(100, 149, 237),
                                ForeColor = Color.White,
                                FlatStyle = FlatStyle.Flat,
                                Tag = courseId
                            };

                            registerBtn.FlatAppearance.BorderSize = 0;
                            registerBtn.Click += RegisterBtn_Click;

                            Button backBtn = new Button
                            {
                                Text = "← Back",
                                Location = new Point(600, 10),
                                Size = new Size(80, 30),
                                BackColor = Color.LightGray
                            };


                            backBtn.Click += (s, e) =>
                            {
                                LoadCourses_user();        // تحميل الكورسات
                                home.BringToFront();       // عرض بانيل الهوم
                                panel1.BringToFront();     // جلبها للواجهة
                            };
                            coursePanel.Controls.Add(lblTitle);
                            coursePanel.Controls.Add(lblDescription);
                            coursePanel.Controls.Add(lblCategory);
                            coursePanel.Controls.Add(lblInstructor);
                            coursePanel.Controls.Add(backBtn);
                            coursePanel.Controls.Add(registerBtn);
                            Register.Controls.Add(coursePanel);


                        }
                    }
                }
            }
        }
        private void LoadGrades(int instructorId)
        {
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string selectQuery = @"
                SELECT 
                    u.UserId AS StudentID,
                    u.Fname + ' ' + u.Lname AS StudentName,
                    c.CourseId AS CourseID,
                    c.Title AS CourseTitle,
                    r.Grade
                FROM 
                    Register r
                INNER JOIN 
                    Userr u ON r.UserId = u.UserId
                INNER JOIN 
                    Course c ON r.CourseId = c.CourseId
                WHERE 
                    u.User_Role <> 'Instructor' AND
                    c.UserId = @InstructorId";

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorId", instructorId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            dataGridView1.DataSource = dt;
        }
        private void RegisterBtn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int courseId = (int)btn.Tag;
            // استبدليها باليوزر الحقيقي

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM Register WHERE UserId = @UserId AND CourseId = @CourseId";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserId", SE_ID);
                    checkCmd.Parameters.AddWithValue("@CourseId", courseId);

                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("You are already registered in this course.");
                        return;
                    }
                }

                string insertQuery = "INSERT INTO Register (UserId, CourseId, Grade) VALUES (@UserId, @CourseId, 0)";
                using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@UserId", SE_ID);
                    insertCmd.Parameters.AddWithValue("@CourseId", courseId);
                    insertCmd.ExecuteNonQuery();

                }

                MessageBox.Show("Registered successfully!");

                // إعادة تحميل كل الكورسات المسجلة
                panel3.Controls.Clear();
                LoadAllRegisteredCourses(SE_ID);

                // إظهار بانل الكورسات
                handleicon(coursesicon);
                courses.BringToFront();
                panel1.BringToFront();
            }


        }

        private void LoadAllRegisteredCourses(int userId)
        {

            //  courses.Controls.Clear();
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string query = @"
                SELECT c.CourseId, c.Title, c.Description, c.Cover , r.Grade
                FROM Course c
                INNER JOIN Register r ON c.CourseId = r.CourseId
                WHERE r.UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int courseId = Convert.ToInt32(reader["CourseId"]);
                            string title = reader["Title"].ToString();
                            string Grade = reader["Grade"].ToString();
                            byte[] coverBytes = reader["Cover"] as byte[];
                            Image coverImage = null;

                            if (coverBytes != null)
                            {
                                using (MemoryStream ms = new MemoryStream(coverBytes))
                                {
                                    coverImage = Image.FromStream(ms);
                                }
                            }

                            AddCoursePanel3(title, panel3, courseId, coverImage, Grade);
                            // courses.BringToFront(); 
                            //panel1.BringToFront();
                        }
                    }
                }
            }
        }

        private void LoadRegisteredCourseToPanel(int courseId)
        {


            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string selectQuery = @"
                SELECT c.CourseId, c.Title, c.Description, c.Cover , r.grade ,r.UserId
                FROM Course c
                 join Register r ON c.CourseId = r.CourseId
                WHERE c.CourseId = @CourseId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            string description = reader["Description"].ToString();
                            string Grade = reader["Grade"].ToString();
                            byte[] coverBytes = reader["Cover"] as byte[];
                            Image coverImage = null;

                            if (coverBytes != null)
                            {
                                using (MemoryStream ms = new MemoryStream(coverBytes))
                                {
                                    coverImage = Image.FromStream(ms);
                                }
                            }

                            AddCoursePanel3(title, courses, courseId, coverImage, Grade);

                        }
                    }
                }
            }
        }


        //------------------------ Exam page (Esraa&Dado) --------------------------------------------



        //------------------------Exam page (Esraa&Dado)--------------------------------------------

        private void LoadExams()
        {
            panel11.Controls.Clear();
            int yOffset = 0;

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string query = "SELECT E.ExamId, E.Title, E.Description, E.Duration, E.Date, C.Title AS CourseTitle " +
                           "FROM Exam E " +
                           "INNER JOIN Course C ON E.ExamId = C.ExamId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int examId = Convert.ToInt32(reader["ExamId"]);






                        Panel card = new Panel
                        {
                            Size = new Size(750, 200),
                            Location = new Point(0, yOffset),
                            BackColor = Color.FromArgb(230, 230, 250),
                            BorderStyle = BorderStyle.Fixed3D,
                            Padding = new Padding(10),
                            //Tag = examId
                        };


                        Label lblCourse = new Label
                        {
                            Text = $"📚 Course: {reader["CourseTitle"]}",
                            Location = new Point(10, 10),
                            Font = new Font("Montserrat light", 11, FontStyle.Bold),
                            AutoSize = true
                        };

                        Label lblTitle = new Label
                        {
                            Text = $"📝 Type: {reader["Title"]}",
                            Location = new Point(10, 45),
                            Font = new Font("Montserrat light", 11),
                            AutoSize = true
                        };

                        Label lblDuration = new Label
                        {
                            Text = $"⏱ Duration: {reader["Duration"]}",
                            Location = new Point(10, 80),
                            Font = new Font("Montserrat light", 11),
                            AutoSize = true
                        };

                        Label lblDate = new Label
                        {
                            Text = $"📅 Date: {Convert.ToDateTime(reader["Date"]).ToShortDateString()}",
                            Location = new Point(10, 115),
                            Font = new Font("Montserrat light", 11),
                            AutoSize = true
                        };

                        Label lblDesc = new Label
                        {
                            Text = $"🧾 Description: {reader["Description"]}",
                            Location = new Point(10, 150),
                            Font = new Font("Montserrat light", 11),
                            AutoSize = true,

                        };

                        // ✅ زرار Start Exam
                        Button startBtn = new Button
                        {
                            Text = "Start Exam",
                            Location = new Point(600, 130),
                            Size = new Size(120, 30),
                            BackColor = Color.FromArgb(153, 102, 255),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Tag = examId
                        };
                        startBtn.FlatAppearance.BorderSize = 0;
                        startBtn.Click += StartExam_Click;

                        // Add controls to card

                        card.Controls.Add(lblCourse);
                        card.Controls.Add(lblTitle);
                        card.Controls.Add(lblDuration);
                        card.Controls.Add(lblDate);
                        card.Controls.Add(lblDesc);
                        card.Controls.Add(startBtn);



                        panel11.Controls.Add(card);
                        yOffset += 200;
                    }
                }
            }

        }

        //-----------------------the exam -----------------------
        private void LoadExamDetails(int examId)
        {
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string query = "SELECT Title, Description, Question, Duration, Date FROM Exam WHERE ExamId = @ExamId";


            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ExamId", examId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // exam.Controls.Clear(); // تأكيد مسح المحتوى
                        Color textColor = Color.FromArgb(40, 43, 130); // #282B82    
                        Color highlight = Color.FromArgb(29, 31, 93);

                        // العنوان
                        Label lblTitle = new Label()
                        {
                            Text = $"📝 Exam: {reader["Title"]}",
                            Font = new Font("Montserrat ExtraBold", 14, FontStyle.Bold),
                            Location = new Point(250, 30),
                            AutoSize = true,
                            ForeColor = highlight

                        };

                        // المدة
                        Label lblDuration = new Label()
                        {
                            Text = $"⏱ Duration: {reader["Duration"]}",
                            Font = new Font("Montserrat ExtraBold", 12),
                            Location = new Point(605, 30),
                            AutoSize = true,
                            ForeColor = textColor
                        };

                        // التاريخ
                        Label lblDate = new Label()
                        {
                            Text = $"📅 Date: {Convert.ToDateTime(reader["Date"]).ToShortDateString()}",
                            Font = new Font("Montserrat ExtraBold", 12),
                            Location = new Point(955, 30),
                            AutoSize = true,
                            ForeColor = textColor
                        };

                        // الوصف
                        Label lblDesc = new Label()
                        {
                            Text = $"📄 Content: {reader["Description"]}",
                            Font = new Font("Montserrat ExtraBold", 12),
                            Location = new Point(250, 90),
                            AutoSize = true,
                            ForeColor = textColor
                        };

                        // السؤال
                        Label lblQ = new Label()
                        {
                            Text = $"❓ Question:",
                            Font = new Font("Montserrat ExtraBold", 12, FontStyle.Bold),
                            Location = new Point(280, 170),
                            AutoSize = true,
                            ForeColor = highlight
                        };

                        RichTextBox txtQuestion = new RichTextBox()
                        {
                            Text = reader["Question"].ToString(),
                            Location = new Point(280, 220),
                            Size = new Size(650, 100),
                            ReadOnly = true,
                            Font = new Font("Montserrat Light", 11),
                            BackColor = Color.FromArgb(245, 245, 245),
                            BorderStyle = BorderStyle.FixedSingle

                        };

                        // 📝 TextBox للإجابة
                        Label lblAnswer = new Label()
                        {
                            Text = "✍️ Your Answer:",
                            Font = new Font("Montserrat ExtraBold", 12, FontStyle.Bold),
                            Location = new Point(280, 330),
                            AutoSize = true,
                            ForeColor = highlight
                        };

                        TextBox txtAnswer = new TextBox()
                        {
                            Location = new Point(280, 380),
                            Size = new Size(650, 80),
                            Multiline = true,
                            Font = new Font("Montserrat Light", 11),
                            BorderStyle = BorderStyle.FixedSingle
                        };

                        // ✅ زرار Submit
                        Button submitBtn = new Button()
                        {
                            Text = "Submit Answer",
                            Location = new Point(1000, 440),
                            Size = new Size(140, 40),
                            BackColor = highlight,
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat
                        };
                        submitBtn.FlatAppearance.BorderSize = 0;
                        submitBtn.Font = new Font("Montserrat ExtraBold", 11);
                        submitBtn.Click += (s, e) =>
                        {
                            SE_ID = 12;
                            string answer = txtAnswer.Text;
                            if (string.IsNullOrWhiteSpace(answer))
                            {
                                MessageBox.Show("Please enter your answer first.", "Empty Answer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                                using (SqlConnection conn = new SqlConnection(connectionString))
                                {
                                    conn.Open();

                                    string currentAnswer = "";
                                    string selectQuery = "SELECT Answers FROM Exam WHERE ExamId = @ExamId";
                                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                                    {
                                        selectCmd.Parameters.AddWithValue("@ExamId", examId);
                                        var result = selectCmd.ExecuteScalar();
                                        if (result != null && result != DBNull.Value)
                                        {
                                            currentAnswer = result.ToString();
                                        }
                                    }

                                    string formattedAnswer = $"/////////////////////// Answers of User ID: {SE_ID} ///////////////////////\n{answer}\n";

                                    // Step 3: Combine old and new answers
                                    string combinedAnswer = string.IsNullOrWhiteSpace(currentAnswer)
                                        ? formattedAnswer
                                        : currentAnswer + "\n" + formattedAnswer;

                                    // Step 4: Update the answer in the database
                                    string updateQuery = "UPDATE Exam SET Answers = @Answer WHERE ExamId = @ExamId";
                                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@Answer", combinedAnswer);
                                        cmd.Parameters.AddWithValue("@ExamId", examId);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                MessageBox.Show("Your answer has been submitted.\nThank you!", "Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadExams();
                            }
                        };

                        // ➕ ضيف كل حاجة لـ panel3
                        exam.Controls.Add(lblTitle);
                        exam.Controls.Add(lblDuration);
                        exam.Controls.Add(lblDate);
                        exam.Controls.Add(lblDesc);
                        exam.Controls.Add(lblQ);
                        exam.Controls.Add(txtQuestion);
                        exam.Controls.Add(lblAnswer);
                        exam.Controls.Add(txtAnswer);
                        exam.Controls.Add(submitBtn);


                    }
                }
            }
        }


        //--------------------------Profile page (Esraa and Dado)-------------------------



        private void LoadUserProfile()
        {


            //string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

            string query = "SELECT FName, LName, Email, User_Role FROM Userr WHERE UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", SE_ID);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        panel15.Controls.Clear(); // نفضي البانل قبل ما نعرض بيانات جديدة

                        Panel card = new Panel
                        {
                            Size = new Size(600, 300),
                            Location = new Point(50, 70),
                            BackColor = Color.White,
                            BorderStyle = BorderStyle.FixedSingle
                        };

                        Label lblFName = new Label() { Text = "First Name:", Location = new Point(20, 20), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold) };
                        TextBox txtFName = new TextBox() { Text = reader["FName"].ToString(), Location = new Point(150, 20), Width = 200, ReadOnly = true, Name = "txtFName" };

                        Label lblLName = new Label() { Text = "Last Name:", Location = new Point(20, 60), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold) };
                        TextBox txtLName = new TextBox() { Text = reader["LName"].ToString(), Location = new Point(150, 60), Width = 200, ReadOnly = true, Name = "txtLName" };

                        Label lblEmail = new Label() { Text = "Email:", Location = new Point(20, 100), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold) };
                        TextBox txtEmail = new TextBox() { Text = reader["Email"].ToString(), Location = new Point(150, 100), Width = 200, ReadOnly = true, Name = "txtEmail" };

                        Label lblrole = new Label() { Text = "Role:", Location = new Point(20, 140), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold) };
                        TextBox txtrole = new TextBox() { Text = reader["User_Role"].ToString(), Location = new Point(150, 140), Width = 200, ReadOnly = true, Name = "txtrole" };
                        Label ID = new Label() { Text = "ID: " + SE_ID, Location = new Point(20, 195), Font = new Font("Montserrat Light", 10, FontStyle.Bold), };
                        ID.BringToFront();
                        Button btnUpdate = new Button()
                        {
                            Text = "Update",
                            Location = new Point(400, 220),
                            Size = new Size(100, 40), // ممكن تعدل الحجم حسب التصميم
                            BackColor = Color.FromArgb(5, 12, 22),
                            ForeColor = Color.White,
                            Font = new Font("Montserrat Light", 10, FontStyle.Bold),
                            FlatStyle = FlatStyle.Flat,
                            TextAlign = ContentAlignment.MiddleCenter,


                        };
                        btnUpdate.Click += EnableEditing;

                        card.Controls.AddRange(new Control[] { lblFName, txtFName, lblLName, txtLName, lblEmail, txtEmail, lblrole, txtrole, btnUpdate, ID });

                        panel15.Controls.Add(card);
                    }
                }
            }
        }
        private void LoadAdminProfile()
        {

            //SE_ID = SE_ID;
            //string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

            string query = "SELECT FName, LName, Email, User_Role FROM Userr WHERE UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", SE_ID);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        profile_cont.Controls.Clear(); // نفضي البانل قبل ما نعرض بيانات جديدة

                        Panel card = new Panel
                        {
                            Size = new Size(550, 300),
                            Location = new Point(50, 50),
                            BackColor = Color.White,
                            BorderStyle = BorderStyle.FixedSingle
                        };

                        Label lblFName = new Label() { Text = "First Name:", Location = new Point(20, 25), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold), };
                        TextBox txtFName = new TextBox() { Text = reader["FName"].ToString(), Location = new Point(150, 25), Width = 200, ReadOnly = true, Name = "txtFName" };

                        Label lblLName = new Label() { Text = "Last Name:", Location = new Point(20, 65), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold), };
                        TextBox txtLName = new TextBox() { Text = reader["LName"].ToString(), Location = new Point(150, 65), Width = 200, ReadOnly = true, Name = "txtLName" };

                        Label lblEmail = new Label() { Text = "Email:", Location = new Point(20, 105), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold), };
                        TextBox txtEmail = new TextBox() { Text = reader["Email"].ToString(), Location = new Point(150, 105), Width = 200, ReadOnly = true, Name = "txtEmail" };

                        Label lblrole = new Label() { Text = "Role:", Location = new Point(20, 145), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold), };
                        TextBox txtrole = new TextBox() { Text = reader["User_Role"].ToString(), Location = new Point(150, 145), Width = 200, ReadOnly = true, Name = "txtrole" };

                        Label ID = new Label() { Text = "UserId: " + SE_ID, Location = new Point(20, 195), Font = new Font("Montserrat ExtraBold", 10, FontStyle.Bold), };

                        Button btnUpdate = new Button()
                        {
                            Text = "Update",
                            Location = new Point(400, 220),
                            Size = new Size(100, 40), // ممكن تعدل الحجم حسب التصميم
                            BackColor = Color.FromArgb(5, 12, 22),
                            ForeColor = Color.White,
                            Font = new Font("Montserrat Light", 10, FontStyle.Bold),
                            FlatStyle = FlatStyle.Flat,
                            TextAlign = ContentAlignment.MiddleCenter,


                        };
                        btnUpdate.Click += EnableEditing2;

                        card.Controls.AddRange(new Control[] { lblFName, txtFName, lblLName, txtLName, lblEmail, txtEmail, lblrole, txtrole, btnUpdate, ID });

                        profile_cont.Controls.Add(card);
                    }
                }
            }
        }


        private void EnableEditing(object sender, EventArgs e)
        {
            Panel card = ((Button)sender).Parent as Panel;

            foreach (Control ctrl in card.Controls)
            {
                if (ctrl is TextBox txt)
                {
                    if (txt.Name != "txtrole")
                        txt.ReadOnly = false;
                }
            }

            Button btnSave = new Button()
            {
                Text = "Save",
                Font = new Font("Montserrat", 10, FontStyle.Bold),
                Location = new Point(300, 220),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnSave.Click += SaveUserProfile;

            card.Controls.Add(btnSave);
        }
        private void EnableEditing2(object sender, EventArgs e)
        {
            Panel card = ((Button)sender).Parent as Panel;

            foreach (Control ctrl in card.Controls)
            {
                if (ctrl is TextBox txt)
                {
                    if (txt.Name != "txtrole")
                        txt.ReadOnly = false;
                }
            }

            Button btnSave = new Button()
            {
                Text = "Save",
                Font = new Font("Montserrat", 10, FontStyle.Bold),
                Location = new Point(300, 220),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnSave.Click += SaveUserProfile2;

            card.Controls.Add(btnSave);
        }

        private void SaveUserProfile(object sender, EventArgs e)
        {
            Panel card = ((Button)sender).Parent as Panel;

            string fname = ((TextBox)card.Controls["txtFName"]).Text;
            string lname = ((TextBox)card.Controls["txtLName"]).Text;
            string email = ((TextBox)card.Controls["txtEmail"]).Text;
            string role = ((TextBox)card.Controls["txtrole"]).Text;

            //string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

            string updateQuery = "UPDATE Userr SET FName = @FName, LName = @LName, Email = @Email, User_Role = @role WHERE UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@FName", fname);
                cmd.Parameters.AddWithValue("@LName", lname);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@role", role);
                cmd.Parameters.AddWithValue("@UserId", SE_ID);

                conn.Open();
                cmd.ExecuteNonQuery();
                MessageBox.Show("Profile updated successfully!");

                LoadUserProfile(); // رجع البيانات بعد التعديل
            }
        }
        private void SaveUserProfile2(object sender, EventArgs e)
        {
            Panel card = ((Button)sender).Parent as Panel;

            string fname = ((TextBox)card.Controls["txtFName"]).Text;
            string lname = ((TextBox)card.Controls["txtLName"]).Text;
            string email = ((TextBox)card.Controls["txtEmail"]).Text;
            string role = ((TextBox)card.Controls["txtrole"]).Text;

            //string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

            string updateQuery = "UPDATE Userr SET FName = @FName, LName = @LName, Email = @Email, User_Role = @role WHERE UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@FName", fname);
                cmd.Parameters.AddWithValue("@LName", lname);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@role", role);
                cmd.Parameters.AddWithValue("@UserId", SE_ID);

                conn.Open();
                cmd.ExecuteNonQuery();
                MessageBox.Show("Profile updated successfully!");

                LoadUserProfile(); // رجع البيانات بعد التعديل
            }
        }






        public Form1()
        {


            List<string> myCourses = new List<string> { "Math", "Physics", "Programming" };

            InitializeComponent();
            string courseName = "Course " + (panel2.Controls.Count + 1);



        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {


            panel2OriginalY = panel2.Location.Y; // Save original position
            panel3OriginalY = panel3.Location.Y;
            chatriginalY = ChatContainer.Location.Y;

            string targetWord = "learn";
            int startIndex = richTextBox1.Text.IndexOf(targetWord, StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                richTextBox1.Select(startIndex, targetWord.Length);
                richTextBox1.SelectionColor = Color.FromArgb(221, 168, 83);
                richTextBox1.Select(0, 0); // Deselect text
            }

            // Example: Fill a DataTable and bind to a DataGridView


            LoadCourses_user();

            LoadCourses_admin();



        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void homeicon_Click(object sender, EventArgs e)
        {
            handleicon(homeicom2);
            home.BringToFront();
            panel1.BringToFront();

        }

        private void classworkicon_Click(object sender, EventArgs e)
        {
            handleicon(classworkicon2);
            classwork.BringToFront();
            LoadClassWork();
            panel1.BringToFront();

        }

        private void coursesicon_Click(object sender, EventArgs e)
        {

            handleicon(coursesicon2);
            panel3.Controls.Clear();

            courses.BringToFront();
            LoadAllRegisteredCourses(SE_ID);

            panel1.BringToFront();

        }

        private void chaticon_Click(object sender, EventArgs e)
        {
            handleicon(chaticon2);
            chat.BringToFront();
            ChatContainer.Controls.Clear();
            loadChats_inpanel(SE_ID, ChatContainer);
            panel1.BringToFront();


        }

        private void examicon_Click(object sender, EventArgs e)
        {
            handleicon(examicon2);
            exam.BringToFront();
            LoadExams();
            panel1.BringToFront();


        }

        private void usericon_MouseEnter(object sender, EventArgs e)
        {
            usericon2.BringToFront();
        }

        private void usericon2_MouseHover(object sender, EventArgs e)
        {
            if (!click_check)
                usericon.BringToFront();
        }

        private void usericon_Click(object sender, EventArgs e)
        {
            handleicon(usericon2);
            click_check = true;
            profile.BringToFront();
            LoadUserProfile();
            panel1.BringToFront();
        }

        private void usericon2_Click(object sender, EventArgs e)
        {

            handleicon(usericon2);
            click_check = true;
            profile.BringToFront();
            LoadUserProfile();
            panel1.BringToFront();
        }


        private void rjTextBox2__TextChanged(object sender, EventArgs e)
        {

        }

        private void rjButton1_Click(object sender, EventArgs e)
        {
            string email = emaillogin.Texts.Trim();
            string password = passwordlogin.Texts;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.");
                return;
            }

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string query = "SELECT UserId, User_Role, Fname FROM Userr WHERE Email = @Email AND Pass_word = @Password";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {

                                SE_ID = reader.GetInt32(reader.GetOrdinal("UserId"));
                                RoleID = reader.GetString(reader.GetOrdinal("User_Role"));
                                NameID = reader.GetString(reader.GetOrdinal("Fname"));

                                MessageBox.Show("Login successful.");
                                if (RoleID == "Instructor")
                                {
                                    richTextBox4.Text = richTextBox4.Text.Replace("Gamal", NameID);
                                    string targetWord2 = "Teaching";
                                    int start2 = 0;

                                    while ((start2 = richTextBox1.Text.IndexOf(targetWord2, start2, StringComparison.InvariantCultureIgnoreCase)) != -1)
                                    {
                                        richTextBox4.Select(start2, targetWord2.Length);
                                        richTextBox4.SelectionColor = Color.FromArgb(221, 168, 83);
                                        start2 += targetWord2.Length;
                                    }

                                    // Reset selection to avoid unintended styling
                                    richTextBox4.SelectionStart = richTextBox4.Text.Length;
                                    richTextBox4.SelectionLength = 0;
                                    richTextBox4.SelectionColor = richTextBox4.ForeColor;
                                    LoadCourses_admin();
                                    icons.BringToFront();
                                    teacher.BringToFront();
                                }
                                else
                                {
                                    richTextBox1.Text = richTextBox1.Text.Replace("Gamal", NameID);
                                    string targetWord = "learn";
                                    int start = 0;

                                    while ((start = richTextBox1.Text.IndexOf(targetWord, start, StringComparison.InvariantCultureIgnoreCase)) != -1)
                                    {
                                        richTextBox1.Select(start, targetWord.Length);
                                        richTextBox1.SelectionColor = Color.FromArgb(221, 168, 83);
                                        start += targetWord.Length;
                                    }

                                    // Reset selection to avoid unintended styling
                                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                    richTextBox1.SelectionLength = 0;
                                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                                    LoadCourses_user();
                                    home.BringToFront();
                                    panel1.BringToFront();
                                }

                            }
                            else
                            {
                                MessageBox.Show("Invalid email or password.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

        }

        private void rjTextBox8__TextChanged(object sender, EventArgs e)
        {

        }





        private void searchbutton_Click(object sender, EventArgs e)
        {


        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void sendbtn_Click(object sender, EventArgs e)
        {
            if (messagebar.Texts != "")
            {
                string msg = messagebar.Texts;
                loadsender_msg(panel5, msg);
                messagebar.Texts = "";

                // Insert message into database
                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string insertQuery = @"INSERT INTO Message (Sender_id, Reciever_id,timee,Datee,Content ) 
                                           VALUES (@SenderId, @RecieverId, @Timee, @Datee,@Content)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SenderId", SE_ID);
                        cmd.Parameters.AddWithValue("@RecieverId", RE_ID);
                        cmd.Parameters.AddWithValue("@Content", msg);
                        cmd.Parameters.AddWithValue("@Datee", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@Timee", DateTime.Now.TimeOfDay);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
            }
        }

        private void messagebar_Click(object sender, EventArgs e)
        {
            messagebar.Texts = "";
        }

        private void ChatContainer_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            string previousText = "ID USER";
            string previousText2 = "Subject";
            if (addbar.Texts != "" && subjectbar.Texts != "" && int.TryParse(addbar.Texts, out int number) && addbar.Texts != previousText && subjectbar.Texts != previousText2)
            {
                string ID = addbar.Texts;

                addbar.Texts = "";

                // Insert message into database
                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string insertQuery = @"INSERT INTO Chat (Sender_id, Reciever_id,Subject) 
                                           VALUES (@SenderId, @RecieverId,@Subject)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {

                        cmd.Parameters.AddWithValue("@SenderId", SE_ID);
                        cmd.Parameters.AddWithValue("@RecieverId", Convert.ToInt32(ID));
                        cmd.Parameters.AddWithValue("@Subject", subjectbar.Texts);
                        cmd.ExecuteNonQuery();
                    }
                    string mirrorQuery = @"INSERT INTO Chat (Sender_id, Reciever_id, Subject) 
                       VALUES (@SenderId, @RecieverId, @Subject)";
                    using (SqlCommand cmdMirror = new SqlCommand(mirrorQuery, conn))
                    {
                        cmdMirror.Parameters.AddWithValue("@SenderId", Convert.ToInt32(ID));
                        cmdMirror.Parameters.AddWithValue("@RecieverId", SE_ID);
                        cmdMirror.Parameters.AddWithValue("@Subject", subjectbar.Texts);
                        cmdMirror.ExecuteNonQuery();
                    }
                    string selectQuery = "SELECT Userr.FName + ' ' + Userr.LName AS fullname FROM Userr WHERE UserId = @RecieverId";
                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                    {
                        selectCmd.Parameters.AddWithValue("@RecieverId", Convert.ToInt32(ID));

                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string name = reader["fullname"]?.ToString() ?? string.Empty;
                                loadChat(ChatContainer, name, Convert.ToInt32(ID));
                            }
                        }
                    }

                }

                Subject_temp = subjectbar.Texts;

            }
            else
            {
            }
        }

        private void addbar_Click(object sender, EventArgs e)
        {
            addbar.Texts = "";
        }

        private void subjectbar_Click(object sender, EventArgs e)
        {
            subjectbar.Texts = "";
        }

        private void addbar__TextChanged(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void grades_Click(object sender, EventArgs e)
        {
            handleicon2(Grades2);

            grades_admin.BringToFront();
            LoadGrades(SE_ID);
            icons.BringToFront();
        }

        private void Home_th_Click(object sender, EventArgs e)
        {
            handleicon2(Home_th2);
            LoadCourses_admin();
            courses_th.BringToFront();
            icons.BringToFront();
        }

        private void classwork_th_Click(object sender, EventArgs e)
        {
            handleicon2(classwork_th2);
            LoadClassWork_th(SE_ID);
            classworkadmin.BringToFront();
            icons.BringToFront();
        }

        private void chat_th_Click(object sender, EventArgs e)
        {
            handleicon2(chat_th2);
            ChatContainer_ad.Controls.Clear();
            chatadmin.BringToFront();
            loadChats_inpanel(SE_ID, ChatContainer_ad);

            icons.BringToFront();
        }

        private void exam_th_Click(object sender, EventArgs e)
        {
            handleicon2(exam_th2);
            LoadExams_th(SE_ID);
            examadmin.BringToFront();

            icons.BringToFront();
        }

        private void user_th_Click(object sender, EventArgs e)
        {
            user_th2.BringToFront();
        }

        private void user_th_MouseEnter(object sender, EventArgs e)
        {
            user_th2.BringToFront();
        }

        private void user_th2_MouseHover(object sender, EventArgs e)
        {
            if (!click_check2)
                user_th.BringToFront();
        }

        private void user_th2_Click(object sender, EventArgs e)
        {
            handleicon2(user_th2);
            admin_info.BringToFront();
            LoadAdminProfile();

            click_check2 = true;
            icons.BringToFront();
        }



        private void uploadbtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select an image";
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;

                image_preview.Image = Image.FromFile(filePath);
                image_preview.SizeMode = PictureBoxSizeMode.Zoom;
                image_preview.BackgroundImage = null;
            }
        }

        private void pictureBox13_Click(object sender, EventArgs e)
        {
            courses_th.BringToFront();
            icons.BringToFront();
        }

        private void add_coursebtn_Click(object sender, EventArgs e)
        {
            add_course.BringToFront();
        }

        private void add_course_sql_Click(object sender, EventArgs e)
        {
            string title = titlebar.Texts;
            string dec = dec_bar.Texts;
            string cate = categorybar.Texts;
            string year = year_bar.Texts;
            string priv = privacybar.SelectedItem?.ToString();
            string sems = semster_bar.Texts;
            string examid = string.IsNullOrWhiteSpace(examidbar.Texts) ? null : examidbar.Texts;
            string courseID = courseid_th.Texts;
            int prive = 0;
            if (privacybar.SelectedItem == "Private")
            {
                prive = 0;
            }
            else
            {
                prive = 1;
            }
            if (!string.IsNullOrWhiteSpace(title) &&
                    !string.IsNullOrWhiteSpace(cate) &&
                    !string.IsNullOrWhiteSpace(year) &&
                    !string.IsNullOrWhiteSpace(priv) &&
                    !string.IsNullOrWhiteSpace(sems) &&
                    image_preview.Image != null)
            {
                byte[] imageBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    image_preview.Image.Save(ms, image_preview.Image.RawFormat);
                    imageBytes = ms.ToArray();
                }

                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string insertQuery = @"INSERT INTO Course (CourseId,UserId,PassedExam_ID,Title,Description, Category,Studying_Year,Semester,is_hidden,Cover ) 
                                           VALUES (@courseid,@userid,@ExamId,@title,@Description, @Category, @Studying_Year, @Semester,@is_hidden,@Cover)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseid", Convert.ToInt32(courseID));
                        cmd.Parameters.AddWithValue("@examid", string.IsNullOrWhiteSpace(examid) ? DBNull.Value : (object)examid);
                        cmd.Parameters.AddWithValue("@userid", SE_ID);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Description", dec);
                        cmd.Parameters.AddWithValue("@Category", cate);
                        cmd.Parameters.AddWithValue("@Studying_Year", Convert.ToInt32(year));
                        cmd.Parameters.AddWithValue("@Semester", Convert.ToInt32(sems));
                        cmd.Parameters.AddWithValue("@is_hidden", prive); // Assuming this is "true"/"false" or "Yes"/"No"
                        cmd.Parameters.AddWithValue("@Cover", imageBytes);

                        cmd.ExecuteNonQuery();
                        string selectQuery = @"SELECT CourseId,Cover
                      FROM Course c 
                      WHERE c.UserId = @SenderId AND c.Title = @title";
                        using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                        {
                            selectCmd.Parameters.AddWithValue("@SenderId", SE_ID);
                            selectCmd.Parameters.AddWithValue("@title", title);

                            // Replace this block inside the `add_course_sql_Click` method
                            using (SqlDataReader reader = selectCmd.ExecuteReader())
                            {
                                Image image = null;

                                while (reader.Read())
                                {
                                    int courseId = reader["CourseId"] != DBNull.Value ? Convert.ToInt32(reader["CourseId"]) : 0;
                                    // Convert the Cover column (byte[]) to an Image
                                    if (reader["Cover"] != DBNull.Value)
                                    {
                                        byte[] coverImageBytes = (byte[])reader["Cover"]; // Renamed variable to avoid conflict
                                        using (MemoryStream ms = new MemoryStream(coverImageBytes))
                                        {
                                            image = Image.FromStream(ms);
                                        }
                                    }


                                }
                            }

                        }

                    }


                }
                MessageBox.Show("The Course Added Successfully");
                LoadCourses_admin();
                courses_th.BringToFront();
                icons.BringToFront();
            }
            else
            {
            }
        }

        private void rjButton5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.Title = "Select an image";
            openFileDialog2.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog2.FileName;

                imagepreview_ed.Image = Image.FromFile(filePath);
                imagepreview_ed.SizeMode = PictureBoxSizeMode.Zoom;
                imagepreview_ed.BackgroundImage = null;
            }
        }

        private void rjButton4_Click(object sender, EventArgs e)
        {

            // Extract values from edit controls
            string title = titlebar_ed.Texts;
            string description = descbar_ed.Texts;
            string category = categorybar_ed.Texts;
            string year = yearbar_ed.Texts;
            string semester = semsterbar_ed.Texts;
            string examIdText = exambar_ed.Texts;
            string courseIdText = coursebar_ed.Texts;
            string privacy = privebar_ed.SelectedItem?.ToString();
            Image coverImage = imagepreview_ed.Image;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(year) ||
                string.IsNullOrWhiteSpace(semester) || string.IsNullOrWhiteSpace(examIdText) ||
                string.IsNullOrWhiteSpace(courseIdText) || string.IsNullOrWhiteSpace(privacy) ||
                coverImage == null)
            {
                MessageBox.Show("Please fill all fields and select a cover image.");
                return;
            }

            int courseId = Convert.ToInt32(courseIdText);
            int examId = Convert.ToInt32(examIdText);
            int isHidden = (privacy == "Private") ? 0 : 1;
            int originalCourseId = courseId;
            // Convert image to byte[]
            byte[] imageBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                coverImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = ms.ToArray();
            }

            // Update in database
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string updateQuery = @"
                    UPDATE Course
                    SET 
                        PassedExam_ID = @ExamId,
                        Title = @Title,
                        Description = @Description,
                        Category = @Category,
                        Studying_Year = @Year,
                        Semester = @Semester,
                        is_hidden = @IsHidden,
                        Cover = @Cover
                    WHERE CourseId = @CourseId";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", examId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);
                    cmd.Parameters.AddWithValue("@IsHidden", isHidden);
                    cmd.Parameters.AddWithValue("@Cover", imageBytes);
                    cmd.Parameters.AddWithValue("@CourseId", courseId);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Course updated successfully.");

            courses_th.BringToFront();
            icons.BringToFront();
            LoadCourses_admin();

        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            courses_th.BringToFront();
            icons.BringToFront();
            LoadCourses_admin();
        }
        private byte[] videoBytes;
        private void rjButton7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog3 = new OpenFileDialog();
            openFileDialog3.Title = "Select an image";
            openFileDialog3.Filter = "Image Files|*.MP4;";

            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog3.FileName;

                // Load the selected video file into byte array
                videoBytes = File.ReadAllBytes(filePath);
                mediaview.Image = Image.FromStream(new MemoryStream(Properties.Resources.image123));
                mediaview.SizeMode = PictureBoxSizeMode.Zoom;
                mediaview.BackgroundImage = null;
            }
        }

        private void addlesson_btn_Click(object sender, EventArgs e)
        {

            // Extract values from edit controls
            string title = titlelesson.Texts;
            string date = datelesson.Texts;
            string content = contentlesson.Texts;


            // Validate inputs
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content) ||
                videoBytes == null)
            {
                MessageBox.Show("Please fill all fields and select a media.");
                return;
            }

            // Update in database
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string insertQuery = @"
                    INSERT INTO Lesson (CourseId,Title, Date, Content,media)
                    VALUES (@courseid,@Title, @Date, @Content,@media)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Parse(date));
                    cmd.Parameters.AddWithValue("@Content", content);
                    cmd.Parameters.AddWithValue("@courseid", course_lesson_ID);
                    cmd.Parameters.AddWithValue("@Media", videoBytes);
                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Lesson Added successfully.");

            courses_th.BringToFront();
            icons.BringToFront();
            LoadCourses_admin();
        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {
            courses_th.BringToFront();
            icons.BringToFront();
            LoadCourses_admin();
        }

        private void rjButton6_Click(object sender, EventArgs e)
        {
            // Extract values from form inputs
            string title = title_edex.Texts;
            DateTime selectedDate = dateTimePicker3.Value;
            string duration = dur_edex.Texts;
            string description = desc_edex.Texts;
            string question = ques_edex.Texts;

            // Validate the inputs (optional but recommended)
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(duration) || string.IsNullOrWhiteSpace(question))
            {
                MessageBox.Show("Please fill in all the fields.");
                return;
            }
            if (selectedDate <= DateTime.Now)
            {
                MessageBox.Show("Please select a future date and time.");
                return;
            }
            // Make sure you have the examId set somewhere
            int examId;
            if (!int.TryParse(label44.Text, out examId))  // Assuming there's a textbox for exam ID
            {
                MessageBox.Show("Invalid Exam ID.");
                return;
            }

            // SQL connection and update command
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string updateQuery = @"
                UPDATE Exam
                SET Title = @Title,
                    Description = @Description,
                    Duration = @Duration,
                    Date = @Date,
                    Question = @Question
                WHERE ExamId = @ExamId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Duration", duration);
                    cmd.Parameters.AddWithValue("@Date", selectedDate);
                    cmd.Parameters.AddWithValue("@Question", question);
                    cmd.Parameters.AddWithValue("@ExamId", examId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Exam updated successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Update failed. Please check the Exam ID.");
                    }
                    LoadExams_th(SE_ID);
                    examadmin.BringToFront();
                    icons.BringToFront();
                }
            }
        }


        private void pictureBox17_Click(object sender, EventArgs e)
        {
            LoadExams_th(SE_ID);
            examadmin.BringToFront();
            icons.BringToFront();

        }

        private void rjButton8_Click(object sender, EventArgs e)
        {
            Examadd.BringToFront();
        }

        private void pictureBox19_Click(object sender, EventArgs e)
        {
            LoadExams_th(SE_ID);
            examadmin.BringToFront();
            icons.BringToFront();
        }
        private void rjButton9_Click(object sender, EventArgs e)
        {
            // Collect input data from the form
            string title = title_edex2.Texts;
            DateTime selectedDate2 = dateTimePicker4.Value;
            string duration = dur_edex2.Texts;
            string description = desc_edex2.Texts;
            string question = ques_edex2.Texts;
            string exam_id = exam_edex.Texts;
            string course_id = courseid_edex2.Texts;

            // Input validation
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(duration) || string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(exam_id) ||
                string.IsNullOrWhiteSpace(course_id))
            {
                MessageBox.Show("Please fill in all fields before saving the exam.");
                return;
            }
            if (selectedDate2 <= DateTime.Now)
            {
                MessageBox.Show("Please select a future date and time.");
                return;
            }

            // Prepare database insert
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string insertExamQuery = @"
                INSERT INTO Exam (ExamId, Title, Description, Duration, Date, Question)
                VALUES (@ExamId, @Title, @Description, @Duration, @Date, @Question)";

            string updateCourseQuery = @"
                    UPDATE Course SET ExamId = @ExamId WHERE CourseId = @CourseId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Insert Exam
                    using (SqlCommand insertCmd = new SqlCommand(insertExamQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@ExamId", exam_id);
                        insertCmd.Parameters.AddWithValue("@Title", title);
                        insertCmd.Parameters.AddWithValue("@Description", description);
                        insertCmd.Parameters.AddWithValue("@Duration", duration);
                        insertCmd.Parameters.AddWithValue("@Date", selectedDate2);
                        insertCmd.Parameters.AddWithValue("@Question", question);

                        insertCmd.ExecuteNonQuery();
                    }

                    // 2. Update Course with ExamId
                    using (SqlCommand updateCmd = new SqlCommand(updateCourseQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@ExamId", exam_id);
                        updateCmd.Parameters.AddWithValue("@CourseId", course_id);

                        updateCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Exam added and Course updated successfully.");
                    LoadExams_th(SE_ID);
                    examadmin.BringToFront();
                    icons.BringToFront();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void label61_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox20_Click(object sender, EventArgs e)
        {
            LoadClassWork_th(SE_ID);
            classworkadmin.BringToFront();
            icons.BringToFront();

        }

        private void rjButton11_Click(object sender, EventArgs e)
        {
            // Read the course ID used to identify the record
            if (!int.TryParse(label61.Text, out int EXID))
            {
                MessageBox.Show("Invalid Course ID.");
                return;
            }

            // Collect updated values from form
            string title = title_edcw.Texts;
            DateTime date = dateTimePicker2.Value;
            string duration = dur_edcw.Texts;
            string description = desc_edcw.Texts;

            if (date < DateTime.Today)
            {
                MessageBox.Show("Please select a current or future date.");
                return;
            }
            // Input validation
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(duration) || string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Please fill in all fields before saving the update.");
                return;
            }

            // Database connection and query
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string updateQuery = @"
                    UPDATE ClassWork 
                    SET Title = @Title, 
                        Description = @Description, 
                        Duration = @Duration, 
                        Date = @Date 
                    WHERE ExId = @exid";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@Duration", duration);
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@exid", EXID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("ClassWork updated successfully.");
                        }
                        else
                        {
                            MessageBox.Show("No ClassWork record was found to update.");
                        }
                    }
                }
                LoadClassWork_th(SE_ID);
                classworkadmin.BringToFront();
                icons.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating: " + ex.Message);
            }
        }

        private void rjButton12_Click(object sender, EventArgs e)
        {
            // Get values from form inputs
            string title = titlebaradd.Texts;
            DateTime date = dateTimePicker1.Value;
            string duration = durbaradd.Texts;
            string description = descbaradd.Texts;

            // Parse CourseId
            if (!int.TryParse(coursecw_id.Texts, out int courseId))
            {
                MessageBox.Show("Invalid Course ID.");
                return;
            }

            // Parse ExamId
            if (!int.TryParse(examcw_id.Texts, out int examId))
            {
                MessageBox.Show("Invalid Exam ID.");
                return;
            }


            // Validate date
            if (date < DateTime.Today)
            {
                MessageBox.Show("Please select a current or future date.");
                return;
            }

            // Validate text fields
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(duration) ||
                string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Please fill in all fields before saving the classwork.");
                return;
            }

            // SQL connection and insert
            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
            string insertQuery = @"
        INSERT INTO ClassWork (CourseId, ExId, Title, Description, Duration, Date)
        VALUES (@CourseId, @ExamId, @Title, @Description, @Duration, @Date)";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@ExamId", examId);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@Duration", duration);
                        cmd.Parameters.AddWithValue("@Date", date);

                        int rowsInserted = cmd.ExecuteNonQuery();
                        if (rowsInserted > 0)
                        {
                            MessageBox.Show("ClassWork added successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Failed to add ClassWork.");
                        }
                    }
                }

                // Refresh UI
                LoadClassWork_th(SE_ID);
                classworkadmin.BringToFront();
                icons.BringToFront();
            }
            catch (SqlException ex)
            {
                // Show detailed SQL errors like foreign key violations
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while inserting ClassWork: " + ex.Message);
            }
        }

        private void pictureBox21_Click(object sender, EventArgs e)
        {
            LoadClassWork_th(SE_ID);
            classworkadmin.BringToFront();
            icons.BringToFront();
        }

        private void rjButton10_Click(object sender, EventArgs e)
        {
            Addclasswork.BringToFront();
        }

        private void btnbar_ad_Click(object sender, EventArgs e)
        {

            if (msgbar_ad.Texts != "")
            {
                string msg = msgbar_ad.Texts;
                loadsender_msg(panel14, msg);
                msgbar_ad.Texts = "";

                // Insert message into database
                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string insertQuery = @"INSERT INTO Message (Sender_id, Reciever_id,timee,Datee,Content) 
                                           VALUES (@SenderId, @RecieverId, @Timee, @Datee,@Content)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {

                        cmd.Parameters.AddWithValue("@SenderId", SE_ID);
                        cmd.Parameters.AddWithValue("@RecieverId", RE_ID);
                        cmd.Parameters.AddWithValue("@Content", msg);
                        cmd.Parameters.AddWithValue("@Datee", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@Timee", DateTime.Now.TimeOfDay);
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            else
            {
            }
        }
        private void rjButton13_Click(object sender, EventArgs e)
        {
            string previousText = "ID USER";
            string previousText2 = "Subject";
            if (userid_ad.Texts != "" && subjectbar_ad.Texts != "" && int.TryParse(userid_ad.Texts, out int number) && userid_ad.Texts != previousText && subjectbar_ad.Texts != previousText2)
            {
                string ID = userid_ad.Texts;

                userid_ad.Texts = "";

                // Insert message into database
                string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string insertQuery = @"INSERT INTO Chat (Sender_id, Reciever_id,Subject) 
                                           VALUES (@SenderId, @RecieverId,@Subject)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {

                        cmd.Parameters.AddWithValue("@SenderId", SE_ID);
                        cmd.Parameters.AddWithValue("@RecieverId", Convert.ToInt32(ID));
                        cmd.Parameters.AddWithValue("@Subject", subjectbar_ad.Texts);
                        cmd.ExecuteNonQuery();
                    }
                    string mirrorQuery = @"INSERT INTO Chat (Sender_id, Reciever_id, Subject) 
                       VALUES (@SenderId, @RecieverId, @Subject)";
                    using (SqlCommand cmdMirror = new SqlCommand(mirrorQuery, conn))
                    {
                        cmdMirror.Parameters.AddWithValue("@SenderId", Convert.ToInt32(ID)); // Receiver becomes sender
                        cmdMirror.Parameters.AddWithValue("@RecieverId", SE_ID);             // Sender becomes receiver
                        cmdMirror.Parameters.AddWithValue("@Subject", subjectbar_ad.Texts);
                        cmdMirror.ExecuteNonQuery();
                    }
                    string selectQuery = "SELECT Userr.FName + ' ' + Userr.LName AS fullname FROM Userr WHERE UserId = @RecieverId";
                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                    {
                        selectCmd.Parameters.AddWithValue("@RecieverId", Convert.ToInt32(ID));

                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string name = reader["fullname"]?.ToString() ?? string.Empty;
                                loadChat(ChatContainer_ad, name, Convert.ToInt32(ID));
                            }
                        }
                    }

                }

                Subject_temp2 = subjectbar_ad.Texts;

            }
            else
            {
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label71_Click(object sender, EventArgs e)
        {

        }

        private void savegradesbtn_Click(object sender, EventArgs e)
        {

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue; // Skip blank row

                        int userId = Convert.ToInt32(row.Cells["StudentID"].Value);
                        int courseId = Convert.ToInt32(row.Cells["CourseID"].Value);
                        string grade = row.Cells["Grade"].Value?.ToString();

                        string updateQuery = @"
                        UPDATE Register
                        SET Grade = @Grade
                        WHERE UserId = @UserId AND CourseId = @CourseId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Grade", grade);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@CourseId", courseId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Grades saved successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while saving grades: " + ex.Message);
            }


        }

        private void label3_Click(object sender, EventArgs e)
        {
            signup.BringToFront();
            emaillogin.Texts = "";
            passwordlogin.Texts = "";
        }

        private void label6_Click(object sender, EventArgs e)
        {
            login.BringToFront();

        }

        private void signupbutton_Click(object sender, EventArgs e)
        {
            string Fname = Fnamesignup.Texts.Trim();
            string Lname = Lnamesignup.Texts.Trim();
            string Email = emailsignup.Texts.Trim();
            string Password = passwordsignup.Texts;
            string Cpassword = Cpasssignup.Texts;
            string user_id = useridsignup.Texts.Trim();
            string type = typesignup.SelectedItem?.ToString();

            // Basic validation
            if (string.IsNullOrWhiteSpace(Fname) || string.IsNullOrWhiteSpace(Lname) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Cpassword) || string.IsNullOrWhiteSpace(user_id) ||
                string.IsNullOrWhiteSpace(type))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Password confirmation
            if (Password != Cpassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            if (!int.TryParse(user_id, out int userId))
            {
                MessageBox.Show("User ID must be a number.");
                return;
            }

            string connectionString = "Server=LAPTOP-I23IVTH3;Database=course_system;Trusted_Connection=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if UserId already exists
                    string checkQuery = "SELECT COUNT(*) FROM Userr WHERE UserId = @UserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("This User ID already exists. Please enter a different one.");
                            return;
                        }
                    }

                    // Insert new user
                    string insertQuery = @"
                INSERT INTO Userr (UserId, Fname, Lname, User_Role, Pass_word, Email)
                VALUES (@UserId, @Fname, @Lname, @User_Role, @Pass_word, @Email)";

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@UserId", userId);
                        insertCmd.Parameters.AddWithValue("@Fname", Fname);
                        insertCmd.Parameters.AddWithValue("@Lname", Lname);
                        insertCmd.Parameters.AddWithValue("@User_Role", type);
                        insertCmd.Parameters.AddWithValue("@Pass_word", Password);  // You may hash this in production
                        insertCmd.Parameters.AddWithValue("@Email", Email);

                        int rowsInserted = insertCmd.ExecuteNonQuery();
                        if (rowsInserted > 0)
                        {
                            MessageBox.Show("Account created successfully.");
                            // Optionally clear the form here
                        }
                        else
                        {
                            MessageBox.Show("Failed to create account.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            login.BringToFront();
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            handleicon2(Home_th2);
            home.BringToFront();
            courses_th.BringToFront();
            handleicon(homeicom2);
            login.BringToFront();
            SE_ID = 0;
            NameID = "";
            RoleID = "";
        }

        private void qbouns_Click(object sender, EventArgs e)
        {
            handleicon2(qbouns2);
            qbouns_pa.BringToFront();
            icons.BringToFront();

        }
        private void btnShowReport_Click(object sender, EventArgs e)
        {
            // Get the button that was clicked
            Button clickedButton = sender as Button;
            if (clickedButton == null)
                return;

            string selectedKey = clickedButton.Text;

            if (!reports.ContainsKey(selectedKey))
            {
                MessageBox.Show("No report found for: " + selectedKey, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (question, query) = reports[selectedKey];

            // Fetch data only
            string connStr = "Server=LAPTOP-I23IVTH3;Database=Course_system;Trusted_Connection=True;";
            SqlDataAdapter adapter = new SqlDataAdapter(query, connStr);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Export to PDF only (no UI output)
            SaveReportToPdf(selectedKey, question, dt);
        }
        private void SaveReportToPdf(string reportTitle, string queryText, DataTable data)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(folderPath, reportTitle.Replace(" ", "_") + "_Report.pdf");

            var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                // Title
                var titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 16);
                doc.Add(new iTextSharp.text.Paragraph(reportTitle, titleFont));
                doc.Add(new iTextSharp.text.Paragraph(" ")); // Line break

                // Query Text
                var queryFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10, iTextSharp.text.BaseColor.GRAY);
                doc.Add(new iTextSharp.text.Paragraph("Query: " + queryText, queryFont));
                doc.Add(new iTextSharp.text.Paragraph(" "));

                // Data table
                PdfPTable pdfTable = new PdfPTable(data.Columns.Count);

                // Header
                foreach (DataColumn column in data.Columns)
                {
                    pdfTable.AddCell(new iTextSharp.text.Phrase(column.ColumnName, iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 10)));
                }

                // Rows
                foreach (DataRow row in data.Rows)
                {
                    foreach (var cell in row.ItemArray)
                    {
                        pdfTable.AddCell(cell?.ToString() ?? "");
                    }
                }

                doc.Add(pdfTable);
                doc.Close();
                stream.Close();
            }

            MessageBox.Show("Report saved to Desktop as PDF!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
