using EI.SI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private const int Port = 9090;

        private const string userTemp = "admin";

        private const string passTemp = "admin";

        private static string fileListPath = Path.Combine(Environment.CurrentDirectory, @"Files\");

        private static TcpListener tcpListener = null;
        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;

        //-------------

        private static SqlConnection connect;

        //-------------

        private static ServiceCriptoAssimetrica servicoAssinaturas;

        private static ServiceCriptoSimetrica servicoCriptografiaSimetrica;

        //-------------

        private static ProtocolSI protocolSI;

        //------------

        static void Main(string[] args)
        {
            byte[] requestClient = null;
            byte[] requestClientBytes = null;
            string requestClienteDecriptada = null;

            protocolSI = new ProtocolSI();

            //inicia o servidor e os vários serviços.
            ServerStart();
            ServerLogin();

            //o programa está sempre neste ciclo.
            while (true)
            {
                //verifica se está ligado
                if (tcpClient.Connected)
                {
                    //se estiver ligado vai ler da networkStream, um buffer, com o request, ou seja o que o cliente deseja fazer.
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    //obtêm a string dos dados do buffer do request lido anteriormente
                    requestClient = protocolSI.GetData();

                    requestClientBytes = servicoCriptografiaSimetrica.DecryptDados(requestClient);

                    requestClienteDecriptada = Encoding.UTF8.GetString(requestClientBytes);

                    //Enviar mensagem com o que o cliente deseja fazer.
                    //Serve APENAS para escrever na consola do server. !=networkstream.write();
                    Console.WriteLine("Client Request ->" + requestClienteDecriptada);

                    //vai verificar se o request é para obter lista de ficheiros
                    if (requestClienteDecriptada == "GETLIST")
                    {
                        //se sim, executa o procedimento ServerSendList();
                        ServerSendList();
                    }

                    else if (requestClienteDecriptada == "GETFILE")
                    {
                        //se for getfile;
                        byte[] requestFile = null;
                        byte[] requestFileBytes = null;

                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        //Recebe do cliente, um buffer com o request, ou seja, o ficheiro pedido pelo cliente.
                        requestFile = protocolSI.GetData();

                        requestFileBytes = servicoCriptografiaSimetrica.DecryptDados(requestFile);

                        string requestFileDecriptado = Encoding.UTF8.GetString(requestFileBytes);
                        //obtem uma string com o nome do ficheiro, através do buffer.

                        Console.WriteLine("Sending File '" + requestFileDecriptado + "' ...");
                        //Escreve na consola a dizer que está a enviar o ficheiro pedido.
                        //Envia o ficheiro.
                        ServerSendFile(requestFileDecriptado);
                    }
                    else if (requestClienteDecriptada == "SHUTDOWN")
                    {
                        Console.WriteLine("Shutting Down ...");
                        //caso o request seja para fechar
                        StopServer();
                        //quebra o ciclo infinito, parando todos os serviços ativos.
                        break;
                    }
                }

                else
                {
                    //se não estiver conectado pára o servidor.
                    StopServer();
                    break;
                }
            }
        }

        private static void ServerStart()
        {
            try
            {
                //linhas de comandos o que vai aparecer
                Console.WriteLine("Starting Server ...");
                //ipadress -> ip do servidor vai comunicar através do Port, com tcp sockets.
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, Port);
                tcpListener = new TcpListener(endPoint);
                //Inicializa o listener que serve para escutar mensagens.
                Console.WriteLine("Server Ready!");
                
                //começa o processo de escuta, para estar a espera de pedidos de ligação ao servidor.
                tcpListener.Start();
                Console.WriteLine("Waiting for connections ...");

                //inicializa a assimetrica, cria public e private key.
                servicoAssinaturas = new ServiceCriptoAssimetrica();
            }

            catch (Exception)
            {
                StopServer();
            }
        }


        private static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            using (HashAlgorithm hashAlgorithm = SHA512.Create())
            {
                // Declarar e inicializar buffer para o texto e salt
                byte[] plainTextWithSaltBytes =
                              new byte[plainText.Length + salt.Length];

                // Copiar texto para buffer
                for (int i = 0; i < plainText.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainText[i];
                }
                // Copiar salt para buffer a seguir ao texto
                for (int i = 0; i < salt.Length; i++)
                {
                    plainTextWithSaltBytes[plainText.Length + i] = salt[i];
                }

                //Devolver hash do text + salt
                return hashAlgorithm.ComputeHash(plainTextWithSaltBytes);
            }
        }

        private static void ServerLogin()
        {

            byte[] usernameClientEncriptado = null;
            byte[] passwordHashEncriptada = null;
            byte[] usernameClientBytes = null;
            byte[] passwordHashBytes = null;
            string usernameClient = null;
            string passwordHash = null;


            byte[] bytesFeedback = null;
            string mensagemFeedback = null;
            byte[] mensagemFeedbackBytes = null;

            byte[] secretkey = null;
            byte[] iv = null;
            byte[] ack = null;

            ProtocolSICmdType protocolTipoRespostaSecretKey;
            ProtocolSICmdType protocolTipoRespostaIV;

            //----------------
            string key = null;
            byte[] keyBytes = null;
            //----------------

            try
            {
                do
                {
                    //o outro inicia o processo, estabelece a ligação com o cliente do outro lado se houver pedidos, se não houver fica parado, à espera.
                    tcpClient = tcpListener.AcceptTcpClient();
                    //aceita o pedido pendente, e devolve o tcpClient, que é, é informação sobre o cliente que está do outro lado.
                    //escreve a mensagem de sucesso de ligação.
                    Console.WriteLine("Connection Successful!");

                    //networkstream --> canal de comunicação.
                    //o tcpClient,GetStream() -> Vai buscar a networkstream criada pelo cliente.
                    networkStream = tcpClient.GetStream();
                    //diz que já pode mandar mensagens, que está preparado.
                    Console.WriteLine("Waiting for message ...");

                    key = servicoAssinaturas.ObterPublicKey();

                    //string --> vai ao objeto servicoAssinaturas do tipo ServiceCriptoAssimetrica, para obter a public key.

                    keyBytes = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, key);
                    //vai fazer um buffer do tipo public key, com a public key recebida anteriormente.
                    networkStream.Write(keyBytes, 0, keyBytes.Length);
                    //vai enviar uma mensagem para o cliente, com o buffer da public key.
                    //o cliente vai receber do seu lado, a public key. linha 99
                    Console.WriteLine("Public Key Shared");

                    do
                    {
                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        //lê para o buffer a key, enviada anteriormente pelo servidor.

                        protocolTipoRespostaSecretKey = protocolSI.GetCmdType();

                        if (protocolTipoRespostaSecretKey == ProtocolSICmdType.SECRET_KEY)
                        {
                            secretkey = servicoAssinaturas.DecriptarDados(protocolSI.GetData());
                            Console.WriteLine("Received Secret Key");

                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);
                            Console.WriteLine("Acknowlegment (ACK) Sent");

                            do
                            {
                                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                                protocolTipoRespostaIV = protocolSI.GetCmdType();

                                if (protocolTipoRespostaIV == ProtocolSICmdType.IV)
                                {
                                    iv = servicoAssinaturas.DecriptarDados(protocolSI.GetData());
                                    Console.WriteLine("Received IV");

                                    servicoCriptografiaSimetrica = new ServiceCriptoSimetrica(secretkey, iv);
                                }
                            }
                            while (protocolTipoRespostaIV != ProtocolSICmdType.IV);

                        }
                    }
                    while (protocolTipoRespostaSecretKey != ProtocolSICmdType.SECRET_KEY);

                    do
                    {
                        //sempre que existe um write do lado do cliente, o código vai para a parte do servidor e efetua a leitura.
                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        //vai arranjar a string do array de bytes dos dados que ele recebeu do buffer.
                        usernameClientEncriptado = protocolSI.GetData();

                        usernameClientBytes = servicoCriptografiaSimetrica.DecryptDados(usernameClientEncriptado);

                        usernameClient = Encoding.UTF8.GetString(usernameClientBytes);

                        //Envia mensagem cmd,a dizer que o username foi recebido.
                        Console.WriteLine("Client Login Attempt -> " + usernameClient);

                        //Vai a ler a password no buffer do protocol SI.
                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        //vai arranjar um array de bytes com a password, através do buffer recebido.
                        passwordHashEncriptada = protocolSI.GetData();

                        passwordHashBytes = servicoCriptografiaSimetrica.DecryptDados(passwordHashEncriptada);

                        passwordHash = Encoding.UTF8.GetString(passwordHashBytes);

                        Console.WriteLine("Client Password Hash Received -> " + passwordHash);
                        //Escreve a dizer que recebeu a palavra-passe.


                        //inicia a parte da base de dados. Onde vai verificar o username e a hash da password.
                        //Database
                        connect = new SqlConnection();
                        connect.ConnectionString = Properties.Settings.Default.connectionString;

                        connect.Open();

                        String sql = "SELECT * FROM Users WHERE Username = @username";
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = sql;

                        //o parâmetro serve para evitar o sql injection.
                        //adiciona o parâmetro ao comando sql
                        //Igual a connection do comando à Connection de cima para saber para onde deverá enviar a query.
                        //vai fazerum reader para leitura dos dados obtidos pela execução da query.*/

                        SqlParameter param = new SqlParameter("@username", usernameClient);
                        cmd.Parameters.Add(param);
                        cmd.Connection = connect;

                        SqlDataReader reader = cmd.ExecuteReader();

                        if(reader.HasRows)
                        {
                            //se o utilizador for válido vai obter as passwords e a hash e tudo, os diversos resultados da query.
                            // devolve o salt que vai ser utilizado para fazer salted hash a password introduzida pelo cliente ao inciar sessão.
                            //gerar a saltedhash, recorrendo a hash devolvida e à password inserida.
                            //fecha a conexão da base de dados.


                            reader.Read();

                            byte[] saltedPasswordHashStored = (byte[])reader["Password"];
                            byte[] saltStored = (byte[])reader["Salt"];
                            byte[] saltedhash = GenerateSaltedHash(Encoding.UTF8.GetBytes(passwordHash), saltStored);

                            connect.Close();
                            

                            //Verifica se a saltedhash gerada é igual a saltedhash registada na base de dados
                            if (saltedhash.SequenceEqual(saltedPasswordHashStored))
                            {
                                  //se for igual inicializa o a mensagemfeedback = successful
                                  //vai transformar a string num buffer para enviar para a network stream.
                                  //enviar a mensagem, o buffer que contem o feedback do login.
                                  //ao escrever vai para o cliente, que estava À espera de receber um feedback sobre o login.
                                  //o cliente estava parado, agora continua com o server de forma síncrona.
                                  //mensagem a dizer o feedback.

                                mensagemFeedback = "SUCCESSFUL";
                                mensagemFeedbackBytes = Encoding.UTF8.GetBytes(mensagemFeedback);
                                bytesFeedback = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptografiaSimetrica.EncryptDados(mensagemFeedbackBytes, mensagemFeedbackBytes.Length));
                                networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);
                                Console.WriteLine("Login Attempt -> " + mensagemFeedback);
                            }
                            else
                            {
                                //se as hash's forem diferentes o feedback fica com erro.
                                //cria um array de bytes, do tipo data, contendo o feedback da mensagem.
                                //O servidor vai enviar através da networkStream(canal de comunicação), o feedback de erro/insucesso na tentativa de login
                                //escreve na consola, que ocorreu um erro na tentativa de login.

                                mensagemFeedback = "FAILED";
                                mensagemFeedbackBytes = Encoding.UTF8.GetBytes(mensagemFeedback);
                                bytesFeedback = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptografiaSimetrica.EncryptDados(mensagemFeedbackBytes, mensagemFeedbackBytes.Length));
                                networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);
                                Console.WriteLine("Login Attempt -> " + mensagemFeedback);
                                
                            }

                            //--------
                        }
                        else
                        {
                            //se as hash's forem diferentes o feedback fica com erro.
                            //cria um array de bytes, do tipo data, contendo o feedback da mensagem.
                            //O servidor vai enviar através da networkStream(canal de comunicação), o feedback de erro/insucesso na tentativa de login
                            //escreve na consola, que ocorreu um erro na tentativa de login.

                            mensagemFeedback = "FAILED";
                            mensagemFeedbackBytes = Encoding.UTF8.GetBytes(mensagemFeedback);
                            bytesFeedback = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptografiaSimetrica.EncryptDados(mensagemFeedbackBytes, mensagemFeedbackBytes.Length));
                            networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);
                            Console.WriteLine("Login Attempt -> " + mensagemFeedback);
                        }
                    }
                    while (mensagemFeedback != "SUCCESSFUL");
                    //faz enquanto não for bem sucedido.

                }
                while (mensagemFeedback != "SUCCESSFUL");
                //faz enquanto não for bem sucedido.
            }

            //Para o caso de ocorrerem excepções.
            //Chama o procedimento StopServer() e fecha as ligações que estiverem abertas.
            catch (Exception)
            {
                //throw;
                StopServer();
            }
        }

        private static void ServerSendList()
        {
            try
            {
                //O servidor vai buscar todos os ficheiros disponíveis, do path (global).
                //Escreve na consola quantos ficheiros estão disponíveis para ser enviados.
                //vai concatenar e devolve a filelist concatenada.


                string[] fileListArray = Directory.GetFiles(fileListPath);
                byte[] fileListBuffer = null;
                string fileList = null;

                //-----------------------

                byte[] assinaturaBytes = null;

                //-----------------------

                
                Console.WriteLine("Sending {0} Files:", fileListArray.Count());
                
                fileList = ConcatFileNames(fileListArray);

                //Serve para verificar a integralidade dos dados, através das assinaturas
                assinaturaBytes = servicoAssinaturas.AssinarDados(fileList);
                byte[] assinaturaPacket = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, assinaturaBytes);
                networkStream.Write(assinaturaPacket, 0, assinaturaPacket.Length);
                //----------------------

                //Obtem um buffer do tipo de dados criado com a filelist
                //Envia para a network stream, o buffer da lista de ficheiros.
                //Ir para o cliente, para ler os dados e segmentar tudo.
                //Escreve na consola que todos os ficheiros foram enviados.

                byte[] fileListBytes = Encoding.UTF8.GetBytes(fileList);

                fileListBuffer = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptografiaSimetrica.EncryptDados(fileListBytes, fileListBytes.Length));
                
                networkStream.Write(fileListBuffer, 0, fileListBuffer.Length);
                
                Console.WriteLine("All File Names SENT!");
            }

            catch (Exception)
            {
                //throw;
                StopServer();
            }
        }

        private static string ConcatFileNames(string[] fileListArray)
        {
            string fileName = null;
            string fileList = null;

            foreach (string filePath in fileListArray)
            {
                fileName = Path.GetFileName(filePath) + ";";
                Console.WriteLine("\t" + fileName);
                //vai fazer um ciclo e vai adicionar à file list, os file names. +=
                fileList = String.Concat(fileList, fileName);
            }

            return fileList;
        }

        private static void ServerSendFile(string fileName)
        {
            try
            {
                //obter caminho do ficheiro, combina o path onde está o filelistpath caminho global.
                string filePath = Path.Combine(fileListPath, fileName);

                //Abre o ficheiro que quer copiar
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    //NOVO
                    
                    byte[] assinaturaBytes = null;
                    //buffer onde vão ficar os bytes da imagem completa.
                    byte[] fullImageBuffer = new byte[fileStream.Length];
                    byte[] imageHash = null;
                    byte[] packetAssinatura = null;

                    //vai ler para o buffer o ficheiro inteiro
                    fileStream.Read(fullImageBuffer, 0, fullImageBuffer.Length);
                    //poe na posição inicial da stream.
                    fileStream.Seek(0, SeekOrigin.Begin);

                    //devolve a hash da imagem num array de bytes.
                    imageHash = servicoAssinaturas.HashImagem(fullImageBuffer);

                    //assina o array de bytes, com private key.
                    assinaturaBytes = servicoAssinaturas.AssinarHash(imageHash);

                    //Devolve um buffer do tipo assinatura digital, com o buffer da assinatura 
                    packetAssinatura = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, assinaturaBytes);
                    networkStream.Write(packetAssinatura, 0, packetAssinatura.Length); //assinatura
                    //envia para o cliente o packet da assinatura
                    
                    //----------


                    byte[] imageBuffer = null;
                    byte[] imageBufferEncriptado = null;
                    int bytesFileRead;

                    int fileBufferSize = 1024;
                    byte[] fileBuffer = new byte[fileBufferSize];

                    byte[] endOfFile;

                    ProtocolSICmdType protocaolTipoResposta;

                    //vai ler um pedaço do ficheiro, para o filebuffer, com o tamanho máximo de 1024.
                    bytesFileRead = fileStream.Read(fileBuffer, 0, fileBufferSize);
                    //cria um array de bytes do tipo data, com o  filebuffer do tamanho bytesfileread.
                    imageBufferEncriptado = servicoCriptografiaSimetrica.EncryptDados(fileBuffer, bytesFileRead);
                    imageBuffer = protocolSI.Make(ProtocolSICmdType.DATA, imageBufferEncriptado);
                    //envia para o cliente o buffer da imagem.
                    networkStream.Write(imageBuffer, 0, imageBuffer.Length);
                    //Escreve na consola o nº bytes enviados.
                    Console.WriteLine("\t-> Bytes Sent: " + bytesFileRead + " + " + (imageBuffer.Length - bytesFileRead));

                    //enquanto copia a imagem por partes
                    do
                    {
                        bytesFileRead = fileStream.Read(fileBuffer, 0, fileBufferSize);
                        //vai ler outro bocado do ficheiro.
                        do
                        {
                            //recebe o ack
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                            //vai buscar o tipo da mensagem anterior, neste caso a ack.
                            protocaolTipoResposta = protocolSI.GetCmdType();

                            //só é enviada se o server recebeu o ack do cliente, do bocado anterior passado.
                            if (protocaolTipoResposta == ProtocolSICmdType.ACK)
                            {
                                //se ele leu uma parte da imagem
                                if (bytesFileRead > 0)
                                {
                                    //Vai enviá-la para o cliente
                                    imageBuffer = protocolSI.Make(ProtocolSICmdType.DATA, servicoCriptografiaSimetrica.EncryptDados(fileBuffer, bytesFileRead));
                                    networkStream.Write(imageBuffer, 0, imageBuffer.Length);
                                    Console.WriteLine("\t-> Bytes Sent: " + bytesFileRead + " + " + (imageBuffer.Length - bytesFileRead));
                                }

                                else
                                {
                                    //Se fo igual a zero, deu fim de ficheiro.
                                    endOfFile = protocolSI.Make(ProtocolSICmdType.EOF);
                                    networkStream.Write(endOfFile, 0, endOfFile.Length);
                                    Console.WriteLine("\t-> End of File (EOF) Sent");
                                }
                            }
                        }
                        while (protocaolTipoResposta != ProtocolSICmdType.ACK);

                    }
                    while (bytesFileRead > 0);
                    //enquanto le o bocado da imagem, quando acabar 

                }

                Console.WriteLine("File '" + fileName + "' SENT!");
                //Envia mensagem a dizer que o ficheiro foi enviado.
            }

            catch (Exception)
            {
                StopServer();
                //throw;
            }
        }

        private static void StopServer()
        {
            if (networkStream != null)
            {
                networkStream.Close();
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
            }

            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }
    }
}
