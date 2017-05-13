using System;
using System.Collections.Generic;
using System.Linq;
using BetterComments.CommentsClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using BetterComments.Options;
using Microsoft.VisualStudio.Text.Editor;

namespace BetterComments.CommentsTagging
{
   internal enum CommentType
   {
      Normal,
      Important,
      Question,
      Crossed,
      Task
   }

   internal class CommentTagger : ITagger<ClassificationTag>, IDisposable
   {
      private readonly Settings settings = new Settings();

      private readonly IClassificationTypeRegistryService classificationRegistry;
      private readonly ITagAggregator<IClassificationTag> tagAggregator;

      private static readonly string[] singleLineCommentStarters = { "//", "'", "#" };

      private static readonly string[] delimitedCommentStarters = { "/*", "(*" };

      private static readonly Dictionary<string, string> delimitedCommentEnders
          = new Dictionary<string, string> { { "/*", "*/" }, { "(*", "*)" } };

      public static CommentTagger Create(ITextView view,
                                      IClassificationTypeRegistryService reg,
                                      ITagAggregator<IClassificationTag> agg)
      {
         return view.Properties.GetOrCreateSingletonProperty(() => new CommentTagger(reg, agg));
      }

      private CommentTagger(IClassificationTypeRegistryService reg,
                            ITagAggregator<IClassificationTag> agg)
      {
         classificationRegistry = reg;
         tagAggregator = agg;
         
         SettingsStore.LoadSettings(settings);
         SettingsStore.SettingsChanged += OnSettingsChanged;
      }

      private void OnSettingsChanged()
      {
         SettingsStore.LoadSettings(settings);
      }

#pragma warning disable 0067
      public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore 0067

      public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
      {
         var snapshot = spans[0].Snapshot;

         if (!snapshot.ContentType.IsOfType("code")) yield break;

         // Work through all comment tags associated with the passed spans. Ignore xml docs.
         foreach (var tagSpan in tagAggregator.GetTags(spans).Where(m => IsComment(m.Tag) && !IsXmlDoc(m.Tag)))
         {
            // Get all the spans associated with the current tag, mapped to our snapshot
            foreach (var span in tagSpan.Span.GetSpans(snapshot))
            {
               TagSpan<ClassificationTag> result = null;

               try
               {
                  var commentText = span.GetText();
                  var commentStarter = GetCommentStarter(commentText);

                  if (commentText.Length < commentStarter.Length + 3)
                     continue; // Don't bother processing immature comments.

                  var trimmedComment = RemoveCommentStarter(commentText);
                  var commentType = GetCommentType(trimmedComment);

                  if (commentType == CommentType.Crossed && settings.DisableStrikethrough)
                     yield break;

                  var startOffset = ComputeSpanStartOffset(trimmedComment, commentStarter.Length);
                  var classificationTag = BuildClassificationTag(commentType);

                  var lengthOffset = 0;

                  if (singleLineCommentStarters.Contains(commentStarter)) //! Single-line comment.
                  {
                     lengthOffset = startOffset;
                  }
                  else if (delimitedCommentStarters.Contains(commentStarter)) //! Delimited comment.
                  {
                     if (!commentText.EndsWith(delimitedCommentEnders[commentStarter]))
                        continue; // Comment span is not complete. Multi-line comments are ignored, except in C#.

                     lengthOffset = startOffset + 2;
                  }
                  else if (commentStarter.Equals("<!--")) //! Hey, look! It's a markup comment!
                  {
                     if (!commentText.EndsWith("-->"))
                        continue; // Comment span is not complete. Markup multi-line comments are ignored.

                     if (classificationTag.ClassificationType.Classification == "comment")
                        continue; // Normal markup comments are ignored so their foreground color won't be overridden.

                     lengthOffset = startOffset + 3;
                  }

                  var spanStart = span.Start + startOffset;
                  var spanLength = span.Length - lengthOffset;

                  // build the span to tag
                  result = BuildTagSpan(classificationTag, BuildSnapshotSpan(span, spanStart, spanLength, commentType));
               }
               catch (Exception) { /*Ignore the exception*/ } //? Not really sure if it's a good idea!

               if (result == null)
                  yield break;

               yield return result;
            }
         }
      }
      
