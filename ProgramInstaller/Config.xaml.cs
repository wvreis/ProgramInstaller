using ProgramInstaller.Models;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace ProgramInstaller;
/// <summary>
/// Interaction logic for Config.xaml
/// </summary>
public partial class Config : Window {
    public Config()
    {
        InitializeComponent();
        GetDados();
    }

    public void GetDados()
    {
        dtProgramas.ItemsSource = null;
        try {
            DataSet dsResultado = new DataSet();
            dsResultado.ReadXml(@"config\config.xml");
            if (dsResultado.Tables.Count != 0) {
                if (dsResultado.Tables[0].Rows.Count > 0) {
                    dtProgramas.ItemsSource = new DataView(dsResultado.Tables["Programa"]);
                }
            }
        }
        catch (Exception ex) {
            throw ex;
        }

        dtProgramas.SelectedIndex = 0;
    }

    public void InserirDados()
    {
        try {
            using (DataSet dsResultado = new DataSet()) {
                dsResultado.ReadXml(@"config\config.xml");
                if (dsResultado.Tables.Count == 0) {
                    //cria uma instância do Produto e atribui valores às propriedades
                    Programa programa = new Programa();
                    programa.Id = Convert.ToInt32(txtId.Text);
                    programa.Nome = txtNome.Text;
                    programa.Caminho = txtCaminho.Text;
                    programa.Argumentos = txtArgumentos.Text;

                    if (chk32bits.IsChecked == true) {
                        programa.x86 = "S";
                    }
                    else {
                        programa.x86 = "N";
                    }

                    if (chk64bits.IsChecked == true) {
                        programa.x64 = "S";
                    }
                    else {
                        programa.x64 = "N";
                    }

                    XmlTextWriter writer = new XmlTextWriter(@"config\config.xml", System.Text.Encoding.UTF8);
                    writer.WriteStartDocument(true);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 2;

                    writer.WriteStartElement("Programas");
                    writer.WriteStartElement("Programa");

                    writer.WriteStartElement("Id");
                    writer.WriteString(programa.Id.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("Nome");
                    writer.WriteString(programa.Nome);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Caminho");
                    writer.WriteString(programa.Caminho.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("Argumentos");
                    writer.WriteString(programa.Argumentos.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("x86");
                    writer.WriteString(programa.x86);
                    writer.WriteEndElement();

                    writer.WriteStartElement("x64");
                    writer.WriteString(programa.x64);
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();
                    dsResultado.ReadXml(@"config\config.xml");
                }
                else {
                    //inclui os dados no DataSet
                    dsResultado.Tables[0].Rows.Add(dsResultado.Tables[0].NewRow());
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Id"] = txtId.Text;
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Nome"] = txtNome.Text;
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Caminho"] = txtCaminho.Text;
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Argumentos"] = txtArgumentos.Text;

                    if (chk32bits.IsChecked == true) {
                        dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["x86"] = "S";
                    }
                    else {
                        dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["x86"] = "N";
                    }

                    if (chk64bits.IsChecked == true) {
                        dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["x64"] = "S";
                    }
                    else {
                        dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["x64"] = "N";
                    }

                    dsResultado.AcceptChanges();
                    //--  Escreve para o arquivo XML final usando o método Write
                    dsResultado.WriteXml(@"config\config.xml", XmlWriteMode.IgnoreSchema);
                }
                //exibe os dados no gridview                                      

                GetDados();

                System.Windows.MessageBox.Show("Dados salvos com sucesso.");
            }
        }
        catch (Exception ex) {
            throw ex;
        }
    }

    private void dtProgramas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DataRowView dt = (DataRowView)dtProgramas.SelectedItem;

        if (dt is null)
            return;

        txtId.Text = dt.Row.ItemArray[0].ToString();
        txtNome.Text = dt.Row.ItemArray[1].ToString();
        txtCaminho.Text = dt.Row.ItemArray[2].ToString();
        txtArgumentos.Text = dt.Row.ItemArray[3].ToString();

        if (dt.Row.ItemArray[4].ToString() == "S") {
            chk32bits.IsChecked = true;
        }
        else {
            chk32bits.IsChecked = false;
        }

        if (dt.Row.ItemArray[5].ToString() == "S") {
            chk64bits.IsChecked = true;
        }
        else {
            chk64bits.IsChecked = false;
        }
    }

    public static void EditarDados(string txtId, string txtNome, string txtCaminho, string txtArgumentos, bool x86, bool x64)
    {
        try {
            XElement xml = XElement.Load(@"config\config.xml");
            XElement x = xml.Elements().Where(p => p.Element("Id").Value == txtId).First();

            if (x != null) {
                x.Element("Nome").SetValue(txtNome);
                x.Element("Caminho").SetValue(txtCaminho);
                x.Element("Argumentos").SetValue(txtArgumentos);

                if (x86 == true) {
                    x.Element("x86").SetValue("S");
                }
                else {
                    x.Element("x86").SetValue("N");
                }

                if (x64 == true) {
                    x.Element("x64").SetValue("S");
                }
                else {
                    x.Element("x64").SetValue("N");
                }

            }
            xml.Save(@"config\config.xml");
        }
        catch (Exception ex) {

            throw ex;
        }
    }

    private void AlterarDados()
    {
        if (txtId.Text != "") {
            EditarDados(txtId.Text, txtNome.Text, txtCaminho.Text, txtArgumentos.Text, (bool)chk32bits.IsChecked, (bool)chk64bits.IsChecked);

            GetDados();

            System.Windows.MessageBox.Show("Dados salvos com sucesso.");
        }
        else {
            System.Windows.MessageBox.Show("Preencha o Campo Id!!!");
        }

    }

    private void ExcluirDados(string txtId)
    {
        if (txtId != "") {
            XElement xml = XElement.Load(@"config\config.xml");
            XElement x = xml.Elements().Where(p => p.Element("Id").Value == txtId).First();
            if (x != null) {
                x.Remove();
            }
            xml.Save("config.xml");

            System.Windows.MessageBox.Show("Registro excluído com sucesso!");
        }
        else {
            System.Windows.MessageBox.Show("Selecione um registro clicando duas vezes nele!");
        }

    }

    private void AtivarEdicao()
    {
        gridDados.IsEnabled = true;
        btnAlterar.IsEnabled = false;
        btnExcluir.IsEnabled = false;
        btnNovo.IsEnabled = false;
        dtProgramas.IsEnabled = false;

        btnOk.IsEnabled = true;
        btnCancelar.IsEnabled = true;
    }

    private void DesativarEdicao()
    {
        gridDados.IsEnabled = false;
        btnAlterar.IsEnabled = true;
        btnExcluir.IsEnabled = true;
        btnNovo.IsEnabled = true;
        dtProgramas.IsEnabled = true;

        btnOk.IsEnabled = false;
        btnCancelar.IsEnabled = false;
    }

    private void LimparTela()
    {
        txtId.Text = "";
        txtNome.Text = "";
        txtCaminho.Text = "";
        txtArgumentos.Text = "";
        chk32bits.IsChecked = false;
        chk64bits.IsChecked = false;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
        if (txtId.Text != "") {
            if (this.Title == "Configuração - Inserindo Novo") {
                InserirDados();
                LimparTela();
                DesativarEdicao();
                GetDados();
            }
            else {
                AlterarDados();
                LimparTela();
                DesativarEdicao();
                GetDados();
            }
        }
        else {
            System.Windows.MessageBox.Show("Preencha o Campo Id!!!");
        }


    }

    private void btnCancelar_Click(object sender, RoutedEventArgs e)
    {
        LimparTela();
        DesativarEdicao();
        this.Title = "Configuração";
    }

    private void btnNovo_Click(object sender, RoutedEventArgs e)
    {
        LimparTela();
        AtivarEdicao();
        this.Title += " - Inserindo Novo";
    }

    private void btnAlterar_Click(object sender, RoutedEventArgs e)
    {
        if (txtId.Text != "") {
            AtivarEdicao();
            this.Title += " - Alterando";
        }
        else {
            System.Windows.MessageBox.Show("Selecione um registro clicando duas vezes nele!");
        }

    }

    private void btnExcluir_Click(object sender, RoutedEventArgs e)
    {

        if (txtId.Text != "") {
            ExcluirDados(txtId.Text);
            LimparTela();
            GetDados();
        }
        else {
            System.Windows.MessageBox.Show("Selecione um registro clicando duas vezes nele!");
        }

    }

    protected override void OnClosed(EventArgs e)
    {
        MainWindow mainWindow = new MainWindow();
        this.Close();
        mainWindow.Show();
    }
}
