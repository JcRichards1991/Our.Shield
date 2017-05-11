using System;

namespace Shield.UmbracoAccess.Models
{
    public class TreeNode : Core.Models.TreeNode
    {
        public override string Id => "-313131";

        public override string Name => "Umbraco Access";

        public override bool HasChildNodes => false;

        public override string Icon => "";

        public override string RoutePath => "";

        public override string ParentId => Core.Constants.Tree.RootNodeId;
    }
}
