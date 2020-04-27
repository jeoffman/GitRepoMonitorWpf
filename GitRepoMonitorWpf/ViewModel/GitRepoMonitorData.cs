using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GitRepoMonitorWpf.ViewModel
{
    //[Notify]
    public class GitRepoMonitorData : INotifyPropertyChanged
    {
        public ObservableCollection<RepoSet> TreeData { get { return treeData; } set { SetProperty(ref treeData, value, treeDataPropertyChangedEventArgs); } }
        public bool IsLoading { get { return isLoading; } set { SetProperty(ref isLoading, value, isLoadingPropertyChangedEventArgs); } }

        public GitRepoMonitorData()
        {
            TreeData = new ObservableCollection<RepoSet>();
        }

        #region NotifyPropertyChangedGenerator

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<RepoSet> treeData;
        private static readonly PropertyChangedEventArgs treeDataPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(TreeData));
        private bool isLoading;
        private static readonly PropertyChangedEventArgs isLoadingPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(IsLoading));

        private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, ev);
            }
        }

        #endregion
    }
}
