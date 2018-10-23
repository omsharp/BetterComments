using System;
using System.Windows.Input;

namespace BetterComments.Options
{
   public class RelayCommand : ICommand
   {
      private Action execute;
      private Func<bool> canExecute;

      public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
      {
         this.execute = methodToExecute;
         this.canExecute = canExecuteEvaluator;
      }

      public event EventHandler CanExecuteChanged
      {
         add { CommandManager.RequerySuggested += value; }
         remove { CommandManager.RequerySuggested -= value; }
      }

      public RelayCommand(Action methodToExecute)
          : this(methodToExecute, null)
      {
      }

      public bool CanExecute(object parameter)
      {
         return canExecute == null
             ? true
             : canExecute.Invoke();
      }

      public void Execute(object parameter)
      {
         execute.Invoke();
      }
   }
}