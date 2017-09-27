using System;

namespace BetterComments.CommentsTagging
{
    internal class CommentIgnoreAttribute : Attribute { }

    internal class CommentDefaultAttribute : Attribute
    {
        public String Value { get; private set; }

        public CommentDefaultAttribute(String value)
        {
            Value = value;
        }
    }
}