      private SnapshotSpan BuildSnapshotSpan(SnapshotSpan span, int start, int length, CommentType commentType)
      {
         switch (commentType)
         {
            case CommentType.Task when settings.HighlightTaskKeywordOnly:
               return BuildTaskSnapshotSpan(span);

            default:
               return new SnapshotSpan(span.Snapshot, start, length);
         }
      }

      public static SnapshotSpan BuildTaskSnapshotSpan(SnapshotSpan span)
      {
         var offset = span.GetText().ToLower().IndexOf("todo");
         return new SnapshotSpan(span.Snapshot, span.Start + offset, 4);
      }

      private static bool IsComment(IClassificationTag tag)
      {
         return tag.ClassificationType.Classification.ContainsCaseIgnored("comment");
      }

      private static bool IsXmlDoc(IClassificationTag tag)
      {
         return tag.ClassificationType.Classification.ContainsCaseIgnored("doc");
      }
      private static string GetCommentStarter(string comment)
      {
         var result = comment.StartsWithOneOf(singleLineCommentStarters);

         if (!string.IsNullOrWhiteSpace(result))
            return result;

         result = comment.StartsWithOneOf(delimitedCommentStarters);

         if (!string.IsNullOrWhiteSpace(result))
            return result;

         if (comment.StartsWith("<!--"))
            return "<!--";

         return string.Empty;
      }

      private static int ComputeSpanStartOffset(string trimmedComment, int commentStarterLength)
      {
         if (trimmedComment.StartsWith("//"))
            return 4;

         if (trimmedComment.StartsWith("'") || trimmedComment.StartsWith("#"))
            return 2;

         return IsTaskComment(trimmedComment) ? commentStarterLength : commentStarterLength + 2;
      }

      private static string RemoveCommentStarter(string comment)
      {
         // comments starting with //, /*, or (*
         if (comment.StartsWith("//") || comment.StartsWithAnyOf(delimitedCommentStarters))
            return comment.Substring(2, comment.Length - 2);

         if (comment.StartsWith("'") || comment.StartsWith("#"))
            return comment.Substring(1, comment.Length - 1);

         if (comment.StartsWith("<!--"))
            return comment.Substring(4, comment.Length - 4);

         return comment;
      }
      private static bool IsTaskComment(string trimmedComment)
      {
         return trimmedComment.Length > 4
                && trimmedComment.IndexOf("todo", 0, StringComparison.OrdinalIgnoreCase) >= 0;
      }

      private static CommentType GetCommentType(string trimmedComment)
      {
         // double comment = crossed comment
         if (trimmedComment.StartsWithAnyOf(singleLineCommentStarters))
            return CommentType.Crossed;

         if (IsTaskComment(trimmedComment))
            return CommentType.Task;

         // there must be an empty space between the token place and the rest of the comment.
         return trimmedComment[0] != ' ' && trimmedComment[1] == ' '
              ? ConvertToCommentType(trimmedComment[0].ToString())
              : CommentType.Normal;
      }

      private static CommentType ConvertToCommentType(string token)
      {
         switch (token)
         {
            case "!":
               return CommentType.Important;
            case "?":
               return CommentType.Question;
            case "x":
            case "X":
               return CommentType.Crossed;
            case "todo":
               return CommentType.Task;
            default:
               return CommentType.Normal;
         }
      }

      private ClassificationTag BuildClassificationTag(CommentType commentType)
      {
         switch (commentType)
         {
            case CommentType.Important:
               return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.IMPORTANT_COMMENT));

            case CommentType.Crossed:
               return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.CROSSED_COMMENT));

            case CommentType.Question:
               return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.QUESTION_COMMENT));

            case CommentType.Task:
               return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.TASK_COMMENT));

            case CommentType.Normal:
               return new ClassificationTag(classificationRegistry.GetClassificationType("comment"));

            default:
               throw new ArgumentException(@"Invalid comment type!", nameof(commentType), null);
         }
      }

      private static TagSpan<ClassificationTag> BuildTagSpan(ClassificationTag classificationTag, SnapshotSpan span)
      {
         return new TagSpan<ClassificationTag>(span, classificationTag);
      }

      public void Dispose()
      {
         SettingsStore.SettingsChanged -= OnSettingsChanged;
         tagAggregator.Dispose();
         GC.SuppressFinalize(this);
      }
   }
}
