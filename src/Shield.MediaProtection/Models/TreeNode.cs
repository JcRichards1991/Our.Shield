using System;

namespace Shield.MediaProtection.Models
{
    public class TreeNode : Core.Models.TreeNode
    {
        public override string Id => "-323232";

        public override string Name => "Media Protection";

        public override bool HasChildNodes => false;

        public override string Icon => "icon-shield";

        public override string RoutePath => $"{Core.Constants.App.Alias}/{Core.Constants.Tree.Alias}/{this.ConfigurationId}/{this.Id}";

        public override string ParentId => Core.Constants.Tree.RootNodeId;

        public override string ConfigurationId => nameof(MediaProtection);
    }
}
