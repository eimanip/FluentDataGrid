using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FluentDataGrid.Core
{
    public class FluentDataGrid : DataGrid
    {
        bool canLeaveEditing;

        public FluentDataGrid()
        {
            // https://blogs.msdn.microsoft.com/wpfsdk/2007/06/08/defining-and-using-shared-resources-in-a-custom-control-library/
            Uri resourceLocater = new Uri("/FluentDataGrid.Core;component/FluentDataGridStyle.xaml", UriKind.Relative);
            var rd = (ResourceDictionary)Application.LoadComponent(resourceLocater);
            Resources.MergedDictionaries.Add(rd);
        }

        protected override void OnBeginningEdit(DataGridBeginningEditEventArgs e)
        {
            canLeaveEditing = e.EditingEventArgs != null && e.EditingEventArgs.RoutedEvent.Name == "TextInput";
            base.OnBeginningEdit(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                if (canLeaveEditing) CommitEdit();
                //e.Handled = false; // by purpose
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat =
                    CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

            base.OnAutoGeneratingColumn(e);
        }

        #region Clipboard Paste

        // https://stackoverflow.com/questions/4118617/wpf-datagrid-pasting/5436437

        public event ExecutedRoutedEventHandler ExecutePasteEvent;
        public event CanExecuteRoutedEventHandler CanExecutePasteEvent;

        static FluentDataGrid()
        {
            CommandManager.RegisterClassCommandBinding(
              typeof(FluentDataGrid),
              new CommandBinding(ApplicationCommands.Paste,
                  new ExecutedRoutedEventHandler(OnExecutedPasteInternal),
                  new CanExecuteRoutedEventHandler(OnCanExecutePasteInternal)));

            //DefaultStyleKeyProperty.OverrideMetadata(
            //    typeof(FluentDataGrid),
            //    new FrameworkPropertyMetadata(typeof(FluentDataGrid)));
        }

        static void OnCanExecutePasteInternal(object target, CanExecuteRoutedEventArgs args)
        {
            ((FluentDataGrid)target).OnCanExecutePaste(target, args);
        }

        /// <summary>
        /// This virtual method is called when ApplicationCommands.Paste command query its state.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCanExecutePaste(object target, CanExecuteRoutedEventArgs args)
        {
            if (CanExecutePasteEvent != null)
            {
                CanExecutePasteEvent(target, args);
                if (args.Handled)
                {
                    return;
                }
            }

            args.CanExecute = CurrentCell != null;
            args.Handled = true;
        }

        static void OnExecutedPasteInternal(object target, ExecutedRoutedEventArgs args)
        {
            ((FluentDataGrid)target).OnExecutedPaste(target, args);
        }

        /// <summary>
        /// This virtual method is called when ApplicationCommands.Paste command is executed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="args"></param>
        protected virtual void OnExecutedPaste(object target, ExecutedRoutedEventArgs args)
        {
            if (ExecutePasteEvent != null)
            {
                ExecutePasteEvent(target, args);
                if (args.Handled)
                {
                    return;
                }
            }

            // parse the clipboard data            
            List<string[]> rowData = ClipboardHelper.ParseClipboardData();

            bool hasAddedNewRow = false;

            // call OnPastingCellClipboardContent for each cell
            int minRowIndex = Items.IndexOf(CurrentItem);
            int maxRowIndex = Items.Count - 1;
            int minColumnDisplayIndex = (SelectionUnit != DataGridSelectionUnit.FullRow) ? Columns.IndexOf(CurrentColumn) : 0;
            int maxColumnDisplayIndex = Columns.Count - 1;
            int rowDataIndex = 0;
            for (int i = minRowIndex; i <= maxRowIndex && rowDataIndex < rowData.Count; i++, rowDataIndex++)
            {
                if (i < Items.Count)
                {
                    CurrentItem = Items[i];

                    BeginEditCommand.Execute(null, this);

                    int columnDataIndex = 0;
                    for (int j = minColumnDisplayIndex; j <= maxColumnDisplayIndex && columnDataIndex < rowData[rowDataIndex].Length; j++, columnDataIndex++)
                    {
                        DataGridColumn column = ColumnFromDisplayIndex(j);
                        column.OnPastingCellClipboardContent(Items[i], rowData[rowDataIndex][columnDataIndex]);

                        //column.OnPastingCellClipboardContent(
                    }

                    CommitEditCommand.Execute(this, this);
                    if (i == maxRowIndex)
                    {
                        maxRowIndex++;
                        hasAddedNewRow = true;
                    }
                }
            }

            // update selection
            if (hasAddedNewRow)
            {
                UnselectAll();
                UnselectAllCells();

                CurrentItem = Items[minRowIndex];

                if (SelectionUnit == DataGridSelectionUnit.FullRow)
                {
                    SelectedItem = Items[minRowIndex];
                }
                else if (SelectionUnit == DataGridSelectionUnit.CellOrRowHeader ||
                         SelectionUnit == DataGridSelectionUnit.Cell)
                {
                    SelectedCells.Add(new DataGridCellInfo(Items[minRowIndex], Columns[minColumnDisplayIndex]));

                }
            }
        }

        /// <summary>
        ///     Whether the end-user can add new rows to the ItemsSource.
        /// </summary>
        public bool CanUserPasteToNewRows
        {
            get { return (bool)GetValue(CanUserPasteToNewRowsProperty); }
            set { SetValue(CanUserPasteToNewRowsProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for CanUserAddRows.
        /// </summary>
        public static readonly DependencyProperty CanUserPasteToNewRowsProperty =
            DependencyProperty.Register("CanUserPasteToNewRows",
                                        typeof(bool), typeof(FluentDataGrid),
                                        new FrameworkPropertyMetadata(true, null, null));

        #endregion Clipboard Paste
    }
}
