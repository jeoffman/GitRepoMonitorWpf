using System.Windows.Input;

namespace GitRepoMonitorWpf
{
	public static class MainWindowCommands
	{
		public static readonly RoutedUICommand NewSet = new RoutedUICommand
			(
				"NewSet",
				"NewSet",
				typeof(MainWindow),
				new InputGestureCollection(){}
			);
		public static readonly RoutedUICommand Refresh = new RoutedUICommand
			(
				"Refresh",
				"Refresh",
				typeof(MainWindow),
				new InputGestureCollection()
				{
					new KeyGesture(Key.F5, ModifierKeys.Control)
				}
			);
		public static readonly RoutedUICommand Exit = new RoutedUICommand
			(
				"Exit",
				"Exit",
				typeof(MainWindow),
				new InputGestureCollection()
				{
					new KeyGesture(Key.F4, ModifierKeys.Alt)
				}
			);
	}
}
