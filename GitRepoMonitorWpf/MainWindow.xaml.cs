using GitRepoMonitorWpf.Model;
using GitRepoMonitorWpf.ViewModel;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Alm.Authentication;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace GitRepoMonitorWpf
{
	/// <summary>MainWindow</summary>
	public partial class MainWindow : Window
	{
		public GitRepoMonitorData Data { get; set; }
		MainModel mainModel = new MainModel();

		public MainWindow()
		{
			Data = new GitRepoMonitorData();

			InitializeComponent();
		}

		void LoadFile(string filePath)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(MainModel));
			StreamReader reader = new StreamReader(filePath);
			mainModel = (MainModel)serializer.Deserialize(reader);
			reader.Close();

			Data.TreeData.Clear();

			foreach (var parentSet in mainModel.RepoSetModels)
			{
				RepoSet repoSet = new RepoSet() { Name = parentSet.Name };
				foreach (var repoPath in parentSet.RepoPaths)
				{
					AddRepo(repoSet, repoPath);
				}
				Data.TreeData.Add(repoSet);
			}
			Properties.Settings.Default.LastFileUsed = filePath;
			Properties.Settings.Default.Save();
		}

		void SaveFile(string filePath)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(MainModel));
			Stream fs = new FileStream(filePath, FileMode.Create);
			XmlTextWriter writer = new XmlTextWriter(fs, Encoding.Unicode);
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 4;
			serializer.Serialize(writer, mainModel);
			writer.Close();
		}

		private void AddRepo(RepoSet repoSet, string path)
		{
			try
			{
				using (var repo = new Repository(path))
				{
					string remoteSource = "<none>";
					if (repo.Network.Remotes.Any())
						remoteSource = repo.Network.Remotes.First().Url.ToString();
					string repoName;
					if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
						repoName = Path.GetFileName(Path.GetDirectoryName(path));
					else
						repoName = Path.GetFileName(path.TrimEnd('\\'));

					var newRepo = new RepoData() { RepoName = repoName, CurrentBranch = repo.Head.FriendlyName, LocalPath = path, RepoOrigin = remoteSource, AheadBy = repo.Head.TrackingDetails.AheadBy, BehindBy = repo.Head.TrackingDetails.BehindBy };
					repoSet.Members.Add(newRepo);
				}
			}
			catch (RepositoryNotFoundException exc)
			{
				Debug.WriteLine($"Path not a GIT repo: {path}");
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc);
			}
		}

		private async void Window_Loaded(object sender, RoutedEventArgs rea)
		{
			try
			{
				if (File.Exists(Properties.Settings.Default.LastFileUsed))
				{
					LoadFile(Properties.Settings.Default.LastFileUsed);
					await RefreshGitInfo();
				}
			}
			catch (FileNotFoundException exc)
			{
				Debug.WriteLine(exc);
				MessageBox.Show(this, exc.ToString());
			}
		}

		protected override void OnClosing(CancelEventArgs cea)
		{
			Properties.Settings.Default.Save();
			base.OnClosing(cea);
		}

		private void NewDocumentCmdExecuted(object sender, ExecutedRoutedEventArgs erea)
		{
			Data.TreeData.Clear();
		}

		private void NewDocumentCanExecute(object sender, CanExecuteRoutedEventArgs cerea)
		{
			cerea.CanExecute = true;
		}

		private void NewSetExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Data.TreeData.Add(new RepoSet { Name = "Placeholder" });
		}

		private void NewSetCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void RefreshCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private async void RefreshCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			await RefreshGitInfo();
		}

		private void LoadCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void LoadCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			openFileDialog.Filter = "RepoSet files (*.RepoSet)|*.RepoSet|xml files (*.xml)|*.xml|All files (*.*)|*.*";
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;

			if (openFileDialog.ShowDialog(this) == true)
			{
				LoadFile(openFileDialog.FileName);
			}
		}

		private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();

			saveFileDialog.Filter = "RepoSet files (*.RepoSet)|*.RepoSet|xml files (*.xml)|*.xml|All files (*.*)|*.*";
			saveFileDialog.FilterIndex = 1;
			saveFileDialog.RestoreDirectory = true;

			if (saveFileDialog.ShowDialog(this) == true)
			{
				SaveFile(saveFileDialog.FileName);
			}
		}

		private async Task RefreshGitInfo()
		{
			await Task.Factory.StartNew(() =>
			{ 
				Data.IsLoading = true;
				foreach (RepoSet set in Data.TreeData)
				{
					foreach (var data in set.Members)
					{
						using (var repo = new Repository(data.LocalPath))
						{
							//FetchOptions options = new FetchOptions();
							//options.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
							//	new UsernamePasswordCredentials()
							//	{
							//		Username = "blah",
							//		Password = "<INSERT YOUR AZURE PAT HERE>"
							//	});

							foreach (Remote remote in repo.Network.Remotes)
							{
								IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

								FetchOptions options = new FetchOptions();
								options.CredentialsProvider = CreateCredentialsHandler(repo, remote.Name);

								try
								{
									Commands.Fetch(repo, remote.Name, refSpecs, options, "logMessage");

									string remoteSource = "<none>";
									if (repo.Network.Remotes.Any())
										remoteSource = repo.Network.Remotes.First().Url.ToString();

									data.CurrentBranch = repo.Head.FriendlyName;
									data.RepoOrigin = remoteSource;
									data.AheadBy = repo.Head.TrackingDetails.AheadBy;
									data.BehindBy = repo.Head.TrackingDetails.BehindBy;
								}
								catch (Exception exc)
								{
									Debug.WriteLine(exc);
								}
							}
						}
					}
				}
				Data.IsLoading = false;
			});
		}

		static CredentialsHandler CreateCredentialsHandler(IRepository repo, string remoteName)
		{
			CredentialsHandler retval = null;

			var remote = repo.Network.Remotes[remoteName];
			var remoteUri = new Uri(remote.Url);

			var secrets = new SecretStore("git");
			var auth = new BasicAuthentication(secrets);

			var targetUrl = remoteUri.GetLeftPart(UriPartial.Authority);

			//HACK ALERT!! to make this thing play nice with Windows Credentials and how it stores my PATs. YMMV
			if (targetUrl.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase))
			{
				var url = new Uri(targetUrl);
				if (!string.IsNullOrWhiteSpace(url.UserInfo))
				{
					targetUrl = url + url.UserInfo;
				}
			}
			//End HACK ALERT!!

			var creds = auth.GetCredentials(new TargetUri(targetUrl));
			if (creds != null)
			{
				retval = (url, user, cred) => new UsernamePasswordCredentials
				{
					Username = creds.Username,
					Password = creds.Password
				};
			}

			return retval;
		}

		private void MainTree_Drop(object sender, DragEventArgs e)
		{
			//find the target folder - the top node of the tree
			TreeViewItem hitTarget;// = dea.OriginalSource;

			TreeViewItem treeViewItem;
			if (e.OriginalSource is System.Windows.Documents.Run)
				hitTarget = VisualUpwardSearch(((System.Windows.Documents.Run)e.OriginalSource).Parent as DependencyObject);
			else
				hitTarget = VisualUpwardSearch(e.OriginalSource as DependencyObject);

			if (hitTarget == null)
			{
				//you must have dropped it in the main "grid" of the tree
				//lets ASSume the "selected" folder, or maybe the top folder?
				hitTarget = MainTree.SelectedItem as TreeViewItem;
			}

			if (hitTarget != null)
			{

				if (hitTarget.DataContext is RepoData)
					hitTarget = VisualUpwardSearch(VisualTreeHelper.GetParent(hitTarget as DependencyObject));

				if (hitTarget.DataContext is RepoSet)
				{
					RepoSet repoSet = (RepoSet)hitTarget.DataContext;

					string[] names = (string[])e.Data.GetData(DataFormats.FileDrop);
					foreach (var name in names)
					{
						if (Directory.Exists(name))
						{
							AddRepo(repoSet, name);
						}
						else
						{
							Debug.WriteLine($"File name {name} NOT handled");
						}
					}
				}

				e.Handled = true;
				Debug.WriteLine("TEST");
				return;
			}
			else
			{
				Debug.WriteLine("NOOOOOOOOOOOOOOOOOOOOOOO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				return;
			}
		}

		private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
		{
			// Search the VisualTree for specified type
			while (current != null)
			{
				if (current is T)
				{
					return (T)current;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			return null;
		}

		static DependencyObject VisualUpwardSearch1<T>(DependencyObject source)
		{
			while (source != null && source.GetType() != typeof(T))
				source = VisualTreeHelper.GetParent(source);

			return source;
		}

		static TreeViewItem VisualUpwardSearch(DependencyObject source)
		{
			while (source != null && source.GetType() != typeof(TreeViewItem))
				source = VisualTreeHelper.GetParent(source);
			return source as TreeViewItem;
		}

		private void MainTree_DragOver(object sender, DragEventArgs e)
		{
			HandleDragFeedback(e);
		}

		private void MainTree_DragEnter(object sender, DragEventArgs e)
		{
			HandleDragFeedback(e);
		}

		private static void HandleDragFeedback(DragEventArgs e)
		{
			e.Effects = DragDropEffects.None;

			bool validDropType = false;
			string[] names = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach (var name in names)
			{
				if (Directory.Exists(name))
				{
					validDropType = true;   //at least 1 folder in the drop list
					break;
				}
			}

			if (validDropType)
			{
				TreeViewItem hitTarget;
				if (e.OriginalSource is System.Windows.Documents.Run)
					hitTarget = VisualUpwardSearch(((System.Windows.Documents.Run)e.OriginalSource).Parent as DependencyObject);
				else
					hitTarget = VisualUpwardSearch(e.OriginalSource as DependencyObject);

				if (hitTarget != null)
				{
					if (hitTarget.DataContext is RepoData)
						hitTarget = VisualUpwardSearch(VisualTreeHelper.GetParent(hitTarget as DependencyObject));
					else if (!(hitTarget.DataContext is RepoSet))
						hitTarget = null;

					if (hitTarget != null)
					{
						e.Effects = DragDropEffects.Copy;
						hitTarget.IsSelected = true;
					}
				}
			}
			e.Handled = true;
		}
	}
}
