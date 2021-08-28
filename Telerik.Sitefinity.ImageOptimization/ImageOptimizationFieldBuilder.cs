using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.ModuleEditor.Web.Services.Model;
using Telerik.Sitefinity.Web.UI.Fields;
using Telerik.Sitefinity.Web.UI.Fields.Enums;

namespace Telerik.Sitefinity.ImageOptimization
{
    public static class ImageOptimizationFieldBuilder
    {
        internal static void CreateRequiredFields()
        {
            // Creation of IsOptimized boolean field
            var contentType = typeof(Image);
            var fieldName = ImageOptimizationFieldBuilder.isOptimizedFieldName;
            var userFriendlyDataType = UserFriendlyDataType.YesNo;

            var metadataManager = MetadataManager.GetManager();
            var metaType = metadataManager.GetMetaType(contentType);

            if (metaType != null)
            {
                var metaField = metaType.Fields.SingleOrDefault(f => string.Compare(f.FieldName, fieldName, true, CultureInfo.InvariantCulture) == 0);

                if (metaField == null)
                {
                    var metaProperty = FieldHelper.GetFields(contentType).FirstOrDefault(f => string.Compare(f.Name, fieldName, true, CultureInfo.InvariantCulture) == 0);

                    if (metaProperty == null)
                    {
                        var wcfField = BuildBooleanWcfField(contentType, fieldName, true);

                        metaField = metadataManager.CreateMetafield(fieldName);

                        CustomFieldsContext.SetFieldDatabaseMappings(metaField, metaType, userFriendlyDataType, wcfField, metadataManager);

                        metaField.Title = fieldName;
                        metaField.MetaAttributes.Add(new MetaFieldAttribute { Name = "UserFriendlyDataType", Value = userFriendlyDataType.ToString() });
                        metaField.MetaAttributes.Add(new MetaFieldAttribute { Name = "IsCommonProperty", Value = "true" });
                        metaField.Origin = Assembly.GetExecutingAssembly().GetName().Name; // needed to override deletion from Export for deployment
                        metaType.Fields.Add(metaField);

                        metadataManager.SaveChanges();
                    }
                }
            }
        }

        private static WcfField BuildBooleanWcfField(Type contentType, string fieldName, bool isHidden)
        {
            var wcfField = new WcfField()
            {
                Name = fieldName,
                ContentType = contentType.FullName,
                FieldTypeKey = UserFriendlyDataType.YesNo.ToString(),
                IsCustom = true,

                //Database mapping
                DatabaseMapping = new WcfDatabaseMapping()
                {
                    ClrType = typeof(bool).FullName,
                    ColumnName = fieldName.ToLower(),
                    Nullable = true,
                    DbType = "BIT"
                },

                //Field definition
                Definition = new WcfFieldDefinition()
                {
                    Title = fieldName,
                    FieldName = fieldName,
                    FieldType = typeof(ChoiceField).FullName,
                    RenderChoiceAs = RenderChoicesAs.SingleCheckBox,
                    Hidden = isHidden
                }
            };

            ChoiceItem yesNoItem = new ChoiceItem()
            {
                Text = fieldName,
                Value = "true",
                Selected = false
            };

            List<ChoiceItem> items = new List<ChoiceItem>();
            items.Add(yesNoItem);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            wcfField.Definition.Choices = serializer.Serialize(items);

            CustomFieldsContext.Validate(wcfField, contentType);

            return wcfField;
        }

        private static readonly string isOptimizedFieldName = "IsOptimized";
    }
}
