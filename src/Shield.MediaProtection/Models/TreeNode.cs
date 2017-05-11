using System;

namespace Shield.MediaProtection.Models
{
    public class TreeNode : Core.Models.TreeNode
    {
        public override bool HasChildNodes => false;

        public override string Icon => "";

        public override string Id => "-323232";

        public override string Name => "Media Protection";

        public override string ParentId => Core.Constants.Tree.RootNodeId;

        public override string RoutePath => "";
    }
}
