namespace Shield.UmbracoAccess.Models
{
    public class TreeNode : Core.Models.TreeNode
    {
        public override string Id => Constants.Tree.NodeId;

        public override string Name => Constants.Tree.Title;

        public override bool HasChildNodes => false;

        public override string Icon => Constants.Tree.Icon;

        public override string RoutePath => $"{Core.Constants.App.Alias}/{Core.Constants.Tree.Alias}/{this.ConfigurationId}/{this.Id}";

        public override string ParentId => Core.Constants.Tree.RootNodeId;

        public override string ConfigurationId => nameof(UmbracoAccess);
    }
}
