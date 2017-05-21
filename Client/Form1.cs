using EI.SI;
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

        //--------------

        private ServiceAssinaturasDigitais servicoAssinaturas;

        //--------------

        private ProtocolSI protocolSI;

        //--------------

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                tcpClient = new TcpClient();
                endPoint = new IPEndPoint(IPAddress.Loopback, port);

                tcpClient.Connect(endPoint);
                networkStream = tcpClient.GetStream();

                protocolSI = new ProtocolSI();
            }

            catch (Exception)
            {
                StopConnection();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtUtilizador.Text.Length > 0 && txtPassword.Text.Length > 0)
                {
                    byte[] usernameLogin = protocolSI.Make(ProtocolSICmdType.DATA, txtUtilizador.Text.Trim());
                    byte[] passwordHashLogin = protocolSI.Make(ProtocolSICmdType.DATA, txtPassword.Text);

                    string serverFeedback = null;

                    networkStream.Write(usernameLogin, 0, usernameLogin.Length);

                    networkStream.Write(passwordHashLogin, 0, passwordHashLogin.Length);

                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    serverFeedback = protocolSI.GetStringFromData();


                    if (serverFeedback == "SUCCESSFUL")
                    {
                        //------------------
                        string key = null;

                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                        if (protocolSI.GetCmdType() == ProtocolSICmdType.PUBLIC_KEY)
                        {
                            key = protocolSI.GetStringFromData();

                            servicoAssinaturas = new ServiceAssinaturasDigitais(key);
                        }

                        else
                        {
                            MessageBox.Show("Erro ao receber a chave pública", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        //------------------

                        gbMenu.Enabled = true;
                        gbMenu.Visible = true;
                        gbLogin.Enabled = false;
                    }

                    else if (serverFeedback == "FAILED")
                    {
                        MessageBox.Show("O username e password introduzidos são incorretos", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                else
                {
                    MessageBox.Show("Preencha todos os campos para efeutuar login", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            catch (Exception)
            {
                //throw;
                StopConnection();
            }
        }

        private void btnObterLista_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] requestList = protocolSI.Make(ProtocolSICmdType.DATA, "GETLIST");
                networkStream.Write(requestList, 0, requestList.Length);

                string fileList = null;

                //--------------------------
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                byte[] assinaturabuffer = protocolSI.GetData();
                string assinatura = Convert.ToBase64String(assinaturabuffer, 0, assinaturabuffer.Length);
                //--------------------------
                
                
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                fileList = protocolSI.GetStringFromData();
                

                if (servicoAssinaturas.VerAssinaturaDados(fileList, assinatura))
                {
                    RefreshFileList(fileList, fileList.Length);
                }
                else
                {
                    MessageBox.Show("Dados corrompidos. Descartados", "Erro");
                }

                
            }

            catch (Exception)
            {
                //throw;
                StopConnection();
            }
        }

        private void RefreshFileList(string fileNameList, int fileNameListSize)
        {
            ListViewItem item;
            string fileName = null;
            int indexStart = 0;
            int indexEnd = 0;

            lvLista.Items.Clear();

            do
            {
                indexEnd = fileNameList.IndexOf(";", indexStart);
                fileName = fileNameList.Substring(indexStart, indexEnd - indexStart);

                indexStart = indexEnd + 1;

                item = new ListViewItem(fileName);
                item.SubItems.Add("Não");
                lvLista.Items.Add(item);
            }
            while (indexStart != fileNameListSize);

        }

        private void btnObterFicheiro_Click(object sender, EventArgs e)
        {
            try
            {
                string fileRequest = lvLista.SelectedItems[0].Text;

                byte[] requestList = protocolSI.Make(ProtocolSICmdType.DATA, "GETFILE");
                networkStream.Write(requestList, 0, requestList.Length);

                byte[] fileRequested = protocolSI.Make(ProtocolSICmdType.DATA, fileRequest);
                networkStream.Write(fileRequested, 0, fileRequested.Length);

                if (File.Exists(fileRequest))
                {
                    File.Delete(fileRequest);
                }

                //NOVO
                
                string assinatura = null;
                byte[] assinaturaBytes = null;
                byte[] imageHash = null;
                byte[] finalImageHash = null;

                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                assinaturaBytes = protocolSI.GetData();
                assinatura = Convert.ToBase64String(assinaturaBytes, 0, assinaturaBytes.Length);

                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                imageHash = protocolSI.GetData();
                
                //----------


                using (FileStream fileStream = new FileStream(fileRequest, FileMode.CreateNew, FileAccess.Write))
                {
                    ProtocolSICmdType protocolTipoResposta;
                    int bytesRead;

                    byte[] acknowledge;
                    byte[] imageBuffer;

                    do
                    {
                        bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        
                        protocolTipoResposta = protocolSI.GetCmdType();

                        if (protocolTipoResposta == ProtocolSICmdType.DATA)
                        {
                            imageBuffer = protocolSI.GetData();
                            fileStream.Write(imageBuffer, 0, imageBuffer.Length);
                            Debug.Print("Received " + imageBuffer.Length + " + " + (bytesRead - imageBuffer.Length) + " Bytes");

                            acknowledge = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(acknowledge, 0, acknowledge.Length);
                            Debug.Print("Acknowlegment (ACK) Sent");
                        }

                        else if (protocolTipoResposta == ProtocolSICmdType.EOF)
                        {
                            Debug.Print("Reached End of File (EOF)");
                        }
                    }
                    while (networkStream.DataAvailable);
                    
                    // NOVO
                    /*
                    byte[] file = new byte[fileStream.Length];
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.Read(file, 0, file.Length);
                    finalImageHash = servicoAssinaturas.HashImagem(file);
                    */
                    //----------
                }

                //NOVO
                if (servicoAssinaturas.VerAssinaturaHash(imageHash, assinatura))
                {
                    /*if (imageHash.SequenceEqual(finalImageHash))
                    {*/
                        lvLista.SelectedItems[0].SubItems[1].Text = "Sim";
                        btnAbrirFicheiro.Enabled = true;
                    /*}
                    else
                    {
                        MessageBox.Show("Dados corrompidos. Descartados", "Erro");
                    }*/
                }
                else
                {
                    MessageBox.Show("Dados corrompidos. Descartados", "Erro");
                }
                //----------
            }

            catch (Exception)
            {
                //StopConnection();
                throw;
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
            byte[] lastMessage = protocolSI.Make(ProtocolSICmdType.DATA, "SHUTDOWN");
            networkStream.Write(lastMessage, 0, lastMessage.Length);

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

        private void lvLista_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvLista.SelectedItems.Count > 0)
            {
                btnObterFicheiro.Enabled = true;

                if (lvLista.SelectedItems[0].SubItems[1].Text == "Sim")
                {
                    btnAbrirFicheiro.Enabled = true;
                }
                else
                {
                    btnAbrirFicheiro.Enabled = false;
                }
            }
            else
            {
                btnObterFicheiro.Enabled = false;
                btnAbrirFicheiro.Enabled = false;
            }
        }
    }
}
