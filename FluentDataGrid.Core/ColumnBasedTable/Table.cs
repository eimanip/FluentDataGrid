using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentDataGrid.Core.ColumnBasedTable
{
    public class Table : ObservableCollection<Row>, ITypedList
    {
        public PropertyDescriptorCollection PropertyDescriptors { get; private set; }

        public Table(params ColumnPropertyDescriptor[] propertyDescriptors)
        {
            if (propertyDescriptors == null)
                throw new ArgumentNullException(nameof(propertyDescriptors));

            for (int i = 0; i < propertyDescriptors.Length; i++)
                propertyDescriptors[i].ColumnIndex = i;

            PropertyDescriptors = new PropertyDescriptorCollection(propertyDescriptors);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (var i in e.NewItems)
                    (i as Row)?.SetTable(this);

            base.OnCollectionChanged(e);
        }

        #region ITypedList

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return PropertyDescriptors;
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return null;
        }

        #endregion
    }
}
