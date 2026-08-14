using ProgramInstaller.Models;
using System.Windows;
using System.Windows.Controls;

namespace ProgramInstaller;

public partial class ProgramaEditor : Window
{
    public Programa Result { get; }

    public ProgramaEditor(Programa? source = null)
    {
        InitializeComponent();

        Result = source is null ? CreateNewProgram() : CopyProgram(source);

        if (source is not null)
        {
            Title = "Editar programa";
            txtTitle.Text = "Editar programa";
            txtSubtitle.Text = "Revise os dados e salve as alterações";
        }

        LoadFields();
        Loaded += (_, _) => txtNome.Focus();
    }

    private void btnSalvar_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateFields())
            return;

        Result.Nome = txtNome.Text.Trim();
        Result.Caminho = txtCaminho.Text.Trim();
        Result.Argumentos = txtArgumentos.Text.Trim();
        Result.x86 = chk32bits.IsChecked == true ? "S" : "N";
        Result.x64 = chk64bits.IsChecked == true ? "S" : "N";
        Result.Ativo = chkAtivo.IsChecked == true;

        DialogResult = true;
    }

    private void btnBuscarWinget_Click(object sender, RoutedEventArgs e)
    {
        WingetCatalogWindow catalog = new() { Owner = this };

        if (catalog.ShowDialog() != true || catalog.Result is not WingetPackage package)
            return;

        txtNome.Text = package.Name;
        txtCaminho.Text = "winget";
        txtArgumentos.Text = BuildWingetInstallArguments(package.Id);
        chk32bits.IsChecked = true;
        chk64bits.IsChecked = true;
        chkAtivo.IsChecked = true;
    }

    private void LoadFields()
    {
        txtNome.Text = Result.Nome;
        txtCaminho.Text = Result.Caminho;
        txtArgumentos.Text = Result.Argumentos;
        chk32bits.IsChecked = Result.x86 == "S";
        chk64bits.IsChecked = Result.x64 == "S";
        chkAtivo.IsChecked = Result.Ativo;
    }

    private bool ValidateFields()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            ShowValidationMessage("Informe o nome do programa.", txtNome);
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtCaminho.Text))
        {
            ShowValidationMessage("Informe o comando ou caminho do programa.", txtCaminho);
            return false;
        }

        if (chk32bits.IsChecked != true && chk64bits.IsChecked != true)
        {
            MessageBox.Show(
                "Selecione ao menos uma arquitetura compatível.",
                "Dados incompletos",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private static Programa CreateNewProgram() =>
        new() { Ativo = true, x64 = "S" };

    private static string BuildWingetInstallArguments(string packageId) =>
        $"install --id {packageId} -e --source winget " +
        "--accept-package-agreements --accept-source-agreements " +
        "--silent --disable-interactivity";

    private static Programa CopyProgram(Programa source) =>
        new()
        {
            Id = source.Id,
            Nome = source.Nome,
            Caminho = source.Caminho,
            Argumentos = source.Argumentos,
            x86 = source.x86,
            x64 = source.x64,
            Ativo = source.Ativo,
            StatusExecucao = source.StatusExecucao
        };

    private static void ShowValidationMessage(string message, Control control)
    {
        MessageBox.Show(message, "Dados incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
        control.Focus();
    }
}
