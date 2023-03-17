using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace ProgramInstaller.Helpers; 
public static class XmlHandler {
    public static void GetDados(DataGrid dtProgramas, bool selectFirstIndex = false)
    {
        try {
            DataSet dsResultado = new();
            dsResultado.ReadXml(@"config\config.xml");
            if (dsResultado.Tables.Count != 0) {
                if (dsResultado.Tables[0].Rows.Count > 0) {
                    dtProgramas.ItemsSource = new DataView(dsResultado.Tables["Programa"]);
                }
            }

            if (selectFirstIndex)
                dtProgramas.SelectedIndex = 0;
        }
        catch (Exception ex) {
            throw;
        }
    }

    public static void InserirDados(Config config)
    {
        try {
            using (DataSet dsResultado = new DataSet()) {
                dsResultado.ReadXml(@"config\config.xml");
                if (dsResultado.Tables.Count == 0) {
                    //cria uma instância do Produto e atribui valores às propriedades
                    Programa programa = new Programa();
                    programa.Id = Convert.ToInt32(config.txtId.Text);
                    programa.Nome = config.txtNome.Text;
                    programa.Caminho = config.txtCaminho.Text;
                    programa.Argumentos = config.txtArgumentos.Text;

                    if (config.chk32bits.IsChecked == true) {
                        programa.x86 = "S";
                    }
                    else {
                        programa.x86 = "N";
                    }

                    if (config.chk64bits.IsChecked == true) {
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
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Id"] = config.txtId.Text;
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Nome"] = config.txtNome.Text;
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Caminho"] = config.txtCaminho.Text;
                    dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["Argumentos"] = config.txtArgumentos.Text;

                    if (config.chk32bits.IsChecked == true) {
                        dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["x86"] = "S";
                    }
                    else {
                        dsResultado.Tables[0].Rows[dsResultado.Tables[0].Rows.Count - 1]["x86"] = "N";
                    }

                    if (config.chk64bits.IsChecked == true) {
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

                XmlHandler.GetDados(config.dtProgramas, selectFirstIndex: true);

                System.Windows.MessageBox.Show("Dados salvos com sucesso.");
            }
        }
        catch (Exception ex) {
            throw ex;
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

    public static void ExcluirDados(string txtId)
    {
        if (txtId != "") {
            XElement xml = XElement.Load(@"config\config.xml");
            XElement x = xml.Elements().Where(p => p.Element("Id").Value == txtId).First();
            if (x != null) {
                x.Remove();
            }
            xml.Save("config.xml");

            MessageBox.Show("Registro excluído com sucesso!");
        }
        else {
            MessageBox.Show("Selecione um registro clicando duas vezes nele!");
        }

    }
}
