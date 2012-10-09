//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// Data Transfer Object for AttributeQualifier object
    /// </summary>
    public partial class AttributeQualifierDto : IDto
    {

#pragma warning disable 1591
        public bool IsSystem { get; set; }
        public int AttributeId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public int? CreatedByPersonId { get; set; }
        public int? ModifiedByPersonId { get; set; }
        public int Id { get; set; }
        public Guid Guid { get; set; }
#pragma warning restore 1591

        /// <summary>
        /// Instantiates a new DTO object
        /// </summary>
        public AttributeQualifierDto ()
        {
        }

        /// <summary>
        /// Instantiates a new DTO object from the model
        /// </summary>
        /// <param name="attributeQualifier"></param>
        public AttributeQualifierDto ( AttributeQualifier attributeQualifier )
        {
            CopyFromModel( attributeQualifier );
        }

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="model">The model.</param>
        public void CopyFromModel( IModel model )
        {
            if ( model is AttributeQualifier )
            {
                var attributeQualifier = (AttributeQualifier)model;
                this.IsSystem = attributeQualifier.IsSystem;
                this.AttributeId = attributeQualifier.AttributeId;
                this.Key = attributeQualifier.Key;
                this.Value = attributeQualifier.Value;
                this.CreatedDateTime = attributeQualifier.CreatedDateTime;
                this.ModifiedDateTime = attributeQualifier.ModifiedDateTime;
                this.CreatedByPersonId = attributeQualifier.CreatedByPersonId;
                this.ModifiedByPersonId = attributeQualifier.ModifiedByPersonId;
                this.Id = attributeQualifier.Id;
                this.Guid = attributeQualifier.Guid;
            }
        }

        /// <summary>
        /// Copies the DTO property values to the model properties
        /// </summary>
        /// <param name="model">The model.</param>
        public void CopyToModel ( IModel model )
        {
            if ( model is AttributeQualifier )
            {
                var attributeQualifier = (AttributeQualifier)model;
                attributeQualifier.IsSystem = this.IsSystem;
                attributeQualifier.AttributeId = this.AttributeId;
                attributeQualifier.Key = this.Key;
                attributeQualifier.Value = this.Value;
                attributeQualifier.CreatedDateTime = this.CreatedDateTime;
                attributeQualifier.ModifiedDateTime = this.ModifiedDateTime;
                attributeQualifier.CreatedByPersonId = this.CreatedByPersonId;
                attributeQualifier.ModifiedByPersonId = this.ModifiedByPersonId;
                attributeQualifier.Id = this.Id;
                attributeQualifier.Guid = this.Guid;
            }
        }
    }
}
