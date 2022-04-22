using System;

namespace YesSql
{
    /// <summary>
    /// The class stored in the Document table of a collection.
    /// </summary>
    public class Document : IEquatable<Document>
    {
        /// <summary>
        /// The unique identifier of the document in the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The type of the document.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the serialized content of the document.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the version number of the document.
        /// </summary>
        /// <remarks>
        /// This property is used to track updates, and optionally detect concurrency violations.
        /// </remarks>
        public long Version { get; set; }
        public bool Equals(Document other)
        {
            if (other == null)
            {
                return false;
            }
            return Id == other.Id && Type == other.Type && Content == other.Content && Version == other.Version;
        }

        public override int GetHashCode()
        {
            var hashCode = 13;
            hashCode = (hashCode * 397) ^ Id;
            hashCode = (hashCode * 397) ^ (!string.IsNullOrEmpty(Type) ? Type.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (!string.IsNullOrEmpty(Content) ? Content.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int)Version;

            return hashCode;
        }
    }
}
