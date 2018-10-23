using System.Windows.Media;

namespace BetterComments.CommentsClassification
{
   internal static class CommentNames
   {
      public const string IMPORTANT_COMMENT = "Comment - Important";
      public const string QUESTION_COMMENT = "Comment - Question";
      public const string CROSSED_COMMENT = "Comment - Crossed";
      public const string TASK_COMMENT = "Comment - Task";
   }

   internal static class CommentColors
   {
      public static readonly Color ImportantColor = Colors.LimeGreen;
      public static readonly Color QuestionColor = Colors.DarkRed;
      public static readonly Color CrossedColor = Colors.Gray;
      public static readonly Color TaskColor = Colors.Yellow;
   }
}