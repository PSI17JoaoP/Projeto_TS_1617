namespace Projeto
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbLogin = new System.Windows.Forms.GroupBox();
            this.gbMenu = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUtilizador = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lvLista = new System.Windows.Forms.ListView();
            this.label3 = new System.Windows.Forms.Label();
            this.btnObterLista = new System.Windows.Forms.Button();
            this.btnObterFicheiro = new System.Windows.Forms.Button();
            this.btnAbrirFicheiro = new System.Windows.Forms.Button();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.gbLogin.SuspendLayout();
            this.gbMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbLogin
            // 
            this.gbLogin.Controls.Add(this.btnLogin);
            this.gbLogin.Controls.Add(this.txtPassword);
            this.gbLogin.Controls.Add(this.txtUtilizador);
            this.gbLogin.Controls.Add(this.label2);
            this.gbLogin.Controls.Add(this.label1);
            this.gbLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbLogin.Location = new System.Drawing.Point(0, 0);
            this.gbLogin.MinimumSize = new System.Drawing.Size(287, 180);
            this.gbLogin.Name = "gbLogin";
            this.gbLogin.Size = new System.Drawing.Size(287, 180);
            this.gbLogin.TabIndex = 0;
            this.gbLogin.TabStop = false;
            this.gbLogin.Text = "Login";
            // 
            // gbMenu
            // 
            this.gbMenu.Controls.Add(this.btnAbrirFicheiro);
            this.gbMenu.Controls.Add(this.btnObterFicheiro);
            this.gbMenu.Controls.Add(this.btnObterLista);
            this.gbMenu.Controls.Add(this.label3);
            this.gbMenu.Controls.Add(this.lvLista);
            this.gbMenu.Location = new System.Drawing.Point(0, 178);
            this.gbMenu.MinimumSize = new System.Drawing.Size(287, 207);
            this.gbMenu.Name = "gbMenu";
            this.gbMenu.Size = new System.Drawing.Size(287, 210);
            this.gbMenu.TabIndex = 1;
            this.gbMenu.TabStop = false;
            this.gbMenu.Text = "Menu";
            this.gbMenu.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Utilizador";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password";
            // 
            // txtUtilizador
            // 
            this.txtUtilizador.Location = new System.Drawing.Point(101, 48);
            this.txtUtilizador.Name = "txtUtilizador";
            this.txtUtilizador.Size = new System.Drawing.Size(174, 24);
            this.txtUtilizador.TabIndex = 2;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(101, 89);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(174, 24);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(194, 138);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(81, 30);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Entrar";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // lvLista
            // 
            this.lvLista.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lvLista.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvLista.Location = new System.Drawing.Point(15, 63);
            this.lvLista.Name = "lvLista";
            this.lvLista.Size = new System.Drawing.Size(260, 96);
            this.lvLista.TabIndex = 0;
            this.lvLista.UseCompatibleStateImageBehavior = false;
            this.lvLista.View = System.Windows.Forms.View.Details;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "Lista de ficheiros";
            // 
            // btnObterLista
            // 
            this.btnObterLista.Location = new System.Drawing.Point(182, 21);
            this.btnObterLista.Name = "btnObterLista";
            this.btnObterLista.Size = new System.Drawing.Size(93, 36);
            this.btnObterLista.TabIndex = 2;
            this.btnObterLista.Text = "Obter lista";
            this.btnObterLista.UseVisualStyleBackColor = true;
            // 
            // btnObterFicheiro
            // 
            this.btnObterFicheiro.Location = new System.Drawing.Point(15, 165);
            this.btnObterFicheiro.Name = "btnObterFicheiro";
            this.btnObterFicheiro.Size = new System.Drawing.Size(126, 39);
            this.btnObterFicheiro.TabIndex = 3;
            this.btnObterFicheiro.Text = "Obter ficheiro";
            this.btnObterFicheiro.UseVisualStyleBackColor = true;
            // 
            // btnAbrirFicheiro
            // 
            this.btnAbrirFicheiro.Location = new System.Drawing.Point(149, 165);
            this.btnAbrirFicheiro.Name = "btnAbrirFicheiro";
            this.btnAbrirFicheiro.Size = new System.Drawing.Size(126, 39);
            this.btnAbrirFicheiro.TabIndex = 4;
            this.btnAbrirFicheiro.Text = "Abrir ficheiro";
            this.btnAbrirFicheiro.UseVisualStyleBackColor = true;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Nome";
            this.columnHeader1.Width = 115;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Disponível";
            this.columnHeader2.Width = 73;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(287, 386);
            this.Controls.Add(this.gbMenu);
            this.Controls.Add(this.gbLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(305, 433);
            this.MinimumSize = new System.Drawing.Size(305, 229);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cliente";
            this.gbLogin.ResumeLayout(false);
            this.gbLogin.PerformLayout();
            this.gbMenu.ResumeLayout(false);
            this.gbMenu.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbLogin;
        private System.Windows.Forms.GroupBox gbMenu;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUtilizador;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAbrirFicheiro;
        private System.Windows.Forms.Button btnObterFicheiro;
        private System.Windows.Forms.Button btnObterLista;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView lvLista;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}

