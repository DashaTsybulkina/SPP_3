using AssenblyBrowserLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace AssemblyBrowser
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private string _openFile;
        public ObservableCollection<AssemblyTreeNode> NamespaceNodes { get; set; }
        public string OpenFile
        {
            get => _openFile;
            set
            {
                _openFile = value;
                NamespaceNodes = null;
                try
                {
                    var assemblyParser = new AssemblyParser();
                    var info = assemblyParser.GetAssemblyInfo(_openFile);
                    NamespaceNodes = new ObservableCollection<AssemblyTreeNode>(info.ChildNodes);
                    OnPropertyChanged(nameof(NamespaceNodes));
                }
                catch (Exception e)
                {
                    _openFile = e.Message;
                    Console.WriteLine(_openFile);
                }
                OnPropertyChanged(nameof(NamespaceNodes));

            }
        }

        private RelayCommand _openAssemblyCommand;
        public RelayCommand OpenAssembly
        {
            get
            {
                return _openAssemblyCommand ??= new RelayCommand(_ =>
                    {
                        var fileDialog = new OpenFileDialog
                        {
                            Filter = "Assemblies|*.dll;*.exe",
                            Title = "Select assembly",
                            Multiselect = false
                        };

                        var isOpen = fileDialog.ShowDialog();

                        if (isOpen != null && isOpen.Value)
                        {

                            OpenFile = fileDialog.FileName;
                            OnPropertyChanged(nameof(OpenFile));
                        }
                    });
            }
        }

    }
}
