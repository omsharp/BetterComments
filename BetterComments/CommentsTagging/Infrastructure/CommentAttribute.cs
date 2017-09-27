using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterComments.CommentsTagging {
    internal class CommentIgnoreAttribute : Attribute { }

    internal class CommentDefaultAttribute : Attribute 
    {
        public String Value { get; private set; }

        public CommentDefaultAttribute(String value) {
            Value = value;
        }

    }
}
