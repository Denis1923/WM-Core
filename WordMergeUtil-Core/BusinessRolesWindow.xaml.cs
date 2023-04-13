using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core
{
    public partial class BusinessRolesPage : UserControl
    {
        private IQueryable<BusinessRole> _roles;

        public BusinessRolesPage()
        {
            InitializeComponent();
            ReloadRoles();
        }

        private void ReloadRoles()
        {
            try
            {
                var ctx = ((App)Application.Current).GetDBContext;
                _roles = from p in ctx.BusinessRoles orderby p.Rolename select p;
                RolesListBox.ItemsSource = _roles;
                RolesListBox.DisplayMemberPath = "rolename";
            }
            catch
            {
            }
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ctx = ((App)Application.Current).GetDBContext;
                var br = new BusinessRole() { Rolename = "Новая роль", Businessrolecode = -1, Businessroleid = Guid.NewGuid() };
                ctx.BusinessRoles.Add(br);
                ctx.SaveChanges();
                ReloadRoles();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ctx = ((App)Application.Current).GetDBContext;

                if (RolesListBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите роль из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Выбранная роль будет безвозвратно удалена. Продолжить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var role = (BusinessRole)RolesListBox.SelectedItem;
                    {
                        ctx.BusinessRoles.Remove(role);
                        ctx.SaveChanges();
                        ReloadRoles();
                    }

                    MessageBox.Show("Роль была успешно удалена", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RolesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RoleNameTextBox.IsEnabled = true;
            RoleCodeTextBox.IsEnabled = true;
            DataSourceGrid.DataContext = RolesListBox.SelectedItem;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                var ctx = ((App)Application.Current).GetDBContext;
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private static GlobalSetting GetSettings()
        {
            GlobalSetting gs;

            var ctx = ((App)Application.Current).GetDBContext;

            var gss = (from p in ctx.GlobalSettings select p);

            if (gss.Any())
                gs = gss.First();
            else
                throw new ApplicationException("Не удалось найти параметры подключения к CRM");

            return gs;
        }

        private void SyncWithCRM_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка синхронизации с CRM устарела!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveRoles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ctx = ((App)Application.Current).GetDBContext;
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }
    }
}
