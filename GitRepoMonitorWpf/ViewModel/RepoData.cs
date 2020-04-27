using System.ComponentModel;

namespace GitRepoMonitorWpf.ViewModel
{
	//[Notify]
	public class RepoData : INotifyPropertyChanged
	{
		public string RepoName { get { return repoName; } set { SetProperty(ref repoName, value, repoNamePropertyChangedEventArgs); } }
		public string RepoOrigin { get { return repoOrigin; } set { SetProperty(ref repoOrigin, value, repoOriginPropertyChangedEventArgs); } }
		public string LocalPath { get { return localPath; } set { SetProperty(ref localPath, value, localPathPropertyChangedEventArgs); } }
		public string CurrentBranch { get { return currentBranch; } set { SetProperty(ref currentBranch, value, currentBranchPropertyChangedEventArgs); } }
		public int? AheadBy { get { return aheadBy; } set { SetProperty(ref aheadBy, value, aheadByPropertyChangedEventArgs); } }
		public int? BehindBy { get { return behindBy; } set { SetProperty(ref behindBy, value, behindByPropertyChangedEventArgs); } }

		#region NotifyPropertyChangedGenerator

		public event PropertyChangedEventHandler PropertyChanged;

		private string repoName;
		private static readonly PropertyChangedEventArgs repoNamePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(RepoName));
		private string repoOrigin;
		private static readonly PropertyChangedEventArgs repoOriginPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(RepoOrigin));
		private string localPath;
		private static readonly PropertyChangedEventArgs localPathPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(LocalPath));
		private string currentBranch;
		private static readonly PropertyChangedEventArgs currentBranchPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(CurrentBranch));
		private int? aheadBy;
		private static readonly PropertyChangedEventArgs aheadByPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(AheadBy));
		private int? behindBy;
		private static readonly PropertyChangedEventArgs behindByPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(BehindBy));

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
