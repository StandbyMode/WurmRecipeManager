using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WurmRecipeManager
{
    public class AddIngredientCommand : ICommand
    {
        private MainWindow Parent;
        public AddIngredientCommand(MainWindow parent)
        {
            Parent = parent;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        void ICommand.Execute(object parameter)
        {
            Parent.AddWorkbenchIngredient(this, null);
        }
    }
}
