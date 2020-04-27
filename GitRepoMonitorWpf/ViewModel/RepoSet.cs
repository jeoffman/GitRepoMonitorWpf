using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GitRepoMonitorWpf.ViewModel
{
	//[Notify]
	public class RepoSet : INotifyPropertyChanged
	{
		public RepoSet()
		{
			Members = new ObservableCollection<RepoData>();
		}

		public string Name { get { return name; } set { SetProperty(ref name, value, namePropertyChangedEventArgs); } }

		public ObservableCollection<RepoData> Members { get { return members; } set { SetProperty(ref members, value, membersPropertyChangedEventArgs); } }

		#region NotifyPropertyChangedGenerator

		public event PropertyChangedEventHandler PropertyChanged;

		private string name;
		private static readonly PropertyChangedEventArgs namePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Name));
		private ObservableCollection<RepoData> members;
		private static readonly PropertyChangedEventArgs membersPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Members));

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
