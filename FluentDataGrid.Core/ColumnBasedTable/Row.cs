using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentDataGrid.Core.ColumnBasedTable
{
    public class Row : CustomTypeDescriptor, INotifyPropertyChanged
    {
        Table table;
        object[] data;

        internal void SetTable(Table table)
        {
            if (this.table != null)
                if (this.table == table) return;
                else
                    throw new InvalidOperationException("Table of a row can not be set twice.");

            this.table = table;
            if (data == null)
            {
                data = new object[table.PropertyDescriptors.Count];
                foreach (var _d in table.PropertyDescriptors)
                {
                    var d = (ColumnPropertyDescriptor)_d;
                    data[d.ColumnIndex] = d.DefaultValue;
                }
            }

            if (data.Length != table.PropertyDescriptors.Count)
                throw new InvalidOperationException("Mismach length of data and Table.PropertyDescriptors");

        }

        public Row(params object[] values)
        {
            data = values;
        }

        public Row() { }

        public object this[int col]
        {
            get
            {
                // TODO: remove this if statement. For some reason which I couldn't figure out, sometimes it called with a invalid value of 'col'
                if (col >= data.Length) return 0;
                return data[col];
            }
            set
            {
                if (data[col] == value) return;
                data[col] = value;
                OnPropertyChanged(table.PropertyDescriptors[col].Name);
            }
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return table.PropertyDescriptors;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
