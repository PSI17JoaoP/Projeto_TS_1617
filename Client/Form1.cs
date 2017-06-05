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

        private ServiceCriptoAssimetrica servicoAssinaturas;

        private ServiceCriptoSimetrica servicoCriptoSimetrico;

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

                //é equivalente à inicialização de objetos e preparação do cliente.
                tcpClient = new TcpClient();
                endPoint = new IPEndPoint(IPAddress.Loopback, port);

                tcpClient.Connect(endPoint);
                networkStream = tcpClient.GetStream();

                protocolSI = new ProtocolSI();

                servicoCriptoSimetrico = new ServiceCriptoSimetrica();

                string publickey = null;
                byte[] secretkey = null;
                byte[] iv = null;

                ProtocolSICmdType protocolTipoResposta;

                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //lê para o buffer a key, enviada anteriormente pelo servidor.

                if (protocolSI.GetCmdType() == ProtocolSICmdType.PUBLIC_KEY)
                {
                    //serve para determinar e verificar se o buffer é do tipo public key, se for executa.
                    publickey = protocolSI.GetStringFromData();
                    //obtêm a string do buffer, este sendo um array de bytes.
                    //Receção e envio da public key, do cliente para o servidor e receber do servidor
                    Debug.Print("Received Public Key");

                    servicoAssinaturas = new ServiceCriptoAssimetrica(publickey);
                    //Vai instanciar um servico de criptografia assimetrica, com a public key do servidor.
                    //O construtor desta classe necessita da public key, por isso é que só foi inicializado agora.
  
                    secretkey = servicoCriptoSimetrico.ObterSecretKey();
                    byte[] secretkeyEncriptada = protocolSI.Make(ProtocolSICmdType.SECRET_KEY, servicoAssinaturas.EncriptarDados(secretkey));
                    networkStream.Write(secretkeyEncriptada, 0, secretkeyEncriptada.Length);
                    Debug.Print("Secret Key Sent");

                    do
                    {
                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                        protocolTipoResposta = protocolSI.GetCmdType();

                        if (protocolTipoResposta == ProtocolSICmdType.ACK)
                        {
                            iv = servicoCriptoSimetrico.ObterIV();
                            byte[] ivEncriptado = protocolSI.Make(ProtocolSICmdType.IV, servicoAssinaturas.EncriptarDados(iv));
                            networkStream.Write(ivEncriptado, 0, ivEncriptado.Length);
                            Debug.Print("IV Sent");
                        }
                    }
                    while (protocolTipoResposta != ProtocolSICmdType.ACK);

                    //encriptar a chave secreta com a publica  envia la ao mesmo tempo
                    //como o do outro lado.
                }
                else
                {
                    //caso não seja do tipo public key envia uma mensagem de erro.
                    MessageBox.Show("Erro ao receber a chave pública", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    // transforma num array de bytes, recorrendo ao protocol, envia o marcador e o nome de utilizador, o mesmo para pass
                    byte[] usernameLogin = Encoding.UTF8.GetBytes(txtUtilizador.Text.Trim());
                    byte[] passwordHashLogin = Encoding.UTF8.GetBytes(txtPassword.Text);
                    //saca os bytes do texto.

                    byte[] serverFeedback = null;

                    //utilizar encriptação assimetrica;

                    //Escreve na network stream (forma de comunicarem, imposta pela comunicação, tcp, é onde se trata de enviar dados receber dados.

                    //vai para a parte do program, e faz um read() no lado do servidor.

                    byte[] usernameEncriptado = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptoSimetrico.EncryptDados(usernameLogin, usernameLogin.Length));
                    byte[] passwordHashEncriptada = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptoSimetrico.EncryptDados(passwordHashLogin, passwordHashLogin.Length));

                    networkStream.Write(usernameEncriptado, 0, usernameEncriptado.Length);

                    //faz exatamente a mesma parte de cima e vai servidor. (((Utilizar a Hash)))
                    networkStream.Write(passwordHashEncriptada, 0, passwordHashEncriptada.Length);

                    //fica à escuta para receber a mensagem de sucesso.
                    //ack --> sao acknoledgments, quando recebe alguma coisa tem de dizer que recebeu, é o feedback
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    //vai por a msg no buffer, que veio do lado do servidor.
                    serverFeedback = protocolSI.GetData();
                    //tranforma em string a resposta

                    byte[] serverFeedbackBytes = servicoCriptoSimetrico.DecryptDados(serverFeedback);
                    string serverFeedbackString = Encoding.UTF8.GetString(serverFeedbackBytes);

                    if (serverFeedbackString == "SUCCESSFUL")
                    {
                        gbMenu.Enabled = true;
                        gbMenu.Visible = true;
                        gbLogin.Enabled = false;
                        //ativa os formulários para uso.
                    }

                    else if (serverFeedbackString == "FAILED")
                    {
                        //caso o login não seja bem sucedido, envia uma mensagem de erro.
                        MessageBox.Show("O username e password introduzidos são incorretos", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                else
                {
                    MessageBox.Show("Preencha todos os campos para efeutuar login", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            //caso tenha alguma excepção.
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
                byte[] requestListBytes = Encoding.UTF8.GetBytes("GETLIST");
                byte[] requestList = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptoSimetrico.EncryptDados(requestListBytes, requestListBytes.Length));
                networkStream.Write(requestList, 0, requestList.Length);

                byte[] fileList = null;
                string fileListDecriptadaString = null;
                byte[] fileListDecriptadaBytes = null;

                //--------------------------

                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //recebe num buffer as assinaturas
                byte[] assinaturabuffer = protocolSI.GetData();

                //obtem a assinatura num array de bytes.

                //--------------------------
                
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //recebe num buffer, a lista de ficheiros
                fileList = protocolSI.GetData();
                //coloca no array de byte a lista de ficheiros.

                fileListDecriptadaBytes = servicoCriptoSimetrico.DecryptDados(fileList);

                fileListDecriptadaString = Encoding.UTF8.GetString(fileListDecriptadaBytes);

                //envia os dados e vai verificar a assinaturas dos dados
                if (servicoAssinaturas.VerAssinaturaDados(fileListDecriptadaBytes, assinaturabuffer))
                {
                    //se os dados não foram alterados, mostra os ficheiros da lista.
                    RefreshFileList(fileListDecriptadaBytes, fileListDecriptadaBytes.Length);
                }
                else
                {
                    //se são diferentes mostra que os dados foram corrompidos.
                    MessageBox.Show("Dados corrompidos. Descartados", "Erro");
                }  
            }

            catch (Exception)
            {
                throw;
                //StopConnection();
            }
        }

        private void RefreshFileList(byte[] fileList, int fileNameListSize)
        {
            ListViewItem item;
            string fileName = null;
            int indexStart = 0;
            int indexEnd = 0;
            string fileNameList = Encoding.UTF8.GetString(fileList);

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
                //vai buscar o nome do ficheiro que quer obter e para mandar para o servidor para pedir a imagem.
                string fileRequest = lvLista.SelectedItems[0].Text;

                //Transforma a string num array de bytes, do tipo dado, e a ação a executar.
                byte[] requestListBytes = Encoding.UTF8.GetBytes("GETFILE");
                byte[] requestList = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptoSimetrico.EncryptDados(requestListBytes, requestListBytes.Length));
                //escreve na network stream para o servidor.
                networkStream.Write(requestList, 0, requestList.Length);

                //vai buscar um buffer do tipo data, com o pedido do ficheiro
                byte[] fileRequestedBytes = Encoding.UTF8.GetBytes(fileRequest);
                byte[] fileRequested = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptoSimetrico.EncryptDados(fileRequestedBytes, fileRequestedBytes.Length));
                //escreve na networkstream o buffer com o nome do ficheiro.
                networkStream.Write(fileRequested, 0, fileRequested.Length);

                if (File.Exists(fileRequest))
                {
                    File.Delete(fileRequest);
                }

                //NOVO
                
                byte[] assinaturaBytes = null;
                byte[] finalImageHash = null;

                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                assinaturaBytes = protocolSI.GetData();
                
                //----------


                using (FileStream fileStream = new FileStream(fileRequest, FileMode.CreateNew, FileAccess.ReadWrite))
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
                            //Encriptado
                            imageBuffer = protocolSI.GetData();
                            byte[] imageBufferDecriptadoBytes = servicoCriptoSimetrico.DecryptDados(imageBuffer);
                            string imageBufferDecriptadoString = Encoding.UTF8.GetString(imageBufferDecriptadoBytes);
                            fileStream.Write(imageBufferDecriptadoBytes, 0, imageBufferDecriptadoBytes.Length);
                            Debug.Print("Received " + imageBufferDecriptadoBytes.Length + " + " + (bytesRead - imageBufferDecriptadoBytes.Length) + " Bytes");

                            acknowledge = protocolSI.Make(ProtocolSICmdType.ACK);
                            //byte[] acknowledgeEncriptado = servicoCriptoSimetrico.EncryptDados(acknowledge, acknowledge.Length);
                            networkStream.Write(acknowledge, 0, acknowledge.Length);
                            Debug.Print("Acknowlegment (ACK) Sent");
                        }

                        else if (protocolTipoResposta == ProtocolSICmdType.EOF)
                        {
                            Debug.Print("Reached End of File (EOF)");
                        }
                    }
                    while (protocolTipoResposta != ProtocolSICmdType.EOF);
                    
                    // NOVO
                    
                    byte[] file = new byte[fileStream.Length];
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.Read(file, 0, file.Length);
                    finalImageHash = servicoAssinaturas.HashImagem(file);
                    
                    //----------
                }

                //NOVO

                if (servicoAssinaturas.VerAssinaturaHash(finalImageHash, assinaturaBytes))
                {
                    lvLista.SelectedItems[0].SubItems[1].Text = "Sim";
                    btnAbrirFicheiro.Enabled = true;
                }

                else
                {
                    MessageBox.Show("Dados corrompidos. Descartados", "Erro");

                    if(File.Exists(fileRequest))
                    {
                        File.Delete(fileRequest);
                    }
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
            byte[] mensagemSaida = Encoding.UTF8.GetBytes("SHUTDOWN");
            byte[] lastMessage = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptoSimetrico.EncryptDados(mensagemSaida, mensagemSaida.Length));
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
