using FluentDataGrid.Core.ColumnBasedTable;
using Shiva;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace FluentDataGrid.Sample
{
    class ViewModel : INotifyPropertyChanged
    {
        DataGridCellInfo currentCell;
        int selectedIndex;
        object selectedItem;

        public ICommand Command { get; set; }

        public Table Table { get; set; }
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                OnPropertyChanged();
            }
        }
        public DataGridCellInfo CurrentCell
        {
            get { return currentCell; }
            set
            {
                currentCell = value;
                OnPropertyChanged();
                //SelectedIndex = Table.IndexOf(currentCell.Item as Row);
            }
        }
        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        public ViewModel()
        {
            Command = new RelayCommand(x => { SelectedItem=null; SelectedIndex = 2; });

            Table = new Table(
                new ColumnPropertyDescriptor("The Good Row", typeof(DateTime)),
                new ColumnPropertyDescriptor("The Bad Row", typeof(float), false, 4),
                new ColumnPropertyDescriptor("The Ugly Row", typeof(float)));


            for (int i = 0; i < 10; i++)
                Table.Add(new Row(DateTime.Now + TimeSpan.FromMinutes(i), i, i * i));

            Table.CollectionChanged += Table_CollectionChanged;
        }

        void Table_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (var i in e.NewItems)
                {
                    (i as Row)[0] = DateTime.Now;
                }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
