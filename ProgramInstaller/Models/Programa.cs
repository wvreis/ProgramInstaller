using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ProgramInstaller.Models;

public sealed class Programa : INotifyPropertyChanged
{
    private string _nome = string.Empty;
    private string _caminho = string.Empty;
    private string _argumentos = string.Empty;
    private string _x86 = "N";
    private string _x64 = "N";
    private bool _ativo = true;
    private string _statusExecucao = "Não executado";

    public string Nome
    {
        get => _nome;
        set => SetField(ref _nome, value ?? string.Empty);
    }

    public string Caminho
    {
        get => _caminho;
        set => SetField(ref _caminho, value ?? string.Empty);
    }

    public string Argumentos
    {
        get => _argumentos;
        set => SetField(ref _argumentos, value ?? string.Empty);
    }

    public string x86
    {
        get => _x86;
        set
        {
            if (SetField(ref _x86, value ?? "N"))
                OnPropertyChanged(nameof(Arquiteturas));
        }
    }

    public string x64
    {
        get => _x64;
        set
        {
            if (SetField(ref _x64, value ?? "N"))
                OnPropertyChanged(nameof(Arquiteturas));
        }
    }

    public bool Ativo
    {
        get => _ativo;
        set => SetField(ref _ativo, value);
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    [XmlIgnore]
    public string Arquiteturas
    {
        get
        {
            List<string> arquiteturas = [];

            if (x86 == "S")
                arquiteturas.Add("x86");

            if (x64 == "S")
                arquiteturas.Add("x64");

            return arquiteturas.Count == 0 ? "Não definida" : string.Join(" / ", arquiteturas);
        }
    }

    [XmlIgnore]
    public string StatusExecucao
    {
        get => _statusExecucao;
        set => SetField(ref _statusExecucao, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
