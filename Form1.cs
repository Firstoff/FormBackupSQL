using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
// Microsoft.SqlServer.SqlManagementObjects nuget package Smo + Common
using System;
using System.Configuration;
using Config.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Diagnostics;
using Microsoft.SqlServer.Management.HadrData;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Collections;
using System.Data.SqlTypes;
using System.Security.Policy;
using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace FormBackupSQL
{
    public partial class Form1 : Form
    {
        //Так объявляю создания объекта типа IMySettings
        public static IMySettings configiniP = new ConfigurationBuilder<IMySettings>()
            .UseIniFile(@"config.ini", true)
            .Build();

        // Ну теперь в Main существует объект configiniP.
        // c полями что описаны в интерфейсе.

        
        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyDown += new KeyEventHandler(textBox2_KeyDown);
            //textBox3.Text = configiniP.BackupFullPath; // не нужен, все в об
            //textBox2.Text = configiniP.BackupDiffPath; // не нужен, все в об
            //txtDatabase.Text = configiniP.SourceDatabase;
            //string dbname = configiniP.SourceDatabase;
            //string FullBackupName = dbname + "_do" + dateTimePicker1.Value.ToString("ddMMyyyy") + ".bak"; // формируем имя фулл бекапа
            //string DiffBackupName = "DIFF_" + dbname + "_do" + dateTimePicker1.Value.ToString("ddMMyyyy") + ".bak"; // формируем имя дифф бекапа
            //txtFullBackupName.Text = FullBackupName;
            //txtDiffBackupName.Text = DiffBackupName;

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        

        // здесь в календаре, после выбора даты, меняется имя архива
        void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            string keepdate = dateTimePicker1.Value.ToString("ddMMyyyy");
            //string keepdate = dateTimePicker1.Value.AddDays(1).ToString("ddMMyyyy"); // работает, +1 день
            //textBox1.Text = keepdate;
            string dbname = txtDatabase.Text;
            string FullBackupName = dbname + "_do" + dateTimePicker1.Value.ToString("ddMMyyyy") + ".bak";
            txtFullBackupName.Text = FullBackupName;

            string DiffBackupName = "DIFF_" + dbname + "_do" + dateTimePicker1.Value.ToString("ddMMyyyy") + ".bak";
            txtDiffBackupName.Text = DiffBackupName;

            if (button1.BackColor == Color.Moccasin)
            {
                button1.BackColor = Color.PaleGreen;
                btnBackup.BackColor = Color.PaleGreen;
            }
            
            //else
            //{
            //    button1.BackColor = Color.Moccasin;
            //    btnBackup.BackColor = Color.Moccasin;
            //}

        }

        void btnBackup_Click(object sender, EventArgs e) // FULL BACKUP BUTTON
        {
            progressBar.Value = 0;
            try
            {

                //Init connect to sql database
                Server dbServer = new Server(new ServerConnection(txtServer.Text));
                // Определение состояния базы данных
                Database db = dbServer.Databases[txtDatabase.Text];
                if (db.Status != DatabaseStatus.Normal)
                {
                    MessageBox.Show("База данных занята другим процессом и не может быть сохранена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Продолжаем резервное копирование базы данных
                Backup dbBackup = new Backup() { Action = BackupActionType.Database, Database = txtDatabase.Text };
                string FullBackupName = txtFullBackupName.Text; // Получаем имя файла из текстового поля, зависит от календаря
                dbBackup.Devices.AddDevice(FullBackupName, DeviceType.File);
                dbBackup.Initialize = true;
                dbBackup.Incremental = false; //FULL backup
                dbBackup.PercentComplete += DbBackup_PercentComplete;
                dbBackup.Complete += DbBackup_Complete;
                dbBackup.SqlBackupAsync(dbServer);
                //MessageBox.Show("Архив выполнен");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button1_Click(object sender, EventArgs e) // DIFF BACKUP BUTTON
        {
            progressBar.Value = 0;
            try
            {
                Server dbServer = new Server(new ServerConnection(configiniP.SourceServer));
                Backup dbBackup = new Backup() { Action = BackupActionType.Database, Database = txtDatabase.Text };
                string DiffBackupName = "DIFF_" + txtFullBackupName.Text; // Получаем имя файла из текстового поля, зависит от календаря
                dbBackup.Devices.AddDevice(DiffBackupName, DeviceType.File);
                dbBackup.Initialize = true;
                dbBackup.Incremental = true; //DIFF backup
                dbBackup.PercentComplete += DbBackup_PercentComplete;
                dbBackup.Complete += DbBackup_Complete;
                dbBackup.SqlBackupAsync(dbServer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DbBackup_Complete(object sender, ServerMessageEventArgs e) // статус выполнения архивации
        {
            if (e.Error != null)
            {
                //Update status with multiple threads
                lblStatus.Invoke((MethodInvoker)delegate
                {
                    lblStatus.Text = e.Error.Message;
                });
            }
        }

        private void DbBackup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            //Update percentage, progressbar
            progressBar.Invoke((MethodInvoker)delegate
            {
                progressBar.Value = e.Percent;
                progressBar.Update();
            });
            lblPercent.Invoke((MethodInvoker)delegate
            {
                lblPercent.Text = $"{e.Percent}%";
            });
        }

        
        private void btnCopyPath_Click(object sender, EventArgs e)
        {
            // TODO: взять данные из пути бекапа и выкинуть их в буфер обмена
        }

        //В конце  объявляю этот тип как интерфейс
        public interface IMySettings
        {
            //здесь обьявляем все нужные переменные из ини файла
            // TODO: разобраться какие переменные куда пихать в IMySettings
            string BackupFullPath { get; }
            string BackupDiffPath { get; }
            //string BackupDiffPath { get; set; }
            string SourceDatabase { get; }
            string SourceServer { get; }
            //количество дней +
            int DaysPlus { get; }
            string SourceServers { get; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog(owner: this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // TODO: сделать загрузку листбокса из config.ini при загрузке формы
            string[] lines = File.ReadAllLines("config.ini");
            //listBox1.DataSource = lines;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            // здесь код открытия в проводнике каталога куда положился архив
            // если каталог существует - открываем, иначе зависнет, поэтому есть проверка.
            // TODO: diff для примера !!!! поменять на что-то нужное
            DirectoryInfo source = new DirectoryInfo(configiniP.BackupDiffPath);
            if (source.Exists)
                Process.Start(configiniP.BackupDiffPath);
            return;


        }

        static List<string> GetServerNamesFromConfig()
        {
            string serversString = ConfigurationManager.AppSettings["Servers"];
            string[] serverNames = serversString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(serverNames);
        }

        static string GetConnectionStringFromConfig(string serverName)
        {
            string connectionStringTemplate = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return string.Format(connectionStringTemplate, serverName);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            SearchBase();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchBase();
            }
        }

        private void SearchBase() // для поиска по нажатию Ентер в поле ввода базы или кнопки Search в форме.
        {
            string databaseName = txtDatabase.Text;
            List<string> serverNames = GetServerNamesFromConfig();

            bool foundDatabase = false;
            foreach (string serverName in serverNames)
            {
                try
                {
                    string connectionString = GetConnectionStringFromConfig(serverName);
                    using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = $"SELECT name, size FROM sys.master_files WHERE DB_NAME(database_id) = '{databaseName}'";
                        using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(query, connection))
                        {
                            System.Data.SqlClient.SqlDataReader reader = command.ExecuteReader();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string name = reader.GetString(0);
                                    int size = reader.GetInt32(1);
                                    txtServer.Text = serverName; //показывает где нашел
                                    txtServers.Text += $"{serverName}, Database: {name}, Size: {size} KB\r\n"; // показывает что нашел
                                    string FullBackupName_bak = serverName + "/ob$/txtFullBackupName.Text"; //здесь задается путь к бекапу

                                    foundDatabase = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!foundDatabase)
            {
                MessageBox.Show($"База '{databaseName}' не найдена или у вас нет доступа!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
    {
    }
    private void BtnGetInfoBak_Click(object sender, EventArgs e)
    {
        //Вызываем окно проводника - работает
            OpenFileDialog opfd = new OpenFileDialog();

        //Если выбрали то
            if (opfd.ShowDialog(this) == DialogResult.OK)
        {
            //Получаем имя файла в текстбокс для отладки - работает
                txtBoxFile.Text = opfd.FileName;
        }
    }

        private void BtnGetBaseParams_Click(object sender, EventArgs e)
        {
            // по нажатию кнопки берем имя базы куда ресторим из текстового поля ищем ее на скл и получаем ее данные
            // имена mdf ldf размер
        }

        private void txtServers_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // TODO: вкладка ресторе
        // может и не надо ее
        //

        // TODO: для ДИФФ вкладки - бэкапы баз из загружаемого списка на завтрашнюю дату
        // быстрый, выбрал дату, закинул список и погнали
        // в конце выдать результат списком баз и путями архива в лог, текстовый месседж и отправить себе на почту.

        // TODO: вкладка актуализации баз
        // для выбора исходной и конечной баз
        // первая кнопка проверка наличия баз и вторая кнопка актуализация
        // результат в техтбокс и на почту

        // планы по др проектам:
        // файловый доступ по ИД03 допилить
        //
        // выполнение кода повершелл из c# и отъем результатов в переменные/массив переменных
        // для дальнейшей обработки
        // пойдет для всякого типа эксч и прочего

        // собрать солянку для абон - не актуально, абон - всё
        // отключить шару, убить все скл процессы, сессии на рдп, включит шару обратно

    }
}
