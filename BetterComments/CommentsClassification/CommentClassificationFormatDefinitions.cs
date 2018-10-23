using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace BetterComments.CommentsClassification
{
   [Export(typeof(EditorFormatDefinition))]
   [ClassificationType(ClassificationTypeNames = CommentNames.IMPORTANT_COMMENT)]
   [Name(CommentNames.IMPORTANT_COMMENT)]
   [UserVisible(true)]
   [Order(After = Priority.High)]
   public sealed class ImportantCommentFormat : ClassificationFormatDefinition
   {
      public ImportantCommentFormat()
      {
         DisplayName = CommentNames.IMPORTANT_COMMENT;
         ForegroundColor = CommentColors.ImportantColor;
      }
   }

   [Export(typeof(EditorFormatDefinition))]
   [ClassificationType(ClassificationTypeNames = CommentNames.QUESTION_COMMENT)]
   [Name(CommentNames.QUESTION_COMMENT)]
   [UserVisible(true)]
   [Order(After = Priority.High)]
   public sealed class QuestionCommentFormat : ClassificationFormatDefinition
   {
      public QuestionCommentFormat()
      {
         DisplayName = CommentNames.QUESTION_COMMENT;
         ForegroundColor = CommentColors.QuestionColor;
      }
   }

   [Export(typeof(EditorFormatDefinition))]
   [ClassificationType(ClassificationTypeNames = CommentNames.CROSSED_COMMENT)]
   [Name(CommentNames.CROSSED_COMMENT)]
   [UserVisible(true)]
   [Order(After = Priority.High)]
   public sealed class CrossedCommentFormat : ClassificationFormatDefinition
   {
      public CrossedCommentFormat()
      {
         DisplayName = CommentNames.CROSSED_COMMENT;
         ForegroundColor = CommentColors.CrossedColor;
         TextDecorations = System.Windows.TextDecorations.Strikethrough;
      }
   }

   [Export(typeof(EditorFormatDefinition))]
   [ClassificationType(ClassificationTypeNames = CommentNames.TASK_COMMENT)]
   [Name(CommentNames.TASK_COMMENT)]
   [UserVisible(true)]
   [Order(After = Priority.High)]
   public sealed class TaskCommentFormat : ClassificationFormatDefinition
   {
      public TaskCommentFormat()
      {
         DisplayName = CommentNames.TASK_COMMENT;
         ForegroundColor = CommentColors.TaskColor;
      }
   }
}