using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto
{
    public partial class Form1 : Form
    {
        private const int port = 9090;

        private TcpClient tcpClient = null;
        private NetworkStream networkStream = null;
        private IPEndPoint endPoint = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tcpClient = new TcpClient();
            endPoint = new IPEndPoint(IPAddress.Loopback, port);

            tcpClient.Connect(endPoint);
            networkStream = tcpClient.GetStream();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                int bytesRead;
                byte[] usernameLogin = Encoding.UTF8.GetBytes(txtUtilizador.Text.Trim());
                byte[] passwordHashLogin = Encoding.UTF8.GetBytes(txtPassword.Text);
                string serverFeedback;

                networkStream.Write(usernameLogin, 0, usernameLogin.Length);

                networkStream.Write(passwordHashLogin, 0, passwordHashLogin.Length);

                int serverFeedbackBufferSize = tcpClient.ReceiveBufferSize;
                byte[] serverFeedbackBuffer = new byte[serverFeedbackBufferSize];

                bytesRead = networkStream.Read(serverFeedbackBuffer, 0, serverFeedbackBufferSize);
                serverFeedback = Encoding.UTF8.GetString(serverFeedbackBuffer, 0, bytesRead);

                if (serverFeedback == "SUCCESSFUL")
                {
                    gbMenu.Enabled = true;
                    gbMenu.Visible = true;
                }

                else if (serverFeedback == "FAILED")
                {
                    MessageBox.Show("O username e password introduzidos são incorretos", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            catch
            {
                StopConnection();
            }
        }

        private void btnObterLista_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] requestList = Encoding.UTF8.GetBytes("GETLIST");
                networkStream.Write(requestList, 0, requestList.Length);

                int fileListBufferSize;
                byte[] fileListBuffer;
                string fileList;

                int bytesRead;

                do
                {
                    fileListBufferSize = tcpClient.ReceiveBufferSize;
                    fileListBuffer = new byte[fileListBufferSize];

                    bytesRead = networkStream.Read(fileListBuffer, 0, fileListBufferSize);
                    fileList = Encoding.UTF8.GetString(fileListBuffer, 0, bytesRead);
                    AddFilesToList(fileList, bytesRead);
                }
                while (networkStream.DataAvailable);

            }

            catch
            {
                StopConnection();
            }
        }

        private void AddFilesToList(string fileNameList, int fileListSize)
        {
            string fileName;
            int indexStart = 0;
            int indexEnd = 0;

            do
            {
                indexEnd = fileNameList.IndexOf(";", indexStart);
                fileName = fileNameList.Substring(indexStart, indexEnd - indexStart);

                indexStart = indexEnd + 1;

                lvLista.Items.Add(fileName);
            }
            while (indexStart != fileListSize);

        }

        private void btnObterFicheiro_Click(object sender, EventArgs e)
        {
            string fileRequest = lvLista.SelectedItems[0].Text;

            byte[] requestList = Encoding.UTF8.GetBytes("GETFILE");
            networkStream.Write(requestList, 0, requestList.Length);

            byte[] fileRequested = Encoding.UTF8.GetBytes(fileRequest);
            networkStream.Write(fileRequested, 0, fileRequested.Length);

            if (File.Exists(fileRequest))
            {
                File.Delete(fileRequest);
            }

            using (FileStream fileStream = new FileStream(fileRequest, FileMode.CreateNew))
            {
                byte[] fileBuffer;
                int fileBufferSize;

                int bytesRead;

                do
                {
                    fileBufferSize = tcpClient.ReceiveBufferSize;
                    fileBuffer = new byte[fileBufferSize];

                    bytesRead = networkStream.Read(fileBuffer, 0, fileBufferSize);
                    fileStream.Write(fileBuffer, 0, bytesRead);
                }
                while (networkStream.DataAvailable);
            }
        }

        private void btnAbrirFicheiro_Click(object sender, EventArgs e)
        {
            string file = lvLista.SelectedItems[0].Text;

            if (File.Exists(file))
            {
                Process.Start(file);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopConnection();
        }

        private void StopConnection()
        {
            if (networkStream != null)
            {
                networkStream.Close();
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
            }
        }
    }
}
