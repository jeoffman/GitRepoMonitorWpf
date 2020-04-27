using System.Collections.Generic;

namespace GitRepoMonitorWpf.Model
{
    public class RepoSetModel
    {
        public string Name { get; set; }
        public List<string> RepoPaths { get; set; }
    }
}
