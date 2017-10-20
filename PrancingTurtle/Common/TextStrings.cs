namespace Common
{
    public static class TextStrings
    {
        public static class Database
        {
            public const string UniqueConstraintFailureCheck = "DUPLICATE ENTRY";
            public static string UniqueConstraintFailureMessage(string objectName) => $"Operation failed - a matching {objectName} already exists!";
        }

        public static class Appearance
        {
            public const string BackButton = "btn btn-default";
            public const string BackArrow = "fa fa-lg fa-arrow-left";

            public const string CreateUpdateButton = "btn btn-success";
            public const string CreateButtonPlus = "fa fa-lg fa-plus-circle";
            public const string DeleteButton = "btn btn-danger";

            public const string SmallEditButton = "fa fa-lg fa-edit";
            public const string SmallDeleteButton = "fa fa-lg fa-trash-o";
            public const string SmallDetailButton = "fa fa-lg fa-info";
            public const string SmallCommentButton = "fa fa-lg fa-comments-o";
        }
    }
}
