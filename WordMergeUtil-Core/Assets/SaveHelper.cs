using System;
using System.Windows;
using WordMergeEngine.Models;
using WordMergeEngine;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WordMergeUtil_Core.Assets
{
    public class SaveHelper
    {
        public DataModel Context { get; set; }

        public SaveHelper(DataModel context)
        {
            Context = context;
        }

        public bool CheckSavingBeforeAction()
        {
            if (!CheckIsModified())
                return false;

            var res = MessageBox.Show("Настраиваемый шаблон был изменен, вы хотите сохранить изменения?", "Подтвердите действие", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (res == MessageBoxResult.Cancel)
            {
                return true;
            }
            else if (res == MessageBoxResult.Yes)
                DocBuilder.SaveChanges(Context, null, null);
            else if (res == MessageBoxResult.No)
                RollbackChanges();

            return false;
        }

        public bool CheckIsModified()
        {
            var entries = Context.ChangeTracker.Entries().Where(x => new[] { EntityState.Modified | EntityState.Deleted | EntityState.Added }.Contains(x.State));

            if (entries.Any(x => x.State == EntityState.Deleted || x.State == EntityState.Added))
                return true;

            foreach (var entry in entries.Where(x => x.State == EntityState.Modified))
            {
                foreach (var propName in entry.GetDatabaseValues().Properties)
                {
                    if (entry.OriginalValues[propName].ToString() != entry.CurrentValues[propName].ToString())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void RollbackChanges()
        {
            var collection = Context.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged);

            foreach (var e in collection)
            {
                switch (e.State)
                {
                    case EntityState.Modified:
                        e.CurrentValues.SetValues(e.OriginalValues);
                        e.State = EntityState.Unchanged;
                        break;

                    case EntityState.Added:
                        e.State = EntityState.Detached;
                        break;

                    case EntityState.Deleted:
                        e.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
