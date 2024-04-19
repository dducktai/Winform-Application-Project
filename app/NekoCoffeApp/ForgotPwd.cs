﻿using FireSharp.Config;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using FireSharp.Response;

namespace UI
{
    public partial class ForgotPwd : Form
    {
    
        public ForgotPwd()
        {
            InitializeComponent();

        }
        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "f5A5LselW6L4lKJHpNGVH6NZHGKIZilErMoUOoLC",
            BasePath = "https://neko-coffe-database-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;
        static string GenerateVerificationCode()
        {
            // Tạo một đối tượng Random để tạo mã ngẫu nhiên
            Random random = new Random();

            // Tạo mã code gồm 6 số ngẫu nhiên
            int code = random.Next(100000, 999999);

            return code.ToString();
        }
        private void SendEmail(string recipientEmail, string verificationCode)
        {
            try
            {
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress("Neko Coffe", "nekocoffe.app@gmail.com"));
                email.To.Add(new MailboxAddress("Client", "laiquanthien15@gmail.com"));

                email.Subject = "[Neko Coffe] - Quên mật khẩu";

                // Tạo nội dung email dạng HTML
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
<p style=""color: black;"">Xin chào,</p>
<p style=""color: black;"">Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn trên Neko Coffe App. Để hoàn tất quá trình này, vui lòng làm nhập mã xác thực sau:</p>
<p style=""color: black;""><b>Mã xác thực của bạn là: {verificationCode}</b></p>
<p style=""color: black;"">Nếu bạn không yêu cầu đặt lại mật khẩu hoặc không nhớ đến yêu cầu này, vui lòng bỏ qua email này. Thông tin đăng nhập của bạn vẫn an toàn và không có hành động nào được thực hiện trừ khi bạn yêu cầu.</p>
<p style=""color: black;"">Nếu bạn cần thêm sự trợ giúp hoặc có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email này.</p>
<p style=""color: black;"">Trân trọng,<br>Neko Coffe Team.</p>

";

                // Gán nội dung email vào email object
                email.Body = bodyBuilder.ToMessageBody();

                // Sử dụng MailKit để gửi email
                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect("smtp.gmail.com", 587, false);

                    // Xác thực với tên người dùng và mật khẩu của tài khoản Gmail
                    smtp.Authenticate("nekocoffe.app@gmail.com", "tffa kqbg luyj nlqy");

                    // Gửi email
                    smtp.Send(email);

                    // Ngắt kết nối sau khi gửi xong
                    smtp.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending email: " + ex.Message);
            }

        }
        private void ForgotPwd_Load(object sender, EventArgs e)
        {
            try
            {
                client = new FireSharp.FirebaseClient(ifc);
            }

            catch
            {
                MessageBox.Show("Kiểm tra lại mạng", "Cảnh báo!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void txtSendCode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtUserName.Text) ||
               string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin", "Cảnh báo!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FirebaseResponse res = client.Get(@"Users/" + txtUserName.Text);
            NekoUser ResUser = res.ResultAs<NekoUser>();


            NekoUser CurUser = new NekoUser()
            {
                Username = txtUserName.Text,
                Email = txtEmail.Text,
            };

            if (NekoUser.IsExist(ResUser, CurUser) == true)
            {
                string recipientEmail = txtEmail.Text; // Địa chỉ email của người nhận

                string verificationCode = GenerateVerificationCode(); // Tạo mã code ngẫu nhiên
                ChangePwd changepwd = new ChangePwd(verificationCode, txtUserName.Text);


                // Gửi mã code đến email của người nhận
                SendEmail(recipientEmail, verificationCode);
                MessageBox.Show("Vui lòng kiểm tra hộp thư", "Lưu ý!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                changepwd.ShowDialog();
                this.ShowDialog();

            }
            else
            {
                NekoUser.ShowError();
            }
        }
    }
}