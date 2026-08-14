using ProgramInstaller.Controllers;
using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProgramInstaller;

public partial class WingetCatalogWindow : Window
{
    private readonly WingetCatalogService _catalogService = new();
    private CancellationTokenSource? _searchCancellation;

    public WingetPackage? Result { get; private set; }

    public WingetCatalogWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => txtPesquisa.Focus();
        Closed += (_, _) => _searchCancellation?.Cancel();
    }

    private async void btnPesquisar_Click(object sender, RoutedEventArgs e) =>
        await SearchAsync();

    private async void txtPesquisa_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        await SearchAsync();
    }

    private void btnEscolher_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: WingetPackage package })
            return;

        Result = package;
        DialogResult = true;
    }

    private async Task SearchAsync()
    {
        string query = txtPesquisa.Text.Trim();

        if (query.Length < 2)
        {
            ShowState("Digite pelo menos 2 caracteres.");
            txtPesquisa.Focus();
            return;
        }

        _searchCancellation?.Cancel();
        _searchCancellation = new CancellationTokenSource();
        SetBusy(true);
        ShowState("Consultando o catálogo do WinGet…");

        try
        {
            IReadOnlyList<WingetPackage> packages = await _catalogService.SearchAsync(
                query,
                _searchCancellation.Token);

            lstResultados.ItemsSource = packages;
            emptyState.Visibility = packages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            txtEstado.Text = packages.Count == 0
                ? "Nenhum programa encontrado. Tente outro nome."
                : string.Empty;
        }
        catch (OperationCanceledException)
        {
        }
        catch (WingetCatalogUnavailableException ex)
        {
            ShowState(ex.Message);
        }
        catch (Exception ex)
        {
            ShowState($"Não foi possível consultar o catálogo.\n{ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        txtPesquisa.IsEnabled = !isBusy;
        btnPesquisar.IsEnabled = !isBusy;
        btnPesquisar.Content = isBusy ? "Buscando…" : "Pesquisar";
    }

    private void ShowState(string message)
    {
        lstResultados.ItemsSource = null;
        txtEstado.Text = message;
        emptyState.Visibility = Visibility.Visible;
    }
}
