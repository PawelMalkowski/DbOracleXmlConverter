using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Text.Json;

namespace DbOracleXmlConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        volatile DataFromDB dataFromDB = null;
        volatile FillDB fillDB=null;
        volatile string login;
        volatile string password;
        volatile DbToXml t=null;
        public MainWindow()
        {
            InitializeComponent();
            
        }
        private void Dowload()
        {
            dataFromDB = new DataFromDB(login,password);
        }
        private void Upload()
        {
            fillDB = new FillDB(t, login, password);
        }
        private async  void CreateXml_Click(object sender, RoutedEventArgs e)
        {
            login = Login.Text;
            password = Passowrd.Password;
            Login.IsEnabled = false;
            Passowrd.IsEnabled = false;
            CreateXml.IsEnabled = false;
            ExportXmlToDB.IsEnabled = false;
            bool connectionSucces = true;
            Comunicate.Content = "Trwa pobieranie danych z bazy...";
            try
            {
                await Task.Run(() => Dowload());
            }
             catch
            {
                MessageBox.Show("zły login lub hasło", "Error");
                connectionSucces = false;
            }
            Comunicate.Content = "";
            Login.IsEnabled = true;
            Passowrd.IsEnabled = true;
            CreateXml.IsEnabled = true;
            ExportXmlToDB.IsEnabled = true ;

            if (connectionSucces)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Import";
                dlg.DefaultExt = ".xml";
                dlg.Filter = ".xml|*.xml|.json|*.json";

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    var extension = System.IO.Path.GetExtension(dlg.FileName);
                    switch (extension.ToLower())
                    {

                        case ".xml":
                            using (TextWriter writer = new StreamWriter(dlg.FileName))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(DbToXml));
                                serializer.Serialize(writer, dataFromDB.dbToXml);
                            }
                            break;
                        case ".json":
                            using (FileStream fs = File.Create(dlg.FileName))
                            {
                                await JsonSerializer.SerializeAsync(fs, dataFromDB.dbToXml);
                            }
                            break;
                    }

                }
            }
           
        }

        private async void ExportXmlToDB_Click(object sender, RoutedEventArgs e)
        {
            string filename="";
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();


            dlg.DefaultExt = ".xml";
            dlg.Filter = "Xml Files (*.xml)|*.xml|JSON Files (*.json)|*.json";
            bool correctFile = true; 

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                var extension = System.IO.Path.GetExtension(dlg.FileName);
                switch (extension.ToLower())
                {

                    case ".xml":
                        filename = dlg.FileName;
                        try
                        {
                            t = DeserializeObject(filename);
                        }
                        catch
                        {
                            correctFile = false;
                        }
                        
                        break;
                    case ".json":
                        using (FileStream fs = File.OpenRead(dlg.FileName))
                        {
                            t = await JsonSerializer.DeserializeAsync<DbToXml>(fs);

                        }
                        break;
                }
                if (correctFile && t.dataFromTables != null && t.foreignKeys != null && t.primaryKeys != null && t.tables != null)
                {
                    login = Login.Text;
                    password = Passowrd.Password;
                    Login.IsEnabled = false;
                    Passowrd.IsEnabled = false;
                    CreateXml.IsEnabled = false;
                    ExportXmlToDB.IsEnabled = false;
                    Comunicate.Content = "Trwa odtwarzenie bazy z pliku xml...";
                    try
                    {
                        await Task.Run(() => Upload());
                        MessageBox.Show("Pomyślnie dodano", "Succes");
                    }
                    catch
                    {
                        MessageBox.Show("zły login lub hasło", "Error");
                    }
                   
                    Comunicate.Content = "";
                    Login.IsEnabled = true;
                    Passowrd.IsEnabled = true;
                    CreateXml.IsEnabled = true;
                    ExportXmlToDB.IsEnabled = true;
                }

                else
                {
                    MessageBox.Show("Niewłaściwy plik", "error");
                }
            }
           
           
        }
        private DbToXml DeserializeObject(string filename)
        {
            XmlSerializer serializer =
            new XmlSerializer(typeof(DbToXml));
            DbToXml i;

            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                i = (DbToXml)serializer.Deserialize(reader);
            }
            
            return i;
        }
        }
}
